using Godot;

namespace BushyCore
{
    [Tool]
    public partial class BushyAnimations : Resource
    {
        [Export]
        public Animation Idle;
        public BushyAnimations() {}
    }
}

