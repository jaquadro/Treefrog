using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.V2.ViewModel.Annotations;

namespace Treefrog.V2.Controls.Annotations
{
    public class AnnotationRendererFactory
    {
        public static AnnotationRenderer Create (Annotation annot)
        {
            if (annot.GetType() == typeof(SelectionAnnot))
                return new SelectionAnnotRenderer(annot as SelectionAnnot);

            return null;
        }
    }
}
