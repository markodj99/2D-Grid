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
    public enum Direction : int
    {
        UP = 0, 
        DOWN = 1, 
        LEFT = 2, 
        RIGHT = 3
    }

    public class QueueItem
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public List<KeyValuePair<int, int>> Path { get; set; }

        public QueueItem()
        {
            Row = Col = 0;
            Path = new List<KeyValuePair<int, int>>();
        }

        public QueueItem(int row, int col, List<KeyValuePair<int, int>> path)
        {
            Row = row; Col = col; Path = path;
        }
    }

    public partial class MainWindow : Window
    {
        #region Fields

        private bool _isLoaded = false;
        private List<List<Rectangle>> _grid;
        private int _dims1, _dims2;
        private double _noviX = 0, _noviY = 0;

        private Dictionary<long,KeyValuePair<int, int>> _printedElements = new Dictionary<long, KeyValuePair<int, int>>(67 + 2043 + 2282);

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
            InitializeLayover();
        }

        private void InitializeLayover()
        {
            _dims1 = (int)(OnlyCanvas.Height / 5);
            _dims2 = (int)(OnlyCanvas.Width / 5);

            _grid = new List<List<Rectangle>>();
            for (int i = 0; i < _dims1; i++)
            {
                _grid.Add(new List<Rectangle>(_dims2));
                for (int j = 0; j < _dims2; j++)
                {
                    _grid[i].Add(null);
                }
            }
        }

        #endregion

        #region LoadModel

        private void MenuItem_LoadModel(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("Geographic.xml");

                Substations(xmlDoc);
                Nodes(xmlDoc);
                Switches(xmlDoc);
                Routes(xmlDoc);

                FindMaxMinX_Y();

                PrintRectangles();
                PrintConnections();
            }

            _isLoaded = true;
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

        //sredi metodu
        private void PrintRectangles()
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

                var coordinates = FindCoordinates((int) ((s.X - _coordinates["minLat"]) / ratioX),
                    (int) ((s.Y - _coordinates["minLon"]) / ratioY), r);

                Canvas.SetBottom(r, coordinates.Key);
                Canvas.SetLeft(r, coordinates.Value);

                OnlyCanvas.Children.Add(r);
                _printedElements.Add(s.Id, new KeyValuePair<int, int>(coordinates.Key, coordinates.Value));
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

                var coordinates = FindCoordinates((int)((n.X - _coordinates["minLat"]) / ratioX),
                    (int)((n.Y - _coordinates["minLon"]) / ratioY), r);

                Canvas.SetBottom(r, coordinates.Key);
                Canvas.SetLeft(r, coordinates.Value);

                OnlyCanvas.Children.Add(r);
                _printedElements.Add(n.Id, new KeyValuePair<int, int>(coordinates.Key, coordinates.Value));
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

                var coordinates = FindCoordinates((int)((s.X - _coordinates["minLat"]) / ratioX),
                    (int)((s.Y - _coordinates["minLon"]) / ratioY), r);

                Canvas.SetBottom(r, coordinates.Key);
                Canvas.SetLeft(r, coordinates.Value);

                OnlyCanvas.Children.Add(r);
                _printedElements.Add(s.Id, new KeyValuePair<int, int>(coordinates.Key, coordinates.Value));
            }
        }

        private KeyValuePair<int, int> FindCoordinates(int bottom, int left, Rectangle r)
        {
            int b = RoundCoordinate(bottom, true), l = RoundCoordinate(left, false);
            int i = b / 5, j = l / 5;

            if (i == _grid.Count) i--;
            if (j == _grid[0].Count) j--;

            if (_grid[i][j] == null)
            {
                _grid[i][j] = r;
                return new KeyValuePair<int, int>(i * 5, j * 5);
            }

            Dictionary<string, int> boundries = new Dictionary<string, int>(4)
            {
                {"start_I", i - 20 > 0 ? i - 20 : i},
                {"finish_I", i - 20 > 0 ? i : i + 20},
                {"start_J", j - 20 > 0 ? j - 20 : j},
                {"finish_J", j - 20 > 0 ? j : j + 20}
            };

            for (int first = boundries["start_I"]; first < boundries["finish_I"]; first++)
            {
                for (int second = boundries["start_J"]; second < boundries["finish_J"]; second++)
                {
                    if (_grid[first][second] != null) continue;
                    i = first;
                    j = second;
                    break;
                }
            }

            _grid[i][j] = r;
            return new KeyValuePair<int, int>(i * 5, j * 5);
        }

        private int RoundCoordinate(int coordinate, bool bottom)
        {
            if (bottom)
            {
                switch (coordinate % 5)
                {
                    case 1:
                        if (coordinate - 1 < 0) return 0;
                        else return coordinate - 1;
                    case 2:
                        if (coordinate - 2 < 0) return 0;
                        else return coordinate - 2;
                    case 3:
                        if (coordinate + 2 > (int)OnlyCanvas.Height) return (int)OnlyCanvas.Height;
                        else return coordinate + 2;
                    case 4:
                        if (coordinate + 1 > (int)OnlyCanvas.Height) return (int)OnlyCanvas.Height;
                        else return coordinate + 1;
                    default: return coordinate;
                }
            }

            switch (coordinate % 5)
            {
                case 1:
                    if (coordinate - 1 < 0) return 0;
                    else return coordinate - 1;
                case 2:
                    if (coordinate - 2 < 0) return 0;
                    else return coordinate - 2;
                case 3:
                    if (coordinate + 2 > (int)OnlyCanvas.Width) return (int)OnlyCanvas.Width;
                    else return coordinate + 2;
                case 4:
                    if (coordinate + 1 > (int)OnlyCanvas.Width) return (int)OnlyCanvas.Width;
                    else return coordinate + 1;
                default: return coordinate;
            }
        }

        private void PrintConnections()
        {
            foreach (var l in _lineEntities)
            {
                long first = l.FirstEnd, second = l.SecondEnd;

                if (!_printedElements.ContainsKey(first)) continue;
                if (!_printedElements.ContainsKey(second)) continue;

                List<KeyValuePair<int, int>> path = BFS(_printedElements[first].Key / 5, _printedElements[first].Value / 5,
                    _printedElements[second].Key / 5, _printedElements[second].Value / 5, 
                    _grid[_printedElements[second].Key / 5][_printedElements[second].Value / 5].Name);


                // dodaj za sledecu iteraciju
                if (path.Count == 0) continue;

                for (int i = 1; i < path.Count; i++)
                {
                    Direction direction = DetermineDirection(i, path, _printedElements[second].Key / 5, _printedElements[second].Value / 5);
                    Line lineSegment;

                    switch (direction)
                    {
                        case Direction.UP:
                            lineSegment = new Line()
                            {
                                X1 = 2.5,
                                Y1 = 5,
                                X2 = 2.5,
                                Y2 = 0,
                                Stroke = Brushes.DarkOrchid,
                                StrokeThickness = 1,
                                ToolTip = new ToolTip()
                                {
                                    Content = $"Id:{l.Id} Name:{l.Name}",
                                    Foreground = Brushes.DarkOrchid
                                }
                            };
                            break;
                        case Direction.DOWN:
                            lineSegment = new Line()
                            {
                                X1 = 2.5,
                                Y1 = 0,
                                X2 = 2.5,
                                Y2 = 5,
                                Stroke = Brushes.DarkOrchid,
                                StrokeThickness = 1,
                                ToolTip = new ToolTip()
                                {
                                    Content = $"Id:{l.Id} Name:{l.Name}",
                                    Foreground = Brushes.DarkOrchid
                                }
                            };
                            break;
                        case Direction.LEFT:
                            lineSegment = new Line()
                            {
                                X1 = 5,
                                Y1 = 2.5,
                                X2 = 0,
                                Y2 = 2.5,
                                Stroke = Brushes.DarkOrchid,
                                StrokeThickness = 1,
                                ToolTip = new ToolTip()
                                {
                                    Content = $"Id:{l.Id} Name:{l.Name}",
                                    Foreground = Brushes.DarkOrchid
                                }
                            };
                            break;
                        case Direction.RIGHT:
                            lineSegment = new Line()
                            {
                                X1 = 0,
                                Y1 = 2.5,
                                X2 = 5,
                                Y2 = 2.5,
                                Stroke = Brushes.DarkOrchid,
                                StrokeThickness = 1,
                                ToolTip = new ToolTip()
                                {
                                    Content = $"Id:{l.Id} Name:{l.Name}",
                                    Foreground = Brushes.DarkOrchid
                                }
                            };
                            break;
                        default:
                            lineSegment = new Line();
                            break;
                    }

                    Rectangle r = new Rectangle()
                    {
                    };

                    _grid[path[i].Key][path[i].Value] = r;

                    Canvas.SetBottom(lineSegment, path[i].Key * 5);
                    Canvas.SetLeft(lineSegment, path[i].Value * 5);

                    OnlyCanvas.Children.Add(lineSegment);
                }
            }
        }

        private List<KeyValuePair<int, int>> BFS(int sRow, int sCol, int dRow, int dCol, string target)
        {
            QueueItem source = new QueueItem(sRow, sCol, new List<KeyValuePair<int, int>>());

            bool[,] visited = new bool[_dims1, _dims2];
            for (int i = 0; i < _dims1; i++)
            {
                for (int j = 0; j < _dims2; j++)
                {
                    visited[i, j] = _grid[i][j] != null;
                }
            }
            visited[dRow, dCol] = false;

            Queue<QueueItem> q = new Queue<QueueItem>();
            q.Enqueue(source);
            visited[source.Row, source.Col] = true;
            while (q.Count != 0)
            {
                QueueItem p = q.Dequeue();

                if (_grid[p.Row][p.Col] != null) if (_grid[p.Row][p.Col].Name.Equals(target)) return p.Path;

                if (p.Row - 1 >= 0 && visited[p.Row - 1, p.Col] == false)
                {
                    p.Path.Add(new KeyValuePair<int, int>(p.Row, p.Col));
                    q.Enqueue(new QueueItem(p.Row - 1, p.Col, new List<KeyValuePair<int, int>>(p.Path)));
                    visited[p.Row - 1, p.Col] = true;
                }

                if (p.Row + 1 < _dims1 && visited[p.Row + 1, p.Col] == false)
                {
                    p.Path.Add(new KeyValuePair<int, int>(p.Row, p.Col));
                    q.Enqueue(new QueueItem(p.Row + 1, p.Col, new List<KeyValuePair<int, int>>(p.Path)));
                    visited[p.Row + 1, p.Col] = true;
                }

                if (p.Col - 1 >= 0 && visited[p.Row, p.Col - 1] == false)
                {
                    p.Path.Add(new KeyValuePair<int, int>(p.Row, p.Col));
                    q.Enqueue(new QueueItem(p.Row, p.Col - 1, new List<KeyValuePair<int, int>>(p.Path)));
                    visited[p.Row, p.Col - 1] = true;
                }

                if (p.Col + 1 < _dims2 && visited[p.Row, p.Col + 1] == false)
                {
                    p.Path.Add(new KeyValuePair<int, int>(p.Row, p.Col));
                    q.Enqueue(new QueueItem(p.Row, p.Col + 1, new List<KeyValuePair<int, int>>(p.Path)));
                    visited[p.Row, p.Col + 1] = true;
                }
            }

            return new List<KeyValuePair<int, int>>();
        }

        private Direction DetermineDirection(int pos, List<KeyValuePair<int, int>> path, int lRow, int lCol)
        {
            if (pos + 1 == path.Count)
            {
                if (path[pos].Key == lRow) return lCol > path[pos].Value ? Direction.RIGHT : Direction.LEFT;
                return lRow > path[pos].Key ? Direction.UP : Direction.DOWN;
            }

            if (path[pos + 1].Key == path[pos].Key) return path[pos + 1].Value > path[pos].Value ? Direction.RIGHT : Direction.LEFT;
            return path[pos + 1].Key > path[pos].Key ? Direction.UP : Direction.DOWN;
        }

        #endregion
    }
}