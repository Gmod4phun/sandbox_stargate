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
	[Net] public List<Rings> TRings { get; private set; } = new();
	[Net] public List<Dhd> DHDs { get; private set; } = new();

	#region Offset arrays and status accessors
	public RampOffset[] GateOffsets => RampAsset.GateOffsets;
	public int MaxGates => RampAsset.GateOffsets.Length;
	public bool HasFreeGateSlots => Gates.Count < MaxGates;
	public int NextFreeGateSlot => Gates.Count;

	public RampOffset[] RingOffsets => RampAsset.RingOffsets;
	public int MaxRings => RampAsset.RingOffsets.Length;
	public bool HasFreeRingSlots => TRings.Count < MaxRings;
	public int NextFreeRingSlot => TRings.Count;

	public RampOffset[] DHDOffsets => RampAsset.DHDOffsets;

	public int MaxDHDs => RampAsset.DHDOffsets.Length;
	public bool HasFreeDHDSlots => DHDs.Count < MaxDHDs;
	public int NextFreeDHDSlot => DHDs.Count;
	#endregion

	public RampController Controller { get; private set; }

	public void SpawnFunction( Entity owner, TraceResult tr, string data )
	{
		if ( data == null )
			return;
		RampAsset = Resource.FromPath<RampAsset>( data );
		if ( RampAsset.HasController )
		{
			Controller = Library.Create<RampController>( RampAsset.ControllerEntityClass );
			Components.Add( Controller );
			Controller.Enabled = true;
		}
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
		/*if (ent is Stargate g )
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
		}*/
		RampOffset Slot;
		switch ( ent )
		{
			case Stargate g:
				if ( !HasFreeGateSlots )
					return;
				Slot = GateOffsets[NextFreeGateSlot];
				g.Ramp = this;
				Gates.Add( g );
				break;
			case Rings r:
				if ( !HasFreeRingSlots )
					return;
				Slot = RingOffsets[NextFreeRingSlot];
				r.Ramp = this;
				TRings.Add( r );
				break;
			case Dhd d:
				if ( !HasFreeDHDSlots )
					return;
				Slot = DHDOffsets[NextFreeRingSlot];
				d.Ramp = this;
				DHDs.Add( d );
				break;
			default:
				return;
		}

		var rot = Slot.Rotation;
		ent.Position = Transform.PointToWorld( Slot.Position );
		ent.Rotation = Transform.RotationToWorld( Rotation.From( new Angles( rot.x, rot.y, rot.z ) ) );
		ent.SetParent( this );

	}

	public static Ramp GetClosest( Vector3 position, float max = -1f )
	{
		var ramps = All.OfType<Ramp>().Where( x => x.HasFreeGateSlots );

		if ( !ramps.Any() ) return null;

		float dist = -1f;
		Ramp ramp = null;
		foreach ( Ramp r in ramps )
		{
			var currDistance = position.Distance( r.Position );
			if ( max != -1f && currDistance > max )
				continue;

			if ( dist == -1f || currDistance < dist )
			{
				dist = currDistance;
				ramp = r;
			}
		}

		return ramp;
	}
}
