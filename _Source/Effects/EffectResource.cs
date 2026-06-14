using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
namespace RTSGame.Units;

[GlobalClass]
public abstract partial class EffectResource: Resource
{
	public enum DisplayTypes
	{
		Hidden,
		Large,
		Small,
	}

	[Export]
	public string _effectName = "";

	[Export]
	public Texture2D _effectIcon;

	[Export]
	public DisplayTypes _displayType;

	[Export]
	public string _effectDescription = "";

	public abstract Effect CreateNode();

	// Defines behavior when an effect is added to a unit that already has unit. If this returns true, then the new effect is added. Otherwise it is not. 
	public virtual bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
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
		RichTextLabel desc = new RichTextLabel();
		desc.Text = _effectDescription.Trim(); // Assumes your resource has a field named _description
		desc.CustomMinimumSize = new Vector2(200, 0); // Limit width
		desc.FitContent = true;
		desc.BbcodeEnabled = true;
		popup.AddChild(desc);

		trigger.AddChild(popup); // Attach popup to trigger for organization
		trigger._popupBox = popup;

		return trigger;
	}

	public static HoverInfoLabel MakeCombinedEffectTooltip(bool clickable, string name, Array<EffectResource> effects)
	{
		HoverInfoLabel trigger = new HoverInfoLabel();
		trigger.Text = name;

		trigger.Flat = false;

		// 2. Create the Popup Box (PanelContainer for the background)
		PanelContainer popup = new PanelContainer();
		popup.ZIndex = 100;
		popup.TopLevel = true; // Essential to avoid parent clipping
		popup.Visible = false;

		// 4. Create the Description Label
		RichTextLabel desc = new RichTextLabel();
		foreach (EffectResource effect in effects)
		{
			desc.Text += effect._effectDescription.Trim();
			desc.Text += "\n";
		}
		desc.Text = desc.Text.Trim();
		desc.CustomMinimumSize = new Vector2(200, 0); // Limit width
		desc.FitContent = true;
		desc.BbcodeEnabled = true;
		popup.AddChild(desc);

		trigger.AddChild(popup); // Attach popup to trigger for organization
		trigger._popupBox = popup;

		return trigger;
	}

	public virtual PanelContainer MakeFullEffectDescription()
	{
		PanelContainer panelContainer = new();

		VBoxContainer container = new VBoxContainer();

		Label name = new();
		name.Text = _effectName;
		container.AddChild(name);

		RichTextLabel description = new();
		description.BbcodeEnabled = true;
		description.Text = _effectDescription;
		description.CustomMinimumSize = new(200, 0);
		description.FitContent = true;
		container.AddChild(description);

		panelContainer.AddChild(container);

		return panelContainer;
	}
}

