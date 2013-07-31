using System;
using System.Collections.Generic;

namespace Treefrog.Presentation.Controllers
{
    public class ZoomMapper
    {
        private List<float> _zoomLevels;
        private List<string> _zoomLevelText;

        public ZoomMapper ()
        {
            _zoomLevels = new List<float>();
            _zoomLevelText = new List<string>();

            AddZoomLevel(.25f, "25%");
            AddZoomLevel(.33f, "33%");
            AddZoomLevel(.5f, "50%");
            AddZoomLevel(1f, "100%");
            AddZoomLevel(2f, "200%");
            AddZoomLevel(3f, "300%");
            AddZoomLevel(4f, "400%");
            AddZoomLevel(6f, "600%");
            AddZoomLevel(8f, "800%");
        }

        public int Count
        {
            get { return _zoomLevels.Count; }
        }

        public void AddZoomLevel (float level, string text)
        {
            _zoomLevels.Add(level);
            _zoomLevelText.Add(text);
        }

        public void RemoveZoomLevel (int index)
        {
            _zoomLevels.RemoveAt(index);
            _zoomLevelText.RemoveAt(index);
        }

        public int ZoomIndex (float level)
        {
            if (!_zoomLevels.Contains(level)) {
                foreach (float f in _zoomLevels) {
                    if (level >= f) {
                        level = f;
                    }
                }
            }

            return _zoomLevels.FindIndex(f => { return f == level; });
        }

        public float ZoomLevel (int index)
        {
            if (index < 0 || index >= _zoomLevels.Count)
                return 0;

            return _zoomLevels[index];
        }

        public string ZoomText (int index)
        {
            if (index < 0 || index >= _zoomLevels.Count)
                return "";

            return _zoomLevelText[index];
        }
    }

    public class ZoomState
    {
        private ZoomMapper _mapper;

        private int _zoomIndex;

        public ZoomState ()
        {
            _mapper = new ZoomMapper();
            _zoomIndex = _mapper.ZoomIndex(1f);
        }

        public int ZoomIndex
        {
            get { return _zoomIndex; }
            set
            {
                if (value < 0 || value >= _mapper.Count)
                    throw new ArgumentOutOfRangeException();

                if (_zoomIndex != value) {
                    _zoomIndex = value;
                    OnZoomLevelChanged(EventArgs.Empty);
                }
            }
        }

        public float ZoomLevel
        {
            get { return _mapper.ZoomLevel(_zoomIndex); }
            set
            {
                int index = _mapper.ZoomIndex(value);
                if (index < 0 || index > _mapper.Count)
                    throw new ArgumentOutOfRangeException();

                if (_zoomIndex != index) {
                    _zoomIndex = index;
                    OnZoomLevelChanged(EventArgs.Empty);
                }
            }
        }

        public string ZoomText
        {
            get { return _mapper.ZoomText(_zoomIndex); }
        }

        public bool CanZoomIn
        {
            get { return _zoomIndex < _mapper.Count - 1; }
        }

        public bool CanZoomOut
        {
            get { return _zoomIndex > 0; }
        }

        public void ZoomIn ()
        {
            if (CanZoomIn) {
                _zoomIndex++;
                OnZoomLevelChanged(EventArgs.Empty);
            }
        }

        public void ZoomOut ()
        {
            if (CanZoomOut) {
                _zoomIndex--;
                OnZoomLevelChanged(EventArgs.Empty);
            }
        }

        public event EventHandler ZoomLevelChanged;

        protected virtual void OnZoomLevelChanged (EventArgs e)
        {
            var ev = ZoomLevelChanged;
            if (ev != null)
                ev(this, e);
        }
    }
}
