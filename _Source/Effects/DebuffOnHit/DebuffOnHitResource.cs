using Godot;
using Godot.Collections;
namespace RTSGame.Units;

[GlobalClass]
public partial class DebuffOnHitResource : EffectResource
{
	[Export] public Array<EffectResource> _debuffs;

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

	public override Effect CreateNode()
	{
		return new DebuffOnHit(this);
	}
}
