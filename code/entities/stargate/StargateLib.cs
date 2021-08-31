using Sandbox;
using System;
using System.Linq;
using System.Text;

public partial class Stargate : Prop, IUse
{
	/// <summary>
	/// Generates a random address.
	/// </summary>
	/// <param name="length">how much the symbol address should be generated.</param>
	/// <returns>Gate Adress.</returns>
	public static string GenerateRandomAddress( int length = 7 )
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

	/// <summary>
	/// verify that the address is valid.
	/// </summary>
	/// <param name="address">The gate address represented in the string.</param>
	/// <returns>True or False</returns>
	public static bool IsValidAddress( string address ) // a valid address has 7, 8, or 9 VALID characters and has no repeating symbols
	{
		if ( address.Length < 7 || address.Length > 9 ) return false; // only 7, 8 or 9 symbol addresses 
		foreach ( char sym in address )
		{
			if ( !Symbols.Contains( sym ) ) return false; // only valid symbols
			if ( address.Count( c => c == sym ) > 1 ) return false; // only one occurence
		}
		return true;
	}

	/// <summary>
	/// Returns the gate if it finds it by string address.
	/// </summary>
	/// <param name="address">The gate address represented in the string.</param>
	/// <returns>A gate that matches the parameter.</returns>
	public static Stargate FindByAddress( string address )
	{
		foreach ( Stargate gate in Entity.All.OfType<Stargate>() )
		{
			if ( gate.Address.Equals( address ) ) return gate;
		}
		return null;
	}

	/// <summary>
	/// Return the random gate.
	/// </summary>
	/// <returns>A gate that matches the parameter.</returns>
	public static Stargate FindRandomGate()
	{
		foreach ( Stargate gate in Entity.All.OfType<Stargate>() )
		{
			return gate;
		}
		return null;
	}

	/// <summary>
	/// Return the random gate, this gate will never be the gate given in the argument
	/// </summary>
	/// <param name="ent">A gate that is eliminated with a random outcome.</param>
	/// <returns>A gate that matches the parameter.</returns>
	public static Stargate FindRandomGate( Stargate ent )
	{
		foreach ( Stargate gate in Entity.All.OfType<Stargate>().ToList() )
		{
			if (gate.IsValid() && ent != gate && !gate.Busy && !ent.Busy )
			{
				return gate;
			}
		}
		return null;
	}

	/// <summary>
	/// It finds the nearest gate from the entity. It returns that gate.
	/// </summary>
	/// <param name="ent">The entity that will be the first point of remoteness.</param>
	/// <returns>A gate that matches the parameter.</returns>
	public static Stargate FindNearestGate( Entity ent )
	{
		var allGates = Entity.All.OfType<Stargate>().ToList();
		if ( allGates.Count() is 0 ) return null;
		var distanceAllGates = new int[allGates.Count()];

		for ( int i = 0; i < allGates.Count(); i++ )
		{
			distanceAllGates[i] = (int)Vector3.DistanceBetween( ent.Position, allGates[i].Position );
		}

		int minDistance = distanceAllGates.Min();
		int indexInArray = distanceAllGates.ToList().IndexOf( minDistance );
		return allGates[indexInArray];
	}

	/// <summary>
	/// It finds the furthest gate from the entity that is in the argument. It returns that gate.
	/// </summary>
	/// <param name="ent">The entity that will be the first point of remoteness.</param>
	/// <returns>A gate that matches the parameter.</returns>
	public static Stargate FindfarthesttGate( Entity ent )
	{
		var allGates = Entity.All.OfType<Stargate>().ToList();
		if ( allGates.Count() is 0 ) return null;
		var distanceAllGates = new int[allGates.Count()];

		for ( int i = 0; i < allGates.Count(); i++ )
		{
			distanceAllGates[i] = (int)Vector3.DistanceBetween( ent.Position, allGates[i].Position );
		}

		int minDistance = distanceAllGates.Max();
		int indexInArray = distanceAllGates.ToList().IndexOf( minDistance );
		return allGates[indexInArray];
	}

	public static StargateIris CreateIris(Stargate gate)
	{
		if ( gate.HasIris() is not true )
		{
			gate.Iris = new StargateIris();
			gate.Iris.Position = gate.Position;
			gate.Iris.Rotation = gate.Rotation;
			gate.Iris.Scale = gate.Scale;
			gate.Iris.SetParent( gate );
			gate.Iris.Gate = gate;
			gate.Iris.Close();
		}
		return gate.Iris;
	}

	public static Stargate RemoveIris(Stargate gate)
	{
		if ( gate.HasIris() is true )
		{
			gate.Iris.Delete();
		}
		return gate;
	}
}
