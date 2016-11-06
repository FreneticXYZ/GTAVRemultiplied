﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticScript;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System.IO;
using GTAVRemultiplied.ServerSystem.CommonCommands;
using GTAVRemultiplied.ServerSystem.EntityCommands;
using GTAVRemultiplied.ServerSystem.TagBases;
using GTA;
using GTA.Math;
using GTA.Native;
using GTAVRemultiplied.SharedSystems;

namespace GTAVRemultiplied.ServerSystem
{
    public class GTAVFreneticServer
    {
        public static string HostAccount
        {
            get
            {
                return AccountHelper.Username;
            }
        }

        public static bool Enabled = false;

        public static GTAVFreneticServerOutputter Output;

        public static Commands CommandSystem;

        public static Scheduler Schedule = new Scheduler();

        public static void AutorunScripts()
        {
            string[] files = Directory.GetFiles(Environment.CurrentDirectory + "/frenetic/server/scripts/", "*.cfg", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string cmd = File.ReadAllText(file).Replace("\r", "").Replace("\0", "\\0");
                CommandSystem.PrecalcScript(file.Replace(Environment.CurrentDirectory, ""), cmd);
            }
            CommandSystem.RunPrecalculated();
        }

        public static float cDelta = 0;

        public static double GlobalTickTime = 100;

        public static void Tick(float delta)
        {
            cDelta = delta;
            GlobalTickTime += cDelta;
            if (Enabled)
            {
                Schedule.RunAllSyncTasks(delta);
                CommandSystem.Tick(delta);
            }
        }

        public static void Shutdown()
        {
            if (!Enabled)
            {
                return;
            }
            Enabled = false;
            Connections.Listener.Stop();
        }

        public static ushort CurrentPort = 28010;

        public static GTAVServerConnection Connections;

        public static void Init(ushort port)
        {
            if (Enabled)
            {
                return;
            }
            Enabled = true;
            Directory.CreateDirectory(Environment.CurrentDirectory + "/frenetic/server/scripts/");
            if (!ModelEnforcementScript.WantedModel.HasValue)
            {
                GTAVUtilities.SwitchCharacter(PedHash.DeadHooker);
            }
            Connections = new GTAVServerConnection();
            Connections.Listen(port);
            Output = new GTAVFreneticServerOutputter();
            CommandSystem = new Commands();
            Output.Syst = CommandSystem;
            CommandSystem.Output = Output;
            CommandSystem.Init();
            // Common Commands
            CommandSystem.RegisterCommand(new DevelCommand());
            // Entity Commands
            CommandSystem.RegisterCommand(new ModVehicleCommand());
            CommandSystem.RegisterCommand(new RepairVehicleCommand());
            // Tag Bases
            CommandSystem.TagSystem.Register(new GameTagBase());
            // TODO: Register tag types!
            // Wrap up
            CommandSystem.PostInit();
            AutorunScripts();
        }

        public static bool IsInRangeOfPlayer(Vector3 location, float range = 100)
        {
            float rsq = range * range;
            for (int i = 0; i < Connections.Connections.Count; i++)
            {
                if (Connections.Connections[i].lPos.DistanceToSquared2D(location) < rsq)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
