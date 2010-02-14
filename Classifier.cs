using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Editor;
using System.Diagnostics;

namespace SelectionForeground
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("any")]
    [TagType(typeof(ClassificationTag))]
    sealed class TaggerProvider : IViewTaggerProvider
    {
        [Import]
        IClassificationTypeRegistryService Registry = null;

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (buffer != textView.TextBuffer)
                return null;

            return new SelectionTagger(textView, Registry.GetClassificationType("selection-foreground")) as ITagger<T>;
        }
    }

    sealed class SelectionTagger : ITagger<ClassificationTag>
    {
        ITextView _view;
        IClassificationType _type;

        NormalizedSnapshotSpanCollection _currentSpans;

        public SelectionTagger(ITextView view, IClassificationType type)
        {
            _view = view;
            _type = type;

            _currentSpans = new NormalizedSnapshotSpanCollection();

            _view.GotAggregateFocus += SetupSelectionChangedListener;
        }

        void OnSelectionChanged(object sender, EventArgs e)
        {
            ITextSnapshot snapshot = _view.TextSnapshot;

            if (_currentSpans.Count > 0 && _currentSpans[0].Snapshot != snapshot)
                _currentSpans = new NormalizedSnapshotSpanCollection(_currentSpans.Select(s => s.TranslateTo(snapshot, SpanTrackingMode.EdgeInclusive)));

            var union = NormalizedSnapshotSpanCollection.Union(_currentSpans, _view.Selection.SelectedSpans);

            if (union.Count == 0)
                return;

            SnapshotSpan changedSpan = new SnapshotSpan(union[0].Start, union[union.Count - 1].End);

            _currentSpans = _view.Selection.SelectedSpans;

            var temp = TagsChanged;
            if (temp != null)
                TagsChanged(this, new SnapshotSpanEventArgs(changedSpan));
        }

        void SetupSelectionChangedListener(object sender, EventArgs e)
        {
            if (_view.Selection != null)
            {
                _view.Selection.SelectionChanged += OnSelectionChanged;
                _view.GotAggregateFocus -= SetupSelectionChangedListener;
            }
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (_currentSpans.Count == 0)
                yield break;
            if (spans == null || spans.Count == 0)
                yield break;

            ITextSnapshot snapshot = spans[0].Snapshot;

            spans = new NormalizedSnapshotSpanCollection(spans.Select(s => s.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive)));

            foreach (var span in NormalizedSnapshotSpanCollection.Intersection(_currentSpans, spans))
                yield return new TagSpan<ClassificationTag>(span, new ClassificationTag(_type));
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
    }
}
