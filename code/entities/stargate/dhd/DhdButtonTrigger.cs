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

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if (DHD.IsValid()) DHD.Delete();
	}
}
