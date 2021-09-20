using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Sandbox;

public partial class GateSpawner {

	public static void CreateGateSpawner() {
		var fileName = Global.MapName;

		if (!FileSystem.Data.DirectoryExists("gatespawners/"))
			FileSystem.Data.CreateDirectory("gatespawners/");

		var model = new GatespawnerModel();
		foreach (IGatespawner e in Entity.All.OfType<IGatespawner>()) {
			model.Entities.Add(e.ToJson());
		}
		FileSystem.Data.WriteAllText($"gatespawners/{fileName}.json", JsonSerializer.Serialize(model, new JsonSerializerOptions() { WriteIndented = true }));
	}

	public static void LoadGateSpawner() {
		var filepath = $"{Global.MapName}.json";

		bool isData = FileSystem.Data.FileExists($"gatespawners/{filepath}");
		bool isRoot = FileSystem.Mounted.FileExists($"sbox_stargate/gatespawner/maps/{filepath}");

		if (!isData && !isRoot)
			return;

		// Gatespawners in data folder will always have priority
		filepath = (isRoot ? "sbox_stargate/gatespawner/maps/" : "gatespawners/") + filepath;

		var file = isRoot ? FileSystem.Mounted.ReadAllText(filepath) : FileSystem.Data.ReadAllText(filepath);
		var data = JsonSerializer.Deserialize<GatespawnerModel>(file);

		foreach (JsonElement o in data.Entities) {
			string entityName = o.GetProperty("EntityName").ToString();
			Entity e = Library.Create<Entity>(entityName, false);
			if (e is null || !e.IsValid())
				continue;

			(e as IGatespawner).FromJson(o);
		}
	}

	[ServerCmd("gatespawner")]
	public static void GateSpawnerCmd(string action) {
		switch(action) {
			case "create":
				CreateGateSpawner();
				break;
			case "load":
				LoadGateSpawner();
				break;
		}
	}

}

public class GatespawnerModel {

	public string Version { get; set; } = "0.0.1";

	public List<object> Entities { get; set; } = new();

}
