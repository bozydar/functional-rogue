using System;
using Ruminate.DataStructures;

namespace Ruminate.GUI.Framework
{

    // Handles/triggers all of the mouse or keyboard events. State management occurs 
    // asynchronously while triggers are handled synchronously.
    internal class InputManager
    {
        public event CharEnteredHandler CharEntered;
        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyUp;
        public event MouseEventHandler MouseDoubleClick;
        public event MouseEventHandler MouseDown;
        public event MouseEventHandler MouseHover;
        public event MouseEventHandler MouseUp;
        public event MouseEventHandler MouseWheel;

        private readonly Root<Widget> _dom;

        // When ever these Elements are not not null they are in the state
        // specified by their names. Used to trigger events and by the element
        // class to see what state the element is in. 
        private Widget _hoverElement;
        internal Widget HoverElement
        {
            get
            {
                return _hoverElement;
            }
            private set
            {
                if (value == _hoverElement) { return; }
                if (value != null) { value.EnterHover(); }
                if (_hoverElement != null) { _hoverElement.ExitHover(); }
                _hoverElement = value;
            }
        }

        //TODO: Combine into type
        private Widget _pressedElement;
        internal Widget PressedElement
        {
            get
            {
                return _pressedElement;
            }
            private set
            {
                if (value == _pressedElement) { return; }
                if (value != null) { value.EnterPressed(); }
                if (_pressedElement != null) { _pressedElement.ExitPressed(); }
                _pressedElement = value;
            }
        }

        private Widget _focusedElement;
        internal Widget FocusedElement
        {
            get
            {
                return _focusedElement;
            }
            private set
            {
                if (value == _focusedElement) { return; }
                if (value != null) { value.EnterFocus(); }
                if (_focusedElement != null) { _focusedElement.ExitFocus(); }
                _focusedElement = value;
            }
        }

        private static InputManager _currentInstance;

        protected virtual void OnMouseMove(object sender, MouseEventArgs eventArgs)
        {
            HoverElement = null;
            foreach (var child in _dom.Children)
            {
                FindHover(child);
            }

            if (PressedElement == null) { return; }

            if (!PressedElement.AbsoluteInputArea.Contains(InputSystem.MouseLocation))
            {
                PressedElement = null;
            }
            if (FocusedElement != null)
            {
                FocusedElement.MouseMove(eventArgs);
            }
        }

        protected virtual void OnMouseUp(Object o, MouseEventArgs e)
        {
            if (PressedElement != null)
            {
                PressedElement.MouseClick(e);
            }
            PressedElement = null;
            if (FocusedElement != null)
            {
                FocusedElement.MouseUp(e);
            }
            if (MouseUp != null)
            {
                MouseUp(this, e);
            }
        }

        protected virtual void OnMouseDown(Object o, MouseEventArgs e)
        {
            if (HoverElement == null)
            {
                return;
            }

            FocusedElement = HoverElement;
            PressedElement = HoverElement;
            FocusedElement.MouseDown(e);
            if (MouseDown != null)
            {
                MouseDown(this, e);
            }
        }

        protected virtual void OnDoubleClick(Object o, MouseEventArgs e)
        {
            if (HoverElement == null)
            {
                return;
            }

            FocusedElement = HoverElement;
            PressedElement = HoverElement;
            FocusedElement.MouseDoubleClick(e);
            if (MouseDoubleClick != null)
            {
                MouseDoubleClick(this, e);
            }
        }

        protected virtual void OnCharEntered(Object o, CharacterEventArgs e)
        {
            if (FocusedElement != null)
            {
                FocusedElement.CharEntered(e);
            }
            if (CharEntered != null)
            {
                CharEntered(this, e);
            }
        }

        protected virtual void OnKeyDown(Object o, KeyEventArgs e)
        {
            if (FocusedElement != null)
            {
                FocusedElement.KeyDown(e);
            }
            if (KeyDown != null)
            {
                KeyDown(this, e);
            }
        }

        protected virtual void OnKeyUp(Object o, KeyEventArgs e)
        {
            if (FocusedElement != null)
            {
                FocusedElement.KeyUp(e);
            }
            if (KeyUp != null)
            {
                KeyUp(this, e);
            }
        }

        protected virtual void OnMouseHover(Object o, MouseEventArgs e)
        {
            if (FocusedElement != null)
            {
                FocusedElement.MouseHover(e);
            }
            if (MouseHover != null)
            {
                MouseHover(this, e);
            }
        }

        protected virtual void OnMouseWheel(Object o, MouseEventArgs e)
        {
            if (FocusedElement != null)
            {
                FocusedElement.MouseWheel(e);
            }
            if (MouseWheel != null)
            {
                MouseWheel(this, e);
            }
        }

        internal InputManager(Root<Widget> dom)
        {
            _dom = dom;

            InputSystem.MouseMove = OnMouseMove;
            InputSystem.MouseUp = OnMouseUp;
            InputSystem.MouseDown = OnMouseDown;
            InputSystem.MouseDoubleClick = OnDoubleClick;
            InputSystem.CharEntered = OnCharEntered;
            InputSystem.KeyDown = OnKeyDown;
            InputSystem.KeyUp = OnKeyUp;
            InputSystem.MouseHover = OnMouseHover;
            InputSystem.MouseWheel = OnMouseWheel;

            _currentInstance = this;
        }

        // Finds the element the mouse is currently being hovered over.
        private void FindHover(TreeNode<Widget> node)
        {

            if (!node.Data.AbsoluteArea.Contains(InputSystem.MouseLocation))
            {
                return;
            }

            if (node.Parent.Data != null && !node.Parent.Data.AbsoluteInputArea.Contains(InputSystem.MouseLocation))
            {
                return;
            }

            if (!node.Data.Active || !node.Data.Visible)
            {
                return;
            }

            HoverElement = node.Data;

            foreach (var child in node.Children)
            {
                FindHover(child);
            }
        }
    }
}