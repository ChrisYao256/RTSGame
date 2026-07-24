using Godot;
using Godot.Collections;
using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

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

	private UpgradeButton _upgradeButton1; // the upgrade button at slot 1. Could be for any level.
	private Vector4I _upgrade1Cost; // this is not the cost of the first upgrade, but it is the cost of whatever upgrade currently sits at slot 1.

	private UpgradeButton _upgradeButton2;
	private Vector4I _upgrade2Cost;

	[Export]
	private Array<EffectResource> _transformEffects = [];

	public enum TargetPriority
	{
		First,
		Last,
		Closest,
		Strongest,
		Smart,
	}

	public enum TowerType
	{
		Defense,
		Support,
		Spawner,
		Null
	}

	public int _priorityCount = 4;

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
		_radius = TDManager.TileSize / (2f * (float)Math.Sqrt(2f));
		_grid = GetTree().CurrentScene.GetNode<Grid>("TileMapLayer");
		_tdManager = GetTree().CurrentScene.GetNode<TDManager>("TdManager");
		base._Ready();
		CollisionLayer = UnitManager.TowerLayerMask;
		_aiControlled = false;
		_lvLabel = GetNode<Label>("Level");
		if (_weapon is not null && _weapon._hasCustomPriority)
		{
			_priorityCount += 1;
			_targetPriority = (TargetPriority)_priorityCount - 1;
		}
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

	public void AddTowerStatsIncrease(StatsIncreaseResource resource)
	{
		resource.MergeWithOld(_data, []);
	}

	public void RemoveTowerStatsIncrease(StatsIncreaseResource resource)
	{
		resource.RemoveFromOld(_data);
	}

	public virtual void OnNewWave()
	{
		_damageDealt = 0;
		_moneyGained = new Vector4I(0,0,0,0);
		EmitSignal(SignalName.UpdateStatsInfo);
		EmitSignal(SignalName.NewWave);
	}

	protected override List<Unit> FormTargetOrder(Array<Unit> bodies)
	{
		List<InvaderUnit> orderedBodies = new List<InvaderUnit>();
		List<InvaderUnit> invaders = bodies.OfType<InvaderUnit>().ToList();
		Godot.Collections.Dictionary<int, Array<InvaderUnit>> sortedBodies = new();
		int maxTauntLevel = 0;
		foreach (InvaderUnit invader in invaders)
		{
			if (!sortedBodies.Keys.Contains(invader._tauntLevel))
			{
				sortedBodies.Add(invader._tauntLevel, []);
			}
			sortedBodies[invader._tauntLevel].Add(invader);
			maxTauntLevel = Math.Max(invader._tauntLevel, maxTauntLevel);
		}
		for (int i = maxTauntLevel; i >= 0; i--)
		{
			if (!sortedBodies.Keys.Contains(i) || sortedBodies[i].Count == 0)
			{
				continue;
			}
			switch (_targetPriority)
			{
				case (TargetPriority.First):
					orderedBodies.AddRange(sortedBodies[i].OrderBy(body => body.GetDistanceToExit()).ToList());
					break;
				case (TargetPriority.Last):
					orderedBodies.AddRange(sortedBodies[i].OrderBy(body => -body.GetDistanceToExit()).ToList());
					break;
				case (TargetPriority.Closest):
					orderedBodies.AddRange(sortedBodies[i].OrderBy(body => GlobalPosition.DistanceSquaredTo(body.GlobalPosition)).ToList());
					break;
				case (TargetPriority.Strongest):
					orderedBodies.AddRange(sortedBodies[i].OrderBy(body => -body.GetHpMax()).ToList());
					break;
				case (TargetPriority.Smart):
					orderedBodies.AddRange(_weapon.FormCustomTargetOrder(sortedBodies[i].ToList()));
					break;
				default:
					throw new Exception("Unknown TargetPriority");
			}
		}

		return orderedBodies.Cast<Unit>().ToList();
	}

	public void SetTargetPriority(TargetPriority priority)
	{
		_targetPriority = priority;
		EmitSignal(SignalName.UpdateInfo);
	}

	public void LastTargetPriority()
	{
		int newPriority = Utils.Mod(((int)_targetPriority - 1), _priorityCount);
		SetTargetPriority((TargetPriority)newPriority);
	}

	public void NextTargetPriority()
	{
		int newPriority = Utils.Mod(((int)_targetPriority + 1), _priorityCount);
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
		popup.ZIndex = 100;
		popup.TopLevel = true; // Essential to avoid parent clipping
		popup.Visible = false;

		HBoxContainer popupH = new();

		if (_towerType == TowerType.Defense)
		{
			popupH.AddChild(GetUnitInfoContainerWithString("BasicInfo"));
			popupH.AddChild(GetUnitInfoContainerWithString("WeaponInfo"));
			popupH.AddChild(GetUnitInfoContainerWithString("EffectsInfo"));
		}
		else if (_towerType == TowerType.Spawner)
		{
			popupH.AddChild(GetUnitInfoContainerWithString("MoneyInfo"));
			popupH.AddChild(GetUnitInfoContainerWithString("SpawnedUnitInfo"));
			popupH.AddChild(GetUnitInfoContainerWithString("EffectsInfo"));
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

		TooltipRichTextLabel totalDamageLabel = new();
		totalDamageLabel.Text = "Bonus gained: \n" + Utils.MakeMoneyText(_moneyGained);
		totalDamageLabel.Name = "TotalGainLabel";
		totalDamageLabel.CustomMinimumSize = new(200, 0);
		totalDamageLabel.BbcodeEnabled = true;
		totalDamageLabel.FitContent = true;
		totalDamageLabel.Visible = false;
		moneyInfoV.AddChild(totalDamageLabel);

		if (_moneyGained != new Vector4I(0,0,0,0))
		{
			totalDamageLabel.Visible = true;
		}
		
		if (_towerType == TowerType.Defense)
		{
			Button sellButton = new();
			sellButton.Text = "Sell for 75%";
			sellButton.Pressed += () =>
			{
				_tdManager.GainMoney(Utils.VectorScalarMultiplication(GetTotalCost(), 0.75f));
				_tdManager._towerManager.RemoveTower(_gridLocation);
			};
			moneyInfoV.AddChild(sellButton);
		}

		if (_towerType == TowerType.Spawner)
		{
			Button sellButton = new();
			sellButton.Text = "Remove";
			sellButton.Pressed += () =>
			{
				_tdManager._towerManager.RemoveTower(_gridLocation);
			};
			moneyInfoV.AddChild(sellButton);
		}


		if (this is Spawner spawner && spawner._spawnerData._units.Count() > 0)
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
			spawnedUnitLabel.CustomMinimumSize = new Vector2(100, 0);
			spawnedUnitTotalInfoV.AddChild(spawnedUnitLabel);

			HBoxContainer spawnedUnitTotalInfoH = new();
			spawnedUnitTotalInfoV.AddChild(spawnedUnitTotalInfoH);

			if (spawner._spawnedUnitArray.Count != 0)
			{
				foreach (InvaderUnit spawnedUnit in spawner._spawnedUnitArray)
				{
					PanelContainer spawnedUnitInfo = spawnedUnit.GetUnitInfoContainerWithString("BasicInfo");
					spawnedUnitTotalInfoH.AddChild(spawnedUnitInfo);
					PanelContainer spawnedUnitEffectInfo = spawnedUnit.GetUnitInfoContainerWithString("EffectsInfo");
					spawnedUnitTotalInfoH.AddChild(spawnedUnitEffectInfo);
				}
			}
			else
			{
				foreach (InvaderStatsIncreaseResource resource in spawner._spawnerData._units)
				{
					InvaderUnit spawnedUnit = resource.GetInvader();
					spawner._spawnedUnitArray.Add(spawnedUnit);
					PanelContainer spawnedUnitInfo = spawnedUnit.GetUnitInfoContainerWithString("BasicInfo");
					spawnedUnitTotalInfoH.AddChild(spawnedUnitInfo);
					PanelContainer spawnedUnitEffectInfo = spawnedUnit.GetUnitInfoContainerWithString("EffectsInfo");
					spawnedUnitTotalInfoH.AddChild(spawnedUnitEffectInfo);
				}
			}


			spawnedUnitTotalInfo.AddChild(spawnedUnitTotalInfoV);

			_infoContainers.Add("SpawnedUnitInfo", spawnedUnitTotalInfo);
		}

		if (GetIncome() != new Vector4I(0,0,0,0))
		{
			TooltipRichTextLabel incomeLabel = new();
			incomeLabel.Text = "Produces " + Utils.MakeMoneyText(GetIncome());
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
			VBoxContainer attackPriorityV = new VBoxContainer();
			attackPriorityV.Name = "VBoxContainer";
			attackPriorityV.Alignment = BoxContainer.AlignmentMode.Center;

			Button upPriorityButton = new();
			upPriorityButton.Text = "△";
			upPriorityButton.Pressed += () => LastTargetPriority();
			attackPriorityV.AddChild(upPriorityButton);

			HoverInfoLabel priorityLabel = new();
			priorityLabel.CustomMinimumSize = new(175, 0);
			priorityLabel.AddThemeConstantOverride("horizontal_alignment", (int)HorizontalAlignment.Center);
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
				case TowerUnit.TargetPriority.Smart:
					priorityLabel.Text = "Smart";
					break;
				default:
					priorityLabel.Text = ((int)_targetPriority).ToString();
					break;
			}
			priorityLabel.Name = "AttackPriorityLabel";
			if (_targetPriority != TowerUnit.TargetPriority.Smart)
			{
				HoverInfoLabel.AddTooltipToButton(priorityLabel, StringDB.Entries["TargetPriority" + _targetPriority.ToString()]);
			}
			else
			{
				string test = _weapon.GetType().ToString();
				HoverInfoLabel.AddTooltipToButton(priorityLabel, StringDB.Entries["TargetPriority" + _weapon.GetType().ToString()]);
			}
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
			VBoxContainer weaponInfoV = _infoContainers["WeaponInfo"].GetNode<VBoxContainer>("VBoxContainer");
			TooltipRichTextLabel totalDamageLabel = weaponInfoV.GetNode<TooltipRichTextLabel>("TotalDamageLabel");
			totalDamageLabel.Text = "Damage dealt: " + _damageDealt;
		}

		if (moneyInfoV.HasNode("TotalGainLabel"))
		{
			TooltipRichTextLabel totalGainLabel = moneyInfoV.GetNode<TooltipRichTextLabel>("TotalGainLabel");
			totalGainLabel.Text = "Bonus gained: \n" + Utils.MakeMoneyText(_moneyGained);
			if (_moneyGained != new Vector4I(0, 0, 0, 0))
			{
				totalGainLabel.Visible = true;
			}
			else
			{
				totalGainLabel.Visible = false;
			}
		}

		if (this is Spawner spawner && spawner._spawnerData._units.Count() > 0)
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

				if (spawner._spawnedUnitArray.Count != 0)
				{
					foreach (InvaderUnit spawnedUnit in spawner._spawnedUnitArray)
					{
						PanelContainer spawnedUnitInfo = spawnedUnit.GetUnitInfoContainerWithString("BasicInfo");
						spawnedUnitTotalInfoH.AddChild(spawnedUnitInfo);
						PanelContainer spawnedUnitEffectInfo = spawnedUnit.GetUnitInfoContainerWithString("EffectsInfo");
						spawnedUnitTotalInfoH.AddChild(spawnedUnitEffectInfo);
					}
				}
				else
				{
					foreach (InvaderStatsIncreaseResource resource in spawner._spawnerData._units)
					{
						InvaderUnit spawnedUnit = resource.GetInvader();
						spawner._spawnedUnitArray.Add(spawnedUnit);
						PanelContainer spawnedUnitInfo = spawnedUnit.GetUnitInfoContainerWithString("BasicInfo");
						spawnedUnitTotalInfoH.AddChild(spawnedUnitInfo);
						PanelContainer spawnedUnitEffectInfo = spawnedUnit.GetUnitInfoContainerWithString("EffectsInfo");
						spawnedUnitTotalInfoH.AddChild(spawnedUnitEffectInfo);
					}
				}

				_infoContainers["SpawnedUnitInfo"].AddChild(spawnedUnitTotalInfoV);
			}
		}

		if (GetIncome() != new Vector4I(0, 0, 0, 0))
		{
			if (moneyInfoV.HasNode("IncomeLabel"))
			{
				TooltipRichTextLabel incomeLabel = moneyInfoV.GetNode<TooltipRichTextLabel>("IncomeLabel");
				incomeLabel.Text = $"Produces {Utils.MakeMoneyText(GetIncome())}";
			}
			else
			{
				TooltipRichTextLabel incomeLabel = new();
				incomeLabel.Text = $"Produces {Utils.MakeMoneyText(GetIncome())}";
				incomeLabel.Name = "IncomeLabel";
				incomeLabel.CustomMinimumSize = new(200, 0);
				incomeLabel.BbcodeEnabled = true;
				incomeLabel.FitContent = true;
				moneyInfoV.AddChild(incomeLabel);
			}
		}
		else
		{
			if (moneyInfoV.HasNode("IncomeLabel"))
			{
				moneyInfoV.GetNode<TooltipRichTextLabel>("IncomeLabel").QueueFree();
			}
		}

		if (_weapon != null)
		{
			VBoxContainer attackPriorityV = _infoContainers["AttackPriority"].GetNode<VBoxContainer>("VBoxContainer");

			HoverInfoLabel priorityLabel = attackPriorityV.GetNode<HoverInfoLabel>("AttackPriorityLabel");
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
				case TowerUnit.TargetPriority.Smart:
					priorityLabel.Text = "Smart";
					break;
				default:
					priorityLabel.Text = ((int)_targetPriority).ToString();
					break;
			}
			priorityLabel._popupBox.QueueFree();
			if (_targetPriority != TowerUnit.TargetPriority.Smart)
			{
				HoverInfoLabel.AddTooltipToButton(priorityLabel, StringDB.Entries["TargetPriority" + _targetPriority.ToString()]);
			}
			else
			{
				string test = _weapon.GetType().ToString();
				HoverInfoLabel.AddTooltipToButton(priorityLabel, StringDB.Entries["TargetPriority" + _weapon.GetType().ToString()]);
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
		else
		{
			UpdateUpgradeButtonProgress();
		}

		_tdManager._towerManager.UpdateIncomeDisplay();
		_tdManager._towerManager.UpdateDPSDisplay();
		_tdManager._towerManager.UpdateTotalHpLabel();
	}

	public void UpdateTowerInfoContainerWithUpgrade(Array<EffectResource> upgrade)
	{
		Array<EffectResource> effectUpgrades = [];
		foreach (EffectResource resource in upgrade)
		{
			if (resource is StatsIncreaseResource statsIncrease)
			{
				if (_weapon != null)
				{
					_weapon.UpdateWeaponInfoContainerWithUpgrade(statsIncrease);
				}
			}
			else if (resource is SpawnerUpgradeResource spawnUpgrade)
			{
				if (this is Spawner spawner && spawner._spawnerData._units.Count() > 0)
				{
					string greenHex = ThemePalette.Green.ToHtml(false);
					VBoxContainer moneyInfoV = _infoContainers["MoneyInfo"].GetNode<VBoxContainer>("VBoxContainer");

					TooltipRichTextLabel spawnLabel = moneyInfoV.GetNode<TooltipRichTextLabel>("SpawnLabel");
					spawnLabel.Text = "Spawns " + spawner.GetSpawns();
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

					Vector4I newIncome = new(0, 0, 0, 0);
					foreach (InvaderStatsIncreaseResource unit in spawner._spawnerData._units)
					{
						if (spawnUpgrade._applySameUpgradeForAllUnits)
						{
							InvaderStatsIncreaseResource unitCopy = (InvaderStatsIncreaseResource)unit.DuplicateDeep();
							spawnUpgrade._units[0].MergeWithOld(unitCopy, []);
							unitCopy._startingEffects = [];
							InvaderUnit newSpawnedUnit = unitCopy.GetInvader();
							newIncome += newSpawnedUnit.GetTotalMoneyDropped();
							PanelContainer spawnedUnitInfo = newSpawnedUnit.GetUnitInfoContainerWithUpgradeWithString("BasicInfo", spawnUpgrade._units[0]);
							spawnedUnitTotalInfoH.AddChild(spawnedUnitInfo);
							PanelContainer spawnedUnitEffectInfo = newSpawnedUnit.GetUnitInfoContainerWithUpgradeWithString("EffectsInfo", spawnUpgrade._units[0]);
							spawnedUnitTotalInfoH.AddChild(spawnedUnitEffectInfo);
							newSpawnedUnit.QueueFree();
						}
						else
						{
							int i = spawner._spawnerData._units.IndexOf(unit);
							InvaderStatsIncreaseResource unitCopy = (InvaderStatsIncreaseResource)unit.DuplicateDeep();
							spawnUpgrade._units[i].MergeWithOld(unitCopy, []);
							InvaderUnit newSpawnedUnit = unitCopy.GetInvader();
							newIncome += newSpawnedUnit.GetTotalMoneyDropped();
							PanelContainer spawnedUnitInfo = newSpawnedUnit.GetUnitInfoContainerWithUpgradeWithString("BasicInfo", spawnUpgrade._units[i]);
							spawnedUnitTotalInfoH.AddChild(spawnedUnitInfo);
							PanelContainer spawnedUnitEffectInfo = newSpawnedUnit.GetUnitInfoContainerWithUpgradeWithString("EffectsInfo", spawnUpgrade._units[i]);
							spawnedUnitTotalInfoH.AddChild(spawnedUnitEffectInfo);
							newSpawnedUnit.QueueFree();
						}
					}

					if (newIncome != new Vector4I(0, 0, 0, 0))
					{
						if (moneyInfoV.HasNode("IncomeLabel"))
						{
							TooltipRichTextLabel incomeLabel = moneyInfoV.GetNode<TooltipRichTextLabel>("IncomeLabel");
							incomeLabel.Text = $"[color=#{greenHex}]Produces {Utils.MakeMoneyText(newIncome)}[/color]";
						}
						else
						{
							TooltipRichTextLabel incomeLabel = new();
							incomeLabel.Text = $"[color=#{greenHex}]Produces {Utils.MakeMoneyText(newIncome)}[/color]";
							incomeLabel.Name = "IncomeLabel";
							incomeLabel.CustomMinimumSize = new(200, 0);
							incomeLabel.BbcodeEnabled = true;
							incomeLabel.FitContent = true;
							moneyInfoV.AddChild(incomeLabel);
						}
					}

					_infoContainers["SpawnedUnitInfo"].AddChild(spawnedUnitTotalInfoV);
				}
			}
			else if (_effects.Any(e => e.GetType() == resource.GetType()))
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
						PanelContainer effectName = effect.MakeFullEffectDescriptionWithUpgrade(newEffect);
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
					effect.SetUpgradeDescription();
				}

				UpgradeButton upgradeButton = new UpgradeButton();
				MakeUpgradeTooltip(_firstUpgrade, upgradeButton);

				upgradeButton.Pressed += (() =>
				{
					if (Utils.VectorLeq(_firstUpgradeCost, _tdManager._money))
					{
						UpgradeFirst();
						_tdManager.SpendMoney(_firstUpgradeCost);
					}
					else
					{
						_tdManager.FlashMoney();
					}
				});

				InputEventKey keyEvent = new InputEventKey();
				keyEvent.Keycode = Key.U;

				// Wrap into Shortcut
				Shortcut shortcut = new Shortcut();
				shortcut.Events.Add(keyEvent);

				// Assign to button
				upgradeButton.Shortcut = shortcut;

				upgradeButton.UpdateAffordabilityDisplay(Utils.VectorDivision(_tdManager._money, _firstUpgradeCost));

				_upgradeButton1 = upgradeButton;
				_upgrade1Cost = _firstUpgradeCost;

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
					effect.SetUpgradeDescription();
				}

				UpgradeButton upgradeButton = new UpgradeButton();
				MakeUpgradeTooltip(_secondUpgrade, upgradeButton);

				upgradeButton.Pressed += (() =>
				{
					if (Utils.VectorLeq(_secondUpgradeCost, _tdManager._money))
					{
						_tdManager.SpendMoney(_secondUpgradeCost);
						UpgradeSecond();
					}
					else
					{
						_tdManager.FlashMoney();
					}
				});

				InputEventKey keyEvent = new InputEventKey();
				keyEvent.Keycode = Key.U;

				// Wrap into Shortcut
				Shortcut shortcut = new Shortcut();
				shortcut.Events.Add(keyEvent);

				// Assign to button
				upgradeButton.Shortcut = shortcut;

				upgradeButton.UpdateAffordabilityDisplay(Utils.VectorDivision(_tdManager._money, _secondUpgradeCost));

				_upgradeButton1 = upgradeButton;
				_upgrade1Cost = _secondUpgradeCost;

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
					effect.SetUpgradeDescription();
				}

				UpgradeButton upgradeButton = new UpgradeButton();
				MakeUpgradeTooltip(_thirdUpgrade, upgradeButton);

				upgradeButton.Pressed += (() =>
				{
					if (Utils.VectorLeq(_thirdUpgradeCost, _tdManager._money))
					{
						_tdManager.SpendMoney(_thirdUpgradeCost);
						UpgradeThird();
					}
					else
					{
						_tdManager.FlashMoney();
					}
				});

				InputEventKey keyEvent = new InputEventKey();
				keyEvent.Keycode = Key.U;

				// Wrap into Shortcut
				Shortcut shortcut = new Shortcut();
				shortcut.Events.Add(keyEvent);

				// Assign to button
				upgradeButton.Shortcut = shortcut;

				upgradeButton.UpdateAffordabilityDisplay(Utils.VectorDivision(_tdManager._money, _thirdUpgradeCost));

				_upgradeButton1 = upgradeButton;
				_upgrade1Cost = _thirdUpgradeCost;

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
					effect.SetUpgradeDescription();
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
					else
					{
						_tdManager.FlashMoney();
					}
				});

				InputEventKey keyEvent = new InputEventKey();
				keyEvent.Keycode = Key.U;

				// Wrap into Shortcut
				Shortcut shortcut = new Shortcut();
				shortcut.Events.Add(keyEvent);

				// Assign to button
				upgradeButton.Shortcut = shortcut;

				upgradeButton.UpdateAffordabilityDisplay(Utils.VectorDivision(_tdManager._money, _fourthUpgradeACost));

				_upgradeButton1 = upgradeButton;
				_upgrade1Cost = _fourthUpgradeACost;

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
					effect.SetUpgradeDescription();
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
					else
					{
						_tdManager.FlashMoney();
					}
				});

				InputEventKey keyEvent = new InputEventKey();
				keyEvent.Keycode = Key.I;

				// Wrap into Shortcut
				Shortcut shortcut = new Shortcut();
				shortcut.Events.Add(keyEvent);

				// Assign to button
				upgradeButton.Shortcut = shortcut;

				upgradeButton.UpdateAffordabilityDisplay(Utils.VectorDivision(_tdManager._money, _fourthUpgradeBCost));

				_upgradeButton2 = upgradeButton;
				_upgrade2Cost = _fourthUpgradeBCost;

				upgrade.AddChild(upgradeButton);

				TextureRect image = new();
				image.Texture = _fourthUpgradeBTexture;
				image.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
				upgrade.AddChild(image);

				upgradesH.AddChild(upgrade);
			}
		}
	}

	public void UpdateUpgradeButtonProgress()
	{
		if (_upgradeButton1 is not null && IsInstanceValid(_upgradeButton1))
		{
			_upgradeButton1.UpdateAffordabilityDisplay(Utils.VectorDivision(_tdManager._money, _upgrade1Cost));
		}
		if (_upgradeButton2 is not null && IsInstanceValid(_upgradeButton2))
		{
			_upgradeButton2.UpdateAffordabilityDisplay(Utils.VectorDivision(_tdManager._money, _upgrade2Cost));
		}
	}

	public HoverInfoLabel MakeUpgradeTooltip(Array<EffectResource> effects, HoverInfoLabel trigger)
	{
		trigger.MouseEntered += () => UpdateTowerInfoContainerWithUpgrade(effects);
		trigger.MouseExited += () =>
		{
			Callable.From(() => UpdateUnitInfoContainer(true)).CallDeferred();
		};
		trigger.Text = "Upgrade";
		trigger.ResetSize();
		return trigger;
	}

	public string GetDescription()
	{
		return _description;
	}

	public override Texture2D GetIconTexture()
	{
		return _iconTexture;
	}

	public int GetTowerLevel()
	{
		if (!_hasFirstUpgrade)
		{
			return 1;
		}
		else if (!_hasSecondUpgrade)
		{
			return 2;
		}
		else if (!_hasThirdUpgrade)
		{
			return 3;
		}
		else
		{
			return 4;
		}
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

	public virtual int GetUnknownIncome()
	{
		int income = 0;
		foreach (MoneyOnWaveStartOneTypeResource resource in _effects.OfType<MoneyOnWaveStartOneTypeResource>())
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

