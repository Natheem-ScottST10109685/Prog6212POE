using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ST10109685_PROG6212POE
{
    /// <summary>
    /// Interaction logic for SubmitClaim.xaml
    /// </summary>
    public partial class SubmitClaim : Window
    {
        // Stores the file path selected by the user
        private string selectedFilePath = "";

        /// <summary>
        /// Initializes the SubmitClaim window.
        /// </summary>
        public SubmitClaim()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Submit button click event.
        /// Ensures that a file is selected and copies the file to the secure storage system.
        /// </summary>
        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                MessageBox.Show("Please upload a supporting document.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Define a secure storage location (e.g., local folder or cloud storage)
            string destinationFolder = "C:\\Users\\Natheem Scott\\Desktop\\2ndyear\\New Content\\PROG2B\\FileStorage"; // Adjust this to your storage system
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder); // Create folder if it doesn't exist
            }

            // Get the file name and destination path
            string fileName = System.IO.Path.GetFileName(selectedFilePath);
            string destinationPath = System.IO.Path.Combine(destinationFolder, fileName);

            try
            {
                // Copy the file to the storage system
                File.Copy(selectedFilePath, destinationPath, true);

                // After storing the file, you can save the file path in your system's database
                MessageBox.Show("Claim submitted successfully with the supporting document.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error storing the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Clears the form fields and resets the file selection.
        /// </summary>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            txtContractID.Clear();
            cmbClaimPeriod.SelectedIndex = -1;
            txtClaimAmount.Clear();
            txtHoursWorked.Clear();
            txtHourlyRate.Clear();
            txtAdditionalNotes.Clear();
            txtSelectedFileName.Text = "No file selected";
            selectedFilePath = "";
        }

        /// <summary>
        /// Handles the Cancel button click event.
        /// Closes the SubmitClaim window and reopens the MainWindow.
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Reopen the MainWindow by creating a new instance
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            // Close the SubmitClaim window
            this.Close();
        }

        /// <summary>
        /// Handles the Browse button click event.
        /// Opens a file dialog for the user to select a document, checks the file size, and updates the form.
        /// </summary>
        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            // Open file dialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set allowed file types
            openFileDialog.Filter = "Documents (*.pdf;*.docx;*.xlsx)|*.pdf;*.docx;*.xlsx";

            if (openFileDialog.ShowDialog() == true)
            {
                selectedFilePath = openFileDialog.FileName;

                // Check file size (limit to 5 MB)
                FileInfo fileInfo = new FileInfo(selectedFilePath);
                if (fileInfo.Length > 5 * 1024 * 1024) // 5 MB size limit
                {
                    MessageBox.Show("File size exceeds the limit of 5 MB.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    selectedFilePath = ""; // Reset the file path if invalid
                    return;
                }

                // Display the file name on the form
                txtSelectedFileName.Text = fileInfo.Name;
            }
        }
    }
}
