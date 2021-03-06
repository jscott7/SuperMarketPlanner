﻿using System;
using System.Windows;

namespace SuperMarketPlanner
{
    /// <summary>
    /// Interaction logic for DateDialog.xaml
    /// </summary>
    public partial class DateDialog : Window
    {
        public DateDialog()
        {
            InitializeComponent();
            calendar.SelectedDate = DateTime.Today;
        }

        public DateTime SelectedDate
        {
            get {return calendar.SelectedDate.Value;}
        }

        public int NumberOfUnits
        {
            get
            {
                int num = 0;
                int.TryParse(chosenNumber.Text, out num);
                return num;
            }
        }

        public UnitsEnum UnitSize
        {
            get
            {
                return (UnitsEnum)Enum.Parse(typeof(UnitsEnum), units.SelectionBoxItem.ToString());
            }
        }

        private void okButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void cancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
