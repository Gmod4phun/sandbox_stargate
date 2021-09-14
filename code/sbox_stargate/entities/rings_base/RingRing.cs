using System;
using System.Threading.Tasks;
using Sandbox;

public partial class RingRing : Prop {

	public Rings RingPlatform;

	public bool isUpsideDown = false;

	private bool reachedPos = false;
	public bool Ready {
		get {
			return reachedPos;
		}
	}
	public float desiredPos;

	public override void Spawn() {
		base.Spawn();
		Tags.Add( "no_rings_teleport" );

		EnableHitboxes = false;
		PhysicsEnabled = false;
		RenderColor = RenderColor.WithAlpha(0);

		Transmit = TransmitType.Always;
		SetModel( "models/gmod4phun/stargate/rings_ancient/ring_ancient.vmdl" );
	}

	public async Task<bool> MoveUp(int num) {
		RenderColor = RenderColor.WithAlpha( 1 );
		reachedPos = false;
		while (!reachedPos) {

			if (!this.IsValid())
				break;

			var speed = (desiredPos - LocalPosition.z) / 10;
			LocalPosition += new Vector3(0, 0, speed);
			
			if (Math.Abs(LocalPosition.z - desiredPos) < 5) {
				reachedPos = true;
				break;
			}

			await Task.Delay(1);
		}

		return true;
	}

	public async Task<bool> Refract() {
		reachedPos = false;
		while (!reachedPos) {

			if (!this.IsValid())
				break;

			var speed = (Math.Abs((desiredPos - LocalPosition.z)) / 10) + 0.1f;
			LocalPosition -= new Vector3(0, 0, speed);

			if (LocalPosition.z <= 4) {
				reachedPos = true;
				break;
			}

			await Task.Delay(1);
		}

		RingPlatform.OnRingReturn();

		Delete();

		return true;
	}

}
