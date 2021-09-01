using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class EventHorizon : AnimEntity
{
	public Stargate Gate;
	public bool IsFullyFormed = false;
	protected Sound WormholeLoop;

	protected Entity CurrentTeleportingEntity;

	// material VARIABLES - probably name this better one day

	// establish material variables
	float minFrame = 0f;
	float maxFrame = 18f;
	float curFrame = 0f;
	bool shouldBeOn = false;
	bool isOn = false;

	bool shouldBeOff = false;
	bool isOff = false;

	// puddle material variables
	float minBrightness = 1f;
	float maxBrightness = 8f;
	float curBrightness = 1f;

	bool shouldEstablish = false;
	bool isEstablished = false;

	bool shouldCollapse = false;
	bool isCollapsed = false;

	float lastSoundTime = 0f;

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;

		SetModel( "models/gmod4phun/stargate/event_horizon/event_horizon.vmdl" );
		SetMaterialGroup( 1 );
		SetupPhysicsFromModel( PhysicsMotionType.Static, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;
		EnableShadowCasting = false;

		EnableAllCollisions = false;
		EnableTouch = true;
	}

	// SERVER CONTROL

	public async void Establish()
	{
		EstablishClientAnim(); // clientside animation tuff

		await GameTask.DelaySeconds(1.5f);
		WormholeLoop = Sound.FromEntity( "wormhole_loop", this );
	}

	public async void Collapse()
	{
		CollapseClientAnim(); // clientside animation tuff

		await GameTask.DelaySeconds( 1f );
		WormholeLoop.Stop();
	}


	// UTILITY
	public void PlayTeleportSound()
	{
		if ( lastSoundTime + 0.1f < Time.Now ) // delay for playing sounds to avoid constant spam
		{
			lastSoundTime = Time.Now;
			Sound.FromEntity( "teleport", this );
		}
	}

	public bool IsEntityBehindEventHorizon( Entity ent )
	{
		return (ent.Position - Position).Dot( Rotation.Forward ) < 0;
	}

	public bool IsPawnBehindEventHorizon( Entity pawn )
	{
		return (pawn.EyePos - Position).Dot( Rotation.Forward ) < 0;
	}

	// CLIENT ANIM CONTROL

	[ClientRpc]
	public void EstablishClientAnim()
	{
		curFrame = minFrame;
		curBrightness = 0;
		shouldBeOn = true;
		shouldBeOff = false;

		SetMaterialGroup( 1 );
	}

	[ClientRpc]
	public void CollapseClientAnim()
	{
		curFrame = maxFrame;
		curBrightness = 1;
		shouldCollapse = true;
		shouldEstablish = false;

		SetMaterialGroup( 0 );
	}

	public void ClientAnimLogic()
	{
		if ( shouldBeOn && !isOn )
		{
			curFrame = MathX.Approach( curFrame, maxFrame, Time.Delta * 30 );
			SceneObject.SetValue( "frame", curFrame.FloorToInt() );

			if ( curFrame == maxFrame )
			{
				isOn = true;
				shouldEstablish = true;
				curBrightness = maxBrightness;
				SetMaterialGroup( 0 );

				//Particles.Create( "particles/water_squirt.vpcf", this, "center", true ); // only test, kawoosh particle will be made at some point
			}
		}

		if ( shouldBeOff && !isOff )
		{
			curFrame = MathX.Approach( curFrame, minFrame, Time.Delta * 30 );
			SceneObject.SetValue( "frame", curFrame.FloorToInt() );
			if ( curFrame == minFrame ) isOff = true;
		}

		if ( shouldEstablish && !isEstablished )
		{
			SceneObject.SetValue( "illumbrightness", curBrightness );
			curBrightness = MathX.Approach( curBrightness, minBrightness, Time.Delta * 5 );
			if ( curBrightness == minBrightness ) isEstablished = true;
		}

		if ( shouldCollapse && !isCollapsed )
		{
			SceneObject.SetValue( "illumbrightness", curBrightness );
			curBrightness = MathX.Approach( curBrightness, maxBrightness, Time.Delta * 5 );

			if ( curBrightness == maxBrightness )
			{
				isCollapsed = true;
				shouldBeOff = true;
				curBrightness = minBrightness;
				SetMaterialGroup( 1 );
			}
		}
	}

	// CLIENT LOGIC
	[Event( "client.tick" )]
	public void EventHorizonClientTick()
	{
		ClientAnimLogic();
	}

	[Event.Frame]
	public void ClientAlphaRenderLogic()
	{
		// draw the EH at 0.6 alpha when looking at it from behind -- doesnt work in thirdperson at the moment
		var pawn = Local.Pawn;
		if ( pawn.IsValid() ) RenderAlpha = IsPawnBehindEventHorizon(pawn) ? 0.6f : 1f;
	}

	// TELEPORT
	public async void TeleportEntity(Entity ent)
	{
		if ( !Gate.IsValid() || !Gate.OtherGate.IsValid() ) return;

		var otherEH = Gate.OtherGate.EventHorizon;

		if ( !otherEH.IsValid() ) return;

		// at this point, we should be able to teleport just fine

		CurrentTeleportingEntity = ent;
		Gate.OtherGate.EventHorizon.CurrentTeleportingEntity = ent;

		otherEH.PlayTeleportSound(); // other EH plays sound now

		var localVelNorm = Transform.NormalToLocal( ent.Velocity.Normal );
		var otherVelNorm = otherEH.Transform.NormalToWorld( localVelNorm.WithX( -localVelNorm.x ).WithY( -localVelNorm.y ) );

		var scaleDiff = otherEH.Scale / Scale;
		var localPos = Transform.PointToLocal( ent.Position );
		var otherPos = otherEH.Transform.PointToWorld( localPos.WithY( -localPos.y ) * scaleDiff );

		var localRot = Transform.RotationToLocal( ent.Rotation );
		var otherRot = otherEH.Transform.RotationToWorld( localRot.RotateAroundAxis(localRot.Up, 180f) );

		if (ent is SandboxPlayer ply)
		{
			var oldController = ply.DevController;
			using ( Prediction.Off() ) ply.DevController = new EventHorizonController();

			var DeltaAngleEH = otherEH.Rotation.Angles() - Rotation.Angles();

			ply.EyeRot = Rotation.From( ply.EyeRot.Angles() + new Angles( 0, DeltaAngleEH.yaw + 180, 0 ) );
			ply.Rotation = ply.EyeRot;

			await GameTask.NextPhysicsFrame();

			using ( Prediction.Off() ) ply.DevController = oldController;
		}
		else
		{
			ent.Rotation = otherRot;
		}

		ent.Position = otherPos;
		ent.ResetInterpolation();
		ent.Velocity = otherVelNorm * ent.Velocity.Length;

		// after any successful teleport, start autoclose timer if gate should autoclose
		if ( Gate.AutoClose ) Gate.AutoCloseTime = Time.Now + Stargate.AutoCloseTimerDuration;
	}

	public void DissolveEntity( Entity ent )
	{
		if ( ent is SandboxPlayer )
		{
			ent.Health = 1;
			var dmg = new DamageInfo();
			dmg.Attacker = Gate;
			dmg.Damage = 100;
			ent.TakeDamage( dmg );
		}
		else
		{
			ent.Delete();
		}
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( !IsServer ) return;

		if ( other is StargateIris ) return;

		if ( other is Sandbox.Player || other is Prop ) // for now only players and props get teleported
		{
			if ( other == CurrentTeleportingEntity ) return;

			PlayTeleportSound(); // event horizon always plays sound if something entered it

			// if ( !IsFullyFormed ) DissolveEntity( other ); -- still crashes, hold on

			if ( Gate.Inbound ) // if we entered inbound gate from any direction, dissolve
			{
				DissolveEntity( other );
			}
			else // we entered a good gate
			{
				if ( IsEntityBehindEventHorizon( other ) ) // check if we entered from the back and if yes, dissolve
				{
					DissolveEntity( other );
				}
				else // othwerwise we entered from the front, so now decide what happens
				{
					if ( !Gate.IsIrisClosed() ) // try teleporting only if our iris is open
					{
						if ( Gate.OtherGate.IsIrisClosed() ) // if other gate's iris is closed, dissolve
						{
							DissolveEntity( other );
							Gate.OtherGate.Iris.PlayHitSound(); // iris goes boom
						}
						else // otherwise we should fine for teleportation
						{
							if ( Gate.OtherGate.IsValid() && Gate.OtherGate.EventHorizon.IsValid() )
							{
								TeleportEntity( other );
							}
							else // if the other gate or EH is removed for some reason, dissolve
							{
								DissolveEntity( other );
							}
							
						}
					}
				}
			}

		}
	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );

		if ( !IsServer ) return;

		if ( other == CurrentTeleportingEntity )
		{
			CurrentTeleportingEntity = null;
			Gate.OtherGate.EventHorizon.CurrentTeleportingEntity = null;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		WormholeLoop.Stop();
	}

	[Event( "server.tick" )]
	public void EventHorizonTick()
	{
		if ( Gate.IsValid() && Scale != Gate.Scale ) Scale = Gate.Scale; // always keep the same scale as gate
	}
}
