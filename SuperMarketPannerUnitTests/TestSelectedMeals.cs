using NUnit.Framework;
using SuperMarketPlanner;
using System;

namespace SuperMarketPannerUnitTests
{
    public class TestSelectedMeals
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestSelectedMealDate()
        {
            SelectedMeal selectedMeal = new SelectedMeal();
            selectedMeal.DateTime = new DateTime(2018, 10, 11, 15, 00, 00, 000);

            Assert.AreEqual("Thursday Oct 11", selectedMeal.Date);
        }

        [Test]
        public void TestSelectedMealBehaviour()
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