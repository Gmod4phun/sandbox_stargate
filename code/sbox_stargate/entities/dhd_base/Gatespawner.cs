using System.Text.Json;

public partial class Dhd : IGatespawner
{
	public virtual void FromJson( JsonElement data )
	{
		Position = Vector3.Parse(data.GetProperty("Position").ToString());
		Rotation = Rotation.Parse(data.GetProperty("Rotation").ToString());
	}

	public virtual object ToJson()
	{
		return new JsonModel {
			EntityName = ClassInfo.Name,
			Position = Position,
			Rotation = Rotation
		};
	}
}
