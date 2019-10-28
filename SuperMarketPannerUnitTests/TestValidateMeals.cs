using NUnit.Framework;
using SuperMarketPlanner;

namespace SuperMarketPannerUnitTests
{
    class TestValidateMeals
    {
        [Test]
        public void TestValidChars()
        {
            bool isValid = RowValidationRule.IsValidXmlString("abc");
            Assert.AreEqual(true, isValid);
            isValid = RowValidationRule.IsValidXmlString("ab/c");
            Assert.AreEqual(false, isValid, "Failed with /");
            isValid = RowValidationRule.IsValidXmlString("ab&c");
            Assert.AreEqual(false, isValid, "Failed with &");
            isValid = RowValidationRule.IsValidXmlString("<abc");
            Assert.AreEqual(false, isValid, "Failed with <");
            isValid = RowValidationRule.IsValidXmlString(">abc");
            Assert.AreEqual(false, isValid, "Failed with >");
            isValid = RowValidationRule.IsValidXmlString("\"abc");
            Assert.AreEqual(false, isValid, "Failed with \"");

        }

    }
}
