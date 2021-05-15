using Kudu.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Kudu.Core.Deployment
{
    public class DeploymentLogger : ILogger
    {
        private readonly ILogger _innerLogger;
        private string siteName = "";
        private string framework = "";
        private string frameworkVersion = "";
        private string buildFlags = "";
        private string siteHostName = "";
        private string deploymentId = "";

        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(log4net.LogManager.GetRepository(Assembly.GetEntryAssembly()).Name, "DeploymentLogger");

        public DeploymentLogger(ILogger innerLogger, string deploymentId = "")
        {
            SetupFieldsFromEnv();
            this.deploymentId = deploymentId;

            _innerLogger = innerLogger;
        }

        public ILogger Log(string value, LogEntryType type)
        {
            if (!string.IsNullOrEmpty(value))
            {
                // Replace ',' with space since ',' seperated strings are used in td-agent-bit parsers
                value = value.Replace(',', ' ').Trim();
                log.Debug($"{siteHostName},{value},{buildFlags},{deploymentId},{type}");
            }

            //return NullLogger.Instance;
            return new CascadeLogger(new DeploymentLogger(NullLogger.Instance,deploymentId), _innerLogger.Log(value, type));
        }

        private void SetupFieldsFromEnv()
        {
            siteName = System.Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
            framework = System.Environment.GetEnvironmentVariable("FRAMEWORK");
            frameworkVersion = System.Environment.GetEnvironmentVariable("FRAMEWORK_VERSION");
            buildFlags = System.Environment.GetEnvironmentVariable("BUILD_FLAGS");
            siteHostName = System.Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
        }
    }
}
