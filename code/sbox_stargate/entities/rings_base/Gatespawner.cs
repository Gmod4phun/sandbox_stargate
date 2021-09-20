using System.Text.Json;

public class RingsBaseJsonModel : JsonModel {

	public string Address { get; set; }

}

public partial class Rings : IGatespawner
{
	public virtual void FromJson( JsonElement data )
	{
		Position = Vector3.Parse(data.GetProperty("Position").ToString());
		Rotation = Rotation.Parse(data.GetProperty("Rotation").ToString());
		Address = data.GetProperty(nameof( RingsBaseJsonModel.Address ) ).ToString();
	}

	public virtual object ToJson()
	{
		return new RingsBaseJsonModel
		{
			EntityName = ClassInfo.Name,
			Position = Position,
			Rotation = Rotation,
			Address = Address
		};
	}
}
