// TooltipManager.cs - autoload singleton
using Godot;
using System.Reflection.Emit;

namespace RTSGame.Units;

public partial class TooltipManager : CanvasLayer
{
	private PanelContainer _panel;

	public override void _Ready()
	{
	}

	public void ShowTooltip((string, string) texts)
	{
		_panel = GetTooltipContainer(texts);
		AddChild(_panel);
	}

	public static PanelContainer GetTooltipContainer((string, string) texts)
	{
		PanelContainer panel = new PanelContainer();
		panel.TopLevel = true;
		var hbox = new HBoxContainer();

		TooltipRichTextLabel label = new TooltipRichTextLabel { BbcodeEnabled = true, FitContent = true };
		TooltipRichTextLabel topRightRTL = new TooltipRichTextLabel { BbcodeEnabled = true, FitContent = true };
		label.CustomMinimumSize = new Vector2(200, 0);
		topRightRTL.CustomMinimumSize = new Vector2(50, 0);

		hbox.AddChild(label);
		hbox.AddChild(topRightRTL);
		panel.AddChild(hbox);
		label.Text = texts.Item1;
		topRightRTL.Text = texts.Item2;
		panel.ResetSize();

		return panel;
	}


	public void HideTooltip()
	{
		_panel.QueueFree();
	}

	public override void _Process(double delta)
	{
		if (_panel != null && IsInstanceValid(_panel) && _panel.Visible)
		{
			Vector2 mousePos = GetViewport().GetMousePosition();
			Vector2 offset = new Vector2(12, 12);
			Vector2 panelSize = _panel.Size;
			Vector2 screenSize = GetViewport().GetVisibleRect().Size;

			Vector2 pos = mousePos + offset;
			pos.X = Mathf.Min(pos.X, screenSize.X - panelSize.X);
			pos.Y = Mathf.Min(pos.Y, screenSize.Y - panelSize.Y);

			_panel.GlobalPosition = pos;
		}
	}
}