/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Text.RegularExpressions;

namespace Adxstudio.Xrm.Text
{
	public class Mask
	{
		public static RegexOptions DefaultRegexOptions = RegexOptions.IgnoreCase;

		private Regex _innerRegex;
		private RegexOptions _options;
		private string _pattern;

		protected virtual Regex InnerRegex
		{
			get
			{
				if (_innerRegex == null)
				{
					_innerRegex = ToRegex();
				}

				return _innerRegex;
			}
		}

		public Mask(string pattern) : this(pattern, DefaultRegexOptions) { }

		public Mask(string pattern, RegexOptions options)
		{
			_pattern = pattern;
			_options = options;
		}

		public virtual bool IsMatch(string input)
		{
			return InnerRegex.IsMatch(input);
		}

		public virtual Match Match(string input)
		{
			return InnerRegex.Match(input);
		}

		public virtual Regex ToRegex()
		{
			return ToRegex(_options);
		}

		public virtual Regex ToRegex(RegexOptions options)
		{
			string pattern = Regex.Escape(_pattern);

			pattern = ReplaceWildcardsWithRegexEquivalent(pattern);
			pattern = ReplaceAttributesWithNamedCaptures(pattern);
			pattern = "^" + pattern + "$";

			return new Regex(pattern, options);
		}

		public override string ToString()
		{
			return _pattern;
		}

		protected virtual string ReplaceAttributesWithNamedCaptures(string escapedMask)
		{
			return Regex.Replace(escapedMask, @"\\\[([^\[\]]+)]", @"(?<$1>[^\/]+)");
		}

		protected virtual string ReplaceWildcardsWithRegexEquivalent(string escapedMask)
		{
			return escapedMask.Replace(@"\*", ".*");
		}

		public static bool IsMatch(string input, string pattern)
		{
			return IsMatch(input, pattern, DefaultRegexOptions);
		}

		public static bool IsMatch(string input, string pattern, RegexOptions options)
		{
			return new Mask(pattern, options).IsMatch(input);
		}

		public static Match Match(string input, string pattern)
		{
			return Match(input, pattern, DefaultRegexOptions);
		}

		public static Match Match(string input, string pattern, RegexOptions options)
		{
			return new Mask(pattern, options).Match(input);
		}
	}
}
