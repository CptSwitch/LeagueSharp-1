using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Color = System.Drawing.Color;

namespace DZRevealer
{
    //Inspired from the discontinued BoL script Disclosures. Many thanks to the Autor for his Trinket\Ward place calcs.

    class Program
    {
        public static Dictionary<String, String> dict;
        public static Dictionary<String, String> acquiredBuffs = new Dictionary<string,string>();
        public static Obj_AI_Base player = ObjectManager.Player;
        public static Menu menu;
        public static int VISION_WARD = 2043;
        public static int TRINKET_RED = 3364;
        public static float Ward_range,trinket_range = 600f;
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
            menu = new Menu("DZReveal!", "DZReveal", true);
            menu.AddItem(new MenuItem("doRev", "Reveal").SetValue(true));
            menu.AddItem(new MenuItem("revDesc1", "Priority:"));
            menu.AddItem(new MenuItem("prior", "ON: Pink OFF: Trinket").SetValue(true));
            menu.AddToMainMenu();
            fillDict();
            Game.PrintChat(player.BaseSkinName);
            Game.OnGameUpdate += Game_GameUpdate;
        }

        private static void Game_GameUpdate(EventArgs args)
        {
            if (!isEn("doRev")) return;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                BuffInstance[] buffs = enemy.Buffs;
                foreach (var buff in buffs)
                {
                    foreach (KeyValuePair<string, string> entry in dict)
                    {
                        
                        if(acquiredBuffs.ContainsKey(entry.Value)&&(entry.Key==enemy.BaseSkinName)&&!ArrayCKey(buffs,entry.Value))
                        {
                            acquiredBuffs.Remove(enemy.BaseSkinName);
                        }
                       
                        if((buff.Name == entry.Value)&&!acquiredBuffs.ContainsValue(entry.Value))
                        { 
                                acquiredBuffs.Add(enemy.BaseSkinName, entry.Value);
                                Reveal(enemy);
                        }
                    }
                }
            }
               
            
        }

        static void Reveal(Obj_AI_Hero enemy)
        {
            
            if (isEn("prior"))
            {
                //W
                if (player.Distance(enemy) <= Ward_range+1000)
                {
                    if(player.Distance(enemy) <= Ward_range)
                    {
                        useItem(VISION_WARD, enemy.Position);
                    }
                    else
                    {
                        Vector3 trPos = new Vector3(player.Position.X + Ward_range, player.Position.Y + Ward_range, player.Position.Z + Ward_range);
                        Vector3 pos = player.Position - (enemy.ServerPosition - player.Position) * trPos;
                        useItem(VISION_WARD, pos);
                    }
                    
                }
            }
            else
            {
                //Trink
                if (player.Distance(enemy) <= trinket_range)
                {
                    if (player.Distance(enemy) <= trinket_range)
                    {
                        useItem(TRINKET_RED, enemy.Position);
                    }
                    else
                    {
                        Vector3 trPos = new Vector3(player.Position.X + (trinket_range + trinket_range / 2), player.Position.Y + (trinket_range + trinket_range / 2), player.Position.Z + (trinket_range + trinket_range / 2));
                        Vector3 pos = player.Position - (enemy.ServerPosition - player.Position) * trPos;
                        useItem(TRINKET_RED, pos);
                    }
                    
                }
            }
            
        }
        public static bool isEn(String item)
        {
            return menu.Item(item).GetValue<bool>();
        }
        static bool ArrayCKey(BuffInstance[] array,String key)
        {
            foreach(var b in array)
            {
                if(b.Name == key)
                {
                    return true;
                }
            }
           return false;
        }
        static void fillDict()
        {
            dict = new Dictionary<String, String>();
            dict.Add("Vayne", "VayneTumbleFade");
            dict.Add("Twitch", "TwitchHideInShadows");
            dict.Add("Rengar", "RengarR");
            dict.Add("MonkeyKing", "monkeykingdecoystealth");
            dict.Add("Khazix", "khazixrstealth");
            dict.Add("Talon", "talonshadowassaultbuff");
            dict.Add("Akali", "akaliwstealth");
        }
        public static void useItem(int id, Vector3 position)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id,position);
            }
        }
    }
}
