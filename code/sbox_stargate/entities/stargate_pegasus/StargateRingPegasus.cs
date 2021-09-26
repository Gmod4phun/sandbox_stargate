using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class StargateRingPegasus : ModelEntity
{
	// ring variables

	[Net]
	public StargatePegasus Gate { get; set; } = null;

	public string RingSymbols { get; private set; } = "?0JKNTR3MBZX*H69IGPL#@QFS1E4AU85OCW72YVD";

	//[Net, Predicted]
	public List<ModelEntity> SymbolParts { get; private set; } = new();

	public StringBuilder ActiveSymbols { get; private set; } = new ("000000000000000000000000000000000000");

	[Net]
	public string ActiveSymbolsString { get; private set; } = "000000000000000000000000000000000000";

	//[Net, Predicted]
	public List<int> DialSequenceActiveSymbols { get; private set; } = new();


	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;

		SetModel( "models/sbox_stargate/gate_atlantis/ring_atlantis.vmdl" );
		EnableAllCollisions = false;

		CreateSymbolParts();
	}


	// CLIENT RPC's for symbol anims

	// reset
	public void DoResetSymbols()
	{
		//ResetSymbols_Client( To.Everyone );
		ResetSymbols();
	}

	[ClientRpc]
	public void ResetSymbols_Client()
	{
		ResetSymbols();
	}

	// lightup
	public void DoLightupSymbols()
	{
		//LightupSymbols_Client( To.Everyone );
		LightupSymbols();
	}

	[ClientRpc]
	public void LightupSymbols_Client()
	{
		LightupSymbols();
	}

	// dialfast
	public void DoRollSymbolsDialFast( int chevCount, bool shouldLastGlow )
	{
		//RollSymbolsDialFast_Client( To.Everyone, chevCount );
		RollSymbolsDialFast( chevCount );
		RollChevronsDialFast( chevCount, shouldLastGlow );
	}

	[ClientRpc]
	public void RollSymbolsDialFast_Client( int chevCount )
	{
		RollSymbolsDialFast( chevCount );
	}

	// dialslow
	public void DoRollSymbolsDialSlow( int chevCount )
	{
		//RollSymbolsDialSlow_Client( To.Everyone, chevCount );
		RollSymbolsDialSlow( chevCount );
		RollChevronsDialSlow( chevCount );
	}

	[ClientRpc]
	public void RollSymbolsDialSlow_Client( int chevCount )
	{
		RollSymbolsDialSlow( chevCount );
	}

	// inbound
	public void DoRollSymbolsInbound( float time, float startDelay = 0, int chevCount = 7 )
	{
		//RollSymbolsInbound_Client( To.Everyone, time, startDelay, chevCount );
		RollSymbolsInbound( time, startDelay, chevCount );
		//RollChevronsInbond( time, startDelay, chevCount );
	}

	[ClientRpc]
	public void RollSymbolsInbound_Client( float time, float startDelay = 0, int chevCount = 7 )
	{
		RollSymbolsInbound( time, startDelay, chevCount );
	}


	// create symbols
	// symbol models

	public void AddSymbolPart(string name)
	{
		var part = new ModelEntity( name );
		part.Position = Position;
		part.Rotation = Rotation;
		part.SetParent( this );
		part.Transmit = TransmitType.Always;
		part.EnableAllCollisions = false;

		SymbolParts.Add( part );
	}

	public void CreateSymbolParts()
	{
		AddSymbolPart( "models/sbox_stargate/gate_atlantis/ring_atlantis_symbols_1_18.vmdl" );
		AddSymbolPart( "models/sbox_stargate/gate_atlantis/ring_atlantis_symbols_19_36.vmdl" );
	}

	protected override void OnDestroy()
	{
		foreach (var part in SymbolParts)
		{
			if ( IsServer && part.IsValid() ) part.Delete();
		}

		base.OnDestroy();
	}

	public int GetSymbolNum(int num)
	{
		return num.UnsignedMod( 36 );
	}

	public async void SetSymbolState(int num, bool state, float delay = 0)
	{
		if (delay > 0)
		{
			await Task.DelaySeconds( delay );
			if ( this.IsValid() ) return;
		}

		num = num.UnsignedMod( 36 );
		var isPart1 = num < 18;
		SymbolParts[isPart1 ? 0 : 1].SetBodyGroup( (isPart1 ? num : num - 18), state ? 1 : 0 );
	}

	public async Task<bool> RollSymbol(int start, int count, bool counterclockwise = false, float time = 2.0f)
	{
		if ( start < 0 || start > 35 ) return false;

		var delay = time / (count + 1);

		try
		{
			for ( var i = 0; i <= count; i++ )
			{
				if ( Gate.ShouldStopDialing ) return false;

				var symIndex = counterclockwise ? (start - i) : start + i;
				var symPrevIndex = counterclockwise ? (symIndex + 1) : symIndex - 1;

				SetSymbolState( symIndex, true );
				if ( !DialSequenceActiveSymbols.Contains( symPrevIndex.UnsignedMod(36) ) ) SetSymbolState( symPrevIndex, false );

				if (i == count)
				{
					DialSequenceActiveSymbols.Add( symIndex.UnsignedMod( 36 ) );
				}

				if ( i < count ) await Task.DelaySeconds( delay );
			}

			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public void ResetSymbols()
	{
		for ( int i = 0; i <= 35; i++ )	SetSymbolState( i, false );
		DialSequenceActiveSymbols.Clear();
	}

	public void LightupSymbols()
	{
		for ( int i = 0; i <= 35; i++ ) SetSymbolState( i, true );
	}

	// INBOUND
	public async void RollSymbolsInbound( float time, float startDelay = 0, int chevCount = 7 )
	{
		try
		{
			if ( startDelay > 0 ) await Task.DelaySeconds( startDelay );

			ResetSymbols();

			var delay = time / 35f;
			var startTime = Time.Now;

			int curChev = 1;
			for ( int i = 0; i <= 35; i++ )
			{
				var i_copy = i;
				var taskTime = startTime + (delay * i_copy);

				Gate.AddTask( taskTime, () => SetSymbolState( i_copy, true ) );

				if ( (i + 1) % 4 == 0 )
				{
					var chev = Gate.GetChevronClockwise( curChev, chevCount );
					void chevAction() => Gate.ChevronActivate( chev, delay * 0.5f, true, i_copy == 35, i_copy == 11 && chevCount == 7, i_copy == 31 );

					if ( (chevCount == 7 && i != 15 && i != 19) || (chevCount == 8 && i != 19) || (chevCount == 9) )
					{
						Gate.AddTask( taskTime, chevAction );
						curChev++;
					}
				}
			}

		}
		catch ( Exception ) { }
	}

	/*
	public async void RollChevronsInbond( float time, float startDelay = 0, int chevCount = 7 )
	{
		try
		{
			if ( startDelay > 0 ) await Task.DelaySeconds( startDelay );

			ResetSymbols();

			Stargate.PlaySound( Gate, Gate.GetSound( "gate_roll_fast" ) );

			var delay = time / 35f;
			var chevDelay = delay / 2f;

			chevDelay += Global.TickInterval;

			for ( int i = 0; i <= 35; i++ )
			{
				if ( Gate.ShouldStopDialing ) return;

				if ( i == 3 ) Gate.ChevronActivate( Gate.GetChevron( 1 ), chevDelay, true );
				if ( i == 7 ) Gate.ChevronActivate( Gate.GetChevron( 2 ), chevDelay, true );
				if ( i == 11 ) Gate.ChevronActivate( Gate.GetChevron( 3 ), chevDelay, true, false, chevCount == 7 ); // use longer sound if connection has 7 chevrons
				if ( chevCount > 7 )
				{
					if ( i == 15 ) Gate.ChevronActivate( Gate.GetChevron( 8 ), chevDelay, true );
					if ( chevCount > 8 ) if ( i == 19 ) Gate.ChevronActivate( Gate.GetChevron( 9 ), chevDelay, true );
				}

				if ( i == 23 ) Gate.ChevronActivate( Gate.GetChevron( 4 ), chevDelay, true );
				if ( i == 27 ) Gate.ChevronActivate( Gate.GetChevron( 5 ), chevDelay, true );
				if ( i == 31 ) Gate.ChevronActivate( Gate.GetChevron( 6 ), chevDelay, true );
				if ( i == 35 ) Gate.ChevronActivate( Gate.GetChevron( 7 ), chevDelay, true, true );

				if ( i < 35 ) await Task.DelaySeconds( delay );
			}
		}
		catch ( Exception )
		{

		}
	}
	*/


	// SLOW
	public async void RollSymbolsDialSlow(int chevCount)
	{
		try
		{
			ResetSymbols();

			var dataSymbols7 = new int[7, 2] { { 35, 32 }, { 3, 40 }, { 7, 32 }, { 11, 48 }, { 23, 32 }, { 27, 40 }, { 31, 32 } };
			var dataSymbols8 = new int[8, 2] { { 35, 32 }, { 3, 40 }, { 7, 32 }, { 11, 48 }, { 23, 32 }, { 27, 40 }, { 31, 52 }, { 15, 56 } };
			var dataSymbols9 = new int[9, 2] { { 35, 32 }, { 3, 40 }, { 7, 32 }, { 11, 48 }, { 23, 32 }, { 27, 40 }, { 31, 52 }, { 15, 40 }, { 19, 56 } };

			var data = (chevCount == 9) ? dataSymbols9 : ((chevCount == 8) ? dataSymbols8 : dataSymbols7);

			var startTime = Time.Now;

			for ( int i = 0; i < chevCount; i++ )
			{
				if ( Gate.ShouldStopDialing ) return;

				var startPos = data[i, 0];
				var symSteps = data[i, 1];
				var symTime = symSteps * 0.05f;

				RollSymbol( startPos, symSteps, i % 2 == 0, symTime );
				await Task.DelaySeconds( symTime );

				if ( Gate.ShouldStopDialing ) return;

				if (i < chevCount - 1) await Task.DelaySeconds( 1.25f );
			}

			Log.Info( $"SlowDial Symbols finished, time elapsed = {Time.Now - startTime}" );
		}
		catch ( Exception ) { }
	}

	public async void RollChevronsDialSlow( int chevCount )
	{
		try
		{
			var dataSymbols7 = new int[7, 2] { { 35, 32 }, { 3, 40 }, { 7, 32 }, { 11, 48 }, { 23, 32 }, { 27, 40 }, { 31, 32 } };
			var dataSymbols8 = new int[8, 2] { { 35, 32 }, { 3, 40 }, { 7, 32 }, { 11, 48 }, { 23, 32 }, { 27, 40 }, { 31, 52 }, { 15, 56 } };
			var dataSymbols9 = new int[9, 2] { { 35, 32 }, { 3, 40 }, { 7, 32 }, { 11, 48 }, { 23, 32 }, { 27, 40 }, { 31, 52 }, { 15, 40 }, { 19, 56 } };

			var data = (chevCount == 9) ? dataSymbols9 : ((chevCount == 8) ? dataSymbols8 : dataSymbols7);

			Sound? rollSound = null;

			for ( int i = 0; i < chevCount; i++ )
			{
				if ( Gate.ShouldStopDialing ) return;

				var symSteps = data[i, 1];
				var symTime = symSteps * 0.05f;

				rollSound = PlaySound( Gate.GetSound( "gate_roll_slow" ) );

				await Task.DelaySeconds( symTime + (Global.TickInterval * 12) );

				if ( rollSound.HasValue ) rollSound.Value.Stop();

				var chev = Gate.GetChevronBasedOnAddressLength( i + 1, chevCount );
				Gate.ChevronActivate( chev, 0, true, true );

				if ( Gate.ShouldStopDialing ) return;

				if ( i < chevCount - 1 ) await Task.DelaySeconds( 1.25f );
			}
		}
		catch ( Exception )
		{

		}
	}

	// FAST
	public async void RollSymbolsDialFast( int chevCount )
	{
		try
		{
			ResetSymbols();

			var symsRollTime = (chevCount == 9) ? 4.5f : ((chevCount == 8) ? 4.82f : 4.9f);

			var symRollTime = symsRollTime / chevCount;
			var delayBetweenSymbols = 1.5f / (chevCount - 1);

			var dataSymbols7 = new List<int>() { 27, 19, 35, 35, 15, 7, 23 };
			var dataSymbols8 = new List<int>() { 27, 19, 35, 35, 15, 7, 3, 11 };
			var dataSymbols9 = new List<int>() { 27, 19, 35, 35, 15, 7, 3, 31, 23 };

			var data = (chevCount == 9) ? dataSymbols9 : ((chevCount == 8) ? dataSymbols8 : dataSymbols7);

			var startTime = Time.Now;

			for (int i = 0; i < chevCount; i++ )
			{
				if ( Gate.ShouldStopDialing ) return;

				await RollSymbol( data[i], 12, i % 2 == 1, symRollTime );

				if ( i < chevCount - 1 )
				{
					await Task.DelaySeconds( delayBetweenSymbols );
				}
			}

			//Log.Info( Time.Now - startTime );
		}
		catch ( Exception )
		{
		}
	}

	public async void RollChevronsDialFast(int chevCount, bool shouldLastGlow)
	{
		var symsRollTime = (chevCount == 9) ? 4.5f : ((chevCount == 8) ? 4.82f : 4.9f);

		var symRollTime = symsRollTime / chevCount;
		var delayBetweenSymbols = 1.5f / (chevCount - 1);

		symRollTime += Global.TickInterval;
		delayBetweenSymbols += Global.TickInterval;

		for ( int i = 0; i < chevCount; i++ )
		{
			if ( !Gate.IsValid() ) return;

			if ( Gate.ShouldStopDialing ) return;

			await Task.DelaySeconds( symRollTime );

			if ( !Gate.IsValid() ) return;

			if ( i < 6 ) Gate.ChevronActivate( Gate.GetChevron( i + 1 ) ); // first 6 chevrons

			if ( chevCount == 7 ) // 7chevron address, chevron 7
			{
				if ( i == 6 ) Gate.ChevronActivate( Gate.GetChevron( 7 ), 0, shouldLastGlow, true );
			}
			else if ( chevCount == 8 ) // 8 chevron adress, chevrons 8 and 7
			{
				if ( i == 6 )
				{
					Gate.ChevronActivate( Gate.GetChevron( 8 ) );
				}
				else if ( i == 7 )
				{
					Gate.ChevronActivate( Gate.GetChevron( 7 ), 0, shouldLastGlow, true );
				}
			}
			else if ( chevCount == 9 ) // 9 chevron address, chevrons 8 9 and 7
			{
				if ( i == 6 )
				{
					Gate.ChevronActivate( Gate.GetChevron( 8 ) );
				}
				else if ( i == 7 )
				{
					Gate.ChevronActivate( Gate.GetChevron( 9 ) );
				}
				else if ( i == 8 )
				{
					Gate.ChevronActivate( Gate.GetChevron( 7 ), 0, shouldLastGlow, true );
				}
			}

			if ( i < chevCount - 1 )
			{
				await Task.DelaySeconds( delayBetweenSymbols );
			}
		}
	}

	// DEBUG
	public void DrawSymbols()
	{
		if ( !this.IsValid() ) return;

		var deg = 10;
		var ang = Rotation.Angles();
		for ( int i = 0; i < 36; i++ )
		{
			var rotAng = ang.WithRoll( ang.roll - (i * deg) - deg);
			var newRot = rotAng.ToRotation();
			var pos = Position + newRot.Forward * 4 + newRot.Up * 117.5f;
			DebugOverlay.Text( pos, i.ToString(), Color.Yellow );
		}

		DebugOverlay.Text( Position, ActiveSymbolsString, Color.Yellow );
	}

	[Event.Frame]
	public void RingSymbolsDebug()
	{
		//DrawSymbols();
	}

}
