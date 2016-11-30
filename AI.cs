using System;
using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk {
    public enum State { InBattle, LookFor };
    public enum Problem { Run, Attack, Push, Defend, Bonus };
    internal class AI {
        static Wizard Me; static World World; static Game Game; static Move Move;
        static Grid grid;
        static Vector moveTarget;
        static UnitInfo attackTarget;
        static bool inBattle = false;
        static Problem problem;

        static List<UnitInfo> AllLivingUnits = new List<UnitInfo>(); // must be ordered
        static List<UnitInfo> EnemyUnitsInFight = new List<UnitInfo>(); // must be ordered

        internal static void MakeMove(Wizard me, World world, Game game, Move move) {
            InitializeTick(me, world, game, move);
            GatherInfo();
            inBattle = InBattle(); //1st level
            problem = CalcProblem(); //2nd level
            CalcTargets(); //3rd level
            ProcessTargets(); //4th level

            LearnSkills();
        }
        static int[] skills = {5,6,7,8,9,0,1,2,3,4 };
        static int level = 0;
        private static void LearnSkills() {
            if(level != Me.Level && Game.IsSkillsEnabled) {
                level = Me.Level;
                for(int i = 0; i < skills.Length; i++) {
                    if(!Me.Skills.Contains((SkillType)i)) {
                        Move.SkillToLearn = (SkillType)i;
                        return;
                    }
                }
            }
        }

        static void ProcessTargets() {
            if(attackTarget != null) {
                SmartAttack(attackTarget);

                moveTarget = CorrectByPotention(moveTarget);

                SmartWalk(moveTarget);
            }
            else {
                GoSimple(moveTarget);
            }
        }

        private static Vector CorrectByPotention(Vector goal) {
            List<UnitInfo> nearBlocks = AllLivingUnits.Where(u => u.Distance <= Me.Radius * 1.3 + u.Unit.Radius).ToList();
            if(nearBlocks.Count > 0) {
                Vector newDir = (goal - UnitInfo.MyPosition).SetLength(50);
                foreach(UnitInfo unit in nearBlocks) {
                    Vector addForce = new Vector(Me.X - unit.Unit.X, Me.Y - unit.Unit.Y);
                    newDir += addForce;
                }
                return UnitInfo.MyPosition + newDir;
            }
            return goal;
        }

        static void SmartAttack(UnitInfo attackTarget) {
            var action = BestAction();
            if(action != ActionType.Staff) {
                Move.MinCastDistance = attackTarget.Distance - attackTarget.Unit.Radius * 1.1;
                Move.MaxCastDistance = attackTarget.Distance + attackTarget.Unit.Radius * 1.1;
                Kick(attackTarget, action);
            }
            else {
                var near = EnemyUnitsInFight.FirstOrDefault(e => e.Distance <= Me.Radius * 2);
                if(near != null) {
                    Kick(near, ActionType.Staff);
                }
                else
                    Kick(attackTarget, ActionType.Staff);
            }
        }

        private static void Kick(UnitInfo target, ActionType type) {
            double angle = Me.GetAngleTo(target.Unit);
            if(Math.Abs(angle) <= 0.01)
                Move.Action = type;
            else
                Move.Turn = angle;
        }

        static ActionType BestAction() {
            if(Me.Skills.Contains(SkillType.FrostBolt) &&
                 Me.RemainingActionCooldownTicks < 5 &&
                 Me.RemainingCooldownTicksByAction[(int)ActionType.FrostBolt] < 5)
                return ActionType.FrostBolt;
            if(Me.RemainingActionCooldownTicks < 5 && Me.RemainingCooldownTicksByAction[(int)ActionType.MagicMissile] < 5)
                return ActionType.MagicMissile;
            return ActionType.Staff;
        }
        static void SmartWalk(Vector goal) {
            if(goal.IsEmpty)
                return;
            Vector meToGoal = goal - UnitInfo.MyPosition;
            Vector correctSpeed = meToGoal.SetLength(3.0);
            Vector correctDir = correctSpeed.Rotate(-Me.Angle);

            Move.Speed = correctDir.X;
            Move.StrafeSpeed = correctDir.Y;
            //Move.Turn = Me.GetAngleTo(goal.X, goal.Y);
        }
        static void GoSimple(Vector goal) {
            if(grid != null) {
                var path = grid.GetPath(Me.X, Me.Y, moveTarget.X, moveTarget.Y);
                if(path != null && path.Count > 1) {
                    GoStupid(path[1].X, path[1].Y);
                    return;
                }
            }
            GoStupid(goal.X, goal.Y);
        }
        static void GoStupid(double x, double y) {
            //if(WalkAround())
            //    return;

            Vector goal = CorrectByPotention(new Vector(x, y));
            Move.Turn = Me.GetAngleTo(goal.X, goal.Y);
            Move.Speed = Game.WizardForwardSpeed;

            // WalkAround();
        }
        static bool WalkAround() {
            try {
                UnitInfo obj = AllLivingUnits.Where(b => b.Unit.Id != Me.Id).FirstOrDefault(); // must be ordered 
                double minDist = Me.Radius + obj.Unit.Radius + 30;
                double angle = Me.GetAngleTo(obj.Unit.X, obj.Unit.Y);

                if(Math.Abs(angle) <= Math.PI && obj.Distance <= minDist) {

                    if(walkAroundcounter == 30)
                        walkArounddir = -2;
                    if(walkAroundcounter == 0)
                        walkArounddir = 1;
                    Move.Speed = Game.WizardForwardSpeed;

                    Move.Turn = -angle;
                    return true;
                }
            }
            catch(Exception e) {

            };
            return false;
        }
        static int walkAroundcounter = 0;
        static int walkArounddir = 1;
        static void CalcTargets() {
            attackTarget = CalcAttackTarget();
            moveTarget = CalcMoveTarget();
            //   DrawOptimalPoint(moveTarget, AllLivingUnits,null,null);

        }
        static UnitInfo CalcAttackTarget() {
            if(EnemyUnitsInFight.Count > 0) {

                var targets = EnemyUnitsInFight.OrderBy(unit=>unit.AttackValue);

               
                    return targets.LastOrDefault();

            }// mist be ordered
            var tree = AllLivingUnits.Find(u => u.Unit is Tree && u.Distance <= Me.Radius * 1.5);// must be ordered
            return tree;
        }
        static Vector CalcMoveTarget() {
            switch(problem) {
                case Problem.Attack:
                    return CalcOptimalLocalPoint(false);
                case Problem.Run:
                    return CalcOptimalLocalPoint(true);
                case Problem.Bonus:
                    return bonus;
                case Problem.Push:
                    return CalcPushPoint();
                case Problem.Defend:
                    return CalcDefendPoint();
            }
            return new Vector(2000, 2000);

        }

        static Vector CalcDefendPoint() {
            var dangerToBase = AllLivingUnits
                .Where(unit => unit.IsEnemy && (unit.Type == UnitType.Wizard || unit.Type == UnitType.Minion))
                .OrderBy(unit => unit.Position.DistanceTo(UnitInfo.HomeBase)).LastOrDefault();
            if(dangerToBase != null)
                return dangerToBase.Position;
            return UnitInfo.HomeBase;
        }

        static Vector CalcPushPoint() {
            return NextOnLane();
            //var en = AllLivingUnits.Where(u => u.IsEnemy).FirstOrDefault();
            //return en == null ? new Vector(2000, 2000) : new Vector(en.Unit.X, en.Unit.Y);
        }

        static Vector bot = new Vector(3600, 3600);
        static Vector mid = new Vector(2000, 2000);
        static Vector top = new Vector(400, 400);

        static int CurrentLane {
            get {
                return CalcCurrentLane();
            }
        }
        static int CalcCurrentLane() {
            // return (int)LaneType.Top;
            double sum = Me.X + Me.Y;
            if(sum > 3400 && sum < 4500)
                return (int)LaneType.Middle;
            if(Me.X < 500 || Me.Y < 500)
                return (int)LaneType.Top;
            if(Me.X > 3500 || Me.Y > 3500)
                return (int)LaneType.Bottom;
            return -1;
        }
        static Vector NextOnLane() {
            if(UnitInfo.MyPosition.DistanceTo(UnitInfo.HomeBase) > UnitInfo.MyPosition.DistanceTo(UnitInfo.TheirBase))
                return UnitInfo.TheirBase;

            switch(CurrentLane) {
                case (int)LaneType.Bottom: return bot;
                case (int)LaneType.Top: return top;
                case (int)LaneType.Middle: return mid;
            }

            return mid;
        }
        static Vector PrevOnLane() {
            if(UnitInfo.MyPosition.DistanceTo(UnitInfo.HomeBase) < UnitInfo.MyPosition.DistanceTo(UnitInfo.TheirBase))
                return UnitInfo.HomeBase;

            switch(CurrentLane) {
                case (int)LaneType.Bottom: return bot;
                case (int)LaneType.Top: return top;
                case (int)LaneType.Middle: return mid;
            }

            return UnitInfo.HomeBase;
        }

        static Vector CalcOptimalLocalPoint(bool run) {
            // return new Vector();

            var blocks = AllLivingUnits.Where(u => u.Distance < Me.CastRange * 1.5).ToList(); // must be order

            var enemies = blocks.Where(u => u.IsEnemy).ToList();

            if(enemies.Count == 1 && Me.Life < Me.MaxLife * 0.4) {
                return PrevOnLane();
            }
            var other = blocks.Where(u => !u.IsEnemy);

            double rMax = Me.CastRange*1.5;

            double rStep = Me.Radius;

            double safeDistance = World.TickIndex > 1000 ? Me.CastRange * 0.3 : Me.CastRange;

            List<Vector> dots = new List<Vector>();
            for(double Rx = -rMax; Rx <= rMax; Rx += rStep)
                for(double Ry = -rMax; Ry <= rMax; Ry += rStep) {

                    Vector dot = new Vector(
                        Me.X + Rx,
                        Me.Y + Ry);



                    if(dot.X < 50 || dot.X > 3950 || dot.Y < 50 || dot.Y > 3950)
                        continue;
                    UnitInfo danger = enemies.FirstOrDefault(u => dot.DistanceTo(u.Position) < (run ? Me.Radius + u.GetCastRange() : Me.Radius + safeDistance));
                    if(danger != null)
                        continue;
                    UnitInfo block = other.FirstOrDefault(u => dot.DistanceTo(u.Position) < Me.Radius + u.Unit.Radius * 1.1);
                    if(block != null)
                        continue;
                    if(attackTarget != null) {


                        if(dot.DistanceTo(attackTarget.Position) > Me.CastRange * 0.7)
                            continue;
                    }
                    dots.Add(dot);
                }
            if(dots.Count > 0) {
                dots = dots.OrderBy(d => d.DistanceTo(PrevOnLane())).ToList();
                return dots[0];
            }

            return PrevOnLane();


        }

        private static void DrawOptimalPoint(Vector result, List<UnitInfo> blocks, List<Vector> dots, List<double> vals) {
            //  Point zero = new Point((int)Me.X, (int)Me.Y);
            float maxx = (float)(blocks.OrderBy(b => b.Unit.X).Last().Unit.X);
            float minx = (float)(blocks.OrderBy(b => b.Unit.X).First().Unit.X);
            float miny = (float)(blocks.OrderBy(b => b.Unit.Y).First().Unit.Y);
            float maxy = (float)(blocks.OrderBy(b => b.Unit.Y).Last().Unit.Y);


            Bitmap bmp = new Bitmap((int)(maxx - minx) + 50, (int)(maxy - miny) + 50);
            Graphics gr = Graphics.FromImage(bmp);

            blocks.Add(new UnitInfo(Me));

            foreach(var bl in blocks) {
                gr.FillEllipse(bl.IsEnemy ? Brushes.Red : bl.Unit.Id == Me.Id ? Brushes.Green : bl.Unit.Faction == Faction.Other ? Brushes.Yellow : Brushes.Gray,
                    (float)bl.Unit.X - minx - (float)bl.Unit.Radius, (float)bl.Unit.Y - miny - (float)bl.Unit.Radius, (float)bl.Unit.Radius * 2, (float)bl.Unit.Radius * 2);
            }


            gr.FillEllipse(Brushes.Magenta, (float)result.X - minx - 10, (float)result.Y - miny - 10, 20, 20);
            bmp.Save("local.png", ImageFormat.Png);

        }
        static Vector bonus;
        static Problem CalcProblem() {
            if(World.Bonuses.Count() > 0) {
                bonus = new Vector(World.Bonuses[0].X, World.Bonuses[0].Y);
                return Problem.Bonus;
            }
            if(inBattle) {
                return Me.Life < Me.MaxLife * 0.4 || DangerPlace() ? Problem.Run : Problem.Attack;
            }

            if(UnitInfo.HomeThrone.Life < UnitInfo.HomeThrone.MaxLife / 2 && UnitInfo.MyPosition.DistanceTo(UnitInfo.HomeBase) < 4000)
                return Problem.Defend;

                return Problem.Push; // TODo add defend
        }
        static bool DangerPlace() {
            var friendlyUnitsNear = AllLivingUnits.Where(
                u => u.Unit.Faction == Me.Faction &&
                u.Distance < Me.VisionRange / 0.7
            ).ToList();
            return friendlyUnitsNear == null || EnemyUnitsInFight.Count - friendlyUnitsNear.Count > 3;
        }
        static bool InBattle() {
            return EnemyUnitsInFight.Count > 0;
        }
        static void InitializeTick(Wizard me, World world, Game game, Move move) {
            Me = me;
            World = world;
            Game = game;
            Move = move;
            UnitInfo.Me = Me;
            UnitInfo.Game = Game;
            UnitInfo.World = World;

            if(UnitInfo.ShouldInit)
                UnitInfo.SetParams();
        }
        static void GatherInfo() {
            UnitInfo.MyPosition = new Vector(Me.X, Me.Y);
            UpdateMap();
            List<LivingUnit> objects = new List<LivingUnit>(World.Wizards);
            objects.AddRange(World.Minions);
            objects.AddRange(World.Trees);
            objects.AddRange(World.Buildings);

            AllLivingUnits = objects.Where(u => u.Id != Me.Id).Select(o => new UnitInfo(o)).OrderBy(u => u.Distance).ToList();
            EnemyUnitsInFight = AllLivingUnits.Where(u => u.IsEnemy && u.Distance <= Me.VisionRange).OrderBy(u => u.Distance).ToList();
            UnitInfo.RangedDamage = BestAction() == ActionType.FrostBolt ? Game.FrostBoltDirectDamage : Game.MagicMissileDirectDamage;
            UnitInfo.RangedDamage += Game.MagicalDamageBonusPerSkillLevel * Me.Level;

        }
        static void UpdateMap() {
            var objects = new List<CircularUnit>();
            objects.AddRange(World.Buildings);
            objects.AddRange(World.Trees);
            if(grid == null)
                grid = new Grid(objects);
            else
                grid.Reveal(objects);
        }
    }
    public enum UnitType { Minion, Wizard, Building, Tree }
    class UnitInfo {
        public static Wizard Me { get; set; }
        public static Game Game { get; set; }
        public LivingUnit Unit { get; internal set; }
        public double Distance { get; internal set; }
        public static bool ShouldInit { get; set; } = true;
        public static bool AttackNeutrals { get; set; } = false;
        public static Vector HomeBase { get; set; }
        public static Vector TheirBase { get; set; }
        public static Faction They { get; set; }
        public bool IsEnemy
        {
            get
            {
                return Unit.Faction == They ||
                  (Unit.Faction == Faction.Neutral &&
                      (AttackNeutrals || Unit.Life < Unit.MaxLife));
            }
        }

        public Vector Position { get; internal set; }
        public static Vector MyPosition { get; internal set; }
        public int AttackValue { get {
                int res = -(int)Distance;
                if(Unit.Life <= RangedDamage) {
                    res += 100000;
                    return res;
                }
                if(Unit is Minion) 
                    res += 1000 ; 
                else
                if(Unit is Wizard)
                    res += 3000;
                else
                if(Unit is Building) {
                    res += ((Building)Unit).Type == BuildingType.GuardianTower ? 4000 : 5000;
                }
                return res;
            }
        }
        public UnitType Type { get; set; }
        public static Building HomeThrone { get; internal set; }
        public static World World { get; internal set; }
        public static int RangedDamage { get; set; }

        public UnitInfo(LivingUnit unit) {
            Unit = unit;
            Distance = Unit.GetDistanceTo(Me);
            Position = new Vector(unit.X, unit.Y);

            if(Unit is Minion)
                Type = UnitType.Minion;
            else
            if(Unit is Wizard)
                Type = UnitType.Wizard;
            else
            if(Unit is Building)
                Type = UnitType.Building;
            else
                Type = UnitType.Tree;
        }

        internal static void SetParams() {
            var myP = new Vector(Me.X, Me.Y);
            HomeBase = (new Vector(2000, 2000) - myP).SetLength(750) + myP;
            TheirBase = new Vector(HomeBase.Y, HomeBase.X);
            // HomeBase = Me.Faction == Faction.Academy ? new Vector(200, 3800) : new Vector(3800, 200);
            //TheirBase = Me.Faction == Faction.Renegades ? new Vector(200, 3800) : new Vector(3800, 200);
            They = Me.Faction == Faction.Academy ? Faction.Renegades : Faction.Academy;
            HomeThrone = World.Buildings.FirstOrDefault(b=>b.Type == BuildingType.FactionBase && b.Faction == Me.Faction);
            ShouldInit = false;
        }
        internal double DotValueInFight(Vector dot) {
            //TODo: include ray!

            return 1000 / Unit.GetDistanceTo(dot.X, dot.Y) + (IsEnemy ? -1 : Distance);
        }
        public override string ToString() {
            return Unit.Faction.ToString() + " " + Unit.GetType().Name + ", d:" + Distance;
        }

        public double GetCastRange() {
            if(Game == null) return Me.CastRange;
            switch(Type) {
                case UnitType.Minion:
                    return Game.FetishBlowdartAttackRange;
                case UnitType.Wizard:
                    return ((Wizard)Unit).CastRange;
                case UnitType.Building:
                    return ((Building)Unit).AttackRange;
            }
            return Me.CastRange;
        }
    }
}