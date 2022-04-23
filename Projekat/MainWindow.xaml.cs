using Projekat.Model;
using Projekat.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using Brushes = System.Windows.Media.Brushes;
using Point = Projekat.Model.Point;
using Projekat.Utils;
using Projekat.Utils.Enums;

namespace Projekat
{
    //TODO: Da mogu da se izmene nacrtani objekti, eventualno tekst
    public partial class MainWindow : Window
    {
        #region Fields

        #region GridFields

        private bool _isLoaded = false;
        private List<List<Rectangle>> _grid;
        private int _dims1, _dims2;
        private double _noviX = 0, _noviY = 0;
        private Dictionary<long, Tuple<long, long>> _drawnLines = new Dictionary<long, Tuple<long, long>>();
        private Dictionary<long, Tuple<long, long, long, string>> _secondIteration = new Dictionary<long, Tuple<long, long, long, string>>();

        private Dictionary<long, Tuple<int, int>> _drawnObjects = new Dictionary<long, Tuple<int, int>>(67 + 2043 + 2282);

        private Dictionary<string, double> _coordinates = new Dictionary<string, double>(4)
        {
            {"maxLat", double.MinValue}, {"maxLon", double.MinValue},
            {"minLat", double.MaxValue}, {"minLon", double.MaxValue}
        };

        private List<SubstationEntity> _substationEntities = new List<SubstationEntity>(67);
        private List<NodeEntity> _nodeEntities = new List<NodeEntity>(2043);
        private List<SwitchEntity> _switchEntities = new List<SwitchEntity>(2282);
        private List<LineEntity> _lineEntities = new List<LineEntity>(2336);

        public static Brush _color = Brushes.BurlyWood;

        #endregion

        #region ShapeFields

        private SelectedShape _selectedShape = SelectedShape.None;
        private System.Windows.Point _curretnPoint;
        private List<System.Windows.Point> _polygonPoints = new List<System.Windows.Point>();

        public static EllipseShape Ellipse = new EllipseShape();
        public static PolygonShape Polygon = new PolygonShape();
        public static TextShape Text = new TextShape();

        private Stack<UIElement> _redo = new Stack<UIElement>(20);
        private Stack<UIElement> _undo = new Stack<UIElement>(20);
        private Stack<UIElement> _drawnUIElements = new Stack<UIElement>(20);

        private bool _flag = false;
        private int _count = 0;
        private int _id = 0;

        #endregion

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

                DrawObjects();
                DrawConnections();
            }

            _isLoaded = true;
        }

        #region ParseXML

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

        private void Nodes(XmlDocument xmlDoc)
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

        private void Switches(XmlDocument xmlDoc)
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

        private void Routes(XmlDocument xmlDoc)
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

        private void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
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

        private void FindMaxMinX_Y()
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

        #endregion

        #region DrawObjects

        private void DrawObjects()
        {
            //Iz nekog razloga moraju da se zamene X i Y i Height i Width
            double ratioX = (_coordinates["maxLat"] - _coordinates["minLat"]) / OnlyCanvas.Height;
            double ratioY = (_coordinates["maxLon"] - _coordinates["minLon"]) / OnlyCanvas.Width;

            DrawSubstations(ratioX, ratioY);
            DrawNodes(ratioX, ratioY);
            DrawSwitches(ratioX, ratioY);
        }

        private void DrawSubstations(double ratioX, double ratioY)
        {
            foreach (var s in _substationEntities)
            {
                Rectangle r = new Rectangle()
                {
                    Name = $"Substation{s.Id}",
                    Width = 5,
                    Height = 5,
                    Fill = Brushes.Blue,
                    Stroke = Brushes.Blue,
                    ToolTip = new ToolTip()
                    {
                        Content = $"Substation Entity\nId: {s.Id}\nName: {s.Name}",
                        Foreground = Brushes.Blue
                    }
                };

                var coordinates = FindCoordinates((int)((s.X - _coordinates["minLat"]) / ratioX),
                    (int)((s.Y - _coordinates["minLon"]) / ratioY), r);

                Canvas.SetBottom(r, coordinates.Key);
                Canvas.SetLeft(r, coordinates.Value);

                OnlyCanvas.Children.Add(r);
                _drawnObjects.Add(s.Id, new Tuple<int, int>(coordinates.Key, coordinates.Value));
            }
        }

        private void DrawNodes(double ratioX, double ratioY)
        {
            foreach (var n in _nodeEntities)
            {
                Rectangle r = new Rectangle()
                {
                    Name = $"Node{n.Id}",
                    Width = 5,
                    Height = 5,
                    Fill = Brushes.Red,
                    Stroke = Brushes.Red,
                    ToolTip = new ToolTip()
                    {
                        Content = $"Node Entity\nId: {n.Id}\nName: {n.Name}",
                        Foreground = Brushes.Red
                    }
                };

                var coordinates = FindCoordinates((int)((n.X - _coordinates["minLat"]) / ratioX),
                    (int)((n.Y - _coordinates["minLon"]) / ratioY), r);

                Canvas.SetBottom(r, coordinates.Key);
                Canvas.SetLeft(r, coordinates.Value);

                OnlyCanvas.Children.Add(r);
                _drawnObjects.Add(n.Id, new Tuple<int, int>(coordinates.Key, coordinates.Value));
            }
        }

        private void DrawSwitches(double ratioX, double ratioY)
        {

            foreach (var s in _switchEntities)
            {
                Rectangle r = new Rectangle()
                {
                    Name = $"Switch{s.Id}",
                    Width = 5,
                    Height = 5,
                    Fill = Brushes.Green,
                    Stroke = Brushes.Green,
                    ToolTip = new ToolTip()
                    {
                        Content = $"Switch Entity\nId: {s.Id}\nName: {s.Name}",
                        Foreground = Brushes.Green
                    }
                };

                var coordinates = FindCoordinates((int)((s.X - _coordinates["minLat"]) / ratioX),
                    (int)((s.Y - _coordinates["minLon"]) / ratioY), r);

                Canvas.SetBottom(r, coordinates.Key);
                Canvas.SetLeft(r, coordinates.Value);

                OnlyCanvas.Children.Add(r);
                _drawnObjects.Add(s.Id, new Tuple<int, int>(coordinates.Key, coordinates.Value));
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

        #endregion

        #region DrawConnections

        private void DrawConnections()
        {
            int counter = 0;

            FirstIteration(ref counter);
            SecondIteration(ref counter);
        }

        public void FirstIteration(ref int counter)
        {
            foreach (var l in _lineEntities)
            {
                long first = l.FirstEnd, second = l.SecondEnd;

                if (!_drawnObjects.ContainsKey(first)) continue;
                if (!_drawnObjects.ContainsKey(second)) continue;

                if (_drawnLines.ContainsValue(new Tuple<long, long>(first, second))
                   || _drawnLines.ContainsValue(new Tuple<long, long>(second, first))) continue;

                var path = BFS(_drawnObjects[first].Item1 / 5, _drawnObjects[first].Item2 / 5,
                    _drawnObjects[second].Item1 / 5, _drawnObjects[second].Item2 / 5,
                    _grid[_drawnObjects[second].Item1 / 5][_drawnObjects[second].Item2 / 5].Name);

                if (path.Count == 0)
                {
                    _secondIteration.Add(counter++, new Tuple<long, long, long, string>(first, second, l.Id, l.Name));
                    continue;
                }

                string firstName = _grid[_drawnObjects[first].Item1 / 5][_drawnObjects[first].Item2 / 5].Name;
                string secondName = _grid[_drawnObjects[second].Item1 / 5][_drawnObjects[second].Item2 / 5].Name;

                Polyline line = new Polyline()
                {
                    Name = $"{firstName}_{secondName}",
                    Stroke = Brushes.Purple,
                    ToolTip = new ToolTip()
                    {
                        Content = $"Line Entity\nId: {l.Id}\nName:{l.Name}",
                        Foreground = Brushes.Purple
                    }
                };
                PointCollection col = new PointCollection();

                foreach (var k in path)
                {
                    col.Add(new System.Windows.Point(k.Value * 5.0f + 2.5f, OnlyCanvas.Height - k.Key * 5.0f - 2.5f));
                    if (_grid[k.Key][k.Value] == null)
                    {
                        _grid[k.Key][k.Value] = new Rectangle()
                        {
                            Name = "Line_ada"
                        };
                    }
                }
                line.Points = col;
                line.StrokeThickness = 1;
                line.MouseRightButtonDown += ChangeColor;

                OnlyCanvas.Children.Add(line);

                _drawnLines.Add(counter++, new Tuple<long, long>(first, second));
            }
        }

        private void SecondIteration(ref int counter)
        {
            foreach (var t in _secondIteration.Values)
            {
                long first = t.Item1, second = t.Item2;

                if (!_drawnObjects.ContainsKey(first)) continue;
                if (!_drawnObjects.ContainsKey(second)) continue;

                if (_drawnLines.ContainsValue(new Tuple<long, long>(first, second))
                    || _drawnLines.ContainsValue(new Tuple<long, long>(second, first))) continue;

                var path = BFS(_drawnObjects[first].Item1 / 5, _drawnObjects[first].Item2 / 5,
                    _drawnObjects[second].Item1 / 5, _drawnObjects[second].Item2 / 5,
                    _grid[_drawnObjects[second].Item1 / 5][_drawnObjects[second].Item2 / 5].Name, true);

                if (path.Count == 0) continue;

                string firstName = _grid[_drawnObjects[first].Item1 / 5][_drawnObjects[first].Item2 / 5].Name;
                string secondName = _grid[_drawnObjects[second].Item1 / 5][_drawnObjects[second].Item2 / 5].Name;

                Polyline line = new Polyline()
                {
                    Name = $"{firstName}_{secondName}",
                    Stroke = Brushes.CornflowerBlue,
                    ToolTip = new ToolTip()
                    {
                        Content = $"Line Entity\nId: {t.Item3}\nName: {t.Item4}",
                        Foreground = Brushes.CornflowerBlue
                    },
                };
                PointCollection col = new PointCollection();

                var intersection = new List<Tuple<Rectangle, double, double>>(20);

                col.Add(new System.Windows.Point(path[0].Value * 5.0f + 2.5f, OnlyCanvas.Height - path[0].Key * 5.0f - 2.5f));
                for (int i  = 1; i < path.Count - 1; i++)
                {
                    col.Add(new System.Windows.Point(path[i].Value * 5.0f + 2.5f, OnlyCanvas.Height - path[i].Key * 5.0f - 2.5f));
                    if (_grid[path[i].Key][path[i].Value] == null)
                    {
                        _grid[path[i].Key][path[i].Value] = new Rectangle()
                        {
                            Name = "Line_ada"
                        };
                    }
                    else
                    {
                        OnlyCanvas.Children.Remove(_grid[path[i].Key][path[i].Value]);

                        Rectangle r = new Rectangle()
                        {
                            Name = "Intersection",
                            Width = 2,
                            Height = 2,
                            Fill = Brushes.Black,
                            Stroke = Brushes.Black,
                            ToolTip = new ToolTip()
                            {
                                Content = "Intersection",
                                Foreground = Brushes.Black
                            }
                        };
                        intersection.Add(new Tuple<Rectangle, double, double>(r, path[i].Key * 5 + 1.5f, path[i].Value * 5 + 1.5f));
                        _grid[path[i].Key][path[i].Value] = r;
                    }
                }
                col.Add(new System.Windows.Point(path[path.Count - 1].Value * 5.0f + 2.5f, OnlyCanvas.Height - path[path.Count - 1].Key * 5.0f - 2.5f));

                line.Points = col;
                line.StrokeThickness = 1;
                line.MouseRightButtonDown += ChangeColor;

                OnlyCanvas.Children.Add(line);

                foreach (var v in intersection)
                {
                    Canvas.SetBottom(v.Item1, v.Item2);
                    Canvas.SetLeft(v.Item1, v.Item3);

                    OnlyCanvas.Children.Add(v.Item1);
                }

                _drawnLines.Add(counter++, new Tuple<long, long>(first, second));
            }
        }

        private List<KeyValuePair<int, int>> BFS(int sRow, int sCol, int dRow, int dCol, string target, bool secondIteration = false)
        {
            QueueItem source = new QueueItem(sRow, sCol, new List<KeyValuePair<int, int>>());

            bool[,] visited = new bool[_dims1, _dims2];
            if (secondIteration)
            {
                for (int i = 0; i < _dims1; i++)
                {
                    for (int j = 0; j < _dims2; j++)
                    {
                        if (_grid[i][j] != null)
                        {
                            if (_grid[i][j].Name.StartsWith("Line"))
                            {
                                visited[i, j] = false;
                            }
                            else
                            {
                                visited[i, j] = true;
                            }
                        }
                        else
                        {
                            visited[i, j] = false;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < _dims1; i++)
                {
                    for (int j = 0; j < _dims2; j++)
                    {
                        if (_grid[i][j] == null)
                        {
                            visited[i, j] = false;
                        }
                        else
                        {
                            visited[i, j] = true;
                        }
                    }
                }
            }

            visited[dRow, dCol] = false;

            Queue<QueueItem> q = new Queue<QueueItem>();
            q.Enqueue(source);
            visited[source.Row, source.Col] = true;
            while (q.Count != 0)
            {
                QueueItem p = q.Dequeue();
                p.Path.Add(new KeyValuePair<int, int>(p.Row, p.Col));

                if (_grid[p.Row][p.Col] != null) if (_grid[p.Row][p.Col].Name.Equals(target)) return p.Path;

                if (p.Row - 1 >= 0 && visited[p.Row - 1, p.Col] == false)
                {
                    q.Enqueue(new QueueItem(p.Row - 1, p.Col, new List<KeyValuePair<int, int>>(p.Path)));
                    visited[p.Row - 1, p.Col] = true;
                }

                if (p.Row + 1 < _dims1 && visited[p.Row + 1, p.Col] == false)
                {
                    q.Enqueue(new QueueItem(p.Row + 1, p.Col, new List<KeyValuePair<int, int>>(p.Path)));
                    visited[p.Row + 1, p.Col] = true;
                }

                if (p.Col - 1 >= 0 && visited[p.Row, p.Col - 1] == false)
                {
                    q.Enqueue(new QueueItem(p.Row, p.Col - 1, new List<KeyValuePair<int, int>>(p.Path)));
                    visited[p.Row, p.Col - 1] = true;
                }

                if (p.Col + 1 < _dims2 && visited[p.Row, p.Col + 1] == false)
                {
                    q.Enqueue(new QueueItem(p.Row, p.Col + 1, new List<KeyValuePair<int, int>>(p.Path)));
                    visited[p.Row, p.Col + 1] = true;
                }
            }

            return new List<KeyValuePair<int, int>>();
        }

        #endregion

        private void ChangeColor(object sender, MouseButtonEventArgs e)
        {
            string firstName = "", secondName = "";
            if (e.Source is Polyline l)
            {
                string[] s = l.Name.Split('_');
                firstName = s[0];
                secondName = s[1];
            }

            ColorPicker c = new ColorPicker();
            c.ShowDialog();
            if (_color == null) return;
            for (int i = 0; i < OnlyCanvas.Children.Count; i++)
            {
                if (!(OnlyCanvas?.Children[i] is Rectangle)) continue;
                if (((Rectangle)OnlyCanvas.Children[i]).Name.Equals(firstName))
                {
                    ((Rectangle)OnlyCanvas.Children[i]).Fill = _color;
                    ((Rectangle)OnlyCanvas.Children[i]).Stroke = _color;
                }

                if (((Rectangle) OnlyCanvas.Children[i]).Name.Equals(secondName))
                {
                    ((Rectangle)OnlyCanvas.Children[i]).Fill = _color;
                    ((Rectangle)OnlyCanvas.Children[i]).Stroke = _color;
                }
            }
        }

        #endregion

        #region Shapes

        #region Elipse

        private void BtnEllipse_Checked(object sender, RoutedEventArgs e)
        {
            if (BtnPolygon.IsChecked == true || BtnText.IsChecked == true) BtnEllipse.IsChecked = false;
            else
            {
                _selectedShape = SelectedShape.Ellipse;
                _polygonPoints.Clear();
            }
        }

        private void BtnEllipse_Unchecked(object sender, RoutedEventArgs e)
        {
            _selectedShape = SelectedShape.None;
        }

        private void DrawEllipse()
        {
            if (!Ellipse.Condition) return;

            Ellipse ellipse = new Ellipse
            {
                Name = $"Ellipse{_id++}",
                Width = Ellipse.RadiusX,
                Height = Ellipse.RadiusY,
                Fill = Ellipse.Fill,
                Stroke = Ellipse.Border,
                StrokeThickness = Ellipse.Conture
            };

            ellipse.MouseLeftButtonDown += EditEllipse;

            Canvas.SetTop(ellipse, _curretnPoint.Y);
            Canvas.SetLeft(ellipse, _curretnPoint.X);

            OnlyCanvas.Children.Add(ellipse);

            BtnEllipse.IsChecked = false;
            _selectedShape = SelectedShape.None;

            _drawnUIElements.Push(ellipse);
        }

        private void EditEllipse(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is Ellipse el)) return;
            EllipseWindow ellipseWindow = new EllipseWindow(el.Width, el.Height,
                (int) el.StrokeThickness, el.Fill.ToString(), el.Stroke.ToString());
            ellipseWindow.ShowDialog();

            if (!Ellipse.Condition) return;
            for (int i = OnlyCanvas.Children.Count - 1; i >= 0; i--)
            {
                if (!(OnlyCanvas?.Children[i] is Ellipse)) continue;
                if (!((Ellipse) OnlyCanvas.Children[i]).Name.Equals(el.Name)) continue;
                ((Ellipse)OnlyCanvas.Children[i]).Fill = Ellipse.Fill;
                ((Ellipse)OnlyCanvas.Children[i]).Stroke = Ellipse.Border;
                ((Ellipse)OnlyCanvas.Children[i]).StrokeThickness = Ellipse.Conture;
            }

            foreach (var v in _drawnUIElements)
            {
                if (!(v is Ellipse ellipse)) continue;
                if (!(ellipse.Name.Equals(el.Name))) continue;
                ellipse.Fill = Ellipse.Fill;
                ellipse.Stroke = Ellipse.Border;
                ellipse.StrokeThickness = Ellipse.Conture;
            }
        }

        #endregion

        #region PolygonWindow

        private void BtnPolygon_Checked(object sender, RoutedEventArgs e)
    {
        if (BtnEllipse.IsChecked == true || BtnText.IsChecked == true) BtnPolygon.IsChecked = false;
        else
        {
            _selectedShape = SelectedShape.Polygon;
            _polygonPoints.Clear();
        }
    }

        private void BtnPolygon_Unchecked(object sender, RoutedEventArgs e)
        {
            _selectedShape = SelectedShape.None;
        }

        private void DrawPolygon()
        {
            if (!Polygon.Condition) return;

            Polygon polygon = new Polygon()
            {
                Name = $"Polygon{_id++}",
                Fill = Polygon.Fill,
                Stroke = Polygon.Border,
                StrokeThickness = Polygon.Conture
            };

            foreach (var p in _polygonPoints) polygon.Points.Add(p);

            polygon.MouseLeftButtonDown += EditPolygon;

            OnlyCanvas.Children.Add(polygon);

            _polygonPoints.Clear();
            BtnPolygon.IsChecked = false;
            _selectedShape = SelectedShape.None;

            _drawnUIElements.Push(polygon);
        }

        private void EditPolygon(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is Polygon p)) return;
            PolygonWindow polygonWindow = new PolygonWindow((int) p.StrokeThickness, p.Fill, p.Stroke);
            polygonWindow.ShowDialog();

            if (!Polygon.Condition) return;
            for (int i = OnlyCanvas.Children.Count - 1; i >= 0; i--)
            {
                if (!(OnlyCanvas?.Children[i] is Polygon)) continue;
                if (!((Polygon)OnlyCanvas.Children[i]).Name.Equals(p.Name)) continue;
                ((Polygon)OnlyCanvas.Children[i]).Fill = Polygon.Fill;
                ((Polygon)OnlyCanvas.Children[i]).Stroke = Polygon.Border;
                ((Polygon)OnlyCanvas.Children[i]).StrokeThickness = Polygon.Conture;
            }

            foreach (var v in _drawnUIElements)
            {
                if (!(v is Polygon polygon)) continue;
                if (!(polygon.Name.Equals(p.Name))) continue;
                polygon.Fill = Polygon.Fill;
                polygon.Stroke = Polygon.Border;
                polygon.StrokeThickness = Polygon.Conture;
            }
        }

        #endregion

        #region TextWindow

        private void BtnText_Checked(object sender, RoutedEventArgs e)
        {
            if (BtnPolygon.IsChecked == true || BtnEllipse.IsChecked == true) BtnText.IsChecked = false;
            else
            {
                _selectedShape = SelectedShape.Text;
                _polygonPoints.Clear();
            }
        }

        private void BtnText_Unchecked(object sender, RoutedEventArgs e)
        {
            _selectedShape = SelectedShape.None;
        }

        private void PrintText()
        {
            if (!Text.Condition) return;

            TextBox textBox = new TextBox()
            {
                Name = $"Text{_id++}",
                Text = Text.Text,
                FontSize = Text.Font,
                Foreground = Text.Foreground,
                Background = Text.Background,
                TextAlignment = TextAlignment.Center,
                IsReadOnly = true,
                Opacity = 99
            };

            textBox.PreviewMouseLeftButtonDown += EditText;

            Canvas.SetTop(textBox, _curretnPoint.Y);
            Canvas.SetLeft(textBox, _curretnPoint.X);

            OnlyCanvas.Children.Add(textBox);

            BtnText.IsChecked = false;
            _selectedShape = SelectedShape.None;

            _drawnUIElements.Push(textBox);
        }

        private void EditText(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is TextBox t)) return;
            TextWindow textWindow = new TextWindow(t.Text, t.FontSize, t.Foreground, t.Background);
            textWindow.ShowDialog();

            if (!Text.Condition) return;
            for (int i = OnlyCanvas.Children.Count - 1; i >= 0; i--)
            {
                if (!(OnlyCanvas?.Children[i] is TextBox)) continue;
                if (!((TextBox)OnlyCanvas.Children[i]).Name.Equals(t.Name)) continue;
                ((TextBox)OnlyCanvas.Children[i]).FontSize = Text.Font;
                ((TextBox)OnlyCanvas.Children[i]).Foreground = Text.Foreground;
                ((TextBox)OnlyCanvas.Children[i]).Background = Text.Background;
            }

            foreach (var v in _drawnUIElements)
            {
                if (!(v is TextBox text)) continue;
                if (!(text.Name.Equals(t.Name))) continue;
                text.FontSize = Text.Font;
                text.Foreground = Text.Foreground;
                text.Background = Text.Background;
            }
        }

        #endregion

        #region Canvas

        private void OnlyCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _curretnPoint = e.GetPosition(OnlyCanvas);

            switch (_selectedShape)
            {
                case SelectedShape.Ellipse:
                    EllipseWindow ellipseWindow = new EllipseWindow();
                    ellipseWindow.ShowDialog();
                    DrawEllipse();
                    break;
                case SelectedShape.Polygon:
                    _polygonPoints.Add(_curretnPoint);
                    break;
                case SelectedShape.Text:
                    TextWindow textWindow = new TextWindow();
                    textWindow.ShowDialog();
                    PrintText();
                    break;
                case SelectedShape.None: return;
                default: return;
            }
        }

        private void OnlyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(_polygonPoints.Count >= 3 && _selectedShape == SelectedShape.Polygon)) return;
            PolygonWindow polygonWindow = new PolygonWindow();
            polygonWindow.ShowDialog();
            DrawPolygon();
        }

        #endregion

        #endregion

        #region Commands

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (_flag)
            {
                for (int i = 0; i < _count; i++)
                {
                    var element = _undo.Pop();
                    OnlyCanvas.Children.Add(element);
                    _drawnUIElements.Push(element);
                }
                _flag = false;
                _count = 0;
            }
            else
            {
                if (_drawnUIElements.Count < 1) return;
                _redo.Push((OnlyCanvas.Children[OnlyCanvas.Children.Count - 1]));
                OnlyCanvas.Children.RemoveAt(OnlyCanvas.Children.Count - 1);
                _drawnUIElements.Pop();
            }
        }

        private void BtnRedo_Click(object sender, RoutedEventArgs e)
        {
            if (_flag) return;
            if (_redo.Count < 1) return;
            var element = _redo.Pop();
            OnlyCanvas.Children.Add(element);
            _drawnUIElements.Push(element);
        }

        private void BtnClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            if (_flag) return;
            _count = _drawnUIElements.Count;
            while (_drawnUIElements.Count > 0)
            {
                _undo.Push(_drawnUIElements.Pop());
                OnlyCanvas.Children.RemoveAt(OnlyCanvas.Children.Count - 1);
                _flag = true;
            }
        }

        #endregion
    }
}