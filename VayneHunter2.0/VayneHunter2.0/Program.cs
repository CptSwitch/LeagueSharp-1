using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Color = System.Drawing.Color;

namespace VayneHunter2._0
{
    class Program
    {
        public static String champName = "Vayne";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static Menu menu;
        public static string[] interrupt;
        public static string[] notarget;
        public static string[] gapcloser;
        public static Obj_AI_Hero tar;
        public static Dictionary<string, SpellSlot> spellData;
        public static Dictionary<Obj_AI_Hero, Vector3> dirDic, lastVecDic= new Dictionary<Obj_AI_Hero,Vector3>();
        public static Dictionary<Obj_AI_Hero, float> angleDic = new Dictionary<Obj_AI_Hero,float>();
        public static Vector3 currentVec, lastVec;
        public static bool sol=false;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (player.BaseSkinName != champName) return;
            menu = new Menu("Vayne Hunter", "VHMenu",true);
            menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker1"));
            Orbwalker = new Orbwalking.Orbwalker(menu.SubMenu("Orbwalker1"));
            var ts = new Menu("Target Selector", "TargetSelector");
            SimpleTs.AddToMenu(ts);
            menu.AddSubMenu(ts);
            menu.AddSubMenu(new Menu("[Hunter]Combo", "Combo"));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            menu.AddSubMenu(new Menu("[Hunter]Mixed Mode", "Harrass"));
            menu.SubMenu("Harrass").AddItem(new MenuItem("UseQH", "Use Q").SetValue(true));
            //menu.SubMenu("Harrass").AddItem(new MenuItem("UseEH", "Use E").SetValue(true));
            //menu.SubMenu("Harrass").AddItem(new MenuItem("UseQPH", "Use Q&Auto While they auto minions").SetValue(true));
            menu.AddSubMenu(new Menu("[Hunter]Misc", "Misc"));
            menu.SubMenu("Misc").AddItem(new MenuItem("AntiGP", "Use AntiGapcloser").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("Interrupt", "Interrupt Spells").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("ENextAuto", "Use E after next AA").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
            menu.SubMenu("Misc").AddItem(new MenuItem("AdvE", "Use AdvE logic").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("SmartQ", "WIP Use Q for GapClose").SetValue(false));
            menu.SubMenu("Misc").AddItem(new MenuItem("UsePK", "Use Packets").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("AutoE", "Use Auto E").SetValue(false));
            menu.SubMenu("Misc").AddItem(new MenuItem("PushDistance", "E Push Dist").SetValue(new Slider(425, 400, 475)));
            menu.AddSubMenu(new Menu("[Hunter]Items", "Items"));
            menu.SubMenu("Items").AddItem(new MenuItem("Botrk", "Use BOTRK").SetValue(true));
            menu.SubMenu("Items").AddItem(new MenuItem("Youmuu", "Use Youmuu").SetValue(true));
            menu.SubMenu("Items").AddItem(new MenuItem("OwnHPercBotrk", "Min Own H % Botrk").SetValue(new Slider(50, 1, 100)));
            menu.SubMenu("Items").AddItem(new MenuItem("EnHPercBotrk", "Min Enemy H % Botrk").SetValue(new Slider(20, 1, 100)));
            menu.SubMenu("Items").AddItem(new MenuItem("ItInMix", "Use Items In Mixed Mode").SetValue(false));
            menu.AddSubMenu(new Menu("[Hunter]Mana Mng", "ManaMan"));
            menu.SubMenu("ManaMan").AddItem(new MenuItem("QManaC", "Min Q Mana in Combo").SetValue(new Slider(30, 1, 100)));
            menu.SubMenu("ManaMan").AddItem(new MenuItem("QManaM", "Min Q Mana in Mixed").SetValue(new Slider(30, 1, 100)));
            menu.SubMenu("ManaMan").AddItem(new MenuItem("EManaC", "Min E Mana in Combo").SetValue(new Slider(20, 1, 100)));
            menu.SubMenu("ManaMan").AddItem(new MenuItem("EManaM", "Min E Mana in Mixed").SetValue(new Slider(20, 1, 100)));
            //menu.AddSubMenu(new Menu("[Hunter]WIP", "ezCondemn"));
           // menu.SubMenu("ezCondemn").AddItem(new MenuItem("CheckDistance", "Condemn check Distance").SetValue(new Slider(25, 1, 200)));
            //menu.SubMenu("ezCondemn").AddItem(new MenuItem("Checks", "Num of Checks").SetValue(new Slider(3, 0, 5)));
            //menu.SubMenu("ezCondemn").AddItem(new MenuItem("MaxDistance", "Max Condemn Distance").SetValue(new Slider(1000, 0, 1500)));
            //Thank you blm95 ;)
            menu.AddSubMenu(new Menu("[Hunter]Gapcloser", "gap"));
            menu.AddSubMenu(new Menu("[Hunter]Gapcloser 2", "gap2"));
            menu.AddSubMenu(new Menu("[Hunter]Interrupts", "int"));
            GPIntmenuCreate();
           // initHeroes();
            
            menu.AddToMainMenu();
            Q = new Spell(SpellSlot.Q, 0f);
            E = new Spell(SpellSlot.E, 550f);
            R = new Spell(SpellSlot.R, 0f);
            E.SetTargetted(0.25f, 2200f);
            Game.OnGameUpdate += OnTick;
            Orbwalking.AfterAttack += OW_AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
        }

       

        public static void OW_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (unit.IsMe)
            {
                Obj_AI_Hero targ = (Obj_AI_Hero)target;
                if (!targ.IsValidTarget()) { return; }
                if (isEnK("ENextAuto"))
                {
                    CastE(targ);
                    menu.Item("ENextAuto").SetValue<KeyBind>(new KeyBind("E".ToCharArray()[0], KeyBindType.Toggle));
                }
                if (isEn("UseQ") && isMode("Combo"))
                {
                    if (isEn("UseR"))
                    {
                        R.Cast();
                    }
                    CastQ(targ);
                }
                if (isEn("UseQH") && isMode("Mixed"))
                {
                    CastQ(targ);
                }
                if (isMode("Combo"))
                {
                    useItems(targ);
                }
                if (isMode("Mixed") && isEn("ItInMix"))
                {
                    useItems(targ);
                }
            }
        }
            
        public static void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            String spellName = args.SData.Name;
            //Interrupts
            if(isEn(spellName) && sender.IsValidTarget(550f) && isEn("Interrupt"))
            {
                CastE((Obj_AI_Hero)sender,true);
            }
            //Targeted GapClosers
            if (isEn(spellName) && sender.IsValidTarget(550f) && isEn("AntiGP") && gapcloser.Any(str => str.Contains(args.SData.Name)) 
                && args.Target.IsMe)
            {
                CastE((Obj_AI_Hero)sender,true);
            }
            //NonTargeted GP
            if (isEn(spellName) && sender.IsValidTarget(550f) && isEn("AntiGP") && notarget.Any(str => str.Contains(args.SData.Name)) 
                && player.Distance(args.End)<=320f)
            {
                CastE((Obj_AI_Hero)sender,true);
            }
        }
        public static void OnTick(EventArgs args)
        {
            
            if (!isMode("Combo") || !isEn("UseE") || !E.IsReady()) { return; }
            if (!isEn("AdvE"))
            {
                foreach (var hero in from hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(550f))
                                     let prediction = E.GetPrediction(hero)
                                     where NavMesh.GetCollisionFlags(
                                         prediction.UnitPosition.To2D().Extend(ObjectManager.Player.ServerPosition.To2D(),-menu.Item("PushDistance").GetValue<Slider>().Value).To3D())
                                         .HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(
                                             prediction.UnitPosition.To2D()
                                                 .Extend(ObjectManager.Player.ServerPosition.To2D(),
                                                     -(menu.Item("PushDistance").GetValue<Slider>().Value / 2))
                                                 .To3D())
                                             .HasFlag(CollisionFlags.Wall)
                                     select hero)
                {
                    CastE(hero);
                }
            }
            else
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
                {
                    if (hero.IsValid && !hero.IsDead && hero.IsVisible && player.Distance(hero) < 715f && player.Distance(hero) > 0f)
                    {
                        
                        var pred = E.GetPrediction(hero);
                        var pushDist = menu.Item("PushDistance").GetValue<Slider>().Value;
                        Vector3 enemyPosition = pred.UnitPosition;
                        if(pred.Hitchance>HitChance.Low)
                        {
                            for(int i=1;i<pushDist;i+=60)
                            {

                                Vector2 checker = (pred.UnitPosition.To2D() - player.ServerPosition.To2D());
                                checker.Normalize();
                                var CheckPosition = pred.UnitPosition.To2D() + checker * i;
                                if(IsWall(CheckPosition.To3D()))
                                {
                                    CastE(hero);
                                    break;
                                    
                                }

                            }
                        }
                    }

                }
            }
            if (isEn("AutoE"))
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
                {
                    if (hero.IsValid && !hero.IsDead && hero.IsVisible && player.Distance(hero) < 715f && player.Distance(hero) > 0f)
                    {

                        var pred = E.GetPrediction(hero);
                        var pushDist = menu.Item("PushDistance").GetValue<Slider>().Value;
                        Vector3 enemyPosition = pred.UnitPosition;
                        if (pred.Hitchance > HitChance.Low)
                        {
                            for (int i = 1; i < pushDist; i += 60)
                            {

                                Vector2 checker = (pred.UnitPosition.To2D() - player.ServerPosition.To2D());
                                checker.Normalize();
                                var CheckPosition = pred.UnitPosition.To2D() + checker * i;
                                if (IsWall(CheckPosition.To3D()))
                                {
                                    CastE(hero);
                                    break;

                                }

                            }
                        }
                    }

                }
            }
        }
        
         public static bool IsWall(Vector3 position)
       {
           var cFlags = NavMesh.GetCollisionFlags(position);
            return (cFlags == CollisionFlags.Wall || cFlags == CollisionFlags.Building || cFlags == CollisionFlags.Prop);
        }
        static void UpdateHeroes()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(550f)))
            {
                currentVec = hero.Position;
                Vector3 direction = Vector3.Subtract(currentVec,lastVec);
                
                if(!(direction == new Vector3(0,0,0)))
                {
                    direction.Normalize();
                }
                float angle = Vector3.Dot(direction,direction);
                lastVecDic[hero] = currentVec;
                dirDic[hero] = direction;
                angleDic[hero] = angle;
                
            }
        }
        static Vector3 condemnCollisionTime(Obj_AI_Hero target)
        {
            Vector3 dir = dirDic[target];
            float angle = angleDic[target];
            if(!(dir==new Vector3(0,0,0)))
            {
                Vector3 windup = target.Position + dir * (target.MoveSpeed * 250 / 1000);
                float time = (float)GetCollisionTime(windup, dir, target.MoveSpeed, player.Position, 1600f);
                if(time == 0)
                {
                    return new Vector3(0, 0, 0);
                }
                
                Vector3 returner = target.Position + dir * (target.MoveSpeed * (time+0.25f))/2;
                return returner;
            }
            return new Vector3(0,0,0);
        }
        //Thanks Yomie
        static double GetCollisionTime(Vector3 position,Vector3 direction,float tSpeed,Vector3 sourcePos,float projSpeed)
        {
            var velocity = direction * tSpeed;
            float velocityX = velocity.X;
	        float velocityY = velocity.Z;
	
	        Vector3 relStart = position - sourcePos;

            float relStartX = relStart.X;
	        float relStartY = relStart.Z;
            float a = velocityX * velocityX + velocityY * velocityY - projSpeed * projSpeed;
	        float b = 2 * velocityX * relStartX + 2 * velocityY * relStartY;
	        float c = relStartX * relStartX + relStartY * relStartY;
         	float disc = b * b - 4 * a * c;
	
	        if(disc >= 0){
		        double t1 = -( b + Math.Sqrt( disc )) / (2 * a );
		        double t2 = -( b - Math.Sqrt( disc )) / (2 * a );
		        if(t1!=null && t2 != null && t1 > 0 && t2 > 0){
			        if (t1 > t2){
				        return t2;
			        }else{
			    	    return t1;
			        }
                }
		        else if(t1!=null && t1 > 0){
			        return t1;
		        }else if(t2!=null && t2 > 0){
			        return t2;
		        }
            }
            return 0;  
        }
        static void GPIntmenuCreate()
        {
            gapcloser = new[]
            {
                "AkaliShadowDance", "Headbutt", "DianaTeleport", "IreliaGatotsu", "JaxLeapStrike", "JayceToTheSkies",
                "MaokaiUnstableGrowth", "MonkeyKingNimbus", "Pantheon_LeapBash", "PoppyHeroicCharge", "QuinnE",
                "XenZhaoSweep", "blindmonkqtwo", "FizzPiercingStrike", "RengarLeap"
            };
            notarget = new[]
            {
                "AatroxQ", "GragasE", "GravesMove", "HecarimUlt", "JarvanIVDragonStrike", "JarvanIVCataclysm", "KhazixE",
                "khazixelong", "LeblancSlide", "LeblancSlideM", "LeonaZenithBlade", "UFSlash", "RenektonSliceAndDice",
                "SejuaniArcticAssault", "ShenShadowDash", "RocketJump", "slashCast"
            };
            interrupt = new[]
            {
                "KatarinaR", "GalioIdolOfDurand", "Crowstorm", "Drain", "AbsoluteZero", "ShenStandUnited", "UrgotSwap2",
                "AlZaharNetherGrasp", "FallenOne", "Pantheon_GrandSkyfall_Jump", "VarusQ", "CaitlynAceintheHole",
                "MissFortuneBulletTime", "InfiniteDuress", "LucianR"
            };
            for (int i = 0; i < gapcloser.Length; i++)
            {
                menu.SubMenu("gap").AddItem(new MenuItem(gapcloser[i], gapcloser[i])).SetValue(true);
            }
            for (int i = 0; i < notarget.Length; i++)
            {
                menu.SubMenu("gap2").AddItem(new MenuItem(notarget[i], notarget[i])).SetValue(true);
            }
            for (int i = 0; i < interrupt.Length; i++)
            {
                menu.SubMenu("int").AddItem(new MenuItem(interrupt[i], interrupt[i])).SetValue(true);
            }
        }
        
        public static void CastQ(Obj_AI_Hero targ)
        {
                if(Q.IsReady())
                {
                    if(isEn("SmartQ") && player.Distance(targ)>=Orbwalking.GetRealAutoAttackRange(null))
                    {
                        if (isMode("Combo") && getManaPer() >= menu.Item("QManaC").GetValue<Slider>().Value)
                        {
                            float tumbleRange = 300f;
                            bool canGapclose = player.Distance(targ) <= Orbwalking.GetRealAutoAttackRange(null) + tumbleRange;
                            if ((player.Distance(targ) >= Orbwalking.GetRealAutoAttackRange(null)))
                            {
                                if (canGapclose)
                                {
                                    Vector3 PositionForQ = new Vector3(targ.Position.X, targ.Position.Y, targ.Position.Z);
                                    Q.Cast(PositionForQ, isEn("UsePK"));
                                }
                            }
                        }
                        else if (isMode("Mixed") && getManaPer() >= menu.Item("QManaM").GetValue<Slider>().Value)
                        {
                            float tumbleRange = 300f;
                            bool canGapclose = player.Distance(targ) <= Orbwalking.GetRealAutoAttackRange(null) + tumbleRange;
                            if ((player.Distance(targ) >= Orbwalking.GetRealAutoAttackRange(null)))
                            {
                                if (canGapclose)
                                {
                                    Vector3 PositionForQ = new Vector3(targ.Position.X, targ.Position.Y, targ.Position.Z);
                                    Q.Cast(PositionForQ, isEn("UsePK"));
                                }
                            }
                        }
                    }else{ 
                    if(isMode("Combo") && getManaPer()>= menu.Item("QManaC").GetValue<Slider>().Value)
                    {
                        Q.Cast(Game.CursorPos, isEn("UsePK"));
                    }else if(isMode("Mixed") && getManaPer()>= menu.Item("QManaM").GetValue<Slider>().Value)
                    {
                        Q.Cast(Game.CursorPos, isEn("UsePK"));
                    } 
                    }
                    
                }
        }
        static void CastE(Obj_AI_Hero Target,bool forGp=false)
        {
            if (E.IsReady() && player.Distance(Target) < 550f)
            {
                if(!forGp)
                { 
                    if (isMode("Combo") && getManaPer() >= menu.Item("EManaC").GetValue<Slider>().Value)
                    {
                        E.Cast(Target, isEn("UsePK"));
                    }
                    else if (isMode("Mixed") && getManaPer() >= menu.Item("EManaM").GetValue<Slider>().Value)
                    {
                        E.Cast(Target, isEn("UsePK"));
                    }
                }else
                {
                    E.Cast(Target, isEn("UsePK"));
                }
            }
        }
        static void useItems(Obj_AI_Hero tar)
        {
            float OwnH = getPlHPer();
            if (menu.Item("Botrk").GetValue<bool>() && (menu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= OwnH) && ((menu.Item("EnHPercBotrk").GetValue<Slider>().Value <= getEnH(tar))))
            {
                useItem(3153, tar);
            }
            if (menu.Item("Youmuu").GetValue<bool>())
            {
                useItem(3142);
            }
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
        static bool isMode(String mode)
        {
            return (Orbwalker.ActiveMode.ToString() == mode);
        }
        public static void useItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }
        public static float getEnH(Obj_AI_Hero target)
        {
            float h = (target.Health / target.MaxHealth) * 100;
            return h;
        }
        public static float getPlHPer()
        {
            float h = (player.Health / player.MaxHealth) * 100;
            return h;
        }
    }
}
