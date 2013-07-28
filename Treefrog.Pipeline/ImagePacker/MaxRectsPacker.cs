using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.Pipeline.ImagePacker
{
    /// <summary>
    /// Packs pages of images using the maximal rectangles bin packing algorithm by Jukka Jylänki. A brute force binary search is used
    /// to pack into the smallest bin possible.
    /// </summary>
    public class MaxRectsPacker
    {
        private RectComparer _rectCompartor;
        private List<FreeRectChoiceHeuristic> _methods;
        private MaxRects _maxRects;
        private Settings _settings;

        public MaxRectsPacker (Settings settings)
        {
            _settings = settings;
            _methods = new List<FreeRectChoiceHeuristic>(Enum.GetValues(typeof(FreeRectChoiceHeuristic)).OfType<FreeRectChoiceHeuristic>());
            _rectCompartor = new RectComparer(this);
            _maxRects = new MaxRects(this);

            if (_settings.MinWidth > _settings.MaxWidth)
                throw new Exception("Page min width cannot be higher than max width");
            if (_settings.MinHeight > _settings.MaxHeight)
                throw new Exception("Page min height cannot be higher than max height");
        }

        public List<Page> Pack (List<Rect> inputRects)
        {
            for (int i = 0, nn = inputRects.Count; i < nn; i++) {
                Rect rect = inputRects[i];
                rect.Width += _settings.PaddingX;
                rect.Height += _settings.PaddingY;
            }

            if (_settings.Fast) {
                if (_settings.Rotation) {
                    inputRects.Sort((r1, r2) => {
                        int n1 = r1.Width > r1.Height ? r1.Width : r1.Height;
                        int n2 = r2.Width > r2.Height ? r2.Width : r2.Height;
                        return n2 - n1;
                    });
                }
                else {
                    inputRects.Sort((r1, r2) => r2.Width - r1.Width);
                }
            }

            List<Page> pages = new List<Page>();
            while (inputRects.Count > 0) {
                Page result = PackPage(inputRects);
                pages.Add(result);
                inputRects = result.RemainingRects;
            }

            return pages;
        }

        private Page PackPage (List<Rect> inputRects)
        {
            int edgePaddingX = 0;
            int edgePaddingY = 0;

            if (!_settings.DuplicatePadding) {
                edgePaddingX = _settings.PaddingX;
                edgePaddingY = _settings.PaddingY;
            }

            int minWidth = int.MaxValue;
            int minHeight = int.MaxValue;

            for (int i = 0, nn = inputRects.Count; i < nn; i++) {
                Rect rect = inputRects[i];
                minWidth = Math.Min(minWidth, rect.Width);
                minHeight = Math.Min(minHeight, rect.Height);

                if (_settings.Rotation) {
                    if ((rect.Width > _settings.MaxWidth || rect.Height > _settings.MaxHeight)
                        && (rect.Width > _settings.MaxHeight || rect.Height > _settings.MaxWidth))
                        throw new Exception(string.Format("Image does not fit with max page size {0}x{1} and padding {2},{3}: {4}",
                            _settings.MaxWidth, _settings.MaxHeight, _settings.PaddingX, _settings.PaddingY, rect));
                }
                else {
                    if (rect.Width > _settings.MaxWidth)
                        throw new Exception(string.Format("Image does not fit with max page width {0} and paddingX {1}: {2}",
                            _settings.MaxWidth, _settings.PaddingX, rect));
                    if (rect.Height > _settings.MaxHeight)
                        throw new Exception(string.Format("Image does not fit with max page height {0} and paddingY {1}: {2}",
                            _settings.MaxHeight, _settings.PaddingY, rect));
                }
            }

            minWidth = Math.Max(minWidth, _settings.MinWidth);
            minHeight = Math.Max(minHeight, _settings.MinHeight);

            BinarySearch widthSearch = new BinarySearch(minWidth, _settings.MaxWidth, _settings.Fast ? 25 : 15, _settings.PowerOfTwo);
            BinarySearch heightSearch = new BinarySearch(minHeight, _settings.MaxHeight, _settings.Fast ? 25 : 15, _settings.PowerOfTwo);

            int width = widthSearch.Reset();
            int height = heightSearch.Reset();

            //int i = 0;
            Page bestRuesult = null;

            while (true) {
                Page bestWidthResult = null;
                while (width != -1) {
                    Page result = PackAtSize(true, width - edgePaddingX, height - edgePaddingY, inputRects);
                    bestWidthResult = GetBest(bestWidthResult, result);
                    width = widthSearch.Next(result == null);
                }

                bestRuesult = GetBest(bestRuesult, bestWidthResult);
                height = heightSearch.Next(bestWidthResult == null);
                if (height == -1)
                    break;

                width = widthSearch.Reset();
            }
            if (bestRuesult == null)
                bestRuesult = PackAtSize(false, _settings.MaxWidth - edgePaddingX, _settings.MaxHeight - edgePaddingY, inputRects);

            bestRuesult.OutputRects.Sort(_rectCompartor);

            return bestRuesult;
        }

        private Page PackAtSize (bool fully, int width, int height, List<Rect> inputRects)
        {
            Page bestResult = null;

            for (int i = 0, n = _methods.Count; i < n; i++) {
                _maxRects.Init(width, height);

                Page result;
                if (!_settings.Fast) {
                    result = _maxRects.Pack(inputRects, _methods[i]);
                }
                else {
                    List<Rect> remaining = new List<Rect>();
                    for (int ii = 0, nn = inputRects.Count; ii < nn; ii++) {
                        Rect rect = inputRects[ii];
                        if (_maxRects.Insert(rect, _methods[i]) == null) {
                            while (ii < nn)
                                remaining.Add(inputRects[ii++]);
                        }
                    }

                    result = _maxRects.GetResult();
                    result.RemainingRects = remaining;
                }

                if (fully && result.RemainingRects.Count > 0)
                    continue;
                if (result.OutputRects.Count == 0)
                    continue;

                bestResult = GetBest(bestResult, result);
            }

            return bestResult;
        }

        private Page GetBest (Page result1, Page result2)
        {
            if (result1 == null)
                return result2;
            if (result2 == null)
                return result1;

            return result1.Occupancy > result2.Occupancy ? result1 : result2;
        }

        private class BinarySearch
        {
            int _min;
            int _max;
            int _fuzziness;
            int _low;
            int _high;
            int _current;
            bool _pot;

            public BinarySearch (int min, int max, int fuzziness, bool pot)
            {
                this._pot = pot;
                this._fuzziness = pot ? 0 : fuzziness;
                this._min = pot ? (int)(Math.Log(MathUtils.NextPowerOfTwo(min)) / Math.Log(2)) : min;
                this._max = pot ? (int)(Math.Log(MathUtils.NextPowerOfTwo(max)) / Math.Log(2)) : max;
            }

            public int Reset () 
            {
    			_low = _min;
	    		_high = _max;
		    	_current = (int)((uint)(_low + _high) >> 1);

			    return _pot ? (int)(1 << _current) : _current;
		    }

            public int Next (bool result) 
            {
			    if (_low >= _high) 
                    return -1;

			    if (result)
				    _low = _current + 1;
			    else
				    _high = _current - 1;

			    _current = (int)((uint)(_low + _high) >> 1);

			    if (Math.Abs(_low - _high) < _fuzziness) 
                    return -1;

			    return _pot ? (int)(1 << _current) : _current;
		    }
        }

        /// <summary>
        /// Maximal rectangles bin packing algorithm. Adapted from this C++ public domain source:
	    /// http://clb.demon.fi/projects/even-more-rectangle-bin-packing
        /// </summary>
        private class MaxRects 
        {
            private MaxRectsPacker _outer;
            private int _binWidth;
            private int _binHeight;
            private readonly List<Rect> _usedRectangles;
            private readonly List<Rect> _freeRectangles;

            public MaxRects (MaxRectsPacker outer)
            {
                _outer = outer;
                _usedRectangles = new List<Rect>();
                _freeRectangles = new List<Rect>();
            }

            public void Init (int width, int height)
            {
                _binWidth = width;
                _binHeight = height;

                _usedRectangles.Clear();
                _freeRectangles.Clear();

                Rect n = new Rect() {
                    X = 0,
                    Y = 0,
                    Width = width,
                    Height = height,
                };

                _freeRectangles.Add(n);
            }

            /// <summary>
            /// Packs a single image. Order is defined externally.
            /// </summary>
            public Rect Insert (Rect rect, FreeRectChoiceHeuristic method)
            {
                Rect newNode = ScoreRect(rect, method);
                if (newNode.Height == 0)
                    return null;

                int numRectanglesToProcess = _freeRectangles.Count;
                for (int i = 0; i < numRectanglesToProcess; i++) {
                    if (SplitFreeNode(_freeRectangles[i], newNode)) {
                        _freeRectangles.RemoveAt(i);
                        i--;
                        numRectanglesToProcess--;
                    }
                }

                PruneFreeList();

                Rect bestNode = new Rect();
                bestNode.Set(rect);
                bestNode.Score1 = newNode.Score1;
                bestNode.Score2 = newNode.Score2;
                bestNode.X = newNode.X;
                bestNode.Y = newNode.Y;
                bestNode.Width = newNode.Width;
                bestNode.Height = newNode.Height;
                bestNode.Rotated = newNode.Rotated;

                _usedRectangles.Add(bestNode);
                return bestNode;
            }

            /// <summary>
            /// For each rectangle, packs each one then chooses the best and packs that. Slow!
            /// </summary>
            public Page Pack (List<Rect> rects, FreeRectChoiceHeuristic method)
            {
                rects = new List<Rect>(rects);
                while (rects.Count > 0) {
                    int bestRectIndex = -1;
                    Rect bestNode = new Rect() {
                        Score1 = int.MaxValue,
                        Score2 = int.MaxValue,
                    };

                    for (int i = 0; i < rects.Count; i++) {
                        Rect newNode = ScoreRect(rects[i], method);
                        if (newNode.Score1 < bestNode.Score1 || (newNode.Score1 == bestNode.Score1 && newNode.Score2 < bestNode.Score2)) {
                            bestNode.Set(rects[i]);
                            bestNode.Score1 = newNode.Score1;
                            bestNode.Score2 = newNode.Score2;
                            bestNode.X = newNode.X;
                            bestNode.Y = newNode.Y;
                            bestNode.Width = newNode.Width;
                            bestNode.Height = newNode.Height;
                            bestNode.Rotated = newNode.Rotated;
                            bestRectIndex = i;
                        }
                    }

                    if (bestRectIndex == -1)
                        break;

                    PlaceRect(bestNode);
                    rects.RemoveAt(bestRectIndex);
                }

                Page result = GetResult();
                result.RemainingRects = rects;

                return result;
            }

            public Page GetResult ()
            {
                int w = 0;
                int h = 0;

                for (int i = 0; i < _usedRectangles.Count; i++) {
                    Rect rect = _usedRectangles[i];
                    w = Math.Max(w, rect.X + rect.Width);
                    h = Math.Max(h, rect.Y + rect.Height);
                }

                Page result = new Page() {
                    OutputRects = new List<Rect>(_usedRectangles),
                    Occupancy = GetOccupancy(),
                    Width = w,
                    Height = h,
                };

                return result;
            }

            private void PlaceRect (Rect node)
            {
                int numRectanglesToPlace = _freeRectangles.Count;
                for (int i = 0; i < numRectanglesToPlace; i++) {
                    if (SplitFreeNode(_freeRectangles[i], node)) {
                        _freeRectangles.RemoveAt(i);
                        i--;
                        numRectanglesToPlace--;
                    }
                }

                PruneFreeList();

                _usedRectangles.Add(node);
            }

            private Rect ScoreRect (Rect rect, FreeRectChoiceHeuristic method)
            {
                int width = rect.Width;
                int height = rect.Height;
                int rotatedWidth = height - _outer._settings.PaddingY + _outer._settings.PaddingX;
                int rotatedHeight = width - _outer._settings.PaddingX + _outer._settings.PaddingY;
                bool rotate = rect.CanRotate && _outer._settings.Rotation;

                Rect newNode = null;
                switch (method) {
                    case FreeRectChoiceHeuristic.BestShortSideFit:
                        newNode = FindPositionForNewNodeBestShortSideFit(width, height, rotatedWidth, rotatedHeight, rotate);
                        break;
                    case FreeRectChoiceHeuristic.BottomLeftRule:
                        newNode = FindPositionForNewNodeBottomLeft(width, height, rotatedWidth, rotatedHeight, rotate);
                        break;
                    case FreeRectChoiceHeuristic.ContactPointRule:
                        newNode = FindPositionForNewNodeContactPoint(width, height, rotatedWidth, rotatedHeight, rotate);
                        newNode.Score1 = -newNode.Score1;
                        break;
                    case FreeRectChoiceHeuristic.BestLongSideFit:
                        newNode = FindPositionForNewNodeBestLongSideFit(width, height, rotatedWidth, rotatedHeight, rotate);
                        break;
                    case FreeRectChoiceHeuristic.BestAreaFit:
                        newNode = FindPositionForNewNodeBestAreaFit(width, height, rotatedWidth, rotatedHeight, rotate);
                        break;
                }

                if (newNode.Height == 0) {
                    newNode.Score1 = int.MaxValue;
                    newNode.Score2 = int.MaxValue;
                }

                return newNode;
            }

            /// <summary>
            /// Computes the ratio of used surface area.
            /// </summary>
            private float GetOccupancy ()
            {
                int usedSurfaceArea = 0;
                for (int i = 0; i < _usedRectangles.Count; i++)
                    usedSurfaceArea += _usedRectangles[i].Width * _usedRectangles[i].Height;

                return (float)usedSurfaceArea / (_binWidth * _binHeight);
            }

            private Rect FindPositionForNewNodeBottomLeft (int width, int height, int rotatedWidth, int rotatedHeight, bool rotate)
            {
                Rect bestNode = new Rect() {
                    Score1 = int.MaxValue,
                };

                for (int i = 0; i < _freeRectangles.Count; i++) {
                    if (_freeRectangles[i].Width >= width && _freeRectangles[i].Height >= height) {
                        int topSideY = _freeRectangles[i].Y + height;
                        if (topSideY < bestNode.Score1 || (topSideY == bestNode.Score1 && _freeRectangles[i].X < bestNode.Score2)) {
                            bestNode.X = _freeRectangles[i].X;
                            bestNode.Y = _freeRectangles[i].Y;
                            bestNode.Width = width;
                            bestNode.Height = height;
                            bestNode.Score1 = topSideY;
                            bestNode.Score2 = _freeRectangles[i].X;
                            bestNode.Rotated = false;
                        }
                    }

                    if (rotate && _freeRectangles[i].Width >= rotatedWidth && _freeRectangles[i].Height >= rotatedHeight) {
                        int topSideY = _freeRectangles[i].Y + rotatedHeight;
                        if (topSideY < bestNode.Score1 || (topSideY == bestNode.Score1 && _freeRectangles[i].X < bestNode.Score2)) {
                            bestNode.X = _freeRectangles[i].X;
                            bestNode.Y = _freeRectangles[i].Y;
                            bestNode.Width = rotatedWidth;
                            bestNode.Height = rotatedHeight;
                            bestNode.Score1 = topSideY;
                            bestNode.Score2 = _freeRectangles[i].X;
                            bestNode.Rotated = true;
                        }
                    }
                }

                return bestNode;
            }

            private Rect FindPositionForNewNodeBestShortSideFit (int width, int height, int rotatedWidth, int rotatedHeight, bool rotate)
            {
                Rect bestNode = new Rect() {
                    Score1 = int.MaxValue,
                };

                for (int i = 0; i < _freeRectangles.Count; i++) {
                    if (_freeRectangles[i].Width >= width && _freeRectangles[i].Height >= height) {
                        int leftoverHoriz = Math.Abs(_freeRectangles[i].Width - width);
                        int leftoverVert = Math.Abs(_freeRectangles[i].Height - height);
                        int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                        int longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                        if (shortSideFit < bestNode.Score1 || (shortSideFit == bestNode.Score1 && longSideFit < bestNode.Score2)) {
                            bestNode.X = _freeRectangles[i].X;
                            bestNode.Y = _freeRectangles[i].Y;
                            bestNode.Width = width;
                            bestNode.Height = height;
                            bestNode.Score1 = shortSideFit;
                            bestNode.Score2 = longSideFit;
                            bestNode.Rotated = false;
                        }
                    }

                    if (rotate && _freeRectangles[i].Width >= rotatedWidth && _freeRectangles[i].Height >= rotatedHeight) {
                        int flippedLeftoverHoriz = Math.Abs(_freeRectangles[i].Width - rotatedWidth);
                        int flippedLeftoverVert = Math.Abs(_freeRectangles[i].Height - rotatedHeight);
                        int flippedShortSideFit = Math.Min(flippedLeftoverHoriz, flippedLeftoverVert);
                        int flippedLongSideFit = Math.Max(flippedLeftoverHoriz, flippedLeftoverVert);

                        if (flippedShortSideFit < bestNode.Score1 || (flippedShortSideFit == bestNode.Score1 && flippedLongSideFit < bestNode.Score2)) {
                            bestNode.X = _freeRectangles[i].X;
                            bestNode.Y = _freeRectangles[i].Y;
                            bestNode.Width = rotatedWidth;
                            bestNode.Height = rotatedHeight;
                            bestNode.Score1 = flippedShortSideFit;
                            bestNode.Score2 = flippedLongSideFit;
                            bestNode.Rotated = true;
                        }
                    }
                }

                return bestNode;
            }

            private Rect FindPositionForNewNodeBestLongSideFit (int width, int height, int rotatedWidth, int rotatedHeight, bool rotate)
            {
                Rect bestNode = new Rect() {
                    Score2 = int.MaxValue,
                };

                for (int i = 0; i < _freeRectangles.Count; i++) {
                    if (_freeRectangles[i].Width >= width && _freeRectangles[i].Height >= height) {
                        int leftoverHoriz = Math.Abs(_freeRectangles[i].Width - width);
                        int leftoverVert = Math.Abs(_freeRectangles[i].Height - height);
                        int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                        int longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                        if (longSideFit < bestNode.Score2 || (longSideFit == bestNode.Score2 && shortSideFit < bestNode.Score1)) {
                            bestNode.X = _freeRectangles[i].X;
                            bestNode.Y = _freeRectangles[i].Y;
                            bestNode.Width = width;
                            bestNode.Height = height;
                            bestNode.Score1 = shortSideFit;
                            bestNode.Score2 = longSideFit;
                            bestNode.Rotated = false;
                        }
                    }

                    if (rotate && _freeRectangles[i].Width >= rotatedWidth && _freeRectangles[i].Height >= rotatedHeight) {
                        int leftoverHoriz = Math.Abs(_freeRectangles[i].Width - rotatedWidth);
                        int leftoverVert = Math.Abs(_freeRectangles[i].Height - rotatedHeight);
                        int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                        int longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                        if (longSideFit < bestNode.Score2 || (longSideFit == bestNode.Score2 && shortSideFit < bestNode.Score1)) {
                            bestNode.X = _freeRectangles[i].X;
                            bestNode.Y = _freeRectangles[i].Y;
                            bestNode.Width = rotatedWidth;
                            bestNode.Height = rotatedHeight;
                            bestNode.Score1 = shortSideFit;
                            bestNode.Score2 = longSideFit;
                            bestNode.Rotated = true;
                        }
                    }
                }

                return bestNode;
            }

            private Rect FindPositionForNewNodeBestAreaFit (int width, int height, int rotatedWidth, int rotatedHeight, bool rotate)
            {
                Rect bestNode = new Rect() {
                    Score1 = int.MaxValue,
                };

                for (int i = 0; i < _freeRectangles.Count; i++) {
                    int areaFit = _freeRectangles[i].Width * _freeRectangles[i].Height - width * height;

                    if (_freeRectangles[i].Width >= width && _freeRectangles[i].Height >= height) {
                        int leftoverHoriz = Math.Abs(_freeRectangles[i].Width - width);
                        int leftoverVert = Math.Abs(_freeRectangles[i].Height - height);
                        int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);

                        if (areaFit < bestNode.Score1 || (areaFit == bestNode.Score1 && shortSideFit < bestNode.Score2)) {
                            bestNode.X = _freeRectangles[i].X;
                            bestNode.Y = _freeRectangles[i].Y;
                            bestNode.Width = width;
                            bestNode.Height = height;
                            bestNode.Score2 = shortSideFit;
                            bestNode.Score1 = areaFit;
                            bestNode.Rotated = false;
                        }
                    }

                    if (rotate && _freeRectangles[i].Width >= rotatedWidth && _freeRectangles[i].Height >= rotatedHeight) {
                        int leftoverHoriz = Math.Abs(_freeRectangles[i].Width - rotatedWidth);
                        int leftoverVert = Math.Abs(_freeRectangles[i].Height - rotatedHeight);
                        int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);

                        if (areaFit < bestNode.Score1 || (areaFit == bestNode.Score1 && shortSideFit < bestNode.Score2)) {
                            bestNode.X = _freeRectangles[i].X;
                            bestNode.Y = _freeRectangles[i].Y;
                            bestNode.Width = rotatedWidth;
                            bestNode.Height = rotatedHeight;
                            bestNode.Score2 = shortSideFit;
                            bestNode.Score1 = areaFit;
                            bestNode.Rotated = true;
                        }
                    }
                }

                return bestNode;
            }

            /// <summary>
            /// Returns 0 if the two intervals i1 and i2 are disjoint, or the length of their overlap otherwise.
            /// </summary>
            private int CommonIntervalLength (int i1start, int i1end, int i2start, int i2end)
            {
                if (i1end < i2start || i2end < i1start) 
                    return 0;

                return Math.Min(i1end, i2end) - Math.Max(i1start, i2start);
            }

            private int ContactPointScoreNode (int x, int y, int width, int height)
            {
                int score = 0;

                if (x == 0 || x + width == _binWidth) 
                    score += height;
                if (y == 0 || y + height == _binHeight) 
                    score += width;

                for (int i = 0; i < _usedRectangles.Count; i++) {
                    if (_usedRectangles[i].X == x + width || _usedRectangles[i].X + _usedRectangles[i].Width == x)
                        score += CommonIntervalLength(_usedRectangles[i].Y, _usedRectangles[i].Y + _usedRectangles[i].Height, y, y + height);
                    if (_usedRectangles[i].Y == y + height || _usedRectangles[i].Y + _usedRectangles[i].Height == y)
                        score += CommonIntervalLength(_usedRectangles[i].X, _usedRectangles[i].X + _usedRectangles[i].Width, x, x + width);
                }

                return score;
            }

            private Rect FindPositionForNewNodeContactPoint (int width, int height, int rotatedWidth, int rotatedHeight, bool rotate)
            {
                Rect bestNode = new Rect() {
                    Score1 = -1,
                };

                for (int i = 0; i < _freeRectangles.Count; i++) {
                    if (_freeRectangles[i].Width >= width && _freeRectangles[i].Height >= height) {
                        int score = ContactPointScoreNode(_freeRectangles[i].X, _freeRectangles[i].Y, width, height);
                        if (score > bestNode.Score1) {
                            bestNode.X = _freeRectangles[i].X;
                            bestNode.Y = _freeRectangles[i].Y;
                            bestNode.Width = width;
                            bestNode.Height = height;
                            bestNode.Score1 = score;
                            bestNode.Rotated = false;
                        }
                    }
                    if (rotate && _freeRectangles[i].Width >= rotatedWidth && _freeRectangles[i].Height >= rotatedHeight) {
                        int score = ContactPointScoreNode(_freeRectangles[i].X, _freeRectangles[i].Y, rotatedWidth, rotatedHeight);
                        if (score > bestNode.Score1) {
                            bestNode.X = _freeRectangles[i].X;
                            bestNode.Y = _freeRectangles[i].Y;
                            bestNode.Width = rotatedWidth;
                            bestNode.Height = rotatedHeight;
                            bestNode.Score1 = score;
                            bestNode.Rotated = true;
                        }
                    }
                }

                return bestNode;
            }

            private bool SplitFreeNode (Rect freeNode, Rect usedNode)
            {
                // Test with SAT if the rectangles even intersect.
                if (usedNode.X >= freeNode.X + freeNode.Width 
                    || usedNode.X + usedNode.Width <= freeNode.X
                    || usedNode.Y >= freeNode.Y + freeNode.Height 
                    || usedNode.Y + usedNode.Height <= freeNode.Y) 
                    return false;

                if (usedNode.X < freeNode.X + freeNode.Width && usedNode.X + usedNode.Width > freeNode.X) {
                    // New node at the top side of the used node.
                    if (usedNode.Y > freeNode.Y && usedNode.Y < freeNode.Y + freeNode.Height) {
                        Rect newNode = new Rect(freeNode);
                        newNode.Height = usedNode.Y - newNode.Y;
                        _freeRectangles.Add(newNode);
                    }

                    // New node at the bottom side of the used node.
                    if (usedNode.Y + usedNode.Height < freeNode.Y + freeNode.Height) {
                        Rect newNode = new Rect(freeNode);
                        newNode.Y = usedNode.Y + usedNode.Height;
                        newNode.Height = freeNode.Y + freeNode.Height - (usedNode.Y + usedNode.Height);
                        _freeRectangles.Add(newNode);
                    }
                }

                if (usedNode.Y < freeNode.Y + freeNode.Height && usedNode.Y + usedNode.Height > freeNode.Y) {
                    // New node at the left side of the used node.
                    if (usedNode.X > freeNode.X && usedNode.X < freeNode.X + freeNode.Width) {
                        Rect newNode = new Rect(freeNode);
                        newNode.Width = usedNode.X - newNode.X;
                        _freeRectangles.Add(newNode);
                    }

                    // New node at the right side of the used node.
                    if (usedNode.X + usedNode.Width < freeNode.X + freeNode.Width) {
                        Rect newNode = new Rect(freeNode);
                        newNode.X = usedNode.X + usedNode.Width;
                        newNode.Width = freeNode.X + freeNode.Width - (usedNode.X + usedNode.Width);
                        _freeRectangles.Add(newNode);
                    }
                }

                return true;
            }

            private void PruneFreeList ()
            {
                for (int i = 0; i < _freeRectangles.Count; i++) {
                    for (int j = i + 1; j < _freeRectangles.Count; j++) {
                        if (IsContainedIn(_freeRectangles[i], _freeRectangles[j])) {
                            _freeRectangles.RemoveAt(i);
                            i--;
                            break;
                        }

                        if (IsContainedIn(_freeRectangles[j], _freeRectangles[i])) {
                            _freeRectangles.RemoveAt(j);
                            j--;
                        }
                    }
                }
            }

            private bool IsContainedIn (Rect a, Rect b)
            {
                return a.X >= b.X
                    && a.Y >= b.Y
                    && a.X + a.Width <= b.X + b.Width
                    && a.Y + a.Height <= b.Y + b.Height;
            }
        }

        public enum FreeRectChoiceHeuristic
        {
            BestShortSideFit,
            BestLongSideFit,
            BestAreaFit,
            BottomLeftRule,
            ContactPointRule,
        }

        public class RectComparer : IComparer<Rect>
        {
            private MaxRectsPacker _outer;

            public RectComparer (MaxRectsPacker outer)
            {
                _outer = outer;
            }

            public int Compare (Rect x, Rect y)
            {
                return Rect.GetAliasName(x.Name, _outer._settings.FlattenPaths).CompareTo(Rect.GetAliasName(y.Name, _outer._settings.FlattenPaths));
            }
        }
    }
}
