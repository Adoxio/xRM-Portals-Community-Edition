/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Adxstudio.Xrm.Cms
{
	public interface ICommentDataAdapter
	{
		IDictionary<string, object> GetCommentAttributes(string content, string authorName = null, string authorEmail = null, string authorUrl = null, HttpContext context = null);

		IEnumerable<IComment> SelectComments();

		IEnumerable<IComment> SelectComments(int startRowIndex, int maximumRows = -1);

		int SelectCommentCount();

		string GetCommentLogicalName();

		string GetCommentContentAttributeName();

		ICommentPolicyReader GetCommentPolicyReader();
	}
}
