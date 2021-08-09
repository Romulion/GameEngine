using System;
using System.Collections.Generic;
using System.Text;

namespace Toys
{
    //Class user for marking values to save
    [AttributeUsage(AttributeTargets.Property)]
    class SaveAttribute : Attribute
    {
    }
}
