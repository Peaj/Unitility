//TODO: Add folder selection

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework.Constraints;
using Unity.Entities;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using Object = System.Object;
#if USE_ENTITIES
using Unity.Entities.Editor;
#endif

namespace Unitility.SelectionHistory
{
    [InitializeOnLoad]
    public static class SelectionHistoryManager
    {
        private const Int32 VK_XBUTTON1 = 0x05;
        private const Int32 VK_XBUTTON2 = 0x06;

        private static HistoryBuffer<SelectionSnapshot> history = new HistoryBuffer<SelectionSnapshot>(50);

        private static bool ignoreSelectionChange = false;

        public static HistoryBuffer<SelectionSnapshot> History => history;
        public static SelectionSnapshot Current => History.Current();
        public static bool SelectionIsEmpty => Selection.activeObject == null;
        public static int Size => history?.Size ?? 0;
        public static Action HistoryChanged;
        
        #if USE_ENTITIES
        private static EntitySelectionProxy entitySelectionProxy;
        #endif

        static SelectionHistoryManager()
        {
            #if UNITY_EDITOR_WIN
            EditorApplication.update += GetMouseButtonStates;
            #endif
            Selection.selectionChanged += OnSelectionChanged;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            // EditorApplication.hierarchyChanged += ClearEmptyEntries;
            // EditorApplication.projectChanged += ClearEmptyEntries;

        }

        private static void OnBeforeAssemblyReload()
        {
            SelectionHistoryHelper.SaveSelection(history);
        } 

        private static void OnAfterAssemblyReload()
        {
            history = SelectionHistoryHelper.LoadSelection();
            HistoryChanged?.Invoke();
        }

        #if UNITY_EDITOR_WIN

        [DllImport("user32.dll")]
        public static extern Int16 GetAsyncKeyState(Int32 virtualKeyCode);

        private static void GetMouseButtonStates()
        {
            //Only register mouse button input if unity has focus
            if (!UnityEditorInternal.InternalEditorUtility.isApplicationActive) return;
            
            short backButtonState = GetAsyncKeyState(VK_XBUTTON1);
            short forwardButtonState = GetAsyncKeyState(VK_XBUTTON2);

            if ((backButtonState & 0x01) > 0) Back();
            if ((forwardButtonState & 0x01) > 0) Forward();
            //if ((state & 0x10000000) > 0) Debug.Log("Held!");
        }

        #endif

        private static void OnSelectionChanged()
        {
            if (ignoreSelectionChange)
            {
                ignoreSelectionChange = false;
                return;
            }

            if (SelectionIsEmpty) return;
            history.Push(TakeSnapshot());
            HistoryChanged?.Invoke();
        }

        public static SelectionSnapshot TakeSnapshot()
        {
            var activeObject = Selection.activeObject;
            var objects = Selection.objects;
            
            
            #if USE_ENTITIES
            if (activeObject is EntitySelectionProxy)
            {
                var selectionProxy = activeObject as EntitySelectionProxy;
                var entitySelectionWrapper = ObjectFactory.CreateInstance<EntitySelectionWrapper>();
                entitySelectionWrapper.hideFlags = HideFlags.HideAndDontSave;

                entitySelectionWrapper.EntityIndex =  selectionProxy.Entity.Index;
                entitySelectionWrapper.EntityVersion = selectionProxy.Entity.Version;
                entitySelectionWrapper.WorldName = selectionProxy.World.Name;
                entitySelectionWrapper.name = selectionProxy.EntityManager.GetName(selectionProxy.Entity);

                activeObject = entitySelectionWrapper;
                objects = new[] {activeObject};
            }
            #endif
            
            var selection = new SelectionSnapshot(activeObject, objects, Selection.activeContext);
            
            Debug.Log(selection.ToString());
            return selection;
        }

        public static void Select(SelectionSnapshot snapshot)
        {
            #if USE_ENTITIES
            if (snapshot.ActiveObject is EntitySelectionWrapper)
            {
                var wrapper = snapshot.ActiveObject as EntitySelectionWrapper;
                var proxy = GetEntitySelectionProxy();
                var world = FindWorldByName(wrapper.WorldName);
                proxy.SetEntity(world, new Entity() { Index = wrapper.EntityIndex, Version = wrapper.EntityVersion });
                Selection.SetActiveObjectWithContext(proxy, null);
                Selection.objects = new[] {proxy};
                //TODO: Set selection in Entity Hierarchy Window
                return;
            }
            #endif
            Selection.SetActiveObjectWithContext(snapshot.ActiveObject, snapshot.Context);
            Selection.objects = snapshot.Objects;
        }
        
        #if USE_ENTITIES
        public static World FindWorldByName(string name)
        {
            if (World.All.Count == 0) return null;

            var selectedWorld = World.All[0];
            if (string.IsNullOrEmpty(name)) return selectedWorld;

            foreach (var world in World.All)
            {
                if (world.Name == name) return world;
            }
            return selectedWorld;
        }

        public static EntitySelectionProxy GetEntitySelectionProxy()
        {
            entitySelectionProxy = Resources.FindObjectsOfTypeAll<EntitySelectionProxy>().FirstOrDefault();
            if (entitySelectionProxy != null) return entitySelectionProxy; 
            entitySelectionProxy = ScriptableObject.CreateInstance<EntitySelectionProxy>();
            entitySelectionProxy.hideFlags = HideFlags.HideAndDontSave;
            return entitySelectionProxy;
        }
        #endif

        public static void Select(int index)
        {
            history.SetCurrent(index);
            Select(history.Current());
            HistoryChanged?.Invoke();
        }

        [Shortcut("History/Back", null, KeyCode.Home, ShortcutModifiers.Alt)]
        public static void Back()
        {
            if (history.Size <= 0) return;

            var prev = SelectionIsEmpty?history.Current():history.Previous();

            if (prev.IsEmpty)
            {
                history.Next();
                ClearEmptyEntries();
                Back();
            }

            ignoreSelectionChange = true;
            Select(prev);
            HistoryChanged?.Invoke();
        }

        [Shortcut("History/Forward", null, KeyCode.End, ShortcutModifiers.Alt)]
        public static void Forward()
        {
            if (history.Size <= 0) return;
            
            var next = SelectionIsEmpty?history.Current():history.Next();

            if (next.IsEmpty)
            {
                history.Previous();
                ClearEmptyEntries();
                Forward();
            }

            ignoreSelectionChange = true;
            Select(next);
            HistoryChanged?.Invoke();
        }

        [Shortcut("History/Clear", null)]
        public static void Clear()
        {
            history.Clear();
            HistoryChanged?.Invoke();
        }

        /// <summary>
        /// Makes selection history ignore the next selection
        /// </summary>
        public static void HideSelectionFromHistory()
        {
            ignoreSelectionChange = true;
        }

        /// <summary>
        /// Removes deleted objects from history buffer
        /// </summary>
        private static void ClearEmptyEntries()
        {
            var array = history.ToArray().ToList();
            int current = history.GetCurrentArrayIndex();

            for (int i = array.Count - 1; i >= 0; i--)
            {
                if (array[i].IsEmpty)
                {
                    array.RemoveAt(i);
                    if (current >= i) current--;
                }
            }

            history = HistoryBuffer<SelectionSnapshot>.FromArray(array.ToArray(), current, 50);
        }
    }
}
