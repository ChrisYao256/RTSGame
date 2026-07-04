using Godot;
using Godot.Collections;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class DebuffOnHitResource : EffectResource
{
	[Export] public Array<EffectResource> _debuffs;

	// If there is an existing DebuffOnHitResource with exactly the same _debuffs, then merge with the old _debuffs. Otherwise add this resource. 
	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		bool matchFound = false;
		foreach (EffectResource resource in allMatchingResource)
		{
			DebuffOnHitResource typedResource = resource as DebuffOnHitResource;
			bool mismatch = false;
			for (int i = 0; i < _debuffs.Count; i++)
			{
				if (_debuffs[i].GetType() != typedResource._debuffs[i].GetType())
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
				throw new System.Exception("Found two DebuffOnHitResource with the same _debuffs!");
			}
			matchFound = true;
			for (int i = 0; i < _debuffs.Count; i++)
			{
				_debuffs[i].MergeWithOld(typedResource._debuffs[i], []);
			}
			typedResource.SetDescription();
		}
		return !matchFound;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = "Every hit inflicts target with ";
		for (int i = 0; i < _debuffs.Count - 1; i++)
		{
			EffectResource resource = _debuffs[i];
			resource.SetDescription();
			_effectDescription += $"[url={TooltipRichTextLabel.EncodeMetaString(resource._effectDescription, resource._effectTopRightString)}]{resource._effectName}[/url]" + ", ";
		}
		EffectResource lastResource = _debuffs[_debuffs.Count - 1];
		lastResource.SetDescription();
		_effectDescription += $"[url={TooltipRichTextLabel.EncodeMetaString(lastResource._effectDescription, lastResource._effectTopRightString)}]{lastResource._effectName}[/url]";
	}

	public override void SetUpgradeDescription()
	{
		_effectDescription = "";
		for (int i = 0; i < _debuffs.Count; i++)
		{
			EffectResource resource = _debuffs[i];
			resource.SetUpgradeDescription();
			_effectDescription += resource._effectDescription;
		}
	}

	public override Effect CreateNode()
	{
		return new DebuffOnHit(this);
	}
}
