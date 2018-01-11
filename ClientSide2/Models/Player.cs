using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSide2.Models
{
    public class Player
    {
        public string Username { get; set; }
        public int NumOfCircelsOut { get; set; }
        public string Color { get; set; }
        public List<Circle> NumOfDeadCircels { get; set; }
        public int NumOfCircelsOnBasis { get; set; }
        public int FirstDice { get; set; }
        public List<int> Turns { get; set; }

        public Player()
        {
            NumOfDeadCircels = new List<Circle>();
            NumOfCircelsOut = 0;
            NumOfCircelsOnBasis = 5;
            Turns = new List<int>();
        }

        public Player(string userName) : this()
        {
            Username = userName;
        }
    }
}
