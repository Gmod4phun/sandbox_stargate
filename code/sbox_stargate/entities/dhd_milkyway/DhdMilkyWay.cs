using System.Collections.Generic;
using System;
using Sandbox;

[Library( "ent_dhd_milkyway", Title = "DHD (Milky Way)", Spawnable = true )]
public partial class DhdMilkyWay : Dhd
{
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/sbox_stargate/dhd/dhd.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;

		CreateButtonTriggers();
		CreateButtons();
	}
}
