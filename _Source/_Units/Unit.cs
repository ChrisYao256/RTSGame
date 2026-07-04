using Godot;
using Godot.Collections;
using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class Unit : CharacterBody2D
{

	protected UnitPathfinder _pathfinder;

	private Sprite2D _selectionVisual;

	protected TextureProgressBar _healthBar;

	protected TextureProgressBar _shieldBar;

	protected Node2D _effectsNode;

	public BaseWeapon _weapon;

	public int _teamId;

	public bool _aiControlled = false;

	private TDManager _tdManager;

	[Export] public int _tauntLevel = 0;

	[Export] public StatsIncreaseResource _data;

	[Export] public float LeashDistance = 200f;

	[Export] protected float _moveSpeed;

	[Export] private int _hpMax;

	[Export] public int _armor = 0;

	[Export] private bool _lockOnTarget = false;


	[Export]
	private Array<EffectResource> _startingEffects;

	public int _hp { get; private set; }

	public int _shield { get; private set; }

	[Export] public string _internalName;

	[Export] public string _name;

	[Export]
	public float _radius = 100f;

	[Export]
	public float _baseHealthBarWidth = 24f;

	[Export]
	public float _maxHealthBarWidth = 100f;
	[Export]
	public float _baseHealthBarHeight = 14f;
	[Export]
	public float _healthBarOffset = 24f;

	[Export]
	public int DebugTeamId
	{
		get => _teamId;
		set { }
	}

	[Export]
	public State DebugState
	{
		get => _state;
		set { }
	}

	[Signal]
	public delegate void DiedEventHandler();

	[Signal]
	public delegate void RemovedEventHandler();

	[Signal]
	public delegate void HpChangeEventHandler(Unit unit, int change);

	[Signal]
	public delegate void NewEffectEventHandler(Unit unit, Effect effect);

	[Signal]
	public delegate void NewWaveEventHandler();

	[Signal]
	public delegate void RemovedEffectEventHandler(Unit unit, Effect effect);

	[Signal]
	public delegate void BeginAttackEventHandler(Unit target);

	[Signal]
	public delegate void StopAttackEventHandler(Unit target);

	[Signal]
	public delegate void ShotFiredEventHandler(Unit unit);

	[Signal]
	public delegate void BeforeHitEnemyEventHandler(Unit target);

	[Signal]
	public delegate void HitEnemyEventHandler(Unit target);

	[Signal]
	public delegate void VolleyEndedEventHandler();

	[Signal]
	public delegate void PlacedTowerEventHandler(TowerUnit tower);

	[Signal]
	public delegate void BeforeIsHitEventHandler(Unit unit);

	[Signal]
	public delegate void IsHitEventHandler(Unit unit);

	[Signal]
	public delegate void KilledUnitEventHandler(Unit unit, Unit target);

	[Signal]
	public delegate void UpdateInfoEventHandler(Unit unit);

	[Signal]
	public delegate void UpdateStatsInfoEventHandler(Unit unit);

	[Signal]
	public delegate void UpdateDamageDealtInfoEventHandler(Unit unit);

	[Signal]
	public delegate void CreationEventHandler(); // used only by transform effects that may remove this unit when created. Allows the tower to be fully placed before OnCreation effects activate. 

	public enum State
	{
		Idle, 
		Attacking,
		Moving,
	}

	protected State _state;
	public List<Command> _commandQueue { get; protected set; } = [];
	public Command _currentCommand { get; protected set; }

	public bool _displayAttackRange;
	public float _attackRange;

	public float _speedDebuff;

	public int _hpMaxModifier;

	public int _armorModifier;

	public int _armorDebuff;

	public float _damageTakenModifier;

	public float _damageTakenDebuff;

	public float _damageReductionBuff;

	public Array<EffectResource> _effects = [];

	protected CollisionShape2D _attackCollisionShape;

	protected Unit _attackTarget;

	protected bool _active = true;

	public bool _isDead = false;

	protected bool _navigationPaused = false;

	public bool _hasEffects = true;

	protected Godot.Collections.Dictionary<string, PanelContainer> _infoContainers;

	public bool _isDisplayUnit = false;

	public int _currentFloatingAnimationCount = 0;

	public float _damageDealt = 0;
	public Vector4I _moneyGained = new Vector4I(0,0,0,0);

	public override void _Ready()
	{
		if (_isDisplayUnit)
		{
			return;
		}
		SetWeapon();
		SetSelectionVisual();
		SetStartingEffects(false);
		SetAttackRange();
		SetPathFinder();
		SetHealthBar();
		SetInitialCommand();
		SetSize();
	}

	public virtual void SetDisplayUnit()
	{
		_isDisplayUnit = true;
		_hp = GetHpMax();
		SetWeapon();
		SetStartingEffects(true);
		SetAttackRange();
		SetSize();
	}

	public virtual void SetSize()
	{
		Sprite2D sprite = GetNode<Sprite2D>("MainSprite");
		Utils.ScaleVisualToRadius(sprite, _radius);
		if (HasNode("AnimatedSprite2D") )
		{
			AnimatedSprite2D animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
			Utils.ScaleVisualToRadius(animatedSprite, _radius);
		}
		CollisionShape2D collision = GetNode<CollisionShape2D>("CollisionShape2D");
		CircleShape2D collisionCircle = new CircleShape2D();
		collisionCircle.Radius = _radius;
		collision.Shape = collisionCircle;
		Sprite2D selectionCircle = GetNode<Sprite2D>("SelectionCircle");
		Utils.ScaleVisualToRadius(selectionCircle, _radius);
	}

	/// <summary>
	/// Adds EffectResources in _startingEffects to _effects. Also sets a blank StatsIncreaseResource as _data, if _data is null. 
	/// </summary>
	/// <param name="addResourceOnly">Set this to false in the call at _Ready(). if this is set to true, then all the EffectResources are added directly to _effects without instantiating their corresponding Effect nodes. 
	/// As a result, these effects will not affect anything in game, but their descriptions can still be accessed. 
	/// Additionally, for EffectResources that merge with itself, adding more of the same type will still not create an Effect node. 
	/// This behavior is ideal for setting effect descriptions of units without actually creating them. </param>
	protected virtual void SetStartingEffects(bool addResourceOnly)
	{
		_effectsNode = GetNode<Node2D>("Effects");
		if (_data == null)
		{
			_data = new StatsIncreaseResource();
		}
		_effects.Add(_data);
		if (addResourceOnly)
		{
			foreach (var effect in _startingEffects)
			{
				_effects.Add((EffectResource)effect.DuplicateDeep());
			}
		}
		else
		{
			foreach (var effect in _startingEffects)
			{
				AddEffect(effect);
			}
		}
	}

	public Effect AddEffect(EffectResource resource)
	{
		
		//Use this to make effects with the same name merge
		if (!_effects.Any(e => e.GetType() == resource.GetType()))
		{
			EffectResource resourceCopy = (EffectResource)resource.DuplicateDeep();
			resourceCopy._source = resource._source;
			_effects.Add(resourceCopy);
			Effect node = EffectManager.Apply(resourceCopy, _effectsNode);
			node.ConnectSignals(this);
			EmitSignal(SignalName.NewEffect, node);
			EmitSignal(SignalName.UpdateInfo);
			return node;
		}
		else
		{
			EffectResource oldEffect = _effects.First(e => e.GetType() == resource.GetType());
			List<EffectResource> allMatchingEffects = (_effects.Where(e => e.GetType() == resource.GetType()).ToList());
			bool addNewEffect = resource.MergeWithOld(oldEffect, allMatchingEffects);
			if (addNewEffect)
			{
				EffectResource resourceCopy = (EffectResource)resource.DuplicateDeep();
				resourceCopy._source = resource._source;
				_effects.Add(resourceCopy);
				Effect node = EffectManager.Apply(resourceCopy, _effectsNode);
				node.ConnectSignals(this);
				EmitSignal(SignalName.NewEffect, node);
				EmitSignal(SignalName.UpdateInfo);
				return node;
			}
			else
			{
				EmitSignal(SignalName.UpdateInfo);
				return null;
			}
		}
	}

	public EffectResource GetEffect(Type type)
	{
		return _effects.First(e => e.GetType() == type);
	}

	public void RemoveAllEffects()
	{
		foreach (Effect effect in _effectsNode.GetChildren())
		{
			effect.RemoveEffectNode();
		}
	}

	protected void SetWeapon()
	{
		if (HasNode("WeaponComponent"))
		{
			_weapon = GetNode<BaseWeapon>("WeaponComponent");
			_weapon.SetDisplayWeapon();
		}
		else
		{
			_weapon = null;
		}
	}

	protected void SetSelectionVisual()
	{
		_selectionVisual = GetNode<Sprite2D>("SelectionCircle");
	}

	public virtual void SetAttackRange()
	{
		if (_weapon is null)
		{
			return;
		}
		_attackCollisionShape = GetNode<CollisionShape2D>("AttackArea/AttackAreaCollision");
		_attackRange = _weapon.GetRange();

		// CRITICAL: Make the shape unique so changing this unit 
		// doesn't change every other unit of the same type.
		if (_attackCollisionShape.Shape is CircleShape2D circle)
		{
			circle = (CircleShape2D)circle.Duplicate();
			circle.Radius = _attackRange;
			if (IsInstanceValid(_attackCollisionShape))
			{
				_attackCollisionShape.Shape = circle;
			}
			//Callable.From(() =>
			//{
			//	if (IsInstanceValid(_attackCollisionShape))
			//	{
			//		_attackCollisionShape.Shape = circle;
			//	}
			//}).CallDeferred();
		}
		else
		{
			throw new Exception("Attack area shape is not a disk");
		}

		if (_attackTarget != null)
		{
			ScanForEnemies();
		}
	}

	protected void SetPathFinder()
	{
		_pathfinder = GetNode<UnitPathfinder>("UnitPathfinder");
		_pathfinder.SetSpeed(GetSpeed());
		_pathfinder.SetTeamId(_teamId);
	}

	protected void SetHealthBar()
	{
		_hp = GetHpMax();
		_healthBar = GetNode<TextureProgressBar>("HealthBar");

		UpdateHealthBar(_hp, GetHpMax(), _shield);

		
	}

	private void UpdateHpMax()
	{
		if (_healthBar != null)
		{
			UpdateHealthBar(_hp, GetHpMax(), _shield);
		}
	}

	public int GetHpMax()
	{
		return  (int)(_hpMax + _hpMaxModifier);
	}

	public float GetSpeed()
	{
		return (_moveSpeed + _data._speedIncrease) * (1 - _speedDebuff);
	}

	public int GetArmor()
	{
		return _armor + _armorModifier + _armorDebuff;
	}

	public virtual Godot.Collections.Dictionary<string, PanelContainer> MakeUnitInfoContainer()
	{
		_infoContainers = new();

		HideAttackRange();
		DisplayAttackRange();

		if (this is not Spawner)
		{
			PanelContainer basicInfo = new();
			basicInfo.CustomMinimumSize = new(200, 0);

			VBoxContainer basicInfoV = new();
			basicInfoV.Name = "VBoxContainer";
			basicInfo.CustomMinimumSize = new(200, 0);

			Label nameLabel = new();
			nameLabel.Text = _name;
			nameLabel.Name = "NameLabel";
			nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
			basicInfoV.AddChild(nameLabel);

			if (this is not TowerUnit)
			{
				if (_shield > 0)
				{
					RichTextLabel shieldLabel = new();
					shieldLabel.Text = "Shield: " + _shield.ToString() + "/" + GetHpMax().ToString();
					shieldLabel.Name = "ShieldLabel";
					shieldLabel.BbcodeEnabled = true;
					shieldLabel.FitContent = true;
					basicInfoV.AddChild(shieldLabel);
				}

				RichTextLabel hpLabel = new();
				hpLabel.Text = "Hp: " + _hp.ToString() + "/" + GetHpMax().ToString();
				hpLabel.Name = "HpLabel";
				hpLabel.BbcodeEnabled = true;
				hpLabel.FitContent = true;
				basicInfoV.AddChild(hpLabel);

				RichTextLabel speedLabel = new();
				speedLabel.Text = "Move speed: " + GetSpeed().ToString();
				speedLabel.Name = "SpeedLabel";
				speedLabel.BbcodeEnabled = true;
				speedLabel.FitContent = true;
				basicInfoV.AddChild(speedLabel);
			}


			if (this is InvaderUnit invader)
			{
				TooltipRichTextLabel moneyDropLabel = new();
				moneyDropLabel.Text = "Drops " + Utils.MakeMoneyText(invader.GetTotalMoneyDropped());
				moneyDropLabel.Name = "MoneyDropLabel";
				moneyDropLabel.AutowrapMode = TextServer.AutowrapMode.Off;
				moneyDropLabel.FitContent = true;
				moneyDropLabel.BbcodeEnabled = true;
				basicInfoV.AddChild(moneyDropLabel);

				TooltipRichTextLabel hpLossLabel = new();
				hpLossLabel.Text = $"Deducts {invader._hpDeducted} Hp";
				hpLossLabel.Name = "HpLossLabel";
				hpLossLabel.AutowrapMode = TextServer.AutowrapMode.Off;
				hpLossLabel.FitContent = true;
				hpLossLabel.BbcodeEnabled = true;
				basicInfoV.AddChild(hpLossLabel);
			}

			if (this is TowerUnit tower)
			{
				TooltipRichTextLabel descriptionLabel = new();
				descriptionLabel.Text = tower.GetDescription();
				descriptionLabel.Name = "DescriptionLabel";
				descriptionLabel.CustomMinimumSize = new(250, 0);
				descriptionLabel.FitContent = true;
				descriptionLabel.BbcodeEnabled = true;
				basicInfoV.AddChild(descriptionLabel);
			}

			basicInfo.AddChild(basicInfoV);

			_infoContainers.Add("BasicInfo", basicInfo);
		}

		if (_weapon != null)
		{
			PanelContainer weaponInfo = _weapon.MakeWeaponInfoContainer();

			TooltipRichTextLabel totalDamageLabel = new();
			totalDamageLabel.Text = "Damage dealt: " + _damageDealt;
			totalDamageLabel.Name = "TotalDamageLabel";
			totalDamageLabel.CustomMinimumSize = new(200, 0);
			totalDamageLabel.BbcodeEnabled = true;
			totalDamageLabel.FitContent = true;
			weaponInfo.GetNode<VBoxContainer>("VBoxContainer").AddChild(totalDamageLabel);

			_infoContainers.Add("WeaponInfo", weaponInfo);
		}

		bool hasDisplayEffects = false;
		foreach (EffectResource effect in _effects)
		{
			effect.SetDescription();
			if (effect._displayType == EffectResource.DisplayTypes.Large || effect._displayType == EffectResource.DisplayTypes.Small)
			{
				hasDisplayEffects = true;
			}
		}

		PanelContainer effectsInfo = new();

		HBoxContainer allEffectsH = new();

		VBoxContainer smallEffectsV = new();

		HBoxContainer largeEffectsH = new();

		foreach (EffectResource effect in _effects)
		{
			switch (effect._displayType)
			{
				case (EffectResource.DisplayTypes.Large):
					VBoxContainer container = new();
					PanelContainer effectName = effect.MakeFullEffectDescription();
					container.AddChild(effectName);
					largeEffectsH.AddChild(container);
					break;
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
		if (!hasDisplayEffects)
		{
			effectsInfo.Visible = false;
		}
		else
		{
			effectsInfo.Visible = true;
		}

		_infoContainers.Add("EffectsInfo", effectsInfo);

		return _infoContainers;
	}

	public virtual void UpdateUnitInfoContainer(bool updateEffects)
	{
		Callable.From(() =>
		{
			SetAttackRange();
			HideAttackRange();
			DisplayAttackRange();
		}).CallDeferred();


		if (this is not Spawner)
		{
			PanelContainer basicInfo = _infoContainers["BasicInfo"];
			basicInfo.CustomMinimumSize = new(200, 0);
			VBoxContainer basicInfoV = basicInfo.GetNode<VBoxContainer>("VBoxContainer");

			if (this is not TowerUnit)
			{
				RichTextLabel hpLabel = basicInfoV.GetNode<RichTextLabel>("HpLabel");
				hpLabel.Text = "Hp: " + _hp.ToString() + "/" + GetHpMax().ToString();

				if (_shield > 0)
				{
					if (basicInfoV.HasNode("ShieldLabel"))
					{
						RichTextLabel shieldLabel = basicInfoV.GetNode<RichTextLabel>("ShieldLabel");
						shieldLabel.Text = "Shield: " + _shield.ToString() + "/" + GetHpMax().ToString();
					}
					else
					{
						RichTextLabel shieldLabel = new();
						shieldLabel.Text = "Shield: " + _shield.ToString() + "/" + GetHpMax().ToString();
						shieldLabel.Name = "ShieldLabel";
						basicInfoV.AddChild(shieldLabel);
						basicInfoV.MoveChild(shieldLabel, 1);
					}
				}
				else
				{
					if (basicInfoV.HasNode("ShieldLabel"))
					{
						RichTextLabel shieldLabel = basicInfoV.GetNode<RichTextLabel>("ShieldLabel");
						shieldLabel.QueueFree();
					}
				}

				RichTextLabel speedLabel = basicInfoV.GetNode<RichTextLabel>("SpeedLabel");
				speedLabel.Text = "Move speed: " + GetSpeed().ToString();
			}

			if (this is InvaderUnit invader)
			{
				TooltipRichTextLabel moneyDropLabel = basicInfoV.GetNode<TooltipRichTextLabel>("MoneyDropLabel");
				moneyDropLabel.Text = "Drops " + Utils.MakeMoneyText(invader.GetTotalMoneyDropped());

				TooltipRichTextLabel hpLossLabel = basicInfoV.GetNode<TooltipRichTextLabel>("HpLossLabel");
				hpLossLabel.Text = $"Deducts {invader._hpDeducted} Hp";
			}
		}
		

		if (_infoContainers.Keys.Contains("WeaponInfo"))
		{
			_weapon.UpdateWeaponInfoContainer();
		}



		if (updateEffects)
		{
			bool hasDisplayEffects = false;
			foreach (EffectResource effect in _effects)
			{
				if (effect._displayType == EffectResource.DisplayTypes.Large || effect._displayType == EffectResource.DisplayTypes.Small)
				{
					hasDisplayEffects = true;
					break;
				}
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
						VBoxContainer container = new();
						PanelContainer effectName = effect.MakeFullEffectDescription();
						container.AddChild(effectName);
						largeEffectsH.AddChild(container);
						break;
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
			if (!hasDisplayEffects)
			{
				effectsInfo.Visible = false;
			}
			else
			{
				effectsInfo.Visible = true;
			}
		}
	}

	/// <summary>
	/// Edits _infoContainers so that upgraded stats are displayed in green. Note that this method should be called on a unit with the upgrade already applied on. 
	/// The upgrade resource that is inputted is only used to determine which stats were changed. 
	/// </summary>
	/// <param name="upgrade"></param>
	/// <exception cref="Exception"></exception>
	public virtual void UpdateUnitInfoContainerWithUpgrade(InvaderStatsIncreaseResource upgrade)
	{
		if (this is not InvaderUnit)
		{
			throw new Exception("attempting to give invader buff to non-invader");
		}
		PanelContainer basicInfo = _infoContainers["BasicInfo"];
		basicInfo.CustomMinimumSize = new(200, 0);
		VBoxContainer basicInfoV = basicInfo.GetNode<VBoxContainer>("VBoxContainer");
		string greenHex = ThemePalette.Green.ToHtml(false);

		if (upgrade._hpBuff != 0)
		{
			RichTextLabel hpLabel = basicInfoV.GetNode<RichTextLabel>("HpLabel");
			hpLabel.Text = $"[color=#{greenHex}]Hp: {GetHpMax()}/{GetHpMax()}[/color]";
		}

		if (upgrade._speedBuff != 0)
		{
			RichTextLabel speedLabel = basicInfoV.GetNode<RichTextLabel>("SpeedLabel");
			speedLabel.Text = $"[color=#{greenHex}]Move speed:{GetSpeed()}[/color]";
		}

		if (upgrade._moneyBuff != new Vector4I(0,0,0,0) || upgrade._startingEffects.Any(o => o.GetType() == typeof(SpawnUnitOnDeathResource)))
		{
			InvaderUnit invader = (InvaderUnit)this;
			TooltipRichTextLabel moneyDropLabel = basicInfoV.GetNode<TooltipRichTextLabel>("MoneyDropLabel");
			moneyDropLabel.Text = $"[color=#{greenHex}]Drops {Utils.MakeMoneyText(invader.GetTotalMoneyDropped())}[/color]"; 
		}

		if (upgrade._startingEffects.Count != 0)
		{
			Array<EffectResource> effectUpgrades = [];
			foreach (EffectResource resource in upgrade._startingEffects)
			{
				if (_effects.Any(e => e.GetType() == resource.GetType()))
				{
					effectUpgrades.Add(resource);
					break;
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
	}

	public virtual void UpdateUnitDamageDealtInfo()
	{
		if (_weapon != null)
		{
			VBoxContainer weaponInfoV = _infoContainers["WeaponInfo"].GetNode<VBoxContainer>("VBoxContainer");
			TooltipRichTextLabel totalDamageLabel = weaponInfoV.GetNode<TooltipRichTextLabel>("TotalDamageLabel");
			totalDamageLabel.Text = "Damage dealt: " + _damageDealt;
		}
	}

	public virtual void ResetUnitInfoContainer()
	{
		if (_infoContainers is null)
		{
			return;
		}
		foreach (PanelContainer panelContainer in _infoContainers.Values)
		{
			panelContainer.QueueFree();
		}
		_infoContainers.Clear();
	}

	public PanelContainer GetUnitInfoContainerWithString(string name)
	{
		Godot.Collections.Dictionary<string, PanelContainer> dict = MakeUnitInfoContainer();
		PanelContainer dictCopy = (PanelContainer)dict[name].Duplicate();
		foreach (PanelContainer panelContainer in dict.Values)
		{
			panelContainer.QueueFree();
		}
		return dictCopy;
	}

	public PanelContainer GetUnitInfoContainerWithUpgradeWithString(string name, InvaderStatsIncreaseResource upgrade)
	{
		Godot.Collections.Dictionary<string, PanelContainer> dict = MakeUnitInfoContainer();
		UpdateUnitInfoContainerWithUpgrade(upgrade);
		PanelContainer dictCopy = (PanelContainer)dict[name].Duplicate();
		foreach (PanelContainer panelContainer in dict.Values)
		{
			panelContainer.QueueFree();
		}
		return dictCopy;
	}

	protected void SetInitialCommand()
	{
		_currentCommand = new NoCommand(this);
	}

	public void SetSelectionVisible(bool b)
	{
		_selectionVisual.Visible = b;
		if (b)
		{
			DisplayAttackRange();
		}
		else
		{
			HideAttackRange();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_active)
		{
			return;
		}
		if (_pathfinder._snapToTarget)
		{
			_pathfinder.CheckSnapToTarget(delta);
		}
		if (_pathfinder.IsNavigationFinished())
		{
			_currentCommand.CheckFinish();
		}
		else if (_state != State.Attacking && !_navigationPaused)
		{
			_state = State.Moving;
			_pathfinder.ProcessMovement(delta);
		}

		if (_state == State.Attacking)
		{
			// switch target if unit is died
			CheckTargetAlive();
		}

		if (_currentCommand is ForceAttack forceAttack)
		{
			if (!forceAttack.CheckFinish())
			{
				_pathfinder.SetTargetPosition(forceAttack._targetUnit.GlobalPosition);
			}
		}
		else if (_currentCommand is AggroedAttackMove aggroedAttackMove)
		{
			if (!aggroedAttackMove.CheckFinish())
			{
				_pathfinder.SetTargetPosition(aggroedAttackMove._targetUnit.GlobalPosition);
			}
		}
		else if (_currentCommand is NoCommand)
		{
			ProcessNextCommand();
		}
	}

	public virtual void ProcessNextCommand()
	{
		Command command = new NoCommand(this);
		if (_commandQueue.Count > 0)
		{
			command = _commandQueue[0];
			_commandQueue.RemoveAt(0);
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

	public void PauseNavigation()
	{
		_navigationPaused = true;
	}

	public void ResumeNavigation()
	{
		_navigationPaused = false;
	}

	protected virtual void ProcessForceMove(ForceMove forceMove)
	{
		_currentCommand = forceMove;
		StopAttackingTarget();
		_pathfinder.SetTargetPosition(forceMove._targetLocation);
	}

	protected virtual void ProcessForceAttack(ForceAttack forceAttack)
	{
		_currentCommand = forceAttack;
		StopAttackingTarget();
		if (IsTargetInRange(forceAttack._targetUnit))
		{
			BeginAttackingTarget(forceAttack._targetUnit);
		}
		else
		{
			_pathfinder.SetTargetPosition(forceAttack._targetUnit.GlobalPosition);
		}	
	}

	protected virtual void ProcessAttackMove(AttackMove attackMove)
	{
		_currentCommand = attackMove;
		ScanForEnemies();
		_pathfinder.SetTargetPosition(attackMove._targetLocation);
	}

	protected virtual void ProcessAggroedAttackMove(AggroedAttackMove aggroedAttackMove)
	{
		_currentCommand = aggroedAttackMove;
		ScanForEnemies();
		_pathfinder.SetTargetPosition(aggroedAttackMove._targetUnit.GlobalPosition);
	}

	public void ClearAllCommands()
	{
		_commandQueue = new List<Command>();
		ProcessNextCommand();
	}

	public void AddCommand(Command command)
	{
		_commandQueue.Add(command);
	}

	public void InsertCommand(int n, Command command)
	{
		_commandQueue.Insert(n, command);
	}

	//private void Chase(Unit unit)
	//{
	//	if (unit._teamId != _teamId)
	//	{
	//		_pathfinder.SetChaseTarget(unit);
	//	}
	//}

	//private void StopMoving()
	//{
	//	_pathfinder.ForceFinishNavigation();
	//}

	protected void BeginAttackingTarget(Unit unit)
	{
		if (_state == State.Attacking)
		{
			throw new Exception("Already attacking a target!");
		}
		_state = State.Attacking;
		_attackTarget = unit;
		_weapon?.BeginAttackingTarget(unit);
		if (_weapon is not null)
		{
			EmitSignal(SignalName.BeginAttack, unit);
		}
	}

	protected void StopAttackingTarget()
	{
		Unit oldTarget = _attackTarget;
		_state = State.Idle;
		_attackTarget = null;
		_weapon?.StopAttackingTarget();
		if (_weapon is not null)
		{
			EmitSignal(SignalName.StopAttack, oldTarget);
		}
	}

	public void OnBeforeHitEnemy(Unit enemy)
	{
		EmitSignal(SignalName.BeforeHitEnemy, enemy);
	}

	public void OnHitEnemy(Unit enemy)
	{
		EmitSignal(SignalName.HitEnemy, enemy);
	}

	public void OnPlacedTower(TowerUnit tower)
	{
		EmitSignal(SignalName.PlacedTower, tower);
	}

	public void OnVolleyEnded()
	{
		EmitSignal(SignalName.VolleyEnded);
	}

	public void Hit(int damage, Unit source, bool ignoreArmor = false)
	{
		if (_isDead)
		{
			return;
		}
		EmitSignal(SignalName.BeforeIsHit, source);
		damage = (int)(damage * (1f + _damageTakenDebuff + _damageTakenModifier) * (1f - _damageReductionBuff));
		if (source is TowerUnit tower)
		{
			source.IncreaseDamageDealtStat(damage);
		}
		IncreaseHp(-damage, ignoreArmor);
		Area2D socialArea = GetNode<Area2D>("AidArea");
		var nearbyBodies = socialArea.GetOverlappingBodies();
		EmitSignal(SignalName.IsHit, source);

		foreach (var body in nearbyBodies)
		{
			// Check if the body is a Unit and on the same team
			if (body is Unit ally && ally._teamId == this._teamId && source is Unit)
			{
				ally.Retaliate(source);
			}
		}

		if (_hp <= 0)
		{
			Die();
			if (source is not null)
			{
				source.EmitSignal(Unit.SignalName.KilledUnit, this);
			}
		}
	}

	public void IncreaseHp(int change, bool ignoreArmor)
	{
		if (change < 0 && _shield > 0)
		{
			int newChange = Math.Min(change + _shield, 0);
			IncreaseShield(change);
			change = newChange;
		}
		if (change < 0 && !ignoreArmor)
		{
			change = Math.Min(change + GetArmor(), 0);
		}
		_hp += change;
		_hp = Math.Min(_hp, GetHpMax());
		UpdateHealthBar(_hp, GetHpMax(), _shield);
		EmitSignal(SignalName.HpChange, change);
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void IncreaseShield(int change)
	{
		_shield += change;
		_shield = Math.Min(_shield, GetHpMax());
		_shield = Math.Max(_shield, 0);
		UpdateHealthBar(_hp, GetHpMax(), _shield);
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void IncreaseArmorModifier(int change)
	{
		_armorModifier += change;
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void SetArmorDebuff(int debuff)
	{
		_armorDebuff = debuff;
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void IncreaseDamageTakenModifier(float change)
	{
		_damageTakenModifier += change;
	}

	public void SetDamageTakenDebuff(float debuff)
	{
		_damageTakenDebuff = debuff;
	}

	public void IncreaseDamageTakenDebuff(float debuff)
	{
		SetDamageTakenDebuff(debuff + _damageTakenDebuff);
	}

	public void SetDamageReductionBuff(float buff)
	{
		_damageReductionBuff = buff;
	}

	public void IncreaseDamageReductionBuff(float buff)
	{
		SetDamageReductionBuff(buff + _damageReductionBuff);
	}

	public void IncreaseDamageDealtStat(float change)
	{
		_damageDealt += change;
		EmitSignal(SignalName.UpdateDamageDealtInfo);
	}

	public void IncreaseMoneyGainedStat(Vector4I change)
	{
		_moneyGained += change;
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public virtual void Retaliate(Unit unit)
	{
		if (unit.CollisionLayer != UnitManager.UnitLayerMask)
		{
			return;
		}
		if (_currentCommand is AttackMove || _currentCommand is NoCommand)
		{
			InsertCommand(0, _currentCommand);
			InsertCommand(0, new AggroedAttackMove(this, GlobalPosition, unit));
			ProcessNextCommand();
		}
	}

	protected virtual void OnScanAreaBodyEntered(Node2D body)
	{
		if (!_active)
		{
			return;
		}
		if (_state == State.Attacking)
		{
			return;
		}
		if (_weapon is null)
		{
			return;
		}
		if (body is Unit unit)
		{
			// Check if the body is in the enemy 
			if (unit._teamId != _teamId &&  (_currentCommand is NoCommand || _currentCommand is AttackMove || _currentCommand is AggroedAttackMove))
			{
				BeginAttackingTarget(unit);
			}
			else if (_currentCommand is ForceAttack forceAttack && forceAttack._targetUnit == body)
			{
				BeginAttackingTarget(unit);
			}
		}
		else
		{
			return;
		}
	}

	protected virtual void OnScanAreaBodyLeft(Node2D body)
	{
		if (!_active)
		{
			return;
		}
		if (body is Unit unit && unit == _attackTarget)
		{
			StopAttackingTarget();
		}
		else
		{
			return;
		}
	}

	public void ScanForEnemies()
	{
		if (_state == State.Attacking && _lockOnTarget)
		{
			return;
		}
		if (_weapon is null)
		{
			return;
		}
		var scanArea = GetNode<Area2D>("AttackArea");

		// Get all overlapping physics bodies
		var bodies = scanArea.GetOverlappingBodies();
		if (!bodies.Any(body => body is Unit))
		{
			return;
		}
		Array<Unit> units = new Godot.Collections.Array<Unit>(bodies.OfType<Unit>());

		var sortedBodies = new List<Unit>();

		sortedBodies = FormTargetOrder(units);

		//GD.Print("List: ");
		//foreach (Node2D body in sortedBodies)
		//{
		//	float dist = GlobalPosition.DistanceSquaredTo(body.GlobalPosition);
		//	GD.Print($"Body: {body.Name}, DistSquared: {dist}");
		//}

		for (int i = 0; i < sortedBodies.Count; i++)
		{
			var body = sortedBodies[i];
			if (body is Unit unit)
			{
				if (unit._teamId != _teamId && (_currentCommand is NoCommand || _currentCommand is AttackMove || _currentCommand is AggroedAttackMove))
				{
					if (_state == State.Attacking && _attackTarget != unit)
					{
						StopAttackingTarget();
					}
					else if (_state == State.Attacking && _attackTarget == unit)
					{
						return;
					}
					BeginAttackingTarget(unit);
					break;
				}
			}
		}
	}

	protected virtual List<Unit> FormTargetOrder(Array<Unit> bodies)
	{
		return bodies
		.OrderBy(body => GlobalPosition.DistanceSquaredTo(body.GlobalPosition))
		.ToList();
	}

	public Array<Unit> GetEnemiesInRange()
	{
		var scanArea = GetNode<Area2D>("AttackArea");

		// Get all overlapping physics bodies
		var bodies = scanArea.GetOverlappingBodies();
		if (!bodies.Any(body => body is Unit))
		{
			return [];
		}
		Array<Unit> units = new Godot.Collections.Array<Unit>(bodies.OfType<Unit>());

		Array<Unit> enemies = [];
		foreach (Unit unit in units)
		{
			if (unit._teamId != _teamId)
			{
				enemies.Add(unit);
			}
		}
		return enemies;
	}

	protected void CheckTargetAlive()
	{
		if (!IsInstanceValid(_attackTarget))
		{
			StopAttackingTarget();
			ScanForEnemies();
		}
	}

	protected bool IsTargetInRange(Unit target)
	{
		var scanArea = GetNode<Area2D>("AttackArea");

		var bodies = scanArea.GetOverlappingBodies();
		foreach (Node2D body in bodies)
		{
			if (body == target)
			{
				return true;
			}
		}
		return false;
	}

	public void SetHpMaxModifier(int hpModifier)
	{
		int newMax = _hpMax + hpModifier;
		int change = newMax - _hpMax;
		_hp += change;
		_hpMaxModifier = hpModifier;
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void IncreaseHpMaxModifier(int change)
	{
		SetHpMaxModifier(_hpMaxModifier + change);
		UpdateHpMax();
	}

	public void SetWeaponModifier(int damageModifier)
	{
		_data._damageIncrease = damageModifier;
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void IncreaseWeaponModifier(int change, BaseWeapon.DamageType type = BaseWeapon.DamageType.Null)
	{
		if (_weapon is null)
		{
			return;
		}
		if (type == BaseWeapon.DamageType.Null || _weapon._damageType == type)
		{
			SetWeaponModifier(_data._damageIncrease + change);
		}
	}

	public void SetWeaponPercentModifier(float damagePercentModifier)
	{
		_data._damagePercentIncrease = damagePercentModifier;
		EmitSignal(SignalName.UpdateStatsInfo);

	}

	public void SetWeaponBuffPercent(float buff)
	{
		_weapon._damageBuffPercent = buff;
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void IncreaseWeaponBuffPercent(float increase)
	{
		SetWeaponBuffPercent(increase + _weapon._damageBuffPercent);
	}

	public void IncreaseWeaponPercentModifier(float change, BaseWeapon.DamageType type = BaseWeapon.DamageType.Null)
	{
		if (_weapon is null)
		{
			return;
		}
		if (type == BaseWeapon.DamageType.Null || _weapon._damageType == type)
		{
			SetWeaponPercentModifier(_data._damagePercentIncrease + change);
		}
	}

	public void SetWeaponRangeModifier(float range)
	{
		_data._rangeIncrease = range;
		SetAttackRange();
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void IncreaseWeaponRangeModifier(float change)
	{
		if (_weapon is null)
		{
			return;
		}
		SetWeaponRangeModifier(_data._rangeIncrease + change);
	}

	public void SetWeaponRangeDebuff(float debuff)
	{
		_weapon._rangeDebuffPercent = debuff;
		SetAttackRange();
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void SetSpeedModifier(float speed)
	{
		if (this is StationaryUnit)
		{
			return;
		}
		_data._speedIncrease = speed;
		SetPathFinder();
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void IncreaseSpeedModifier(float change)
	{
		SetSpeedModifier(_data._speedIncrease + change);
	}

	public void SetSpeedDebuff(float debuff)
	{
		_speedDebuff = debuff;
		SetPathFinder();
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void SetAttackSpeedModifier(double speed)
	{
		_data._attackSpeedIncrease = speed;
		EmitSignal(SignalName.UpdateStatsInfo);
		_weapon.ResetCooldown();
	}

	public void IncreaseAttackSpeedModifier(double change)
	{
		if (_weapon is null)
		{
			return;
		}
		SetAttackSpeedModifier(_data._attackSpeedIncrease + change);
	}

	public void SetAttackSpeedDebuff(double debuff)
	{
		if (_weapon is null)
		{
			return;
		}
		_weapon._attackSpeedDebuff = debuff;
		EmitSignal(SignalName.UpdateStatsInfo);
		_weapon.ResetCooldown();
	}

	public void SetAttackDelayModifier(double speed)
	{
		_data._attackDelayModifierIncrease = speed;
		EmitSignal(SignalName.UpdateStatsInfo);
	}

	public void IncreaseAttackDelayModifier(double change)
	{
		if (_weapon is null)
		{
			return;
		}
		SetAttackDelayModifier(_data._attackDelayModifierIncrease + change);
	}

	public void DisableAttackDelay()
	{
		if (_weapon is null)
		{
			return;
		}
		_weapon._useAttackDelay = false;
	}

	public void IncreasePierceCountModifier(int change)
	{
		if (_weapon is null)
		{
			return;
		}
		if (_weapon is not ProjectileWeapon)
		{
			return;
		}
		_data._pierceCount += change;
	}

	public void UpdateWeaponAttackZone(Shape2D shape)
	{
		if (_weapon is null || shape is null)
		{
			return;
		}
		if (_weapon is not ZoneWeapon)
		{
			return;
		}
		ZoneWeapon zWeapon = (ZoneWeapon)_weapon;
		zWeapon._shape = shape;
	}

	private void UpdateHealthBar(float currentHp, float maxHp, float shield)
	{
		float healthPercent = currentHp / maxHp;

		_healthBar.MaxValue = GetHpMax();
		float length = Math.Clamp(10f * (float)Math.Pow(GetHpMax(), 0.2f), _baseHealthBarWidth, _maxHealthBarWidth);
		_healthBar.Size = new Vector2(length, _baseHealthBarHeight);
		_healthBar.Position = new Vector2(-length / 2, - _healthBarOffset);

		_shieldBar = GetNode<TextureProgressBar>("ShieldBar");
		_shieldBar.MaxValue = GetHpMax();
		_shieldBar.Modulate = ThemePalette.Blue;
		_shieldBar.Size = new Vector2(length, _baseHealthBarHeight);
		_shieldBar.Position = new Vector2(-length / 2, -_healthBarOffset - _baseHealthBarHeight);

		_healthBar.Value = currentHp;

		// Transition from Green (0.33) to Red (0.0) using HSV
		// Or use a simple Lerp between two specific colors:
		Color healthyColor = ThemePalette.Green;
		Color criticalColor = ThemePalette.Red;

		// This blends the two colors based on the health percentage
		//_healthBar.Modulate = criticalColor.Lerp(healthyColor, healthPercent);
		_healthBar.TintProgress = healthyColor;

		if (_shield > 0)
		{
			float shieldPercent = shield / maxHp;
			_shieldBar.Value = shield;

			_shieldBar.Show();
		}
		else
		{
			if (_shieldBar.Visible)
			{
				_shieldBar.Hide();
			}
			
		}
	}

	protected virtual void Die()
	{
		EmitSignal(SignalName.Died);
		_isDead = true;
		SetProcess(false);
		SetPhysicsProcess(false);

		// 2. Disable collisions so other units don't bump into a corpse
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);

		QueueFree();
	}

	protected virtual void RemoveSelf()
	{
		SetProcess(false);
		SetPhysicsProcess(false);

		// 2. Disable collisions so other units don't bump into a corpse
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);

		EmitSignal(SignalName.Removed);

		QueueFree();
	}

	public override void _Draw()
	{
		if (_displayAttackRange)
		{
			// Get the radius from your CollisionShape2D's resource
			var attackAreaShape = GetNode<CollisionShape2D>("AttackArea/AttackAreaCollision").Shape as CircleShape2D;

			if (attackAreaShape != null)
			{
				Color drawColor = ThemePalette.Yellow; // Light blue, semi-transparent
				drawColor.A = 0.5f;
				DrawCircle(Vector2.Zero, attackAreaShape.Radius, drawColor);
			}
		}
	}

	public virtual void DisplayAttackRange()
	{
		if (_weapon is null)
		{
			return;
		}
		_displayAttackRange = true;
		UpdateVisualRange();
	}

	public virtual void HideAttackRange()
	{
		_displayAttackRange = false;
		UpdateVisualRange();
	}

	// Call this whenever the range changes to update the visual
	public void UpdateVisualRange()
	{
		QueueRedraw();
	}

	public void DisablePhysicsProcess()
	{
		_active = false;
	}

	public void EnablePhysicsProcess()
	{
		_active = true;
	}

	public UnitPathfinder GetPathfinder()
	{
		return _pathfinder;
	}

	public virtual Texture2D GetIconTexture()
	{
		if (HasNode("AnimatedSprite2D"))
		{
			return GetNode<AnimatedSprite2D>("AnimatedSprite2D").SpriteFrames.GetFrameTexture("default", 0);
		}
		else
		{
			return GetNode<Sprite2D>("MainSprite").Texture;
		}
	}
}
