using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "App.config", Watch = true)]
namespace SensorConfiguration
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static log4net.ILog OperationLog = log4net.LogManager.GetLogger("OperationLog");
        public static log4net.ILog InfoLog = log4net.LogManager.GetLogger("InfoLog");
        public static log4net.ILog ErrorLog = log4net.LogManager.GetLogger("ErrorLog");

    }
}
