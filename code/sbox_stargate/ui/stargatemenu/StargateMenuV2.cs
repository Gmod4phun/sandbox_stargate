using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.Html;
using Sandbox.UI;
using Sandbox.UI.Tests;
using static Stargate;

[UseTemplate]
public class StargateMenuV2 : Panel
{

	private Stargate Gate;
	private Dhd DHD;

	private string _gateAddress = "";
	public string GateAddress
	{
		get
		{
			return _gateAddress;
		}
		set
		{
			if ( _gateAddress == value )
				return;
			_gateAddress = value;
			if ( _gateAddress.Length == 6 )
				Stargate.RequestAddressChange( Gate.NetworkIdent, _gateAddress );
		}
	}

	private string _gateName = "";
	public string GateName
	{
		get
		{
			return _gateName;
		}
		set
		{
			if ( _gateName == value )
				return;
			_gateName = value;
			Stargate.RequestNameChange( Gate.NetworkIdent, _gateName );
		}
	}

	private string _gateGroup = "";
	public string GateGroup
	{
		get
		{
			return _gateGroup;
		}
		set
		{
			if ( _gateGroup == value )
				return;
			_gateGroup = value;
			Stargate.RequestGroupChange( Gate.NetworkIdent, _gateGroup );
		}
	}

	private bool _isPrivate = false;
	public bool IsPrivate
	{
		get
		{
			return _isPrivate;
		}
		set
		{
			if ( _isPrivate == value )
				return;

			_isPrivate = value;
			Stargate.SetGatePrivate( Gate.NetworkIdent, _isPrivate );
		}
	}

	private bool _isLocal = false;
	public bool IsLocal
	{
		get
		{
			return _isLocal;
		}
		set
		{
			if ( _isLocal == value )
				return;

			_isLocal = value;

			Stargate.SetGateLocal( Gate.NetworkIdent, _isLocal );
		}
	}

	private bool _autoClose = false;
	public bool AutoClose
	{
		get
		{
			return _autoClose;
		}
		set
		{
			if ( _autoClose == value )
				return;

			_autoClose = value;

			Stargate.SetAutoClose( Gate.NetworkIdent, _autoClose );
		}
	}

	public bool FastDial { get; set; } = true;

	public string DialAddress { get; set; }

	private string _searchFilter = "";
	public string SearchFilter
	{
		get => _searchFilter;
		set
		{
			if ( _searchFilter == value )
				return;

			_searchFilter = value;

			FillGates();
		}
	}

	private Titlebar menuBar;

	public StargateMenuV2( Stargate gate, Dhd dhd = null )
	{

		StyleSheet.Load( "sbox_stargate/ui/stargatemenu/StargateMenuV2.scss" );

		menuBar = AddChild<Titlebar>();
		menuBar.SetTitle( true, "Stargate" );
		menuBar.SetCloseButton( true, "X", () => CloseMenu() );

		SetGate( gate );

		DHD = dhd;
	}

	public void CloseMenu()
	{
		Blur(); // finally, this makes it lose focus
		Delete( true );
	}

	public override void Tick()
	{
		base.Tick();

		// closes menu if player goes too far -- in the future we will want to freeze player's input
		if ( !Gate.IsValid() )
		{
			CloseMenu();
			return;
		}

		if ( !DHD.IsValid() )
		{
			var dist = Local.Pawn.Position.Distance( Gate.Position );
			if ( dist > 220 * Gate.Scale ) CloseMenu();
		}
		else
		{
			var dist = Local.Pawn.Position.Distance( DHD.Position );
			if ( dist > 80 * DHD.Scale ) CloseMenu();
		}

	}

	public void SetGate( Stargate gate )
	{
		this.Gate = gate;
		FillGates();
		RefreshGateInformation();
	}

	[Event( "stargate.refreshgateinformation" )]
	private void RefreshGateInformation()
	{
		GateAddress = Gate.GateAddress;
		GateName = Gate.GateName;
		GateGroup = Gate.GateGroup;
		AutoClose = Gate.AutoClose;
		IsPrivate = Gate.GatePrivate;
		IsLocal = Gate.GateLocal;

		FillGates();
	}

	private Table GetTable()
	{
		Table table = null;
		foreach ( Panel c in Children )
		{
			var tables = c.ChildrenOfType<Table>();
			if ( tables.Any() )
			{
				table = tables.First();
				break;
			}
		}

		return table;
	}

	public string GetGlyphsFontForGate( Stargate gate )
	{
		var glyphs = gate.GateGlyphType;
		var name = "concept";

		if ( glyphs == GlyphType.MILKYWAY ) name = "sg1";
		else if ( glyphs == GlyphType.PEGASUS ) name = "sga";
		else if ( glyphs == GlyphType.UNIVERSE ) name = "sgu";

		return $"stargate-font {name}";
	}

	public void FillGates()
	{
		Table table = GetTable();
		// table.Rows.DeleteChildren(true);
		table.Rows.Clear();
		table.Rows.Layout.Columns = 1;
		table.Rows.Layout.ItemSize = new Vector2( -1, 40 );
		table.Rows.OnCreateCell = ( cell, data ) =>
		{
			var gate = (Stargate)data;
			var panel = cell.Add.Panel( "row" );
			panel.AllowChildSelection = true;

			var address = GetOtherGateAddressForMenu( Gate, gate );
			var glyphAddress = address;
			if ( Gate.GateGlyphType == GlyphType.MILKYWAY && Gate.EarthPointOfOrigin ) glyphAddress = glyphAddress.Replace( '#', '?' );

			var td = panel.Add.Panel( $"td {GetGlyphsFontForGate( Gate )}" );
			td.AddChild<Label>().Text = glyphAddress;

			td = panel.Add.Panel( "td" );
			td.AddChild<Label>().Text = address;

			td = panel.Add.Panel( "td" );
			td.AddChild<Label>().Text = gate.GateName;

			panel.AddEventListener( "onclick", () =>
			{
				DialAddress = address;
			} );

			panel.AddEventListener( "ondoubleclick", () =>
			{
				DialAddress = address;
				OpenGate();
			} );
		};

		// list gates that arent private
		List<Stargate> gates = Entity.All.OfType<Stargate>().Where( x => x.GateAddress != Gate.GateAddress && !x.GatePrivate ).ToList();

		if ( SearchFilter != null && SearchFilter != "" )
		{
			gates = gates.Where( x => x.GateAddress.Contains( SearchFilter ) || (x.GateName != null && x.GateName != "" && x.GateName.Contains( SearchFilter )) ).ToList();
		}

		foreach ( Stargate gate in gates )
		{
			if ( Gate.GateLocal && Gate.GateGroup != gate.GateGroup ) continue;

			// only show other gate if both gates have same group, or a different group but both are not local
			if ( (Gate.GateGroup == gate.GateGroup) || (Gate.GateGroup != gate.GateGroup && !Gate.GateLocal && !gate.GateLocal) )
			{
				table.Rows.AddItem( gate );
			}
		}
	}

	// Needed for HTML Template
	//public void FillGatesHTML( string refresh = "false" )
	//{
	//	FillGates( refresh == "true" );
	//}

	public void OpenGate()
	{
		Stargate.RequestDial( FastDial ? DialType.FAST : DialType.SLOW, DialAddress, Gate.NetworkIdent );
	}

	public void CloseGate()
	{
		Stargate.RequestClose( Gate.NetworkIdent );
	}

}
