using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox;

public partial class Rings : AnimEntity, IUse
{

	public static readonly float MAX_RING_RANGE = 1024f;


	[Net]
	public string Address { get; protected set; }

	protected int returnedRings = 0;

	protected const int AmountOfRings = 5;

	protected List<RingRing> ChildRings = new();

	protected Rings DestinationRings;

	protected Vector3 EndPos;

	public bool Busy { get; protected set; }

	protected bool IsUpsideDown
	{
		get
		{
			return Rotation.Up.Dot( new Vector3( 0, 0, -1 ) ) > 1 / Math.Sqrt( 2 );
		}
	}

	public bool HasAllRingsReachedPosition
	{
		get
		{
			return ChildRings.ToArray().All( x => x.Ready );
		}
	}

	public bool RingsDeployed { get; protected set; } = false;
	// public const string Symbols = "0123456789";
	public const string Symbols = "12345";

	public static bool IsAddressValid( string address )
	{

		foreach ( char sym in address )
		{
			if ( !Symbols.Contains( sym ) ) return false; // only valid symbols
			if ( address.Count( c => c == sym ) > 1 ) return false; // only one occurence
		}

		if ( Entity.All.OfType<Rings>().Where( x => x.Address == address ).Any() ) return false;

		return true;
	}

	public static string GenerateRandomAddress( int length = 4, bool checkValidity = false )
	{
		if ( length < 1 || length > 9 ) return "";

		StringBuilder symbolsCopy = new( Symbols );

		string generatedAddress = "";
		for ( int i = 0; i < length; i++ ) // pick random symbols without repeating
		{
			var randomIndex = new Random().Int( 0, symbolsCopy.Length - 1 );
			generatedAddress += symbolsCopy[randomIndex];

			symbolsCopy = symbolsCopy.Remove( randomIndex, 1 );
		}

		if ( checkValidity && !IsAddressValid( generatedAddress ) )
			return GenerateRandomAddress( length, checkValidity );

		return generatedAddress;
	}

	public static Rings GetClosestRing( Vector3 position, Rings[] exclude = null, float maxDistance = -1f )
	{

		var allRings = Entity.All.OfType<Rings>();

		Rings final = null;
		float dist = float.MaxValue;

		foreach ( Rings r in allRings )
		{
			if ( exclude is not null && exclude.Contains( r ) )
				continue;

			var currDistance = position.Distance( r.Position );
			if ( maxDistance != -1 && currDistance > maxDistance )
				continue;
			if ( currDistance < dist )
			{
				final = r;
				dist = currDistance;
			}
		}

		return final;

	}

	public Rings GetClosestRing()
	{
		return Rings.GetClosestRing( Position, new Rings[] { this } );
	}

	public override void Spawn()
	{
		Tags.Add( "no_rings_teleport" );

		this.Address = GenerateRandomAddress();
	}

	public virtual bool IsUsable( Entity user )
	{
		return true;
	}

	public virtual bool OnUse( Entity user )
	{
		// Make & show a rings menu
		return false;
	}

	[ServerCmd]
	public void DialClosest()
	{
		if ( IsClient )
			return;
		Rings ring = GetClosestRing();
		if ( ring is not null && ring.IsValid() )
			DialRing( ring );
	}

	[ServerCmd]
	public void DialAddress( string address )
	{
		if ( IsClient ) return;

		var other = Entity.All.OfType<Rings>().Where( x => x.Address == address ).FirstOrDefault();
		if ( other is not null && other.IsValid() )
			DialRing( other );
	}

	[ServerCmd]
	public void DialRing( Rings ring )
	{
		if ( IsClient ) return;
		if ( Busy ) return;
		if ( ring == this ) return;

		Busy = true;
		if ( ring.IsValid() && !ring.Busy )
		{
			ring.Busy = true;
			DestinationRings = ring;
			ring.DestinationRings = this;

			DeployRings( true );
		}
		else
			Busy = false;
	}

	public virtual void OnRingReturn()
	{
		returnedRings++;

		if ( returnedRings < AmountOfRings )
			return;

		returnedRings = 0;
		ShowBase();
		EnableAllCollisions = true;
		Busy = false;
		DestinationRings = null;
		RingsDeployed = false;
	}

	public Particles PlayTeleportEffect()
	{
		var particlePos = Vector3.Zero;
		var particleVelocity = Vector3.Zero;
		var destEndPosZ = Math.Floor( DestinationRings.Transform.PointToWorld( DestinationRings.EndPos ).z );
		var selfEndPosZ = Math.Floor( Transform.PointToWorld( EndPos ).z );
		if ( IsUpsideDown )
		{
			if ( destEndPosZ >= selfEndPosZ )
			{
				particlePos = Transform.PointToWorld( ChildRings[^1].LocalPosition );
				particleVelocity = Rotation.Down * -110;
			}
			else if ( destEndPosZ < selfEndPosZ )
			{
				particlePos = Transform.PointToWorld( ChildRings[0].LocalPosition );
				// particlePos = Position;
				particleVelocity = Rotation.Up * -110;
			}
		}
		else
		{
			if ( destEndPosZ <= selfEndPosZ )
			{
				particlePos = Transform.PointToWorld( EndPos ) - Rotation.Down * 50;
				particleVelocity = Rotation.Up * -110;
			}
			else if ( destEndPosZ > selfEndPosZ )
			{
				// particlePos = Transform.PointToWorld(EndPos) + Rotation.Down * 50;
				particlePos = Position;
				particleVelocity = Rotation.Up * 110;
			}
		}
		var a = Rotation.Angles();

		var particle = Particles.Create( "particles/sbox_stargate/rings_transporter.vpcf", particlePos );
		particle.SetPosition( 1, particleVelocity );
		particle.SetPosition( 2, new Vector3( a.roll, a.pitch, a.yaw ) );
		particle.SetPosition( 3, new Vector3( Scale, 0, 0 ) );

		return particle;
	}

	protected virtual async void HideBase()
	{
		CurrentSequence.Name = "down";

		await Task.DelaySeconds( CurrentSequence.Duration );

		RenderColor = RenderColor.WithAlpha( 0 );
	}

	protected virtual void ShowBase()
	{
		RenderColor = RenderColor.WithAlpha( 1 );
		CurrentSequence.Name = "up";
	}

	public bool IsAbleToExpand()
	{
		TraceResult tr = Trace.Ray( Position + Rotation.Up * 10, Position + Rotation.Up * 150 )
			.Run();

		// Object too close, impossible to deploy rings
		if ( tr.Hit && tr.Distance < 100 )
			return false;

		return true;
	}

	public async virtual void DeployRings( bool withTeleport = false )
	{

		Busy = true;

		// Not enough space
		if ( !IsAbleToExpand() || (DestinationRings.IsValid() && !DestinationRings.IsAbleToExpand()) )
		{
			Busy = false;
			DestinationRings.Busy = false;
			return;
		}
		else if ( DestinationRings.IsValid() && withTeleport )
		{
			DestinationRings.DeployRings();
		}


		ChildRings.Clear();

		// Make the base entity static to prevent droping under the map ...
		if ( !Parent.IsValid() )
			PhysicsBody.BodyType = PhysicsBodyType.Static;
		PlaySound( "ring_transporter2" );

		// Playing the animation
		HideBase();

		// Disable base collisions
		EnableAllCollisions = false;

		// Avoid making a calculation 2 times foreach ring
		var isUpDown = IsUpsideDown;

		var tr = Trace.Sweep( PhysicsBody, Transform.WithPosition( Position + Rotation.Up * 110 ), Transform.WithPosition( Position + Rotation.Up * MAX_RING_RANGE ) )
			.Ignore( this )
			.Run();

		var hitGround = false;
		if ( isUpDown && tr.Hit )
			hitGround = true;

		for ( int i = 0; i < 5; i++ )
		{

			var endPos = hitGround ? tr.EndPos - (Rotation.Up * 110) + (Rotation.Up * 20) * (i + 1) : Position + (Rotation.Up * 20) * (i + 1);
			var endPos2 = hitGround ? tr.EndPos - Rotation.Up * 50 : Position + (Rotation.Up * 50);
			EndPos = Transform.PointToLocal( endPos2 );

			RingRing r = new();

			r.RingParent = this;
			r.SetParent( this );
			r.isUpsideDown = isUpDown;
			r.Position = Position;
			r.Rotation = Rotation;
			if ( r.isUpsideDown ) r.Rotation = Rotation.RotateAroundAxis( Vector3.Left, 180f );
			r.Scale = Scale;
			r.Transmit = TransmitType.Always;

			r.desiredPos = Transform.PointToLocal( endPos );

			ChildRings.Add( r );
		}

		int[] times = { 2000, 500, 500, 500, isUpDown ? 100 : 200 };

		var reversed = ChildRings;
		reversed.Reverse();

		var y = 0;
		foreach ( RingRing r in reversed )
		{

			await Task.Delay( times[y] );

			if ( !this.IsValid() )
				return;

			if ( !r.IsValid() )
				return;

			r.MoveUp();

			y++;
		}

		RingsDeployed = true;

		if ( withTeleport )
			DoTeleport();
	}

	public async virtual void DoTeleport()
	{

		if ( !DestinationRings.IsValid() )
		{
			RetractRings();
			return;
		}

		List<Entity> toDest = new();
		List<Entity> fromDest = new();

		while ( !HasAllRingsReachedPosition || (DestinationRings.IsValid() && !DestinationRings.HasAllRingsReachedPosition) )
			await Task.Delay( 10 );

		var testPos = Transform.PointToWorld( EndPos );
		var toTp = Entity.All.Where( x => x.Position.Distance( testPos ) <= 80 );

		foreach ( Entity p in toTp )
		{

			if ( p.Tags.Has( "no_rings_teleport" ) )
				continue;

			toDest.Add( p );

		}

		if ( DestinationRings.IsValid() )
		{

			var testPos2 = DestinationRings.Transform.PointToWorld( DestinationRings.EndPos );
			var fromTp = Entity.All.Where( x => x.Position.Distance( testPos2 ) <= 80 && !x.Parent.IsValid() );

			foreach ( Entity p in fromTp )
			{

				if ( p.Tags.Has( "no_rings_teleport" ) )
					continue;

				fromDest.Add( p );
			}
		}

		var particle = PlayTeleportEffect();
		var particle2 = DestinationRings.PlayTeleportEffect();


		var worldEndPos = Transform.PointToWorld( EndPos );
		var tempBody = new PhysicsBody();
		tempBody.Position = worldEndPos;
		var tempBody2 = new PhysicsBody();
		tempBody2.Position = DestinationRings.Transform.PointToWorld( DestinationRings.EndPos );
		foreach ( Entity e in toDest )
		{
			var localPos = tempBody.Transform.PointToLocal( e.Position );
			var newPos = tempBody2.Transform.PointToWorld( localPos );

			e.Position = newPos;
			e.ResetInterpolation();
		}

		foreach ( Entity e in fromDest )
		{
			var localPos = tempBody2.Transform.PointToLocal( e.Position );
			var newPos = tempBody.Transform.PointToWorld( localPos );

			e.Position = newPos;
			e.ResetInterpolation();
		}

		tempBody.Remove();
		tempBody2.Remove();

		await Task.Delay( 500 );

		particle.Destroy();
		particle2.Destroy();

		await Task.Delay( 500 );

		RetractRings();
		DestinationRings.RetractRings();

		EndPos = Vector3.Zero;
	}

	public async virtual void RetractRings()
	{

		PlaySound( "ring_transporter3" );

		int[] times = { 400, 200, 300, 500, 200 };

		var tRings = ChildRings;
		tRings.Reverse();

		var i = 0;
		foreach ( RingRing r in tRings )
		{
			await Task.Delay( times[i] );

			r.desiredPos = LocalPosition.z;

			r.Refract();

			i++;
		}

		ChildRings.Clear();
	}

	[Event.Frame]
	public void OnFrame()
	{
		DebugOverlay.Text( Position, $"Address: {this.Address}", Color.White );

		// return;

		if ( !IsUpsideDown )
			return;

		var tr = Trace.Sweep( PhysicsBody, Transform.WithPosition( Position + Rotation.Up * 110 ), Transform.WithPosition( Position + Rotation.Up * 1024 ) ).Ignore( this ).Run();
		DebugOverlay.TraceResult( tr );
		DebugOverlay.Text( tr.EndPos - Rotation.Up * 30, tr.Distance.ToString(), Color.Magenta );
	}

	protected override void OnDestroy()
	{
		if ( DestinationRings.IsValid() )
			DestinationRings.RetractRings();
	}

	public void SetAddress( string address )
	{
		if ( Address == address ) return;

		if ( address == null ) Address = "";

		Address = address;
	}

}
