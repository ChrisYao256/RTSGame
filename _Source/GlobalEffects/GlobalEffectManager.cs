using Godot;
using RTSGame.Source;
namespace RTSGame.Units;

public class GlobalEffectManager
{
	public static GlobalEffect Apply(GlobalEffectResource res, TDManager tdManager)
	{
		GlobalEffect effectNode = res.CreateNode();
		effectNode._tdManager = tdManager;
		effectNode.ConnectSignals();
		effectNode.OnCreation();
		return effectNode;
	}
}
