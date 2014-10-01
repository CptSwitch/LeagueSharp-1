using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Color = System.Drawing.Color;

namespace DZDraven
{
    class Program
    {
        public static String charName = "Draven"; //Not Draven,Draaaaaaaaaaven
        public static List<Reticle> reticleList = new List<Reticle>();
        public static List<Obj_AI_Turret> towerPos = new List<Obj_AI_Turret>();
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static Menu menu;
        public static float autoRange = 550f;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (player.BaseSkinName != charName) { return; }
            menu = new Menu("DZDraven", "DZdrvenMenu", true);
            menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker1"));
            Orbwalker = new Orbwalking.Orbwalker(menu.SubMenu("Orbwalker1"));
            var ts = new Menu("Target Selector", "TargetSelector");
            SimpleTs.AddToMenu(ts);
            menu.AddSubMenu(ts);
            menu.AddSubMenu(new Menu("[Draven]Skill Q", "QMenu"));
            //Q Menu
            
            menu.SubMenu("QMenu").AddItem(new MenuItem("QC", "Use Q Combo").SetValue(true));
            
            menu.SubMenu("QMenu").AddItem(new MenuItem("QM", "Use Q Mixed").SetValue(false));
            menu.SubMenu("QMenu").AddItem(new MenuItem("QLH", "Use Q LastHit").SetValue(false));
            
            menu.SubMenu("QMenu").AddItem(new MenuItem("QLC", "Use Q LaneClear").SetValue(false));
            menu.SubMenu("QMenu").AddItem(new MenuItem("QKs", "Use Q Ks").SetValue(true));
            menu.SubMenu("QMenu").AddItem(new MenuItem("MaxQNum", "Max n of Q").SetValue(new Slider(2, 1, 4)));
            menu.SubMenu("QMenu").AddItem(new MenuItem("SafeZone", "BETA SafeZone").SetValue(new Slider(100, 0, 400)));
            menu.SubMenu("QMenu").AddItem(new MenuItem("QRadius", "Catch Radius").SetValue(new Slider(600, 200, 800)));      
            menu.SubMenu("QMenu").AddItem(new MenuItem("QManaC", "Min Q Mana in Combo").SetValue(new Slider(10, 1, 100)));
            menu.SubMenu("QMenu").AddItem(new MenuItem("QManaM", "Min Q Mana in Mixed").SetValue(new Slider(10, 1, 100)));
            menu.SubMenu("QMenu").AddItem(new MenuItem("UseAARet", "Use AA while orbwalking to reticle").SetValue(true));
            menu.SubMenu("QMenu").AddItem(new MenuItem("QRefresh", "Refresh List (if bug)").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));

            menu.AddSubMenu(new Menu("[Draven]Skill W", "WMenu"));
            
            //W Menu
            menu.SubMenu("WMenu").AddItem(new MenuItem("WC", "Use W Combo").SetValue(true));
            menu.SubMenu("WMenu").AddItem(new MenuItem("WM", "Use W Mixed").SetValue(true));
            menu.SubMenu("WMenu").AddItem(new MenuItem("WLH", "Use W LastHit").SetValue(false));
            menu.SubMenu("WMenu").AddItem(new MenuItem("WLC", "Use W LaneClear").SetValue(false));
            menu.SubMenu("WMenu").AddItem(new MenuItem("WManaC", "Min W Mana in Combo").SetValue(new Slider(60, 1, 100)));
            menu.SubMenu("WMenu").AddItem(new MenuItem("WManaM", "Min W Mana in Mixed").SetValue(new Slider(60, 1, 100)));


            menu.AddSubMenu(new Menu("[Draven]Skill E", "EMenu"));

            //E Menu
            menu.SubMenu("EMenu").AddItem(new MenuItem("EC", "Use E Combo").SetValue(true));
            menu.SubMenu("EMenu").AddItem(new MenuItem("EM", "Use E Mixed").SetValue(false));
            menu.SubMenu("EMenu").AddItem(new MenuItem("EKs", "Use E Ks").SetValue(true));
            menu.SubMenu("EMenu").AddItem(new MenuItem("EGapCloser", "Use E AntiGapcloser").SetValue(true));
            menu.SubMenu("EMenu").AddItem(new MenuItem("EInterrupt", "Use E Interrupt").SetValue(true));
            menu.SubMenu("EMenu").AddItem(new MenuItem("EManaC", "Min E Mana in Combo").SetValue(new Slider(20, 1, 100)));
            menu.SubMenu("EMenu").AddItem(new MenuItem("EManaM", "Min R Mana in Mixed").SetValue(new Slider(20, 1, 100)));


            menu.AddSubMenu(new Menu("[Draven]Skill R (2000un)", "RMenu"));

            //R Menu
            menu.SubMenu("RMenu").AddItem(new MenuItem("RC", "Use R Combo").SetValue(false));
            menu.SubMenu("RMenu").AddItem(new MenuItem("RM", "Use R Mixed").SetValue(false));
            menu.SubMenu("RMenu").AddItem(new MenuItem("RKs", "Use R Ks").SetValue(true));
            //menu.SubMenu("RMenu").AddItem(new MenuItem("ManualR", "Manual R Cast").SetValue(new KeyBind("T".ToCharArray()[0],KeyBindType.Press)));
            menu.SubMenu("RMenu").AddItem(new MenuItem("RManaC", "Min R Mana in Combo").SetValue(new Slider(5, 1, 100)));
            menu.SubMenu("RMenu").AddItem(new MenuItem("RManaM", "Min R Mana in Mixed").SetValue(new Slider(5, 1, 100)));
            
            //Axe Catcher
            menu.AddSubMenu(new Menu("[Draven]Axe Catcher", "AxeCatcher"));

            menu.SubMenu("AxeCatcher").AddItem(new MenuItem("ACC", "AxeC Combo").SetValue(true));
            menu.SubMenu("AxeCatcher").AddItem(new MenuItem("ACM", "AxeC Mixed").SetValue(true));
            menu.SubMenu("AxeCatcher").AddItem(new MenuItem("ACLH", "Axe CLastHit").SetValue(true));
            menu.SubMenu("AxeCatcher").AddItem(new MenuItem("ACLC", "AxeC LaneClear").SetValue(true));

            menu.AddSubMenu(new Menu("[Draven]Items", "Items"));

            //Items Menu
            menu.SubMenu("Items").AddItem(new MenuItem("BOTRK", "Use BOTRK").SetValue(true));
            menu.SubMenu("Items").AddItem(new MenuItem("Youmuu", "Use Youmuu").SetValue(true));
            menu.SubMenu("Items").AddItem(new MenuItem("SOTD", "Use SOTD if Oneshot").SetValue(true));


            menu.AddSubMenu(new Menu("[Draven]Drawing", "Drawing"));

            //Drawings Menu
            menu.SubMenu("Drawing").AddItem(new MenuItem("DrawCRange", "Draw CatchRange").SetValue(new Circle(true,  Color.FromArgb(80, 255, 0, 255))));
            menu.SubMenu("Drawing").AddItem(new MenuItem("DrawRet", "Draw Reticles").SetValue(new Circle(true,Color.Yellow)));
            
            
            menu.AddToMainMenu();
            Game.PrintChat("DZDraven 1.22 Loaded.");
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 20000);
            E.SetSkillshot(250f, 130f, 1400f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(400f, 160f, 2000f, false, SkillshotType.SkillshotLine);
            compileTowerArray();
            GameObject.OnCreate += OnCreateObject;
            GameObject.OnDelete += OnDeleteObject;
            AntiGapcloser.OnEnemyGapcloser += OnGapcloser;
            Interrupter.OnPossibleToInterrupt += OnInterruptCreate;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            var QRadius = menu.Item("QRadius").GetValue<Slider>().Value;
            var drawCatch = menu.Item("DrawCRange").GetValue<Circle>();
            var drawRet = menu.Item("DrawRet").GetValue<Circle>();
            if(drawCatch.Active)
            {
                Drawing.DrawCircle(Game.CursorPos, QRadius, drawCatch.Color);
            }
            if (drawRet.Active)
            {
                foreach(Reticle r in reticleList)
                {
                    if(r.getObj().IsValid)
                    {
                        Drawing.DrawCircle(r.getPosition(), 100 , drawRet.Color);
                    }
                }
                
            }
        }

        static void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (!unit.IsMe) { return; }
            
            var tar = (Obj_AI_Hero)target;
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if(isEn("WC"))
                    {
                        var WManaCombo = menu.Item("WManaC").GetValue<Slider>().Value;
                        if (getManaPer() >= WManaCombo) { W.Cast(); }
                    }
                    //Botrk
                    if (isEn("BOTRK"))
                    {
                        useItem(3153, (Obj_AI_Hero)target);
                    }
                    //Youmuu
                    if (isEn("Youmuu"))
                    {
                        useItem(3142);
                    }
                    //SOTD
                    if (isEn("SOTD"))
                    {
                        var hasIE = Items.HasItem(3031);
                        var coeff = hasIE ? 2.5 : 2.0;
                        if ((player.GetAutoAttackDamage(target) * coeff * 3 >= target.Health))
                        {
                            useItem(3131);
                            Orbwalker.ForceTarget(target);
                        }
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (isEn("WM"))
                    {
                        var WManaMix = menu.Item("WManaM").GetValue<Slider>().Value;
                        if (getManaPer() >= WManaMix) { W.Cast(); }
                    }
                   
                    break;

                case Orbwalking.OrbwalkingMode.LastHit:
                    if (isEn("QLH")) { CastQ(); }
                    if (isEn("WLH")) { W.Cast(); }
                    
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (isEn("WLC")) { W.Cast(); }
                    if (isEn("QLC")) { CastQ(); }
                    
                    break;
                default:
                    return;
            }
        }
        private static bool PlayerInTurretRange()
        {
            foreach(var val in towerPos)
            {
                if(val.Health == 0)
                {
                    towerPos.Remove(val);
                }
            }
            foreach (var val in towerPos)
            {
                if(player.Distance(val)< 975f)
                {
                    return true;
                }
            }
            return false;
        }
        private static bool RetInTurretRange(Vector3 retPosition)
        {
            foreach (var val in towerPos)
            {
                if (val.Health == 0)
                {
                    towerPos.Remove(val);
                }
            }
            foreach (var val in towerPos)
            {
                if (Vector3.Distance(retPosition,val.Position) < 975f)
                {
                    return true;
                }
            }
            return false;
        }
        private static void compileTowerArray()
        {
            foreach(var tower in ObjectManager.Get<Obj_AI_Turret>().Where(tower=>tower.IsEnemy))
            {
                towerPos.Add(tower);
            }
        }
        private static bool IsZoneSafe(Vector3 v, float dist)
        {
            foreach(var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
            {
                if(Vector3.Distance(enemy.Position,v)< dist && !enemy.IsDead && enemy!=null)
                {
                    return false;
                }
            }
            return true;
        }
        private static Obj_AI_Hero ClosestHero(float range)
        {
             Obj_AI_Hero clhero = null;
            foreach(var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                if(!hero.IsDead && hero.IsVisible && player.Distance(hero)<player.Distance(clhero))
                {
                    clhero = hero;
                }
            }
            return clhero;
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            var safeZone = menu.Item("SafeZone").GetValue<Slider>().Value;
            var target = SimpleTs.GetTarget(550f, SimpleTs.DamageType.Physical);
            var ETarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
            var RTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Physical);
            //
            if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && isEn("ACC"))
            {
                OrbWalkToReticle(safeZone, 100);
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && isEn("ACM"))
            {
                OrbWalkToReticle(safeZone, 100);
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit && isEn("ALH"))
            {
                OrbWalkToReticle(safeZone, 100);
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && isEn("ALC"))
            {
                OrbWalkToReticle(safeZone, 100);
            }
                foreach(var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero=>hero.IsEnemy))
                {
                    if (isEn("EKs"))
                    {
                        var ePred = E.GetPrediction(hero);
                        if (E.GetHealthPrediction(hero) <= 0)
                        {
                            E.Cast(hero);
                            break;
                        }
                        
                    }
                    if(isEn("QKs"))
                    {
                        if(Q.GetDamage(hero)+player.GetAutoAttackDamage(hero)>=hero.Health)
                        {
                            if(GetQNumber()<1){Q.Cast();}
                            Orbwalker.SetAttacks(true);
                            Orbwalker.ForceTarget(hero);
                            break;
                        }
                    }
                    if (isEn("RKs"))
                    {
                        var RPred = R.GetPrediction(hero);
                        if (R.GetHealthPrediction(hero)<=0 && player.Distance(hero)<=2000f)
                        {
                            R.Cast(hero);
                            break;
                        }
                    }
                }
            if(menu.Item("QRefresh").GetValue<KeyBind>().Active)
            {
                reticleList.Clear();
            }
            if (target == null) return;
            switch (Orbwalker.ActiveMode)
            {  
                case Orbwalking.OrbwalkingMode.Combo:
                    if (isEn("QC")) { CastQ(); }
                    if (isEn("EC")) { CastE(ETarget);}
                    if (isEn("RC")) { CastR(RTarget); }
                    if (isEn("ACC")) {  OrbWalkToReticle(safeZone, 100);  }
                    
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (isEn("QM")) { CastQ(); }
                    if (isEn("EM")) { CastE(ETarget); }
                    if (isEn("RM")) { CastR(RTarget); }
                    if (isEn("ACM")) {OrbWalkToReticle(safeZone, 100);  }
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    if (isEn("ACLH")) {  OrbWalkToReticle(safeZone, 100);  }
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (isEn("ACLC")) { OrbWalkToReticle(safeZone, 100);  }
                    break;
                default:
                    break;
            }
        }
        public static void useItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }
        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Q_reticle_self")) { return; }
            reticleList.Add(new Reticle(sender, Game.Time, sender.Position,Game.Time + 1.20, sender.NetworkId));
        }

        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Q_reticle_self")) { return; }
            foreach(Reticle ret in reticleList)
            {
                if(ret.getNetworkId() == sender.NetworkId){reticleList.Remove(ret);}
            }
        }

        private static void OnGapcloser(ActiveGapcloser gapcloser)
        {
            if (!isEn("EGapCloser")) { return; }
            if (gapcloser.End.Distance(player.ServerPosition) <= 50f)
            {
                var EPred = E.GetPrediction(gapcloser.Sender);
                if(EPred.Hitchance>=HitChance.Medium)
                {
                    E.Cast(EPred.CastPosition);
                }
            }
        }

        private static void OnInterruptCreate(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!isEn("EInterrupt")) { return; }
            var EPred = E.GetPrediction(unit);
            if (EPred.Hitchance >= HitChance.Medium)
            {
                E.Cast(EPred.CastPosition);
            }
        }
        private static bool IsInStandRange()
        {
            return (Vector3.Distance(Game.CursorPos,player.Position)<220);
        }
        /// <summary>
        /// Orbwalker to catch the axes.
        /// </summary>
        /// <param name="SafeZone">Heroes safe zone,def 100</param>
        /// <param name="RetSafeZone">Reticle safe zone, def 100</param>
        private static void OrbWalkToReticle(int SafeZone,int RetSafeZone)
        {
            bool toggle = isEn("UseAARet");
            var target = ClosestHero(900f);
            Reticle ClosestRet = null;
            var QRadius = menu.Item("QRadius").GetValue<Slider>().Value;
            foreach(Reticle r in reticleList)
            {
                if (!r.getObj().IsValid) { reticleList.Remove(r); }
            }
            if(reticleList.Count >0)
            {
                ClosestRet = reticleList.OrderBy(reticle => reticle.getEndTime()).OrderBy(reticle => reticle.DistanceToPlayer()).FirstOrDefault();
                float closestDist = player.Distance(ClosestRet.getPosition());
                
                foreach(Reticle r in reticleList.OrderBy(reticle=>reticle.getEndTime()).OrderBy(reticle=>reticle.DistanceToPlayer()))
                {
                        if(r.getPosition().Distance(Game.CursorPos)<=QRadius && player.Distance(r.getPosition())< closestDist)
                        {
                            if (IsZoneSafe(r.getPosition(), RetSafeZone) && IsZoneSafe(player.Position,SafeZone) )
                            {
                                ClosestRet = r;
                                closestDist = player.Distance(r.getPosition());
                            }
                        }
                }
            }
            
            if(ClosestRet!=null && !RetInTurretRange(ClosestRet.getPosition()))
            {
                float myHitbox = 65;
                float QDist = Vector2.Distance(ClosestRet.getPosition().To2D(), player.ServerPosition.To2D())-myHitbox;
                float QDist1 = player.GetPath(ClosestRet.getPosition()).ToList().To2D().PathLength();
                bool CanReachRet = ( QDist1 / player.MoveSpeed+Game.Time)<(ClosestRet.getEndTime());
                bool CanReachRetWBonus  = ( QDist1 / (player.MoveSpeed+(player.MoveSpeed*(getMoveSpeedBonusW()/100))) + Game.Time)<(ClosestRet.getEndTime());
                bool WNeeded = false;
                if (CanReachRetWBonus && !CanReachRet)
                {
                    W.Cast();
                    WNeeded = true;
                    
                }
                if((CanReachRet || WNeeded))
                {
                    WNeeded = false;
                    if (!toggle)
                    {
                        Orbwalker.SetAttacks(false);
                    }
                    if (player.Distance(ClosestRet.getPosition()) >= 100)
                    {
                        if (ClosestRet.getPosition() != Game.CursorPos)
                        {
                            Orbwalker.SetOrbwalkingPoint(ClosestRet.getPosition());
                        }
                        else
                        {
                            Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                        } 
                    }
                    if (!toggle)
                    {
                        Orbwalker.SetAttacks(true);
                    }
                    Console.WriteLine("Orbwalking to " + ClosestRet.getPosition().ToString());
                }
                
            }
        }
        public static int getMoveSpeedBonusW()
        {
            switch(W.Level)
            {
                case 1:
                    return 40;
                case 2:
                    return 45;
                case 3:
                    return 50;
                case 4:
                    return 55;
                case 5:
                    return 60;
                default:
                    return 0;
            }
        }
        public static void CastE(Obj_AI_Base unit)
        {
            if (unit == null) { return; }
            var EPrediction = E.GetPrediction(unit);
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    var EManaCombo = menu.Item("EManaC").GetValue<Slider>().Value;
                    if ((getManaPer() >= EManaCombo) ) { E.Cast(unit); }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    var EManaMix = menu.Item("EManaM").GetValue<Slider>().Value;
                    if ((getManaPer() >= EManaMix) ) { E.Cast(unit); }
                    break;
                default:
                    break;
            }
        }
        public static void CastR(Obj_AI_Base unit)
        {
            if (unit == null) { return; }
            var RPrediction = R.GetPrediction(unit);
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    var RManaCombo = menu.Item("RManaC").GetValue<Slider>().Value;
                    if ((getManaPer() >= RManaCombo) && player.Distance(unit) < 2000f) { R.Cast(unit); }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    var RManaMix = menu.Item("RManaM").GetValue<Slider>().Value;
                    if ((getManaPer() >= RManaMix) &&  player.Distance(unit) < 2000f) { R.Cast(unit); }
                    break;
                default:
                    
                    break;
            }
        }
        public static void CastQ()
        {
            var qNumberOnPlayer = GetQNumber();
            if (reticleList.Count + 1 > menu.Item("MaxQNum").GetValue<Slider>().Value) { return; }
            if (qNumberOnPlayer > 2) { return; }
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    var QManaCombo = menu.Item("QManaC").GetValue<Slider>().Value;
                    if (getManaPer() >= QManaCombo) { Q.Cast(); }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    var QManaMix = menu.Item("QManaM").GetValue<Slider>().Value;
                    if (getManaPer() >= QManaMix) { Q.Cast(); }
                    break;
                default:
                    Q.Cast();
                    break;
            }
        }
        public static int GetQNumber()
        {
            var buff = ObjectManager.Player.Buffs.FirstOrDefault(buff1 => buff1.Name.Equals("dravenspinningattack"));
            return buff != null ? buff.Count : 0;
        }
        static bool isEn(String opt)
        {
            return menu.Item(opt).GetValue<bool>();
        }
        public static float getManaPer()
        {
            float mana = (player.Mana / player.MaxMana) * 100;
            return mana;
        }
        static bool isEnK(String opt)
        {
            return menu.Item(opt).GetValue<KeyBind>().Active;
        }
    }
}
