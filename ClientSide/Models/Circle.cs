using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSide
{
    public class Circle
    {
        public string Color { get; set; }
        public bool IsAlive { get; set; }
        public bool IsSelected { get; set; }
        public bool NeedToRemove { get; set; }

        public Circle()
        {
            IsAlive = true;
            IsSelected = false;
        }
    }

    
}
