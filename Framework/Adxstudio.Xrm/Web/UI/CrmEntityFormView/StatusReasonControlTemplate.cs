/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Adxstudio.Xrm.Globalization;
using Adxstudio.Xrm.Web.UI.WebControls;
using Microsoft.Xrm.Portal.Web.UI.CrmEntityFormView;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.UI.CrmEntityFormView
{
	public class StatusReasonControlTemplate : CellTemplate, ICustomFieldControlTemplate
	{
		public StatusReasonControlTemplate(CrmEntityFormViewField field, FormXmlCellMetadata metadata, string validationGroup, IDictionary<string, CellBinding> bindings)
			: base(metadata, validationGroup, bindings)
		{
			Field = field;
		}

		public override string CssClass
		{
			get { return "status"; }
		}

		public CrmEntityFormViewField Field { get; private set; }

		protected override void InstantiateControlIn(Control container)
		{
			var label = new HtmlGenericControl("span") { ID = ControlID };

			label.Attributes["class"] = string.Join(" ", CssClass, Metadata.CssClass);

			container.Controls.Add(label);

			var options = Metadata.StatusOptionSetOptions;
			
			Bindings[Metadata.DataFieldName] = new CellBinding
			{
				Get = () => null,
				Set = obj =>
				{
					var value = ((OptionSetValue)obj).Value;

					foreach (var option in options)
					{
						if (option == null || option.Value == null || option.Value.Value != value)
						{
							continue;
						}

						var localizedLabel = option.Label.LocalizedLabels.FirstOrDefault(l => l.LanguageCode == Metadata.LanguageCode);

						label.InnerHtml = localizedLabel == null ? option.Label.GetLocalizedLabelString() : localizedLabel.Label;
					}
				}
			};
		}
	}
}
