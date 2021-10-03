using Sandbox;

[Library( "ent_rings_ancient", Title = "Rings (Ancient)", Spawnable = true, Group = "Stargate.Rings" )]
public partial class RingsAncient : Rings {
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/sbox_stargate/rings_ancient/ring_ancient_cover.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;
	}

}
