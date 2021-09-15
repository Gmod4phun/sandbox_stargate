using Sandbox;
using Sandbox.UI;

[Library]
public partial class SandboxHud : HudEntity<RootPanel>
{

	public static SpawnMenu SpawnMenuInstance { get; private set; }

	public SandboxHud()
	{
		if ( !IsClient )
			return;

		RootPanel.StyleSheet.Load( "/ui/SandboxHud.scss" );

		RootPanel.AddChild<NameTags>();
		RootPanel.AddChild<CrosshairCanvas>();
		RootPanel.AddChild<ChatBox>();
		RootPanel.AddChild<VoiceList>();
		RootPanel.AddChild<KillFeed>();
		RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
		RootPanel.AddChild<Health>();
		RootPanel.AddChild<InventoryBar>();
		RootPanel.AddChild<CurrentTool>();
		SpawnMenuInstance = RootPanel.AddChild<SpawnMenu>();
	}

	[Event.Hotload]
	public async void ReloadSpawnMenu() {
		if ( !IsClient )
			return;

		SpawnMenuInstance?.Delete(true);
		await Task.Delay(250);
		SpawnMenuInstance = RootPanel.AddChild<SpawnMenu>();
	}
}
