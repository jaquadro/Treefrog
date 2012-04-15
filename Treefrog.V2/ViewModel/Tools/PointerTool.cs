using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.V2.ViewModel.Tools
{
    public abstract class PointerTool
    {
        public virtual void StartPointerSequence (PointerEventInfo info)
        {
        }

        public virtual void UpdatePointerSequence (PointerEventInfo info)
        {
        }

        public virtual void EndPointerSequence (PointerEventInfo info)
        {
        }

        public virtual void PointerPosition (PointerEventInfo info)
        {
        }

        public virtual void PointerEnterField ()
        {
        }

        public virtual void PointerLeaveField ()
        {
        }
    }
}
