/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.EventHubBasedInvalidation
{
	public enum MessageType
	{
		Unknown,
		Other,
		Associate,
		Disassociate,
		Create,
		Update,
		Delete,
		MetadataChange,
	}
}
