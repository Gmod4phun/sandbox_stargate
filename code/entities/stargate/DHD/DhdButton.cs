using Sandbox;

[Library( "ent_dhd_button", Title = "DHD Button", Spawnable = false )]
public partial class DhdButton : Prop, IUse {

	[Net]
	[Property(Name = "On", Group = "Stargate")]
	public bool On { get; set; } = false;

	public override void Spawn() {
		base.Spawn();

		Transmit = TransmitType.Always;

		SetMaterialGroup(0);
	}

	public virtual bool OnUse(Entity ent) {
		On = !On;
		return false;
	}

	public virtual bool IsUsable(Entity ent) {
		return true;
	}

	[Event.Tick]
	public void ButtonThink() {
		if ( IsServer ) {
			int group = On ? 1 : 0;
			if ( GetMaterialGroup() != group ) {
				SetMaterialGroup( group );
			}
		}
	}

}
