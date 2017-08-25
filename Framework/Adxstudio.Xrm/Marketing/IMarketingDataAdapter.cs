/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;

namespace Adxstudio.Xrm.Marketing
{
	public interface IMarketingDataAdapter
	{
		IEnumerable<IMarketingList> GetMarketingLists(string encodedEmail, string signature);
		IEnumerable<IMarketingList> Unsubscribe(string encodedEmail, string signature);
		IEnumerable<IMarketingList> Unsubscribe(string encodedEmail, string encodedList, string signature);
		IEnumerable<IMarketingList> Unsubscribe(string encodedEmail, IEnumerable<string> listIds, string signature);
		string ConstructSignature(string emailAddress, string listId = "");
	}
}
