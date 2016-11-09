using System;
using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Model;
using System.Linq;
using System.Collections.Generic;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk {
    public sealed class MyStrategy : IStrategy {

        Wizard me;
        World world;
        Game game;
        Move move;

        Unit fave;

        // get target: 
        //correct distance to fave, go away, attack weak enemy, get bomus

        public void Move(Wizard me, World world, Game game, Move move) {
            this.me = me;
            this.world = world;
            this.move = move;
            this.game = game;

            //move.Action = ActionType.Staff;
            // moving block
            //Projectile bullet = CheckProjectiles();
            //if(bullet != null) {
            //    StrafeFrom(bullet);
            //    return;
            //}
            //Unit danger = CheckForDanger();
            //if(danger != null) {
            //    RunFrom(danger);
            //    return;
            //}


            //LivingUnit enemies = GetCloseEnemy(); // nearest, weakest
            //if(enemy != null)
            //    Kick();
            //else {
            //    enemy = NearWeakEnemy(); // weakest in priority

            //    if(enemy != null) {
            //        if(CanShoot(enemy))
            //            Shoot(enemy);
            //        else
            //            GetCloseToEnemy(enemy);
            //    }
            //}

            //Bonus bonus = FindBestBonus(); // shields and power
            //if(bonus!=null && bonus.Type != BonusType.Haste) {
            //    RunTo(bonus);
            //    return;
            //}

            //            fave = UpdateFavoriteWizard();


            if(me.Life < me.MaxLife * 0.3) {
                goal = me.Faction == Faction.Academy ? new Vector(0, world.Height) : new Vector(world.Width, 0);
            }

            List<LivingUnit> enemiesList = new List<LivingUnit>();
            enemiesList.AddRange( world.Minions);
            enemiesList.AddRange(world.Wizards);
            enemiesList.AddRange(world.Buildings);

                foreach(var en in enemiesList) {
    
                    if(en.Faction != me.Faction && en.Faction!=  Faction.Neutral) {
                    if(me.GetDistanceTo(en) < me.CastRange*0.95)
                        if(Math.Abs(me.GetAngleTo(en)) > 0.01) {
                            move.Turn = me.GetAngleTo(en);
                            move.Speed = 0;
                            return;
                        }
                        else {
                            move.Action = ActionType.MagicMissile;
                            move.Speed = -0.5;
                            return;
                        }
                }
            }



                    double dist = 10000.0;
            foreach(var w in world.Wizards) {
                if(w.Faction != me.Faction || w.IsMe)
                    continue;
                double curD = me.GetDistanceTo(w.X, w.Y);
                if(curD < dist) {
                    dist = curD;
                    fave = w;
                }
            }
            if(fave==null)
            foreach(var m in world.Minions) {
                if(m.Faction != me.Faction)
                    continue;
                double curD = me.GetDistanceTo(m.X, m.Y);
                if(curD < dist) {
                    dist = curD;
                    fave = m;
                }
            }



            foreach(var en in enemiesList)
                if(en.Faction != me.Faction ) 
                    if(me.GetDistanceTo(en) < me.Radius + en.Radius + 10) 
                    {
                    move.Turn = me.GetAngleTo(en);
                    move.Action = ActionType.Staff;
                    return;
                }


                List<CircularUnit> list = new List<CircularUnit>();
            list.AddRange(world.Buildings);
            list.AddRange(world.Trees);
            list.AddRange(world.Minions);
            list.AddRange(world.Wizards);

            foreach(var obj in list) {
                if(obj.Id == me.Id)
                    continue;
                double closeDist = me.Radius + obj.Radius;
                if(obj.GetDistanceTo(me.X, me.Y) < closeDist + 10) {

                    move.Turn = -me.GetAngleTo(obj.X, obj.Y);
                    move.Speed = game.WizardForwardSpeed;
                    return;
                }
                else {
                    move.Speed--;
                }
            }

            if(fave != null) {
               
                    goal = new Vector(fave.X - Math.Cos(fave.Angle+Math.PI/2.0)*100, fave.Y -Math.Sin(fave.Angle + Math.PI / 2.0) * 100);
            }

            //if(bonus != null)
            //    GoTo(bonus);
            //else







            ChaseGoal();

            //move.Speed = game.WizardForwardSpeed;
            //move.StrafeSpeed = game.WizardStrafeSpeed;
            //move.Turn = game.WizardMaxTurnAngle;
            //move.Action = ActionType.MagicMissile;
        }

        void ChaseGoal() {
            if(goal != null) {
                move.Turn = me.GetAngleTo(goal.X, goal.Y);
                move.Speed = 300;

            }
        }

        Vector goal;

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

        Projectile CheckProjectiles() {
            var projectiles = world.Projectiles;
            foreach(var pr in projectiles) {
                Vector prVec = new Vector(pr.SpeedX, pr.SpeedY);
                Vector toMe = new Vector(me.X - pr.X, me.Y - pr.Y);
                var angle = prVec.AngleTo(toMe);
                if(angle < Math.Atan(me.Radius / me.GetDistanceTo(pr.X, pr.Y)))
                    return pr;
            }
            return null;
        }
    }
    public class Vector {

        public Vector(double x, double y) {
            this.X = x;
            this.Y = y;
        }

        public double X { get; set; }
        public double Y { get; set; }


        public double AngleTo(Vector vector2) {
            double sin = X * vector2.Y - vector2.X * Y;
            double cos = X * vector2.X + Y * vector2.Y;

            return Math.Atan2(sin, cos) * (180 / Math.PI);
        }
    }
}