using System.Collections;
using KinematicCharacterController;
using UnityEngine;

namespace CryptidLand.MapSegment
{


    public class Platform : MonoBehaviour, IMoverController
    {
        public float RiseTime;
        public float FallTime;


        private bool _positionDirty;
        [SerializeField]
        private Vector3 _originalPosition;
        private Vector3 _nuPosition;

        public bool Risen => _risen;

        [SerializeField]
        private bool _isMoving;
        [SerializeField]
        private bool _risen;

        private Vector3 _targetPosition = Vector3.zero;
        private Quaternion _targetRotation = Quaternion.identity;



        public void UpdateTargetPosition(float yPosition)
        {
            if (!_isMoving)
            {
                _originalPosition = base.transform.position;

                _nuPosition = new(_originalPosition.x, yPosition, _originalPosition.z);

                _positionDirty = true;
                _isMoving = true;
            }
        }

        public void Reset()
        {
            _positionDirty = true;
        }

        public void Awake()
        {
            _targetPosition = base.transform.position;
            _targetRotation = base.transform.rotation;

            GetComponent<PhysicsMover>().MoverController = this;
        }

        void Update()
        {
            if (_positionDirty)
            {
                _positionDirty = false;
                StartCoroutine(RiseOrFall(!_risen));
            }
        }


        public IEnumerator RiseOrFall(bool rising)
        {
            float t = 0f;
            if (rising)
            {
                while (t <= RiseTime)
                {
                    _targetPosition = Vector3.Lerp(_originalPosition, _nuPosition, t / RiseTime);
                    //base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler( TargetRotation), t/RiseTime); //,// t / RotationSpeed));

                    if (transform.position == _nuPosition)
                    {
                        break;
                    }

                    yield return null;
                    t += Time.deltaTime;
                }

                _targetPosition = _nuPosition;
                _isMoving = false;
                _risen = true;
            }
            else
            {


                while (t < FallTime)
                {
                    t += Time.deltaTime;
                    _targetPosition = Vector3.Lerp(_nuPosition, _originalPosition, t / FallTime);
                    yield return null;
                }

                // straight up set it
                _targetPosition = _originalPosition;
                _isMoving = false;
                _risen = false;
                yield break;
            }
        }

        public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            goalPosition = _targetPosition;
            goalRotation = _targetRotation;

        }
    }
}