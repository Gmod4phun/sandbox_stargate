using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class StargateRing : AnimEntity
{
	// ring variables

	public Stargate Gate;
	public float RingAngle { get; private set; } = 0.0f;
	public float TargetRingAngle { get; private set; } = 0.0f;
	public float RingRotSpeed { get; private set; } = 0.0f;
	private float RingRotMinSpeed = 0.5f;
	private float RingRotMaxSpeed = 17f;
	public int RingRotDir { get; private set; } = 1;
	public bool RingShouldRotate { get; private set; } = false;
	private bool RingShouldAcc = false;
	private bool RingShouldDecc = false;
	public bool RingFreeSpin { get; private set; } = false;
	public String RingSymbols { get; private set; } = "?0JKNTR3MBZX*H69IGPL#@QFS1E4AU85OCW72YVD";

	private Sound RollSound;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/gmod4phun/stargate/gate_sg1/ring_sg1.vmdl" );

		Transmit = TransmitType.Always;
	}

	public void FlipRingRotDir()
	{
		RingRotDir = -RingRotDir;
	}

	public void PlayRollSound()
	{
		StopRollSound();
		RollSound = Sound.FromEntity( "gate_roll_long", this );
	}

	public void StopRollSound()
	{
		RollSound.Stop();
	}

	public void StartRingRotation() // start ring free spin rotation
	{
		FlipRingRotDir(); // swap the direction on each rotation call

		RingShouldRotate = true;
		RingFreeSpin = true;
		RingShouldAcc = true;
		RingShouldDecc = false;

		PlayRollSound();
	}

	public void StopRingRotation() // stop ring free spin rotation
	{
		RingShouldRotate = true;
		RingFreeSpin = true;
		RingShouldAcc = false;
		RingShouldDecc = true;

		StopRollSound();
	}

	public void RotateRingTo( float targetAng ) // gradually rotates the ring to a specific angle
	{
		FlipRingRotDir(); // swap the direction on each rotation call

		TargetRingAngle = targetAng;
		RingShouldRotate = true;
		RingFreeSpin = false;
		RingShouldAcc = true;
		RingShouldDecc = false;

		PlayRollSound();
	}

	public void StopRingRotationInstant() // instantly stops the ring rotation
	{
		RingRotSpeed = 0;
		RingShouldRotate = false;
		RingFreeSpin = false;
		RingShouldAcc = false;
		RingShouldDecc = false;

		StopRollSound();
	}

	public void ResetRingRotation() // stops rotation and resets the ring angle back to 0
	{
		RingAngle = 0;
		TargetRingAngle = 0;
		RingRotSpeed = 0;
		RingShouldRotate = false;
		RingShouldAcc = false;
		RingShouldDecc = false;
	}

	private void RingRotationLogic() // think logic for the ring rotation
	{
		if ( RingShouldRotate )
		{
			var angDiff = Math.Abs( RingAngle - TargetRingAngle );

			var angDeltaCalc = (1 / ((angDiff * 0.1f) + 1)) - 0.05f;

			if ( angDiff < 40f && RingShouldAcc && !RingFreeSpin )
			{
				RingShouldAcc = false;
				RingShouldDecc = true;
			}

			if ( RingShouldAcc && !RingShouldDecc && (RingRotSpeed != RingRotMaxSpeed) )
			{
				RingRotSpeed = RingRotSpeed.LerpTo( RingRotMaxSpeed, Time.Delta * 1.5f );
				if ( (RingRotMaxSpeed - RingRotSpeed) < 0.05 ) RingRotSpeed = RingRotMaxSpeed;
				//Log.Info($"Accelerating, curSpeed = {RingRotSpeed}");
			}
			else if ( RingShouldDecc && !RingShouldAcc && (RingRotSpeed != RingRotMinSpeed) )
			{
				if (!RingFreeSpin)
				{
					RingRotSpeed = RingRotSpeed.LerpTo( RingRotMinSpeed, Time.Delta * angDeltaCalc * 2.5f);
					if ( (RingRotSpeed - RingRotMinSpeed) < 0.05 ) RingRotSpeed = RingRotMinSpeed;
				}
				else
				{
					RingRotSpeed = RingRotSpeed.LerpTo( 0, Time.Delta * 2 );
					if ( Math.Abs( RingRotSpeed - 0 ) < 0.05 ) RingRotSpeed = 0;
				}
				//Log.Info( $"Deccelerating, curSpeed = {RingRotSpeed}" );
			}

			if (!RingFreeSpin)
			{
				if ( TargetRingAngle > RingAngle ) RingAngle += 0.1f * RingRotSpeed;
				else if ( TargetRingAngle < RingAngle ) RingAngle -= 0.1f * RingRotSpeed;
			}
			else
			{
				RingAngle += 0.1f * RingRotSpeed * -RingRotDir;
				if (RingAngle > 360f || RingAngle < 0f) RingAngle = RingAngle.UnsignedMod( 360f );
			}

			if ( RingShouldDecc && angDiff < 0.1f )
			{
				if ( angDiff != 0 && !RingFreeSpin)
				{
					RingAngle = TargetRingAngle;
					StopRingRotationInstant();
				}
			}

			if ( RingFreeSpin && RingRotSpeed == 0 )
			{
				RingShouldRotate = false;
				RingAngle = RingAngle.UnsignedMod( 360 );
			}

			LocalRotation = LocalRotation.Angles().WithRoll( RingAngle ).ToRotation();
		}
	}

	public float GetSymbolPosition(char sym) // gets the symbols position on the ring
	{
		sym = sym.ToString().ToUpper()[0];
		return RingSymbols.Contains( sym ) ? RingSymbols.IndexOf( sym ) : -1;
	}

	public float GetDesiredRingAngleForSymbol(char sym, int angOffset = 0)
	{
		// get the symbol's position on the ring
		var symPos = GetSymbolPosition(sym);

		// if we input an invalid symbol, return current ring angles
		if (symPos == -1) return RingAngle;

		// if its a valid symbol, lets calc the required angle
		var symAng = symPos * 9; // there are 40 symbols, each 9 degrees apart

		// clockwise and counterclockwise symbol angles relative to 0 (the top chevron)
		var D_CW = symAng - RingAngle - angOffset; // offset, if we want it to be relative to another chevron (for movie stargate dialing)
		var D_CCW = 360 - D_CW;

		D_CW = D_CW.UnsignedMod( 360 );
		D_CCW = D_CCW.UnsignedMod( 360 );

		// angle differences are setup, choose based on the direction of ring rotation
		// if the required angle to too small, spin it around once
		var angToRotate = (RingRotDir == -1) ? D_CCW : D_CW;
		if (angToRotate < 170f) angToRotate += 360f;

		// set the final angle to the current angle + the angle needed to rotate, also considering ring direction
		var finalAng = RingAngle + (angToRotate * RingRotDir);

		//Log.Info($"Sym = {sym}, RingAng = {RingAngle}, SymPos = {symPos}, D_CCW = {D_CCW}, D_CW = {D_CW}, finalAng = {finalAng}" );

		return finalAng;
	}

	public void RotateRingToSymbol(char sym, int angOffset = 0)
	{
		if ( RingSymbols.Contains( sym ) ) RotateRingTo( GetDesiredRingAngleForSymbol( sym, angOffset ) );
	}

	public async Task<bool> RotateRingToSymbolAsync(char sym, int angOffset = 0)
	{
		RotateRingToSymbol( sym, angOffset ); // rotate ring to the desired position

		var rotStartTime = Time.Now;

		while ( RingAngle != TargetRingAngle ) // wait until its rotated
		{
			await Task.DelaySeconds( 0.1f ); // we need to delay this otherwise the game hangs

			if ( Time.Now > rotStartTime + 30 ) return false;

			if ( !this.IsValid() || !Gate.IsValid() ) return false;

			if (Gate.Dialing && Gate.ShouldStopDialing)
			{
				Gate.ShouldStopDialing = false;
				StopRingRotation();
				return false;
			}
		}

		return true;
	}

	[Event( "server.tick" )]
	public void Think()
	{
		RingRotationLogic();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		StopRollSound();
	}
}
