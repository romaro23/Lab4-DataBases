using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Lab4
{
    /// <summary>
    /// Interaction logic for QueriesWindow.xaml
    /// </summary>
    public partial class QueriesWindow : Window
    {
        public string? Query { get; set; }
        public List<string> Queries { get; set; }
        public QueriesWindow()
        {
            InitializeComponent();
        }

        public QueriesWindow(List<string> queries) : this()
        {
            Queries = queries.ToList();
            QueriesComboBox.ItemsSource = Queries;
        }
 
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (QueriesComboBox.SelectedItem == null)
            {
                MessageBox.Show("Будь ласка, заповніть всі поля.");
                return;
            }
            Query = QueriesComboBox.SelectedItem.ToString();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
