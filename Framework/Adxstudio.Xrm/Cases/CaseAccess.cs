/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Adxstudio.Xrm.Cases
{
	public class CaseAccess : ICaseAccess
	{
		public static readonly ICaseAccess None = new CaseAccess();

		public CaseAccess(bool read = false, bool write = false, bool delete = false, bool @public = false)
		{
			Read = read;
			Write = write;
			Delete = delete;
			Public = @public;
		}

		public bool Delete { get; private set; }

		public bool Public { get; private set; }

		public bool Read { get; private set; }

		public bool Write { get; private set; }

		public static ICaseAccess FromPermissions(ICaseAccessPermissions permissions, bool @public = false)
		{
			if (permissions == null) throw new ArgumentNullException("permissions");

			return new CaseAccess(permissions.Read, permissions.Write, permissions.Delete, @public);
		}

		public static ICaseAccess FromPermissions(IEnumerable<ICaseAccessPermissions> permissions, bool @public = false)
		{
			if (permissions == null) throw new ArgumentNullException("permissions");

			// Aggregate permissions by starting with a seed value of no access except for whatever the value of
			// @public is, and then iterate through all permissions, ORing each right together. So, for example,
			// if you have Read on one of the permissions, you'll have Read in the final aggregated case access.
			return permissions.Aggregate(new CaseAccess(@public: @public), (access, permission) =>
				new CaseAccess(
					access.Read || permission.Read,
					access.Write || permission.Write,
					access.Delete || permission.Delete,
					@public));
		}
	}
}
