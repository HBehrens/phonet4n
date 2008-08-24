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

    public abstract class AbstractSpeedTest
    {
        #region Fields

        string[] data;

        #endregion Fields

        #region Methods

        [Test]
        public void BatchTestMethod()
        {
            for (int i = 0; i < 10; i++)
                TestMethod();
        }

        [SetUp]
        public virtual void SetUp()
        {
            data = System.IO.File.ReadAllLines("../../src/tests/data/nachnamen.txt", System.Text.Encoding.ASCII);
        }

        [Test]
        public void TestMethod()
        {
            DateTime started = DateTime.Now;
            for (int i = 0; i < 10; i++)
                foreach (string line in data)
                    DoPhonetize(line);
            TimeSpan duration = DateTime.Now - started;

            Console.WriteLine("Needed Time for " + GetType().Name + ": " + duration.ToString());
        }

        protected abstract void DoPhonetize(string input);

        #endregion Methods
    }

    [TestFixture]
    public class CImplSpeedTest : AbstractSpeedTest
    {
        #region Fields

        private CImplAdapter adapter;

        #endregion Fields

        #region Methods

        public override void SetUp()
        {
            base.SetUp();
            adapter = new CImplAdapter();
        }

        protected override void DoPhonetize(string input)
        {
            adapter.Phonetize(input);
        }

        #endregion Methods
    }

    [TestFixture]
    public class DotNetSpeedTest : AbstractSpeedTest
    {
        #region Fields

        private Phonetizer phonetizer;

        #endregion Fields

        #region Methods

        public override void SetUp()
        {
            base.SetUp();
            phonetizer = new Phonetizer();
        }

        protected override void DoPhonetize(string input)
        {
            phonetizer.Phonetize(input);
        }

        #endregion Methods
    }
}