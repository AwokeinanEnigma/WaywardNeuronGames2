using System;
using Enigmaware.EntityStates;
using Enigmaware.Player;

namespace Enigmaware.Weaponry
{
    public abstract class BaseWeaponState : EntityState
    {
        public WeaponMachine machine;
        
        public int Overheat;
        public bool Overheated;
        public bool AttemptingUse;
        public abstract int OverheatDrain { get; }
        public abstract int OverheatPerUse { get; }
        public abstract int OverheatMaximum { get; }
        
        public abstract string Name { get; }

        
        protected CameraController cameraController;
        protected PlayerMotor playerMovement;

        public override void OnEnter()
        {
            base.OnEnter();
            cameraController = machine.Camera;
            playerMovement = machine.CharacterMotor;
        }

        public abstract void Primary();
        public abstract void Secondary();

        public virtual void OnWeaponSwitchedFrom(BaseWeaponState nextGun) { 

        }

        public virtual void OnWeaponSwitchedTo(BaseWeaponState previous) {

        }
        
        public static new BaseWeaponState InstantiateState(Type stateType)
        {
            if (stateType != null && stateType.IsSubclassOf(typeof(BaseWeaponState)))
            {
                return Activator.CreateInstance(stateType) as BaseWeaponState;
            }
            return null;
        }
    }
}
