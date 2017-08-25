/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	public interface IFileSystem
	{
		ICrmEntitySecurityProvider SecurityProvider { get; }

		OrganizationServiceContext ServiceContext { get; }

		IEntityUrlProvider UrlProvider { get; }

		EntityReference Website { get; }

		T Using<T>(Func<IFileSystemContext, T> action);

		T Using<T>(DirectoryContentHash cwd, Func<IFileSystemContext, T> action);
	}

	internal interface IEntityDirectoryFileSystem : IFileSystem
	{
		IDictionary<string, DirectoryType> EntityDirectoryTypes { get; }
	}
	
	internal abstract class EntityDirectoryFileSystem : IEntityDirectoryFileSystem
	{
		protected EntityDirectoryFileSystem(IDataAdapterDependencies dependencies)
		{
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Dependencies = dependencies;

			SecurityProvider = Dependencies.GetSecurityProvider();
			ServiceContext = Dependencies.GetServiceContext();
			UrlProvider = Dependencies.GetUrlProvider();
			Website = Dependencies.GetWebsite();
		}

		public abstract IDictionary<string, DirectoryType> EntityDirectoryTypes { get; }

		public ICrmEntitySecurityProvider SecurityProvider { get; private set; }

		public OrganizationServiceContext ServiceContext { get; private set; }

		public IEntityUrlProvider UrlProvider { get; private set; }

		public EntityReference Website { get; private set; }
		
		protected IDataAdapterDependencies Dependencies { get; private set; }

		public abstract T Using<T>(Func<IFileSystemContext, T> action);

		public abstract T Using<T>(DirectoryContentHash cwd, Func<IFileSystemContext, T> action);
	}
}
