using SlowRacer.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SlowRacer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {


        public static string[] args;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            args = e.Args;

            if (App.args.Count() > 0) HandyTools.AppSavePath = HandyTools.AppSavePath + App.args[0].ToString() + '\\';
           




        }
    }

    
}
