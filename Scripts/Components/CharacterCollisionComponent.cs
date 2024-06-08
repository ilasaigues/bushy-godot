using Godot;

namespace BushyCore
{
	public partial class CharacterCollisionComponent : CollisionShape2D
	{
		public enum ShapeMode 
		{
			RECTANGULAR,
			CILINDER,
			CIRCLE,
			POINT
		}

		[Export]
		public Shape2D RectShape;
		[Export]
		public Shape2D CilinderShape;
		[Export]
		public Shape2D CircleShape;
		[Export]
		public Shape2D PointShape;

		public override void _Ready()
		{
			this.Shape = RectShape;
		}

        public override void _Process(double delta)
        {
			
        }

        public void SwitchShape(ShapeMode mode)
		{
			switch (mode)
			{
				case ShapeMode.RECTANGULAR:
					this.Shape = RectShape;
					break;
				case ShapeMode.CILINDER:
					this.Shape = CilinderShape;
					break;
				case ShapeMode.CIRCLE:
					this.Shape = CircleShape;
					break;
				case ShapeMode.POINT:
					this.Shape = PointShape;
					break;
			}
		}
	}
}
