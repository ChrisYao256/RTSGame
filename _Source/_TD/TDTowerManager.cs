using Godot;
using Godot.Collections;
using RTSGame.Units;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace RTSGame.Source;

public partial class TDTowerManager : Node2D
{
	public static Texture2D TowerBackgroundTexture = GD.Load<Texture2D>("res://_Assets/TowerIconBackground.png");

	private TDManager _tdManager;
	private UnitManager _unitManager;
	private VBoxContainer _rightPanel;
	private GridContainer _towersBox;
	private Grid _grid;

	private Godot.Collections.Array<string> _towers;

	public bool _placementMode { get; private set; } = false;
	private string _towerToPlace;
	private TowerUnit _previewTower;

	public Array<TowerUnit> _allTowers = [];

	private TowerUnit.TowerType _tab;

	public override void _Ready()
	{
		_rightPanel = GetParent().GetNode("RightPanelCanvasLayer").GetNode("RightPanel").GetNode<VBoxContainer>("VBoxContainer");
		_grid = GetParent().GetNode<Grid>("TileMapLayer");
		_tdManager = GetParent().GetNode<TDManager>("TdManager");
		UpdateIncomeDisplay();
		UpdateDPSDisplay();
		UpdateTotalHpLabel();
	}

	public void Initialize(UnitManager unitManager)
	{
		_unitManager = unitManager;
	}

	public override void _Process(double delta)
	{
		if (_placementMode)
		{
			UpdatePlacement();
		}
	}

	public void InitializeTowersPanel(TowerUnit.TowerType tab)
	{
		_tdManager._infoPanel.MakeAllTowersMenu(tab);
	}

	public void UpdateTowersPanel(Godot.Collections.Array<string> towers)
	{
		_towers = towers;
		foreach (Node child in _towersBox.GetChildren())
		{
			child.QueueFree();
		}
		foreach (string name in towers)
		{
			VBoxContainer container = new VBoxContainer();

			string name_ = name;
			TowerUnit unit = (TowerUnit)UnitManager.GetUnit(name, true);

			if (unit._towerType != _tab)
			{
				unit.QueueFree();
				container.QueueFree();
				continue;
			}

			Label nameLabel = new Label();
			nameLabel.Text = unit._name;
			nameLabel.CustomMinimumSize = new(160, 0);
			nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
			container.AddChild(nameLabel);

			AddChild(unit);

			HoverInfoImage towerButton
				= unit.MakeTowerTooltip(true);
			towerButton.Pressed += (() =>
			{
				if (Utils.VectorLeq(unit._cost, _tdManager._money))
				{
					EnterPlacementMode(name_);
				}
			});
			towerButton.MouseEntered += () => nameLabel.AddThemeColorOverride("font_color", ThemePalette.White);
			towerButton.MouseExited += () => nameLabel.AddThemeColorOverride("font_color", ThemePalette.Yellow);

			PanelContainer panelContainer = new();
			panelContainer.AddChild(towerButton);
			container.AddChild(panelContainer);

			TooltipRichTextLabel costLabel = new TooltipRichTextLabel();
			costLabel.FitContent = true;
			costLabel.BbcodeEnabled = true;
			if (unit is not Spawner)
			{
				costLabel.Text = Utils.MakeMoneyText(unit._cost);
			}
			else
			{
				costLabel.Text = "+" + Utils.MakeMoneyText(unit.GetIncome());
			}
			costLabel.HorizontalAlignment = HorizontalAlignment.Center;
			container.AddChild(costLabel);

			_towersBox.AddChild(container);

			unit.QueueFree();

		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent &&
				mouseEvent.ButtonIndex == MouseButton.Right)
		{
			if (mouseEvent.Pressed)
			{
				if (_placementMode)
				{
					ExitPlacementMode();
					GetViewport().SetInputAsHandled();
				}
			}
		}
	}

	public void EnterPlacementMode(string towerName)
	{
		_placementMode = true;
		_towerToPlace = towerName;
		_previewTower = (TowerUnit)_unitManager.SpawnUnit(GetGlobalMousePosition(), 0 ,towerName, hasEffects: false);
		_previewTower.Modulate = new Color(0.5f, 0.5f, 0.5f, 1f);
		_previewTower.SetAttackRange();
		_previewTower.DisablePhysicsProcess();
		_unitManager.UpdatePlayerSelection([_previewTower]);
	}

	public void ExitPlacementMode()
	{
		_unitManager.UpdatePlayerSelection([]);
		_placementMode = false;
		_towerToPlace = "";
		if (_previewTower is Spawner spawner)
		{
			spawner.HideSpawnRange();
		}
		_previewTower.QueueFree();
		_previewTower = null;
	}

	private void UpdatePlacement()
	{
		Vector2 mousePos = GetGlobalMousePosition();
		mousePos = _grid.ToLocal(mousePos);

		// Convert mouse position to grid coordinates (e.g., Vector2I(5, 3))
		Vector2I gridCoords = _grid.LocalToMap(mousePos);
		Vector2I buildableGridCoords;
		if (_previewTower is Spawner)
		{
			buildableGridCoords = _grid.FindClosestBuildableCell(gridCoords, true);
		}
		else
		{
			buildableGridCoords = _grid.FindClosestBuildableCell(gridCoords, false);
		}

		_previewTower.GlobalPosition = _grid.ToGlobal(_grid.MapToLocal(buildableGridCoords));
		_previewTower._gridLocation = buildableGridCoords;

		if (Input.IsActionJustPressed("Left_click"))
		{
			if (_previewTower._towerType == TowerUnit.TowerType.Spawner && !_tdManager.CanBuildExtraSpawner())
			{
				_tdManager.FlashPortalLimit();
				ExitPlacementMode();
			}
			else if (Utils.VectorLeq(_previewTower._cost, _tdManager._money))
			{
				PlaceDraggingTower(buildableGridCoords);
			}
			else
			{
				_tdManager.FlashMoney();
				ExitPlacementMode();
			}
		}
		
	}

	// for automatically placing tower
	public TowerUnit PlaceTower(Vector2I gridCoords, string towerName)
	{
		Vector2 position = _grid.ToGlobal(_grid.MapToLocal(gridCoords));
		TowerUnit newTower = (TowerUnit)_unitManager.SpawnUnit(position, 0, towerName, false, gridCoords);
		_tdManager.Connect(TDManager.SignalName.NewWave, Callable.From(newTower.OnNewWave));
		if (newTower._towerType == TowerUnit.TowerType.Spawner)
		{
			_tdManager.IncreaseSpawnerCount(1);
		}
		_grid.OccupyCell(gridCoords, (TowerUnit)newTower);
		List<TowerUnit> allTowersCopy = _allTowers.ToList();
		_allTowers.Add(newTower);
		foreach (TowerUnit tower in allTowersCopy)
		{
			tower.OnPlacedTower(newTower);
		}
		
		newTower.EmitSignal(Unit.SignalName.Creation);
		UpdateIncomeDisplay();
		UpdateDPSDisplay();
		UpdateTotalHpLabel();
		return newTower;
	}

	// for dragging tower from right panel to place
	private void PlaceDraggingTower(Vector2I gridCoords)
	{
		Vector2 position = _grid.ToGlobal(_grid.MapToLocal(gridCoords));
		TowerUnit newTower = (TowerUnit)_unitManager.SpawnUnit(position, 0, _towerToPlace, false, gridCoords);
		_tdManager.SpendMoney(((TowerUnit)newTower)._cost);
		_tdManager.Connect(TDManager.SignalName.NewWave, Callable.From(newTower.OnNewWave));
		if (newTower._towerType == TowerUnit.TowerType.Spawner)
		{
			_tdManager.IncreaseSpawnerCount(1);
		}
		_grid.OccupyCell(gridCoords, (TowerUnit)newTower);
		List<TowerUnit> allTowersCopy = _allTowers.ToList();
		_allTowers.Add(newTower);
		foreach (TowerUnit tower in allTowersCopy)
		{
			tower.OnPlacedTower(newTower);
		}		
		newTower.EmitSignal(Unit.SignalName.Creation);
		UpdateIncomeDisplay();
		UpdateDPSDisplay();
		UpdateTotalHpLabel();
		//_unitManager.UpdatePlayerSelection([newTower]);
	}

	public void RemoveTower(Vector2I gridCoords)
	{
		if (_grid.IsCellVacant(gridCoords))
		{
			return;
		}
		TowerUnit tower = _grid.GetTowerOnCell(gridCoords);
		if (tower._towerType == TowerUnit.TowerType.Spawner)
		{
			_tdManager.IncreaseSpawnerCount(-1);
		}
		_allTowers.Remove(tower);
		tower.EmitSignal(Unit.SignalName.Removed);
		tower.RemoveAllEffects();
		_grid.UnoccupyCell(gridCoords);
		tower.QueueFree();
	}

	public void TransformTower(Vector2I gridCoords, string newTower, bool inheritTotalCost)
	{
		TowerUnit tower = _grid.GetTowerOnCell(gridCoords);
		Vector4I oldCost = tower.GetTotalCost();
		RemoveTower(gridCoords);
		TowerUnit newTowerNode = PlaceTower(gridCoords, newTower);
		newTowerNode._cost = oldCost;
		_unitManager.UpdatePlayerSelection([newTowerNode]);
	}

	public void UpdateIncomeDisplay()
	{
		TooltipRichTextLabel incomeLabel = _rightPanel.GetNode<TooltipRichTextLabel>("IncomeLabel");

		Vector4I income = new Vector4I(0,0,0,0);

		foreach (TowerUnit tower in _allTowers)
		{
			income += tower.GetIncome();
		}

		int unknownIncome = 0;
		foreach (TowerUnit tower in _allTowers)
		{
			unknownIncome += tower.GetUnknownIncome();
		}

		incomeLabel.Text = "Maximum Income: \n" + Utils.MakeMoneyText(income, multiline: true, unknownMoney: unknownIncome, additionSigns: true);
	}

	public void UpdateDPSDisplay()
	{
		TooltipRichTextLabel dpsLabel = _rightPanel.GetNode<TooltipRichTextLabel>("DPSLabel");

		float dps = 0;

		foreach (TowerUnit tower in _allTowers)
		{
			if (tower._weapon is not null)
			{
				dps += tower._weapon.GetDPS();
			}
		}

		dpsLabel.Text = "Total DPS: " + dps.ToString("F0");
	}

	public void UpdateTotalHpLabel()
	{
		TooltipRichTextLabel totalHpLabel = _rightPanel.GetNode<TooltipRichTextLabel>("TotalHpLabel");

		int hp = 0;

		foreach (TowerUnit tower in _allTowers)
		{
			if (tower is Spawner spawner)
			{
				hp += spawner.GetTotalHp();
			}
		}

		totalHpLabel.Text = "Total Enemy Hp: " + hp.ToString();
	}
}
