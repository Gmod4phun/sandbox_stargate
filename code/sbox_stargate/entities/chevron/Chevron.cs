using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class Chevron : AnimEntity
{
	public bool On;
	public PointLightEntity Light;

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;

		SetModel( "models/sbox_stargate/gate_sg1/chevron.vmdl" );

		CreateLight();
	}

	public void CreateLight()
	{
		var att = (Transform) GetAttachment( "light" );

		Light = new PointLightEntity();
		Light.Position = att.Position;
		Light.Rotation = att.Rotation;
		Light.SetParent( this, "light" );

		Light.SetLightColor( Color.Parse( "#FF6A00" ).GetValueOrDefault() );
		Light.Brightness = 0.6f;
		Light.Range = 12f;
		Light.Enabled = On;
	}

	public async void ChevronAnim(string name, float delay = 0)
	{
		if ( delay > 0 )
		{
			await Task.DelaySeconds( delay );
			if ( !this.IsValid() ) return;
		}

		CurrentSequence.Name = name;
	}

	public async void TurnOn(float delay = 0)
	{
		if ( delay > 0 )
		{
			await Task.DelaySeconds( delay );
			if ( !this.IsValid() ) return;
		}
		
		On = true;
	}

	public async void TurnOff(float delay = 0)
	{
		if ( delay > 0 )
		{
			await Task.DelaySeconds( delay );
			if ( !this.IsValid() ) return;
		}

		On = false;
	}


	[Event( "server.tick" )]
	public void ChevronThink( )
	{
		var group = On ? 1 : 0;
		if ( GetMaterialGroup() != group ) SetMaterialGroup( group );

		if ( Light.IsValid() ) Light.Enabled = On;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Light.IsValid() ) Light.Delete();
	}
}
