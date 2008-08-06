/*
 *  This file is part of phonet4n.
 * 
 *  Copyright 2008 Heiko Behrens (HeikoBehrens a t gmx de)
 *
 *  phonet4n is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as
 *  published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  phonet4n is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with phonet4n.
 *  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;

namespace phonet4n.Core
{
	public class RuleLoader
	{
	    private static String Escape(String s) {
	        if(s == null)
	            return null;
	        
	        if(s == "<null>")
	            return null;

	        return s;
	    }
		
		public static String[] LoadFromCSV(String filename){
			List<String> result = new List<String>();
			foreach(String line in File.ReadAllLines(filename, Encoding.UTF8))
			{
				if(line.StartsWith("#"))
				   continue;
				
				Match m = Regex.Match(line, "\"([^\"]*)\",\"([^\"]*)\"");
				Debug.Assert(m.Success);

				result.Add(Escape(m.Groups[1].Value));
				result.Add(Escape(m.Groups[2].Value));
				result.Add(null);
			}
			return result.ToArray();
	    }		
		
		public static String[] LoadFromLanguage(String lang) {
			return LoadFromCSV("D:/Devel/csharp/phonet4n_bak/phonet4n/rules/" + lang + ".csv");
		}
	}
}