using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace AvalonDock.Layout
{
    [Serializable]
    public abstract class LayoutPositionableGroup<T> : LayoutGroup<T>, ILayoutPositionableElement, ILayoutPositionableElementWithActualSize where T : class, ILayoutElement
    {
        public LayoutPositionableGroup()
        { }

        GridLength _dockWidth = new GridLength(1.0, GridUnitType.Star);
        public GridLength DockWidth
        {
            get
            {
                return _dockWidth;
            }
            set
            {
                if (DockWidth != value)
                {
                    RaisePropertyChanging("DockWidth");
                    _dockWidth = value;
                    RaisePropertyChanged("DockWidth");
                }
            }
        }

        GridLength _dockHeight = new GridLength(1.0, GridUnitType.Star);
        public GridLength DockHeight
        {
            get
            {
                return _dockHeight;
            }
            set
            {
                if (DockHeight != value)
                {
                    RaisePropertyChanging("DockHeight");
                    _dockHeight = value;
                    RaisePropertyChanged("DockHeight");
                }
            }
        }


        #region DockMinWidth

        private double _dockMinWidth = 25.0;
        public double DockMinWidth
        {
            get { return _dockMinWidth; }
            set
            {
                if (_dockMinWidth != value)
                {
                    MathHelper.AssertIsPositiveOrZero(value);
                    RaisePropertyChanging("DockMinWidth");
                    _dockMinWidth = value;
                    RaisePropertyChanged("DockMinWidth");
                }
            }
        }

        #endregion

        #region DockMinHeight

        private double _dockMinHeight = 25.0;
        public double DockMinHeight
        {
            get { return _dockMinHeight; }
            set
            {
                if (_dockMinHeight != value)
                {
                    MathHelper.AssertIsPositiveOrZero(value);
                    RaisePropertyChanging("DockMinHeight");
                    _dockMinHeight = value;
                    RaisePropertyChanged("DockMinHeight");
                }
            }
        }

        #endregion

        #region FloatingWidth

        private double _floatingWidth = 0.0;
        public double FloatingWidth
        {
            get { return _floatingWidth; }
            set
            {
                if (_floatingWidth != value)
                {
                    RaisePropertyChanging("FloatingWidth");
                    _floatingWidth = value;
                    RaisePropertyChanged("FloatingWidth");
                }
            }
        }

        #endregion

        #region FloatingHeight

        private double _floatingHeight = 0.0;
        public double FloatingHeight
        {
            get { return _floatingHeight; }
            set
            {
                if (_floatingHeight != value)
                {
                    RaisePropertyChanging("FloatingHeight");
                    _floatingHeight = value;
                    RaisePropertyChanged("FloatingHeight");
                }
            }
        }

        #endregion

        #region FloatingLeft

        private double _floatingLeft = 0.0;
        public double FloatingLeft
        {
            get { return _floatingLeft; }
            set
            {
                if (_floatingLeft != value)
                {
                    RaisePropertyChanging("FloatingLeft");
                    _floatingLeft = value;
                    RaisePropertyChanged("FloatingLeft");
                }
            }
        }

        #endregion

        #region FloatingTop

        private double _floatingTop = 0.0;
        public double FloatingTop
        {
            get { return _floatingTop; }
            set
            {
                if (_floatingTop != value)
                {
                    RaisePropertyChanging("FloatingTop");
                    _floatingTop = value;
                    RaisePropertyChanged("FloatingTop");
                }
            }
        }

        #endregion

        #region IsMaximized

        private bool _isMaximized = false;
        public bool IsMaximized
        {
            get { return _isMaximized; }
            set
            {
                if (_isMaximized != value)
                {
                    _isMaximized = value;
                    RaisePropertyChanged("IsMaximized");
                }
            }
        }

        #endregion



        [NonSerialized]
        double _actualWidth;
        double ILayoutPositionableElementWithActualSize.ActualWidth
        {
            get
            {
                return _actualWidth;
            }
            set
            {
                _actualWidth = value;
            }
        }
        
        [NonSerialized]
        double _actualHeight;
        double ILayoutPositionableElementWithActualSize.ActualHeight
        {
            get
            {
                return _actualHeight;
            }
            set
            {
                _actualHeight = value;
            }
        }

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            if (DockWidth.Value != 1.0 || !DockWidth.IsStar)
                writer.WriteAttributeString("DockWidth", DockWidth.ToString());
            if (DockHeight.Value != 1.0 || !DockHeight.IsStar)
                writer.WriteAttributeString("DockHeight", DockHeight.ToString());

            if (DockMinWidth != 25.0)
                writer.WriteAttributeString("DocMinWidth", DockMinWidth.ToString());
            if (DockMinHeight != 25.0)
                writer.WriteAttributeString("DockMinHeight", DockMinHeight.ToString());

            if (FloatingWidth != 0.0)
                writer.WriteAttributeString("FloatingWidth", FloatingWidth.ToString());
            if (FloatingHeight != 0.0)
                writer.WriteAttributeString("FloatingHeight", FloatingHeight.ToString());
            if (FloatingLeft != 0.0)
                writer.WriteAttributeString("FloatingLeft", FloatingLeft.ToString());
            if (FloatingTop != 0.0)
                writer.WriteAttributeString("FloatingTop", FloatingTop.ToString());
            
            base.WriteXml(writer);
        }

        static GridLengthConverter _gridLengthConverter = new GridLengthConverter();
        public override void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.MoveToAttribute("DockWidth"))
                _dockWidth = (GridLength)_gridLengthConverter.ConvertFromInvariantString(reader.Value);
            if (reader.MoveToAttribute("DockHeight"))
                _dockHeight = (GridLength)_gridLengthConverter.ConvertFromInvariantString(reader.Value);

            if (reader.MoveToAttribute("DocMinWidth"))
                _dockMinWidth = double.Parse(reader.Value);
            if (reader.MoveToAttribute("DocMinHeight"))
                _dockMinHeight = double.Parse(reader.Value);

            if (reader.MoveToAttribute("FloatingWidth"))
                _floatingWidth = double.Parse(reader.Value);
            if (reader.MoveToAttribute("FloatingHeight"))
                _floatingHeight = double.Parse(reader.Value);
            if (reader.MoveToAttribute("FloatingLeft"))
                _floatingLeft = double.Parse(reader.Value);
            if (reader.MoveToAttribute("FloatingTop"))
                _floatingTop = double.Parse(reader.Value);

            base.ReadXml(reader);
        }
    }
}
