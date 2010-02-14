using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace SelectionForeground
{
    static class TypeExports
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("selection-foreground")]
        static ClassificationTypeDefinition OrdinaryClassificationType = null;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "selection-foreground")]
    [Name("selection-foreground")]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    sealed class SelectionForeground : ClassificationFormatDefinition
    {
        public SelectionForeground()
        {
            this.DisplayName = "Selection Foreground";
            this.ForegroundColor = Colors.White;
        }
    }
}
