using System.Linq;
using Sandbox;

[Library( "ent_rings_goauld", Title = "Rings (Goa'uld)", Spawnable = true, Group = "Stargate.Stargate" )]
public partial class RingsGoauld : Rings {
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/sbox_stargate/rings_ancient/ring_ancient.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
	}

	protected override void HideBase() {}
	protected override void ShowBase() {}
}
