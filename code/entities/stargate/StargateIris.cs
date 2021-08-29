using System;
using Sandbox;

public partial class StargateIris : AnimEntity
{

	public Stargate Gate;
	public bool Closed = false;

	public readonly string[] HitSounds = {
		"hit_1",
		"hit_2",
		"hit_3",
		"hit_4"
	};

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
		Closed = true;
		EnableAllCollisions = true;
		Sequence = "iris_close";
		//Sound.FromEntity("iris_close", this);
	}

	public void Open() {
		Closed = false;
		EnableAllCollisions = false;
		Sequence = "iris_open";
		//Sound.FromEntity("iris_open", this);
	}

	public void Toggle() {
		if (Closed)
			Open();
		else
			Close();
	}

	public void MakeHitSound() {
		//string sound = RandomExtension.FromArray<string>(new Random(), HitSounds);
		//Sound.FromEntity(sound, this);
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
