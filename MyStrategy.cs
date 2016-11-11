
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

        public void Move(Wizard me, World world, Game game, Move move) {
            this.me = me;
            this.world = world;
            this.move = move;
            this.game = game;



            //Path finding test:

            //if(grid == null)
            //    grid = new Grid(world);
            //else
            //    grid.Reveal(world);



            //foreach(Bonus item in world.Bonuses) {
            //    List<Vector> path =  grid.GetPath(new Point((int)me.X, (int)me.Y), new Point((int)item.X, (int)item.Y));
            //}




            //

            //move.Action = ActionType.Staff;
            // moving block
            //Projectile bullet = CheckProjectiles();
            //if(bullet != null) {
            //    StrafeFrom(bullet);
            //    return;
            //}


            //run if need
            {
                LivingUnit close = GetClosestEnemyUnit();

                if(close != null && (me.Life < me.MaxLife * 0.6 || me.GetDistanceTo(close.X, close.Y) < me.CastRange * 0.6)) {
                    ChaseGoal(false, close.X, close.Y);
                    move.Action = ActionType.MagicMissile;
                    return;
                }
            }
            //

            LivingUnit archEnemy = FindArchEnemy();
            if(archEnemy != null) {
                if(me.GetDistanceTo(archEnemy.X, archEnemy.Y) > me.CastRange) {
                    ChaseGoal(true, archEnemy.X, archEnemy.Y);
                    return;
                }
                else
                if(Math.Abs(me.GetAngleTo(archEnemy)) > 0.01) {
                    move.Turn = me.GetAngleTo(archEnemy);
                    move.Speed = 0;
                    return;
                }
                else {
                    move.Speed = 0;
                    move.Action = ActionType.MagicMissile;
                    if(strafe == 30) {
                        strafeSpeed = -1;
                    }
                    if(strafe == -30) {
                        strafeSpeed = 1;
                    }
                    move.StrafeSpeed = strafeSpeed * game.WizardStrafeSpeed;
                    strafe += strafeSpeed;

                    return;
                }

            }
            strafe = 0;

            //Unit fave = GetTopRatedWizard();

            Unit fave = GetClosestEnemyUnit();

            if(fave == null) {
                double dist = double.MaxValue;
                foreach(var m in world.Minions) {
                    if(m.Faction != me.Faction)
                        continue;
                    double curD = me.GetDistanceTo(m.X, m.Y);
                    if(curD < dist) {
                        dist = curD;
                        fave = m;
                    }
                }
            }


            if(fave != null) {
                Vector goal = CalcFaveNearPoint(fave, 0, 70);
                ChaseGoal(true, goal.X, goal.Y);
            }

        }

        Unit GetTopRatedWizard() {
            Unit result = null;
            double maxrate = double.MinValue;
            foreach(var w in world.Wizards) {
                if(w.Faction != me.Faction || w.IsMe)
                    continue;
                if(me.GetDistanceTo(w.X, w.Y) > me.VisionRange)
                    continue;

                double rate = GetPlayerScore(w.OwnerPlayerId);


                if(rate > maxrate) {
                    maxrate = rate;
                    result = w;
                }
            }
            return result;
        }

        LivingUnit GetClosestEnemyUnit() {
            double dist = double.MaxValue;
            LivingUnit result = null;
            List<LivingUnit> list = new List<LivingUnit>();
            list.AddRange(world.Minions);
            list.AddRange(world.Wizards);
            foreach(var w in list) {
                if(w.Faction != me.Faction || w.Id == me.Id)
                    continue;
                double curD = me.GetDistanceTo(w.X, w.Y);
                if(curD < dist) {
                    dist = curD;
                    result = w;
                }
            }
            return result;
        }

        private double GetPlayerScore(long id) {
            Player player = world.Players.First(p => p.Id == id);

            return player != null ? player.Score : 0;
        }

        private LivingUnit FindArchEnemy() {
            List<LivingUnit> enemiesList = new List<LivingUnit>();
            enemiesList.AddRange(world.Minions);
            enemiesList.AddRange(world.Wizards);
            enemiesList.AddRange(world.Buildings);

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
        double GetTypeFactor(LivingUnit en) {
            if(en is Minion) return 0.3;

            if(en is Wizard) return 1;

            if(en is Building) {
                if((en as Building).Type == BuildingType.FactionBase) return 0.8;
                if((en as Building).Type == BuildingType.GuardianTower) return 0.6;
            }

            return 0;
        }

        private Vector CalcFaveNearPoint(Unit goalUnit, double angleTo, double distTo) {
            double angleTotal = goalUnit.Angle + angleTo;
            return new Vector(goalUnit.X - Math.Cos(angleTotal) * distTo, goalUnit.Y - Math.Sin(angleTotal) * distTo);
        }

        void ChaseGoal(bool fwd, double x, double y) {

            move.Turn = me.GetAngleTo(x, y);
            move.Speed = fwd ? 30 : -30;

            if(!fwd) move.Action = ActionType.MagicMissile;

            WalkAroundIfNeed();
        }



        private void WalkAroundIfNeed() {
            List<CircularUnit> blocks = new List<CircularUnit>();
            blocks.AddRange(world.Buildings);
            blocks.AddRange(world.Trees);
            blocks.AddRange(world.Minions);
            blocks.AddRange(world.Wizards);

            foreach(CircularUnit obj in blocks) {
                if(obj.Id == me.Id)
                    continue;
                double closeDist = me.Radius + obj.Radius + 10;
                if(obj.GetDistanceTo(me.X, me.Y) <= closeDist) {

                    double angle = me.GetAngleTo(obj.X, obj.Y);

                    move.Speed = -Math.Cos(angle) * 30;
                    move.StrafeSpeed = -Math.Sin(angle) * 30;
                    move.Turn = 0;
                    //move.Turn = -me.GetAngleTo(obj.X, obj.Y);
                    //move.Speed = 0;
                    return;
                }


            }
        }

        //Vector goal;

        private void StrafeFrom(Unit obj) {
            var angle = me.GetAngleTo(obj.X, obj.Y);
            if(angle > Math.PI / 2.0)
                move.Speed = game.WizardForwardSpeed;
            else move.Speed = game.WizardBackwardSpeed;

            if(0 < angle && angle < Math.PI)
                move.Turn = Math.PI / 2.0 - angle;
            else
                move.Turn = 3 * Math.PI / 2.0 - angle;
        }


    }
    public struct Vector {

        public Vector(double x, double y) {
            this.X = x;
            this.Y = y;
        }

        public double X { get; set; }
        public double Y { get; set; }


    }
}