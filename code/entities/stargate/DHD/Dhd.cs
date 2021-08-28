using System;
using System.Collections.Generic;
using Sandbox;

public abstract partial class Dhd : Prop {

	[Net]
	[Property(Name = "Gate", Group = "Stargate")]
	public Stargate Gate { get; protected set; }

	protected List<DhdButton> Buttons = new();

	protected string[] ButtonModels { get; set; } = Array.Empty<string>();

	protected readonly string[] ButtonsOrder = {
		"0",
		"1",
		"2",
		"3",
		"4",
		"5",
		"6",
		"7",
		"8",
		"9",
		"A",
		"B",
		"C",
		"D",
		"E",
		"F",
		"G",
		"H",
		"I",
		"J",
		"K",
		"L",
		"M",
		"N",
		"O",
		"#",
		"P",
		"Q",
		"R",
		"S",
		"T",
		"U",
		"V",
		"W",
		"X",
		"Y",
		"Z",
		"@",
		"DIAL"
	};

	protected Vector3[] ButtonPositions { get; set; } = Array.Empty<Vector3>();

	public override void Spawn()
	{
		base.Spawn();

		Stargate linked = null;
		List<Stargate> tested = new();
		while (linked == null) {
			Stargate tried = Stargate.FindClosestGate(Position, 5000f, tested.ToArray());
			if (tried == null)
				break;

			if (tried.Dhd == null || !tried.Dhd.IsValid())
				linked = tried;
			else
				tested.Add(tried);
		}

		if (linked != null) {
			Gate = linked;
			Gate.SetDhd(this);
		}
	}

	public virtual void CreateButtons() {
		Log.Info(ButtonModels.Length);
		int i = 0;
		foreach (string mdl in ButtonModels) {
			DhdButton but;
			if (mdl == ButtonModels[ButtonModels.Length - 1])
				but = new DialDhdButton();
			else
				but = new DhdButton();
			but.SetModel(mdl);
			but.SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
			Vector3 offset = ButtonPositions[i];
			but.Position = Position + offset;
			but.Rotation = Rotation;
			but.Scale = Scale;
			but.SetParent(this);

			Buttons.Add(but);
			i++;
		}

	}

	public virtual void DisableButtons() {
		foreach (DhdButton but in Buttons) {
			but.On = false;
		}
	}

	public void EnableButton(string c) {
		for (int i = 0; i < ButtonsOrder.Length; i++) {
			if (ButtonsOrder[i] == c) {
				Buttons[i].On = true;
				break;
			}
		}
	}

	protected void SetModels(string[] btnModels) {
		this.ButtonModels = btnModels;
	}

	protected void SetPositions(Vector3[] pos) {
		this.ButtonPositions = pos;
	}

}
