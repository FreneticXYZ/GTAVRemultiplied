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

namespace GTAVRemultiplied
{
    public class GTAVFrenetic
    {
        public static GTAVFreneticOutputter Output;

        public static Commands CommandSystem;

        public static void AutorunScripts()
        {
            string[] files = Directory.GetFiles(Environment.CurrentDirectory + "/frenetic/scripts/", "*.cfg", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string cmd = File.ReadAllText(file).Replace("\r", "").Replace("\0", "\\0");
                CommandSystem.PrecalcScript(file.Replace(Environment.CurrentDirectory, ""), cmd);
            }
            CommandSystem.RunPrecalculated();
        }

        public static void Tick(float delta)
        {
            CommandSystem.Tick(delta);
        }

        public static void Init()
        {
            Output = new GTAVFreneticOutputter();
            CommandSystem = new Commands();
            Output.Syst = CommandSystem;
            CommandSystem.Output = Output;
            CommandSystem.Init();
            // Register things...
            CommandSystem.PostInit();
            AutorunScripts();
        }
    }
}
