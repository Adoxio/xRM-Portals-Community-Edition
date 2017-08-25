/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Decorators
{
	using System.Web;

	/// <summary>
	/// Abstract base class for all HttpContext decoration
	/// </summary>
	public abstract class HttpContextDecorator : IHttpContextDecorator
	{
		/// <summary>
		/// HttpContext to decorate
		/// </summary>
		protected HttpContextBase Context { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpContextDecorator" /> class.
		/// </summary>
		/// <param name="context">type: HttpContext</param>
		protected HttpContextDecorator(HttpContextBase context)
		{
			this.Context = context;
		}

		/// <summary>
		/// Decorates the HttpContext
		/// </summary>
		/// <returns>true if the context is not null; otherwise false</returns>
		public abstract bool Decorate();

		/// <summary>
		/// Returns true if the context is not null; otherwise false
		/// </summary>
		public bool IsContextAvailable
		{
			get
			{
				return this.Context != null;
			}
		}
	}
}
