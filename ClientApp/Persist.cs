using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace SuperMarketPlanner
{
    /*
     * Class to handle interaction between the UI and Rest Server
     */
    class Persist
    {
        public async void LoadLatest(SelectedMealCollection mealData, SelectedIngredientsCollection ingredientsData)
        {
            try
            {
                string data = await ServerGet("");

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(data);
                XmlNodeList meals = xmlDoc.SelectNodes("//ArrayOfSelectedMeal/SelectedMeal");
                foreach (XmlNode xmlNode in meals)
                {
                    //<ArrayOfSelectedMeal>
                    // Now populate meals and staples              
                    string date = xmlNode.SelectSingleNode("DateTime").InnerText;
                    var ingredients = xmlNode.SelectNodes("Ingredients/string");
                    var mealsForDate = xmlNode.SelectNodes("Meals/string");
                    SelectedMeal selectedMeal = new SelectedMeal();

                    // Datetime conversion
                    DateTime mealDate;
                    if (DateTime.TryParse(date, out mealDate))
                    {
                        selectedMeal.DateTime = mealDate;
                    }

                    foreach(XmlNode mealNode in mealsForDate)
                    {
                        selectedMeal.addMeal(mealNode.InnerText);
                    }

                    foreach (XmlNode ingredientNode in ingredients)
                    {
                        selectedMeal.Ingredients.Add(ingredientNode.InnerText);
                    }

                    mealData.Add(selectedMeal);
                }

                //<ArrayOfSelectedIngredient>
                XmlNodeList selectedIngredients = xmlDoc.SelectNodes("//ArrayOfSelectedIngredient/SelectedIngredient");
                foreach (XmlNode xmlNode in selectedIngredients)
                {
                    string ingredient = xmlNode.SelectSingleNode("Ingredient").InnerText;
                    string date = xmlNode.SelectSingleNode("DateToUse").InnerText;

                    SelectedIngredient selectedIngredient = new SelectedIngredient();
                    selectedIngredient.Ingredient = ingredient;
                    selectedIngredient.DateToUse = date;
                    ingredientsData.Add(selectedIngredient);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load data from server: " + ex, "Failed Load", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// Post the xml string to the webservice on the Raspberry Pi
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public async Task<string> Post(string payload, DateTime listDate)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                try
                {
                    // Key is date of start of list in yyyyMMdd format         
                    var postData = new KeyValuePair<string, string>[]
                    {
                     new KeyValuePair<string, string>(listDate.ToString("yyyyMMdd"), payload)
                    };

                    var content = new FormUrlEncodedContent(postData);

                    var response = await client.PostAsync("http://192.168.0.202:8080/index", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var message = String.Format("Server returned HTTP error {0}: {1}.", (int)response.StatusCode, response.ReasonPhrase);
                        MessageBox.Show(message, "Successful Sync", MessageBoxButton.OK, MessageBoxImage.Error);
                        throw new InvalidOperationException(message);
                    }

                    var data = await response.Content.ReadAsStringAsync();

                    if (data.Length > 0)
                    {
                        MessageBox.Show("List sent to server", "Successful Sync", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }

                    return data;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to send data to server: " + ex, "Failed Sync", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "";
                }
            }
        }

        private async Task<string> ServerGet(string request)
        {
            using (var client = new HttpClient())
            {
                var data = await client.GetStringAsync("http://192.168.0.202:8080/index");
                return data;
            }
        }
    }
}
