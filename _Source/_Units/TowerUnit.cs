using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class TowerUnit : StationaryUnit
{
	[Export]
	public Vector4I _cost;

	[Export]
	public Texture2D _iconTexture;

	[Export]
	public TowerType _towerType;

	[Export]
	public string _description;

	[Export]
	public Array<EffectResource> _firstUpgrade;

	[Export]
	public Vector4I _firstUpgradeCost;

	
	public readonly string _firstUpgradeName = "Upgrade to Lv 2";

	[Export]
	public Array<EffectResource> _secondUpgrade;
	[Export]
	public Vector4I _secondUpgradeCost;
	
	public readonly string _secondUpgradeName = "Upgrade to Lv 3";

	[Export]
	public Array<EffectResource> _thirdUpgrade;
	[Export]
	public Vector4I _thirdUpgradeCost;

	public readonly string _thirdUpgradeName = "Upgrade to Lv 4";

	[Export]
	public Array<EffectResource> _fourthUpgradeA;
	[Export]
	public Texture2D _fourthUpgradeATexture;
	[Export]
	public Vector4I _fourthUpgradeACost;

	[Export]
	public Array<EffectResource> _fourthUpgradeB;
	[Export]
	public Texture2D _fourthUpgradeBTexture;
	[Export]
	public Vector4I _fourthUpgradeBCost;

	public readonly string _fourthUpgradeAName = "Upgrade to Lv 5";

	public readonly string _fourthUpgradeBName = "Upgrade to Lv 5";

	[Export]
	private Array<EffectResource> _transformEffects = [];

	public enum TargetPriority
	{
		First,
		Last,
		Closest,
		Strongest,
	}

	public enum TowerType
	{
		Defense,
		Support,
		Spawner
	}

	public const int PriorityCount = 4;

	private float _upgradeCostScaling = 1;

	public bool _hasFirstUpgrade = false;

	public bool _unlockedSecondUpgrade = true;
	public bool _hasSecondUpgrade = false;

	public bool _unlockedThirdUpgrade = true;
	public bool _hasThirdUpgrade = false;

	public bool _unlockedFourthUpgrade = true;
	public bool _hasFourthUpgrade = false;

	private float _iconSize = TDManager.TileSize / 4f;

	public TargetPriority _targetPriority;

	public TDManager _tdManager;
	public Grid _grid;
	public Vector2I _gridLocation;
	private Label _lvLabel;

	public override void _Ready()
	{
		_radius = TDManager.TileSize / 2f;
		_grid = GetTree().CurrentScene.GetNode<Grid>("TileMapLayer");
		_tdManager = GetTree().CurrentScene.GetNode<TDManager>("TdManager");
		base._Ready();
		CollisionLayer = UnitManager.TowerLayerMask;
		_aiControlled = false;
		_lvLabel = GetNode<Label>("Level");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_active)
		{
			return;
		}
		if (_state == State.Attacking)
		{
			// switch target if unit is died
			CheckTargetAlive();
			if (!IsTargetInRange(_attackTarget))
			{
				StopAttackingTarget();
				ScanForEnemies();
			}
		}

		if (_currentCommand is NoCommand)
		{
			ProcessNextCommand();
		}
	}

	protected override void SetStartingEffects()
	{
		if (!_hasEffects)
		{
			return;
		}
		base.SetStartingEffects();
		foreach (var effect in _transformEffects)
		{
			AddTransformEffect(effect);
		}
	}

	private void AddTransformEffect(EffectResource resource)
	{
		EffectResource resourceCopy = (EffectResource)resource.Duplicate();
		Effect node = EffectManager.Apply(resourceCopy, _effectsNode);
		node.ConnectSignals(this);
	}

	public override void SetSize()
	{
		base.SetSize();
		if (HasNode("TurretTurner"))
		{
			TurretTurner turret = GetNode<TurretTurner>("TurretTurner"); 
			AnimatedSprite2D sprite = turret.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
			Utils.ScaleVisualToRadius(sprite, _radius);
		}
	}

	protected override List<Unit> FormTargetOrder(Array<Unit> bodies)
	{
		List<InvaderUnit> sortedBodies = new List<InvaderUnit>();
		List<InvaderUnit> invaders = bodies.OfType<InvaderUnit>().ToList();
		switch (_targetPriority)
		{
			case (TargetPriority.First):
				sortedBodies = invaders.OrderBy(body => body.GetDistanceToExit()).ToList();
				break;
			case (TargetPriority.Last):
				sortedBodies = invaders.OrderBy(body => -body.GetDistanceToExit()).ToList();
				break;
				break;
			case (TargetPriority.Closest):
				sortedBodies = invaders.OrderBy(body => GlobalPosition.DistanceSquaredTo(body.GlobalPosition)).ToList();
				break;
			case (TargetPriority.Strongest):
				sortedBodies = invaders.OrderBy(body => -body.GetHpMax()).ToList();
				break;
			default:
				throw new Exception("Unknown TargetPriority");
		}
		return sortedBodies.Cast<Unit>().ToList();
	}

	public void SetTargetPriority(TargetPriority priority)
	{
		_targetPriority = priority;
		EmitSignal(SignalName.UpdateInfo);
	}

	public void LastTargetPriority()
	{
		int newPriority = Utils.Mod(((int)_targetPriority - 1), PriorityCount);
		SetTargetPriority((TargetPriority)newPriority);
	}

	public void NextTargetPriority()
	{
		int newPriority = Utils.Mod(((int)_targetPriority + 1), PriorityCount);
		SetTargetPriority((TargetPriority)newPriority);
	}

	public void UnlockSecondUpgrades()
	{
		_unlockedSecondUpgrade = true;
		EmitSignal(SignalName.UpdateInfo);
	}

	public void UnlockThirdUpgrade()
	{
		_unlockedThirdUpgrade = true;
		EmitSignal(SignalName.UpdateInfo);
	}

	public void UnlockFourthUpgrade()
	{
		_unlockedFourthUpgrade = true;
		EmitSignal(SignalName.UpdateInfo);
	}

	public void UpgradeFirst()
	{
		_hasFirstUpgrade = true;
		foreach (var effect in _firstUpgrade)
		{
			AddEffect(effect);
		}
		_lvLabel.Text = "Lv 2";
	}

	public void UpgradeSecond()
	{
		_hasSecondUpgrade = true;
		foreach (var effect in _secondUpgrade)
		{
			AddEffect(effect);
		}
		_lvLabel.Text = "Lv 3";
	}

	public void UpgradeThird()
	{
		_hasThirdUpgrade = true;
		foreach (var effect in _thirdUpgrade)
		{
			AddEffect(effect);
		}
		_lvLabel.Text = "Lv 4";
	}

	public void UpgradeFourthA()
	{
		_hasFourthUpgrade = true;
		foreach (var effect in _fourthUpgradeA)
		{
			AddEffect(effect);
		}
	}

	public void UpgradeFourthB()
	{
		_hasFourthUpgrade = true;
		foreach (var effect in _fourthUpgradeB)
		{
			AddEffect(effect);
		}
	}

	public virtual HoverInfoImage MakeTowerTooltip(bool clickable)
	{
		HoverInfoImage trigger = new HoverInfoImage();
		trigger.TextureNormal = GetIconTexture();
		trigger.TextureFilter = TextureFilterEnum.Nearest;

		// Buttons use "Flat" mode if you want them to look like your old labels
		// or you can leave it off for a standard button look

		// Create the solid background for the Button
		StyleBoxFlat btnStyle = new StyleBoxFlat();
		if (!clickable)
		{
			btnStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 1.0f); // Solid
		}
		else
		{
			btnStyle.BgColor = new Color(0.2f, 0.5f, 0.2f, 1.0f); // Solid
		}
		btnStyle.SetContentMarginAll(6);
		btnStyle.CornerRadiusTopLeft = 3;
		btnStyle.CornerRadiusBottomLeft = 3;
		btnStyle.CornerRadiusTopRight = 3;
		btnStyle.CornerRadiusBottomRight = 3;

		// Apply to multiple states so it stays solid when clicked
		trigger.AddThemeStyleboxOverride("normal", btnStyle);
		trigger.AddThemeStyleboxOverride("hover", btnStyle);
		trigger.AddThemeStyleboxOverride("pressed", btnStyle);

		PanelContainer popup = new PanelContainer();
		popup.MouseFilter = Control.MouseFilterEnum.Ignore;
		popup.ZIndex = 100;
		popup.TopLevel = true; // Essential to avoid parent clipping
		popup.Visible = false;

		HBoxContainer popupH = new();

		if (_towerType == TowerType.Defense)
		{
			popupH.AddChild(MakeUnitInfoContainer()["BasicInfo"]);
			popupH.AddChild(MakeUnitInfoContainer()["WeaponInfo"]);
		}
		else if (_towerType == TowerType.Spawner)
		{
			popupH.AddChild(MakeUnitInfoContainer()["MoneyInfo"]);
			popupH.AddChild(MakeUnitInfoContainer()["SpawnedUnitInfo"]);
		}
		popup.AddChild(popupH);
		trigger._popupBox = popup;

		Utils.ScaleTextureButtonToRadius(trigger, _iconSize);

		return trigger;
	}

	public TextureRect MakeTowerIconBackground()
	{
		TextureRect background = new TextureRect();
		background.Texture = TDTowerManager.TowerBackgroundTexture;
		Utils.ScaleVisualToRadius(background, _iconSize);
		return background;
	}

	public override Godot.Collections.Dictionary<string, PanelContainer> MakeUnitInfoContainer()
	{
		base.MakeUnitInfoContainer();

		PanelContainer moneyInfo = new();
		moneyInfo.CustomMinimumSize = new (200, 0);

		VBoxContainer moneyInfoV = new();
		moneyInfoV.Name = "VBoxContainer";
		_infoContainers.Add("MoneyInfo", moneyInfo);

		if (GetTotalCost() != new Vector4I(0, 0, 0, 0))
		{
			RichTextLabel costLabel = new();
			costLabel.Text = "Total Cost: \n" + Utils.MakeMoneyText(GetTotalCost());
			costLabel.Name = "CostLabel";
			costLabel.CustomMinimumSize = new(200, 0);
			costLabel.BbcodeEnabled = true;
			costLabel.FitContent = true;
			moneyInfoV.AddChild(costLabel);
		}


		if (this is Spawner spawner && spawner._data._units.Count() > 0)
		{
			RichTextLabel spawnLabel = new();
			spawnLabel.Text = "Spawns " + spawner.GetSpawns();
			spawnLabel.Name = "SpawnLabel";
			spawnLabel.CustomMinimumSize = new(200, 0);
			spawnLabel.BbcodeEnabled = true;
			spawnLabel.FitContent = true;
			moneyInfoV.AddChild(spawnLabel);

			PanelContainer spawnedUnitTotalInfo = new();
			VBoxContainer spawnedUnitTotalInfoV = new();

			Label spawnedUnitLabel = new();
			spawnedUnitLabel.Text = "Spawned Unit:";
			spawnedUnitTotalInfoV.AddChild(spawnedUnitLabel);

			Unit spawnedUnit = UnitManager.GetUnit(spawner._data._units[0]);
			spawnedUnit._hpMaxModifier += spawner._data._hpBuff;
			spawnedUnit.IncreaseSpeedModifier(spawner._data._speedBuff);
			((InvaderUnit)spawnedUnit).IncreaseMoneyModifier(spawner._data._moneyBuff);
			PanelContainer spawnedUnitInfo = spawnedUnit.MakeUnitInfoContainer()["BasicInfo"];
			spawnedUnitTotalInfoV.AddChild(spawnedUnitInfo);

			spawnedUnitTotalInfo.AddChild(spawnedUnitTotalInfoV);

			_infoContainers.Add("SpawnedUnitInfo", spawnedUnitTotalInfo);
		}

		if (GetIncome() != new Vector4I(0,0,0,0))
		{
			RichTextLabel incomeLabel = new();
			incomeLabel.Text = "Maximum Income: \n" + Utils.MakeMoneyText(GetIncome());
			incomeLabel.Name = "IncomeLabel";
			incomeLabel.CustomMinimumSize = new(200, 0);
			incomeLabel.BbcodeEnabled = true;
			incomeLabel.FitContent = true;
			moneyInfoV.AddChild(incomeLabel);
		}

		moneyInfo.AddChild(moneyInfoV);

		if (_weapon != null)
		{
			PanelContainer attackPriority = new();
			attackPriority.CustomMinimumSize = new(100, 0);
			VBoxContainer attackPriorityV = new VBoxContainer();
			attackPriorityV.Name = "VBoxContainer";
			attackPriorityV.Alignment = BoxContainer.AlignmentMode.Center;

			Button upPriorityButton = new();
			upPriorityButton.Text = "△";
			upPriorityButton.Pressed += () => LastTargetPriority();
			attackPriorityV.AddChild(upPriorityButton);

			Label priorityLabel = new();
			priorityLabel.HorizontalAlignment = HorizontalAlignment.Center;
			switch (_targetPriority)
			{
				case TowerUnit.TargetPriority.First:
					priorityLabel.Text = "First";
					break;
				case TowerUnit.TargetPriority.Last:
					priorityLabel.Text = "Last";
					break;
				case TowerUnit.TargetPriority.Closest:
					priorityLabel.Text = "Closest";
					break;
				case TowerUnit.TargetPriority.Strongest:
					priorityLabel.Text = "Strongest";
					break;
				default:
					priorityLabel.Text = ((int)_targetPriority).ToString();
					break;
			}
			priorityLabel.Name = "AttackPriorityLabel";
			attackPriorityV.AddChild(priorityLabel);

			Button downPriorityButton = new();
			downPriorityButton.Text = "▽";
			downPriorityButton.Pressed += () => NextTargetPriority();
			attackPriorityV.AddChild(downPriorityButton);

			attackPriority.AddChild(attackPriorityV);
			_infoContainers.Add("AttackPriority", attackPriority);
		}
		

		PanelContainer upgrades = new();

		HBoxContainer upgradesH = new();
		upgradesH.Name = "HBoxContainer";

		if (!_hasFirstUpgrade)
		{
			if (_firstUpgrade is not null)
			{
				VBoxContainer upgrade = new VBoxContainer();
				RichTextLabel cost = new();
				cost.HorizontalAlignment = HorizontalAlignment.Center;
				cost.FitContent = true;
				cost.BbcodeEnabled = true;
				cost.Text = Utils.MakeMoneyText(_firstUpgradeCost);
				upgrade.AddChild(cost);

				foreach (EffectResource effect in _firstUpgrade)
				{
					effect.SetDescription();
				}

				Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, _firstUpgradeName, _firstUpgrade);
				upgradeButton.Pressed += (() =>
				{
					if (Utils.VectorLeq(_firstUpgradeCost, _tdManager._money))
					{
						_tdManager.SpendMoney(_firstUpgradeCost);
						UpgradeFirst();
					}
				});
				upgrade.AddChild(upgradeButton);
				upgradesH.AddChild(upgrade);
			}
		}
		else if ((!_hasSecondUpgrade) && _unlockedSecondUpgrade)
		{
			if (_secondUpgrade is not null)
			{
				VBoxContainer upgrade = new VBoxContainer();
				RichTextLabel cost = new();
				cost.HorizontalAlignment = HorizontalAlignment.Center;
				cost.FitContent = true;
				cost.BbcodeEnabled = true;
				cost.Text = Utils.MakeMoneyText(_secondUpgradeCost);
				upgrade.AddChild(cost);

				foreach (EffectResource effect in _secondUpgrade)
				{
					effect.SetDescription();
				}

				Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, _secondUpgradeName, _secondUpgrade);
				upgradeButton.Pressed += (() =>
				{
					if (Utils.VectorLeq(_secondUpgradeCost, _tdManager._money))
					{
						_tdManager.SpendMoney(_secondUpgradeCost);
						UpgradeSecond();
					}
				});
				upgrade.AddChild(upgradeButton);
				upgradesH.AddChild(upgrade);
			}
			
		}
		else if (!_hasThirdUpgrade && _unlockedThirdUpgrade)
		{
			if (_thirdUpgrade is not null)
			{
				VBoxContainer upgrade = new VBoxContainer();
				RichTextLabel cost = new();
				cost.HorizontalAlignment = HorizontalAlignment.Center;
				cost.FitContent = true;
				cost.BbcodeEnabled = true;
				cost.Text = Utils.MakeMoneyText(_thirdUpgradeCost);
				upgrade.AddChild(cost);

				foreach (EffectResource effect in _thirdUpgrade)
				{
					effect.SetDescription();
				}

				Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, _thirdUpgradeName, _thirdUpgrade);
				upgradeButton.Pressed += (() =>
				{
					if (Utils.VectorLeq(_thirdUpgradeCost, _tdManager._money))
					{
						_tdManager.SpendMoney(_thirdUpgradeCost);
						UpgradeThird();
					}

				});
				upgrade.AddChild(upgradeButton);
				upgradesH.AddChild(upgrade);
			}
			
		}
		else if (!_hasFourthUpgrade && _unlockedFourthUpgrade)
		{
			if (_fourthUpgradeA is not null && _fourthUpgradeA.Count != 0)
			{
				VBoxContainer upgrade = new VBoxContainer();
				RichTextLabel cost = new();
				cost.HorizontalAlignment = HorizontalAlignment.Center;
				cost.FitContent = true;
				cost.BbcodeEnabled = true;
				cost.Text = Utils.MakeMoneyText(_fourthUpgradeACost);
				upgrade.AddChild(cost);

				foreach (EffectResource effect in _fourthUpgradeA)
				{
					effect.SetDescription();
				}

				Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, _fourthUpgradeAName, _fourthUpgradeA);
				upgradeButton.Pressed += (() =>
				{
					if (Utils.VectorLeq(_fourthUpgradeACost, _tdManager._money))
					{
						_tdManager.SpendMoney(_fourthUpgradeACost);
						UpgradeFourthA();
					}

				});
				upgrade.AddChild(upgradeButton);

				TextureRect image = new();
				image.Texture = _fourthUpgradeATexture;
				image.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
				upgrade.AddChild(image);

				upgradesH.AddChild(upgrade);
			}
			if (_fourthUpgradeB is not null && _fourthUpgradeB.Count != 0)
			{
				VBoxContainer upgrade = new VBoxContainer();
				RichTextLabel cost = new();
				cost.HorizontalAlignment = HorizontalAlignment.Center;
				cost.FitContent = true;
				cost.BbcodeEnabled = true;
				cost.Text = Utils.MakeMoneyText(_fourthUpgradeBCost);
				upgrade.AddChild(cost);

				foreach (EffectResource effect in _fourthUpgradeB)
				{
					effect.SetDescription();
				}

				Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, _fourthUpgradeBName, _fourthUpgradeB);
				upgradeButton.Pressed += (() =>
				{
					if (Utils.VectorLeq(_fourthUpgradeBCost, _tdManager._money))
					{
						_tdManager.SpendMoney(_fourthUpgradeBCost);
						UpgradeFourthB();
					}

				});
				upgrade.AddChild(upgradeButton);

				TextureRect image = new();
				image.Texture = _fourthUpgradeBTexture;
				image.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
				upgrade.AddChild(image);

				upgradesH.AddChild(upgrade);
			}
		}

		upgrades.AddChild(upgradesH);

		_infoContainers.Add("Upgrades", upgrades);

		return _infoContainers;
	}

	public override void UpdateUnitInfoContainer(bool updateEffects)
	{
		base.UpdateUnitInfoContainer(updateEffects);

		VBoxContainer moneyInfoV = _infoContainers["MoneyInfo"].GetNode<VBoxContainer>("VBoxContainer");

		if (GetTotalCost() != new Vector4I(0, 0, 0, 0))
		{
			RichTextLabel costLabel = moneyInfoV.GetNode<RichTextLabel>("CostLabel");
			costLabel.Text = "Total Cost: \n" + Utils.MakeMoneyText(GetTotalCost());
		}

		if (this is Spawner spawner && spawner._data._units.Count() > 0)
		{
			RichTextLabel spawnLabel = moneyInfoV.GetNode<RichTextLabel>("SpawnLabel");
			spawnLabel.Text = "Spawns " + spawner.GetSpawns();
			if (updateEffects)
			{
				foreach (var child in _infoContainers["SpawnedUnitInfo"].GetChildren())
				{
					child.QueueFree();
				}
				VBoxContainer spawnedUnitTotalInfoV = new();

				Label spawnedUnitLabel = new();
				spawnedUnitLabel.Text = "Spawned Unit:";
				spawnedUnitTotalInfoV.AddChild(spawnedUnitLabel);

				Unit spawnedUnit = UnitManager.GetUnit(spawner._data._units[0]);
				spawnedUnit._hpMaxModifier += spawner._data._hpBuff;
				spawnedUnit.IncreaseSpeedModifier(spawner._data._speedBuff);
				((InvaderUnit)spawnedUnit).IncreaseMoneyModifier(spawner._data._moneyBuff);
				PanelContainer spawnedUnitInfo = spawnedUnit.MakeUnitInfoContainer()["BasicInfo"];
				spawnedUnitTotalInfoV.AddChild(spawnedUnitInfo);

				_infoContainers["SpawnedUnitInfo"].AddChild(spawnedUnitTotalInfoV);
			}
		}

		if (GetIncome() != new Vector4I(0, 0, 0, 0))
		{
			RichTextLabel incomeLabel = moneyInfoV.GetNode<RichTextLabel>("IncomeLabel");
			incomeLabel.Text = "Maximum Income: " + Utils.MakeMoneyText(GetIncome());
		}

		if (_weapon != null)
		{
			VBoxContainer attackPriorityV = _infoContainers["AttackPriority"].GetNode<VBoxContainer>("VBoxContainer");

			Label priorityLabel = attackPriorityV.GetNode<Label>("AttackPriorityLabel");
			switch (_targetPriority)
			{
				case TowerUnit.TargetPriority.First:
					priorityLabel.Text = "First";
					break;
				case TowerUnit.TargetPriority.Last:
					priorityLabel.Text = "Last";
					break;
				case TowerUnit.TargetPriority.Closest:
					priorityLabel.Text = "Closest";
					break;
				case TowerUnit.TargetPriority.Strongest:
					priorityLabel.Text = "Strongest";
					break;
				default:
					priorityLabel.Text = ((int)_targetPriority).ToString();
					break;
			}
		}

		if (updateEffects)
		{
			HBoxContainer upgradesH = _infoContainers["Upgrades"].GetNode<HBoxContainer>("HBoxContainer");

			foreach (var child in upgradesH.GetChildren())
			{
				child.QueueFree();
			}

			if (!_hasFirstUpgrade)
			{
				if (_firstUpgrade is not null)
				{
					VBoxContainer upgrade = new VBoxContainer();
					RichTextLabel cost = new();
					cost.FitContent = true;
					cost.BbcodeEnabled = true;
					cost.Text = Utils.MakeMoneyText(_firstUpgradeCost);
					upgrade.AddChild(cost);

					foreach (EffectResource effect in _firstUpgrade)
					{
						effect.SetDescription();
					}

					Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, _firstUpgradeName, _firstUpgrade);
					upgradeButton.Pressed += (() =>
					{
						if (Utils.VectorLeq(_firstUpgradeCost, _tdManager._money))
						{
							_tdManager.SpendMoney(_firstUpgradeCost);
							UpgradeFirst();
						}
					});
					upgrade.AddChild(upgradeButton);
					upgradesH.AddChild(upgrade);
				}
			}
			else if ((!_hasSecondUpgrade) && _unlockedSecondUpgrade)
			{
				if (_secondUpgrade is not null)
				{
					VBoxContainer upgrade = new VBoxContainer();
					RichTextLabel cost = new();
					cost.FitContent = true;
					cost.BbcodeEnabled = true;
					cost.Text = Utils.MakeMoneyText(_secondUpgradeCost);
					upgrade.AddChild(cost);

					foreach (EffectResource effect in _secondUpgrade)
					{
						effect.SetDescription();
					}

					Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, _secondUpgradeName, _secondUpgrade);
					upgradeButton.Pressed += (() =>
					{
						if (Utils.VectorLeq(_secondUpgradeCost, _tdManager._money))
						{
							_tdManager.SpendMoney(_secondUpgradeCost);
							UpgradeSecond();
						}
					});
					upgrade.AddChild(upgradeButton);
					upgradesH.AddChild(upgrade);
				}

			}
			else if (!_hasThirdUpgrade && _unlockedThirdUpgrade)
			{
				if (_thirdUpgrade is not null)
				{
					VBoxContainer upgrade = new VBoxContainer();
					RichTextLabel cost = new();
					cost.FitContent = true;
					cost.BbcodeEnabled = true;
					cost.Text = Utils.MakeMoneyText(_thirdUpgradeCost);
					upgrade.AddChild(cost);

					foreach (EffectResource effect in _thirdUpgrade)
					{
						effect.SetDescription();
					}

					Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, _thirdUpgradeName, _thirdUpgrade);
					upgradeButton.Pressed += (() =>
					{
						if (Utils.VectorLeq(_thirdUpgradeCost, _tdManager._money))
						{
							_tdManager.SpendMoney(_thirdUpgradeCost);
							UpgradeThird();
						}

					});
					upgrade.AddChild(upgradeButton);
					upgradesH.AddChild(upgrade);
				}

			}
			else if (!_hasFourthUpgrade && _unlockedFourthUpgrade)
			{
				if (_fourthUpgradeA is not null && _fourthUpgradeA.Count != 0)
				{
					VBoxContainer upgrade = new VBoxContainer();
					RichTextLabel cost = new();
					cost.FitContent = true;
					cost.BbcodeEnabled = true;
					cost.Text = Utils.MakeMoneyText(_fourthUpgradeACost);
					upgrade.AddChild(cost);

					foreach (EffectResource effect in _fourthUpgradeA)
					{
						effect.SetDescription();
					}

					Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, _fourthUpgradeAName, _fourthUpgradeA);
					upgradeButton.Pressed += (() =>
					{
						if (Utils.VectorLeq(_fourthUpgradeACost, _tdManager._money))
						{
							_tdManager.SpendMoney(_fourthUpgradeACost);
							UpgradeFourthA();
						}

					});
					upgrade.AddChild(upgradeButton);

					TextureRect image = new();
					image.Texture = _fourthUpgradeATexture;
					image.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
					upgrade.AddChild(image);

					upgradesH.AddChild(upgrade);
				}
				if (_fourthUpgradeB is not null && _fourthUpgradeB.Count != 0)
				{
					VBoxContainer upgrade = new VBoxContainer();
					RichTextLabel cost = new();
					cost.FitContent = true;
					cost.BbcodeEnabled = true;
					cost.Text = Utils.MakeMoneyText(_fourthUpgradeBCost);
					upgrade.AddChild(cost);

					foreach (EffectResource effect in _fourthUpgradeB)
					{
						effect.SetDescription();
					}

					Button upgradeButton = EffectResource.MakeCombinedEffectTooltip(true, _fourthUpgradeBName, _fourthUpgradeB);
					upgradeButton.Pressed += (() =>
					{
						if (Utils.VectorLeq(_fourthUpgradeBCost, _tdManager._money))
						{
							_tdManager.SpendMoney(_fourthUpgradeBCost);
							UpgradeFourthB();
						}

					});
					upgrade.AddChild(upgradeButton);

					TextureRect image = new();
					image.Texture = _fourthUpgradeBTexture;
					image.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
					upgrade.AddChild(image);

					upgradesH.AddChild(upgrade);
				}
			}
		}
	}

	public string GetDescription()
	{
		return _description;
	}

	public string GetTooltipDescription()
	{
		string fullDesc = _description + "\n";
		if (_weapon != null)
		{
			fullDesc += "Weapon Damage: " + _weapon.GetDamage().ToString() + "\n";
			fullDesc += "Weapon Cooldown: " + _weapon.GetCooldown().ToString("F2") + "\n";
			fullDesc += "DPS: " + _weapon.GetDPS().ToString("F0") + "\n";
			fullDesc += "Range: " + _weapon.GetRange().ToString() + "\n";
		}
		if (GetIncome() != new Vector4I(0,0,0,0))
		{
			fullDesc += "Maximum Income: " + Utils.MakeMoneyText(GetIncome()) + "\n";
		}
		return fullDesc;
	}

	public override Texture2D GetIconTexture()
	{
		return _iconTexture;
	}

	public string GetDPS()
	{
		SetWeapon();
		if (_weapon is not null)
		{
			return _weapon.GetDPS().ToString();
		}
		else
		{
			return "No Weapon"; 
		}
	}

	public virtual Vector4I GetIncome()
	{
		Vector4I income = new();
		foreach (MoneyOnWaveStartResource resource in _effects.OfType<MoneyOnWaveStartResource>())
		{
			income += resource._money;
		}
		return income;
	}

	public Vector4I GetTotalCost()
	{
		Vector4I totalCost = _cost;
		if (_hasFirstUpgrade)
		{
			totalCost += _firstUpgradeCost;
		}
		if (_hasSecondUpgrade)
		{
			totalCost += _secondUpgradeCost;
		}
		if (_hasThirdUpgrade)
		{
			totalCost += _thirdUpgradeCost;
		}
		return totalCost;
	}
}

