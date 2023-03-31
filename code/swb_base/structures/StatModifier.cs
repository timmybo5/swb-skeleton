﻿using Sandbox;

namespace SWB_Base;

public class StatModifier
{
    public float Damage { get; set; } = 0;
    public float Recoil { get; set; } = 0;
    public float Spread { get; set; } = 0;
    public float RPM { get; set; } = 0;
    public float BulletVelocity { get; set; } = 0;

    public static readonly StatModifier Zero = new();

    private bool applied;
    private int applyTries;

    public async void Apply(WeaponBase weapon)
    {
        if (applied || applyTries > 10) return;
        if (weapon.InitialStats == null)
        {
            var instanceID = weapon.InstanceID;
            applyTries++;

            await GameTask.DelaySeconds(0.1f);
            if (weapon != null && weapon.IsValid && weapon.IsAsyncValid(weapon, instanceID))
                Apply(weapon);

            return;
        }

        Apply(weapon.Primary, weapon.InitialStats);
        Apply(weapon.Secondary, weapon.InitialStats);

        weapon.BulletVelocityMod += BulletVelocity;
        applied = true;
    }

    private void Apply(ClipInfo clipInfo, StatModifier initialStats)
    {
        if (clipInfo == null) return;

        clipInfo.Damage += initialStats.Damage * Damage;
        clipInfo.Recoil += initialStats.Recoil * Recoil;
        clipInfo.Spread += initialStats.Spread * Spread;
        clipInfo.RPM += (int)(initialStats.RPM * RPM);
    }

    public void Remove(WeaponBase weapon)
    {
        if (!applied) return;

        Remove(weapon.Primary, weapon.InitialStats);
        Remove(weapon.Secondary, weapon.InitialStats);

        weapon.BulletVelocityMod -= BulletVelocity;
        applied = false;
    }

    private void Remove(ClipInfo clipInfo, StatModifier initialStats)
    {
        if (clipInfo == null) return;

        clipInfo.Damage -= initialStats.Damage * Damage;
        clipInfo.Recoil -= initialStats.Recoil * Recoil;
        clipInfo.Spread -= initialStats.Spread * Spread;
        clipInfo.RPM -= (int)(initialStats.RPM * RPM);
    }
}
