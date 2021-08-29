using System.Collections.Generic;
using System;
using Sandbox;

[Library( "ent_dhd_sg1", Title = "Stargate SG-1 DHD", Spawnable = true )]
public partial class DhdSg1 : Dhd, IUse {

	protected static new string[] ButtonModels { get; set; } = {
		"models/markjaw/dhd_new/s2/buttons/b33.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b34.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b35.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b36.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b37.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b38.vmdl",

		"models/markjaw/dhd_new/s2/buttons/b20.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b21.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b22.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b23.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b24.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b25.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b26.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b27.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b28.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b29.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b30.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b31.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b32.vmdl",

		"models/markjaw/dhd_new/s2/buttons/b14.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b15.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b16.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b17.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b18.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b19.vmdl",

		"models/markjaw/dhd_new/s2/buttons/b1.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b2.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b3.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b4.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b5.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b6.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b7.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b8.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b9.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b10.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b11.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b12.vmdl",
		"models/markjaw/dhd_new/s2/buttons/b13.vmdl",

		"models/markjaw/dhd_new/s2/buttons/dialorb.vmdl"
	};

	protected static new Vector3[] ButtonPositions { get; set; } = {
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .2f),
		// 5
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .2f),
		// 10
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .2f),
		// 15
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .2f),
		// 20
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .2f),
		new Vector3(0, 0, .35f),
		new Vector3(0, 0, .5f),
		// 25
		new Vector3(0, 0, .55f),
		new Vector3(0, 0, .55f),
		new Vector3(0, 0, .75f),
		new Vector3(0, 0, .6f),
		new Vector3(0, 0, .6f),
		// 30
		new Vector3(0, 0, .6f),
		new Vector3(0, 0, .6f),
		new Vector3(0, 0, .6f),
		new Vector3(0, 0, .6f),
		new Vector3(0, 0, .55f),
		// 30
		new Vector3(0, 0, .55f), //
		new Vector3(0, 0, .3f),
		new Vector3(0, 0, .3f),
		// Dial
		new Vector3(0, 0, .1f),
	};
	
	public override void Spawn() {
		base.Spawn();

		Transmit = TransmitType.Always;
		SetModel( "models/markjaw/dhd_new/s2/dhd.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		PhysicsBody.BodyType = PhysicsBodyType.Static;

		this.SetModels(DhdSg1.ButtonModels);
		this.SetPositions(DhdSg1.ButtonPositions);

		CreateButtons();
	}

	public bool OnUse(Entity ent) {
		// if (Gate != null) {
		// 	if ((Gate.Active || Gate.Open) && ! Gate.Inbound) {
		// 		Gate.StargateClose(true);
		// 		return true;
		// 	}
		// 	Stargate target = Gate.FindClosestGate();
		// 	if (target != null)
		// 		(Gate as StargateSG1).BeginDialFast(target.Address);
		// }
		return false;
	}

	public bool IsUsable(Entity ent) {
		return true;
	}

	public override void CreateButtons() {
		base.CreateButtons();
	}

	// public override void CreateButtons() {
	// 	int i = 0;
	// 	foreach (string mdl in buttons_models) {
	// 		DhdButton but;
	// 		if (mdl == buttons_models[buttons_models.Length - 1])
	// 			but = new DialDhdButton();
	// 		else
	// 			but = new DhdButton();
	// 		but.SetModel(mdl);
	// 		but.SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
	// 		Vector3 offset = ButtonPositions[i];
	// 		but.Position = Position + offset;
	// 		// but.Position = Position + new Vector3(0, 0, .35f);
	// 		but.Rotation = Rotation;
	// 		but.Scale = Scale;
	// 		but.SetParent(this);

	// 		Buttons.Add(but);
	// 		i++;
	// 	}
	// }

}
