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
        private const int MaxMinionDistance = 1000;
        private static float casterDamage = 0;
        private static List<Obj_AI_Minion> _killableMinions = new List<Obj_AI_Minion>();

        public static Obj_AI_Base player = ObjectManager.Player;
        public static Menu menu;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            menu = new Menu("DZ191's Minion HP Bar", "MinionHPBar", true);
            menu.AddSubMenu(new Menu("Drawing", "Drawing"));
            //Drawings Menu
            menu.SubMenu("Drawing").AddItem(new MenuItem("DrawLines", "Draw Bars").SetValue(true));
            //menu.SubMenu("Drawing").AddItem(new MenuItem("Debug", "Debug").SetValue(false));
            menu.AddSubMenu(new Menu("Ranges", "Range"));
            menu.SubMenu("Range").AddItem(new MenuItem("DrRange", "Draw Range").SetValue(new Slider(850,450,1200)));
            menu.AddToMainMenu();
            Game.PrintChat("MinionHPBar by DZ191. Damage Calculations by Lizzaran.");
            
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if(isEn("DrawLines"))
            {
               // Game.PrintChat("Called");
                var Minions = MinionManager.GetMinions(player.Position, menu.Item("DrRange").GetValue<Slider>().Value, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
                foreach(var minion in Minions.Where(minion=>!minion.IsDead && minion.IsValid && minion!=null && minion.Team != player.Team && minion.IsVisible && player.Distance(minion)<=menu.Item("DrRange").GetValue<Slider>().Value && minion.IsEnemy))
                {
                    
                    var autoToKill = Math.Ceiling(minion.MaxHealth/ DamageCalculator.Calculate((Obj_AI_Hero)player, minion));
                    var BTD = Math.Ceiling(minion.MaxHealth / DamageCalculator.Calculate((Obj_AI_Hero)player, minion));
                    var HPBarPos = minion.HPBarPosition;
                    var BarsToDraw = Math.Ceiling((100/minion.MaxHealth) / autoToKill);
                    var width = minion.IsMelee()?75:81;
                    if (!minion.IsMelee())casterDamage = minion.BaseAttackDamage;
                    if(minion.HasBuff("turretshield",true))width = 70;
                    var barDistanceBetween =  width/ autoToKill;
                    for (int i=0;i<BTD;i++)
                    {
                        if(i!=0 || i!=BTD-1)
                        {
                            Drawing.DrawLine(new Vector2(HPBarPos.X + 45.1f + (float)(barDistanceBetween) * i, HPBarPos.Y + 18), new Vector2(HPBarPos.X + 45.1f + ((float)(barDistanceBetween) * i), HPBarPos.Y + 23), 1f, (minion.Health<=DamageCalculator.Calculate((Obj_AI_Hero)player, minion))?Color.Lime:Color.Black);
                        }
                    }
                }
            }
        }
        static bool isEn(String opt)
        {
            return menu.Item(opt).GetValue<bool>();
        }
        //Creditz to Lizzaran <3
        internal class DamageCalculator
        {
            public static double Calculate(Obj_AI_Hero player, Obj_AI_Base target)
            {
                //no % armor/magic penetration calculation
                //can't find magic resist??
                //no masteries yet
                double targetArmor = target.Armor + target.FlatArmorMod;
                double targetSpellbock = target.SpellBlock + target.FlatSpellBlockMod;
                double attackDamage = player.BaseAttackDamage + player.FlatPhysicalDamageMod;
                //Game.PrintChat(attackDamage.ToString());
                double magicDamage = player.BaseAbilityDamage + player.FlatMagicDamageMod;

                targetArmor -= player.FlatArmorPenetrationMod;
                targetSpellbock -= player.FlatMagicPenetrationMod;

                double totalDamage = attackDamage * (100 / (100 + targetArmor));

                if (HasMasteryArcaneBlade())
                {
                    totalDamage += (magicDamage * 0.05) * (100 / (100 + targetSpellbock));
                }
                if (HasMasteryDoubleEdgedSword())
                {
                    if (player.AttackRange > 250)
                    {
                        totalDamage *= 1.02;
                    }
                    else
                    {
                        totalDamage *= 1.015;
                    }
                }
                if (HasMasteryHavoc())
                {
                    totalDamage *= 1.03;
                }
                if (target.IsMinion & HasMasteryButcher())
                {
                    totalDamage += 2;
                }
                return totalDamage;
            }

            public static bool HasMasteryDoubleEdgedSword()
            {
                return
                    ObjectManager.Player.Masteries.Where(mastery => mastery.Page == MasteryPage.Offense)
                        .Any(mastery => mastery.Id == 65 && mastery.Points == 1);
            }

            public static bool HasMasteryButcher()
            {
                return
                    ObjectManager.Player.Masteries.Where(mastery => mastery.Page == MasteryPage.Offense)
                        .Any(mastery => mastery.Id == 68 && mastery.Points == 1);
            }

            public static bool HasMasteryArcaneBlade()
            {
                return
                    ObjectManager.Player.Masteries.Where(mastery => mastery.Page == MasteryPage.Offense)
                        .Any(mastery => mastery.Id == 132 && mastery.Points == 1);
            }

            public static bool HasMasteryHavoc()
            {
                return
                    ObjectManager.Player.Masteries.Where(mastery => mastery.Page == MasteryPage.Offense)
                        .Any(mastery => mastery.Id == 146 && mastery.Points == 1);
            }
        }
    }
}
