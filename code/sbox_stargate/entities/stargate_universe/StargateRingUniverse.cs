using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class StargateRingUniverse : StargateRingMilkyWay
{
	// ring variables

	public List<ModelEntity> SymbolParts { get; private set; } = new();

	public StargateRingUniverse()
	{
		EarthPointOfOrigin = false;
		StopSoundOnSpinDown = false;

		StartSoundName = "gate_roll_long";
		StopSoundName = "gate_sg1_ring_stop";

		RingSymbols = " ZB9J QNLMFVKO6 DCWY #R@S 8APU T7H5X4IG0 12E3";

		RingMaxSpeed = 75f;
		RingAccelStep = 1f;
		RingDeccelStep = 1f;
	}

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;

		SetModel( "models/sbox_stargate/gate_universe/gate_universe.vmdl" );
		EnableAllCollisions = false;

		CreateSymbolParts();
	}

	// create symbols
	// symbol models

	public void AddSymbolPart( string name )
	{
		var part = new ModelEntity( name );
		part.Position = Position;
		part.Rotation = Rotation;
		part.SetParent( this );
		part.Transmit = TransmitType.Always;
		part.EnableAllCollisions = false;

		SymbolParts.Add( part );
	}

	public void CreateSymbolParts()
	{
		AddSymbolPart( "models/sbox_stargate/gate_universe/gate_universe_symbols_1_18.vmdl" );
		AddSymbolPart( "models/sbox_stargate/gate_universe/gate_universe_symbols_19_36.vmdl" );
	}

	protected override void OnDestroy()
	{
		foreach ( var part in SymbolParts )
		{
			if ( IsServer && part.IsValid() ) part.Delete();
		}

		base.OnDestroy();
	}

	public int GetSymbolNumber(char sym)
	{
		if ( !RingSymbols.Contains( sym ) ) return -1;

		var syms = new StringBuilder( RingSymbols );
		syms = syms.Replace( " ", "" );
		syms = syms.Replace( "F", "" );
		syms = syms.Replace( "X", "" );

		return syms.ToString().IndexOf( sym );
	}

	public async void SetSymbolState( int num, bool state, float delay = 0 )
	{
		if ( delay > 0 )
		{
			await Task.DelaySeconds( delay );
			if ( this.IsValid() ) return;
		}

		num = num.UnsignedMod( 36 );
		var isPart1 = num < 18;
		SymbolParts[isPart1 ? 0 : 1].SetBodyGroup( (isPart1 ? num : num - 18), state ? 1 : 0 );
	}

	public void SetSymbolState( char sym, bool state )
	{
		var symNum = GetSymbolNumber( sym );
		if (symNum >= 0) SetSymbolState( symNum, state );
	}

	public void ResetSymbols()
	{
		for ( int i = 0; i <= 35; i++ ) SetSymbolState( i, false );
	}

}
