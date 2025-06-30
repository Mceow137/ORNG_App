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

namespace ORNG_App
{
    /// <summary>
    /// Логика взаимодействия для ViewDocumentWindow.xaml
    /// </summary>
    public partial class ViewDocumentWindow : Window
    {
        private GasActivationDocument _document;

        public ViewDocumentWindow(GasActivationDocument document)
        {
            InitializeComponent();
            _document = document;
            DisplayDocumentInfo();
        }

        private void DisplayDocumentInfo()
        {
            txtDocNumber.Text = _document.DocumentNumber;
            txtAddress.Text = _document.Address;
            txtMeterNumber.Text = _document.MeterNumber;
            txtInitialReading.Text = _document.InitialMeterReading.ToString("N2");
            txtActivationDate.Text = _document.ActivationDate.ToString("dd.MM.yyyy");

            if (_document.DocumentScan == null || _document.DocumentScan.Length == 0)
            {
                btnViewScan.IsEnabled = false;
                btnViewScan.Content = "Скан документа отсутствует";
            }
        }

        private void btnViewScan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"GasActivation_{_document.DocumentNumber}.pdf");
                DatabaseHelper.SaveImageFromByteArray(_document.DocumentScan, tempFilePath);
                System.Diagnostics.Process.Start(tempFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть документ: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
