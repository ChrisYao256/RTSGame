using Godot;
using RTSGame.Units;
using System.Collections.Generic;
using System.Linq;

namespace RTSGame.Source;

public partial class TDTowerManager : Node2D
{

	private TDManager _tdManager;
	private UnitManager _unitManager;
	private Panel _rightPanel;
	private GridContainer _towersBox;
	private Grid _grid;

	private Godot.Collections.Array<string> _towers;

	private bool _placementMode = false;
	private string _towerToPlace;
	private TowerUnit _previewTower;

	private List<TowerUnit> _allTowers = [];

	public override void _Ready()
	{
		_rightPanel = GetParent().GetNode<Panel>("RightPanel");
		_towersBox = _rightPanel.GetNode<GridContainer>("Towers");
		_grid = GetParent().GetNode<Grid>("TileMapLayer");
		_tdManager = GetParent().GetNode<TDManager>("TdManager");
		UpdateIncomeDisplay();
	}

	public override void _Process(double delta)
	{
		if (_placementMode)
		{
			UpdatePlacement();
		}
	}

	public void InitializeTowersPanel(Godot.Collections.Array<string> towers, UnitManager unitManager, TowerUnit.TowerType tab)
	{
		_unitManager = unitManager;
		_towers = towers;
		foreach (Node child in _towersBox.GetChildren())
		{
			child.QueueFree();
		}
		foreach (string name in towers)
		{
			VBoxContainer container = new VBoxContainer();

			string name_ = name;
			TowerUnit unit = (TowerUnit)UnitManager.GetUnit(name);

			if (unit._towerType != tab)
			{
				continue;
			}

			AddChild(unit);

			Label nameLabel = new Label();
			nameLabel.Text = unit._name;
			container.AddChild(nameLabel);

			HoverInfoImage towerButton
				= unit.MakeTowerTooltip(true);
			towerButton.Pressed += (()=> 
			{
				if (_tdManager._money >= unit._cost)
				{
					EnterPlacementMode(name_); 
				} 
			});

			container.AddChild(towerButton);

			Label costLabel = new Label();
			costLabel.Text = "$" + unit._cost.ToString();
			container.AddChild(costLabel);
			
			_towersBox.AddChild(container);

			unit.QueueFree();

		}
	}

	public void SwitchDisplayedTabDefense()
	{
		InitializeTowersPanel(_tdManager._availTowerList, _unitManager, TowerUnit.TowerType.Defense);
	}

	public void SwitchDisplayedTabSupport()
	{
		InitializeTowersPanel(_tdManager._availTowerList, _unitManager, TowerUnit.TowerType.Support);
	}

	public void SwitchDisplayedTabSpawner()
	{
		InitializeTowersPanel(_tdManager._availTowerList, _unitManager, TowerUnit.TowerType.Spawner);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent &&
				mouseEvent.ButtonIndex == MouseButton.Right)
		{
			if (mouseEvent.Pressed)
			{
				if (_placementMode)
				{
					ExitPlacementMode();
				}
			}
		}
	}

	private void EnterPlacementMode(string towerName)
	{
		_placementMode = true;
		_towerToPlace = towerName;
		_previewTower = (TowerUnit)_unitManager.SpawnUnit(GetGlobalMousePosition(), 0 ,towerName);
		_previewTower.DisablePhysicsProcess();
		_unitManager.UpdatePlayerSelection([_previewTower]);
	}

	private void ExitPlacementMode()
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

		// Snap the preview to the center of the tile
		_previewTower.GlobalPosition = _grid.ToGlobal(_grid.MapToLocal(gridCoords));
		_previewTower._gridLocation = gridCoords;

		if (_previewTower is Spawner spawner)
		{
			spawner.ShowSpawnRange();
		}

		// Check if the tile allows building
		TileData data = _grid.GetCellTileData(gridCoords);
		bool canBuild = false;

		if (data != null)
		{
			// Access the custom data we set up in the editor
			canBuild = (bool)data.GetCustomData("Buildable");
		}

		canBuild = canBuild && _grid.IsCellVacant(gridCoords);

		// Visual feedback: Green if valid, Red if blocked
		_previewTower.Modulate = canBuild ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
		
		if (Input.IsActionJustPressed("Left_click"))
		{
			if (canBuild && _previewTower._cost <= _tdManager._money)
			{
				PlaceDraggingTower(gridCoords);
			}
			else
			{
				ExitPlacementMode();
			}
		}
		
	}

	// for automatically placing tower
	public void PlaceTower(Vector2I gridCoords, string towerName)
	{
		Vector2 position = _grid.ToGlobal(_grid.MapToLocal(gridCoords));
		TowerUnit newTower = (TowerUnit)_unitManager.SpawnUnit(position, 0, towerName, false, gridCoords);
		if (newTower is Spawner spawner)
		{
			_tdManager.Connect(TDManager.SignalName.NewWave, Callable.From(spawner.OnNewWave));
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
	}

	// for dragging tower from right panel to place
	private void PlaceDraggingTower(Vector2I gridCoords)
	{
		Vector2 position = _grid.ToGlobal(_grid.MapToLocal(gridCoords));
		TowerUnit newTower = (TowerUnit)_unitManager.SpawnUnit(position, 0, _towerToPlace, false, gridCoords);
		_tdManager.SpendMoneyOnTower(((TowerUnit)newTower)._cost);
		if (newTower is Spawner spawner)
		{
			_tdManager.Connect(TDManager.SignalName.NewWave, Callable.From(spawner.OnNewWave));
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
	}

	public void RemoveTower(Vector2I gridCoords)
	{
		if (_grid.IsCellVacant(gridCoords))
		{
			return;
		}
		TowerUnit tower = _grid.GetTowerOnCell(gridCoords);
		_allTowers.Remove(tower);
		tower.RemoveAllEffects();
		_grid.UnoccupyCell(gridCoords);
		tower.QueueFree();
	}

	public void TransformTower(Vector2I gridCoords, string newTower)
	{
		RemoveTower(gridCoords);
		PlaceTower(gridCoords, newTower);
	}

	public void UpdateIncomeDisplay()
	{
		Label incomeLabel = _rightPanel.GetNode<Label>("IncomeLabel");

		int income = 0;

		foreach (TowerUnit tower in _allTowers)
		{
			income += tower.GetIncome();
		}

		incomeLabel.Text = "Maximum Income: $" + income;
	}

	public List<TowerUnit> GetAllTowers()
	{
		return _allTowers;
	}
}
