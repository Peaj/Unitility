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
        private SelectionSnapshot[] history;
        private int current;
        
        [MenuItem("Window/Selection History")]
        static void Init()
        {
            SelectionHistoryWindow window = (SelectionHistoryWindow) GetWindow(typeof(SelectionHistoryWindow));
            window.Show();
        }

        private void OnEnable()
        {
            Refresh();
            SelectionHistoryManager.HistoryChanged += Refresh;
        }

        private void OnDisable()
        {
            SelectionHistoryManager.HistoryChanged -= Refresh;
        }

        private void Refresh()
        {
            this.history = SelectionHistoryManager.History.ToArray().Reverse().ToArray();
            this.current = SelectionHistoryManager.History.Size - SelectionHistoryManager.History.GetCurrentArrayIndex() - 1;
            Repaint();
        }

        void OnGUI()
        {
            Refresh();

            for (var i = 0; i < this.history.Length; i++)
            {
                SelectionSnapshot snapshot = this.history[i];
                bool isCurrent = i == current && !SelectionHistoryManager.SelectionIsEmpty;

                if (!snapshot.IsEmpty)
                {
                    GUILayout.Label(snapshot.ActiveObject.name, isCurrent ? EditorStyles.boldLabel : EditorStyles.label);
                }
            }
        }
    }
}
