using Godot;
using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class GlobalMultiEffect : GlobalEffect
{
	GlobalMultiEffectResource _resource;

	public GlobalMultiEffect(GlobalMultiEffectResource resource) : base(resource)
	{
		_resource = resource;
	}

	public override void OnCreation()
	{
		QueueFree();
	}
}
