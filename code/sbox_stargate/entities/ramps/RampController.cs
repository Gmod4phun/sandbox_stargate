using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public abstract partial class RampController : EntityComponent<Ramp>
{
	public Stargate Stargate => Entity.Gates.Count == 0 ? null : Entity.Gates[0];
	private Stargate.GateState oldState = Stargate.GateState.IDLE;

	[Event.Tick.Server]
	public void BaseTick()
	{
		if ( Stargate == null || !Stargate.IsValid )
			return;
		if (oldState != Stargate.CurGateState )
		{
			oldState = Stargate.CurGateState;
			OnGateStateChanged();
		}
		Tick();
	}

	public virtual void Tick()
	{
		//nothing lol
	}
	public virtual void OnGateStateChanged()
	{

	}
}
