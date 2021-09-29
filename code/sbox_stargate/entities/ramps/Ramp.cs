using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public interface ISpawnFunction
{
	public void SpawnFunction(Entity owner, TraceResult tr, string data);
}

[Library("stargate_ramp", Spawnable = true)]
public partial class Ramp : ModelEntity, ISpawnFunction
{
	[Net] public RampAsset RampAsset { get; private set; }

	public List<Stargate> Gates { get; private set; } = new();
	public int MaxGates => RampAsset.GateOffsets.Length;
	public bool HasFreeSlots => Gates.Count < MaxGates;
	public int NextFreeSlot => Gates.Count;

	public RampOffset[] GateOffsets => RampAsset.GateOffsets;
	public RampOffset[] RingOffsets => RampAsset.RingOffsets;
	public RampOffset[] DHDOffsets => RampAsset.DHDOffsets;

	public void SpawnFunction( Entity owner, TraceResult tr, string data )
	{
		if ( data == null )
			return;
		RampAsset = Resource.FromPath<RampAsset>( data );
		SetModel( RampAsset.Model );
		Position += RampAsset.SpawnOffset;
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		foreach ( var offset in GateOffsets )
			Log.Info( offset.Position );
	}

	public override void Spawn()
	{
		base.Spawn();
	}

	public void PositionObject( Entity ent )
	{
		if (ent is Stargate g )
		{
			if ( !HasFreeSlots )
				return;
			var Slot = GateOffsets[NextFreeSlot];
			var r = Slot.Rotation;
			g.Position = Transform.PointToWorld( Slot.Position );
			g.Rotation = Transform.RotationToWorld( Rotation.From( new Angles(r.x, r.y, r.z) ) );
			g.SetParent( this );
			g.Ramp = this;

			Gates.Add( g );
		}
	}
}
