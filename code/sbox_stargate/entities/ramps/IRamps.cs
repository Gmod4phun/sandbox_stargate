using System.Collections.Generic;

public interface IStargateRamp {

	int AmountOfGates { get; }
	Vector3[] StargatePositionOffset { get; }
	Angles[] StargateRotationOffset { get; }

	List<Stargate> Gate { get; set; }
}

public interface IRingsRamp {
	Vector3 RingsPositionOffset { get; }
	Angles RingsRotationOffset { get; }

	Rings RingBase { get; set; }
}

public interface IDHDRamp {
	Vector3 DHDPositionOffset { get; }
	Angles DHDRotationOffset { get; }

	Rings DHD { get; set; }
}
