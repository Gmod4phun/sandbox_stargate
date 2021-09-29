using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Sandbox;

public partial class Ramp : IGateSpawner
{
	public class RampJsonModel : JsonModel
	{
		public int RampAssetID { get; set; }
	}
	public void FromJson( JsonElement data )
	{
		Position = Vector3.Parse( data.GetProperty( "Position" ).ToString() );
		Rotation = Rotation.Parse( data.GetProperty( "Rotation" ).ToString() );
		RampAsset = Resource.FromId<RampAsset>( data.GetProperty( "RampAssetID" ).GetInt32() );
	}

	public object ToJson()
	{
		return new RampJsonModel()
		{
			RampAssetID = RampAsset.ResourceId,
			Position = Position,
			Rotation = Rotation
		};
	}
}
