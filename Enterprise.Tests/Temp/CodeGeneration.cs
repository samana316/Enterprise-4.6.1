using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Collections.Extensions;
using Enterprise.Core.Common.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enterprise.Tests.Temp
{
    [TestClass]
    public sealed class CodeGeneration
    {
        [TestMethod]
        [TestCategory("CodeGeneration")]
        [Timeout(10000)]
        public async Task CodeGenerationTest()
        {
            try
            {
                var generator = new CodeGenerator();

                Action action = generator.Start;

                await Task.Run(action);
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);

                Assert.Fail(exception.Message);
            }
        }

        private class CodeGenerator
        {
            public void Start()
            {
                var methods = this.GetLinqMethods();

                foreach (var method in methods)
                {

                }
            }

            private IEnumerable<MethodInfo> GetLinqMethods()
            {
                var methods = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static);

                var query =
                    from method in methods
                    where this.MethodFilter(method)
                    orderby method.Name, method.GetParameters().Length
                    select method;

                return query.ToArray();
            }

            private bool MethodFilter(
                MethodInfo method)
            {
                if (method == null)
                {
                    return false;
                }

                var parameters = method.GetParameters();
                if (parameters.IsNullOrEmpty())
                {
                    return false;
                }

                var thisParameter = parameters.First();
                if (!typeof(IEnumerable).IsAssignableFrom(thisParameter.ParameterType))
                {
                    return false;
                }

                if (typeof(IEnumerable).IsAssignableFrom(method.ReturnType))
                {
                    return false;
                }

                return true;
            }
        }
    }
}
