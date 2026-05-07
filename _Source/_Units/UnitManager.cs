using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace RTSGame.Units;

public partial class UnitManager : Node2D
{
	private static Dictionary<string, PackedScene> UnitLibrary = new Dictionary<string, PackedScene>
	{
		{ "Shooter", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Shooter.tscn") },
		{ "Fighter", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Fighter.tscn") },
		{ "Turret", GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Units/Turret.tscn") },
	};

	static uint UnitLayerMask = 2;

	private UnitInfoPanel _unitInfoPanel;

	private bool _isDraggingLeft = false;
	private bool _isDraggingRight = false;
	private Vector2 _dragStart;
	private Vector2 _dragEnd;
	private Color _boxColor = new Color(0, 1, 0, 0.3f); // Transparent green
	private Color _borderColor = new Color(0, 1, 0, 0.7f);

	private List<Unit> _selectedUnits = new List<Unit>();
	private List<Unit> _activeUnits = new List<Unit>();

	private bool _isAMovePending = false;

	public Signal UnitDied;

	public override void _Ready()
	{
		for (int i = 0;  i < 3; i++)
		{
			RandomNumberGenerator rng = new();
			rng.Randomize();
			Vector2 jitter = new Vector2(rng.RandfRange(-20, 20), rng.RandfRange(-20, 20));
			SpawnUnit(new Vector2(100, 200) + jitter, 1, "Fighter");
		}
		for (int i = 0; i < 5; i++)
		{
			RandomNumberGenerator rng = new();
			rng.Randomize();
			Vector2 jitter = new Vector2(rng.RandfRange(-20, 20), rng.RandfRange(-20, 20));
			SpawnUnit(new Vector2(500, 200) + jitter, 0, "Shooter");
		}
		_unitInfoPanel = GetParent().GetNode<UnitInfoPanel>("UnitInfoPanel");
	}

	public override void _Input(InputEvent @event)
	{
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
		UpdateSelection([unit]);
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
			CollisionMask = UnitLayerMask,
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

	private void GiveMoveOrder(Vector2 destination)
	{
		foreach (Unit unit in _selectedUnits)
		{
			unit.ClearAllCommands();

			int rngRangeSize = _selectedUnits.Count * 5;

			// Simple spread logic so units don't overlap perfectly
			float offsetX = (float)GD.RandRange(-rngRangeSize, rngRangeSize);
			float offsetY = (float)GD.RandRange(-rngRangeSize, rngRangeSize);
			ForceMove forceMove = new ForceMove(unit, destination + new Vector2(offsetX, offsetY));
			unit.AddCommand(forceMove);
		}
	}

	private void GiveAttackMoveOrder(Vector2 destination)
	{
		foreach (Unit unit in _selectedUnits)
		{
			unit.ClearAllCommands();

			// Simple spread logic so units don't overlap perfectly
			float offsetX = (float)GD.RandRange(-20, 20);
			float offsetY = (float)GD.RandRange(-20, 20);

			AttackMove attackMove = new AttackMove(unit, destination + new Vector2(offsetX, offsetY));
			unit.AddCommand(attackMove);
		}
	}

	private void GiveStopOrder()
	{
		foreach (Unit unit in _selectedUnits)
		{
			unit.ClearAllCommands();
		}
	}

	private void GiveForceAttackOrder(Unit unit)
	{
		foreach (Unit unit_ in _selectedUnits)
		{
			unit_.ClearAllCommands();
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
		query.CollisionMask = 2; 

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

		UpdateSelection(foundUnits);
	}

	private void UpdateSelection(List<Unit> newSelection)
	{
		if (newSelection.Count == 0)
		{
			return;
		}

		// Deselect old units first
		foreach (var unit in _selectedUnits)
		{
			unit.SetSelectionVisible(false);
		}

		_selectedUnits = newSelection;

		// Select new units
		foreach (var unit in _selectedUnits)
		{
			unit.SetSelectionVisible(true);
		}

		_unitInfoPanel.UpdateSelectedUnits(newSelection);
	}

	public void SpawnUnit(Vector2 position, int teamId, string unitName)
	{
		if (UnitLibrary.TryGetValue(unitName, out PackedScene scene))
		{
			Unit newUnit = scene.Instantiate<Unit>();
			newUnit._teamId = teamId;

			newUnit.GlobalPosition = position;

			newUnit.Died += OnUnitDied;

			AddChild(newUnit);

			_activeUnits.Add(newUnit);

			GD.Print($"Spawned unit for team {teamId}");
		}
		else
		{
			GD.PrintErr($"Unit type {unitName} not found in library!");
		}
	}

	private void OnUnitDied(Unit deadUnit)
	{
		deadUnit.Died -= OnUnitDied;

		_activeUnits.Remove(deadUnit);
		if (_selectedUnits.Contains(deadUnit))
		{
			_selectedUnits.Remove(deadUnit);
			UpdateSelection(_selectedUnits);
		}
	}
}
