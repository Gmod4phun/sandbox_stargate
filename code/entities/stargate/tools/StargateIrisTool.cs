namespace Sandbox.Tools
{
	[Library( "tool_stargate_iris", Title = "Iris", Description = "Used to create the iris on the gate.\n\nMOUSE1 - Spawn iris\nMOUSE2 - Remove iris\nR - toggle iris", Group = "construction" )]
	public partial class StargateIrisTool : BaseTool
	{
		PreviewEntity previewModel;
		private string Model => "models/gmod4phun/stargate/iris/iris.vmdl";

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
				if ( Owner.IsValid() )
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

			if ( Owner.IsValid() && Owner.Health > 0 )
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
					

					if ( tr.Entity is Stargate gate )
					{
						Stargate.CreateIris(gate);
						CreateHitEffects( tr.EndPos );
					}

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

					if ( tr.Entity is Stargate gate && gate.Iris.IsValid())
					{
						gate.Iris.Toggle();
						CreateHitEffects( tr.EndPos );
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


					if ( tr.Entity is StargateSG1 gate )
					{
						Stargate.RemoveIris( gate );
						CreateHitEffects( tr.EndPos );
					}

				}


			}
		}
	}
}
