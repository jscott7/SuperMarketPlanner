using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SuperMarketPlanner;

namespace SuperMarketPlannerTest
{
    [TestClass]
    public class SelectedMealTests
    {
        [TestMethod]
        public void TestSelectedMealDate()
        {
            SelectedMeal selectedMeal = new SelectedMeal();
            selectedMeal.DateTime = new DateTime(2018, 10, 11, 15, 00, 00, 000);

            Assert.AreEqual("Thursday Oct 11", selectedMeal.Date);
        }

        [TestMethod]
        public void TestSelectedMeals()
        { 
            SelectedMeal selectedMeal = new SelectedMeal() { DateTime = DateTime.Now };
            selectedMeal.addMeal("Sausages");
            selectedMeal.addMeal("Eggs");

            Assert.AreEqual(2, selectedMeal.Meals.Count);
            Assert.AreEqual("Sausages\r\nEggs", selectedMeal.MealsString);

            selectedMeal.Clear();
            Assert.AreEqual(0, selectedMeal.Meals.Count, "Meals should have been cleared");
        }
    }
}
