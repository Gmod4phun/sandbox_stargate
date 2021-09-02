using Sandbox;

public partial class DhdButton : AnimEntity
{
	[Net]
	public string Action { get; set; } = "";
	public DhdButtonTrigger Trigger;

	[Net]
	public bool On { get; set; } = false;

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}

	[Event.Frame]
	public void ButtonGlowLogic()
	{
		SetMaterialGroup( On ? 1 : 0 );
	}
}
