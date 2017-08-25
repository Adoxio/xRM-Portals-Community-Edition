/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Adxstudio.Xrm.Cms;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Portal.Web.Providers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Adxstudio.Xrm.Web.Handlers.ElFinder
{
	public interface IDirectory
	{
		bool CanRead { get; }

		bool CanWrite { get; }

		DirectoryContent[] Children { get; }

		Entity Entity { get; }

		EntityReference EntityReference { get; }

		bool Exists { get; }

		DirectoryContent Info { get; }

		bool SupportsUpload { get; }

		string WebFileForeignKeyAttribute { get; }
	}

	internal abstract class Directory : IDirectory
	{
		protected const string DirectoryMimeType = "directory";

		private readonly Lazy<bool> _canRead;
		private readonly Lazy<bool> _canWrite;
		private readonly Lazy<DirectoryContent[]> _children;
		private readonly Lazy<DirectoryContent> _info;
		private readonly Lazy<Entity> _entity;
		private readonly Lazy<string> _url;
		
		protected Directory(IEntityDirectoryFileSystem fileSystem)
		{
			if (fileSystem == null) throw new ArgumentNullException("fileSystem");

			FileSystem = fileSystem;

			_canRead = new Lazy<bool>(GetCanRead, LazyThreadSafetyMode.None);
			_canWrite = new Lazy<bool>(GetCanWrite, LazyThreadSafetyMode.None);
			_children = new Lazy<DirectoryContent[]>(GetChildren, LazyThreadSafetyMode.None);
			_entity = new Lazy<Entity>(GetEntity, LazyThreadSafetyMode.None);
			_info = new Lazy<DirectoryContent>(GetInfo, LazyThreadSafetyMode.None);
			_url = new Lazy<string>(GetUrl, LazyThreadSafetyMode.None);
		}

		public bool CanRead
		{
			get { return _canRead.Value; }
		}

		public bool CanWrite
		{
			get { return _canWrite.Value; }
		}

		public DirectoryContent[] Children
		{
			get { return _children.Value; }
		}

		public Entity Entity
		{
			get { return _entity.Value; }
		}

		public abstract EntityReference EntityReference { get; }

		public virtual bool Exists
		{
			get { return Entity != null && CanRead; }
		}

		public DirectoryContent Info
		{
			get { return _info.Value; }
		}

		public abstract bool SupportsUpload { get; }

		public abstract string WebFileForeignKeyAttribute { get; }
		
		protected IEntityDirectoryFileSystem FileSystem { get; private set; }

		protected ICrmEntitySecurityProvider SecurityProvider
		{
			get { return FileSystem.SecurityProvider; }
		}

		protected OrganizationServiceContext ServiceContext
		{
			get { return FileSystem.ServiceContext; }
		}

		protected string Url
		{
			get { return _url.Value; }
		}

		protected IEntityUrlProvider UrlProvider
		{
			get { return FileSystem.UrlProvider; }
		}

		protected EntityReference Website
		{
			get { return FileSystem.Website; }
		}

		protected virtual bool GetCanRead()
		{
			return TryAssertSecurity(CrmEntityRight.Read);
		}

		protected virtual bool GetCanWrite()
		{
			return TryAssertSecurity(CrmEntityRight.Change);
		}

		protected abstract DirectoryContent[] GetChildren();

		protected abstract DirectoryContent GetInfo();

		protected abstract Entity GetEntity();

		protected abstract string GetUrl();

		protected virtual DirectoryContent GetDirectoryContent(Entity entity)
		{
			if (entity == null)
			{
				return null;
			}

			string url;

			try
			{
				url = UrlProvider.GetUrl(ServiceContext, entity);
			}
			catch (Exception e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Error getting URL for entity [{0}:{1}]: {2}", entity.LogicalName, entity.Id, e.ToString()));

                return null;
			}

			if (url == null)
			{
				return null;
			}

			bool canWrite;

			try
			{
				if (!SecurityProvider.TryAssert(ServiceContext, entity, CrmEntityRight.Read))
				{
					return null;
				}

				canWrite = SecurityProvider.TryAssert(ServiceContext, entity, CrmEntityRight.Change);
			}
			catch (InvalidOperationException e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Error validating security for entity [{0}:{1}]: {2}", entity.LogicalName, entity.Id, e.ToString()));

                return null;
			}
			
			var content = new DirectoryContent
			{
				url = url,
				date = FormatDateTime(entity.GetAttributeValue<DateTime?>("modifiedon")),
				read = true,
				rm = false,
			};

			DirectoryType directoryType;

			if (FileSystem.EntityDirectoryTypes.TryGetValue(entity.LogicalName, out directoryType))
			{
				content.hash = new DirectoryContentHash(entity, true).ToString();
				content.name = directoryType.GetDirectoryName(entity);
				content.mime = DirectoryMimeType;
				content.size = 0;
				content.write = directoryType.SupportsUpload && SecurityProvider.TryAssert(ServiceContext, entity, CrmEntityRight.Change);

				return content;
			}

			content.write = canWrite;
			content.name = entity.GetAttributeValue<string>("adx_name");
			content.hash = new DirectoryContentHash(entity).ToString();

			if (entity.LogicalName != "adx_webfile")
			{
				content.mime = "application/x-{0}".FormatWith(entity.LogicalName);
				content.size = 0;

				return content;
			}

			var fileNote = ServiceContext.GetNotes(entity)
				.Where(e => e.GetAttributeValue<bool?>("isdocument").GetValueOrDefault())
				.OrderByDescending(e => e.GetAttributeValue<DateTime?>("createdon"))
				.FirstOrDefault();

			if (fileNote == null)
			{
				return null;
			}

			content.mime = fileNote.GetAttributeValue<string>("mimetype");
			content.size = fileNote.GetAttributeValue<int?>("filesize").GetValueOrDefault();
			content.rm = canWrite;

			return content;
		}

		protected static string FormatDateTime(DateTime? dateTime)
		{
			return dateTime == null ? null : dateTime.ToString();
		}

		private bool TryAssertSecurity(CrmEntityRight right)
		{
			try
			{
				return SecurityProvider.TryAssert(ServiceContext, Entity, right);
			}
			catch (InvalidOperationException e)
			{
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Error validating security for entity [{0}:{1}]: {2}", Entity.LogicalName, Entity.Id, e.ToString()));

                return false;
			}
		}
	}
}
