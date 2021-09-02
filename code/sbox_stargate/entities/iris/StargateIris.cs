using System;
using Sandbox;

public partial class StargateIris : AnimEntity
{
	public Stargate Gate;
	public bool Closed = false;
	public bool Busy = false;

	private float OpenCloseDleay = 3f;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/gmod4phun/stargate/iris/iris.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;
		SetMaterialGroup(1);

		Transmit = TransmitType.Always;
	}

	public async void Close() {
		if ( Busy || Closed ) return;

		Busy = true;

		Closed = true;
		EnableAllCollisions = true;
		CurrentSequence.Name = "iris_close";
		Sound.FromEntity("iris_close", this);

		await Task.DelaySeconds( OpenCloseDleay );
		Busy = false;
	}

	public async void Open() {
		if ( Busy || !Closed ) return;

		Busy = true;

		Closed = false;
		EnableAllCollisions = false;
		CurrentSequence.Name = "iris_open";
		Sound.FromEntity("iris_open", this);

		await Task.DelaySeconds( OpenCloseDleay );
		Busy = false;
	}

	public void Toggle() {
		if ( Busy ) return;

		if (Closed) Open();
		else Close();
	}

	public void PlayHitSound() {
		Sound.FromEntity( "iris_hit", this );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (Gate.IsValid()) Gate.Iris = null;
	}

	[Event( "server.tick" )]
	public void IrisTick()
	{
		if ( Gate.IsValid() && Scale != Gate.Scale ) Scale = Gate.Scale; // always keep the same scale as gate
	}
}
