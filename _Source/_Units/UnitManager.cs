using Godot;
using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Formats.Asn1.AsnWriter;

namespace RTSGame.Units;

public partial class UnitManager : Node2D
{
	private static Dictionary<string, PackedScene> UnitLibrary = new Dictionary<string, PackedScene>
	{
		// test units
		{ "Shooter", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Shooter.tscn") },
		{ "Fighter", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Fighter.tscn") },
		{ "Turret", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Turret.tscn") },

		{ "Exit", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Exit.tscn") },

		// towers
		{ "GunTurret", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/GunTurret.tscn") },
		{ "LaserTurret", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/LaserTurret.tscn") },
		{ "PiercingTower", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/PiercingTower.tscn") },
		{ "FlameTurret", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/FlameTurret.tscn") },
		{ "DualGunTurret", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/DualGunTurret.tscn") },
		{ "ExplosiveGunTurret", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/ExplosiveGunTurret.tscn") },
		{ "MoneyTurret", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/MoneyTurret.tscn") },
		{ "GunScannerTurret", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/GunScannerTurret.tscn") },
		{ "VulnerableScannerTurret", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/VulnerableScannerTurret.tscn") },
		{ "StunPiercingTower", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/StunPiercingTower.tscn") },
		{ "TeslaTurret", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/TeslaTurret.tscn") },
		{ "TestTurret", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/TestTurret.tscn") },
		{ "BatteryLaserTurret", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/BatteryLaserTurret.tscn") },
		{ "InfinitePiercingTower", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/InfinitePiercingTower.tscn") },

		{ "SlimeSpawner", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/SlimeSpawner.tscn") },
		{ "HoundSpawner", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/HoundSpawner.tscn") },
		{ "PriestSpawner", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/PriestSpawner.tscn") },
		{ "YetiSpawner", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/YetiSpawner.tscn") },
		{ "SoldierSpawner", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/SoldierSpawner.tscn") },
		{ "UndeadSpawner", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/UndeadSpawner.tscn") },
		{ "LargeSlimeSpawner", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/LargeSlimeSpawner.tscn") },
		{ "BlinkerSpawner", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/BlinkerSpawner.tscn") },
		{ "DisablerSpawner", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/DisablerSpawner.tscn") },
		{ "SummonerSpawner", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/SummonerSpawner.tscn") },
		{ "SentrySpawner", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/SentrySpawner.tscn") },

		{ "DepoweredSpawner", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/DepoweredSpawner.tscn") },

		{ "PortalAmplifier", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/PortalAmplifier.tscn") },
		{ "Armory", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/Armory.tscn") },
		{ "Mine", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/Mine.tscn") },
		{ "DamageArmory", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/DamageArmory.tscn") },
		{ "Reactor", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/Reactor.tscn") },
		{ "Lab", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Towers/Lab.tscn") },

		// invaders
		{ "Slime", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Invaders/Slime.tscn") },
		{ "Undead", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Invaders/Undead.tscn") },
		{ "Hound", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Invaders/Hound.tscn") },
		{ "MegaSlime",GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Invaders/MegaSlime.tscn")},
		{ "Priest", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Invaders/Priest.tscn")},
		{ "Archbishop", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Invaders/Archbishop.tscn")},
		{ "Yeti", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Invaders/Yeti.tscn")},
		{ "Soldier", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Invaders/Soldier.tscn")},
		{ "BigArchbishop", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Invaders/BigArchbishop.tscn")},
		{ "Bonus", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Invaders/Bonus.tscn")},
		{ "LargeSlime", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Invaders/LargeSlime.tscn")},
		{ "Blinker", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Invaders/Blinker.tscn")},
		{ "Disabler", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Invaders/Disabler.tscn")},
		{ "Summoner", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Invaders/Summoner.tscn")},
		{ "Summoned", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Invaders/Summoned.tscn")},
		{ "Sentry", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Invaders/Sentry.tscn")},
	};

	public static uint UnitLayerMask = 2;
	public static uint TowerLayerMask = 4;

	public static Unit GetUnit(string unitName, bool setUnit) // if setUnit is false, then return unit has no _weapon, _hp, _startingEffect, etc.
	{
		if (UnitLibrary.TryGetValue(unitName, out PackedScene scene))
		{
			Unit newUnit = (Unit)scene.Instantiate<Unit>();
			if (setUnit)
			{
				newUnit.SetDisplayUnit();
			}
			return newUnit;
		}
		else
		{
			GD.PrintErr($"Unit type {unitName} not found in library!");
		}
		return null;
	}

	public static string InternalNameToName(string internalName)
	{
		Unit unit = UnitManager.GetUnit(internalName, false);
		string name = unit._name;
		unit.QueueFree();
		return name;
	}

	private UnitInfoPanel _unitInfoPanel;
	private TDTowerManager _towerManager;

	private bool _isDraggingLeft = false;
	private bool _isDraggingRight = false;
	private Vector2 _dragStart;
	private Vector2 _dragEnd;
	private Color _boxColor = new Color(0, 1, 0, 0.3f); // Transparent green
	private Color _borderColor = new Color(0, 1, 0, 0.7f);

	private List<Unit> _selectedUnits = new List<Unit>();
	private List<Unit> _activeUnits = new List<Unit>();

	private bool _isAMovePending = false;
	private bool _shiftMode = false;

	public Signal UnitDied;

	public override void _Ready()
	{
		//for (int i = 0;  i < 3; i++)
		//{
		//	RandomNumberGenerator rng = new();
		//	rng.Randomize();
		//	Vector2 jitter = new Vector2(rng.RandfRange(-20, 20), rng.RandfRange(-20, 20));
		//	SpawnUnit(new Vector2(100, 200) + jitter, 1, "Fighter");
		//}
		//for (int i = 0; i < 5; i++)
		//{
		//	RandomNumberGenerator rng = new();
		//	rng.Randomize();
		//	Vector2 jitter = new Vector2(rng.RandfRange(-20, 20), rng.RandfRange(-20, 20));
		//	SpawnUnit(new Vector2(500, 200) + jitter, 0, "Shooter");
		//}
		_unitInfoPanel = GetParent().GetNode<UnitInfoPanel>("UnitInfoPanel");
		_towerManager = GetParent().GetNode<TDTowerManager>("TowerManager");
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent_ && keyEvent_.Keycode == Key.Shift)
		{
			if (keyEvent_.Pressed)
			{
				_shiftMode = true;
			}
			else
			{
				_shiftMode = false;
			}
		}
		if (Input.IsActionJustPressed("attack_move_command"))
		{
			_isAMovePending = true;
			// Optional: Change cursor to a crosshair here
			Input.SetDefaultCursorShape(Input.CursorShape.Cross);
		}
		else if (_isAMovePending && @event is InputEventMouseButton mouseEvent_ && mouseEvent_.ButtonIndex == MouseButton.Left)
		{
			Vector2 targetPos = GetGlobalMousePosition();
			Unit unit = GetUnitUnderCursor();
			if (unit is not null)
			{
				GiveForceAttackOrder(unit);
			}
			else
			{
				GiveAttackMoveOrder(targetPos);
			}

			_isAMovePending = false; // Reset mode
			Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
			GetViewport().SetInputAsHandled();
		}
		else if ((@event is InputEventKey keyEvent && keyEvent.Pressed) || (@event is InputEventMouseButton mouseEvent__ && mouseEvent__.ButtonIndex == MouseButton.Right))
		{
			_isAMovePending = false;

			Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
		}

		if (Input.IsActionJustPressed("show_attack_range"))
		{
			ToggleAttackRange();
		}

		if (Input.IsActionJustPressed("stop_move_command"))
		{
			GiveStopOrder();
			// Optional: Change cursor to a crosshair here
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		// Right-click to move
		if (@event is InputEventMouseButton mouseEvent &&
				mouseEvent.ButtonIndex == MouseButton.Right)
		{
			if (mouseEvent.Pressed)
			{
				_isDraggingRight = true;
				_dragStart = GetGlobalMousePosition();
				_dragEnd = _dragStart;
				Vector2 targetPos = GetGlobalMousePosition();
				GiveMoveOrder(targetPos);
			}
			else if (_isDraggingRight)
			{
				_isDraggingRight = false;
			}
		}
		else if (@event is InputEventMouseButton mouseEvent_ && mouseEvent_.ButtonIndex == MouseButton.Left)
		{
			if (_towerManager._placementMode)
			{
				return;
			}
			if (mouseEvent_.Pressed)
			{
				_isDraggingLeft = true;
				_dragStart = GetGlobalMousePosition();
				_dragEnd = _dragStart;
				QueueRedraw();
			}
			else if (_isDraggingLeft)
			{
				_isDraggingLeft = false;
				if (_dragStart == _dragEnd)
				{
					SelectUnitUnderCursor();
				}
				SelectUnitsInBox();
				QueueRedraw(); // Clear the box after release
			}
		}
		else if (_isDraggingLeft && @event is InputEventMouseMotion)
		{
			if (_towerManager._placementMode)
			{
				return;
			}
			_dragEnd = GetGlobalMousePosition();
			QueueRedraw();
		}
		else if(_isDraggingRight && @event is InputEventMouseMotion)
		{
			Vector2 targetPos = GetGlobalMousePosition();
			GiveMoveOrder(targetPos);
		}
	}

	private void SelectUnitUnderCursor()
	{
		Unit unit = GetUnitUnderCursor();
		if (unit is null)
		{
			return;
		}
		UpdatePlayerSelection([unit]);
	}

	private Unit GetUnitUnderCursor()
	{
		Vector2 worldPosition = GetGlobalMousePosition();

		// 2. Access the DirectSpaceState2D for physics queries
		var spaceState = GetWorld2D().DirectSpaceState;

		// 3. Setup the point query parameters
		var query = new PhysicsPointQueryParameters2D
		{
			Position = worldPosition,
			CollisionMask = UnitLayerMask + TowerLayerMask,
			CollideWithBodies = true // Set to true if units use CharacterBody2D
		};

		// 4. Intersect the point. We only care about the top-most result (maxResults: 1)
		var results = spaceState.IntersectPoint(query, 1);

		if (results.Count == 1)
		{
			// The result is a Godot Dictionary
			var hitData = results[0];
			var hitObject = hitData["collider"].As<Node>();

			if (hitObject is Unit unit)
			{
				return unit;
			}
		}
		if (results.Count > 1)
		{
			throw new Exception("found multiple units").InnerException;
		}
		return null;
	}

	public void GiveMoveOrder(Vector2 destination)
	{
		foreach (Unit unit in _selectedUnits)
		{
			if (unit._aiControlled)
			{
				continue;
			}
			if (!_shiftMode)
			{
				unit.ClearAllCommands();
			}
			
			int rngRangeSize = _selectedUnits.Count * 5;

			// Simple spread logic so units don't overlap perfectly
			float offsetX = (float)GD.RandRange(-rngRangeSize, rngRangeSize);
			float offsetY = (float)GD.RandRange(-rngRangeSize, rngRangeSize);
			ForceMove forceMove = new ForceMove(unit, destination + new Vector2(offsetX, offsetY));
			unit.AddCommand(forceMove);
		}
	}

	public void GiveAttackMoveOrder(Vector2 destination)
	{
		foreach (Unit unit in _selectedUnits)
		{
			if (unit._aiControlled)
			{
				break;
			}
			if (!_shiftMode)
			{
				unit.ClearAllCommands();
			}

			// Simple spread logic so units don't overlap perfectly
			float offsetX = (float)GD.RandRange(-20, 20);
			float offsetY = (float)GD.RandRange(-20, 20);

			AttackMove attackMove = new AttackMove(unit, destination + new Vector2(offsetX, offsetY));
			unit.AddCommand(attackMove);
		}
	}

	public void GiveStopOrder()
	{
		foreach (Unit unit in _selectedUnits)
		{
			if (unit._aiControlled)
			{
				break;
			}
			unit.ClearAllCommands();
		}
	}

	public void GiveForceAttackOrder(Unit unit)
	{
		foreach (Unit unit_ in _selectedUnits)
		{
			if (unit_._aiControlled)
			{
				break;
			}
			if (!_shiftMode)
			{
				unit.ClearAllCommands();
			}
			ForceAttack forceAttack = new ForceAttack(unit_, unit);
			unit_.AddCommand(forceAttack);
		}
	}

	private void ToggleAttackRange()
	{
		// turn on if at least one is off. turn off if all are on. 
		bool anyOff = false;
		foreach (Unit unit in _selectedUnits)
		{
			if (!unit._displayAttackRange)
			{
				anyOff = true;
			}
		}

		if (anyOff)
		{
			foreach (Unit unit in _selectedUnits)
			{
				unit.DisplayAttackRange();
			}
		}
		else
		{
			foreach (Unit unit in _selectedUnits)
			{
				unit.HideAttackRange();
			}
		}
	}

	public override void _Draw()
	{
		if (_isDraggingLeft)
		{
			// Calculate the rect from start to end (handles dragging in any direction)
			Rect2 rect = new Rect2(_dragStart, _dragEnd - _dragStart).Abs();

			DrawRect(rect, _boxColor, true);           // The fill
			DrawRect(rect, _borderColor, false, 2.0f); // The outline
		}
	}

	private void SelectUnitsInBox()
	{
		// 1. Create a rectangle shape for the query
		Rect2 selectionRect = new Rect2(_dragStart, _dragEnd - _dragStart).Abs();

		// Safety check for tiny clicks
		if (selectionRect.Size.X < 5 && selectionRect.Size.Y < 5) return;

		// 2. Prepare the physics query
		var query = new PhysicsShapeQueryParameters2D();
		var shape = new RectangleShape2D();
		shape.Size = selectionRect.Size;

		query.Shape = shape;
		query.Transform = new Transform2D(0, selectionRect.Position + (selectionRect.Size / 2));

		// 3. Optional: Only look for units on a specific physics layer (e.g., Layer 2)
		query.CollisionMask = UnitLayerMask + TowerLayerMask; 

		var spaceState = GetWorld2D().DirectSpaceState;
		var results = spaceState.IntersectShape(query);

		// 4. Handle results
		List<Unit> foundUnits = new List<Unit>();
		foreach (var result in results)
		{
			if (result["collider"].Obj is Unit unit)
			{
				foundUnits.Add(unit);
			}
		}

		UpdatePlayerSelection(foundUnits);
	}

	public void UpdatePlayerSelection(List<Unit> newSelection)
	{

		// Deselect old units first
		foreach (var unit in _selectedUnits)
		{
			if (IsInstanceValid(unit))
			{
				unit.SetSelectionVisible(false);
			}
		}

		_selectedUnits = newSelection;

		// Select new units
		foreach (var unit in _selectedUnits)
		{
			unit.SetSelectionVisible(true);
		}

		_unitInfoPanel.UpdateSelectedUnits(newSelection);
	}

	public Unit SpawnUnit(Vector2 position, int teamId, string unitName, bool aiControlled = true, Vector2I? gridLocation = null, bool hasEffects = true)
	{
		if (UnitLibrary.TryGetValue(unitName, out PackedScene scene))
		{
			Unit newUnit = scene.Instantiate<Unit>();
			newUnit._teamId = teamId;

			newUnit.GlobalPosition = position;

			newUnit._aiControlled = aiControlled;

			newUnit.Died += () => OnUnitDied(newUnit);
			newUnit.Removed += () => OnUnitRemoved(newUnit);

			if (gridLocation is not null && newUnit is TowerUnit tower)
			{
				tower._gridLocation = (Vector2I)gridLocation;
			}

			newUnit._hasEffects = hasEffects;

			GetParent().AddChild(newUnit);

			_activeUnits.Add(newUnit);

			return newUnit;
		}
		else
		{
			GD.PrintErr($"Unit type {unitName} not found in library!");
		}
		return null;
	}

	private void OnUnitDied(Unit deadUnit)
	{
		_activeUnits.Remove(deadUnit);
		if (_selectedUnits.Contains(deadUnit))
		{
			_selectedUnits.Remove(deadUnit);
			UpdatePlayerSelection(_selectedUnits);
		}
	}

	private void OnUnitRemoved(Unit removedUnit)
	{
		_activeUnits.Remove(removedUnit);
		if (_selectedUnits.Contains(removedUnit))
		{
			_selectedUnits.Remove(removedUnit);
			UpdatePlayerSelection(_selectedUnits);
		}
	}
}
