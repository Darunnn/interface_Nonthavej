namespace interface_Nonthavej
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources =
                new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));

            tabControl = new TabControl();
            tabDatabase = new TabPage();
            btnTestConnection = new Button();
            lblConnectionStatus = new Label();
            txtPassword = new TextBox();
            lblPassword = new Label();
            txtUserId = new TextBox();
            lblUserId = new Label();
            txtDatabase = new TextBox();
            lblDatabase = new Label();
            txtServer = new TextBox();
            lblServer = new Label();

            // ── NEW: PharmacyCode ──
            lblPharmacyCode = new Label();
            txtPharmacyCode = new TextBox();
            lblPharmacyCodeHint = new Label();

            tabAPI = new TabPage();
            numApiTimeout = new NumericUpDown();
            lblApiTimeout = new Label();
            txtApiEndpoint = new TextBox();
            lblApiEndpoint = new Label();
            tabLog = new TabPage();
            lblLogRetentionInfo = new Label();
            numLogRetention = new NumericUpDown();
            lblLogRetention = new Label();
            panelButtons = new Panel();
            btnCancel = new Button();
            btnSave = new Button();

            tabControl.SuspendLayout();
            tabDatabase.SuspendLayout();
            tabAPI.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numApiTimeout).BeginInit();
            tabLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numLogRetention).BeginInit();
            panelButtons.SuspendLayout();
            SuspendLayout();

            // ── tabControl ─────────────────────────────────────────────
            tabControl.Controls.Add(tabDatabase);
            tabControl.Controls.Add(tabAPI);
            tabControl.Controls.Add(tabLog);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Tahoma", 9F);
            tabControl.Location = new Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(636, 430);   // taller to fit new row
            tabControl.TabIndex = 0;

            // ── tabDatabase ────────────────────────────────────────────
            tabDatabase.BackColor = Color.White;
            tabDatabase.Controls.Add(btnTestConnection);
            tabDatabase.Controls.Add(lblConnectionStatus);
            tabDatabase.Controls.Add(txtPassword);
            tabDatabase.Controls.Add(lblPassword);
            tabDatabase.Controls.Add(txtUserId);
            tabDatabase.Controls.Add(lblUserId);
            tabDatabase.Controls.Add(txtDatabase);
            tabDatabase.Controls.Add(lblDatabase);
            tabDatabase.Controls.Add(txtServer);
            tabDatabase.Controls.Add(lblServer);
            // NEW
            tabDatabase.Controls.Add(lblPharmacyCode);
            tabDatabase.Controls.Add(txtPharmacyCode);
            tabDatabase.Controls.Add(lblPharmacyCodeHint);

            tabDatabase.Location = new Point(4, 23);
            tabDatabase.Name = "tabDatabase";
            tabDatabase.Padding = new Padding(3);
            tabDatabase.Size = new Size(628, 403);      // taller
            tabDatabase.TabIndex = 0;
            tabDatabase.Text = "🗄️ Database";

            // lblServer
            lblServer.AutoSize = true;
            lblServer.Location = new Point(30, 50);
            lblServer.Name = "lblServer";
            lblServer.Size = new Size(46, 14);
            lblServer.TabIndex = 0;
            lblServer.Text = "Server:";

            // txtServer
            txtServer.Location = new Point(165, 47);
            txtServer.Name = "txtServer";
            txtServer.Size = new Size(450, 22);
            txtServer.TabIndex = 1;

            // lblDatabase
            lblDatabase.AutoSize = true;
            lblDatabase.Location = new Point(30, 92);
            lblDatabase.Name = "lblDatabase";
            lblDatabase.Size = new Size(61, 14);
            lblDatabase.TabIndex = 2;
            lblDatabase.Text = "Database:";

            // txtDatabase
            txtDatabase.Location = new Point(165, 89);
            txtDatabase.Name = "txtDatabase";
            txtDatabase.Size = new Size(450, 22);
            txtDatabase.TabIndex = 3;

            // lblUserId
            lblUserId.AutoSize = true;
            lblUserId.Location = new Point(30, 134);
            lblUserId.Name = "lblUserId";
            lblUserId.Size = new Size(51, 14);
            lblUserId.TabIndex = 4;
            lblUserId.Text = "User ID:";

            // txtUserId
            txtUserId.Location = new Point(165, 131);
            txtUserId.Name = "txtUserId";
            txtUserId.Size = new Size(450, 22);
            txtUserId.TabIndex = 5;

            // lblPassword
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(30, 176);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(62, 14);
            lblPassword.TabIndex = 6;
            lblPassword.Text = "Password:";

            // txtPassword
            txtPassword.Location = new Point(165, 173);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '●';
            txtPassword.Size = new Size(450, 22);
            txtPassword.TabIndex = 7;

            // ── NEW: PharmacyCode row (Y=215) ─────────────────────────
            lblPharmacyCode.AutoSize = true;
            lblPharmacyCode.Location = new Point(30, 218);
            lblPharmacyCode.Name = "lblPharmacyCode";
            lblPharmacyCode.TabIndex = 8;
            lblPharmacyCode.Text = "Pharmacy Code:";

            txtPharmacyCode.Location = new Point(165, 215);
            txtPharmacyCode.Name = "txtPharmacyCode";
            txtPharmacyCode.Size = new Size(200, 22);
            txtPharmacyCode.TabIndex = 9;
            txtPharmacyCode.CharacterCasing = CharacterCasing.Upper;

            lblPharmacyCodeHint.ForeColor = Color.Gray;
            lblPharmacyCodeHint.Font = new Font("Tahoma", 7.5F);
            lblPharmacyCodeHint.Location = new Point(375, 218);
            lblPharmacyCodeHint.Name = "lblPharmacyCodeHint";
            lblPharmacyCodeHint.Size = new Size(240, 18);
            lblPharmacyCodeHint.TabIndex = 10;
            lblPharmacyCodeHint.Text = "เช่น PH1, PH2, PH3 หรือ ALL (ทุกชั้น)";

            // ── Test Connection button (shifted down) ──────────────────
            btnTestConnection.BackColor = Color.FromArgb(46, 204, 113);
            btnTestConnection.FlatStyle = FlatStyle.Flat;
            btnTestConnection.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            btnTestConnection.ForeColor = Color.White;
            btnTestConnection.Location = new Point(165, 258);
            btnTestConnection.Name = "btnTestConnection";
            btnTestConnection.Size = new Size(200, 35);
            btnTestConnection.TabIndex = 11;
            btnTestConnection.Text = "🔌 Test Connection";
            btnTestConnection.UseVisualStyleBackColor = false;
            btnTestConnection.Click += BtnTestConnection_Click;

            // lblConnectionStatus
            lblConnectionStatus.Font = new Font("Tahoma", 8.25F);
            lblConnectionStatus.ForeColor = Color.Gray;
            lblConnectionStatus.Location = new Point(165, 302);
            lblConnectionStatus.Name = "lblConnectionStatus";
            lblConnectionStatus.Size = new Size(450, 40);
            lblConnectionStatus.TabIndex = 12;
            lblConnectionStatus.Text = "ℹ️ Click 'Test Connection' to verify database settings";

            // ── tabAPI ─────────────────────────────────────────────────
            tabAPI.BackColor = Color.White;
            tabAPI.Controls.Add(numApiTimeout);
            tabAPI.Controls.Add(lblApiTimeout);
            tabAPI.Controls.Add(txtApiEndpoint);
            tabAPI.Controls.Add(lblApiEndpoint);
            tabAPI.Location = new Point(4, 23);
            tabAPI.Name = "tabAPI";
            tabAPI.Padding = new Padding(3);
            tabAPI.Size = new Size(628, 403);
            tabAPI.TabIndex = 1;
            tabAPI.Text = "🌐 API Settings";

            numApiTimeout.Location = new Point(200, 116);
            numApiTimeout.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            numApiTimeout.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            numApiTimeout.Name = "numApiTimeout";
            numApiTimeout.Size = new Size(120, 22);
            numApiTimeout.TabIndex = 3;
            numApiTimeout.Value = new decimal(new int[] { 30, 0, 0, 0 });

            lblApiTimeout.AutoSize = true;
            lblApiTimeout.Location = new Point(30, 118);
            lblApiTimeout.Name = "lblApiTimeout";
            lblApiTimeout.Size = new Size(161, 14);
            lblApiTimeout.TabIndex = 2;
            lblApiTimeout.Text = "Timeout (seconds) [5-300]:";

            txtApiEndpoint.Location = new Point(33, 70);
            txtApiEndpoint.Name = "txtApiEndpoint";
            txtApiEndpoint.Size = new Size(587, 22);
            txtApiEndpoint.TabIndex = 1;

            lblApiEndpoint.AutoSize = true;
            lblApiEndpoint.Location = new Point(30, 36);
            lblApiEndpoint.Name = "lblApiEndpoint";
            lblApiEndpoint.Size = new Size(429, 14);
            lblApiEndpoint.TabIndex = 0;
            lblApiEndpoint.Text = "API Endpoint URL (e.g., https://example.com/api/conHIS/insertPrescription):";

            // ── tabLog ─────────────────────────────────────────────────
            tabLog.BackColor = Color.White;
            tabLog.Controls.Add(lblLogRetentionInfo);
            tabLog.Controls.Add(numLogRetention);
            tabLog.Controls.Add(lblLogRetention);
            tabLog.Location = new Point(4, 23);
            tabLog.Name = "tabLog";
            tabLog.Size = new Size(628, 403);
            tabLog.TabIndex = 2;
            tabLog.Text = "📋 Logging";

            lblLogRetentionInfo.ForeColor = Color.Gray;
            lblLogRetentionInfo.Location = new Point(30, 110);
            lblLogRetentionInfo.Name = "lblLogRetentionInfo";
            lblLogRetentionInfo.Size = new Size(578, 60);
            lblLogRetentionInfo.TabIndex = 2;
            lblLogRetentionInfo.Text =
                "ℹ️ Log files older than the specified number of days will be automatically deleted.\r\nDefault: 30 days";

            numLogRetention.Location = new Point(200, 70);
            numLogRetention.Maximum = new decimal(new int[] { 365, 0, 0, 0 });
            numLogRetention.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numLogRetention.Name = "numLogRetention";
            numLogRetention.Size = new Size(120, 22);
            numLogRetention.TabIndex = 1;
            numLogRetention.Value = new decimal(new int[] { 30, 0, 0, 0 });

            lblLogRetention.AutoSize = true;
            lblLogRetention.Location = new Point(30, 72);
            lblLogRetention.Name = "lblLogRetention";
            lblLogRetention.Size = new Size(173, 14);
            lblLogRetention.TabIndex = 0;
            lblLogRetention.Text = "Log Retention (days) [1-365]:";

            // ── panelButtons ───────────────────────────────────────────
            panelButtons.BackColor = Color.FromArgb(240, 240, 240);
            panelButtons.Controls.Add(btnCancel);
            panelButtons.Controls.Add(btnSave);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(0, 430);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(636, 60);
            panelButtons.TabIndex = 1;

            btnCancel.Location = new Point(505, 12);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(120, 35);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "❌ Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;

            btnSave.BackColor = Color.FromArgb(52, 152, 219);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(365, 12);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(120, 35);
            btnSave.TabIndex = 0;
            btnSave.Text = "💾 Save";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += BtnSave_Click;

            // ── SettingsForm ───────────────────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 14F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(636, 490);        // taller overall
            Controls.Add(tabControl);
            Controls.Add(panelButtons);
            Font = new Font("Tahoma", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Settings - Interface Pattaya";

            tabControl.ResumeLayout(false);
            tabDatabase.ResumeLayout(false);
            tabDatabase.PerformLayout();
            tabAPI.ResumeLayout(false);
            tabAPI.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numApiTimeout).EndInit();
            tabLog.ResumeLayout(false);
            tabLog.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numLogRetention).EndInit();
            panelButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        // ── Controls ───────────────────────────────────────────────────
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabDatabase;
        private System.Windows.Forms.TabPage tabAPI;
        private System.Windows.Forms.TabPage tabLog;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;

        // Database Tab
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.TextBox txtServer;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.TextBox txtDatabase;
        private System.Windows.Forms.Label lblUserId;
        private System.Windows.Forms.TextBox txtUserId;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblConnectionStatus;
        private System.Windows.Forms.Button btnTestConnection;

        // ── NEW ────────────────────────────────────────────────────────
        private System.Windows.Forms.Label lblPharmacyCode;
        private System.Windows.Forms.TextBox txtPharmacyCode;
        private System.Windows.Forms.Label lblPharmacyCodeHint;

        // API Tab
        private System.Windows.Forms.Label lblApiEndpoint;
        private System.Windows.Forms.TextBox txtApiEndpoint;
        private System.Windows.Forms.Label lblApiTimeout;
        private System.Windows.Forms.NumericUpDown numApiTimeout;

        // Log Tab
        private System.Windows.Forms.Label lblLogRetention;
        private System.Windows.Forms.NumericUpDown numLogRetention;
        private System.Windows.Forms.Label lblLogRetentionInfo;
    }
}