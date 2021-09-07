using System.Linq;
using Sandbox;

[Library( "ent_rings_goauld", Title = "Rings (Goa'uld)", Spawnable = true, Group = "Stargate" )]
public partial class RingsGoauld : Rings {
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/gmod4phun/stargate/rings_ancient/ring_ancient.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
	}

}
