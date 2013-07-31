using System.ComponentModel;
using Treefrog.Framework.Imaging;
using Treefrog.Utility;

namespace Treefrog.Presentation.Layers
{
    public class WorkspaceLayerPresenter : RenderLayerPresenter, INotifyPropertyChanged
    {
        private Color _patternColor1 = new Color(255, 255, 255, 255);
        private Color _patternColor2 = new Color(242, 242, 242, 255);
        private Color _borderColor = new Color(0, 0, 0, 255);
        private Color _originGuideColor = new Color(0, 0, 0, 128);

        public event PropertyChangedEventHandler PropertyChanged;

        public Color PatternColor1
        {
            get { return _patternColor1; }
            set
            {
                if (_patternColor1 != value) {
                    _patternColor1 = value;
                    PropertyChanged.Notify(() => PatternColor1);
                }
            }
        }

        public Color PatternColor2
        {
            get { return _patternColor2; }
            set
            {
                if (_patternColor2 != value) {
                    _patternColor2 = value;
                    PropertyChanged.Notify(() => PatternColor2);
                }
            }
        }

        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                if (_borderColor != value) {
                    _borderColor = value;
                    PropertyChanged.Notify(() => BorderColor);
                }
            }
        }

        public Color OriginGuideColor
        {
            get { return _originGuideColor; }
            set
            {
                if (_originGuideColor != value) {
                    _originGuideColor = value;
                    PropertyChanged.Notify(() => OriginGuideColor);
                }
            }
        }
    }
}
