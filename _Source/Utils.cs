using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

namespace RTSGame.Units
{
	public static class Utils
	{
		public static void ScaleVisualToRadius(Sprite2D visual, float radius)
		{
			Vector2 textureSize = visual.Texture.GetSize();

			// Diameter / Texture Width = Scale
			float targetScaleX = (radius * 2.0f) / textureSize.X;
			float targetScaleY = (radius * 2.0f) / textureSize.Y;
			visual.Scale = new Vector2(targetScaleX, targetScaleY);
		}

		public static void ScaleTextureButtonToRadius(TextureButton button, float radius)
		{
			float diameter = radius * 2.0f;

			// 1. Force the button's layout boundaries to be the target diameter
			button.CustomMinimumSize = new Vector2(diameter, diameter);
			button.Size = new Vector2(diameter, diameter); // Force immediate update if not in a container

			// 2. Configure the button's texture settings so it actually stretches to match
			button.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
			button.IgnoreTextureSize = true;
		}



		public static List<Node> QueryPhysicsCircle(World2D world, Vector2 position, float radius, uint collisionMask = 2)
		{
			var spaceState = world.DirectSpaceState;
			var query = new PhysicsShapeQueryParameters2D();

			// Create a circular shape for the query
			var circle = new CircleShape2D();
			circle.Radius = radius;

			query.Shape = circle;
			query.Transform = new Transform2D(0, position);
			query.CollisionMask = collisionMask; // Target only the 'Enemies' layer

			var results = spaceState.IntersectShape(query);
			List<Node> nodes = new List<Node>();
			foreach (var result in results)
			{
				var collider = result["collider"].As<Node2D>();
				if (collider is Node node)
				{
					nodes.Add(node);
				}
			}
			return nodes;
		}

		public static int Mod(int dividend, int divisor)
		{
			// Guard against division by zero
			if (divisor == 0)
				throw new DivideByZeroException();

			int remainder = dividend % divisor;
			// Make sure result is positive
			return remainder < 0 ? remainder + divisor : remainder;
		}
	}
}
