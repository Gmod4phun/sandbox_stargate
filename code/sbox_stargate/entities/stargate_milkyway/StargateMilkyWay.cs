using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

[Library( "ent_stargate_milkyway", Title = "Stargate (Milky Way)", Spawnable = true, Group = "Stargate" )]
public partial class StargateMilkyWay : Stargate
{

	public StargateRing Ring;
	public List<Chevron> EncodedChevronsOrdered = new ();

	// SPAWN

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/gmod4phun/stargate/gate_sg1/gate_sg1.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;

		CreateRing();
		CreateAllChevrons();

		Group = "M@";
		Address = GenerateRandomAddress(7);
	}

	public override void ResetGateVariablesToIdle()
	{
		base.ResetGateVariablesToIdle();

		EncodedChevronsOrdered.Clear();
	}

	// RING

	public void CreateRing()
	{
		Ring = new ();
		Ring.Position = Position;
		Ring.Rotation = Rotation;
		Ring.SetParent( this );
		Ring.Gate = this;
		Ring.Transmit = TransmitType.Always;
	}

	public async Task<bool> RotateRingToSymbol( char sym, int angOffset = 0 )
	{
		return await Ring.RotateRingToSymbolAsync( sym, angOffset );
	}

	// CHEVRONS

	public Chevron CreateChevron( int n )
	{
		var chev = new Chevron();
		chev.Position = Position;
		chev.Rotation = Rotation.Angles().WithRoll( -ChevronAngles[n-1] ).ToRotation();
		chev.SetParent( this );
		chev.Transmit = TransmitType.Always;
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

		PlaySound( this, (ActiveChevrons > 0 || CurDialType is DialType.DHD) ? "dial_fail_sg1" : "gate_sg1_dial_fail_noclose" );

		if ( Ring.IsValid() && Ring.IsMoving ) Ring.SpinDown();
	}

	public override void OnStopDialingFinish()
	{
		base.OnStopDialingFinish();

		SetChevronsGlowState( false );
	}

	public override void OnStargateBeginOpen()
	{
		base.OnStargateBeginOpen();

		PlaySound(this, "gate_open_sg1" );
	}

	public override void OnStargateOpened()
	{
		base.OnStargateOpened();
	}

	public override void OnStargateBeginClose()
	{
		base.OnStargateBeginClose();

		PlaySound( this, "gate_close" );
	}

	public override void OnStargateClosed()
	{
		base.OnStargateClosed();

		SetChevronsGlowState( false );
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
	}

	// CHEVRON ANIMS & SOUNDS

	public void TopChevronEncode(Chevron chev, bool lightup = true, bool keeplit = false)
	{
		if ( chev.IsValid() )
		{
			PlaySound( chev, "chevron_sg1_open" );
			PlaySound( chev, "chevron_sg1_close", 0.75f );
			chev.ChevronAnim( "lock" );
			chev.ChevronAnim( "unlock", 0.8f );

			if ( lightup )
			{
				chev.TurnOn( 0.5f );
				if (!keeplit) chev.TurnOff( 1.5f );
			}
		}
	}

	public void ChevronEncode( Chevron chev, bool lightup = true )
	{
		if ( chev.IsValid() )
		{
			if ( lightup )
			{
				chev.TurnOn( 0.5f );
			}
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

			if ( !IsValidAddress( address ) ) { StopDialing(); return; }

			var target = FindByAddress( address );
			var wasTargetReadyOnStart = false; // if target gate was not available on dial start, dont bother doing anything at the end

			if ( target.IsValid() && target != this && target.IsStargateReadyForInboundFast() )
			{
				wasTargetReadyOnStart = true;
				target.BeginInboundFast( Address, target.Address.Length );
				OtherGate = target; // this is needed so that the gate can stop dialing if we cancel the dial
				OtherGate.OtherGate = this;
			}

			Ring.SpinUp(); // start rotating ring

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
					chev.TurnOn();
					PlaySound( chev, "chevron_sg1_open" );
					ActiveChevrons++;
				}

				if ( i == addrLen - 1 ) Ring.SpinDown(); // stop rotating ring when the last looped chevron locks

				await Task.DelaySeconds( chevronDelay );
			}

			if ( ShouldStopDialing ) { StopDialing(); return; } // check if we should stop dialing

			await Task.DelaySeconds( chevronBeforeLastDelay ); // wait before locking the last chevron

			if ( ShouldStopDialing ) { StopDialing(); return; } // check if we should stop dialing

			Busy = true; // gate has to lock last chevron, lets go busy so we cant stop the dialing at this point

			var topChev = GetChevron( 7 ); // lock last (top) chevron
			if ( topChev.IsValid() )
			{
				if ( wasTargetReadyOnStart && target.IsValid() && target != this && target.IsStargateReadyForInboundFastEnd() ) topChev.TurnOn();

				topChev.ChevronAnim( "lock" );
				PlaySound( topChev, "chevron_sg1_open", 0.2f );
				topChev.ChevronAnim( "unlock", 1f );
				PlaySound( topChev, "chevron_sg1_close", 1.05f );

				ActiveChevrons++;
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
	public async override void BeginInboundFast( string address, int numChevs = 7 )
	{
		if ( !IsStargateReadyForInboundFast() ) return;

		try
		{
			if ( Dialing ) await DoStargateReset();

			CurGateState = GateState.ACTIVE;
			Inbound = true;

			// duration of the dial until the gate starts opening - let's stick to 7 seconds for total (just like GMod stargates)
			// default values are for 7 chevron sequence
			var chevronsStartDelay = (numChevs == 9) ? 0.25f : ((numChevs == 8) ? 0.40f : 0.50f);
			var chevronsLoopDuration = (numChevs == 9) ? 6.75f : ((numChevs == 8) ? 6.60f : 6.75f);
			var chevronBeforeLastDelay = (numChevs == 9) ? 0.50f : ((numChevs == 8) ? 0.60f : 0.50f);
			var chevronDelay = chevronsLoopDuration / (numChevs);

			await Task.DelaySeconds( chevronsStartDelay ); // wait 0.5 sec and start locking chevrons

			for ( var i = 1; i < numChevs; i++ )
			{
				if ( !OtherGate.IsValid() )
				{
					StopDialing();
					return;
				}

				if ( ShouldStopDialing ) return; // check if we should stop dialing or not

				var chev = GetChevronBasedOnAddressLength( i, numChevs );
				if ( chev.IsValid() )
				{
					chev.TurnOn();
					PlaySound( chev, "chevron_sg1_open" );
					ActiveChevrons++;
				}

				await Task.DelaySeconds( chevronDelay ); // each chevron delay
			}

			await Task.DelaySeconds( chevronBeforeLastDelay ); // wait before locking the last chevron

			var topChev = GetChevron( 7 );
			if ( topChev.IsValid() )
			{
				topChev.TurnOn();
				PlaySound( topChev, "chevron_sg1_open" );
				ActiveChevrons++;
			}
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

			if ( !IsValidAddress( address ) )
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
				var success = await RotateRingToSymbol( sym ); // wait for ring to rotate to the target symbol
				if ( !success || ShouldStopDialing )
				{
					ResetGateVariablesToIdle();
					return;
				}

				await Task.DelaySeconds( 0.65f ); // wait a bit

				if ( isLastChev ) target = FindByAddress( address ); // if its last chevron, try to find the target gate

				// go do chevron stuff

				var chev = GetChevronBasedOnAddressLength( chevNum, address.Length );
				var topChev = GetChevron( 7 );

				if ( !isLastChev )
				{
					TopChevronEncode( topChev );
					ChevronEncode( chev );
				}
				else
				{
					TopChevronEncode( topChev, (isLastChev && target.IsValid() && target != this && target.IsStargateReadyForInboundInstantSlow()), true );
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
					target.BeginInboundSlow( address, address.Length );
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
	public async override void BeginInboundSlow( string address, int numChevs = 7 )
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
					chev.TurnOn();
					if ( i == 1 ) PlaySound( chev, "chevron_sg1_open" ); // play once to avoid insta earrape
				}
				
			}
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

			if ( !IsValidAddress( address ) )
			{
				StopDialing();
				return;
			}

			var otherGate = FindByAddress( address );
			if ( !otherGate.IsValid() || otherGate == this || !otherGate.IsStargateReadyForInboundInstantSlow() )
			{
				StopDialing();
				return;
			}

			otherGate.BeginInboundSlow( Address );

			for ( var i = 1; i <= address.Length; i++ )
			{
				var chev = GetChevronBasedOnAddressLength( i, address.Length );
				if ( chev.IsValid() )
				{
					chev.TurnOn();
					if ( i == 1 ) PlaySound( chev, "chevron_sg1_open" ); // play once to avoid insta earrape
				}
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

			var otherGate = FindByAddress( address );
			if ( otherGate.IsValid() && otherGate != this && otherGate.IsStargateReadyForInboundDHD() )
			{
				otherGate.BeginInboundDHD( Address, DialingAddress.Length );
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

	public async override void BeginInboundDHD( string address, int numChevs = 7 )
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
				if ( chev.IsValid() )
				{
					chev.TurnOn();
					if ( i == 1 ) PlaySound( chev, "chevron_sg1_open" ); // play once to avoid insta earrape
				}

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

		//var clampLen = DialingAddress.Length.Clamp( 7, 9 );

		var chev = GetChevronBasedOnAddressLength(DialingAddress.Length, 9 );
		EncodedChevronsOrdered.Add( chev );

		chev?.TurnOn(0.1f);
		PlaySound( chev, "chevron_sg1_open", 0.15f );
	}

	public override void DoChevronLock( char sym ) // only the top chevron locks, always
	{
		base.DoChevronLock( sym );

		var chev = GetTopChevron();
		EncodedChevronsOrdered.Add( chev );

		PlaySound( chev, "chevron_sg1_open", 0.15f );
		PlaySound( chev, "chevron_sg1_close", 1.05f );
		chev?.ChevronAnim( "lock", 0.2f );
		chev?.ChevronAnim( "unlock", 1f );

		var gate = FindByAddress( DialingAddress );
		if (gate != this && gate.IsValid() && gate.IsStargateReadyForInboundDHD()) chev?.TurnOn(0.5f);
	}

	public override void DoChevronUnlock( char sym )
	{
		base.DoChevronUnlock( sym );

		var chev = EncodedChevronsOrdered.Last();
		EncodedChevronsOrdered.Remove( chev );

		chev?.TurnOff();
	}

}
