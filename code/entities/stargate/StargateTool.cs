namespace Sandbox.Tools
{
	[Library( "tool_stargatespawner", Title = "Stargate", Description = "Use wormholes to transport matter\n\nMOUSE1 - Spawn gate\nR - copy gate address\nMOUSE2 - Close gate/Stop dialling/Fast dial copied address\nSHIFT + MOUSE2 - Slow dial copied address\nCTRL + MOUSE2 - Instant dial copied address\nSHIFT + R - Add an Iris or Toggle it if the gate already have one", Group = "construction" )]
	public partial class StargateSpawnerTool : BaseTool
	{
		PreviewEntity previewModel;

		static string address = "";

		private string Model => "models/gmod4phun/stargate/gate_sg1/gate_sg1.vmdl";

		protected override bool IsPreviewTraceValid( TraceResult tr )
		{
			if ( !base.IsPreviewTraceValid( tr ) )
				return false;

			if ( tr.Entity is Stargate )
				return false;

			return true;
		}

		public override void CreatePreviews()
		{
			if ( TryCreatePreview( ref previewModel, Model ) )
			{
				if (Owner.IsValid())
				{
					previewModel.RelativeToNormal = false;
					previewModel.OffsetBounds = false;
					previewModel.PositionOffset = new Vector3( 0, 0, 90 );
					previewModel.RotationOffset = new Angles( 0, Owner.EyeRot.Angles().yaw + 180, 0 ).ToRotation();
				}

			}
		}

		public override void OnFrame()
		{
			base.OnFrame();

			if ( Owner.IsValid() && Owner.Health > 0)
			{
				RefreshPreviewAngles();
			}
		}

		public void RefreshPreviewAngles()
		{
			foreach ( var preview in Previews )
			{
				if ( !preview.IsValid() || !Owner.IsValid() )
					continue;

				preview.RotationOffset = new Angles( 0, Owner.EyeRot.Angles().yaw + 180, 0 ).ToRotation();

			}
		}

		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				if ( Input.Pressed( InputButton.Attack1 ) )
				{
					var startPos = Owner.EyePos;
					var dir = Owner.EyeRot.Forward;

					var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
						.Ignore( Owner )
						.Run();

					if ( !tr.Hit || !tr.Entity.IsValid() )
						return;

					CreateHitEffects( tr.EndPos );

					if ( tr.Entity is Stargate )
					{
						// TODO: Set properties

						return;
					}

					var gate = new StargateSG1();

					gate.Position = tr.EndPos + gate.SpawnOffset;
					gate.Rotation = new Angles( 0, Owner.EyeRot.Angles().yaw + 180, 0 ).ToRotation();
					gate.Owner = Owner;
				}

				if ( Input.Pressed( InputButton.Reload ) )
				{					
					var startPos = Owner.EyePos;
					var dir = Owner.EyeRot.Forward;

					var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
						.Ignore( Owner )
						.Run();

					if ( !tr.Hit || !tr.Entity.IsValid() )
						return;

					CreateHitEffects( tr.EndPos );

					if ( tr.Entity is Stargate gate)
					{

						if (Input.Down(InputButton.Run)) {
							if (gate.Iris.IsValid()) gate.Iris.Toggle();
							else
							{
								gate.Iris = new StargateIris();
								gate.Iris.Position = gate.Position;
								gate.Iris.Rotation = gate.Rotation;
								gate.Iris.Scale = gate.Scale;
								gate.Iris.SetParent( gate );
								gate.Iris.Gate = gate;
								gate.Iris.Close();
							}

							return;
						}

						address = gate.Address;

						Log.Info( $"Copied this gates address: {address}" );
						return;
					}

				}


				if ( Input.Pressed( InputButton.Attack2 ) )
				{
					var startPos = Owner.EyePos;
					var dir = Owner.EyeRot.Forward;

					var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
						.Ignore( Owner )
						.Run();

					if ( !tr.Hit || !tr.Entity.IsValid() )
						return;

					CreateHitEffects( tr.EndPos );


					if ( tr.Entity is StargateSG1 gate )
					{
						if ( gate.Busy ) return;

						if (gate.Open)
						{
							gate.DoStargateClose(true);
						}
						else
						{
							if ( !gate.Dialing )
							{
								if (Input.Down(InputButton.Run))
								{
									gate.BeginDialSlow( address );
								}
								else if (Input.Down(InputButton.Duck))
								{
									gate.BeginDialInstant( address );
								}
								else
								{
									gate.BeginDialFast( address );
								}
							}
							else
							{
								gate.StopDialing();
							}
						}


					}

				}


			}
		}
	}
}
