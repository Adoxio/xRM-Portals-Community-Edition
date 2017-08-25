/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using System.Threading;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Security;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Blogs
{
	/// <summary>
	/// Handles blog security assertions.
	/// </summary>
	public class BlogSecurityInfo
	{
		private readonly Lazy<Entity> _record;
		private readonly Lazy<bool> _userHasAuthorPermission;

		public BlogSecurityInfo(IDataAdapterDependencies dependencies)
		{
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Dependencies = dependencies;

			_userHasAuthorPermission = new Lazy<bool>(GetUserHasAuthorPermission, LazyThreadSafetyMode.None);
		}

		public BlogSecurityInfo(Entity record, IDataAdapterDependencies dependencies) : this(dependencies)
		{
			if (record == null) throw new ArgumentNullException("record");
			
			_record = new Lazy<Entity>(() => record, LazyThreadSafetyMode.None);
		}

		public BlogSecurityInfo(EntityReference record, IDataAdapterDependencies dependencies) : this(dependencies)
		{
			if (record == null) throw new ArgumentNullException("record");
			
			_record = new Lazy<Entity>(() => GetRecord(record), LazyThreadSafetyMode.None);
		}

		public bool UserHasAuthorPermission
		{
			get { return _userHasAuthorPermission.Value; }
		}

		protected IDataAdapterDependencies Dependencies { get; private set; }

		private Entity GetRecord(EntityReference record)
		{
			if (record == null)
			{
				return null;
			}

			if (record.LogicalName == "adx_blog")
			{
				var serviceContext = Dependencies.GetServiceContext();

				return serviceContext.CreateQuery("adx_blog")
					.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_blogid") == record.Id);
			}

			if (record.LogicalName == "adx_blogpost")
			{
				var serviceContext = Dependencies.GetServiceContext();

				return serviceContext.CreateQuery("adx_blogpost")
					.FirstOrDefault(e => e.GetAttributeValue<Guid>("adx_blogpostid") == record.Id);
			}

			return null;
		}

		private bool GetUserHasAuthorPermission()
		{
			var user = Dependencies.GetPortalUser();

			if (user == null)
			{
				return false;
			}

			var blog = _record.Value;

			if (blog == null)
			{
				return false;
			}

			var serviceContext = Dependencies.GetServiceContext();

			if (!serviceContext.IsAttached(blog))
			{
				blog = serviceContext.MergeClone(blog);
			}

			var securityProvider = Dependencies.GetSecurityProvider();

			return securityProvider.TryAssert(serviceContext, blog, CrmEntityRight.Change);
		}
	}
}
