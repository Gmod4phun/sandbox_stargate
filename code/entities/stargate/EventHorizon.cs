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
	

	// CLIENT ANIM CONTROL

	[ClientRpc]
	public void EstablishClientAnim()
	{
		curFrame = minFrame;
		curBrightness = 0;
		shouldBeOn = true;
		shouldBeOff = false;

		SetMaterialGroup( 1 );
		EnableShadowCasting = false;
	}

	[ClientRpc]
	public void CollapseClientAnim()
	{
		curFrame = maxFrame;
		curBrightness = 1;
		shouldCollapse = true;
		shouldEstablish = false;

		SetMaterialGroup( 0 );
		EnableShadowCasting = true;
	}

	// CLIENT ANIM LOGIC
	[Event( "client.tick" )]
	public void EventHorizonClientTick()
	{
		if (shouldBeOn && !isOn)
		{
			curFrame = MathX.Approach( curFrame, maxFrame, Time.Delta * 30 );
			SceneObject.SetValue( "frame", curFrame.FloorToInt() );

			if ( curFrame == maxFrame )
			{
				isOn = true;
				shouldEstablish = true;
				curBrightness = maxBrightness;
				SetMaterialGroup( 0 );
				EnableShadowCasting = true;

				//Particles.Create( "particles/water_squirt.vpcf", this, "center", true ); // only test, kawoosh particle will be made at some point

			}
		}

		if ( shouldBeOff && !isOff )
		{
			curFrame = MathX.Approach( curFrame, minFrame, Time.Delta * 30 );
			SceneObject.SetValue( "frame", curFrame.FloorToInt() );
			if ( curFrame == minFrame ) isOff = true;
		}

		if (shouldEstablish && !isEstablished)
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
				EnableShadowCasting = false;
			}
		}

	}


	// TELEPORT

	public async void TeleportEntity(Entity ent)
	{
		if ( !Gate.IsValid() || !Gate.OtherGate.IsValid() ) return;

		var otherEH = Gate.OtherGate.EventHorizon;

		if ( !otherEH.IsValid() ) return;

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

		Sound.FromEntity("teleport", this);
		Sound.FromEntity("teleport", otherEH);
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

		if ( Gate.Inbound ) return;

		if ( other is Sandbox.Player || other is Prop ) // for now only players and props get teleported
		{
			if ( !Gate.Iris.IsValid() || !Gate.Iris.Closed ) // try teleporting only if our iris is open
			{
				if ( Gate.OtherGate.Iris.IsValid() && Gate.OtherGate.Iris.Closed ) // if other iris is closed, dissolve
				{
					DissolveEntity( other );
					Gate.OtherGate.Iris.MakeHitSound();
				}
				else // otherwise we are fine for teleportation
				{
					TeleportEntity( other );
				}
			}
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
