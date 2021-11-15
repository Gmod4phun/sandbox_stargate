using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

[Library]
public class SkinChangingRampController : RampController
{
	public override void OnGateStateChanged()
	{
		Entity.SetMaterialGroup( Stargate.CurGateState != Stargate.GateState.IDLE ? 1 : 0 );
	}
}
