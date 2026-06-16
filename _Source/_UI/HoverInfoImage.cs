using Godot;
using System.Drawing;

public partial class HoverInfoImage : TextureButton
{
	public Control _popupBox;

	public override void _Ready()
	{
		MouseEntered += () => SetPopupVisibility(true);
		MouseExited += () => SetPopupVisibility(false);
		GetTree().CurrentScene.AddChild(_popupBox);
	}

	private void SetPopupVisibility(bool visible)
	{
		if (_popupBox == null) return;

		if (visible)
		{
			_popupBox.Visible = true;
			UpdatePopupPosition();
		}
		else
		{
			_popupBox.Visible = false;
		}
	}

	private void UpdatePopupPosition()
	{
		Vector2 screenSize = GetViewportRect().Size;
		Vector2 triggerPos = GlobalPosition;
		Vector2 triggerSize = Size;
		Vector2 popupSize = _popupBox.Size;

		// Default position: Directly to the left of the label
		float x = triggerPos.X - popupSize.X - 5;
		float y = triggerPos.Y;

		// --- Boundary Checks (Clamping) ---

		// If it goes off the bottom, flip it to be ABOVE the label
		if (y + popupSize.Y > screenSize.Y)
		{
			y = triggerPos.Y - popupSize.Y - 5;
		}

		// If it goes off the right edge, shift it left
		if (x + popupSize.X > screenSize.X)
		{
			x = screenSize.X - popupSize.X - 10; // 10px margin from edge
		}

		// Final safety for top/left
		x = Mathf.Max(x, 10);
		y = Mathf.Max(y, 10);

		_popupBox.GlobalPosition = new Vector2(x, y);
	}
}