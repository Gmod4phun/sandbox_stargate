using System;
using Sandbox;

[Library( "ent_stargate_ship_teltac", Title = "Teltac", Spawnable = true, Group = "Stargate" )]
public partial class Teltac : AnimEntity
{
	private struct InputState
	{
		public float throttle;
		public float turning;
		public float up;
		public float tilt;
		public float roll;

		public bool allowRotation;
		public bool resetRoll;

		public bool brake;

		public void Reset()
		{
			throttle = 0;
			turning = 0;
			up = 0;
			tilt = 0;
			roll = 0;
			brake = false;
			allowRotation = false;
			resetRoll = false;
		}
	}

	private struct AccelerationData
	{
		public float Forward;
		public float Right;
		public float Up;

		public float Roll;

		public AccelerationData( float forward = 0, float right = 0, float up = 0, float roll = 0 )
		{
			Forward = forward;
			Right = right;
			Up = up;
			Roll = roll;
		}
	}

	[Net]
	public Vector3 SpawnOffset { get; private set; } = new( 0, 0, 250 );
	public float MovementSpeed { get; internal set; }
	public bool Grounded { get; internal set; }

	public bool IsGrounded
	{
		get
		{

			var tr = Trace.Sweep( PhysicsBody, Transform, Transform.WithPosition( Position + Rotation.Down * 150 ) )
				.Ignore( this )
				.WorldOnly()
				.Run();

			return tr.Hit;
		}
	}

	private bool EngineOn = false;

	private InputState _currentInput;

	private TeltacDoor OutDoor;

	private TeltacSeat DriverSeat;
	private SandboxPlayer Driver;

	private AccelerationData _accel;

	private Sound _engineSound;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Default;

		SetModel( "models/sbox_stargate/ships/teltacv2/teltac.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

		CurrentSequence.Name = "idle";

		_accel = new AccelerationData();

		SpawnComponents();

		if ( EngineOn )
			_engineSound = PlaySound( "teltac_engine" );
	}

	private void SpawnComponents()
	{
		OutDoor = new TeltacDoor();
		OutDoor.SetParent( this );
		OutDoor.Rotation = Rotation;

		var door = new TeltacInnerDoor();
		door.SetParent( this );
		door.LocalPosition = Rotation.Forward * 300;
		door.Rotation = Rotation;

		var door2 = new TeltacInnerDoor();
		door2.SetParent( this );
		door2.LocalPosition = Rotation.Forward * -40;
		door2.Rotation = Rotation;

		var rings = new RingsAncient();
		rings.SetParent( this );
		rings.LocalPosition = Rotation.Up * 44;
		rings.Rotation = Rotation;
		rings.SetAddress( null );
		rings.PhysicsBody.BodyType = PhysicsBodyType.Dynamic;

		var rings2 = new RingsAncient();
		rings2.SetParent( this );
		rings2.LocalPosition = Rotation.Up * 10;
		rings2.Rotation = Rotation.RotateAroundAxis( Vector3.OneX, 180 );
		rings2.SetAddress( null );
		rings2.PhysicsBody.BodyType = PhysicsBodyType.Dynamic;

		var panel = new RingPanelGoauld();
		panel.SetParent( this );
		panel.LocalPosition = Rotation.Forward * 137 + Rotation.Right * -50 + Rotation.Up * 100;
		panel.Rotation = Rotation.Angles().WithYaw( 180 ).ToRotation();
		panel.PhysicsBody.BodyType = PhysicsBodyType.Dynamic;

		var d1Panel = new GoauldControlPanel();
		d1Panel.SetParent( this );
		d1Panel.Scale = 0.75f;
		d1Panel.LocalPosition = GetBoneTransform( "door1_panel", false ).Position;
		d1Panel.Rotation = Rotation.RotateAroundAxis( Vector3.OneZ, 90 );
		d1Panel.LinkButton( "1", ( Entity user ) =>
		{
			door.Toggle();
		} );

		var outDoorPanel = new GoauldControlPanel();
		outDoorPanel.SetParent( this );
		outDoorPanel.Scale = 0.75f;
		outDoorPanel.LocalPosition = GetBoneTransform( "outdoor_panel", false ).Position;
		outDoorPanel.Rotation = Rotation.RotateAroundAxis( Vector3.OneZ, 180 ).RotateAroundAxis( Vector3.OneY * -1, 8 );
		outDoorPanel.LinkButton( "1", ( Entity user ) =>
		{
			OutDoor.Toggle( true );
		} );

		var bone = GetBoneTransform( "pilot_chair", false );
		DriverSeat = new TeltacSeat();
		DriverSeat.SetParent( this );
		DriverSeat.EnableTraceAndQueries = true;
		DriverSeat.LocalPosition = bone.Position;
		DriverSeat.Rotation = Rotation;
		// Define custom Enter/Leave action for driver seat
		var oldEnterFunc = DriverSeat.OnEnter;
		DriverSeat.OnEnter = ( Entity user ) =>
		{
			if ( !oldEnterFunc( user ) ) return false;

			Drive( user as SandboxPlayer );

			return true;
		};
		var oldLeaveFunc = DriverSeat.OnLeave;
		DriverSeat.OnLeave = ( Entity user ) =>
		{
			if ( !oldLeaveFunc( user ) ) return false;

			Leave( user as SandboxPlayer );

			return true;
		};
	}

	private void Drive( SandboxPlayer player )
	{
		player.Vehicle = this;
		player.VehicleController = new TeltacController();
		player.VehicleCamera = new TeltacCamera();
		Driver = player;
	}

	private void Leave( SandboxPlayer player )
	{
		if ( !Driver.IsValid() || Driver != player ) return;
		Driver = null;
		player.Vehicle = null;
		player.SetParent( null );
		player.Position = Position;
		player.VehicleController = null;
		player.VehicleCamera = null;
	}

	private float _lastCheck = Time.Now;
	private void CheckOnGround()
	{
		if ( _lastCheck + 1 <= Time.Now )
		{

			_lastCheck = Time.Now;

			var seq = CurrentSequence.Name;
			var isGround = IsGrounded;

			if ( !isGround )
			{
				if ( seq == "hide" )
					CurrentSequence.Name = "unhide";

				if ( seq == "open" || seq == "close" )
				{
					CurrentSequence.Name = "fullunhide";
					OutDoor.Close();
				}
			}
			else
			{
				if ( seq == "unhide" || seq == "fullunhide" || seq == "idle" )
				{
					CurrentSequence.Name = "hide";
				}
			}
		}

	}

	[Event.Tick]
	public void OnTick()
	{
		if ( !IsServer ) return;
		CheckOnGround();
	}

	public override void Simulate( Client owner )
	{
		if ( !IsServer || owner == null ) return;

		using ( Prediction.Off() )
		{
			if ( Input.Pressed( InputButton.Use ) )
			{
				if ( owner.Pawn is SandboxPlayer player && !player.IsUseDisabled() )
				{
					DriverSeat.OnLeave( owner.Pawn );

					return;
				}
			}
		}

		if ( EngineOn )
		{
			_currentInput.throttle = (Input.Down( InputButton.Forward ) ? 1 : 0) + (Input.Down( InputButton.Back ) ? -1 : 0);
			_currentInput.turning = (Input.Down( InputButton.Left ) ? -1 : 0) + (Input.Down( InputButton.Right ) ? 1 : 0);
			_currentInput.up = (Input.Down( InputButton.Jump ) ? 1 : 0) + (Input.Down( InputButton.Duck ) ? -1 : 0);
			_currentInput.allowRotation = Input.Down( InputButton.Walk );
			_currentInput.resetRoll = Input.Pressed( InputButton.Zoom );
			_currentInput.roll = Input.MouseWheel;
		}

		if ( Input.Pressed( InputButton.Reload ) )
		{
			EngineOn = !EngineOn;

			if ( EngineOn )
				_engineSound.SetVolume( .1f );
			else
				_engineSound.SetVolume( 0 );
		}
	}

	[Event.Physics.PreStep]
	public void OnPrePhysicsStep()
	{
		if ( !IsServer )
			return;

		var selfBody = PhysicsBody;
		if ( !selfBody.IsValid() )
			return;

		var dt = Time.Delta;

		if ( _currentInput.throttle != 0 )
		{
			_accel.Forward += (_currentInput.throttle == 1 ? 20.0f : 0) + (_currentInput.throttle == -1 ? -20.0f : 0);
		}
		else
		{
			if ( _accel.Forward >= 0 )
				_accel.Forward = Math.Max( 0, _accel.Forward - 5f );
			else
				_accel.Forward += 5f;
		}

		if ( _currentInput.turning != 0 )
		{
			_accel.Right += (_currentInput.turning == 1 ? 20.0f : 0) + (_currentInput.turning == -1 ? -20.0f : 0);
		}
		else
		{
			if ( _accel.Right >= 0 )
				_accel.Right = Math.Max( 0, _accel.Right - 5f );
			else
				_accel.Right += 5f;
		}

		if ( _currentInput.up != 0 )
		{
			_accel.Up += (_currentInput.up == 1 ? 15.0f : 0) + (_currentInput.up == -1 ? -15.0f : 0);
		}
		else
		{
			if ( _accel.Up >= 0 )
				_accel.Up = Math.Max( 0, _accel.Up - 5f );
			else
				_accel.Up += 5f;
		}

		_accel.Forward = Math.Min( _accel.Forward, 1000 );
		_accel.Forward = Math.Max( _accel.Forward, -1000 );
		_accel.Up = Math.Min( _accel.Up, 750 );
		_accel.Up = Math.Max( _accel.Up, -750 );

		if ( EngineOn )
		{
			selfBody.GravityEnabled = false;
			if ( _currentInput.resetRoll )
			{
				var a = selfBody.Rotation.Angles();
				selfBody.Rotation = new Angles( a.pitch, a.yaw, 0 ).ToRotation();
			}
			if ( _currentInput.allowRotation )
			{
				var x = Input.MouseWheel * 10;
				var y = _currentInput.throttle * -1;
				var z = _currentInput.turning * -1;
				selfBody.AngularVelocity = Rotation.Forward * x + Rotation.Left * y + Rotation.Up * z;//new Vector3( x, y, z );
				selfBody.AngularDamping = 0;
			}
			else
			{
				selfBody.AngularDamping = 1;
				selfBody.Velocity = (Rotation.Forward * _accel.Forward) + (Rotation.Right * _accel.Right) + (Rotation.Up * _accel.Up);
			}
		}
		else
		{
			selfBody.GravityEnabled = true;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		_engineSound.Stop();

		if ( Driver.IsValid() )
			Leave( Driver );
	}
}
