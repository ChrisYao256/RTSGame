using Godot;

namespace RTSGame.Units;

public partial class Entrance : TowerUnit
{
	public override void _Ready()
	{
		base._Ready();
		_movable = true;
		_removable = false;
		_sellable = false;
		_mustBeNextToPath = true;
	}
}