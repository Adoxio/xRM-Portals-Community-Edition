/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;

namespace Adxstudio.Xrm.AspNet.PortalBus
{
	/// <summary>
	/// A portal bus message that can be validated and invoked.
	/// </summary>
	public interface IPortalBusMessage
	{
		Task InvokeAsync(IOwinContext context);
		void Initialize(IOwinContext context, IDataProtector protector);
		bool Validate(IOwinContext context, IDataProtector protector);
	}

	/// <summary>
	/// A portal bus message that can be validated and invoked.
	/// </summary>
	public abstract class PortalBusMessage : IPortalBusMessage
	{
		public string Id { get; set; }
		public string Token { get; set; }

		public abstract Task InvokeAsync(IOwinContext context);

		public virtual void Initialize(IOwinContext context, IDataProtector protector)
		{
			var id = Guid.NewGuid().ToString();
			var token = Convert.ToBase64String(protector.Protect(Encoding.UTF8.GetBytes(id)));

			Id = id;
			Token = token;
		}

		public virtual bool Validate(IOwinContext context, IDataProtector protector)
		{
			try
			{
				return string.Equals(Id, Encoding.UTF8.GetString(protector.Unprotect(Convert.FromBase64String(Token))));
			}
			catch (CryptographicException)
			{
				return false;
			}
		}
	}
}
