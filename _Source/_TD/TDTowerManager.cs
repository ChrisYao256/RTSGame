using Godot;
using RTSGame.Units;
using System;
using System.Collections.Generic;

namespace RTSGame.Source;

public partial class TDTowerManager : Node2D
{
	private TDManager _tdManager;
	private UnitManager _unitManager;
	private Panel _rightPanel;
	private VBoxContainer _towersBox;
	private Grid _grid;

	private List<string> _towers;

	private bool _placementMode = false;
	private string _towerToPlace;
	private TowerUnit _previewTower;

	public override void _Ready()
	{
		_rightPanel = GetParent().GetNode<Panel>("RightPanel");
		_towersBox = _rightPanel.GetNode<VBoxContainer>("Towers");
		_grid = GetParent().GetNode<Grid>("TileMapLayer");
		_tdManager = GetParent().GetNode<TDManager>("TdManager");
	}

	public override void _Process(double delta)
	{
		if (_placementMode)
		{
			UpdatePlacement();
		}
	}

	public void InitializeTowersPanel(List<string> towers, UnitManager unitManager)
	{
		_unitManager = unitManager;
		_towers = towers;
		foreach (string name in towers)
		{
			VBoxContainer container = new VBoxContainer();

			string name_ = name;
			TowerUnit unit = (TowerUnit)_unitManager.GetUnit(name);

			Label nameLabel = new Label();
			nameLabel.Text = unit._name;
			container.AddChild(nameLabel);

			HoverBoxTextureButton towerButton = new HoverBoxTextureButton();
			towerButton.TextureNormal = unit._iconTexture;
			towerButton.IgnoreTextureSize = true;
			towerButton.StretchMode = TextureButton.StretchModeEnum.KeepAspect;
			towerButton.CustomMinimumSize = new Vector2(48, 48);
			towerButton.Pressed += (()=> 
			{
				if (_tdManager._money >= unit._cost)
				{
					EnterPlacementMode(name_); 
				} 
			});

			Panel infoPanel = new Panel();

			StyleBoxFlat solidBox = new StyleBoxFlat();

			solidBox.BgColor = Colors.Black;

			solidBox.SetBorderWidthAll(0);
			infoPanel.AddThemeStyleboxOverride("panel", solidBox);

			Label towerDesc = new Label();
			towerDesc.Text = unit.GetDescription();
			towerDesc.Text += "\n" + "DPS: " + unit.GetDPS();
			//towerDesc.ForceUpdateTransform();
			infoPanel.Size = towerDesc.GetCombinedMinimumSize();
			infoPanel.ZIndex = 1;
			infoPanel.AddChild(towerDesc);

			towerButton._popUpBox = infoPanel;
			//towerButton.Initialize();

			container.AddChild(towerButton);

			Label costLabel = new Label();
			costLabel.Text = "$" + unit._cost.ToString();
			container.AddChild(costLabel);

			_towersBox.AddChild(container);

		}
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
				PlaceTower(gridCoords);
			}
			else
			{
				ExitPlacementMode();
			}
		}
		
	}

	private void PlaceTower(Vector2I gridCoords)
	{
		Vector2 position = _grid.ToGlobal(_grid.MapToLocal(gridCoords));
		Unit newTower = _unitManager.SpawnUnit(position, 0, _towerToPlace, false);
		_tdManager.SpendMoneyOnTower(((TowerUnit)newTower)._cost);
		if (newTower is Spawner spawner)
		{
			_tdManager.Connect(TDManager.SignalName.NewWave, Callable.From(spawner.OnNewWave));
		}
		_grid.OccupyCell(gridCoords);
	}
}
