using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using Treefrog.V2.ViewModel.Commands;
using Treefrog.V2.ViewModel.Tools;

namespace Treefrog.V2.ViewModel
{
    public abstract class DocumentVM : ViewModelBase
    {
        public abstract string Name { get; }

        public virtual bool SupportsZoom
        {
            get { return false; }
        }

        public virtual double ZoomLevel
        {
            get { return 1; }
            set { }
        }

        public virtual string Coordinates
        {
            get { return ""; }
            set { }
        }

        public virtual CommandHistory CommandHistory
        {
            get { return null; }
        }

        public virtual IEnumerable<IToolCollection> RegisteredToolCollections
        {
            get { yield break; }
        }

        public T LookupToolCollection<T> () 
            where T : IToolCollection 
        {
            foreach (IToolCollection tc in RegisteredToolCollections)
                if (tc is T)
                    return (T)tc;
            return default(T);
        }
    }
}
