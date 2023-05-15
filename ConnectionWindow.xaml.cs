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

namespace XSD_Mapper
{
    /// <summary>
    /// Lógica de interacción para ConnectionWindow.xaml
    /// </summary>
    public partial class ConnectionWindow : Window
    {
        public ConnectionWindow()
        {
            InitializeComponent();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            var serverName = ServerName.Text;
            var databaseName = DatabaseName.Text;
            var userName = UserName.Text;
            var password = Password.Password;

            serverName = "SRVSAGE\\SAGEEXPRESS";
            databaseName = "MBVic";
            userName = "logic";
            password = "Sage2009+";

            if (string.IsNullOrWhiteSpace(serverName) || string.IsNullOrWhiteSpace(databaseName) || string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter all fields to connect to the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Config.ConnectionString = $"Server={serverName.Trim()};Database={databaseName.Trim()};User ID={userName.Trim()};Password={password.Trim()};Encrypt=False";

            // close window after
            this.DialogResult = true;
            this.Close();
        }
    }


}
