using System.Collections.Generic;
using System;
using Sandbox;

[Library( "ent_dhd_sg1", Title = "DHD (Milky Way)", Spawnable = true )]
public partial class DHD_SG1 : DHD
{
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/gmod4phun/stargate/dhd/dhd.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;

		CreateButtons();
	}
}
