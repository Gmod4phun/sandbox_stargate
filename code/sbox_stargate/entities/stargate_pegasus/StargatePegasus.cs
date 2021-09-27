using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

[Library( "ent_stargate_pegasus", Title = "Stargate (Pegasus)", Spawnable = true, Group = "Stargate.Stargate" )]
public partial class StargatePegasus : Stargate
{
	public StargateRingPegasus Ring;
	public List<Chevron> EncodedChevronsOrdered = new ();

	public StargatePegasus()
	{
		SoundDict = new()
		{
			{ "gate_open", "gate_atlantis_open" },
			{ "gate_close", "gate_close" },
			{ "gate_roll_fast", "gate_atlantis_roll" },
			{ "gate_roll_slow", "gate_atlantis_roll_slow" },
			{ "chevron", "chevron_atlantis_roll" },
			{ "chevron_inbound", "chevron_atlantis_roll_incoming" },
			{ "chevron_inbound_longer", "chevron_atlantis_roll_incoming_longer" },
			{ "chevron_inbound_shorter", "chevron_atlantis_roll_incoming_short" },
			{ "chevron_lock", "chevron_atlantis_lock" },
			{ "chevron_lock_inbound", "chevron_atlantis_lock_incoming" },
			{ "chevron_dhd", "chevron_atlantis" },
			{ "dial_fail", "dial_fail_atlantis" }
		};
	}

	// SPAWN

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/sbox_stargate/gate_atlantis/gate_atlantis.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;

		CreateRing();
		CreateAllChevrons();

		GateGroup = "P@";
		GateAddress = GenerateGateAddress( GateGroup );
	}

	public override void ResetGateVariablesToIdle()
	{
		base.ResetGateVariablesToIdle();

		EncodedChevronsOrdered.Clear();
	}

	// RING
	public void CreateRing()
	{
		Ring = new();
		Ring.Position = Position;
		Ring.Rotation = Rotation;
		Ring.SetParent( this );
		Ring.Gate = this;
		Ring.Transmit = TransmitType.Always;
	}

	// CHEVRONS

	public virtual Chevron CreateChevron( int n )
	{
		var chev = new Chevron();
		chev.Position = Position;
		chev.Rotation = Rotation.Angles().WithRoll( -ChevronAngles[n-1] ).ToRotation();
		chev.SetParent( this );
		chev.Transmit = TransmitType.Always;
		chev.Gate = this;

		chev.ChevronStateSkins = new()
		{
			{ "Off", 3 },
			{ "On", 4 },
		};

		//chev.Light.SetLightColor( Color.Parse( "#00A9FF" ).GetValueOrDefault() );

		chev.UsesDynamicLight = false;

		return chev;
	}

	public void CreateAllChevrons()
	{
		for (int i = 1; i <= 9; i++ )
		{
			var chev = CreateChevron( i );
			Chevrons.Add( chev );
		}
	}

	public Chevron GetChevron( int num )
	{
		return ( num <= Chevrons.Count ) ? Chevrons[num - 1] : null;
	}

	public Chevron GetChevronBasedOnAddressLength(int num, int len = 7)
	{
		if ( len == 8 )
		{
			if ( num == 7 ) return GetChevron( 8 );
			else if ( num == 8 ) return GetChevron( 7 );
		}
		else if ( len == 9 )
		{
			if ( num == 7 ) return GetChevron( 8 );
			else if ( num == 8 ) return GetChevron( 9 );
			else if ( num == 9 ) return GetChevron( 7 );
		}
		return GetChevron( num );
	}

	public Chevron GetTopChevron()
	{
		return GetChevron( 7 );
	}

	// DIALING

	public async void SetChevronsGlowState( bool state, float delay = 0)
	{
		if (delay > 0) await Task.DelaySeconds( delay );

		foreach ( Chevron chev in Chevrons ) chev.On = state;
	}

	public override void OnStopDialingBegin()
	{
		base.OnStopDialingBegin();

		PlaySound( this, GetSound( "dial_fail" ) );
		Ring?.StopRollSound();
	}

	public override void OnStopDialingFinish()
	{
		base.OnStopDialingFinish();

		SetChevronsGlowState( false );
		Ring?.ResetSymbols();
	}

	public override void OnStargateBeginOpen()
	{
		base.OnStargateBeginOpen();

		PlaySound( this, GetSound( "gate_open" ) );
	}

	public override void OnStargateOpened()
	{
		base.OnStargateOpened();
	}

	public override void OnStargateBeginClose()
	{
		base.OnStargateBeginClose();

		PlaySound( this, GetSound( "gate_close" ) );
	}

	public override void OnStargateClosed()
	{
		base.OnStargateClosed();

		SetChevronsGlowState( false );
		Ring?.ResetSymbols();
	}

	public override void DoStargateReset()
	{
		if ( Dialing ) ShouldStopDialing = true;

		base.DoStargateReset();

		SetChevronsGlowState( false );
		Ring?.ResetSymbols();
		Ring?.StopRollSound();
	}


	// CHEVRON ANIMS & SOUNDS

	public void ChevronActivate( Chevron chev, float delay = 0, bool turnon = true, bool chevLock = false, bool longer = false, bool shorter = false )
	{
		if ( chev.IsValid() )
		{
			Stargate.PlaySound( chev, GetSound( "chevron" + (chevLock ? "_lock" : "") + (Inbound ? "_inbound" : "") + ( longer ? "_longer" : "" ) + (shorter ? "_shorter" : "") ), delay );
			if (turnon) chev.TurnOn( delay );
		}
	}

	public void ChevronDeactivate( Chevron chev, float delay = 0 )
	{
		if ( chev.IsValid() )
		{
			chev.TurnOff( delay );
		}
	}

	public void ChevronActivateDHD( Chevron chev, float delay = 0, bool turnon = true )
	{
		if ( chev.IsValid() )
		{
			Stargate.PlaySound( chev, GetSound( "chevron_dhd" ), delay );
			if ( turnon ) chev.TurnOn( delay );
		}
	}

	public void ChevronLightup( Chevron chev, float delay = 0 )
	{
		if ( chev.IsValid() ) chev.TurnOn( delay );
	}

	// INDIVIDUAL DIAL TYPES

	// FAST DIAL
	public override void BeginDialFast(string address)
	{
		if ( !CanStargateStartDial() ) return;

		try
		{
			CurGateState = GateState.DIALING;
			CurDialType = DialType.FAST;

			if ( !IsValidFullAddress( address ) ) { StopDialing(); return; }

			var target = FindDestinationGateByDialingAddress( this, address );
			var wasTargetReadyOnStart = false; // if target gate was not available on dial start, dont bother doing anything at the end

			if ( target.IsValid() && target != this && target.IsStargateReadyForInboundFast() )
			{
				wasTargetReadyOnStart = true;
				target.BeginInboundFast( address.Length );
				OtherGate = target; // this is needed so that the gate can stop dialing if we cancel the dial
				OtherGate.OtherGate = this;
			}

			var startTime = Time.Now;
			var addrLen = address.Length;

			bool gateValidCheck() { return wasTargetReadyOnStart && target.IsValid() && target != this && target.IsStargateReadyForInboundFastEnd(); }

			Ring.RollSymbolsDialFast( addrLen, gateValidCheck );

			async void openOrStop() {

				if ( ShouldStopDialing ) { StopDialing(); return; } // check if we should stop dialing

				if ( wasTargetReadyOnStart && target.IsValid() && target != this && target.IsStargateReadyForInboundFastEnd() ) // if valid, open both gates
				{
					EstablishWormholeTo( target );
				}
				else
				{
					await Task.DelaySeconds( 0.25f ); // otherwise wait a bit, fail and stop dialing
					StopDialing();
				}
			}

			AddTask( startTime + 7, openOrStop, TimedTaskCategory.DIALING );

		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	// FAST INBOUND
	public override void BeginInboundFast( int numChevs )
	{
		if ( !IsStargateReadyForInboundFast() ) return;

		try
		{
			if ( Dialing ) DoStargateReset();

			CurGateState = GateState.ACTIVE;
			Inbound = true;

			PlaySound( this, GetSound("gate_roll_fast"), 0.35f );

			Ring.RollSymbolsInbound( 5.5f, 1f, numChevs );
		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	// SLOW DIAL
	public async override void BeginDialSlow( string address )
	{
		if ( !CanStargateStartDial() ) return;

		try
		{
			CurGateState = GateState.DIALING;
			CurDialType = DialType.SLOW;

			if ( !IsValidFullAddress( address ) )
			{
				StopDialing();
				return;
			}
			
			var startTime = Time.Now;
			var addrLen = address.Length;

			Stargate target = FindDestinationGateByDialingAddress( this, address );

			bool gateValidCheck() { return target.IsValid() && target != this && target.IsStargateReadyForInboundFastEnd(); }

			Ring.RollSymbolsDialSlow(address.Length, gateValidCheck);

			var dialTime = (addrLen == 9) ? 40f : ((addrLen == 8) ? 32f : 26f);
			
			void startInboundAnim()
			{
				Stargate target = FindDestinationGateByDialingAddress( this, address );
				if ( target.IsValid() && target != this && target.IsStargateReadyForInboundFast() )
				{
					target.BeginInboundFast( address.Length );
					OtherGate = target; // this is needed so that the gate can stop dialing if we cancel the dial
					OtherGate.OtherGate = this;
				}
			}

			AddTask( startTime + dialTime - 7f, startInboundAnim, TimedTaskCategory.DIALING );

			void openOrStop()
			{
				Ring.StopRollSound();

				if ( ShouldStopDialing || !Dialing )
				{
					ResetGateVariablesToIdle();
					return;
				}

				//var readyForOpen = false;
				//if ( target.IsValid() && target != this && target.IsStargateReadyForInboundFastEnd() ) readyForOpen = true;

				Busy = false;

				if ( gateValidCheck() )
				{
					EstablishWormholeTo( target );
				}
				else
				{
					StopDialing();
				}
			}

			AddTask( startTime + dialTime, openOrStop, TimedTaskCategory.DIALING );
		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	// SLOW INBOUND
	public async override void BeginInboundSlow( int numChevs )
	{
		if ( !IsStargateReadyForInboundInstantSlow() ) return;

		try
		{
			if ( Dialing ) DoStargateReset();

			CurGateState = GateState.ACTIVE;
			Inbound = true;

			for ( var i = 1; i <= numChevs; i++ )
			{
				var chev = GetChevronBasedOnAddressLength( i, numChevs );
				ChevronLightup( chev );
			}

			PlaySound( this, GetSound( "chevron_lock_inbound" ) );
			Ring.LightupSymbols();

			ActiveChevrons = numChevs;
		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	public async override void BeginDialInstant( string address )
	{
		if ( !CanStargateStartDial() ) return;

		try
		{
			CurGateState = GateState.DIALING;
			CurDialType = DialType.INSTANT;

			if ( !IsValidFullAddress( address ) )
			{
				StopDialing();
				return;
			}

			var otherGate = FindDestinationGateByDialingAddress( this, address );
			if ( !otherGate.IsValid() || otherGate == this || !otherGate.IsStargateReadyForInboundInstantSlow() )
			{
				StopDialing();
				return;
			}

			otherGate.BeginInboundSlow( address.Length );

			for ( var i = 1; i <= address.Length; i++ )
			{
				var chev = GetChevronBasedOnAddressLength( i, address.Length );
				ChevronActivate( chev );
			}

			await Task.DelaySeconds( 0.5f );

			EstablishWormholeTo( otherGate );
		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	// DHD DIAL

	public async override void BeginOpenByDHD( string address )
	{
		if ( !CanStargateStartDial() ) return;

		try
		{
			CurGateState = GateState.DIALING;
			CurDialType = DialType.DHD;

			await Task.DelaySeconds( 0.35f );

			var otherGate = FindDestinationGateByDialingAddress( this, address );
			if ( otherGate.IsValid() && otherGate != this && otherGate.IsStargateReadyForInboundDHD() )
			{
				otherGate.BeginInboundDHD( address.Length );
			}
			else
			{
				StopDialing();
				return;
			}

			await Task.DelaySeconds( 0.15f );

			EstablishWormholeTo( otherGate );
		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	public async override void BeginInboundDHD( int numChevs )
	{
		if ( !IsStargateReadyForInboundDHD() ) return;

		try
		{
			if ( Dialing ) DoStargateReset();

			CurGateState = GateState.ACTIVE;
			Inbound = true;

			for ( var i = 1; i <= numChevs; i++ )
			{
				var chev = GetChevronBasedOnAddressLength( i, numChevs );
				ChevronActivate( chev, 0, true );
			}
		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	// CHEVRON STUFF - DHD DIALING
	public override void DoChevronEncode(char sym)
	{
		base.DoChevronEncode( sym );

		var chev = GetChevronBasedOnAddressLength(DialingAddress.Length, 9 );
		EncodedChevronsOrdered.Add( chev );

		ChevronActivateDHD( chev, 0.15f, true );
	}

	public override void DoChevronLock( char sym ) // only the top chevron locks, always
	{
		base.DoChevronLock( sym );

		var chev = GetTopChevron();
		EncodedChevronsOrdered.Add( chev );

		var gate = FindDestinationGateByDialingAddress( this, DialingAddress );
		var valid = (gate != this && gate.IsValid() && gate.IsStargateReadyForInboundDHD());

		ChevronActivateDHD( chev, 0.15f, true );
	}

	public override void DoChevronUnlock( char sym )
	{
		base.DoChevronUnlock( sym );

		var chev = EncodedChevronsOrdered.Last();
		EncodedChevronsOrdered.Remove( chev );

		ChevronDeactivate( chev );
	}

}
