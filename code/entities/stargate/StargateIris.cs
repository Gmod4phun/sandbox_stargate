using System;
using Sandbox;

public partial class StargateIris : AnimEntity
{

	public Stargate Gate;
	public bool Closed = false;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/gmod4phun/stargate/iris/iris.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;
		SetMaterialGroup(1);

		Transmit = TransmitType.Always;
	}

	public void Close() {
		if ( Closed ) return;

		Closed = true;
		EnableAllCollisions = true;
		CurrentSequence.Name = "iris_close";
		Sound.FromEntity("iris_close", this);
	}

	public void Open() {
		if ( !Closed ) return;

		Closed = false;
		EnableAllCollisions = false;
		CurrentSequence.Name = "iris_open";
		Sound.FromEntity("iris_open", this);
	}

	public void Toggle() {
		if (Closed)
			Open();
		else
			Close();
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
