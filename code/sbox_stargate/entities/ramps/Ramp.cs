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

	[Net] public List<Stargate> Gates { get; private set; } = new();
	[Net] public List<Rings> Rings { get; private set; } = new();
	[Net] public List<Dhd> DHDs { get; private set; } = new();

	public RampOffset[] GateOffsets => RampAsset.GateOffsets;
	public int MaxGates => RampAsset.GateOffsets.Length;
	public bool HasFreeGateSlots => Gates.Count < MaxGates;
	public int NextFreeGateSlot => Gates.Count;

	public RampOffset[] RingOffsets => RampAsset.RingOffsets;
	public int MaxRings => RampAsset.RingOffsets.Length;
	public bool HasFreeRingSlots => Rings.Count < MaxRings;
	public int NextFreeRingSlot => Rings.Count;

	public RampOffset[] DHDOffsets => RampAsset.DHDOffsets;

	public int MaxDHDs => RampAsset.DHDOffsets.Length;
	public bool HasFreeDHDSlots => DHDs.Count < MaxDHDs;
	public int NextFreeDHDSlot => DHDs.Count;


	public void SpawnFunction( Entity owner, TraceResult tr, string data )
	{
		if ( data == null )
			return;
		RampAsset = Resource.FromPath<RampAsset>( data );
		SetModel( RampAsset.Model );
		Position += RampAsset.SpawnOffset;
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
	}

	public override void Spawn()
	{
		base.Spawn();
	}

	public void PositionObject( Entity ent )
	{
		if (ent is Stargate g )
		{
			if ( !HasFreeGateSlots )
				return;
			var Slot = GateOffsets[NextFreeGateSlot];
			var r = Slot.Rotation;
			g.Position = Transform.PointToWorld( Slot.Position );
			g.Rotation = Transform.RotationToWorld( Rotation.From( new Angles(r.x, r.y, r.z) ) );
			g.SetParent( this );
			g.Ramp = this;

			Gates.Add( g );
		}
	}
}
