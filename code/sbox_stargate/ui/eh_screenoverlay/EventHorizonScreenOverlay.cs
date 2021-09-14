using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class EventHorizonScreenOverlay : Panel
{
	private float StartTime = 0;

	public EventHorizonScreenOverlay()
	{
		StyleSheet.Load( "sbox_stargate/ui/eh_screenoverlay/EventHorizonScreenOverlay.scss" );
		StartTime = Time.Now;
	}

	public override void Tick()
	{
		if ( Time.Now > StartTime + 0.05f) SetClass( "hidden", true );
		if ( Time.Now > StartTime + 0.5f ) Delete( true );
		base.Tick();
	}
}
