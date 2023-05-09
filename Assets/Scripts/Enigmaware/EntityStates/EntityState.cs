using System;
using UnityEngine;

namespace Enigmaware.EntityStates
{


        public class EntityState
        {
            /// <summary>
            /// The state machine that is executing this state.
            /// </summary>
            public EntityStateMachine stateMachine;

            /// <summary>
            /// The game object that the state is running on.
            /// </summary>
            public virtual GameObject gameObject { get { return stateMachine.gameObject; } }

            /// <summary>
            /// The transform of the game object that the state is running on
            /// </summary>
            public virtual Transform transform { get { return stateMachine.transform; } }

            /// <summary>
            /// Deltatime float
            /// </summary>
            protected float age;
            /// <summary>
            /// FixedDeltaTime float
            /// </summary>
            protected float fixedAge;
            /// <summary>
            /// When this state is first ran.
            /// </summary>
            public virtual void OnEnter() { }
            /// <summary>
            /// When this state is over
            /// </summary>
            public virtual void OnExit() { }
            /// <summary>
            /// Executed every second
            /// </summary>
            public virtual void Update() { age += Time.deltaTime; }
            /// <summary>
            /// Executed every frame
            /// </summary>
            public virtual void FixedUpdate() { fixedAge += Time.fixedDeltaTime; }

            public virtual void LateUpdate() {  }


            public static EntityState InstantiateState(Type stateType)
            {
                if (stateType != null && stateType.IsSubclassOf(typeof(EntityState)))
                {
                    return Activator.CreateInstance(stateType) as EntityState;
                }
                return null;
            }

            public virtual bool CanBeInterrupted()
            {
                return true;
            }
        }

    }
