/*
 * Created by SharpDevelop.
 * User: hbehrens
 * Date: 06.08.2008
 * Time: 14:49
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using phonet4n.Core;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace phonet4n.Tests
{
	[TestFixture]
	public class PhonetizerTests
	{
		[Test]
		public void TestSimpleGerman1()
		{
			Phonetizer p = new Phonetizer();
			p.Rules = RuleLoader.LoadFromCSV("../../rules/german_1.csv");
			Assert.AreEqual(919*3, p.Rules.Length);
			
			Assert.AreEqual("MEIA", p.Phonetize("Meier"));
			Assert.AreEqual("MEIA", p.Phonetize("Mayer"));
			Assert.AreEqual("FEIFA", p.Phonetize("Pfeiffer"));
			Assert.AreEqual("FEIFA", p.Phonetize("Pfeifer"));
		}
	
		[Test]
		public void TestSimpleGerman2()
		{
			Phonetizer p = new Phonetizer();
			p.Rules = RuleLoader.LoadFromCSV("../../rules/german_2.csv");
			Assert.AreEqual(919*3, p.Rules.Length);
			
			Assert.AreEqual("NEIA", p.Phonetize("Meier"));
			Assert.AreEqual("NEIA", p.Phonetize("Mayer"));
			Assert.AreEqual("FEIFA", p.Phonetize("Pfeiffer"));
			Assert.AreEqual("FEIFA", p.Phonetize("Pfeifer"));
		}
	}
	
}
