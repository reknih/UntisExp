using System;
using System.Collections.Generic;
using NUnit.Framework;
using UntisExp;

namespace NUnitTests
{
    [TestFixture]
    public sealed class DataTests
    {
        [Test]
        [Category("Constructors")]
        public void MakeHeading()
        {
            const string s = "Jolla";
            var sut = new Data(s);
            Assert.AreEqual(s, sut.Line1);
        }

        [Test]
        [Category("Constructors")]
        public void MakeDateHeading()
        {
            var d = new DateTime(2017, 11, 13);
            var sut = new Data(d);
            Assert.AreEqual("Montag, 13.11", sut.Line1);
        }

        [Test]
        [Category("Constructors")]
        public void MakeHeadingEmptySecond()
        {
            const string s = "Jolla";
            var sut = new Data(s);
            Assert.IsTrue(string.IsNullOrEmpty(sut.Line2));
        }

        [Test]
        [Category("Constructors")]
        public void MakeDateHeadingEmptySecond()
        {
            var d = new DateTime(2017, 11, 13);
            var sut = new Data(d);
            Assert.IsTrue(string.IsNullOrEmpty(sut.Line2));
        }

        [Test]
        [Category("Constructors")]
        public void QuickInstanceInitializerWillFillLine1()
        {
            var sut = new Data("3-4", "DE", "CHR", "MD", "E21", "");
            Assert.IsFalse(string.IsNullOrWhiteSpace(sut.Line2));
        }

        [Test]
        [Category("Constructors")]
        public void QuickInstanceInitializerWillNotBeADate()
        {
            var sut = new Data("3-4", "DE", "CHR", "MD", "E21", "");
            Assert.IsFalse(sut.DateHeader);
        }

        [Test]
        [Category("Returns")]
        public void RefreshReturnsSameObject()
        {
            var sut = new Data();
            Assert.AreEqual(sut, sut.Refresh());
        }

        [Test]
        public void WillConvertCancelledStringToState()
        {
            var sut = new Data { OutageStr = " x " };
            sut.Refresh();
            Assert.IsTrue(sut.Outage);
        }

        [Test]
        public void WillConvertCocareStringToState()
        {
            var sut = new Data { CareStr = " x " };
            sut.Refresh();
            Assert.IsTrue(sut.Cared);
        }

        [Test]
        public void FirstRowGenerationFromMinimalData()
        {
            var sut = new Data {OldSubject = "DE", Lesson = "3"};
            sut.Refresh();
            Assert.AreEqual("3. Std: Deutsch", sut.Line1);
        }

        [Test]
        public void SecondRowGenerationCancelledLesson()
        {
            var sut = new Data { OldSubject = "PH", OutageStr = " x ", Teacher = "BL" };
            sut.Refresh();
            Assert.AreEqual("Physik bei BL entfällt.", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationOtherLesson()
        {
            var sut = new Data { OldSubject = "MA", Subject = "EN", Teacher = "HC", Cover = "MUE" };
            sut.Refresh();
            Assert.AreEqual("Englisch bei MUE statt Mathe", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationOnlyNewTeacher()
        {
            var sut = new Data { Subject = "KU", Cover = "WH" };
            sut.Refresh();
            Assert.AreEqual("Bei WH", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationOnlyNewTeacherWithRoom()
        {
            var sut = new Data { Subject = "KU", Cover = "WH", Room = "112" };
            sut.Refresh();
            Assert.AreEqual("Bei WH in 112", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationOtherTeacher()
        {
            var sut = new Data { OldSubject="IN", Subject = "IN", Cover = "BLAU", Teacher = "ZR"};
            sut.Refresh();
            Assert.AreEqual("BLAU vertritt ZR", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationOtherTeacherWithRoom()
        {
            var sut = new Data { OldSubject = "IN", Subject = "IN", Cover = "BLAU", Teacher = "ZR", Room = "D33" };
            sut.Refresh();
            Assert.AreEqual("BLAU vertritt ZR | D33", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationOtherRoom()
        {
            var sut = new Data { OldSubject = "CH", Subject = "CH", Cover = "KS", Teacher = "KS", Room = "D35" };
            sut.Refresh();
            Assert.AreEqual("KS | D35", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationMinData()
        {
            var sut = new Data { OldSubject = "CH", Subject = "CH", Cover = "KS", Teacher = "KS"};
            sut.Refresh();
            Assert.AreEqual("KS", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationOtherLessonWithRoom()
        {
            var sut = new Data { OldSubject = "MA", Subject = "EN", Teacher = "HC", Cover = "MUE", Room = "C17"};
            sut.Refresh();
            Assert.AreEqual("Englisch bei MUE statt Mathe | C17", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationCocare()
        {
            var sut = new Data { Subject = "MU", CareStr = " x ", Teacher = "LK", Cover = "LOH"};
            sut.Refresh();
            Assert.AreEqual("Musik bei LK wird durch LOH mitbetreut.", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationCocareWithRoom()
        {
            var sut = new Data { Subject = "MU", CareStr = " x ", Teacher = "LK", Cover = "LOH", Room = "316"};
            sut.Refresh();
            Assert.AreEqual("Musik bei LK wird durch LOH mitbetreut. | 316", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationEvent()
        {
            var sut = new Data { Subject = "DS", Event = true, Teacher = "ME", Room = "BUE", Notice = "PeoplesTheater" };
            sut.Refresh();
            Assert.AreEqual("Peoples Theater; Raum: BUE | Mit ME", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationEventWithGroup()
        {
            var sut = new Data { Subject = "DS", Event = true, Teacher = "ME", Room = "BUE", Notice = "PeoplesTheater", Group = "7A1, 7A2, 7N1"};
            sut.Refresh();
            Assert.AreEqual("Peoples Theater; Raum: BUE | Mit ME und 7A1, 7A2, 7N1", sut.Line2);
        }

        [Test]
        [Category("Extension methods")]
        public void SortsCorrectly()
        {
            var a = new Data(DateTime.Now);
            var b = new Data(DateTime.Now.AddDays(2));
            var c = new Data("1", "DE", "ZE", "MD", "E21", "");
            var d = new Data("1-2", "MA", "ZR", "ZS", "C14", "");
            var e = new Data("1-4", "DS", "ME", "ME", "AULA", "Sonderprobe");
            var f = new Data("4", "MU", "LK", "HAI", "317", "");
            var g = new Data("5-6", "POWI", "CHR", "CHR", "BUE", "Vortrag");
            var res = new List<Data> { e, b, d, g, f, a, c };
            res.Sort();
            var exp = new List<Data> { a, b, c, d, e, f, g };
            Assert.AreEqual(exp, res);
        }
    
    }
}
