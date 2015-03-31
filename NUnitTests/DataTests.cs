using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit;
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
            var s = "Jolla";
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
            var s = "Jolla";
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
            var sut = new Data { EntfallStr = " x " };
            sut.Refresh();
            Assert.IsTrue(sut.Entfall);
        }

        [Test]
        public void WillConvertCocareStringToState()
        {
            var sut = new Data { MitbeStr = " x " };
            sut.Refresh();
            Assert.IsTrue(sut.Mitbetreung);
        }

        [Test]
        public void FirstRowGenerationFromMinimalData()
        {
            var sut = new Data {AltFach = "DE", Stunde = "3"};
            sut.Refresh();
            Assert.AreEqual("3. Std: Deutsch", sut.Line1);
        }

        [Test]
        public void SecondRowGenerationCancelledLesson()
        {
            var sut = new Data { AltFach = "PH", EntfallStr = " x ", Lehrer = "BL" };
            sut.Refresh();
            Assert.AreEqual("Physik bei BL entfällt.", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationOtherLesson()
        {
            var sut = new Data { AltFach = "MA", Fach = "EN", Lehrer = "HC", Vertreter = "MUE" };
            sut.Refresh();
            Assert.AreEqual("Englisch bei MUE statt Mathe", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationOnlyNewTeacher()
        {
            var sut = new Data { Fach = "KU", Vertreter = "WH" };
            sut.Refresh();
            Assert.AreEqual("Bei WH", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationOnlyNewTeacherWithRoom()
        {
            var sut = new Data { Fach = "KU", Vertreter = "WH", Raum = "112" };
            sut.Refresh();
            Assert.AreEqual("Bei WH in 112", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationOtherTeacher()
        {
            var sut = new Data { AltFach="IN", Fach = "IN", Vertreter = "BLAU", Lehrer = "ZR"};
            sut.Refresh();
            Assert.AreEqual("BLAU vertritt ZR", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationOtherTeacherWithRoom()
        {
            var sut = new Data { AltFach = "IN", Fach = "IN", Vertreter = "BLAU", Lehrer = "ZR", Raum = "D33" };
            sut.Refresh();
            Assert.AreEqual("BLAU vertritt ZR | D33", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationOtherRoom()
        {
            var sut = new Data { AltFach = "CH", Fach = "CH", Vertreter = "KS", Lehrer = "KS", Raum = "D35" };
            sut.Refresh();
            Assert.AreEqual("KS | D35", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationMinData()
        {
            var sut = new Data { AltFach = "CH", Fach = "CH", Vertreter = "KS", Lehrer = "KS"};
            sut.Refresh();
            Assert.AreEqual("KS", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationOtherLessonWithRoom()
        {
            var sut = new Data { AltFach = "MA", Fach = "EN", Lehrer = "HC", Vertreter = "MUE", Raum = "C17"};
            sut.Refresh();
            Assert.AreEqual("Englisch bei MUE statt Mathe | C17", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationCocare()
        {
            var sut = new Data { Fach = "MU", MitbeStr = " x ", Lehrer = "LK", Vertreter = "LOH"};
            sut.Refresh();
            Assert.AreEqual("Musik bei LK wird durch LOH mitbetreut.", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationCocareWithRoom()
        {
            var sut = new Data { Fach = "MU", MitbeStr = " x ", Lehrer = "LK", Vertreter = "LOH", Raum = "316"};
            sut.Refresh();
            Assert.AreEqual("Musik bei LK wird durch LOH mitbetreut. | 316", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationEvent()
        {
            var sut = new Data { Fach = "DS", Veranstaltung = true, Lehrer = "ME", Raum = "BUE", Notiz = "PeoplesTheater" };
            sut.Refresh();
            Assert.AreEqual("Peoples Theater; Raum: BUE | Mit ME", sut.Line2);
        }

        [Test]
        public void SecondRowGenerationEventWithGroup()
        {
            var sut = new Data { Fach = "DS", Veranstaltung = true, Lehrer = "ME", Raum = "BUE", Notiz = "PeoplesTheater", Klasse = "7A1, 7A2, 7N1"};
            sut.Refresh();
            Assert.AreEqual("Peoples Theater; Raum: BUE | Mit ME und 7A1, 7A2, 7N1", sut.Line2);
        }
    
    }
}
