using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Linq;
using NUnit.Framework;
using UntisExp;
using UntisExp.Containers;

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
            var res = Helpers.GetRandomArrayItem(a);
            Assert.IsTrue(a.Contains(res));
        }

        [Test]
        public void CountsTodayTomorrowArrayCorrectly()
        {
            var a = new List<Data>(new[]
            {
                new Data(new DateTime(2015, 7, 20)), new Data("1", "", "", "", "", ""), new Data(new DateTime(2015, 7, 21)),
                new Data("1", "", "", "", "", ""), new Data("3", "", "", "", "", ""), new Data("4", "", "", "", "", ""),
                new Data(new DateTime(2015, 7, 22)), new Data("1", "", "", "", "", ""), new Data(new DateTime(2015, 7, 23)),
                new Data("1", "", "", "", "", ""), new Data("3", "", "", "", "", ""), new Data("4", "", "", "", "", "")
            });
            var exp = new[] { 1, 3, 1, 3 };
            var res = Helpers.GetTodayTomorrowNum(a);
            Assert.AreEqual(exp, res);
        }

        [Test]
        public void DayListsWillCorrectlyJoin()
        {
            var serializer = new JavaScriptSerializer();
            var a = new List<Data>(new[]
            {
                new Data(new DateTime(2015, 7, 20)), new Data("1", "Deutsch", "", "", "", ""),
                new Data(new DateTime(2015, 7, 21)), new Data("1", "", "", "", "", ""),
                new Data("3", "", "", "", "", ""), new Data("4", "", "", "", "", ""),
                new Data(new DateTime(2015, 7, 23)), new Data("5", "", "", "", "", "")
            });

            var b = new List<Data>(new[]
            {
                new Data(new DateTime(2015, 7, 20)), new Data("3-4", "Englisch", "", "", "", ""),
                new Data(new DateTime(2015, 7, 22)), new Data("5", "", "", "", "", "")
            });

            var exp = new List<Data>(new[]
            {
                new Data(new DateTime(2015, 7, 20)), new Data("1", "Deutsch", "", "", "", ""),
                new Data("3-4", "Englisch", "", "", "", ""), new Data(new DateTime(2015, 7, 21)),
                new Data("1", "", "", "", "", ""), new Data("3", "", "", "", "", ""),
                new Data("4", "", "", "", "", ""), new Data(new DateTime(2015, 7, 22)),
                new Data("5", "", "", "", "", ""), new Data(new DateTime(2015, 7, 23)),
                new Data("5", "", "", "", "", "")
            });

            var res = Helpers.JoinTwoDataLists(b, a);

            var comparableRes = serializer.Serialize(res);
            var comparableExp = serializer.Serialize(exp);

            Assert.AreEqual(comparableExp, comparableRes);

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
            Assert.AreEqual(exp, Helpers.GetTodayTomorrowNum(a));
        }
        [Test]
        public void CountsTodayTomorrowArrayCorrectlyWithOnlyOneDay()
        {
            var a = new List<Data>(new[]
            {
                new Data(new DateTime(2015, 7, 20)), new Data("1", "", "", "", "", ""), new Data("3", "", "", "", "", ""), new Data("4", "", "", "", "", "")
            });
            var exp = new[] { 3, 0 };
            Assert.AreEqual(exp, Helpers.GetTodayTomorrowNum(a));
        }
        [Test]
        public void CountsTodayTomorrowArrayCorrectlyWithOnlyEmptyDays()
        {
            var a = new List<Data>(new[]
            {
                new Data(new DateTime(2015, 7, 20)),  new Data(new DateTime(2015, 7, 21))
            });
            var exp = new[] { 0, 0 };
            Assert.AreEqual(exp, Helpers.GetTodayTomorrowNum(a));
        }
#endif
    }
}