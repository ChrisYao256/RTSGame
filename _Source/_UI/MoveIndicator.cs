using Godot;

public partial class MoveIndicator : Node2D
{
	public Vector2 StartPos { get; set; }
	public Vector2 EndPos { get; set; }
	public Color LineColor { get; set; } = ThemePalette.Blue;
	public float ArrowHeadLength { get; set; } = 16.0f;
	public float ArrowHeadAngle { get; set; } = Mathf.DegToRad(30);

	public override void _Draw()
	{
		if (StartPos.DistanceSquaredTo(EndPos) < 16.0f)
			return; // Don't draw if too close to the origin

		Vector2 direction = (StartPos - EndPos).Normalized();

		// Calculate arrowhead vertices
		Vector2 arrowPoint1 = EndPos + direction.Rotated(ArrowHeadAngle) * ArrowHeadLength;
		Vector2 arrowPoint2 = EndPos + direction.Rotated(-ArrowHeadAngle) * ArrowHeadLength;

		// Calculate the base center of the arrowhead triangle
		float baseOffset = ArrowHeadLength * Mathf.Cos(ArrowHeadAngle);
		Vector2 lineEndPos = EndPos + direction * baseOffset;

		// Draw line stopping cleanly at the base of the arrowhead
		DrawLine(StartPos, lineEndPos, LineColor, 3.0f, false);

		// Draw arrowhead polygon
		Vector2[] arrowPoints = new Vector2[] { EndPos, arrowPoint1, arrowPoint2 };
		DrawColoredPolygon(arrowPoints, LineColor);
	}

	public void UpdatePositions(Vector2 start, Vector2 end)
	{
		StartPos = start;
		EndPos = end;
		QueueRedraw();
	}
}