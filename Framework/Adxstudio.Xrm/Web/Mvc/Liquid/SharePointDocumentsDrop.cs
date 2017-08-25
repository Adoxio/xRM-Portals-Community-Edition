/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class SharePointDocumentsDrop : PortalDrop
	{
		private readonly string _folderName;
		private readonly bool _folderOverride;

		public SharePointDocumentsDrop(IPortalLiquidContext portalLiquidContext, bool folderOverride = false, string folderName = null) : base(portalLiquidContext)
		{
			_folderName = folderName;
			_folderOverride = folderOverride;
		}

		public override object BeforeMethod(string method)
		{
			if (method == null)
			{
				return null;
			}

			if (_folderOverride && string.IsNullOrWhiteSpace(_folderName))
			{
				return new SharePointDocumentsDrop(this, true, method);
			}
			
			return new SharePointDocumentListDrop(this, method, _folderName);
		}

		public SharePointDocumentsDrop Folder
		{
			get { return new SharePointDocumentsDrop(this, true); }
		}
	}
}
