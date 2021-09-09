using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

[Library( "ent_stargate_ringtest", Title = "Stargate Ring Test", Spawnable = true, Group = "Stargate" )]
public partial class StargateRingTest : PlatformEntity, IUse
{
	// ring variables

	public Stargate Gate;
	public float RingAngle { get; private set; } = 0.0f;
	public float TargetRingAngle { get; private set; } = 0.0f;
	public float RingRotSpeed { get; private set; } = 0.0f;
	private float RingRotMinSpeed = 0f;
	private float RingRotMaxSpeed = 50f;
	private float RingAcceleration = 15f;
	private float RingFriction = 0.25f;
	public int RingRotDir { get; private set; } = 1;
	public bool RingShouldRotate { get; private set; } = false;
	private bool RingShouldAcc = false;
	private bool RingShouldDecc = false;
	public bool RingFreeSpin { get; private set; } = false;
	public string RingSymbols { get; private set; } = "?0JKNTR3MBZX*H69IGPL#@QFS1E4AU85OCW72YVD";


	private float TestTime = 0f;

	private bool ShouldStopAtAngle = false;
	private float CurStopAtAngle = 0f;

	public override void Spawn()
	{
		Transmit = TransmitType.Always;

		SetModel( "models/gmod4phun/stargate/gate_sg1/ring_sg1.vmdl" );
		//PhysicsBody.BodyType = PhysicsBodyType.Static;
		EnableAllCollisions = false;

		SpawnSettings = Flags.LoopMovement;
		MoveDirType = PlatformMoveType.RotatingContinious;
		MoveDirIsLocal = true;
		MoveDir = Rotation.Up.EulerAngles;
		MoveDistance = 360;

		StartMoveSound = "gate_sg1_ring_start";
		MovingSound = "gate_sg1_ring_loop";
		StopMoveSound = "gate_sg1_ring_stop";

		base.Spawn();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void SpinUp()
	{
		RingShouldDecc = false;
		RingShouldAcc = true;
	}

	public void SpinDown()
	{
		RingShouldAcc = false;
		RingShouldDecc = true;

		TestTime = Time.Now;
	}

	public float CalcStopAngleForAngle( float targetAng )
	{
		//var localRot = Transform.RotationToLocal( Rotation ); // prop rotation instead
		//var ringAng = LocalRotation.Angles();

		return targetAng - 33.333f;
	}

	public void StopAtAngle(float targetAng)
	{
		TargetRingAngle = targetAng;
		CurStopAtAngle = CalcStopAngleForAngle( targetAng );
		ShouldStopAtAngle = true;

		Log.Info("should stop at angle");
	}

	public bool OnUse( Entity user )
	{
		if (!IsMoving)
		{
			ReverseMoving();
			SpinUp();
			
		}
		else
		{
			SpinDown();
		}
		

		//SpinUp();
		//StopAtAngle( -9f );

		return false;
	}

	public bool IsUsable( Entity user )
	{
		return true;
	}

	[Event.Tick.Server]
	public void Think()
	{
		var localRot = Transform.RotationToLocal( Rotation );
		RingAngle = LocalRotation.Angles().roll;

		if (RingShouldAcc )
		{
			if ( !IsMoving ) StartMoving();

			if ( RingRotSpeed < RingRotMaxSpeed)
			{
				RingRotSpeed += 0.2f * RingAcceleration * RingFriction;
			}
			else
			{
				RingRotSpeed = RingRotMaxSpeed;
				RingShouldAcc = false;
			}
		}
		else if ( RingShouldDecc )
		{
			if ( RingRotSpeed > RingRotMinSpeed )
			{
				RingRotSpeed -= 0.2f * RingAcceleration * RingFriction;
			}
			else
			{
				RingRotSpeed = RingRotMinSpeed;
				RingShouldDecc = false;
				StopMoving();

				var endTime = Time.Now - TestTime;

				Log.Info($"Ring rot finished, time from stop call = {endTime}, TargetAngle = {TargetRingAngle}, RingAngle = {RingAngle}");
			}
		}

		SetSpeed( RingRotSpeed );


		if (ShouldStopAtAngle && IsMoving)
		{
			if (!RingShouldDecc)
			{
				var angDiff = MathF.Abs( RingAngle - CurStopAtAngle );
				//Log.Info(angDiff);
				if (angDiff < 1f)
				{
					SpinDown();
					ShouldStopAtAngle = false;
					//Log.Info( "stopatangle spindown start" );
				}
			}
		}

		Log.Info( currentRotation );

	}
}
