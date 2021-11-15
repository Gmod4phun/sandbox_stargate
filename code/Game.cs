using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

[Library( "sandbox", Title = "Sandbox Stargate" )]
partial class SandboxGame : Game
{
	public SandboxGame()
	{
		if ( IsServer )
		{
			// Create the HUD
			_ = new SandboxHud();

			// Stargate GateSpawner
			GateSpawner.LoadGateSpawner();
		}
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );
		var player = new SandboxPlayer();
		player.Respawn();

		cl.Pawn = player;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	[ServerCmd( "spawn" )]
	public static void Spawn( string modelname )
	{
		var owner = ConsoleSystem.Caller?.Pawn;

		if ( ConsoleSystem.Caller == null )
			return;

		var tr = Trace.Ray( owner.EyePos, owner.EyePos + owner.EyeRot.Forward * 500 )
			.UseHitboxes()
			.Ignore( owner )
			.Run();

		var ent = new Prop();
		ent.Position = tr.EndPos;
		ent.Rotation = Rotation.From( new Angles( 0, owner.EyeRot.Angles().yaw, 0 ) ) * Rotation.FromAxis( Vector3.Up, 180 );
		ent.SetModel( modelname );
		ent.Position = tr.EndPos - Vector3.Up * ent.CollisionBounds.Mins.z;
		ent.Owner = owner;
	}

	[ServerCmd( "spawn_entity" )]
	public static void SpawnEntity( string entName, string data = null )
	{
		var owner = ConsoleSystem.Caller.Pawn;

		if ( owner == null )
			return;

		var attribute = Library.GetAttribute( entName );

		if ( attribute == null || !attribute.Spawnable )
			return;

		var tr = Trace.Ray( owner.EyePos, owner.EyePos + owner.EyeRot.Forward * 4096 )
			.UseHitboxes()
			.Ignore( owner )
			.Size( 2 )
			.Run();

		if ( !tr.Hit ) return;

		var ent = Library.Create<Entity>( entName );
		if ( ent is BaseCarriable && owner.Inventory != null )
		{
			if ( owner.Inventory.Add( ent, true ) )
				return;
		}

		ent.Position = tr.EndPos;
		ent.Rotation = Rotation.From( new Angles( 0, owner.EyeRot.Angles().yaw + 180f, 0 ) );
		ent.Owner = owner;

		if (ent is ISpawnFunction x )
		{
			x.SpawnFunction( owner, tr, data );
		}

		if ( attribute.Group != null && attribute.Group.Contains("Stargate")) // spawn offsets for Stargate stuff
		{
			var type = ent.GetType();
			var property_spawnoffset = type.GetProperty( "SpawnOffset" );
			if (property_spawnoffset != null) ent.Position += (Vector3) property_spawnoffset.GetValue( ent );

			
			var property_spawnoffset_ang = type.GetProperty( "SpawnOffsetAng" );
			if ( property_spawnoffset_ang != null )
			{
				var ang = (Angles) property_spawnoffset_ang.GetValue( ent );
				var newRot = (ent.Rotation.Angles() + ang).ToRotation();
				ent.Rotation = newRot;
			}
			
		}
		if ( tr.Entity is Ramp newRamp ) newRamp.PositionObject( ent );

	}

	public override void DoPlayerNoclip( Client player )
	{
		if ( player.Pawn is Player basePlayer )
		{
			if ( basePlayer.DevController is NoclipController )
			{
				Log.Info( "Noclip Mode Off" );
				basePlayer.DevController = null;
			}
			else
			{
				Log.Info( "Noclip Mode On" );
				basePlayer.DevController = new NoclipController();
			}
		}
	}

	[ServerCmd("undo")]
	public static void OnUndoCommand()
	{
		Client caller = ConsoleSystem.Caller;

		if ( !caller.IsValid() ) return;

		Entity ent = Prop.All.LastOrDefault(x => x.Owner == caller.Pawn && (x is not BaseCarriable));

		if ( ent.IsValid() )
		{
			ent.Owner.PlaySound( "balloon_pop_cute" );
			ent.Delete();
		}
	}
}
