/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Adxstudio.Xrm.Forums;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class ForumDrop : PortalViewEntityDrop
	{
		private IDataAdapterDependencies _dependencies;

		public ForumDrop(IPortalLiquidContext portalLiquidContext, IForum forum, IDataAdapterDependencies dependencies)
			: base(portalLiquidContext, forum)
		{
			if (dependencies == null) throw new ArgumentNullException("dependencies");

			Forum = forum;

			_dependencies = dependencies;
		}

		protected IForum Forum { get; private set; }

		public string Name
		{
			get { return Forum.Name; }
		}

		public ForumThreadsDrop Threads
		{
			get { return new ForumThreadsDrop(this, _dependencies, Forum); }
		}

		public int ThreadCount
		{
			get { return Forum.ThreadCount; }
		}

		public int PostCount
		{
			get { return Forum.PostCount;  }
		}
	}
}
