using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class Chevron : AnimEntity
{
	public bool Glowing;
	public PointLightEntity Light;

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;

		SetModel( "models/gmod4phun/stargate/gate_sg1/chevron.vmdl" );

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
		Light.Brightness = 0.25f;
		Light.Range = 8f;
		Light.Enabled = false;
	}

	public void ChevronLock()
	{
		CurrentSequence.Name = "idle_locked";
	}

	public void ChevronUnlock()
	{
		CurrentSequence.Name = "idle";
	}

	public void ChevronLockUnlock()
	{
		CurrentSequence.Name = "lock_unlock_long";
	}

	[Event( "server.tick" )]
	public void ChevronThink( )
	{
		var group = Glowing ? 1 : 0;
		if ( GetMaterialGroup() != group ) SetMaterialGroup( group );

		if ( Light.IsValid() ) Light.Enabled = Glowing;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Light.IsValid() ) Light.Delete();
	}
}
