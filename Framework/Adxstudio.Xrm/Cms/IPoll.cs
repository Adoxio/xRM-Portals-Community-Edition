/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
    public interface IPoll
	{
		DateTime? CloseVotingDate { get; }

		Entity Entity { get; }

		Guid Id { get; }

        string Name { get; }
		
        IEnumerable<IPollOption> Options { get; set; }

		string Question { get; }

        string SubmitButtonLabel { get; }

		IPollOption UserSelectedOption { get; set; }

		int Votes { get; }

	    EntityReference WebTemplate { get; }
	}
}
