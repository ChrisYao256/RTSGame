using Godot;

public partial class Camera : Camera2D
{
	[Export] public float PanSpeed = 0.0f; // 800f
	[Export] public int EdgeMargin = 40; // Pixels from the edge

	//public override void _UnhandledInput(InputEvent @event)
	//{
	//	if (@event is InputEventMouseButton mouseEvent)
	//	{
	//		if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
	//			Zoom += new Vector2(0.1f, 0.1f);
	//		if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
	//			Zoom -= new Vector2(0.1f, 0.1f);

	//		// Clamp zoom so you don't go inside the ground or too far out
	//		Zoom = Zoom.Clamp(new Vector2(0.5f, 0.5f), new Vector2(2.0f, 2.0f));
	//	}
	//}

	//public override void _Process(double delta)
	//{
	//	Vector2 inputDirection = Vector2.Zero;
	//	Vector2 mousePos = GetViewport().GetMousePosition();
	//	Vector2 viewportSize = GetViewportRect().Size;

	//	// Check Left and Right edges
	//	if (mousePos.X < EdgeMargin)
	//		inputDirection.X = -1;
	//	else if (mousePos.X > viewportSize.X - EdgeMargin)
	//		inputDirection.X = 1;

	//	// Check Top and Bottom edges
	//	if (mousePos.Y < EdgeMargin)
	//		inputDirection.Y = -1;
	//	else if (mousePos.Y > viewportSize.Y - EdgeMargin)
	//		inputDirection.Y = 1;

	//	// Move the camera
	//	GlobalPosition += inputDirection.Normalized() * PanSpeed * (float)delta;
	//}
}