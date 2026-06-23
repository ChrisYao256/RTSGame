using Godot;
using Godot.Collections;
using RTSGame.Units;
using System;
using System.Collections.Generic;
using System.Linq;

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
	private Vector4I _startingMoney = new Vector4I(100, 0, 0, 0);

	[Export]
	private int _startingSpawnerLimit = 5;

	public static float TileSize = 120f;

	private System.Collections.Generic.Dictionary<int, List<(List<string>, float)>> _waveList;
	public int _waveIndex = 0;

	private UnitManager _unitManager;
	public TDTowerManager _towerManager;
	private VBoxContainer _rightPanel;
	private UnitInfoPanel _infoPanel;
	private Label _hpLabel;
	private TooltipRichTextLabel _moneyLabel;
	private Grid _grid;
	private Label _waveCounter;
	private Label _bossWaveLabel;
	private Label _spawnerLimitLabel;
	public UpgradeButton _spawnerLimitIncreaseButton;
	private TooltipRichTextLabel _spawnerLimitIncreaseButtonText;
	private Control _fullscreenOverlay;

	private Exit _base;

	public int _hp;
	public Vector4I _money;
	private int _spawnerLimit;
	private int _spawnerCount;
	public float _spawnerExpanderCostMultipler;

	public override void _Ready()
	{
		_rightPanel = GetParent().GetNode("RightPanelCanvasLayer").GetNode("RightPanel").GetNode<VBoxContainer>("VBoxContainer");
		_hpLabel = _rightPanel.GetNode<Label>("HpLabel");
		_moneyLabel = _rightPanel.GetNode<TooltipRichTextLabel>("MoneyLabel");
		_grid = GetParent().GetNode<Grid>("TileMapLayer");
		_waveCounter = _rightPanel.GetNode<Label>("WaveCounter");
		_bossWaveLabel = _rightPanel.GetNode<Label>("BossWaveLabel");
		_spawnerLimitLabel = _rightPanel.GetNode("HBoxContainer").GetNode<Label>("SpawnerLimitLabel");
		_spawnerLimitIncreaseButton = _rightPanel.GetNode("HBoxContainer").GetNode<UpgradeButton>("Button");
		_spawnerLimitIncreaseButtonText = _spawnerLimitIncreaseButton.GetNode<TooltipRichTextLabel>("RichTextLabel");
		_spawnerLimitIncreaseButton.MouseEntered += () => _spawnerLimitIncreaseButtonText.AddThemeColorOverride("default_color", ThemePalette.White);
		_spawnerLimitIncreaseButton.MouseExited += () => _spawnerLimitIncreaseButtonText.AddThemeColorOverride("default_color",  GetSpawnerLimitIncreaseButtonTextColor());

		_fullscreenOverlay = GetParent().GetNode<Control>("FullscreenOverlay");
		_infoPanel = GetParent().GetNode<UnitInfoPanel>("UnitInfoPanel");
	}

	public void Initialize()
	{
		_unitManager = GetParent().GetNode<UnitManager>("UnitManager");
		_towerManager = GetParent().GetNode<TDTowerManager>("TowerManager");
		_towerManager.InitializeTowersPanel(_availTowerList, _unitManager, TowerUnit.TowerType.Defense);

		_waveList = new System.Collections.Generic.Dictionary<int, List<(List<string>, float)>>{
			{ 6, [(["MegaSlime"], 1f)]},
			{ 12, [(["Archbishop"], 1f)]},
			{ 20, [(["BigArchbishop"], 1f)]},
		};

		_base = (Exit)(_unitManager.SpawnUnit(_grid.GetExitLocation(), 0, "Exit"));
		_base._tdManager = this;
		_base.CollisionLayer = UnitManager.TowerLayerMask;
		_base._radius = 0.1f;
		_base.SetSize();

		UpdateHp(20);

		IncreaseSpawnerLimit(_startingSpawnerLimit);
		UpdateMoney(_startingMoney);
		UpdateWaveIndexCounter();
		UpdatePortalLimitButtonText();
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

	public void UpdateSpawnerLimit(int newLimit)
	{
		_spawnerLimit = newLimit;
		_spawnerLimitLabel.Text = "Portals: " + _spawnerCount + "/" + _spawnerLimit;
	}

	public void IncreaseSpawnerLimit(int change)
	{
		UpdateSpawnerLimit(_spawnerLimit + change);
	}

	public void UpdateSpawnerCount(int newCount)
	{
		_spawnerCount = newCount;
		_spawnerLimitLabel.Text = "Portals: " + _spawnerCount + "/" + _spawnerLimit;
	}

	public void IncreaseSpawnerCount(int change)
	{
		UpdateSpawnerCount(_spawnerCount + change);
	}

	public bool CanBuildExtraSpawner()
	{
		return _spawnerCount < _spawnerLimit;
	}

	public void BuyExtraPortalLimit()
	{
		int cost = GetPortalLimitUpgradeCost();
		if (_money[0] < cost)
		{
			return;
		}
		IncreaseSpawnerLimit(1);
		SpendMoney(new Vector4I(cost, 0, 0, 0));
		UpdatePortalLimitButtonText();
	}

	public void UpdatePortalLimitButtonText()
	{
		int cost = 10 * (int)Math.Pow(2, (double)(_spawnerLimit - _startingSpawnerLimit));
		_spawnerLimitIncreaseButtonText.Text = "Increase Portal Limit: \n" + Utils.MakeMoneyText(new Vector4I(cost, 0, 0, 0));
		_spawnerLimitIncreaseButtonText.AddThemeColorOverride("default_color", GetSpawnerLimitIncreaseButtonTextColor());
	}

	public int GetPortalLimitUpgradeCost()
	{
		return 10 * (int)Math.Pow(2, (double)(_spawnerLimit - _startingSpawnerLimit));
	}

	private void SpawnEnemyAtEntrance(string name)
	{
		Unit unit = _unitManager.SpawnUnit(_grid.GetEntrancePosition(), 1, name, true);
		List<Vector2> waypoints = _grid.GetPath(_grid.GetEntrancePosition(), _grid.GetExitLocation());
		foreach (Vector2 waypoint in waypoints)
		{
			unit.AddCommand(new AttackMove(unit, waypoint));
		}
		unit.Connect(Unit.SignalName.Died, Callable.From<Unit>(OnUnitDied));
	}

	public InvaderUnit SpawnEnemyAtGlobalPosition(string name, Vector2 position)
	{
		Unit unit = _unitManager.SpawnUnit(position, 1, name, true);
		List<Vector2> waypoints = _grid.GetPath(position, _grid.GetExitLocation());
		if (unit is InvaderUnit invader)
		{
			invader._pathToExit = new Godot.Collections.Array<Vector2>(waypoints);
		}
		unit.Connect(Unit.SignalName.Died, Callable.From<Unit>(OnUnitDied));
		return (InvaderUnit)unit;
	}

	public InvaderUnit SpawnEnemyFromTower(string name, Vector2 position)
	{
		Unit unit = _unitManager.SpawnUnit(position, 1, name, true);
		List<Vector2> waypoints = _grid.GetPath(position, _grid.GetExitLocation());
		if (unit is InvaderUnit invader)
		{
			invader._pathToExit = new Godot.Collections.Array<Vector2>(waypoints);
		}
		unit.Connect(Unit.SignalName.Died, Callable.From<Unit>(OnUnitDied));
		return (InvaderUnit)unit;
	}

	public InvaderUnit SpawnEnemyFromTower(string name, Vector2I gridPosition)
	{
		Unit unit = _unitManager.SpawnUnit(_grid.MapToGlobal(gridPosition), 1, name, true);
		List<Vector2> waypoints = _grid.GetPath(gridPosition, _grid.GetExitLocation());
		if (unit is InvaderUnit invader)
		{
			invader._pathToExit = new Godot.Collections.Array<Vector2>(waypoints);
		}
		unit.Connect(Unit.SignalName.Died, Callable.From<Unit>(OnUnitDied));
		return (InvaderUnit)unit;
	}

	public static InvaderUnit GetEnemy(string name, bool setUnit)
	{
		InvaderUnit unit = (InvaderUnit) UnitManager.GetUnit(name, setUnit);
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

	private void SwitchPauseState()
	{
		if (GetTree().Paused)
		{
			UnpauseTD();
		}
		else
		{
			PauseTD();
		}
	}

	private void PauseTD()
	{
		GetTree().Paused = true;
		_fullscreenOverlay.GetNode<Label>("Paused").Visible = true;
	}

	private void UnpauseTD()
	{
		GetTree().Paused = false;
		_fullscreenOverlay.GetNode<Label>("Paused").Visible = false;
	}

	private void UpdateMoney(Vector4I newMoney)
	{
		_money = newMoney;
		_moneyLabel.Text = Utils.MakeMoneyText(_money, true);
		if (_infoPanel.GetSelectedUnit() is not null)
		{
			_infoPanel.GetSelectedUnit().EmitSignal(Unit.SignalName.UpdateInfo);
		}
		_spawnerLimitIncreaseButton.UpdateAffordabilityDisplay(Utils.VectorDivision(newMoney, new(GetPortalLimitUpgradeCost(),0,0,0)));
		_spawnerLimitIncreaseButtonText.AddThemeColorOverride("default_color", GetSpawnerLimitIncreaseButtonTextColor());
	}

	private Color GetSpawnerLimitIncreaseButtonTextColor()
	{
		if (Utils.VectorDivision(_money, new(GetPortalLimitUpgradeCost(), 0, 0, 0)) >= 1f)
		{
			return ThemePalette.Green;
		}
		else
		{
			return ThemePalette.Red;
		}
	}

	public void GainMoney(Vector4I gain)
	{
		UpdateMoney(_money + gain);
	}

	public void SpendMoney(Vector4I cost)
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
