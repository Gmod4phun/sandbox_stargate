using Sandbox;

[Library( "ent_dhd_dial_button", Title = "DHD Dial Button", Spawnable = false )]
public partial class DHDButtonDial : DHDButton
{

	private Stargate Gate
	{
		get
		{
			return Parent is DHD dhd ? dhd.Gate : null;
		}
	}

	public override bool OnUse(Entity ent)
	{
		if ( Gate.IsValid() )
		{
			if ( (Gate.Active && Gate.Open) && !Gate.Inbound) {
				Gate.DoStargateClose(true);
				return false;
			}
			// Gate.OpenGateMenu();
		}
		else
		{
			On = !On;
		}
		
		return false;
	}

}
