using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class EventHorizonScreenPanel : Panel
{
	private float EHOpacity = 1f;

	public EventHorizonScreenPanel()
	{
		StyleSheet.Load( "sbox_stargate/ui/eh_screenpanel/EventHorizonScreenPanel.scss" );

		Style.Opacity = EHOpacity;
	}

	public override void Tick()
	{
		EHOpacity = EHOpacity.Approach( 0f, Time.Delta * 0.5f );

		//Style.Opacity = EHOpacity;

		if ( EHOpacity < 0.05f )
		{
			Log.Info( "buh bye" );
			Delete( true );
		}
	}

	/*

	namespace SCPRP.UI
	{
		public class BlinkUI : Panel
		{
			public BlinkUI()
			{
				StyleSheet.Load( "/ui/BlinkUI.scss" );
			}

			public override void Tick()
			{
				if ( Local.Pawn is not PlayerCharacter character ) return;
				SetClass( "hidden", !character.IsBlinking );
				base.Tick();
			}
		}
	}

	blinkui {
		position: absolute;
		background-color: black;
		height: 100%;
		width: 100%;
		background-position: center;
		background-repeat: no-repeat;
		background-size: cover;
		z-index: 1;
		transition: all 0.1s ease-in;

		&.hidden {
			//display: none;
			background-color: rgba(#000, 0);
		}
	}

	*/
}
