using UnityEditor;
using UnityEngine;

namespace Unitility.SelectionHistory
{
    public class SelectionSnapshot
    {
        public readonly Object ActiveObject;
        public readonly Object[] Objects;
        public readonly Object Context;

        public bool IsEmpty => this.ActiveObject == null;

        public SelectionSnapshot()
        {
            this.ActiveObject = Selection.activeObject;
            this.Objects = Selection.objects;
            this.Context = Selection.activeContext;
        }

        public SelectionSnapshot(Object activeObject, Object[] objects, Object context = null)
        {
            this.ActiveObject = activeObject;
            this.Objects = objects;
            this.Context = context;
        }

        public void Select()
        {
            Selection.SetActiveObjectWithContext(this.ActiveObject, this.Context);
            Selection.objects = this.Objects;
        }

        public override string ToString()
        {
            return $"Active: {this.ActiveObject} Objects: {this.Objects.Length} Context: {this.Context}";
        }
    }
}