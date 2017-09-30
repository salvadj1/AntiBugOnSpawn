using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Fougerite;
using Fougerite.Events;
using UnityEngine;
using System.IO;
using RustPP.Social;

namespace AntiBugOnSpawn
{
    public class AntiBugOnSpawn : Fougerite.Module
    {
        public override string Name
        {
            get { return "AntiBugOnSpawn"; }
        }

        public override string Author
        {
            get { return "Salva/Juli"; }
        }

        public override string Description
        {
            get { return "AntiBugOnSpawn"; }
        }

        public override Version Version
        {
            get { return new Version("1.0"); }
        }
        private string red = "[color #B40404]";
        private string blue = "[color #81F7F3]";
        private string green = "[color #82FA58]";
        private string yellow = "[color #F4FA58]";
        private string orange = "[color #FF8000]";
        private string pink = "[color #FA58F4]";
        private string white = "[color #FFFFFF]";

        public static IniParser cfg;
        public static System.Random rnd;
        public bool RustPPSupport = false;

        public override void Initialize()
        {
            Fougerite.Hooks.OnCommand += OnCommand;
            Fougerite.Hooks.OnPlayerSpawned += OnPlayerSpawned;
            Fougerite.Hooks.OnModulesLoaded += OnModulesLoaded;

            rnd = new System.Random();
            cfg = new IniParser(Path.Combine(ModuleFolder, "DefaultLoc.ini"));
        }
        public override void DeInitialize()
        {
            Fougerite.Hooks.OnCommand -= OnCommand;
            Fougerite.Hooks.OnPlayerSpawned -= OnPlayerSpawned;
            Fougerite.Hooks.OnModulesLoaded -= OnModulesLoaded;
        }

        public void OnModulesLoaded()
        {
            if (Fougerite.Server.GetServer().HasRustPP)
            {
                RustPPSupport = true;
                ConsoleSystem.Print("Plugin " + Name + " " + Version.ToString() + " Integration with Rust++ OK!!");
            }
            else
            {
                ConsoleSystem.Print("Plugin " + Name + " " + Version.ToString() + " WARNING, integration with Rust++ has Failed!!");
            }
        }
        public void OnCommand(Fougerite.Player player, string cmd, string[] args)
        {
            if (cmd == "antibug")
            {
                player.MessageFrom("AntiBug", yellow + "ANTIBUG is a system that prevents players from using bugs in your house.");
                player.MessageFrom("*", yellow + "To be able to SPAWN at a friend's house, he must be your friend.");
                player.MessageFrom("*", yellow + "Your friend and you must use command " + blue + "/addfriend NAME");
            }
        }
        public void OnPlayerSpawned(Fougerite.Player player, SpawnEvent se)
        {
            foreach (Fougerite.Entity xx in Util.GetUtil().FindEntitiesAround(player.Location, 6.5f))
            {
                if (xx.Name.ToLower().Contains("wall") || xx.Name.ToLower().Contains("foundation") || xx.Name.ToLower().Contains("ceiling") || xx.Name.ToLower().Contains("pillar"))
                {
                    if (xx.OwnerID == player.SteamID)
                    {
                        player.MessageFrom("AntiBug", green + "You spawned at you home :)");
                        break;
                    }
                    else
                    {
                        if (RustPPSupport)
                        {
                            var friendc = Fougerite.Server.GetServer().GetRustPPAPI().GetFriendsCommand.GetFriendsLists();
                            if (friendc.ContainsKey(xx.UOwnerID))
                            {
                                var fs = (RustPP.Social.FriendList)friendc[xx.UOwnerID];
                                bool isfriend = fs.Cast<FriendList.Friend>().Any(friend => friend.GetUserID() == player.UID);
                                if (isfriend)
                                {
                                    player.MessageFrom("AntiBug", green + "You spawned at friendly home :)");
                                    break;
                                }
                                else
                                {
                                    SendToSafeArea(player);
                                    break;
                                }
                            }
                            else
                            {
                                SendToSafeArea(player);
                                break;
                            }
                        }
                        else
                        {
                            SendToSafeArea(player);
                            break;
                        }
                    }
                }
            } 
        }
        public void SendToSafeArea(Fougerite.Player player)
        {
            player.MessageFrom("AntiBug", red + "Warning: " + yellow + "You have been moved to a random zone for security reasons");
            player.MessageFrom("AntiBug", yellow + "You need to get" + blue + " /addfriend " + yellow + "of the owner of the house to be able to spawn on it");
            player.MessageFrom("AntiBug", yellow + "More info " + red + " /antibug");
            int r = rnd.Next(1, 8156);
            string l = cfg.GetSetting("DefaultLoc", r.ToString());
            Vector3 v = Util.GetUtil().ConvertStringToVector3(l);
            player.SafeTeleportTo(v);//ATENCION SAFE TELEPORT
            return;
        }
    }
}
