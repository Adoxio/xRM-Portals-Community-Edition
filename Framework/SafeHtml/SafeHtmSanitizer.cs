/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace SafeHtml
{
	public sealed class SafeHtmSanitizer
	{
		/// <summary>
		/// Private constructor to prevent instantiation.
		/// </summary>
		private SafeHtmSanitizer()
		{
		}

		[FlagsAttribute]
		public enum SafeHtmlFlags
		{
			None = 0x0,
			HighSecurity = 0x1,
			PlainTextSecurity = 0x2,
			NoSecurity = 0x4,
			NoHtmlEscaping = 0x8,
			AllowIframe = 0x10,
			NoAllowPosition = 0x20,
			Fragment = 0x40,
			MsAllowcapture = 0x80,
			MarkChangedTags = 0x100,
			NoWriteBOM = 0x200,
			WriteDoctype = 0x400,
			TerminateWithZero = 0x800,
			IndicateIfUnsafe = 0x1000,
			DebugNoPopup = 0x2000,
			DebugNoAssert = 0x4000,
			DebugNoReport = 0x8000,
			DebugOutputHigh = 0x10000,
			DebugOutputMedium = 0x20000,
			DebugOutputLow = 0x40000
		}

		private enum SafeHtmlCodePages
		{
			CodePageJapan = 932,
			CodePageChina = 936,
			CodePageKorea = 949,
			CodePageChineseTraditional = 950,
			CodePageUnicodeLittle = 1200,
			CodePageUnicodeBig = 1201,
			CodePageEastEurope = 1250,
			CodePageRussian = 1251,
			CodePageWestEurope = 1252,
			CodePageGreek = 1253,
			CodePageTurkish = 1254,
			CodePageHebrew = 1255,
			CodePageArabic = 1256,
			CodePageBaltic = 1257,
			CodePageVietnamese = 1258,
			CodePageASCII = 20127,
			CodePageRussianKOI8R = 20866,
			CodePageISOLatin1 = 28591,
			CodePageISOEastEurope = 28592,
			CodePageISOTurkish = 28593,
			CodePageISOBaltic = 28594,
			CodePageISORussian = 28595,
			CodePageISOArabic = 28596,
			CodePageISOGreek = 28597,
			CodePageISOHebrew = 28598,
			CodePageISOTurkish2 = 28599,
			CodePageISOLatin9 = 28605,
			CodePageHebrewLog = 38598,
			CodePageUser = 50000,
			CodePageAutoALL = 50001,
			CodePageJapanNHK = 50220,
			CodePageJapanESC = 50221,
			CodePageJapanSIO = 50222,
			CodePageKoreaISO = 50225,
			CodePageChineseTraditionalISO = 50227,
			CodePageChinaISO = 50229,
			CodePageAutoJapan = 50932,
			CodePageAutoChina = 50936,
			CodePageAutoKorea = 50949,
			CodePageAutoChineseTraditional = 50950,
			CodePageAutoRussian = 51251,
			CodePageAutoGreek = 51253,
			CodePageAutoArabic = 51256,
			CodePageJapanEUC = 51932,
			CodePageChinaEUC = 51936,
			CodePageKoreaEUC = 51949,
			CodePageChineseTraditionalEUC = 51950,
			CodePageChinaHZ = 52936,
			CodePageUTF7 = 65000,
			CodePageUTF8 = 65001,
			CodePageUnicode = CodePageUnicodeLittle,
			CodePageACP = 0,
			// CodePageGetDefault = Encoding.Default.WindowsCodePage,
			CodePageUnknown = -1,
		}

		/// <summary>
		/// Get a safe version of a given string representing an HTML fragment.
		/// </summary>
		/// <param name="currentHtml">HTML string to make a safe version of</param>
		/// <returns>The safe HTML string.</returns>
		public static string GetSafeHtml(string currentHtml)
		{
			bool wasBad;
			return GetSafeHtml(currentHtml, SafeHtmSanitizer.SafeHtmlFlags.Fragment, out wasBad);
		}

		/// <summary>
		/// Get a safe version of a given string representing HTML.  This is a wrapper for the unsafe method BuildSafeHtml.
		/// </summary>
		/// <param name="currentHtml">HTML string to make a safe version of</param>
		/// <param name="flags">Flags as to how to process the string</param>
		/// <param name="wasBad">returns true if the given HTML string had potentially dangerous content, else false</param>
		/// <returns>The safe HTML string</returns>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "", MessageId = "2#")]
		public static string GetSafeHtml(string currentHtml, SafeHtmlFlags flags, out bool wasBad)
		{
			string newHtml = string.Empty;
			wasBad = BuildSafeHtml(currentHtml, flags, out newHtml);
			return newHtml;
		}

		/// <summary>
		/// Get a safe version of a given string representing HTML.  Note that this function is marked unsafe because
		/// it calls an unsafe extern function.
		/// </summary>
		/// <param name="currentHtml">HTML string to make a safe version of</param>
		/// <param name="flags">Flags as to how to process the string</param>
		/// <param name="newHtml">The safe HTML string</param>
		/// <returns>true if the given HTML string had potentially dangerous content, else false</returns>
		private static unsafe bool BuildSafeHtml(string existingHtml, SafeHtmlFlags flags, out string newHtml)
		{
			byte* rgbTmp = null;

			// Set newHtml to a blank string in case we encounter a failure
			newHtml = string.Empty;

			// Early exit if the existing Html is null or an empty string.
			// The native call below doesn't return when the existing Html is an empty string.
			if (existingHtml == null || existingHtml.Length == 0)
			{
				return false;
			}

			try
			{
				byte[] rgbSrc = Encoding.UTF8.GetBytes(existingHtml);

				int iSrc = rgbSrc.Length;
				int cbDst = 0;

				// Note that we do not have the SafeHtml component write out the "byte order mark" to indicate 
				// Unicode/UTF-8 - that is handled separately by callers.
				uint returnCode = NativeMethods.OshFGetSafeHTMLAllocForManaged2(
					rgbSrc,
					iSrc,
					(int)SafeHtmlCodePages.CodePageUTF8,
					&rgbTmp,
					out cbDst,
					(int)SafeHtmlCodePages.CodePageUnicode,
					(int)(flags | SafeHtmlFlags.DebugNoPopup | SafeHtmlFlags.IndicateIfUnsafe | SafeHtmlFlags.NoWriteBOM));

				StringBuilder Result = new StringBuilder(cbDst / 2);

				for (int i = 0; i < cbDst; i += 2)
				{
					char ch = *(char*)(rgbTmp + i);
					Result.Append(ch);
				}

				newHtml = Result.ToString();

				return returnCode == 1;
			}
			finally
			{
				if (rgbTmp != null)
					NativeMethods.OshFreePv((void*)rgbTmp);
			}
		}

		private sealed class NativeMethods
		{
			// Empty constructor
			private NativeMethods() { }

			// Interface function for generating safe html from managed code.  Note that this function is marked "unsafe"
			// because of the use of pointers.
			[DllImport("osafehtm.dll", SetLastError = false)]
			internal static extern unsafe uint OshFGetSafeHTMLAllocForManaged2(
				byte[] rgbSrc,      // Source byte array
				int cbSrc,          // Size of source byte array
				int cpSrc,          // Codepage of source byte array
				byte** rgbDst,      // Pointer to the destination byte array
				out int cbDst,      // Size of the destination byte array
				int cpDst,          // Codepage to convert to the destination byte array
				int grfosh);        // Safe HTML behavior flags


			// Interface function for freeing data allocated by osafehtm.dll from managed code.  Note that this function is marked "unsafe"
			// because of the use of pointers.
			[DllImport("osafehtm.dll", SetLastError = false)]
			internal static extern unsafe void OshFreePv(void* pv);
		}
	}

}
