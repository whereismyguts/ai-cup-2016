
using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Model;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk {
    public sealed class MyStrategy : IStrategy {

        Wizard me;
        World world;
        Game game;
        Move move;

        ClusterController clController = new ClusterController();

        static Vector home;
        static Vector mordor;
        Faction OppositeFaction;
        //  static Vector firstAssault;

        public void Move(Wizard me, World world, Game game, Move move) {
            this.me = me;
            this.world = world;
            this.move = move;
            this.game = game;
          
            if(home.IsEmpty) {
                home = me.Faction == Faction.Academy ? new Vector(600, 3390) : new Vector(3390, 600);
                mordor = me.Faction == Faction.Renegades ? new Vector(600, 3390) : new Vector(3390, 600);
                OppositeFaction = me.Faction == Faction.Academy ? Faction.Renegades : Faction.Academy;
            }

            AI.MakeMove(me, world, game, move);
            return;
            UpdateMap();

            if(me.Life < me.MaxLife * 0.5) {
                LivingUnit runFrom = FindDanger(); //TODO make separate logic block with loacl pathfinding to escape
                if(runFrom != null && runFrom.GetDistanceTo(me) <= me.VisionRange ) {

                    if(me.GetDistanceTo(home.X, home.Y) <= me.VisionRange)
                        TryToGo(mordor.X, mordor.Y);
                    else
                        TryToGo(home.X, home.Y);
                    move.Action = ActionType.Staff;
                    //TryToGo(runFrom.X, runFrom.Y);
                    return;
                }
            }
            //attack
            LivingUnit archEnemy = FindArchEnemy();
            if(archEnemy != null) {
                Fight(archEnemy);
                return;
            }

            MakeMove();


            //   FollowMinions();
        }
        bool attackNeutrals = false;
        private void MakeMove() {


            var bouns = world.Bonuses.OrderBy(b => b.GetDistanceTo(me)).ToList();

            if(bouns.Count > 0) {
                TryToGo(bouns[0].X, bouns[0].Y);
                return;
            }

            var objects = new List<CircularUnit>();
            objects.AddRange(world.Wizards.Where(w => !w.IsMe && w.Faction == OppositeFaction));
            objects.AddRange(world.Buildings.Where(b => b.Faction == OppositeFaction));
            objects.AddRange(world.Minions.Where(b => b.Faction !=  Faction.Neutral ));
            clController.Update(objects);

            if(clController.Clusters.Count > 0) {
                var goals = clController.Clusters.OrderBy(c => c.Value(me)).ToList();


                TryToGo(goals);
                return;
            }
            
                TryToGo(2000, 2000);
        }

        //int 

        private void TryToGo(List<Cluster> goals) {
            for(int i = goals.Count - 1; i >= 0; i--) {
                var path = grid.GetPath(me.X, me.Y, goals[i].Position.X, goals[i].Position.Y);
                if(path != null && path.Count > 1) {
                    Goal(true, path[1].X, path[1].Y);
                   
                    return;
                    
                }
            }
            Goal(true, goals[0].Position.X, goals[0].Position.Y);
        }

        class Cluster {
            const int COLORSPACE = 0xFF * 0xFF * 0xFF;
            const int ALPHA = 0xFF << 24;
            public List<CircularUnit> units = new List<CircularUnit>();
            public Cluster(CircularUnit obj1) {
                units.Add(obj1);
                UpdatePosition();
                //penColor = Color.FromArgb(rnd.Next(COLORSPACE) + ALPHA);
            }
            public Vector Position { get; internal set; }
            //public Color penColor { get; internal set; }
            //static Random rnd = new Random();
            public static int Radius = 200;
            public object Value(Wizard me) {
                try {
                    if(units.Count < 3)
                        return 0d;
                    double d = me.GetDistanceTo(Position.X, Position.Y);
                    return d <= me.VisionRange * 2 ? 40000/d : units.Count;
                }
                catch(Exception e) {
                }
                return 100 / Position.DistanceTo(me.X, me.Y);
            }


            internal void AddUnit(CircularUnit obj1) {
                units.Add(obj1);
                UpdatePosition();
            }

            void UpdatePosition() {
                Position = new Vector(units.Sum(u => u.X) / units.Count, units.Sum(u => u.Y) / units.Count);
            }
            internal bool IsOwnerOf(CircularUnit obj) {
                foreach(var unit in units) {
                    if(unit.GetDistanceTo(obj) <= Radius)
                        return true;
                }
                return Position.DistanceTo(obj.X, obj.Y) <= Radius;
            }
        }
        class ClusterController {
            public List<Cluster> Clusters { get; set; } = new List<Cluster>();
            internal void Update(List<CircularUnit> objects) {
                Clusters = new List<Cluster>();
                foreach(var obj1 in objects) {
                    Cluster owner = Clusters.Find(c => c.IsOwnerOf(obj1));
                    if(owner != null)
                        owner.AddUnit(obj1);
                    else {
                        Clusters.Add(new Cluster(obj1));
                    }
                }
                //    if(Clusters.Count>0)
                // Draw();
            }
            //public void Draw() {
            //    Pen pen = new Pen(Color.Red, 5);
            //    Bitmap bmp = new Bitmap(4000, 4000);
            //    Graphics gr = Graphics.FromImage(bmp);
            //    foreach(var cluster in Clusters) {
            //       // gr.DrawEllipse(pen, (float)(cluster.Position.X - cluster.Radius), (float)(cluster.Position.Y - cluster.Radius), (float)cluster.Radius*2f, (float)cluster.Radius*2f);
            //        foreach(var unit in cluster.units) {
            //            Brush b = new SolidBrush(Color.White);
            //            if(unit is Wizard) {
            //                if(unit.Faction == Faction.Academy)
            //                    b = new SolidBrush(Color.Green);
            //                else
            //                    b = new SolidBrush(Color.Red);
            //            }
            //            if(unit is Minion) {
            //                if(unit.Faction == Faction.Academy)
            //                    b = new SolidBrush(Color.Lime);
            //                else
            //                    b = new SolidBrush(Color.Cyan);
            //            }
            //            gr.FillEllipse(b, (float)(unit.X - unit.Radius), (float)(unit.Y - unit.Radius), (float)unit.Radius * 2f, (float)unit.Radius * 2f);
            //            gr.DrawEllipse(new Pen( cluster.penColor, 10), (float)(unit.X - unit.Radius), (float)(unit.Y - unit.Radius), (float)unit.Radius * 2f, (float)unit.Radius * 2f);
            //        }
            //    }
            //    bmp.Save("clusters.png", ImageFormat.Png);
            //}
        }

        private void Fight(LivingUnit archEnemy) {
            double dist = me.GetDistanceTo(archEnemy);
            if(dist > me.CastRange * 0.9) {
                Goal(true, archEnemy.X, archEnemy.Y);
                return;
            }
            else
                if(dist < me.CastRange * 0.7) {
                Goal(false, archEnemy.X, archEnemy.Y);
            }
            Attack(archEnemy);
        }

        private void UpdateMap() {
            var objects = new List<CircularUnit>();
            objects.AddRange(world.Buildings);
            objects.AddRange(world.Trees);
            if(grid == null)
                grid = new Grid(objects);
            else
                grid.Reveal(objects);
        }

        private void TryToGo(double x, double y) {
            if(grid != null) {
                var path = grid.GetPath(me.X, me.Y, x, y);
                if(path != null && path.Count > 1) {
                    Goal(true, path[1].X, path[1].Y);
                    return;
                }
            }
            Goal(true, x, y);
        }

        Grid grid;

        bool BeatACreeps() {
            attackNeutrals = true;
            List<Minion> objects = world.Minions.Where(b => b.Faction == OppositeFaction || (attackNeutrals && b.Faction == Faction.Neutral)).ToList();

            if(objects.Count != 0) {
                var neutrals = GetNearEnemyUnits(3000);
                if(neutrals != null) {
                    var neu = neutrals.OrderBy(n => me.GetDistanceTo(n)).Last();
                    TryToGo(neu.X, neu.Y);
                    return true;
                }
                
            }
            return false;
        }
        void Attack(LivingUnit archEnemy) {
            double angle = me.GetAngleTo(archEnemy);
            var dist = archEnemy.GetDistanceTo(me);

            if(Math.Abs(angle) > 0.01)
                move.Turn = angle;

            //move.Speed = 0;
            move.Action = ActionType.MagicMissile;
            move.MinCastDistance = dist - archEnemy.Radius * 1.1;
            move.MaxCastDistance = dist + archEnemy.Radius * 1.1;
            //if(strafe == 30) {
            //    strafeSpeed = -1;
            //}
            //if(strafe == -30) {
            //    strafeSpeed = 1;
            //}
            //move.StrafeSpeed = strafeSpeed * game.WizardStrafeSpeed;
            //strafe += strafeSpeed;
        }
        LivingUnit GetFave() {
            try {
                return world.Minions.OrderBy(m => m.GetDistanceTo(me)).First();
            }
            catch (Exception e){
            }
            return null;
        }
        LivingUnit FindDanger() {
            List<LivingUnit> danger = GetNearEnemyUnits(me.VisionRange);
            if(danger != null && danger.Count > 0)
                return danger[0];
            return null;
            //TODO calc danger units in order and local path to escape
            //if(danger != null && (me.Life < me.MaxLife * 0.5 && me.GetDistanceTo(danger.X, danger.Y) < me.CastRange ))
            //    return danger;
            //return null;
        }
        List<LivingUnit> GetNearEnemyUnits(double range) {
            try {
                List<LivingUnit> list = new List<LivingUnit>();
                list.AddRange(world.Minions);
                list.AddRange(world.Wizards);
                list.AddRange(world.Buildings);
                var all = list.Where(
                    w => w.Faction == OppositeFaction ||
                    (w.Faction == Faction.Neutral && (w.Life < w.MaxLife || attackNeutrals)));
                if(attackNeutrals && all.FirstOrDefault(u => u.Faction == OppositeFaction) != null)
                    attackNeutrals = false;
                return all.Where(u => u.GetDistanceTo(me.X, me.Y) <=range).ToList();
            }
            catch(Exception e) {
            }
            return null;
        }
        LivingUnit FindArchEnemy() {

      
                var enemies = GetNearEnemyUnits(me.VisionRange);
                if(enemies == null)
                    return null;
                double bestValue = double.MinValue;
                LivingUnit result = null;
                foreach(var en in enemies) {
                    double HPfactor = 1.0 - (double)en.Life / en.MaxLife;
                    var dist = en.GetDistanceTo(me);
                    double distFactor = dist >= me.VisionRange ? -10 : dist >= en.Radius + me.Radius ? 100 / dist : dist * 100;
                    double typeFactor = GetTypeFactor(en);

                    double value = (HPfactor + typeFactor + distFactor) / 3.0;
                    if(value > bestValue && value > 0) {
                        bestValue = value;
                        result = en;
                    }
                }
                return result;
          
         
        }
        double GetTypeFactor(LivingUnit en) {
            return 1;
            if(en is Minion) return 0.6;
            if(en is Wizard) return 1;
            if(en is Building) {
                if((en as Building).Type == BuildingType.FactionBase) return 0.8;
                if((en as Building).Type == BuildingType.GuardianTower) return 0.6;
            }
            return 0;
        }
        Vector CalcFaveNearPoint(Unit goalUnit, double angleTo, double distTo) {
            double angleTotal = goalUnit.Angle + angleTo;
            return new Vector(goalUnit.X - Math.Cos(angleTotal) * distTo, goalUnit.Y - Math.Sin(angleTotal) * distTo);
        }
        void Goal(bool fwd, double x, double y) {
            if(WalkAround())
                return;
            move.Turn = me.GetAngleTo(x, y);
            move.Speed = fwd ? game.WizardForwardSpeed : -game.WizardBackwardSpeed;
            //if(!fwd) move.Action = ActionType.MagicMissile;

        }
        bool WalkAround() {
            List<CircularUnit> blocks = new List<CircularUnit>();
            blocks.AddRange(world.Buildings);
            blocks.AddRange(world.Trees);
            blocks.AddRange(world.Minions);
            blocks.AddRange(world.Wizards);

            try {
                CircularUnit obj = blocks.Where(b => b.Id != me.Id).OrderBy(u => u.GetDistanceTo(me)).First();
                double minDist = me.Radius + obj.Radius + 50;
                double angle = me.GetAngleTo(obj.X, obj.Y);
                double dist = obj.GetDistanceTo(me);
                if(Math.Abs(angle) <= Math.PI / 2 && dist <= minDist) {
                    if(wallAroundcounter == 30)
                        wallArounddir = -1;

                    if(wallAroundcounter == 0)
                        wallArounddir = 1;


                    move.Speed = game.WizardForwardSpeed * wallArounddir;

                    wallAroundcounter += wallArounddir;

                    move.Turn = -angle;
                    return true;
                }
            }
            catch(Exception e) {
            };
            //  wallAroundcounter = 0;
            return false;
        }
        int wallAroundcounter = 0;
        int wallArounddir = 1;
    }

    //public static class CpWalker {

    //    public static CheckpointList points;

    //    static CpWalker() {
    //        InitializeMap();

    //    }

    //    public static Vector NextPointToGo(double x0, double y0, double x1, double y1) {
    //        Vector end = new Vector(x1, y1);
    //        Vector start = new Vector(x0, y0);

    //        var around = points.list.Where(p => p.Value.Position.DistanceTo(start) < Checkpoint.Radius).ToList();
    //        //in some point
    //        if(around != null && around.Count > 0) {
    //            var nextPoints = around.First().Value.Next;
    //            int currentPoint = around.First().Key;

    //            double min = double.MaxValue;
    //            int resultIndex = -1;
    //            foreach(int index in nextPoints) {
    //                double dist = points[index].Position.DistanceTo(end);
    //                if(dist < min) {
    //                    min = dist;
    //                    resultIndex = index;
    //                }
    //            }
    //            return points[resultIndex].Position;
    //        }

    //        //not in some point
    //        var closest = points.list.OrderBy(p => p.Value.Position.DistanceTo(start)).Take(2);
    //        var goTo = closest.OrderBy(p => p.Value.Position.DistanceTo(end)).First();

    //        return goTo.Value.Position;
    //    }

    //    static void InitializeMap() {
    //        points = new CheckpointList();
    //        points.Add(0, 185, 3330, 1, 3);
    //        points.Add(1, 630, 3390, 1, 2, 4);
    //        points.Add(2, 650, 3830, 1, 5);
    //        points.Add(3, 190, 2670, 0, 6);
    //        points.Add(4, 1080, 2980, 1, 7);
    //        points.Add(5, 1390, 3820, 2, 8);
    //        points.Add(6, 170, 1650, 3, 9);
    //        points.Add(7, 1545, 2435, 4, 10);
    //        points.Add(8, 2280, 3820, 5, 11);
    //        points.Add(9, 275, 275, 6, 10, 12);
    //        points.Add(10, 2000, 2000, 7, 9, 13, 11);
    //        points.Add(11, 3600, 3600, 8, 10, 14);
    //        points.Add(12, 1675, 233, 9, 15);
    //        points.Add(13, 2529, 1457, 10, 16);
    //        points.Add(14, 3840, 2350, 11, 17);
    //        points.Add(15, 2650, 170, 12, 18);
    //        points.Add(16, 2960, 980, 13, 19);
    //        points.Add(17, 2777, 1313, 14, 20);
    //        points.Add(18, 3300, 177, 15, 19);
    //        points.Add(19, 3385, 561, 16, 18, 20);
    //        points.Add(20, 3785, 600, 17, 19);

    //    }
    //}
    //public class CheckpointList {
    //    public Dictionary<int, Checkpoint> list = new Dictionary<int, Checkpoint>();
    //    internal void Add(int id, double x, double y, params int[] next) {
    //        list[id] = new Checkpoint(x, y, next);
    //    }
    //    public Checkpoint this[int i] {
    //        get { return list[i]; }
    //        //  set { InnerList[i] = value; }
    //    }
    //}
    //public class Checkpoint {
    //    public Vector Position;
    //    public const int Radius = 50;
    //    public List<int> Next = new List<int>();

    //    public Checkpoint(double x, double y, int[] next) {
    //        Position = new Vector(x, y);
    //        Next.AddRange(next);
    //    }
    //}
}