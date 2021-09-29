using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using System.ComponentModel;

[Library]
public class RampOffset
{
	public bool ForceKeep { get; set; } = true;
	public Vector3 Position { get; set; } = new();
	public Vector3 Rotation { get; set; } = new();
}

[Library("sg_ramp")]
public partial class RampAsset : Asset
{
	public static IReadOnlyList<RampAsset> All => _all;
	internal static List<RampAsset> _all = new();

	[Property]
	public string Title { get; set; }

	[Property, ResourceType( "vmdl" )]
	public string Model { get; set; }

	[Property]
	public Vector3 SpawnOffset { get; set; }
	[Property]
	public RampOffset[] GateOffsets { get; set; }
	[Property(Title = "DHD Offsets")]
	public RampOffset[] DHDOffsets { get; set; }
	[Property]
	public RampOffset[] RingOffsets { get; set; }
	[Property]
	public string ControllerEntityClass { get; set; }


	protected override void PostLoad()
	{
		base.PostLoad();

		if ( !_all.Contains( this ) )
			_all.Add( this );
		if ( StargateList.Current == null )
			return;
		StargateList.Current.Ramps.AddItem( this );
	}

}
