using Sandbox;

[Library( "ent_rings_panel_goauld", Title = "Rings Panel (Goa'uld)", Spawnable = true, Group = "Stargate.Stargate" )]
public partial class RingPanelGoauld : RingPanel {

	protected override int DialButtonNumber => 6;

	protected override int AmountOfButtons => 6;

	protected override string[] ButtonsSounds { get; } = {
		"goauld_button1",
		"goauld_button2"
	};

	protected override float[][] ButtonsPositions => new float[][] {
		new float[2] {121f, 13.5f},
		new float[2] {121f, 79f},
		new float[2] {151f, 13.5f},
		new float[2] {151f, 79f},
		new float[2] {181f, 13.5f},
		new float[2] {184f, 74f},
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
