using Godot;
namespace RTSGame.Units;

[GlobalClass]
public abstract partial class EffectResource: Resource
{
	[Export]
	public string _effectName;

	[Export]
	public Texture2D _effectIcon;

	[Export]
	public string _effectDescription;

	public abstract Effect CreateNode();

	// Defines behavior when an effect is added to a unit that already has unit. By default, the new effect replaces the old one. 
	public virtual void MergeWithOld(EffectResource oldResource)
	{
		return;
	}

}

