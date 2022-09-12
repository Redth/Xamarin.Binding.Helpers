using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using NuGet.Frameworks;
using NuGet.Packaging;
using Xamarin.Binding.Helpers.NugetResolvers;
using Xamarin.Build;
using MSBuildTaskItem = Microsoft.Build.Utilities.TaskItem;

namespace Xamarin.Binding.Helpers.Tasks
{
	public class XbhAndroidStudioProjectTask : AsyncTask
	{

		[Required]
		public string IntermediateOutputPath { get; set; }

		public ITaskItem[] Projects { get; set; }

		public ITaskItem[] MavenNugetPairings { get; set; }

		[Output]
		public ITaskItem[] ResolvedArtifacts { get; set; }

		[Output]
		public ITaskItem[] NugetReferences { get; set; }

		[Output]
		public ITaskItem[] LocalAarArtifacts { get; set; }

		[Output]
		public ITaskItem[] LocalJarArtifacts { get; set; }

		[Output]
		public ITaskItem[] BindableLocalAars { get;set; }

		[Required]
		public string TargetFrameworkPath { get; set; }

		public override bool Execute()
		{
			Task.Run(async () =>
			{
				try
				{
					await DoExecute();
				}
				catch (Exception ex)
				{
					Log.LogErrorFromException(ex);
				}
				finally
				{
					Complete();
				}

			});

			return base.Execute();
		}

		async Task DoExecute()
		{
			var mappings = new List<ExplicitMavenNugetMapping>();
			var ignoredMappings = new List<ExplicitMavenNugetMapping>();

			if (MavenNugetPairings != null)
			{
				foreach (var m in MavenNugetPairings)
				{
					var gid = m.GetMetadata("MavenGroupId");
					var aid = m.GetMetadata("MavenArtifactId");
					var mv = m.GetMetadata("MavenVersion");
					var nid = m.GetMetadata("NuGetPackageId");
					var nv = m.GetMetadata("NuGetVersion");
					var ignore = m.GetMetadata("Ignore")?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;

					if (string.IsNullOrEmpty(gid))
						throw new ArgumentNullException("MavenNuGetMapping item is missing the 'MavenGroupId' attribute.");
					if (string.IsNullOrEmpty(aid))
						throw new ArgumentNullException("MavenNuGetMapping item is missing the 'MavenArtifactId' attribute.");

					// Don't need nuget info if we're explicitly ignoring it from being mapped
					if (!ignore)
					{
						if (string.IsNullOrEmpty(nid))
							throw new ArgumentNullException("MavenNuGetMapping item is missing the 'NuGetPackageId' attribute.");
						if (string.IsNullOrEmpty(nv))
							throw new ArgumentNullException("MavenNuGetMapping item is missing the 'NuGetVersion' attribute.");
					}

					var mapping = new ExplicitMavenNugetMapping(gid, aid, mv, nid, nv, ignore);

					mv = string.IsNullOrEmpty(mv) ? "*" : mv;
					nv = string.IsNullOrEmpty(nv) ? "*" : nv;

					var nugetPkg = ignore ? "<Ignored>" : $"{nid}:{nv}";
					Log.LogMessage(MessageImportance.Normal, $"Explicitly mapping Maven Artifact: {gid}:{aid}:{mv} to NuGet Package: {nugetPkg}");

					if (ignore)
						ignoredMappings.Add(mapping);
					else
						mappings.Add(mapping);
				}
			}

			var bindableAars = new List<ITaskItem>();
			var allDeps = new List<MavenArtifactNuGetPairing>();

			foreach (var p in Projects)
			{
				var projectPath = p.ItemSpec;
				var module = p.GetMetadata("Module");
				var generateBinding = false;
				
				bool.TryParse(p.GetMetadata("GenerateBinding"), out generateBinding);

				if (string.IsNullOrEmpty(module))
					throw new ArgumentNullException("AndroidStudioProject item is missing the 'Module' attribute.");

				var ap = new AndroidStudioProject(projectPath);

				var projInfo = await ap.GetDependencies(module, mappings, ignoredMappings);
				allDeps.AddRange(projInfo.AllDependencies);

				if (!string.IsNullOrEmpty(projInfo.ModuleArtifact))
				{
					bindableAars.Add(new MSBuildTaskItem(projInfo.ModuleArtifact, new Dictionary<string, string> {
						{
							"MavenIdentityHash",
							Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(
								Encoding.UTF8.GetBytes(projInfo.ModuleArtifact))).Substring(0, 8) 
						},
						{ "GenerateBinding", generateBinding.ToString() }
					}));
				}
			}

			BindableLocalAars = bindableAars.ToArray();

			var resolvedArtifacts = new List<MSBuildTaskItem>();
			var nugetReferences = new List<MSBuildTaskItem>();
			var localAarArtifacts = new List<MSBuildTaskItem>();
			var localJarArtifacts = new List<MSBuildTaskItem>();

			var targetsArtifacts = new List<string>();

			foreach (var d in allDeps)
			{
				var attr = new Dictionary<string, string>();
				string itemSpec = $"{d.MavenDependency.GroupId}:{d.MavenDependency.ArtifactId}:{d.MavenDependency.ResolvedVersion ?? d.MavenDependency.RequestedVersion}";
				var artifactType = string.Empty;
				if (!string.IsNullOrEmpty(d.MavenDependency.GroupId))
					attr.Add("MavenGroupId", d.MavenDependency.GroupId);
				if (!string.IsNullOrEmpty(d.MavenDependency.ArtifactId))
					attr.Add("MavenArtifactId", d.MavenDependency.ArtifactId);
				if (!string.IsNullOrEmpty(d.MavenDependency.ArtifactId) && !string.IsNullOrEmpty(d.MavenDependency.GroupId))
					attr.Add("MavenIdentity", $"{d.MavenDependency.GroupId}.{d.MavenDependency.ArtifactId}");
				if (!string.IsNullOrEmpty(d.MavenDependency.ResolvedVersion))
					attr.Add("MavenVersion", d.MavenDependency.ResolvedVersion);
				if (!string.IsNullOrEmpty(d.MavenDependency.RequestedVersion))
					attr.Add("MavenRequestedVersion", d.MavenDependency.RequestedVersion);
				if (!string.IsNullOrEmpty(d.MavenDependency.File))
				{
					attr.Add("Path", d.MavenDependency.File);
					if (d.MavenDependency.IsAar || d.MavenDependency.IsJar)
						attr.Add("ArtifactType", d.MavenDependency.IsAar ? "aar" : "jar");
				}

				if (d.NuGet != null)
				{
					if (!string.IsNullOrEmpty(d.NuGet.PackageId))
						attr.Add("NuGetPackageId", d.NuGet.PackageId);
					if (!string.IsNullOrEmpty(d.NuGet.Version))
						attr.Add("NuGetVersion", d.NuGet.Version);

					nugetReferences.Add(new MSBuildTaskItem(itemSpec, attr));
				}
				else
				{
					if (d.MavenDependency.IsAar)
					{
						localAarArtifacts.Add(new MSBuildTaskItem(itemSpec, attr));
						if (File.Exists(d.MavenDependency.File))
							targetsArtifacts.Add(Path.GetFileName(d.MavenDependency.File));
					}
					else if (d.MavenDependency.IsJar)
						localJarArtifacts.Add(new MSBuildTaskItem(itemSpec, attr));
				}

				resolvedArtifacts.Add(new MSBuildTaskItem(itemSpec, attr));
			}

			ResolvedArtifacts = resolvedArtifacts.ToArray();
			NugetReferences = nugetReferences.ToArray();
			LocalAarArtifacts = localAarArtifacts.ToArray();
			LocalJarArtifacts = localJarArtifacts.ToArray();

			var fullIntermediateOutputPath = new DirectoryInfo(IntermediateOutputPath);
			if (!fullIntermediateOutputPath.Exists)
				fullIntermediateOutputPath.Create();

			using (var w = new StreamWriter(Path.Combine(fullIntermediateOutputPath.FullName, "package.targets"), false))
			{
				w.WriteLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
				w.WriteLine(@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">");
				w.WriteLine("  <ItemGroup>");

				foreach (var a in targetsArtifacts)
					w.WriteLine(@"    <AndroidAarLibrary Include=""$(MSBuildThisFileDirectory)..\..\build\" + TargetFrameworkPath + "\\" + a + "\" />");

				foreach (var a in bindableAars)
				{
					var bindableAarFilename = Path.GetFileName(a.ItemSpec);
					w.WriteLine(@"    <AndroidAarLibrary Include=""$(MSBuildThisFileDirectory)..\..\build\" + TargetFrameworkPath + "\\" + bindableAarFilename + "\" />");
				}

				w.WriteLine("  </ItemGroup>");
				w.WriteLine(@"</Project>");
			}

			var localArtifactsIntermediate = allDeps.Where(d => d.NuGet == null && d.MavenDependency != null && d.MavenDependency.IsAar).ToList();
			foreach (var a in localArtifactsIntermediate)
				a.MavenDependency.File = Path.Combine(fullIntermediateOutputPath.FullName, "artifacts", Path.GetFileName(a.MavenDependency.File));

			var bindableArtifactsIntermediate = bindableAars.Select(a =>
				Path.Combine(fullIntermediateOutputPath.FullName, "artifacts", Path.GetFileName(a.ItemSpec)));

			var s = new AndroidSuggestions
			{
				NuGets = allDeps.Where(d => d.NuGet != null && !string.IsNullOrEmpty(d.NuGet.PackageId) && !string.IsNullOrEmpty(d.NuGet.Version)).ToList(),
				LocalArtifacts = localArtifactsIntermediate.ToList(),
				LocalBindableArtifacts = bindableArtifactsIntermediate.ToList(),
				TargetFramework = NuGetFramework.Parse(TargetFrameworkPath).Framework
			};

			var json = Newtonsoft.Json.JsonConvert.SerializeObject(s);

			File.WriteAllText(Path.Combine(fullIntermediateOutputPath.FullName, "_xbhsuggestandroid.json"), json);
		}
	}

	public class AndroidSuggestions
	{
		public string TargetFramework { get; set; }

		public List<MavenArtifactNuGetPairing> NuGets { get; set; } = new List<MavenArtifactNuGetPairing>();
		public List<MavenArtifactNuGetPairing> LocalArtifacts { get; set; } = new List<MavenArtifactNuGetPairing>();

		public List<string> LocalBindableArtifacts { get; set; } = new List<string>();
	}
}
