/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Cms
{
	public interface IPollDataAdapter
    {
        IPoll SelectPoll(Guid pollId);

        IPoll SelectPoll(string pollName);

        IPollPlacement SelectPollPlacement(Guid pollPlacementId);

        IPollPlacement SelectPollPlacement(string pollPlacementName);

        IPoll SelectRandomPoll(Guid pollPlacementId);

        IPoll SelectRandomPoll(string pollPlacementName);

		void SubmitPoll(IPoll poll, IPollOption pollOption);

		bool HasUserVoted(IPoll poll);
    }
}
