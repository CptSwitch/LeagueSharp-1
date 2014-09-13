using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Color = System.Drawing.Color;

namespace Vayne_The_Hunter_Of_The_Night
{
    class Vayne
    {
        public static String champName = "Vayne";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static Menu VayneMenu;
        public static string[] interrupt;
        public static string[] notarget;
        public static string[] gapcloser;
        public static Obj_AI_Hero tar;
        public static Dictionary<string, SpellSlot> spellData;
        static void Main(string[] args)
        {
            try
            {
                CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
        }
        static void Game_OnGameLoad(EventArgs args)
        {
            if (player.BaseSkinName != champName) return;
            //Q = new Spell(SpellSlot.Q, 0);
            
            //R = new Spell(SpellSlot.Q, 0);
            spellData = new Dictionary<string, SpellSlot>();
            VayneMenu = new Menu("Vayne - Hunter","VayneHMenu",true);
            VayneMenu.AddSubMenu(new Menu("Orbwalker","Orbwalker1"));
            Orbwalker = new Orbwalking.Orbwalker(VayneMenu.SubMenu("Orbwalker1"));
            var ts = new Menu("Target Selector","TargetSelector");
            SimpleTs.AddToMenu(ts);
            VayneMenu.AddSubMenu(ts);
 
            VayneMenu.AddSubMenu(new Menu( "Vayne Combo","Combo"));
            VayneMenu.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            VayneMenu.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            VayneMenu.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            VayneMenu.AddSubMenu(new Menu( "Vayne Harrass","Harrass"));
            VayneMenu.SubMenu("Harrass").AddItem(new MenuItem("UseQM", "Use Q").SetValue(true));
            VayneMenu.SubMenu("Harrass").AddItem(new MenuItem("UseEM", "Use E").SetValue(false));
            VayneMenu.SubMenu("Harrass").AddItem(new MenuItem("3Bolt", "3rd Bolt with E").SetValue(false));
            VayneMenu.AddSubMenu(new Menu("Vayne Anti Gap Closer","AntiGP"));
            VayneMenu.SubMenu("AntiGP").AddItem(new MenuItem("InCombo", "In Combo").SetValue(true));
            VayneMenu.SubMenu("AntiGP").AddItem(new MenuItem("InHarrass", "In Mixed").SetValue(false));
            VayneMenu.SubMenu("AntiGP").AddItem(new MenuItem("InAlwa", "Always").SetValue(true));
            VayneMenu.AddSubMenu(new Menu( "Vayne Items","Items"));
            VayneMenu.SubMenu("Items").AddItem(new MenuItem("Botrk", "Use BOTRK").SetValue(true));
            VayneMenu.SubMenu("Items").AddItem(new MenuItem("Youmuu", "Use Youmuu").SetValue(true));
            VayneMenu.SubMenu("Items").AddItem(new MenuItem("OwnHPercBotrk", "Min Own H % Botrk").SetValue(new Slider(50, 1, 100)));
            VayneMenu.SubMenu("Items").AddItem(new MenuItem("EnHPercBotrk", "Min Enemy H % Botrk").SetValue(new Slider(20, 1, 100)));
            VayneMenu.SubMenu("Items").AddItem(new MenuItem("ItInMix", "Use Items In Mixed Mode").SetValue(false));
            VayneMenu.AddSubMenu(new Menu( "Vayne Mana Manager","ManaMan"));
            VayneMenu.SubMenu("ManaMan").AddItem(new MenuItem("QManaC", "Min Q Mana in Combo").SetValue(new Slider(30, 0, 100)));
            VayneMenu.SubMenu("ManaMan").AddItem(new MenuItem("QManaM", "Min Q Mana in Mixed").SetValue(new Slider(30, 0, 100)));
            VayneMenu.SubMenu("ManaMan").AddItem(new MenuItem("EManaC", "Min E Mana in Combo").SetValue(new Slider(20, 0, 100)));
            VayneMenu.SubMenu("ManaMan").AddItem(new MenuItem("EManaM", "Min E Mana in Mixed").SetValue(new Slider(20, 0, 100)));
            //Thank you blm95 ;)
            VayneMenu.AddSubMenu(new Menu("Gapcloser List", "gap"));
            VayneMenu.AddSubMenu(new Menu("Gapcloser List 2", "gap2"));
            VayneMenu.AddSubMenu(new Menu("Interrupt List", "int"));
            VayneMenu.AddSubMenu(new Menu("Vayne Misc","Misc"));
            VayneMenu.SubMenu("Misc").AddItem(new MenuItem("ENextAuto", "Use E after next AA").SetValue(new KeyBind(69, KeyBindType.Toggle)));
            VayneMenu.SubMenu("Misc").AddItem(new MenuItem("Interrupt", "Interrupt Spells").SetValue(true));
            VayneMenu.SubMenu("Misc").AddItem(new MenuItem("UseRQ", "Use RQ").SetValue(false));
            VayneMenu.SubMenu("Misc").AddItem(new MenuItem("PushDistance", "E Push Dist").SetValue(new Slider(425, 400, 475)));
            Q = new Spell(SpellSlot.Q, 0f);
            E = new Spell(SpellSlot.E, float.MaxValue);
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
                VayneMenu.SubMenu("gap").AddItem(new MenuItem(gapcloser[i], gapcloser[i])).SetValue(true);
            }
            for (int i = 0; i < notarget.Length; i++)
            {
                VayneMenu.SubMenu("gap2").AddItem(new MenuItem(notarget[i], notarget[i])).SetValue(true);
            }
            for (int i = 0; i < interrupt.Length; i++)
            {
                VayneMenu.SubMenu("int").AddItem(new MenuItem(interrupt[i], interrupt[i])).SetValue(true);
            }
            
            E.SetTargetted(0.25f, 2200f);
            Game.PrintChat("Vayne - The Hunter of The Night By DZ191 Loaded");
            Obj_AI_Base.OnProcessSpellCast += Game_ProcessSpell;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
           // Obj_AI_Base.OnPlayAnimation += OnRengarAnimation;
            VayneMenu.AddToMainMenu();
        }
        public static void Game_ProcessSpell(Obj_AI_Base hero, GameObjectProcessSpellCastEventArgs args)
        {
            if (VayneMenu.Item("Interrupt").GetValue<bool>() && hero.IsValidTarget(550f) && VayneMenu.Item(args.SData.Name).GetValue<bool>())
            {
                if (interrupt.Any(str => str.Contains(args.SData.Name)))
                {
                    E.Cast(hero,true);
                }
            }
            if (gapcloser.Any(str => str.Contains(args.SData.Name)) && args.Target == ObjectManager.Player &&
                hero.IsValidTarget(550f) && VayneMenu.Item(args.SData.Name).GetValue<bool>() && VayneMenu.Item("InAlwa").GetValue<bool>())
            {
                E.Cast(hero,true);
            }
            if (Orbwalker.ActiveMode.ToString() == "Combo" && VayneMenu.Item("InCombo").GetValue<bool>() && gapcloser.Any(str => str.Contains(args.SData.Name)) && args.Target == ObjectManager.Player &&
                hero.IsValidTarget(550f) && VayneMenu.Item(args.SData.Name).GetValue<bool>())
            {
                E.Cast(hero,true);
            }
            if (Orbwalker.ActiveMode.ToString() == "Mixed" && VayneMenu.Item("InHarrass").GetValue<bool>() && gapcloser.Any(str => str.Contains(args.SData.Name)) && args.Target == ObjectManager.Player &&
                hero.IsValidTarget(550f) && VayneMenu.Item(args.SData.Name).GetValue<bool>())
            {
                E.Cast(hero,true);
            }
            if (notarget.Any(str => str.Contains(args.SData.Name)) &&
                Vector3.Distance(args.End, ObjectManager.Player.Position) <= 300 && hero.IsValidTarget(550f) &&
                VayneMenu.Item(args.SData.Name).GetValue<bool>())
            {
                E.Cast(hero,true);
            }
        }
        
        public static void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (unit.IsMe)
            {
                
                tar = (Obj_AI_Hero)target;

                if (VayneMenu.Item("ENextAuto").GetValue<KeyBind>().Active)
                {
                    E.Cast(target,true);
                    VayneMenu.Item("ENextAuto").SetValue<KeyBind>(new KeyBind("E".ToCharArray()[0], KeyBindType.Toggle));
                }

                if ((Orbwalker.ActiveMode.ToString() == "Combo" || Orbwalker.ActiveMode.ToString() == "Mixed") && VayneMenu.Item("UseQ").GetValue<bool>()  &&
                    Q.IsReady())
                {
                    var after = ObjectManager.Player.Position + Normalize(Game.CursorPos - ObjectManager.Player.Position) * 300;

                    var disafter = Vector3.DistanceSquared(after, tar.Position);
                    if ((disafter < 630 * 630) && disafter > 150 * 150)
                    {
                        float ManaVal1 = 1;

                        if (Orbwalker.ActiveMode.ToString() == "Combo")
                        {
                            ManaVal1 = VayneMenu.Item("QManaC").GetValue<Slider>().Value;
                        }
                        else if (Orbwalker.ActiveMode.ToString() == "Mixed")
                        {
                            ManaVal1 = VayneMenu.Item("QManaM").GetValue<Slider>().Value;
                        }
                        //Game.PrintChat((getManaPer() >= ManaVal1).ToString());
                        if (getManaPer() >= ManaVal1)
                        {
                            Q.Cast(Game.CursorPos,true);
                            if (VayneMenu.Item("UseR").GetValue<bool>() == true && R.IsReady() && VayneMenu.Item("UseRQ").GetValue<bool>() == true)
                            {
                                R.Cast();
                            }
                        }
                    }
                    if (Vector3.DistanceSquared(tar.Position, ObjectManager.Player.Position) > 630 * 630 &&
                        disafter < 630 * 630)
                    {
                        float ManaVal = 1;

                        if (Orbwalker.ActiveMode.ToString() == "Combo")
                        {
                            ManaVal = VayneMenu.Item("QManaC").GetValue<Slider>().Value;
                        }
                        else if (Orbwalker.ActiveMode.ToString() == "Mixed")
                        {
                            ManaVal = VayneMenu.Item("QManaM").GetValue<Slider>().Value;
                        }
                        
                    if(getManaPer() >= ManaVal){     
                             Q.Cast(Game.CursorPos,true);
                             if (VayneMenu.Item("UseR").GetValue<bool>() && R.IsReady() && VayneMenu.Item("UseRQ").GetValue<bool>())
                             {
                                 R.Cast();
                             }
                    }
                    }
                }
                float OwnH = getPlHPer();
                if (VayneMenu.Item("Botrk").GetValue<bool>() && (VayneMenu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= OwnH) && ((VayneMenu.Item("EnHPercBotrk").GetValue<Slider>().Value <= getEnH(tar))))
                {
                    useItem(3153,tar);
                }
                if (VayneMenu.Item("Youmuu").GetValue<bool>()) { 
                    useItem(3142, tar);
                }
            }
        }
         public static Vector3 Normalize(Vector3 A)
        {
            double distance = Math.Sqrt(A.X*A.X + A.Y*A.Y);
            return new Vector3(new Vector2((float) (A.X/distance)), (float) (A.Y/distance));
        }

         public static void Game_OnGameUpdate(EventArgs args)
         {
             if (VayneMenu.Item("UseR").GetValue<bool>() && R.IsReady() && !(VayneMenu.Item("UseRQ").GetValue<bool>()))
             {
                 R.Cast();
             }
             if ((!E.IsReady()) || (((Orbwalker.ActiveMode.ToString() != "Combo") || (Orbwalker.ActiveMode.ToString() != "Mixed")) && !VayneMenu.Item("UseE").GetValue<bool>())) return;
            foreach (var hero in from hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(550f))
                let prediction = E.GetPrediction(hero)
                where NavMesh.GetCollisionFlags(
                    prediction.UnitPosition.To2D()
                        .Extend(ObjectManager.Player.ServerPosition.To2D(),
                            -VayneMenu.Item("PushDistance").GetValue<Slider>().Value)
                        .To3D())
                    .HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(
                        prediction.UnitPosition.To2D()
                            .Extend(ObjectManager.Player.ServerPosition.To2D(),
                                -(VayneMenu.Item("PushDistance").GetValue<Slider>().Value/2))
                            .To3D())
                        .HasFlag(CollisionFlags.Wall)
                select hero)
                {
                    float ManaVal = 1;
            
                if (Orbwalker.ActiveMode.ToString() == "Combo")
                {
                    ManaVal = VayneMenu.Item("EManaC").GetValue<Slider>().Value;
                }
                else if (Orbwalker.ActiveMode.ToString() == "Mixed")
                {
                    ManaVal = VayneMenu.Item("EManaM").GetValue<Slider>().Value;
                }
                Game.PrintChat((getManaPer() >= ManaVal).ToString());
               if (getManaPer() >= ManaVal)
                {
                    E.Cast(hero,true);
                }
            }
         }
         public static void OnRengarAnimation(LeagueSharp.Obj_AI_Base sender, LeagueSharp.GameObjectPlayAnimationEventArgs args)
         {
             if(sender.BaseSkinName == "Rengar" && !sender.IsAlly)
             {
                 if(args.Animation == "Rengar_LeapSound.troy" && VayneMenu.Item("RengarLeap").GetValue<bool>())
                 {
                     AntiGapcloseRengarLeap(sender);
                 }
             }
         }
        public static void AntiGapcloseRengarLeap(Obj_AI_Base rengar)
         {
             bool EState = E.IsReady();
             bool state = false;
             if (VayneMenu.Item("InAlwa").GetValue<bool>())
             {
                 state = true;
             }else if (VayneMenu.Item("InCombo").GetValue<bool>() && Orbwalker.ActiveMode.ToString() == "Combo")
             {
                 state = true;
             }else if(VayneMenu.Item("InHarrass").GetValue<bool>() && Orbwalker.ActiveMode.ToString() == "Mixed"){
                 state = true;
             }
             if(Vector3.DistanceSquared(ObjectManager.Player.Position, rengar.Position)<1000*1000)
             { 
                if(EState && state)
                {
                    E.Cast(rengar,true);
                }
             }
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
        public static float getManaPer()
         {
             float mana = (player.Mana / player.MaxMana)*100;
             return mana;
         }
        public static float getPlHPer()
        {
            float h = (player.Health / player.MaxHealth) * 100;
            return h;
        }
    }
}
