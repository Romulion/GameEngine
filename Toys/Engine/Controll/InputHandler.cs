using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common;

namespace Toys
{
    /// <summary>
    /// proceen input ascii only
    /// </summary>
    class InputHandler
    {
        readonly char[] asciiTable =  { '~', '-', '+', '<', '>', ';', '\'', ',' , '^', '/', '\\'};
        readonly char[] altAsciiTable = { '`', '-', '=', ',', '>', ';', '\'', ',', '^', '/', '\\' };
        readonly char[] altnum = { ')', '!', '@', '#', '$', '%', '^', '&', '*', '(' };

        List<Keys> blockedKeys = new List<Keys>();

        TextInputComponent inputContext;
        KeyboardState keyboardState;
        GLWindow windowHandler;
        StringBuilder textBuffer;
        bool remove;
        bool release;
        public InputHandler()
        {
            windowHandler = GLWindow.gLWindow;
            windowHandler.TextInput += ProcessInputEvent;
            windowHandler.KeyDown += WindowHandler_KeyDown;
            textBuffer = new StringBuilder(10);
        }

        internal TextInputComponent SetTextInputContext
        {
            get
            {
                return inputContext;
            }
            set
            {
                ReleaseContext();
                inputContext = value;
            }
        }

        internal void Update()
        {
            keyboardState = GLWindow.gLWindow.KeyboardState;
            ProcessInput();
        }

        void ProcessInput()
        {
            if (inputContext)
            {
                if (release)
                {
                    ReleaseContext();
                    release = false;
                }
                else if (remove && inputContext.Text.Text.Length > 0)
                {
                    inputContext.Text.SetText(inputContext.Text.Text.Substring(0, inputContext.Text.Text.Length - 1));
                    remove = false;
                }
                else if (textBuffer.Length > 0)
                    inputContext.Text.SetText(inputContext.Text.Text + textBuffer.ToString());
            }

            textBuffer.Clear();
        }
        /*
                void ProcessInput()
                {
                    //dont perform check if co active input on window is not focused
                    if (!inputContext || !GLWindow.gLWindow.Focused)
                        return;

                    //unfocus on escape
                    if (keyboardState.IsKeyDown(Key.Escape) || keyboardState.IsKeyDown(Key.Enter))
                    {
                        ReleaseContext();
                        return;
                    }


                    bool remove = false;
                    StringBuilder text = new StringBuilder();
                    //unblock released key
                    for (int i = 0; i < blockedKeys.Count; i++)
                        if (keyboardState.IsKeyUp(blockedKeys[i]))
                            blockedKeys.Remove(blockedKeys[i]);

                    if (keyboardState.IsKeyDown(Key.BackSpace) && !blockedKeys.Contains(Key.BackSpace))
                    {
                        remove = true;
                        blockedKeys.Add(Key.BackSpace);
                    }

                    //scan a-z
                    for (Key a = Key.A; a <= Key.Z; a++)
                        if (keyboardState.IsKeyDown(a) && !blockedKeys.Contains(a)) {
                            blockedKeys.Add(a);
                            if (keyboardState.IsKeyDown(Key.ShiftLeft) || keyboardState.IsKeyDown(Key.ShiftRight))
                                text.Append(a.ToString());
                            else 
                                text.Append(a.ToString().ToLower());
                        }

                    //scan 0-9
                    for (Key a = Key.Number0; a <= Key.Number9; a++)
                        if (keyboardState.IsKeyDown(a) && !blockedKeys.Contains(a))
                        {
                            blockedKeys.Add(a);
                            if (keyboardState.IsKeyDown(Key.ShiftLeft) || keyboardState.IsKeyDown(Key.ShiftRight))
                                text.Append(altnum[(int)a - 109]);
                            else 
                                text.Append((int)a-109);
                        }

                    //scan -+=_/
                    for (Key a = Key.Tilde; a <= Key.NonUSBackSlash; a++)
                        if (keyboardState.IsKeyDown(a) && !blockedKeys.Contains(a))
                        {
                            blockedKeys.Add(a);
                            text.Append(asciiTable[(int)a - 119]);
                        }

                    //scan space
                    if (keyboardState.IsKeyDown(Key.Space) && !blockedKeys.Contains(Key.Space))
                    {
                        blockedKeys.Add(Key.Space);
                        text.Append(" ");
                    }

                    if (remove && inputContext.Text.Text.Length > 0)
                        inputContext.Text.SetText(inputContext.Text.Text.Substring(0, inputContext.Text.Text.Length - 1));
                    else if (text.Length > 0)
                        inputContext.Text.SetText(inputContext.Text.Text + text);
                }
        */

        
        private void ProcessInputEvent(TextInputEventArgs e)
        {
            if (inputContext)
                textBuffer.Append(e.AsString);
        }

       

        private void WindowHandler_KeyDown(KeyboardKeyEventArgs e)
        {
            if (!inputContext)
                return;

            if (e.Key == Keys.Backspace)
                remove = true;
            else if (e.Key == Keys.Enter || e.Key == Keys.Escape)
                release = true;
        }
        void ReleaseContext()
        {
            blockedKeys.Clear();
            inputContext?.Deactivate();
            inputContext = null;
        }
    }
}
