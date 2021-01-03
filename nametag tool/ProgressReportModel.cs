using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nametag_tool
{
    class ProgressReportModel
    {
        public List<ItemDataModel> Items { get; set; } = new List<ItemDataModel>();
        public int PercentageComplete { get; set; } = 0;
    }
}
