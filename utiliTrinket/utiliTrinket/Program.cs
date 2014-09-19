using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Color = System.Drawing.Color;

namespace utiliTrinket
{
    class Program
    {
        public static Menu menu;
        public static Obj_AI_Base player = ObjectManager.Player;
        static int SightStone = 2049;
        static int Orb = 3363;
        static int YellowW = 3340;
        static int TRINKET_RED = 3341;
        static int QuillCoat = 3204;
        static int Wriggle = 3154;
        static bool boughtSweepS = false;
        static bool boughtSweepW = false;
        static bool boughtSweepQ = false;
        static bool boughtYellow = false;
        static bool boughtSweep = false;
        static bool boughtBlue = false;
        static int trinketSlot = 134;
        static Vector3 position;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            
            menu = new Menu("utiliTrinket", "UtiliTrkMenu", true);
            menu.AddItem(new MenuItem("ward", "Buy WTotem at start").SetValue(true));
            menu.AddItem(new MenuItem("timer", "Buy Sw at x min").SetValue(new Slider(15, 1, 30)));
            menu.AddItem(new MenuItem("orb", "Buy Orb").SetValue(true));
            menu.AddItem(new MenuItem("timer2", "Buy Orb at x min").SetValue(new Slider(40, 30, 60)));
            menu.AddItem(new MenuItem("sweeperS", "Buy Sw On Sightstone").SetValue(true));
            menu.AddItem(new MenuItem("sweeperQ", "Buy Sw QuillCoat").SetValue(true));
            menu.AddItem(new MenuItem("sweeperW", "Buy Sw on Wriggle").SetValue(true));
            position = player.Position;
            menu.AddToMainMenu();
            Game.PrintChat("utiliTrinket By DZ191 Based on PewPewPew Loaded!");
            Game.OnGameUpdate += OnTick;
            
        }

        private static void OnTick(EventArgs args)
        {
            Obj_AI_Hero player1 = (Obj_AI_Hero)player;
            GetTimer();
 	        if(player.IsDead || Utility.InShopRange())
            {
                if(GetTimer()<1 && !hasItem(YellowW))
                {
                    Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(YellowW, ObjectManager.Player.NetworkId)).Send();
                }
                if(hasItem(SightStone) && isEn("sweeperS") && !hasItem(TRINKET_RED))
                {
                    player1.SellItem(trinketSlot);
                    Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(TRINKET_RED, ObjectManager.Player.NetworkId)).Send();
                }
                if (hasItem(QuillCoat) && isEn("sweeperQ") && !hasItem(TRINKET_RED))
                {
                    player1.SellItem(trinketSlot);
                    Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(TRINKET_RED, ObjectManager.Player.NetworkId)).Send();
                }
                if (hasItem(Wriggle) && isEn("sweeperW") && !hasItem(TRINKET_RED))
                {
                    player1.SellItem(trinketSlot);
                    Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(TRINKET_RED, ObjectManager.Player.NetworkId)).Send();
                }
                if (isEn("orb") && (GetTimer() >= menu.Item("timer2").GetValue<Slider>().Value) && !hasItem(Orb))
                {
                    
                    player1.SellItem(trinketSlot);
                    Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(Orb, ObjectManager.Player.NetworkId)).Send();
                }
                if (hasItem(YellowW) && GetTimer() >= menu.Item("timer").GetValue<Slider>().Value && !hasItem(TRINKET_RED))
                {
                    //Game.PrintChat("Called");
                    
                    player1.SellItem(trinketSlot);
                    Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(TRINKET_RED, ObjectManager.Player.NetworkId)).Send();
                }

            }
        }
        
        public static float GetTimer()
        {
            return Game.Time/60;
        }
        public static bool hasItem(int id)
        {
            return Items.HasItem(id, (Obj_AI_Hero)player);
        }
        public static bool isEn(String op)
        {
            return menu.Item(op).GetValue<bool>();
        }
    }
}
