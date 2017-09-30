/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Mvc
{
	public class PortalViewEntityAttribute : IPortalViewAttribute
	{
		private readonly Lazy<string> _description;

		public PortalViewEntityAttribute(IPortalViewEntity entity, string logicalName, object value, string description = null)
			: this(entity, logicalName, value, new Lazy<string>(() => description, LazyThreadSafetyMode.None)) { }

		internal PortalViewEntityAttribute(IPortalViewEntity entity, string logicalName, object value, Lazy<string> description)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (logicalName == null)
			{
				throw new ArgumentNullException("logicalName");
			}

			if (description == null)
			{
				throw new ArgumentNullException("description");
			}

			Entity = entity;
			LogicalName = logicalName;
			Value = value;
			EntityReference = Entity.EntityReference;

			_description = description;
		}

		public string Description
		{
			get { return _description.Value; }
		}

		public EntityReference EntityReference { get; private set; }

		public string LogicalName { get; private set; }

		protected IPortalViewEntity Entity { get; private set; }

		public bool Editable
		{
			get { return Entity.Editable; }
		}

		public object Value { get; private set; }

		public override string ToString()
		{
			return Value == null ? string.Empty : Value.ToString();
		}
	}
}
