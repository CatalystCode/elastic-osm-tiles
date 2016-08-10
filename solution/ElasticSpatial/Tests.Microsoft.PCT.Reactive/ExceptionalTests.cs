using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.PCT.TestingUtilities;
using Microsoft.PCT.Reactive;

namespace Tests.Microsoft.PCT.Reactive
{
    [TestClass]
    public class ExceptionalTests
    {
        [TestMethod]
        public void Exceptional_ArgumentChecks()
        {
            AssertEx.Throws<ArgumentNullException>(() => Exceptional.ToExceptional<object>(null), ex => Assert.AreEqual("exception", ex.ParamName));
        }

        [TestMethod]
        public void Exceptional_Simple()
        {
            var m1 = (42).ToExceptional();
            Assert.AreEqual(42, m1.Value);
            Assert.IsTrue(m1.HasValue);
            Assert.IsNull(m1.Exception);

            var ex = new Exception();
            var m2 = ex.ToExceptional<object>();
            Assert.AreSame(ex, m2.Exception);
            Assert.IsFalse(m2.HasValue);
            Assert.IsNull(m2.Value);
        }

        [TestMethod]
        public void Exceptional_Equals()
        {
            var ex = new Exception();
            var mv1 = (42).ToExceptional();
            var mv2 = (42).ToExceptional();
            var mv3 = (43).ToExceptional();
            var me1 = ex.ToExceptional<int>();
            var me2 = ex.ToExceptional<int>();
            var me3 = new Exception().ToExceptional<int>();
            var mo1 = ("foo").ToExceptional<string>();

            Assert.IsTrue(mv1.Equals(mv2));
            Assert.IsTrue(object.Equals(mv1, mv2));
            Assert.IsFalse(mv1.Equals(mv3));
            Assert.IsTrue(mv1 == mv2);
            Assert.IsTrue(mv1 != mv3);
            Assert.AreEqual(mv1.GetHashCode(), mv2.GetHashCode());

            Assert.IsTrue(me1.Equals(me2));
            Assert.IsTrue(object.Equals(me1, me2));
            Assert.IsFalse(me1.Equals(me3));
            Assert.IsTrue(me1 == me2);
            Assert.IsTrue(me1 != me3);
            Assert.AreEqual(me1.GetHashCode(), me2.GetHashCode());

            Assert.IsFalse(mv1.Equals(mo1));
        }

        [TestMethod]
        public void Exceptional_ToString()
        {
            var ex = new Exception();
            var mv = (42).ToExceptional();
            var me = ex.ToExceptional<int>();
            var mn = Exceptional.ToExceptional(default(object));
            var md = default(Exceptional<object>);

            Assert.AreEqual("Exceptional(42)", mv.ToString());
            Assert.AreEqual("Exceptional(" + ex.ToString() + ")", me.ToString());
            Assert.AreEqual("Exceptional(null)", mn.ToString());
            Assert.AreEqual("Exceptional()", md.ToString());
        }
    }
}
