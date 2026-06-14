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

		public static void ScaleVisualToRadius(AnimatedSprite2D visual, float radius)
		{
			//if(visual.SpriteFrames.GetFrameCount() == 0) return;
			// Get the texture from the current animation frame
			Texture2D currentTexture = visual.SpriteFrames.GetFrameTexture(visual.Animation, visual.Frame);
			Vector2 textureSize = currentTexture.GetSize();

			// Diameter / Texture Width = Scale
			float targetScaleX = (radius * 2.0f) / textureSize.X;
			float targetScaleY = (radius * 2.0f) / textureSize.Y;
			visual.Scale = new Vector2(targetScaleX, targetScaleY);
		}

		public static void ScaleVisualToRadius(TextureRect visual, float radius)
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

		public static bool VectorLeq(Vector4I a, Vector4I b)
		{
			if (a[0] > b[0])
			{
				return false;
			}
			if (a[1] > b[1])
			{
				return false;
			}
			if (a[2] > b[2])
			{
				return false;
			}
			if (a[3] > b[3])
			{
				return false;
			}
			return true;
		}

		public static bool VectorLNeq(Vector4I a, Vector4I b)
		{
			if (a[0] >= b[0])
			{
				return false;
			}
			if (a[1] >= b[1])
			{
				return false;
			}
			if (a[2] >= b[2])
			{
				return false;
			}
			if (a[3] >= b[3])
			{
				return false;
			}
			return true;
		}

		public static string MakeMoneyText(Vector4I money, bool displayAll = false)
		{
			string yellowHex = ThemePalette.Yellow.ToHtml(false);
			string redHex = ThemePalette.Red.ToHtml(false);
			string blueHex = ThemePalette.Blue.ToHtml(false);
			string greenHex = ThemePalette.Green.ToHtml(false);
			string output = "";
			if (money[0]!= 0 || displayAll)
			{
				output += $"[color=#{yellowHex}]${money[0]}[/color]  ";
			}
			if (money[1] != 0 || displayAll)
			{
				output += $"[color=#{redHex}]■{money[1]}[/color]  ";
			}
			if (money[2] != 0 || displayAll)
			{
				output += $"[color=#{blueHex}]●{money[2]}[/color]  ";
			}
			if (money[3] != 0 || displayAll)
			{
				output += $"[color=#{greenHex}]▲{money[3]}[/color]  ";
			}
			return output.Trim();
		}
	}
}
