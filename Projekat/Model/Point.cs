using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekat.Model
{
    public class Point
    {
        private double _x;
        private double _y;

        public Point() { }

        public double X { get => _x; set => _x = value; }

        public double Y { get => _y; set => _y = value; }
    }
}
