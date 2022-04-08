using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Model
{
    public class PowerEntity
    {
        private long _id;
        private string _name;
        private double _x;
        private double _y;

        public PowerEntity() { }

        public long Id { get => _id; set => _id = value; }

        public string Name { get => _name; set => _name = value; }

        public double X { get => _x; set => _x = value; }

        public double Y { get => _y; set => _y = value; }
    }
}
