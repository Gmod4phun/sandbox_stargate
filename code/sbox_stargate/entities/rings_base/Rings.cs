using System.Numerics;
using System;
using System.Linq;
using System.Collections.Generic;
using Sandbox;

public partial class Rings : AnimEntity, IUse
{

	[Net]
	public string Address { get; protected set; }

	protected int returnedRings = 0;

	protected const int AmountOfRings = 5;

	protected List<RingRing> ChildRings = new();

	protected Rings DestinationRings;

	protected Vector3 EndPos;

	public bool Busy { get; protected set; }

	protected bool IsUpsideDown {
		get {
			return Rotation.Up.Dot(new Vector3(0, 0, -1)) > 1 / Math.Sqrt(2);
		}
	}

	public bool HasAllRingsReachedPosition {
		get {
			return ChildRings.ToArray().All(x => x.Ready);
		}
	}

	public override void Spawn() {
		Tags.Add( "no_rings_teleport" );
	}

	public virtual bool IsUsable( Entity user )
	{
		return true;
	}

	public virtual bool OnUse( Entity user )
	{
		if (Busy) return false;

		var other = Entity.All.OfType<Rings>().Where(x => x != this && !x.Busy).FirstOrDefault();
		if (other.IsValid()) {
			DestinationRings = other;
			other.DestinationRings = this;
			other.DeployRings();
		}
		DeployRings(true);

		return false;
	}

	public virtual void OnRingReturn() {
		returnedRings++;

		if (returnedRings < AmountOfRings)
			return;

		returnedRings = 0;
		CurrentSequence.Name = "up";
		EnableAllCollisions = true;
		Busy = false;
		DestinationRings = null;
	}

	public async virtual void DeployRings(bool withTeleport = false) {

		Busy = true;

		ChildRings.Clear();

		// Make the base entity static to prevent droping under the map ...
		PhysicsBody.BodyType = PhysicsBodyType.Static;
		PlaySound("ring_transporter2");

		// Playing the animation
		CurrentSequence.Name = "down";
		
		// Disable base collisions
		EnableAllCollisions = false;

		// Avoid making a calculation 2 times foreach ring
		var isUpDown = IsUpsideDown;

		var tr = Trace.Ray(Position + Rotation.Up * 110, Position + Rotation.Up * 1024)
			.WorldOnly()
			.Run();

		var hitGround = false;
		if (isUpDown && tr.Hit && tr.Distance < 999 && tr.Distance > 100)
			hitGround = true;

		for (int i = 0; i < 5; i++) {

			var endPos = hitGround ? tr.EndPos - (Rotation.Up * 110) + (Rotation.Up * 20) * ( i + 1 ) : Position + (Rotation.Up * 20) * (i + 1);
			var endPos2 = hitGround ? tr.EndPos - Rotation.Up * 50 : Position + (Rotation.Up * 50);
			EndPos = Transform.PointToLocal(endPos2);

			RingRing r = new();

			r.Parent = this;
			r.SetParent(this);
			r.isUpsideDown = isUpDown;
			r.Position = isUpDown ? Position + Rotation.Up * 15 : Position;
			r.Rotation = Rotation;
			r.Scale = Scale;
			r.Transmit = TransmitType.Always;
			
			r.desiredPos = Transform.PointToLocal( endPos ).z;

			ChildRings.Add(r);
		}

		int[] times = {2000, 500, 500, 500, isUpDown ? 100 : 200 };

		var reversed = ChildRings;
		reversed.Reverse();

		var y = 0;
		foreach (RingRing r in reversed) {

			await Task.Delay(times[y]);

			_ = r.MoveUp(0);

			y++;
		}

		if (withTeleport)
			DoTeleport();
	}

	public async virtual void DoTeleport() {

		List<Entity> toDest = new();
		List<Entity> fromDest = new();

		while (!HasAllRingsReachedPosition || (DestinationRings.IsValid() && !DestinationRings.HasAllRingsReachedPosition))
			await Task.Delay(10);

		var testPos = Transform.PointToWorld( EndPos );
		var toTp = Entity.All.Where(x => x.Position.Distance(testPos) <= 80);

		foreach (Entity p in toTp) {

			if (p.Tags.Has( "no_rings_teleport" ))
				continue;

			toDest.Add(p);

		}

		if (DestinationRings.IsValid()) {

			var testPos2 = DestinationRings.Transform.PointToWorld( DestinationRings.EndPos );
			var fromTp = Entity.All.Where(x => x.Position.Distance(testPos2) <= 80);

			foreach (Entity p in fromTp) {

				if (p.Tags.Has( "no_rings_teleport" ))
					continue;

				fromDest.Add(p);
			}
		}

		var particle = Particles.Create("particles/rings_transporter.vpcf", Transform.PointToWorld(EndPos));

		var	particle2 = Particles.Create("particles/rings_transporter.vpcf", DestinationRings.Transform.PointToWorld(DestinationRings.EndPos));

		foreach (Entity e in toDest) {
			var localPos = ChildRings[0].Transform.PointToLocal( e.Position );
			var newPos = DestinationRings.ChildRings[0].Transform.PointToWorld( localPos );

			e.Position = newPos;
		}

		foreach (Entity e in fromDest) {
			var localPos = DestinationRings.ChildRings[0].Transform.PointToLocal( e.Position );
			var newPos = ChildRings[0].Transform.PointToWorld( localPos );

			e.Position = newPos;
		}

		await Task.Delay(500);

		particle.Destroy();
		particle2.Destroy();

		RefractRings();
		DestinationRings.RefractRings();

		EndPos = Vector3.Zero;
	}

	public async virtual void RefractRings() {

		PlaySound("ring_transporter3");

		int[] times = {200, 200, 300, 500, 200 };

		var tRings = ChildRings;
		tRings.Reverse();

		var i = 0;
		foreach (RingRing r in tRings) {
			await Task.Delay(times[i]);

			r.desiredPos = LocalPosition.z;

			_ = r.Refract();

			i++;
		}

		ChildRings.Clear();
	}

}
