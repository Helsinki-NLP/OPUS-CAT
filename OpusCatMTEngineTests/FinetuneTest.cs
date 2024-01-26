using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpusCatMTEngineTests
{
    [TestClass]
    public class FinetuneTest
    {
        /// <summary>
        /// This is not functional, there should be tests for most common
        /// functionalities, but no time to implement right now. Use this as
        /// starting point if time is available.
        /// </summary>
        [TestMethod]
        public void TestFinetune()
        {
            var modelInstallPath = OpusCatMtEngine.HelperFunctions.GetLocalAppDataPath("models\\eng-fin\\opus+bt-2021-03-09");
            var model = new OpusCatMtEngine.MTModel("eng-fin\\opus+bt-2021-03-09", modelInstallPath);
            var testTranslation = model.Translate("this is a test", model.SourceLanguages.First(), model.TargetLanguages.First());
            var result = testTranslation.Result;
            var finetuneWindow = new OpusCatMtEngine.ModelCustomizerView(model);

        }
    }
}
