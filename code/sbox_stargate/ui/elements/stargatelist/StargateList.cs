using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.UI.Tests;
using System.Collections.Generic;
using System.Linq;

[Library(Title = "Stargate Addon")]
public partial class StargateList : Panel, ILeftSpawnMenuTab
{
	public static StargateList Current { get; private set; }

	public VirtualScrollPanel Ramps;

	VirtualScrollPanel Canvas;

	private string[] categories = {
		"Stargate",
		"Rings",
		"Weapons",
		"Other"
	};

	public StargateList()
	{
		Current = this;
		AddClass( "spawnpage" );

		StyleSheet.Load( "sbox_stargate/ui/elements/stargatelist/stargatelist.scss" );

		Dictionary<string, VirtualScrollPanel> CategoriesCanvas = new();

		foreach (string cat in categories) {
			Add.Label(cat, "category");
			var can = AddChild<VirtualScrollPanel>("canvas");

			can.Layout.AutoColumns = true;
			can.Layout.ItemHeight = 120;
			can.Layout.ItemWidth = 120;
			can.OnCreateCell = ( cell, data ) =>
			{
				var entry = (LibraryAttribute)data;

				var btn = cell.Add.Button( entry.Title );
				btn.AddClass( "icon" );
				btn.AddEventListener( "onclick", () => ConsoleSystem.Run( "spawn_entity", entry.Name ) );
				btn.Style.BackgroundImage = Texture.Load( $"/entity/sbox_stargate/{entry.Name}.png", false );
			};

			CategoriesCanvas.Add(cat, can);
		}

		var ents = Library.GetAllAttributes<Entity>().Where( x => x.Spawnable && x.Group != null && x.Group.StartsWith("Stargate") ).OrderBy( x => x.Title ).ToArray();

		foreach ( var entry in ents )
		{
			var parse = entry.Group.Split("Stargate.");
			if (parse.Length > 1 && CategoriesCanvas[parse[1]] != null) {
				CategoriesCanvas[parse[1]].AddItem( entry );
			} else {
				CategoriesCanvas["Other"].AddItem( entry );
			}
		}

		Add.Label( "Ramps", "category" );
		Ramps = AddChild<VirtualScrollPanel>( "canvas" );
		Ramps.Layout.AutoColumns = true;
		Ramps.Layout.ItemHeight = 120;
		Ramps.Layout.ItemWidth = 120;
		Ramps.OnCreateCell = ( cell, data ) =>
		{
			var entry = (RampAsset)data;
			var btn = cell.Add.Button( entry.Title );
			btn.AddClass( "icon" );
			btn.AddEventListener( "onclick", () => ConsoleSystem.Run( "spawn_entity", "stargate_ramp", entry.Path ) );
			btn.Style.BackgroundImage = Texture.Load( $"/entity/sbox_stargate/{entry.Name}.png", false );
		};

		CategoriesCanvas.Add( "Ramps", Ramps );

		foreach ( RampAsset ramp in RampAsset.All )
		{
			CategoriesCanvas["Ramps"].AddItem( ramp );
		}
	}
}
