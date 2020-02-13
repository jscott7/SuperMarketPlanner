using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Data;

namespace SuperMarketPlanner
{
    /// <summary>
    /// Class to handle interaction between the UI and Rest Server
    /// </summary>
    class Persist
    {
        private string _serverUrl = "";
  
        public Persist(string serverUrl)
        {
            _serverUrl = serverUrl;
        }

        public async Task<DateTime> LoadLatest(SelectedMealCollection mealData, SelectedIngredientsCollection ingredientsData)
        {
            DateTime startDate = DateTime.MinValue;

            try
            {
                if (_serverUrl == "NOSERVER")
                {
                    throw new Exception("No server has been setup. Please go to settings");
                }

                string data = await ServerGet("");

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(data);
                var meals = xmlDoc.SelectNodes("//ArrayOfSelectedMeal/SelectedMeal");

                mealData.Clear();
                foreach (XmlNode xmlNode in meals)
                {
                    // <ArrayOfSelectedMeal>
                    // Now populate meals and staples              
                    string date = xmlNode.SelectSingleNode("DateTime").InnerText;
                    var ingredients = xmlNode.SelectNodes("Ingredients/string");
                    var mealsForDate = xmlNode.SelectNodes("Meals/string");
                    var selectedMeal = new SelectedMeal();

                    // Datetime conversion
                    DateTime mealDate;
                    if (DateTime.TryParse(date, out mealDate))
                    {
                        selectedMeal.DateTime = mealDate;
                        if (startDate == DateTime.MinValue || mealDate < startDate)
                        {
                            startDate = mealDate;
                        }
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
                var selectedIngredients = xmlDoc.SelectNodes("//ArrayOfSelectedIngredient/SelectedIngredient");

                ingredientsData.Clear();
                foreach (XmlNode xmlNode in selectedIngredients)
                {
                    string ingredient = xmlNode.SelectSingleNode("Ingredient").InnerText;
                    string date = xmlNode.SelectSingleNode("DateToUse").InnerText;

                    var selectedIngredient = new SelectedIngredient();
                    selectedIngredient.Ingredient = ingredient;
                    selectedIngredient.DateToUse = date;
                    ingredientsData.Add(selectedIngredient);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load data from server: " + ex.Message, "Failed Load", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return startDate;
        }

        /// <summary>
        /// Post the xml string to the webservice on the Raspberry Pi
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public async Task<string> Post(string payload, DateTime listDate)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    if (_serverUrl == "NOSERVER")
                    {
                        throw new Exception("No server has been setup. Please go to settings");
                    }

                    // Key is date of start of list in yyyyMMdd format         
                    var postData = new KeyValuePair<string, string>[]
                    {
                     new KeyValuePair<string, string>(listDate.ToString("yyyyMMdd"), payload)
                    };

                    var content = new FormUrlEncodedContent(postData);

                    var response = await client.PostAsync(_serverUrl, content);

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
                    MessageBox.Show($"Unable to send data to server: {ex.Message}", "Failed Sync", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "";
                }
            }
        }

        public static void PersistStaplesToFile(string staplesPath, StaplesCollection staplesCollection)
        {
            var xs = new XmlSerializer(typeof(StaplesCollection));
            using (StreamWriter wr = new StreamWriter(staplesPath))
            {
                xs.Serialize(wr, staplesCollection);
            }
        }

        public static void PersistMealsToFile(XmlDataProvider xmlDataProvider)
        {
            var uri = new Uri(xmlDataProvider.Source.ToString());
            if (uri.IsFile)
            {
                var path = uri.AbsolutePath;
                xmlDataProvider.Document.Save(path);
            }
        }

        private async Task<string> ServerGet(string request)
        {
            try
            {
                if (_serverUrl == "NOSERVER")
                {
                    throw new Exception("No server has been setup. Please go to settings");
                }

                using (var client = new HttpClient())
                {
                    var data = await client.GetStringAsync(_serverUrl);
                    return data;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Unable to get data from server: {ex.Message}", "Failed Sync", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }
        }
    }
}
