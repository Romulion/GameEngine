using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    public interface ISave
    {
        Dictionary<string, string> SaveSequence(bool extended = false);
        void LoadSequence(Dictionary<string, string> saveData, bool extended = false);
        bool IsInSave { get; set;}
    }
}
