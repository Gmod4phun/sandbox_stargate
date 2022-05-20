using Sandbox;

[Library( "ent_goauld_control_panel", Title = "Goauld Control Panel", Spawnable = true, Group = "Stargate" )]
public partial class GoauldControlPanel : ControlPanelBase
{

	protected override string ButtonsPath => "models/sbox_stargate/rings_panel/goauld/ring_panel_goauld_button_";
	protected override int AmountOfButtons => 6;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/sbox_stargate/rings_panel/goauld/ring_panel_goauld.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

}
