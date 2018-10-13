using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Tiled2Unity
{
    // Helper class to write to Editor.log file
    public static class Log
    {
        private static readonly bool Enabled = true;

        public static void Report(string msg)
        {
            if (Enabled)
            {
                System.Console.WriteLine(msg);
            }
        }

        public static void Report(string fmt, params object[] args)
        {
            string msg = String.Format(fmt, args);
            Report(msg);
        }
    }

   
}
