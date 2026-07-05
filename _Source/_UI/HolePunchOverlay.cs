using Godot;

public partial class HolePunchOverlay : Control
{
	private Rect2? _cutoutRect = null;
	[Export] private Color _overlayColor = new Color(0, 0, 0, 0.6f); // Dark black with alpha

	/// <summary>
	/// Sets the screen area that should remain perfectly clear.
	/// Pass null to make the entire screen dark again.
	/// </summary>
	public void SetHighlightArea(Rect2? globalRect)
	{
		if (globalRect != null)
		{
			// Convert global screen coordinates to local coordinates for this control
			Vector2 localPos = MakeCanvasPositionLocal(globalRect.Value.Position);
			_cutoutRect = new Rect2(localPos, globalRect.Value.Size);
		}
		else
		{
			_cutoutRect = null;
		}

		// Tell Godot to redraw the control
		QueueRedraw();
	}

	public override void _Draw()
	{
		// If there is no active highlight, paint the whole screen dark
		if (!_cutoutRect.HasValue)
		{
			DrawRect(new Rect2(Vector2.Zero, Size), _overlayColor);
			return;
		}

		Rect2 hole = _cutoutRect.Value;

		// Draw 4 distinct rectangles around the hole to completely cover the screen
		// 1. Top block
		DrawRect(new Rect2(0, 0, Size.X, hole.Position.Y), _overlayColor);
		// 2. Bottom block
		DrawRect(new Rect2(0, hole.End.Y, Size.X, Size.Y - hole.End.Y), _overlayColor);
		// 3. Left block (between top and bottom)
		DrawRect(new Rect2(0, hole.Position.Y, hole.Position.X, hole.Size.Y), _overlayColor);
		// 4. Right block (between top and bottom)
		DrawRect(new Rect2(hole.End.X, hole.Position.Y, Size.X - hole.End.X, hole.Size.Y), _overlayColor);
	}
}