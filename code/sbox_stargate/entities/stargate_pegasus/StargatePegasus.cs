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

	public async override Task DoStargateReset()
	{
		if ( Dialing )
		{
			ShouldStopDialing = true;
			await Task.DelaySeconds( 0.2f ); // give the ring logic a chance to catch up (checks at 0.1 second intervals)
		}

		base.DoStargateReset();
		SetChevronsGlowState( false );
		Ring?.ResetSymbols();
	}


	// CHEVRON ANIMS & SOUNDS

	public void ChevronActivate( Chevron chev, float delay = 0, bool turnon = true, bool chevLock = false )
	{
		if ( chev.IsValid() )
		{
			Stargate.PlaySound( chev, GetSound( "chevron" + (chevLock ? "_lock" : "") + (Inbound ? "_inbound" : "") ), delay );
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

	// INDIVIDUAL DIAL TYPES

	// FAST DIAL
	public async override void BeginDialFast(string address)
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

			var addrLen = address.Length;

			// duration of the dial until the gate starts opening - let's stick to 7 seconds for total (just like GMod stargates)
			// default values are for 7 chevron sequence
			var chevronsStartDelay = (addrLen == 9) ? 0.60f : ((addrLen == 8) ? 0.70f : 0.70f);
			var chevronsLoopDuration = (addrLen == 9) ? 4.40f : ((addrLen == 8) ? 4.25f : 3.90f);
			var chevronBeforeLastDelay = (addrLen == 9) ? 0.75f : ((addrLen == 8) ? 0.80f : 1.05f);
			var chevronAfterLastDelay = (addrLen == 9) ? 1.25f : ((addrLen == 8) ? 1.25f : 1.35f);
			var chevronDelay = chevronsLoopDuration / (addrLen - 1);

			await Task.DelaySeconds( chevronsStartDelay ); // wait 0.5 sec and start locking chevrons

			// lets encode each chevron but the last
			for ( var i = 1; i < addrLen; i++ )
			{
				if ( ShouldStopDialing ) { StopDialing(); return; } // check if we should stop dialing

				var chev = GetChevronBasedOnAddressLength( i, addrLen );
				if ( chev.IsValid() )
				{

					ChevronActivate( chev, 0, true );

					ActiveChevrons++;
				}

				await Task.DelaySeconds( chevronDelay );
			}

			if ( ShouldStopDialing ) { StopDialing(); return; } // check if we should stop dialing

			await Task.DelaySeconds( chevronBeforeLastDelay ); // wait before locking the last chevron

			if ( ShouldStopDialing ) { StopDialing(); return; } // check if we should stop dialing

			Busy = true; // gate has to lock last chevron, lets go busy so we cant stop the dialing at this point

			var topChev = GetChevron( 7 ); // lock last (top) chevron
			if ( wasTargetReadyOnStart && target.IsValid() && target != this && target.IsStargateReadyForInboundFastEnd() )
			{
				ActiveChevrons++;
				ChevronActivate( topChev, 0, true, true );
			}
			else
			{
				ChevronActivate( topChev, 0, false, true );
			}

			await Task.DelaySeconds( chevronAfterLastDelay ); // wait after the last chevron, then open the gate or fail dial (if gate became invalid/was busy)

			if ( ShouldStopDialing ) { StopDialing(); return; } // check if we should stop dialing

			Busy = false;

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
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	// FAST INBOUND
	public async override void BeginInboundFast( int numChevs )
	{
		if ( !IsStargateReadyForInboundFast() ) return;

		try
		{
			if ( Dialing ) await DoStargateReset();

			CurGateState = GateState.ACTIVE;
			Inbound = true;

			Ring.RollSymbolsInbound(5f, 0.8f, numChevs );
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

			Stargate target = null;

			var readyForOpen = false;
			foreach ( var sym in address )
			{
				var chevNum = address.IndexOf( sym ) + 1;
				var isLastChev = (chevNum == address.Length);

				// try to encode each symbol
				var success = true; //await RotateRingToSymbol( sym ); // wait for ring to rotate to the target symbol
				if ( !success || ShouldStopDialing )
				{
					ResetGateVariablesToIdle();
					return;
				}

				await Task.DelaySeconds( 0.65f ); // wait a bit

				if ( isLastChev ) target = FindDestinationGateByDialingAddress( this, address ); // if its last chevron, try to find the target gate

				// go do chevron stuff
				var chev = GetChevronBasedOnAddressLength( chevNum, address.Length );
				var topChev = GetChevron( 7 );

				if ( !isLastChev )
				{
					ChevronActivate( chev );
				}
				else
				{
					ChevronActivate( topChev, 0, (isLastChev && target.IsValid() && target != this && target.IsStargateReadyForInboundInstantSlow()), true );
				}

				ActiveChevrons++;

				await Task.DelaySeconds( 0.5f );

				if ( ShouldStopDialing || !Dialing )
				{
					ResetGateVariablesToIdle();
					return;
				}

				if ( isLastChev && target.IsValid() && target != this && target.IsStargateReadyForInboundInstantSlow() )
				{
					target.BeginInboundSlow( address.Length );
					readyForOpen = true;
				}

				await Task.DelaySeconds( 1.5f ); // wait a bit

				chevNum++;
			}

			// prepare for open or fail

			Busy = false;

			if ( target.IsValid() && target != this && readyForOpen )
			{
				EstablishWormholeTo( target );
			}
			else
			{
				StopDialing();
			}
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
			if ( Dialing ) await DoStargateReset();

			CurGateState = GateState.ACTIVE;
			Inbound = true;

			for ( var i = 1; i <= numChevs; i++ )
			{
				var chev = GetChevronBasedOnAddressLength( i, numChevs );
				if ( chev.IsValid() )
				{
					ChevronActivate( chev, 0, true );
				}
			}

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
			if ( Dialing ) await DoStargateReset();

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

		ChevronActivate( chev, 0.15f, true );
	}

	public override void DoChevronLock( char sym ) // only the top chevron locks, always
	{
		base.DoChevronLock( sym );

		var chev = GetTopChevron();
		EncodedChevronsOrdered.Add( chev );

		var gate = FindDestinationGateByDialingAddress( this, DialingAddress );
		var valid = (gate != this && gate.IsValid() && gate.IsStargateReadyForInboundDHD());

		ChevronActivate( chev, 0.15f, true );
	}

	public override void DoChevronUnlock( char sym )
	{
		base.DoChevronUnlock( sym );

		var chev = EncodedChevronsOrdered.Last();
		EncodedChevronsOrdered.Remove( chev );

		ChevronDeactivate( chev );
	}

}
