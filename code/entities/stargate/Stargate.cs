using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public enum DialType {
	SLOW = 0,
	FAST,
	INSTANT
}

public partial class Stargate : Prop, IUse
{
	public Vector3 SpawnOffset = new ( 0, 0, 90 );

	protected string Symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789#?@*";
	protected string SymbolsNoOrigins = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@*";

	public EventHorizon EventHorizon;
	public StargateIris Iris;
	public Stargate OtherGate;

	[Net]
	public string Address { get; protected set; } = "";
	[Net]
	public string Group { get; protected set; } = "";
	[Net]
	public string Name { get; protected set; } = "";

	public bool Active { get; protected set; } = false;
	public bool Inbound = false;
	public bool Open { get; protected set; } = false;
	public bool Dialing = false;
	public bool ShouldStopDialing = false;
	public bool Busy = false;

	// VARIABLE RESET
	public void ResetGateVariablesToIdle()
	{
		Active = false;
		Open = false;
		Inbound = false;
		Dialing = false;
		ShouldStopDialing = false;
		Busy = false;
		OtherGate = null;
	}

	// USABILITY

	public bool IsUsable( Entity user )
	{
		return user.Position.Distance(Position) < StargateMenu.AutoCloseMenuDistance;
	}

	public bool OnUse( Entity user )
	{
		OpenStargateMenu(To.Single( user ));
		return false; // aka SIMPLE_USE, not continuously
	}

	[ClientRpc]
	public void OpenStargateMenu() {
		Local.Hud.AddChild<StargateMenuV2>().SetGate(this);
	}

	// SPAWN

	public override void Spawn()
	{
		base.Spawn();
	}

	// ADDRESS
	public string GenerateRandomAddress(int length = 7)
	{
		if ( length < 7 || length > 9 ) return "";

		StringBuilder symbolsCopy = new( SymbolsNoOrigins );

		string generatedAddress = "";
		for ( int i = 1; i < length; i++ ) // pick random symbols without repeating
		{
			var randomIndex = new Random().Int( 0, symbolsCopy.Length - 1 );
			generatedAddress += symbolsCopy[randomIndex];

			symbolsCopy = symbolsCopy.Remove( randomIndex, 1 );
		}
		generatedAddress += '#'; // add a point of origin
		return generatedAddress;
	}

	public bool IsValidAddress(string address) // a valid address has 7, 8, or 9 VALID characters and has no repeating symbols
	{
		if ( address.Length < 7 || address.Length > 9 ) return false; // only 7, 8 or 9 symbol addresses 
		foreach (char sym in address)
		{
			if ( !Symbols.Contains(sym) ) return false; // only valid symbols
			if ( address.Count( c => c == sym ) > 1 ) return false; // only one occurence
		}
		return true;
	}

	public Stargate FindByAddress(string address)
	{
		foreach ( Stargate gate in Entity.All.OfType<Stargate>() )
		{
			if ( gate.Address.Equals(address) ) return gate;
		}
		return null;
	}

	// EVENT HORIZON

	public void CreateEventHorizon()
	{
		EventHorizon = new EventHorizon();
		EventHorizon.Position = Position;
		EventHorizon.Rotation = Rotation;
		EventHorizon.Scale = Scale;
		EventHorizon.SetParent( this );
		EventHorizon.Gate = this;
	}

	public void DeleteEventHorizon()
	{
		EventHorizon?.Delete();
	}

	public async Task EstablishEventHorizon(float delay = 0)
	{
		await GameTask.DelaySeconds( delay );
		if ( !this.IsValid() ) return;

		CreateEventHorizon();
		EventHorizon.Establish();

		await GameTask.DelaySeconds( 2f );
		if ( !this.IsValid() || !EventHorizon.IsValid() ) return;

		EventHorizon.IsFullyFormed = true;
	}

	public async Task CollapseEventHorizon( float sec = 0 )
	{
		await GameTask.DelaySeconds( sec );
		if ( !this.IsValid() || !EventHorizon.IsValid() ) return;

		EventHorizon.IsFullyFormed = false;
		EventHorizon.CollapseClientAnim();

		await GameTask.DelaySeconds( sec + 2f );
		if ( !this.IsValid() || !EventHorizon.IsValid() ) return;

		DeleteEventHorizon();
	}

	// STARGATE
	public Stargate FindRandomGate()
	{
		foreach ( Stargate gate in Entity.All.OfType<Stargate>() )
		{
			if ( gate != this ) return gate;
		}
		return null;
	}

	protected override void OnDestroy()
	{
		// GuiController.CloseStargateMenu( this );
		base.OnDestroy();

		if ( IsServer && OtherGate.IsValid() )
		{
			OtherGate.DoStargateClose();
		}
	}


	// DIALING

	public async void DoStargateOpen()
	{
		Busy = true;
		Open = true;

		OnStargateBeginOpen();

		await EstablishEventHorizon( 0.5f );

		Busy = false;
		OnStargateOpened();
	}

	public async void DoStargateClose( bool alsoCloseOther = false )
	{
		if ( alsoCloseOther && OtherGate.IsValid() ) OtherGate.DoStargateClose();

		Busy = true;

		OnStargateBeginClose();

		await CollapseEventHorizon( 0.25f );

		ResetGateVariablesToIdle();
		OnStargateClosed();
	}

	public virtual async void BeginDialFast(string address) { }
	public virtual async void BeginDialSlow(string address) { }
	public virtual void BeginDialInstant(string address) { } // instant gate open, with kawoosh
	public virtual void BeginDialNox( string address ) { } // instant gate open without kawoosh - asgard/ancient/nox style 
	public virtual async void BeginInboundFast( string address, int numChevs = 7 ) { }
	public virtual async void BeginInboundSlow( string address, int numChevs = 7 ) { }

	public async void StopDialing()
	{
		OnStopDialingBegin();

		await GameTask.DelaySeconds( 1.25f );

		OnStopDialingFinish();
	}

	public virtual void OnStopDialingBegin()
	{
		//Log.Info( "stopdial begin" );
		Busy = true;
		ShouldStopDialing = true; // can be used in ring/gate logic to to stop ring/gate rotation

		if ( OtherGate.IsValid() && OtherGate.Inbound && !OtherGate.ShouldStopDialing )
		{
			OtherGate.StopDialing();
		}
	}

	public virtual void OnStopDialingFinish()
	{
		Dialing = false; // must be set to false AFTER setting ShouldStopDialing to true
		Busy = false;
		ResetGateVariablesToIdle();
		//Log.Info( "stopdial done" );
	}

	public virtual void OnStargateBeginOpen()
	{
		Busy = true;
	}
	public virtual void OnStargateOpened()
	{
		Busy = false;
	}
	public virtual void OnStargateBeginClose()
	{
		Busy = true;
	}
	public virtual void OnStargateClosed()
	{
		Busy = false;
		ResetGateVariablesToIdle();
	}

	[ClientRpc]
	public void RefreshGateInformations() {
		Event.Run("stargate.refreshgateinformations");
	}

	[ServerCmd]
	public static void RequestDial(DialType type, string address, int gate) {
		if (Entity.FindByIndex( gate ) is Stargate g && g.IsValid()) {
			switch ( type ) {
				case DialType.FAST:
					g.BeginDialFast( address );
					break;

				case DialType.SLOW:
					g.BeginDialSlow( address );
					break;

				case DialType.INSTANT:
					g.BeginDialInstant( address );
					break;
			}
		}
	}

	[ServerCmd]
	public static void RequestClose(int gate) {
		if (Entity.FindByIndex( gate ) is Stargate g && g.IsValid()) {
			if ( g.Busy || ((g.Open || g.Active || g.Dialing) && g.Inbound) )
				return;
			if (g.Open)
				g.DoStargateClose( true );
			else if (g.Dialing)
				g.StopDialing();
		}
	}

	[ServerCmd]
	public static void ToggleIris(int gate, int state) {
		if (Entity.FindByIndex( gate ) is Stargate g && g.IsValid()) {
			if (g.Iris.IsValid()) {
				if (state == -1)
					g.Iris.Toggle();

				if (state == 0)
					g.Iris.Close();

				if (state == 1)
					g.Iris.Open();
			}
		}
	}

	[ServerCmd]
	public static void RequestAddressChange(int gate, string address) {
		if (Entity.FindByIndex( gate ) is Stargate g && g.IsValid()) {
			if (g.Address == address || !g.IsValidAddress( address ))
				return;

			g.Address = address;
		}
	}

	[ServerCmd]
	public static void RequestNameChange(int gate, string name) {
		if (Entity.FindByIndex( gate ) is Stargate g && g.IsValid()) {
			if (g.Name == name)
				return;

			g.Name = name;
		}
	}
}
