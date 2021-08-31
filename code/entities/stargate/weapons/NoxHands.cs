using Sandbox;

[Library( "weapon_stargate_noxhands", Title = "nox hands", Description = "Instant dialling of the gate, without kawoosh effect.", Spawnable = true, Group = "Stargate" )]
public partial class StargateNoxHands : Weapon
{
	//later add a hand model
	//public override string ViewModelPath => "hand model";
	public override float PrimaryRate => 1.0f;
	public override float SecondaryRate => 1.0f;

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
		// when the instant dial and gui menu is functional, it will be replaced by them
		// mouse 1 => find nearest gate ==> dial to random gate.
		// mouse 2 => close nearest gate

		TimeSincePrimaryAttack = 0;

		var gate = Stargate.FindNearestGate( Owner );
		if ( gate is null ) return;
		if ( gate.Busy || gate.Open ) return;

		if ( !gate.Dialing )
		{
			var secondGate = Stargate.FindRandomGate( gate );
			if ( secondGate is not null )
			{
				gate.BeginDialInstant( secondGate.Address );
			}
		}
		else
		{
			gate.StopDialing();
		}

	}

	public override void AttackSecondary()
	{
		TimeSinceSecondaryAttack = 0;
		var gate = Stargate.FindNearestGate( Owner );
		if ( gate is null ) return;
		gate.DoStargateClose( true );
	}
}
