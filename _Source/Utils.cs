using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units
{
	public static class Utils
	{

		public static void ScaleVisualToRadius(Node2D visual, float radius)
		{
			var sprite = visual.GetNode<Sprite2D>("Sprite2D");
			Vector2 textureSize = sprite.Texture.GetSize();

			// Diameter / Texture Width = Scale
			float targetScale = (radius * 2.0f) / textureSize.X;
			visual.Scale = new Vector2(targetScale, targetScale);
		}
	}
}
