using System.Collections.Generic;

namespace Projekat.Model
{
    public class LineEntity
    {
        private long _id;
        private string _name;
        private bool _isUnderground;
        private float _r;
        private string _conductorMaterial;
        private string _lineType;
        private long _thermalConstantHeat;
        private long _firstEnd;
        private long _secondEnd;
        private List<Point> _vertices;

        public LineEntity() { }

        public long Id {get => _id; set => _id = value; }

        public string Name { get => _name; set => _name = value; }

        public bool IsUnderground { get => _isUnderground; set => _isUnderground = value; }

        public float R { get => _r; set => _r = value; }

        public string ConductorMaterial { get => _conductorMaterial; set => _conductorMaterial = value; }

        public string LineType { get => _lineType; set => _lineType = value; }

        public long ThermalConstantHeat { get => _thermalConstantHeat; set => _thermalConstantHeat = value; }

        public long FirstEnd { get => _firstEnd; set => _firstEnd = value; }

        public long SecondEnd { get => _secondEnd; set => _secondEnd = value; }

        public List<Point> Vertices { get => _vertices; set => _vertices = value; }
    }
}
