#region Header

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

#endregion Header

namespace phonet4n.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    public class RuleLoader
    {
        #region Fields

        public static readonly string[] DefaultRules = LoadFromRessource("phonet4n.Core.german_1.csv");

        #endregion Fields

        #region Methods

        public static String[] LoadFromCSV(String filename)
        {
            return LoadFromLines(File.ReadAllLines(filename, Encoding.UTF8));
        }

        public static String[] LoadFromLines(string[] lines)
        {
            List<String> result = new List<String>();
            foreach (String line in lines)
            {
                if (line.StartsWith("#"))
                    continue;

                Match m = Regex.Match(line, "\"([^\"]*)\",\"([^\"]*)\"");
                Debug.Assert(m.Success);

                result.Add(Escape(m.Groups[1].Value));
                result.Add(Escape(m.Groups[2].Value));
                result.Add(null);
            }
            return result.ToArray();
        }

        public static String[] LoadFromRessource(String name)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {

                if (stream == null)
                    throw new ArgumentException(name + " is no valid ressource name");

                using (TextReader text = new StreamReader(stream, Encoding.UTF8))
                {
                    List<string> lines = new List<string>();
                    string line;
                    while ((line = text.ReadLine()) != null)
                        lines.Add(line);
                    return LoadFromLines(lines.ToArray());
                }
            }
        }

        private static String Escape(String s)
        {
            if (s == "<null>")
                return null;

            return s;
        }

        #endregion Methods
    }
}