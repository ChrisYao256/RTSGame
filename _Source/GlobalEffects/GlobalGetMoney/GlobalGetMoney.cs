using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;
public partial class GlobalGetMoney : GlobalEffect
{
	private GlobalGetMoneyResource _resource;

	public GlobalGetMoney(GlobalGetMoneyResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void OnCreation()
	{
		if (_noImmediateEffect) return;
		if (_resource._percent != 0)
		{
			_tdManager.GainMoney(Utils.VectorScalarMultiplication(_tdManager._money, _resource._percent));
		}
		else
		{
			_tdManager.GainMoney(_resource._money);
		}
			
	}
}