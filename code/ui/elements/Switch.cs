using Sandbox.UI.Construct;
using System;
using System.Linq;

namespace Sandbox.UI
{
	public class Switch : Checkbox
	{
		public Switch()
		{
			AddClass( "switch" );

			CheckMark?.Delete( true );

			CheckMark = Add.Panel( "checkmark" );
			CheckMark.Add.Panel( "handle" );
		}
	}
}
