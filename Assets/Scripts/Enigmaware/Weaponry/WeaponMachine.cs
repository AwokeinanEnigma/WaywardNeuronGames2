using Enigmaware.EntityStates;
using Enigmaware.Player;
using UnityEngine;
using UnityEngine.InputSystem;
 using UnityEngine.Serialization;

namespace Enigmaware.Weaponry
{
    //deez nuts

    public class WeaponMachine : MonoBehaviour
    {
        public LayerMask hitmask;
        

        public string machineName;
        [FormerlySerializedAs("_cam")] public CameraController Camera;
        [FormerlySerializedAs("_motor")] public PlayerMotor CharacterMotor;

        public SerializableEntityStateType[] EditorLoadout;
        // LOCK'N'LOAD!!!
        public BaseWeaponState[] Loadout;
        [SerializeField]
        private int _currentLoadoutIndex = 0;
        
        public void Awake()
        {
            // create dummy array
            BaseWeaponState[] array = new BaseWeaponState[EditorLoadout.Length];
            
            // fill entries
            foreach (SerializableEntityStateType a in EditorLoadout)
            {
                BaseWeaponState weapon = BaseWeaponState.InstantiateState(a.type);
                weapon.machine = this;
                weapon.OnEnter();
                
                Debug.Log("Adding " + weapon.Name + " to loadout.");
                array[_currentLoadoutIndex] = weapon;
                _currentLoadoutIndex++;
            }

            // set our loadout to the dummy
            Loadout = array;
            
            _currentLoadoutIndex = 0;
            Loadout[0].OnWeaponSwitchedTo(new EmptyWeapon());
        }
        
        public void FixedUpdate()
        {
            for (int i = 0; i < Loadout.Length; i++)
            {
                Loadout[i].FixedUpdate();
            }
        }

        public void Update()
        {
            Loadout[_currentLoadoutIndex].AttemptingUse = tryingToFire;
            for (int i = 0; i < Loadout.Length; i++)
            {
                Loadout[i].Update();
            }
           
            if (tryingToFire) {
                //Debug.Log("FUUUUUCK CUBA!");
                Loadout[_currentLoadoutIndex].Primary();
            }
        }

        public bool tryingToFire = false;

        public void Secondary (InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Loadout[_currentLoadoutIndex].Secondary();
            }
        }



        public void UpdateFiringInput(InputAction.CallbackContext context)
        {
            //inputEventData.UpdateKeyState(context);
            if (context.performed)
            {
                tryingToFire = true;             //    isSprinting = true;
            }
            else if (context.canceled)
            {
                tryingToFire = false;            //    isSprinting = false;
            }
        }

        public void LateUpdate()
        {
            for (int i = 0; i < Loadout.Length; i++)
            {
                Loadout[i].LateUpdate();
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < Loadout.Length; i++)
            {
                Loadout[i].OnExit();
            }
        }

        public void Scroll(InputAction.CallbackContext context)
        {
            int y = (int)context.ReadValue<Vector2>().y;
            if (Loadout[_currentLoadoutIndex].CanBeInterrupted())
            {
                if (y > 0)
                {
                    Debug.Log("Scrolling up");

                    int oldIndex = _currentLoadoutIndex;
                    _currentLoadoutIndex = Mathf.Min(_currentLoadoutIndex + 1, Loadout.Length - 1);
                    /* this is mega autistic
                    if (_currentLoadoutIndex + 1 == Loadout.Length)
                    {
                        _currentLoadoutIndex = 0;
                    }
                    else
                    {
                        _currentLoadoutIndex++;
                    }*/

                    Loadout[oldIndex].OnWeaponSwitchedFrom(Loadout[_currentLoadoutIndex]);
                    Loadout[_currentLoadoutIndex].OnWeaponSwitchedTo(Loadout[oldIndex]);

                    //insert network code
                }

                else if (y < 0)
                {
                    Debug.Log("Scrolling down");
                    Debug.Log("Scrolling up");

                    int oldIndex = _currentLoadoutIndex;
                    _currentLoadoutIndex = Mathf.Max(_currentLoadoutIndex - 1, 0);
                    /* this is mega autistic
                    if (_currentLoadoutIndex + 1 == Loadout.Length)
                    {
                        _currentLoadoutIndex = 0;
                    }
                    else
                    {
                        _currentLoadoutIndex++;
                    }*/

                    Loadout[oldIndex].OnWeaponSwitchedFrom(Loadout[_currentLoadoutIndex]);
                    Loadout[_currentLoadoutIndex].OnWeaponSwitchedTo(Loadout[oldIndex]);

                    /*if (_currentLoadoutIndex - 1 == -1)
                    {
                        _currentLoadoutIndex = Loadout.Length - 1;
                    }
                    else
                    {
                        _currentLoadoutIndex--;
                    }*/
                }
            }
        }
    }
}