using System;
using System.Diagnostics;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Transactions
{
    [TestClass]
    public sealed class TransactionalCommandTest
    {
        [TestMethod]
        [TestCategory("Transactions")]
        [Timeout(30000)]
        public void SimpleTransactionalEntityTest()
        {
            var entity = new SimpleTransactionalEntity();
            Trace.WriteLine(entity.State, "Original State");

            try
            {
                using (var scope = new TransactionScope())
                {
                    for (var i = 1; i <= 10; i++)
                    {
                        entity.State++;
                        Trace.WriteLine(entity.State, "Modified State");
                        Assert.AreEqual(i, entity.State);
                    }

                    Trace.WriteLine("Rollback Transaction.");
                }

                Trace.WriteLine(entity.State, "Current State");
                Assert.AreEqual(0, entity.State);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Transactions")]
        [Timeout(30000)]
        public void SimpleTransactionalCollectionTest()
        {
            var entity = new SimpleTransactionalCollection<int>();
            Trace.WriteLine(entity.Count, "Original State");

            try
            {
                using (var scope = new TransactionScope())
                {
                    for (var i = 1; i <= 10; i++)
                    {
                        entity.Add(i);
                        Trace.WriteLine(entity.Count, "Modified State");
                        Assert.AreEqual(i, entity.Count);
                    }

                    Trace.WriteLine("Rollback Transaction.");
                }

                Trace.WriteLine(entity.Count, "Current State");
                Assert.AreEqual(0, entity.Count);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }
    }
}
