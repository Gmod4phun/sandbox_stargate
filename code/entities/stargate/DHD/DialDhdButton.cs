using Sandbox;

[Library( "ent_dhd_dial_button", Title = "Dial DHD Button", Spawnable = false )]
public partial class DialDhdButton : DhdButton {

	private Stargate Gate {
		get {
			return Parent is Dhd ? (Parent as Dhd).Gate : null;
		}
	}

	public override bool OnUse(Entity ent) {
		if (Gate != null) {
			if ((Gate.Active || Gate.Open) && !Gate.Inbound) {
				Gate.DoStargateClose(true);
				return false;
			}
			// Gate.OpenGateMenu();
		} else {
			On = !On;
		}
		
		return false;
	}

}
