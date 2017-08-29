/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Adxstudio.Xrm.Resources;
using DotLiquid;
using DotLiquid.Exceptions;
using Microsoft.Xrm.Client;
using Newtonsoft.Json.Linq;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class EmbeddedResourceFileSystem : IComposableFileSystem
	{
		private readonly Regex _resourceRegex;

		public EmbeddedResourceFileSystem(Assembly assembly, string root)
		{
			if (assembly == null) throw new ArgumentNullException("assembly");
			if (root == null) throw new ArgumentNullException("root");

			Assembly = assembly;
			Root = root;

			_resourceRegex = new Regex(@"^{0}\.(?<name>[a-zA-Z0-9_\.]+)\.(liquid|json)$".FormatWith(Regex.Escape(Root)), RegexOptions.CultureInvariant);
		}

		public Assembly Assembly { get; private set; }

		public string Root { get; private set; }

		public IEnumerable<TemplateFileInfo> GetTemplateFiles()
		{
			return Assembly.GetManifestResourceNames()
				.Select(name => new { Resource = name, Match = _resourceRegex.Match(name) })
				.Where(e => e.Match.Success)
				.GroupBy(e => e.Match.Groups["name"].Value, e => e.Resource, StringComparer.InvariantCulture)
				.Select(e => new TemplateFileInfo(e.Key.Replace(".", "/"), GetTemplateMetadata(e)))
				.ToArray();
		}

		public string ReadTemplateFile(Context context, string templateName)
		{
			string template;

			if (TryReadTemplateFile(context, templateName, out template))
			{
				return template;
			}

			throw new FileSystemException("Template {0} not found.", context[templateName] as string);
		}

		public bool TryReadTemplateFile(Context context, string templateName, out string template)
		{
			var templatePath = (string)context[templateName];

			return TryReadTemplateFile(templatePath, out template);
		}

		public bool TryReadTemplateFile(string templateName, out string template)
		{
			template = null;

			string fullPath;

			if (!TryGetFullPath(templateName, out fullPath))
			{
				return false;
			}

			try
			{
				var stream = Assembly.GetManifestResourceStream(fullPath);

				if (stream == null)
				{
					return false;
				}

				using (var reader = new StreamReader(stream))
				{
					template = reader.ReadToEnd();
				}

				return true;
			}
			catch
			{
				return false;
			}
		}

		private JObject GetTemplateMetadata(IEnumerable<string> resources)
		{
			var metadataResource = resources
				.FirstOrDefault(e => string.Equals(Path.GetExtension(e), ".json", StringComparison.InvariantCulture));

			return metadataResource == null
				? null
				: GetTemplateMetadata(metadataResource);
		}

		private JObject GetTemplateMetadata(string resource)
		{
			try
			{
				var stream = Assembly.GetManifestResourceStream(resource);

				if (stream == null)
				{
					return null;
				}

				using (var reader = new StreamReader(stream))
				{
					var metadata = JObject.Parse(reader.ReadToEnd());
					var localizedMetadata = new JObject();

					JToken title;

					if (metadata.TryGetValue("title", out title) && title.Type == JTokenType.String)
					{
						localizedMetadata["title"] = ResourceManager.GetString(title.Value<string>()) ?? title.Value<string>();
					}

					JToken description;

					if (metadata.TryGetValue("description", out description) && description.Type == JTokenType.String)
					{
						localizedMetadata["description"] = ResourceManager.GetString(description.Value<string>()) ?? description.Value<string>();
					}

					metadata.Merge(localizedMetadata);

					return metadata;
				}
			}
			catch
			{
				return null;
			}
		}

		private bool TryGetFullPath(string templatePath, out string fullPath)
		{
			fullPath = null;

			if (templatePath == null || !Regex.IsMatch(templatePath, @"^[a-zA-Z0-9_][a-zA-Z0-9_\/]*$"))
			{
				return false;
			}

			try
			{
				var basePath = templatePath.Contains("/")
					? Path.Combine(Root, Path.GetDirectoryName(templatePath))
					: Root;

				var fileName = "{0}.liquid".FormatWith(Path.GetFileName(templatePath));

				fullPath = Regex.Replace(Path.Combine(basePath, fileName), @"\\|/", ".");

				return true;
			}
			catch (System.ArgumentException)
			{
				return false;
			}
		}
	}
}
