using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace MinionHPBar
{

    class Program
    {
        public static Obj_AI_Base Player = ObjectManager.Player;
        public static Menu Menu;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("DZ191's Minion HP Bar", "MinionHPBar", true);
            Menu.AddSubMenu(new Menu("Drawing", "Drawing"));

            //Drawings Menu
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawLines", "Draw Bars").SetValue(new Circle(true,Color.Black)));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("LastHitH", "LastHit Helper").SetValue(new Circle(true,Color.Lime)));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("Thick", "Thickness").SetValue(new Slider(2,1,3)));
            //Range Menu
            Menu.AddSubMenu(new Menu("Ranges", "Range"));
            Menu.SubMenu("Range").AddItem(new MenuItem("DrRange", "Draw Range").SetValue(new Slider(850,450,1200)));
            Menu.AddToMainMenu();

            //Credits
            Game.PrintChat("MinionHPBars by DZ191");
            
            //Events
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Menu.Item("DrawLines").GetValue<Circle>().Active)
                return;
            var minionList = MinionManager.GetMinions(Player.Position, Menu.Item("DrRange").GetValue<Slider>().Value, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
            foreach (var minion in minionList.Where(minion => minion.IsValidTarget(Menu.Item("DrRange").GetValue<Slider>().Value)))
            {
                var attackToKill  = Math.Ceiling(minion.MaxHealth/Player.GetAutoAttackDamage(minion, true));
                var hpBarPosition = minion.HPBarPosition;
                var barWidth = minion.IsMelee() ? 75 : 80;
                if (minion.HasBuff("turretshield", true))
                {
                    barWidth = 70;
                }
                var barDistance = barWidth/attackToKill;
                for (var i = 0; i < attackToKill; i++)
                {
                    if (i != 0)
                    {
                        Drawing.DrawLine(
                            new Vector2(hpBarPosition.X + 45 + (float) (barDistance)*i, hpBarPosition.Y + 18),
                            new Vector2(hpBarPosition.X + 45 + ((float) (barDistance)*i), hpBarPosition.Y + 23), Menu.Item("Thick").GetValue<Slider>().Value,
                            ((minion.Health <= Player.GetAutoAttackDamage(minion, true) && Menu.Item("LastHitH").GetValue<Circle>().Active) ? Menu.Item("LastHitH").GetValue<Circle>().Color : Menu.Item("DrawLines").GetValue<Circle>().Color));
                    }
                }
            }
        }
        
    }
}
