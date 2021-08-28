using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public partial class GuiController : NetworkComponent
{
	private const float _distanceAutoCloseMenu = 160;
	private static Panel _stargateMenuOpened;
	private static Stargate _stargate;
	private static Entity _ply;
	public static void OpenStargateMenu( Stargate stargate, Entity user )
	{
		if ( _stargateMenuOpened is null )
		{
			//_stargateMenuOpened = new StargateMenu(); // here calling gui
			_stargate = stargate;
			_ply = user;
		}
	}
	public static void CloseStargateMenu( Stargate stargate)
	{
		if ( _stargateMenuOpened is not null && stargate == _stargate )
		{
			//_stargateMenuOpened.Delete(); // here deleting gui
			_stargateMenuOpened = null;
			_ply = null;
		}
	}
	public static void RangeCheckTick()
	{
		if ( _ply is not null )
		{
			if ( _ply.IsValid() && _stargate.IsValid() )
			{
				float distance = Vector3.DistanceBetween( _ply.Position, _stargate.Position );
				if ( distance > _distanceAutoCloseMenu )
				{
					CloseStargateMenu( _stargate );
				}
			}
		}
	}
}

