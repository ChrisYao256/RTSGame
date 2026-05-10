using Godot;
using Godot.Collections;
using RTSGame.Source;
using System;

namespace RTSGame.Units;

public partial class Spawner : TowerUnit
{
	[Export]
	public Array<string> _units = new Array<string>();

	[Export]
	public float _spawnAreaRadius = 50f;

	private TDManager _tdManager;
	private Grid _grid;
	private Area2D _spawnArea;

	public override void _Ready()
	{
		base._Ready();
		_tdManager = GetTree().CurrentScene.GetNode<TDManager>("TdManager");
		_spawnArea = GetNode<Area2D>("AttackArea");
		_grid = GetTree().CurrentScene.GetNode<Grid>("TileMapLayer");
	}

	protected override void SetAttackRange()
	{
		_attackCollisionShape = GetNode<CollisionShape2D>("AttackArea/AttackAreaCollision");
		_attackRange = _spawnAreaRadius;

		// CRITICAL: Make the shape unique so changing this unit 
		// doesn't change every other unit of the same type.
		if (_attackCollisionShape.Shape is CircleShape2D circle)
		{
			circle = (CircleShape2D)circle.Duplicate();
			circle.Radius = _attackRange;
			_attackCollisionShape.Shape = circle;
		}
		else
		{
			throw new Exception("Attack area shape is not a disk");
		}
	}

	public void SpawnWave()
	{
		foreach (string unit in _units)
		{
			int maxAttempts = 50; // Safety cap to prevent infinite loops

			for (int i = 0; i < maxAttempts; i++)
			{
				Vector2 position = GetRandomPositionInCircle(_spawnArea);
				Vector2I tilePos = _grid.LocalToMap(_grid.ToLocal(position));

				// 3. Check if this tile is a "path"
				// Option A: Check a custom data layer in TileSet
				TileData data = _grid.GetCellTileData(tilePos);
				if (data != null && (bool)data.GetCustomData("Path"))
				{
					_tdManager.SpawnEnemyFromTower(unit, position);
					break;
				}
			}
			GD.Print("No valid position found!");
		}
	}

	public static Vector2 GetRandomPositionInCircle(Area2D area)
	{
		var collisionShape = area.GetNode<CollisionShape2D>("AttackAreaCollision");
		var circleShape = (CircleShape2D)collisionShape.Shape;
		float radius = circleShape.Radius;

		// 1. Get a random angle (0 to 2*PI)
		float angle = (float)GD.RandRange(0, Mathf.Tau);

		// 2. Get a random distance (Square root ensures even distribution)
		float r = radius * Mathf.Sqrt((float)GD.RandRange(0, 1));

		// 3. Convert Polar to Cartesian coordinates
		Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;

		return collisionShape.GlobalPosition + offset;
	}

	public void OnNewWave()
	{
		SpawnWave();
	}
}

