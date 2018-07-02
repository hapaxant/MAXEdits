using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerIO.GameLibrary;

namespace ChatServer
{
    public class Player : BasePlayer
    {
        public bool Inited = false;
        public string Username = "";
    }

    [RoomType("Chat")]
    public class Chat : Game<Player>
    {
        public override void UserJoined(Player plr)
        {
            if (RoomId != "lol")
            {
                plr.Send("what the fuck are you doing");
                plr.Disconnect();
                base.UserJoined(plr);
                return;
            }

            ScheduleCallback(() =>
            {
                if (!plr.Inited)
                {
                    plr.Send("timeout", "Did not send init in 10 seconds; disconnecting.");
                    plr.Disconnect();
                }
            }, 10000);

            base.UserJoined(plr);
        }
        public override void UserLeft(Player plr)
        {
            if (plr.Inited)
            {
                Broadcast("left", plr.Username);
            }

            base.UserLeft(plr);
        }
        public void BroadcastInited(string type, params object[] args) => ForEachPlayer((p) => { if (p.Inited) p.Send(type, args); });
        public readonly string AllowedChars = "qwertyuiopasdfghjklzxcvbnm1234567890_";
        public const int MaxUsernameLength = 16;
        public const int MaxMessageLength = 256;
        public const int MaxMessagesHistory = 24;
        public bool ValidateName(string str)
        {
            if (str.Length > MaxUsernameLength) return false;
            foreach (var ch in str.ToLower())
            {
                if (!AllowedChars.Contains(ch.ToString())) return false;
            }
            return true;
        }
        public List<Tuple<string, string>> messagesHistory = new List<Tuple<string, string>>();
        public Random rnd = new Random();
        public override void GotMessage(Player plr, Message m)
        {
            if (!plr.Inited)
                if (m.Type != "init")
                {
                    base.GotMessage(plr, m);
                    return;
                }

            switch (m.Type)
            {
                case "init":
                    {
                        if (m.Count != 0) break;
                        bool lol = true;
                        string name = null;
                        while (lol)
                        {
                            lol = false;
                            name = "guest" + rnd.Next(0, 10000);
                            ForEachPlayer((p) =>
                            {
                                if (p.Inited && !lol && p.Username.ToLower() == name)
                                {
                                    lol = true;
                                }
                            });
                        }
                        plr.Username = name;
                        plr.Send("name", name);
                        foreach (var item in messagesHistory)
                        {
                            plr.Send("oldsay", item.Item1, item.Item2);
                        }
                        plr.Inited = true;
                        //plr.Send("init", plr.Username);
                        //BroadcastInited("add", name);
                        ForEachPlayer((p) =>
                        {
                            if (p.Inited && p.Username != name)
                            {
                                p.Send("add", name);
                            }
                        });
                        List<string> ok = new List<string>();
                        ForEachPlayer((p) =>
                        {
                            if (p.Inited && p.Username != name)
                            {
                                ok.Add(p.Username);
                            }
                        });
                        plr.Send("online", ok.ToArray<object>());
                        break;
                    }
                case "say":
                    {
                        if (m.Count != 1 || m[0].GetType() != typeof(string) ||
                            String.IsNullOrWhiteSpace(m.GetString(0).Trim()) || m.GetString(0).Trim().Length > MaxMessageLength)
                            break;
                        string msg = m.GetString(0).Trim();
                        if (msg[0] != '/')
                        {//normal chat
                            BroadcastInited("say", plr.Username, msg);
                            messagesHistory.Add(Tuple.Create(plr.Username, msg));
                            if (messagesHistory.Count > MaxMessagesHistory) messagesHistory.RemoveAt(0);
                        }
                        else
                        {//commands
                            string[] args = msg.Split(' ');
                            string cmd = args[0].Substring(1).ToLower();
                            StringBuilder response = new StringBuilder("");

                            switch (cmd)
                            {
                                case "help":
                                case "?":
                                    response.AppendLine("/help - list available commands.");
                                    response.AppendLine("/nick <newName> - change your nickname.");
                                    response.AppendLine("/me <action> - * zyhrllos dies");
                                    response.AppendLine("/roll [max(100)] / [min(0)] [max(100)] - rolls a random number");
                                    break;
                                case "nick":
                                    {
                                        if (args.Length == 1)
                                        {
                                            response.Append("Required parameter missing.");
                                            break;
                                        }
                                        if (args.Length > 2)
                                        {
                                            response.Append($"Invalid characters or length greater than {MaxUsernameLength}.");
                                            break;
                                        }
                                        string name = args[1];
                                        if (ValidateName(name))
                                        {
                                            if (name.ToLower() == "system")
                                            {
                                                response.Append("nice try");
                                                break;
                                            }
                                            bool fail = false;
                                            ForEachPlayer((p) =>
                                            {
                                                if (p.Inited && !fail)
                                                {
                                                    if (p.Username.ToLower() == name.ToLower()) fail = true;
                                                }
                                            });
                                            if (fail)
                                            {
                                                response.Append("Name is already taken.");
                                                break;
                                            }

                                            BroadcastInited("rename", plr.Username, name);
                                            plr.Username = name;
                                        }
                                        else
                                        {
                                            response.Append($"Invalid characters or length greater than {MaxUsernameLength}.");
                                        }
                                    }
                                    break;
                                case "me":
                                    if (args.Length <= 1)
                                    {
                                        response.AppendLine("Required parameter missing.");
                                        break;
                                    }
                                    else BroadcastInited("me", plr.Username, String.Join(" ", args.Skip(1)));
                                    break;
                                case "roll":
                                    {
                                        int min = 0, max = 100;
                                        bool fail = false;
                                        if (args.Length == 2)
                                        {
                                            fail = !int.TryParse(args[1], out max);
                                        }
                                        else if (args.Length >= 3)
                                        {
                                            fail = !int.TryParse(args[1], out min) || !int.TryParse(args[2], out max);
                                        }
                                        if (fail)
                                        {
                                            response.AppendLine("Invalid parameter - please input a valid number.");
                                            break;
                                        }

                                        if (min > max)
                                        {
                                            response.AppendLine("Min cannot be greater than max.");
                                            break;
                                        }
                                        BroadcastInited("roll", plr.Username, rnd.Next(min, max + 1), min, max);
                                    }
                                    break;
                                default:
                                    response.AppendLine("Command not found. Type /help to list available commands.");
                                    break;
                            }
                            if (!String.IsNullOrWhiteSpace(response.ToString())) plr.Send("system", response.ToString().Trim());
                        }
                        break;
                    }
            }

            base.GotMessage(plr, m);
        }
    }
}
