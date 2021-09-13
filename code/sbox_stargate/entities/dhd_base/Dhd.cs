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

		PostSpawn();
	}

	public virtual async void PostSpawn()
	{
		await GameTask.NextPhysicsFrame();
		if ( !this.IsValid() ) return;

		Gate = Stargate.FindNearestGate( this );
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

	// TOUCH

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if (other is Stargate gate)
		{
			Gate = gate;
			PlaySound( "balloon_pop_cute" );
		}
	}

	// BUTTON PRESS LOGIC
	public string GetPressedActions()
	{
		var retVal = "";
		foreach ( string action in PressedActions )
		{
			retVal += action;
		}
		return retVal;
	}

	public void EnableButtonsForDialingAddress()
	{
		if ( !Gate.IsValid() )
		{
			DisableAllButtons(); return;
		}

		DisableAllButtons();
		foreach (char sym in Gate.DialingAddress) SetButtonState( sym.ToString() , true);

		if ( Gate.Open || Gate.Opening || Gate.Closing )
		{
			var dial = GetButtonByAction( "DIAL" );
			if ( dial.IsValid() ) SetButtonState( dial, true );
		}
	}

	public void TriggerAction( string action ) // this gets called from the Button Trigger after pressing it
	{
		if ( !Gate.IsValid() || Gate.Busy || Gate.Inbound ) return; // if we have no gate to control or we are busy, we cant do anything

		if ( Gate.Dialing && Gate.CurDialType is not Stargate.DialType.DHD ) return; // if we are dialing, but not by DHD, cant do anything

		if ( action is not "DIAL" ) // if we pressed a regular symbol
		{
			if ( PressedActions.Contains( "DIAL" ) ) return; // do nothing if we already have dial pressed
			if ( !PressedActions.Contains( action ) && PressedActions.Count is 9 ) return; // do nothing if we already have max symbols pressed
			if ( !PressedActions.Contains( action ) && action is "#" )
			{
				if ( PressedActions.Count < 6 ) return;
			}
			if ( Gate.Opening || Gate.Open || Gate.Closing ) return;
		}

		var button = GetButtonByAction( action );

		if ( action is "DIAL" ) // we presed dial button
		{
			if ( Gate.Idle ) // if gate is idle, open dial menu
			{
				//Gate.OpenStargateMenu(); // TODO - make the menu stay open
				return;
			}

			if (Gate.Open) // if gate is open, close the gate
			{
				if (Gate.CanStargateClose())
				{
					Gate.DoStargateClose( true );
					PressedActions.Clear();
				}
				return;
			}

			if (PressedActions.Count < 7) // if we pressed less than 7 symbols, we should cancel dial
			{
				if (Gate.Dialing && Gate.CurDialType is Stargate.DialType.DHD)
				{
					PlayButtonPressAnim( button );

					Gate.StopDialing();
					PressedActions.Clear();
				}

				return;
			}
			else // try dial
			{
				var sequence = GetPressedActions();
				Log.Info( $"Address for dial = {sequence}" );

				PlayButtonPressAnim( button );

				var target = Stargate.FindByAddress( sequence );
				if ( target.IsValid() && target != Gate && target.IsStargateReadyForInboundDHD() && Gate.CanStargateOpen() )
				{
					PlaySound( "dhd_dial" );

					Gate.CurGateState = Stargate.GateState.IDLE; // temporarily make it idle so it can 'begin' dialing
					Gate.BeginOpenByDHD(sequence);
				}
				else
				{
					Gate.StopDialing();
					PressedActions.Clear();
					return;
				}
			}

		}
		else // we pressed a symbol
		{
			var symbol = action[0];

			if ( symbol != '#' && PressedActions.Contains( "#" ) ) return; // if # is pressed, and we try to depress other symbol, do nothing

			if ( PressedActions.Contains( action ) ) // if symbol was pressed before already, deactivate it
			{
				Gate.DoChevronUnlock( symbol );

				if ( PressedActions.Count == 1 ) // if we are deactivating last symbol, stop dialing and go back to idle
				{
					Gate.ResetGateVariablesToIdle();
				}

				PressedActions.Remove( action );
				PlayButtonPressAnim( button );
			}
			else // otherwise activate it
			{
				if ( !Gate.Dialing ) // if gate wasnt dialing, begin dialing
				{
					Gate.CurGateState = Stargate.GateState.DIALING;
					Gate.CurDialType = Stargate.DialType.DHD;
				}

				if ( PressedActions.Count == 8 || symbol is '#' ) // lock if we are putting Point of Origin or 9th symbol, otherwise encode
				{
					Gate.DoChevronLock( symbol );
				}
				else
				{
					Gate.DoChevronEncode( symbol );
				}
					

				PressedActions.Add( action );
				PlayButtonPressAnim( button );
				PlaySound("dhd_sg1_press");
			}
		}


	}

	[Event.Tick.Server]
	public void ButtonThink()
	{
		EnableButtonsForDialingAddress();

		if ( ((!Gate.IsValid()) || (Gate.IsValid() && Gate.Idle)) && PressedActions.Count != 0 )
		{
			PressedActions.Clear();
		} 
	}

}
