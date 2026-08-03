using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;
public partial class GlobalGetHp : GlobalEffect
{
	private GlobalGetHpResource _resource;

	public GlobalGetHp(GlobalGetHpResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void OnCreation()
	{
		_tdManager.IncreaseHp(_resource._hp);
	}

	protected override void OnNewWave()
	{
		if (_resource._everyTurn)
		{
			_tdManager.IncreaseHp(_resource._hp);
		}
	}
}