using Projekat.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using Point = Projekat.Model.Point;

namespace Projekat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double _noviX = 0, _noviY = 0;
        private double _maxX = double.MinValue, _maxY = double.MinValue;
        private double _minX = double.MaxValue, _minY = double.MaxValue;
        private int switchCount = 0;
        private List<SubstationEntity> _substationEntities = new List<SubstationEntity>(67);
        private List<NodeEntity> _nodeEntities = new List<NodeEntity>(2043);
        private List<SwitchEntity> _switchEntities = new List<SwitchEntity>(2282);
        private List<LineEntity> _lineEntities = new List<LineEntity>(2336);

        public MainWindow()
        {
            InitializeComponent();
        }

        #region LoadModel

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("Geographic.xml");

            Substations(xmlDoc);
            Nodes(xmlDoc);
            Switches(xmlDoc);
            Routes(xmlDoc);

            FindMaxMinX_Y();

            TranslatePoints();
        }

        private void Substations(XmlDocument xmlDoc)
        {
            SubstationEntity sub = new SubstationEntity();
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");

            foreach (XmlNode node in nodeList)
            {
                sub.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                sub.Name = node.SelectSingleNode("Name").InnerText;
                sub.X = double.Parse(node.SelectSingleNode("X").InnerText);
                sub.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                ToLatLon(sub.X, sub.Y, 34, out _noviX, out _noviY);
                sub.X = _noviX;
                sub.Y = _noviY;
                sub.X -= 45.0f;
                sub.Y -= 19.0f;
                sub.X *= 1000000.0f;
                sub.Y *= 1000000.0f;
                _substationEntities.Add(sub);
            }
        }

        public void Nodes(XmlDocument xmlDoc)
        {
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            NodeEntity nodeEntity = new NodeEntity();

            foreach (XmlNode node in nodeList)
            {
                nodeEntity.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                nodeEntity.Name = node.SelectSingleNode("Name").InnerText;
                nodeEntity.X = double.Parse(node.SelectSingleNode("X").InnerText);
                nodeEntity.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                ToLatLon(nodeEntity.X, nodeEntity.Y, 34, out _noviX, out _noviY);
                nodeEntity.X = _noviX;
                nodeEntity.Y = _noviY;
                nodeEntity.X -= 45.0f;
                nodeEntity.Y -= 19.0f;
                nodeEntity.X *= 1000000.0f;
                nodeEntity.Y *= 1000000.0f;
                _nodeEntities.Add(nodeEntity);
            }
        }

        public void Switches(XmlDocument xmlDoc)
        {
            SwitchEntity switchobj = new SwitchEntity();
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");

            foreach (XmlNode node in nodeList)
            {
                switchobj.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                switchobj.Name = node.SelectSingleNode("Name").InnerText;
                switchobj.X = double.Parse(node.SelectSingleNode("X").InnerText);
                switchobj.Y = double.Parse(node.SelectSingleNode("Y").InnerText);
                switchobj.Status = node.SelectSingleNode("Status").InnerText;

                ToLatLon(switchobj.X, switchobj.Y, 34, out _noviX, out _noviY);
                switchobj.X = _noviX;
                switchobj.Y = _noviY;
                switchobj.X -= 45.0f;
                switchobj.Y -= 19.0f;
                switchobj.X *= 1000000.0f;
                switchobj.Y *= 1000000.0f;
                _switchEntities.Add(switchobj);
            }
        }

        public void Routes(XmlDocument xmlDoc)
        {
            LineEntity l = new LineEntity();
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");

            foreach (XmlNode node in nodeList)
            {
                l.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                l.Name = node.SelectSingleNode("Name").InnerText;
                l.IsUnderground = node.SelectSingleNode("IsUnderground").InnerText.Equals("true");
                l.R = float.Parse(node.SelectSingleNode("R").InnerText);
                l.ConductorMaterial = node.SelectSingleNode("ConductorMaterial").InnerText;
                l.LineType = node.SelectSingleNode("LineType").InnerText;
                l.ThermalConstantHeat = long.Parse(node.SelectSingleNode("ThermalConstantHeat").InnerText);
                l.FirstEnd = long.Parse(node.SelectSingleNode("FirstEnd").InnerText);
                l.SecondEnd = long.Parse(node.SelectSingleNode("SecondEnd").InnerText);
                l.Vertices = new List<Point>();

                foreach (XmlNode pointNode in node.ChildNodes[9].ChildNodes)
                {
                    Point p = new Point
                    {
                        X = double.Parse(pointNode.SelectSingleNode("X").InnerText),
                        Y = double.Parse(pointNode.SelectSingleNode("Y").InnerText)
                    };

                    ToLatLon(p.X, p.Y, 34, out _noviX, out _noviY);
                    p.X = _noviX;
                    p.Y = _noviY;
                    p.X -= 45.0f;
                    p.Y -= 19.0f;
                    p.X *= 1000000.0f;
                    p.Y *= 1000000.0f;

                    l.Vertices.Add(p);
                }

                _lineEntities.Add(l);
            }
        }

        public void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }

        public void FindMaxMinX_Y()
        {
            foreach (var s in _substationEntities)
            {
                if (s.X > _maxX) _maxX = s.X;
                if (s.Y > _maxY) _maxY = s.Y;
                if (s.X < _minX) _minX = s.X;
                if (s.Y < _minY) _minY = s.Y;
            }

            foreach (var n in _nodeEntities)
            {
                if (n.X > _maxX) _maxX = n.X;
                if (n.Y > _maxY) _maxY = n.Y;
                if (n.X < _minX) _minX = n.X;
                if (n.Y < _minY) _minY = n.Y;
            }

            foreach (var s in _switchEntities)
            {
                if (s.X > _maxX) _maxX = s.X;
                if (s.Y > _maxY) _maxY = s.Y;
                if (s.X < _minX) _minX = s.X;
                if (s.Y < _minY) _minY = s.Y;
            }

            foreach (var v in _lineEntities.SelectMany(s => s.Vertices))
            {
                if (v.X > _maxX) _maxX = v.X;
                if (v.Y > _maxY) _maxY = v.Y;
                if (v.X < _minX) _minX = v.X;
                if (v.Y < _minY) _minY = v.Y;
            }
        }

        private void TranslatePoints()
        {
            _maxX -= _minX;
            _maxY -= _minY;

            double ratioX = OnlyCanvas.Width / _maxX;
            double ratioY = OnlyCanvas.Height / _maxY;

            foreach (var s in _substationEntities)
            {
                Rectangle r = new Rectangle()
                {
                    Width = 5, Height = 5, Fill = Brushes.Black, Stroke = Brushes.Red, Name = $"Switch{++switchCount}",
                    ToolTip = $"{Name}", 
                };

                s.X -= _minX;
                s.Y -= _minY;

                Canvas.SetTop(r, s.Y * ratioY);
                Canvas.SetLeft(r, s.X * ratioX);

                OnlyCanvas.Children.Add(r);
            }
        }

        #endregion
    }
}
