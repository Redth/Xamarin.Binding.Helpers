using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using NuGet.Frameworks;
using Xamarin.Build;

namespace Xamarin.Binding.Helpers.Tasks
{
	public class XbhAddDependenciesToNupkgTask : AsyncTask
	{
		public ITaskItem[] NuGetPackOutput { get; set; }

		public ITaskItem[] TargetFrameworks { get; set; }

		[Required]
		public string IntermediateOutputPath { get; set; }

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
			var fullIntermediateOutputPath = new System.IO.DirectoryInfo(IntermediateOutputPath);
			if (!fullIntermediateOutputPath.Exists)
				fullIntermediateOutputPath.Create();

			// Find package refs for any tfm's
			var packageRefFiles = new Dictionary<string, NuGetFramework>();

			foreach (var tfm in TargetFrameworks)
			{
				var nugetFramework = NuGetFramework.Parse(tfm.ItemSpec);
	
				var pkgRefFile = Path.Combine(fullIntermediateOutputPath.FullName, nugetFramework?.GetShortFolderName() ?? ".", "xbh", "_xbhpackagerefs.txt");
				if (File.Exists(pkgRefFile))
					packageRefFiles.Add(pkgRefFile, nugetFramework);

				LogMessage($"Found: {tfm.ItemSpec} -> {pkgRefFile}");
			}

			
			var pkgRefs = new List<NuGetSuggestion>();


			foreach (var pkgRefFile in packageRefFiles)
			{
				var lines = File.ReadAllLines(pkgRefFile.Key);

				foreach (var line in lines)
				{
					if (string.IsNullOrEmpty(line))
						continue;

					var parts = line?.Split(new[] { '|' }, 2);
					if ((parts?.Length ?? 0) == 2)
						pkgRefs.Add(new NuGetSuggestion
						{
							PackageId = parts[0],
							Version = parts[1],
							Framework = pkgRefFile.Value
						});
				}
			}

			foreach (var ns in pkgRefs)
				LogMessage($"NuGet Suggestion: {ns.PackageId} ({ns.Version}) for {ns.Framework}");

			foreach (var item in NuGetPackOutput)
			{
				var file = new FileInfo(item.ItemSpec);
				
				if (file.Exists && file.Extension.Equals(".nupkg", StringComparison.OrdinalIgnoreCase))
				{
					LogMessage($"Adding nuget deps to: {file.FullName}");
					AddPackageReferencesToNupkg(file.FullName, pkgRefs);
				}
			}
		}

		void AddPackageReferencesToNupkg(string nupkg, IEnumerable<NuGetSuggestion> pkgRefs)
		{
			var pkgRefsByTfm = pkgRefs.GroupBy(p => p.Framework);

			using (var zipa = new ZipArchive(new FileStream(nupkg, FileMode.Open), ZipArchiveMode.Update))
			{
				string nuspecEntryName = zipa.Entries.FirstOrDefault(e => Path.GetExtension(e.Name).Equals(".nuspec", StringComparison.OrdinalIgnoreCase))?.FullName;

				var nuspecEntry = zipa.GetEntry(nuspecEntryName);
				var updatedNuspec = new MemoryStream();
				XNamespace xmlns = null;

				using (var entryStream = nuspecEntry.Open())
				{
					var xdoc = XDocument.Load(entryStream);
					
					xmlns = xdoc.Root.GetDefaultNamespace();

					var depsElem = xdoc.Descendants().FirstOrDefault(d => d.Name.LocalName == "dependencies");

					// It's possible dependencies element doesn't exist yet, so we may need to add it
					if (depsElem == null)
					{
						LogMessage("dependencies element missing, creating...");
						var metadataElem = xdoc.Descendants().FirstOrDefault(d => d.Name?.LocalName == "metadata");
						depsElem = new XElement(XName.Get("dependencies", xmlns.NamespaceName));
						metadataElem.Add(depsElem);
					}

					// Find existing dependency->group elements
					var depGroupElems = depsElem.Descendants().Where(d2 => d2.Name.LocalName == "group");

					// Parse out frameworks and keep the element so we can match existing ones later
					var existingDepGroups = new List<(NuGetFramework, XElement)>();

					foreach (var depGroupElem in depGroupElems)
					{
						var tfm = depGroupElem?.Attributes().FirstOrDefault(a => a?.Name?.LocalName == "targetFramework")?.Value;

						if (!string.IsNullOrEmpty(tfm))
							existingDepGroups.Add((NuGetFramework.Parse(tfm), depGroupElem));
					}

					// Go through each tfm we have package refs for
					// try and find the existing element to add to, otherwise create a new one
					foreach (var tfmGrp in pkgRefsByTfm)
					{
						var depGrp = existingDepGroups.FirstOrDefault(d => d.Item1.Equals(tfmGrp.Key));
						
						// Add the dependency group for this TFM if it doesn't exist
						if (depGrp == default || depGrp.Item1 == null || depGrp.Item2 == null)
						{
							LogMessage($"Dependency Group Missing, creating: {tfmGrp.Key.Framework}...");
							var newDepGrpElem = new XElement(XName.Get("group", xmlns.NamespaceName),
											new XAttribute("targetFramework", tfmGrp.Key.Framework));

							depsElem.Add(newDepGrpElem);

							depGrp = (tfmGrp.Key, newDepGrpElem);
						}

						// Add all the dependencies to the group
						foreach (var pkgRef in tfmGrp)
						{
							depGrp.Item2.Add(new XElement(XName.Get("dependency", xmlns.NamespaceName),
											new XAttribute("id", pkgRef.PackageId),
											new XAttribute("version", pkgRef.Version),
											new XAttribute("include", "All")));
						}
					}

					using (var xmlWriter = XmlWriter.Create(updatedNuspec))
					{
						xdoc.WriteTo(xmlWriter);
					}
				}

				// Remove old entry, add new one
				nuspecEntry.Delete();
				var newEntry = zipa.CreateEntry(nuspecEntryName);
				using (var newStream = newEntry.Open())
				{
					updatedNuspec.Position = 0;
					updatedNuspec.CopyTo(newStream);
				}
			}
		}
	}
}
