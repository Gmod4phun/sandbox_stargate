using System.Numerics;
using System;
using Sandbox;
using System.Collections.Generic;

[Library( "ent_sgc_ramp", Title = "SGC Ramp", Spawnable = true, Group = "Stargate" )]
public partial class SGCRamp : ModelEntity, IStargateRamp
{
	[Net]
	public Vector3 SpawnOffset { get; private set; } = new( 0, 0, 148 );

	public int AmountOfGates => 1;

	public Vector3[] StargatePositionOffset => new Vector3[] {
		Vector3.Zero
	};

	public Angles[] StargateRotationOffset => new Angles[] {
		Angles.Zero
	};

	public List<Stargate> Gate { get; set; } = new();

	public override void Spawn()
	{

		base.Spawn();

		Transmit = TransmitType.Default;

		SetModel("models/sbox_stargate/ramps/sgc_ramp/sgc_ramp.vmdl");
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
	}
}
