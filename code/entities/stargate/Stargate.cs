using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class Stargate : Prop, IUse
{
	public Vector3 SpawnOffset = new ( 0, 0, 90 );

	protected const string Symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789#?@*";
	protected const string SymbolsNoOrigins = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@*";

	public EventHorizon EventHorizon;
	public StargateIris Iris;
	public Stargate OtherGate;

	public string Address { get; protected set; } = "";
	public string Group { get; protected set; } = "";

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
		return true;
	}

	public bool OnUse( Entity user )
	{
		GuiController.OpenStargateMenu( this, user );
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
		GuiController.CloseStargateMenu( this );
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

	[Event( "server.tick" )]
	public void StargateTick()
	{
		GuiController.RangeCheckTick();
	}
}
