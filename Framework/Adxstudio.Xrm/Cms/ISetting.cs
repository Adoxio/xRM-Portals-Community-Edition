/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	public interface ISetting
	{
		Entity Entity { get; }

		string Name { get; }

		string Value { get; }
	}
}
