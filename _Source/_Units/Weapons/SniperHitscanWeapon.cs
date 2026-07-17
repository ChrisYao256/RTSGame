using RTSGame.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class SniperHitscanWeapon : HitscanWeapon
{
	public override void _Ready()
	{
		base._Ready();
		_hasCustomPriority = true;
	}

	public override List<InvaderUnit> FormCustomTargetOrder(List<InvaderUnit> units)
	{
		return units.OrderBy(body => body._hp).ToList();
	}
}

