using Godot;
using System.Drawing;

public partial class HoverInfoLabel : Button
{
	public Control PopupBox;

	public override void _Ready()
	{
		MouseEntered += () => SetPopupVisibility(true);
		MouseExited += () => SetPopupVisibility(false);
	}

	private void SetPopupVisibility(bool visible)
	{
		if (PopupBox == null) return;

		if (visible)
		{
			PopupBox.Visible = true;
			UpdatePopupPosition();
		}
		else
		{
			PopupBox.Visible = false;
		}
	}

	private void UpdatePopupPosition()
	{
		Vector2 screenSize = GetViewportRect().Size;
		Vector2 triggerPos = GlobalPosition;
		Vector2 triggerSize = Size;
		Vector2 popupSize = PopupBox.Size;

		// Default position: Directly below the label
		float x = triggerPos.X;
		float y = triggerPos.Y + triggerSize.Y + 5; // 5px gap

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

		PopupBox.GlobalPosition = new Vector2(x, y);
	}
}