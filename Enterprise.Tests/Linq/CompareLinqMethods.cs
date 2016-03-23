using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Enterprise.Core.Common.Collections.Extensions;
using Enterprise.Core.Linq;
using Enterprise.Core.Linq.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Linq
{
    [TestClass]
    public sealed class CompareLinqMethods
    {
        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("Temp")]
        [Timeout(30000)]
        public void CompareEnumerableLinqMethods()
        {
            try
            {
                var linqMethods = this.GetLinqMethods(typeof(Enumerable));
                var asyncLinqMethods = this.GetLinqMethods(typeof(AsyncEnumerable));

                var query = linqMethods.Except(asyncLinqMethods);

                query.ForEach(item => Trace.WriteLine(item));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        [TestCategory("Linq")]
        [TestCategory("Temp")]
        [Timeout(30000)]
        public void CompareQueryableLinqMethods()
        {
            try
            {
                var linqMethods = this.GetLinqMethods(typeof(Queryable));
                var asyncLinqMethods = this.GetLinqMethods(typeof(QueryableExtensions));

                var query = linqMethods.Except(asyncLinqMethods);

                query.ForEach(item => Trace.WriteLine(item));
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        private IEnumerable<string> GetLinqMethods(
            Type type)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.Static;

            var query =
                from method in type.GetMethods(bindingFlags)
                orderby method.Name
                select method.Name.Replace("Async", string.Empty);

            return query.Distinct();
        }
    }
}
