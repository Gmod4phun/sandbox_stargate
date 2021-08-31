using Sandbox;

public partial class DhdButtonTrigger : AnimEntity, IUse
{
	public string Action;
	public Dhd DHD;
	public DhdButton Button;

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
		Health = 100;
	}

	public virtual bool OnUse(Entity ent)
	{
		if ( Time.Now < DHD.lastPressTime + DHD.pressDelay ) return false;

		DHD.lastPressTime = Time.Now;
		DHD.TriggerAction( Action );

		return false;
	}

	public virtual bool IsUsable(Entity ent)
	{
		return true;
	}

	public void DestroyTriggerAndButton()
	{
		if ( Button.IsValid() ) Button.Delete();
		Delete();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Button.IsValid() ) Button.Delete();
	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );

		Log.Info( $"{info.Damage} {Health}" );

		if ( Health <= 0 ) DestroyTriggerAndButton();
	}
}
