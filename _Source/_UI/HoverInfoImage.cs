//using Godot;
//using System.Drawing;

//public partial class HoverInfoImage : TextureButton
//{
//	public Control _popupBox;

//	public override void _Ready()
//	{
//		MouseEntered += () => SetPopupVisibility(true);
//		MouseExited += () => SetPopupVisibility(false);
//		AddChild(_popupBox);
//	}

//	private void SetPopupVisibility(bool visible)
//	{
//		if (_popupBox == null) return;

//		if (visible)
//		{
//			_popupBox.Visible = true;
//			UpdatePopupPosition();
//		}
//		else
//		{
//			_popupBox.Visible = false;
//		}
//	}

//	private void UpdatePopupPosition()
//	{
//		Vector2 screenSize = GetViewportRect().Size;
//		Vector2 triggerPos = GlobalPosition;
//		Vector2 triggerSize = Size;
//		Vector2 popupSize = _popupBox.Size;

//		// Default position: Directly to the left of the label
//		float x = triggerPos.X - popupSize.X - 5;
//		float y = triggerPos.Y;

//		// --- Boundary Checks (Clamping) ---

//		// If it goes off the bottom, flip it to be ABOVE the label
//		if (y + popupSize.Y > screenSize.Y)
//		{
//			y = triggerPos.Y - popupSize.Y - 5;
//		}

//		// If it goes off the right edge, shift it left
//		if (x + popupSize.X > screenSize.X)
//		{
//			x = screenSize.X - popupSize.X - 10; // 10px margin from edge
//		}

//		// Final safety for top/left
//		x = Mathf.Max(x, 10);
//		y = Mathf.Max(y, 10);

//		_popupBox.GlobalPosition = new Vector2(x, y);
//	}
//}



using Godot;

// Remove using System.Drawing; — Godot uses its own Vector2/Color types, this namespace is irrelevant
public partial class HoverInfoImage : TextureButton
{
	public Control _popupBox; // Use Export so assign in inspector, rename _popupBox → PopupBox for convention
	public float HoverCloseDelay = 0.05f; // Delay before closing after mouse leaves both elements

	private Timer _closeTimer;

	private bool _mouseOnPopup = false;

	public override void _Ready()
	{
		// Create & configure auto-close timer
		_closeTimer = new Timer();
		_closeTimer.WaitTime = HoverCloseDelay;
		_closeTimer.Autostart = false;
		_closeTimer.OneShot = true;
		AddChild(_closeTimer);
		_closeTimer.Timeout += HidePopupOnTimeout;

		// Bind mouse events for THIS TextureButton
		//MouseEntered += OnTriggerMouseEnter;
		MouseExited += OnTriggerMouseExit;

		// Bind mouse events for PopupBox if it exists
		if (_popupBox != null)
		{
			BindAllPopupChildMouseSignals(_popupBox);

			// Add popup as child (keep your original logic)
			AddChild(_popupBox);
			_popupBox.Visible = false; // Start hidden
		}
		else
		{
			throw new System.Exception("No _popupBox found!");
		}
	}

	public override void _GuiInput(InputEvent @event)
	{
		base._GuiInput(@event);

		// Check if it's a mouse button press
		if (@event is InputEventMouseButton mouseEvent)
		{
			// MouseButton.Right = right click, Pressed = true means click down
			if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
			{
				OnRightClicked();
				// Optional: Consume input so it doesn't pass to underlying UI
				AcceptEvent();
			}
		}
	}

	private void OnRightClicked()
	{
		if (!_popupBox.Visible)
		{
			KeepPopupOpen();
		}
		else
		{
			SchedulePopupClose();
		}
	}

	private void BindAllPopupChildMouseSignals(Node node)
	{
		if (node is Control ctrl)
		{
			ctrl.MouseEntered += OnPopupMouseEnter;
			ctrl.MouseExited += OnPopupMouseExit;
		}
		foreach (Node child in node.GetChildren())
		{
			BindAllPopupChildMouseSignals(child);
		}
	}

	// Shared logic: Keep popup open, cancel close timer
	private void KeepPopupOpen()
	{
		if (_popupBox == null) return;

		_closeTimer.Stop(); // Stop pending close
		if (!_popupBox.Visible)
		{
			_popupBox.Visible = true;
			UpdatePopupPosition();
		}
	}

	// Shared logic: Start timer to close popup after delay
	private void SchedulePopupClose()
	{
		if (_popupBox == null || _popupBox.Visible == false) return;
		_closeTimer.Start();
	}

	// Timer callback: Finally hide popup when delay ends
	private void HidePopupOnTimeout()
	{
		if (_popupBox != null)
			_popupBox.Visible = false;
	}

	// Mouse enters TextureButton trigger
	private void OnTriggerMouseEnter()
	{
		KeepPopupOpen();
	}

	// Mouse leaves TextureButton trigger
	private void OnTriggerMouseExit()
	{
		if (!_mouseOnPopup)
		{
			SchedulePopupClose();
		}
	}

	// Mouse enters Popup panel
	private void OnPopupMouseEnter()
	{
		_mouseOnPopup = true;
		KeepPopupOpen();
	}

	// Mouse leaves Popup panel
	private void OnPopupMouseExit()
	{
		_mouseOnPopup = false;
		SchedulePopupClose();
	}

	private void UpdatePopupPosition()
	{
		Vector2 screenSize = GetViewportRect().Size;
		Vector2 triggerPos = GlobalPosition;
		Vector2 triggerSize = Size;
		Vector2 popupSize = _popupBox.Size;

		// Default position: Left side of button with 5px gap
		float x = triggerPos.X;
		float y = triggerPos.Y - popupSize.Y - 5;

		// If popup goes off top screen → place below trigger
		if (y < 0)
		{
			y = triggerPos.Y + triggerSize.Y + 5;
		}

		// If popup goes off right screen → clamp to right margin
		if (x + popupSize.X > screenSize.X)
		{
			x = screenSize.X - popupSize.X - 10;
		}

		// Left/top safety margin
		x = Mathf.Max(x, 10);
		y = Mathf.Max(y, 10);

		_popupBox.GlobalPosition = new Vector2(x, y);
	}
}