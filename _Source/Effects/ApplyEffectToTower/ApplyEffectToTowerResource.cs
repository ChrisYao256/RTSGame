using Godot;
using Godot.Collections;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class ApplyEffectToTowerResource : EffectResource
{
	[Export] public Array<EffectResource> _debuffs;
	[Export] public float _interval;
	[Export] public float _range;
	[Export] public Color _tracerColor = ThemePalette.Blue;
	[Export] public float _tracerWidth = 4.0f;
	[Export] public float _tracerDuration = 1.0f;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		bool matchFound = false;
		foreach (EffectResource resource in allMatchingResource)
		{
			ApplyEffectToTowerResource typedResource = resource as ApplyEffectToTowerResource;
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
				throw new System.Exception("Found two ApplyEffectToTowerResource with the same _debuffs!");
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
		_effectDescription = "Periodically inflicts a nearby tower with ";
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

	public override Effect CreateNode()
	{
		return new ApplyEffectToTower(this);
	}
}
