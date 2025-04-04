using Godot;
using System;
using System.Linq;
namespace BushyCore
{
    [Tool]
    [GlobalClass]
    public partial class AnimationNodeAlias : AnimationNodeBlendTree
    {

        [Export]
        public AnimationNode[] Nodes = [];


        /*public override double _Process(double time, bool seek, bool isExternalSeeking, bool testOnly)
        {
            var split = this.ResourcePath.Split("/");
            var parentPath = string.Join("/", split.Take(split.Length - 1));
            GD.Print(Get("/."));
            return time;
        }*/

    }

}