using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UntisExp;

namespace NUnitTests
{
    [TestFixture]
    public class FetcherTests
    {
        [Test]
        [Category("Callback driven async methods")]
        [Category("Network dependant integration test")]
        public void CallbackOfPressMethodWillBeCalled()
        {
            bool calledBack = false;
            List<Data> res = new List<Data>();
            Action<List<Data>> callback = _res =>
            {
                calledBack = true;
                res = _res;
            };
            var sut = new Fetcher(() => { }, (a, b, c) => { }, callback);
            sut.GetTimes(12, Activity.ParseFirstSchedule, 13);
            for (int i = 0; (i < 20) && !calledBack; i++)
            {
                Thread.Sleep(1000);
            }
            Assert.IsTrue(res.Count > 1);
        }

        [Test]
        [Category("Mocked dependencies")]
        public void WillConstructRightUrlString()
        {
            Action<List<Data>> callback = res =>
            {
            };
            var spy = new MockedNetworkAccessor();
            var sut = new Fetcher(() => { }, (a, b, c) => { }, callback, d => { }, l => { }, spy);
            sut.GetTimes(12, Activity.ParseFirstSchedule, 13);
            Assert.AreEqual("http://vp.cws-usingen.de/Schueler/13/w/w00012.htm", spy.CalledUri);
        }

        [Test]
        [Category("Mocked dependencies")]
        public void WillConstructRightUrlStringForFollowups()
        {
            Action<List<Data>> callback = res =>
            {
            };
            var spy = new MockedNetworkAccessor();
            var sut = new Fetcher(() => { }, callback, 5, spy);
            sut.GetTimes(12, Activity.ParseSecondSchedule, 14);
            Assert.AreEqual("http://vp.cws-usingen.de/Schueler/15/w/w00012.htm", spy.CalledUri);
        }
    }
}