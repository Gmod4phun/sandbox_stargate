using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

[Library( "ent_stargate_sg1", Title = "Stargate SG-1", Spawnable = true )]
public partial class StargateSG1 : Stargate
{
	public Ring Ring;
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

		Address = GenerateRandomAddress();

		Log.Info( Address );
	}

	// RING

	public void CreateRing()
	{
		Ring = new Ring();
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
		await GameTask.DelaySeconds( delay );
		foreach ( Chevron chev in Chevrons ) chev.Glowing = state;
	}

	public override void OnStopDialing()
	{
		base.OnStopDialing();

		Sound.FromEntity( "dial_fail_sg1", this);
		SetChevronsGlowState( false, 1.25f );
	}

	public override void OnStargateOpen()
	{
		base.OnStargateOpen();
		SetChevronsGlowState( true );
	}

	public override void OnStargateClose()
	{
		base.OnStargateClose();
		SetChevronsGlowState( false, 2.5f );
	}

	// TEST STUFF

	public async override void BeginDialFast(string address)
	{
		if ( !IsValidAddress( address ) ) return;

		var addrLen = address.Length;

		Ring.StartRingRotation();

		await GameTask.DelaySeconds( 0.5f );

		Chevron chev = null;

		for (var i = 1; i <= addrLen; i++ )
		{
			chev = GetChevron( i );

			if ( chev.IsValid() )
			{
				chev.Glowing = true;
				Sound.FromEntity( "chevron_dhd", this );
			}

			await GameTask.DelaySeconds( 0.7f );
		}

		await GameTask.DelaySeconds( 1f );

		chev = GetChevron( 7 );
		if (chev.IsValid())
		{
			chev.Glowing = true;
			chev.ChevronLockUnlock();
			Sound.FromEntity( "chevron_lock_sg1", this );
		}

		await GameTask.DelaySeconds( 1.5f );

		if (this.IsValid())
		{
			StargateOpen();
		}
		

		await GameTask.DelaySeconds( 1.5f );
		if (this.IsValid())
		{
			WormholeLoop = Sound.FromEntity( "wormhole_loop", this );
		}

		// close gate
		await GameTask.DelaySeconds( 5f );
		WormholeLoop.Stop();

		if (this.IsValid())
		{
			StargateClose();
		}
		
		
	}

	public async void TestDial( string addr )
	{
		String curAddr = "";

		//Log.Info( $"Started dialing sequence {addr}" );

		Dialing = true;

		Stargate targetGate = null;

		/*
		var chevNum = 1;
		foreach (var sym in addr)
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

			var topChev = Chevrons[6];
			if (topChev.IsValid())
			{
				Sound.FromEntity( (sym.Equals(addr.Last())) ? "chevron_lock" : "chevron_sg1", topChev );
				topChev.ChevronLockUnlock(); // play top chevron anim
			}

			if (chevNum == 7)
			{
				targetGate = FindByAddress( addr );
			}

			var chev = Chevrons[addr.IndexOf(sym)];
			if (chev.IsValid())
			{
				await GameTask.DelaySeconds( 0.5f );
				if (chevNum == 7)
				{
					if (targetGate.IsValid())
					{
						chev.Glowing = true; // glow current chevron after a small delay
					}
				}
				else
				{
					chev.Glowing = true; // glow current chevron after a small delay
				}
			}

			await GameTask.DelaySeconds( 1.75f ); // wait a bit

			curAddr += sym; // append symbol to final address

			chevNum++;

			//Log.Info( $"Current dialed sequence: {curAddr}" );
		}
		*/

		targetGate = FindByAddress( addr );

		if (targetGate.IsValid() && targetGate != this && !targetGate.Open)
		{
			Dialing = false;

			targetGate.OtherGate = this;
			OtherGate = targetGate;

			targetGate.StargateOpen();
			StargateOpen();

			targetGate.Inbound = true;

			//Log.Info( $"Done, dialed sequence {curAddr}" );

			//await GameTask.DelaySeconds( 15f );

			//targetGate.StargateClose();
			//StargateClose();
		}
		else
		{
			StopDialing();
		}

	}

}
