﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Controller needed to properly set player rotation and eye rotations on teleport
public partial class EventHorizonController : BasePlayerController
{
	public override void BuildInput( InputBuilder input )
	{
		input.ViewAngles = EyeRot.Angles();
		input.StopProcessing = true;
	}

	public override void FrameSimulate() { }

	public override void Simulate() { }

}
