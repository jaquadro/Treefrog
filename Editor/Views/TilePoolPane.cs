using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Editor.Forms;
using System.IO;
using Editor.Model;
using Editor.Model.Controls;
using Treefrog.Framework;
using Treefrog.Framework.Model;

namespace Editor
{
    public partial class TilePoolPane : UserControl
    {
        private Project _project;

        private TilePoolCollection _pools;

        private TilePool _selected;
        //private TileSet1D _selectedSet;

        private TileSetControlLayer _tileLayer;

        private TilePoolPanelProperties _data;

        public TilePoolPane ()
        {
            InitializeComponent();

            // Load form elements

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _buttonRemove.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.minus16.png"));
            _buttonAdd.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.plus16.png"));
            _buttonEdit.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.pencil16.png"));

            // Setup control

            _tileControl.BackColor = Color.SlateGray;
            _tileControl.WidthSynced = true;
            _tileControl.Alignment = LayerControlAlignment.UpperLeft;

            _tileLayer = new TileSetControlLayer(_tileControl);
            _tileLayer.ShouldDrawContent = LayerCondition.Always;
            _tileLayer.ShouldDrawGrid = LayerCondition.Always;
            _tileLayer.ShouldRespondToInput = LayerCondition.Always;

            /*_tileControl.CanSelectRange = false;
            _tileControl.CanSelectDisjoint = false;
            _tileControl.Mode = TileControlMode.Select;*/

            importNewToolStripMenuItem.Click += ImportPoolHandler;

            _data = new TilePoolPanelProperties();
        }

        public TilePoolPane (Project project)
            : this()
        {
            SetupDefault(project);
        }

        public void SetupDefault (Project project)
        {
            _project = project;

            _pools = new TilePoolCollection(_project.TilePools);
            _pools.PoolAdded += PoolAddedHandler;
            _pools.PoolRemoved += PoolRemovedHandler;
            _pools.PoolRemapped += PoolRemappedHandler;

            _pools.Synchronize(_project.TilePools);

            // Configure default selection

            //_selected = _project.TilePools["Default"];
            //_selectedSet = TileSet1D.CreatePoolSet("Default", _selected);

            //_tileLayer.Layer = new TileSetLayer(_selectedSet);

            // Setup list

            foreach (TilePool pool in _project.TilePools) {
                _poolComboBox.Items.Add(pool.Name);
            }

            if (_poolComboBox.Items.Count > 0) {
                SelectPool(_poolComboBox.Items[0] as string);
                _poolComboBox.SelectedIndex = 0;
            }
            //_poolComboBox.SelectedItem = _selected.Name;

        }

        public LayerControl TileControl
        {
            get { return _tileControl; }
        }

        public TileControlLayer TileLayer
        {
            get { return _tileLayer; }
        }

        public PanelProperties PanelProperties
        {
            get { return _data; }
        }

        private void PoolAddedHandler (object sender, NamedResourceEventArgs<TilePool> e)
        {
            _poolComboBox.Items.Add(e.Name);

            //if (_poolComboBox.Items.Count == 1) {
                _poolComboBox.SelectedItem = e.Name;
                SelectPool(e.Name);
            //}
        }

        private void PoolRemovedHandler (object sender, NamedResourceEventArgs<TilePool> e)
        {
            _poolComboBox.Items.Remove(e.Name);

            if (_poolComboBox.Items.Count == 0) {
                _poolComboBox.SelectedItem = null;

                //_selectedSet = null;
                _selected = null;
                return;
            }

            if (_poolComboBox.SelectedText == e.Name) {
                foreach (string item in _poolComboBox.Items) {
                    _poolComboBox.SelectedItem = item;
                    SelectPool(item);
                    break;
                }
            }
        }

        private void PoolRemappedHandler (object sender, NamedResourceEventArgs<TilePool> e)
        {
            _poolComboBox.Items.Add(e.Resource.Name);

            if ((string)_poolComboBox.SelectedItem == e.Name) {
                _poolComboBox.SelectedItem = e.Resource.Name;
            }

            _poolComboBox.Items.Remove(e.Name);
        }

        private void ImportPoolHandler (object sender, EventArgs e)
        {
            ImportTilePool form = new ImportTilePool(_project);
            form.ShowDialog(this);

            /*using (FileStream fs = File.OpenRead(@"E:\Workspace\Managed\Treefrog\Tilesets\jungle_tiles.png")) {
                TilePool pool = TilePool.Import("Jungle", _project.Registry, fs, 16, 16, 1, 1);
                _project.TilePools.Add(pool);

                _selected = _pools["Jungle"].TilePool;
                _selectedSet = _pools["Jungle"].TileSet;

                _tileControl.TileSource = _selectedSet;

                _poolComboBox.SelectedItem = "Jungle";
            }*/
        }

        public void Deactivate ()
        {
            _project = null;
            _selected = null;

            _pools = null;
            _tileLayer.Layer = null;
        }

        public void Activate (EditorState project, PanelProperties properties)
        {
            _project = project.Project;

            _pools = new TilePoolCollection(_project.TilePools);
            _pools.PoolAdded += PoolAddedHandler;
            _pools.PoolRemoved += PoolRemovedHandler;
            _pools.PoolRemapped += PoolRemappedHandler;

            _pools.Synchronize(_project.TilePools);

            // Configure default selection

            //_selected = _project.TilePools["Default"];
            //_selectedSet = TileSet1D.CreatePoolSet("Default", _selected);

            //_tileLayer.Layer = new TileSetLayer(_selectedSet);

            // Setup list

            foreach (TilePool pool in _project.TilePools) {
                _poolComboBox.Items.Add(pool.Name);
            }

            if (_poolComboBox.Items.Count > 0) {
                SelectPool(_poolComboBox.Items[0] as string);
                _poolComboBox.SelectedIndex = 0;
            }
        }

        private class TilePoolRecord
        {
            private INamedResource _resource;

            public TilePool TilePool { get; private set; }
            //public TileSet1D TileSet { get; private set; }

            public string Name
            {
                get { return _resource.Name; }
            }

            public TilePoolRecord (string name, TilePool pool)
            {
                _resource = pool;
                TilePool = pool;

                //TileSet = TileSet1D.CreatePoolSet(name, pool);
            }
        }

        private class TilePoolCollection
        {
            private Dictionary<string, TilePoolRecord> _records;

            public event EventHandler<NamedResourceEventArgs<TilePool>> PoolAdded;
            public event EventHandler<NamedResourceEventArgs<TilePool>> PoolRemoved;
            public event EventHandler<NamedResourceEventArgs<TilePool>> PoolRemapped;

            public TilePoolCollection (NamedResourceCollection<TilePool> poolCollection)
            {
                _records = new Dictionary<string, TilePoolRecord>();

                poolCollection.ResourceAdded += ResourceAddedHandler;
                poolCollection.ResourceRemoved += ResourceRemovedHandler;
                poolCollection.ResourceRemapped += ResourceRemappedHandler;
            }

            public TilePoolRecord this[string name]
            {
                get { return _records[name]; }
            }

            public void Synchronize (NamedResourceCollection<TilePool> collection)
            {
                // Remove entries that have no match in collection

                List<TilePoolRecord> rlist = new List<TilePoolRecord>();
                foreach (TilePoolRecord rec in _records.Values) {
                    rlist.Add(rec);
                }

                foreach (TilePoolRecord rec in rlist) {
                    if (!collection.Contains(rec.Name)) {
                        _records.Remove(rec.Name);
                    }
                }

                // Add entries that have no match in records

                foreach (INamedResource nr in collection) {
                    if (!_records.ContainsKey(nr.Name)) {
                        _records[nr.Name] = new TilePoolRecord(nr.Name, nr as TilePool);
                    }
                }
            }

            private void ResourceAddedHandler (object sender, NamedResourceEventArgs<TilePool> e)
            {
                _records[e.Name] = new TilePoolRecord(e.Name, e.Resource);

                OnPoolAdded(e);
            }

            private void ResourceRemovedHandler (object sender, NamedResourceEventArgs<TilePool> e)
            {
                _records.Remove(e.Name);

                OnPoolRemoved(e);
            }

            private void ResourceRemappedHandler (object sender, NamedResourceEventArgs<TilePool> e)
            {
                TilePoolRecord record = _records[e.Name];
                _records[e.Resource.Name] = record;

                OnPoolRemapped(e);
            }

            private void OnPoolAdded (NamedResourceEventArgs<TilePool> e)
            {
                if (PoolAdded != null) {
                    PoolAdded(this, e);
                }
            }

            private void OnPoolRemoved (NamedResourceEventArgs<TilePool> e)
            {
                if (PoolRemoved != null) {
                    PoolRemoved(this, e);
                }
            }

            private void OnPoolRemapped (NamedResourceEventArgs<TilePool> e)
            {
                if (PoolRemapped != null) {
                    PoolRemapped(this, e);
                }
            }
        }

        private void _poolComboBox_SelectedIndexChanged (object sender, EventArgs e)
        {
            string item = (string)_poolComboBox.SelectedItem;
            SelectPool(item);
        }

        private void SelectPool (string item)
        {
            _selected = _pools[item].TilePool;

            _tileLayer.Layer = new TileSetLayer(_selected.Name, _selected);

            _tileControl.SetScrollSmallChange(ScrollOrientation.HorizontalScroll, _selected.TileWidth);
            _tileControl.SetScrollSmallChange(ScrollOrientation.VerticalScroll, _selected.TileHeight);
            _tileControl.SetScrollLargeChange(ScrollOrientation.HorizontalScroll, _selected.TileWidth * 4);
            _tileControl.SetScrollLargeChange(ScrollOrientation.VerticalScroll, _selected.TileHeight * 4);
        }
    }

    public class TilePoolPanelProperties : PanelProperties
    {

    }
}
