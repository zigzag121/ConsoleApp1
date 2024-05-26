using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Interfaces
{
    public interface IPlayer
    {
        int PlayerId { get; set; }
        int Number { get; set; }
        string Name { get; set; }
    }
}
