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
using phonet4n.Core;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace phonet4n.Tests
{
    public abstract class AbstractSpeedTest
    {
        abstract protected void DoPhonetize(string input);

        string[] data;

        [SetUp]
        virtual public void SetUp()
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

            Console.WriteLine("Needed Time for " + GetType().Name + ": " + duration.Milliseconds.ToString());
        }

        [Test]
        public void BatchTestMethod()
        {
            for (int i = 0; i < 10; i++)
                TestMethod();
        }

    }

    [TestFixture]
    public class DotNetSpeedTest : AbstractSpeedTest
    {

        private Phonetizer phonetizer;

        override public void SetUp()
        {
            base.SetUp();
            phonetizer = new Phonetizer();
            phonetizer.SetLangage("german_1");
        }

        override protected void DoPhonetize(string input)
        {
            phonetizer.Phonetize(input);
        }
    }

    [TestFixture]
    public class CImplSpeedTest : AbstractSpeedTest
    {
        private CImplAdapter adapter;

        override public void SetUp()
        {
            base.SetUp();
            adapter = new CImplAdapter();
        }

        override protected void DoPhonetize(string input)
        {
            adapter.Phonetize(input);
        }
    }


}
