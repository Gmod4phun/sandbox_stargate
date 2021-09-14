using System.Collections.Generic;
using Sandbox;

public partial class RingPanel : ModelEntity, IUse
{

	protected List<Transform> Buttons = new();
	protected string ComposedAddress = "";

	protected virtual int DialButtonNumber { get; }
	protected virtual string[] ButtonsSounds { get; } = {
		"goauld_button1",
		"goauld_button2"
	};

	protected void GetButtonsPos() {
		Buttons.Clear();
		for (int i = 1; i <= 6; i++) {
			var btnTr = GetBoneTransform( $"button{i}", true );

			if (btnTr == null)
			{
				Buttons.Clear();
				throw new System.Exception($"Unable to find {i} button bone on ring panel {GetType()}");
			}

			Buttons.Add(btnTr);
		}
	}

	protected async void ToggleButton(int id) {
		SetMaterialGroup(id);
		PlaySound(id != DialButtonNumber ? ButtonsSounds[1] : ButtonsSounds[0]);
		await Task.Delay(350);
		SetMaterialGroup(0);
	}

	public bool IsUsable( Entity user )
	{
		return true;
	}

	public virtual bool OnUse( Entity ent )
	{
		if ( ent is not SandboxPlayer )
			return false;

		var tra = Trace.Ray(ent.EyePos, ent.EyePos + ent.EyeRot.Forward * 250)
			.Ignore(ent)
			.Run();

		if (tra.Hit && tra.Entity is RingPanel) {

			// Force Refresh buttons pos if the panel move
			GetButtonsPos();
			for (int i = 0; i < Buttons.Count; i++)
			// foreach ( Transform btn in Buttons)
			{
				Transform btn = Buttons[i];
				var dist = tra.EndPos.Distance(btn.Position);

				if (dist <= 1.65f) {
					ToggleButton(Buttons.IndexOf(btn) + 1);

					if (i + 1 == DialButtonNumber)
					{
						Rings self = Rings.GetClosestRing(Position, null, 500f);
						if (ComposedAddress.Length == 0)
						{
							if (self is not null)
								self.DialClosest();
						}
						else
						{
							if (self is not null)
								self.DialAddress( ComposedAddress );

							ComposedAddress = "";
						}
					}
					else
					{
						ComposedAddress += (char) i + 1;
						Log.Info(ComposedAddress);
					}
					break;
				}
			}
		}
		
		return false;
	}
}
