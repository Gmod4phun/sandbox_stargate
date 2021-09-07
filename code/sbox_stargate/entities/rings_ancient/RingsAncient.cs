using System.Linq;
using System;
using System.Collections.Generic;
using Sandbox;

[Library( "ent_rings_ancient", Title = "Ancient Rings", Spawnable = true, Group = "Stargate" )]
public partial class RingsAncient : Rings {
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/gmod4phun/stargate/rings_ancient/ring_ancient_cover.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
	}

}
