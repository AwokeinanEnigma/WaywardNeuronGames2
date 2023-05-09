using Enigmaware.Weaponry;
using UnityEngine;

namespace CryptidLand.Weapons
{
    public class Rifle : BaseWeaponState
    {
        public override int OverheatDrain { get; }
        public override int OverheatPerUse { get; }
        public override int OverheatMaximum { get; }
        public override string Name => "Rifle";
        
        public bool CheckFiringConditions() =>timeSinceLastFired > 1f / (fireRate / 60f);

        public  float fireRate => 600;
        public float timeSinceLastFired;
        
        
        
        
        
        public override void Primary()
        {
            Debug.Log("Hey, I'm a dummy!");
            if (CheckFiringConditions())
            {
                Debug.Log("Hey, I'm a big dummy!");
                if (Physics.Raycast(new Ray(cameraController.Position, cameraController.Forward), out RaycastHit hit, machine.hitmask))
                {
                    GameObject sleep = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sleep.transform.position = hit.point;
                    sleep.transform.localScale = new Vector3(3, 3, 3);
                    Debug.Log("I LOOOOOOOOOOOOOVE CUBA!");
                    timeSinceLastFired = 0;
                }
            }
        }

        private int type;
        
        public override void Secondary()
        {
        }

        #region  Entry/Exit


        public override void OnWeaponSwitchedTo(BaseWeaponState previous)
        {
            base.OnWeaponSwitchedTo(previous);
            cameraController.transform.Find("Sphere").gameObject.SetActive(true);
        }

        public override void OnWeaponSwitchedFrom(BaseWeaponState nextGun)
        {
            base.OnWeaponSwitchedFrom(nextGun);
            cameraController.transform.Find("Sphere").gameObject.SetActive(false);
        }
        
        

        #endregion

        #region  Update

        public override void Update()
        {
            base.Update();
            timeSinceLastFired += Time.deltaTime;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
       
        #endregion
    }
}