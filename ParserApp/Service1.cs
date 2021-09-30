using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParserApp
{
    public partial class Service1 : ServiceBase
    {
        Logger logger;
        public bool categoriesParsed = false;
        public Service1()
        {
            InitializeComponent();
            this.EventLog.Source = this.ServiceName;
            EventLog.Log = "Application";
            this.AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            logger = new Logger();
            Thread thread = new Thread(new ThreadStart(logger.Start));
            thread.Start();
        }

        protected override void OnStop()
        {
            logger.Stop();
            Thread.Sleep(1000);
        }
    }

    class Logger
    {
        bool enabled = true;
        ParseRia parser = new ParseRia();
        TimeSpan time = new TimeSpan(0, 20, 0);

        public void Start()
        {
            while (enabled)
            {
                parser.ParseFromRia();
                Thread.Sleep(time);
            }
        }
        public void Stop()
        {
            enabled = false;
        }
    }
}
