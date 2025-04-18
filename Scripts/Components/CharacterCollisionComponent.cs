using System.Diagnostics;
using Godot;

namespace BushyCore
{
	public partial class CharacterCollisionComponent : CollisionShape2D
	{
		public RayCast2D CourseCorrectRay1 { get; private set; }
		public RayCast2D CourseCorrectRay2 { get; private set; }
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

		private PlayerController parentController;

		public override void _Ready()
		{
			Shape = RectShape;
		}

		public void SwitchShape(ShapeMode mode)
		{
			SwitchShape((int)mode);
		}
		public void SwitchShape(int modeInt)
		{
			switch (modeInt)
			{
				case (int)ShapeMode.RECTANGULAR:
					Shape = RectShape;
					break;
				case (int)ShapeMode.CILINDER:
					Shape = CilinderShape;
					break;
				case (int)ShapeMode.CIRCLE:
					Shape = CircleShape;
					break;
				case (int)ShapeMode.POINT:
					Shape = PointShape;
					break;
			}

			// I'm leaving this so we NEVER forget that changing the shape of an Area2D's CollisionShape2D
			// triggers an _on_body_exit event. Never forget
			// AreaDetectionComponent.SetShape(this.Shape);
		}

		public void SetParentController(PlayerController val) { parentController = val; }
		public void ToggleHedgeCollision(bool isOn)
		{
			parentController.SetCollisionMaskValue(CollisionLayerConsts.HEDGE, isOn);
		}
	}
}
