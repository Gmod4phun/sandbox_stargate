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

	public void SetGate(Stargate gate) {
		this.Gate = gate;
		RefreshGateInformations();
	}

	[Event("stargate.refreshgateinformations")]
	private void RefreshGateInformations() {
		GateAddress = Gate.Address;
		GateName = Gate.Name;
		GateGroup = Gate.Group;
	}

	public void OpenGate() {
		Stargate.RequestDial(DialType.FAST, DialAddress, Gate.NetworkIdent);
	}

	public void CloseGate() {
		Stargate.RequestClose(this.Gate.NetworkIdent);
	}

}
