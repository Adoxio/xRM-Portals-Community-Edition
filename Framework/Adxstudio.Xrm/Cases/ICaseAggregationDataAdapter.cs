/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;

namespace Adxstudio.Xrm.Cases
{
	public enum CaseState
	{
		Active = 0,
		Resolved = 1
	}

	/// <summary>
	/// Provides data operations on a given set of cases.
	/// </summary>
	public interface ICaseAggregationDataAdapter
	{
		IEnumerable<ICase> SelectCases();

		IEnumerable<ICase> SelectCases(CaseState state);

		IEnumerable<ICase> SelectCases(Guid account);

		IEnumerable<ICase> SelectCases(Guid account, CaseState state);
	}
}
