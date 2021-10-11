using System;
using System.Threading.Tasks;
using Sandbox;

public partial class Teltac
{

	private partial class TeltacInnerDoor : AnimEntity, IUse
	{

		public bool State = false;

		public bool IsUsable( Entity user ) => true;

		public bool OnUse( Entity user )
		{
			if ( State )
				Close();
			else
				Open();

			return false;
		}

		public void Open()
		{
			if ( State ) return;
			CurrentSequence.Name = "open";
			EnableAllCollisions = false;
			State = true;

			PlaySound( "teltac_centerdoor_open" );
		}

		public void Close()
		{
			if ( !State ) return;
			CurrentSequence.Name = "close";
			EnableAllCollisions = true;
			State = false;

			PlaySound( "teltac_centerdoor_close" );
		}

		public void Toggle()
		{
			if ( State ) Close();
			else Open();
		}

		public override void Spawn()
		{
			base.Spawn();

			Transmit = TransmitType.Always;
			SetModel( "models/sbox_stargate/ships/teltacv2/gibs/inner_door.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		}
	}

	private partial class TeltacDoor : AnimEntity, IUse
	{

		private bool open = false;

		public bool IsUsable( Entity user ) => true;

		public bool OnUse( Entity user )
		{

			if ( !open ) Open( true );
			else Close( true );

			return false;
		}

		public void Open( bool withParent = false )
		{
			if ( open ) return;
			if ( !(Parent as Teltac).IsGrounded ) return;
			open = true;
			CurrentSequence.Name = "open";
			EnableAllCollisions = false;
			if ( withParent )
				(Parent as Teltac).CurrentSequence.Name = "open";

			PlaySound( "teltac_outterdoor_open" );
		}

		public void Close( bool withParent = false )
		{
			if ( !open ) return;
			open = false;
			CurrentSequence.Name = "close";
			EnableAllCollisions = true;
			if ( withParent )
				(Parent as Teltac).CurrentSequence.Name = "close";

			PlaySound( "teltac_outterdoor_close" );
		}

		public void Toggle( bool withParent = false )
		{
			if ( open ) Close( withParent );
			else Open( withParent );
		}

		public override void Spawn()
		{
			base.Spawn();
			Transmit = TransmitType.Always;
			SetModel( "models/sbox_stargate/ships/teltacv2/teltac_door.vmdl" );

			CurrentSequence.Name = "idle";
			// Why we need to await next frame ???
			MakePhysics();
		}

		private async void MakePhysics()
		{
			await GameTask.NextPhysicsFrame();
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		}

	}

	private partial class TeltacSeat : AnimEntity, IUse
	{

		public SandboxPlayer Occupied;

		public bool IsDriverSeat = false;

		public Func<Entity, bool> OnEnter;
		public Func<Entity, bool> OnLeave;

		public override void Spawn()
		{
			base.Spawn();

			Transmit = TransmitType.Always;
			SetModel( "models/sbox_stargate/ships/teltacv2/gibs/chair.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );

			OnEnter = ( Entity user ) =>
			{
				if ( user is not SandboxPlayer player ) return false;
				if ( Occupied.IsValid() ) return false;

				user.Parent = this;
				user.Position = Position + Rotation.Backward * 25;
				user.Rotation = Rotation;
				user.EyeRot = Rotation;

				Occupied = player;

				if ( Math.Round( LocalRotation.z ) == 0 )
					_ = RotateToDrive();

				return true;
			};

			OnLeave = ( Entity user ) =>
			{
				if ( !Occupied.IsValid() ) return false;
				if ( user is not SandboxPlayer ) return false;
				if ( user != Occupied ) return false;

				user.Parent = null;
				Occupied = null;
				if ( Math.Round( LocalRotation.z ) == -1 )
					_ = RotateToLeave();

				return true;
			};
		}

		public async Task<bool> RotateToDrive()
		{
			var targetRot = LocalRotation.RotateAroundAxis( Vector3.OneZ * -1, 180 );
			while ( targetRot.Distance( LocalRotation ) > 9 )
			{
				await Task.Delay( 10 );
				LocalRotation = LocalRotation.RotateAroundAxis( Vector3.OneZ * -1, 5 );
			}
			return true;
		}

		public async Task<bool> RotateToLeave()
		{
			var targetRot = LocalRotation.RotateAroundAxis( Vector3.OneZ, 180 );
			while ( targetRot.Distance( LocalRotation ) > 9 )
			{
				if ( !IsValid ) break;
				LocalRotation = LocalRotation.RotateAroundAxis( Vector3.OneZ, 5 );
				await Task.Delay( 10 );
			}
			return true;
		}

		public bool IsUsable( Entity user ) => true;

		public bool OnUse( Entity user )
		{
			OnEnter( user );
			return false;
		}
	}

}
