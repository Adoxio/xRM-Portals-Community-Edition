/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.SharePoint
{
	public interface ISharePointDataAdapter
	{
		ISharePointResult AddFiles(EntityReference regarding, IList<HttpPostedFileBase> files, bool overwrite = true, string folderPath = null);
		
		ISharePointResult DeleteItem(EntityReference regarding, int id);
		
		ISharePointCollection GetFoldersAndFiles(EntityReference regarding, string sortExpression, int page, int pageSize, string pagingInfo, string folderPath = null);
	}
}
