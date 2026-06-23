using Godot;
using Godot.Collections;
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
