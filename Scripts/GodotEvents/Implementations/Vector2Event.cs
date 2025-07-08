using Godot;

namespace BushyCore
{
    [Tool]
    [GlobalClass]
    public partial class Vector2Event : BaseEventResource<Vector2>
    {
        [Export]
        protected override Vector2 _editorValue { get; set; }
        [ExportToolButton("Try Editor Value")]
        private Callable _editorCallable => Callable.From(TestWithEditorValue);
    }
}