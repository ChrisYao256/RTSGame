using Godot;
using RTSGame.Source;
namespace RTSGame.Units;

public class GlobalEffectManager
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="res"></param>
	/// <param name="tdManager"></param>
	/// <param name="noImmediateEffect">Used for loading the game. All effects that have an immediate effects should not trigger said effect if this is true</param>
	/// <returns></returns>
	public static GlobalEffect Apply(GlobalEffectResource res, TDManager tdManager, bool noImmediateEffect)
	{
		GlobalEffect effectNode = res.CreateNode();
		effectNode._tdManager = tdManager;
		effectNode._noImmediateEffect = noImmediateEffect;
		effectNode.ConnectSignals();
		effectNode.OnCreation();
		return effectNode;
	}
}
