using System.Linq;
using Sandbox;
using Sandbox.Html;
using Sandbox.UI;
using Sandbox.UI.Tests;

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

	private bool _isPrivate = false;
	public bool IsPrivate {
		get {
			return _isPrivate;
		}
		set {
			if (_isPrivate == value)
				return;

			_isPrivate = value;

			Stargate.SetPrivacy(Gate.NetworkIdent, _isPrivate);
		}
	}

	public bool IsLocal;

	private bool _autoClose = true;
	public bool AutoClose {
		get {
			return _autoClose;
		}
		set {
			if (_autoClose == value)
				return;

			_autoClose = value;

			Stargate.SetAutoClose(Gate.NetworkIdent, _autoClose);
		}
	}

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
		FillGates();
		RefreshGateInformation();
	}

	[Event("stargate.refreshgateinformation")]
	private void RefreshGateInformation() {
		GateAddress = Gate.Address;
		GateName = Gate.Name;
		GateGroup = Gate.Group;
		AutoClose = Gate.AutoClose;
		IsPrivate = Gate.Private;
	}

	private Table GetTable() {
		Table table = null;
		foreach (Panel c in Children) {
			var tables = c.ChildrenOfType<Table>();
			if ( tables.Any() ) {
				table = tables.First();
				break;
			}
		}

		return table;
	}

	public void FillGates() {
		Table table = GetTable();
		table.Rows.DeleteChildren(true);
		table.Rows.Layout.Columns = 1;
		table.Rows.Layout.ItemSize = new Vector2(-1, 30);
		table.Rows.OnCreateCell = ( cell, data ) =>
		{
			var gate = (Stargate)data;
			var panel = cell.Add.Panel( "row" );
			panel.AllowChildSelection = true;
			var td = panel.Add.Panel( "td stargate-font sg1" );
			td.AddChild<Label>().Text = gate.Address;
			td = panel.Add.Panel( "td" );
			td.AddChild<Label>().Text = gate.Address;
			td = panel.Add.Panel( "td" );
			td.AddChild<Label>().Text = gate.Name;

			panel.AddEventListener( "onclick", () => {
				DialAddress = gate.Address;
			});

			panel.AddEventListener( "ondoubleclick", () => {
				Stargate.RequestDial(DialType.FAST, gate.Address, Gate.NetworkIdent);
			});
		};

		Stargate[] gates = Entity.All.OfType<Stargate>().Where(x => x.Address != Gate.Address && !x.Private).ToArray();

		foreach (Stargate gate in gates) {
			table.Rows.AddItem(gate);
		}
	}

	public void OpenGate() {
		Stargate.RequestDial(DialType.FAST, DialAddress, Gate.NetworkIdent);
	}

	public void CloseGate() {
		Stargate.RequestClose(Gate.NetworkIdent);
	}

}
