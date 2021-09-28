using System.Collections.Generic;
using System;
using Sandbox;

[Library( "ent_dhd_pegasus", Title = "DHD (Pegasus)", Spawnable = true, Group = "Stargate.Stargate" )]
public partial class DhdPegasus : Dhd
{
	public Vector3 SpawnOffset { get; private set; } = new( 0, 0, -5 );
	public Angles SpawnOffsetAng { get; private set; } = new( 15, 0, 0 );

	public DhdPegasus()
	{
		Data = new( 2, 3, "dhd_press_atlantis", "dhd_dial" );
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/sbox_stargate/dhd/dhd.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;

		SetMaterialGroup( 1 );

		CreateButtonTriggers();
		CreateButtons();
	}
}
