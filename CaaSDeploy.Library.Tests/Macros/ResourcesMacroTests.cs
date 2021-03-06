﻿using System.Collections.Generic;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Macros;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace DD.CBU.CaasDeploy.Library.Tests.Macros
{
    /// <summary>
    /// Unit tests for the <see cref="ResourcesMacro"/> class.
    /// </summary>
    [TestClass]
    public class ResourcesMacroTests
    {
        /// <summary>
        /// Tests the macro under normal conditions.
        /// </summary>
        /// <returns>The async <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task Resources_SubstituteTokensInString_Success()
        {
            var context = new TaskContext
            {
                ResourcesProperties = new Dictionary<string, JObject>()
                {
                    { "Param1", JObject.Parse("{ name: 'Param2' }") },
                    { "Param2", JObject.Parse("{ name: 'Param3' }") }
                }
            };

            var macro = new ResourcesMacro();
            var input = "http://$resources[$resources['Param1'].name].name/index.html";
            var output = await macro.SubstituteTokensInString(null, context, input);

            Assert.AreEqual("http://Param3/index.html", output);
        }

        /// <summary>
        /// Tests the macro under error conditions.
        /// </summary>
        /// <returns>The async <see cref="Task"/>.</returns>
        [TestMethod]
        [ExpectedException(typeof(TemplateParserException))]
        public async Task Resources_SubstituteTokensInString_NotFound()
        {
            var context = new TaskContext
            {
                ResourcesProperties = new Dictionary<string, JObject>()
                {
                    { "Param1", JObject.Parse("{ name: 'Value1' }") }
                }
            };

            var macro = new ResourcesMacro();
            var input = "Hello_$resources['Param2'].name";
            await macro.SubstituteTokensInString(null, context, input);
        }
    }
}
