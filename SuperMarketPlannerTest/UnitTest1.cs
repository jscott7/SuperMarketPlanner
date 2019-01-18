using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SuperMarketPlanner;

namespace SuperMarketPlannerTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            SelectedMeal selectedMeal = new SelectedMeal();
            selectedMeal.DateTime = new DateTime(2018, 10, 11, 15, 00, 00, 000);

            Assert.AreEqual("Thursday Oct 11", selectedMeal.Date);
        }
    }
}
