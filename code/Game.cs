using System;
using System.Linq;
using Sandbox;

[Library( "sandbox", Title = "Sandbox Stargate" )]
partial class SandboxGame : Game
{

	public GateSpawner gateSpawner;
	public SandboxGame()
	{
		if ( IsServer )
		{
			// Create the HUD
			_ = new SandboxHud();

			// Stargate GateSpawner
			gateSpawner = new GateSpawner();
			gateSpawner.LoadGateSpawner();
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
	public static void SpawnEntity( string entName )
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

		if ( attribute.Group != null && attribute.Group.Contains("Stargate")) // spawn offsets for Stargate stuff
		{
			var type = ent.GetType();
			var property = type.GetProperty( "SpawnOffset" );
			if (property != null)
			{
				var offset = (Vector3) property.GetValue( ent );
				ent.Position += offset;
			}
		}

		if (ent is Stargate gate) // gate ramps
		{
			if (tr.Entity is IStargateRamp ramp) Stargate.PutGateOnRamp( gate, ramp );
		}

		//Log.Info( $"ent: {ent}" );
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
