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

		public static Array<T> GetRandomElements<[MustBeVariant] T>(Array<T> sourceArray, int n)
		{
			Random random = new Random();
			// 1. Safety checks
			if (sourceArray == null || sourceArray.Count == 0 || n <= 0)
				return new Array<T>();

			if (n >= sourceArray.Count)
				return sourceArray.Duplicate(); // Return a shallow copy of the whole array

			// 2. Create a shallow copy to shuffle so the original list order is preserved
			Array<T> shuffled = sourceArray.Duplicate();

			// 3. Perform a partial Fisher-Yates shuffle up to 'n' items
			for (int i = 0; i < n; i++)
			{
				// Pick a random remaining index from the rest of the array
				int randomIndex = random.Next(i, shuffled.Count);

				// Swap the items
				T temp = shuffled[i];
				shuffled[i] = shuffled[randomIndex];
				shuffled[randomIndex] = temp;
			}

			// 4. Extract the first 'n' shuffled items into our final Godot Array
			Array<T> result = new Array<T>();
			for (int i = 0; i < n; i++)
			{
				result.Add(shuffled[i]);
			}

			return result;
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

		public static int VectorSum(Vector4I a)
		{
			return a[0] + a[1] + a[2] + a[3];
		}

		public static Vector4I VectorScalarMultiplication(Vector4I a, float b)
		{
			for (int i = 0; i < 4; i++)
			{
				a[i] = (int)Math.Ceiling(a[i] * b);
			}
			return a;
		}

		public static float VectorDivision(Vector4I top, Vector4I bot)
		{
			if (bot == new Vector4I(0, 0, 0, 0))
			{
				return 1f;
			}
			for (int i =0; i < 4; i++)
			{
				top[i] = Math.Clamp(top[i], 0, bot[i]);
			}
			return (float)VectorSum(top) / (float)VectorSum(bot);
		}

		public static Vector4I VectorMaxClamp(Vector4I a, Vector4I b)
		{
			for (int i = 0; i < 4; i++)
			{
				a[i] = Math.Clamp(a[i], 0, b[i]);
			}
			return a;
		}

		public static string MakeMoneyText(Vector4I money, bool displayAll = false)
		{
			string yellowHex = ThemePalette.Yellow.ToHtml(false);
			string redHex = ThemePalette.Red.ToHtml(false);
			string blueHex = ThemePalette.Blue.ToHtml(false);
			string greenHex = ThemePalette.Green.ToHtml(false);
			string output = "";
			if (displayAll)
			{
				output += $"[color=#{yellowHex}][url={StringDB.Entries["YellowMoneyDescription"]}][img=18x18]res://_Assets/Electricity.png[/img]{money[0]}[/url][/color]  ";
				output += $"[color=#{redHex}][url={StringDB.Entries["RedMoneyDescription"]}][img=18x18]res://_Assets/Steel.png[/img]{money[1]}[/url][/color]  ";
				output += $"[color=#{blueHex}][url={StringDB.Entries["BlueMoneyDescription"]}][img=18x18]res://_Assets/Water.png[/img]{money[2]}[/url][/color]  ";
				output += $"[color=#{greenHex}][url={StringDB.Entries["GreenMoneyDescription"]}][img=18x18]res://_Assets/Gas.png[/img]{money[3]}[/url][/color]  ";
			}
			else
			{
				if (money[0] != 0)
				{
					output += $"[color=#{yellowHex}][img=18x18]res://_Assets/Electricity.png[/img]{money[0]}[/color]  ";
				}

				if (money[1] != 0 || displayAll)
				{
					output += $"[color=#{redHex}][img=18x18]res://_Assets/Steel.png[/img]{money[1]}[/color]  ";
				}
				if (money[2] != 0 || displayAll)
				{
					output += $"[color=#{blueHex}][img=18x18]res://_Assets/Water.png[/img]{money[2]}[/color]  ";
				}
				if (money[3] != 0 || displayAll)
				{
					output += $"[color=#{greenHex}][img=18x18]res://_Assets/Gas.png[/img] {money[3]} [/color]  ";
				}
			}

			return output.Trim();
		}
	}
}
