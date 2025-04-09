using Godot;

namespace BushyCore
{
    [Tool]
    [GlobalClass]
    public partial class IntEvent : BaseEventResource<int>
    {
        [Export]
        protected override int _editorValue { get; set; }
        [ExportToolButton("Try Editor Value")]
        private Callable _editorCallable => Callable.From(TestWithEditorValue);
    }
}