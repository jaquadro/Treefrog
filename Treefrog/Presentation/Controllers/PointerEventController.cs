using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Treefrog.Presentation.Layers;
using Treefrog.Presentation.Tools;
using System.Windows.Forms;

namespace Treefrog.Presentation.Controllers
{
    public interface IPointerTarget
    {
        Point OriginOffset { get; }
        Point InteriorOffset { get; }
        Point ScrollOffset { get; }
        float Zoom { get; }
    }

    public interface IPointerResponder
    {
        void HandleStartPointerSequence (PointerEventInfo info);
        void HandleEndPointerSequence (PointerEventInfo info);
        void HandleUpdatePointerSequence (PointerEventInfo info);
        void HandlePointerPosition (PointerEventInfo info);
        void HandlePointerLeaveField ();
    }

    public interface IPointerResponderProvider
    {
        IPointerResponder PointerEventResponder { get; }
        event EventHandler PointerEventResponderChanged;
    }

    public class PointerEventResponder : IPointerResponder
    {
        public virtual void HandleStartPointerSequence (PointerEventInfo info)
        { }

        public virtual void HandleEndPointerSequence (PointerEventInfo info)
        { }

        public virtual void HandleUpdatePointerSequence (PointerEventInfo info)
        { }

        public virtual void HandlePointerPosition (PointerEventInfo info)
        { }

        public virtual void HandlePointerLeaveField ()
        { }
    }

    public class PointerEventController
    {
        private IPointerTarget _target;
        private IPointerResponder _responder;

        private Dictionary<PointerEventType, bool> _sequenceOpen = new Dictionary<PointerEventType, bool>
        {
            { PointerEventType.Primary, false },
            { PointerEventType.Secondary, false },
        };

        public PointerEventController (IPointerTarget target)
        {
            _target = target;
        }

        public IPointerResponder Responder
        {
            get { return _responder; }
            set { _responder = value; }
        }

        private PointerEventType GetPointerType (MouseButtons button)
        {
            switch (button) {
                case MouseButtons.Left:
                    return PointerEventType.Primary;
                case MouseButtons.Right:
                    return PointerEventType.Secondary;
                default:
                    return PointerEventType.None;
            }
        }

        public Point TranslatePosition (Point position)
        {
            Point offset = _target.InteriorOffset;
            position.X = (int)((position.X - offset.X) / _target.Zoom);
            position.Y = (int)((position.Y - offset.Y) / _target.Zoom);

            Point scroll = _target.ScrollOffset;
            position.X += scroll.X;
            position.Y += scroll.Y;

            Point origin = _target.OriginOffset;
            position.X += origin.X;
            position.Y += origin.Y;

            return position;
        }

        public void TargetMouseDown (object sender, MouseEventArgs e)
        {
            PointerEventType type = GetPointerType(e.Button);
            if (_responder == null || type == PointerEventType.None)
                return;

            Point position = TranslatePosition(e.Location);
            PointerEventInfo info = new PointerEventInfo(type, position.X, position.Y);

            // Ignore event if a sequence is active
            if (_sequenceOpen.Count(kv => { return kv.Value; }) == 0) {
                _sequenceOpen[info.Type] = true;
                _responder.HandleStartPointerSequence(info);
            }
        }

        public void TargetMouseUp (object sender, MouseEventArgs e)
        {
            PointerEventType type = GetPointerType(e.Button);
            if (_responder == null || type == PointerEventType.None)
                return;

            Point position = TranslatePosition(e.Location);
            PointerEventInfo info = new PointerEventInfo(type, position.X, position.Y);

            if (_sequenceOpen[info.Type]) {
                _sequenceOpen[info.Type] = false;
                _responder.HandleEndPointerSequence(info);
            }
        }

        public void TargetMouseMove (object sender, MouseEventArgs e)
        {
            if (_responder == null)
                return;

            Point position = TranslatePosition(e.Location);

            if (_sequenceOpen[PointerEventType.Primary])
                _responder.HandleUpdatePointerSequence(new PointerEventInfo(PointerEventType.Primary, position.X, position.Y));
            if (_sequenceOpen[PointerEventType.Secondary])
                _responder.HandleUpdatePointerSequence(new PointerEventInfo(PointerEventType.Secondary, position.X, position.Y));

            _responder.HandlePointerPosition(new PointerEventInfo(PointerEventType.None, position.X, position.Y));
        }

        public void TargetMouseLeave (object sender, EventArgs e)
        {
            if (_responder != null)
                _responder.HandlePointerLeaveField();
        }

        public void TargetMouseClick (object sender, MouseEventArgs e)
        {
            if (_responder == null)
                return;

            Point position = TranslatePosition(e.Location);

            _responder.HandlePointerPosition(new PointerEventInfo(GetPointerType(e.Button), position.X, position.Y));
        }
    }
}
