/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web.UI;
using System.Web.UI.WebControls;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	/// <summary>
	/// Binds the value of a property of a collection item to a parameter object.
	/// </summary>
	public sealed class CacheItemParameter : Parameter
	{
		private string _propertyName;

		/// <summary>
		/// Gets or sets the name of the property from which to obtain the value.
		/// </summary>
		public string PropertyName
		{
			get { return _propertyName; }
			set { _propertyName = value; }
		}

		private string _format;

		/// <summary>
		/// Gets or sets an optional format string for augmenting the value.
		/// </summary>
		public string Format
		{
			get { return _format; }
			set { _format = value; }
		}

		public object Eval(object container)
		{
			if (string.IsNullOrEmpty(Format))
			{
				return DataBinder.Eval(container, PropertyName);
			}
			else
			{
				return DataBinder.Eval(container, PropertyName, Format);
			}
		}
	}
}
