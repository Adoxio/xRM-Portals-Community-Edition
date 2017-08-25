/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Decorators
{
	/// <summary>
	/// Interface for all HttpContextDecorators
	/// </summary>
	public interface IHttpContextDecorator
	{
		/// <summary>
		/// Decorates the HttpContext
		/// </summary>
		/// <returns>bool if successful; otherwise false</returns>
		bool Decorate();
	}
}
