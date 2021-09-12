using System;
using System.Threading.Tasks;
using Sandbox;

public partial class RingRing : Prop {

	public Rings Parent;

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
		RenderAlpha = 0;

		Transmit = TransmitType.Always;
		SetModel( "models/gmod4phun/stargate/rings_ancient/ring_ancient.vmdl" );
	}

	public override void MoveFinished() {
		reachedPos = true;

		if (Retract) {
			Parent.OnRingReturn();
			Delete();
		}
	}

	public override void MoveBlocked( Entity ent ) {
		var dmg = new DamageInfo();
		dmg.Attacker = Parent;
		dmg.Damage = 200;
		ent.TakeDamage( dmg );
	}

	public async void MoveUp() {
		RenderAlpha = 1;
		Retract = false;
		Move();
	}

	public void Move() {
		MoveTo(Retract ? Parent.Position : Parent.Transform.PointToWorld(desiredPos), 0.2f);
	}

	public void Refract() {
		Retract = true;
		Move();
	}

}
