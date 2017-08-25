/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Cms
{
	public interface ISolutionDefinitionProvider
	{
		IDictionary<string, object> GetQueryParameters();
		SolutionDefinition GetSolution();
	}
}
