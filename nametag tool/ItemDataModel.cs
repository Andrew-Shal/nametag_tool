using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nametag_tool
{
    public class ItemDataModel
    {
        public string Name { get; set; }
        public string FilePath { get; set; }

        public ItemDataModel(string name, string filePath) {
            Name = name;
            FilePath = filePath;
        }
    }
}
