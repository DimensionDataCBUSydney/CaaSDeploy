﻿using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Models;
using DD.CBU.CaasDeploy.Library.Utilities;
using DD.CBU.CaasDeploy.PostDeployScriptRunner;
using Newtonsoft.Json.Linq;

namespace DD.CBU.CaasDeploy.Library.Tasks
{
    /// <summary>
    /// An implementation of <see cref="ITask"/> which executes a post deployment script.
    /// </summary>
    public sealed class ExecuteScriptTask : ITask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteScriptTask"/> class.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="scriptPath">The path to the scripts folder.</param>
        public ExecuteScriptTask(Resource resource, string scriptPath)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            Resource = resource;
            ScriptPath = scriptPath;
        }

        /// <summary>
        /// Gets the resource to execute the script for.
        /// </summary>
        public Resource Resource { get; private set; }

        /// <summary>
        /// Gets the path to the scripts folder.
        /// </summary>
        public string ScriptPath { get; private set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="runtimeContext">The runtime context.</param>
        /// <param name="taskContext">The task execution context.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task Execute(RuntimeContext runtimeContext, TaskContext taskContext)
        {
            throw new NotSupportedException("Script are currently not supported by the console application.");

            var details = taskContext.ResourcesProperties[Resource.ResourceId];

            runtimeContext.LogProvider.LogMessage($"Running deployment scripts.");
            string ipv6Address = details["networkInfo"]["primaryNic"]["ipv6"].Value<string>();
            string ipv6Unc = IPv6ToUnc(ipv6Address);
            OSType osType = details["operatingSystem"]["family"].Value<string>() == "WINDOWS" ? OSType.Windows : OSType.Linux;

            string userName = osType == OSType.Windows ? "administrator" : "root";
            string password = Resource.ResourceDefinition["administratorPassword"].Value<string>();

            var scriptRunner = PostDeployScriptRunnerFactory.Create(ipv6Unc, userName, password, osType);

            string scriptPath = UnzipScriptBundle(ScriptPath, Resource.Scripts.BundleFile);
            var scriptDirectory = new DirectoryInfo(scriptPath);
            foreach (var scriptFile in scriptDirectory.EnumerateFiles())
            {
                runtimeContext.LogProvider.LogMessage("\tCopying file " + scriptFile.Name);
                await scriptRunner.UploadScript(scriptFile.FullName);
                scriptFile.Delete();
            }
            scriptDirectory.Delete();

            string deployScript = await TokenHelper.SubstituteTokensInString(runtimeContext, taskContext, Resource.Scripts.OnDeploy);
            runtimeContext.LogProvider.LogMessage("\tExecuting script " + deployScript);
            await scriptRunner.ExecuteScript(deployScript);
        }

        /// <summary>
        /// Unzips the script bundle.
        /// </summary>
        /// <param name="scriptPath">The script path.</param>
        /// <param name="bundleFile">The bundle file name.</param>
        /// <returns>The absolute path to the extracted folder.</returns>
        private string UnzipScriptBundle(string scriptPath, string bundleFile)
        {
            var bundlePath = Path.Combine(scriptPath, bundleFile);
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            ZipFile.ExtractToDirectory(bundlePath, path);
            return path;
        }

        /// <summary>
        /// Converts an IP V6 address to a UNC path.
        /// </summary>
        /// <param name="ipv6Address">The IP V6 address.</param>
        /// <returns>The UNC path</returns>
        private string IPv6ToUnc(string ipv6Address)
        {
            return ipv6Address.Replace(':', '-').Replace('%', 's') + ".ipv6-literal.net";
        }
    }
}
