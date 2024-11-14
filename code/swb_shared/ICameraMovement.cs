﻿namespace SWB.Shared;

public interface ICameraMovement
{
	public bool IsFirstPerson { get; }
	public float InputSensitivity { get; set; }

	// External influence on the camera offsets
	public Angles EyeAnglesOffset { get; set; }
	public Angles AnglesOffset { get; set; }
	public Vector3 PosOffset { get; set; }
}
