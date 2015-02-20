using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tools.Logging.Common.Test
{
    [TestClass]
    public class SwManagerTest
    {
        [TestMethod]
        public void TestExtraWriteToLog()
        {
            var res = "";
            using (SwManager.Start("aaa", s => res = s))
            {
                using (SwManager.Start("bbb", s => res = s))
                {
                    using (SwManager.Start("ccc", s => res = s))
                    {

                    }
                }
                using (SwManager.Start("bbb2"))
                {
                    using (SwManager.Start("ccc2"))
                    {

                    }
                }
            }

            Assert.IsTrue(!String.IsNullOrWhiteSpace(res));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestArgumentNullException()
        {
            using (SwManager.Start("test")){}
        }

        [TestMethod]
        public void TestParallel()
        {
            var res = "";
            var act = new Action(() =>
            {
                var resLocal = "";
                using (SwManager.Start("aaa", s => resLocal = s))
                {
                    using (SwManager.Start("bbb"))
                    {
                        using (SwManager.Start("ccc"))
                        {
                            Thread.Sleep(500);
                        }
                    }
                    using (SwManager.Start("bbb2"))
                    {
                        using (SwManager.Start("ccc2"))
                        {

                        }
                    }
                }
                res += resLocal;
            });

            Parallel.Invoke(act, act, act, act, act, act, act, act, act, act);

            Assert.IsTrue(!String.IsNullOrWhiteSpace(res));
        }

    }
}