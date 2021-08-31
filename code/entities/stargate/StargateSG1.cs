using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

[Library( "ent_stargate_sg1", Title = "Stargate SG-1", Spawnable = true, Group = "Stargate" )]
public partial class StargateSG1 : Stargate
{
	public StargateRing Ring;
	public List<Chevron> Chevrons = new ();
	private List<int> ChevronAngles = new ( new int[] { 40, 80, 120, 240, 280, 320, 0, 160, 200 } );

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

		Log.Info( Address );
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
		var success = await Ring.RotateRingToSymbolAsync( sym, angOffset );
		return success;
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
		return Chevrons[num - 1];
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
		if (delay > 0) await GameTask.DelaySeconds( delay );

		foreach ( Chevron chev in Chevrons ) chev.Glowing = state;
	}

	public override void OnStopDialingBegin()
	{
		base.OnStopDialingBegin();

		Sound.FromEntity( "dial_fail_sg1", this);
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

	// TEST STUFF

	public async override void BeginDialFast(string address)
	{
		if ( Busy || Inbound || Dialing || Open) return;

		if ( address.Equals( Address ) ) return;

		if ( !IsValidAddress( address ) )
		{
			Dialing = true;
			StopDialing();
			return;
		}

		var targetGate = FindByAddress( address );

		var wasTargetGateValidOnDialStart = false;
		var wasDialStopped = false;

		if (targetGate.IsValid() && targetGate != this && !targetGate.Open && !targetGate.Busy && !targetGate.Inbound)
		{
			wasTargetGateValidOnDialStart = true;
			targetGate.BeginInboundFast( this.Address, targetGate.Address.Length );
			OtherGate = targetGate;
		}

		Dialing = true;

		var timeStart = RealTime.Now; // dial start time

		// start rotating ring
		Ring.StartRingRotation();

		var addrLen = address.Length;

		// duration of the dial until the gate starts opening - let's stick to 7 seconds for total (just like GMod stargates)
		// default values are for 7 chevron sequence
		var chevronsStartDelay = 0.7f;
		var chevronsLoopDuration = 3.9f;
		var chevronBeforeLastDelay = 1.05f;
		var chevronAfterLastDelay = 1.35f;

		if ( addrLen == 8 )
		{
			chevronsStartDelay = 0.7f;
			chevronsLoopDuration = 4.25f;
			chevronBeforeLastDelay = 0.8f;
			chevronAfterLastDelay = 1.25f;
		}
		else if ( addrLen == 9 )
		{
			chevronsStartDelay = 0.6f;
			chevronsLoopDuration = 4.4f;
			chevronBeforeLastDelay = 0.75f;
			chevronAfterLastDelay = 1.25f;
		}

		var chevronDelay = chevronsLoopDuration / (addrLen - 1);

		await GameTask.DelaySeconds( chevronsStartDelay ); // wait 0.5 sec and start locking chevrons
		
		for (var i = 1; i < addrLen; i++ )
		{
			if ( ShouldStopDialing && !wasDialStopped ) // check if we should stop dialing or not
			{
				Ring.StopRingRotation();
				wasDialStopped = true;
				StopDialing();
				return;
			}

			var chev = GetChevronBasedOnAddressLength( i, addrLen );

			if ( chev.IsValid() )
			{
				chev.Glowing = true;
				Sound.FromEntity( "chevron_dhd", this );
			}

			if (i == addrLen - 1) Ring.StopRingRotation(); // stop rotating ring when the last looped chevron locks

			await GameTask.DelaySeconds( chevronDelay ); // each chevron delay
			if ( !this.IsValid() ) return;
		}

		if ( ShouldStopDialing && !wasDialStopped ) // check if we should stop dialing at the end or not
		{
			wasDialStopped = true;
			StopDialing();
			return;
		}

		await GameTask.DelaySeconds( chevronBeforeLastDelay ); // wait before locking the last chevron
		if ( !this.IsValid() ) return;

		if ( ShouldStopDialing && !wasDialStopped ) // check if we should stop dialing at the end or not
		{
			wasDialStopped = true;
			StopDialing();
			return;
		}

		Busy = true; // so we cant stop dial at this point

		var topChev = GetChevron( 7 ); // lock last (top) chevron
		if ( topChev.IsValid())
		{
			//if ( !shouldStopDialingAtEnd )
			//{
			if ( wasTargetGateValidOnDialStart && targetGate.IsValid() && targetGate != this && !targetGate.Open && !targetGate.Busy && !targetGate.Dialing )
				{
					topChev.Glowing = true;
				}

				topChev.ChevronLockUnlock();
				Sound.FromEntity( "chevron_lock_sg1", this );
			//}
		}

		await GameTask.DelaySeconds( chevronAfterLastDelay ); // wait after the last chevron, then open the gate or fail dial (if gate became invalid/was busy)
		if ( !this.IsValid() ) return;

		if ( ShouldStopDialing && !wasDialStopped ) // check if we should stop dialing at the end or not
		{
			wasDialStopped = true;
			StopDialing();
			return;
		}

		var timeEnd = RealTime.Now - timeStart;
		Log.Info( $"Gate dial time: {timeEnd}" );

		if ( !wasDialStopped && wasTargetGateValidOnDialStart && targetGate.IsValid() && targetGate != this && !targetGate.Open && !targetGate.Busy && !targetGate.Dialing ) // if valid, open both gates
		{
			Dialing = false;

			targetGate.OtherGate = this;
			OtherGate = targetGate;

			targetGate.DoStargateOpen();
			DoStargateOpen();

			targetGate.Inbound = true;
		}
		else
		{
			await GameTask.DelaySeconds( 0.5f ); // if not valid, wait 0.5 sec, then fail and stop dialing
			if ( !this.IsValid() ) return;

			//if ( wasTargetGateValidOnDialStart && OtherGate.IsValid() && OtherGate != this && !OtherGate.Open && !OtherGate.Busy && !OtherGate.Dialing )
			//{
			//	OtherGate.StopDialing();
			//}

			if ( (ShouldStopDialing && !wasDialStopped && Dialing) || !wasTargetGateValidOnDialStart ) // check if we should stop dialing at the end or not
			{
				wasDialStopped = true;
				StopDialing();
				return;
			}
		}
	}

	public async override void BeginInboundFast( string address, int numChevs = 7 )
	{
		if ( Dialing ) return;

		Inbound = true;

		//var timeStart = RealTime.Now; // dial start time

		// duration of the dial until the gate starts opening - let's stick to 7 seconds for total (just like GMod stargates)
		// default values are for 7 chevron sequence
		var chevronsStartDelay = 0.5f;
		var chevronsLoopDuration = 6.75f;
		var chevronBeforeLastDelay = 0.5f;

		if ( numChevs == 8 )
		{
			chevronsStartDelay = 0.4f;
			chevronsLoopDuration = 6.6f;
			chevronBeforeLastDelay = 0.6f;
		}
		else if ( numChevs == 9 )
		{
			chevronsStartDelay = 0.25f;
			chevronsLoopDuration = 6.75f;
			chevronBeforeLastDelay = 0.5f;
		}

		var chevronDelay = chevronsLoopDuration / (numChevs);

		await GameTask.DelaySeconds( chevronsStartDelay ); // wait 0.5 sec and start locking chevrons

		for ( var i = 1; i < numChevs; i++ )
		{
			if ( ShouldStopDialing ) // check if we should stop dialing or not
			{
				//Ring.StopRingRotation();
				//StopDialing();
				return;
			}

			var chev = GetChevronBasedOnAddressLength( i, numChevs );

			if ( chev.IsValid() )
			{
				chev.Glowing = true;
				Sound.FromEntity( "chevron_incoming", this );
			}

			await GameTask.DelaySeconds( chevronDelay ); // each chevron delay
			if ( !this.IsValid() ) return;
		}

		await GameTask.DelaySeconds( chevronBeforeLastDelay ); // wait before locking the last chevron
		if ( !this.IsValid() ) return;

		var topChev = GetChevron( 7 );
		if ( topChev.IsValid() )
		{
			topChev.Glowing = true;
			Sound.FromEntity( "chevron_incoming", this );
		}
	}

	public async override void BeginDialSlow( string address )
	{
		if ( Busy || Inbound ) return;

		Dialing = true;

		if ( !IsValidAddress( address ) )
		{
			StopDialing();
			return;
		}

		Stargate targetGate = null;

		var chevNum = 1;
		foreach ( var sym in address )
		{
			// try to encode each symbol
			var success = await RotateRingToSymbol( sym ); // wait for ring to rotate to the target symbol
			if ( !success )
			{
				Dialing = false;
				return;
			}

			await GameTask.DelaySeconds( 0.2f ); // wait a bit

			// go do chevron stuff

			var topChev = GetChevron( 7 );
			if (topChev.IsValid())
			{
				Sound.FromEntity( (sym.Equals( address.Last())) ? "chevron_lock" : "chevron_sg1", topChev );
				topChev.ChevronLockUnlock(); // play top chevron anim
			}

			if ( chevNum == address.Length )
			{
				targetGate = FindByAddress( address );
			}

			var chev = GetChevronBasedOnAddressLength( chevNum, address.Length);
			if (chev.IsValid())
			{
				await GameTask.DelaySeconds( 0.5f );
				if (chevNum == address.Length)
				{
					if ( targetGate.IsValid() && targetGate != this && !targetGate.Open && !targetGate.Busy )
					{
						chev.Glowing = true;
					}
				}
				else
				{
					chev.Glowing = true; // glow current chevron after a small delay
				}
			}

			if (chevNum == address.Length)
			{
				if ( targetGate.IsValid() && targetGate != this && !targetGate.Open && !targetGate.Busy )
				{
					targetGate.BeginInboundSlow( address, address.Length );
				}
			}

			await GameTask.DelaySeconds( 1.75f ); // wait a bit

			chevNum++;
		}

		if ( targetGate.IsValid() && targetGate != this && !targetGate.Open && !targetGate.Busy ) // if valid, open both gates
		{
			Dialing = false;

			targetGate.OtherGate = this;
			OtherGate = targetGate;

			targetGate.DoStargateOpen();
			DoStargateOpen();

			targetGate.Inbound = true;
		}
		else
		{
			StopDialing();
		}
	}

	public async override void BeginInboundSlow( string address, int numChevs = 7 )
	{
		Inbound = true;

		for ( var i = 1; i <= numChevs; i++ )
		{
			var chev = GetChevronBasedOnAddressLength( i, numChevs );
			if ( chev.IsValid() )
			{
				chev.Glowing = true;
			}
		}
		Sound.FromEntity( "chevron_incoming", this ); // play once to avoid insta earrape
	}

	public async override void BeginDialInstant( string address )
	{
		if ( Busy || Inbound || Dialing || Open) return;

		Dialing = true;

		if ( !IsValidAddress( address ) )
		{
			StopDialing();
			return;
		}

		var otherGate = FindByAddress( address );
		if ( !otherGate.IsValid() || otherGate.Busy || otherGate.Open || otherGate.Inbound || otherGate.Dialing )
		{
			StopDialing();
			return;
		}

		Busy = true;

		OtherGate = otherGate;
		otherGate.OtherGate = this;

		OtherGate.Inbound = true;
		OtherGate.Busy = true;

		for ( var i = 1; i <= address.Length; i++ )
		{
			var chev = GetChevronBasedOnAddressLength( i, address.Length );
			if ( chev.IsValid() )
			{
				chev.Glowing = true;
			}
		}
		Sound.FromEntity( "chevron_incoming", this ); // play once to avoid insta earrape

		otherGate.BeginInboundSlow( Address );

		await GameTask.DelaySeconds( 0.5f );

		DoStargateOpen();
		OtherGate.DoStargateOpen();

	}
}
