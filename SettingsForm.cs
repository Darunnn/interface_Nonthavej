using interface_Nonthavej.FunctionFrom.Settings;
using interface_Nonthavej.Models;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace interface_Nonthavej
{
    public partial class SettingsForm : System.Windows.Forms.Form
    {
        // Managers
        private readonly FnDatabaseSettings _dbManager;
        private readonly FnAPISettings _apiManager;
        private readonly FnLogSettings _logManager;
        private readonly SettingsValidator _validator;

        // Current Settings
        private DatabaseSettings _dbSettings;
        private APISettings _apiSettings;
        private LogSettings _logSettings;

        public bool SettingsChanged { get; private set; }

        public SettingsForm()
        {
            InitializeComponent();

            // Initialize managers
            _dbManager = new FnDatabaseSettings();
            _apiManager = new FnAPISettings();
            _logManager = new FnLogSettings();
            _validator = new SettingsValidator();

            LoadCurrentSettings();
        }

        #region Load Settings

        private void LoadCurrentSettings()
        {
            try
            {
                LoadDatabaseSettings();
                LoadAPISettings();
                LoadLogSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"เกิดข้อผิดพลาดในการโหลดการตั้งค่า:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void LoadDatabaseSettings()
        {
            _dbSettings = _dbManager.Load();

            txtServer.Text = _dbSettings.Server;
            txtDatabase.Text = _dbSettings.Database;
            txtUserId.Text = _dbSettings.UserId;
            txtPassword.Text = _dbSettings.Password;
        }

        private void LoadAPISettings()
        {
            _apiSettings = _apiManager.Load();

            txtApiEndpoint.Text = _apiSettings.ApiEndpoint;
            numApiTimeout.Value = _apiSettings.ApiTimeoutSeconds;
            numApiRetry.Value = _apiSettings.ApiRetryAttempts;
            numApiRetryDelay.Value = _apiSettings.ApiRetryDelaySeconds;
        }

        private void LoadLogSettings()
        {
            _logSettings = _logManager.Load();
            numLogRetention.Value = _logSettings.LogRetentionDays;
        }

        #endregion

        #region Button Click Events

        private async void BtnTestConnection_Click(object sender, EventArgs e)
        {
            // Update settings from form
            UpdateDatabaseSettingsFromForm();

            // Validate
            var validation = _validator.ValidateDatabase(_dbSettings);
            if (!validation.IsValid)
            {
                MessageBox.Show(validation.ErrorMessage, "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedTab = tabDatabase;
                FocusDatabaseField(validation.FieldName);
                return;
            }

            // Test connection
            btnTestConnection.Enabled = false;
            lblConnectionStatus.Text = "⏳ Testing connection...";
            lblConnectionStatus.ForeColor = Color.Orange;

            try
            {
                var result = await _dbManager.TestConnectionAsync(_dbSettings);

                if (result.IsSuccess)
                {
                    lblConnectionStatus.Text = $"✅ Connected successfully!\nSQL Server Version: {result.Version}";
                    lblConnectionStatus.ForeColor = Color.Green;

                    MessageBox.Show(
                        $"✅ การเชื่อมต่อสำเร็จ!\n\n" +
                        $"Server: {_dbSettings.Server}\n" +
                        $"Database: {_dbSettings.Database}\n" +
                        $"SQL Server Version: {result.Version}",
                        "Connection Successful",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    lblConnectionStatus.Text = $"❌ Connection failed: {result.Message}";
                    lblConnectionStatus.ForeColor = Color.Red;

                    MessageBox.Show(
                        $"❌ การเชื่อมต่อล้มเหลว!\n\n{result.Message}\n\n" +
                        $"กรุณาตรวจสอบ:\n" +
                        $"• Server address และ port\n" +
                        $"• Database name\n" +
                        $"• Username และ Password\n" +
                        $"• Network connectivity",
                        "Connection Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                lblConnectionStatus.Text = $"❌ Error: {ex.Message}";
                lblConnectionStatus.ForeColor = Color.Red;

                MessageBox.Show(
                    $"❌ เกิดข้อผิดพลาด!\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnTestConnection.Enabled = true;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Update all settings from form
                UpdateAllSettingsFromForm();

                // Validate all settings
                if (!ValidateAllSettings())
                    return;

                // Confirm save
                var result = MessageBox.Show(
                    "คุณต้องการบันทึกการตั้งค่าทั้งหมดหรือไม่?\n\n" +
                    "📁 ไฟล์ที่จะถูกแก้ไข:\n" +
                    "   • Connection\\connectdatabase.ini\n" +
                    "   • Config\\appsettings.ini\n" +
                    "   • App.config\n\n" +
                    "⚠️ หมายเหตุ: การตั้งค่าบางอย่างอาจต้อง Restart โปรแกรม",
                    "Confirm Save",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result != DialogResult.Yes)
                    return;

                // Save all settings
                SaveAllSettings();

                SettingsChanged = true;

                MessageBox.Show(
                    "✅ บันทึกการตั้งค่าทั้งหมดสำเร็จ!\n\n" +
                    "ไฟล์ที่ถูกอัพเดท:\n" +
                    "✓ Connection\\connectdatabase.ini\n" +
                    "✓ Config\\appsettings.ini\n" +
                    "✓ App.config\n\n" +
                    "💡 การตั้งค่าจะมีผลในครั้งถัดไปที่โปรแกรมโหลดข้อมูล",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ เกิดข้อผิดพลาดในการบันทึกการตั้งค่า:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "คุณต้องการยกเลิกการเปลี่ยนแปลงหรือไม่?\n\nการตั้งค่าที่แก้ไขจะไม่ถูกบันทึก",
                "Confirm Cancel",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        #endregion

        #region Helper Methods

        private void UpdateDatabaseSettingsFromForm()
        {
            _dbSettings.Server = txtServer.Text.Trim();
            _dbSettings.Database = txtDatabase.Text.Trim();
            _dbSettings.UserId = txtUserId.Text.Trim();
            _dbSettings.Password = txtPassword.Text.Trim();
        }

        private void UpdateAPISettingsFromForm()
        {
            _apiSettings.ApiEndpoint = txtApiEndpoint.Text.Trim();
            _apiSettings.ApiTimeoutSeconds = (int)numApiTimeout.Value;
            _apiSettings.ApiRetryAttempts = (int)numApiRetry.Value;
            _apiSettings.ApiRetryDelaySeconds = (int)numApiRetryDelay.Value;
        }

        private void UpdateLogSettingsFromForm()
        {
            _logSettings.LogRetentionDays = (int)numLogRetention.Value;
        }

        private void UpdateAllSettingsFromForm()
        {
            UpdateDatabaseSettingsFromForm();
            UpdateAPISettingsFromForm();
            UpdateLogSettingsFromForm();
        }

        private bool ValidateAllSettings()
        {
            // Validate Database
            var dbValidation = _validator.ValidateDatabase(_dbSettings);
            if (!dbValidation.IsValid)
            {
                MessageBox.Show(dbValidation.ErrorMessage, "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedTab = tabDatabase;
                FocusDatabaseField(dbValidation.FieldName);
                return false;
            }

            // Validate API
            var apiValidation = _validator.ValidateAPI(_apiSettings);
            if (!apiValidation.IsValid)
            {
                MessageBox.Show(apiValidation.ErrorMessage, "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedTab = tabAPI;
                FocusAPIField(apiValidation.FieldName);
                return false;
            }

            // Validate Log
            var logValidation = _validator.ValidateLog(_logSettings);
            if (!logValidation.IsValid)
            {
                MessageBox.Show(logValidation.ErrorMessage, "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControl.SelectedTab = tabLog;
                numLogRetention.Focus();
                return false;
            }

            return true;
        }

        private void SaveAllSettings()
        {
            _dbManager.Save(_dbSettings);
            _apiManager.Save(_apiSettings);
            _logManager.Save(_logSettings);
        }

        private void FocusDatabaseField(string fieldName)
        {
            switch (fieldName)
            {
                case "Server":
                    txtServer.Focus();
                    break;
                case "Database":
                    txtDatabase.Focus();
                    break;
                case "UserId":
                    txtUserId.Focus();
                    break;
                case "Password":
                    txtPassword.Focus();
                    break;
            }
        }

        private void FocusAPIField(string fieldName)
        {
            switch (fieldName)
            {
                case "ApiEndpoint":
                    txtApiEndpoint.Focus();
                    break;
                case "ApiTimeout":
                    numApiTimeout.Focus();
                    break;
                case "ApiRetry":
                    numApiRetry.Focus();
                    break;
                case "ApiRetryDelay":
                    numApiRetryDelay.Focus();
                    break;
            }
        }

        #endregion
    }
}