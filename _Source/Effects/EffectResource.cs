using Godot;
using Godot.Collections;
using System.Xml.Linq;
namespace RTSGame.Units;

[GlobalClass]
public abstract partial class EffectResource: Resource
{
	[Export]
	public string _effectName = "";

	[Export]
	public Texture2D _effectIcon;

	public string _effectDescription = "";

	public abstract Effect CreateNode();

	// Defines behavior when an effect is added to a unit that already has unit. If this returns true, then the new effect is added. Otherwise it is not. 
	public virtual bool MergeWithOld(EffectResource oldResource)
	{
		return true;
	}

	public abstract void SetDescription();

	public virtual HoverInfoLabel MakeEffectTooltip(bool clickable)
	{
		HoverInfoLabel trigger = new HoverInfoLabel();
		trigger.Text = _effectName;

		// Buttons use "Flat" mode if you want them to look like your old labels
		// or you can leave it off for a standard button look
		trigger.Flat = false;

		// Create the solid background for the Button
		StyleBoxFlat btnStyle = new StyleBoxFlat();
		if (!clickable)
		{
			btnStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 1.0f); // gray
		}
		else
		{
			btnStyle.BgColor = new Color(0.2f, 0.5f, 0.2f, 1.0f); // green
		}
			btnStyle.SetContentMarginAll(6);
		btnStyle.CornerRadiusTopLeft = 3;
		btnStyle.CornerRadiusBottomLeft = 3;
		btnStyle.CornerRadiusTopRight = 3;
		btnStyle.CornerRadiusBottomRight = 3;

		// Apply to multiple states so it stays solid when clicked
		trigger.AddThemeStyleboxOverride("normal", btnStyle);
		trigger.AddThemeStyleboxOverride("hover", btnStyle);
		trigger.AddThemeStyleboxOverride("pressed", btnStyle);

		// 2. Create the Popup Box (PanelContainer for the background)
		PanelContainer popup = new PanelContainer();
		popup.ZIndex = 100;
		popup.TopLevel = true; // Essential to avoid parent clipping
		popup.Visible = false;

		// 3. Style the background (Solid Color)
		StyleBoxFlat style = new StyleBoxFlat();
		style.BgColor = new Color(0.2f, 0.2f, 0.2f, 1.0f); // Dark Grey
		style.SetContentMarginAll(5); // Padding around text
		popup.AddThemeStyleboxOverride("panel", style);

		// 4. Create the Description Label
		Label desc = new Label();
		desc.Text = _effectDescription.Trim(); // Assumes your resource has a field named _description
		desc.CustomMinimumSize = new Vector2(200, 0); // Limit width

		popup.AddChild(desc);
		trigger.AddChild(popup); // Attach popup to trigger for organization
		trigger._popupBox = popup;

		return trigger;
	}

	public static HoverInfoLabel MakeCombinedEffectTooltip(bool clickable, string name, Array<EffectResource> effects)
	{
		HoverInfoLabel trigger = new HoverInfoLabel();
		trigger.Text = name;

		// Buttons use "Flat" mode if you want them to look like your old labels
		// or you can leave it off for a standard button look
		trigger.Flat = false;

		// Create the solid background for the Button
		StyleBoxFlat btnStyle = new StyleBoxFlat();
		if (!clickable)
		{
			btnStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 1.0f); // gray
		}
		else
		{
			btnStyle.BgColor = new Color(0.2f, 0.5f, 0.2f, 1.0f); // green
		}
		btnStyle.SetContentMarginAll(6);
		btnStyle.CornerRadiusTopLeft = 3;
		btnStyle.CornerRadiusBottomLeft = 3;
		btnStyle.CornerRadiusTopRight = 3;
		btnStyle.CornerRadiusBottomRight = 3;

		// Apply to multiple states so it stays solid when clicked
		trigger.AddThemeStyleboxOverride("normal", btnStyle);
		trigger.AddThemeStyleboxOverride("hover", btnStyle);
		trigger.AddThemeStyleboxOverride("pressed", btnStyle);

		// 2. Create the Popup Box (PanelContainer for the background)
		PanelContainer popup = new PanelContainer();
		popup.ZIndex = 100;
		popup.TopLevel = true; // Essential to avoid parent clipping
		popup.Visible = false;

		// 3. Style the background (Solid Color)
		StyleBoxFlat style = new StyleBoxFlat();
		style.BgColor = new Color(0.2f, 0.2f, 0.2f, 1.0f); // Dark Grey
		style.SetContentMarginAll(5); // Padding around text
		popup.AddThemeStyleboxOverride("panel", style);

		// 4. Create the Description Label
		Label desc = new Label();
		foreach (EffectResource effect in effects)
		{
			desc.Text += effect._effectDescription.Trim();
			desc.Text += "\n";
		}
		desc.Text = desc.Text.Trim();
		desc.CustomMinimumSize = new Vector2(200, 0); // Limit width

		popup.AddChild(desc);
		trigger.AddChild(popup); // Attach popup to trigger for organization
		trigger._popupBox = popup;

		return trigger;
	}
}

