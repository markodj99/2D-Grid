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
        #region Fields

        private double _noviX = 0, _noviY = 0;

        private Dictionary<string, double> _coordinates = new Dictionary<string, double>(4)
        {
            {"maxLat", double.MinValue}, {"maxLon", double.MinValue},
            {"minLat", double.MaxValue}, {"minLon", double.MaxValue}
        };

        private List<SubstationEntity> _substationEntities = new List<SubstationEntity>(67);
        private List<NodeEntity> _nodeEntities = new List<NodeEntity>(2043);
        private List<SwitchEntity> _switchEntities = new List<SwitchEntity>(2282);
        private List<LineEntity> _lineEntities = new List<LineEntity>(2336);

        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

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
            PaintPoints();

            int a = 5;
        }

        private void Substations(XmlDocument xmlDoc)
        {
            
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");

            foreach (XmlNode node in nodeList)
            {
                SubstationEntity sub = new SubstationEntity()
                {
                    Id = long.Parse(node.SelectSingleNode("Id").InnerText),
                    Name = node.SelectSingleNode("Name").InnerText,
                    X = double.Parse(node.SelectSingleNode("X").InnerText),
                    Y = double.Parse(node.SelectSingleNode("Y").InnerText)
                };

                ToLatLon(sub.X, sub.Y, 34, out _noviX, out _noviY);
                sub.X = _noviX;
                sub.Y = _noviY;
                _substationEntities.Add(sub);
            }
        }

        public void Nodes(XmlDocument xmlDoc)
        {
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");

            foreach (XmlNode node in nodeList)
            {
                NodeEntity nodeEntity = new NodeEntity()
                {
                    Id = long.Parse(node.SelectSingleNode("Id").InnerText),
                    Name = node.SelectSingleNode("Name").InnerText,
                    X = double.Parse(node.SelectSingleNode("X").InnerText),
                    Y = double.Parse(node.SelectSingleNode("Y").InnerText)
                };

                ToLatLon(nodeEntity.X, nodeEntity.Y, 34, out _noviX, out _noviY);
                nodeEntity.X = _noviX;
                nodeEntity.Y = _noviY;
                _nodeEntities.Add(nodeEntity);
            }
        }

        public void Switches(XmlDocument xmlDoc)
        {
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");

            foreach (XmlNode node in nodeList)
            {
                SwitchEntity switchobj = new SwitchEntity()
                {
                    Id = long.Parse(node.SelectSingleNode("Id").InnerText),
                    Name = node.SelectSingleNode("Name").InnerText,
                    X = double.Parse(node.SelectSingleNode("X").InnerText),
                    Y = double.Parse(node.SelectSingleNode("Y").InnerText),
                    Status = node.SelectSingleNode("Status").InnerText
                };

                ToLatLon(switchobj.X, switchobj.Y, 34, out _noviX, out _noviY);
                switchobj.X = _noviX;
                switchobj.Y = _noviY;
                _switchEntities.Add(switchobj);
            }
        }

        public void Routes(XmlDocument xmlDoc)
        {
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");

            foreach (XmlNode node in nodeList)
            {
                LineEntity l = new LineEntity()
                {
                    Id = long.Parse(node.SelectSingleNode("Id").InnerText),
                    Name = node.SelectSingleNode("Name").InnerText,
                    IsUnderground = node.SelectSingleNode("IsUnderground").InnerText.Equals("true"),
                    R = float.Parse(node.SelectSingleNode("R").InnerText),
                    ConductorMaterial = node.SelectSingleNode("ConductorMaterial").InnerText,
                    LineType = node.SelectSingleNode("LineType").InnerText,
                    ThermalConstantHeat = long.Parse(node.SelectSingleNode("ThermalConstantHeat").InnerText),
                    FirstEnd = long.Parse(node.SelectSingleNode("FirstEnd").InnerText),
                    SecondEnd = long.Parse(node.SelectSingleNode("SecondEnd").InnerText),
                    Vertices = new List<Point>()
                };

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
            List<double> x = new List<double>(67 + 2043 + 2282 + 2336);
            List<double> y = new List<double>(67 + 2043 + 2282 + 2336);

            foreach (var s in _substationEntities)
            {
                x.Add(s.X);
                y.Add(s.Y);
            }

            foreach (var n in _nodeEntities)
            {
                x.Add(n.X);
                y.Add(n.Y);
            }

            foreach (var s in _switchEntities)
            {
                x.Add(s.X);
                y.Add(s.Y);
            }

            foreach (var l in _lineEntities)
            {
                foreach (var v in l.Vertices)
                {
                    x.Add(v.X);
                    y.Add(v.Y);
                }
            }

            _coordinates["maxLat"] = x.Max();
            _coordinates["maxLon"] = y.Max();
            _coordinates["minLat"] = x.Min();
            _coordinates["minLon"] = y.Min();
        }

        private void PaintPoints()
        {
            //Iz nekog razloga moraju da se zamene X i Y i Height i Width
            double ratioX = (_coordinates["maxLat"] - _coordinates["minLat"]) / OnlyCanvas.Height;
            double ratioY = (_coordinates["maxLon"] - _coordinates["minLon"]) / OnlyCanvas.Width;

            foreach (var s in _substationEntities)
            {
                Rectangle r = new Rectangle()
                {
                    Name = string.Join("", s.Name.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries)),
                    Width = 5,
                    Height = 5,
                    Fill = Brushes.Black,
                    Stroke = Brushes.Blue,
                    ToolTip = new ToolTip()
                    {
                        Content = $"Id:{s.Id} Name:{s.Name}",
                        Foreground = Brushes.Blue
                    }
                };

                Canvas.SetBottom(r, (s.X - _coordinates["minLat"]) / ratioX);
                Canvas.SetLeft(r, (s.Y - _coordinates["minLon"]) / ratioY);

                OnlyCanvas.Children.Add(r);
            }

            foreach (var n in _nodeEntities)
            {
                Rectangle r = new Rectangle()
                {
                    Name = string.Join("", n.Name.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries)),
                    Width = 5,
                    Height = 5,
                    Fill = Brushes.Black,
                    Stroke = Brushes.Red,
                    ToolTip = new ToolTip()
                    {
                        Content = $"Id:{n.Id} Name:{n.Name}",
                        Foreground = Brushes.Red
                    }
                };

                Canvas.SetBottom(r, (n.X - _coordinates["minLat"]) / ratioX);
                Canvas.SetLeft(r, (n.Y - _coordinates["minLon"]) / ratioY);

                OnlyCanvas.Children.Add(r);
            }

            foreach (var s in _switchEntities)
            {
                Rectangle r = new Rectangle()
                {
                    Name = string.Join("", s.Name.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries)),
                    Width = 5,
                    Height = 5,
                    Fill = Brushes.Black,
                    Stroke = Brushes.Green,
                    ToolTip = new ToolTip()
                    {
                        Content = $"Id:{s.Id} Name:{s.Name}",
                        Foreground = Brushes.Green
                    }
                };

                Canvas.SetBottom(r, (s.X - _coordinates["minLat"]) / ratioX);
                Canvas.SetLeft(r, (s.Y - _coordinates["minLon"]) / ratioY);

                OnlyCanvas.Children.Add(r);
            }
        }

        #endregion
    }
}
