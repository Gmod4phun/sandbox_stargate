using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

[Library( "ent_stargate_movie", Title = "Stargate (Movie)", Spawnable = true, Group = "Stargate.Stargate" )]
public partial class StargateMovie : StargateMilkyWay
{

	public StargateMovie()
	{
		SoundDict = new()
		{
			{ "gate_open", "gate_movie_open" },
			{ "gate_close", "gate_movie_close" },
			{ "chevron_open", "chevron_movie_open" },
			{ "chevron_close", "chevron_movie_close" },
			{ "dial_fail", "dial_fail_sg1" },
			{ "dial_fail_noclose", "gate_sg1_dial_fail_noclose" }
		};

		MovieDialingType = true;
		ChevronLightup = false;
	}

	// SPAWN

	public override void Spawn()
	{
		base.Spawn();
		SetBodyGroup( 0, 1 );

		Ring.StartSoundName = "gate_movie_ring_roll";
		Ring.StopSoundName = "";
		Ring.StopSoundOnSpinDown = true;

		Ring.SetBodyGroup( 0, 1 );
		Ring.EarthPointOfOrigin = true;
	}

	// CHEVRONS

	public override Chevron CreateChevron( int n )
	{
		var chev = base.CreateChevron(n);
		chev.UsesDynamicLight = ChevronLightup;

		chev.ChevronStateSkins = new()
		{
			{ "Off", 2 },
			{ "On", ChevronLightup ? 1 : 2 },
		};

		if ( n == 7 ) chev.SetBodyGroup( 0, 1 );

		return chev;
	}

}
