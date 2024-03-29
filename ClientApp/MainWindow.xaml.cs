﻿using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SuperMarketPlanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {  
        /// <summary>
        /// Start point for drag drop. Used to determine if we hold the mouse and move far enough to initiate drag
        /// </summary>
        private Point _startPoint = new Point();

        /// <summary>
        /// The index of the SelectedMealCollection being dragged
        /// </summary>
        private int _index = -1;

        /// <summary>
        /// The index of the staple being dragged
        /// </summary>
        private int _staplesIndex = -1;

        /// <summary>
        /// Start date of current list
        /// </summary>
        private DateTime _startDate;

        private PrintDocument _printDocument = new PrintDocument();

        private Persist _persist;

        public MainWindow()
        {
            InitializeComponent();

            SetupXmlDataProviderSources();
            _printDocument.PrintPage += new PrintPageEventHandler(PrintPage);

            CreateServerConnection();
        }

        private void CreateServerConnection()
        {
            var serverUrl = SettingsUtilities.LoadSetting("serverurl");
            if (serverUrl != null)
            {
                _persist = new Persist(serverUrl.ToString());
            }
            else
            {
                _persist = new Persist("NOSERVER");
            }
        }

        private void SetupXmlDataProviderSources()
        {
            // We need to set the XmlDataProvider to be in the appData folder
            var mealsXmlDataProvider = FindResource("MealData") as XmlDataProvider;

            // TODO load from rest service
            string mealsPath = AppDataPath + "\\SuperMarketPlanner\\SuperMarketDataMeals.xml";
            string staplesPath = AppDataPath + "\\SuperMarketPlanner\\SuperMarketDataStaples.xml";
         
            // One-time only on startup
            if (!File.Exists(mealsPath))
            {
                FirstTimeAppDataSetup();
            }

            // Read staples data using Linq
            var xDocument = XDocument.Load(staplesPath);
            IEnumerable<string> staples = from x in xDocument.Descendants("StapleItem")
                                          select (string)x.Attribute("name").Value;

            // Backward compatibility with old format
            if (staples.Count() == 0)
            {
                staples = from x in xDocument.Descendants("Staple")
                          select (string)x.Attribute("name").Value;
            }

            // For sorted meals we need create a new MealItem and MealsCollection
            // Populate it from the meals path persisted xml
            // var mealsDocument = XDocument.Load(mealsPath);

            var sortedStaples = staples.ToList();
            sortedStaples.Sort();
            var staplesCollection = (StaplesCollection)this.FindResource("StaplesCollectionData");

            foreach(var staple in sortedStaples)
            {
                staplesCollection.Add(new StapleItem() { Staple = staple });
            }

            mealsXmlDataProvider.Source = new Uri(mealsPath);
        }

        /// <summary>
        /// Copies required data files to User Application Data folder
        /// </summary>
        private void FirstTimeAppDataSetup()
        {
            var xmlSourceDirectory = Path.Combine(Directory.GetCurrentDirectory(), "FirstTimeData");
            var destinationDirectory = Path.Combine(AppDataPath, "SuperMarketPlanner");

            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            File.Copy(Path.Combine(xmlSourceDirectory, "SuperMarketDataMeals.xml"), Path.Combine(destinationDirectory, "SuperMarketDataMeals.xml") );
            File.Copy(Path.Combine(xmlSourceDirectory, "SuperMarketDataStaples.xml"), Path.Combine(destinationDirectory, "SuperMarketDataStaples.xml") );
        }

        #region Printing 

        void PrintPage(object sender, PrintPageEventArgs e)
        {
            SelectedMealCollection colData = (SelectedMealCollection)this.FindResource("SelectedMealCollectionData");
            float leftMargin = e.MarginBounds.Left;
            float rightMargin = e.MarginBounds.Right;
            float topMargin = e.MarginBounds.Top;
          
            float yPos = leftMargin;
            float xPos = 0;
            float centrePos = 2 * (rightMargin - leftMargin) / 5;
            float rightPos =  4 * (rightMargin - leftMargin) / 5;
            int count = 0;

            System.Drawing.Font printFont = new System.Drawing.Font("Arial", 10);
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);

            // Work out the number of lines per page, using the MarginBounds.
            float linesPerPage = e.MarginBounds.Height / printFont.GetHeight(e.Graphics);
            float lineCount = 0;
            foreach (var selectedMeal in colData)
            {
                // calculate the next line position based on the height of the font according to the printing device
                yPos = topMargin + (count++ * printFont.GetHeight(e.Graphics));
                e.Graphics.DrawString(selectedMeal.Date, printFont, myBrush, xPos, yPos);
                lineCount++;
   
                yPos = topMargin + (count++ * printFont.GetHeight(e.Graphics));

                foreach (string meal in selectedMeal.Meals)
                {
                    e.Graphics.DrawString(meal, printFont, myBrush, xPos + 20, yPos);
                }

                lineCount++;

                if (lineCount > linesPerPage) { break; }
            }

            lineCount = 0;
            count = 0;

            SelectedIngredientsCollection ingredientData = (SelectedIngredientsCollection)this.FindResource("SelectedIngredientsCollectionData");

            foreach (SelectedIngredient ingredient in ingredientData)
            {               
                // calculate the next line position based on the height of the font according to the printing device
                yPos = topMargin + (count++ * printFont.GetHeight(e.Graphics));
                xPos = centrePos;
                if (lineCount > linesPerPage) xPos = rightPos;
                e.Graphics.DrawString(ingredient.Ingredient, printFont, myBrush, xPos, yPos);
                lineCount++;
            }

            // If there are more lines, print another page.
            if (lineCount > linesPerPage)
            {
           //     e.HasMorePages = true;
            }
            else
            {
                e.HasMorePages = false;
                myBrush.Dispose();
            }
        }

        private void PrintList(object sender, RoutedEventArgs e)
        {
             // Configure printer dialog box
            var dlg = new PrintDialog();
            dlg.PageRangeSelection = PageRangeSelection.AllPages;
            dlg.UserPageRangeEnabled = true;

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results 
            if (result == true)
            {
                // Print document
                _printDocument.Print();
            }
        }

        /// <summary>
        /// Publish meal data to server
        /// </summary>
        private async void PublishMeals()
        {
            var colData = (SelectedMealCollection)this.FindResource("SelectedMealCollectionData");
            var xs = new XmlSerializer(typeof(SelectedMealCollection));
            var xmlBuilder = new StringBuilder();

            var emptyNamepsaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var settings = new XmlWriterSettings();

            settings.OmitXmlDeclaration = true;
        
            xmlBuilder.Append("<ShoppingList update=\"")
                .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .AppendLine("\">");

            using (StringWriter stream = new StringWriter())
            {
                using (var writer = XmlWriter.Create(stream, settings))
                {
                    xs.Serialize(writer, colData, emptyNamepsaces);
                    xmlBuilder.Append(stream.ToString());
                }
            }

            var shoppingItemsData = (SelectedIngredientsCollection)this.FindResource("SelectedIngredientsCollectionData");
            var xs2 = new XmlSerializer(typeof(SelectedIngredientsCollection));

            using (StringWriter stream = new StringWriter())
            {
                using (var writer = XmlWriter.Create(stream, settings))
                {
                    xs2.Serialize(writer, shoppingItemsData, emptyNamepsaces);
                    xmlBuilder.Append(stream.ToString());
                }
            }

            xmlBuilder.AppendLine("</ShoppingList>");

            //http://www.briangrinstead.com/blog/multipart-form-post-in-c
            await _persist.Post("index", xmlBuilder.ToString(), _startDate);
        }

        #endregion

        #region ToolBar handlers

        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            var mealData = (SelectedMealCollection)this.FindResource("SelectedMealCollectionData");
            var ingredientsData = (SelectedIngredientsCollection)this.FindResource("SelectedIngredientsCollectionData");

            _startDate = await _persist.LoadLatest("index", mealData, ingredientsData);    
        }

        private void NewList_Click(object sender, RoutedEventArgs e)
        {
            // Instantiate the dialog box
            DateDialog dlg = new DateDialog();

            // Configure the dialog box
            dlg.Owner = this;

            // Open the dialog box modally 
            dlg.ShowDialog();

            if (dlg.DialogResult.Value == false)
            {
                return;
            }

            DateTime date = dlg.SelectedDate;
            _startDate = date;

            int numberOfUnits = dlg.NumberOfUnits;
            UnitsEnum unitSize = dlg.UnitSize;

            if (unitSize == UnitsEnum.Weeks)
            {
                numberOfUnits *= 7;
            }

            if (unitSize == UnitsEnum.Months)
            {
                numberOfUnits *= 30;
            }

            // Clean and add new selected items
            var colData = (SelectedMealCollection)this.FindResource("SelectedMealCollectionData");
            colData.Clear();

            for (int unitIndex = 0; unitIndex < numberOfUnits; unitIndex++)
            {
                var mealDate = new SelectedMeal { DateTime = date };
                colData.Add(mealDate);
                date = date.AddDays(1);
            }

            var ingData = (SelectedIngredientsCollection)this.FindResource("SelectedIngredientsCollectionData");
            ingData.Clear();
        }

        /// <summary>
        /// Saves changes the the meal data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var xmlMealDataProvider = (XmlDataProvider)this.FindResource("MealData");
                Persist.PersistMealsToFile(xmlMealDataProvider);

                var staplesCollection = (StaplesCollection)this.FindResource("StaplesCollectionData");
                string staplesPath = AppDataPath + "\\SuperMarketPlanner\\SuperMarketDataStaples.xml";
             
                _persist.PersistStaplesToFile(staplesPath, staplesCollection);

                MessageBox.Show("Saved meals and staples", "Successful save", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Can't save meals and staples: {ex.Message}", "Failed save", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Sync_Click(object sender, RoutedEventArgs e)
        {
            PublishMeals();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsDialog = new Settings();
            if (settingsDialog.ShowDialog() == true)
            {
                CreateServerConnection();
            }
        }

        public string AppDataPath
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); }
        }

        #endregion

        #region Editing Meals

        /// <summary>
        /// Handler for DataGridRow Mouse double click
        /// </summary>
        /// <param name="sender">The DataGridRow</param>
        /// <param name="e"></param>
        /// <remarks>Toggles the DetailsVisiblity from Collapsed to Visible</remarks>
        private void MealGrid_RowDoubleClick(object sender, RoutedEventArgs e)
        {
            var row = (DataGridRow)sender;
            row.DetailsVisibility = row.DetailsVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

            // When we collapse the details view we need to refresh the grid, otherwise the rows beneath will still refer to the expanded item
            // This is essentially a resizing problem
            if (row.DetailsVisibility == Visibility.Collapsed)
            {
                mealGrid.CommitEdit(DataGridEditingUnit.Row, true);
                mealGrid.Items.Refresh();
            }
        }

        private void ButtonClick_DeleteStaple(object sender, RoutedEventArgs e)
        {
            var staplesCollection = (StaplesCollection)this.FindResource("StaplesCollectionData");
            var stapleItem =  ((FrameworkElement)sender).DataContext as StapleItem;
            staplesCollection.Remove(stapleItem);

        }

        private void ButtonClick_DeleteMeal(object sender, RoutedEventArgs e)
        {
            var selectedMeal = ((FrameworkElement)sender).DataContext as SelectedMeal;
            var ingredientData = (SelectedIngredientsCollection)this.FindResource("SelectedIngredientsCollectionData");

            foreach( string ingredient in selectedMeal.Ingredients)
            {
                string date = selectedMeal.DateTime.ToString("yyyy-MM-dd");
                ingredientData.Remove(new SelectedIngredient(ingredient, date));
            }

            selectedMeal.Clear();
        }

        /// <summary>
        /// Handler for removing a meal from the main source list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClick_DeleteMealFromSourceList(object sender, RoutedEventArgs e)
        {
            var item = ((FrameworkElement)sender).DataContext as XmlNode;
            XmlNode parent = ((XmlElement)mealGrid.Items[0]).ParentNode;
            parent.RemoveChild(item);
        }

        private void ButtonClick_AddNewIngredient(object sender, RoutedEventArgs e)
        {
            var obj = ((FrameworkElement)sender).Parent as StackPanel;
            var ingredientGrid = (DataGrid)obj.Children[0];
            var xmlDataProvider = (XmlDataProvider)this.FindResource("MealData");
            var newElement = xmlDataProvider.Document.CreateElement("Ingredient");

            AppendChildToDataGrid(ingredientGrid, newElement);
        }

        private void ButtonClick_AddNewMeal(object sender, RoutedEventArgs e)
        {
            var newMealDialog = new NewMealDialog();
            if (newMealDialog.ShowDialog() == true)
            {
                var newMeal = newMealDialog.NewMeal;
                var xmlDataProvider = (XmlDataProvider)this.FindResource("MealData");

                var newMealElement = xmlDataProvider.Document.CreateElement("Meal");
                var ingredientsElement = xmlDataProvider.Document.CreateElement("Ingredients");        
                var ingredientElement = xmlDataProvider.Document.CreateElement("Ingredient");
                ingredientsElement.AppendChild(ingredientElement);
                newMealElement.AppendChild(ingredientsElement);
 
                var  nameAttribute = xmlDataProvider.Document.CreateAttribute("name");
                nameAttribute.Value = newMeal;
                newMealElement.Attributes.Append(nameAttribute);

                AppendChildToDataGrid(mealGrid, newMealElement);
            }
        }

        /// <summary>
        /// We can't change the Items collection directly on a DataGrid, so we need to find the parent XmlNode and append the xmlnode to that
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="newElement"></param>
        private void AppendChildToDataGrid(DataGrid grid, XmlNode newElement)
        {
            if (!grid.Items.IsEmpty)
            {
                var parent = ((XmlElement)grid.Items[0]).ParentNode;
                parent.AppendChild(newElement);
            }
        }

        #endregion

        #region Drag Drop code

        /// <summary>
        /// Delegate that will return the position of the DragDropEventArgs and the MouseButtonEventArgs event object
        /// </summary>
        public delegate Point GetDragDropPosition(IInputElement theElement);

        /// <summary>
        /// Returns the row of the DataGrid that's been clicked on in the Drag event
        /// </summary>
        private DataGridRow GetDataGridRowItem(DataGrid inputGrid, int index)
        {
            return inputGrid.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow;
        }

        private bool isTheMouseOnTargetRow(Visual theTarget, GetDragDropPosition pos)
        {
            if (theTarget == null)
            {
                return false;
            }

            Rect posBounds = VisualTreeHelper.GetDescendantBounds(theTarget);
            Point theMousePos = pos((IInputElement)theTarget);
            return posBounds.Contains(theMousePos);
        }

        /// <summary>
        /// Gets the index of the DataGrid Rod under the drag mouse click
        /// </summary>
        private int GetDataGridItemCurrentRowIndex(GetDragDropPosition pos, DataGrid inputGrid)
        {
            int curIndex = -1;
            for (int i = 0; i < inputGrid.Items.Count; i++)
            {
                DataGridRow itm = GetDataGridRowItem(inputGrid, i);
                if (isTheMouseOnTargetRow(itm, pos))
                {
                    curIndex = i;
                    break;
                }
            }

            return curIndex;
        }

        private void MealGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            _startPoint = e.GetPosition(null);
            _index = GetDataGridItemCurrentRowIndex(e.GetPosition, mealGrid);
        }

        private void StaplesGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            _startPoint = e.GetPosition(null);
            _staplesIndex = GetDataGridItemCurrentRowIndex(e.GetPosition, staplesGrid);
        }

        private void StaplesGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (_staplesIndex < 0) { return; }
            Point currentPoint = e.GetPosition(null);
            Vector diff = _startPoint - currentPoint;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {

                var obj = staplesGrid.Items[_staplesIndex] as StapleItem;

                // Initialise the drag & drop operation
                DataObject dragData = new DataObject("dragStapleFormat", obj);
                DragDrop.DoDragDrop(staplesGrid, dragData, DragDropEffects.Copy);
            }
        }

        private void MealGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (_index < 0) { return; }
            Point currentPoint = e.GetPosition(null);
            Vector diff = _startPoint - currentPoint;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {

                var obj = mealGrid.Items[_index] as XmlElement;
                // Initialise the drag & drop operation
                DataObject dragData = new DataObject("dragMealFormat", obj);
                DragDrop.DoDragDrop(mealGrid, dragData, DragDropEffects.Copy);
            }
        }

        /// <summary>
        /// Event called when drag enters a drop region
        /// </summary>
        /// <param name="sender">This will be the control under the mouse</param>
        private void List_DragEnter(object sender, DragEventArgs e)
        {   
            if (!e.Data.GetDataPresent("dragMealFormat"))
            {   
                e.Effects = DragDropEffects.None;
            }
        }

        private void AllItems_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("dragStapleFormat"))
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private int DecideDropTarget(DragEventArgs e, ItemsControl targetItemsControl)
        {
            int targetItemsControlCount = targetItemsControl.Items.Count;
            DependencyObject targetItemContainer;
            int insertionIndex = -1;

            if (targetItemsControlCount > 0)
            {
                targetItemContainer = targetItemsControl.ContainerFromElement((DependencyObject)e.OriginalSource) as FrameworkElement;

                if (targetItemContainer != null)
                {
                     insertionIndex = targetItemsControl.ItemContainerGenerator.IndexFromContainer(targetItemContainer);
                }
                else
                {
                    targetItemContainer = targetItemsControl.ItemContainerGenerator.ContainerFromIndex(targetItemsControlCount - 1) as FrameworkElement;
                    insertionIndex = targetItemsControlCount;
                }
            }
            else
            {
                targetItemContainer = null;
                insertionIndex = 0;
            }

            return insertionIndex;
        }

        private void AllItems_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("dragStapleFormat"))
            {
                var ingredientData = (SelectedIngredientsCollection)this.FindResource("SelectedIngredientsCollectionData");
                var stapleElement = e.Data.GetData("dragStapleFormat") as StapleItem;
                string stapleName = stapleElement.Staple;
                var staple = new SelectedIngredient(stapleName, "");
                ingredientData.Add(staple);
            }
        }

        /// <summary>
        /// The drop event for adding a meal to the Daily List
        /// </summary>
        private void List_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("dragMealFormat"))
            {
                // sender is itemsControl 
                // Update the table data
                int updateIndex = DecideDropTarget(e, (ItemsControl)sender);
                var xElement = e.Data.GetData("dragMealFormat") as XmlElement;
                var colData = (SelectedMealCollection)this.FindResource("SelectedMealCollectionData");
                SelectedMeal mealToUpdate = colData[updateIndex];

                mealToUpdate.addMeal(xElement.GetAttribute("name"));

                if (mealToUpdate.Ingredients == null)
                {
                    mealToUpdate.Ingredients = new List<string>();
                }

                foreach (XmlNode ingredientNode in xElement.SelectNodes("Ingredients/Ingredient"))
                {
                    mealToUpdate.Ingredients.Add(ingredientNode.InnerText);
                }

                // Update the selected ingredient view
                var ingredientData = (SelectedIngredientsCollection)this.FindResource("SelectedIngredientsCollectionData");
                if (mealToUpdate.Ingredients != null)
                {
                    foreach (string ingredient in mealToUpdate.Ingredients)
                    {
                        var selectedIngredient = new SelectedIngredient(ingredient, mealToUpdate.DateTime.ToString("yyyy-MM-dd"));
                        if (!ingredientData.Contains(selectedIngredient))
                        {
                            ingredientData.Add(selectedIngredient);
                        }
                    }
                }
            }

            _index = -1;
        }

        #endregion
    }
}
