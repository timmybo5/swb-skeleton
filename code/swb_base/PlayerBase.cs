﻿using System.Linq;
using Sandbox;
using SWB_Base.UI;

namespace SWB_Base
{
    public partial class PlayerBase : Player
    {
        public DamageInfo LastDamage;

        public override void TakeDamage(DamageInfo info)
        {
            LastDamage = info;

            // Headshot double damage
            if (GetHitboxGroup(info.HitboxIndex) == 1)
            {
                info.Damage *= 2.0f;
            }

            base.TakeDamage(info);

            if (info.Attacker is PlayerBase attacker && attacker != this)
            {
                // Note - sending this only to the attacker!
                attacker.DidDamage(To.Single(attacker), info.Position, info.Damage, Health, ((float)Health).LerpInverse(100, 0));

                // Hitmarker
                var weapon = info.Weapon as WeaponBase;
                if (weapon != null && weapon.UISettings.ShowHitmarker)
                    attacker.ShowHitmarker(To.Single(attacker), !Alive(), weapon.UISettings.PlayHitmarkerSound);

                TookDamage(To.Single(this), info.Weapon.IsValid() ? info.Weapon.Position : info.Attacker.Position);
            }
        }

        public virtual bool Alive()
        {
            return Health > 0;
        }

        [ClientRpc]
        public virtual void DidDamage(Vector3 pos, float amount, float health, float healthinv)
        {
            Sound.FromScreen("dm.ui_attacker")
                .SetPitch(1 + healthinv * 1);
        }

        [ClientRpc]
        public virtual void TookDamage(Vector3 pos)
        {
            //DebugOverlay.Sphere( pos, 5.0f, Color.Red, false, 50.0f );

            DamageIndicator.Current?.OnHit(pos);
        }

        [ClientRpc]
        public virtual void ShowHitmarker(bool isKill, bool playSound)
        {
            Hitmarker.Current?.Create(isKill);

            if (playSound)
                PlaySound("swb_hitmarker");
        }
    }
}
