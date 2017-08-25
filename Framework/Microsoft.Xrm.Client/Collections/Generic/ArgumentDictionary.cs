/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace Microsoft.Xrm.Client.Collections.Generic
{
	/// <summary>
	/// A dictionary for processing command line arguments.
	/// </summary>
	public sealed class ArgumentDictionary : Dictionary<string, string>
	{
		private static readonly Regex _splitter = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private static readonly Regex _remover = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public ArgumentDictionary()
			: base(StringComparer.OrdinalIgnoreCase)
		{
		}

		public ArgumentDictionary(params string[] args)
			: base(args.Length, StringComparer.OrdinalIgnoreCase)
		{
			AddRange(args);
		}

		/// <summary>
		/// Merges a collection into the dictionary.
		/// </summary>
		/// <param name="items"></param>
		public void AddRange(NameValueCollection items)
		{
			foreach (string key in items.Keys)
			{
				this[key] = string.Join(",", items.GetValues(key));
			}
		}

		/// <summary>
		/// Merges a collection of pair strings into the dictionary.
		/// </summary>
		/// <remarks>
		/// Each argument should specify a colon separated name/value pair.
		/// </remarks>
		/// <param name="args"></param>
		public void AddRange(params string[] args)
		{
			string parameter = null;

			foreach (string arg in args)
			{
				string[] parts = _splitter.Split(arg, 3);

				switch (parts.Length)
				{
					case 1:
						// this is a value for the previous parameter
						if (parameter != null)
						{
							parts[0] = _remover.Replace(parts[0], "$1");
							this[parameter] = parts[0];
							parameter = null;
						}
						break;
					case 2:
						// found an argument without a value or with a pending value
						if (parameter != null)
						{
							this[parameter] = "true";
						}

						parameter = parts[1];
						break;
					case 3:
						// found an argument with key and value
						if (parameter != null)
						{
							this[parameter] = "true";
						}

						parameter = parts[1];
						parts[2] = _remover.Replace(parts[2], "$1");
						this[parameter] = parts[2];
						parameter = null;
						break;
				}
			}

			if (parameter != null)
			{
				this[parameter] = "true";
			}
		}

		/// <summary>
		/// Determines if an entry contains the string value "true".
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool IsEnabled(string key)
		{
			if (!ContainsKey(key)) return false;
			
			if (string.Compare(this[key], "true", StringComparison.InvariantCultureIgnoreCase) == 0) return true;

			if (string.Compare(this[key], "false", StringComparison.InvariantCultureIgnoreCase) == 0) return false;

			throw new FormatException(@"The value provided for '{0}' needs to be ""true"" or ""false"".".FormatWith(key));
		}
	}
}
