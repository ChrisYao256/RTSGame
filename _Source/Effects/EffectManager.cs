using Godot;
namespace RTSGame.Units;

public class EffectManager
{
	public static Effect Apply(EffectResource res, Node2D target)
	{
		Effect effectNode = res.CreateNode();
		target.AddChild(effectNode);
		return effectNode;
	}
}
