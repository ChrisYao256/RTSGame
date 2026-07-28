using Godot;
using Godot.Collections;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public abstract partial class GlobalEffectResource : Resource
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

	public Texture2D _defaultIcon = GD.Load<Texture2D>("res://_Assets/Upgrade.png");

	[Export]
	public string _effectDescription = "";

	[Export]
	public PackedScene _floatingTextScene;

	private HoverInfoImage _iconNode;

	public GlobalEffect _effect;

	public abstract GlobalEffect CreateNode();

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
	/// Makes an UpgradeButton whose popup is created using TooltipManager.GetTooltipContainer((_effectDescription, _effectTopRightString));
	/// </summary>
	/// <param name="clickable"></param>
	/// <returns></returns>
	public virtual HoverInfoImage MakeEffectTooltip(bool clickable)
	{
		HoverInfoImage trigger = new HoverInfoImage();
		if (_effectIcon != null)
		{
			trigger.TextureNormal = _effectIcon;
		}
		else
		{
			trigger.TextureNormal = _defaultIcon;
		};
		PanelContainer popup = TooltipManager.GetTooltipContainer((_effectDescription, ""));
		trigger._popupBox = popup;
		popup.Visible = false;

		_iconNode = trigger;

		trigger.ResetSize();

		return trigger;
	}
}

