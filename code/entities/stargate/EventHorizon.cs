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

	public void Establish()
	{
		EH_Establish(); // clientside animation tuff
	}

	public void Collapse()
	{
		EH_Collapse(); // clientside animation tuff
	}

	[ClientRpc]
	public void EH_Establish()
	{
		curFrame = minFrame;
		curBrightness = 0;
		shouldBeOn = true;
		shouldBeOff = false;

		SetMaterialGroup( 1 );
		EnableShadowCasting = false;
	}

	[ClientRpc]
	public void EH_Collapse()
	{
		curFrame = maxFrame;
		curBrightness = 1;
		shouldCollapse = true;
		shouldEstablish = false;

		SetMaterialGroup( 0 );
		EnableShadowCasting = true;
	}

	[Event( "client.tick" )]
	public void EH_ClientTick()
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

				//Particles.Create( "particles/water_squirt.vpcf", this, "center", true );

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

	public void TeleportEntity(Entity ent)
	{
		var otherEH = Gate.OtherGate.EventHorizon;

		if ( !otherEH.IsValid() ) return;

		var localVelNorm = this.Transform.NormalToLocal( ent.Velocity.Normal );
		var otherVelNorm = otherEH.Transform.NormalToWorld( localVelNorm.WithX( -localVelNorm.x ) );

		var localPos = this.Transform.PointToLocal( ent.Position );
		var otherPos = otherEH.Transform.PointToWorld( localPos.WithY( -localPos.y ) );

		var localRot = this.Transform.RotationToLocal( ent.Rotation );
		var otherRot = otherEH.Transform.RotationToWorld( localRot.RotateAroundAxis(localRot.Up, 180f) );

		if (ent is SandboxPlayer ply)
		{
			// player eye/body rotation todo

		}

		ent.Position = otherPos;
		ent.Rotation = otherRot;
		ent.ResetInterpolation();
		ent.Velocity = otherVelNorm * ent.Velocity.Length;
	}

	public void DissolveEntity( Entity ent )
	{
		if ( ent is SandboxPlayer )
		{
			ent.Health = 1;
			var dmg = new DamageInfo();
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

		if (other is Sandbox.Player || other is Prop)
		{
			Log.Info( $"I was touched by {other}" );

			TeleportEntity(other);
		}

		

		//if ( other != Stargate )
		//{
		//	if ( other is SandboxPlayer || other is Prop )
		//	{
		//		if ( IsServer )
		//		{
		//			if ( IsFullyFormed )
		//			{
		//				TeleportEntity( other );
		//			}
		//			else
		//			{
		//				DissolveEntity( other );
		//			}

		//		}
		//	}

		//}
	}

}
