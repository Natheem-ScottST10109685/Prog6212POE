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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ST10109685_PROG6212POE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Event handler for "Submit New Claim" button click
        private void BtnSubmitClaims_Click(object sender, RoutedEventArgs e)
        {
            SubmitClaim submitClaimWindow = new SubmitClaim();
            submitClaimWindow.Show();

            this.Close();
        }

        private void BtnViewClaims_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnSubmitClaims1_Click(object sender, RoutedEventArgs e)
        {
            SubmitClaim submitClaimWindow = new SubmitClaim();
            submitClaimWindow.Show();

            this.Close();
        }

        private void BtnView_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnReports_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
