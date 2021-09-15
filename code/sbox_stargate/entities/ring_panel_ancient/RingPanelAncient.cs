using Sandbox;

[Library( "ent_rings_panel_ancient", Title = "Rings Panel (Ancient)", Spawnable = true, Group = "Stargate.Stargate" )]
public partial class RingPanelAncient : RingPanel {

	protected override string[] ButtonsSounds { get; } = { "ancient_button1", "ancient_button2" };

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/gmod4phun/stargate/rings_panel/ancient/ring_panel_ancient.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

		PhysicsBody.BodyType = PhysicsBodyType.Static;

		CreateButtons();
	}

	public virtual void CreateButtons() // visible models of buttons that turn on/off and animate
	{
		for ( var i = 1; i <= 9; i++ )
		{
			var button = new RingPanelButton();
			button.SetModel( $"models/gmod4phun/stargate/rings_panel/ancient/ring_panel_ancient_button_{i}.vmdl" );
			button.SetupPhysicsFromModel( PhysicsMotionType.Static, true ); // needs to have physics for traces
			button.PhysicsBody.BodyType = PhysicsBodyType.Static;
			button.EnableAllCollisions = false; // no collissions needed
			button.EnableTraceAndQueries = true; // needed for Use

			button.Position = Position;
			button.Rotation = Rotation;
			button.Scale = Scale;
			button.SetParent( this );

			var action = (i == 9) ? "DIAL" : i.ToString();
			button.Action = action;
			button.RingPanel = this;
			Buttons.Add( action, button );
		}
	}
}
