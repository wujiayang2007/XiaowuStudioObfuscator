﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace XiaowuStudioObfuscator
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainStart());
        }
    }
}
