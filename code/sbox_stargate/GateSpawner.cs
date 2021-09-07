using System.Collections.Generic;
using System.Linq;
using Sandbox;

public partial class GateSpawner {

	private readonly Dictionary<string, string> basics = new() {
		{"facepunch.flatgrass", "stargate;0;[4861.788,3152.11,90];[-0,-132.26733,0];FARWY1#;Far Away\nstargate;0;[366.3628,367.0638,222];[-0,-131.34433,0];SPAWN1#;Spawn\ndhd;0;[4912.369,2977.028,-5];[15.000012,-106.01099,0]\ndhd;0;[423.5872,200.8627,126.4018];[15.000012,-105.729454,0]"},
		{"facepunch.construct", "stargate;0;[-1505.784,3020.586,90.0003];[-0,-90.80003,0];NOWAY1#;No Way\nstargate;0;[1902.504,1959.156,1.977];[-0,68.954735,0];GARDEN#;Garden\nstargate;0;[1016.254,-3922.026,-50.0003];[-0,92.395454,0];WATER1#;Waterway\nstargate;0;[-3680.159,94.6243,146];[-0,-137.60132,0];HANGR1#;Hangar\ndhd;0;[-1317.12,2906.848,-0.9997];[15.000012,-131.30551,0]\ndhd;0;[1750.458,2090.236,-90.1592];[15.000012,95.72829,-8.838919E-07]\ndhd;0;[872.4154,-3881.677,-145.0003];[15.000012,109.323494,0]\ndhd;0;[-3593.798,-78.6327,51];[15.0000105,-102.06455,0]"}
	};

	public GateSpawner() {

		if (!FileSystem.Data.DirectoryExists("gatespawners/"))
			FileSystem.Data.CreateDirectory("gatespawners/");

		foreach (var e in basics) {
			if (!FileSystem.Data.FileExists($"gatespawners/{e.Key}.txt"))
				FileSystem.Data.WriteAllText($"gatespawners/{e.Key}.txt", e.Value);
		}
	}

	private string FromAngles(Angles a) {
		return $"{a.pitch},{a.yaw},{a.roll}";
	}

	public void CreateGateSpawner() {
		var fileName = Global.MapName;

		var gates = Entity.All.OfType<Stargate>();
		var dhds = Entity.All.OfType<Dhd>();

		string fileContent = "";

		foreach (Stargate gate in gates) {
			var type = gate is StargateMilkyWay ? 0 : 1;
			fileContent += $"stargate;{type};[{gate.Position}];[{FromAngles(gate.Rotation.Angles())}];{gate.Address};{gate.Name}\n";
		}

		foreach (Dhd dhd in dhds) {
			var type = dhd is DhdMilkyWay ? 0 : 1;
			fileContent += $"dhd;{type};[{dhd.Position}];[{FromAngles(dhd.Rotation.Angles())}]\n";
		}

		if (!FileSystem.Data.DirectoryExists("gatespawners/"))
			FileSystem.Data.CreateDirectory("gatespawners/");
		FileSystem.Data.WriteAllText($"gatespawners/{fileName}.txt", fileContent);
	}

	public void LoadGateSpawner() {
		var fileName = $"{Global.MapName}.txt";

		if (!FileSystem.Data.FileExists($"gatespawners/{fileName}"))
			return;

		string[] lines = FileSystem.Data.ReadAllText($"gatespawners/{fileName}").Split('\n');

		foreach(string l in lines) {
			SpawnEntity(l);
		}
	}

	private void SpawnEntity(string line) {
		string[] ent = line.Split(';');

		switch(ent[0]) {

			case "stargate":

				Stargate gate;
				if (ent[1] == "0")
					gate = new StargateMilkyWay();
				else
					gate = new StargateMilkyWay();

				var pos = ent[2].Substring(1);
				pos = pos.Remove(pos.Length - 1);
				var sPos = pos.Split(',');

				gate.Position = new Vector3(float.Parse(sPos[0]), float.Parse(sPos[1]), float.Parse(sPos[2]));

				var rot = ent[3].Substring(1);
				rot = rot.Remove(rot.Length - 1);
				var sRot = rot.Split(',');

				gate.Rotation = new Angles(float.Parse(sRot[0]), float.Parse(sRot[1]), float.Parse(sRot[2])).ToRotation();

				gate.Address = ent[4];
				gate.Name = ent[5];
				
				break;

			case "dhd": {
				Dhd dhd;
				if (ent[1] == "0")
					dhd = new DhdMilkyWay();
				else
					dhd = new DhdMilkyWay();

				var posD = ent[2].Substring(1);
				posD = posD.Remove(posD.Length - 1);
				var sPosD = posD.Split(',');

				dhd.Position = new Vector3(float.Parse(sPosD[0]), float.Parse(sPosD[1]), float.Parse(sPosD[2]));

				var rotD = ent[3].Substring(1);
				rotD = rotD.Remove(rotD.Length - 1);
				var sRotD = rotD.Split(',');

				dhd.Rotation = new Angles(float.Parse(sRotD[0]), float.Parse(sRotD[1]), float.Parse(sRotD[2])).ToRotation();

				break;
			}

		}
	}

	[ServerCmd("gatespawner")]
	public static void GateSpawnerCmd(string action) {
		switch(action) {
			case "create":
				(Game.Current as SandboxGame).gateSpawner.CreateGateSpawner();
				break;
			case "load":
				(Game.Current as SandboxGame).gateSpawner.LoadGateSpawner();
				break;
			case "test":
				foreach(Stargate s in Entity.All.OfType<Stargate>()) {
					Log.Info(s.Rotation);
				}
				break;
		}
	}

}
