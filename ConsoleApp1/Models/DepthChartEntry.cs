using ConsoleApp1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Models
{
    public class DepthChartEntry
    {
        public int DepthChartEntryId { get; set; }
        public string Position { get; set; }
        public int PositionDepth { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; }
    }
}
