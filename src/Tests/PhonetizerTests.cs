/*
 *  This file is part of phonet4n.
 * 
 *  Copyright 2008 Heiko Behrens (HeikoBehrens a t gmx de)
 *
 *  Contributions by
 *    Sebastian Zarnekow
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
            p.Rules = RuleLoader.LoadFromRessource("phonet4n.Core.german_1.csv");
            Assert.AreEqual(919 * 3, p.Rules.Length);

            Assert.AreEqual("MEIA", p.Phonetize("Meier"));
            Assert.AreEqual("MEIA", p.Phonetize("Mayer"));
            Assert.AreEqual("FEIFA", p.Phonetize("Pfeiffer"));
            Assert.AreEqual("FEIFA", p.Phonetize("Pfeifer"));
        }

        [Test]
        public void TestSimpleGerman2()
        {
            Phonetizer p = new Phonetizer();
            p.Rules = RuleLoader.LoadFromRessource("phonet4n.Core.german_2.csv");
            Assert.AreEqual(919 * 3, p.Rules.Length);

            Assert.AreEqual("NEIA", p.Phonetize("Meier"));
            Assert.AreEqual("NEIA", p.Phonetize("Mayer"));
            Assert.AreEqual("FEIFA", p.Phonetize("Pfeiffer"));
            Assert.AreEqual("FEIFA", p.Phonetize("Pfeifer"));
        }

        [Test]
        public void TestRecurringDigits_Keep()
        {
            Phonetizer p = new Phonetizer(true);
            p.Rules = RuleLoader.LoadFromRessource("phonet4n.Core.german_1.csv");
            Assert.AreEqual(919 * 3, p.Rules.Length);

            Assert.AreEqual("TEST01", p.Phonetize("Teest01"));
            Assert.AreEqual("TEST001", p.Phonetize("Teest001"));
            Assert.AreEqual("00112233445566778899", p.Phonetize("00112233445566778899"));
            Assert.AreEqual("0000", p.Phonetize("0000"));
        }

        [Test]
        public void TestRecurringDigits_Strip()
        {
            Phonetizer p = new Phonetizer(false);
            p.Rules = RuleLoader.LoadFromRessource("phonet4n.Core.german_1.csv");
            Assert.AreEqual(919 * 3, p.Rules.Length);

            Assert.AreEqual("TEST01", p.Phonetize("Teest01"));
            Assert.AreEqual("TEST01", p.Phonetize("Teest001"));
            Assert.AreEqual("0123456789", p.Phonetize("00112233445566778899"));
            Assert.AreEqual("0", p.Phonetize("0000"));
        }
    }

}
