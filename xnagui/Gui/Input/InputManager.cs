using System;
using Ruminate.DataStructures;

namespace Ruminate.GUI.Framework {

    // Handles/triggers all of the mouse or keyboard events. State management occurs 
    // asynchronously while triggers are handled synchronously.
    internal class InputManager {

        private readonly Root<Widget> _dom;

        // When ever these Elements are not not null they are in the state
        // specified by their names. Used to trigger events and by the element
        // class to see what state the element is in. 
        private Widget _hoverElement;
        internal Widget HoverElement {
            get {
                return _hoverElement;
            } private set {
                if (value == _hoverElement) { return; }
                if (value != null) { value.EnterHover(); } 
                if (_hoverElement != null) { _hoverElement.ExitHover(); }
                _hoverElement = value;
            }
        }

        //TODO: Combine into type
        private Widget _pressedElement;
        internal Widget PressedElement {
            get {
                return _pressedElement;
            } private set {
                if (value == _pressedElement) { return; }
                if (value != null) { value.EnterPressed(); }
                if (_pressedElement != null) { _pressedElement.ExitPressed(); }
                _pressedElement = value;
            }
        }

        private Widget _focusedElement;
        internal Widget FocusedElement {
            get {
                return _focusedElement;
            } private set {
                if (value == _focusedElement) { return; }
                if (value != null) { value.EnterFocus(); }
                if (_focusedElement != null) { _focusedElement.ExitFocus(); }
                _focusedElement = value;
            }
        }

        private static InputManager _currentInstance;

        internal InputManager(Root<Widget> dom) {
            _dom = dom;

            /* ## Input Events to Manage Internal State ## */
            #region Manage Internal State

            InputSystem.MouseMove += delegate {

                HoverElement = null;
                foreach (var child in _dom.Children) { FindHover(child); }
                if (PressedElement == null) { return; }

                if (!PressedElement.AbsoluteInputArea.Contains(InputSystem.MouseLocation)) {
                    PressedElement = null;
                }
            };

            InputSystem.MouseUp += delegate(Object o, MouseEventArgs e) {

                if (PressedElement != null) {
                    PressedElement.MouseClick(e);
                }

                PressedElement = null;
            };

            InputSystem.MouseDown += delegate {

                if (HoverElement == null) { return; }

                FocusedElement = HoverElement;
                PressedElement = HoverElement;
            };

            InputSystem.MouseDoubleClick += delegate {

                if (HoverElement == null) { return; }

                FocusedElement = HoverElement;
                PressedElement = HoverElement;
            };
            #endregion

            /* ## Input Events to fire the focused element's event handlers ## */
            #region Element Triggers

            InputSystem.CharEntered += delegate(Object o, CharacterEventArgs e) {
                if (FocusedElement != null) {
                    FocusedElement.CharEntered(e);
                }
            };

            InputSystem.KeyDown += delegate(Object o, KeyEventArgs e) {
                if (FocusedElement != null) {
                    FocusedElement.KeyDown(e);
                }
            };

            InputSystem.KeyUp += delegate(Object o, KeyEventArgs e) {
                if (FocusedElement != null) {
                    FocusedElement.KeyUp(e);
                }
            };

            InputSystem.MouseDoubleClick += delegate(Object o, MouseEventArgs e) {
                if (FocusedElement != null) {
                    FocusedElement.MouseDoubleClick(e);
                }
            };

            InputSystem.MouseDown += delegate(Object o, MouseEventArgs e) {
                if (FocusedElement != null) {
                    FocusedElement.MouseDown(e);
                }
            };

            InputSystem.MouseHover += delegate(Object o, MouseEventArgs e) {
                if (FocusedElement != null) {
                    FocusedElement.MouseHover(e);
                }
            };

            InputSystem.MouseMove += delegate(Object o, MouseEventArgs e) {
                if (FocusedElement != null) {
                    FocusedElement.MouseMove(e);
                }
            };

            InputSystem.MouseUp += delegate(Object o, MouseEventArgs e) {
                if (FocusedElement != null) {
                    FocusedElement.MouseUp(e);
                }                
            };

            InputSystem.MouseWheel += delegate(Object o, MouseEventArgs e) {
                if (FocusedElement != null) {
                    FocusedElement.MouseWheel(e);
                }
            };

            #endregion

            _currentInstance = this;
        }

        // Finds the element the mouse is currently being hovered over.
        private void FindHover(TreeNode<Widget> node) {

            if (!node.Data.AbsoluteArea.Contains(InputSystem.MouseLocation)) {
                return;
            }

            if (node.Parent.Data != null && !node.Parent.Data.AbsoluteInputArea.Contains(InputSystem.MouseLocation)) {
                return;
            }

            if (!node.Data.Active || !node.Data.Visible) {
                return;
            }

            HoverElement = node.Data;            
           
            foreach (var child in node.Children) {
                FindHover(child);
            }
        }
    }
}