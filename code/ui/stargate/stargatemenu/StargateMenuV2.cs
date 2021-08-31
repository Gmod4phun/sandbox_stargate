using Sandbox;
using Sandbox.UI;

[UseTemplate]
public class StargateMenuV2 : Panel {

	private Stargate Gate;

	private string _gateAddress = "";
	public string GateAddress {
		get {
			return _gateAddress;
		}
		set {
			if (_gateAddress == value)
				return;
			_gateAddress = value;
			if (_gateAddress.Length == 7)
				Stargate.RequestAddressChange(Gate.NetworkIdent, _gateAddress);
		}
	}

	private string _gateName = "";
	public string GateName {
		get {
			return _gateName;
		}
		set {
			if (_gateName == value)
				return;
			_gateName = value;
			Stargate.RequestNameChange(Gate.NetworkIdent, _gateName);
		}
	}

	public string GateGroup { get; set; }

	public bool IsPrivate;

	public bool IsLocal;

	public bool Autoclose;

	public string DialAddress { get; set; }

	private Titlebar menuBar;

	public StargateMenuV2() {

		StyleSheet.Load( "ui/stargate/stargatemenu/StargateMenuV2.scss" );

		menuBar = AddChild<Titlebar>();
		menuBar.SetTitle(true, "Stargate");
		menuBar.SetCloseButton( true, "×", () => this.Delete() );
	}

	public override void Tick()
	{
		base.Tick();
		
		// closes menu if player goes too far -- in the future we will want to freeze player's input
		var dist = Local.Pawn.Position.Distance( Gate.Position );
		if ( dist > 220 * Gate.Scale ) Delete();
	}

	public void SetGate(Stargate gate) {
		this.Gate = gate;
		RefreshGateInformation();
	}

	[Event("stargate.refreshgateinformation")]
	private void RefreshGateInformation() {
		GateAddress = Gate.Address;
		GateName = Gate.Name;
		GateGroup = Gate.Group;
	}

	public void OpenGate() {
		Stargate.RequestDial(DialType.FAST, DialAddress, Gate.NetworkIdent);
	}

	public void CloseGate() {
		Stargate.RequestClose(Gate.NetworkIdent);
	}

}
