using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UntisExp;

namespace NUnitTests
{
    [TestFixture]
    public sealed class HelperTests
    {
        [Test]
        public void AddSpaces()
        {
            Assert.AreEqual("My Incredible Text. It Rocks!", Helpers.AddSpaces("MyIncredibleText.ItRocks!"));
        }

        [Test]
        public void TrunciateCorrectly()
        {
            Assert.AreEqual("Dies ist ein", Helpers.TruncateWithPreservation("Dies ist ein unglaublich idiotischer Test.", 11));
        }

        [Test]
        public void NullStringEmpty()
        {
            Assert.IsTrue(Helpers.IsEmpty(null));
        }

        [Test]
        public void EmptyStringEmpty()
        {
            Assert.IsTrue(Helpers.IsEmpty(""));
        }

        [Test]
        public void WhitespaceStringEmpty()
        {
            Assert.IsTrue(Helpers.IsEmpty("  "));
        }

        [Test]
        public void FilledStringNotEmpty()
        {
            Assert.IsFalse(Helpers.IsEmpty(" hgfdsfghjk "));
        }
# if DEBUG
        [Test]
        public void IsRandomItemPartOfTheArray()
        {
            var a = new [] {"Ging", "der", "Jockel", "Feuer", "holen"};
            var res = Helpers.getRandomArrayItem(a);
            Assert.IsTrue(a.Contains(res));
        }

        [Test]
        public void CountsTodayTomorrowArrayCorrectly()
        {
            var a = new List<Data>(new[]
            {
                new Data(new DateTime(2015, 7, 20)), new Data("1", "", "", "", "", ""), new Data(new DateTime(2015, 7, 21)),
                new Data("1", "", "", "", "", ""), new Data("3", "", "", "", "", ""), new Data("4", "", "", "", "", "")
            });
            var exp = new[] { 1, 3 };
            var res = Helpers.getTodayTomorrowNum(a);
            Assert.AreEqual(exp, res);
        }
        [Test]
        public void CountsTodayTomorrowArrayCorrectlyWithEmptyDays()
        {
            var a = new List<Data>(new[]
            {
                new Data(new DateTime(2015, 7, 20)), new Data(), new Data(new DateTime(2015, 7, 21)),
                new Data("1", "", "", "", "", ""), new Data("3", "", "", "", "", ""), new Data("4", "", "", "", "", "")
            });
            var exp = new[] { 0, 3 };
            Assert.AreEqual(exp, Helpers.getTodayTomorrowNum(a));
        }
        [Test]
        public void CountsTodayTomorrowArrayCorrectlyWithOnlyOneDay()
        {
            var a = new List<Data>(new[]
            {
                new Data(new DateTime(2015, 7, 20)), new Data("1", "", "", "", "", ""), new Data("3", "", "", "", "", ""), new Data("4", "", "", "", "", "")
            });
            var exp = new[] { 3, 0 };
            Assert.AreEqual(exp, Helpers.getTodayTomorrowNum(a));
        }
        [Test]
        public void CountsTodayTomorrowArrayCorrectlyWithOnlyEmptyDays()
        {
            var a = new List<Data>(new[]
            {
                new Data(new DateTime(2015, 7, 20)),  new Data(new DateTime(2015, 7, 21))
            });
            var exp = new[] { 0, 0 };
            Assert.AreEqual(exp, Helpers.getTodayTomorrowNum(a));
        }
#endif
    }
}