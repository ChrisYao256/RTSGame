using Godot;
namespace RTSGame.Units;

[GlobalClass]
public partial class DebuffNearbyTowersResource : EffectResource
{
	[Export]
	public EffectResource _debuff;

	[Export]
	public float _radius;

	[Export]
	public double _period;

	[Export]
	public PackedScene _slowVisualScene;

	public override void SetDescription()
	{
		_debuff.SetDescription();
		_effectDescription = "Debuff all towers in a " + _radius + " radius with " + $"[url={TooltipRichTextLabel.EncodeMetaString(_debuff._effectDescription, _debuff._effectTopRightString)}]{_debuff._effectName}[/url]";
	}

	public override Effect CreateNode()
	{
		return new DebuffNearbyTowers(this);
	}
}
