using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk {
    public class Grid {
        const double cellSize = 10;
        const int skipCells = 1;
        Random rnd = new Random();

        List<long> objectsId = new List<long>();
        PointList cells = new PointList();

        public Grid(World world) {
            Reveal(world);
        }
        Bitmap bmp;
        public void  Reveal(World world) {
            var objects = new List<CircularUnit>();
            objects.AddRange(world.Buildings);
            objects.AddRange(world.Trees);

            CreateCells(objects);

            bmp = new Bitmap(cells.Xmax, cells.Ymax);
            Graphics g = Graphics.FromImage(bmp);

            foreach(var c in cells.List) {
                g.FillRectangle(Brushes.Red, c.X, c.Y, 1, 1);
            }

          //  bmp.Save("1.bmp", ImageFormat.Bmp);
            //Process.Start("1.bmp");
        }

        public PointList GetPath(Point start, Point end) {

            //Point start = GetRandomPoint(cells);
            //Point end = GetRandomPoint(cells);
            PointList path = AStar.FindPath(cells, start, end);

            if(path == null)
                return null;
            if(skipCells > 0) {
                int counter = 0;
                for(int i = 0; i < path.List.Count; i++) {
                    if(counter == skipCells)
                        counter = 0;
                    else {
                        path.Remove(i);
                        counter++;
                        i--;
                    }
                }
            }

            return path;
        }

        private Point GetRandomPoint(PointList cells) {
            Point p = new Point(rnd.Next(cells.Xmin, cells.Xmax), rnd.Next(cells.Ymin, cells.Ymax));
            return cells.Contains(p) ? GetRandomPoint(cells) : p;
        }

        void CreateCells(List<CircularUnit> newOjects) {
            foreach(CircularUnit unit in newOjects) 
                if(!objectsId.Contains(unit.Id))
                {
                //square method
                var left = unit.X + unit.Radius;
                var bottom = unit.Y + unit.Radius;
                    for(var i = unit.X - unit.Radius; i < left; i += cellSize)
                        for(var j = unit.Y - unit.Radius; j < bottom; j += cellSize) {
                            cells.Add((int)Math.Floor(i / cellSize), (int)Math.Floor(j / cellSize));
                            objectsId.Add(unit.Id);
                        }

                //circular method
                //for(double a = 0; a < Math.PI*2; a += 0.01) {
                //    double y = Math.Sin(a) * unit.Radius + unit.Y;
                //    double x = Math.Cos(a) * unit.Radius + unit.X;
                //    AddCell((int)Math.Floor(x/h), (int)Math.Floor(y/h));
                //}
            }
        }

        
        



        public void CreateBitmapAtRuntime(PointList path) {
            Bitmap bitmap = new Bitmap(cells.Xmax, cells.Ymax);
            Graphics gr = Graphics.FromImage(bitmap);
            //gr.Clear(Color.White);
            foreach(Point point in cells.List)
                gr.FillRectangle(Brushes.Red, (float)point.X, (float)point.Y, 1, 1);

            if(path != null) {
                foreach(Point point in path.List)
                    gr.FillRectangle(Brushes.Green, (float)point.X, (float)point.Y, 1, 1);
                gr.FillRectangle(Brushes.White, path.List[0].X, path.List[0].Y, 1, 1);
            }
            bitmap.Save("test.bmp", ImageFormat.Bmp);
            //Process.Start("test.bmp");
        }
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
                List.Add(point);
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

        public Point(int x, int y) {
            this.X = x;
            this.Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

    }
}