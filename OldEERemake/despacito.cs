using PlayerIO.GameLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//https://playerio-a.akamaihd.net/oldeeremake-xmiwpfxd106ptwe5p7xoq/oldee.swf
//oldeeremake-xmiwpfxd106ptwe5p7xoq

namespace Oldy
{//note: this code will make you :sick:, proceed with caution
    public class Player : BasePlayer
    {
        public bool Initiated = false;

        public int Face = 0;
        public int Xpos = 16;
        public int Ypos = 16;
    }

    [RoomType("FlixelWalker1")]
    public class FlixelWalker1 : Game<Player>
    {
        public int[,] World = new int[100, 100];

        public override void GameStarted()
        {
            World = new int[100, 100];
            //check if room id is 0x0, if it is then make a special digbot world
            if (this.RoomId != "0x0")
            {//normal world
                for (int x = 0; x < 100; x++)
                {
                    for (int y = 0; y < 100; y++)
                    {
                        if ((x == 0 || x == 99)
                            || (y == 0 || y == 99))
                        {
                            World[x, y] = 5;
                        }
                        else
                        {
                            World[x, y] = 0;
                        }
                    }
                }
            }
            else
            {//digbot world
                for (int i = 0; i < 100; i++)
                {//place pure black blocks on left and right border
                    World[0, i] = 20;
                    World[99, i] = 20;
                }

                for (int i = 1; i < 99; i++)
                {//place red metal blocks on bottom border
                    World[i, 99] = 18;
                }

                for (int i = 1; i < 99; i++)
                {//place green bricks on surface
                    World[i, 5] = 16;
                }

                for (int i = 0; i <= 5; i++)
                {//make a hole in the center of surface so u can go in
                    if (i == 0) World[47 - 1, 4] = 4;//hack
                    if (i == 5) World[47 + 6, 4] = 4;//hack
                    World[47 + i, 4] = 4; //nogravity
                    World[47 + i, 5] = 13; //bricksy
                }

                for (int y = 6; y <= 98; y++)
                {//fill world with bricks
                    for (int x = 1; x < 99; x++)
                    {
                        World[x, y] = 13;
                    }
                }

                //make a house in upper right
                {
                    int[] z = new int[]
                    {//end my suffering
                        //0,0,1,0,0,
                        //0,1,1,1,0,
                        //1,0,0,0,1,
                        //1,0,0,0,1,
                        //0,0,1,0,1,
                        //1,1,1,1,1,

                        //0,0,0,0,1,0,0,0,0,
                        //0,0,0,1,1,1,0,0,0,
                        //0,0,1,1,1,1,1,0,0,
                        //0,1,0,0,0,0,0,1,0,
                        //1,0,0,0,0,0,0,0,1,
                        //0,0,0,0,0,0,0,0,1,
                        //0,1,0,0,0,0,0,1,1,
                        //0,1,0,1,1,1,0,1,1,
                        //0,1,1,0,1,0,1,1,1,
                        //1,1,1,1,1,1,1,1,1,

                        // 0 = air
                        // 1 = cyan block
                        // 2 = red block
                        // 3 = black background
                        
                        0,0,3,1,1,1,1,1,1,1,3,
                        0,0,1,0,0,0,0,0,0,0,1,
                        0,0,0,0,0,0,0,0,0,0,1,
                        0,0,1,2,0,2,2,2,0,2,1,
                        0,1,1,2,2,0,2,0,2,2,1,
                        1,1,1,1,1,1,1,1,1,1,1,
                        //13,13,13,13,13,13,13,13,13,13,13,//hack
                    };

                    int r = 11;
                    int x = 98 - r, y = 0;
                    for (int i = 0; i < z.Length; i++)
                    {
                        int id;
                        switch (z[i])
                        {
                            case 0:
                                id = 0;
                                break;
                            case 1:
                                id = 11;
                                break;
                            case 2:
                                id = 8;
                                break;
                            case 3:
                                id = -1;
                                break;
                            default:
                                id = z[i];
                                break;
                        }

                        x++;
                        if (i != 0 && i % r == 0) { y++; x -= r; }

                        World[x, y] = id;
                    }//i wrote this few months ago without comments and now i don't even know what it does but it works so

                }

                AddTimer(delegate { Refill(); }, 30 * 60 * 1000);// refill world with bricks every 30 minutes
            }

            base.GameStarted();
        }

        public void Refill(int x = 0, int y = 99)
        {
            int b = 0;
            while (y >= 5)
            {
                x = 0;
                while (x < 100)
                {
                    if (World[x, y] == 4)
                    {
                        if (b > 4)
                        {
                            b = 0;
                            ScheduleCallback(delegate { Refill(x, y); }, 25); //hack cpu time limit
                            return;
                        }
                        World[x, y] = 13;
                        Broadcast("change", x, y, 13);
                        b++;
                    }
                    x++;
                }
                y--;
            }
        }

        public override void GameClosed()
        {
            base.GameClosed();
        }

        public override void UserJoined(Player player)
        {
            ScheduleCallback(delegate
            {
                if (!player.Initiated)
                {
                    player.Send("timeout", "Did not send init message in 10 seconds; disconnecting.");
                    player.Disconnect();
                }
            }, 10000);
            base.UserJoined(player);
        }

        public override void UserLeft(Player player)
        {
            if (player.Initiated) { Broadcast("left", player.Id); }
            base.UserLeft(player);
        }

        public override void GotMessage(Player player, Message message)
        {
            switch (message.Type)
            {
                case "init":
                    if (!player.Initiated)
                    {
                        player.Initiated = true;

                        StringBuilder Serialize = new StringBuilder("");

                        //Serialize the world data
                        for (int y = 0; y < 100; y++)
                        {
                            Serialize.Append(World[0, y].ToString());
                            for (int x = 1; x < 100; x++)
                            {
                                Serialize.Append(",");
                                Serialize.Append(World[x, y].ToString());
                            }
                            if (y != 99)
                            {
                                Serialize.Append("\n");
                            }
                        }
                        Broadcast("add", player.Id, player.Face, player.Xpos, player.Ypos);
                        player.Send("init", Serialize.ToString(), player.Id);
                        ForEachPlayer(delegate (Player plr)
                        {
                            if (plr.Id != player.Id)
                            {
                                player.Send("add", plr.Id, plr.Face, plr.Xpos, plr.Ypos);
                            }
                        });
                    }
                    break;
                case "face":
                    if (message.Count == 1)
                    {
                        if (Int32.TryParse(message[0].ToString(), out int id))
                        {
                            if (this.RoomId != "0x0" || (id >= 0 && id <= 5) || id == 16) //if room is not a digbot world, or id is between 0 and 5 inclusive, or id equals 16 (MAX smiley)
                            {
                                Broadcast("face", player.Id, id);
                                player.Face = id;
                            }
                        }
                    }
                    break;
                case "update":
                    if (message.Count == 8)
                    {
                        double[] Args = new double[8];
                        bool Successful = true;
                        for (uint i = 0; i <= 7; i++)
                        {
                            if (!Double.TryParse(message[i].ToString(), out Args[i]))
                            {
                                Successful = false;
                            }
                        }

                        if (Successful)
                        {
                            if (Args[0] < -16 * 16 || Args[0] > 115 * 16 || Args[1] < -48 * 16 || Args[1] > 115 * 16)
                            {// if player is outside of the world (fell into the void), tp player back into the world
                             //(normally if you send the player movement packet of itself it doesn't do anything, but i edited client swf so it allows that)
                                Broadcast("update", player.Id, 16d, 16d, 0d, 0d, Args[4], Args[5], Args[6], Args[7]);
                                player.Xpos = (int)Args[0];
                                player.Ypos = (int)Args[1];
                            }
                            else
                            {
                                ForEachPlayer(delegate (Player plr)
                                {
                                    if (plr.Id != player.Id)
                                    {
                                        plr.Send("update", player.Id, Args[0], Args[1], Args[2], Args[3], Args[4], Args[5], Args[6], Args[7]);
                                    }
                                });
                                player.Xpos = (int)Args[0];
                                player.Ypos = (int)Args[1];
                            }
                        }

                        if (this.RoomId == "0x0")
                        {//additional logic for digbot world
                            int x = (int)((message.GetDouble(0) / 16) + 0.5d);
                            int y = (int)((message.GetDouble(1) / 16) + 0.5d);

                            int offsetx = (int)message.GetDouble(6);
                            int offsety = (int)message.GetDouble(7);


                            if (x + offsetx < 0 || x + offsetx > 99) break;
                            if (y + offsety < 0 || y + offsety > 99) break;

                            if (World[x + offsetx, y] == 13)
                            {
                                World[x + offsetx, y] = 4;
                                Broadcast("change", x + offsetx, y, 4);
                            }
                            if (World[x, y + offsety] == 13)
                            {
                                World[x, y + offsety] = 4;
                                Broadcast("change", x, y + offsety, 4);
                            }
                            // if player moves in no gravity dot, client will send movement packet constantly, so you cannot reliably detect if player just started moving or not.
                            // because of this, it will dig automatically when you hold movement key. i don't know pcode very well so i can't fix that.
                        }

                    }
                    break;
                case "change":
                    if (this.RoomId == "0x0" && player.Id != 1) break; // if room is digbot world, and player is NOT the first to join the room, disallow any editing
                    if (message.Count == 3)
                    {
                        if (Int32.TryParse(message[0].ToString(), out int x))
                        {
                            if (Int32.TryParse(message[1].ToString(), out int y))
                            {
                                if (Int32.TryParse(message[2].ToString(), out int id))
                                {
                                    if (x > -1 && x < 100)
                                    {// yeah, i allow removing blocks on world border. for fun i guess? it will tp player back to world if it falls out of the world, see comments above
                                        if (y > -1 && y < 100)
                                        {
                                            if (World[x, y] != id)
                                            {
                                                Broadcast("change", x, y, id);
                                                World[x, y] = id;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
            base.GotMessage(player, message);
        }
    }
}