using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    /// <summary>
    /// Common data for not dependent on Scene
    /// </summary>
    public class ShareData
    {
        public SceneNode ScriptHolder { get; private set; }

        public Canvas SharedInterface { get; private set; }

        internal ShareData()
        {
            ScriptHolder = new SceneNode();
            ScriptHolder.Name = "ScriptHolder";
            SharedInterface = new Canvas();
        }
    }
}
