using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk {
    public class Grid {
        public const float CellSize = 150;
        public const int MapSize = 4000;
        Random rnd = new Random();

        public Grid(List<CircularUnit> objects) {

            //work code:
            //objects = new List<CircularUnit>();
            //objects.AddRange(world.Buildings);
            //objects.AddRange(world.Trees);
            //

            ////test code:
            //objects = new List<CircularUnit>();
            //for(int i = 0; i < 120; i++) {
            //    objects.Add(new CircularUnit(rnd.Next(100, 3900), rnd.Next(100, 3900), rnd.Next(20, 50)));
            //}
            ////

            //  CreateCells(objects);


            //Point start = GetRandomPoint(cells);
            //Point end = GetRandomPoint(cells);

            //var path = GetPath(start, end);



            Reveal(objects);

            //  CreateBitmapAtRuntime(null, Point.Empty, Point.Empty, "init");

        }
        List<CircularUnit> units = new List<CircularUnit>();
        public void Reveal(List<CircularUnit> objects) {
            if(cells.List.Count != objects.Count) {
                CreateCells(objects);
                units.AddRange(objects.Where(o => units.Find(u => u.Id == o.Id) == null));
                //    CreateBitmapAtRuntime(null, Point.Empty, Point.Empty, "rev_" + objects.Count);
            }
        }

        List<Vector> GetPath(Point start, Point end) {
            // var max = (int)Math.Ceiling(4000 / cellSize);
            PointList pathInCells = AStar.FindPath(cells, start, end);
            if(pathInCells == null) {
                // CreateBitmapAtRuntime(null, start, end, "path_");
                return null;
            }
            pathInCells = Opimize(pathInCells);
            List<Vector> pathInCoord = new List<Vector>();
            foreach(Point cell in pathInCells.List) {
                pathInCoord.Add(new Vector(cell.X * CellSize + CellSize / 2.0, cell.Y * CellSize + CellSize / 2.0));
            }

            // CreateBitmapAtRuntime(pathInCoord, start, end, "path_");
            return pathInCoord;
        }

        public List<Vector> GetPath(double x, double y, double x1, double y1) {
            Point start = new Point((int)(x / CellSize), (int)(y / CellSize));
            Point end = new Point((int)(x1 / CellSize), (int)(y1 / CellSize));
            return GetPath(start, end);
        }
        List<long> processedObjects = new List<long>();
        const double radStep = Math.PI / 8;
        void CreateCells(List<CircularUnit> objects) {
            foreach(CircularUnit unit in objects)
                if(!processedObjects.Contains(unit.Id)) {
                    double radius = unit.Radius * 1.5;
                    for(double a = 0; a < Math.PI * 2; a += radStep) {
                        double y = Math.Sin(a) * radius + unit.Y;
                        double x = Math.Cos(a) * radius + unit.X;
                        cells.Add((int)Math.Floor(x / CellSize), (int)Math.Floor(y / CellSize));
                    }
                    processedObjects.Add(unit.Id);
                }
        }
        private PointList Opimize(PointList path) {
            List<Point> toRemove = new List<Point>();
            for(int i = 1; i < path.List.Count - 1; i++) {
                Point cur = (Point)path.List[i];
                Point prev = (Point)path.List[i - 1];
                Point next = (Point)path.List[i + 1];
                double angle = Point.Angle(cur - prev, cur - next);
                if(Math.Abs(angle - Math.PI) <= 0.1)
                    toRemove.Add(cur);
            }
            if(toRemove.Count > 0)
                foreach(var r in toRemove) {
                    path.List.Remove(r);
                }
            return path;
        }
        public void CreateBitmapAtRuntime(List<Vector> path, Point start, Point end, string name) {
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(MapSize, MapSize);
            System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bitmap);
            //gr.Clear(Color.White);

            System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Gray, 5);

            foreach(Point point in cells.List) {
                gr.DrawRectangle(pen, point.X * CellSize, point.Y * CellSize, CellSize, CellSize);
                gr.DrawString(point.X + " " + point.Y,
                    new System.Drawing.Font(System.Drawing.FontFamily.Families[0], 20),
                    System.Drawing.Brushes.BlueViolet, point.X * CellSize, point.Y * CellSize, System.Drawing.StringFormat.GenericDefault);
            }

            if(path != null) {
                foreach(Vector point in path)
                    gr.FillEllipse(System.Drawing.Brushes.Gray, (float)point.X - dotRadius, (float)point.Y - dotRadius, dotRadius * 2, dotRadius * 2);
                //gr.FillRectangle(Brushes.White, path.List[0].X, path.List[0].Y, 1, 1);
            }

            if(!start.IsEmpty && !end.IsEmpty) {
                gr.FillEllipse(System.Drawing.Brushes.Green, start.X * CellSize + CellSize / 2 - dotRadius, start.Y * CellSize + CellSize / 2 - dotRadius, dotRadius * 2, dotRadius * 2);
                gr.FillEllipse(System.Drawing.Brushes.Red, end.X * CellSize + CellSize / 2 - dotRadius, end.Y * CellSize + CellSize / 2 - dotRadius, dotRadius * 2, dotRadius * 2);
            }

            foreach(var point in units) {
                gr.FillEllipse(System.Drawing.Brushes.White,
                    (float)(point.X - point.Radius),
                    (float)(point.Y - point.Radius),
                    (float)(point.Radius * 2),
                    (float)(point.Radius * 2));
            }

            bitmap.Save(name + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            //  System.Diagnostics.Process.Start(name + ".bmp");
        }

        PointList cells = new PointList();
        public PointList Cells {
            get { return cells; }
        }
        //PointList path = new PointList();
        private Point GetRandomPoint(PointList cells) {
            Point p = new Point(rnd.Next(0, (int)(MapSize / CellSize)), rnd.Next(0, (int)(MapSize / CellSize)));
            return cells.Contains(p) ? GetRandomPoint(cells) : p;
        }
        const int dotRadius = 35;
    }
    //class CircularUnit { // test class
    //    static Random rnd = new Random();
    //    public double X;
    //    public double Y;
    //    public double Radius;
    //    public CircularUnit(double x, double y, double radius) {
    //        X = x + rnd.NextDouble();
    //        Y = y + rnd.NextDouble();
    //        Radius = radius + rnd.NextDouble();
    //    }
    //}
    public class PointList {
        public ArrayList List = new ArrayList();//  new List<Point>();
        //public int Xmax = int.MinValue;
        //public int Xmin = int.MaxValue;
        //public int Ymax = int.MinValue;
        //public int Ymin = int.MaxValue;

        public bool Contains(Point p) {
            return List.Contains(p);
        }

        public void Reverse() {
            List.Reverse();
        }

        public void Remove(Point p) {
            if(List.Contains(p))
                List.Remove(p);
        }
        public void Remove(int i) {
            List.RemoveAt(i);
        }

        public void Add(Point point) {
            AddInternal(point);
        }
        public void Add(int x, int y) {
            if(x < 0 || y < 0)
                return;
            AddInternal(new Point(x, y));
        }

        void AddInternal(Point point) {
            if(!List.Contains(point)) {
                //if(point.X > Xmax)
                //    Xmax = point.X;
                //if(point.X < Xmin)
                //    Xmin = point.X;
                //if(point.Y > Ymax)
                //    Ymax = point.Y;
                //if(point.Y < Ymin)
                //    Ymin = point.Y;
                List.Add(point);
            }
        }
    }
    public struct Vector {
        public Vector(double x, double y) {
            this.X = x;
            this.Y = y;
        }
        public Vector Rotate(double rad) {
            double x = X * Math.Cos(rad) - Y * Math.Sin(rad);
            double y = X * Math.Sin(rad) + Y * Math.Cos(rad);
            return new Vector(x, y);
        }
        public bool IsEmpty { get { return X == 0 && Y == 0; } }
        public double X { get; set; }
        public double Y { get; set; }

        internal double DistanceTo(Vector point) {
            double dx = X - point.X;
            double dy = Y - point.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public Vector SetLength(double length) {
            return this / DistanceTo(0, 0) * length;
        }

        internal double DistanceTo(double x, double y) {
            double dx = X - x;
            double dy = Y - y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        internal static double Angle(Vector p1, Vector p2) {
            double sin = p1.X * p2.Y - p2.X * p1.Y;
            double cos = p1.X * p2.X + p1.Y * p2.Y;
            return Math.Atan2(sin, cos);
        }

        internal Point toPoint() {
            return new Point((int)X, (int)Y);
        }

        public static Vector operator /(Vector c1, double f) {
            return new Vector(c1.X / f, c1.Y / f);
        }
        public static Vector operator *(Vector c1, double f) {
            return new Vector(c1.X * f, c1.Y * f);
        }
        public static Vector operator +(Vector c1, Vector c2) {
            return new Vector(c1.X + c2.X, c1.Y + c2.Y);
        }
        public static Vector operator -(Vector c1, Vector c2) {
            return new Vector(c1.X - c2.X, c1.Y - c2.Y);
        }
    }
    public struct Point {
        public static Point Empty { get { return new Point(); } }
        public bool IsEmpty {
            get { return X == 0 && Y == 0; }
        }
        //public override bool Equals(object obj) {
        //    Point p = (Point)obj;
        //    return !(this.X != p.X || this.Y != p.Y);
        //}

        public static bool operator ==(Point p1, Point p2) {
            return p1.X == p2.X && p1.Y == p2.Y;
        }
        public static bool operator !=(Point p1, Point p2) {
            return p1.X != p2.X || p1.Y != p2.Y;
        }
        public static Point operator -(Point c1, Point c2) {
            return new Point(c1.X - c2.X, c1.Y - c2.Y);
        }
        internal static double Angle(Point p1, Point p2) {
            double sin = p1.X * p2.Y - p2.X * p1.Y;
            double cos = p1.X * p2.X + p1.Y * p2.Y;
            return Math.Atan2(sin, cos);
        }
        public Point(int x, int y) {
            this.X = x;
            this.Y = y;
        }
        public int X { get; set; }
        public int Y { get; set; }

    }
    static class AStar {
        public static PointList FindPath(PointList field, Point start, Point goal) {
            long exitTick = DateTime.Now.Ticks + 1 * TimeSpan.TicksPerSecond;

            field.Remove(goal);
            field.Remove(start);

            // Шаг 1.
            var closedSet = new List<PathNode>();
            var openSet = new List<PathNode>();
            // Шаг 2.
            PathNode startNode = new PathNode() {
                Position = start,
                CameFrom = null,
                PathLengthFromStart = 0,
                HeuristicEstimatePathLength = GetHeuristicPathLength(start, goal)
            };
            openSet.Add(startNode);
            while(openSet.Count > 0) {
                if(DateTime.Now.Ticks > exitTick)
                    return null;
                // Шаг 3.
                var currentNode = openSet.OrderBy(node =>
                  node.EstimateFullPathLength).First();
                // Шаг 4.
                if(currentNode.Position.Equals(goal))
                    return GetPathForNode(currentNode);
                // Шаг 5.
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                // Шаг 6.
                foreach(var neighbourNode in GetNeighbours(currentNode, goal, field)) {
                    // Шаг 7.
                    if(closedSet.Count(node => node.Position.Equals(neighbourNode.Position)) > 0)
                        continue;
                    var openNode = openSet.FirstOrDefault(node =>
                      node.Position.Equals(neighbourNode.Position));
                    // Шаг 8.
                    if(openNode == null)
                        openSet.Add(neighbourNode);
                    else
                        if(openNode.PathLengthFromStart > neighbourNode.PathLengthFromStart) {
                        // Шаг 9.
                        openNode.CameFrom = currentNode;
                        openNode.PathLengthFromStart = neighbourNode.PathLengthFromStart;
                    }
                }
            }
            // Шаг 10.
            return null;
        }
        private static int GetDistanceBetweenNeighbours() {
            return 1;
        }
        private static int GetHeuristicPathLength(Point from, Point to) {
            return (int)(Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y));
        }
        private static List<PathNode> GetNeighbours(PathNode pathNode, Point goal, PointList field) {
            var result = new List<PathNode>();

            // Соседними точками являются соседние по стороне клетки.
            Point[] neighbourPoints = new Point[8];
            neighbourPoints[0] = new Point(pathNode.Position.X + 1, pathNode.Position.Y);
            neighbourPoints[1] = new Point(pathNode.Position.X - 1, pathNode.Position.Y);
            neighbourPoints[2] = new Point(pathNode.Position.X, pathNode.Position.Y + 1);
            neighbourPoints[3] = new Point(pathNode.Position.X, pathNode.Position.Y - 1);

            neighbourPoints[4] = new Point(pathNode.Position.X + 1, pathNode.Position.Y + 1);
            neighbourPoints[5] = new Point(pathNode.Position.X - 1, pathNode.Position.Y - 1);
            neighbourPoints[6] = new Point(pathNode.Position.X - 1, pathNode.Position.Y + 1);
            neighbourPoints[7] = new Point(pathNode.Position.X + 1, pathNode.Position.Y - 1);

            foreach(var point in neighbourPoints) {
                // Проверяем, что не вышли за границы карты.
                //if(point.X > max.X || point.X < min.X)
                //    continue;
                //if(point.Y > max.Y || point.Y < min.Y)
                //    continue;
                // Проверяем, что по клетке можно ходить.
                if(field.Contains(new Point((int)point.X, (int)point.Y)))
                    continue;
                // Заполняем данные для точки маршрута.
                var neighbourNode = new PathNode() {
                    Position = point,
                    CameFrom = pathNode,
                    PathLengthFromStart = pathNode.PathLengthFromStart +
                      GetDistanceBetweenNeighbours(),
                    HeuristicEstimatePathLength = GetHeuristicPathLength(point, goal)
                };
                result.Add(neighbourNode);
            }
            return result;
        }
        private static PointList GetPathForNode(PathNode pathNode) {
            var result = new PointList();
            var currentNode = pathNode;
            while(currentNode != null) {
                result.Add(currentNode.Position);
                currentNode = currentNode.CameFrom;
            }
            result.Reverse();
            return result;
        }
    }
    public class PathNode {
        public Point Position { get; set; }
        // Длина пути от старта (G).
        public int PathLengthFromStart { get; set; }
        // Точка, из которой пришли в эту точку.
        public PathNode CameFrom { get; set; }
        // Примерное расстояние до цели (H).
        public int HeuristicEstimatePathLength { get; set; }
        // Ожидаемое полное расстояние до цели (F).
        public int EstimateFullPathLength {
            get {
                return this.PathLengthFromStart + this.HeuristicEstimatePathLength;
            }
        }
    }
}
