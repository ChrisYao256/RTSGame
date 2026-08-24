using Godot;

public partial class Camera : Camera2D
{
	[Export] public float PanSpeed = 1200f; // 800f
	[Export] public int EdgeMargin = 30; // Pixels from the edge
	[Export] private float MaxZoomOut = 2.0f;

	public float _leftBoundary;
	public float _rightBoundary;
	public float _topBoundary;
	public float _bottomBoundary;

	private bool _middleMousePressed;
	public Vector2 _offset;

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
			{
				Zoom += new Vector2(0.1f, 0.1f);
			}
			if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
			{
				Zoom -= new Vector2(0.1f, 0.1f);
			}
			Zoom = Zoom.Clamp(new Vector2(1 / MaxZoomOut, 1 / MaxZoomOut), new Vector2(1.0f, 1.0f));

			if (mouseEvent.ButtonIndex == MouseButton.Middle)
			{
				_middleMousePressed = mouseEvent.Pressed;
			}
		}

		if (_middleMousePressed && @event is InputEventMouseMotion mouseMotion)
		{
			// Dividing by Zoom ensures dragging feels 1:1 at any zoom level
			Position -= mouseMotion.Relative / Zoom;
		}
		Vector2 test = GetViewport().GetVisibleRect().Size;
		GlobalPosition = GlobalPosition.Clamp(new Vector2(_leftBoundary, _topBoundary) + (GetViewport().GetVisibleRect().Size) * 0.5f, new Vector2(_rightBoundary, _bottomBoundary) - (GetViewport().GetVisibleRect().Size) * 0.5f + 2 * _offset);

		if (@event is InputEventKey keyEvent && keyEvent.Keycode == Key.F1)
		{
			CenterCamera();
		}
	}

	public override void _Process(double delta)
	{
		/// uncomment for edge pan. 
		//Vector2 inputDirection = Vector2.Zero;
		//Vector2 mousePos = GetViewport().GetMousePosition();
		//Vector2 viewportSize = GetViewportRect().Size;

		//// Check Left and Right edges
		//if (mousePos.X < EdgeMargin)
		//	inputDirection.X = -1;
		//else if (mousePos.X > viewportSize.X - EdgeMargin)
		//	inputDirection.X = 1;

		//// Check Top and Bottom edges
		//if (mousePos.Y < EdgeMargin)
		//	inputDirection.Y = -1;
		//else if (mousePos.Y > viewportSize.Y - EdgeMargin)
		//	inputDirection.Y = 1;

		//// Move the camera
		//GlobalPosition += inputDirection.Normalized() * PanSpeed * (float)delta;
		
	}

	public void CenterCamera()
	{
		GlobalPosition = new Vector2((_leftBoundary + _rightBoundary) / 2f, (_topBoundary + _bottomBoundary) / 2f) + _offset;
	}
}