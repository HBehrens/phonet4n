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
using System.Runtime.InteropServices;

namespace phonet4n.Tests
{
    public class CImplAdapter
    {

        [DllImport("phonet.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int phonet(char[] src, [In, Out] char[] dest, int len, int mode);

        public string Phonetize(string input)
        {
            // output must be long enough and will be shorter than input, so twice as long should be enough
            char[] output = new char[input.Length * 2];
            int len = phonet(input.ToCharArray(), output, output.Length, 1);

            return new String(output).Substring(0, len);
        }

    }
}
