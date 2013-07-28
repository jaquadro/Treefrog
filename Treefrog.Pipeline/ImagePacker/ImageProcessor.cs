/*******************************************************************************
 * Copyright 2011 See AUTHORS file.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 ******************************************************************************/
/* Copyright 2013 See AUTHORS file.
 * This work is a port from libgdx.
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Treefrog.Pipeline.ImagePacker
{
    public class ImageProcessor
    {
        private static readonly Bitmap _emptyImage = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        private static readonly Regex _indexPattern = new Regex("(.+)_(\\d+)$");

        private string _rootPath;
        private readonly Settings _settings;
        private readonly Dictionary<string, Rect> _crcs = new Dictionary<string, Rect>();
        private readonly List<Rect> _rects = new List<Rect>();

        public ImageProcessor (string rootDir, Settings settings)
        {
            _settings = settings;

            if (rootDir != null) {
                _rootPath = Path.GetFullPath(rootDir).Replace('\\', '/');
                if (!_rootPath.EndsWith("/"))
                    _rootPath += "/";
            }
        }

        public ImageProcessor (Settings settings)
            : this(null, settings)
        { }

        public List<Rect> Images
        {
            get { return _rects; }
        }

        public void AddImage (string file)
        {
            Bitmap image;
            try {
                image = new Bitmap(file);
            }
            catch (Exception ex) {
                throw new Exception("Error reading image: " + file, ex);
            }

            if (image == null) 
                throw new Exception("Unable to read image: " + file);

            // Strip root dir off front of image path.
            string name = Path.GetFullPath(file).Replace('\\', '/');

            if (!name.StartsWith(_rootPath)) 
                throw new Exception("Path '" + name + "' does not start with root: " + _rootPath);

            name = name.Substring(_rootPath.Length);

            // Strip extension.
            name = Path.GetFileNameWithoutExtension(name);

            AddImage(image, name);
            image.Dispose();
        }

        public void AddImage (Bitmap image, String name)
        {
            if (image.PixelFormat != PixelFormat.Format32bppArgb) {
                Bitmap newImage = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(image)) {
                    g.DrawImage(image, 0, 0);
                }
                image = newImage;
            }
            else
                image = new Bitmap(image);

            Rect rect = null;

            int[] splits = null;
            int[] pads = null;

            if (name.EndsWith(".9")) {
                name = name.Substring(0, name.Length - 1);
                splits = GetSplits(image, name);
                pads = GetPads(image, name, splits);

                Bitmap newImage = new Bitmap(image.Width - 2, image.Height - 2, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(newImage)) {
                    g.DrawImage(image, new Rectangle(0, 0, newImage.Width, newImage.Height), new Rectangle(1, 1, image.Width - 1, image.Height - 1), GraphicsUnit.Pixel);
                }

                image.Dispose();
                image = newImage;

                rect = new Rect(image, 0, 0, image.Width, image.Height);
                rect.Splits = splits;
                rect.Pads = pads;
                rect.CanRotate = false;
            }

            int index = -1;
            if (_settings.UseIndexes) {
                Match match = _indexPattern.Match(name);
                if (match.Success) {
                    name = match.Groups[1].Value;
                    index = int.Parse(match.Groups[2].Value);
                }
            }

            if (rect == null) {
                rect = CreateRect(image);
                if (rect == null)
                    return;
            }

            rect.Name = name;
            rect.Index = index;

            if (_settings.Alias) {
                string crc = Hash(rect.Image);

                Rect existing;
                if (_crcs.TryGetValue(crc, out existing)) {
                    existing.Aliases.Add(new Alias(rect));
                    return;
                }

                _crcs[crc] = rect;
            }

            _rects.Add(rect);
        }

        private Rect CreateRect (Bitmap source)
        {
            if (!Bitmap.IsAlphaPixelFormat(source.PixelFormat) || (!_settings.StripWhitespaceX && !_settings.StripWhitespaceY))
                return new Rect(source, 0, 0, source.Width, source.Height);

            int top = 0;
            int bottom = source.Height;

            if (_settings.StripWhitespaceX) {
                for (int y = 0; y < source.Height; y++) {
                    for (int x = 0; x < source.Width; x++) {
                        int alpha = source.GetPixel(x, y).A;
                        if (alpha > _settings.AlphaThreshold)
                            goto Break1;
                    }
                    top++;
                }
                Break1:;

                for (int y = source.Height; --y >= top; ) {
                    for (int x = 0; x < source.Width; x++) {
                        int alpha = source.GetPixel(x, y).A;
                        if (alpha > _settings.AlphaThreshold)
                            goto Break2;
                    }
                    bottom--;
                }
                Break2:;
            }

            int left = 0;
            int right = source.Width;

            if (_settings.StripWhitespaceY) {
                for (int x = 0; x < source.Width; x++) {
                    for (int y = top; y < bottom; y++) {
                        int alpha = source.GetPixel(x, y).A;
                        if (alpha > _settings.AlphaThreshold)
                            goto Break1;
                    }
                    left++;
                }
                Break1:;

                for (int x = source.Width; --x >= left;) {
                    for (int y = top; y < bottom; y++) {
                        int alpha = source.GetPixel(x, y).A;
                        if (alpha > _settings.AlphaThreshold)
                            goto Break2;
                    }
                    right--;
                }
                Break2:;
            }

            int newWidth = right - left;
            int newHeight = bottom - top;

            if (newWidth <= 0 || newHeight <= 0) {
                if (_settings.IgnoreBlankImages)
                    return null;
                else
                    return new Rect(_emptyImage, 0, 0, 1, 1);
            }

            return new Rect(source, left, top, newWidth, newHeight);
        }

        private int[] GetSplits (Bitmap image, string name)
        {
            int startX = GetSplitPoint(image, name, 1, 0, true, true);
            int endX = GetSplitPoint(image, name, startX, 0, false, true);
            int startY = GetSplitPoint(image, name, 0, 1, true, false);
            int endY = GetSplitPoint(image, name, 0, startY, false, false);

            GetSplitPoint(image, name, endX + 1, 0, true, true);
            GetSplitPoint(image, name, 0, endY + 1, true, false);

            if (startX == 0 && endX == 0 && startY == 0 && endY == 0)
                return null;

            if (startX != 0) {
                startX--;
                endX = image.Width - 2 - (endX - 1);
            }
            else
                endX = image.Width - 2;

            if (startY != 0) {
                startY--;
                endY = image.Height - 2 - (endY - 1);
            }
            else
                endY = image.Height - 2;

            return new int[] { startX, endX, startY, endY };
        }

        private int[] GetPads (Bitmap image, string name, int[] splits)
        {
            int bottom = image.Height - 1;
            int right = image.Width - 1;

            int startX = GetSplitPoint(image, name, 1, bottom, true, true);
            int startY = GetSplitPoint(image, name, right, 1, true, false);

            int endX = 0;
            int endY = 0;

            if (startX != 0)
                endX = GetSplitPoint(image, name, startX + 1, bottom, false, true);
            if (startY != 0)
                endY = GetSplitPoint(image, name, right, startY + 1, false, false);

            if (startX == 0 && endX == 0 && startY == 0 && endY == 0)
                return null;

            if (startX == 0 && endX == 0) {
                startX = -1;
                endX = -1;
            }
            else {
                if (startX > 0) {
                    startX--;
                    endX = image.Width - 2 - (endX - 1);
                }
                else
                    endX = image.Width - 2;
            }

            if (startY == 0 && endY == 0) {
                startY = -1;
                endY = -1;
            }
            else {
                if (startY > 0) {
                    startY--;
                    endY = image.Height - 2 - (endY - 1);
                }
                else
                    endY = image.Height - 2;
            }

            int[] pads = new int[] { startX, endX, startY, endY };

            if (splits != null && splits[0] == pads[0] && splits[1] == pads[1] && splits[2] == pads[2] && splits[3] == pads[3])
                return null;

            return pads;
        }

        private int GetSplitPoint (Bitmap image, string name, int startX, int startY, bool startPoint, bool xAxis)
        {
            int next = xAxis ? startX : startY;
            int end = xAxis ? image.Width : image.Height;
            int breakA = startPoint ? 255 : 0;

            int x = startX;
            int y = startY;

            while (next != end) {
                if (xAxis)
                    x = next;
                else
                    y = next;

                Color c = image.GetPixel(x, y);
                if (c.A == breakA)
                    return next;

                if (!startPoint && (c.R != 0 || c.G != 0 || c.B != 0 || c.A != 255))
                    throw new Exception(string.Format("Invalid {0} ninepatch split pixel at {1}, {2}, rgba: {3}, {4}, {5}, {6}",
                        name, x, y, c.R, c.G, c.B, c.A));

                next++;
            }

            return 0;
        }

        static private string Hash (Bitmap image)
        {
            SHA1Managed sha1 = new SHA1Managed();

            int width = image.Width;
            int height = image.Height;
            bool disposeImage = false;

            if (image.PixelFormat != PixelFormat.Format32bppArgb) {
                Bitmap newImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(image)) {
                    g.DrawImage(image, 0, 0);
                }
                image = newImage;
                disposeImage = true;
            }

            BitmapData imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            IntPtr scan0 = imageData.Scan0;
            byte[] data = new byte[imageData.Stride];

            for (int y = 0; y < height; y++) {
                Marshal.Copy(scan0 + y * imageData.Stride, data, 0, data.Length);
                sha1.TransformBlock(data, 0, data.Length, null, 0);
            }

            image.UnlockBits(imageData);

            sha1.TransformBlock(BitConverter.GetBytes(width), 0, 4, null, 0);
            sha1.TransformBlock(BitConverter.GetBytes(height), 0, 4, null, 0);

            sha1.TransformFinalBlock(new byte[0], 0, 0);

            if (disposeImage)
                image.Dispose();

            return BitConverter.ToString(sha1.Hash, 0, 16);
        }
    }
}
