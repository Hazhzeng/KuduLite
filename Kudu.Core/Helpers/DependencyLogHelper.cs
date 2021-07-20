using Kudu.Core.Infrastructure;
using Kudu.Core.Tracing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kudu.Core.Helpers
{
    class DependencyLogHelper
    {
        public static async Task LogDependenciesFile(string builtFolder)
        {
            try
            {
                await PrintRequirementsTxtDependenciesAsync(builtFolder);
                await PrintPackageJsonDependenciesAsync(builtFolder);
                PrintCsprojDependencies(builtFolder);
            }
            catch (Exception)
            {
                KuduEventGenerator.Log().GenericEvent(
                    ServerConfiguration.GetApplicationName(),
                    $"dependencies,failed to parse function app dependencies",
                    Guid.Empty.ToString(),
                    string.Empty,
                    string.Empty,
                    string.Empty);
            }
        }

        private static async Task PrintRequirementsTxtDependenciesAsync(string builtFolder)
        {
            string filename = "requirements.txt";
            string requirementsTxtPath = Path.Combine(builtFolder, filename);
            if (File.Exists(requirementsTxtPath))
            {
                string[] lines = await File.ReadAllLinesAsync(requirementsTxtPath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    {
                        continue;
                    }

                    int separatorIndex;
                    if (line.IndexOf("==") >= 0)
                    {
                        separatorIndex = line.IndexOf("==");
                    }
                    else if (line.IndexOf(">=") >= 0)
                    {
                        separatorIndex = line.IndexOf(">=");
                    }
                    else if (line.IndexOf("<=") >= 0)
                    {
                        separatorIndex = line.IndexOf("<=");
                    }
                    else if (line.IndexOf(">") >= 0)
                    {
                        separatorIndex = line.IndexOf(">");
                    }
                    else if (line.IndexOf("<") >= 0)
                    {
                        separatorIndex = line.IndexOf("<");
                    }
                    else
                    {
                        separatorIndex = line.Length;
                    }

                    string package = line.Substring(0, separatorIndex).Trim();
                    string version = line.Substring(separatorIndex).Trim();

                    KuduEventGenerator.Log().GenericEvent(
                        ServerConfiguration.GetApplicationName(),
                        $"dependencies,python,{filename},{package},{version}",
                        Guid.Empty.ToString(),
                        string.Empty,
                        string.Empty,
                        string.Empty);
                }
            }
        }

        private static async Task PrintPackageJsonDependenciesAsync(string builtFolder)
        {
            string filename = "package.json";
            string packageJsonPath = Path.Combine(builtFolder, filename);
            if (File.Exists(packageJsonPath))
            {
                string content = await File.ReadAllTextAsync(packageJsonPath);
                JObject jobj = JObject.Parse(content);
                if (jobj.ContainsKey("devDependencies"))
                {
                    Dictionary<string, string> dictObj = jobj["devDependencies"].ToObject<Dictionary<string, string>>();
                    foreach (string key in dictObj.Keys)
                    {
                        KuduEventGenerator.Log().GenericEvent(
                            ServerConfiguration.GetApplicationName(),
                            $"dependencies,node,{filename},{key},{dictObj[key]},devDependencies",
                            Guid.Empty.ToString(),
                            string.Empty,
                            string.Empty,
                            string.Empty);
                    }
                }

                if (jobj.ContainsKey("dependencies"))
                {
                    Dictionary<string, string> dictObj = jobj["dependencies"].ToObject<Dictionary<string, string>>();
                    foreach (string key in dictObj.Keys)
                    {
                        KuduEventGenerator.Log().GenericEvent(
                            ServerConfiguration.GetApplicationName(),
                            $"dependencies,node,{filename},{key},{dictObj[key]},dependencies",
                            Guid.Empty.ToString(),
                            string.Empty,
                            string.Empty,
                            string.Empty);
                    }
                }
            }
        }

        private static void PrintCsprojDependencies(string builtFolder)
        {
            foreach (string csprojPath in Directory.GetFiles(builtFolder, "*.csproj", SearchOption.TopDirectoryOnly))
            {
                string filename = Path.GetFileName(csprojPath);
                XElement purchaseOrder = XElement.Load(csprojPath);
                foreach (var itemGroup in purchaseOrder.Elements("ItemGroup"))
                {
                    foreach (var packageReference in itemGroup.Elements("PackageReference"))
                    {
                        string include = packageReference.Attribute("Include").Value;
                        string version = packageReference.Attribute("Version").Value;
                        KuduEventGenerator.Log().GenericEvent(
                            ServerConfiguration.GetApplicationName(),
                            $"dependencies,dotnet,{filename},{include},{version}",
                            Guid.Empty.ToString(),
                            string.Empty,
                            string.Empty,
                            string.Empty);
                    }
                }
            }
        }
    }
}
