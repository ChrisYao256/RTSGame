using Godot;
using Godot.Collections;
using RTSGame.Units;
using System;
using System.Linq;
using System.Collections.Generic;

namespace RTSGame.Source;

public partial class TDManager : Node
{
	[Signal]
	public delegate void NewWaveEventHandler();
	[Export]
	public string MenuPath = "res://_Content/_Scenes/StartScene.tscn";

	[Export]
	public Array<string> _availTowerList;

	[Export]
	private int _startingMoney = 100;

	public static float TileSize = 64f;

	private System.Collections.Generic.Dictionary<int, List<(List<string>, float)>> _waveList;
	public int _waveIndex = 0;

	private UnitManager _unitManager;
	public TDTowerManager _towerManager;
	private Panel _rightPanel;
	private Label _hpLabel;
	private Label _moneyLabel;
	private Grid _grid;
	private Label _waveCounter;
	private Label _bossWaveLabel;

	private Exit _base;

	public int _hp;
	public int _money;

	public override void _Ready()
	{
		_rightPanel = GetParent().GetNode<Panel>("RightPanel");
		_hpLabel = _rightPanel.GetNode<Label>("HpLabel");
		_moneyLabel = _rightPanel.GetNode<Label>("MoneyLabel");
		_grid = GetParent().GetNode<Grid>("TileMapLayer");
		_waveCounter = _rightPanel.GetNode<Label>("WaveCounter");
		_bossWaveLabel = _rightPanel.GetNode<Label>("BossWaveLabel");
	}

	public void Initialize()
	{
		_unitManager = GetParent().GetNode<UnitManager>("UnitManager");
		_towerManager = GetParent().GetNode<TDTowerManager>("TowerManager");
		_towerManager.InitializeTowersPanel(_availTowerList, _unitManager, TowerUnit.TowerType.Defense);

		_waveList = new System.Collections.Generic.Dictionary<int, List<(List<string>, float)>>{
			{ 7, [(["MegaSlime"], 1f)]},
			{ 15, [(["Archbishop"], 1f)]},
			{ 20, [(["BigArchbishop"], 1f)]},
		};

		_base = (Exit)(_unitManager.SpawnUnit(_grid.GetExitLocation(), 0, "Exit"));
		_base._tdManager = this;
		_base.CollisionLayer = UnitManager.TowerLayerMask;
		_base._radius = 0.1f;
		_base.SetSize();

		UpdateHp(20);

		UpdateMoney(_startingMoney);
		UpdateWaveIndexCounter();
	}

	public async void SpawnNextWave()
	{
		_waveIndex++;
		EmitSignal(SignalName.NewWave);
		UpdateWaveIndexCounter();
		if (_waveList.Keys.Contains(_waveIndex))
		{
			List<(List<string>, float)> enemyList = _waveList[_waveIndex];
			for (int i = 0; i < enemyList.Count; i++)
			{
				(List<string> enemies, float delay) = enemyList[i];
				foreach (string enemy in enemyList[i].Item1)
				{
					SpawnEnemyAtEntrance(enemy);
				}
				if (delay > 0)
				{
					await ToSignal(GetTree().CreateTimer(delay), SceneTreeTimer.SignalName.Timeout);
				}
			}
		}
	}

	private void UpdateWaveIndexCounter()
	{
		_waveCounter.Text = "Wave " + _waveIndex;
		_bossWaveLabel.Text = "Boss at wave " + GetNextBossWave();
	}

	private void SpawnEnemyAtEntrance(string name)
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

	public InvaderUnit SpawnEnemyFromTower(string name, Vector2 position)
	{
		Unit unit = _unitManager.SpawnUnit(position, 1, name, true);
		GD.Print(name + " Spawned");
		List<Vector2> waypoints = _grid.GetPath(position, _grid.GetExitLocation());
		foreach (Vector2 waypoint in waypoints)
		{
			unit.AddCommand(new AttackMove(unit, waypoint));
		}
		unit.Connect(Unit.SignalName.Died, Callable.From<Unit>(OnUnitDied));
		return (InvaderUnit)unit;
	}

	public InvaderUnit GetEnemy(string name)
	{
		InvaderUnit unit = (InvaderUnit) UnitManager.GetUnit(name);
		return unit;
	}

	public Unit SpawnAllyFromTower(string name, Vector2 position)
	{
		Unit unit = _unitManager.SpawnUnit(position, 0, name, true);
		GD.Print(name + " Spawned");
		unit.Connect(Unit.SignalName.Died, Callable.From<Unit>(OnUnitDied));
		return (Unit)unit;
	}

	public void AddEnemyToQueue(string unit)
	{
		int nextWave = GetNextBossWave();
		if (nextWave == -1)
		{
			return;
		}
		else
		{
			_waveList[nextWave].Add(([unit], 0.5f));
		}
	}

	public void UnitExited(InvaderUnit unit)
	{
		UpdateMoney(_money - unit._moneyDeducted);
		UpdateHp(_hp - unit._hpDeducted);
	}

	private void UpdateHp(int newHp)
	{
		_hp = newHp;
		_hpLabel.Text = "Hp: " + _hp.ToString();
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

	public void GainMoney(int gain)
	{
		UpdateMoney(_money + gain);
	}

	public void SpendMoneyOnTower(int cost)
	{
		UpdateMoney(_money - cost);
	}
	
	private void OnUnitDied(Unit unit)
	{
		if (unit is InvaderUnit invader)
		{
			GainMoney(invader.GetMoneyDropped());
		}
	}

	public int GetNextBossWave()
	{
		List<int> waves = _waveList.Keys.ToList();
		int? result = waves.Where(x => x > _waveIndex)
									.Cast<int?>() // Allows returning null if none found
									.Min();
		if (result is not null)
		{
			return (int)result;
		}
		else
		{
			return -1;
		}
	}
}
