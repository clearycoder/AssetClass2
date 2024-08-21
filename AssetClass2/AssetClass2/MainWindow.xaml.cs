using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using System.DirectoryServices.ActiveDirectory;
using System.Windows.Input;

namespace AssetClass2
{
    public partial class MainWindow : Window // code behind class for window that users will interact with
    {
        public AssetClassRepository _assetClassRepository;
        private string _connectionString;

        
        



        public MainWindow()
        {
            InitializeComponent(); // used for initializing interaction of window C# and XAML code
            _connectionString = App.ConnectionString;
            _assetClassRepository = new AssetClassRepository(_connectionString);

            LoadAssetClasses();
            PopulateDropdowns();

            IndexDropDown.SelectionChanged += DropDown_SelectionChanged;
            YearDropDown.SelectionChanged += DropDown_SelectionChanged;
            
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) // code for allowing the user to drag the app window 
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }


        private void CloseWindow(object sender, RoutedEventArgs e) // code for window close button for users
        {
            this.Close();
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e) // code for window minimize button for users
        {
            this.WindowState = WindowState.Minimized;
        }

        
        public void LoadAssetClasses() // method that uses repository to establish objects with information from database
        {
            var largeGrowth = _assetClassRepository.GetLargeGrowthInfo();
            var smallValue = _assetClassRepository.GetSmallValueInfo();
            var totalBond = _assetClassRepository.GetTotalBondInfo();
        }

        public void PopulateDropdowns() // method used for data users will see when choosing from comboboxes
        {
            IndexDropDown.ItemsSource = new List<string> { "US Large Growth", "US Small Value", "US Total Bond" };
            YearDropDown.ItemsSource = Enumerable.Range(1, 20).ToList();
        }

        public void DropDown_SelectionChanged(object sender, SelectionChangedEventArgs e) //method used to process changes from user selections
        {
            UpdateResults();
        }



        public void UpdateResults() // method used to display appropriate data to user by using repository and calculator classes
        {
            string selectedIndex = IndexDropDown.SelectedItem as string;
            int? selectedYear = YearDropDown.SelectedItem as int?;
            if (string.IsNullOrEmpty(selectedIndex) || !selectedYear.HasValue)
            {
                return;
            }
            string tableName = GetTableName(selectedIndex);
            if (string.IsNullOrEmpty(tableName))
            {
                MessageBox.Show("Please select a valid index.");
                return;
            }
            try
            {
                var response = _assetClassRepository.RollingAverage(tableName, selectedYear);
                if (response != null && response.Count() == 3)
                {
                    var results = AssetClassCalculator.CalculateInvestmentResults(response, selectedYear.Value);
                    Lowest1000.Text = results.Lowest.ToString("C2");
                    Mean1000.Text = results.Average.ToString("C2");
                    Highest1000.Text = results.Highest.ToString("C2");
                }
                else
                {
                    MessageBox.Show("No data found for the selected criteria.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        public string GetTableName(string index) //method used for appropriate format return of index for above calculations
        {
            switch (index)
            {
                case "US Large Growth":
                    return "uslargegrowth";
                case "US Small Value":
                    return "ussmallvalue";
                case "US Total Bond":
                    return "ustotalbond";
                default:
                    return string.Empty;
            }
        }

        
    }
}