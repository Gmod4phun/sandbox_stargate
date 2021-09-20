using System.Text.Json;

public class StargateJsonModel : JsonModel {
	public string Name { get; set; }
	public string Address { get; set; }
	public bool Private { get; set; }
	public bool AutoClose { get; set; }
}

public partial class Stargate : IGatespawner {

	public virtual object ToJson() {
		return new StargateJsonModel {
			EntityName = ClassInfo.Name,
			Position = Position,
			Rotation = Rotation,
			Name = Name,
			Address = Address,
			Private = Private,
			AutoClose = AutoClose
		};

	}

	public virtual void FromJson(JsonElement data) {
		Position = Vector3.Parse(data.GetProperty("Position").ToString());
		Rotation = Rotation.Parse(data.GetProperty("Rotation").ToString());
		Name = data.GetProperty(nameof( StargateJsonModel.Name ) ).ToString();
		Address = data.GetProperty(nameof( StargateJsonModel.Address ) ).ToString();
		Private = data.GetProperty( nameof (StargateJsonModel.Private) ).GetBoolean();
		AutoClose = data.GetProperty( nameof (StargateJsonModel.AutoClose) ).GetBoolean();

	}

}
