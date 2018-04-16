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
        public override string Name { get { return "AntiBugOnSpawn"; } }
        public override string Author { get { return "Salva/Juli"; } }
        public override string Description { get { return "AntiBugOnSpawn"; } }
        public override Version Version { get { return new Version("1.2"); } }

        private string red = "[color #B40404]";
        private string blue = "[color #81F7F3]";
        private string green = "[color #82FA58]";
        private string yellow = "[color #F4FA58]";
        private string orange = "[color #FF8000]";
        private string pink = "[color #FA58F4]";
        private string white = "[color #FFFFFF]";

        public static IniParser cfg;
        public static IniParser ClansIni;

        public static System.Random rnd;
        public bool PluginEnabled = false;
        public bool RustPPSupport = false;
        public bool ClansSupport = false;

        public override void Initialize()
        {
            Hooks.OnModulesLoaded += OnModulesLoaded;
            Hooks.OnServerLoaded += OnServerLoaded;
            Hooks.OnCommand += OnCommand;
            Hooks.OnPlayerSpawned += OnPlayerSpawned;
            

            rnd = new System.Random();
            cfg = new IniParser(Path.Combine(ModuleFolder, "DefaultLoc.ini"));
        }

        public override void DeInitialize()
        {
            Hooks.OnModulesLoaded -= OnModulesLoaded;
            Hooks.OnServerLoaded -= OnServerLoaded;
            Hooks.OnCommand -= OnCommand;
            Hooks.OnPlayerSpawned -= OnPlayerSpawned;
            
        }

        public void OnModulesLoaded()
        {
            ConsoleSystem.Print("");
            if (Fougerite.Server.GetServer().HasRustPP)
            {
                RustPPSupport = true;
                ConsoleSystem.Print("Plugin " + Name + " " + Version.ToString() + " Integration with Rust++ OK!!");
                if (File.Exists(Directory.GetCurrentDirectory() + "\\save\\PyPlugins\\Clans\\Clans.ini"))
                {
                    ClansSupport = true;
                    ClansIni = new IniParser(Directory.GetCurrentDirectory() + "\\save\\PyPlugins\\Clans\\Clans.ini");
                    ConsoleSystem.Print("Plugin " + Name + " " + Version.ToString() + " Integration with Clans OK!!");
                    //Logger.LogWarning("FOUGERITE");
                }
                else
                {
                    ConsoleSystem.PrintError("Plugin " + Name + " " + Version.ToString() + " Integration with Clans has Failed!! (OPTIONAL)");
                    ClansSupport = false;
                }
            }
            else
            {
                ConsoleSystem.PrintError("WARNING ,Plugin " + Name + " " + Version.ToString() + " DISABLED, Not found Rust ++, make sure you have Rust ++ installed");
                RustPPSupport = false;
                Fougerite.Hooks.OnPlayerSpawned -= OnPlayerSpawned;
            }
            ConsoleSystem.Print("");
        }

        public void OnServerLoaded()
        {
            char comillas = '"';
            ConsoleSystem.Run("sleepers.on " + comillas + "false" + comillas, false);
            Timer1(5 * 60000, null).Start();
        }
        public void OnCommand(Fougerite.Player player, string cmd, string[] args)
        {
            if (cmd == "antibug")
            {
                player.MessageFrom("AntiBug", yellow + "/antibug is a system that prevents players from using bugs in your house.");
                player.MessageFrom("*", yellow + " Option 1 - To be able to SPAWN at a friend's house, he must be your friend." + blue + "/addfriend NAME");
                if (ClansSupport == true)
                {
                    player.MessageFrom("*", yellow + " Option 2 - To be able to SPAWN at a Clan's house, you need to be a member of a clan");
                }
            }
        }
        public void OnPlayerSpawned(Fougerite.Player player, SpawnEvent se)
        {
            if (!PluginEnabled)
            {
                return;
            }

            if (RustPPSupport == true)
            {
                foreach (Fougerite.Entity xx in Util.GetUtil().FindEntitiesAround(player.Location, 5f))
                {
                    if (xx.Name.ToLower().Contains("ceiling"))
                    {
                        if (xx.OwnerID == player.SteamID)
                        {
                            player.MessageFrom("AntiBug", green + "You spawned at: You Home :)");
                            break;
                        }
                        else
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
                                    if (ClansSupport == true)
                                    {
                                        RecuperaListaDeClanes();
                                        if (SonDeElMismoClan(player.SteamID, xx.OwnerID, NombreDelClan(player.SteamID)))
                                        {
                                            player.MessageFrom("AntiBug", green + "You spawned at: Clan Home :)");
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
                            }
                            else
                            {
                                if (ClansSupport == true)
                                {
                                    RecuperaListaDeClanes();
                                    if (SonDeElMismoClan(player.SteamID, xx.OwnerID, NombreDelClan(player.SteamID)))
                                    {
                                        player.MessageFrom("AntiBug", green + "You spawned at: Clan Home :)");
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
                        }
                    }
                }
            }
        }
        public void RecuperaListaDeClanes()
        {
            ClansIni = new IniParser(Directory.GetCurrentDirectory() + "\\save\\PyPlugins\\Clans\\Clans.ini");
            return;
        }
        public string NombreDelClan(string IDjugador)
        {
            string nombredelclan = "XXXXXXXXXXXXX";
            if (ClansIni.ContainsSetting("ClanOwners", IDjugador))
            {
                nombredelclan = ClansIni.GetSetting("ClanOwners", IDjugador);
            }
            else
            {
                if (ClansIni.ContainsSetting("ClanMembers", IDjugador))
                {
                    nombredelclan = ClansIni.GetSetting("ClanMembers", IDjugador);
                }
            }
            return nombredelclan;
        }
        public bool SonDeElMismoClan(string IDjugador, string IDcasa, string ClanName)
        {
            if (ClansIni.ContainsSetting(ClanName, IDjugador) && ClansIni.ContainsSetting(ClanName, IDcasa))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool SonAmigosRustPP(string IDjugador, string IDcasa)
        {
            var friendc = Fougerite.Server.GetServer().GetRustPPAPI().GetFriendsCommand.GetFriendsLists();
            if (friendc.ContainsKey(IDcasa))
            {
                var fs = (RustPP.Social.FriendList)friendc[IDcasa];
                bool isfriend = fs.Cast<FriendList.Friend>().Any(friend => friend.GetUserID() == Convert.ToUInt64(IDjugador));
                if (isfriend)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public void SendToSafeArea(Fougerite.Player player)
        {
            player.MessageFrom("AntiBug", red + "Warning: " + white + " /antibug " + yellow + " You have been moved to a random zone for security reasons");
            player.MessageFrom("AntiBug", yellow + " Option 1 - You need to get" + blue + " /addfriend " + yellow + "of the OWNER of the house to be able to spawn on it");
            if (ClansSupport == true)
            {
                player.MessageFrom("AntiBug", yellow + " Option 2 - You need to get" + blue + " Clan Menber " + yellow + "of the CLAN of the house to be able to spawn on it");
            }
            int r = rnd.Next(1, 8156);
            string l = cfg.GetSetting("DefaultLoc", r.ToString());
            Vector3 v = Util.GetUtil().ConvertStringToVector3(l);
            player.SafeTeleportTo(v);//ATENCION SAFE TELEPORT
            return;
        }

        public TimedEvent Timer1(int timeoutDelay, Dictionary<string, object> args)
        {
            TimedEvent timedEvent = new TimedEvent(timeoutDelay);
            timedEvent.Args = args;
            timedEvent.OnFire += CallBack;
            return timedEvent;
        }
        public void CallBack(TimedEvent e)
        {
            e.Kill();
            PluginEnabled = true;
            char comillas = '"';
            ConsoleSystem.Run("sleepers.on " + comillas + "true" + comillas, false);
            Logger.Log(Name + Version + " Enabled after 5 mins of Server Restart");
            ConsoleSystem.Print(Name + Version + " Enabled after 5 mins of Server Restart");
        }
    }
}
