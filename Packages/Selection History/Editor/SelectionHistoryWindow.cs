using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unitility.SelectionHistory;
using UnityEditor;
using UnityEngine;

namespace Unitility.SelectionHistory
{
    public class SelectionHistoryWindow : EditorWindow
    {
        [MenuItem("Window/Selection History")]
        static void Init()
        {
            SelectionHistoryWindow window = (SelectionHistoryWindow) GetWindow(typeof(SelectionHistoryWindow));
            window.Show();
        }

        private void OnEnable()
        {
            Selection.selectionChanged += Repaint;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= Repaint;
        }

        void OnGUI()
        {
            var array = SelectionHistoryManager.History.ToArray().Reverse().ToArray();
            var current = SelectionHistoryManager.History.Size - SelectionHistoryManager.History.GetCurrentArrayIndex() - 1;

            for (var i = 0; i < array.Length; i++)
            {
                SelectionSnapshot snapshot = array[i];
                bool isCurrent = i == current;

                if (!snapshot.IsEmpty)
                {
                    GUILayout.Label(snapshot.ActiveObject.name, isCurrent ? EditorStyles.boldLabel : EditorStyles.label);
                }
            }
        }
    }
}
