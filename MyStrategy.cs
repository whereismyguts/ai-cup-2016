using System;
using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk {
    public sealed class MyStrategy: IStrategy {

        Wizard self;
        World world;
        Game game;
        Move move;

        Wizard fave;

        Vector target;

        // get target: 
        //correct distance to fave, go away, attack weak enemy, get bomus

        public void Move(Wizard self, World world, Game game, Move move) {
            this.self = self;
            this.world = world;
            this.move = move;
            this.game = game;


            // moving block
            Projectile bullet = CheckProjectiles();
            if(bullet != null) {
                StrafeFrom(bullet);
                return;
            }
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

            //fave = UpdateFavoriteWizard();
            //if(fave != null) {
            //    CorrectPosition();
            //    return;
            //}

            //if(bonus != null)
            //    GoTo(bonus);
            //else
                GoAmongTheLane();





            if(target != null) {
                move.Turn = self.GetAngleTo(target.X, target.Y);
                move.Speed = game.WizardForwardSpeed;
                move.StrafeSpeed = 0;
            }

            //move.Speed = game.WizardForwardSpeed;
            //move.StrafeSpeed = game.WizardStrafeSpeed;
            //move.Turn = game.WizardMaxTurnAngle;
            //move.Action = ActionType.MagicMissile;
        }

        private void GoAmongTheLane() {
            target = new Vector(world.Width / 2, world.Height / 2);
        }

        private void StrafeFrom(Unit obj) {
            var angle = self.GetAngleTo(obj.X, obj.Y);
            if(angle > Math.PI / 2.0)
                move.Speed = game.WizardForwardSpeed;
            else move.Speed = game.WizardBackwardSpeed;

            if(0 < angle && angle < Math.PI)
                move.Turn = Math.PI / 2.0 - angle;
            else
                move.Turn = 3*Math.PI / 2.0 - angle;
        }

        Projectile CheckProjectiles() {
            var projectiles = world.Projectiles;
            foreach(var pr in projectiles) {
                Vector prVec = new Vector(pr.SpeedX, pr.SpeedY);
                Vector toMe = new Vector(self.X - pr.X, self.Y - pr.Y);
                var angle = prVec.AngleTo(toMe);
                if(angle < Math.Atan(self.Radius / self.GetDistanceTo(pr.X, pr.Y)))
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
            double sin = X* vector2.Y - vector2.X * Y;
            double cos = X * vector2.X + Y * vector2.Y;

            return Math.Atan2(sin, cos) * (180 / Math.PI);
        }
    }
}