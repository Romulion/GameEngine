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
        void Awake()
        {
            base.Awake();
        }

        void Update()
        {
            
            KeyboardState kb = Keyboard.GetState();
            if (GLWindow.gLWindow.Focused && kb.IsKeyDown(Key.Up)){
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

            base.Update();
            
        }
    }
}
