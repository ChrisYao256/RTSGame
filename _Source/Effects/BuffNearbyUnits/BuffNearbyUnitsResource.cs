using Godot;
using Godot.Collections;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class BuffNearbyUnitsResource : EffectResource
{
	[Export]
	public Array<EffectResource> _buffs;

	[Export]
	public float _radius;

	[Export]
	public PackedScene _slowVisualScene;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		bool matchFound = false;
		foreach (EffectResource resource in allMatchingResource)
		{
			DebuffNearbyTowersResource typedResource = resource as DebuffNearbyTowersResource;
			bool mismatch = false;
			for (int i = 0; i < _buffs.Count; i++)
			{
				if (_buffs[i].GetType() != typedResource._debuffs[i].GetType())
				{
					mismatch = true;
					break;
				}
			}
			if (mismatch)
			{
				continue;
			}
			if (matchFound)
			{
				throw new System.Exception("Found two DebuffNearbyTowersResource with the same _debuffs!");
			}
			matchFound = true;
			for (int i = 0; i < _buffs.Count; i++)
			{
				_buffs[i].MergeWithOld(typedResource._debuffs[i], []);
			}
			typedResource.SetDescription();
		}
		return !matchFound;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = "Nearby enemies are ";
		for (int i = 0; i < _buffs.Count - 1; i++)
		{
			EffectResource resource = _buffs[i];
			resource.SetDescription();
			_effectDescription += $"[url={TooltipRichTextLabel.EncodeMetaString(resource._effectDescription, resource._effectTopRightString)}]{resource._effectName}[/url]" + ", ";
		}
		EffectResource lastResource = _buffs[_buffs.Count - 1];
		lastResource.SetDescription();
		_effectDescription += $"[url={TooltipRichTextLabel.EncodeMetaString(lastResource._effectDescription, lastResource._effectTopRightString)}]{lastResource._effectName}[/url]";
	}

	public override Effect CreateNode()
	{
		return new BuffNearbyUnits(this);
	}
}
