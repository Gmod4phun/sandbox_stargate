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
		public string RampAssetPath { get; set; }
	}
	public void FromJson( JsonElement data )
	{
		Position = Vector3.Parse( data.GetProperty( "Position" ).ToString() );
		Rotation = Rotation.Parse( data.GetProperty( "Rotation" ).ToString() );
		RampAsset = Resource.FromPath<RampAsset>( data.GetProperty( "RampAssetPath" ).ToString() );

		SetModel( RampAsset.Model );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		//PhysicsBody.MotionEnabled = false;
		if ( RampAsset.HasController )
		{
			Controller = Components.Add(Library.Create<RampController>( RampAsset.ControllerEntityClass ));
			Controller.Enabled = true;
		}
	}

	public object ToJson()
	{
		return new RampJsonModel()
		{
			RampAssetPath = RampAsset.Path,
			EntityName = ClassInfo.Name,
			Position = Position,
			Rotation = Rotation
		};
	}
}
