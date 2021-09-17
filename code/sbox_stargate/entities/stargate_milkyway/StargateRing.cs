using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class StargateRing : PlatformEntity
{
	// ring variables

	[Net]
	public Stargate Gate { get; set; } = null;

	public string RingSymbols { get; private set; } = "?0JKNTR3MBZX*H69IGPL#@QFS1E4AU85OCW72YVD";

	[Net]
	public float RingAngle { get; private set; } = 0.0f;
	[Net]
	public char CurDialingSymbol { get; private set; } = '!';
	[Net]
	public string CurRingSymbol { get; private set; } = "";
	public float TargetRingAngle { get; private set; } = 0.0f;

	private float RingCurSpeed = 0f;
	private float RingMaxSpeed = 50f;
	private float RingAccelStep = 1f;
	private float RingDeccelStep = 0.75f;

	private int RingDirection = 1;
	private bool ShouldAcc = false;
	private bool ShouldDecc = false;

	private bool ShouldStopAtAngle = false;
	private float CurStopAtAngle = 0f;

	private float StartedAccelAngle = 0f;
	private float StoppedAccelAngle = 0f;

	public string StartSoundName = "gate_roll_long";
	public string StopSoundName = "gate_sg1_ring_stop";

	private Sound? StartSoundInstance;
	private Sound? StopSoundInstance;

	public bool StopSoundOnSpinDown = false; // play the stopsound on spindown, or on spin stop 

	public override void Spawn()
	{
		Transmit = TransmitType.Always;

		SetModel( "models/sbox_stargate/gate_sg1/ring_sg1.vmdl" );

		SpawnSettings = Flags.LoopMovement;
		MoveDirType = PlatformMoveType.RotatingContinious;
		MoveDirIsLocal = true;
		MoveDir = Rotation.Up.EulerAngles;
		MoveDistance = 360;

		base.Spawn();

		EnableAllCollisions = false;
	}

	protected override void OnDestroy()
	{
		if ( StartSoundInstance.HasValue ) StartSoundInstance.Value.Stop();
		if ( StopSoundInstance.HasValue ) StopSoundInstance.Value.Stop();

		base.OnDestroy();
	}

	// symbol pos/ang
	public float GetSymbolPosition( char sym ) // gets the symbols position on the ring
	{
		sym = sym.ToString().ToUpper()[0];
		return RingSymbols.Contains( sym ) ? RingSymbols.IndexOf( sym ) : -1;
	}

	public float GetSymbolAngle( char sym ) // gets the symbols angle on the ring
	{
		sym = sym.ToString().ToUpper()[0];
		return GetSymbolPosition( sym ) * 9;
	}

	// sounds
	public void StopStartSound()
	{
		if ( StartSoundInstance.HasValue )
		{
			StartSoundInstance.Value.SetVolume( 0 );
		}
	}

	public void PlayStartSound()
	{
		StopStartSound();
		StartSoundInstance = PlaySound( StartSoundName );
	}

	public void StopStopSound()
	{
		if ( StopSoundInstance.HasValue ) StopSoundInstance.Value.SetVolume( 0 );
	}

	public void PlayStopSound()
	{
		StopStopSound();
		StopSoundInstance = PlaySound( StopSoundName );
	}

	// spinup/spindown - starts or stops rotating the ring
	public void SpinUp()
	{
		ShouldDecc = false;
		ShouldAcc = true;
	}

	public void SpinDown()
	{
		ShouldAcc = false;
		ShouldDecc = true;

		if ( StopSoundOnSpinDown )
		{
			PlayStopSound();
			StopStartSound();
		}
	}

	public void OnStart()
	{
		PlayStartSound();
	}

	public void OnStop()
	{
		if (Gate.IsValid() && Gate.Dialing)
		{
			if ( !StopSoundOnSpinDown )
			{
				PlayStopSound();
				StopStartSound();
			}
		}
	}

	// rotate to angle/symbol
	public void RotateRingTo( float targetAng ) // starts rotating the ring and stops (hopefully) at the specified angle
	{
		TargetRingAngle = targetAng;
		ShouldStopAtAngle = true;
		SpinUp();
	}

	public void RotateRingToSymbol( char sym, int angOffset = 0 )
	{
		if ( RingSymbols.Contains( sym ) ) RotateRingTo( GetDesiredRingAngleForSymbol( sym, angOffset ) );
	}

	// helper calcs
	public float GetDesiredRingAngleForSymbol( char sym, int angOffset = 0 )
	{
		// get the symbol's position on the ring
		var symPos = GetSymbolPosition( sym );

		// if we input an invalid symbol, return current ring angles
		if ( symPos == -1 ) return RingAngle;

		// if its a valid symbol, lets calc the required angle
		var symAng = symPos * 9; // there are 40 symbols, each 9 degrees apart

		// clockwise and counterclockwise symbol angles relative to 0 (the top chevron)
		var D_CW = -symAng - RingAngle - angOffset; // offset, if we want it to be relative to another chevron (for movie stargate dialing)
		var D_CCW = 360 - D_CW;

		D_CW = D_CW.UnsignedMod( 360 );
		D_CCW = D_CCW.UnsignedMod( 360 );

		// angle differences are setup, choose based on the direction of ring rotation
		// if the required angle to too small, spin it around once
		var angToRotate = (RingDirection == -1) ? D_CCW : D_CW;
		if ( angToRotate < 170f ) angToRotate += 360f;

		// set the final angle to the current angle + the angle needed to rotate, also considering ring direction
		var finalAng = RingAngle + (angToRotate * RingDirection);

		//Log.Info($"Sym = {sym}, RingAng = {RingAngle}, SymPos = {symPos}, D_CCW = {D_CCW}, D_CW = {D_CW}, finalAng = {finalAng}" );

		return finalAng;
	}

	public async Task<bool> RotateRingToSymbolAsync( char sym, int angOffset = 0 )
	{
		RotateRingToSymbol( sym, angOffset );
		CurDialingSymbol = sym;

		await Task.DelaySeconds( Global.TickInterval ); // wait, otherwise it hasnt started moving yet and can cause issues

		while (IsMoving)
		{
			await Task.DelaySeconds( Global.TickInterval ); // wait here, too, otherwise game hangs :)

			if ( Gate.ShouldStopDialing )
			{
				SpinDown();
				Gate.CurGateState = Stargate.GateState.IDLE;
				return false;
			}
		}

		return true;
	}

	public void RingSymbolThink() // keeps track of the current symbol under the top chevron
	{
		var symRange = 360f / RingSymbols.Length;
		var symCoverage = (RingAngle + symRange/2f).UnsignedMod( symRange );
		var symIndex = ((int) Math.Round(-RingAngle / ( symRange ))).UnsignedMod( RingSymbols.Length );
		
		CurRingSymbol = (symCoverage < 8 && symCoverage > 1) ? RingSymbols[symIndex].ToString() : "";
	}

	public void RingRotationThink()
	{
		if ( !Gate.IsValid() ) return;

		if ( IsMoving && Gate.ShouldStopDialing )
		{
			SpinDown();
			Gate.CurGateState = Stargate.GateState.IDLE;
		}

		if ( ShouldAcc )
		{
			if ( !IsMoving )
			{
				StartedAccelAngle = RingAngle;
				StartMoving();
				OnStart();
			}

			if ( RingCurSpeed < RingMaxSpeed )
			{
				RingCurSpeed += RingAccelStep;
			}
			else
			{
				RingCurSpeed = RingMaxSpeed;
				ShouldAcc = false;
				StoppedAccelAngle = MathF.Abs( RingAngle - StartedAccelAngle ) + 0.5f;
				CurStopAtAngle = TargetRingAngle - (StoppedAccelAngle * RingDirection * (RingAccelStep / RingDeccelStep));
			}
		}
		else if ( ShouldDecc )
		{
			if ( RingCurSpeed > 0 )
			{
				RingCurSpeed -= RingDeccelStep;
			}
			else
			{
				RingCurSpeed = 0;
				ShouldDecc = false;
				StopMoving();
				OnStop();

				ReverseMoving();
				CurrentRotation %= 360f;
			}
		}

		SetSpeed( RingCurSpeed );

		if ( ShouldStopAtAngle && IsMoving )
		{
			if ( !ShouldAcc && !ShouldDecc )
			{
				if ( MathF.Abs( CurrentRotation - CurStopAtAngle ) < 1f ) // if the angle difference is smal enough, start spindown
				{
					SpinDown();
					ShouldStopAtAngle = false;
				}
			}
		}

		RingAngle = CurrentRotation;
		RingDirection = IsMovingForwards ? -1 : 1;
	}

	[Event.Tick.Server]
	public void Think()
	{
		RingRotationThink();
		RingSymbolThink();
	}

	// DEBUG
	public void DrawSymbols()
	{
		var deg = 360f / RingSymbols.Length;
		var i = 0;
		var ang = Rotation.Angles();
		foreach ( char sym in RingSymbols )
		{
			var rotAng = ang.WithRoll( ang.roll - (i * deg) );
			var newRot = rotAng.ToRotation();
			var pos = Position + newRot.Forward * 4 + newRot.Up * 117.5f;
			DebugOverlay.Text( pos, sym.ToString(), sym == CurDialingSymbol ? Color.Green : Color.Yellow );
			i++;
		}

		DebugOverlay.Text( Position, CurRingSymbol, Color.White, 0, 512 );
	}

	[Event.Frame]
	public void RingSymbolsDebug()
	{
		DrawSymbols();
	}
}
