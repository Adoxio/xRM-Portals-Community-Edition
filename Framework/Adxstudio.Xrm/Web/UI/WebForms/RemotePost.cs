/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace Adxstudio.Xrm.Web.UI.WebForms
{
	/// <summary>
	/// Post a collection of name value pairs to a remote url.
	/// </summary>
	public class RemotePost
	{
		private readonly NameValueCollection _parameters = new NameValueCollection();
		private readonly string _url;

		/// <summary>
		/// Post a collection of name value pairs to a remote url.
		/// </summary>
		public RemotePost(string url)
		{
			_url = url;
		}

		/// <summary>
		/// Add name and value pair to collection that gets posted to remote url.
		/// </summary>
		/// <param name="name">key name</param>
		/// <param name="value">value</param>
		public void AddParameter(string name, string value)
		{
			_parameters.Add(name, value);
		}

		/// <summary>
		/// Posts the collection of name value pairs to remote url.
		/// </summary>
		public string Post()
		{
			return PostAndGetResponseString(_url, _parameters);
		}

		private static string PostAndGetResponseString(string url, NameValueCollection parameters)
		{
			if (string.IsNullOrWhiteSpace(url) || (parameters == null || !parameters.HasKeys()))
			{
				return string.Empty;
			}

			var httpRequest = (HttpWebRequest)WebRequest.Create(url);
	
			httpRequest.Method = "POST"; 

			httpRequest.ContentType = "application/x-www-form-urlencoded";

			var postString = ConstructStringFromParameters(parameters);

			var bytedata = Encoding.UTF8.GetBytes(postString);

			httpRequest.ContentLength = bytedata.Length;

			var requestStream = httpRequest.GetRequestStream();

			requestStream.Write(bytedata, 0, bytedata.Length);

			requestStream.Close();
			
			var httpWebResponse = (HttpWebResponse)httpRequest.GetResponse();
			var responseStream =  httpWebResponse.GetResponseStream();

			var sb = new StringBuilder();

			if (responseStream != null)
			{
				using (var reader = new StreamReader(responseStream, Encoding.UTF8))
				{
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						sb.Append(line);
					}
				}
			}

			return sb.ToString();
		}

		private static string ConstructStringFromParameters(NameValueCollection parameters)
		{
			var sb = new StringBuilder();

			foreach (string name in parameters)
			{
				sb.Append(string.Concat(name, "=", System.Web.HttpUtility.UrlEncode(parameters[name]), "&"));
			}
			
			return sb.Length > 0 ? sb.ToString(0, sb.Length - 1) : string.Empty;
		}
	}
}
