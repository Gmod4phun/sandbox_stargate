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
			{ "gate_open", "gate_universe_open" },
			{ "gate_close", "gate_universe_close_2" },
			{ "gate_roll_fast", "gate_universe_roll_long" },
			{ "gate_roll_slow", "gate_universe_roll_long" },
			{ "gate_activate", "gate_universe_activate" },
			{ "symbol", "gate_universe_symbol_encode" },
			{ "chevron_dhd", "gate_universe_symbol_encode" },
			{ "dial_fail", "gate_universe_dial_fail" }
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

		PlaySound( this, GetSound( "dial_fail" ), 1f );
	}

	public override void OnStopDialingFinish()
	{
		base.OnStopDialingFinish();

		SetChevronsGlowState( false );
		Ring?.ResetSymbols();

		if ( !IsGateUpright() ) AddTask( Time.Now + 2.5f, () => DoResetGateRoll(), TimedTaskCategory.GENERIC );
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

		if ( !IsGateUpright() ) AddTask( Time.Now + 1.5f, () => DoResetGateRoll(), TimedTaskCategory.GENERIC );
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
		PlaySound( this, GetSound( "gate_activate" ) );
	}

	public bool IsGateUpright(float tolerance = 1f)
	{
		return MathF.Abs( 0 - (Ring.RingAngle.UnsignedMod( 360f )) ) < tolerance;
	}

	public void DoResetGateRoll()
	{
		if (Idle) Ring.RotateRingToSymbol( ' ' );
	}

	public void SymbolOn(char sym, bool nosound = false)
	{
		Ring.SetSymbolState( sym, true );
		if ( !nosound ) PlaySound( this, GetSound( "symbol" ) );
	}

	public void SymbolOff( char sym )
	{
		Ring.SetSymbolState( sym, false );
	}

	// INDIVIDUAL DIAL TYPES

	// FAST DIAL
	public override void BeginDialFast( string address )
	{
		if ( !CanStargateStartDial() ) return;

		try
		{
			CurGateState = GateState.DIALING;
			CurDialType = DialType.FAST;

			if ( !IsValidFullAddress( address ) ) { StopDialing(); return; }

			DoPreRoll();

			var target = FindDestinationGateByDialingAddress( this, address );
			var wasTargetReadyOnStart = false; // if target gate was not available on dial start, dont bother doing anything at the end

			if ( target.IsValid() && target != this && target.IsStargateReadyForInboundFast() )
			{
				wasTargetReadyOnStart = true;
				target.BeginInboundFast( GetSelfAddressBasedOnOtherAddress( this, address ).Length );
				OtherGate = target; // this is needed so that the gate can stop dialing if we cancel the dial
				OtherGate.OtherGate = this;
			}

			var startTime = Time.Now;
			var addrLen = address.Length;

			bool gateValidCheck() { return wasTargetReadyOnStart && target.IsValid() && target != this && target.IsStargateReadyForInboundFastEnd(); }

			var rollStartTime = startTime + 0.5f;
			var rollEndTime = rollStartTime + 4.8f;

			AddTask( rollStartTime, () => Ring.SpinUp(), TimedTaskCategory.DIALING );
			AddTask( rollEndTime, () => Ring.SpinDown(), TimedTaskCategory.DIALING );

			var symbolStartDelay = 0.5f;
			var symbolDelay = 5f / addrLen;

			// lets encode each chevron but the last
			for ( var i = 0; i < addrLen; i++ )
			{
				var i_copy = i;
				var symTime = rollStartTime + symbolStartDelay + (symbolDelay * i_copy);

				AddTask( symTime, () => SymbolOn( address[i_copy] ), TimedTaskCategory.DIALING );
			}

			async void openOrStop()
			{
				if ( gateValidCheck() ) // if valid, open both gates
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

			DoPreRoll();

			var rollStartTime = Time.Now + 0.5f;
			var rollEndTime = rollStartTime + 4.8f;

			AddTask( rollStartTime, () => Ring.SpinUp(), TimedTaskCategory.DIALING );
			AddTask( rollEndTime, () => Ring.SpinDown(), TimedTaskCategory.DIALING );
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

			DoPreRoll();

			await Task.DelaySeconds( 1.5f );

			Stargate target = null;
			var readyForOpen = false;

			bool gateValidCheck()
			{
				target = FindDestinationGateByDialingAddress( this, address ); // if its last chevron, try to find the target gate
				if ( target.IsValid() && target != this && target.IsStargateReadyForInboundInstantSlow() )
				{
					target.BeginInboundSlow( address.Length );
					return true;
				}

				return false;
			}

			foreach ( var sym in address )
			{
				var isLastChev = sym == address.Last();

				// try to encode each symbol
				var success = await RotateRingToSymbol( sym ); // wait for ring to rotate to the target symbol
				if ( !success || ShouldStopDialing )
				{
					ResetGateVariablesToIdle();
					return;
				}

				AddTask( Time.Now + 0.65f, () => SymbolOn(sym), TimedTaskCategory.DIALING);

				await Task.DelaySeconds( 1.25f );

				if ( isLastChev ) readyForOpen = gateValidCheck();
			}

			void openOrStop()
			{
				if ( readyForOpen ) // if valid, open both gates
				{
					EstablishWormholeTo( target );
				}
				else
				{
					StopDialing();
				}
			}

			AddTask( Time.Now + 1f, openOrStop, TimedTaskCategory.DIALING );
		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	// SLOW INBOUND
	public override void BeginInboundSlow( int numChevs )
	{
		if ( !IsStargateReadyForInboundInstantSlow() ) return;

		try
		{
			if ( Dialing ) DoStargateReset();

			CurGateState = GateState.ACTIVE;
			Inbound = true;

			DoPreRoll();

			ActiveChevrons = numChevs;
		}
		catch ( Exception )
		{
			if ( this.IsValid() ) StopDialing();
		}
	}

	public override void BeginDialInstant( string address )
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

			DoPreRoll();
			otherGate.BeginInboundSlow( address.Length );

			AddTask( Time.Now + 0.5f, () => EstablishWormholeTo( otherGate ), TimedTaskCategory.DIALING );

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

		if ( DialingAddress.Length == 1 ) DoPreRoll();

		AddTask( Time.Now + 0.25f, () => SymbolOn( sym, DialingAddress.Length == 1 ), TimedTaskCategory.DIALING );
	}

	public override void DoChevronLock( char sym ) // only the top chevron locks, always
	{
		base.DoChevronLock( sym );

		AddTask( Time.Now + 0.25f, () => SymbolOn( sym ), TimedTaskCategory.DIALING );
	}

	public override void DoChevronUnlock( char sym )
	{
		base.DoChevronUnlock( sym );

		SymbolOff( sym );

		if ( DialingAddress.Length == 0 ) DoStargateReset();
	}

}
