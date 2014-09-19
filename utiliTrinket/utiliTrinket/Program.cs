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
        static int trinketSlot;
        static Vector3 position;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            menu = new Menu("UtiliTrinket DZ191", "UtiliTrkMenu", true);
            menu.AddItem(new MenuItem("ward", "Buy W Totem at start of the game").SetValue(true));
            menu.AddItem(new MenuItem("timer", "Buy Sweeper at x minutes").SetValue(new Slider(15, 1, 30)));
            menu.AddItem(new MenuItem("orb", "Buy Orb").SetValue(true));
            menu.AddItem(new MenuItem("timer2", "Buy Orb at x minutes").SetValue(new Slider(40, 30, 60)));
            menu.AddItem(new MenuItem("sweeperS", "Buy Sweeper On Sightstone").SetValue(true));
            menu.AddItem(new MenuItem("sweeperQ", "Buy Sweeper On Quill Coat").SetValue(true));
            menu.AddItem(new MenuItem("sweeperW", "Buy Sweeper on Wriggle").SetValue(true));
            position = player.Position;
            menu.AddToMainMenu();
            Game.PrintChat("utiliTrinket By DZ191 Based on PewPewPew2 Loaded!")
            Game.OnGameUpdate += OnTick;
            
        }

        private static void OnTick(EventArgs args)
        {
            Obj_AI_Hero player1 = (Obj_AI_Hero)player;
            GetTimer();
 	        if(player.IsDead || Utility.InShopRange())
            {
                if(GetTimer()<1 && !hasItem(YellowW) && !boughtYellow)
                {
                    boughtYellow = true;
                    Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(YellowW, ObjectManager.Player.NetworkId)).Send();
                }
                if(hasItem(SightStone) && isEn("sweeperS") && !boughtSweepS)
                {
                    boughtSweepS = true;
                    player1.SellItem(trinketSlot);
                    Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(TRINKET_RED, ObjectManager.Player.NetworkId)).Send();
                }
                if (hasItem(QuillCoat) && isEn("sweeperQ") && !boughtSweepQ)
                {
                    boughtSweepQ = true;
                    player1.SellItem(trinketSlot);
                    Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(TRINKET_RED, ObjectManager.Player.NetworkId)).Send();
                }
                if (hasItem(Wriggle) && isEn("sweeperW") && !boughtSweepW)
                {
                    boughtSweepW = true;
                    player1.SellItem(trinketSlot);
                    Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(TRINKET_RED, ObjectManager.Player.NetworkId)).Send();
                }
                if(isEn("orb") && (GetTimer()>= menu.Item("timer2").GetValue<Slider>().Value) && !boughtBlue)
                {
                    boughtBlue = true;
                    player1.SellItem(trinketSlot);
                    Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(Orb, ObjectManager.Player.NetworkId)).Send();
                }
                if(hasItem(YellowW) && GetTimer()>= menu.Item("timer").GetValue<Slider>().Value && !boughtSweep)
                {
                    boughtSweep = true;
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
