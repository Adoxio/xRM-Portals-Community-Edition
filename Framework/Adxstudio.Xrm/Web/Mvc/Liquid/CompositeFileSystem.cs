/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using DotLiquid;
using DotLiquid.Exceptions;
using DotLiquid.FileSystems;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public interface IComposableFileSystem : IFileSystem
	{
		IEnumerable<TemplateFileInfo> GetTemplateFiles();

		bool TryReadTemplateFile(Context context, string templateName, out string template);

		bool TryReadTemplateFile(string templateName, out string template);
	}

	public class CompositeFileSystem : IComposableFileSystem
	{
		private readonly IEnumerable<IComposableFileSystem> _fileSystems;

		public CompositeFileSystem(params IComposableFileSystem[] fileSystems) : this(fileSystems as IEnumerable<IComposableFileSystem>) { }

		public CompositeFileSystem(IEnumerable<IComposableFileSystem> fileSystems)
		{
			if (fileSystems == null) throw new ArgumentNullException("fileSystems");

			_fileSystems = fileSystems;
		}

		public string ReadTemplateFile(Context context, string templateName)
		{
			string template;

			if (TryReadTemplateFile(context, templateName, out template))
			{
				return template;
			}

            var templateNotFoundException = string.Format(ResourceManager.GetString("Template_Not_Found_Exception"), context[templateName] as string);

            // Log template not found
            ADXTrace.Instance.TraceWarning(TraceCategory.Monitoring, templateNotFoundException);
            return templateNotFoundException;
		}

		public IEnumerable<TemplateFileInfo> GetTemplateFiles()
		{
			return _fileSystems
				.SelectMany(fs => fs.GetTemplateFiles())
				.GroupBy(template => template.Name, template => template, StringComparer.InvariantCulture)
				.Select(template => template.First());
		}

		public bool TryReadTemplateFile(Context context, string templateName, out string template)
		{
			template = null;

			foreach (var fileSystem in _fileSystems)
			{
				if (fileSystem.TryReadTemplateFile(context, templateName, out template))
				{
					return true;
				}
			}
            
            // Log template not found
            ADXTrace.Instance.TraceWarning(TraceCategory.Monitoring, string.Format(ResourceManager.GetString("Template_Not_Found_Exception"), templateName));
            return false;
		}

		public bool TryReadTemplateFile(string templateName, out string template)
		{
			template = null;

			foreach (var fileSystem in _fileSystems)
			{
				if (fileSystem.TryReadTemplateFile(templateName, out template))
				{
					return true;
				}
            }

            // Log template not found
            ADXTrace.Instance.TraceWarning(TraceCategory.Monitoring, string.Format(ResourceManager.GetString("Template_Not_Found_Exception"), templateName));
            return false;
		}
	}
}
