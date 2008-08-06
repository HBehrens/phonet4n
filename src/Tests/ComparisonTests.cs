/*
 * Created by SharpDevelop.
 * User: hbehrens
 * Date: 06.08.2008
 * Time: 15:08
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.IO;
using System.Text;
using phonet4n.Core;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace phonet4n.Tests
{
	[TestFixture]
	public class CImplComparisonTests
	{
		CImplAdapter adapter;
		Phonetizer phonetizer;
		
		[SetUp]
		public void Setup()
		{
			adapter = new CImplAdapter();
			phonetizer = new Phonetizer();
			phonetizer.Rules = RuleLoader.LoadFromCSV("../../rules/german_1.csv");
		}
		
		[Test]
		public void TestAdapter()
		{
			Assert.AreEqual("MEIA", adapter.Phonetize("Meier"));
			Assert.AreEqual("MEIA", adapter.Phonetize("Mayer"));
			Assert.AreEqual("FEIFA", adapter.Phonetize("Pfeiffer"));
			Assert.AreEqual("FEIFA", adapter.Phonetize("Pfeifer"));
		}
		
		[Test]
		public void CompareNachname()
		{
			TestFileAgainstCImpl("../../src/tests/data/nachnamen.txt");
		}

		[Test]
		public void CompareGermanWords()
		{
			TestFileAgainstCImpl("../../src/tests/data/ngerman.txt");
		}
		
        private void TestFileAgainstCImpl(String filename)
        {
            using (StreamReader reader = new StreamReader(filename, Encoding.ASCII))
            {
                String line;
                while ((line = reader.ReadLine()) != null)
                    TestWordAgainsAgainstCImpl(line);
            }
        }		
        
		private void TestWordAgainsAgainstCImpl(String line)
        {
            String resultCImpl = adapter.Phonetize(line);
            String resultDotNet = phonetizer.Phonetize(line);
            Assert.AreEqual(resultCImpl, resultDotNet, "Testing: \"" + line + "\"");
        }
	
	}
}
