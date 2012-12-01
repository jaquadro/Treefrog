using System;
using System.Windows.Forms;
using Treefrog.Presentation;
using Treefrog.Windows.Controls.Composite;

namespace Treefrog.Windows.Forms
{
    public partial class Main : Form
    {
        private StandardMenu _menu;
        private StandardToolbar _standardToolbar;
        private TileToolbar _tileToolbar;
        private InfoStatus _infoStatus;

        private EditorPresenter _editor;

        public Main ()
        {
            InitializeComponent();

            // Toolbars

            _menu = new StandardMenu();

            _standardToolbar = new StandardToolbar();
            _tileToolbar = new TileToolbar();

            toolStripContainer1.TopToolStripPanel.Controls.AddRange(new Control[] {
                _standardToolbar.Strip, 
                _tileToolbar.Strip
            });

            Controls.Add(_menu.Strip);
            MainMenuStrip = _menu.Strip;

            _infoStatus = new InfoStatus(statusBar);

            _editor = new EditorPresenter();
            _editor.SyncContentTabs += SyncContentTabsHandler;
            _editor.SyncContentView += SyncContentViewHandler;
            _editor.SyncModified += SyncProjectModified;

            _editor.NewDefault();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        }

        private void SyncContentTabsHandler (object sender, EventArgs e)
        {
            tabControlEx1.TabPages.Clear();

            foreach (ILevelPresenter lp in _editor.OpenContent) {
                TabPage page = new TabPage("Level");
                tabControlEx1.TabPages.Add(page);

                LevelPanel lpanel = new LevelPanel();
                lpanel.BindController(lp);
                lpanel.Dock = DockStyle.Fill;

                page.Controls.Add(lpanel);
            }
        }

        private void SyncContentViewHandler (object sender, EventArgs e)
        {
            ILevelPresenter lp = _editor.CurrentLevel;

            foreach (TabPage page in tabControlEx1.TabPages) {
                if (page.Text == lp.LayerControl.Name) {
                    tabControlEx1.SelectedTab = page;
                }
            }

            if (_editor.CanShowLayerPanel)
                layerPane1.BindController(_editor.Presentation.LayerList);

            if (_editor.CanShowTilePoolPanel)
                tilePoolPane1.BindController(_editor.Presentation.TilePoolList);

            if (_editor.CanShowObjectPoolPanel)
                objectPanel1.BindController(_editor.Presentation.ObjectPoolCollection);

            if (_editor.CanShowPropertyPanel)
                propertyPane1.BindController(_editor.Presentation.PropertyList);

            _menu.BindController(_editor);
            _menu.BindCommandManager(_editor.CommandManager);
            //_tileToolbar.BindController(_editor.Presentation.LevelTools);
            _tileToolbar.BindCommandManager(_editor.CurrentLevel.CommandManager);
            _standardToolbar.BindStandardToolsController(_editor.Presentation.StandardTools);
            _standardToolbar.BindDocumentToolsController(_editor.Presentation.DocumentTools);
            _standardToolbar.BindCommandManager(_editor.CommandManager);
            _infoStatus.BindController(_editor.Presentation.ContentInfo);
        }

        private void SyncProjectModified (object sender, EventArgs e)
        {
            if (_editor.Modified) {
                base.Text = "Treefrog [*]";
            }
            else {
                base.Text = "Treefrog";
            }
        }
    }
}
