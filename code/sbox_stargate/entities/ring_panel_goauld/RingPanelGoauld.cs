using Sandbox;

[Library( "ent_rings_panel_goauld", Title = "Rings Panel (Goa'uld)", Spawnable = true, Group = "Stargate.Rings" )]
public partial class RingPanelGoauld : RingPanel {

	protected override string[] ButtonsSounds { get; } = { "goauld_button1", "goauld_button2" };

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/sbox_stargate/rings_panel/goauld/ring_panel_goauld.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

		PhysicsBody.BodyType = PhysicsBodyType.Static;

		CreateButtons();
	}

	public virtual void CreateButtons() // visible models of buttons that turn on/off and animate
	{
		for ( var i = 1; i <= 6; i++ )
		{
			var button = new RingPanelButton();
			button.SetModel( $"models/sbox_stargate/rings_panel/goauld/ring_panel_goauld_button_{i}.vmdl" );
			button.SetupPhysicsFromModel( PhysicsMotionType.Static, true ); // needs to have physics for traces
			button.PhysicsBody.BodyType = PhysicsBodyType.Static;
			button.EnableAllCollisions = false; // no collissions needed
			button.EnableTraceAndQueries = true; // needed for Use

			button.Position = Position;
			button.Rotation = Rotation;
			button.Scale = Scale;
			button.SetParent( this );

			var action = (i == 6) ? "DIAL" : i.ToString();
			button.Action = action;
			button.RingPanel = this;
			Buttons.Add( action, button );
		}
	}
}
