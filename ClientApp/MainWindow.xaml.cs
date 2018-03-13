using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
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
        private Point startPoint = new Point();

        /// <summary>
        /// The index of the SelectedMealCollection being dragged
        /// </summary>
        private int index = -1;

        /// <summary>
        /// The index of the staple being dragged
        /// </summary>
        private int staplesIndex = -1;

        /// <summary>
        /// Start date of current list
        /// </summary>
        private DateTime startDate;

        private PrintDocument m_printDocument = new PrintDocument();

        public MainWindow()
        {
            InitializeComponent();

            SetupXmlDataProviderSources();
            m_printDocument.PrintPage += new PrintPageEventHandler(m_printDocument_PrintPage);                  
        }

        private void SetupXmlDataProviderSources()
        {
            // We need to set the XmlDataProvider to be in the appData folder
            XmlDataProvider mealsXmlDataProvider = FindResource("MealData") as XmlDataProvider;
            XmlDataProvider staplesXmlDataProvider = FindResource("StaplesData") as XmlDataProvider;

            string mealsPath = AppDataPath + "\\SuperMarketPlanner\\SuperMarketDataMeals.xml";
            string staplesPath = AppDataPath + "\\SuperMarketPlanner\\SuperMarketDataStaples.xml";

            // One-time only on startup
            if (!File.Exists(mealsPath))
            {
                FirstTimeAppDataSetup();
            }

            mealsXmlDataProvider.Source = new Uri(mealsPath);
            staplesXmlDataProvider.Source = new Uri(staplesPath);
        }

        private void FirstTimeAppDataSetup(  )
        {
            string xmlSourceDirectory = System.IO.Directory.GetCurrentDirectory() + "\\Data";
            string destinationDirectory = AppDataPath + "\\SuperMarketPlanner";
            Directory.CreateDirectory(destinationDirectory);
            File.Copy(xmlSourceDirectory + "\\SuperMarketDataMeals.xml", destinationDirectory + "\\SuperMarketDataMeals.xml" );
            File.Copy(xmlSourceDirectory + "\\SuperMarketDataStaples.xml", destinationDirectory + "\\SuperMarketDataStaples.xml");
        }

        #region Printing 

        void m_printDocument_PrintPage(object sender, PrintPageEventArgs e)
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
                e.Graphics.DrawString(selectedMeal.Meal, printFont, myBrush, xPos + 20, yPos);

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

        private void printList(object sender, RoutedEventArgs e)
        {
            publishMeals();

            /*
            System.Windows.Forms.PrintPreviewDialog printPreviewDialog1 = new System.Windows.Forms.PrintPreviewDialog(); // instantiate new print preview dialog
            printPreviewDialog1.Document = m_printDocument;        
            printPreviewDialog1.ShowDialog(); // Show the print preview dialog, uses print page event to draw preview screen
            */
            // Configure printer dialog box
            System.Windows.Controls.PrintDialog dlg = new System.Windows.Controls.PrintDialog();
            dlg.PageRangeSelection = PageRangeSelection.AllPages;
            dlg.UserPageRangeEnabled = true;

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results 
            if (result == true)
            {
                // Print document
                m_printDocument.Print();
            }
        }

        public class FileParameter
        {
            public byte[] File { get; set; }
            public string FileName { get; set; }
            public string ContentType { get; set; }
            public FileParameter(byte[] file) : this(file, null) { }
            public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
            public FileParameter(byte[] file, string filename, string contenttype)
            {
                File = file;
                FileName = filename;
                ContentType = contenttype;
            }
        }

        /// <summary>
        /// Publish meal data
        /// </summary>
        private async void publishMeals()
        {
            SelectedMealCollection colData = (SelectedMealCollection)this.FindResource("SelectedMealCollectionData");
            XmlSerializer xs = new XmlSerializer(typeof(SelectedMealCollection));
            StringBuilder xmlBuilder = new StringBuilder();

            var emptyNamepsaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var settings = new XmlWriterSettings();

            settings.OmitXmlDeclaration = true;

            xmlBuilder.AppendLine("<ShoppingList>");

            using (StringWriter stream = new StringWriter())
            {
                using (var writer = XmlWriter.Create(stream, settings))
                {
                    xs.Serialize(writer, colData, emptyNamepsaces);
                    xmlBuilder.Append(stream.ToString());
                }
            }

            SelectedIngredientsCollection shoppingItemsData = (SelectedIngredientsCollection)this.FindResource("SelectedIngredientsCollectionData");
            XmlSerializer xs2 = new XmlSerializer(typeof(SelectedIngredientsCollection));

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
            await post(xmlBuilder.ToString());
        }


        /// <summary>
        /// Post the xml string to the webservice on the Raspberry Pi
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        private async Task<string> post( string payload )
        {          
            using (var client = new HttpClient())
            {
                // Key is date of start of list in yyyyMMdd format         
                var postData = new KeyValuePair<string, string>[]
                {
                     new KeyValuePair<string, string>(startDate.ToString("yyyyMMdd"), payload)
                };
           
                var content = new FormUrlEncodedContent(postData);

                var response = await client.PostAsync("http://192.168.0.202:8080/index", content);

                if (!response.IsSuccessStatusCode)
                {
                    var message = String.Format("Server returned HTTP error {0}: {1}.", (int)response.StatusCode, response.ReasonPhrase);
                    throw new InvalidOperationException(message);
                }

                var data = await response.Content.ReadAsStringAsync();

                return data;
            }
        }

        #endregion

        #region ToolBar handlers

        private void newListClick(object sender, RoutedEventArgs e)
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
            startDate = date;

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
            SelectedMealCollection colData = (SelectedMealCollection)this.FindResource("SelectedMealCollectionData");
            colData.Clear();

            for (int unitIndex = 0; unitIndex < numberOfUnits; unitIndex++)
            {
                var mealDate = new SelectedMeal { Date = date.ToString("dddd MMM dd"), Meal = "" };
                colData.Add(mealDate);
                date = date.AddDays(1);
            }

            SelectedIngredientsCollection ingData = (SelectedIngredientsCollection)this.FindResource("SelectedIngredientsCollectionData");
            ingData.Clear();
        }

        /// <summary>
        /// Saves changes the the meal data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var xmlMealDataProvider = (XmlDataProvider)this.FindResource("MealData");
        
            // TODO : Refactor
            Uri mealUri = new Uri(xmlMealDataProvider.Source.ToString());
            if (mealUri.IsFile)
            {
                string localMealsSource = mealUri.AbsolutePath;
                xmlMealDataProvider.Document.Save(localMealsSource);
            }

            var xmlStaplesDataProvider = (XmlDataProvider)this.FindResource("StaplesData");
            Uri staplesUri = new Uri(xmlStaplesDataProvider.Source.ToString());
            if (staplesUri.IsFile)
            {
                string staplesSource = staplesUri.AbsolutePath;
                xmlStaplesDataProvider.Document.Save(staplesSource);
            }

             MessageBox.Show("Saved meals and staples", "Successful save", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void mealGrid_RowDoubleClick(object sender, RoutedEventArgs e)
        {
            var row = (DataGridRow)sender;
            row.DetailsVisibility = row.DetailsVisibility == System.Windows.Visibility.Collapsed ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            // When we collapse the details view we need to refresh the grid, otherwise the rows beneath will still refer to the expanded item
            // This is essentially a resizing problem
            if (row.DetailsVisibility == Visibility.Collapsed)
            {
                mealGrid.CommitEdit(DataGridEditingUnit.Row, true);
                mealGrid.Items.Refresh();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            XmlNode obj = ((FrameworkElement)sender).DataContext as XmlNode;
            XmlNode parent = obj.ParentNode;
            parent.RemoveChild(obj);
        }

        private void ButtonClick_AddNewIngredient(object sender, RoutedEventArgs e)
        {
            //TODO: Robust checks
            StackPanel obj = ((FrameworkElement)sender).Parent as StackPanel;
            var x = (DataGrid)obj.Children[0];
            var xmlDataProvider = (XmlDataProvider)this.FindResource("MealData");
            var newElement = xmlDataProvider.Document.CreateElement("Ingredient");

            // We can't change the Items collection directly, so we need to find the parent XmlNode and append the child to that
            XmlNode parent = ((XmlElement)x.Items[0]).ParentNode;
            parent.AppendChild(newElement);
        }

        private void ButtonClick_AddNewStaple(object sender, RoutedEventArgs e)
        {
            var xmlDataProvider = (XmlDataProvider)this.FindResource("StaplesData");
            var newElement = xmlDataProvider.Document.CreateElement("Staple");
            XmlAttribute attribute = xmlDataProvider.Document.CreateAttribute("name");
            newElement.Attributes.Append(attribute);

            // We can't change the Items collection directly, so we need to find the parent XmlNode and append the child to that
            xmlDataProvider.Document.SelectSingleNode("//Staples").AppendChild(newElement);
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
        private DataGridRow getDataGridRowItem(DataGrid inputGrid, int index)
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
        private int getDataGridItemCurrentRowIndex(GetDragDropPosition pos, DataGrid inputGrid)
        {
            int curIndex = -1;
            for (int i = 0; i < inputGrid.Items.Count; i++)
            {
                DataGridRow itm = getDataGridRowItem(inputGrid, i);
                if (isTheMouseOnTargetRow(itm, pos))
                {
                    curIndex = i;
                    break;
                }
            }

            return curIndex;
        }

        private void mealGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            startPoint = e.GetPosition(null);
            index = getDataGridItemCurrentRowIndex(e.GetPosition, mealGrid);
        }

        private void staplesGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            startPoint = e.GetPosition(null);
            staplesIndex = getDataGridItemCurrentRowIndex(e.GetPosition, staplesGrid);
        }

        private void staplesGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (staplesIndex < 0) { return; }
            Point currentPoint = e.GetPosition(null);
            Vector diff = startPoint - currentPoint;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {

                var obj = staplesGrid.Items[staplesIndex] as XmlElement;

                // Initialise the drag & drop operation
                DataObject dragData = new DataObject("dragStapleFormat", obj);
                DragDrop.DoDragDrop(staplesGrid, dragData, DragDropEffects.Copy);
            }
        }

        private void mealGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (index < 0) { return; }
            Point currentPoint = e.GetPosition(null);
            Vector diff = startPoint - currentPoint;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {

                var obj = mealGrid.Items[index] as XmlElement;
                // Initialise the drag & drop operation
                DataObject dragData = new DataObject("dragMealFormat", obj);
                DragDrop.DoDragDrop(mealGrid, dragData, DragDropEffects.Copy);
            }
        }

        /// <summary>
        /// Event called when drag enters a drop region
        /// </summary>
        /// <param name="sender">This will be the control under the mouse</param>
        private void list_DragEnter(object sender, DragEventArgs e)
        {   
            if (!e.Data.GetDataPresent("dragMealFormat"))
            {   
                e.Effects = DragDropEffects.None;
            }
        }

        private void allItems_DragEnter(object sender, DragEventArgs e)
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

        private void allItems_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("dragStapleFormat"))
            {
                SelectedIngredientsCollection ingredientData = (SelectedIngredientsCollection)this.FindResource("SelectedIngredientsCollectionData");
                var stapleElement = e.Data.GetData("dragStapleFormat") as XmlElement;
                String stapleName = stapleElement.Attributes.GetNamedItem("name").Value;
                SelectedIngredient staple = new SelectedIngredient(stapleName, "");
                ingredientData.Add(staple);
            }
        }

        /// <summary>
        /// The drop event
        /// </summary>
        private void list_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("dragMealFormat"))
            {
                // sender is itemsControl 
                // Update the table data
                int updateIndex = DecideDropTarget(e, (ItemsControl)sender);
                XmlElement xElement = e.Data.GetData("dragMealFormat") as XmlElement;
                SelectedMealCollection colData = (SelectedMealCollection)this.FindResource("SelectedMealCollectionData");
                SelectedMeal mealToUpdate = colData[updateIndex];
                mealToUpdate.Meal = xElement.GetAttribute("name");

                if (mealToUpdate.Ingredients == null)
                {
                    mealToUpdate.Ingredients = new List<string>();
                }
                else
                {
                    mealToUpdate.Ingredients.Clear();
                }

                foreach (XmlNode ingredientNode in xElement.SelectNodes("Ingredients/Ingredient"))
                {
                    mealToUpdate.Ingredients.Add(ingredientNode.InnerText);
                }

                // Refresh the selected ingredient view
                SelectedIngredientsCollection ingredientData = (SelectedIngredientsCollection)this.FindResource("SelectedIngredientsCollectionData");
                ingredientData.Clear();

                string pattern = "dddd MMM dd";
                foreach (SelectedMeal meal in colData)
                {
                    DateTime parsedDate = DateTime.Now;
                    DateTime.TryParseExact(meal.Date, pattern, null, System.Globalization.DateTimeStyles.None, out parsedDate);

                    if (meal.Ingredients == null) {continue;}
                    foreach (string ingredient in meal.Ingredients)
                    {                   
                        SelectedIngredient selectedIngredient = new SelectedIngredient(ingredient, parsedDate.ToString("yyyy-MM-dd"));
                        ingredientData.Add(selectedIngredient);
                    }
                }
            }

            index = -1;
        }

        #endregion
    }
}
