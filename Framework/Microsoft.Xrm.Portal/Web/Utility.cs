/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;

namespace Microsoft.Xrm.Portal.Web
{
	internal static class Utility
	{
		public static EmbeddedResourceVirtualPathProvider RegisterEmbeddedResourceVirtualPathProvider()
		{
			return RegisterVirtualPathProvider<EmbeddedResourceVirtualPathProvider>();
		}

		public static T RegisterVirtualPathProvider<T>() where T : VirtualPathProvider, new()
		{
			return RegisterPrecompiledVirtualPathProvider<T>();
		}

		private static T RegisterPrecompiledVirtualPathProvider<T>() where T : VirtualPathProvider, new()
		{
			var providerInstance = new T();

			var hostingEnvironmentInstance = typeof(HostingEnvironment).InvokeMember(
				"_theHostingEnvironment",
				BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField,
				null,
				null,
				null) as HostingEnvironment;

			if (hostingEnvironmentInstance == null) return null;

			var mi = typeof(HostingEnvironment).GetMethod(
				"RegisterVirtualPathProviderInternal",
				BindingFlags.NonPublic | BindingFlags.Static);

			if (mi == null) return null;

			mi.Invoke(hostingEnvironmentInstance, new object[] { providerInstance });

			return providerInstance;
		}

		public static IEnumerable<Assembly> GetAssemblies()
		{
			return
				from assembly in AppDomain.CurrentDomain.GetAssemblies()
				// AssemblyBuilder is a subclass of Assembly where GetManifestResourceStream is not implemented and needs to be skipped
				where !(assembly is AssemblyBuilder) && assembly.GetType().FullName != "System.Reflection.Emit.InternalAssemblyBuilder"
				select assembly;
		}

		public static IEnumerable<EmbeddedResourceAssemblyAttribute> GetEmbeddedResourceMappingAttributes()
		{
			return
				from assembly in GetAssemblies()
				from attribute in assembly.GetCustomAttributes(typeof(EmbeddedResourceAssemblyAttribute), true)
				select attribute as EmbeddedResourceAssemblyAttribute;
		}

		public static EmbeddedResourceAssemblyAttribute Match(this IEnumerable<EmbeddedResourceAssemblyAttribute> attributes, string path)
		{
			var appRelativePath = VirtualPathUtility.ToAppRelative(path);
			var relativePath = VirtualPathUtility.IsAppRelative(appRelativePath) ? appRelativePath : "~" + appRelativePath;

			return attributes.FirstOrDefault(m => Regex.IsMatch(relativePath, m.VirtualPathMask, RegexOptions.IgnoreCase));
		}
	}
}
