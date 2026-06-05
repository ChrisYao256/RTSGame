using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class ExplosionOnHitBuff : Effect
{
	ExplosionOnHitBuffResource _resource;

	public ExplosionOnHitBuff(ExplosionOnHitBuffResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		OnCreation();
	}

	protected override void OnCreation()
	{
		ExplosionOnHitResource resource = (ExplosionOnHitResource)_parentUnit.GetEffect(typeof(ExplosionOnHitResource));
		resource._explosionDamage += _resource._explosionDamage;
		resource._explosionRadius += _resource._explosionRadius;
		resource.SetDescription();
	}
}
