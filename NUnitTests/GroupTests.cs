using UntisExp;
using NUnit.Framework;

namespace NUnitTests
{
    [TestFixture]
    public class GroupTests
    {

        [Test]
        public void NormalConstructorIdWillBeEmpty()
        {
            Assert.AreEqual(0, (new Group().ID));
        }

        [Test]
        public void NormalConstructorNameWillBeEmpty()
        {
            Assert.AreEqual(null, (new Group().ClassName));
        }

        [Test]
        public void PrefilledConstructorWillFillOutName()
        {
            Assert.AreEqual("Q2", (new Group(35, "Q2").ClassName));
        }

        [Test]
        public void PrefilledConstructorWillFillOutId()
        {
            Assert.AreEqual(2, (new Group(2, "5A").ID));
        }

        [Test]
        public void ToStringReturnsClassName()
        {
            var sut = new Group(36, "Q3");
            Assert.AreEqual("Q3", sut.ToString());
        }
    }
}