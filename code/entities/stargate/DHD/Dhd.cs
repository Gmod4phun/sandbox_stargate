using System;
using System.Collections.Generic;
using Sandbox;

public abstract partial class DHD : Prop
{
	[Net]
	[Property(Name = "Gate", Group = "Stargate")]
	public Stargate Gate { get; protected set; }

	protected readonly string ButtonsModel = "models/gmod4phun/stargate/dhd/dhd_buttons.vmdl";
	protected readonly string ButtonsSymbols = "0123456789ABCDEFGHIJKLMNO#PQRSTUVWXYZ@.";

	protected Dictionary<char, DHDButton> Buttons = new();

	public override void Spawn()
	{
		base.Spawn();

		Stargate linked = null;
		List<Stargate> tested = new();
		while (linked == null) {
			Stargate tried = Stargate.FindClosestGate(Position, 5000f, tested.ToArray());
			if (tried == null)
				break;

			if (tried.Dhd == null || !tried.Dhd.IsValid())
				linked = tried;
			else
				tested.Add(tried);
		}

		if (linked != null) {
			Gate = linked;
			Gate.SetDhd(this);
		}
	}

	public virtual void CreateButtons()
	{
		var i = 1;
		foreach ( char symbol in ButtonsSymbols )
		{
			var button = symbol.Equals( '.' ) ? new DHDButtonDial() : new DHDButton();
			button.SetModel( ButtonsModel );
			button.SetBodyGroup( 0, i++ );

			button.SetupPhysicsFromModel( PhysicsMotionType.Static, true );
			button.PhysicsBody.BodyType = PhysicsBodyType.Static;

			button.EnableAllCollisions = false;
			button.EnableTraceAndQueries = true;

			button.Position = Position;
			button.Rotation = Rotation;
			button.Scale = Scale;
			button.SetParent( this );

			Buttons.Add( symbol, button );
		}
	}

	public void EnableButton( char symbol )
	{
		if ( !Buttons.ContainsKey(symbol) ) return;

		var b = Buttons[symbol];
		if ( b.IsValid() ) b.On = true;
	}

	public void DisableButton( char symbol )
	{
		if ( !Buttons.ContainsKey( symbol ) ) return;

		var b = Buttons[symbol];
		if ( b.IsValid() ) b.On = false;
	}

	public void EnableButtons( string symbols )
	{
		foreach (char symbol in symbols) EnableButton( symbol );
	}

	public virtual void DisableButtons( string symbols )
	{
		foreach ( char symbol in symbols ) DisableButton( symbol );
	}

	public void EnableAllButtons()
	{
		foreach ( char symbol in ButtonsSymbols ) EnableButton( symbol );
	}

	public virtual void DisableAllButtons()
	{
		foreach ( char symbol in ButtonsSymbols ) DisableButton( symbol );
	}

	public override void StartTouch(Entity ent)
	{
		if ( ent is not Stargate || ent == Gate ) return;

		DisableAllButtons();

		Gate.SetDhd(null);
		Gate = ent as Stargate;
		Gate.SetDhd(this);

		if ( Gate.Open )
		{
			EnableButtons( Gate.OtherGate.Address );
			EnableButton( '.' );
		}
			
	}

}
