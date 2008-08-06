/*
 * Created by SharpDevelop.
 * User: hbehrens
 * Date: 06.08.2008
 * Time: 15:05
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
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
        	char[] output = new char[input.Length*2];
            int len = phonet(input.ToCharArray(), output, output.Length, 1);

            return new String(output).Substring(0, len);
        }
        
	}
}
