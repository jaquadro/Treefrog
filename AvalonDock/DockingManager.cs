//Copyright (c) 2007-2012, Adolfo Marinucci
//All rights reserved.

//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the 
//following conditions are met:

//* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

//* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following 
//disclaimer in the documentation and/or other materials provided with the distribution.

//* Neither the name of Adolfo Marinucci nor the names of its contributors may be used to endorse or promote products
//derived from this software without specific prior written permission.

//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
//INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, 
//EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
//STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
//EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Markup;
using System.ComponentModel;
using System.Windows.Interop;
using System.Diagnostics;

using AvalonDock.Layout;
using AvalonDock.Controls;
using System.Windows.Input;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Data;
using System.Windows.Threading;
using AvalonDock.Commands;
using AvalonDock.Themes;

namespace AvalonDock
{
    [ContentProperty("Layout")]
    [TemplatePart(Name="PART_AutoHideArea")]
    public class DockingManager : Control, IOverlayWindowHost, ILogicalChildrenContainer
    {
        static DockingManager()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockingManager), new FrameworkPropertyMetadata(typeof(DockingManager)));
            FocusableProperty.OverrideMetadata(typeof(DockingManager), new FrameworkPropertyMetadata(false));
            HwndSource.DefaultAcquireHwndFocusInMenuMode = false;
            Keyboard.DefaultRestoreFocusMode = RestoreFocusMode.None;
        }


        public DockingManager()
        {
            Layout = new LayoutRoot() { RootPanel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane())) };
            this.Loaded += new RoutedEventHandler(DockingManager_Loaded);
            this.Unloaded += new RoutedEventHandler(DockingManager_Unloaded);
        }

        #region Layout

        /// <summary>
        /// Layout Dependency Property
        /// </summary>
        public static readonly DependencyProperty LayoutProperty =
            DependencyProperty.Register("Layout", typeof(LayoutRoot), typeof(DockingManager),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnLayoutChanged),
                    new CoerceValueCallback(CoerceLayoutValue)));

        /// <summary>
        /// Gets or sets the Layout property.  This dependency property 
        /// indicates layout tree.
        /// </summary>
        public LayoutRoot Layout
        {
            get { return (LayoutRoot)GetValue(LayoutProperty); }
            set { SetValue(LayoutProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Layout property.
        /// </summary>
        private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnLayoutChanged(e.OldValue as LayoutRoot, e.NewValue as LayoutRoot);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the <see cref="DockingManager.Layout"/> property.
        /// </summary>
        protected virtual void OnLayoutChanged(LayoutRoot oldLayout, LayoutRoot newLayout)
        {

            if (oldLayout != null)
            {
                oldLayout.PropertyChanged -= new PropertyChangedEventHandler(OnLayoutRootPropertyChanged);
                oldLayout.Updated -= new EventHandler(OnLayoutRootUpdated);
            }

            foreach (var fwc in _fwList.ToArray())
                fwc.InternalClose();

            _fwList.Clear();

            DetachDocumentsSource(oldLayout, DocumentsSource);
            DetachAnchorablesSource(oldLayout, AnchorablesSource);

            if (oldLayout != null &&
                oldLayout.Manager == this)
                oldLayout.Manager = null;

            ClearLogicalChildrenList();
            DetachLayoutItems();

            Layout.Manager = this;

            if (IsInitialized)
            {
                LayoutRootPanel = CreateUIElementForModel(Layout.RootPanel) as LayoutPanelControl;
                LeftSidePanel = CreateUIElementForModel(Layout.LeftSide) as LayoutAnchorSideControl;
                TopSidePanel = CreateUIElementForModel(Layout.TopSide) as LayoutAnchorSideControl;
                RightSidePanel = CreateUIElementForModel(Layout.RightSide) as LayoutAnchorSideControl;
                BottomSidePanel = CreateUIElementForModel(Layout.BottomSide) as LayoutAnchorSideControl;

                foreach (var fw in Layout.FloatingWindows.ToArray())
                {
                    if (fw.IsValid)
                       _fwList.Add(CreateUIElementForModel(fw) as LayoutFloatingWindowControl);
                }

                foreach (var fw in _fwList)
                    fw.Owner = Window.GetWindow(this);
            }

            AttachDocumentsSource(newLayout, DocumentsSource);
            AttachAnchorablesSource(newLayout, AnchorablesSource);

            if (newLayout != null)
            {
                newLayout.PropertyChanged += new PropertyChangedEventHandler(OnLayoutRootPropertyChanged);
                newLayout.Updated += new EventHandler(OnLayoutRootUpdated);
            }

            AttachLayoutItems();

            if (LayoutChanged != null)
                LayoutChanged(this, EventArgs.Empty);


            CommandManager.InvalidateRequerySuggested();
        }

        void OnLayoutRootPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RootPanel")
            {
                if (IsInitialized)
                {
                    var layoutRootPanel = CreateUIElementForModel(Layout.RootPanel) as LayoutPanelControl;
                    LayoutRootPanel = layoutRootPanel;
                }
            }
            else if (e.PropertyName == "ActiveContent")
            {
                if (Layout.ActiveContent != null)
                {
                    //set focus on active element only after a layout pass is completed
                    //it's possible that it is not yet visible in the visual tree
                    Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (Layout.ActiveContent != null)
                                FocusElementManager.SetFocusOnLastElement(Layout.ActiveContent);
                        }), DispatcherPriority.Background);
                }

                if (!_insideInternalSetActiveContent)
                    ActiveContent = Layout.ActiveContent != null ?
                        Layout.ActiveContent.Content : null;
            }
        }

        void OnLayoutRootUpdated(object sender, EventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
        }


        /// <summary>
        /// Event fired when <see cref="DockingManager.Layout"/> property changes
        /// </summary>
        public event EventHandler LayoutChanged;

        /// <summary>
        /// Coerces the <see cref="DockingManager.Layout"/> value.
        /// </summary>
        private static object CoerceLayoutValue(DependencyObject d, object value)
        {
            if (value == null)
                return new LayoutRoot() { RootPanel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane())) };

            return value;
        }


        /// <summary>
        /// Get or set the strategy object to use when insert an anchorable 
        /// in layout
        /// </summary>
        /// <remarks>Sometimes it's impossible to automatically insert an anchorable in the layout without specifing the target parent pane.
        /// Set this property to an object that will be asked to insert the anchorable to the desidered position.</remarks>
        public ILayoutUpdateStrategy LayoutUpdateStrategy
        {
            get;
            set;
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            SetupAutoHideArea();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (!DesignerProperties.GetIsInDesignMode(this) && Layout.Manager == this)
            {
                LayoutRootPanel = CreateUIElementForModel(Layout.RootPanel) as LayoutPanelControl;
                LeftSidePanel = CreateUIElementForModel(Layout.LeftSide) as LayoutAnchorSideControl;
                TopSidePanel = CreateUIElementForModel(Layout.TopSide) as LayoutAnchorSideControl;
                RightSidePanel = CreateUIElementForModel(Layout.RightSide) as LayoutAnchorSideControl;
                BottomSidePanel = CreateUIElementForModel(Layout.BottomSide) as LayoutAnchorSideControl;
                foreach (var fw in Layout.FloatingWindows)
                    _fwList.Add(CreateUIElementForModel(fw) as LayoutAnchorableFloatingWindowControl);
            }

        }

        void DockingManager_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                foreach (var fw in _fwList)
                    fw.Owner = Window.GetWindow(this);

                CreateOverlayWindow();
                FocusElementManager.SetupFocusManagement(this);
            } 
        }

        void DockingManager_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                foreach (var fw in _fwList.ToArray())
                {
                    fw.Owner = null;
                    fw.InternalClose();
                }

                DestroyOverlayWindow();
                FocusElementManager.FinalizeFocusManagement(this);
            }
        }

        internal UIElement CreateUIElementForModel(ILayoutElement model)
        {
            if (model is LayoutPanel)
                return new LayoutPanelControl(model as LayoutPanel);
            if (model is LayoutAnchorablePaneGroup)
                return new LayoutAnchorablePaneGroupControl(model as LayoutAnchorablePaneGroup);
            if (model is LayoutDocumentPaneGroup)
                return new LayoutDocumentPaneGroupControl(model as LayoutDocumentPaneGroup);

            if (model is LayoutAnchorSide)
            {
                var templateModelView = new LayoutAnchorSideControl(model as LayoutAnchorSide);
                templateModelView.SetBinding(LayoutAnchorSideControl.TemplateProperty, new Binding("AnchorSideTemplate") { Source = this });
                return templateModelView;
            }
            if (model is LayoutAnchorGroup)
            {
                var templateModelView = new LayoutAnchorGroupControl(model as LayoutAnchorGroup);
                templateModelView.SetBinding(LayoutAnchorGroupControl.TemplateProperty, new Binding("AnchorGroupTemplate") { Source = this });
                return templateModelView;
            }

            if (model is LayoutDocumentPane)
            {
                var templateModelView = new LayoutDocumentPaneControl(model as LayoutDocumentPane);
                templateModelView.SetBinding(LayoutDocumentPaneControl.StyleProperty, new Binding("DocumentPaneControlStyle") { Source = this });
                return templateModelView;
            }
            if (model is LayoutAnchorablePane)
            {
                var templateModelView = new LayoutAnchorablePaneControl(model as LayoutAnchorablePane);
                templateModelView.SetBinding(LayoutAnchorablePaneControl.StyleProperty, new Binding("AnchorablePaneControlStyle") { Source = this });
                return templateModelView;
            }

            if (model is LayoutAnchorableFloatingWindow)
            {
                if (DesignerProperties.GetIsInDesignMode(this))
                    return null;
                var modelFW = model as LayoutAnchorableFloatingWindow;
                var newFW = new LayoutAnchorableFloatingWindowControl(modelFW);

                var paneForExtentions = modelFW.RootPanel.Children.OfType<LayoutAnchorablePane>().FirstOrDefault();
                if (paneForExtentions != null)
                {
                    newFW.Left = paneForExtentions.FloatingLeft;
                    newFW.Top = paneForExtentions.FloatingTop;
                    newFW.Width = paneForExtentions.FloatingWidth;
                    newFW.Height = paneForExtentions.FloatingHeight;
                }

                newFW.ShowInTaskbar = false;
                newFW.Show();
                return newFW;
            }

            if (model is LayoutDocumentFloatingWindow)
            {
                if (DesignerProperties.GetIsInDesignMode(this))
                    return null;
                var modelFW = model as LayoutDocumentFloatingWindow;
                var newFW = new LayoutDocumentFloatingWindowControl(modelFW);

                var paneForExtentions = modelFW.RootDocument;
                if (paneForExtentions != null)
                {
                    newFW.Left = paneForExtentions.FloatingLeft;
                    newFW.Top = paneForExtentions.FloatingTop;
                    newFW.Width = paneForExtentions.FloatingWidth;
                    newFW.Height = paneForExtentions.FloatingHeight;
                }

                newFW.ShowInTaskbar = false;
                newFW.Show();
                return newFW;
            }

            if (model is LayoutDocument)
            {
                var templateModelView = new LayoutDocumentControl() { Model = model as LayoutDocument };
                return templateModelView;
            }

            return null;
        }

        

        #region DocumentPaneTemplate

        /// <summary>
        /// DocumentPaneTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty DocumentPaneTemplateProperty =
            DependencyProperty.Register("DocumentPaneTemplate", typeof(ControlTemplate), typeof(DockingManager),
                new FrameworkPropertyMetadata((ControlTemplate)null,
                    new PropertyChangedCallback(OnDocumentPaneTemplateChanged)));

        /// <summary>
        /// Gets or sets the DocumentPaneDataTemplate property.  This dependency property 
        /// indicates .
        /// </summary>
        public ControlTemplate DocumentPaneTemplate
        {
            get { return (ControlTemplate)GetValue(DocumentPaneTemplateProperty); }
            set { SetValue(DocumentPaneTemplateProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DocumentPaneTemplate property.
        /// </summary>
        private static void OnDocumentPaneTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnDocumentPaneTemplateChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DocumentPaneTemplate property.
        /// </summary>
        protected virtual void OnDocumentPaneTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region AnchorablePaneTemplate

        /// <summary>
        /// AnchorablePaneTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorablePaneTemplateProperty =
            DependencyProperty.Register("AnchorablePaneTemplate", typeof(ControlTemplate), typeof(DockingManager),
                new FrameworkPropertyMetadata((ControlTemplate)null,
                    new PropertyChangedCallback(OnAnchorablePaneTemplateChanged)));

        /// <summary>
        /// Gets or sets the AnchorablePaneTemplate property.  This dependency property 
        /// indicates ....
        /// </summary>
        public ControlTemplate AnchorablePaneTemplate
        {
            get { return (ControlTemplate)GetValue(AnchorablePaneTemplateProperty); }
            set { SetValue(AnchorablePaneTemplateProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AnchorablePaneDataTemplate property.
        /// </summary>
        private static void OnAnchorablePaneTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnAnchorablePaneTemplateChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AnchorablePaneDataTemplate property.
        /// </summary>
        protected virtual void OnAnchorablePaneTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region AnchorSideTemplate

        /// <summary>
        /// AnchorSideTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorSideTemplateProperty =
            DependencyProperty.Register("AnchorSideTemplate", typeof(ControlTemplate), typeof(DockingManager),
                new FrameworkPropertyMetadata((ControlTemplate)null));

        /// <summary>
        /// Gets or sets the AnchorSideTemplate property.  This dependency property 
        /// indicates ....
        /// </summary>
        public ControlTemplate AnchorSideTemplate
        {
            get { return (ControlTemplate)GetValue(AnchorSideTemplateProperty); }
            set { SetValue(AnchorSideTemplateProperty, value); }
        }

        #endregion

        #region AnchorGroupTemplate

        /// <summary>
        /// AnchorGroupTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorGroupTemplateProperty =
            DependencyProperty.Register("AnchorGroupTemplate", typeof(ControlTemplate), typeof(DockingManager),
                new FrameworkPropertyMetadata((ControlTemplate)null));

        /// <summary>
        /// Gets or sets the AnchorGroupTemplate property.  This dependency property 
        /// indicates the template used to render the AnchorGroup control.
        /// </summary>
        public ControlTemplate AnchorGroupTemplate
        {
            get { return (ControlTemplate)GetValue(AnchorGroupTemplateProperty); }
            set { SetValue(AnchorGroupTemplateProperty, value); }
        }

        #endregion

        #region AnchorTemplate

        /// <summary>
        /// AnchorTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorTemplateProperty =
            DependencyProperty.Register("AnchorTemplate", typeof(ControlTemplate), typeof(DockingManager),
                new FrameworkPropertyMetadata((ControlTemplate)null));

        /// <summary>
        /// Gets or sets the AnchorTemplate property.  This dependency property 
        /// indicates ....
        /// </summary>
        public ControlTemplate AnchorTemplate
        {
            get { return (ControlTemplate)GetValue(AnchorTemplateProperty); }
            set { SetValue(AnchorTemplateProperty, value); }
        }

        #endregion

        #region DocumentPaneControlStyle

        /// <summary>
        /// DocumentPaneControlStyle Dependency Property
        /// </summary>
        public static readonly DependencyProperty DocumentPaneControlStyleProperty =
            DependencyProperty.Register("DocumentPaneControlStyle", typeof(Style), typeof(DockingManager),
                new FrameworkPropertyMetadata((Style)null,
                    new PropertyChangedCallback(OnDocumentPaneControlStyleChanged)));

        /// <summary>
        /// Gets or sets the DocumentPaneControlStyle property.  This dependency property 
        /// indicates ....
        /// </summary>
        public Style DocumentPaneControlStyle
        {
            get { return (Style)GetValue(DocumentPaneControlStyleProperty); }
            set { SetValue(DocumentPaneControlStyleProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DocumentPaneControlStyle property.
        /// </summary>
        private static void OnDocumentPaneControlStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnDocumentPaneControlStyleChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DocumentPaneControlStyle property.
        /// </summary>
        protected virtual void OnDocumentPaneControlStyleChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region AnchorablePaneControlStyle

        /// <summary>
        /// AnchorablePaneControlStyle Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorablePaneControlStyleProperty =
            DependencyProperty.Register("AnchorablePaneControlStyle", typeof(Style), typeof(DockingManager),
                new FrameworkPropertyMetadata((Style)null,
                    new PropertyChangedCallback(OnAnchorablePaneControlStyleChanged)));

        /// <summary>
        /// Gets or sets the AnchorablePaneControlStyle property.  This dependency property 
        /// indicates the style to apply to AnchorablePaneControl.
        /// </summary>
        public Style AnchorablePaneControlStyle
        {
            get { return (Style)GetValue(AnchorablePaneControlStyleProperty); }
            set { SetValue(AnchorablePaneControlStyleProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AnchorablePaneControlStyle property.
        /// </summary>
        private static void OnAnchorablePaneControlStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnAnchorablePaneControlStyleChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AnchorablePaneControlStyle property.
        /// </summary>
        protected virtual void OnAnchorablePaneControlStyleChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region DocumentHeaderTemplate

        /// <summary>
        /// DocumentHeaderTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty DocumentHeaderTemplateProperty =
            DependencyProperty.Register("DocumentHeaderTemplate", typeof(DataTemplate), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplate)null,
                    new PropertyChangedCallback(OnDocumentHeaderTemplateChanged),
                    new CoerceValueCallback(CoerceDocumentHeaderTemplateValue)));

        /// <summary>
        /// Gets or sets the DocumentHeaderTemplate property.  This dependency property 
        /// indicates data template to use for document header.
        /// </summary>
        public DataTemplate DocumentHeaderTemplate
        {
            get { return (DataTemplate)GetValue(DocumentHeaderTemplateProperty); }
            set { SetValue(DocumentHeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DocumentHeaderTemplate property.
        /// </summary>
        private static void OnDocumentHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnDocumentHeaderTemplateChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DocumentHeaderTemplate property.
        /// </summary>
        protected virtual void OnDocumentHeaderTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
            if (DocumentPaneMenuItemHeaderTemplate == null)
                DocumentPaneMenuItemHeaderTemplate = DocumentHeaderTemplate;
        }

        /// <summary>
        /// Coerces the DocumentHeaderTemplate value.
        /// </summary>
        private static object CoerceDocumentHeaderTemplateValue(DependencyObject d, object value)
        {
            if (value != null &&
                d.GetValue(DocumentHeaderTemplateSelectorProperty) != null)
                return null;
            return value;
        }

        #endregion

        #region DocumentHeaderTemplateSelector

        /// <summary>
        /// DocumentHeaderTemplateSelector Dependency Property
        /// </summary>
        public static readonly DependencyProperty DocumentHeaderTemplateSelectorProperty =
            DependencyProperty.Register("DocumentHeaderTemplateSelector", typeof(DataTemplateSelector), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplateSelector)null,
                    new PropertyChangedCallback(OnDocumentHeaderTemplateSelectorChanged),
                    new CoerceValueCallback(CoerceDocumentHeaderTemplateSelectorValue)));

        /// <summary>
        /// Gets or sets the DocumentHeaderTemplateSelector property.  This dependency property 
        /// indicates the template selector that is used when selcting the data template for the header.
        /// </summary>
        public DataTemplateSelector DocumentHeaderTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(DocumentHeaderTemplateSelectorProperty); }
            set { SetValue(DocumentHeaderTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DocumentHeaderTemplateSelector property.
        /// </summary>
        private static void OnDocumentHeaderTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnDocumentHeaderTemplateSelectorChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DocumentHeaderTemplateSelector property.
        /// </summary>
        protected virtual void OnDocumentHeaderTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null &&
                DocumentHeaderTemplate != null)
                DocumentHeaderTemplate = null;

            if (DocumentPaneMenuItemHeaderTemplateSelector == null)
                DocumentPaneMenuItemHeaderTemplateSelector = DocumentHeaderTemplateSelector;

        }

        /// <summary>
        /// Coerces the DocumentHeaderTemplateSelector value.
        /// </summary>
        private static object CoerceDocumentHeaderTemplateSelectorValue(DependencyObject d, object value)
        {
            return value;
        }

        #endregion

        #region DocumentTitleTemplate

        /// <summary>
        /// DocumentTitleTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty DocumentTitleTemplateProperty =
            DependencyProperty.Register("DocumentTitleTemplate", typeof(DataTemplate), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplate)null,
                    new PropertyChangedCallback(OnDocumentTitleTemplateChanged),
                    new CoerceValueCallback(CoerceDocumentTitleTemplateValue)));

        /// <summary>
        /// Gets or sets the DocumentTitleTemplate property.  This dependency property 
        /// indicates the datatemplate to use when creating the title for a document.
        /// </summary>
        public DataTemplate DocumentTitleTemplate
        {
            get { return (DataTemplate)GetValue(DocumentTitleTemplateProperty); }
            set { SetValue(DocumentTitleTemplateProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DocumentTitleTemplate property.
        /// </summary>
        private static void OnDocumentTitleTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnDocumentTitleTemplateChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DocumentTitleTemplate property.
        /// </summary>
        protected virtual void OnDocumentTitleTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Coerces the DocumentTitleTemplate value.
        /// </summary>
        private static object CoerceDocumentTitleTemplateValue(DependencyObject d, object value)
        {
            if (value != null &&
                d.GetValue(DocumentTitleTemplateSelectorProperty) != null)
                return null;

            return value;
        }

        #endregion

        #region DocumentTitleTemplateSelector

        /// <summary>
        /// DocumentTitleTemplateSelector Dependency Property
        /// </summary>
        public static readonly DependencyProperty DocumentTitleTemplateSelectorProperty =
            DependencyProperty.Register("DocumentTitleTemplateSelector", typeof(DataTemplateSelector), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplateSelector)null,
                    new PropertyChangedCallback(OnDocumentTitleTemplateSelectorChanged),
                    new CoerceValueCallback(CoerceDocumentTitleTemplateSelectorValue)));

        /// <summary>
        /// Gets or sets the DocumentTitleTemplateSelector property.  This dependency property 
        /// indicates the data template selector to use when creating the data template for the title.
        /// </summary>
        public DataTemplateSelector DocumentTitleTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(DocumentTitleTemplateSelectorProperty); }
            set { SetValue(DocumentTitleTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DocumentTitleTemplateSelector property.
        /// </summary>
        private static void OnDocumentTitleTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnDocumentTitleTemplateSelectorChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DocumentTitleTemplateSelector property.
        /// </summary>
        protected virtual void OnDocumentTitleTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                DocumentTitleTemplate = null;                
        }

        /// <summary>
        /// Coerces the DocumentTitleTemplateSelector value.
        /// </summary>
        private static object CoerceDocumentTitleTemplateSelectorValue(DependencyObject d, object value)
        {
            return value;
        }

        #endregion

        #region AnchorableTitleTemplate

        /// <summary>
        /// AnchorableTitleTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorableTitleTemplateProperty =
            DependencyProperty.Register("AnchorableTitleTemplate", typeof(DataTemplate), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplate)null,
                    new PropertyChangedCallback(OnAnchorableTitleTemplateChanged),
                    new CoerceValueCallback(CoerceAnchorableTitleTemplateValue)));

        /// <summary>
        /// Gets or sets the AnchorableTitleTemplate property.  This dependency property 
        /// indicates the data template to use for anchorables title.
        /// </summary>
        public DataTemplate AnchorableTitleTemplate
        {
            get { return (DataTemplate)GetValue(AnchorableTitleTemplateProperty); }
            set { SetValue(AnchorableTitleTemplateProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AnchorableTitleTemplate property.
        /// </summary>
        private static void OnAnchorableTitleTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnAnchorableTitleTemplateChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AnchorableTitleTemplate property.
        /// </summary>
        protected virtual void OnAnchorableTitleTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Coerces the AnchorableTitleTemplate value.
        /// </summary>
        private static object CoerceAnchorableTitleTemplateValue(DependencyObject d, object value)
        {
            if (value != null &&
                d.GetValue(AnchorableTitleTemplateSelectorProperty) != null)
                return null;
            return value;
        }

        #endregion

        #region AnchorableTitleTemplateSelector

        /// <summary>
        /// AnchorableTitleTemplateSelector Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorableTitleTemplateSelectorProperty =
            DependencyProperty.Register("AnchorableTitleTemplateSelector", typeof(DataTemplateSelector), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplateSelector)null,
                    new PropertyChangedCallback(OnAnchorableTitleTemplateSelectorChanged)));

        /// <summary>
        /// Gets or sets the AnchorableTitleTemplateSelector property.  This dependency property 
        /// indicates selctor to use when selecting data template for the title of anchorables.
        /// </summary>
        public DataTemplateSelector AnchorableTitleTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(AnchorableTitleTemplateSelectorProperty); }
            set { SetValue(AnchorableTitleTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AnchorableTitleTemplateSelector property.
        /// </summary>
        private static void OnAnchorableTitleTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnAnchorableTitleTemplateSelectorChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AnchorableTitleTemplateSelector property.
        /// </summary>
        protected virtual void OnAnchorableTitleTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null &&
                AnchorableTitleTemplate != null)
                AnchorableTitleTemplate = null;
        }

        #endregion

        #region AnchorableHeaderTemplate

        /// <summary>
        /// AnchorableHeaderTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorableHeaderTemplateProperty =
            DependencyProperty.Register("AnchorableHeaderTemplate", typeof(DataTemplate), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplate)null,
                    new PropertyChangedCallback(OnAnchorableHeaderTemplateChanged),
                    new CoerceValueCallback(CoerceAnchorableHeaderTemplateValue)));

        /// <summary>
        /// Gets or sets the AnchorableHeaderTemplate property.  This dependency property 
        /// indicates the data template to use for anchorable templates.
        /// </summary>
        public DataTemplate AnchorableHeaderTemplate
        {
            get { return (DataTemplate)GetValue(AnchorableHeaderTemplateProperty); }
            set { SetValue(AnchorableHeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AnchorableHeaderTemplate property.
        /// </summary>
        private static void OnAnchorableHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnAnchorableHeaderTemplateChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AnchorableHeaderTemplate property.
        /// </summary>
        protected virtual void OnAnchorableHeaderTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Coerces the AnchorableHeaderTemplate value.
        /// </summary>
        private static object CoerceAnchorableHeaderTemplateValue(DependencyObject d, object value)
        {
            if (value != null &&
                d.GetValue(AnchorableHeaderTemplateSelectorProperty) != null)
                return null;

            return value;
        }

        #endregion

        #region AnchorableHeaderTemplateSelector

        /// <summary>
        /// AnchorableHeaderTemplateSelector Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorableHeaderTemplateSelectorProperty =
            DependencyProperty.Register("AnchorableHeaderTemplateSelector", typeof(DataTemplateSelector), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplateSelector)null,
                    new PropertyChangedCallback(OnAnchorableHeaderTemplateSelectorChanged)));

        /// <summary>
        /// Gets or sets the AnchorableHeaderTemplateSelector property.  This dependency property 
        /// indicates the selector to use when selecting the data template for anchorable headers.
        /// </summary>
        public DataTemplateSelector AnchorableHeaderTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(AnchorableHeaderTemplateSelectorProperty); }
            set { SetValue(AnchorableHeaderTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AnchorableHeaderTemplateSelector property.
        /// </summary>
        private static void OnAnchorableHeaderTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnAnchorableHeaderTemplateSelectorChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AnchorableHeaderTemplateSelector property.
        /// </summary>
        protected virtual void OnAnchorableHeaderTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                AnchorableHeaderTemplate = null;
        }

        #endregion

        protected override void OnGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            //if (e.NewFocus is Grid)
            //    Debug.WriteLine(string.Format("DockingManager.OnGotKeyboardFocus({0})", e.NewFocus));
            base.OnGotKeyboardFocus(e);
        }

        protected override void OnPreviewGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            //Debug.WriteLine(string.Format("DockingManager.OnPreviewGotKeyboardFocus({0})", e.NewFocus));
            base.OnPreviewGotKeyboardFocus(e);
        }

        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            //Debug.WriteLine(string.Format("DockingManager.OnPreviewLostKeyboardFocus({0})", e.OldFocus));
            base.OnPreviewLostKeyboardFocus(e);
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            Debug.WriteLine(string.Format("DockingManager.OnMouseLeftButtonDown([{0}])", e.GetPosition(this)));
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            //Debug.WriteLine(string.Format("DockingManager.OnMouseMove([{0}])", e.GetPosition(this)));
            base.OnMouseMove(e);
        }

        #region LayoutRootPanel

        /// <summary>
        /// LayoutRootPanel Dependency Property
        /// </summary>
        public static readonly DependencyProperty LayoutRootPanelProperty =
            DependencyProperty.Register("LayoutRootPanel", typeof(LayoutPanelControl), typeof(DockingManager),
                new FrameworkPropertyMetadata((LayoutPanelControl)null,
                    new PropertyChangedCallback(OnLayoutRootPanelChanged)));

        /// <summary>
        /// Gets or sets the LayoutRootPanel property.  This dependency property 
        /// indicates the layout panel control which is attached to the Layout.Root property.
        /// </summary>
        public LayoutPanelControl LayoutRootPanel
        {
            get { return (LayoutPanelControl)GetValue(LayoutRootPanelProperty); }
            set { SetValue(LayoutRootPanelProperty, value); }
        }

        /// <summary>
        /// Handles changes to the LayoutRootPanel property.
        /// </summary>
        private static void OnLayoutRootPanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnLayoutRootPanelChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the LayoutRootPanel property.
        /// </summary>
        protected virtual void OnLayoutRootPanelChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
                ((ILogicalChildrenContainer)this).InternalRemoveLogicalChild(e.OldValue);
            if (e.NewValue != null)
                ((ILogicalChildrenContainer)this).InternalAddLogicalChild(e.NewValue);
        }

        #endregion

        #region RightSidePanel

        /// <summary>
        /// RightSidePanel Dependency Property
        /// </summary>
        public static readonly DependencyProperty RightSidePanelProperty =
            DependencyProperty.Register("RightSidePanel", typeof(LayoutAnchorSideControl), typeof(DockingManager),
                new FrameworkPropertyMetadata((LayoutAnchorSideControl)null,
                    new PropertyChangedCallback(OnRightSidePanelChanged)));

        /// <summary>
        /// Gets or sets the RightSidePanel property.  This dependency property 
        /// indicates right side anchor panel.
        /// </summary>
        public LayoutAnchorSideControl RightSidePanel
        {
            get { return (LayoutAnchorSideControl)GetValue(RightSidePanelProperty); }
            set { SetValue(RightSidePanelProperty, value); }
        }

        /// <summary>
        /// Handles changes to the RightSidePanel property.
        /// </summary>
        private static void OnRightSidePanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnRightSidePanelChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the RightSidePanel property.
        /// </summary>
        protected virtual void OnRightSidePanelChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
                ((ILogicalChildrenContainer)this).InternalRemoveLogicalChild(e.OldValue);
            if (e.NewValue != null)
                ((ILogicalChildrenContainer)this).InternalAddLogicalChild(e.NewValue);
        }

        #endregion

        #region LeftSidePanel

        /// <summary>
        /// LeftSidePanel Dependency Property
        /// </summary>
        public static readonly DependencyProperty LeftSidePanelProperty =
            DependencyProperty.Register("LeftSidePanel", typeof(LayoutAnchorSideControl), typeof(DockingManager),
                new FrameworkPropertyMetadata((LayoutAnchorSideControl)null,
                    new PropertyChangedCallback(OnLeftSidePanelChanged)));

        /// <summary>
        /// Gets or sets the LeftSidePanel property.  This dependency property 
        /// indicates the left side panel control.
        /// </summary>
        public LayoutAnchorSideControl LeftSidePanel
        {
            get { return (LayoutAnchorSideControl)GetValue(LeftSidePanelProperty); }
            set { SetValue(LeftSidePanelProperty, value); }
        }

        /// <summary>
        /// Handles changes to the LeftSidePanel property.
        /// </summary>
        private static void OnLeftSidePanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnLeftSidePanelChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the LeftSidePanel property.
        /// </summary>
        protected virtual void OnLeftSidePanelChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
                ((ILogicalChildrenContainer)this).InternalRemoveLogicalChild(e.OldValue);
            if (e.NewValue != null)
                ((ILogicalChildrenContainer)this).InternalAddLogicalChild(e.NewValue);
        }

        #endregion

        #region TopSidePanel

        /// <summary>
        /// TopSidePanel Dependency Property
        /// </summary>
        public static readonly DependencyProperty TopSidePanelProperty =
            DependencyProperty.Register("TopSidePanel", typeof(LayoutAnchorSideControl), typeof(DockingManager),
                new FrameworkPropertyMetadata((LayoutAnchorSideControl)null,
                    new PropertyChangedCallback(OnTopSidePanelChanged)));

        /// <summary>
        /// Gets or sets the TopSidePanel property.  This dependency property 
        /// indicates top side control panel.
        /// </summary>
        public LayoutAnchorSideControl TopSidePanel
        {
            get { return (LayoutAnchorSideControl)GetValue(TopSidePanelProperty); }
            set { SetValue(TopSidePanelProperty, value); }
        }

        /// <summary>
        /// Handles changes to the TopSidePanel property.
        /// </summary>
        private static void OnTopSidePanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnTopSidePanelChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the TopSidePanel property.
        /// </summary>
        protected virtual void OnTopSidePanelChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
                ((ILogicalChildrenContainer)this).InternalRemoveLogicalChild(e.OldValue);
            if (e.NewValue != null)
                ((ILogicalChildrenContainer)this).InternalAddLogicalChild(e.NewValue);
        }

        #endregion

        #region BottomSidePanel

        /// <summary>
        /// BottomSidePanel Dependency Property
        /// </summary>
        public static readonly DependencyProperty BottomSidePanelProperty =
            DependencyProperty.Register("BottomSidePanel", typeof(LayoutAnchorSideControl), typeof(DockingManager),
                new FrameworkPropertyMetadata((LayoutAnchorSideControl)null,
                    new PropertyChangedCallback(OnBottomSidePanelChanged)));

        /// <summary>
        /// Gets or sets the BottomSidePanel property.  This dependency property 
        /// indicates bottom side panel control.
        /// </summary>
        public LayoutAnchorSideControl BottomSidePanel
        {
            get { return (LayoutAnchorSideControl)GetValue(BottomSidePanelProperty); }
            set { SetValue(BottomSidePanelProperty, value); }
        }

        /// <summary>
        /// Handles changes to the BottomSidePanel property.
        /// </summary>
        private static void OnBottomSidePanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnBottomSidePanelChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the BottomSidePanel property.
        /// </summary>
        protected virtual void OnBottomSidePanelChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
                ((ILogicalChildrenContainer)this).InternalRemoveLogicalChild(e.OldValue);
            if (e.NewValue != null)
                ((ILogicalChildrenContainer)this).InternalAddLogicalChild(e.NewValue);
        }

        #endregion


        #region LogicalChildren

        List<object> _logicalChildren = new List<object>();

        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
                return _logicalChildren.GetEnumerator();
            }
        }


        void ILogicalChildrenContainer.InternalAddLogicalChild(object element)
        {
            //System.Diagnostics.Debug.WriteLine("[{0}]InternalAddLogicalChild({1})", this, element);

            if (_logicalChildren.Contains(element))
                throw new InvalidOperationException();
            _logicalChildren.Add(element);
            AddLogicalChild(element);
        }

        void ILogicalChildrenContainer.InternalRemoveLogicalChild(object element)
        {
            //System.Diagnostics.Debug.WriteLine("[{0}]InternalRemoveLogicalChild({1})", this, element);

            if (_logicalChildren.Contains(element))
            {
                _logicalChildren.Remove(element);
                RemoveLogicalChild(element);
            }
        }

        void ClearLogicalChildrenList()
        {
            foreach (var child in _logicalChildren.ToArray())
            {
                _logicalChildren.Remove(child);
                RemoveLogicalChild(child);
            }
        }

        #endregion  
    
        #region AutoHide window
        internal void ShowAutoHideWindow(LayoutAnchorControl anchor)
        {
            if (_autohideArea == null)
                return;

            if (AutoHideWindow != null && AutoHideWindow.Model == anchor.Model)
                return;

            HideAutoHideWindow();

            SetAutoHideWindow(new LayoutAutoHideWindowControl(anchor));
        }

        internal void HideAutoHideWindow()
        {
            if (AutoHideWindow != null)
            {
                AutoHideWindow.Dispose();
                SetAutoHideWindow(null);
            }
        }

        FrameworkElement _autohideArea;
        internal FrameworkElement GetAutoHideAreaElement()
        {
            return _autohideArea;
        }

        void SetupAutoHideArea()
        {
            _autohideArea = GetTemplateChild("PART_AutoHideArea") as FrameworkElement;
        }

        #region AutoHideWindow

        /// <summary>
        /// AutoHideWindow Read-Only Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey AutoHideWindowPropertyKey
            = DependencyProperty.RegisterReadOnly("AutoHideWindow", typeof(LayoutAutoHideWindowControl), typeof(DockingManager),
                new FrameworkPropertyMetadata((LayoutAutoHideWindowControl)null,
                    new PropertyChangedCallback(OnAutoHideWindowChanged)));

        public static readonly DependencyProperty AutoHideWindowProperty
            = AutoHideWindowPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the AutoHideWindow property.  This dependency property 
        /// indicates the currently shown autohide window.
        /// </summary>
        public LayoutAutoHideWindowControl AutoHideWindow
        {
            get { return (LayoutAutoHideWindowControl)GetValue(AutoHideWindowProperty); }
        }

        /// <summary>
        /// Provides a secure method for setting the AutoHideWindow property.  
        /// This dependency property indicates the currently shown autohide window.
        /// </summary>
        /// <param name="value">The new value for the property.</param>
        protected void SetAutoHideWindow(LayoutAutoHideWindowControl value)
        {
            SetValue(AutoHideWindowPropertyKey, value);
        }

        /// <summary>
        /// Handles changes to the AutoHideWindow property.
        /// </summary>
        private static void OnAutoHideWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnAutoHideWindowChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AutoHideWindow property.
        /// </summary>
        protected virtual void OnAutoHideWindowChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
                ((ILogicalChildrenContainer)this).InternalRemoveLogicalChild(e.OldValue);
            if (e.NewValue != null)
                ((ILogicalChildrenContainer)this).InternalAddLogicalChild(e.NewValue);
        }

        #endregion



        #endregion

        #region Floating Windows
        List<LayoutFloatingWindowControl> _fwList = new List<LayoutFloatingWindowControl>();

        internal void StartDraggingFloatingWindowForContent(LayoutContent contentModel, bool startDrag = true)
        {
            var contentModelAsAnchorable = contentModel as LayoutAnchorable;
            if (contentModelAsAnchorable != null &&
                contentModelAsAnchorable.IsAutoHidden)
                contentModelAsAnchorable.ToggleAutoHide();

            var parentPane = contentModel.Parent as ILayoutPane;
            var parentPaneAsPositionableElement = contentModel.Parent as ILayoutPositionableElement;
            var parentPaneAsWithActualSize = contentModel.Parent as ILayoutPositionableElementWithActualSize;
            var contentModelParentChildrenIndex = parentPane.Children.ToList().IndexOf(contentModel);

            if (contentModel.FindParent<LayoutFloatingWindow>() == null)
            {
                contentModel.PreviousContainer = parentPane;
                contentModel.PreviousContainerIndex = contentModelParentChildrenIndex;
            }

            parentPane.RemoveChildAt(contentModelParentChildrenIndex);

            double fwWidth = parentPaneAsPositionableElement.FloatingWidth;
            double fwHeight = parentPaneAsPositionableElement.FloatingHeight;

            if (fwWidth == 0.0)
                fwWidth = parentPaneAsWithActualSize.ActualWidth;
            if (fwHeight == 0.0)
                fwHeight = parentPaneAsWithActualSize.ActualHeight;

            LayoutFloatingWindow fw;
            LayoutFloatingWindowControl fwc;
            if (contentModel is LayoutAnchorable)
            {
                var anchorableContent = contentModel as LayoutAnchorable;
                fw = new LayoutAnchorableFloatingWindow()
                {
                    RootPanel = new LayoutAnchorablePaneGroup(
                        new LayoutAnchorablePane(anchorableContent)
                        {
                            DockWidth = parentPaneAsPositionableElement.DockWidth,
                            DockHeight = parentPaneAsPositionableElement.DockHeight,
                            DockMinHeight = parentPaneAsPositionableElement.DockMinHeight,
                            DockMinWidth = parentPaneAsPositionableElement.DockMinWidth,
                            FloatingLeft = parentPaneAsPositionableElement.FloatingLeft,
                            FloatingTop = parentPaneAsPositionableElement.FloatingTop,
                            FloatingWidth = parentPaneAsPositionableElement.FloatingWidth,
                            FloatingHeight = parentPaneAsPositionableElement.FloatingHeight,
                        })
                };

                fwc = new LayoutAnchorableFloatingWindowControl(
                    fw as LayoutAnchorableFloatingWindow)
                    {
                        Width = fwWidth,
                        Height = fwHeight
                    };
            }
            else
            {
                var anchorableDocument = contentModel as LayoutDocument;
                fw = new LayoutDocumentFloatingWindow()
                {
                    RootDocument = anchorableDocument
                };

                fwc = new LayoutDocumentFloatingWindowControl(
                    fw as LayoutDocumentFloatingWindow)
                {
                    Width = fwWidth,
                    Height = fwHeight
                };
            }
            
            Layout.FloatingWindows.Add(fw);

            fwc.Owner = Window.GetWindow(this);

            _fwList.Add(fwc);
            
            Layout.CollectGarbage();

            if (startDrag)
                fwc.AttachDrag();
            fwc.Show();
        }

        internal void StartDraggingFloatingWindowForPane(LayoutAnchorablePane paneModel)
        {
            var paneAsPositionableElement = paneModel as ILayoutPositionableElement;
            var paneAsWithActualSize = paneModel as ILayoutPositionableElementWithActualSize;

            double fwWidth = paneAsPositionableElement.FloatingWidth;
            double fwHeight = paneAsPositionableElement.FloatingHeight;

            if (fwWidth == 0.0)
                fwWidth = paneAsWithActualSize.ActualWidth;
            if (fwHeight == 0.0)
                fwHeight = paneAsWithActualSize.ActualHeight;

            var destPane = new LayoutAnchorablePane()
            {
                DockWidth = paneAsPositionableElement.DockWidth,
                DockHeight = paneAsPositionableElement.DockHeight,
                DockMinHeight = paneAsPositionableElement.DockMinHeight,
                DockMinWidth = paneAsPositionableElement.DockMinWidth,
                FloatingLeft = paneAsPositionableElement.FloatingLeft,
                FloatingTop = paneAsPositionableElement.FloatingTop,
                FloatingWidth = paneAsPositionableElement.FloatingWidth,
                FloatingHeight = paneAsPositionableElement.FloatingHeight,
            };

            bool savePreviousContainer = paneModel.FindParent<LayoutFloatingWindow>() == null;
            int currentSelectedContentIndex = paneModel.SelectedContentIndex;
            while (paneModel.Children.Count > 0)
            {
                var contentModel = paneModel.Children[paneModel.Children.Count - 1] as LayoutAnchorable;

                if (savePreviousContainer)
                {
                    contentModel.PreviousContainer = paneModel;
                    contentModel.PreviousContainerIndex = paneModel.Children.Count - 1;
                }

                paneModel.RemoveChildAt(paneModel.Children.Count - 1);
                destPane.Children.Insert(0, contentModel);
            }

            if (destPane.Children.Count > 0)
            {
                destPane.SelectedContentIndex = currentSelectedContentIndex;
            }


            LayoutFloatingWindow fw;
            LayoutFloatingWindowControl fwc;
            fw = new LayoutAnchorableFloatingWindow()
            {
                RootPanel = new LayoutAnchorablePaneGroup(
                    destPane)
            };

            fwc = new LayoutAnchorableFloatingWindowControl(
                fw as LayoutAnchorableFloatingWindow)
            {
                Width = fwWidth,
                Height = fwHeight
            };
            

            Layout.FloatingWindows.Add(fw);

            fwc.Owner = Window.GetWindow(this);

            _fwList.Add(fwc);

            Layout.CollectGarbage();

            fwc.AttachDrag();
            fwc.Show();

        }

        internal IEnumerable<LayoutFloatingWindowControl> GetFloatingWindowsByZOrder()
        {
            var parentWindow = Window.GetWindow(this);

            if (parentWindow == null)
                yield break;

            IntPtr windowParentHanlde = new WindowInteropHelper(parentWindow).Handle;
            
            IntPtr currentHandle = Win32Helper.GetWindow(windowParentHanlde, (uint)Win32Helper.GetWindow_Cmd.GW_HWNDFIRST);
            while (currentHandle != IntPtr.Zero)
            {
                LayoutFloatingWindowControl ctrl = _fwList.FirstOrDefault(fw => new WindowInteropHelper(fw).Handle == currentHandle);
                if (ctrl != null && ctrl.Model.Root.Manager == this)
                    yield return ctrl;

                currentHandle = Win32Helper.GetWindow(currentHandle, (uint)Win32Helper.GetWindow_Cmd.GW_HWNDNEXT);
            }
        }

        internal void RemoveFloatingWindow(LayoutFloatingWindowControl floatingWindow)
        {
            _fwList.Remove(floatingWindow);
        }

        public IEnumerable<LayoutFloatingWindowControl> FloatingWindows
        {
            get { return _fwList; }
        }
        #endregion

        #region OverlayWindow

        bool IOverlayWindowHost.HitTest(Point dragPoint)
        {
            Rect detectionRect = new Rect(this.PointToScreenDPI(new Point()), this.TransformActualSizeToAncestor());
            return detectionRect.Contains(dragPoint);
        }

        OverlayWindow _overlayWindow = null;
        void CreateOverlayWindow()
        {
            if (_overlayWindow == null)
            {
                _overlayWindow = new OverlayWindow(this);
            }
            Rect rectWindow = new Rect(this.PointToScreenDPI(new Point()), this.TransformActualSizeToAncestor());
            _overlayWindow.Left = rectWindow.Left;
            _overlayWindow.Top = rectWindow.Top;
            _overlayWindow.Width = rectWindow.Width;
            _overlayWindow.Height = rectWindow.Height;
        }

        void DestroyOverlayWindow()
        {
            if (_overlayWindow != null)
            {
                _overlayWindow.Close();
                _overlayWindow = null;
            }
        }

        IOverlayWindow IOverlayWindowHost.ShowOverlayWindow(LayoutFloatingWindowControl draggingWindow)
        {
            //Debug.WriteLine("ShowOverlayWindow");
            CreateOverlayWindow();
            _overlayWindow.Owner = draggingWindow;
            _overlayWindow.EnableDropTargets();
            _overlayWindow.Show();
            return _overlayWindow;
        }

        void IOverlayWindowHost.HideOverlayWindow()
        {
            //Debug.WriteLine("HideOverlayWindow");
            _areas = null;
            _overlayWindow.Owner = null;
            _overlayWindow.HideDropTargets();
        }

        List<IDropArea> _areas = null;

        IEnumerable<IDropArea> IOverlayWindowHost.GetDropAreas(LayoutFloatingWindowControl draggingWindow)
        {
            if (_areas != null)
                return _areas;

            bool isDraggingDocuments = draggingWindow.Model is LayoutDocumentFloatingWindow;

            _areas = new List<IDropArea>();

            if (!isDraggingDocuments)
            {
                _areas.Add(new DropArea<DockingManager>(
                    this,
                    DropAreaType.DockingManager));

                foreach (var areaHost in this.FindVisualChildren<LayoutAnchorablePaneControl>())
                {
                    _areas.Add(new DropArea<LayoutAnchorablePaneControl>(
                        areaHost,
                        DropAreaType.AnchorablePane));
                }
            }

            foreach (var areaHost in this.FindVisualChildren<LayoutDocumentPaneControl>())
            {
                _areas.Add(new DropArea<LayoutDocumentPaneControl>(
                    areaHost,
                    DropAreaType.DocumentPane));
            }

            foreach (var areaHost in this.FindVisualChildren<LayoutDocumentPaneGroupControl>())
            {
                var documentGroupModel = areaHost.Model as LayoutDocumentPaneGroup;
                if (documentGroupModel.Children.Where(c => c.IsVisible).Count() == 0)
                {
                    _areas.Add(new DropArea<LayoutDocumentPaneGroupControl>(
                        areaHost,
                        DropAreaType.DocumentPaneGroup));
                }
            }

            return _areas;
        }


        #endregion

        #region LayoutDocument & LayoutAnchorable Templates

        #region LayoutItemTemplate

        /// <summary>
        /// LayoutItemTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty LayoutItemTemplateProperty =
            DependencyProperty.Register("LayoutItemTemplate", typeof(DataTemplate), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplate)null,
                    new PropertyChangedCallback(OnLayoutItemTemplateChanged)));

        /// <summary>
        /// Gets or sets the AnchorableTemplate property.  This dependency property 
        /// indicates the template to use to render anchorable and document contents.
        /// </summary>
        public DataTemplate LayoutItemTemplate
        {
            get { return (DataTemplate)GetValue(LayoutItemTemplateProperty); }
            set { SetValue(LayoutItemTemplateProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AnchorableTemplate property.
        /// </summary>
        private static void OnLayoutItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnLayoutItemTemplateChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AnchorableTemplate property.
        /// </summary>
        protected virtual void OnLayoutItemTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region LayoutItemTemplateSelector

        /// <summary>
        /// LayoutItemTemplateSelector Dependency Property
        /// </summary>
        public static readonly DependencyProperty LayoutItemTemplateSelectorProperty =
            DependencyProperty.Register("LayoutItemTemplateSelector", typeof(DataTemplateSelector), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplateSelector)null,
                    new PropertyChangedCallback(OnLayoutItemTemplateSelectorChanged)));

        /// <summary>
        /// Gets or sets the LayoutItemTemplateSelector property.  This dependency property 
        /// indicates selector object to use for anchorable templates.
        /// </summary>
        public DataTemplateSelector LayoutItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(LayoutItemTemplateSelectorProperty); }
            set { SetValue(LayoutItemTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the LayoutItemTemplateSelector property.
        /// </summary>
        private static void OnLayoutItemTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnLayoutItemTemplateSelectorChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the LayoutItemTemplateSelector property.
        /// </summary>
        protected virtual void OnLayoutItemTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion


        #endregion

        #region DocumentsSource

        /// <summary>
        /// DocumentsSource Dependency Property
        /// </summary>
        public static readonly DependencyProperty DocumentsSourceProperty =
            DependencyProperty.Register("DocumentsSource", typeof(IEnumerable), typeof(DockingManager),
                new FrameworkPropertyMetadata((IEnumerable)null,
                    new PropertyChangedCallback(OnDocumentsSourceChanged)));

        /// <summary>
        /// Gets or sets the DocumentsSource property.  This dependency property 
        /// indicates the source collection of documents.
        /// </summary>
        public IEnumerable DocumentsSource
        {
            get { return (IEnumerable)GetValue(DocumentsSourceProperty); }
            set { SetValue(DocumentsSourceProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DocumentsSource property.
        /// </summary>
        private static void OnDocumentsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnDocumentsSourceChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DocumentsSource property.
        /// </summary>
        protected virtual void OnDocumentsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            DetachDocumentsSource(Layout, e.OldValue as IEnumerable);
            AttachDocumentsSource(Layout, e.NewValue as IEnumerable);
        }


        void AttachDocumentsSource(LayoutRoot layout, IEnumerable documentsSource)
        {
            if (documentsSource == null)
                return;

            if (layout == null)
                return;

            //if (layout.Descendents().OfType<LayoutDocument>().Any())
            //    throw new InvalidOperationException("Unable to set the DocumentsSource property if LayoutDocument objects are already present in the model");
            var documentsImported = layout.Descendents().OfType<LayoutDocument>().Select(d => d.Content).ToArray();
            var documents = documentsSource as IEnumerable;
            var listOfDocumentsToImport = new List<object>(documents.OfType<object>());

            foreach (var document in listOfDocumentsToImport.ToArray())
            {
                if (documentsImported.Contains(document))
                    listOfDocumentsToImport.Remove(document);
            }


            LayoutDocumentPane documentPane = null;
            if (layout.LastFocusedDocument != null)
            {
                documentPane = layout.LastFocusedDocument.Parent as LayoutDocumentPane;
            }

            if (documentPane == null)
            {
                documentPane = layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
            }

            if (documentPane != null)
            {
                foreach (var documentToImport in listOfDocumentsToImport)
                {
                    documentPane.Children.Add(new LayoutDocument() { Content = documentToImport });
                }
            }

            var documentsSourceAsNotifier = documentsSource as INotifyCollectionChanged;
            if (documentsSourceAsNotifier != null)
                documentsSourceAsNotifier.CollectionChanged += new NotifyCollectionChangedEventHandler(documentsSourceElementsChanged);
        }

        internal bool SuspendDocumentsSourceBinding = false;

        void documentsSourceElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Layout == null)
                return;

            //When deserializing documents are created automatically by the deserializer
            if (SuspendDocumentsSourceBinding)
                return;

            //handle remove
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                if (e.OldItems != null)
                {
                    var documentsToRemove = Layout.Descendents().OfType<LayoutDocument>().Where(d => e.OldItems.Contains(d.Content)).ToArray();
                    foreach (var documentToRemove in documentsToRemove)
                    {
                        (documentToRemove.Parent as ILayoutContainer).RemoveChild(
                            documentToRemove);
                    }
                }
            }

            //handle add
            if (e.NewItems != null &&
                (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace))
            {
                if (e.NewItems != null)
                {
                    LayoutDocumentPane documentPane = null;
                    if (Layout.LastFocusedDocument != null)
                    {
                        documentPane = Layout.LastFocusedDocument.Parent as LayoutDocumentPane;
                    }

                    if (documentPane == null)
                    {
                        documentPane = Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
                    }

                    if (documentPane != null)
                    {
                        foreach (var documentToImport in e.NewItems)
                        {
                            documentPane.Children.Add(new LayoutDocument() { Content = documentToImport });
                        }
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                //NOTE: I'm going to clear every document present in layout but
                //some documents may have been added directly to the layout, for now I clear them too
                var documentsToRemove = Layout.Descendents().OfType<LayoutDocument>().ToArray();
                foreach (var documentToRemove in documentsToRemove)
                {
                    (documentToRemove.Parent as ILayoutContainer).RemoveChild(
                        documentToRemove);
                }                
            }
        }

        void DetachDocumentsSource(LayoutRoot layout, IEnumerable documentsSource)
        {
            if (documentsSource == null)
                return;

            if (layout == null)
                return;

            var documentsToRemove = layout.Descendents().OfType<LayoutDocument>()
                .Where(d => documentsSource.Contains(d.Content)).ToArray();

            foreach (var documentToRemove in documentsToRemove)
            {
                (documentToRemove.Parent as ILayoutContainer).RemoveChild(
                    documentToRemove);
            }

            var documentsSourceAsNotifier = documentsSource as INotifyCollectionChanged;
            if (documentsSourceAsNotifier != null)
                documentsSourceAsNotifier.CollectionChanged -= new NotifyCollectionChangedEventHandler(documentsSourceElementsChanged);
        }


        #endregion

        #region DocumentCloseCommand

        internal void _ExecuteCloseCommand(LayoutDocument document)
        {
            if (DocumentClosing != null)
            {
                var evargs = new DocumentClosingEventArgs(document);
                DocumentClosing(this, evargs);
                if (evargs.Cancel)
                    return;
            }

            document.Close();

            if (DocumentClosed != null)
            { 
                var evargs = new DocumentClosedEventArgs(document);
                DocumentClosed(this, evargs);
            }
        }

        /// <summary>
        /// Event fired when a document is about to be closed
        /// </summary>
        /// <remarks>Subscribers have the opportuniy to cancel the operation.</remarks>
        public event EventHandler<DocumentClosingEventArgs> DocumentClosing;

        /// <summary>
        /// Event fired after a document is closed
        /// </summary>
        public event EventHandler<DocumentClosedEventArgs> DocumentClosed;



        #endregion

        internal void _ExecuteCloseAllButThisCommand(LayoutContent contentSelected)
        {
            foreach (var contentToClose in Layout.Descendents().OfType<LayoutContent>().Where(d => d != contentSelected && (d.Parent is LayoutDocumentPane || d.Parent is LayoutDocumentFloatingWindow)).ToArray())
            {
                if (!contentToClose.CanClose)
                    continue;
                if (contentToClose is LayoutDocument)
                    _ExecuteCloseCommand(contentToClose as LayoutDocument);
                else if (contentToClose is LayoutAnchorable)
                    _ExecuteCloseCommand(contentToClose as LayoutAnchorable);
            }
        }

        #region DocumentContextMenu

        /// <summary>
        /// DocumentContextMenu Dependency Property
        /// </summary>
        public static readonly DependencyProperty DocumentContextMenuProperty =
            DependencyProperty.Register("DocumentContextMenu", typeof(ContextMenu), typeof(DockingManager),
                new FrameworkPropertyMetadata((ContextMenu)null));

        /// <summary>
        /// Gets or sets the DocumentContextMenu property.  This dependency property 
        /// indicates context menu to show for documents.
        /// </summary>
        public ContextMenu DocumentContextMenu
        {
            get { return (ContextMenu)GetValue(DocumentContextMenuProperty); }
            set { SetValue(DocumentContextMenuProperty, value); }
        }

        #endregion

        #region AnchorablesSource

        /// <summary>
        /// AnchorablesSource Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorablesSourceProperty =
            DependencyProperty.Register("AnchorablesSource", typeof(IEnumerable), typeof(DockingManager),
                new FrameworkPropertyMetadata((IEnumerable)null,
                    new PropertyChangedCallback(OnAnchorablesSourceChanged)));

        /// <summary>
        /// Gets or sets the AnchorablesSource property.  This dependency property 
        /// indicates source collection of anchorables.
        /// </summary>
        public IEnumerable AnchorablesSource
        {
            get { return (IEnumerable)GetValue(AnchorablesSourceProperty); }
            set { SetValue(AnchorablesSourceProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AnchorablesSource property.
        /// </summary>
        private static void OnAnchorablesSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnAnchorablesSourceChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AnchorablesSource property.
        /// </summary>
        protected virtual void OnAnchorablesSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            DetachAnchorablesSource(Layout, e.OldValue as IEnumerable);
            AttachAnchorablesSource(Layout, e.NewValue as IEnumerable);
        }

        void AttachAnchorablesSource(LayoutRoot layout, IEnumerable anchorablesSource)
        {
            if (anchorablesSource == null)
                return;

            if (layout == null)
                return;

            //if (layout.Descendents().OfType<LayoutAnchorable>().Any())
            //    throw new InvalidOperationException("Unable to set the AnchorablesSource property if LayoutAnchorable objects are already present in the model");
            var anchorablesImported = layout.Descendents().OfType<LayoutAnchorable>().Select(d => d.Content).ToArray();
            var anchorables = anchorablesSource as IEnumerable;
            var listOfAnchorablesToImport = new List<object>(anchorables.OfType<object>());

            foreach (var document in listOfAnchorablesToImport.ToArray())
            {
                if (anchorablesImported.Contains(document))
                    listOfAnchorablesToImport.Remove(document);
            }

            LayoutAnchorablePane anchorablePane = null;
            if (layout.ActiveContent != null)
            {
                //look for active content parent pane
                anchorablePane = layout.ActiveContent.Parent as LayoutAnchorablePane;
            }

            if (anchorablePane == null)
            {
                //look for a pane on the right side
                anchorablePane = layout.Descendents().OfType<LayoutAnchorablePane>().Where(pane => !pane.IsHostedInFloatingWindow && pane.GetSide() == AnchorSide.Right).FirstOrDefault();
            }

            if (anchorablePane == null)
            {
                //look for an available pane
                anchorablePane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault();
            }

            if (anchorablePane == null)
            { 
                //create a pane on the fly on the right side

            }

            if (anchorablePane != null)
            {
                _suspendLayoutItemCreation = true;
                foreach (var anchorableContentToImport in listOfAnchorablesToImport)
                {
                    var anchorableToImport = new LayoutAnchorable()
                    {
                        Content = anchorableContentToImport
                    };

                    bool added = false;
                    if (LayoutUpdateStrategy != null)
                    {
                        added = LayoutUpdateStrategy.BeforeInsertAnchorable(layout, anchorableToImport, anchorablePane);
                    }

                    if (!added && anchorablePane != null)
                    {
                        anchorablePane.Children.Add(anchorableToImport);
                        added = true;
                    }

                    if (!added && LayoutUpdateStrategy != null)
                    {
                        added = LayoutUpdateStrategy.InsertAnchorable(layout, anchorableToImport, anchorablePane);
                    }

                    if (added)
                    {
                        var anchorableItem = new LayoutAnchorableItem();
                        anchorableItem.Attach(anchorableToImport);
                        ApplyStyleToLayoutItem(anchorableItem);
                        _layoutItems.Add(anchorableItem);
                    }
                }

                _suspendLayoutItemCreation = false;
            }

            var anchorablesSourceAsNotifier = anchorablesSource as INotifyCollectionChanged;
            if (anchorablesSourceAsNotifier != null)
                anchorablesSourceAsNotifier.CollectionChanged += new NotifyCollectionChangedEventHandler(anchorablesSourceElementsChanged);
        }

        internal bool SuspendAnchorablesSourceBinding = false;

        void anchorablesSourceElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Layout == null)
                return;

            //When deserializing documents are created automatically by the deserializer
            if (SuspendAnchorablesSourceBinding)
                return;

            //handle remove
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                if (e.OldItems != null)
                {
                    var anchorablesToRemove = Layout.Descendents().OfType<LayoutAnchorable>().Where(d => e.OldItems.Contains(d.Content)).ToArray();
                    foreach (var anchorableToRemove in anchorablesToRemove)
                    {
                        (anchorableToRemove.Parent as ILayoutContainer).RemoveChild(
                            anchorableToRemove);
                    }
                }
            }

            //handle add
            if (e.NewItems != null &&
                (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace))
            {
                if (e.NewItems != null)
                {
                    LayoutAnchorablePane anchorablePane = null;

                    if (Layout.ActiveContent != null)
                    {
                        //look for active content parent pane
                        anchorablePane = Layout.ActiveContent.Parent as LayoutAnchorablePane;
                    }

                    if (anchorablePane == null)
                    {
                        //look for a pane on the right side
                        anchorablePane = Layout.Descendents().OfType<LayoutAnchorablePane>().Where(pane => !pane.IsHostedInFloatingWindow && pane.GetSide() == AnchorSide.Right).FirstOrDefault();
                    }

                    if (anchorablePane == null)
                    {
                        //look for an available pane
                        anchorablePane = Layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault();
                    }
                    _suspendLayoutItemCreation = true;
                    foreach (var anchorableContentToImport in e.NewItems)
                    {
                        var anchorableToImport = new LayoutAnchorable()
                        {
                            Content = anchorableContentToImport
                        };

                        bool added = false;
                        if (LayoutUpdateStrategy != null)
                        {
                            added = LayoutUpdateStrategy.BeforeInsertAnchorable(Layout, anchorableToImport, anchorablePane);
                        }

                        if (!added && anchorablePane != null)
                        {
                            anchorablePane.Children.Add(anchorableToImport);
                            added = true;
                        }

                        if (!added && LayoutUpdateStrategy != null)
                        {
                            added = LayoutUpdateStrategy.InsertAnchorable(Layout, anchorableToImport, anchorablePane);
                        }

                        var root = anchorableToImport.Root;

                        if (added && root != null && root.Manager == this)
                        {
                            var anchorableItem = new LayoutAnchorableItem();
                            anchorableItem.Attach(anchorableToImport);
                            ApplyStyleToLayoutItem(anchorableItem);
                            _layoutItems.Add(anchorableItem);
                        }

                    }
                    _suspendLayoutItemCreation = false;
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                //NOTE: I'm going to clear every anchorable present in layout but
                //some anchorable may have been added directly to the layout, for now I clear them too
                var anchorablesToRemove = Layout.Descendents().OfType<LayoutAnchorable>().ToArray();
                foreach (var anchorableToRemove in anchorablesToRemove)
                {
                    (anchorableToRemove.Parent as ILayoutContainer).RemoveChild(
                        anchorableToRemove);
                }
            }
        }

        void DetachAnchorablesSource(LayoutRoot layout, IEnumerable anchorablesSource)
        {
            if (anchorablesSource == null)
                return;

            if (layout == null)
                return;

            var anchorablesToRemove = layout.Descendents().OfType<LayoutAnchorable>()
                .Where(d => anchorablesSource.Contains(d.Content)).ToArray();

            foreach (var anchorableToRemove in anchorablesToRemove)
            {
                (anchorableToRemove.Parent as ILayoutContainer).RemoveChild(
                    anchorableToRemove);
            }

            var anchorablesSourceAsNotifier = anchorablesSource as INotifyCollectionChanged;
            if (anchorablesSourceAsNotifier != null)
                anchorablesSourceAsNotifier.CollectionChanged -= new NotifyCollectionChangedEventHandler(anchorablesSourceElementsChanged);
        }

        #endregion

        internal void _ExecuteCloseCommand(LayoutAnchorable anchorable)
        {
            var model = anchorable as LayoutAnchorable;
            if (model != null)
            {
                if (model.IsAutoHidden)
                    model.ToggleAutoHide();

                model.Close();
                return;
            }
        }

        internal void _ExecuteHideCommand(LayoutAnchorable anchorable)
        {
            var model = anchorable as LayoutAnchorable;
            if (model != null)
            {
                if (model.IsAutoHidden)
                    model.ToggleAutoHide();
                //by default hide the anchorable
                model.Hide();
            }

            //var paneModel = anchorable as LayoutAnchorablePane;
            //if (paneModel != null)
            //{
            //    foreach (var anchorableModel in paneModel.Children.ToArray())
            //    {
            //        anchorableModel.Hide();
            //    }
            //}
        }

        internal void _ExecuteAutoHideCommand(LayoutAnchorable _anchorable)
        {
            _anchorable.ToggleAutoHide();
        }

        internal void _ExecuteFloatCommand(LayoutContent contentToFloat)
        {
            contentToFloat.Float();
        }

        internal void _ExecuteDockCommand(LayoutAnchorable anchorable)
        {
            anchorable.Dock();
        }

        internal void _ExecuteDockAsDocumentCommand(LayoutContent content)
        {
            content.DockAsDocument();
        }

        #region ActiveContent

        /// <summary>
        /// ActiveContent Dependency Property
        /// </summary>
        public static readonly DependencyProperty ActiveContentProperty =
            DependencyProperty.Register("ActiveContent", typeof(object), typeof(DockingManager),
                new FrameworkPropertyMetadata((object)null,
                    new PropertyChangedCallback(OnActiveContentChanged)));

        /// <summary>
        /// Gets or sets the ActiveContent property.  This dependency property 
        /// indicates the content currently active.
        /// </summary>
        public object ActiveContent
        {
            get { return (object)GetValue(ActiveContentProperty); }
            set { SetValue(ActiveContentProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ActiveContent property.
        /// </summary>
        private static void OnActiveContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).InternalSetActiveContent(e.NewValue);
            ((DockingManager)d).OnActiveContentChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ActiveContent property.
        /// </summary>
        protected virtual void OnActiveContentChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ActiveContentChanged != null)
                ActiveContentChanged(this, EventArgs.Empty);
        }


        bool _insideInternalSetActiveContent = false;
        void InternalSetActiveContent(object contentObject)
        {
            var layoutContent = Layout.Descendents().OfType<LayoutContent>().FirstOrDefault(lc => lc == contentObject || lc.Content == contentObject);
            _insideInternalSetActiveContent = true;
            Layout.ActiveContent = layoutContent;
            _insideInternalSetActiveContent = false;
        }

        public event EventHandler ActiveContentChanged;

        #endregion

        #region AnchorableContextMenu

        /// <summary>
        /// AnchorableContextMenu Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorableContextMenuProperty =
            DependencyProperty.Register("AnchorableContextMenu", typeof(ContextMenu), typeof(DockingManager),
                new FrameworkPropertyMetadata((ContextMenu)null));

        /// <summary>
        /// Gets or sets the AnchorableContextMenu property.  This dependency property 
        /// indicates the context menu to show up for anchorables.
        /// </summary>
        public ContextMenu AnchorableContextMenu
        {
            get { return (ContextMenu)GetValue(AnchorableContextMenuProperty); }
            set { SetValue(AnchorableContextMenuProperty, value); }
        }

        #endregion

        #region Theme

        /// <summary>
        /// Theme Dependency Property
        /// </summary>
        public static readonly DependencyProperty ThemeProperty =
            DependencyProperty.Register("Theme", typeof(Theme), typeof(DockingManager),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnThemeChanged)));

        /// <summary>
        /// Gets or sets the Theme property.  This dependency property 
        /// indicates the theme to use for AvalonDock controls.
        /// </summary>
        public Theme Theme
        {
            get { return (Theme)GetValue(ThemeProperty); }
            set { SetValue(ThemeProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Theme property.
        /// </summary>
        private static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnThemeChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Theme property.
        /// </summary>
        protected virtual void OnThemeChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldTheme = e.OldValue as Theme;
            var newTheme = e.NewValue as Theme;

            if (oldTheme != null)
            {
                var resourceDictionaryToRemove =
                    Application.Current.Resources.MergedDictionaries.FirstOrDefault(r => r.Source == oldTheme.GetResourceUri());
                if (resourceDictionaryToRemove != null)
                    Application.Current.Resources.MergedDictionaries.Remove(
                        resourceDictionaryToRemove);
            }

            if (newTheme != null)
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = newTheme.GetResourceUri() });
            }
        }

        #endregion

        #region GridSplitterWidth

        /// <summary>
        /// GridSplitterWidth Dependency Property
        /// </summary>
        public static readonly DependencyProperty GridSplitterWidthProperty =
            DependencyProperty.Register("GridSplitterWidth", typeof(double), typeof(DockingManager),
                new FrameworkPropertyMetadata((double)6.0));

        /// <summary>
        /// Gets or sets the GridSplitterWidth property.  This dependency property 
        /// indicates width of grid splitters.
        /// </summary>
        public double GridSplitterWidth
        {
            get { return (double)GetValue(GridSplitterWidthProperty); }
            set { SetValue(GridSplitterWidthProperty, value); }
        }

        #endregion

        #region GridSplitterHeight

        /// <summary>
        /// GridSplitterHeight Dependency Property
        /// </summary>
        public static readonly DependencyProperty GridSplitterHeightProperty =
            DependencyProperty.Register("GridSplitterHeight", typeof(double), typeof(DockingManager),
                new FrameworkPropertyMetadata((double)6.0));

        /// <summary>
        /// Gets or sets the GridSplitterHeight property.  This dependency property 
        /// indicates height of grid splitters.
        /// </summary>
        public double GridSplitterHeight
        {
            get { return (double)GetValue(GridSplitterHeightProperty); }
            set { SetValue(GridSplitterHeightProperty, value); }
        }

        #endregion

        internal void _ExecuteContentActivateCommand(LayoutContent content)
        {
            content.IsActive = true;
        }

        #region DocumentPaneMenuItemHeaderTemplate

        /// <summary>
        /// DocumentPaneMenuItemHeaderTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty DocumentPaneMenuItemHeaderTemplateProperty =
            DependencyProperty.Register("DocumentPaneMenuItemHeaderTemplate", typeof(DataTemplate), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplate)null,
                    new PropertyChangedCallback(OnDocumentPaneMenuItemHeaderTemplateChanged),
                    new CoerceValueCallback(CoerceDocumentPaneMenuItemHeaderTemplateValue)));

        /// <summary>
        /// Gets or sets the DocumentPaneMenuItemHeaderTemplate property.  This dependency property 
        /// indicates the header template to use while creating menu items for the document panes.
        /// </summary>
        public DataTemplate DocumentPaneMenuItemHeaderTemplate
        {
            get { return (DataTemplate)GetValue(DocumentPaneMenuItemHeaderTemplateProperty); }
            set { SetValue(DocumentPaneMenuItemHeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DocumentPaneMenuItemHeaderTemplate property.
        /// </summary>
        private static void OnDocumentPaneMenuItemHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnDocumentPaneMenuItemHeaderTemplateChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DocumentPaneMenuItemHeaderTemplate property.
        /// </summary>
        protected virtual void OnDocumentPaneMenuItemHeaderTemplateChanged(DependencyPropertyChangedEventArgs e)
        { 
        }

        /// <summary>
        /// Coerces the DocumentPaneMenuItemHeaderTemplate value.
        /// </summary>
        private static object CoerceDocumentPaneMenuItemHeaderTemplateValue(DependencyObject d, object value)
        {
            if (value != null &&
                d.GetValue(DocumentPaneMenuItemHeaderTemplateSelectorProperty) != null)
                return null; 
            
            return value;
        }

        #endregion

        #region DocumentPaneMenuItemHeaderTemplateSelector

        /// <summary>
        /// DocumentPaneMenuItemHeaderTemplateSelector Dependency Property
        /// </summary>
        public static readonly DependencyProperty DocumentPaneMenuItemHeaderTemplateSelectorProperty =
            DependencyProperty.Register("DocumentPaneMenuItemHeaderTemplateSelector", typeof(DataTemplateSelector), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplateSelector)null,
                    new PropertyChangedCallback(OnDocumentPaneMenuItemHeaderTemplateSelectorChanged),
                    new CoerceValueCallback(CoerceDocumentPaneMenuItemHeaderTemplateSelectorValue)));

        /// <summary>
        /// Gets or sets the DocumentPaneMenuItemHeaderTemplateSelector property.  This dependency property 
        /// indicates the data template selector to use for the menu items show when user select the DocumentPane document switch context menu.
        /// </summary>
        public DataTemplateSelector DocumentPaneMenuItemHeaderTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(DocumentPaneMenuItemHeaderTemplateSelectorProperty); }
            set { SetValue(DocumentPaneMenuItemHeaderTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DocumentPaneMenuItemHeaderTemplateSelector property.
        /// </summary>
        private static void OnDocumentPaneMenuItemHeaderTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnDocumentPaneMenuItemHeaderTemplateSelectorChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DocumentPaneMenuItemHeaderTemplateSelector property.
        /// </summary>
        protected virtual void OnDocumentPaneMenuItemHeaderTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null &&
                DocumentPaneMenuItemHeaderTemplate != null)
                DocumentPaneMenuItemHeaderTemplate = null;

        }

        /// <summary>
        /// Coerces the DocumentPaneMenuItemHeaderTemplateSelector value.
        /// </summary>
        private static object CoerceDocumentPaneMenuItemHeaderTemplateSelectorValue(DependencyObject d, object value)
        {
            return value;
        }

        #endregion

        #region IconContentTemplate

        /// <summary>
        /// IconContentTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty IconContentTemplateProperty =
            DependencyProperty.Register("IconContentTemplate", typeof(DataTemplate), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplate)null));

        /// <summary>
        /// Gets or sets the IconContentTemplate property.  This dependency property 
        /// indicates the data template to use while extracting the icon from model.
        /// </summary>
        public DataTemplate IconContentTemplate
        {
            get { return (DataTemplate)GetValue(IconContentTemplateProperty); }
            set { SetValue(IconContentTemplateProperty, value); }
        }

        #endregion

        #region IconContentTemplateSelector

        /// <summary>
        /// IconContentTemplateSelector Dependency Property
        /// </summary>
        public static readonly DependencyProperty IconContentTemplateSelectorProperty =
            DependencyProperty.Register("IconContentTemplateSelector", typeof(DataTemplateSelector), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplateSelector)null));

        /// <summary>
        /// Gets or sets the IconContentTemplateSelector property.  This dependency property 
        /// indicates data template selector to use while selecting the datatamplate for content icons.
        /// </summary>
        public DataTemplateSelector IconContentTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(IconContentTemplateSelectorProperty); }
            set { SetValue(IconContentTemplateSelectorProperty, value); }
        }

        #endregion


        #region LayoutItems

        List<LayoutItem> _layoutItems = new List<LayoutItem>();

        bool _suspendLayoutItemCreation = false;

        void DetachLayoutItems()
        {
            if (Layout != null)
            {
                _layoutItems.ForEach<LayoutItem>(i => i.Detach());
                _layoutItems.Clear();
                Layout.ElementAdded -= new EventHandler<LayoutElementEventArgs>(Layout_ElementAdded);
                Layout.ElementRemoved -= new EventHandler<LayoutElementEventArgs>(Layout_ElementRemoved);
            }
        }

        void Layout_ElementRemoved(object sender, LayoutElementEventArgs e)
        {
            if (_suspendLayoutItemCreation)
                return;

            if (e.Element is LayoutContent)
            {
                var layoutItem = _layoutItems.First(item => item.LayoutElement == e.Element);
                layoutItem.Detach();
                _layoutItems.Remove(layoutItem);
            }
            else if (e.Element is ILayoutContainer)
            {
                foreach (var content in e.Element.Descendents().OfType<LayoutContent>())
                {
                    var itemToRemove = _layoutItems.First(item => item.LayoutElement == content);
                    itemToRemove.Detach();
                    _layoutItems.Remove(itemToRemove);
                }
            }
        }

        void Layout_ElementAdded(object sender, LayoutElementEventArgs e)
        {
            if (_suspendLayoutItemCreation)
                return;
            
            if (e.Element is LayoutDocument)
            {
                var document = e.Element as LayoutDocument;
                var documentItem = new LayoutDocumentItem();
                documentItem.Attach(document);
                ApplyStyleToLayoutItem(documentItem);
                _layoutItems.Add(documentItem);
            }
            else if (e.Element is LayoutAnchorable)
            {
                var anchorable = e.Element as LayoutAnchorable;
                var anchorableItem = new LayoutAnchorableItem();
                anchorableItem.Attach(anchorable);
                ApplyStyleToLayoutItem(anchorableItem);
                _layoutItems.Add(anchorableItem);
            }
            else if (e.Element is ILayoutContainer)
            {
                foreach (var document in e.Element.Descendents().OfType<LayoutDocument>().ToArray())
                {
                    var documentItem = new LayoutDocumentItem();
                    documentItem.Attach(document);
                    ApplyStyleToLayoutItem(documentItem);
                    _layoutItems.Add(documentItem);
                }
                foreach (var anchorable in e.Element.Descendents().OfType<LayoutAnchorable>().ToArray())
                {
                    var anchorableItem = new LayoutAnchorableItem();
                    anchorableItem.Attach(anchorable);
                    ApplyStyleToLayoutItem(anchorableItem);
                    _layoutItems.Add(anchorableItem);
                }                
            }
        }

        void AttachLayoutItems()
        {
            if (Layout != null)
            {
                foreach (var document in Layout.Descendents().OfType<LayoutDocument>().ToArray())
                {
                    var documentItem = new LayoutDocumentItem();
                    documentItem.Attach(document);
                    ApplyStyleToLayoutItem(documentItem);
                    _layoutItems.Add(documentItem);
                }
                foreach (var anchorable in Layout.Descendents().OfType<LayoutAnchorable>().ToArray())
                {
                    var anchorableItem = new LayoutAnchorableItem();
                    anchorableItem.Attach(anchorable);
                    ApplyStyleToLayoutItem(anchorableItem);
                    _layoutItems.Add(anchorableItem);
                }

                Layout.ElementAdded += new EventHandler<LayoutElementEventArgs>(Layout_ElementAdded);
                Layout.ElementRemoved += new EventHandler<LayoutElementEventArgs>(Layout_ElementRemoved);
            }
        }

        void ApplyStyleToLayoutItem(LayoutItem layoutItem)
        {
            layoutItem._ClearDefaultCommandBindings();
            if (LayoutItemContainerStyle != null)
                layoutItem.Style = LayoutItemContainerStyle;
            else if (LayoutItemContainerStyleSelector != null)
                layoutItem.Style = LayoutItemContainerStyleSelector.SelectStyle(layoutItem.Model, layoutItem);
            layoutItem._SetDefaultCommandBindings();
        }

        #region LayoutItemContainerStyle

        /// <summary>
        /// LayoutItemContainerStyle Dependency Property
        /// </summary>
        public static readonly DependencyProperty LayoutItemContainerStyleProperty =
            DependencyProperty.Register("LayoutItemContainerStyle", typeof(Style), typeof(DockingManager),
                new FrameworkPropertyMetadata((Style)null,
                    new PropertyChangedCallback(OnLayoutItemContainerStyleChanged)));

        /// <summary>
        /// Gets or sets the LayoutItemContainerStyle property.  This dependency property 
        /// indicates the style to apply to LayoutDocumentItem objects. A LayoutDocumentItem object is created when a new LayoutDocument is created inside the current Layout.
        /// </summary>
        public Style LayoutItemContainerStyle
        {
            get { return (Style)GetValue(LayoutItemContainerStyleProperty); }
            set { SetValue(LayoutItemContainerStyleProperty, value); }
        }

        /// <summary>
        /// Handles changes to the LayoutItemContainerStyle property.
        /// </summary>
        private static void OnLayoutItemContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnLayoutItemContainerStyleChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the LayoutItemContainerStyle property.
        /// </summary>
        protected virtual void OnLayoutItemContainerStyleChanged(DependencyPropertyChangedEventArgs e)
        {
            AttachLayoutItems();
        }

        #endregion

        #region LayoutItemContainerStyleSelector

        /// <summary>
        /// LayoutItemContainerStyleSelector Dependency Property
        /// </summary>
        public static readonly DependencyProperty LayoutItemContainerStyleSelectorProperty =
            DependencyProperty.Register("LayoutItemContainerStyleSelector", typeof(StyleSelector), typeof(DockingManager),
                new FrameworkPropertyMetadata((StyleSelector)null,
                    new PropertyChangedCallback(OnLayoutItemContainerStyleSelectorChanged)));

        /// <summary>
        /// Gets or sets the LayoutItemContainerStyleSelector property.  This dependency property 
        /// indicates style selector of the LayoutDocumentItemStyle.
        /// </summary>
        public StyleSelector LayoutItemContainerStyleSelector
        {
            get { return (StyleSelector)GetValue(LayoutItemContainerStyleSelectorProperty); }
            set { SetValue(LayoutItemContainerStyleSelectorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the LayoutItemContainerStyleSelector property.
        /// </summary>
        private static void OnLayoutItemContainerStyleSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnLayoutItemContainerStyleSelectorChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the LayoutItemContainerStyleSelector property.
        /// </summary>
        protected virtual void OnLayoutItemContainerStyleSelectorChanged(DependencyPropertyChangedEventArgs e)
        {
            AttachLayoutItems();
        }

        #endregion


        internal LayoutItem GetLayoutItemFromModel(LayoutContent content)
        {
            return _layoutItems.FirstOrDefault(item => item.LayoutElement == content);
        }
        #endregion


        #region NavigatorWindow
        NavigatorWindow _navigatorWindow = null;

        void ShowNavigatorWindow()
        {
            if (_navigatorWindow == null)
            {
                _navigatorWindow = new NavigatorWindow(this) { Owner = Window.GetWindow(this), WindowStartupLocation = WindowStartupLocation.CenterOwner };
            }

            _navigatorWindow.Show();
            Debug.WriteLine("ShowNavigatorWindow()");
        }

        void HideNavigatorWindow()
        {
            if (_navigatorWindow == null)
                return;

            _navigatorWindow.Hide();


            if (_navigatorWindow.SelectedAnchorable == null &&
                _navigatorWindow.SelectedDocument != null &&
                _navigatorWindow.SelectedDocument.ActivateCommand.CanExecute(null))
                _navigatorWindow.SelectedDocument.ActivateCommand.Execute(null);

            _navigatorWindow.Close();
            _navigatorWindow = null;
            Debug.WriteLine("HideNavigatorWindow()");

        }

        bool IsNavigatorWindowActive
        {
            get { return _navigatorWindow != null; }
        }

        
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.IsDown && e.Key == Key.Tab)
                {
                    if (!IsNavigatorWindowActive)
                    {
                        ShowNavigatorWindow();
                        e.Handled = true;
                    }
                    else
                    {
                        _navigatorWindow.SelectNextDocument();
                        e.Handled = true;
                    }
                }
            }

            if (e.Key != Key.Tab && IsNavigatorWindowActive)
            {
                HideNavigatorWindow();
                e.Handled = true;
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            if (e.Key != Key.Tab && IsNavigatorWindowActive)
            {
                HideNavigatorWindow();
                e.Handled = true;
            }

            base.OnPreviewKeyUp(e);
        }

        #endregion
    }
}
