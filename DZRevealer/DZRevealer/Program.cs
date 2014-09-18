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
        public static float wardrange = 600f;
        public static float trinket_range = 600f;
        public static bool debug = true;
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
            if (!debug)
            {
                Game.PrintChat("Work in progress");
                return;
            }
            menu = new Menu("DZReveal!", "DZReveal", true);
            menu.AddItem(new MenuItem("doRev", "Reveal").SetValue(true));
            menu.AddItem(new MenuItem("revDesc1", "Priority:"));
            menu.AddItem(new MenuItem("prior", "ON: Pink OFF: Trinket").SetValue(true));
            Game.PrintChat("DZReveal Loaded");
            menu.AddToMainMenu();
            fillDict();
            Game.PrintChat(player.BaseSkinName);
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
            Game.OnGameUpdate += Game_GameUpdate;
        }

        private static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            byte[] pdata = args.PacketData;
            Game.PrintChat("PacketProcess "+args.PacketData[0].ToString());
            if(pdata[0] == 0xB7)
            {
                Game.PrintChat("OnGainBuffDump");
                foreach(var b in pdata)
                {
                    
                    Game.PrintChat(b.ToString());
                }
            }
        }

        private static void Game_GameUpdate(EventArgs args)
        {
            if (!isEn("doRev")) return;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                BuffInstance[] buffs = enemy.Buffs;
                foreach (var buff in buffs)
                {
                    
                        if (dict.ContainsKey(enemy.BaseSkinName) && dict.ContainsValue(buff.Name))
                        {
                            foreach (var vari in acquiredBuffs)
                            {
                                if (!ArrayCKey(buffs, vari.Value))
                                {
                                    acquiredBuffs.Remove(vari.Key);
                                }
                            }
                            if (!acquiredBuffs.ContainsKey(buff.Name))
                            {
                                 acquiredBuffs.Add(enemy.BaseSkinName,buff.Name);
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
                if (player.Distance(enemy) <= wardrange+1000)
                {
                    if(player.Distance(enemy) <= wardrange)
                    {
                        useItem(VISION_WARD, enemy.Position);
                    }
                    else
                    {
                        Vector3 trPos = new Vector3(player.Position.X + wardrange, player.Position.Y + wardrange, player.Position.Z + wardrange);
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
