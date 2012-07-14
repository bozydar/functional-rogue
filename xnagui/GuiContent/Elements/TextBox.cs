using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Ruminate.GUI.Framework;
using Timer = System.Timers.Timer;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Ruminate.GUI.Content {

    /// <summary>
    /// Plain text box.
    /// </summary>
    public class TextBox : WidgetBase<TextBoxRenderRule>, IDisposable {

        /*####################################################################*/
        /*                               Variables                            */
        /*####################################################################*/

        #region Variables

        /* ## Local Variables ## */

        private readonly Timer _cursorTimer;
        private void ResetTimer() {

            _isPressed = false;
            RenderRule.CursorVisible = true;
            _cursorTimer.Interval = 500;
        }

        private bool _isPressed;

        /* ## Exposed Variables ## */

        public int MaxCharactors { get; set; }
        public int Padding { get; set; }

        #endregion

        /*####################################################################*/
        /*                           Initialization                           */
        /*####################################################################*/

        #region Node Initialization

        protected override TextBoxRenderRule BuildRenderRule() {
            return new TextBoxRenderRule(500);
        }

        /// <summary>
        /// Creates a new textbox that automatically fills the parent element.
        /// </summary>
        /// <param name="buffer">Padding between the textbox and the parent Element.</param>
        /// <param name="maxChars"></param>
        public TextBox(int buffer, short maxChars) {

            Padding = buffer;

            MaxCharactors = maxChars;

            _cursorTimer = new Timer(500);
            _cursorTimer.Elapsed += delegate {
                RenderRule.CursorVisible = !RenderRule.CursorVisible;
            };
            _cursorTimer.Start();            
        }

        public void Dispose() {
            _cursorTimer.Dispose();
        }

        protected override void Attach() {
            var a = Parent.AbsoluteInputArea;
            Area = new Rectangle(0, 0, a.Width, a.Height);
        }

        public override void Layout() { }

        #endregion

        /*####################################################################*/
        /*                                Events                              */
        /*####################################################################*/

        #region Events

        protected override void CharEntered(CharacterEventArgs e) {

            var ch = e.Character;

            lock (RenderRule) {
                switch (ch) {
                    case '\b': {
                        //Test[x]
                        //Backspace <= remove selected or remove before cursor
                        if (RenderRule.HasSelected) {
                            RenderRule.RemoveSelected();
                        } else {
                            RenderRule.BackSpace();
                        }
                        break;
                    } case '\n': case '\r': {
                        //Test[x]
                        //New Line <= Insert character after cursor                        
                        if (RenderRule.HasSelected) { RenderRule.RemoveSelected(); }
                        RenderRule.Insert(e.Character);
                        break;                    
                    } case '\t': { 
                        //Tabs currently not supported
                        return;
                    } case (char)3: {
                        //Test[x]
                        //Copy                        
                        Clipboard.SetDataObject(RenderRule.GetSelected(), true);
                        break;                    
                    } case (char)22: {
                        //Test[x]
                        //Paste
                        if (RenderRule.HasSelected) { RenderRule.RemoveSelected(); }

                        var dataObject = Clipboard.GetDataObject();
                        if (dataObject != null) {
                            var text = dataObject.GetData(DataFormats.Text).ToString();
                            RenderRule.Insert(text);
                        }
                        break;
                    } case (char)24: {
                        //Test[x]
                        //Cut
                        if (RenderRule.HasSelected) {
                            Clipboard.SetDataObject(RenderRule.GetSelected(), true);
                            RenderRule.RemoveSelected();
                        }
                        break;
                    } default: { 
                        //Test[x]
                        //Add the character
                        if (RenderRule.HasSelected) { RenderRule.RemoveSelected(); }
                        RenderRule.Insert(e.Character);
                        break;
                    }
                }
                RenderRule.BakeText();
            }

            ResetTimer();
        }

        protected override void MouseDoubleClick(MouseEventArgs e) {
            MouseDown(e);
        }

        protected override void MouseDown(MouseEventArgs e) {

            _isPressed = true;

            lock (RenderRule) {
                RenderRule.SetSelection(
                    new Point(
                        e.X - RenderRule.Area.X,
                        e.Y - RenderRule.Area.Y));
                RenderRule.SetTextCursor(
                    new Point(
                        e.X - RenderRule.Area.X,
                        e.Y - RenderRule.Area.Y));
            }
        }

        protected override void MouseMove(MouseEventArgs e) {

            if (!_isPressed) { return; }

            lock (RenderRule) {
                RenderRule.SetTextCursor(
                    new Point(
                        e.Location.X - RenderRule.Area.X,
                        e.Location.Y - RenderRule.Area.Y));
            }
        }

        protected override void MouseUp(MouseEventArgs e) {
            
            _isPressed = false;

            lock (RenderRule) {
                RenderRule.SetTextCursor(
                    new Point(
                        e.Location.X - RenderRule.Area.X,
                        e.Location.Y - RenderRule.Area.Y));
            }
        }

        protected override void KeyDown(KeyEventArgs e) {

            lock (RenderRule) {
                if (RenderRule.Length != 0) {
                    switch (e.KeyCode) {
                        case Keys.Left: {
                                RenderRule.TextCursor--;
                                break;
                        } case Keys.Right: {
                                RenderRule.TextCursor++;
                                break;
                        } case Keys.Up: {
                                RenderRule.CursorUp();
                                break;
                        } case Keys.Down: {
                                RenderRule.CursorDown();
                                break;
                        } case Keys.Home: {
                                RenderRule.CursorHome();
                                break;
                        } case Keys.End: {
                                RenderRule.CursorEnd();
                                break;
                        } case Keys.PageUp: {
                                RenderRule.TextCursor = 0;
                                break;
                        } case Keys.PageDown: {
                                RenderRule.TextCursor = RenderRule.Length;
                                break;
                        } case Keys.Delete: {
                                RenderRule.Delete();
                                RenderRule.BakeText();
                                break;
                        }
                    }
                }
            }

            ResetTimer();
        }

        #endregion

        protected override void Update() { }
    }
}