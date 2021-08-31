using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public enum DialType
{
	SLOW = 0,
	FAST = 1,
	INSTANT = 2,
	NOX = 3
}

public abstract partial class Stargate : Prop, IUse
{
	public Vector3 SpawnOffset = new ( 0, 0, 90 );

	protected const string Symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789#?@*";
	protected const string SymbolsNoOrigins = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@*";

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
		return true; // we should be always usable
	}

	public bool OnUse( Entity user )
	{
		OpenStargateMenu(To.Single( user ));
		return false; // aka SIMPLE_USE, not continuously
	}

	[ClientRpc]
	public void OpenStargateMenu()
	{
		var hud = Local.Hud;
		var count = 0;
		foreach (StargateMenuV2 menu in hud.ChildrenOfType<StargateMenuV2>()) count++;

		// this makes sure if we already have the menu open, we cant open it again
		if (count == 0) hud.AddChild<StargateMenuV2>().SetGate( this );
	}

	// SPAWN

	public override void Spawn()
	{
		base.Spawn();
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
  
	// IRIS
	public bool HasIris()
	{
		return Iris.IsValid();
	}

	public bool IsIrisClosed()
	{
		return HasIris() && Iris.Closed;
	}
  
	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsServer && OtherGate.IsValid() )
		{
			OtherGate.DoStargateClose();
		}
	}


	// DIALING -- please don't touch any of these, dialing is heavy WIP

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
	public virtual void BeginDialInstant( string address ) { } // instant gate open, with kawoosh
	public virtual void BeginDialNox( string address ) { } // instant gate open without kawoosh - asgard/ancient/nox style 
	public virtual async void BeginInboundFast( string address, int numChevs = 7 ) { }
	public virtual async void BeginInboundSlow( string address, int numChevs = 7 ) { } // this can be used with Instant dial, too

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


	// UI Related stuff

	[ClientRpc]
	public void RefreshGateInformations() {
		Event.Run("stargate.refreshgateinformation");
	}

	[ServerCmd]
	public static void RequestDial(DialType type, string address, int gate) {
		if (FindByIndex( gate ) is Stargate g && g.IsValid()) {
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
	public static void RequestClose(int gateID) {
		if (FindByIndex( gateID ) is Stargate g && g.IsValid()) {
			if ( g.Busy || ((g.Open || g.Active || g.Dialing) && g.Inbound) )
				return;
			if (g.Open)
				g.DoStargateClose( true );
			else if (g.Dialing)
				g.StopDialing();
		}
	}

	[ServerCmd]
	public static void ToggleIris(int gateID, int state) {
		if (FindByIndex( gateID ) is Stargate g && g.IsValid()) {
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
	public static void RequestAddressChange(int gateID, string address) {
		if (FindByIndex( gateID ) is Stargate g && g.IsValid()) {
			if (g.Address == address || !IsValidAddress( address ))
				return;

			g.Address = address;
		}
	}

	[ServerCmd]
	public static void RequestNameChange(int gateID, string name) {
		if (FindByIndex( gateID ) is Stargate g && g.IsValid()) {
			if (g.Name == name)
				return;

			g.Name = name;
		}
	}

	public Stargate FindClosestGate() {
		return Stargate.FindClosestGate(this.Position, 0, new Entity[] { this });
	}

	public static Stargate FindClosestGate(Vector3 postition, float max_distance = 0, Entity[] exclude = null) {
		Stargate current = null;
		float distance = float.PositiveInfinity;

		foreach ( Stargate gate in Entity.All.OfType<Stargate>() ) {
			if (exclude != null && exclude.Contains(gate))
				continue;

			float currDist = gate.Position.Distance(postition);
			if (distance > currDist && (max_distance > 0 && currDist <= max_distance)) {
				distance = currDist;
				current = gate;
			}
		}

		return current;
	}
}
