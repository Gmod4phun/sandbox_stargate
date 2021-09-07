using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class DhdButtonPanel : WorldPanel
{
	public Label label;

	public DhdButtonPanel(string action) // TEST, will probably use this for dhd button overlay, using DebugOverlay for now
	{
		PanelBounds = new Rect(0, 0, 10, 10);

		StyleSheet.Load( "sbox_stargate/ui/elements/titlebar/titlebar.scss" );

		label = Add.Label( action, "action" );
	}
}
