using System.Text.Json;

public class StargateJsonModel : JsonModel {
	public string Name { get; set; }
	public string Address { get; set; }
	public bool Private { get; set; }
	public bool AutoClose { get; set; }
}

public partial class Stargate : IGateSpawner {

	public virtual object ToJson() {
		return new StargateJsonModel {
			EntityName = ClassInfo.Name,
			Position = Position,
			Rotation = Rotation,
			Name = GateName,
			Address = GateAddress,
			Private = Private,
			AutoClose = AutoClose
		};

	}

	public virtual void FromJson(JsonElement data) {
		Position = Vector3.Parse(data.GetProperty("Position").ToString());
		Rotation = Rotation.Parse(data.GetProperty("Rotation").ToString());
		GateName = data.GetProperty(nameof( StargateJsonModel.Name ) ).ToString();
		GateAddress = data.GetProperty(nameof( StargateJsonModel.Address ) ).ToString();
		Private = data.GetProperty( nameof (StargateJsonModel.Private) ).GetBoolean();
		AutoClose = data.GetProperty( nameof (StargateJsonModel.AutoClose) ).GetBoolean();

	}

}
