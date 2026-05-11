using Godot;
using RTSGame.Units;
using System;
using System.Collections.Generic;

namespace RTSGame.Source;

public partial class TDManager : Node
{
	[Signal]
	public delegate void NewWaveEventHandler();
	[Export]
	public string MenuPath = "res://_Content/_Scenes/StartScene.tscn";

	private List<string> _availTowerList;

	private Dictionary<int, List<(List<string>, float)>> _waveList;
	public int _waveIndex = 0;

	private UnitManager _unitManager;
	private TDTowerManager _towerManager;
	private Panel _rightPanel;
	private Label _hpLabel;
	private Label _moneyLabel;
	private Grid _grid;

	private Exit _base;

	public int _hp;
	public int _money;

	public override void _Ready()
	{
		_rightPanel = GetParent().GetNode<Panel>("RightPanel");
		_hpLabel = _rightPanel.GetNode<Label>("HpLabel");
		_moneyLabel = _rightPanel.GetNode<Label>("MoneyLabel");
		_grid = GetParent().GetNode<Grid>("TileMapLayer");
	}

	public void Initialize()
	{
		_unitManager = GetParent().GetNode<UnitManager>("UnitManager");
		_towerManager = GetParent().GetNode<TDTowerManager>("TowerManager");

		_availTowerList = ["GunTurret", "LaserTurret", "BombTurret", "SlimeSpawner", "HoundSpawner"];
		_towerManager.InitializeTowersPanel(_availTowerList, _unitManager);

		_waveList = new Dictionary<int, List<(List<string>, float)>>{
			{ 5, [(["MegaSlime"], 1f)]}
		};

		_base = (Exit)(_unitManager.SpawnUnit(_grid.GetExitLocation(), 0, "Exit"));
		_base._tdManager = this;
		_base.CollisionLayer = UnitManager.TowerLayerMask;

		_hp = 20;
		_hpLabel.Text = _hp.ToString();

		_money = 100;
		_moneyLabel.Text = _money.ToString();
	}

	public async void SpawnNextWave()
	{
		//List<(List<string>, float)> enemyList = _waveList[_waveIndex];
		//for (int i = 0; i < enemyList.Count; i++)
		//{
		//	(List<string> enemies, float delay) = enemyList[i];
		//	foreach (string enemy in enemyList[i].Item1)
		//	{
		//		SpawnEnemy(enemy);
		//	}
		//	if (delay > 0)
		//	{
		//		await ToSignal(GetTree().CreateTimer(delay), SceneTreeTimer.SignalName.Timeout);
		//	}
		//}

		_waveIndex++;
		EmitSignal(SignalName.NewWave);
		if (_waveList.Keys.Contains(_waveIndex))
		{
			List<(List<string>, float)> enemyList = _waveList[_waveIndex];
			for (int i = 0; i < enemyList.Count; i++)
			{
				(List<string> enemies, float delay) = enemyList[i];
				foreach (string enemy in enemyList[i].Item1)
				{
					SpawnEnemy(enemy);
				}
				if (delay > 0)
				{
					await ToSignal(GetTree().CreateTimer(delay), SceneTreeTimer.SignalName.Timeout);
				}
			}
		}
	}

	private void SpawnEnemy(string name)
	{
		Unit unit = _unitManager.SpawnUnit(_grid.GetEntrancePosition(), 1, name, true);
		GD.Print(name+ " Spawned");
		List<Vector2> waypoints = _grid.GetPath(_grid.GetEntrancePosition(), _grid.GetExitLocation());
		foreach (Vector2 waypoint in waypoints)
		{
			unit.AddCommand(new AttackMove(unit, waypoint));
		}
		unit.Connect(Unit.SignalName.Died, Callable.From<Unit>(OnUnitDied));
	}

	public void SpawnEnemyFromTower(string name, Vector2 position)
	{
		Unit unit = _unitManager.SpawnUnit(position, 1, name, true);
		GD.Print(name + " Spawned");
		List<Vector2> waypoints = _grid.GetPath(position, _grid.GetExitLocation());
		foreach (Vector2 waypoint in waypoints)
		{
			unit.AddCommand(new AttackMove(unit, waypoint));
		}
		unit.Connect(Unit.SignalName.Died, Callable.From<Unit>(OnUnitDied));
	}

	public void UnitExited(InvaderUnit unit)
	{
		UpdateHp(_hp - unit._hpDeducted);
	}

	private void UpdateHp(int newHp)
	{
		_hp = newHp;
		_hpLabel.Text = "Hp" + _hp.ToString();
		if (_hp <= 0)
		{
			EndTD();
		}
	}

	private void EndTD()
	{
		GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, MenuPath);
	}

	private void UpdateMoney(int newMoney)
	{
		_money = newMoney;
		_moneyLabel.Text = "$" + _money.ToString();
	}

	public void SpendMoneyOnTower(int cost)
	{
		UpdateMoney(_money - cost);
	}
	
	private void OnUnitDied(Unit unit)
	{
		if (unit is InvaderUnit invader)
		{
			UpdateMoney(_money + invader._moneyDropped);
		}
	}
}
