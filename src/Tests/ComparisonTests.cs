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
