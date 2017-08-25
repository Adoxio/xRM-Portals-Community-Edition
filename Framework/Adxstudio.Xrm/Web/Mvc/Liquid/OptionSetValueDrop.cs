/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using Adxstudio.Xrm.Core;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class OptionSetValueDrop : PortalDrop
	{
		private readonly string _attributeLogicalName;
		private readonly string _entityLogicalName;
		private readonly Lazy<string> _label;
		private readonly Lazy<int> _languageCode;
		private readonly OptionSetValue _value;

		public OptionSetValueDrop(IPortalLiquidContext portalLiquidContext, string entityLogicalName, string attributeLogicalName, OptionSetValue value, Lazy<int> languageCode = null) : base(portalLiquidContext)
		{
			if (entityLogicalName == null) throw new ArgumentNullException("entityLogicalName");
			if (attributeLogicalName == null) throw new ArgumentNullException("attributeLogicalName");
			if (value == null) throw new ArgumentNullException("value");

			_entityLogicalName = entityLogicalName;
			_attributeLogicalName = attributeLogicalName;
			_value = value;
			_languageCode = languageCode;

			_label = new Lazy<string>(GetLabel, LazyThreadSafetyMode.None);
		}

		public string Label
		{
			get { return _label.Value; }
		}

		public int Value
		{
			get { return _value.Value; }
		}

		private string GetLabel()
		{
			try
			{
				using (var serviceContext = PortalViewContext.CreateServiceContext())
				{
					return _languageCode != null && _languageCode.Value != 0
						? serviceContext.GetOptionSetValueLabel(_entityLogicalName, _attributeLogicalName, _value.Value, _languageCode.Value)
						: serviceContext.GetOptionSetValueLabel(_entityLogicalName, _attributeLogicalName, _value.Value);
				}
			}
			catch
			{
				return null;
			}
		}
	}
}
