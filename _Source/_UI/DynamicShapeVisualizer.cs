using Godot;

public partial class DynamicShapeVisualizer : Node2D
{
	private Shape2D _shapeToDraw;
	private Color _fillColor = new Color(1, 0, 0, 0.3f);   // Semi-transparent red
	private Color _lineColor = new Color(1, 0, 0, 1.0f);   // Solid red outline
	private float _lineWidth = 2.0f;

	/// <summary>
	/// Pass any Shape2D here right after instantiating this scene.
	/// </summary>
	public void SetShape(Shape2D shape)
	{
		_shapeToDraw = shape;
		QueueRedraw(); // Forces Godot to execute the _Draw() loop
	}

	public override void _Draw()
	{
		if (_shapeToDraw == null) return;

		// Pattern match the generic Shape2D into its specific subclass
		switch (_shapeToDraw)
		{
			case CircleShape2D circle:
				DrawCircle(Vector2.Zero, circle.Radius, _fillColor);
				DrawArc(Vector2.Zero, circle.Radius, 0, Mathf.Tau, 32, _lineColor, _lineWidth);
				break;

			case RectangleShape2D rect:
				// Shape2D rectangles are drawn from the center outward
				Rect2 drawRect = new Rect2(-rect.Size / 2, rect.Size);
				DrawRect(drawRect, _fillColor, filled: true);
				DrawRect(drawRect, _lineColor, filled: false, width: _lineWidth);
				break;

			case CapsuleShape2D capsule:
				DrawCapsuleShape(capsule);
				break;

			case ConvexPolygonShape2D convexPoly:
				DrawPolygon(convexPoly.Points, new Color[] { _fillColor });
				// Draw outline by connecting the points
				for (int i = 0; i < convexPoly.Points.Length; i++)
				{
					Vector2 p1 = convexPoly.Points[i];
					Vector2 p2 = convexPoly.Points[(i + 1) % convexPoly.Points.Length];
					DrawLine(p1, p2, _lineColor, _lineWidth);
				}
				break;

			case ConcavePolygonShape2D concavePoly:
				// Concave polygons are stored as pairs of line segments
				var segments = concavePoly.Segments;
				for (int i = 0; i < segments.Length; i += 2)
				{
					DrawLine(segments[i], segments[i + 1], _lineColor, _lineWidth);
				}
				break;

			default:
				GD.PrintErr($"Shape type '{_shapeToDraw.GetType().Name}' is not supported by the visualizer.");
				break;
		}
	}

	// Capsule shapes don't have a direct "DrawCapsule" method, so we draw it manually
	private void DrawCapsuleShape(CapsuleShape2D capsule)
	{
		// Godot capsules stand vertically by default. 
		// Height is total end-to-end length; radius is the width of the caps.
		float cylinderHeight = capsule.Height - (capsule.Radius * 2);
		float halfHeight = cylinderHeight / 2;

		// Draw center rectangle body
		Rect2 centerRect = new Rect2(new Vector2(-capsule.Radius, -halfHeight), new Vector2(capsule.Radius * 2, cylinderHeight));
		DrawRect(centerRect, _fillColor, filled: true);

		// Draw top and bottom hemisphere caps
		DrawCircle(new Vector2(0, -halfHeight), capsule.Radius, _fillColor);
		DrawCircle(new Vector2(0, halfHeight), capsule.Radius, _fillColor);

		// Draw outer outlines
		DrawLine(new Vector2(-capsule.Radius, -halfHeight), new Vector2(-capsule.Radius, halfHeight), _lineColor, _lineWidth);
		DrawLine(new Vector2(capsule.Radius, -halfHeight), new Vector2(capsule.Radius, halfHeight), _lineColor, _lineWidth);
		DrawArc(new Vector2(0, -halfHeight), capsule.Radius, Mathf.Pi, Mathf.Tau, 16, _lineColor, _lineWidth);
		DrawArc(new Vector2(0, halfHeight), capsule.Radius, 0, Mathf.Pi, 16, _lineColor, _lineWidth);
	}
}