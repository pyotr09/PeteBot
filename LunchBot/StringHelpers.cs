using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;

namespace LunchBot
{	
	public static class StringHelpers
	{
		readonly static TextInfo _default = new CultureInfo("en-US", false).TextInfo;
		public static string ToTitleCase(this string s, TextInfo textinfo = null)
		{
			if (textinfo == null) textinfo = _default;
			return textinfo.ToTitleCase(s);
		}
	}
}