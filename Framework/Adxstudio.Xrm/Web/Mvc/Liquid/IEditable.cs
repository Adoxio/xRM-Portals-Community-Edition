/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using DotLiquid;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public interface IEditable
	{
		string GetEditable(Context context, EditableOptions options);
	}
}
