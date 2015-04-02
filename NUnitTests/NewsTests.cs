using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UntisExp;

namespace NUnitTests
{
    [TestFixture]
    public class NewsTests
    {
        [Test]
        [Category("Constructors")]
        public void NormalConstructorWillHaveEmptyTitle()
        {
            Assert.AreEqual(null, (new News()).Title);
        }

        [Test]
        [Category("Constructors")]
        public void PrefilledConstructorWillRunAndPrefillSummary()
        {
            var sut = new News(1, "Aliens über Onenett", new Uri("http://gnampf.de"), "gffs", "http://gnampf.de/image.png",
                "gffses");
            Assert.AreEqual("gffs", sut.Summary);
        }

        [Test]
        [Category("Self refreshing objects")]
        public void SummaryWillBeGeneratedFromContent()
        {
            var sut = new News { Content = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. Cras dapibus. Vivamus elementum semper nisi. Aenean vulputate eleifend tellus. Aenean leo ligula, porttitor eu, consequat vitae, eleifend ac, enim. Aliquam lorem ante, dapibus in, viverra quis, feugiat a" };
            sut.Refresh();
            Assert.AreEqual("Lorem ipsum dolor sit amet, consectetuer adipiscing", sut.Summary);
        }

        [Test]
        [Category("Self refreshing objects")]
        public void SourceWillBeSpelledOutFromUrl1()
        {
            var sut = new News { Source = new Uri(VConfig.Url) };
            sut.Refresh();
            Assert.AreEqual("CHRISTIAN-WIRTH-SCHULE", sut.SourcePrint);
        }

        [Test]
        [Category("Self refreshing objects")]
        public void SourceWillBeSpelledOutFromUrl2()
        {
            var sut = new News { Source = new Uri("https://ign.com/") };
            sut.Refresh();
            Assert.AreEqual("SR-BLOG", sut.SourcePrint);
        }

        [Test]
        [Category("Network dependant integration test")]
        public async void PressWillCallBackWithNewsListSpecified()
        {
            var sut = new Press();
            var res = await sut.GetNews();
            Assert.IsTrue(res.Count > 1);
        }

        [Test]
        [Category("Callback driven async methods")]
        public void CallbackOfPressMethodWillBeCalled()
        {
            bool calledBack = false;
            List<News> res = new List<News>();
            Action<List<News>> callback = (List<News> _res) => {
                calledBack = true;
                res = _res;
            };
            var sut = new Press();
            sut.GetCalledBackForNews(callback);
            for (int i = 0; (i < 10) && !calledBack; i++)
            {
                Thread.Sleep(1000);
            }
            Assert.IsTrue(res.Count > 1);
        }

    }
}