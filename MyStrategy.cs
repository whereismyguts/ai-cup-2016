
using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Model;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk {
    public sealed class MyStrategy: IStrategy {

        Wizard me;
        World world;
        Game game;
        Move move;

        int strafe = 0;
        int strafeSpeed = 1;
        //static Grid grid;
        // get target: 
        //correct distance to fave, go away, attack weak enemy, get bomus

        Vector home;

        public void Move(Wizard me, World world, Game game, Move move) {
            this.me = me;
            this.world = world;
            this.move = move;
            this.game = game;
            if(home.IsEmpty)
                home = me.Faction == Faction.Academy ?
                    CpWalker.points[0].Poisition :
                    CpWalker.points[10].Poisition;

            //run
            LivingUnit runFrom = FindDanger();
            if(runFrom != null) {
             //   var save = CpWalker.NextPointToGo(me.X, me.Y, home.X, home.Y);
                Goal(false, runFrom.X, runFrom.Y);
                return;
            }
            //attack
            LivingUnit archEnemy = FindArchEnemy();
            if(archEnemy != null) {
                if(me.GetDistanceTo(archEnemy.X, archEnemy.Y) > me.CastRange * 0.8) {
                    Goal(true, archEnemy.X, archEnemy.Y);
                    return;
                }
                else {
                    Attack(archEnemy);
                    return;
                }
            }
            //strafe = 0; ??
            //find what to do



            //test
            //var pointToGo = CpWalker.NextPointToGo(me.X, me.Y, 2000, 50);
            //Goal(true, pointToGo.X, pointToGo.Y);
            //




            FollowMinions();
        }

        private void PerformRoute() {
            //Path finding test:
            //if(grid == null)
            //    grid = new Grid(world);
            //else
            //    grid.Reveal(world);
            //foreach(Bonus item in world.Bonuses) {
            //    List<Vector> path =  grid.GetPath(new Point((int)me.X, (int)me.Y), new Point((int)item.X, (int)item.Y));
            //}
            //
        }

        private void FollowMinions() {
            LivingUnit fave = GetFave();
            if(fave != null) {
                Vector goal = CalcFaveNearPoint(fave, 0, 100);
                Goal(true, goal.X, goal.Y);
            }
        }

        void Attack(LivingUnit archEnemy) {
            if(Math.Abs(me.GetAngleTo(archEnemy)) > 0.01)
                move.Turn = me.GetAngleTo(archEnemy);
            move.Speed = 0;

            var dist = archEnemy.GetDistanceTo(me.X, me.Y);

            move.Action = ActionType.MagicMissile;
            move.MinCastDistance = dist - archEnemy.Radius * 1.5;
            move.MaxCastDistance = dist + archEnemy.Radius * 1.5;
            if(strafe == 30) {
                strafeSpeed = -1;
            }
            if(strafe == -30) {
                strafeSpeed = 1;
            }
            move.StrafeSpeed = strafeSpeed * game.WizardStrafeSpeed;
            strafe += strafeSpeed;
        }

        private LivingUnit GetFave() {
            try {
                return world.Minions.OrderBy(m => m.GetDistanceTo(me)).First();
            }
            catch { }
            return null;
        }

        LivingUnit FindDanger() {
            LivingUnit danger = GetClosestEnemyUnit();
            if(danger != null && (me.Life < me.MaxLife * 0.5 || me.GetDistanceTo(danger.X, danger.Y) < me.CastRange * 0.4))
                return danger;
            return null;
        }
        LivingUnit GetClosestEnemyUnit() {
            try {
                List<LivingUnit> list = new List<LivingUnit>();
                list.AddRange(world.Minions);
                list.AddRange(world.Wizards);
                list.AddRange(world.Buildings);

                return list
                    .Where(w => w.Faction == me.Faction || w.Faction == Faction.Neutral)
                    .OrderBy(u => u.GetDistanceTo(me))
                    .Last();
            }
            catch { }
            return null;
        }
        private LivingUnit FindArchEnemy() {
            List<LivingUnit> enemiesList = new List<LivingUnit>();
            enemiesList.AddRange(world.Minions);
            enemiesList.AddRange(world.Wizards);
            enemiesList.AddRange(world.Buildings);
            try {
                var enemies = enemiesList.Where(en => (en.Faction != me.Faction && en.Faction != Faction.Neutral));

                // now just find enimy with max value
                double bestValue = double.MinValue;
                LivingUnit result = null;
                foreach(var en in enemies) {
                    double HPfactor = 8.0 - (double)en.Life / en.MaxLife;
                    var dist = en.GetDistanceTo(me);
                    double distFactor = dist >= me.VisionRange ? -10 : dist >= en.Radius + me.Radius ? 1 : dist * 100;
                    double typeFactor = GetTypeFactor(en);

                    double value = (HPfactor + typeFactor + distFactor) / 3.0;
                    if(value > bestValue && value > 0) {
                        bestValue = value;
                        result = en;
                    }
                }
                return result;
            }
            catch { }
            return null;
        }
        double GetTypeFactor(LivingUnit en) {
            if(en is Minion) return 0.3;
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
            move.Turn = me.GetAngleTo(x, y);
            move.Speed = fwd ? 30 : -30;
            //if(!fwd) move.Action = ActionType.MagicMissile;
            WalkAroundIfNeed();
        }



        private void WalkAroundIfNeed() {
            List<CircularUnit> blocks = new List<CircularUnit>();
            blocks.AddRange(world.Buildings);
            blocks.AddRange(world.Trees);
            blocks.AddRange(world.Minions);
            blocks.AddRange(world.Wizards);

            try {
                CircularUnit obj = blocks.Where(b => b.Id != me.Id).OrderBy(u => u.GetDistanceTo(me)).First();
                double closeDist = me.Radius + obj.Radius + 10;
                if(obj.GetDistanceTo(me.X, me.Y) < closeDist) {
                    double angle = me.GetAngleTo(obj.X, obj.Y);
                    move.Speed = -Math.Cos(angle) * 4;
                    move.StrafeSpeed = -Math.Sin(angle) * 3;
                    move.Turn = 0;
                }
            }
            catch { };
        }
    }
    public struct Vector {

        public Vector(double x, double y) {
            this.X = x;
            this.Y = y;
        }

        public bool IsEmpty { get { return X == 0 && Y == 0; } }
        public double X { get; set; }
        public double Y { get; set; }

        internal double DistanceTo(Vector point) {
            double dx = X - point.X;
            double dy = Y - point.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        public static Vector operator /(Vector c1, double f) {
            return new Vector(c1.X / f, c1.Y / f);
        }

    }


    public static class CpWalker {

        public static CheckpointList points;

        static CpWalker() {
            InitializeMap();

        }

        public static Vector NextPointToGo(double x0, double y0, double x1, double y1) {
            Vector end = new Vector(x1, y1);
            Vector start = new Vector(x0, y0);
            Checkpoint closest = points.list.OrderBy(p => p.Value.Poisition.DistanceTo(start)).First().Value;
            if(closest.Poisition.DistanceTo(start) > Checkpoint.Radius)
                return closest.Poisition;

            double min = double.MaxValue;
            int goTo = 0;
            foreach(int index in closest.Next) {
                double dist = points[index].Poisition.DistanceTo(end);
                if(dist < min) {
                    min = dist;
                    goTo = index;
                }
            }
            return points[goTo].Poisition;
        }

        static void InitializeMap() {
            points = new CheckpointList();
            points.Add(0, 100, 3900, 1, 2, 3);
            points.Add(1, 20, 3700, 0);
            points.Add(2, 200, 3800, 0, 4);
            points.Add(3, 300, 3980, 0);
            points.Add(4, 300, 3750, 2);
            points.Add(10, 3900, 100, 4);// another base
        }




    }
    public class CheckpointList {
        public Dictionary<int, Checkpoint> list = new Dictionary<int, Checkpoint>();
        internal void Add(int id, double x, double y, params int[] next) {
            list[id] = new Checkpoint(x, y, next);
        }
        public Checkpoint this[int i] {
            get { return list[i]; }
            //  set { InnerList[i] = value; }
        }
    }
    public class Checkpoint {
        public Vector Poisition;
        public const int Radius = 50;
        public List<int> Next = new List<int>();

        public Checkpoint(double x, double y, int[] next) {
            Poisition = new Vector(x, y);
            Next.AddRange(next);
        }


    }


}