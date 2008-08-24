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

namespace phonet4n.Tests
{
    using System;

    using NUnit.Framework;
    using NUnit.Framework.SyntaxHelpers;

    using phonet4n.Core;

    [TestFixture]
    public class RuleLoaderTests
    {
        #region Methods

        [Test]
        public void TestDefaultRules()
        {
            Assert.IsNotNull(RuleLoader.DefaultRules);
            Assert.AreEqual(919 * 3, RuleLoader.DefaultRules.Length);
        }

        [Test]
        public void TestLoadInvalidRessource()
        {
            try
            {
                RuleLoader.LoadFromRessource("invalidName");
                Assert.Fail("ArgumentException expected");
            }
            catch (ArgumentException)
            {
            }
        }

        [Test]
        public void TestLoadRessources()
        {
            string[] res1 = RuleLoader.LoadFromRessource("phonet4n.Core.german_1.csv");
            Assert.AreEqual(RuleLoader.DefaultRules, res1);

            string[] csv1 = RuleLoader.LoadFromCSV("../../rules/german_1.csv");
            Assert.AreEqual(res1, csv1);

            string[] res2 = RuleLoader.LoadFromRessource("phonet4n.Core.german_2.csv");
            string[] csv2 = RuleLoader.LoadFromCSV("../../rules/german_2.csv");
            Assert.AreEqual(res2, csv2);

            Assert.AreNotEqual(res1, res2);
        }

        #endregion Methods
    }
}