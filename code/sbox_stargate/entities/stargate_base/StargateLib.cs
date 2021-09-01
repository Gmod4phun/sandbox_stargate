using Sandbox;
using System;
using System.Linq;
using System.Text;
public partial class Stargate : Prop, IUse
{
	public enum DialType
	{
		SLOW = 0,
		FAST = 1,
		INSTANT = 2,
		NOX = 3
	}

	public enum GateState
	{
		IDLE = 0,
		ACTIVE = 1,
		OPENING = 2,
		OPEN = 3,
		CLOSING = 4
	}

	public const string Symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789#?@*";
	public const string SymbolsNoOrigins = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@*";

	public readonly int[] ChevronAngles = { 40, 80, 120, 240, 280, 320, 0, 160, 200 };

	public const int AutoCloseTimerDuration = 5;

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
	public static Stargate FindFarthestGate( Entity ent )
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

	/// <summary>
	/// Adds an Iris on the target Stargate if it does not have one yet.
	/// </summary>
	/// <returns>The just created, or already existing Iris.</returns>
	public static StargateIris AddIris(Stargate gate, Entity owner = null)
	{
		if ( !gate.HasIris() )
		{
			var iris = new StargateIris();
			iris.Position = gate.Position;
			iris.Rotation = gate.Rotation;
			iris.Scale = gate.Scale;
			iris.SetParent( gate );
			iris.Gate = gate;
			//iris.Owner = owner; -- why the fuck does this break iris anims
			gate.Iris = iris;
		}
		return gate.Iris;
	}

	/// <summary>
	/// Attempts to remove the Iris from the target Stargate.
	/// </summary>
	/// <returns>Whether or not the Iris was removed succesfully.</returns>
	public static bool RemoveIris(Stargate gate)
	{
		if ( gate.HasIris() )
		{
			gate.Iris.Delete();
			return true;
		}
		return false;
	}
}
