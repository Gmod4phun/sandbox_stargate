using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.Html;
using Sandbox.UI;
using Sandbox.UI.Tests;
using static Stargate;

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

	public bool FastDial { get; set; } = true;

	public string DialAddress { get; set; }

	private string _searchFilter = "";
	public string SearchFilter {
		get => _searchFilter;
		set {
			if (_searchFilter == value)
				return;

			_searchFilter = value;

			FillGates("true");
		}
	}

	private Titlebar menuBar;

	public StargateMenuV2() {

		StyleSheet.Load( "sbox_stargate/ui/stargatemenu/StargateMenuV2.scss" );

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
		FillGates(false);
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

	public void FillGates(bool refresh = false) {
		Table table = GetTable();
		// table.Rows.DeleteChildren(true);
		if (refresh)
			table.Rows.Clear();
		else {
			table.Rows.Layout.Columns = 1;
			table.Rows.Layout.ItemSize = new Vector2(-1, 30);
			table.Rows.OnCreateCell = ( cell, data ) =>
			{
				var gate = (Stargate)data;
				var panel = cell.Add.Panel( "row" );
				panel.AllowChildSelection = true;
				var td = panel.Add.Panel( "td stargate-font concept" );
				td.AddChild<Label>().Text = gate.Address;
				td = panel.Add.Panel( "td" );
				td.AddChild<Label>().Text = gate.Address;
				td = panel.Add.Panel( "td" );
				td.AddChild<Label>().Text = gate.Name;

				panel.AddEventListener( "onclick", () => {
					DialAddress = gate.Address;
				});

				panel.AddEventListener( "ondoubleclick", () => {
					DialAddress = gate.Address;
					OpenGate();
				});
			};
		}

		List<Stargate> gates = Entity.All.OfType<Stargate>().Where(x => x.Address != Gate.Address && !x.Private).ToList();

		if (SearchFilter != null && SearchFilter != "") {
			gates = gates.Where( x => x.Address.Contains(SearchFilter) || (x.Name != null && x.Name != "" && x.Name.Contains(SearchFilter)) ).ToList();
		}

		foreach (Stargate gate in gates) {
			table.Rows.AddItem(gate);
		}
	}

	// Needed for HTML Template
	public void FillGates(string refresh = "false") {
		FillGates(refresh == "true");
	}

	public void OpenGate() {
		Stargate.RequestDial(FastDial ? DialType.FAST : DialType.SLOW, DialAddress, Gate.NetworkIdent);
	}

	public void CloseGate() {
		Stargate.RequestClose(Gate.NetworkIdent);
	}

}
