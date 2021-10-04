using System.Collections.Generic;
using System.Text.Json;
using Sandbox;

[Library( "ent_stargate_universe_ramp", Title = "Universe Ramp", Spawnable = true, Group = "Stargate" )]
public partial class UniverseRamp : ModelEntity, IStargateRamp, IGateSpawner
{

	[Net]
	public Vector3 SpawnOffset { get; private set; } = new( 0, 0, 0 );
	public int AmountOfGates => 1;

	public Vector3[] StargatePositionOffset => new Vector3[] {
		new Vector3( -108.75f, 0, 135 )
	};

	public Angles[] StargateRotationOffset => new Angles[] {
		Angles.Zero
	};

	public List<Stargate> Gate { get; set; } = new();

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Default;

		SetModel( "models/sbox_stargate/ramps/sgu_ramp/sgu_ramp.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
	}

	public void FromJson( JsonElement data )
	{
		Position = Vector3.Parse( data.GetProperty( "Position" ).ToString() );
		Rotation = Rotation.Parse( data.GetProperty( "Rotation" ).ToString() );

		PhysicsBody.BodyType = PhysicsBodyType.Static;
	}

	public object ToJson()
	{
		return new JsonModel()
		{
			EntityName = ClassInfo.Name,
			Position = Position,
			Rotation = Rotation
		};
	}
}
