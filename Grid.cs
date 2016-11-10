using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk {
    class TestUnit: CircularUnit {
        protected TestUnit(double x, double y, double radius) : base(0, x, y, 0,0,0,  Faction.Other, radius) {
        }
    }

    internal class Grid {
        const double step = 10;
        public Grid(World world) {


            List<CircularUnit> objects = new List<CircularUnit>();
            objects.AddRange(world.Buildings);
            objects.AddRange(world.Trees);

            CreateCells(objects);
            CreateBitmapAtRuntime();

            //List<CircularUnit> objects = new List<CircularUnit>();
        }

        public void CreateBitmapAtRuntime() {

            Bitmap bitmap = new Bitmap(600, 600);
            Graphics gr = Graphics.FromImage(bitmap);
            //gr.Clear(Color.White);
            foreach(Vector point in cells)
                gr.FillRectangle(Brushes.Red, (float)point.X, (float)point.Y, 1, 1);

            bitmap.Save("test.bmp", ImageFormat.Bmp);
            Process.Start("test.bmp");

        }

        private void CreateCells(List<CircularUnit> objects) {
            foreach(CircularUnit unit in objects) {
                double h = 0.01;
                for(double a = 0; a < Math.PI * 2; a += h) {
                    double y = Math.Sin(a) * unit.Radius + unit.Y;
                    double x = Math.Cos(a) * unit.Radius + unit.X;

                    AddCell((int)Math.Floor(x), (int)Math.Floor(y));
                }

            }
        }
        List<Vector> cells = new List<Vector>();
        private void AddCell(int x, int y) {
            Vector point = new Vector(x, y);
            if(!cells.Contains(point))
                cells.Add(point);
        }
    }
}