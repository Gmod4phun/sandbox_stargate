using Sandbox;

[Library( "ent_rings_panel_ancient", Title = "Rings Panel (Ancient)", Spawnable = true, Group = "Stargate.Stargate" )]
public partial class RingPanelAncient : RingPanel {

	protected override int DialButtonNumber => 9;

	protected override int AmountOfButtons => 9;

	protected override string[] ButtonsSounds { get; } = {
		"ancient_button1",
		"ancient_button2"
	};

	protected override float TraceDistance => 1.4f;

	protected override float[][] ButtonsPositions => new float[][] {
		new float[2] {49.5f, 30f},
		new float[2] {49.5f, 61f},
		new float[2] {86f, 46f},
		new float[2] {122.5f, 30f},
		new float[2] {122.5f, 61f},
		new float[2] {159, 46f},
		new float[2] {195.5f, 22f},
		new float[2] {195.5f, 46.5f},
		new float[2] {198.5f, 66f},
	};

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/gmod4phun/stargate/rings_panel/ancient/ancient.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

		PhysicsBody.BodyType = PhysicsBodyType.Static;
	}
}
