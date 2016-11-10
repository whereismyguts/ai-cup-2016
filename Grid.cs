using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk {
    public class Grid {
        const float cellSize = 10;
        const int dotSize =5;

        Random rnd = new Random();

        List<long> objectsIds = new List<long>();
        PointList cells = new PointList();

        public Grid(World world) {
            Reveal(world);
        }
        Bitmap bmp;
        public void Reveal(World world) {
            var objects = new List<CircularUnit>();
            objects.AddRange(world.Buildings);
            objects.AddRange(world.Trees);

            CreateCells(objects);

            //SaveImage();
        }
        public List<Vector> GetPath(Point start, Point end) {
            PointList pathInCells = AStar.FindPath(cells, start, end);
            if(pathInCells == null)
                return null;
            pathInCells = Opimize(pathInCells);
            List<Vector> pathInCoord = new List<Vector>();
            foreach(var cell in pathInCells.List) {
                pathInCoord.Add(new Vector(cell.X * cellSize + cellSize / 2.0, cell.Y * cellSize + cellSize / 2.0));
            }

            return pathInCoord;
        }

       




        void CreateCells(List<CircularUnit> objects) {
            foreach(CircularUnit unit in objects)
                if(!objectsIds.Contains(unit.Id)) {

                 
                    objectsIds.Add(unit.Id);

                //circular method
                //for(double a = 0; a < Math.PI*2; a += 0.01) {
                //    double y = Math.Sin(a) * unit.Radius + unit.Y;
                //    double x = Math.Cos(a) * unit.Radius + unit.X;

                //    AddCell((int)Math.Floor(x/h), (int)Math.Floor(y/h));
                //}

                //square method
                var left = unit.X + unit.Radius;
                var bottom = unit.Y + unit.Radius;
                for(var i = unit.X - unit.Radius; i < left; i += cellSize)
                    for(var j = unit.Y - unit.Radius; j < bottom; j += cellSize)
                        cells.Add((int)Math.Floor(i / cellSize), (int)Math.Floor(j / cellSize));

            }
        }
        private PointList Opimize(PointList path) {
            //    return path;
            List<Point> toRemove = new List<Point>();
            for(int i = 1; i < path.List.Count - 1; i++) {
                double angle = Point.Angle(path.List[i] - path.List[i - 1], path.List[i] - path.List[i + 1]);
                if(Math.Abs(angle - Math.PI) <= 0.1)
                    toRemove.Add(path.List[i]);
            }
            if(toRemove.Count > 0)
                foreach(var r in toRemove) {
                    path.List.Remove(r);
                }

            return path;
        }
        public void CreateBitmapAtRuntime(List<Vector> path) {
            Bitmap bitmap = new Bitmap((int)(cells.Xmax * cellSize) + 1, (int)(cells.Ymax * cellSize) + 1);
            Graphics gr = Graphics.FromImage(bitmap);
            //gr.Clear(Color.White);
            foreach(Point point in cells.List)
                gr.FillRectangle(Brushes.Red, point.X * cellSize, point.Y * cellSize, cellSize, cellSize);

            if(path != null) {
                foreach(Vector point in path)
                    gr.FillEllipse(Brushes.Green, (float)point.X - dotSize / 2, (float)point.Y - dotSize / 2, dotSize, dotSize);
                //gr.FillRectangle(Brushes.White, path.List[0].X, path.List[0].Y, 1, 1);
            }
            bitmap.Save("test.bmp", ImageFormat.Bmp);
            Process.Start("test.bmp");
        }
    }

    public class PointList {
        public List<Point> List = new List<Point>();
        public int Xmax = int.MinValue;
        public int Xmin = int.MaxValue;
        public int Ymax = int.MinValue;
        public int Ymin = int.MaxValue;

        public bool Contains(Point p) {
            return List.Contains(p);
        }

        public void Reverse() {
            List.Reverse();
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
                if(point.X > Xmax)
                    Xmax = point.X;
                if(point.X < Xmin)
                    Xmin = point.X;
                if(point.Y > Ymax)
                    Ymax = point.Y;
                if(point.Y < Ymin)
                    Ymin = point.Y;
                List.Add(point);
            }
        }
    }

    public struct Point {
        public override bool Equals(object obj) {
            Point p = (Point)obj;
            return !(this.X != p.X || this.Y != p.Y);
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
}