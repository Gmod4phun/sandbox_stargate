using System;
using System.Collections.Generic;
using Sandbox;

public abstract partial class Dhd : Prop
{
	[Net]
	[Property(Name = "Gate", Group = "Stargate")]
	public Stargate Gate { get; protected set; }

	protected readonly string ButtonSymbols = "ABCDEFGHI0123456789STUVWXYZ@JKLMNO#PQR";

	public Dictionary<string, DhdButtonTrigger> ButtonTriggers { get; protected set; } = new();
	public Dictionary<string, DhdButton> Buttons { get; protected set; } = new();

	public float lastPressTime = 0;
	public float pressDelay = 0.5f;

	public List<string> PressedActions = new();

	public override void Spawn()
	{
		base.Spawn();
	}

	public void CreateSingleButtonTrigger(string model, string action) // invisible triggers used for handling the user interaction
	{
		var buttonTrigger = new DhdButtonTrigger();
		buttonTrigger.SetModel( model );

		buttonTrigger.SetupPhysicsFromModel( PhysicsMotionType.Static, true ); // needs to have physics for traces
		buttonTrigger.PhysicsBody.BodyType = PhysicsBodyType.Static;
		buttonTrigger.EnableAllCollisions = false; // no collissions needed
		buttonTrigger.EnableTraceAndQueries = true; // needed for Use
		buttonTrigger.EnableDrawing = false; // it should have an invisible material, but lets be safe and dont render it anyway

		buttonTrigger.Position = Position;
		buttonTrigger.Rotation = Rotation;
		buttonTrigger.Scale = Scale;
		buttonTrigger.SetParent( this );

		buttonTrigger.DHD = this;
		buttonTrigger.Action = action;
		ButtonTriggers.Add( action, buttonTrigger );
	}

	public virtual void CreateButtonTriggers()
	{
		// SYMBOL BUTTONS
		for ( var i = 0; i < ButtonSymbols.Length; i++ )
		{
			var modelName = $"models/gmod4phun/stargate/dhd/trigger_buttons/dhd_trigger_button_{i + 1}.vmdl";
			var actionName = ButtonSymbols[i].ToString();
			CreateSingleButtonTrigger( modelName, actionName );
		}

		// CENTER DIAL BUTTON
		CreateSingleButtonTrigger( "models/gmod4phun/stargate/dhd/trigger_buttons/dhd_trigger_button_39.vmdl", "DIAL" );
	}

	public virtual void CreateSingleButton(string model, string action, DhdButtonTrigger buttonTrigger, int bodygroup, int subgroup) // visible model of buttons that turn on/off and animate
	{
		var button = new DhdButton();
		button.SetModel( model );
		button.SetBodyGroup( bodygroup, subgroup );

		button.EnableAllCollisions = false;
		button.EnableTraceAndQueries = false;

		button.Position = Position;
		button.Rotation = Rotation;
		button.Scale = Scale;
		button.SetParent( this );

		button.Action = action;
		button.Trigger = buttonTrigger;
		buttonTrigger.Button = button;

		Buttons.Add( action, button );
	}

	public virtual void CreateButtons() // visible models of buttons that turn on/off and animate
	{
		var i = 0;
		foreach ( var trigger in ButtonTriggers )
		{
			// uses a single model that has all buttons as bodygroups, that way animations/matgroups for all buttons can be edited at once
			CreateSingleButton( "models/gmod4phun/stargate/dhd/dhd_buttons.vmdl", trigger.Key, trigger.Value, 1, i++);
		}
	}

	public DhdButton GetButtonByAction(string action)
	{
		return Buttons.GetValueOrDefault( action );
	}

	public void PlayButtonPressAnim(DhdButton button)
	{
		if ( button.IsValid() ) button.CurrentSequence.Name = "button_press";
	}

	public string GetPressedActions()
	{
		var retVal = "";
		foreach (string action in PressedActions)
		{
			retVal += action;
		}
		return retVal;
	}

	public void TriggerAction(string action) // this gets called from the Button Trigger after pressing it
	{
		if ( action == "DIAL" && PressedActions.Count < 7 ) return; // cant press dial unless we have atleast 7 symbols

		if ( action != "DIAL" ) // if we pressed a regular symbol
		{
			if ( PressedActions.Contains( "DIAL" ) ) return; // do nothing if we already have dial pressed

			if ( !PressedActions.Contains(action) && PressedActions.Count == 9 ) return; // do nothing if we already have max symbols pressed
		}

		var button = GetButtonByAction( action );
		PlayButtonPressAnim( button );

		if (PressedActions.Contains(action))
		{
			PressedActions.Remove( action );
			SetButtonState( button, false );
		}
		else
		{
			PressedActions.Add( action );
			SetButtonState( button, true );

			if ( action == "DIAL" )
			{
				var allActions = GetPressedActions();
				var sequence = allActions.Substring( 0, allActions.Length - 4 );
				Log.Info( $"Address for dial = {sequence}" );

				var gate = Stargate.FindNearestGate( this );
				if (gate.IsValid())
				{
					if (gate.CanStargateStartDial())
					{
						gate.BeginDialFast( sequence );
					}
				}

			}
		}

	}

	public void SetButtonState( string action, bool glowing )
	{
		var b = GetButtonByAction( action );
		if ( b.IsValid() ) b.On = glowing;
	}

	public void SetButtonState( DhdButton b, bool glowing )
	{
		if ( b.IsValid() ) b.On = glowing;
	}

	public void ToggleButton( string action )
	{
		var b = GetButtonByAction( action );
		if ( b.IsValid() ) SetButtonState( b, !b.On);
	}
	public void ToggleButton( DhdButton b )
	{
		if ( b.IsValid() ) SetButtonState( b, !b.On );
	}

	public void EnableAllButtons()
	{
		foreach ( DhdButton b in Buttons.Values ) SetButtonState( b, true );
	}

	public void DisableAllButtons()
	{
		foreach ( DhdButton b in Buttons.Values ) SetButtonState( b, false );
	}

}
