using System;
using UnityEngine;

public struct ComponentCache
{
    public Animator animator;
    //public RigBuilder rigBuilder;

    public ComponentCache(GameObject obj)
    {
        animator = obj.GetComponentInChildren<Animator>();
    }
}

namespace Enigmaware.EntityStates
{
    //deez nuts

    public class EntityStateMachine : MonoBehaviour
    {

        public SerializableEntityStateType initialStateType;

        public ComponentCache componentCache;

        public EntityState currentState;
        public EntityState nextState;

        public string machineName;

        public void Awake()
        {
            componentCache = new ComponentCache(gameObject);
            currentState = new Empty();
            currentState.stateMachine = this;
        }

        public void Start()
        {
            if (this.nextState != null)
            {
                this.SetState(this.nextState);
                return;
            }

            Type stateType = this.initialStateType.type;
            if (currentState is Empty && stateType != null && stateType.IsSubclassOf(typeof(EntityState)))
            {
                this.SetState(EntityState.InstantiateState(stateType));
            }
        }

        public bool HasNextState()
        {
            return nextState != null;
        }

        public void SetNextState(EntityState state)
        {
            nextState = state;
        }

        public void SetState(EntityState newState)
        {
            if (newState != null)
            {
                nextState = null;
                newState.stateMachine = this;

                currentState.OnExit();
                currentState = newState;
                currentState.OnEnter();
            }
            else
            {
                Debug.LogWarning($"Tried to go into null state on GameObject {gameObject.name}!");
            }
            //insert network code
        }

        public void SetStateInterrupt(EntityState newState)
        {
            if (currentState.CanBeInterrupted() && newState != null)
            {
                nextState = null;
                newState.stateMachine = this;

                currentState.OnExit();
                currentState = newState;
                currentState.OnEnter();
            }
            else
            {
                Debug.LogWarning($"Tried to interrupt into null state on GameObject {gameObject.name}!");
            }
        }

        public void SetNextStateToNull()
        {
            nextState = EntityState.InstantiateState(typeof(Empty));
        }

        public void FixedUpdate()
        {
            if (nextState != null)
            {
                SetState(nextState);
            }
            currentState.FixedUpdate();
        }

        public void Update()
        {
            currentState.Update();
        }

        public void LateUpdate()
        {
            currentState.LateUpdate();
        }

        private void OnDestroy()
        {
            if (currentState != null)
            {
                currentState.OnExit();
                currentState = null;
            }
        }
    }
}