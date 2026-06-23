using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using static Godot.Control;
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

	[Export]
	public string _effectTopRightString = "";

	[Export]
	public PackedScene _floatingTextScene;

	public UpgradeButton _tempDebuffIcon;

	public abstract Effect CreateNode();

	// Defines behavior when an effect is added to a unit that already has unit. If this returns true, then the new effect is added. Otherwise it is not. 
	public virtual bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		return true;
	}

	public abstract void SetDescription();

	public virtual HoverInfoLabel MakeEffectTooltip(bool clickable)
	{
		UpgradeButton trigger = new UpgradeButton();
		trigger.Text = _effectName;

		trigger.Flat = false;

		PanelContainer popup = TooltipManager.GetTooltipContainer((_effectDescription, _effectTopRightString));
		trigger._popupBox = popup;
		popup.Visible = false;
		trigger.AddChild(popup);

		_tempDebuffIcon = trigger;

		trigger.ResetSize();

		return trigger;
	}

	public static HoverInfoLabel MakeCombinedEffectTooltip(bool clickable, string name, Array<EffectResource> effects, HoverInfoLabel trigger = null)
	{
		if (trigger == null)
		{
			trigger = new HoverInfoLabel();
		}
		trigger.Text = name;

		trigger.Flat = false;

		string desc = "";
		foreach (EffectResource effect in effects)
		{
			desc += effect._effectDescription.Trim();
			desc += "\n";
		}
		PanelContainer popup = TooltipManager.GetTooltipContainer((desc, ""));
		trigger._popupBox = popup;
		popup.Visible = false;
		trigger.AddChild(popup);
		trigger.ResetSize();
		return trigger;
	}

	public virtual PanelContainer MakeFullEffectDescription()
	{
		PanelContainer panelContainer = new();
		VBoxContainer container = new VBoxContainer();

		//Label name = new();
		//name.Text = _effectName;
		//container.AddChild(name);

		TooltipRichTextLabel description = new();
		description.BbcodeEnabled = true;
		description.Text = _effectDescription;
		description.CustomMinimumSize = new(200, 0);
		description.FitContent = true;
		description.HintUnderlined = true;
		container.AddChild(description);
		panelContainer.AddChild(container);

		return panelContainer;
	}
}

