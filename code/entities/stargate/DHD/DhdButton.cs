using Sandbox;

public partial class DhdButton : AnimEntity
{
	public string Action;
	public DhdButtonTrigger Trigger;

	[Net]
	public bool Glowing { get; set; } = false;

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}

	[Event.Frame]
	public void ButtonGlowLogic()
	{
		SetMaterialGroup( Glowing ? 1 : 0 );
	}
}
