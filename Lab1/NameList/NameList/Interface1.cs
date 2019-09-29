using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameList
{
    public interface IListaNomes
    {
        void AddName(String name);

        List<String> ListNames();

        void ClearNames();
    }
}
    