using ConsoleApp1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Models
{
    public class Player : IPlayer
    {
        public int PlayerId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
    }
}
