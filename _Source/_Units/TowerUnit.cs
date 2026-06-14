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
		RichTextLabel desc = new RichTextLabel();
		desc.BbcodeEnabled = true;
		desc.FitContent = true;
		desc.Text = GetDescription(); 
		desc.CustomMinimumSize = new Vector2(200, 0); // Limit width

		if (this is Spawner spawner && spawner._data._units.Count > 0)
		{
			desc.Text += "Spawns " + spawner.GetSpawns();
		}

		desc.Text = desc.Text.Trim();

		vContainer.AddChild(desc);

		popup.AddChild(vContainer);
		trigger.AddChild(popup); // Attach popup to trigger for organization
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

	public string GetDescription()
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

