using Sandbox;

[Library( "ent_rings_panel_goauld", Title = "Rings Panel (Goa'uld)", Spawnable = true, Group = "Stargate.Stargate" )]
public partial class RingPanelGoauld : RingPanel {

	protected override int DialButtonNumber => 6;

	protected override int AmountOfButtons => 6;

	protected override string[] ButtonsSounds { get; } = {
		"goauld_button1",
		"goauld_button2"
	};

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/gmod4phun/stargate/rings_panel/goauld/goauld.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

		PhysicsBody.BodyType = PhysicsBodyType.Static;
	}

}
