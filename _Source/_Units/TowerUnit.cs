using Godot;
using Godot.Collections;
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

	public readonly string _fourthUpgradeAName = "Rank up A";

	public readonly string _fourthUpgradeBName = "Rank up B";

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
	public Array<bool> _hasFourthUpgrade = [false, false];

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

	protected override void SetStartingEffects(bool addResourceOnly)
	{
		base.SetStartingEffects(addResourceOnly);
		if (addResourceOnly)
		{
			return;
		}
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

	public virtual void OnNewWave()
	{
		_damageDealt = 0;
		EmitSignal(SignalName.UpdateStatsInfo);
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
		_hasFourthUpgrade[0] = true;
		foreach (var effect in _fourthUpgradeA)
		{
			AddEffect(effect);
		}
	}

	public void UpgradeFourthB()
	{
		_hasFourthUpgrade[1] = true;
		foreach (var effect in _fourthUpgradeB)
		{
			AddEffect(effect);
		}
	}

	public virtual HoverInfoImage MakeTowerTooltip(bool clickable)
	{
		_tdManager = GetTree().CurrentScene.GetNode<TDManager>("TdManager");
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
			popupH.AddChild(GetUnitInfoContainerWithString("BasicInfo"));
			popupH.AddChild(GetUnitInfoContainerWithString("WeaponInfo"));
		}
		else if (_towerType == TowerType.Spawner)
		{
			popupH.AddChild(GetUnitInfoContainerWithString("MoneyInfo"));
			popupH.AddChild(GetUnitInfoContainerWithString("SpawnedUnitInfo"));
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
			TooltipRichTextLabel costLabel = new();
			costLabel.Text = "Total cost: \n" + Utils.MakeMoneyText(GetTotalCost());
			costLabel.Name = "CostLabel";
			costLabel.CustomMinimumSize = new(200, 0);
			costLabel.BbcodeEnabled = true;
			costLabel.FitContent = true;
			moneyInfoV.AddChild(costLabel);
		}

		if (_weapon != null)
		{
			TooltipRichTextLabel totalDamageLabel = new();
			totalDamageLabel.Text = "Damage dealt: \n" + _damageDealt;
			totalDamageLabel.Name = "TotalDamageLabel";
			totalDamageLabel.CustomMinimumSize = new(200, 0);
			totalDamageLabel.BbcodeEnabled = true;
			totalDamageLabel.FitContent = true;
			moneyInfoV.AddChild(totalDamageLabel);
		}
		

		Button sellButton = new();
		sellButton.Text = "Sell for 75%";
		sellButton.Pressed += () =>
		{
			_tdManager.GainMoney(Utils.VectorScalarMultiplication(GetTotalCost(), 0.75f));
			_tdManager._towerManager.RemoveTower(_gridLocation);
		};
		moneyInfoV.AddChild(sellButton);

		if (this is Spawner spawner && spawner._data._units.Count() > 0)
		{
			TooltipRichTextLabel spawnLabel = new();
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

			HBoxContainer spawnedUnitTotalInfoH = new();
			spawnedUnitTotalInfoV.AddChild(spawnedUnitTotalInfoH);

			foreach (InvaderStatsIncreaseResource resource in spawner._data._units)
			{
				InvaderUnit spawnedUnit = resource.GetInvader();
				PanelContainer spawnedUnitInfo = spawnedUnit.GetUnitInfoContainerWithString("BasicInfo");
				spawnedUnitTotalInfoH.AddChild(spawnedUnitInfo);
				PanelContainer spawnedUnitEffectInfo = spawnedUnit.GetUnitInfoContainerWithString("EffectsInfo");
				spawnedUnitTotalInfoH.AddChild(spawnedUnitEffectInfo);
				spawnedUnit.QueueFree();
			}
			
			spawnedUnitTotalInfo.AddChild(spawnedUnitTotalInfoV);

			_infoContainers.Add("SpawnedUnitInfo", spawnedUnitTotalInfo);
		}

		if (GetIncome() != new Vector4I(0,0,0,0))
		{
			TooltipRichTextLabel incomeLabel = new();
			incomeLabel.Text = "Total: " + Utils.MakeMoneyText(GetIncome());
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
			attackPriority.CustomMinimumSize = new(150, 0);
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

		MakeUpgradeUI(upgradesH);

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
			TooltipRichTextLabel costLabel = moneyInfoV.GetNode<TooltipRichTextLabel>("CostLabel");
			costLabel.Text = "Total cost: \n" + Utils.MakeMoneyText(GetTotalCost());
		}

		if (_weapon != null)
		{
			TooltipRichTextLabel totalDamageLabel = moneyInfoV.GetNode<TooltipRichTextLabel>("TotalDamageLabel");
			totalDamageLabel.Text = "Damage dealt: \n" + _damageDealt;
		}

		if (this is Spawner spawner && spawner._data._units.Count() > 0)
		{
			TooltipRichTextLabel spawnLabel = moneyInfoV.GetNode<TooltipRichTextLabel>("SpawnLabel");
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

				HBoxContainer spawnedUnitTotalInfoH = new();
				spawnedUnitTotalInfoV.AddChild(spawnedUnitTotalInfoH);

				foreach (InvaderStatsIncreaseResource resource in spawner._data._units)
				{
					InvaderUnit spawnedUnit = resource.GetInvader();
					PanelContainer spawnedUnitInfo = spawnedUnit.GetUnitInfoContainerWithString("BasicInfo");
					spawnedUnitTotalInfoH.AddChild(spawnedUnitInfo);
					PanelContainer spawnedUnitEffectInfo = spawnedUnit.GetUnitInfoContainerWithString("EffectsInfo");
					spawnedUnitTotalInfoH.AddChild(spawnedUnitEffectInfo);
					spawnedUnit.QueueFree();
				}

				_infoContainers["SpawnedUnitInfo"].AddChild(spawnedUnitTotalInfoV);
			}
		}
		

		if (GetIncome() != new Vector4I(0, 0, 0, 0))
		{
			TooltipRichTextLabel incomeLabel = moneyInfoV.GetNode<TooltipRichTextLabel>("IncomeLabel");
			incomeLabel.Text = "Total: " + Utils.MakeMoneyText(GetIncome());
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

			MakeUpgradeUI(upgradesH);
		}

		_tdManager._towerManager.UpdateIncomeDisplay();
	}

	private void MakeUpgradeUI(HBoxContainer upgradesH)
	{
		if (!_hasFirstUpgrade)
		{
			if (_firstUpgrade is not null)
			{
				VBoxContainer upgrade = new VBoxContainer();
				TooltipRichTextLabel cost = new();
				cost.HorizontalAlignment = HorizontalAlignment.Center;
				cost.FitContent = true;
				cost.BbcodeEnabled = true;
				cost.Text = Utils.MakeMoneyText(_firstUpgradeCost);
				cost.CustomMinimumSize = new Vector2(150, 0);
				upgrade.AddChild(cost);

				foreach (EffectResource effect in _firstUpgrade)
				{
					effect.SetDescription();
				}

				UpgradeButton upgradeButton = new UpgradeButton();
				EffectResource.MakeCombinedEffectTooltip(true, _firstUpgradeName, _firstUpgrade, upgradeButton);

				upgradeButton.Pressed += (() =>
				{
					if (Utils.VectorLeq(_firstUpgradeCost, _tdManager._money))
					{
						_tdManager.SpendMoney(_firstUpgradeCost);
						UpgradeFirst();
					}
				});

				upgradeButton.UpdateAffordabilityDisplay(Utils.VectorDivision(_tdManager._money, _firstUpgradeCost));

				//upgradeTooltip.AddChild(upgradeButton);
				upgrade.AddChild(upgradeButton);
				upgradesH.AddChild(upgrade);
			}
		}
		else if ((!_hasSecondUpgrade) && _unlockedSecondUpgrade)
		{
			if (_secondUpgrade is not null)
			{
				VBoxContainer upgrade = new VBoxContainer();
				TooltipRichTextLabel cost = new();
				cost.HorizontalAlignment = HorizontalAlignment.Center;
				cost.FitContent = true;
				cost.BbcodeEnabled = true;
				cost.Text = Utils.MakeMoneyText(_secondUpgradeCost);
				cost.CustomMinimumSize = new Vector2(150, 0);
				upgrade.AddChild(cost);

				foreach (EffectResource effect in _secondUpgrade)
				{
					effect.SetDescription();
				}

				UpgradeButton upgradeButton = new UpgradeButton();
				EffectResource.MakeCombinedEffectTooltip(true, _secondUpgradeName, _secondUpgrade, upgradeButton);

				upgradeButton.Pressed += (() =>
				{
					if (Utils.VectorLeq(_secondUpgradeCost, _tdManager._money))
					{
						_tdManager.SpendMoney(_secondUpgradeCost);
						UpgradeSecond();
					}
				});

				upgradeButton.UpdateAffordabilityDisplay(Utils.VectorDivision(_tdManager._money, _secondUpgradeCost));
				upgrade.AddChild(upgradeButton);
				upgradesH.AddChild(upgrade);
			}

		}
		else if (!_hasThirdUpgrade && _unlockedThirdUpgrade)
		{
			if (_thirdUpgrade is not null)
			{
				VBoxContainer upgrade = new VBoxContainer();
				TooltipRichTextLabel cost = new();
				cost.HorizontalAlignment = HorizontalAlignment.Center;
				cost.FitContent = true;
				cost.BbcodeEnabled = true;
				cost.Text = Utils.MakeMoneyText(_thirdUpgradeCost);
				cost.CustomMinimumSize = new Vector2(150, 0);
				upgrade.AddChild(cost);

				foreach (EffectResource effect in _thirdUpgrade)
				{
					effect.SetDescription();
				}

				UpgradeButton upgradeButton = new UpgradeButton();
				EffectResource.MakeCombinedEffectTooltip(true, _thirdUpgradeName, _thirdUpgrade, upgradeButton);

				upgradeButton.Pressed += (() =>
				{
					if (Utils.VectorLeq(_thirdUpgradeCost, _tdManager._money))
					{
						_tdManager.SpendMoney(_thirdUpgradeCost);
						UpgradeThird();
					}
				});

				upgradeButton.UpdateAffordabilityDisplay(Utils.VectorDivision(_tdManager._money, _thirdUpgradeCost));
				upgrade.AddChild(upgradeButton);
				upgradesH.AddChild(upgrade);
			}

		}
		else if (!_hasFourthUpgrade[0] && !_hasFourthUpgrade[1] && _unlockedFourthUpgrade)
		{
			if (_fourthUpgradeA is not null && _fourthUpgradeA.Count != 0)
			{
				VBoxContainer upgrade = new VBoxContainer();
				TooltipRichTextLabel cost = new();
				cost.HorizontalAlignment = HorizontalAlignment.Center;
				cost.FitContent = true;
				cost.BbcodeEnabled = true;
				cost.Text = Utils.MakeMoneyText(_fourthUpgradeACost);
				cost.CustomMinimumSize = new Vector2(150, 0);
				upgrade.AddChild(cost);

				foreach (EffectResource effect in _fourthUpgradeA)
				{
					effect.SetDescription();
				}

				UpgradeButton upgradeButton = new UpgradeButton();
				EffectResource.MakeCombinedEffectTooltip(true, _fourthUpgradeAName, _fourthUpgradeA, upgradeButton);

				upgradeButton.Pressed += (() =>
				{
					if (Utils.VectorLeq(_fourthUpgradeACost, _tdManager._money))
					{
						_tdManager.SpendMoney(_fourthUpgradeACost);
						UpgradeFourthA();
					}
				});

				upgradeButton.UpdateAffordabilityDisplay(Utils.VectorDivision(_tdManager._money, _fourthUpgradeACost));
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
				TooltipRichTextLabel cost = new();
				cost.HorizontalAlignment = HorizontalAlignment.Center;
				cost.FitContent = true;
				cost.BbcodeEnabled = true;
				cost.Text = Utils.MakeMoneyText(_fourthUpgradeBCost);
				cost.CustomMinimumSize = new Vector2(150, 0);
				upgrade.AddChild(cost);

				foreach (EffectResource effect in _fourthUpgradeB)
				{
					effect.SetDescription();
				}

				UpgradeButton upgradeButton = new UpgradeButton();
				EffectResource.MakeCombinedEffectTooltip(true, _fourthUpgradeBName, _fourthUpgradeB, upgradeButton);

				upgradeButton.Pressed += (() =>
				{
					if (Utils.VectorLeq(_fourthUpgradeBCost, _tdManager._money))
					{
						_tdManager.SpendMoney(_fourthUpgradeBCost);
						UpgradeFourthB();
					}
				});

				upgradeButton.UpdateAffordabilityDisplay(Utils.VectorDivision(_tdManager._money, _fourthUpgradeBCost));
				upgrade.AddChild(upgradeButton);

				TextureRect image = new();
				image.Texture = _fourthUpgradeBTexture;
				image.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
				upgrade.AddChild(image);

				upgradesH.AddChild(upgrade);
			}
		}
	}

	public string GetDescription()
	{
		return _description;
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
		if (_hasFourthUpgrade[0])
		{
			totalCost += _fourthUpgradeACost;
		}
		if (_hasFourthUpgrade[1])
		{
			totalCost += _fourthUpgradeBCost;
		}
		return totalCost;
	}
}

