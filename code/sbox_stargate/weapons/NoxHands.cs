using Sandbox;
using System.Linq;

[Library( "weapon_stargate_noxhands", Title = "Nox Hands", Description = "Instant dialling of the gate, without kawoosh effect.", Spawnable = true, Group = "Stargate" )]
public partial class StargateNoxHands : Weapon
{
	//later add a hand model
	//public override string ViewModelPath => "hand model";
	public override float PrimaryRate => 1.0f;
	public override float SecondaryRate => 1.0f;
	/// <summary>
	/// in the future when all settings are globalized, there is this value so that the gate opening distance can be adjusted.
	/// </summary>
	public static float MaxDistance = 1500;

	public override void Spawn()
	{
		base.Spawn();
		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack() && Input.Pressed( InputButton.Attack1 );
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;

		var gate = Stargate.FindClosestGate( Owner.Position, MaxDistance );
		if ( gate is not null )
		{
			if ( gate.IsValid() )
			{
				gate.OpenStargateMenu();
			}
		}

	}

	public override void AttackSecondary()
	{
		TimeSinceSecondaryAttack = 0;

		//var gate = Stargate.FindClosestGate( Owner.Position, MaxDistance );
		//if ( gate is not null )
		//{
		//	if ( gate.IsValid() )
		//	{
		//		var _Hud = Local.Hud.ChildrenOfType<StargateMenuV2>().ToArray();

		//		if ( _Hud[0] is not null )
		//		{
		//			_Hud[0].Delete();
		//		}
		//	}
		//}
	}
}
