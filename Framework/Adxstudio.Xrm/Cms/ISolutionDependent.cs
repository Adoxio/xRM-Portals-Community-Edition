/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Cms
{
	/// <summary>
	/// Depends on CRM Solution Customizations
	/// </summary>
	public interface ISolutionDependent
	{
		/// <summary>
		/// Unique names of the required CRM Solutions.
		/// </summary>
		IEnumerable<string> RequiredSolutions { get; }
	}
}
