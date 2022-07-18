﻿
using Sandbox;

/* Result from Pain Day 4, this will be here temporarily until it is clear how templates work */

namespace SWB_Base
{
    [Library]
    public class PlayerDuck : BaseNetworkable
    {
        public PlayerBaseController Controller;

        public bool IsActive; // replicate

        public PlayerDuck(PlayerBaseController controller)
        {
            Controller = controller;
        }

        public virtual void PreTick()
        {
            bool wants = Input.Down(InputButton.Duck);

            if (wants != IsActive)
            {
                if (wants) TryDuck();
                else TryUnDuck();
            }

            if (IsActive)
            {
                Controller.SetTag("ducked");
                Controller.EyeLocalPosition *= 0.5f;
            }
        }

        protected virtual void TryDuck()
        {
            IsActive = true;
        }

        protected virtual void TryUnDuck()
        {
            var pm = Controller.TraceBBox(Controller.Position, Controller.Position, originalMins, originalMaxs);
            if (pm.StartedSolid) return;

            IsActive = false;
        }

        // Uck, saving off the bbox kind of sucks
        // and we should probably be changing the bbox size in PreTick
        Vector3 originalMins;
        Vector3 originalMaxs;

        public virtual void UpdateBBox(ref Vector3 mins, ref Vector3 maxs, float scale)
        {
            originalMins = mins;
            originalMaxs = maxs;

            if (IsActive)
                maxs = maxs.WithZ(36 * scale);
        }

        //
        // Coudl we do this in a generic callback too?
        //
        public virtual float GetWishSpeed()
        {
            if (!IsActive) return -1;
            return 64.0f;
        }
    }
}
