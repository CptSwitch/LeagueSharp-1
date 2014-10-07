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

                    var autoToKill = Math.Ceiling(minion.MaxHealth / Damage.GetAutoAttackDamage(player, minion, true));
                    var BTD = Math.Ceiling(minion.MaxHealth / Damage.GetAutoAttackDamage(player,minion,true));
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
                            Drawing.DrawLine(new Vector2(HPBarPos.X + 45.1f + (float)(barDistanceBetween) * i, HPBarPos.Y + 18), new Vector2(HPBarPos.X + 45.1f + ((float)(barDistanceBetween) * i), HPBarPos.Y + 23), 1f, (minion.Health<=Damage.GetAutoAttackDamage(player,minion,true)?Color.Lime:Color.Black);
                        }
                    }
                }
            }
        }
        static bool isEn(String opt)
        {
            return menu.Item(opt).GetValue<bool>();
        }
    }
}
