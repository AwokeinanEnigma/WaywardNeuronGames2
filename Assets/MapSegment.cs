using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSegment : MonoBehaviour
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
            base.transform.position = Vector3.LerpUnclamped(_originalPosition, _nuPosition, t / RiseTime);
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
            base.transform.rotation = Quaternion.LerpUnclamped(base.transform.rotation, Quaternion.Euler( TargetRotation), t/RotationTime); //,// t / RotationSpeed));
        
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
            base.transform.position = Vector3.Lerp(_nuPosition, _originalPosition, t/FallTime);
            yield return null;
        }

        _isRotating = false;
        yield break;
    }

}