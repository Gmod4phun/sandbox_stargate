using Sandbox;

[Library( "ent_rings_panel_ancient", Title = "Rings Panel (Ancient)", Spawnable = true, Group = "Stargate.Stargate" )]
public partial class RingPanelAncient : RingPanel {

	protected override int DialButtonNumber => 9;

	protected override int AmountOfButtons => 9;

	protected override string[] ButtonsSounds { get; } = {
		"goauld_button1",
		"goauld_button2"
	};

	protected override float TraceDistance => 1.4f;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/gmod4phun/stargate/rings_panel/ancient/ancient.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

		PhysicsBody.BodyType = PhysicsBodyType.Static;
	}

}
