using System;
using System.Collections.Generic;
using System.Text;

namespace Toys
{
    //For marking values for saving scene 
    [AttributeUsage(AttributeTargets.Property)]
    class SaveSceneAttribute : Attribute
    {
    }
}
