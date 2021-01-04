using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nametag_tool
{
    public class NameDataModel
    {
        public int Id{get;set;}
        public string Name { get; set; }
        public NameDataModel() { }
        public NameDataModel(int id, string name) {
            Id = id;
            Name = name;
        }
    }
}
