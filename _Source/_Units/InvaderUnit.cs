using Godot;
using RTSGame.Source;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class InvaderUnit : Unit
{
	[Export]
	private Vector4I _moneyDropped;
	[Export] 
	private PackedScene _floatingTextScene;
	[Export]
	public int _hpDeducted = 1;
	[Export]
	public Vector4I _moneyDeducted = new Vector4I();

	/// <summary>
	/// Write level stat gains here. Index n means increasing from n to n+1. 
	/// </summary>
	[Export]
	public Array<InvaderStatsIncreaseResource> _levelStats = [];

	public int _level;

	public float _moneyTempModifierDMM;

	public float _moneyTempModifierDMMOK;

	private Vector2 _pathOffset;
	private Array<Vector2> _pathToExit;

	public override void _Ready()
	{
		base._Ready();
		_aiControlled = true;
		_tdManager = GetTree().CurrentScene.GetNode<TDManager>("TdManager");
	}

	public override void ProcessNextCommand()
	{
		Command command = new NoCommand(this);
		if (_commandQueue.Count > 0)
		{
			command = _commandQueue[0];
			_commandQueue.RemoveAt(0);
		}
		else if (_pathToExit.Count > 0)
		{
			command = new AttackMove(this, _pathToExit[0]);
			_pathToExit.RemoveAt(0);
		}
		else
		{
			_currentCommand = command;
		}
		if (command is AttackMove attackMove)
		{
			ProcessAttackMove(attackMove);
		}
		else if (command is ForceMove forceMove)
		{
			ProcessForceMove(forceMove);
		}
		else if (command is ForceAttack forceAttack)
		{
			ProcessForceAttack(forceAttack);
		}
		else if (command is AggroedAttackMove aggroedAttackMove)
		{
			ProcessAggroedAttackMove(aggroedAttackMove);
		}
		else if (command is NoCommand noCommand)
		{
			ScanForEnemies();
		}
	}

	public void SetRandomPathOffset()
	{
		_pathOffset = Grid.GetRandomOffset();
	}

	public void SetPathOffset(Vector2 offset)
	{
		_pathOffset = Grid.ClampOffset(offset);
	}

	public void SetPathToExit(Array<Vector2> path)
	{
		for (int i = 0; i < path.Count; i++)
		{
			path[i] += _pathOffset;
		}
		_pathToExit = path;
	}

	public void IncreaseLevel(int n)
	{	
		if (n <= 0)
		{
			return;
		}
		for (int i = 0; i < n; i++)
		{
			if (_levelStats.Count <= _level + i)
			{
				break;
			}
			
			AddEffect(_levelStats[_level + i]);
		}
		_level += n;
	}

	public InvaderStatsIncreaseResource GetIncreaseLevelData(int n)
	{
		InvaderStatsIncreaseResource totalStats = new InvaderStatsIncreaseResource();
		if (n <= 0)
		{
			return totalStats;
		}
		for (int i = 0; i < n; i++)
		{
			if (_levelStats.Count <= i + _level)
			{
				break;
			}
			_levelStats[i + _level].MergeWithOld(totalStats, []);
		}
		return totalStats;
	}

	public void SetMoneyModifier(Vector4I money)
	{
		_data._moneyIncrease = money;
	}

	public void IncreaseMoneyModifier(Vector4I change)
	{
		_data._moneyIncrease += change;
	}

	public void SetPercentMoneyModifier(float increase)
	{
		_data._percentMoneyIncrease = increase;
	}

	public void IncreasePercentMoneyModifier(float change)
	{
		_data._percentMoneyIncrease += change;
	}

	public void SetMoneyTempModifierDMM(float percentIncrease)
	{
		_moneyTempModifierDMM = percentIncrease;
	}

	public void SetMoneyTempModifierDMMOK(float percentIncrease)
	{
		_moneyTempModifierDMMOK = percentIncrease;
	}

	protected override void Die()
	{
		if (_floatingTextScene != null && GetSelfMoneyDropped() != new Vector4I(0, 0, 0, 0))
		{
			_currentFloatingAnimationCount++;

			var textNode = _floatingTextScene.Instantiate<FloatingText>();
			textNode.BbcodeEnabled = true;
			textNode.FitContent = true;

			if (_moneyTempModifierDMM != 0 || _moneyTempModifierDMMOK != 0)
			{
				textNode.Text = "+" + Utils.MakeMoneyText(GetNormalMoneyDropped()) + "\n+bonus " + Utils.MakeMoneyText(GetBonusMoneyDropped());
			}
			else
			{
				textNode.Text = "+" + Utils.MakeMoneyText(GetNormalMoneyDropped());
			}


			// Set the position to the unit's current global position
			textNode.GlobalPosition = GlobalPosition + new Vector2(0, 30) * (_currentFloatingAnimationCount - 1);

			// VERY IMPORTANT: Add it to the world, not the unit!
			// If you add it to the unit, it will disappear instantly when the unit is freed.
			GetTree().Root.AddChild(textNode);

			Timer timer = new();
			timer.Timeout += () =>
			{
				_currentFloatingAnimationCount--;
				timer.QueueFree();
			};
			timer.OneShot = true;
			GetTree().Root.AddChild(timer);
			timer.Start(1f);

			textNode.StartFloatingAnimation();
		}
		base.Die();
	}


	public void Exit()
	{
		if (_floatingTextScene == null) return;

		var textNode = _floatingTextScene.Instantiate<FloatingText>();
		textNode.BbcodeEnabled = true;
		textNode.FitContent = true;

		// Set the text
		textNode.Text = Utils.MakeMoneyText(_moneyDeducted) + $"\n -{_hpDeducted} hp";

		textNode.GlobalPosition = GlobalPosition;

		GetTree().CurrentScene.AddChild(textNode);

		textNode.StartFloatingAnimation();

		RemoveSelf();
	}

	/// <summary>
	/// Edits _infoContainers so that upgraded stats are displayed in green. Note that this method should be called on a unit with the upgrade already applied on. 
	/// The upgrade resource that is inputted is only used to determine which stats were changed. 
	/// </summary>
	/// <param name="upgrade"></param>
	/// <exception cref="Exception"></exception>
	public void UpdateUnitInfoContainerWithUpgrade(InvaderStatsIncreaseResource upgrade)
	{
		if (this is not InvaderUnit)
		{
			throw new Exception("attempting to give invader buff to non-invader");
		}
		InvaderStatsIncreaseResource totalUpgrade = (InvaderStatsIncreaseResource)upgrade.DuplicateDeep();

		// construct totalUpgrade which includes both buffs from upgrade itself and buffs from level. (totalUpgrade is only used to determine which texts should be green)
		_level -= upgrade._level;
		GetIncreaseLevelData(upgrade._level).MergeWithOld(totalUpgrade, []);
		_level += upgrade._level;

		PanelContainer basicInfo = _infoContainers["BasicInfo"];
		basicInfo.CustomMinimumSize = new(200, 0);
		VBoxContainer basicInfoV = basicInfo.GetNode<VBoxContainer>("VBoxContainer");
		string greenHex = ThemePalette.Green.ToHtml(false);

		if (totalUpgrade._hpBuff != 0)
		{
			RichTextLabel hpLabel = basicInfoV.GetNode<RichTextLabel>("HpLabel");
			hpLabel.Text = $"[color=#{greenHex}]Hp: {GetHpMax()}/{GetHpMax()}[/color]";
		}

		if (totalUpgrade._speedBuff != 0)
		{
			RichTextLabel speedLabel = basicInfoV.GetNode<RichTextLabel>("SpeedLabel");
			speedLabel.Text = $"[color=#{greenHex}]Move speed: {GetSpeed()}[/color]";
		}

		if (totalUpgrade._moneyBuff != new Vector4I(0, 0, 0, 0) || upgrade._startingEffects.Any(o => o.GetType() == typeof(SpawnUnitOnDeathResource)))
		{
			InvaderUnit invader = (InvaderUnit)this;
			TooltipRichTextLabel moneyDropLabel = basicInfoV.GetNode<TooltipRichTextLabel>("MoneyDropLabel");
			moneyDropLabel.Text = $"[color=#{greenHex}]Drops {Utils.MakeMoneyText(invader.GetTotalMoneyDropped())}[/color]";
		}

		if (totalUpgrade._startingEffects.Count != 0)
		{
			Array<EffectResource> effectUpgrades = [];
			foreach (EffectResource resource in totalUpgrade._startingEffects)
			{
				if (_effects.Any(e => e.GetType() == resource.GetType()))
				{
					effectUpgrades.Add(resource);
				}
			}
			if (effectUpgrades.Count == 0)
			{
				return;
			}
			PanelContainer effectsInfo = _infoContainers["EffectsInfo"];
			foreach (var child in effectsInfo.GetChildren())
			{
				child.QueueFree();
			}

			HBoxContainer allEffectsH = new();

			VBoxContainer smallEffectsV = new();

			HBoxContainer largeEffectsH = new();

			foreach (EffectResource effect in _effects)
			{
				switch (effect._displayType)
				{
					case (EffectResource.DisplayTypes.Large):
						if (effectUpgrades.Any(o => o.GetType() == effect.GetType()))
						{
							EffectResource newEffect = effectUpgrades.First(o => o.GetType() == effect.GetType());
							VBoxContainer container = new();
							PanelContainer effectName = effect.MakeFullEffectDescriptionWithUpgrade(newEffect, false);
							container.AddChild(effectName);
							largeEffectsH.AddChild(container);
							break;
						}
						else
						{
							VBoxContainer container = new();
							PanelContainer effectName = effect.MakeFullEffectDescription();
							container.AddChild(effectName);
							largeEffectsH.AddChild(container);
							break;
						}

					case (EffectResource.DisplayTypes.Small):
						VBoxContainer container1 = new();
						HoverInfoLabel effectName1 = effect.MakeEffectTooltip(false);
						container1.AddChild(effectName1);
						smallEffectsV.AddChild(container1);
						break;
					case (EffectResource.DisplayTypes.Hidden):
						continue;
				}
			}

			if (largeEffectsH.GetChildren().Count != 0)
			{
				allEffectsH.AddChild(largeEffectsH);
			}
			else
			{
				largeEffectsH.QueueFree();
			}
			if (smallEffectsV.GetChildren().Count != 0)
			{
				allEffectsH.AddChild(smallEffectsV);
			}
			else
			{
				smallEffectsV.QueueFree();
			}
			effectsInfo.AddChild(allEffectsH);
		}
	}

	/// <summary>
	/// Returns the money that will be dropped if this unit dies now. Used to actually award money for kills. Is equivalent ot GetNormalMoneyDropped() if the unit is not added to TD. 
	/// </summary>
	/// <returns></returns>
	public Vector4I GetSelfMoneyDropped()
	{
		return Utils.VectorScalarMultiplication((_moneyDropped + _data._moneyIncrease), (1f + _data._percentMoneyIncrease) * (1f + _moneyTempModifierDMM) * (1f + _moneyTempModifierDMMOK));
	}

	/// <summary>
	/// Returns the money that will be dropped if this unit and units that it spawns die. Used to calculate total income for spawner towers. 
	/// </summary>
	/// <returns></returns>
	public Vector4I GetTotalMoneyDropped()
	{
		if (!_effects.Any(o => o.GetType() == typeof(SpawnUnitOnDeathResource)))
		{
			return GetSelfMoneyDropped();
		}
		else
		{
			Vector4I moneyFromSpawns = new(0,0,0,0);
			SpawnUnitOnDeathResource spawnEffectResource = (SpawnUnitOnDeathResource)_effects.First(o => o.GetType() == typeof(SpawnUnitOnDeathResource));
			foreach (InvaderStatsIncreaseResource unit in spawnEffectResource._spawnedUnits)
			{
				InvaderUnit invader = unit.GetInvader();
				moneyFromSpawns += invader.GetTotalMoneyDropped();
				invader.QueueFree();
			}
			return GetSelfMoneyDropped() + moneyFromSpawns;
		}
	}

	/// <summary>
	/// Returns the money that should be dropped without temporary modifiers such as Analyzed. Used to display bonus floating text when the unit dies. 
	/// </summary>
	/// <returns></returns>
	public Vector4I GetNormalMoneyDropped()
	{
		return Utils.VectorScalarMultiplication((_moneyDropped + _data._moneyIncrease), (1f + _data._percentMoneyIncrease));
	}

	public Vector4I GetBonusMoneyDropped()
	{
		return Utils.VectorScalarMultiplication(GetNormalMoneyDropped(), (1f + _moneyTempModifierDMMOK) * (1f + _moneyTempModifierDMM) - 1f);
	}

	public override string GetName()
	{
		return base.GetName() + $" Lv {_level}";
	}

	public Array<Vector2> GetPathToExit() => _pathToExit;

	public float GetDistanceToExit()
	{
		if (_pathToExit is null)
		{
			return 9999;
		}
		float distance = 0;
		Vector2 oldPos = GlobalPosition;
		foreach (Vector2 pos in _pathToExit)
		{
			distance += pos.DistanceTo(oldPos);
			oldPos = pos;
		}
		return distance;
	}
}

