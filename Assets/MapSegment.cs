using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using KinematicCharacterController;
using UnityEngine;

public class MapSegment : MonoBehaviour, IMoverController
{
    public Vector3 TargetRotation;
    
    public float RiseDistance;
 
    public float RiseTime;
    public float FallTime;
    public float RotationTime;

    
    private bool _rotationDirty;
    [SerializeField]
    private Vector3 _originalPosition;
    private Vector3 _nuPosition;
    [SerializeField]
    private bool _isRotating;

    private Vector3 _targetPosition = Vector3.zero;
    private Quaternion _targetRotation = Quaternion.identity;

    
    
    public void UpdateTargetRotation(Vector3 targetRotation)
    {
        if (!_isRotating)
        {
            TargetRotation = targetRotation;
            
            _originalPosition = base.transform.position;
            _nuPosition = _originalPosition + Vector3.up * RiseDistance;
            
            _rotationDirty = true;
            _isRotating = true;
        }
    }

    public void SetRotation(Vector3 rotation)
    {
        base.transform.rotation = Quaternion.Euler(rotation);
    }

    public void Awake()
    {
        _targetPosition = base.transform.position;
        GetComponent<PhysicsMover>().MoverController = this;
    }

    void Update()
    {
        if (_rotationDirty)
        {
            _rotationDirty = false;
            StartCoroutine(RiseThenRotate());
        }
    }

    
    
    // TODO: Sometimes there's a slight pause between rising/rotating/falling
    // 
    public IEnumerator RiseThenRotate()
    {
        float t = 0f;
        while (t < RiseTime)
        {
            t += Time.deltaTime;
            _targetPosition = Vector3.LerpUnclamped(_originalPosition, _nuPosition, t / RiseTime);
            //base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler( TargetRotation), t/RiseTime); //,// t / RotationSpeed));
            
            if (transform.position == _nuPosition)
            {
                break;
            }
            
            yield return null;
        }
        t = 0f;
        
        while (t < RotationTime)
        {
            t += Time.deltaTime;
            //base.transform.position = Vector3.Lerp(_originalPosition, _nuPosition, t / RiseTime);
            _targetRotation = Quaternion.LerpUnclamped(base.transform.rotation, Quaternion.Euler( TargetRotation), t/RotationTime); //,// t / RotationSpeed));
        
            // this stuff is finnicky
            if (transform.rotation.eulerAngles == TargetRotation)
            {
                break;
            }

            yield return null;
        }
        t = 0f;
        
        while (t < FallTime)
        {
            t += Time.deltaTime;
            _targetPosition = Vector3.LerpUnclamped(_nuPosition, _originalPosition, t/FallTime);
            yield return null;
        }
        // straight up set it
        _targetPosition = _originalPosition;
        _targetRotation = Quaternion.Euler(TargetRotation);
        
        _isRotating = false;
        yield break;
    }

    public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
    {
        goalPosition = _targetPosition ;
        goalRotation = _targetRotation;
        
    }
}