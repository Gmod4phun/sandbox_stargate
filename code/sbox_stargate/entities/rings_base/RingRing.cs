using System;
using System.Threading.Tasks;
using Sandbox;

public partial class RingRing : Prop {

	public Rings RingParent;

	public bool isUpsideDown = false;

	private bool reachedPos = false;
	public bool Ready {
		get {
			return reachedPos;
		}
	}
	public Vector3 desiredPos;

	public bool Retract = false;

	public override void Spawn() {
		base.Spawn();
		Tags.Add( "no_rings_teleport" );

		EnableHitboxes = false;
		PhysicsEnabled = false;
		RenderColor = RenderColor.WithAlpha(0);

		Transmit = TransmitType.Always;
		SetModel( "models/gmod4phun/stargate/rings_ancient/ring_ancient.vmdl" );
	}

	public override void MoveFinished() {
		reachedPos = true;

		if (Retract) {
			RingParent.OnRingReturn();
			Delete();
		}
	}

	public override void MoveBlocked( Entity ent ) {
		var dmg = new DamageInfo();
		dmg.Attacker = RingParent;
		dmg.Damage = 200;
		ent.TakeDamage( dmg );
	}

	public void MoveUp() {
		RenderColor = RenderColor.WithAlpha(1);
		Retract = false;
		Move();
	}

	public void Move() {
		MoveTo(Retract ? RingParent.Position : RingParent.Transform.PointToWorld(desiredPos), 0.2f);
	}

	public void Refract() {
		Retract = true;
		Move();
	}

}
