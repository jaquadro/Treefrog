using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
    public class NavigatorWindow : Window
    {
        static NavigatorWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavigatorWindow), new FrameworkPropertyMetadata(typeof(NavigatorWindow)));
            ShowActivatedProperty.OverrideMetadata(typeof(NavigatorWindow), new FrameworkPropertyMetadata(false));
            ShowInTaskbarProperty.OverrideMetadata(typeof(NavigatorWindow), new FrameworkPropertyMetadata(false));
        }

        DockingManager _manager;
        internal NavigatorWindow(DockingManager manager)
        {
            _manager = manager;

            SetAnchorables(_manager.Layout.Descendents().OfType<LayoutAnchorable>().Where(a => a.IsVisible).Select(d => (LayoutAnchorableItem)_manager.GetLayoutItemFromModel(d)).ToArray());
            SetDocuments(_manager.Layout.Descendents().OfType<LayoutDocument>().OrderByDescending(d => d.LastActivationTimeStamp.GetValueOrDefault()).Select(d => (LayoutDocumentItem)_manager.GetLayoutItemFromModel(d)).ToArray());

            if (Documents.Length > 1)
                InternalSetSelectedDocument(Documents[1]);

            this.DataContext = this;
        }

        #region Documents

        /// <summary>
        /// Documents Read-Only Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey DocumentsPropertyKey
            = DependencyProperty.RegisterReadOnly("Documents", typeof(IEnumerable<LayoutDocumentItem>), typeof(NavigatorWindow),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty DocumentsProperty
            = DocumentsPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the Documents property.  This dependency property 
        /// indicates the list of documents.
        /// </summary>
        public LayoutDocumentItem[] Documents
        {
            get { return (LayoutDocumentItem[])GetValue(DocumentsProperty); }
        }

        /// <summary>
        /// Provides a secure method for setting the Documents property.  
        /// This dependency property indicates the list of documents.
        /// </summary>
        /// <param name="value">The new value for the property.</param>
        protected void SetDocuments(LayoutDocumentItem[] value)
        {
            SetValue(DocumentsPropertyKey, value);
        }

        #endregion

        #region Anchorables

        /// <summary>
        /// Anchorables Read-Only Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey AnchorablesPropertyKey
            = DependencyProperty.RegisterReadOnly("Anchorables", typeof(IEnumerable<LayoutAnchorableItem>), typeof(NavigatorWindow),
                new FrameworkPropertyMetadata((IEnumerable<LayoutAnchorableItem>)null));

        public static readonly DependencyProperty AnchorablesProperty
            = AnchorablesPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the Anchorables property.  This dependency property 
        /// indicates the list of anchorables.
        /// </summary>
        public IEnumerable<LayoutAnchorableItem> Anchorables
        {
            get { return (IEnumerable<LayoutAnchorableItem>)GetValue(AnchorablesProperty); }
        }

        /// <summary>
        /// Provides a secure method for setting the Anchorables property.  
        /// This dependency property indicates the list of anchorables.
        /// </summary>
        /// <param name="value">The new value for the property.</param>
        protected void SetAnchorables(IEnumerable<LayoutAnchorableItem> value)
        {
            SetValue(AnchorablesPropertyKey, value);
        }

        #endregion

        #region SelectedDocument

        /// <summary>
        /// SelectedDocument Dependency Property
        /// </summary>
        public static readonly DependencyProperty SelectedDocumentProperty =
            DependencyProperty.Register("SelectedDocument", typeof(LayoutDocumentItem), typeof(NavigatorWindow),
                new FrameworkPropertyMetadata((LayoutDocumentItem)null,
                    new PropertyChangedCallback(OnSelectedDocumentChanged)));

        /// <summary>
        /// Gets or sets the SelectedDocument property.  This dependency property 
        /// indicates the selected document.
        /// </summary>
        public LayoutDocumentItem SelectedDocument
        {
            get { return (LayoutDocumentItem)GetValue(SelectedDocumentProperty); }
            set { SetValue(SelectedDocumentProperty, value); }
        }

        /// <summary>
        /// Handles changes to the SelectedDocument property.
        /// </summary>
        private static void OnSelectedDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NavigatorWindow)d).OnSelectedDocumentChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the SelectedDocument property.
        /// </summary>
        protected virtual void OnSelectedDocumentChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_internalSetSelectedDocument)
                return;

            var selectedDocument = e.NewValue as LayoutDocumentItem;
            if (selectedDocument != null && selectedDocument.ActivateCommand.CanExecute(null))
                selectedDocument.ActivateCommand.Execute(null);

            Hide();
        }

        bool _internalSetSelectedDocument = false;
        void InternalSetSelectedDocument(LayoutDocumentItem documentToSelect)
        {
            _internalSetSelectedDocument = true;
            SelectedDocument = documentToSelect;
            _internalSetSelectedDocument = false;
        }

        #endregion

        #region SelectedAnchorable

        /// <summary>
        /// SelectedAnchorable Dependency Property
        /// </summary>
        public static readonly DependencyProperty SelectedAnchorableProperty =
            DependencyProperty.Register("SelectedAnchorable", typeof(LayoutAnchorableItem), typeof(NavigatorWindow),
                new FrameworkPropertyMetadata((LayoutAnchorableItem)null,
                    new PropertyChangedCallback(OnSelectedAnchorableChanged)));

        /// <summary>
        /// Gets or sets the SelectedAnchorable property.  This dependency property 
        /// indicates the selected anchorable.
        /// </summary>
        public LayoutAnchorableItem SelectedAnchorable
        {
            get { return (LayoutAnchorableItem)GetValue(SelectedAnchorableProperty); }
            set { SetValue(SelectedAnchorableProperty, value); }
        }

        /// <summary>
        /// Handles changes to the SelectedAnchorable property.
        /// </summary>
        private static void OnSelectedAnchorableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NavigatorWindow)d).OnSelectedAnchorableChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the SelectedAnchorable property.
        /// </summary>
        protected virtual void OnSelectedAnchorableChanged(DependencyPropertyChangedEventArgs e)
        {
            var selectedAnchorable = e.NewValue as LayoutAnchorableItem;
            if (selectedAnchorable.ActivateCommand.CanExecute(null))
            {
                selectedAnchorable.ActivateCommand.Execute(null);
                Hide();
            }
        }

        #endregion


        internal void SelectNextDocument()
        {
            if (SelectedDocument != null)
            {
                int docIndex = Documents.IndexOf<LayoutDocumentItem>(SelectedDocument);
                docIndex++;
                if (docIndex == Documents.Length)
                    docIndex = 0;
                InternalSetSelectedDocument(Documents[docIndex]);
            }

        }
    }
}
