using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class RingWorldPanel : WorldPanel
{
	public Label[] Labels;

	public RingWorldPanel()
	{
		PanelBounds = new Rect(-100, -500, 1000, 1000);
	}

	public void SetAmount(int amount, float[][] pos)
	{
		Labels = new Label[amount];
		for (int i = 1; i <= amount; i++) {
			bool isDial = i == amount;
			var lab = Add.Label(isDial ? "DIAL" : i.ToString());
			lab.Style.FontColor = Color.White;
			lab.Style.Opacity = .3f;
			lab.Style.Position = PositionMode.Absolute;
			if (isDial) {
				lab.Style.FontSize = 8;
			}
			lab.Style.Top = pos[i - 1][0];
			lab.Style.Left = pos[i - 1][1];
			lab.Style.Dirty();
			Labels.SetValue(lab, i - 1);
		}
	}
}
