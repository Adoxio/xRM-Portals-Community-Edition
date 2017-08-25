/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Decorators
{
	using System.Web;

	/// <summary>
	/// Class encapsulating the decoration of the HttpContext by modifying the headers
	/// </summary>
	public sealed class HeaderDecorator : HttpContextDecorator
	{
		/// <summary>
		/// Header to be removed from the context's response
		/// </summary>
		private const string ServerHeader = "Server";

		/// <summary>
		/// Initializes a new instance of the <see cref="HeaderDecorator" /> class.
		/// </summary>
		/// <param name="context">HttpContext to decorate</param>
		private HeaderDecorator(HttpContextBase context)
			: base(context)
		{

		}

		/// <summary>
		/// Gets an instance of the HeadersDecorator
		/// </summary>
		/// <param name="context">HttpContext to decorate</param>
		/// <returns>an instance of the IHttpContextDecorator</returns>
		public static IHttpContextDecorator GetInstance(HttpContextBase context)
		{
			return new HeaderDecorator(context);
		}

		/// <summary>
		/// Gets an instance of the HeadersDecorator
		/// </summary>
		/// <returns>an instance of the IHttpContextDecorator</returns>
		public static IHttpContextDecorator GetInstance()
		{
			return new HeaderDecorator(new HttpContextWrapper(HttpContext.Current));
		}

		/// <summary>
		/// Decorates the Context by modifying the headers
		/// </summary>
		/// <returns>true if successful; false otherwise</returns>
		public override bool Decorate()
		{
			if (!this.IsContextAvailable)
			{
				return false;
			}

			this.Context.Response.Headers.Remove(HeaderDecorator.ServerHeader);

			return true;
		}
	}
}
