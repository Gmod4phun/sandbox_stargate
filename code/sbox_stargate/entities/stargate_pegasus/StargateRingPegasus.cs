using System;
using System.Collections.Generic;
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

	public List<ModelEntity> SymbolParts = new();

	[Net]
	public StringBuilder ActiveSymbols { get; private set; } = new ("000000000000000000000000000000000000");

	public List<int> DialSequenceActiveSymbols = new();

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;

		SetModel( "models/sbox_stargate/gate_atlantis/ring_atlantis.vmdl" );
		EnableAllCollisions = false;

		CreateSymbolParts();

		//RollSymbolsDialFast(9);
	}

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
			if ( part.IsValid() ) part.Delete();
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
		ActiveSymbols[num] = state ? '1' : '0';
	}

	public async Task<bool> RollSymbol(int start, int count, bool counterclockwise = false, float delay = 0.05f)
	{
		if ( start < 0 || start > 35 ) return false;

		try
		{
			for ( var i = 0; i <= count; i++ )
			{
				var symIndex = counterclockwise ? (start - i) : start + i;
				var symPrevIndex = counterclockwise ? (symIndex + 1) : symIndex - 1;

				SetSymbolState( symIndex, true );
				if ( !DialSequenceActiveSymbols.Contains( symPrevIndex.UnsignedMod(36) ) ) SetSymbolState( symPrevIndex, false );

				await Task.DelaySeconds( delay );

				if (i == count)
				{
					DialSequenceActiveSymbols.Add( symIndex.UnsignedMod( 36 ) );
				}
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

	public async void RollSymbolsInbound( float time, float startDelay = 0, int chevCount = 7)
	{
		try
		{
			if ( startDelay > 0 ) await Task.DelaySeconds( startDelay );

			ResetSymbols();

			Stargate.PlaySound( Gate, Gate.GetSound( "gate_roll_fast" ) );

			await Task.DelaySeconds( 0.75f );

			var delay = time / 36f;
			var chevDelay = delay / 2f;

			for ( int i = 0; i <= 35; i++ )
			{
				if ( Gate.ShouldStopDialing )
				{
					Gate.StopDialing();
					return;
				}

				SetSymbolState( i, true );

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

				await Task.DelaySeconds( delay );
			}
		}
		catch (Exception)
		{

		}
	}

	public async void RollSymbolsDialSlow()
	{
		try
		{
			ResetSymbols();

			await RollSymbol( 34, 31, true );
			await Task.DelaySeconds( 1f );

			await RollSymbol( 3, 40 );
			await Task.DelaySeconds( 1f );

			await RollSymbol( 7, 32, true );
			await Task.DelaySeconds( 1f );

			await RollSymbol( 11, 48 );
			await Task.DelaySeconds( 1f );

			await RollSymbol( 23, 32, true );
			await Task.DelaySeconds( 1f );

			await RollSymbol( 27, 40 );
			await Task.DelaySeconds( 1f );

			await RollSymbol( 31, 32, true );
			await Task.DelaySeconds( 1f );
		}
		catch (Exception)
		{

		}

	}

	public async void RollSymbolsDialFast( int chevCount )
	{
		try
		{
			ResetSymbols();

			await RollSymbol( 27, 12 );
			await Task.DelaySeconds( 0.25f );

			await RollSymbol( 19, 12, true );
			await Task.DelaySeconds( 0.25f );

			await RollSymbol( 35, 12 );
			await Task.DelaySeconds( 0.25f );

			await RollSymbol( 35, 12, true );
			await Task.DelaySeconds( 0.25f );

			await RollSymbol( 15, 12 );
			await Task.DelaySeconds( 0.25f );

			await RollSymbol( 7, 12, true );
			await Task.DelaySeconds( 0.25f );

			if (chevCount > 7)
			{
				await RollSymbol( 3, 12 );
				await Task.DelaySeconds( 0.25f );

				if ( chevCount == 8 )
				{
					await RollSymbol( 11, 12, true );
					await Task.DelaySeconds( 0.25f );
				}
				else if ( chevCount == 9 )
				{
					await RollSymbol( 31, 12, true );
					await Task.DelaySeconds( 0.25f );
				}
			}

			if ( chevCount != 8 )
			{
				await RollSymbol( 23, 12 );
				await Task.DelaySeconds( 0.25f );
			}

		}
		catch ( Exception )
		{

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

		DebugOverlay.Text( Position, ActiveSymbols.ToString(), Color.Yellow );
	}

	[Event.Frame]
	public void RingSymbolsDebug()
	{
		DrawSymbols();
	}

}
