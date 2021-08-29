﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class Chevron : AnimEntity
{
	public bool Glowing;

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;

		SetModel( "models/gmod4phun/stargate/gate_sg1/chevron.vmdl" );
	}

	public void ChevronLock()
	{
		CurrentSequence.Name = "idle_locked";
	}

	public void ChevronUnlock()
	{
		CurrentSequence.Name = "idle";
	}

	public void ChevronLockUnlock()
	{
		CurrentSequence.Name = "lock_unlock_long";
	}

	[Event( "server.tick" )]
	public void ChevronThink( )
	{
		var group = Glowing ? 1 : 0;
		if ( GetMaterialGroup() != group ) SetMaterialGroup( group );
	}

}
