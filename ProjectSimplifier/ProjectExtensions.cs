using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet;

namespace ProjectSimplifier
{
    internal static class ProjectExtensions
    {
        public static void LogProjectProperties(this IProject project, string logFileName)
        {
            var lines = new List<string>();
            foreach (var prop in project.Properties.OrderBy(p => p.Name))
            {
                lines.Add($"{prop.Name} = {prop.EvaluatedValue}");
            }
            File.WriteAllLines(logFileName, lines);
        }

        public static string GetTargetFramework(this IProject project)
        {
            var tf = project.GetPropertyValue("TargetFramework");
            if (!string.IsNullOrEmpty(tf))
            {
                return tf;
            }
            var tfs = project.GetPropertyValue("TargetFrameworks");
            if (!string.IsNullOrEmpty(tfs))
            {
                return tfs;
            }

            var tfi = project.GetPropertyValue("TargetFrameworkIdentifier");
            if (tfi == "")
            {
                throw new InvalidOperationException("TargetFrameworkIdentifier is not set!");
            }

            var tfv = project.GetPropertyValue("TargetFrameworkVersion");

            switch (tfi)
            {
                case ".NETFramework":
                    tf = "net";
                    break;
                case ".NETStandard":
                    tf = "netstandard";
                    break;
                case ".NETCoreApp":
                    tf = "netcoreapp";
                    break;
                case ".NETPortable":
                    tf = "netstandard";
                    break;
                default:
                    throw new InvalidOperationException($"Unknown TargetFrameworkIdentifier {tfi}");
            }

            if (tfi == ".NETPortable")
            {
                var profile = project.GetPropertyValue("TargetFrameworkProfile");

                if (profile == string.Empty && tfv == "v5.0")
                {
                    tf = GetTargetFrameworkFromProjectJson(project);
                }
                else
                {
                    var netstandardVersion = Facts.PCLToNetStandardVersionMapping[profile];
                    tf += netstandardVersion;
                }
            }
            else
            {
                if (tfv == "")
                {
                    throw new InvalidOperationException("TargetFrameworkVersion is not set!");
                }

                tf += tfv.TrimStart('v');
            }

            return tf;
        }

        private static string GetTargetFrameworkFromProjectJson(IProject project)
        {
            var projectFolder = project.GetPropertyValue("MSBuildProjectDirectory");
            var projectJsonPath = Path.Combine(projectFolder, "project.json");

            var projectJsonContents = File.ReadAllText(projectJsonPath);

            var json = JObject.Parse(projectJsonContents);

            var frameworks = json["frameworks"];
            return ((JProperty)frameworks.Single()).Name;
        }

        public static IEnumerable<PackageReference> GetPackages(this IProject project)
        {
            var projectFolder = project.GetPropertyValue("MSBuildProjectDirectory");
            var projectJsonPath = Path.Combine(projectFolder, "packages.config");

            var file = new PackageReferenceFile(projectJsonPath);

            return file.GetPackageReferences();
        }
    }
}
