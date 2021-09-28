using Sandbox;

public partial class DhdButton : AnimEntity
{
	[Net]
	public string Action { get; set; } = "";
	[Net]
	public Dhd DHD { get; set; } = null;
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
		if (DHD.IsValid())
		{
			SetMaterialGroup( On ? DHD.Data.ButtonSkinOn : DHD.Data.ButtonSkinOff );
		}
	}
}
