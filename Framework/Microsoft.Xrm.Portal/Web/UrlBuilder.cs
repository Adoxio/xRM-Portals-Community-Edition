/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;

namespace Microsoft.Xrm.Portal.Web
{
	/// <summary>
	/// Builds out a url.
	/// </summary>
	public sealed class UrlBuilder : UriBuilder
	{
		private const string _urlBuilderReadOnly = "UrlBuilder is set to read-only.";
		private bool _isReadOnly;

		private bool IsReadOnly
		{
			get
			{
				return _isReadOnly;
			}
		}

		private QueryStringCollection _queryString;

		public QueryStringCollection QueryString
		{
			get
			{
				if (_queryString == null)
				{
					_queryString = new QueryStringCollection(string.Empty);
				}

				return _queryString;
			}

			set
			{
				if (IsReadOnly)
				{
					throw new NotSupportedException(_urlBuilderReadOnly);
				}

				_queryString = value;
			}
		}

		public new string Query
		{
			get
			{
				return QueryString.ToString();
			}

			set
			{
				if (IsReadOnly)
				{
					throw new NotSupportedException(_urlBuilderReadOnly);
				}

				_queryString = value;
			}
		}

		public string PathWithQueryString
		{
			get
			{
				return Path + Query;
			}
		}

		public new Uri Uri
		{
			get
			{
				// trim the '?' because the base setter adds it
				base.Query = Query.TrimStart('?');
				return base.Uri;
			}
		}

		#region UriBuilder Methods

		public new string Fragment
		{
			get
			{
				return base.Fragment;
			}

			set
			{
				if (IsReadOnly)
				{
					throw new NotSupportedException(_urlBuilderReadOnly);
				}

				base.Fragment = value;
			}
		}

		public new string Host
		{
			get
			{
				return base.Host;
			}

			set
			{
				if (IsReadOnly)
				{
					throw new NotSupportedException(_urlBuilderReadOnly);
				}

				base.Host = value;
			}
		}

		public new string Password
		{
			get
			{
				return base.Password;
			}

			set
			{
				if (IsReadOnly)
				{
					throw new NotSupportedException(_urlBuilderReadOnly);
				}

				base.Password = value;
			}
		}

		public new string Path
		{
			get
			{
				return base.Path;
			}

			set
			{
				if (IsReadOnly)
				{
					throw new NotSupportedException(_urlBuilderReadOnly);
				}

				base.Path = value;
			}
		}

		public new int Port
		{
			get
			{
				return base.Port;
			}

			set
			{
				if (IsReadOnly)
				{
					throw new NotSupportedException(_urlBuilderReadOnly);
				}

				base.Port = value;
			}
		}

		public new string Scheme
		{
			get
			{
				return base.Scheme;
			}

			set
			{
				if (IsReadOnly)
				{
					throw new NotSupportedException(_urlBuilderReadOnly);
				}

				base.Scheme = value;
			}
		}

		public new string UserName
		{
			get
			{
				return base.UserName;
			}

			set
			{
				if (IsReadOnly)
				{
					throw new NotSupportedException(_urlBuilderReadOnly);
				}

				base.UserName = value;
			}
		}

		#endregion

		#region Constructors

		public UrlBuilder()
		{
		}

		public UrlBuilder(string uri)
			: base(ValidateUri(uri))
		{
			// initialize the QueryString
			Query = uri;

			// special handling for tilde based paths
			if (uri.StartsWith("~/"))
			{
				// tilde paths will have an extra leading '/'
				Path = Path.TrimStart('/');
			}
		}

		public UrlBuilder(UrlBuilder url)
			: base(url.Uri)
		{
		}

		public UrlBuilder(Uri uri)
			: base(uri)
		{
			// initialize the QueryString
			Query = uri.Query;
		}

		public UrlBuilder(string schemeName, string hostName)
			: base(schemeName, hostName)
		{
		}

		public UrlBuilder(string scheme, string host, int portNumber)
			: base(scheme, host, portNumber)
		{
		}

		public UrlBuilder(string scheme, string host, int port, string pathValue)
			: base(scheme, host, port, pathValue)
		{
		}

		public UrlBuilder(string scheme, string host, int port, string path, string extraValue)
			: base(scheme, host, port, path, extraValue)
		{
			Query = extraValue;
		}

		public UrlBuilder(System.Web.UI.Page page)
			: base(page.Request.Url.AbsoluteUri)
		{
			Query = page.Request.Url.Query;
		}

		public UrlBuilder(HttpContext context)
			: base(context.Request.Url.AbsoluteUri)
		{
			Query = context.Request.Url.Query;
		}

		public UrlBuilder(HttpRequest request)
			: base(request.Url.AbsoluteUri)
		{
			Query = request.Url.Query;
		}

		#endregion

		public static implicit operator UrlBuilder(string url)
		{
			return new UrlBuilder(url);
		}

		public static implicit operator string(UrlBuilder url)
		{
			if (url == null)
			{
				return null;
			}

			return url.ToString();
		}

		/// <summary>
		/// The name of the aspx page.
		/// </summary>
		public string PageName
		{
			get
			{
				string path = base.Path;
				return path.Substring(path.LastIndexOf("/") + 1);
			}

			set
			{
				if (IsReadOnly)
				{
					throw new NotSupportedException("UrlBuilder is set to read-only.");
				}

				string path = base.Path;
				path = path.Substring(0, path.LastIndexOf("/"));
				base.Path = string.Concat(path, "/", value);
			}
		}

		public void Redirect()
		{
			_Redirect(true);
		}

		public void Redirect(bool endResponse)
		{
			_Redirect(endResponse);
		}

		private void _Redirect(bool endResponse)
		{
			string uri = this.ToString();
			Tracing.FrameworkInformation("UrlBuilder", "Redirect", "Redirecting to: " + uri);
			HttpContext.Current.Response.Redirect(uri, endResponse);
		}

		public UrlBuilder Clone()
		{
			return new UrlBuilder(this.ToString());
		}

		public void EnableReadOnly()
		{
			this.QueryString.EnableReadOnly();
			_isReadOnly = true;
		}

		public new string ToString()
		{
			base.Query = Query.TrimStart('?'); // trim the '?' because the base setter adds it
			
			if (!Path.StartsWith("~/"))
			{
				return Uri.AbsoluteUri;
			}

			// we allow the Path to store a ~ based path which should be removed when calling ToString()
			UrlBuilder temp = new UrlBuilder(this);
			temp.Path = temp.Path.Remove(0, 2);
			return temp.Uri.AbsoluteUri;
		}

		/// <summary>
		/// Prepares a raw string URL to be valid for the base UriBuilder contstructor.
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		private static string ValidateUri(string uri)
		{
			// the Uri object requires a hostname so prepend a hostname if it is missing
			if (uri.StartsWith("/") || uri.StartsWith("~/"))
			{
				// tilde based paths are allowed but we need to first add a leading '/'
				if (uri.StartsWith("~/"))
				{
					uri = "/" + uri;
				}

				// a host name is required by the UriBuilder base class
				if (HttpContext.Current != null)
				{
					var httpHost = HttpContext.Current.Request.ServerVariables["HTTP_HOST"];
					if (httpHost == null)
					{
						// fail back on SERVER_NAME in case HTTP_HOST was not provided
						// ezGDS fix - possibly related to their F5 configuration, but a safe assumption
						// because if this is null, the next line will throw an error anyways
						httpHost = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
					}

					var url = HttpContext.Current.Request.Url;
					var hostAndPort = httpHost.Contains(":") ? httpHost : httpHost + ":" + url.Port;

					// include the current protocol (scheme) value and port
					uri = "{0}://{1}{2}".FormatWith(
						url.Scheme,
						hostAndPort,
						uri);
				}
				else
				{
					uri = "localhost" + uri;
				}
			}

			return uri;
		}
	}
}
