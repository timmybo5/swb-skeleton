namespace SWB_WEAPONS
{
    [Library("template_weapon", Title = "Template Weapon")]
    public class TemplateWeapon : WeaponBase
    {
        public override string ViewModelPath => "path/to/v_model.vmdl";
        public override string WorldModelPath => "path/to/w_model.vmdl";
        public override string Icon => "/path/to/icon.png";

        public TemplateWeapon()
        {
            General = new WeaponInfo
            {
                DrawTime = 1f,
                ReloadTime = 1f,
            };

            Primary = new ClipInfo
            {
                Ammo = 25,
                AmmoType = AmmoTypes.Rifle,
                ClipSize = 25,

                BulletSize = 3f,
                BulletType = new RifleBullet(),
                Damage = 15f,
                Force = 3f,
                HitFlinch = 1.25f,
                Spread = 0.08f,
                Recoil = 0.35f,
                RPM = 600,
                FiringType = FiringType.auto,
                ScreenShake = new ScreenShake
                {
                    Length = 0.08f,
                    Delay = 0.02f,
                    Size = 1f,
                    Rotation = 0.1f
                },

                DryFireSound = "swb_rifle.empty",
                ShootSound = "shoot.sound",

                BulletEjectParticle = "particles/pistol_ejectbrass.vpcf",
                MuzzleFlashParticle = "particles/swb/muzzle/flash_medium.vpcf",
                BulletTracerParticle = "particles/swb/tracer/phys_tracer_medium.vpcf",
            };

            RunAnimData = new AngPos
            {
                Angle = new Angles(10, 40, 0),
                Pos = new Vector3(5, 0, 0)
            };
        }
    }
}
