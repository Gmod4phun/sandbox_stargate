using Sandbox;
using System.Linq;

[Library( "weapon_stargate_ringscontroller", Title = "Rings Controller", Description = "", Spawnable = true, Group = "Stargate" )]
public partial class StargateRingsController : Weapon
{
	//later add a hand model
	//public override string ViewModelPath => "hand model";
	public override float PrimaryRate => 15.0f;
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
		TimeSincePrimaryAttack = 0;

		var ring = Entity.All.OfType<Rings>().FirstOrDefault();

		ring.DialClosest();
	}
}
