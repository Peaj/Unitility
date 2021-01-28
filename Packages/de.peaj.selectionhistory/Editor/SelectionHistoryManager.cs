//TODO: Add folder selection
//TODO: Handle deselection
//TODO: Recover from domain reload

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

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

        static SelectionHistoryManager()
        {
            EditorApplication.update += Update;
            Selection.selectionChanged += SelectionChanged;
        }

        private static void Update()
        {
            short backButtonState = GetAsyncKeyState(VK_XBUTTON1);
            short forwardButtonState = GetAsyncKeyState(VK_XBUTTON2);

            if ((backButtonState & 0x01) > 0) Back();
            if ((forwardButtonState & 0x01) > 0) Forward();
            //if ((state & 0x10000000) > 0) Debug.Log("Held!");
        }

        [DllImport("user32.dll")]
        public static extern Int16 GetAsyncKeyState(Int32 virtualKeyCode);

        private static void SelectionChanged()
        {
            if (ignoreSelectionChange)
            {
                ignoreSelectionChange = false;
                return;
            }

            if (Selection.activeObject == null) return;
            history.Push(new SelectionSnapshot());
        }

        [Shortcut("History/Back", null, KeyCode.Home, ShortcutModifiers.Alt)]
        private static void Back()
        {
            var prev = history.Previous();

            if (prev.IsEmpty) //Next selected object has been destroyed
            {
                Back();
                return;
            }

            ignoreSelectionChange = true;
            prev.Select();
        }

        [Shortcut("History/Forward", null, KeyCode.End, ShortcutModifiers.Alt)]
        private static void Forward()
        {
            var next = history.Next();

            if (next.IsEmpty) //Next selected object has been destroyed
            {
                Forward();
                return;
            }

            ignoreSelectionChange = true;
            next.Select();
        }

        [Shortcut("History/Clear", null)]
        private static void Clear()
        {
            history.Clear();
        }
    }
}
