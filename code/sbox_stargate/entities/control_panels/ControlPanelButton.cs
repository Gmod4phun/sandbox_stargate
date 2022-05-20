using System;
using Sandbox;

public partial class ControlPanelButton : AnimEntity, IUse
{

	public bool On { get; set; } = false;

	public Action<Entity> OnUseCallback;

	public override void Spawn()
	{
		base.Spawn();
		Tags.Add( "no_rings_teleport" );
		Transmit = TransmitType.Always;
	}
	public bool OnUse( Entity ent )
	{
		if ( On ) return false;
		ToggleButton();
		if ( OnUseCallback is not null )
			OnUseCallback( ent );
		return false;
	}

	public async void ToggleButton()
	{
		On = true;
		PlaySound( "goauld_button2" );
		await Task.Delay( 500 );
		On = false;
	}

	public bool IsUsable( Entity ent ) => true;

	public void ButtonGlowLogic()
	{
		SetMaterialGroup( On ? 1 : 0 );
	}

	[Event.Tick.Server]
	public void OnTick()
	{
		Scale = Parent.Scale;
		ButtonGlowLogic();
	}

}
