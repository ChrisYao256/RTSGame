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

	public Unit _source;

	public Effect _effect;

	public abstract Effect CreateNode();

	/// <summary>
	/// Defines the behavior when multiple EffectResources of the same type are added to the same unit (or added to the same list, e.g. in the case of DebuffOnHit)
	/// </summary>
	/// <param name="oldResource">The first matching resource in the existing list. </param>
	/// <param name="allMatchingResource">All matching resources in the existing list. This list becomes useful if this method returns true and false depending on the EffectResource, giving fine control over which EffectResources to merge.</param>
	/// <returns></returns>
	public virtual bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		return true;
	}

	public abstract void SetDescription();

	/// <summary>
	/// Called for effectResources in tower upgrades. Override this for every effectResource that could go in a tower upgrade.
	/// </summary>
	public virtual void SetUpgradeDescription()
	{
		SetDescription();
	}

	/// <summary>
	/// Makes an UpgradeButton whose popup is created using TooltipManager.GetTooltipContainer((_effectDescription, _effectTopRightString));
	/// </summary>
	/// <param name="clickable"></param>
	/// <returns></returns>
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

	/// <summary>
	/// Makes an UpgradeButton whose popup is created using TooltipManager.GetTooltipContainer() with the combined descriptions of the given effects. TopRightString not supported. 
	/// </summary>
	/// <param name="clickable"></param>
	/// <returns></returns>
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

	/// <summary>
	/// Makes a panelcontainer containing a vboxcontainer containing a rtl. Text is effectDescription. TopRightString not supported. 
	/// </summary>
	/// <returns></returns>
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
		description.CustomMinimumSize = new(250, 0);
		description.FitContent = true;
		description.HintUnderlined = true;
		container.AddChild(description);
		panelContainer.AddChild(container);

		return panelContainer;
	}

	public virtual PanelContainer MakeFullEffectDescriptionWithUpgrade(EffectResource upgrade)
	{
		if (upgrade.GetType() != GetType())
		{
			throw new System.Exception("Upgrade effectresource does not match the origional type.");
		}
		PanelContainer panelContainer = new();
		VBoxContainer container = new VBoxContainer();

		EffectResource copy = (EffectResource)DuplicateDeep();
		EffectResource newCopy = (EffectResource)upgrade.DuplicateDeep();
		newCopy.MergeWithOld(copy, [copy]);
		copy.SetDescription();

		string greenHex = ThemePalette.Green.ToHtml(false);
		TooltipRichTextLabel description = new();
		description.BbcodeEnabled = true;
		description.Text = $"[color=#{greenHex}]{copy._effectDescription}[/color]";
		description.CustomMinimumSize = new(250, 0);
		description.FitContent = true;
		description.HintUnderlined = true;
		container.AddChild(description);
		panelContainer.AddChild(container);

		return panelContainer;
	}
}

