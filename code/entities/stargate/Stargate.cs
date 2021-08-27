using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class Stargate : Prop, IUse
{
	public Vector3 SpawnOffset = new ( 0, 0, 90 );

	protected string Symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789#?@*";
	protected string SymbolsNoOrigins = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@*";

	public EventHorizon EventHorizon;
	public Stargate OtherGate;

	[Net]
	[Property( "Address", Group = "Stargate" )]
	public string Address { get; protected set; } = "";

	[Net]
	[Property( "Active", Group = "Stargate" )]
	public bool Active { get; protected set; } = false;
	public bool Inbound = false;

	[Net]
	[Property( "Open", Group = "Stargate" )]
	public bool Open { get; protected set; } = false;

	public bool Dialing = false;
	public bool ShouldStopDialing = false;

	protected Sound WormholeLoop;

	// USABILITY

	public bool IsUsable( Entity user )
	{
		return true;
	}

	public bool OnUse( Entity user )
	{
		// OpenGateMenu();
		return false; // SIMPLE_USE, not continuously
	}

	// SPAWN

	public override void Spawn()
	{
		base.Spawn();
	}

	// ADDRESS
	public string GenerateRandomAddress(int length = 7)
	{
		if ( length < 7 || length > 9 ) return "";

		StringBuilder symbolsCopy = new( SymbolsNoOrigins );

		string generatedAddress = "";
		for ( int i = 1; i < length; i++ ) // pick random symbols without repeating
		{
			var randomIndex = new Random().Int( 0, symbolsCopy.Length - 1 );
			generatedAddress += symbolsCopy[randomIndex];

			symbolsCopy = symbolsCopy.Remove( randomIndex, 1 );
		}
		generatedAddress += '#'; // add a point of origin
		return generatedAddress;
	}

	public bool IsValidAddress(string address) // a valid address has 7, 8, or 9 VALID characters and has no repeating symbols
	{
		if ( address.Length < 7 || address.Length > 9 ) return false; // only 7, 8 or 9 symbol addresses 
		foreach (char sym in address)
		{
			if ( !Symbols.Contains(sym) ) return false; // only valid symbols
			if ( address.Count( c => c == sym ) > 1 ) return false; // only one occurence
		}
		return true;
	}

	public Stargate FindByAddress(string address)
	{
		foreach ( Stargate gate in Entity.All.OfType<Stargate>() )
		{
			if ( gate.Address.Equals(address) ) return gate;
		}
		return null;
	}

	// EVENT HORIZON

	public void CreateEventHorizon()
	{
		EventHorizon = new EventHorizon();
		EventHorizon.Position = Position;
		EventHorizon.Rotation = Rotation;
		EventHorizon.Scale = Scale;
		EventHorizon.SetParent( this );
		EventHorizon.Gate = this;
	}

	public void DeleteEventHorizon()
	{
		EventHorizon?.Delete();
	}

	public async void EstablishEventHorizon()
	{
		EventHorizon.Establish();

		await GameTask.DelaySeconds( 2f );
		EventHorizon.IsFullyFormed = true;
	}

	public async void CollapseEventHorizon( float sec )
	{
		await GameTask.DelaySeconds( sec );
		EventHorizon.EH_Collapse();

		await GameTask.DelaySeconds( sec + 2f );
		DeleteEventHorizon();

		Open = false;
		Inbound = false;
		OtherGate = null;
	}

	// STARGATE
	public Stargate FindRandomGate()
	{
		foreach ( Stargate gate in Entity.All.OfType<Stargate>() )
		{
			if ( gate != this && !gate.Active ) return gate;
		}
		return null;
	}

	public void StargateOpen()
	{
		Open = true;
		Sound.FromEntity( "gate_open_sg1", this );

		CreateEventHorizon();
		EstablishEventHorizon();

		OnStargateOpen();
	}

	public void StargateClose(bool alsoCloseOther = false)
	{
		Sound.FromEntity( "gate_close", this );

		EventHorizon.IsFullyFormed = false;
		CollapseEventHorizon(0.25f);

		OnStargateClose();

		if (alsoCloseOther && OtherGate.IsValid()) OtherGate.StargateClose();
	}

	//[Event( "server.tick" )]
	//public void StargateTick()
	//{

	//}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		WormholeLoop.Stop();

		if ( IsServer && OtherGate.IsValid() )
		{
			OtherGate.StargateClose();
		}
	}

	// DIALING

	public virtual void BeginDialFast(string address) { }
	public virtual void BeginDialSlow(string address) { }

	public void StopDialing()
	{
		if ( !Dialing ) return;

		//Dialing = false;

		ShouldStopDialing = true;
		OnStopDialing();
	}

	public virtual void OnStopDialing()
	{
		Dialing = false;
	}
	public virtual void OnStargateOpen() { }
	public virtual void OnStargateClose() { }

}
