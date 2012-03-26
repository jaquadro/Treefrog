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

namespace AvalonDock
{
    [ContentProperty("Layout")]
    [TemplatePart(Name="PART_AutoHideArea")]
    public class DockingManager : Control, IOverlayWindowHost, ILogicalChildrenContainer
    {
        static DockingManager()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockingManager), new FrameworkPropertyMetadata(typeof(DockingManager)));
            FocusableProperty.OverrideMetadata(typeof(DockingManager), new FrameworkPropertyMetadata(true));
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

            DetachDocumentsSource(oldLayout, DocumentsSource);

            if (oldLayout != null &&
                oldLayout.Manager == this)
                oldLayout.Manager = null;

            ClearLogicalChildrenList();

            Layout.Manager = this;

            if (IsInitialized)
            {
                LayoutRootPanel = GetUIElementForModel(Layout.RootPanel) as LayoutPanelControl;
                LeftSidePanel = GetUIElementForModel(Layout.LeftSide) as LayoutAnchorSideControl;
                TopSidePanel = GetUIElementForModel(Layout.TopSide) as LayoutAnchorSideControl;
                RightSidePanel = GetUIElementForModel(Layout.RightSide) as LayoutAnchorSideControl;
                BottomSidePanel = GetUIElementForModel(Layout.BottomSide) as LayoutAnchorSideControl;

                foreach (var fwc in _fwList)
                    fwc.InternalClose();
                _fwList.Clear();

                foreach (var fw in Layout.FloatingWindows)
                    _fwList.Add(GetUIElementForModel(fw) as LayoutFloatingWindowControl);

                foreach (var fw in _fwList)
                    fw.Owner = Window.GetWindow(this);
            }

            AttachDocumentsSource(newLayout, DocumentsSource);

            if (newLayout != null)
            {
                newLayout.PropertyChanged += new PropertyChangedEventHandler(OnLayoutRootPropertyChanged);
                newLayout.Updated += new EventHandler(OnLayoutRootUpdated);
            }

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
                    var layoutRootPanel = GetUIElementForModel(Layout.RootPanel) as LayoutPanelControl;
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
                        }), DispatcherPriority.Loaded);
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
                LayoutRootPanel = GetUIElementForModel(Layout.RootPanel) as LayoutPanelControl;
                LeftSidePanel = GetUIElementForModel(Layout.LeftSide) as LayoutAnchorSideControl;
                TopSidePanel = GetUIElementForModel(Layout.TopSide) as LayoutAnchorSideControl;
                RightSidePanel = GetUIElementForModel(Layout.RightSide) as LayoutAnchorSideControl;
                BottomSidePanel = GetUIElementForModel(Layout.BottomSide) as LayoutAnchorSideControl;
                foreach (var fw in Layout.FloatingWindows)
                    _fwList.Add(GetUIElementForModel(fw) as LayoutAnchorableFloatingWindowControl);
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

        internal UIElement GetUIElementForModel(ILayoutElement model)
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
                new FrameworkPropertyMetadata((DataTemplate)null));

        /// <summary>
        /// Gets or sets the DocumentHeaderTemplate property.  This dependency property 
        /// indicates data template to use when creating document headers.
        /// </summary>
        public DataTemplate DocumentHeaderTemplate
        {
            get { return (DataTemplate)GetValue(DocumentHeaderTemplateProperty); }
            set { SetValue(DocumentHeaderTemplateProperty, value); }
        }

        #endregion

        #region DocumentHeaderTemplateSelector

        /// <summary>
        /// DocumentHeaderTemplateSelector Dependency Property
        /// </summary>
        public static readonly DependencyProperty DocumentHeaderTemplateSelectorProperty =
            DependencyProperty.Register("DocumentHeaderTemplateSelector", typeof(DataTemplateSelector), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplateSelector)null));

        /// <summary>
        /// Gets or sets the DocumentHeaderTemplateSelector property.  This dependency property 
        /// indicates data template selector to use when selecting data template for documents header.
        /// </summary>
        public DataTemplateSelector DocumentHeaderTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(DocumentHeaderTemplateSelectorProperty); }
            set { SetValue(DocumentHeaderTemplateSelectorProperty, value); }
        }

        #endregion

        #region DocumentTitleTemplate

        /// <summary>
        /// DocumentTitleTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty DocumentTitleTemplateProperty =
            DependencyProperty.Register("DocumentTitleTemplate", typeof(DataTemplate), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplate)null));

        /// <summary>
        /// Gets or sets the DocumentTitleTemplate property.  This dependency property 
        /// indicates the data template to use when rendering documents title.
        /// </summary>
        public DataTemplate DocumentTitleTemplate
        {
            get { return (DataTemplate)GetValue(DocumentTitleTemplateProperty); }
            set { SetValue(DocumentTitleTemplateProperty, value); }
        }

        #endregion

        #region DocumentTitleTemplateSelector

        /// <summary>
        /// DocumentTitleTemplateSelector Dependency Property
        /// </summary>
        public static readonly DependencyProperty DocumentTitleTemplateSelectorProperty =
            DependencyProperty.Register("DocumentTitleTemplateSelector", typeof(DataTemplateSelector), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplateSelector)null));

        /// <summary>
        /// Gets or sets the DocumentTitleTemplateSelector property.  This dependency property 
        /// indicates the data template selector to use when selecting a data template for the document title.
        /// </summary>
        public DataTemplateSelector DocumentTitleTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(DocumentTitleTemplateSelectorProperty); }
            set { SetValue(DocumentTitleTemplateSelectorProperty, value); }
        }

        #endregion

        #region AnchorableTitleTemplate

        /// <summary>
        /// AnchorableTitleTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorableTitleTemplateProperty =
            DependencyProperty.Register("AnchorableTitleTemplate", typeof(DataTemplate), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplate)null));

        /// <summary>
        /// Gets or sets the AnchorableTitleTemplate property.  This dependency property 
        /// indicates the data template to use when instatiating the anchorable title part.
        /// </summary>
        public DataTemplate AnchorableTitleTemplate
        {
            get { return (DataTemplate)GetValue(AnchorableTitleTemplateProperty); }
            set { SetValue(AnchorableTitleTemplateProperty, value); }
        }

        #endregion

        #region AnchorableTitleTemplateSelector

        /// <summary>
        /// AnchorableTitleTemplateSelector Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorableTitleTemplateSelectorProperty =
            DependencyProperty.Register("AnchorableTitleTemplateSelector", typeof(DataTemplateSelector), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplateSelector)null));

        /// <summary>
        /// Gets or sets the AnchorableTitleTemplateSelector property.  This dependency property 
        /// indicates the data template selector to use when selecting the data template for the anchorable title.
        /// </summary>
        public DataTemplateSelector AnchorableTitleTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(AnchorableTitleTemplateSelectorProperty); }
            set { SetValue(AnchorableTitleTemplateSelectorProperty, value); }
        }

        #endregion

        #region AnchorableHeaderTemplate

        /// <summary>
        /// AnchorableHeaderTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorableHeaderTemplateProperty =
            DependencyProperty.Register("AnchorableHeaderTemplate", typeof(DataTemplate), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplate)null));

        /// <summary>
        /// Gets or sets the AnchorableHeaderTemplate property.  This dependency property 
        /// indicates data template to use when creating anchorable headers.
        /// </summary>
        public DataTemplate AnchorableHeaderTemplate
        {
            get { return (DataTemplate)GetValue(AnchorableHeaderTemplateProperty); }
            set { SetValue(AnchorableHeaderTemplateProperty, value); }
        }

        #endregion

        #region AnchorableHeaderTemplateSelector

        /// <summary>
        /// AnchorableHeaderTemplateSelector Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorableHeaderTemplateSelectorProperty =
            DependencyProperty.Register("AnchorableHeaderTemplateSelector", typeof(DataTemplateSelector), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplateSelector)null));

        /// <summary>
        /// Gets or sets the AnchorableHeaderTemplateSelector property.  This dependency property 
        /// indicates data template selector to use when creating data template for anchorable headers.
        /// </summary>
        public DataTemplateSelector AnchorableHeaderTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(AnchorableHeaderTemplateSelectorProperty); }
            set { SetValue(AnchorableHeaderTemplateSelectorProperty, value); }
        }

        #endregion

        protected override void OnGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus is Grid)
                Debug.WriteLine(string.Format("DockingManager.OnGotKeyboardFocus({0})", e.NewFocus));
            base.OnGotKeyboardFocus(e);
        }

        protected override void OnPreviewGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            Debug.WriteLine(string.Format("DockingManager.OnPreviewGotKeyboardFocus({0})", e.NewFocus));
            base.OnPreviewGotKeyboardFocus(e);
        }

        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            Debug.WriteLine(string.Format("DockingManager.OnPreviewLostKeyboardFocus({0})", e.OldFocus));
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
        /// indicates ....
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
            if (_logicalChildren.Contains(element))
                throw new InvalidOperationException();
            _logicalChildren.Add(element);
            AddLogicalChild(element);
        }

        void ILogicalChildrenContainer.InternalRemoveLogicalChild(object element)
        {
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
        /// indicates ....
        /// </summary>
        public LayoutAutoHideWindowControl AutoHideWindow
        {
            get { return (LayoutAutoHideWindowControl)GetValue(AutoHideWindowProperty); }
        }

        /// <summary>
        /// Provides a secure method for setting the AutoHideWindow property.  
        /// This dependency property indicates ....
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

        internal void StartDraggingFloatingWindowForContent(LayoutContent contentModel)
        {
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
                            DockMinWidth = parentPaneAsPositionableElement.DockMinWidth
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
                DockMinWidth = paneAsPositionableElement.DockMinWidth
            };

            bool savePreviousContainer = paneModel.FindParent<LayoutFloatingWindow>() == null;

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
            Debug.WriteLine("ShowOverlayWindow");
            CreateOverlayWindow();
            _overlayWindow.Owner = draggingWindow;
            _overlayWindow.EnableDropTargets();
            _overlayWindow.Show();
            return _overlayWindow;
        }

        void IOverlayWindowHost.HideOverlayWindow()
        {
            Debug.WriteLine("HideOverlayWindow");
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

        #region AutoHide
        public void ToggleAutoHide(LayoutAnchorable anchorableModel)
        {
            #region Anchorable is already auto hidden
            if (anchorableModel.Parent is LayoutAnchorGroup)
            {
                var parentGroup = anchorableModel.Parent as LayoutAnchorGroup;
                var parentSide = parentGroup.Parent as LayoutAnchorSide;
                var previousContainer = parentGroup.PreviousContainer;

                if (previousContainer == null)
                {
                    AnchorSide side = (parentGroup.Parent as LayoutAnchorSide).Side;
                    switch (side)
                    {
                        case AnchorSide.Right:
                            if (parentGroup.Root.RootPanel.Orientation == Orientation.Horizontal)
                            {
                                previousContainer = new LayoutAnchorablePane();
                                parentGroup.Root.RootPanel.Children.Add(previousContainer);
                            }
                            else
                            {
                                previousContainer = new LayoutAnchorablePane();
                                LayoutPanel panel = new LayoutPanel() { Orientation = Orientation.Horizontal };
                                LayoutRoot root = parentGroup.Root as LayoutRoot;
                                LayoutPanel oldRootPanel = parentGroup.Root.RootPanel as LayoutPanel;
                                root.RootPanel = panel;
                                panel.Children.Add(oldRootPanel);
                                panel.Children.Add(previousContainer);
                            }
                            break;
                        case AnchorSide.Left:
                            if (parentGroup.Root.RootPanel.Orientation == Orientation.Horizontal)
                            {
                                previousContainer = new LayoutAnchorablePane();
                                parentGroup.Root.RootPanel.Children.Insert(0, previousContainer);
                            }
                            else
                            {
                                previousContainer = new LayoutAnchorablePane();
                                LayoutPanel panel = new LayoutPanel() { Orientation = Orientation.Horizontal };
                                LayoutRoot root = parentGroup.Root as LayoutRoot;
                                LayoutPanel oldRootPanel = parentGroup.Root.RootPanel as LayoutPanel;
                                root.RootPanel = panel;
                                panel.Children.Add(previousContainer);
                                panel.Children.Add(oldRootPanel);
                            }
                            break;
                        case AnchorSide.Top:
                            if (parentGroup.Root.RootPanel.Orientation == Orientation.Vertical)
                            {
                                previousContainer = new LayoutAnchorablePane();
                                parentGroup.Root.RootPanel.Children.Insert(0, previousContainer);
                            }
                            else
                            {
                                previousContainer = new LayoutAnchorablePane();
                                LayoutPanel panel = new LayoutPanel() { Orientation = Orientation.Vertical };
                                LayoutRoot root = parentGroup.Root as LayoutRoot;
                                LayoutPanel oldRootPanel = parentGroup.Root.RootPanel as LayoutPanel;
                                root.RootPanel = panel;
                                panel.Children.Add(previousContainer);
                                panel.Children.Add(oldRootPanel);
                            }
                            break;
                        case AnchorSide.Bottom:
                            if (parentGroup.Root.RootPanel.Orientation == Orientation.Vertical)
                            {
                                previousContainer = new LayoutAnchorablePane();
                                parentGroup.Root.RootPanel.Children.Add(previousContainer);
                            }
                            else
                            {
                                previousContainer = new LayoutAnchorablePane();
                                LayoutPanel panel = new LayoutPanel() { Orientation = Orientation.Vertical };
                                LayoutRoot root = parentGroup.Root as LayoutRoot;
                                LayoutPanel oldRootPanel = parentGroup.Root.RootPanel as LayoutPanel;
                                root.RootPanel = panel;
                                panel.Children.Add(oldRootPanel);
                                panel.Children.Add(previousContainer);
                            }
                            break;
                    }
                }


                foreach (var anchorableToToggle in parentGroup.Children.ToArray())
                    previousContainer.Children.Add(anchorableToToggle);

                parentSide.Children.Remove(parentGroup);

                HideAutoHideWindow();
            }
            #endregion
            #region Anchorable is docked
            else if (anchorableModel.Parent is LayoutAnchorablePane)
            {
                var parentPane = anchorableModel.Parent as LayoutAnchorablePane;

                var newAnchorGroup = new LayoutAnchorGroup() { PreviousContainer = parentPane };

                foreach (var anchorableToImport in parentPane.Children.ToArray())
                    newAnchorGroup.Children.Add(anchorableToImport);

                //detect anchor side for the pane
                var anchorSide = parentPane.GetSide();

                switch (anchorSide)
                {
                    case AnchorSide.Right:
                        Layout.RightSide.Children.Add(newAnchorGroup);
                        break;
                    case AnchorSide.Left:
                        Layout.LeftSide.Children.Add(newAnchorGroup);
                        break;
                    case AnchorSide.Top:
                        Layout.TopSide.Children.Add(newAnchorGroup);
                        break;
                    case AnchorSide.Bottom:
                        Layout.BottomSide.Children.Add(newAnchorGroup);
                        break;
                }
            }
            #endregion
        }

        #endregion

        #region LayoutDocument & LayoutAnchorable Templates

        #region DocumentTemplate

        /// <summary>
        /// DocumentTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty DocumentTemplateProperty =
            DependencyProperty.Register("DocumentTemplate", typeof(DataTemplate), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplate)null,
                    new PropertyChangedCallback(OnDocumentTemplateChanged)));

        /// <summary>
        /// Gets or sets the DocumentTemplate property.  This dependency property 
        /// indicates data template to used when creating document contents.
        /// </summary>
        public DataTemplate DocumentTemplate
        {
            get { return (DataTemplate)GetValue(DocumentTemplateProperty); }
            set { SetValue(DocumentTemplateProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DocumentTemplate property.
        /// </summary>
        private static void OnDocumentTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnDocumentTemplateChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DocumentTemplate property.
        /// </summary>
        protected virtual void OnDocumentTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region DocumentTemplateSelector

        /// <summary>
        /// DocumentTemplateSelector Dependency Property
        /// </summary>
        public static readonly DependencyProperty DocumentTemplateSelectorProperty =
            DependencyProperty.Register("DocumentTemplateSelector", typeof(DataTemplateSelector), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplateSelector)null,
                    new PropertyChangedCallback(OnDocumentTemplateSelectorChanged)));

        /// <summary>
        /// Gets or sets the DocumentTemplateSelector property.  This dependency property 
        /// indicates the data template selector to use when creating data templates for documents.
        /// </summary>
        public DataTemplateSelector DocumentTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(DocumentTemplateSelectorProperty); }
            set { SetValue(DocumentTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DocumentTemplateSelector property.
        /// </summary>
        private static void OnDocumentTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnDocumentTemplateSelectorChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DocumentTemplateSelector property.
        /// </summary>
        protected virtual void OnDocumentTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region AnchorableTemplate

        /// <summary>
        /// AnchorableTemplate Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorableTemplateProperty =
            DependencyProperty.Register("AnchorableTemplate", typeof(DataTemplate), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplate)null,
                    new PropertyChangedCallback(OnAnchorableTemplateChanged)));

        /// <summary>
        /// Gets or sets the AnchorableTemplate property.  This dependency property 
        /// indicates the template to use to render anchorable contents.
        /// </summary>
        public DataTemplate AnchorableTemplate
        {
            get { return (DataTemplate)GetValue(AnchorableTemplateProperty); }
            set { SetValue(AnchorableTemplateProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AnchorableTemplate property.
        /// </summary>
        private static void OnAnchorableTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnAnchorableTemplateChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AnchorableTemplate property.
        /// </summary>
        protected virtual void OnAnchorableTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region AnchorableTemplateSelector

        /// <summary>
        /// AnchorableTemplateSelector Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorableTemplateSelectorProperty =
            DependencyProperty.Register("AnchorableTemplateSelector", typeof(DataTemplateSelector), typeof(DockingManager),
                new FrameworkPropertyMetadata((DataTemplateSelector)null,
                    new PropertyChangedCallback(OnAnchorableTemplateSelectorChanged)));

        /// <summary>
        /// Gets or sets the AnchorableTemplateSelector property.  This dependency property 
        /// indicates selector object to use for anchorable templates.
        /// </summary>
        public DataTemplateSelector AnchorableTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(AnchorableTemplateSelectorProperty); }
            set { SetValue(AnchorableTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AnchorableTemplateSelector property.
        /// </summary>
        private static void OnAnchorableTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnAnchorableTemplateSelectorChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AnchorableTemplateSelector property.
        /// </summary>
        protected virtual void OnAnchorableTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
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

            if (layout.Descendents().OfType<LayoutDocument>().Any())
                throw new InvalidOperationException("Unable to set the DocumentsSource property if LayoutDocument objects are already present in the model");

            var documents = documentsSource as IEnumerable;
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
                foreach (var documentToImport in (documentsSource as IEnumerable))
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

        static ICommand _defaultDocumentCloseCommand = new RelayCommand((p)=>ExecuteDocumentCloseCommand(p), (p) => CanExecuteDocumentCloseCommand(p));

        /// <summary>
        /// DocumentCloseCommand Dependency Property
        /// </summary>
        public static readonly DependencyProperty DocumentCloseCommandProperty =
            DependencyProperty.Register("DocumentCloseCommand", typeof(ICommand), typeof(DockingManager),
                new FrameworkPropertyMetadata(_defaultDocumentCloseCommand,
                    new PropertyChangedCallback(OnDocumentCloseCommandChanged),
                    new CoerceValueCallback(CoerceDocumentCloseCommandValue)));

        /// <summary>
        /// Gets or sets the DocumentCloseCommand property.  This dependency property 
        /// indicates the command to execute when user click the document close button.
        /// </summary>
        public ICommand DocumentCloseCommand
        {
            get { return (ICommand)GetValue(DocumentCloseCommandProperty); }
            set { SetValue(DocumentCloseCommandProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DocumentCloseCommand property.
        /// </summary>
        private static void OnDocumentCloseCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnDocumentCloseCommandChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DocumentCloseCommand property.
        /// </summary>
        protected virtual void OnDocumentCloseCommandChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Coerces the DocumentCloseCommand value.
        /// </summary>
        private static object CoerceDocumentCloseCommandValue(DependencyObject d, object value)
        {
            if (value == null)
                return _defaultDocumentCloseCommand;

            return value;
        }

        private static bool CanExecuteDocumentCloseCommand(object parameter)
        {
            return parameter != null;
        }

        private static void ExecuteDocumentCloseCommand(object parameter)
        {
            var document = parameter as LayoutDocument;
            if (document == null)
                return;

            var dockingManager = document.Root.Manager;
            dockingManager._ExecuteDocumentCloseCommand(document);
        }

        void _ExecuteDocumentCloseCommand(LayoutDocument document)
        {
            if (DocumentClosing != null)
            {
                var evargs = new DocumentClosingEventArgs(document);
                DocumentClosing(this, evargs);
                if (evargs.Cancel)
                    return;
            }

            document.Close();

            if (DocumentClose != null)
            { 
                var evargs = new DocumentCloseEventArgs(document);
                DocumentClose(this, evargs);
            }
        }

        /// <summary>
        /// Event fired when a document is about to be closed
        /// </summary>
        /// <remarks>Subscribers have the opportuniy to cancel the operation.</remarks>
        public event EventHandler<CancelEventArgs> DocumentClosing;

        /// <summary>
        /// Event fired after a document is closed
        /// </summary>
        public event EventHandler DocumentClose;



        #endregion

        #region DocumentCloseAllButThisCommand
        static ICommand _defaultDocumentCloseAllButThisCommand = new RelayCommand((p) => ExecuteDocumentCloseAllButThisCommand(p), (p) => CanExecuteDocumentCloseAllButThisCommand(p));

        /// <summary>
        /// DocumentCloseAllButThisCommand Dependency Property
        /// </summary>
        public static readonly DependencyProperty DocumentCloseAllButThisCommandProperty =
            DependencyProperty.Register("DocumentCloseAllButThisCommand", typeof(ICommand), typeof(DockingManager),
                new FrameworkPropertyMetadata(_defaultDocumentCloseAllButThisCommand,
                    new PropertyChangedCallback(OnDocumentCloseAllButThisCommandChanged),
                    new CoerceValueCallback(CoerceDocumentCloseAllButThisCommandValue)));

        /// <summary>
        /// Gets or sets the DocumentCloseAllButThisCommand property.  This dependency property 
        /// indicates the 'Close All But This' command.
        /// </summary>
        public ICommand DocumentCloseAllButThisCommand
        {
            get { return (ICommand)GetValue(DocumentCloseAllButThisCommandProperty); }
            set { SetValue(DocumentCloseAllButThisCommandProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DocumentCloseAllButThisCommand property.
        /// </summary>
        private static void OnDocumentCloseAllButThisCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnDocumentCloseAllButThisCommandChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DocumentCloseAllButThisCommand property.
        /// </summary>
        protected virtual void OnDocumentCloseAllButThisCommandChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Coerces the DocumentCloseAllButThisCommand value.
        /// </summary>
        private static object CoerceDocumentCloseAllButThisCommandValue(DependencyObject d, object value)
        {
            if (value == null)
                return _defaultDocumentCloseAllButThisCommand;

            return value;
        }

        private static bool CanExecuteDocumentCloseAllButThisCommand(object parameter)
        {
            var document = parameter as LayoutDocument;
            if (document == null)
                return false;
            var root = document.Root;
            if (root == null)
                return false;

            return document.Root.Manager.Layout.
                Descendents().OfType<LayoutDocument>().Where(d => d != document).Any();
        }

        private static void ExecuteDocumentCloseAllButThisCommand(object parameter)
        {
            var document = parameter as LayoutDocument;
            if (document == null)
                return;
            var root = document.Root;
            if (root == null)
                return;

            root.Manager._ExecuteDocumentCloseAllButThisCommand(document);
        }

        void _ExecuteDocumentCloseAllButThisCommand(LayoutDocument document)
        {
            foreach (var documentToClose in Layout.Descendents().OfType<LayoutDocument>().Where(d => d != document).ToArray())
            {
                if (DocumentClosing != null)
                {
                    var evargs = new DocumentClosingEventArgs(documentToClose);
                    DocumentClosing(this, evargs);
                    if (evargs.Cancel)
                        continue;
                }

                documentToClose.Close();

                if (DocumentClose != null)
                {
                    var evargs = new DocumentCloseEventArgs(document);
                    DocumentClose(this, evargs);
                }
            }
        }

        #endregion

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


        void DetachAnchorablesSource(LayoutRoot layout, IEnumerable anchorablesSource)
        {
            if (anchorablesSource == null)
                return;

            if (layout == null)
                return;

            if (layout.Descendents().OfType<LayoutAnchorable>().Any())
                throw new InvalidOperationException("Unable to set the AnchorablesSource property if LayoutAnchorable objects are already present in the model");

            var anchorables = anchorablesSource as IEnumerable;
            LayoutAnchorablePane anchorablePane = null;
            if (layout.ActiveContent != null)
            {
                //look for active content parent pane
                anchorablePane = layout.ActiveContent.Parent as LayoutAnchorablePane;
            }

            if (anchorablePane == null)
            {
                //look for a pane on the right side
                anchorablePane = layout.Descendents().OfType<LayoutAnchorablePane>().Where(pane => pane.GetSide() == AnchorSide.Right).FirstOrDefault();
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
                foreach (var anchorableToImport in (anchorablesSource as IEnumerable))
                {
                    anchorablePane.Children.Add(new LayoutAnchorable() { Content = anchorableToImport });
                }
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
                    var anchorablesToRemove = Layout.Descendents().OfType<LayoutDocument>().Where(d => e.OldItems.Contains(d.Content)).ToArray();
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
                        anchorablePane = Layout.Descendents().OfType<LayoutAnchorablePane>().Where(pane => pane.GetSide() == AnchorSide.Right).FirstOrDefault();
                    }

                    if (anchorablePane == null)
                    {
                        //look for an available pane
                        anchorablePane = Layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault();
                    }

                    if (anchorablePane == null)
                    {
                        //create a pane on the fly on the right side

                    }

                    if (anchorablePane != null)
                    {
                        foreach (var anchorableToImport in e.NewItems)
                        {
                            anchorablePane.Children.Add(new LayoutAnchorable() { Content = anchorableToImport });
                        }
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                //NOTE: I'm going to clear every document present in layout but
                //some documents may have been added directly to the layout, for now I clear them too
                var anchorablesToRemove = Layout.Descendents().OfType<LayoutDocument>().ToArray();
                foreach (var anchorableToRemove in anchorablesToRemove)
                {
                    (anchorableToRemove.Parent as ILayoutContainer).RemoveChild(
                        anchorableToRemove);
                }
            }
        }

        void AttachAnchorablesSource(LayoutRoot layout, IEnumerable anchorablesSource)
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

        #region AnchorableCloseCommand

        static ICommand _defaultAnchorableCloseCommand = new RelayCommand((p) => ExecuteAnchorableCloseCommand(p), (p) => CanExecuteAnchorableCloseCommand(p));

        /// <summary>
        /// AnchorableCloseCommand Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorableCloseCommandProperty =
            DependencyProperty.Register("AnchorableCloseCommand", typeof(ICommand), typeof(DockingManager),
                new FrameworkPropertyMetadata((ICommand)_defaultAnchorableCloseCommand,
                    new PropertyChangedCallback(OnAnchorableCloseCommandChanged),
                    new CoerceValueCallback(CoerceAnchorableCloseCommandValue)));

        /// <summary>
        /// Gets or sets the AnchorableCloseCommand property.  This dependency property 
        /// indicates the command to execute when an anchorable is closed.
        /// </summary>
        public ICommand AnchorableCloseCommand
        {
            get { return (ICommand)GetValue(AnchorableCloseCommandProperty); }
            set { SetValue(AnchorableCloseCommandProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AnchorableCloseCommand property.
        /// </summary>
        private static void OnAnchorableCloseCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnAnchorableCloseCommandChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AnchorableCloseCommand property.
        /// </summary>
        protected virtual void OnAnchorableCloseCommandChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Coerces the AnchorableCloseCommand value.
        /// </summary>
        private static object CoerceAnchorableCloseCommandValue(DependencyObject d, object value)
        {
            if (value == null)
                return _defaultAnchorableCloseCommand;

            return value;
        }


        private static bool CanExecuteAnchorableCloseCommand(object anchorable)
        {
            return true;
        }

        private static void ExecuteAnchorableCloseCommand(object anchorable)
        {
            var model = anchorable as LayoutAnchorable;
            if (model != null)
            { 
                model.Close();
            }
        }

        #endregion

        #region AnchorableHideCommand

        static ICommand _defaultAnchorableHideCommand = new RelayCommand((p) => ExecuteAnchorableHideCommand(p), (p) => CanExecuteAnchorableHideCommand(p));

        /// <summary>
        /// AnchorableHideCommand Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorableHideCommandProperty =
            DependencyProperty.Register("AnchorableHideCommand", typeof(ICommand), typeof(DockingManager),
                new FrameworkPropertyMetadata((ICommand)_defaultAnchorableHideCommand,
                    new PropertyChangedCallback(OnAnchorableHideCommandChanged),
                    new CoerceValueCallback(CoerceAnchorableHideCommandValue)));

        /// <summary>
        /// Gets or sets the AnchorableHideCommand property.  This dependency property 
        /// indicates the command to execute when an anchorable is hidden.
        /// </summary>
        public ICommand AnchorableHideCommand
        {
            get { return (ICommand)GetValue(AnchorableHideCommandProperty); }
            set { SetValue(AnchorableHideCommandProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AnchorableHideCommand property.
        /// </summary>
        private static void OnAnchorableHideCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnAnchorableHideCommandChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AnchorableHideCommand property.
        /// </summary>
        protected virtual void OnAnchorableHideCommandChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Coerces the AnchorableHideCommand value.
        /// </summary>
        private static object CoerceAnchorableHideCommandValue(DependencyObject d, object value)
        {
            if (value == null)
                return _defaultAnchorableHideCommand;

            return value;
        }


        private static bool CanExecuteAnchorableHideCommand(object anchorable)
        {
            return true;
        }

        private static void ExecuteAnchorableHideCommand(object anchorable)
        {
            var model = anchorable as LayoutAnchorable;
            if (model != null)
            {
                //by default hide the anchorable
                model.Hide();
            }
        }

        #endregion

        #region AnchorableAutoHideCommand

        static ICommand _defaultAnchorableAutoHideCommand = new RelayCommand((p) => ExecuteAnchorableAutoHideCommand(p), (p) => CanExecuteAnchorableAutoHideCommand(p));

        /// <summary>
        /// AnchorableAutoHideCommand Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorableAutoHideCommandProperty =
            DependencyProperty.Register("AnchorableAutoHideCommand", typeof(ICommand), typeof(DockingManager),
                new FrameworkPropertyMetadata((ICommand)_defaultAnchorableAutoHideCommand,
                    new PropertyChangedCallback(OnAnchorableAutoHideCommandChanged),
                    new CoerceValueCallback(CoerceAnchorableAutoHideCommandValue)));

        /// <summary>
        /// Gets or sets the AnchorableAutoHideCommand property.  This dependency property 
        /// indicates the command to execute when user click the auto hide button.
        /// </summary>
        /// <remarks>By default this command toggles auto hide state for an anchorable.</remarks>
        public ICommand AnchorableAutoHideCommand
        {
            get { return (ICommand)GetValue(AnchorableAutoHideCommandProperty); }
            set { SetValue(AnchorableAutoHideCommandProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AnchorableAutoHideCommand property.
        /// </summary>
        private static void OnAnchorableAutoHideCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnAnchorableAutoHideCommandChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AnchorableAutoHideCommand property.
        /// </summary>
        protected virtual void OnAnchorableAutoHideCommandChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Coerces the AnchorableAutoHideCommand value.
        /// </summary>
        private static object CoerceAnchorableAutoHideCommandValue(DependencyObject d, object value)
        {
            return value;
        }

        private static bool CanExecuteAnchorableAutoHideCommand(object anchorable)
        {
            var model = anchorable as LayoutAnchorable;
            if (model == null || model.FindParent<LayoutAnchorableFloatingWindow>() != null)
                return false;//is floating
            return true;
        }

        private static void ExecuteAnchorableAutoHideCommand(object anchorable)
        {
            var model = anchorable as LayoutAnchorable;
            if (model != null)
                model.Root.Manager.ToggleAutoHide(model);
        }

        #endregion

        #region AnchorableFloatCommand

        static ICommand _defaultAnchorableFloatCommand = new RelayCommand((p) => ExecuteAnchorableFloatCommand(p), (p) => CanExecuteAnchorableFloatCommand(p));

        /// <summary>
        /// AnchorableFloatCommand Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorableFloatCommandProperty =
            DependencyProperty.Register("AnchorableFloatCommand", typeof(ICommand), typeof(DockingManager),
                new FrameworkPropertyMetadata((ICommand)_defaultAnchorableFloatCommand,
                    new PropertyChangedCallback(OnAnchorableFloatCommandChanged),
                    new CoerceValueCallback(CoerceAnchorableFloatCommandValue)));

        /// <summary>
        /// Gets or sets the AnchorableFloatCommand property.  This dependency property 
        /// indicates the command to execute when user click the float button.
        /// </summary>
        /// <remarks>By default this command move the anchorable inside new floating window.</remarks>
        public ICommand AnchorableFloatCommand
        {
            get { return (ICommand)GetValue(AnchorableFloatCommandProperty); }
            set { SetValue(AnchorableFloatCommandProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AnchorableFloatCommand property.
        /// </summary>
        private static void OnAnchorableFloatCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnAnchorableFloatCommandChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AnchorableFloatCommand property.
        /// </summary>
        protected virtual void OnAnchorableFloatCommandChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Coerces the AnchorableFloatCommand value.
        /// </summary>
        private static object CoerceAnchorableFloatCommandValue(DependencyObject d, object value)
        {
            return value;
        }

        private static bool CanExecuteAnchorableFloatCommand(object p)
        {
            return true;
        }

        private static void ExecuteAnchorableFloatCommand(object anchorable)
        {
            var model = anchorable as LayoutAnchorable;
            
        }

        #endregion

        #region AnchorableDockCommand

        static ICommand _defaultAnchorableDockCommand = new RelayCommand((p) => ExecuteAnchorableDockCommand(p), (p) => CanExecuteAnchorableDockCommand(p));

        /// <summary>
        /// AnchorableDockCommand Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorableDockCommandProperty =
            DependencyProperty.Register("AnchorableDockCommand", typeof(ICommand), typeof(DockingManager),
                new FrameworkPropertyMetadata((ICommand)_defaultAnchorableDockCommand,
                    new PropertyChangedCallback(OnAnchorableDockCommandChanged),
                    new CoerceValueCallback(CoerceAnchorableDockCommandValue)));

        /// <summary>
        /// Gets or sets the AnchorableDockCommand property.  This dependency property 
        /// indicates the command to execute when user click the Dock button.
        /// </summary>
        /// <remarks>By default this command moves the anchorable inside the container pane which previously hosted the object.</remarks>
        public ICommand AnchorableDockCommand
        {
            get { return (ICommand)GetValue(AnchorableDockCommandProperty); }
            set { SetValue(AnchorableDockCommandProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AnchorableDockCommand property.
        /// </summary>
        private static void OnAnchorableDockCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnAnchorableDockCommandChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AnchorableDockCommand property.
        /// </summary>
        protected virtual void OnAnchorableDockCommandChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Coerces the AnchorableDockCommand value.
        /// </summary>
        private static object CoerceAnchorableDockCommandValue(DependencyObject d, object value)
        {
            return value;
        }

        private static bool CanExecuteAnchorableDockCommand(object p)
        {
            return true;
        }

        private static void ExecuteAnchorableDockCommand(object anchorable)
        {
            var model = anchorable as LayoutAnchorable;

        }

        #endregion

        #region AnchorableDockAsDocumentCommand

        static ICommand _defaultAnchorableDockAsDocumentCommand = new RelayCommand((p) => ExecuteAnchorableDockAsDocumentCommand(p), (p) => CanExecuteAnchorableDockAsDocumentCommand(p));

        /// <summary>
        /// AnchorableDockAsDocumentCommand Dependency Property
        /// </summary>
        public static readonly DependencyProperty AnchorableDockAsDocumentCommandProperty =
            DependencyProperty.Register("AnchorableDockAsDocumentCommand", typeof(ICommand), typeof(DockingManager),
                new FrameworkPropertyMetadata((ICommand)_defaultAnchorableDockAsDocumentCommand,
                    new PropertyChangedCallback(OnAnchorableDockAsDocumentCommandChanged),
                    new CoerceValueCallback(CoerceAnchorableDockAsDocumentCommandValue)));

        /// <summary>
        /// Gets or sets the AnchorableDockAsDocumentCommand property.  This dependency property 
        /// indicates the command to execute when user click the DockAsDocument button.
        /// </summary>
        /// <remarks>By default this command move the anchorable inside the last focused document pane.</remarks>
        public ICommand AnchorableDockAsDocumentCommand
        {
            get { return (ICommand)GetValue(AnchorableDockAsDocumentCommandProperty); }
            set { SetValue(AnchorableDockAsDocumentCommandProperty, value); }
        }

        /// <summary>
        /// Handles changes to the AnchorableDockAsDocumentCommand property.
        /// </summary>
        private static void OnAnchorableDockAsDocumentCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockingManager)d).OnAnchorableDockAsDocumentCommandChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AnchorableDockAsDocumentCommand property.
        /// </summary>
        protected virtual void OnAnchorableDockAsDocumentCommandChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Coerces the AnchorableDockAsDocumentCommand value.
        /// </summary>
        private static object CoerceAnchorableDockAsDocumentCommandValue(DependencyObject d, object value)
        {
            return value;
        }

        private static bool CanExecuteAnchorableDockAsDocumentCommand(object p)
        {
            return true;
        }

        private static void ExecuteAnchorableDockAsDocumentCommand(object anchorable)
        {
            var model = anchorable as LayoutAnchorable;

        }

        #endregion

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


    }
}
