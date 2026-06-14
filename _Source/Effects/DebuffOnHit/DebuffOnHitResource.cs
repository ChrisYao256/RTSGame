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
			_effectDescription += resource._effectName + ", ";
		}
		EffectResource lastResource = _debuffs[_debuffs.Count - 1];
		lastResource.SetDescription();
		_effectDescription += lastResource._effectName;

	}

	public override Effect CreateNode()
	{
		return new DebuffOnHit(this);
	}
}
