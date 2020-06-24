using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK;

namespace Toys
{
    public class CharacterControllPlayer : CharacterControllBase
    {
        GLWindow game;

        void Awake()
        {
            base.Awake();
            game = GLWindow.gLWindow;
        }

        void Update()
        {
            
            KeyboardState kb = Keyboard.GetState();
            if (game.Focused && kb.IsKeyDown(Key.Up)){
                Walk(CoreEngine.ActiveCore.elapsed);
            }
            else if (GLWindow.gLWindow.Focused && kb.IsKeyDown(Key.Down))
            {
                Walk(-CoreEngine.ActiveCore.elapsed);
            }
            else if (GLWindow.gLWindow.Focused && kb.IsKeyDown(Key.Space))
            {
                Jump();
            }
            else
            {
                Stop();
            }
            
            base.Update();
            
        }

    }
}
