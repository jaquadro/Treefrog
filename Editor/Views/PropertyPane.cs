using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Editor.Model;
using Editor.Controls;

namespace Editor.Views
{
    

    public class PropertyPane : UserControl
    {
        private ToolStripContainer toolStripContainer1;
        private Controls.EditableListView _propertyList;
        private ColumnHeader _colLabel;
        private ColumnHeader _colName;
        private ColumnHeader _colValue;
        private ToolStrip toolStrip1;
        private ToolStripButton _buttonAddProp;
        private ToolStripButton _buttonRemoveProp;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripTextBox toolStripTextBox1;

        private IPropertyProvider _propertyProvider;

        private ListViewGroup _groupPredefined;
        private ListViewGroup _groupCustom;

        #region Constructors

        public PropertyPane () {
            InitializeComponent();

            // Load form elements

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _buttonAddProp.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.tag--plus.png"));
            _buttonRemoveProp.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.tag--minus.png"));

            // Setup control
            _groupPredefined = new ListViewGroup("Predefined Properties");
            _groupCustom = new ListViewGroup("Custom Properties");

            _propertyList.Groups.Add(_groupPredefined);
            _propertyList.Groups.Add(_groupCustom);

            _propertyList.Items.Clear();

            _propertyList.SubItemClicked += PropertyListSubItemClickHandler;
            _propertyList.SubItemEndEditing += PropertyListEndEditingHandler;
        }

        #endregion

        #region Properties

        public IPropertyProvider PropertyProvider
        {
            get { return _propertyProvider; }
            set 
            {
                if (_propertyProvider != value) {
                    _propertyProvider = value;
                    OnPropertyProviderChanged(EventArgs.Empty);
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler PropertyProviderChanged;

        #endregion

        #region Event Dispatchers

        protected virtual void OnPropertyProviderChanged (EventArgs e)
        {
            _propertyList.Items.Clear();

            foreach (Property prop in _propertyProvider.PredefinedProperties) {
                ListViewItem item = new ListViewItem(new string[] { "", prop.Name, prop.ToString()});
                item.Group = _groupPredefined;
                _propertyList.Items.Add(item);
            }

            foreach (Property prop in _propertyProvider.CustomProperties) {
                ListViewItem item = new ListViewItem(new string[] { "", prop.Name, prop.ToString()});
                item.Group = _groupCustom;
                _propertyList.Items.Add(item);
            }

            if (PropertyProviderChanged != null) {
                PropertyProviderChanged(this, e);
            }
        }

        protected override void OnSizeChanged (EventArgs e)
        {
            base.OnSizeChanged(e);

            int width = _propertyList.Width - _colLabel.Width - 4;
            _colName.Width = width / 2;
            _colValue.Width = width / 2 + (width % 2);

            toolStrip1.CanOverflow = false;
            toolStripTextBox1.Width = toolStrip1.Width - _buttonAddProp.Width - _buttonRemoveProp.Width - toolStripSeparator1.Width - toolStrip1.Padding.Horizontal - _buttonAddProp.Margin.Horizontal - _buttonRemoveProp.Margin.Horizontal - toolStripSeparator1.Margin.Horizontal - toolStripTextBox1.Margin.Horizontal - 1;
        }

        #endregion

        #region Event Handlers

        private void PropertyListSubItemClickHandler (object sender, SubItemEventArgs e) {
            TextBox econtrol = new TextBox { Parent = this };

            _propertyList.StartEditing(econtrol, e.Item, e.SubItem);
        }

        private void PropertyListEndEditingHandler (object sender, SubItemEndEditingEventArgs e)
        {
            if (e.SubItem != 2) {
                return;
            }

            string name = e.Item.SubItems[1].Text;

            foreach (Property prop in _propertyProvider.PredefinedProperties) {
                if (prop.Name == name) {
                    prop.Parse(e.DisplayText);
                    break;
                }
            }
        }

        #endregion

        private void InitializeComponent ()
        {
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Predefined Properties", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Custom Properties", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "",
            "Location",
            "50,50"}, -1);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] {
            "",
            "Collision",
            "true"}, -1);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertyPane));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this._propertyList = new Editor.Controls.EditableListView();
            this._colLabel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this._buttonAddProp = new System.Windows.Forms.ToolStripButton();
            this._buttonRemoveProp = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this._propertyList);
            this.toolStripContainer1.ContentPanel.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(239, 264);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(239, 289);
            this.toolStripContainer1.TabIndex = 0;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // _propertyList
            // 
            this._propertyList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._colLabel,
            this._colName,
            this._colValue});
            this._propertyList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._propertyList.DoubleClickActivation = true;
            this._propertyList.FullRowSelect = true;
            listViewGroup1.Header = "Predefined Properties";
            listViewGroup1.Name = "_groupPredef";
            listViewGroup2.Header = "Custom Properties";
            listViewGroup2.Name = "_groupCustom";
            this._propertyList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this._propertyList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            listViewItem1.Group = listViewGroup1;
            listViewItem2.Group = listViewGroup2;
            this._propertyList.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2});
            this._propertyList.Location = new System.Drawing.Point(0, 0);
            this._propertyList.Margin = new System.Windows.Forms.Padding(0);
            this._propertyList.Name = "_propertyList";
            this._propertyList.Size = new System.Drawing.Size(239, 264);
            this._propertyList.TabIndex = 0;
            this._propertyList.UseCompatibleStateImageBehavior = false;
            this._propertyList.View = System.Windows.Forms.View.Details;
            // 
            // _colLabel
            // 
            this._colLabel.Text = "";
            this._colLabel.Width = 0;
            // 
            // _colName
            // 
            this._colName.Text = "Name";
            this._colName.Width = 100;
            // 
            // _colValue
            // 
            this._colValue.Text = "Value";
            this._colValue.Width = 135;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._buttonAddProp,
            this._buttonRemoveProp,
            this.toolStripSeparator1,
            this.toolStripTextBox1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(239, 25);
            this.toolStrip1.Stretch = true;
            this.toolStrip1.TabIndex = 0;
            // 
            // _buttonAddProp
            // 
            this._buttonAddProp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._buttonAddProp.Image = ((System.Drawing.Image)(resources.GetObject("_buttonAddProp.Image")));
            this._buttonAddProp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._buttonAddProp.Name = "_buttonAddProp";
            this._buttonAddProp.Size = new System.Drawing.Size(23, 22);
            this._buttonAddProp.Text = "toolStripButton1";
            // 
            // _buttonRemoveProp
            // 
            this._buttonRemoveProp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._buttonRemoveProp.Image = ((System.Drawing.Image)(resources.GetObject("_buttonRemoveProp.Image")));
            this._buttonRemoveProp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._buttonRemoveProp.Name = "_buttonRemoveProp";
            this._buttonRemoveProp.Size = new System.Drawing.Size(23, 22);
            this._buttonRemoveProp.Text = "toolStripButton2";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripTextBox1
            // 
            this.toolStripTextBox1.BorderStyle = BorderStyle.None;
            this.toolStripTextBox1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.TextBox.BorderStyle = BorderStyle.FixedSingle;
            this.toolStripTextBox1.TextBox.AutoSize = false;
            this.toolStripTextBox1.TextBox.Height = 22;
            this.toolStripTextBox1.AutoSize = false;
            this.toolStripTextBox1.Height = 22;
            this.toolStripTextBox1.Width = 150;
            // 
            // PropertyPane
            // 
            this.Controls.Add(this.toolStripContainer1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "PropertyPane";
            this.Size = new System.Drawing.Size(239, 289);
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}
