using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;

namespace IVWPF
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {

        /// </summary>
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public static void Main(string[] args)
        {
            IVWPF.Loader.args = args;
            IVWPF.App app = new IVWPF.App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
