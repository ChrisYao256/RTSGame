using Godot;
using RTSGame.Units;
public partial class AddEffectContext : RefCounted
{
	public Unit _target { get; set; }

	public EffectResource _effect;

	public AddEffectContext(Unit target, EffectResource effect)
	{
		_target = target;
		_effect = effect;
	}
}