using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public abstract partial class Stargate : Prop, IUse
{
	[Net]
	public Vector3 SpawnOffset { get; private set; }= new( 0, 0, 90 );

	public List<Chevron> Chevrons = new();

	public EventHorizon EventHorizon;
	public StargateIris Iris;
	public Stargate OtherGate;

	public float AutoCloseTime = -1;

	public Dictionary<string, string> SoundDict = new()
	{
		{ "gate_open", "baseValue" },
		{ "gate_close", "baseValue" },
		{ "chevron_open", "baseValue" },
		{ "chevron_close", "baseValue" },
		{ "dial_fail", "baseValue" },
		{ "dial_fail_noclose", "baseValue" },
	};

	[Net]
	public string GateAddress { get; set; } = "";
	[Net]
	public string GateGroup { get { return GateGroup; } set { if ( value.Length != GateGroupLength ) return; GateGroup = value; } }
	[Net]
	public int GateGroupLength  { get; set; } = 2;
	[Net]
	public string GateName { get; set; } = "";
	[Net]
	public bool AutoClose { get; set; } = true;
	[Net]
	public bool GatePrivate { get; set; } = false;
	[Net]
	public bool GateLocal { get; set; } = true;

	public bool Busy { get; set; } = false; // this is pretty much used anytime the gate is busy to do anything (usually during animations/transitions)
	public bool Inbound { get; set; } = false;

	[Net]
	public bool ShouldStopDialing { get; set; } = false;
	public GateState CurGateState { get; set; } = GateState.IDLE;
	public DialType CurDialType { get; set; } = DialType.FAST;

	// gate state accessors
	public bool Idle { get => CurGateState is GateState.IDLE; }
	public bool Active { get => CurGateState is GateState.ACTIVE; }
	public bool Dialing { get => CurGateState is GateState.DIALING; }
	public bool Opening { get => CurGateState is GateState.OPENING; }
	public bool Open { get => CurGateState is GateState.OPEN; }
	public bool Closing { get => CurGateState is GateState.CLOSING; }

	public string DialingAddress { get; set; } = "";
	public int ActiveChevrons = 0;

	public TimeSince TimeSinceDHDAction = 0f;
	public float DhdDialShutdownTime = 20f;

	public IStargateRamp Ramp = null;

	// SOUNDS
	public virtual string GetSound( string key )
	{
		return SoundDict.GetValueOrDefault( key, "" );
	}

	// VARIABLE RESET
	public virtual void ResetGateVariablesToIdle()
	{
		ShouldStopDialing = false;
		OtherGate = null;
		Inbound = false;
		Busy = false;
		CurGateState = GateState.IDLE;
		CurDialType = DialType.FAST;
		DialingAddress = "";
		ActiveChevrons = 0;
	}

	// USABILITY
	public bool IsUsable( Entity user )
	{
		return true; // we should be always usable
	}

	public virtual bool OnUse( Entity user )
	{
		OpenStargateMenu(To.Single( user ));
		return false; // aka SIMPLE_USE, not continuously
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
		await Task.DelaySeconds( delay );
		if ( !this.IsValid() ) return;

		CreateEventHorizon();
		EventHorizon.Establish();

		await Task.DelaySeconds( 2f );
		if ( !this.IsValid() || !EventHorizon.IsValid() ) return;

		EventHorizon.IsFullyFormed = true;
	}

	public async Task CollapseEventHorizon( float sec = 0 )
	{
		await Task.DelaySeconds( sec );
		if ( !this.IsValid() || !EventHorizon.IsValid() ) return;

		EventHorizon.IsFullyFormed = false;
		EventHorizon.CollapseClientAnim();

		await Task.DelaySeconds( sec + 2f );
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
		if ( Ramp != null ) Ramp.Gate.Remove( this );

		if ( IsServer && OtherGate.IsValid() )
		{
			OtherGate.DoStargateClose();
		}

		base.OnDestroy();
	}

	// DIALING -- please don't touch any of these, dialing is heavy WIP

	public bool CanStargateOpen()
	{
		return ( !Busy && !Opening && !Open && !Closing );
	}

	public bool CanStargateClose()
	{
		return ( !Busy && Open);
	}

	public bool CanStargateStartDial()
	{
		return ( Idle && !Busy && !Dialing && !Inbound );
	}

	public bool CanStargateStopDial()
	{
		if (!Inbound) return (!Busy && Dialing);

		return ( !Busy && Active );
	}

	public async void DoStargateOpen()
	{
		if ( !CanStargateOpen() ) return;

		OnStargateBeginOpen();

		await EstablishEventHorizon( 0.5f ); // TODO - fix EH crash when gate gets removed during opening

		OnStargateOpened();
	}

	public async void DoStargateClose( bool alsoCloseOther = false )
	{
		if ( !CanStargateClose() ) return;

		if ( alsoCloseOther && OtherGate.IsValid() && OtherGate.Open ) OtherGate.DoStargateClose();

		OnStargateBeginClose();

		await CollapseEventHorizon( 0.25f );

		OnStargateClosed();
	}

	public bool IsStargateReadyForInboundFast() // checks if the gate is ready to do a inbound anim for fast dial
	{
		if ( !Dialing )
		{
			return (!Busy && !Open && !Inbound);
		}
		else
		{
			return ( !Busy && !Open && !Inbound && (CurDialType is DialType.SLOW || CurDialType is DialType.DHD) );
		}
	}

	public bool IsStargateReadyForInboundFastEnd() // checks if the gate is ready to open when finishing fast dial?
	{
		return ( !Busy && !Open && !Dialing && Inbound );
	}

	public bool IsStargateReadyForInboundInstantSlow() // checks if the gate is ready to do inbound for instant or slow dial
	{
		return ( !Busy && !Open && !Inbound );
	}

	public bool IsStargateReadyForInboundDHD() // checks if the gate is ready to be locked onto by dhd dial
	{
		if ( !Dialing )
		{
			return (!Busy && !Open && !Inbound);
		}
		else
		{
			return (!Busy && !Open && !Inbound && CurDialType == DialType.SLOW);
		}
	}

	public bool IsStargateReadyForInboundDHDEnd() // checks if the gate is ready to be opened while locked onto by a gate using dhd dial
	{
		if ( !Dialing )
		{
			return (!Busy && !Open && Inbound);
		}
		else
		{
			return (!Busy && !Open && Inbound && CurDialType == DialType.SLOW);
		}
	}

	public virtual void BeginDialFast(string address) { }
	public virtual void BeginDialSlow(string address) { }
	public virtual void BeginDialInstant( string address ) { } // instant gate open, with kawoosh
	public virtual void BeginDialNox( string address ) { } // instant gate open without kawoosh - asgard/ancient/nox style 

	public virtual void BeginInboundFast( int numChevs ) { }
	public virtual void BeginInboundSlow( int numChevs ) { } // this can be used with Instant dial, too


	// DHD DIAL
	public virtual void BeginOpenByDHD( string address ) { } // when dhd dial button is pressed
	public virtual void BeginInboundDHD( int numChevs ) { } // when a dhd dialing gate locks onto another gate

	public async void StopDialing()
	{
		if ( !CanStargateStopDial() ) return;

		OnStopDialingBegin();

		await Task.DelaySeconds( 1.25f );

		OnStopDialingFinish();
	}

	public virtual void OnStopDialingBegin()
	{
		Busy = true;
		ShouldStopDialing = true; // can be used in ring/gate logic to to stop ring/gate rotation

		if ( OtherGate.IsValid() && OtherGate.Inbound && !OtherGate.ShouldStopDialing )
		{
			OtherGate.StopDialing();
		}
	}

	public virtual void OnStopDialingFinish()
	{
		ResetGateVariablesToIdle();
	}

	public virtual void OnStargateBeginOpen()
	{
		CurGateState = GateState.OPENING;
		Busy = true;
	}
	public virtual void OnStargateOpened()
	{
		CurGateState = GateState.OPEN;
		Busy = false;
	}
	public virtual void OnStargateBeginClose()
	{
		CurGateState = GateState.CLOSING;
		Busy = true;
	}
	public virtual void OnStargateClosed()
	{
		ResetGateVariablesToIdle();
	}

	public async virtual Task DoStargateReset()
	{
		ResetGateVariablesToIdle();
	}

	public virtual void EstablishWormholeTo(Stargate target)
	{
		target.OtherGate = this;
		OtherGate = target;

		target.Inbound = true;

		target.DoStargateOpen();
		DoStargateOpen();
	}

	// CHEVRON

	public virtual void DoChevronEncode(char sym)
	{
		DialingAddress += sym;
		Log.Info( $"Encoded {sym}, DialingAddress = '{DialingAddress}'" );
	}

	public virtual void DoChevronLock(char sym)
	{
		DialingAddress += sym;
		Log.Info( $"Locked {sym}, DialingAddress = '{DialingAddress}'" );
	}

	public virtual void DoChevronUnlock(char sym)
	{
		var sb = new StringBuilder(DialingAddress);
		sb.Remove( DialingAddress.IndexOf( sym ), 1 );

		DialingAddress = sb.ToString();

		Log.Info( $"Unlocked {sym}, DialingAddress = '{DialingAddress}'" );
	}

	// THINK
	public void AutoCloseThink()
	{
		if ( AutoClose && AutoCloseTime != -1 && AutoCloseTime <= Time.Now && CanStargateClose() )
		{
			AutoCloseTime = -1;
			DoStargateClose( true );
		}
	}

	public void CloseIfNoOtherGate()
	{
		if ( Open && !OtherGate.IsValid() )
		{
			DoStargateClose();
		}
	}

	public void DhdDialTimerThink()
	{
		if ( Dialing && CurDialType is DialType.DHD && TimeSinceDHDAction > DhdDialShutdownTime )
		{
			StopDialing();
		}
	}

	[Event( "server.tick" )]
	public void StargateTick()
	{
		AutoCloseThink();
		CloseIfNoOtherGate();
		DhdDialTimerThink();
	}


	// UI Related stuff

	[ClientRpc]
	public void OpenStargateMenu(Dhd dhd = null)
	{
		var hud = Local.Hud;
		var count = 0;
		foreach ( StargateMenuV2 menu in hud.ChildrenOfType<StargateMenuV2>() ) count++;

		// this makes sure if we already have the menu open, we cant open it again
		if ( count == 0 ) hud.AddChild( new StargateMenuV2( this, dhd ) );
	}

	[ClientRpc]
	public void RefreshGateInformation() {
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
			if (g.GateAddress == address || !IsValidAddressOnly( address ))
				return;

			g.GateAddress = address;

			g.RefreshGateInformation();
		}
	}

	[ServerCmd]
	public static void RequestGroupChange( int gateID, string group )
	{
		if ( FindByIndex( gateID ) is Stargate g && g.IsValid() )
		{
			if ( g.GateGroup == group || !IsValidGroup( group ) )
				return;

			g.GateGroup = group;

			g.RefreshGateInformation();
		}
	}

	[ServerCmd]
	public static void RequestNameChange(int gateID, string name) {
		if (FindByIndex( gateID ) is Stargate g && g.IsValid()) {
			if (g.GateName == name)
				return;

			g.GateName = name;

			g.RefreshGateInformation();
		}
	}

	[ServerCmd]
	public static void SetAutoClose(int gateID, bool state) {
		if (FindByIndex( gateID ) is Stargate g && g.IsValid()) {
			if (g.AutoClose == state)
				return;

			g.AutoClose = state;

			g.RefreshGateInformation();
		}
	}

	[ServerCmd]
	public static void SetGatePrivate(int gateID, bool state) {
		if (FindByIndex( gateID ) is Stargate g && g.IsValid()) {
			if (g.GatePrivate == state)
				return;

			g.GatePrivate = state;

			g.RefreshGateInformation();
		}
	}

	[ServerCmd]
	public static void SetGateLocal( int gateID, bool state )
	{
		if ( FindByIndex( gateID ) is Stargate g && g.IsValid() )
		{
			if ( g.GateLocal == state )
				return;

			g.GateLocal = state;

			g.RefreshGateInformation();
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
