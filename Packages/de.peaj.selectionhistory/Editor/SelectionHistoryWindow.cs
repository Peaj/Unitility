using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unitility.SelectionHistory;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Unitility.SelectionHistory
{
    public class SelectionHistoryWindow : EditorWindow
    {
        private static class Styles
        {
            public static GUIStyle LineStyle = (GUIStyle) "TV Line";
            public static GUIStyle LineBoldStyle = (GUIStyle) "TV LineBold";
            public static GUIStyle SelectionStyle = (GUIStyle) "TV Selection";
            public static GUIStyle CountBadge = (GUIStyle) "CN CountBadge";
        }
        
        private SelectionSnapshot[] history;
        private int current;

        private Vector2 scrollPosition;
        
        [MenuItem("Window/Selection History")]
        static void Init()
        {
            SelectionHistoryWindow window = (SelectionHistoryWindow) GetWindow(typeof(SelectionHistoryWindow));
            window.titleContent = new GUIContent("Selection History");
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

            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            for (var i = 0; i < this.history.Length; i++)
            {
                SelectionSnapshot snapshot = this.history[i];
                bool isCurrent = i == current && !SelectionHistoryManager.SelectionIsEmpty;
                if (!snapshot.IsEmpty) DrawSnapshot(snapshot, isCurrent);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawSnapshot(SelectionSnapshot snapshot, bool selected)
        {
            Rect rowRect = EditorGUILayout.GetControlRect(
                hasLabel: true,
                height: EditorGUIUtility.singleLineHeight, 
                style: EditorStyles.label);

            float paddingLeft = 5f;
            float iconSize = 16f;
            
            rowRect.x -= 2f;
            rowRect.width += 4f;
            
            if (Event.current.type == UnityEngine.EventType.Repaint)
            {
                if (selected) Styles.SelectionStyle.Draw(rowRect, false, false, true, true);

                var contentRect = rowRect;
                contentRect.xMin += paddingLeft;

                GUIContent content = EditorGUIUtility.ObjectContent(snapshot.ActiveObject, snapshot.ActiveObject.GetType());
                var icon = GetMiniThumbnail(snapshot.ActiveObject);

                var iconRect = contentRect;
                iconRect.width = iconSize;
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

                var labelRect = contentRect;
                labelRect.xMin += iconSize + 2f;
                labelRect.yMin += 1f;

                Styles.LineStyle.Draw(labelRect, snapshot.ActiveObject.name, false, false, selected, true);

                int count = snapshot.Objects.Length;
                if (count > 1) //Show count badge
                {
                    var countRect = contentRect;
                    countRect.xMin += countRect.width-27f;
                    countRect.yMin += 1;
                    Styles.CountBadge.Draw(countRect, new GUIContent($"+{count - 1}"), 0);
                }
            }
        }

        private static Texture2D GetMiniThumbnail(Object obj)
        {
            if ((bool) obj) return AssetPreview.GetMiniThumbnail(obj);
            else return AssetPreview.GetMiniTypeThumbnail(obj.GetType());
        }
    }
}
