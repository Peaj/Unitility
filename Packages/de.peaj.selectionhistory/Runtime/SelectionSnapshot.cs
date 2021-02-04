using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unitility.SelectionHistory
{
    #if USE_ENTITIES
    public class EntitySelectionWrapper : ScriptableObject
    {
        public int EntityIndex;
        public int EntityVersion;
        public string WorldName;
    }
    #endif
    
    [Serializable]
    public class SelectionSnapshot
    {
        public Object ActiveObject;
        public Object[] Objects;
        public Object Context;

        public bool IsEmpty => this.ActiveObject == null;

        public SelectionSnapshot(Object activeObject, Object[] objects, Object context = null)
        {
            this.ActiveObject = activeObject;
            this.Objects = objects;
            this.Context = context;
        }

        public override string ToString()
        {
            return $"Active: {this.ActiveObject} Objects: {this.Objects.Length} Context: {this.Context}";
        }
    }
}