using Sandbox;

[Library( "ent_dhd_button", Title = "DHD Button", Spawnable = false )]
public partial class DHDButton : AnimEntity, IUse
{
	[Net]
	[Property( Name = "On", Group = "Stargate" )]
	public bool On { get; set; } = false;
	public char Symbol;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		On = false;
	}

	public void PlayPressAnim()
	{
		CurrentSequence.Name = "idle_pressed";
	}

	public virtual bool OnUse(Entity ent)
	{
		On = !On;

		Log.Info(Symbol);
		PlayPressAnim();

		return false;
	}

	public virtual bool IsUsable(Entity ent)
	{
		return true;
	}

	[Event.Tick]
	public void ButtonThink() // probably will use a different logic, I will need to think about this
	{
		if ( IsServer )
		{
			var group = On ? 1 : 0;
			if ( GetMaterialGroup() != group ) SetMaterialGroup( group );
		}
	}

}
