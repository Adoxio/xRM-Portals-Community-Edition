/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Decorators
{
	using System;
	using System.Web;

	/// <summary>
	/// Class encapsulating the decoration of the HttpContext by modifying the headers
	/// </summary>
	public sealed class ItemDecorator : HttpContextDecorator, ICollectionInspector
	{
		/// <summary>
		/// Item to be added into the HttpContext's Item collection
		/// </summary>
		public const string RequestStartTime = "RequestStartTime";

		/// <summary>
		/// Initializes a new instance of the <see cref="ItemDecorator" /> class.
		/// </summary>
		/// <param name="context">HttpContext to decorate</param>
		private ItemDecorator(HttpContextBase context)
			: base(context)
		{

		}

		/// <summary>
		/// Gets an instance of the ItemDecorator
		/// </summary>
		/// <param name="context">HttpContext to decorate</param>
		/// <returns>an instance of the IHttpContextDecorator</returns>
		public static IHttpContextDecorator GetInstance(HttpContextBase context)
		{
			return new ItemDecorator(context);
		}

		/// <summary>
		/// Gets an instance of the ItemDecorator
		/// </summary>
		/// <returns>an instance of the IHttpContextDecorator</returns>
		public static IHttpContextDecorator GetInstance()
		{
			return new ItemDecorator(new HttpContextWrapper(HttpContext.Current));
		}

		/// <summary>
		/// Gets an instance of the ICollectionInspector
		/// </summary>
		/// <param name="context">HttpContext to decorate</param>
		/// <returns>an instance of the ICollectionInspector</returns>
		public static ICollectionInspector GetInspectorInstance(HttpContextBase context)
		{
			return new ItemDecorator(context);
		}

		/// <summary>
		/// Gets an instance of the ICollectionInspector
		/// </summary>
		/// <returns>an instance of the ICollectionInspector</returns>
		public static ICollectionInspector GetInspectorInstance()
		{
			return new ItemDecorator(new HttpContextWrapper(HttpContext.Current));
		}

		/// <summary>
		/// Decorates the Context by modifying the HttpContext's ItemCollection
		/// </summary>
		/// <returns>true if successful; false otherwise</returns>
		public override bool Decorate()
		{
			if (!this.IsContextAvailable)
			{
				return false;
			}

			this.Context.Items[ItemDecorator.RequestStartTime] = DateTime.UtcNow;

			return true;
		}
		
		/// <summary>
		/// Get the value for the given key from the HttpContext.ItemCollection
		/// </summary>
		/// <param name="key">type: string</param>
		/// <returns>type: object if it exists; otherwise null</returns>
		public object this[string key]
		{
			get
			{
				return this.IsContextAvailable && this.Context.Items.Contains(key)
					? this.Context.Items[ItemDecorator.RequestStartTime]
					: null;
			}
		}
	}
}
