using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        private Point startPoint = new Point();

        /// <summary>
        /// The index of the SelectedMealCollection being dragged
        /// </summary>
        private int index = -1;     

        private PrintDocument m_printDocument = new PrintDocument();

        public MainWindow()
        {
            InitializeComponent();   
            
            m_printDocument.PrintPage += new PrintPageEventHandler(m_printDocument_PrintPage);                  
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
        private void publishMeals()
        {
            SelectedMealCollection colData = (SelectedMealCollection)this.FindResource("SelectedMealCollectionData");
            XmlSerializer xs = new XmlSerializer(typeof(SelectedMealCollection));

            // TODO serialise direct to byte array
            string xml = "";

            using (StringWriter writer = new StringWriter())
            {
                xs.Serialize(writer, colData);
                xml = writer.ToString();
            }

            //http://www.briangrinstead.com/blog/multipart-form-post-in-c
            post( xml );
       
        }

        /// <summary>
        /// Post the xml string to the webservice on the Raspberry Pi
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        private async Task<string> post( string payload )
        {
            
           // string json = "{ \"test\" : \"this\", \"array\" :[ \"as\" : \"b\" } ";
            using (var client = new HttpClient())
            {
                var postData = new KeyValuePair<string, string>[]
                {
                     new KeyValuePair<string, string>("data", payload),
                };

                var content = new FormUrlEncodedContent(postData);

                var response = await client.PostAsync("http://192.168.0.2:8080/index", content);

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
            string localMealsSource = xmlMealDataProvider.Source.ToString();
            xmlMealDataProvider.Document.Save(localMealsSource);

            var xmlStaplesDataProvider = (XmlDataProvider)this.FindResource("StaplesData");
            string localStaplesSource = xmlStaplesDataProvider.Source.ToString();
            xmlStaplesDataProvider.Document.Save(localStaplesSource);

            MessageBox.Show("Saved meals and staples", "Successful save", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private DataGridRow getDataGridRowItem(int index)
        {
            return mealGrid.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow;
        }

        private bool isTheMouseOnTargetRow(Visual theTarget, GetDragDropPosition pos)
        {
            // BUG: If we've expanded the row and collapsed it - this returns true for the wrong row
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
                DataGridRow itm = getDataGridRowItem(i);
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
            index = getDataGridItemCurrentRowIndex(e.GetPosition, staplesGrid);
        }

        private void staplesGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (index < 0) { return; }
            Point currentPoint = e.GetPosition(null);
            Vector diff = startPoint - currentPoint;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {

                var obj = staplesGrid.Items[index] as XmlElement;

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

        /// <summary>
        /// The drop event
        /// </summary>
        private void list_DragDrop(object sender, DragEventArgs e)
        {
            List<string> staples = new List<string>();
            if (e.Data.GetDataPresent("dragMealFormat"))
            {
                foreach (var row in staplesGrid.Items)
                {
                    XmlElement element = row as XmlElement;
                    if (element != null)
                    {
                        staples.Add(element.Attributes["name"].Value);
                    }
                }

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

                foreach (SelectedMeal meal in colData)
                {
                    if (meal.Ingredients == null) {continue;}
                    foreach (string ingredient in meal.Ingredients)
                    {
                        SelectedIngredient selectedIngredient = new SelectedIngredient(ingredient);
                        ingredientData.Add(selectedIngredient);
                    }
                }

                foreach (string staple in staples)
                {
                    SelectedIngredient selectedStaple = new SelectedIngredient(staple);
                    ingredientData.Add(selectedStaple);
                }
            }

            index = -1;
        }

        #endregion

    }
}
