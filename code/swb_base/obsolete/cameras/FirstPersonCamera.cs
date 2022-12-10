﻿using Sandbox;

namespace SWB_Base;

public class FirstPersonCamera : CameraMode
{
    public override void UpdateCamera()
    {
        base.UpdateCamera();

        if (Entity is not PlayerBase player) return;

        Camera.ZNear = 1f;
        Camera.ZFar = 25000.0f;
        Camera.Rotation = player.ViewAngles.ToRotation();
        Camera.Position = player.EyePosition;
        Camera.FieldOfView = Game.Preferences.FieldOfView;
        Camera.FirstPersonViewer = player;
        Camera.Main.SetViewModelCamera(Camera.FieldOfView, 0.01f, 100.0f);

        if (player.ActiveChild is WeaponBase weapon)
        {
            weapon.UpdateViewmodelCamera();
            weapon.UpdateCamera();
        }
    }
}
