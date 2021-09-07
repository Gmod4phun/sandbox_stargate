using Sandbox;

public partial class DhdButtonTrigger : AnimEntity, IUse
{
	[Net]
	public string Action { get; set; } = "";
	public Dhd DHD;
	public DhdButton Button;

	private DhdButtonPanel Panel;

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
		Health = 100;
	}

	public void UpdatePanelPos()
	{
		if ( Panel is null ) return;

		var bone = GetBoneTransform( "dhd_buttons" );
		var boneRot = bone.Rotation;
		var rot = Transform.RotationToWorld( boneRot );

		rot = rot.RotateAroundAxis( boneRot.Right, -10 );

		var pos = Transform.PointToWorld( GetModel().RenderBounds.Center ) + rot.Up * 0.8f;

		var panelRot = rot.RotateAroundAxis( boneRot.Up, 180 ).RotateAroundAxis( boneRot.Right, -90 );

		Panel.Position = pos;
		Panel.Rotation = panelRot;

		//DebugOverlay.Line( pos, pos + rot.Up * 8, 0, false );
	}

	public void CreatePanel()
	{
		Panel?.Delete();
		Panel = new DhdButtonPanel( Action );
		UpdatePanelPos();
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		//CreatePanel();
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

		Panel?.Delete();
	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );

		Log.Info( $"{info.Damage} {Health}" );

		if ( Health <= 0 ) DestroyTriggerAndButton();
	}

	public void DrawSymbols()
	{
		if ( Action.Length > 0 )
		{
			var pos = Transform.PointToWorld( GetModel().RenderBounds.Center );
			DebugOverlay.Text( pos, Action, Color.Green );
		}
	}

	[Event.Frame]
	public void DhdButtonTriggerThink()
	{
		if ( Local.Pawn.IsValid() && Local.Pawn.Position.Distance(Position) < 580) DrawSymbols();
		//UpdatePanelPos();
	}
}
