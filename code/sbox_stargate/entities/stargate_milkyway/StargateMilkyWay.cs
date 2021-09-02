using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

[Library( "ent_stargate_sg1", Title = "Stargate SG-1", Spawnable = true, Group = "Stargate" )]
public partial class StargateMilkyWay : Stargate
{

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

		Address = GenerateRandomAddress(7);
	}

	// RING

	public void CreateRing()
	{
		Ring = new StargateRing();
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

	// DIALING

	public async void SetChevronsGlowState( bool state, float delay = 0)
	{
		if (delay > 0) await Task.DelaySeconds( delay );

		foreach ( Chevron chev in Chevrons ) chev.On = state;
	}

	public override void OnStopDialingBegin()
	{
		base.OnStopDialingBegin();

		Sound.FromEntity( "dial_fail_sg1", this);

		if ( Ring.IsValid() && Ring.RingRotSpeed != 0 ) Ring.StopRingRotation();
	}

	public override void OnStopDialingFinish()
	{
		base.OnStopDialingFinish();

		SetChevronsGlowState( false );
	}

	public override void OnStargateBeginOpen()
	{
		base.OnStargateBeginOpen();

		Sound.FromEntity( "gate_open_sg1", this );
	}

	public override void OnStargateOpened()
	{
		base.OnStargateOpened();
	}

	public override void OnStargateBeginClose()
	{
		base.OnStargateBeginClose();

		Sound.FromEntity( "gate_close", this );
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

	// INDIVIDUAL DIAL TYPES
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
			}

			Ring.StartRingRotation(); // start rotating ring

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
					chev.ChevronSound( "chevron_dhd" );
				}

				if ( i == addrLen - 1 ) Ring.StopRingRotation(); // stop rotating ring when the last looped chevron locks

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
				topChev.ChevronLockUnlockLong();
				topChev.ChevronSound( "chevron_lock_sg1" );
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
				if ( ShouldStopDialing ) return; // check if we should stop dialing or not

				var chev = GetChevronBasedOnAddressLength( i, numChevs );
				if ( chev.IsValid() )
				{
					chev.TurnOn();
					chev.ChevronSound( "chevron_incoming" );
				}

				await Task.DelaySeconds( chevronDelay ); // each chevron delay
			}

			await Task.DelaySeconds( chevronBeforeLastDelay ); // wait before locking the last chevron

			var topChev = GetChevron( 7 );
			if ( topChev.IsValid() )
			{
				topChev.TurnOn();
				topChev.ChevronSound( "chevron_incoming" );
			}
		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

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
			var chevNum = 1;
			foreach ( var sym in address )
			{
				// try to encode each symbol
				var success = await RotateRingToSymbol( sym ); // wait for ring to rotate to the target symbol
				if ( !success || ShouldStopDialing )
				{
					CurGateState = GateState.IDLE;
					return;
				}

				await Task.DelaySeconds( 0.2f ); // wait a bit

				// go do chevron stuff

				var topChev = GetChevron( 7 );
				if ( topChev.IsValid() )
				{
					topChev.ChevronSound( (sym.Equals( address.Last() )) ? "chevron_lock" : "chevron_sg1" );
					topChev.ChevronLockUnlock(); // play top chevron anim
					if ( chevNum != address.Length )
					{
						topChev.TurnOn();
						topChev.TurnOff( 1.5f );
					}

				}

				if ( chevNum == address.Length ) target = FindByAddress( address );

				var chev = GetChevronBasedOnAddressLength( chevNum, address.Length );
				if ( chev.IsValid() )
				{
					await Task.DelaySeconds( 0.5f );
					if ( ( chevNum == address.Length && target.IsValid() && target != this && target.IsStargateReadyForInboundInstantSlow() ) || chevNum != address.Length )
					{
						chev.TurnOn();
					}
				}

				if ( chevNum == address.Length )
				{
					if ( chevNum == address.Length && target.IsValid() && target != this && target.IsStargateReadyForInboundInstantSlow() )
					{
						target.BeginInboundSlow( address, address.Length );
						readyForOpen = true;
					}
				}

				await Task.DelaySeconds( 1.75f ); // wait a bit

				chevNum++;
			}

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
					if ( i == 1 ) chev.ChevronSound( "chevron_incoming" ); // play once to avoid insta earrape
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
					if ( i == 1 ) chev.ChevronSound( "chevron_incoming" ); // play once to avoid insta earrape
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
}
