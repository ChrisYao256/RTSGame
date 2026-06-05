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
	public int _cost;

	[Export]
	public Texture2D _iconTexture;

	[Export]
	public TowerType _towerType;

	[Export]
	public string _description;

	[Export]
	public Array<EffectResource> _firstUpgrade;

	[Export]
	public int _firstUpgradeCost;

	[Export]
	public string _firstUpgradeName = "Lv 1";

	[Export]
	public EffectResource _secondUpgradeA;
	[Export]
	public int _secondUpgradeACost;
	[Export]
	public EffectResource _secondUpgradeB;
	[Export]
	public int _secondUpgradeBCost;
	[Export]
	public EffectResource _secondUpgradeC;
	[Export]
	public int _secondUpgradeCCost;

	[Export]
	public Array<EffectResource> _thirdUpgrade;
	[Export]
	public int _thirdUpgradeCost;
	[Export]
	public string _thirdUpgradeName = "Lv 3";

	[Export]
	public Array<EffectResource> _fourthUpgrade;
	[Export]
	public int _fourthUpgradeCost;
	[Export]
	public string _fourthUpgradeName = "Lv 4";

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

	public bool _unlockedSecondUpgrades = false;
	public List<bool> _hasSecondUpgrade = [false, false, false];

	public bool _unlockedThirdUpgrade = false;
	public bool _hasThirdUpgrade = false;

	public bool _unlockedFourthUpgrade = false;
	public bool _hasFourthUpgrade = false;

	private float _iconSize = TDManager.TileSize / 2f;

	public TargetPriority _targetPriority;

	public TDManager _tdManager;
	public Grid _grid;
	public Vector2I _gridLocation;

	public override void _Ready()
	{
		_radius = TDManager.TileSize / 2f;
		_grid = GetTree().CurrentScene.GetNode<Grid>("TileMapLayer");
		_tdManager = GetTree().CurrentScene.GetNode<TDManager>("TdManager");
		base._Ready();
		CollisionLayer = UnitManager.TowerLayerMask;
		_aiControlled = false;
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
			TurretTurner turret = GetNode<TurretTurner>("TurretTurner"); Sprite2D sprite = turret.GetNode<Sprite2D>("Sprite2D");
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
		_unlockedSecondUpgrades = true;
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
	}

	public void UpgradeSecondA()
	{
		_hasSecondUpgrade[0] = true;
		_secondUpgradeBCost = (int)(_secondUpgradeBCost * _upgradeCostScaling);
		_secondUpgradeCCost = (int)(_secondUpgradeCCost * _upgradeCostScaling);
		AddEffect( _secondUpgradeA);

	}

	public void UpgradeSecondB()
	{
		_hasSecondUpgrade[1] = true;
		_secondUpgradeCCost = (int)(_secondUpgradeCCost * _upgradeCostScaling);
		_secondUpgradeACost = (int)(_secondUpgradeACost * _upgradeCostScaling);
		AddEffect(_secondUpgradeB);

	}

	public void UpgradeSecondC()
	{
		_hasSecondUpgrade[2] = true;
		_secondUpgradeBCost = (int)(_secondUpgradeBCost * _upgradeCostScaling);
		_secondUpgradeACost = (int)(_secondUpgradeACost * _upgradeCostScaling);
		AddEffect(_secondUpgradeC);

	}

	public void UpgradeThird()
	{
		_hasThirdUpgrade = true;
		foreach (var effect in _thirdUpgrade)
		{
			AddEffect(effect);
		}
	}

	public void UpgradeFourth()
	{
		_hasFourthUpgrade = true;
		foreach (var effect in _fourthUpgrade)
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

		// 2. Create the Popup Box (PanelContainer for the background)
		PanelContainer popup = new PanelContainer();
		popup.ZIndex = 100;
		popup.TopLevel = true; // Essential to avoid parent clipping
		popup.Visible = false;

		// 3. Style the background (Solid Color)
		StyleBoxFlat style = new StyleBoxFlat();
		style.BgColor = new Color(0.2f, 0.2f, 0.2f); // Dark Grey
		style.SetContentMarginAll(5); // Padding around text
		popup.AddThemeStyleboxOverride("panel", style);

		VBoxContainer vContainer = new VBoxContainer();

		// 4. Create the Description Label
		Label desc = new Label();
		desc.Text = GetDescription(); 
		desc.CustomMinimumSize = new Vector2(200, 0); // Limit width

		if (this is Spawner spawner && spawner._data._units.Count > 0)
		{
			desc.Text += "Spawns " + spawner.GetSpawns();
		}

		desc.Text = desc.Text.Trim();

		vContainer.AddChild(desc);

		//foreach (EffectResource effect in _effects)
		//{
		//	VBoxContainer container = new();
		//	HoverInfoLabel effectName = effect.MakeEffectTooltip(false);
		//	container.AddChild(effectName);
		//	vContainer.AddChild(container);
		//}

		popup.AddChild(vContainer);
		trigger.AddChild(popup); // Attach popup to trigger for organization
		trigger._popupBox = popup;


		Utils.ScaleTextureButtonToRadius(trigger, _iconSize);

		return trigger;
	}

	public string GetDescription()
	{
		string fullDesc = _description + "\n";
		if (_weapon != null)
		{
			fullDesc += "Weapon Damage: " + _weapon.GetDamage().ToString() + "\n";
			fullDesc += "Weapon Cooldown: " + _weapon.GetCooldown().ToString() + "\n";
			fullDesc += "DPS: " + _weapon.GetDPS().ToString() + "\n";
			fullDesc += "Range: " + _weapon.GetRange().ToString() + "\n";
		}
		if (GetIncome() > 0)
		{
			fullDesc += "Maximum Income: $" + GetIncome() + "\n";
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

	public virtual int GetIncome()
	{
		int income = 0;
		foreach (MoneyOnWaveStartResource resource in _effects.OfType<MoneyOnWaveStartResource>())
		{
			income += resource._money;
		}
		return income;
	}

	public int GetTotalCost()
	{
		int totalCost = _cost;
		if (_hasFirstUpgrade)
		{
			totalCost += _firstUpgradeCost;
		}
		if (_hasSecondUpgrade[0])
		{
			totalCost += _secondUpgradeACost;
		}
		if (_hasSecondUpgrade[1])
		{
			totalCost += _secondUpgradeBCost;
		}
		if (_hasSecondUpgrade[2])
		{
			totalCost += _secondUpgradeCCost;
		}
		if (_hasThirdUpgrade)
		{
			totalCost += _thirdUpgradeCost;
		}
		return totalCost;
	}
}

