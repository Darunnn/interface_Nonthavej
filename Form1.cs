using System;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;
using interface_Nonthavej.Configuration;

namespace interface_Nonthavej
{
    public partial class Form1 : Form
    {
        private AppConfig _config;

        public Form1()
        {
            InitializeComponent();
            InitializeApp();
        }

        private void InitializeApp()
        {
            try
            {
                // Load configuration
                _config = new AppConfig();
                _config.LoadConfiguration();

                // Test database connection
                TestDatabaseConnection();
            }
            catch (Exception ex)
            {
                connectionStatusLabel.Text = "Database: ? Failed to load config";
                connectionStatusLabel.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void TestDatabaseConnection()
        {
            SqlConnection connection = null;

            try
            {
                connectionStatusLabel.Text = "Database: ?? Connecting...";
                connectionStatusLabel.ForeColor = System.Drawing.Color.Orange;
                Application.DoEvents();

                // Create and open connection
                connection = new SqlConnection(_config.ConnectionString);
                connection.Open();

                // Connection successful
                connectionStatusLabel.Text = $"Database: ? Connected ({connection.Database})";
                connectionStatusLabel.ForeColor = System.Drawing.Color.Green;
            }
            catch (SqlException sqlEx)
            {
                connectionStatusLabel.Text = "Database: ? Connection Failed";
                connectionStatusLabel.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception ex)
            {
                connectionStatusLabel.Text = "Database: ? Error";
                connectionStatusLabel.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                // Close connection
                if (connection != null && connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }
    }
}