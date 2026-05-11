using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public static List<Node> QueryPhysicsCircle(World2D world, Vector2 position, float radius)
		{
			var spaceState = world.DirectSpaceState;
			var query = new PhysicsShapeQueryParameters2D();

			// Create a circular shape for the query
			var circle = new CircleShape2D();
			circle.Radius = radius;

			query.Shape = circle;
			query.Transform = new Transform2D(0, position);
			query.CollisionMask = 2; // Target only the 'Enemies' layer

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
	}
}
