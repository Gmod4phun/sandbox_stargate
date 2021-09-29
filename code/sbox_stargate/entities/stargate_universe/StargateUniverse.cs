using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

[Library( "ent_stargate_universe", Title = "Stargate (Universe)", Spawnable = true, Group = "Stargate.Stargate" )]
public partial class StargateUniverse : Stargate
{
	public StargateRingUniverse Ring;
	public List<Chevron> EncodedChevronsOrdered = new ();
	public Chevron Chevron;

	public StargateUniverse()
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
		SetModel( "models/sbox_stargate/gate_universe/gate_universe.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;
		EnableDrawing = false; // dont draw the base ent, the gate will be a part of the 'ring'

		CreateRing();
		CreateAllChevrons();

		GateGroup = "U@";
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

	public async Task<bool> RotateRingToSymbol( char sym, int angOffset = 0 )
	{
		return await Ring.RotateRingToSymbolAsync( sym, angOffset );
	}

	// CHEVRONS
	public void CreateAllChevrons()
	{
		var chev = new Chevron();
		chev.SetModel( "models/sbox_stargate/gate_universe/chevrons_universe.vmdl" );
		chev.Position = Ring.Position;
		chev.Rotation = Ring.Rotation;
		chev.SetParent( Ring );
		chev.Transmit = TransmitType.Always;
		chev.Gate = this;

		chev.ChevronStateSkins = new()
		{
			{ "Off", 0 },
			{ "On", 1 },
		};

		chev.UsesDynamicLight = false;

		Chevron = chev;
	}

	// DIALING

	public async void SetChevronsGlowState( bool state, float delay = 0)
	{
		if (delay > 0) await Task.DelaySeconds( delay );

		Chevron.On = state;
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

	public override void DoStargateReset()
	{
		if ( Dialing ) ShouldStopDialing = true;

		base.DoStargateReset();

		SetChevronsGlowState( false );
		Ring?.ResetSymbols();
	}


	public void DoPreRoll()
	{
		SetChevronsGlowState( true, 0.2f );
	}

	// INDIVIDUAL DIAL TYPES

	// FAST DIAL
	public async override void BeginDialFast( string address )
	{
		if ( !CanStargateStartDial() ) return;

		try
		{
			CurGateState = GateState.DIALING;
			CurDialType = DialType.FAST;

			if ( !IsValidFullAddress( address ) ) { StopDialing(); return; }

			Stargate target = FindDestinationGateByDialingAddress( this, address );
			var wasTargetReadyOnStart = false; // if target gate was not available on dial start, dont bother doing anything at the end

			if ( target.IsValid() && target != this && target.IsStargateReadyForInboundFast() )
			{
				wasTargetReadyOnStart = true;

				target.BeginInboundFast( GetSelfAddressBasedOnOtherAddress( this, address ).Length );

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

				//var chev = GetChevronBasedOnAddressLength( i, addrLen );
				//if ( chev.IsValid() )
				//{
				//	if ( MovieDialingType )
				//	{
				//		ChevronAnimLock( chev, 0, ChevronLightup );
				//	}
				//	else
				//	{
				//		ChevronActivate( chev, 0, ChevronLightup );
				//	}

				//	ActiveChevrons++;
				//}

				Ring.SetSymbolState( address[i-1], true );

				if ( i == addrLen - 1 ) Ring.SpinDown(); // stop rotating ring when the last looped chevron locks

				await Task.DelaySeconds( chevronDelay );
			}

			if ( ShouldStopDialing ) { StopDialing(); return; } // check if we should stop dialing

			await Task.DelaySeconds( chevronBeforeLastDelay ); // wait before locking the last chevron

			if ( ShouldStopDialing ) { StopDialing(); return; } // check if we should stop dialing

			Busy = true; // gate has to lock last chevron, lets go busy so we cant stop the dialing at this point

			/*
			var topChev = GetChevron( 7 ); // lock last (top) chevron
			if ( topChev.IsValid() )
			{
				if ( wasTargetReadyOnStart && target.IsValid() && target != this && target.IsStargateReadyForInboundFastEnd() )
				{
					if ( ChevronLightup ) topChev.TurnOn( 0.25f );
				}

				if ( MovieDialingType )
				{
					ChevronAnimLock( topChev, 0.2f );
				}
				else
				{
					ChevronAnimLock( topChev, 0.2f );
					ChevronAnimUnlock( topChev, 1f );
				}

				ActiveChevrons++;
			}
			*/

			if ( wasTargetReadyOnStart && target.IsValid() && target != this && target.IsStargateReadyForInboundFastEnd() )
			{
				Ring.SetSymbolState( address[addrLen], true );
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
			if ( Dialing ) DoStargateReset();

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
				if ( ShouldStopDialing && ActiveChevrons > 0 ) return; // check if we should stop dialing or not

				/*
				var chev = GetChevronBasedOnAddressLength( i, numChevs );
				if ( chev.IsValid() )
				{
					if ( MovieDialingType )
					{
						ChevronAnimLock( chev, 0, ChevronLightup );
					}
					else
					{
						ChevronActivate( chev, 0, ChevronLightup );
					}

					ActiveChevrons++;
				}
				*/

				await Task.DelaySeconds( chevronDelay ); // each chevron delay
			}

			await Task.DelaySeconds( chevronBeforeLastDelay - 0.4f ); // wait before locking the last chevron

			/*
			var topChev = GetChevron( 7 );
			if ( topChev.IsValid() )
			{
				if ( MovieDialingType )
				{
					ChevronAnimLock( topChev, 0, ChevronLightup );
				}
				else
				{
					ChevronActivate( topChev, 0, ChevronLightup );
				}

				ActiveChevrons++;
			}
			*/
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

			DoPreRoll();

			await Task.DelaySeconds(1.25f);

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

				await Task.DelaySeconds( 0.25f ); // wait a bit

				if ( isLastChev ) target = FindDestinationGateByDialingAddress( this, address ); // if its last chevron, try to find the target gate

				// go do chevron stuff

				Ring.SetSymbolState( sym, true );

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

				await Task.DelaySeconds( 0.5f ); // wait a bit

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
			if ( Dialing ) DoStargateReset();

			CurGateState = GateState.ACTIVE;
			Inbound = true;

			// turn on chevs here

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

			// turn on chevs here

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

			// turn on chevs here

		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	// CHEVRON STUFF - DHD DIALING
	public override void DoChevronEncode( char sym )
	{
		base.DoChevronEncode( sym );

		//var chev = GetChevronBasedOnAddressLength( DialingAddress.Length, 9 );
		//EncodedChevronsOrdered.Add( chev );

		// turn on chevs and symbol here

	}

	public override void DoChevronLock( char sym ) // only the top chevron locks, always
	{
		base.DoChevronLock( sym );

		//var chev = GetTopChevron();
		//EncodedChevronsOrdered.Add( chev );

		// turn on chevs and symbol here

		
	}

	public override void DoChevronUnlock( char sym )
	{
		base.DoChevronUnlock( sym );

		var chev = EncodedChevronsOrdered.Last();
		EncodedChevronsOrdered.Remove( chev );

		// turn on chevs and symbol here

	}

}
