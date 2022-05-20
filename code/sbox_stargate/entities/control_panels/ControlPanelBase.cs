using System;
using System.Collections.Generic;
using Sandbox;

public partial class ControlPanelBase : ModelEntity
{

	public Dictionary<string, ControlPanelButton> Buttons { get; protected set; } = new();

	protected virtual int AmountOfButtons { get; } = 6;

	protected virtual string ButtonsPath { get; }

	public override void Spawn()
	{
		base.Spawn();

		CreateButtons();
	}

	public virtual void CreateButtons() // visible models of buttons that turn on/off and animate
	{
		for ( var i = 1; i <= AmountOfButtons; i++ )
		{
			var button = new ControlPanelButton();
			button.SetModel( $"{ButtonsPath}{i}.vmdl" );
			button.SetupPhysicsFromModel( PhysicsMotionType.Static, true ); // needs to have physics for traces
			button.PhysicsBody.BodyType = PhysicsBodyType.Static;
			button.EnableAllCollisions = false; // no collissions needed
			button.EnableTraceAndQueries = true; // needed for Use

			button.Position = Position;
			button.Rotation = Rotation;
			button.Scale = Scale;
			button.SetParent( this );

			Buttons.Add( i.ToString(), button );
		}
	}

	public bool LinkButton( string btn, Action<Entity> callback )
	{
		var butt = Buttons[btn];
		if ( butt is null ) return false;
		if ( butt.OnUseCallback is not null ) return false;
		butt.OnUseCallback = callback;
		return true;
	}

}
