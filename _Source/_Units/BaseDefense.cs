using Godot;

namespace RTSGame.Units;

public partial class BaseDefense : TowerUnit
{
	public override void _Ready()
	{
		base._Ready();
		_movable = true;
		_removable = false;
		_sellable = true;
	}
}