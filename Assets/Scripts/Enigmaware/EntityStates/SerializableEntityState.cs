using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Enigmaware.EntityStates
{
    [System.Serializable]
    public class SerializableEntityStateType
    {
        // The type is internally serialized as a string.
        [SerializeField]
        //[HideInInspector]
        public string typeName;

        // Setting or getting the type described by this object is done through this property.
        public System.Type type
        {
            set { typeName = ((value != null) ? value.AssemblyQualifiedName : null); }
            get { return (typeName != null) ? System.Type.GetType(typeName) : null; }
        }

        // Implicit conversion for SerializableEntityStateType -> System.Type.
        public static implicit operator System.Type(SerializableEntityStateType obj)
        {
            return (obj != null) ? obj.type : null;
        }

        // Implicit conversion for System.Type -> SerializableEntityStateType.
        public static implicit operator SerializableEntityStateType(System.Type type)
        {
            if (type == null)
                return null;
            SerializableEntityStateType obj = new SerializableEntityStateType();
            obj.type = type;
            return obj;
        }
    }
}