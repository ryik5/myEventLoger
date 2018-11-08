namespace myEventLoger
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.StatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.ProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.StatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabelCurrent = new System.Windows.Forms.ToolStripStatusLabel();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageCtrl = new System.Windows.Forms.TabPage();
            this.groupBoxDevices = new System.Windows.Forms.GroupBox();
            this.buttonGetInfoDevices = new System.Windows.Forms.Button();
            this.comboBoxSelectDevice = new System.Windows.Forms.ComboBox();
            this.checkBoxDeviceDisable = new System.Windows.Forms.CheckBox();
            this.DisableDevice = new System.Windows.Forms.Button();
            this.groupBoxPC = new System.Windows.Forms.GroupBox();
            this.labelLockScreen = new System.Windows.Forms.Label();
            this.chkbxLockScreen = new System.Windows.Forms.CheckBox();
            this.labelReboot2 = new System.Windows.Forms.Label();
            this.chkbxReboot2 = new System.Windows.Forms.CheckBox();
            this.buttonPing = new System.Windows.Forms.Button();
            this.labelControlPing = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chkbxUser = new System.Windows.Forms.CheckBox();
            this.labelPowerOff = new System.Windows.Forms.Label();
            this.chkbxPowerOff = new System.Windows.Forms.CheckBox();
            this.labelReboot1 = new System.Windows.Forms.Label();
            this.chkbxReboot1 = new System.Windows.Forms.CheckBox();
            this.groupBoxService = new System.Windows.Forms.GroupBox();
            this.checkBoxChangeStateService = new System.Windows.Forms.CheckBox();
            this.labelSelectedService = new System.Windows.Forms.Label();
            this.labelStatusService = new System.Windows.Forms.Label();
            this.labelDisplayNameService = new System.Windows.Forms.Label();
            this.buttonGetServices = new System.Windows.Forms.Button();
            this.chkbxService = new System.Windows.Forms.CheckBox();
            this.comboBoxService = new System.Windows.Forms.ComboBox();
            this.groupBoxProcess = new System.Windows.Forms.GroupBox();
            this.checkBoxReRunProcess = new System.Windows.Forms.CheckBox();
            this.labelParentProcess = new System.Windows.Forms.Label();
            this.labelProcess = new System.Windows.Forms.Label();
            this.labelPathAndTimeProcess = new System.Windows.Forms.Label();
            this.buttonGetProcess = new System.Windows.Forms.Button();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.comboBoxProcess = new System.Windows.Forms.ComboBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.textBoxNameProgramm = new System.Windows.Forms.TextBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBoxRunProcess = new System.Windows.Forms.CheckBox();
            this.checkBoxKillProcess = new System.Windows.Forms.CheckBox();
            this.tabPageLog = new System.Windows.Forms.TabPage();
            this.textBoxLogs = new System.Windows.Forms.TextBox();
            this.tabDataGrid = new System.Windows.Forms.TabPage();
            this.pictureBoxLogoDataGrid = new System.Windows.Forms.PictureBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.contextMenuDataGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.GetArchievesHostsItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator7 = new System.Windows.Forms.ToolStripSeparator();
            this.GetPreviousScanItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DeSelectUnknowItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DeSelectUnknowNoPermissionItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator0 = new System.Windows.Forms.ToolStripSeparator();
            this.ScannedNetsTime1 = new System.Windows.Forms.ToolStripMenuItem();
            this.ScannedNetsTime2 = new System.Windows.Forms.ToolStripMenuItem();
            this.ScannedNetsTime3 = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator5 = new System.Windows.Forms.ToolStripSeparator();
            this.GetArchieveOfHostItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator1 = new System.Windows.Forms.ToolStripSeparator();
            this.PingItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GetFileItems = new System.Windows.Forms.ToolStripMenuItem();
            this.GetRegistryItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GetEventsItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GetWholeDataItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPageRDP = new System.Windows.Forms.TabPage();
            this.RDP = new AxMSTSCLib.AxMsRdpClient9NotSafeForScripting();
            this.comboUsers = new System.Windows.Forms.ComboBox();
            this.listBoxNetsRow = new System.Windows.Forms.ListBox();
            this.textBoxLogin = new System.Windows.Forms.TextBox();
            this.textBoxDomain = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.labelLicense = new System.Windows.Forms.Label();
            this.labelLogin = new System.Windows.Forms.Label();
            this.labelPassword = new System.Windows.Forms.Label();
            this.labelInputNetOrInputPC = new System.Windows.Forms.Label();
            this.comboBoxTargedPC = new System.Windows.Forms.ComboBox();
            this.textBoxInputNetOrInputPC = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.MainMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ResetAuthorizationItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ChangePCItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CorrectAccessErrorItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuFunction = new System.Windows.Forms.ToolStripMenuItem();
            this.SearchMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ScanEnvironmentItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ScanSelectedNetItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GetHostsFromDomainItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator2 = new System.Windows.Forms.ToolStripSeparator();
            this.PingHostItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator4 = new System.Windows.Forms.ToolStripSeparator();
            this.ArchievGotFromNetsItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator6 = new System.Windows.Forms.ToolStripSeparator();
            this.CurrentTCPConnectionsItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SmartSearchMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.smartLoginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smartWordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smartLoginAndWordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GetDataMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.GetLogRemotePCItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GetFilesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GetRegItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator8 = new System.Windows.Forms.ToolStripSeparator();
            this.FullScanItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator3 = new System.Windows.Forms.ToolStripSeparator();
            this.GetDataHostsArchievItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DBMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.DBCreateTestItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DBInsertTestItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DBReadNamesOfTablesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DBReadColumnsOfTheTableItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DBHostsAccessItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DBHostsAccessDeleteRowItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DBUsersPrivateItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FindTablesFitIntoPersonalItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DBUsersPrivateDeleteRowItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ShrinkDBPersonalItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportExcelDataItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewStoredDataPersonalMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewStoredDataPersonalMainItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewStoredDataPersonalPhonesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewStoredDataPersonalAddressItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewStoredDataPersonalWorkItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewStoredDataPersonalLearnItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewStoredDataPersonalDocumentItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewStoredDataPersonalRelationItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportExcelDataIntoDBMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportExcelDataIntoDbMainItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportExcelDataIntoDbPhoneItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportExcelDataIntoDbAddressItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportExcelDataIntoDbWorkItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportExcelDataIntoDbLearnItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportExcelDataIntoDbDocumentItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportExcelDataIntoDbRelationItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AnalyseDataMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.listTablesDBItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ControlHostMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.RDPMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StopSearchItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SignerItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SignerRecoverItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SignGnerateLicenseItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.LicenseItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HelpItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AboutItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importExcelItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelNets = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.labelTables = new System.Windows.Forms.Label();
            this.comboTables = new System.Windows.Forms.ComboBox();
            this.comboAnalyse = new System.Windows.Forms.ComboBox();
            this.labelCurrentNet = new System.Windows.Forms.Label();
            this.textBoxLicense = new System.Windows.Forms.TextBox();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.labelResultCheckingLicense = new System.Windows.Forms.Label();
            this.labelDomain = new System.Windows.Forms.Label();
            this.labelTargedPCColor = new System.Windows.Forms.Label();
            this.labelNetsColor = new System.Windows.Forms.Label();
            this.labelTablesColor = new System.Windows.Forms.Label();
            this.labelRows = new System.Windows.Forms.Label();
            this.labelRowsColor = new System.Windows.Forms.Label();
            this.comboRows = new System.Windows.Forms.ComboBox();
            this.textBoxRows = new System.Windows.Forms.TextBox();
            this.labelNameTable = new System.Windows.Forms.Label();
            this.textBoxSearchUser = new System.Windows.Forms.TextBox();
            this.labelSearchUser = new System.Windows.Forms.Label();
            this.comboRows2 = new System.Windows.Forms.ComboBox();
            this.textBoxRows2 = new System.Windows.Forms.TextBox();
            this.labelNameTable2 = new System.Windows.Forms.Label();
            this.labelRowsColor2 = new System.Windows.Forms.Label();
            this.labelListRow = new System.Windows.Forms.Label();
            this.comboBoxMask1 = new System.Windows.Forms.ComboBox();
            this.comboBoxMask2 = new System.Windows.Forms.ComboBox();
            this.toolTipText1 = new System.Windows.Forms.ToolTip(this.components);
            this.openFileDialogExcel = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip1.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageCtrl.SuspendLayout();
            this.groupBoxDevices.SuspendLayout();
            this.groupBoxPC.SuspendLayout();
            this.groupBoxService.SuspendLayout();
            this.groupBoxProcess.SuspendLayout();
            this.tabPageLog.SuspendLayout();
            this.tabDataGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogoDataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.contextMenuDataGrid.SuspendLayout();
            this.tabPageRDP.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RDP)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel1,
            this.ProgressBar1,
            this.StatusLabel2,
            this.StatusLabelCurrent});
            this.statusStrip1.Location = new System.Drawing.Point(0, 528);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(871, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // StatusLabel1
            // 
            this.StatusLabel1.Name = "StatusLabel1";
            this.StatusLabel1.Size = new System.Drawing.Size(73, 17);
            this.StatusLabel1.Text = "StatusLabel1";
            // 
            // ProgressBar1
            // 
            this.ProgressBar1.Name = "ProgressBar1";
            this.ProgressBar1.Size = new System.Drawing.Size(100, 16);
            // 
            // StatusLabel2
            // 
            this.StatusLabel2.Name = "StatusLabel2";
            this.StatusLabel2.Size = new System.Drawing.Size(634, 17);
            this.StatusLabel2.Spring = true;
            this.StatusLabel2.Text = "StatusLabel2";
            this.StatusLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // StatusLabelCurrent
            // 
            this.StatusLabelCurrent.MergeAction = System.Windows.Forms.MergeAction.Replace;
            this.StatusLabelCurrent.Name = "StatusLabelCurrent";
            this.StatusLabelCurrent.Size = new System.Drawing.Size(47, 17);
            this.StatusLabelCurrent.Text = "Current";
            this.StatusLabelCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // notifyIcon
            // 
            this.notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon.BalloonTipText = "SIEM ©RYIK 2016-2017";
            this.notifyIcon.BalloonTipTitle = "ryik.yuri@gmail.com";
            this.notifyIcon.Text = "Security and Information Events and Management by hosts";
            this.notifyIcon.Visible = true;
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabPageCtrl);
            this.tabControl.Controls.Add(this.tabPageLog);
            this.tabControl.Controls.Add(this.tabDataGrid);
            this.tabControl.Controls.Add(this.tabPageRDP);
            this.tabControl.Location = new System.Drawing.Point(0, 93);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(871, 431);
            this.tabControl.TabIndex = 3;
            this.tabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tabControl_Selecting);
            // 
            // tabPageCtrl
            // 
            this.tabPageCtrl.Controls.Add(this.groupBoxDevices);
            this.tabPageCtrl.Controls.Add(this.groupBoxPC);
            this.tabPageCtrl.Controls.Add(this.groupBoxService);
            this.tabPageCtrl.Controls.Add(this.groupBoxProcess);
            this.tabPageCtrl.Location = new System.Drawing.Point(4, 22);
            this.tabPageCtrl.Name = "tabPageCtrl";
            this.tabPageCtrl.Size = new System.Drawing.Size(863, 405);
            this.tabPageCtrl.TabIndex = 5;
            this.tabPageCtrl.Text = "Управление хостом";
            this.tabPageCtrl.UseVisualStyleBackColor = true;
            // 
            // groupBoxDevices
            // 
            this.groupBoxDevices.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxDevices.Controls.Add(this.buttonGetInfoDevices);
            this.groupBoxDevices.Controls.Add(this.comboBoxSelectDevice);
            this.groupBoxDevices.Controls.Add(this.checkBoxDeviceDisable);
            this.groupBoxDevices.Controls.Add(this.DisableDevice);
            this.groupBoxDevices.Location = new System.Drawing.Point(536, 255);
            this.groupBoxDevices.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxDevices.Name = "groupBoxDevices";
            this.groupBoxDevices.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxDevices.Size = new System.Drawing.Size(296, 83);
            this.groupBoxDevices.TabIndex = 245;
            this.groupBoxDevices.TabStop = false;
            this.groupBoxDevices.Text = "Устройства";
            // 
            // buttonGetInfoDevices
            // 
            this.buttonGetInfoDevices.AutoSize = true;
            this.buttonGetInfoDevices.Location = new System.Drawing.Point(162, 18);
            this.buttonGetInfoDevices.Name = "buttonGetInfoDevices";
            this.buttonGetInfoDevices.Size = new System.Drawing.Size(138, 27);
            this.buttonGetInfoDevices.TabIndex = 245;
            this.buttonGetInfoDevices.Text = "Get Devices\' info";
            this.buttonGetInfoDevices.UseVisualStyleBackColor = true;
            this.buttonGetInfoDevices.Click += new System.EventHandler(this.buttonGetInfoDevices_Click);
            // 
            // comboBoxSelectDevice
            // 
            this.comboBoxSelectDevice.FormattingEnabled = true;
            this.comboBoxSelectDevice.Location = new System.Drawing.Point(162, 47);
            this.comboBoxSelectDevice.Name = "comboBoxSelectDevice";
            this.comboBoxSelectDevice.Size = new System.Drawing.Size(119, 21);
            this.comboBoxSelectDevice.TabIndex = 244;
            this.comboBoxSelectDevice.SelectedIndexChanged += new System.EventHandler(this.checkBoxDeviceDisable_CheckedChanged);
            // 
            // checkBoxDeviceDisable
            // 
            this.checkBoxDeviceDisable.AutoSize = true;
            this.checkBoxDeviceDisable.Location = new System.Drawing.Point(14, 23);
            this.checkBoxDeviceDisable.Name = "checkBoxDeviceDisable";
            this.checkBoxDeviceDisable.Size = new System.Drawing.Size(15, 14);
            this.checkBoxDeviceDisable.TabIndex = 243;
            this.checkBoxDeviceDisable.Tag = "220";
            this.checkBoxDeviceDisable.UseVisualStyleBackColor = true;
            this.checkBoxDeviceDisable.CheckedChanged += new System.EventHandler(this.checkBoxDeviceDisable_CheckedChanged);
            // 
            // DisableDevice
            // 
            this.DisableDevice.AutoSize = true;
            this.DisableDevice.Location = new System.Drawing.Point(38, 18);
            this.DisableDevice.Name = "DisableDevice";
            this.DisableDevice.Size = new System.Drawing.Size(116, 27);
            this.DisableDevice.TabIndex = 242;
            this.DisableDevice.Text = "Disable Device";
            this.DisableDevice.UseVisualStyleBackColor = true;
            this.DisableDevice.Click += new System.EventHandler(this.buttonControlDevices_Click);
            // 
            // groupBoxPC
            // 
            this.groupBoxPC.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxPC.Controls.Add(this.labelLockScreen);
            this.groupBoxPC.Controls.Add(this.chkbxLockScreen);
            this.groupBoxPC.Controls.Add(this.labelReboot2);
            this.groupBoxPC.Controls.Add(this.chkbxReboot2);
            this.groupBoxPC.Controls.Add(this.buttonPing);
            this.groupBoxPC.Controls.Add(this.labelControlPing);
            this.groupBoxPC.Controls.Add(this.label2);
            this.groupBoxPC.Controls.Add(this.chkbxUser);
            this.groupBoxPC.Controls.Add(this.labelPowerOff);
            this.groupBoxPC.Controls.Add(this.chkbxPowerOff);
            this.groupBoxPC.Controls.Add(this.labelReboot1);
            this.groupBoxPC.Controls.Add(this.chkbxReboot1);
            this.groupBoxPC.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBoxPC.Location = new System.Drawing.Point(14, 11);
            this.groupBoxPC.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxPC.Name = "groupBoxPC";
            this.groupBoxPC.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxPC.Size = new System.Drawing.Size(201, 327);
            this.groupBoxPC.TabIndex = 234;
            this.groupBoxPC.TabStop = false;
            this.groupBoxPC.Text = "ПК";
            // 
            // labelLockScreen
            // 
            this.labelLockScreen.AutoSize = true;
            this.labelLockScreen.Location = new System.Drawing.Point(43, 95);
            this.labelLockScreen.Name = "labelLockScreen";
            this.labelLockScreen.Size = new System.Drawing.Size(106, 13);
            this.labelLockScreen.TabIndex = 247;
            this.labelLockScreen.Text = "Блокировать экран";
            // 
            // chkbxLockScreen
            // 
            this.chkbxLockScreen.AutoSize = true;
            this.chkbxLockScreen.Location = new System.Drawing.Point(14, 95);
            this.chkbxLockScreen.Name = "chkbxLockScreen";
            this.chkbxLockScreen.Size = new System.Drawing.Size(15, 14);
            this.chkbxLockScreen.TabIndex = 246;
            this.chkbxLockScreen.Tag = "4";
            this.chkbxLockScreen.UseVisualStyleBackColor = true;
            // 
            // labelReboot2
            // 
            this.labelReboot2.AutoSize = true;
            this.labelReboot2.Location = new System.Drawing.Point(43, 195);
            this.labelReboot2.Name = "labelReboot2";
            this.labelReboot2.Size = new System.Drawing.Size(102, 13);
            this.labelReboot2.TabIndex = 245;
            this.labelReboot2.Text = "Перезагрузить ПК";
            // 
            // chkbxReboot2
            // 
            this.chkbxReboot2.AutoSize = true;
            this.chkbxReboot2.Location = new System.Drawing.Point(14, 195);
            this.chkbxReboot2.Name = "chkbxReboot2";
            this.chkbxReboot2.Size = new System.Drawing.Size(15, 14);
            this.chkbxReboot2.TabIndex = 244;
            this.chkbxReboot2.Tag = "3";
            this.chkbxReboot2.UseVisualStyleBackColor = true;
            // 
            // buttonPing
            // 
            this.buttonPing.Location = new System.Drawing.Point(41, 20);
            this.buttonPing.Name = "buttonPing";
            this.buttonPing.Size = new System.Drawing.Size(132, 23);
            this.buttonPing.TabIndex = 239;
            this.buttonPing.Text = "Stop Ping";
            this.buttonPing.UseVisualStyleBackColor = true;
            this.buttonPing.Click += new System.EventHandler(this.ButtonPing_Click);
            // 
            // labelControlPing
            // 
            this.labelControlPing.Location = new System.Drawing.Point(43, 227);
            this.labelControlPing.Name = "labelControlPing";
            this.labelControlPing.Size = new System.Drawing.Size(128, 82);
            this.labelControlPing.TabIndex = 242;
            this.labelControlPing.Text = "Searching...";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(43, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(135, 13);
            this.label2.TabIndex = 240;
            this.label2.Text = "Выгрузить пользователя";
            // 
            // chkbxUser
            // 
            this.chkbxUser.AutoSize = true;
            this.chkbxUser.Location = new System.Drawing.Point(14, 63);
            this.chkbxUser.Name = "chkbxUser";
            this.chkbxUser.Size = new System.Drawing.Size(15, 14);
            this.chkbxUser.TabIndex = 237;
            this.chkbxUser.Tag = "2";
            this.chkbxUser.UseVisualStyleBackColor = true;
            // 
            // labelPowerOff
            // 
            this.labelPowerOff.AutoSize = true;
            this.labelPowerOff.Location = new System.Drawing.Point(43, 127);
            this.labelPowerOff.Name = "labelPowerOff";
            this.labelPowerOff.Size = new System.Drawing.Size(82, 13);
            this.labelPowerOff.TabIndex = 238;
            this.labelPowerOff.Text = "Выключить ПК";
            // 
            // chkbxPowerOff
            // 
            this.chkbxPowerOff.AutoSize = true;
            this.chkbxPowerOff.Location = new System.Drawing.Point(14, 127);
            this.chkbxPowerOff.Name = "chkbxPowerOff";
            this.chkbxPowerOff.Size = new System.Drawing.Size(15, 14);
            this.chkbxPowerOff.TabIndex = 235;
            this.chkbxPowerOff.Tag = "1";
            this.chkbxPowerOff.UseVisualStyleBackColor = true;
            // 
            // labelReboot1
            // 
            this.labelReboot1.AutoSize = true;
            this.labelReboot1.Location = new System.Drawing.Point(43, 161);
            this.labelReboot1.Name = "labelReboot1";
            this.labelReboot1.Size = new System.Drawing.Size(102, 13);
            this.labelReboot1.TabIndex = 236;
            this.labelReboot1.Text = "Перезагрузить ПК";
            // 
            // chkbxReboot1
            // 
            this.chkbxReboot1.AutoSize = true;
            this.chkbxReboot1.Location = new System.Drawing.Point(14, 161);
            this.chkbxReboot1.Name = "chkbxReboot1";
            this.chkbxReboot1.Size = new System.Drawing.Size(15, 14);
            this.chkbxReboot1.TabIndex = 234;
            this.chkbxReboot1.Tag = "0";
            this.chkbxReboot1.UseVisualStyleBackColor = true;
            // 
            // groupBoxService
            // 
            this.groupBoxService.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxService.Controls.Add(this.checkBoxChangeStateService);
            this.groupBoxService.Controls.Add(this.labelSelectedService);
            this.groupBoxService.Controls.Add(this.labelStatusService);
            this.groupBoxService.Controls.Add(this.labelDisplayNameService);
            this.groupBoxService.Controls.Add(this.buttonGetServices);
            this.groupBoxService.Controls.Add(this.chkbxService);
            this.groupBoxService.Controls.Add(this.comboBoxService);
            this.groupBoxService.Location = new System.Drawing.Point(234, 11);
            this.groupBoxService.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxService.Name = "groupBoxService";
            this.groupBoxService.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxService.Size = new System.Drawing.Size(283, 327);
            this.groupBoxService.TabIndex = 235;
            this.groupBoxService.TabStop = false;
            this.groupBoxService.Text = "Cлужбы";
            // 
            // checkBoxChangeStateService
            // 
            this.checkBoxChangeStateService.AutoSize = true;
            this.checkBoxChangeStateService.Location = new System.Drawing.Point(14, 26);
            this.checkBoxChangeStateService.Name = "checkBoxChangeStateService";
            this.checkBoxChangeStateService.Size = new System.Drawing.Size(15, 14);
            this.checkBoxChangeStateService.TabIndex = 241;
            this.checkBoxChangeStateService.Tag = "104";
            this.checkBoxChangeStateService.UseVisualStyleBackColor = true;
            this.checkBoxChangeStateService.CheckStateChanged += new System.EventHandler(this.checkBoxChangeStateService_CheckStateChanged);
            // 
            // labelSelectedService
            // 
            this.labelSelectedService.Location = new System.Drawing.Point(37, 95);
            this.labelSelectedService.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelSelectedService.Name = "labelSelectedService";
            this.labelSelectedService.Size = new System.Drawing.Size(221, 28);
            this.labelSelectedService.TabIndex = 240;
            this.labelSelectedService.Text = "Selected Service";
            // 
            // labelStatusService
            // 
            this.labelStatusService.Location = new System.Drawing.Point(37, 131);
            this.labelStatusService.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelStatusService.Name = "labelStatusService";
            this.labelStatusService.Size = new System.Drawing.Size(221, 28);
            this.labelStatusService.TabIndex = 239;
            this.labelStatusService.Text = "Status Service\'s";
            // 
            // labelDisplayNameService
            // 
            this.labelDisplayNameService.Location = new System.Drawing.Point(37, 170);
            this.labelDisplayNameService.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelDisplayNameService.Name = "labelDisplayNameService";
            this.labelDisplayNameService.Size = new System.Drawing.Size(221, 91);
            this.labelDisplayNameService.TabIndex = 238;
            this.labelDisplayNameService.Text = "Display Name of Service";
            // 
            // buttonGetServices
            // 
            this.buttonGetServices.Location = new System.Drawing.Point(39, 20);
            this.buttonGetServices.Name = "buttonGetServices";
            this.buttonGetServices.Size = new System.Drawing.Size(220, 25);
            this.buttonGetServices.TabIndex = 237;
            this.buttonGetServices.Text = "Получить список служб";
            this.buttonGetServices.UseVisualStyleBackColor = true;
            this.buttonGetServices.Click += new System.EventHandler(this.buttonGetServices_Click);
            // 
            // chkbxService
            // 
            this.chkbxService.AutoSize = true;
            this.chkbxService.Location = new System.Drawing.Point(14, 66);
            this.chkbxService.Name = "chkbxService";
            this.chkbxService.Size = new System.Drawing.Size(15, 14);
            this.chkbxService.TabIndex = 235;
            this.chkbxService.Tag = "104";
            this.chkbxService.UseVisualStyleBackColor = true;
            this.chkbxService.CheckedChanged += new System.EventHandler(this.chkbxService_CheckedChanged);
            // 
            // comboBoxService
            // 
            this.comboBoxService.FormattingEnabled = true;
            this.comboBoxService.Location = new System.Drawing.Point(39, 63);
            this.comboBoxService.Name = "comboBoxService";
            this.comboBoxService.Size = new System.Drawing.Size(220, 21);
            this.comboBoxService.TabIndex = 236;
            this.comboBoxService.SelectedIndexChanged += new System.EventHandler(this.comboBoxService_SelectedIndexChanged);
            // 
            // groupBoxProcess
            // 
            this.groupBoxProcess.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxProcess.Controls.Add(this.checkBoxReRunProcess);
            this.groupBoxProcess.Controls.Add(this.labelParentProcess);
            this.groupBoxProcess.Controls.Add(this.labelProcess);
            this.groupBoxProcess.Controls.Add(this.labelPathAndTimeProcess);
            this.groupBoxProcess.Controls.Add(this.buttonGetProcess);
            this.groupBoxProcess.Controls.Add(this.checkBox6);
            this.groupBoxProcess.Controls.Add(this.comboBoxProcess);
            this.groupBoxProcess.Controls.Add(this.checkBox4);
            this.groupBoxProcess.Controls.Add(this.textBoxNameProgramm);
            this.groupBoxProcess.Controls.Add(this.checkBox3);
            this.groupBoxProcess.Controls.Add(this.checkBoxRunProcess);
            this.groupBoxProcess.Controls.Add(this.checkBoxKillProcess);
            this.groupBoxProcess.Location = new System.Drawing.Point(535, 11);
            this.groupBoxProcess.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxProcess.Name = "groupBoxProcess";
            this.groupBoxProcess.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxProcess.Size = new System.Drawing.Size(296, 240);
            this.groupBoxProcess.TabIndex = 236;
            this.groupBoxProcess.TabStop = false;
            this.groupBoxProcess.Text = "Процессы";
            // 
            // checkBoxReRunProcess
            // 
            this.checkBoxReRunProcess.AutoSize = true;
            this.checkBoxReRunProcess.Location = new System.Drawing.Point(112, 221);
            this.checkBoxReRunProcess.Name = "checkBoxReRunProcess";
            this.checkBoxReRunProcess.Size = new System.Drawing.Size(15, 14);
            this.checkBoxReRunProcess.TabIndex = 245;
            this.checkBoxReRunProcess.Tag = "204";
            this.checkBoxReRunProcess.UseVisualStyleBackColor = true;
            this.checkBoxReRunProcess.CheckStateChanged += new System.EventHandler(this.checkBoxReRunProcess_CheckStateChanged);
            // 
            // labelParentProcess
            // 
            this.labelParentProcess.Location = new System.Drawing.Point(14, 175);
            this.labelParentProcess.Name = "labelParentProcess";
            this.labelParentProcess.Size = new System.Drawing.Size(268, 14);
            this.labelParentProcess.TabIndex = 244;
            this.labelParentProcess.Text = "labelParentProcess";
            // 
            // labelProcess
            // 
            this.labelProcess.Location = new System.Drawing.Point(14, 89);
            this.labelProcess.Name = "labelProcess";
            this.labelProcess.Size = new System.Drawing.Size(268, 14);
            this.labelProcess.TabIndex = 242;
            this.labelProcess.Text = "labelProcess";
            // 
            // labelPathAndTimeProcess
            // 
            this.labelPathAndTimeProcess.Location = new System.Drawing.Point(14, 111);
            this.labelPathAndTimeProcess.Name = "labelPathAndTimeProcess";
            this.labelPathAndTimeProcess.Size = new System.Drawing.Size(268, 56);
            this.labelPathAndTimeProcess.TabIndex = 243;
            this.labelPathAndTimeProcess.Text = "labelPathAndTimeProcess";
            // 
            // buttonGetProcess
            // 
            this.buttonGetProcess.Location = new System.Drawing.Point(14, 20);
            this.buttonGetProcess.Name = "buttonGetProcess";
            this.buttonGetProcess.Size = new System.Drawing.Size(269, 25);
            this.buttonGetProcess.TabIndex = 240;
            this.buttonGetProcess.Text = "Получить список запущенных программ";
            this.buttonGetProcess.UseVisualStyleBackColor = true;
            this.buttonGetProcess.Click += new System.EventHandler(this.buttonGetProcess_Click);
            // 
            // checkBox6
            // 
            this.checkBox6.AutoSize = true;
            this.checkBox6.Location = new System.Drawing.Point(266, 221);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(15, 14);
            this.checkBox6.TabIndex = 238;
            this.checkBox6.Tag = "205";
            this.checkBox6.UseVisualStyleBackColor = true;
            // 
            // comboBoxProcess
            // 
            this.comboBoxProcess.FormattingEnabled = true;
            this.comboBoxProcess.Location = new System.Drawing.Point(14, 63);
            this.comboBoxProcess.Name = "comboBoxProcess";
            this.comboBoxProcess.Size = new System.Drawing.Size(268, 21);
            this.comboBoxProcess.TabIndex = 239;
            this.comboBoxProcess.SelectedIndexChanged += new System.EventHandler(this.comboBoxProcess_SelectedIndexChanged);
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(161, 221);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(15, 14);
            this.checkBox4.TabIndex = 236;
            this.checkBox4.Tag = "203";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // textBoxNameProgramm
            // 
            this.textBoxNameProgramm.Location = new System.Drawing.Point(14, 195);
            this.textBoxNameProgramm.Name = "textBoxNameProgramm";
            this.textBoxNameProgramm.Size = new System.Drawing.Size(268, 20);
            this.textBoxNameProgramm.TabIndex = 241;
            this.textBoxNameProgramm.TextChanged += new System.EventHandler(this.textBoxNameProgramm_TextChanged);
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(211, 221);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(15, 14);
            this.checkBox3.TabIndex = 235;
            this.checkBox3.Tag = "202";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBoxRunProcess
            // 
            this.checkBoxRunProcess.AutoSize = true;
            this.checkBoxRunProcess.Location = new System.Drawing.Point(62, 221);
            this.checkBoxRunProcess.Name = "checkBoxRunProcess";
            this.checkBoxRunProcess.Size = new System.Drawing.Size(15, 14);
            this.checkBoxRunProcess.TabIndex = 237;
            this.checkBoxRunProcess.Tag = "204";
            this.checkBoxRunProcess.UseVisualStyleBackColor = true;
            this.checkBoxRunProcess.CheckStateChanged += new System.EventHandler(this.checkBoxRunProcess_CheckedChanged);
            // 
            // checkBoxKillProcess
            // 
            this.checkBoxKillProcess.AutoSize = true;
            this.checkBoxKillProcess.Location = new System.Drawing.Point(16, 221);
            this.checkBoxKillProcess.Name = "checkBoxKillProcess";
            this.checkBoxKillProcess.Size = new System.Drawing.Size(15, 14);
            this.checkBoxKillProcess.TabIndex = 233;
            this.checkBoxKillProcess.Tag = "200";
            this.checkBoxKillProcess.UseVisualStyleBackColor = true;
            this.checkBoxKillProcess.CheckStateChanged += new System.EventHandler(this.checkBoxKillProcess_CheckedChanged);
            // 
            // tabPageLog
            // 
            this.tabPageLog.Controls.Add(this.textBoxLogs);
            this.tabPageLog.Location = new System.Drawing.Point(4, 22);
            this.tabPageLog.Name = "tabPageLog";
            this.tabPageLog.Size = new System.Drawing.Size(863, 405);
            this.tabPageLog.TabIndex = 3;
            this.tabPageLog.Text = "Логи работы";
            this.tabPageLog.UseVisualStyleBackColor = true;
            // 
            // textBoxLogs
            // 
            this.textBoxLogs.AcceptsReturn = true;
            this.textBoxLogs.AcceptsTab = true;
            this.textBoxLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLogs.Location = new System.Drawing.Point(0, 1);
            this.textBoxLogs.Multiline = true;
            this.textBoxLogs.Name = "textBoxLogs";
            this.textBoxLogs.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLogs.Size = new System.Drawing.Size(863, 409);
            this.textBoxLogs.TabIndex = 5;
            this.textBoxLogs.WordWrap = false;
            // 
            // tabDataGrid
            // 
            this.tabDataGrid.Controls.Add(this.pictureBoxLogoDataGrid);
            this.tabDataGrid.Controls.Add(this.dataGridView1);
            this.tabDataGrid.Location = new System.Drawing.Point(4, 22);
            this.tabDataGrid.Margin = new System.Windows.Forms.Padding(2);
            this.tabDataGrid.Name = "tabDataGrid";
            this.tabDataGrid.Padding = new System.Windows.Forms.Padding(2);
            this.tabDataGrid.Size = new System.Drawing.Size(863, 405);
            this.tabDataGrid.TabIndex = 4;
            this.tabDataGrid.Text = "Таблицы и отчеты";
            this.tabDataGrid.UseVisualStyleBackColor = true;
            // 
            // pictureBoxLogoDataGrid
            // 
            this.pictureBoxLogoDataGrid.Location = new System.Drawing.Point(280, 47);
            this.pictureBoxLogoDataGrid.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBoxLogoDataGrid.Name = "pictureBoxLogoDataGrid";
            this.pictureBoxLogoDataGrid.Size = new System.Drawing.Size(300, 300);
            this.pictureBoxLogoDataGrid.TabIndex = 236;
            this.pictureBoxLogoDataGrid.TabStop = false;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowDrop = true;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.ContextMenuStrip = this.contextMenuDataGrid;
            this.dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridView1.Location = new System.Drawing.Point(-1, -1);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(2);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(868, 407);
            this.dataGridView1.StandardTab = true;
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dataGridView1_EditingControlShowing);
            this.dataGridView1.DoubleClick += new System.EventHandler(this.dataGridView1_DoubleClick);
            // 
            // contextMenuDataGrid
            // 
            this.contextMenuDataGrid.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuDataGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.GetArchievesHostsItem,
            this.Separator7,
            this.GetPreviousScanItem,
            this.DeSelectUnknowItem,
            this.DeSelectUnknowNoPermissionItem,
            this.Separator0,
            this.ScannedNetsTime1,
            this.ScannedNetsTime2,
            this.ScannedNetsTime3,
            this.Separator5,
            this.GetArchieveOfHostItem,
            this.Separator1,
            this.PingItem,
            this.GetFileItems,
            this.GetRegistryItem,
            this.GetEventsItem,
            this.GetWholeDataItem});
            this.contextMenuDataGrid.Name = "contextMenuStrip1";
            this.contextMenuDataGrid.Size = new System.Drawing.Size(351, 314);
            // 
            // GetArchievesHostsItem
            // 
            this.GetArchievesHostsItem.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.GetArchievesHostsItem.Name = "GetArchievesHostsItem";
            this.GetArchievesHostsItem.Size = new System.Drawing.Size(350, 22);
            this.GetArchievesHostsItem.Text = "Архивы данных полученных с хостов";
            this.GetArchievesHostsItem.Click += new System.EventHandler(this.GetArchievesHostsItem_Click);
            // 
            // Separator7
            // 
            this.Separator7.Name = "Separator7";
            this.Separator7.Size = new System.Drawing.Size(347, 6);
            // 
            // GetPreviousScanItem
            // 
            this.GetPreviousScanItem.BackColor = System.Drawing.Color.LightSkyBlue;
            this.GetPreviousScanItem.Name = "GetPreviousScanItem";
            this.GetPreviousScanItem.Size = new System.Drawing.Size(350, 22);
            this.GetPreviousScanItem.Text = "Предыдущий поиск хостов";
            this.GetPreviousScanItem.Click += new System.EventHandler(this.GetPreviousScanItem_Click);
            // 
            // DeSelectUnknowItem
            // 
            this.DeSelectUnknowItem.Name = "DeSelectUnknowItem";
            this.DeSelectUnknowItem.Size = new System.Drawing.Size(350, 22);
            this.DeSelectUnknowItem.Text = "Исключить только Unknow хосты";
            this.DeSelectUnknowItem.ToolTipText = "Will deselect unknow hosts";
            this.DeSelectUnknowItem.Click += new System.EventHandler(this.DeSelectUnknowItem_Click);
            // 
            // DeSelectUnknowNoPermissionItem
            // 
            this.DeSelectUnknowNoPermissionItem.Name = "DeSelectUnknowNoPermissionItem";
            this.DeSelectUnknowNoPermissionItem.Size = new System.Drawing.Size(350, 22);
            this.DeSelectUnknowNoPermissionItem.Text = "Исключить недоступные хосты";
            this.DeSelectUnknowNoPermissionItem.ToolTipText = "Will be deselect unknow and without permission hosts";
            this.DeSelectUnknowNoPermissionItem.Click += new System.EventHandler(this.DeSelectUnknowNoPermissionItem_Click);
            // 
            // Separator0
            // 
            this.Separator0.Name = "Separator0";
            this.Separator0.Size = new System.Drawing.Size(347, 6);
            // 
            // ScannedNetsTime1
            // 
            this.ScannedNetsTime1.BackColor = System.Drawing.Color.LightSkyBlue;
            this.ScannedNetsTime1.Name = "ScannedNetsTime1";
            this.ScannedNetsTime1.Size = new System.Drawing.Size(350, 22);
            this.ScannedNetsTime1.Text = "ScannedNetsTime1";
            this.ScannedNetsTime1.Click += new System.EventHandler(this.ScannedNetsTime1_Click);
            // 
            // ScannedNetsTime2
            // 
            this.ScannedNetsTime2.BackColor = System.Drawing.Color.LightSkyBlue;
            this.ScannedNetsTime2.Name = "ScannedNetsTime2";
            this.ScannedNetsTime2.Size = new System.Drawing.Size(350, 22);
            this.ScannedNetsTime2.Text = "ScannedNetsTime2";
            this.ScannedNetsTime2.Click += new System.EventHandler(this.ScannedNetsTime2_Click);
            // 
            // ScannedNetsTime3
            // 
            this.ScannedNetsTime3.BackColor = System.Drawing.Color.LightSkyBlue;
            this.ScannedNetsTime3.Name = "ScannedNetsTime3";
            this.ScannedNetsTime3.Size = new System.Drawing.Size(350, 22);
            this.ScannedNetsTime3.Text = "ScannedNetsTime3";
            this.ScannedNetsTime3.Click += new System.EventHandler(this.ScannedNetsTime3_Click);
            // 
            // Separator5
            // 
            this.Separator5.Name = "Separator5";
            this.Separator5.Size = new System.Drawing.Size(347, 6);
            // 
            // GetArchieveOfHostItem
            // 
            this.GetArchieveOfHostItem.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.GetArchieveOfHostItem.Name = "GetArchieveOfHostItem";
            this.GetArchieveOfHostItem.Size = new System.Drawing.Size(350, 22);
            this.GetArchieveOfHostItem.Text = "Архив данных собранных с хоста";
            this.GetArchieveOfHostItem.Click += new System.EventHandler(this.GetArchiveOfHost_Click);
            // 
            // Separator1
            // 
            this.Separator1.Name = "Separator1";
            this.Separator1.Size = new System.Drawing.Size(347, 6);
            // 
            // PingItem
            // 
            this.PingItem.Name = "PingItem";
            this.PingItem.Size = new System.Drawing.Size(350, 22);
            this.PingItem.Text = "PingItem";
            this.PingItem.Click += new System.EventHandler(this.PingItem_Click);
            // 
            // GetFileItems
            // 
            this.GetFileItems.Name = "GetFileItems";
            this.GetFileItems.Size = new System.Drawing.Size(350, 22);
            this.GetFileItems.Text = "Получить список файлов с хоста";
            this.GetFileItems.Click += new System.EventHandler(this.GetFiles_Click);
            // 
            // GetRegistryItem
            // 
            this.GetRegistryItem.BackColor = System.Drawing.Color.SandyBrown;
            this.GetRegistryItem.Name = "GetRegistryItem";
            this.GetRegistryItem.Size = new System.Drawing.Size(350, 22);
            this.GetRegistryItem.Text = "Сканировать реестр  хоста";
            this.GetRegistryItem.ToolTipText = "Run only if the SIEM have ran with the highest privelegies";
            this.GetRegistryItem.Click += new System.EventHandler(this.GetRegistry_Click);
            // 
            // GetEventsItem
            // 
            this.GetEventsItem.BackColor = System.Drawing.Color.SandyBrown;
            this.GetEventsItem.Name = "GetEventsItem";
            this.GetEventsItem.Size = new System.Drawing.Size(350, 22);
            this.GetEventsItem.Text = "Загрузить список событий и конфигурацию хоста";
            this.GetEventsItem.Click += new System.EventHandler(this.GetEvents_Click);
            // 
            // GetWholeDataItem
            // 
            this.GetWholeDataItem.BackColor = System.Drawing.Color.SandyBrown;
            this.GetWholeDataItem.Name = "GetWholeDataItem";
            this.GetWholeDataItem.Size = new System.Drawing.Size(350, 22);
            this.GetWholeDataItem.Text = "Сбор всей информации c хоста";
            this.GetWholeDataItem.ToolTipText = "Run only if the SIEM have ran with the highest privelegies";
            this.GetWholeDataItem.Click += new System.EventHandler(this.GetWholeDataItem_Click);
            // 
            // tabPageRDP
            // 
            this.tabPageRDP.Controls.Add(this.RDP);
            this.tabPageRDP.Location = new System.Drawing.Point(4, 22);
            this.tabPageRDP.Name = "tabPageRDP";
            this.tabPageRDP.Size = new System.Drawing.Size(863, 405);
            this.tabPageRDP.TabIndex = 6;
            this.tabPageRDP.Text = "RDP";
            this.tabPageRDP.UseVisualStyleBackColor = true;
            // 
            // RDP
            // 
            this.RDP.AllowDrop = true;
            this.RDP.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RDP.Enabled = true;
            this.RDP.Location = new System.Drawing.Point(0, 0);
            this.RDP.Name = "RDP";
            this.RDP.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("RDP.OcxState")));
            this.RDP.Size = new System.Drawing.Size(867, 405);
            this.RDP.TabIndex = 0;
            // 
            // comboUsers
            // 
            this.comboUsers.FormattingEnabled = true;
            this.comboUsers.Location = new System.Drawing.Point(502, 63);
            this.comboUsers.Name = "comboUsers";
            this.comboUsers.Size = new System.Drawing.Size(157, 21);
            this.comboUsers.TabIndex = 243;
            // 
            // listBoxNetsRow
            // 
            this.listBoxNetsRow.Location = new System.Drawing.Point(725, 29);
            this.listBoxNetsRow.Margin = new System.Windows.Forms.Padding(2);
            this.listBoxNetsRow.Name = "listBoxNetsRow";
            this.listBoxNetsRow.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.listBoxNetsRow.Size = new System.Drawing.Size(140, 56);
            this.listBoxNetsRow.TabIndex = 2;
            this.listBoxNetsRow.SelectedIndexChanged += new System.EventHandler(this.listBoxNetsRow_SelectedIndexChanged);
            this.listBoxNetsRow.Enter += new System.EventHandler(this.listBoxListOfNet_Enter);
            this.listBoxNetsRow.Leave += new System.EventHandler(this.listBoxListOfNet_Leave);
            // 
            // textBoxLogin
            // 
            this.textBoxLogin.Location = new System.Drawing.Point(57, 29);
            this.textBoxLogin.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxLogin.Name = "textBoxLogin";
            this.textBoxLogin.Size = new System.Drawing.Size(98, 20);
            this.textBoxLogin.TabIndex = 5;
            this.textBoxLogin.TextChanged += new System.EventHandler(this.textBoxUser_TextChanged);
            this.textBoxLogin.Enter += new System.EventHandler(this.textBoxUser_Enter);
            this.textBoxLogin.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxUser_KeyUp);
            this.textBoxLogin.Leave += new System.EventHandler(this.textBoxUser_Leave);
            // 
            // textBoxDomain
            // 
            this.textBoxDomain.Location = new System.Drawing.Point(224, 29);
            this.textBoxDomain.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxDomain.Name = "textBoxDomain";
            this.textBoxDomain.Size = new System.Drawing.Size(199, 20);
            this.textBoxDomain.TabIndex = 6;
            this.textBoxDomain.Enter += new System.EventHandler(this.textBoxDomain_Enter);
            this.textBoxDomain.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxDomain_KeyUp);
            this.textBoxDomain.Leave += new System.EventHandler(this.textBoxDomain_Leave);
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(57, 63);
            this.textBoxPassword.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(98, 20);
            this.textBoxPassword.TabIndex = 7;
            this.textBoxPassword.Enter += new System.EventHandler(this.textBoxPassword_Enter);
            this.textBoxPassword.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxPassword_KeyUp);
            this.textBoxPassword.Leave += new System.EventHandler(this.textBoxPassword_Leave);
            // 
            // labelLicense
            // 
            this.labelLicense.BackColor = System.Drawing.Color.Transparent;
            this.labelLicense.Location = new System.Drawing.Point(59, 27);
            this.labelLicense.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelLicense.Name = "labelLicense";
            this.labelLicense.Size = new System.Drawing.Size(163, 23);
            this.labelLicense.TabIndex = 8;
            this.labelLicense.Text = "Домен:";
            this.labelLicense.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelLogin
            // 
            this.labelLogin.BackColor = System.Drawing.Color.Transparent;
            this.labelLogin.Location = new System.Drawing.Point(4, 28);
            this.labelLogin.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelLogin.Name = "labelLogin";
            this.labelLogin.Size = new System.Drawing.Size(152, 22);
            this.labelLogin.TabIndex = 9;
            this.labelLogin.Text = "Логин:";
            this.labelLogin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelPassword
            // 
            this.labelPassword.Location = new System.Drawing.Point(4, 62);
            this.labelPassword.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(152, 22);
            this.labelPassword.TabIndex = 10;
            this.labelPassword.Text = "Пароль:";
            this.labelPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelInputNetOrInputPC
            // 
            this.labelInputNetOrInputPC.Location = new System.Drawing.Point(428, 28);
            this.labelInputNetOrInputPC.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelInputNetOrInputPC.Name = "labelInputNetOrInputPC";
            this.labelInputNetOrInputPC.Size = new System.Drawing.Size(201, 22);
            this.labelInputNetOrInputPC.TabIndex = 11;
            this.labelInputNetOrInputPC.Text = "Сеть/хост:";
            this.labelInputNetOrInputPC.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBoxTargedPC
            // 
            this.comboBoxTargedPC.FormattingEnabled = true;
            this.comboBoxTargedPC.Location = new System.Drawing.Point(224, 63);
            this.comboBoxTargedPC.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxTargedPC.Name = "comboBoxTargedPC";
            this.comboBoxTargedPC.Size = new System.Drawing.Size(199, 21);
            this.comboBoxTargedPC.Sorted = true;
            this.comboBoxTargedPC.TabIndex = 9;
            this.comboBoxTargedPC.SelectedIndexChanged += new System.EventHandler(this.comboBoxTargedPC_SelectedIndexChanged);
            this.comboBoxTargedPC.Enter += new System.EventHandler(this.comboBoxTargedPC_Enter);
            this.comboBoxTargedPC.Leave += new System.EventHandler(this.comboBoxTargedPC_Leave);
            // 
            // textBoxInputNetOrInputPC
            // 
            this.textBoxInputNetOrInputPC.Location = new System.Drawing.Point(487, 29);
            this.textBoxInputNetOrInputPC.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxInputNetOrInputPC.Name = "textBoxInputNetOrInputPC";
            this.textBoxInputNetOrInputPC.Size = new System.Drawing.Size(141, 20);
            this.textBoxInputNetOrInputPC.TabIndex = 8;
            this.textBoxInputNetOrInputPC.TextChanged += new System.EventHandler(this.textBoxInputNetOrInputPC_TextChanged);
            this.textBoxInputNetOrInputPC.Enter += new System.EventHandler(this.textBoxInputNetOrInputPC_Enter_1);
            this.textBoxInputNetOrInputPC.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxInputNetOrInputPC_KeyUp);
            this.textBoxInputNetOrInputPC.Leave += new System.EventHandler(this.textBoxInputNetOrInputPC_KeyUp);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MainMenu,
            this.MenuFunction,
            this.MenuHelp});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(871, 24);
            this.menuStrip1.TabIndex = 18;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // MainMenu
            // 
            this.MainMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ResetAuthorizationItem,
            this.ChangePCItem,
            this.CorrectAccessErrorItem,
            this.ExitItem});
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Size = new System.Drawing.Size(74, 20);
            this.MainMenu.Text = "Основное";
            // 
            // ResetAuthorizationItem
            // 
            this.ResetAuthorizationItem.Name = "ResetAuthorizationItem";
            this.ResetAuthorizationItem.Size = new System.Drawing.Size(278, 22);
            this.ResetAuthorizationItem.Text = "Сбросить авторизацию";
            this.ResetAuthorizationItem.Click += new System.EventHandler(this.resetI_Click);
            // 
            // ChangePCItem
            // 
            this.ChangePCItem.Name = "ChangePCItem";
            this.ChangePCItem.Size = new System.Drawing.Size(278, 22);
            this.ChangePCItem.Text = "Сменить ПК, очистить таблицы";
            this.ChangePCItem.Click += new System.EventHandler(this.changePC_Click);
            // 
            // CorrectAccessErrorItem
            // 
            this.CorrectAccessErrorItem.Name = "CorrectAccessErrorItem";
            this.CorrectAccessErrorItem.Size = new System.Drawing.Size(278, 22);
            this.CorrectAccessErrorItem.Text = "Исправить ошибки доступа к шарам";
            this.CorrectAccessErrorItem.ToolTipText = "Исправление \"Ошибки 86\" при доступе к шарам WindowsXP. Нужно перезагрузить данный" +
    " ПК после этой команды";
            this.CorrectAccessErrorItem.Click += new System.EventHandler(this.CorrectAccessErrorItem_Click);
            // 
            // ExitItem
            // 
            this.ExitItem.Name = "ExitItem";
            this.ExitItem.Size = new System.Drawing.Size(278, 22);
            this.ExitItem.Text = "Выход";
            this.ExitItem.Click += new System.EventHandler(this.ApplicationExit);
            // 
            // MenuFunction
            // 
            this.MenuFunction.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SearchMenu,
            this.SmartSearchMenu,
            this.GetDataMenu,
            this.DBMenu,
            this.AnalyseDataMenu,
            this.ControlHostMenu,
            this.StopSearchItem,
            this.SignerItem,
            this.SignerRecoverItem,
            this.SignGnerateLicenseItem});
            this.MenuFunction.Name = "MenuFunction";
            this.MenuFunction.Size = new System.Drawing.Size(68, 20);
            this.MenuFunction.Text = "Функции";
            this.MenuFunction.DropDownOpening += new System.EventHandler(this.GetDataMenu_DropDownOpened);
            // 
            // SearchMenu
            // 
            this.SearchMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ScanEnvironmentItem,
            this.ScanSelectedNetItem,
            this.GetHostsFromDomainItem,
            this.Separator2,
            this.PingHostItem,
            this.Separator4,
            this.ArchievGotFromNetsItem,
            this.Separator6,
            this.CurrentTCPConnectionsItem});
            this.SearchMenu.Name = "SearchMenu";
            this.SearchMenu.Size = new System.Drawing.Size(216, 22);
            this.SearchMenu.Text = "Поиск хостов";
            this.SearchMenu.ToolTipText = "Scan Environment";
            // 
            // ScanEnvironmentItem
            // 
            this.ScanEnvironmentItem.Name = "ScanEnvironmentItem";
            this.ScanEnvironmentItem.Size = new System.Drawing.Size(345, 22);
            this.ScanEnvironmentItem.Text = "Поиск ближайших сетей";
            this.ScanEnvironmentItem.Click += new System.EventHandler(this.scanEnvironmentItem_Click);
            // 
            // ScanSelectedNetItem
            // 
            this.ScanSelectedNetItem.Name = "ScanSelectedNetItem";
            this.ScanSelectedNetItem.Size = new System.Drawing.Size(345, 22);
            this.ScanSelectedNetItem.Text = "Сканировать выбранные сети";
            this.ScanSelectedNetItem.Click += new System.EventHandler(this.scanSelectedNetworkItem_Click);
            // 
            // GetHostsFromDomainItem
            // 
            this.GetHostsFromDomainItem.Name = "GetHostsFromDomainItem";
            this.GetHostsFromDomainItem.Size = new System.Drawing.Size(345, 22);
            this.GetHostsFromDomainItem.Text = "Получить перечень хостов и логинов из домена";
            this.GetHostsFromDomainItem.Click += new System.EventHandler(this.getHostsFromDomainMenu_Click);
            // 
            // Separator2
            // 
            this.Separator2.Name = "Separator2";
            this.Separator2.Size = new System.Drawing.Size(342, 6);
            // 
            // PingHostItem
            // 
            this.PingHostItem.Name = "PingHostItem";
            this.PingHostItem.Size = new System.Drawing.Size(345, 22);
            this.PingHostItem.Text = "Пинг выбранного хоста";
            this.PingHostItem.Click += new System.EventHandler(this.PingHostItem_Click);
            // 
            // Separator4
            // 
            this.Separator4.Name = "Separator4";
            this.Separator4.Size = new System.Drawing.Size(342, 6);
            // 
            // ArchievGotFromNetsItem
            // 
            this.ArchievGotFromNetsItem.BackColor = System.Drawing.Color.LightSkyBlue;
            this.ArchievGotFromNetsItem.Name = "ArchievGotFromNetsItem";
            this.ArchievGotFromNetsItem.Size = new System.Drawing.Size(345, 22);
            this.ArchievGotFromNetsItem.Text = "Загрузить предыдущий поиск хостов";
            this.ArchievGotFromNetsItem.Click += new System.EventHandler(this.archievGotFromNetsItem_Click);
            // 
            // Separator6
            // 
            this.Separator6.Name = "Separator6";
            this.Separator6.Size = new System.Drawing.Size(342, 6);
            // 
            // CurrentTCPConnectionsItem
            // 
            this.CurrentTCPConnectionsItem.Name = "CurrentTCPConnectionsItem";
            this.CurrentTCPConnectionsItem.Size = new System.Drawing.Size(345, 22);
            this.CurrentTCPConnectionsItem.Text = "Открытые сетевые соединения локального хоста";
            this.CurrentTCPConnectionsItem.Click += new System.EventHandler(this.currentTCPConnectionsItem_Click);
            // 
            // SmartSearchMenu
            // 
            this.SmartSearchMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.smartLoginToolStripMenuItem,
            this.smartWordToolStripMenuItem,
            this.smartLoginAndWordToolStripMenuItem});
            this.SmartSearchMenu.Name = "SmartSearchMenu";
            this.SmartSearchMenu.Size = new System.Drawing.Size(216, 22);
            this.SmartSearchMenu.Text = "Интеллектуальный поиск";
            this.SmartSearchMenu.ToolTipText = "AI Search";
            // 
            // smartLoginToolStripMenuItem
            // 
            this.smartLoginToolStripMenuItem.Name = "smartLoginToolStripMenuItem";
            this.smartLoginToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.smartLoginToolStripMenuItem.Text = "Smart Login";
            // 
            // smartWordToolStripMenuItem
            // 
            this.smartWordToolStripMenuItem.Name = "smartWordToolStripMenuItem";
            this.smartWordToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.smartWordToolStripMenuItem.Text = "SmartWord";
            // 
            // smartLoginAndWordToolStripMenuItem
            // 
            this.smartLoginAndWordToolStripMenuItem.Name = "smartLoginAndWordToolStripMenuItem";
            this.smartLoginAndWordToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.smartLoginAndWordToolStripMenuItem.Text = "Smart LoginAndWord";
            // 
            // GetDataMenu
            // 
            this.GetDataMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.GetLogRemotePCItem,
            this.GetFilesItem,
            this.GetRegItem,
            this.Separator8,
            this.FullScanItem,
            this.Separator3,
            this.GetDataHostsArchievItem});
            this.GetDataMenu.Name = "GetDataMenu";
            this.GetDataMenu.Size = new System.Drawing.Size(216, 22);
            this.GetDataMenu.Text = "Сбор информации";
            this.GetDataMenu.ToolTipText = "Find out Information of the target";
            this.GetDataMenu.DropDownOpening += new System.EventHandler(this.GetDataMenu_DropDownOpened);
            // 
            // GetLogRemotePCItem
            // 
            this.GetLogRemotePCItem.BackColor = System.Drawing.Color.SandyBrown;
            this.GetLogRemotePCItem.Name = "GetLogRemotePCItem";
            this.GetLogRemotePCItem.Size = new System.Drawing.Size(318, 22);
            this.GetLogRemotePCItem.Text = "Загрузить список событий и конфигурацию";
            this.GetLogRemotePCItem.Click += new System.EventHandler(this.loadRemoteLogsItem_Click);
            // 
            // GetFilesItem
            // 
            this.GetFilesItem.Name = "GetFilesItem";
            this.GetFilesItem.Size = new System.Drawing.Size(318, 22);
            this.GetFilesItem.Text = "Получить список файлов";
            this.GetFilesItem.Click += new System.EventHandler(this.searchFilesMenu_Click);
            // 
            // GetRegItem
            // 
            this.GetRegItem.BackColor = System.Drawing.Color.SandyBrown;
            this.GetRegItem.Name = "GetRegItem";
            this.GetRegItem.Size = new System.Drawing.Size(318, 22);
            this.GetRegItem.Text = "Сканировать реестр";
            this.GetRegItem.ToolTipText = "Run only if the SIEM have ran with the highest privelegies";
            this.GetRegItem.Click += new System.EventHandler(this.regScanItem_Click);
            // 
            // Separator8
            // 
            this.Separator8.Name = "Separator8";
            this.Separator8.Size = new System.Drawing.Size(315, 6);
            // 
            // FullScanItem
            // 
            this.FullScanItem.BackColor = System.Drawing.Color.SandyBrown;
            this.FullScanItem.Name = "FullScanItem";
            this.FullScanItem.Size = new System.Drawing.Size(318, 22);
            this.FullScanItem.Text = "Сбор всей информации";
            this.FullScanItem.ToolTipText = "Run only if the SIEM have ran with the highest privelegies";
            this.FullScanItem.Click += new System.EventHandler(this.GetFullDataMenu_Click);
            // 
            // Separator3
            // 
            this.Separator3.Name = "Separator3";
            this.Separator3.Size = new System.Drawing.Size(315, 6);
            // 
            // GetDataHostsArchievItem
            // 
            this.GetDataHostsArchievItem.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.GetDataHostsArchievItem.Name = "GetDataHostsArchievItem";
            this.GetDataHostsArchievItem.Size = new System.Drawing.Size(318, 22);
            this.GetDataHostsArchievItem.Text = "Список архивов  хостов";
            this.GetDataHostsArchievItem.Click += new System.EventHandler(this.GetDataHostsArchievItem_Click);
            // 
            // DBMenu
            // 
            this.DBMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DBCreateTestItem,
            this.DBInsertTestItem,
            this.DBReadNamesOfTablesItem,
            this.DBReadColumnsOfTheTableItem,
            this.DBHostsAccessItem,
            this.DBUsersPrivateItem});
            this.DBMenu.Name = "DBMenu";
            this.DBMenu.Size = new System.Drawing.Size(216, 22);
            this.DBMenu.Text = "База Данных";
            this.DBMenu.ToolTipText = "Service of DB";
            // 
            // DBCreateTestItem
            // 
            this.DBCreateTestItem.Name = "DBCreateTestItem";
            this.DBCreateTestItem.Size = new System.Drawing.Size(234, 22);
            this.DBCreateTestItem.Text = "DB-CreateDB";
            this.DBCreateTestItem.Click += new System.EventHandler(this.DBCreateTestItem_Click);
            // 
            // DBInsertTestItem
            // 
            this.DBInsertTestItem.Name = "DBInsertTestItem";
            this.DBInsertTestItem.Size = new System.Drawing.Size(234, 22);
            this.DBInsertTestItem.Text = "DB-Insert Test";
            this.DBInsertTestItem.Click += new System.EventHandler(this.dBInsertToolStripMenuItem_Click);
            // 
            // DBReadNamesOfTablesItem
            // 
            this.DBReadNamesOfTablesItem.Name = "DBReadNamesOfTablesItem";
            this.DBReadNamesOfTablesItem.Size = new System.Drawing.Size(234, 22);
            this.DBReadNamesOfTablesItem.Text = "DB-Read  Names of Tables";
            this.DBReadNamesOfTablesItem.Click += new System.EventHandler(this.dBReadNamesOfTablesToolStripMenuItem_Click);
            // 
            // DBReadColumnsOfTheTableItem
            // 
            this.DBReadColumnsOfTheTableItem.Name = "DBReadColumnsOfTheTableItem";
            this.DBReadColumnsOfTheTableItem.Size = new System.Drawing.Size(234, 22);
            this.DBReadColumnsOfTheTableItem.Text = "DB-Read Columns of the table";
            this.DBReadColumnsOfTheTableItem.Click += new System.EventHandler(this.dBReadColumnsOfTheTableToolStripMenuItem_Click);
            // 
            // DBHostsAccessItem
            // 
            this.DBHostsAccessItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DBHostsAccessDeleteRowItem});
            this.DBHostsAccessItem.Name = "DBHostsAccessItem";
            this.DBHostsAccessItem.Size = new System.Drawing.Size(234, 22);
            this.DBHostsAccessItem.Text = "База с доступами к хостам";
            this.DBHostsAccessItem.Click += new System.EventHandler(this.dBHostsAccessItem_Click);
            // 
            // DBHostsAccessDeleteRowItem
            // 
            this.DBHostsAccessDeleteRowItem.Name = "DBHostsAccessDeleteRowItem";
            this.DBHostsAccessDeleteRowItem.Size = new System.Drawing.Size(231, 22);
            this.DBHostsAccessDeleteRowItem.Text = "Удалить выделенную запись";
            this.DBHostsAccessDeleteRowItem.Click += new System.EventHandler(this.DBHostsAccessDeleteRowItem_Click);
            // 
            // DBUsersPrivateItem
            // 
            this.DBUsersPrivateItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FindTablesFitIntoPersonalItem,
            this.DBUsersPrivateDeleteRowItem,
            this.ShrinkDBPersonalItem,
            this.ImportExcelDataItem,
            this.ViewStoredDataPersonalMenuItem,
            this.ImportExcelDataIntoDBMenuItem});
            this.DBUsersPrivateItem.Name = "DBUsersPrivateItem";
            this.DBUsersPrivateItem.Size = new System.Drawing.Size(234, 22);
            this.DBUsersPrivateItem.Text = "База людей";
            this.DBUsersPrivateItem.Click += new System.EventHandler(this.DBUsersPrivateItem_Click);
            // 
            // FindTablesFitIntoPersonalItem
            // 
            this.FindTablesFitIntoPersonalItem.Name = "FindTablesFitIntoPersonalItem";
            this.FindTablesFitIntoPersonalItem.Size = new System.Drawing.Size(291, 22);
            this.FindTablesFitIntoPersonalItem.Text = "Определить таблицы для импорта в БД";
            this.FindTablesFitIntoPersonalItem.ToolTipText = "Попытаться определить таблицы в БД, подходящие для импортируемого Excel-файла";
            this.FindTablesFitIntoPersonalItem.Click += new System.EventHandler(this.findTablesFitItem_Click);
            // 
            // DBUsersPrivateDeleteRowItem
            // 
            this.DBUsersPrivateDeleteRowItem.Name = "DBUsersPrivateDeleteRowItem";
            this.DBUsersPrivateDeleteRowItem.Size = new System.Drawing.Size(291, 22);
            this.DBUsersPrivateDeleteRowItem.Text = "Удалить выделенную запись";
            this.DBUsersPrivateDeleteRowItem.Click += new System.EventHandler(this.DBUsersPrivateDeleteRowItem_Click);
            // 
            // ShrinkDBPersonalItem
            // 
            this.ShrinkDBPersonalItem.Name = "ShrinkDBPersonalItem";
            this.ShrinkDBPersonalItem.Size = new System.Drawing.Size(291, 22);
            this.ShrinkDBPersonalItem.Text = "Сжать базу";
            this.ShrinkDBPersonalItem.Click += new System.EventHandler(this.ShrinkDBItem_Click);
            // 
            // ImportExcelDataItem
            // 
            this.ImportExcelDataItem.Name = "ImportExcelDataItem";
            this.ImportExcelDataItem.Size = new System.Drawing.Size(291, 22);
            this.ImportExcelDataItem.Text = "ImportExcelData (Test)";
            this.ImportExcelDataItem.Visible = false;
            this.ImportExcelDataItem.Click += new System.EventHandler(this.ImportDataItem_Click);
            // 
            // ViewStoredDataPersonalMenuItem
            // 
            this.ViewStoredDataPersonalMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ViewStoredDataPersonalMainItem,
            this.ViewStoredDataPersonalPhonesItem,
            this.ViewStoredDataPersonalAddressItem,
            this.ViewStoredDataPersonalWorkItem,
            this.ViewStoredDataPersonalLearnItem,
            this.ViewStoredDataPersonalDocumentItem,
            this.ViewStoredDataPersonalRelationItem});
            this.ViewStoredDataPersonalMenuItem.Name = "ViewStoredDataPersonalMenuItem";
            this.ViewStoredDataPersonalMenuItem.Size = new System.Drawing.Size(291, 22);
            this.ViewStoredDataPersonalMenuItem.Text = "Вывести на экран";
            this.ViewStoredDataPersonalMenuItem.ToolTipText = "Вывести на экран таблицу из БД";
            this.ViewStoredDataPersonalMenuItem.Click += new System.EventHandler(this.importDataIntoDataGridItem_Click);
            // 
            // ViewStoredDataPersonalMainItem
            // 
            this.ViewStoredDataPersonalMainItem.Name = "ViewStoredDataPersonalMainItem";
            this.ViewStoredDataPersonalMainItem.Size = new System.Drawing.Size(130, 22);
            this.ViewStoredDataPersonalMainItem.Text = "Main";
            this.ViewStoredDataPersonalMainItem.Click += new System.EventHandler(this.mainToolStripMenuItem_Click);
            // 
            // ViewStoredDataPersonalPhonesItem
            // 
            this.ViewStoredDataPersonalPhonesItem.Name = "ViewStoredDataPersonalPhonesItem";
            this.ViewStoredDataPersonalPhonesItem.Size = new System.Drawing.Size(130, 22);
            this.ViewStoredDataPersonalPhonesItem.Text = "Phones";
            this.ViewStoredDataPersonalPhonesItem.Click += new System.EventHandler(this.phonesToolStripMenuItem_Click);
            // 
            // ViewStoredDataPersonalAddressItem
            // 
            this.ViewStoredDataPersonalAddressItem.Name = "ViewStoredDataPersonalAddressItem";
            this.ViewStoredDataPersonalAddressItem.Size = new System.Drawing.Size(130, 22);
            this.ViewStoredDataPersonalAddressItem.Text = "Address";
            this.ViewStoredDataPersonalAddressItem.Click += new System.EventHandler(this.addressToolStripMenuItem_Click);
            // 
            // ViewStoredDataPersonalWorkItem
            // 
            this.ViewStoredDataPersonalWorkItem.Name = "ViewStoredDataPersonalWorkItem";
            this.ViewStoredDataPersonalWorkItem.Size = new System.Drawing.Size(130, 22);
            this.ViewStoredDataPersonalWorkItem.Text = "Work";
            this.ViewStoredDataPersonalWorkItem.Click += new System.EventHandler(this.ViewStoredDataPersonalWorkItem_Click);
            // 
            // ViewStoredDataPersonalLearnItem
            // 
            this.ViewStoredDataPersonalLearnItem.Name = "ViewStoredDataPersonalLearnItem";
            this.ViewStoredDataPersonalLearnItem.Size = new System.Drawing.Size(130, 22);
            this.ViewStoredDataPersonalLearnItem.Text = "Learn";
            this.ViewStoredDataPersonalLearnItem.Click += new System.EventHandler(this.ViewStoredDataPersonalLearnItem_Click);
            // 
            // ViewStoredDataPersonalDocumentItem
            // 
            this.ViewStoredDataPersonalDocumentItem.Name = "ViewStoredDataPersonalDocumentItem";
            this.ViewStoredDataPersonalDocumentItem.Size = new System.Drawing.Size(130, 22);
            this.ViewStoredDataPersonalDocumentItem.Text = "Document";
            this.ViewStoredDataPersonalDocumentItem.Click += new System.EventHandler(this.ViewStoredDataPersonalDocumentItem_Click);
            // 
            // ViewStoredDataPersonalRelationItem
            // 
            this.ViewStoredDataPersonalRelationItem.Name = "ViewStoredDataPersonalRelationItem";
            this.ViewStoredDataPersonalRelationItem.Size = new System.Drawing.Size(130, 22);
            this.ViewStoredDataPersonalRelationItem.Text = "Relation";
            this.ViewStoredDataPersonalRelationItem.Click += new System.EventHandler(this.ViewStoredDataPersonalRelationItem_Click);
            // 
            // ImportExcelDataIntoDBMenuItem
            // 
            this.ImportExcelDataIntoDBMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ImportExcelDataIntoDbMainItem,
            this.ImportExcelDataIntoDbPhoneItem,
            this.ImportExcelDataIntoDbAddressItem,
            this.ImportExcelDataIntoDbWorkItem,
            this.ImportExcelDataIntoDbLearnItem,
            this.ImportExcelDataIntoDbDocumentItem,
            this.ImportExcelDataIntoDbRelationItem});
            this.ImportExcelDataIntoDBMenuItem.Name = "ImportExcelDataIntoDBMenuItem";
            this.ImportExcelDataIntoDBMenuItem.Size = new System.Drawing.Size(291, 22);
            this.ImportExcelDataIntoDBMenuItem.Text = "Импорт в БД";
            this.ImportExcelDataIntoDBMenuItem.ToolTipText = "Импорт таблицы из Excel-file в БД";
            this.ImportExcelDataIntoDBMenuItem.Click += new System.EventHandler(this.ImportExcelDataIntoDBItem_Click);
            // 
            // ImportExcelDataIntoDbMainItem
            // 
            this.ImportExcelDataIntoDbMainItem.Name = "ImportExcelDataIntoDbMainItem";
            this.ImportExcelDataIntoDbMainItem.Size = new System.Drawing.Size(130, 22);
            this.ImportExcelDataIntoDbMainItem.Text = "Main";
            this.ImportExcelDataIntoDbMainItem.Click += new System.EventHandler(this.ImportExcelDataIntoDbMainItem_Click);
            // 
            // ImportExcelDataIntoDbPhoneItem
            // 
            this.ImportExcelDataIntoDbPhoneItem.Name = "ImportExcelDataIntoDbPhoneItem";
            this.ImportExcelDataIntoDbPhoneItem.Size = new System.Drawing.Size(130, 22);
            this.ImportExcelDataIntoDbPhoneItem.Text = "Phone";
            this.ImportExcelDataIntoDbPhoneItem.Click += new System.EventHandler(this.ImportExcelDataIntoDbPhoneItem_Click);
            // 
            // ImportExcelDataIntoDbAddressItem
            // 
            this.ImportExcelDataIntoDbAddressItem.Name = "ImportExcelDataIntoDbAddressItem";
            this.ImportExcelDataIntoDbAddressItem.Size = new System.Drawing.Size(130, 22);
            this.ImportExcelDataIntoDbAddressItem.Text = "Address";
            this.ImportExcelDataIntoDbAddressItem.Click += new System.EventHandler(this.ImportExcelDataIntoDbAddressItem_Click);
            // 
            // ImportExcelDataIntoDbWorkItem
            // 
            this.ImportExcelDataIntoDbWorkItem.Name = "ImportExcelDataIntoDbWorkItem";
            this.ImportExcelDataIntoDbWorkItem.Size = new System.Drawing.Size(130, 22);
            this.ImportExcelDataIntoDbWorkItem.Text = "Work";
            this.ImportExcelDataIntoDbWorkItem.Click += new System.EventHandler(this.ImportExcelDataIntoDbWorkItem_Click);
            // 
            // ImportExcelDataIntoDbLearnItem
            // 
            this.ImportExcelDataIntoDbLearnItem.Name = "ImportExcelDataIntoDbLearnItem";
            this.ImportExcelDataIntoDbLearnItem.Size = new System.Drawing.Size(130, 22);
            this.ImportExcelDataIntoDbLearnItem.Text = "Learn";
            this.ImportExcelDataIntoDbLearnItem.Click += new System.EventHandler(this.ImportExcelDataIntoDbLearnItem_Click);
            // 
            // ImportExcelDataIntoDbDocumentItem
            // 
            this.ImportExcelDataIntoDbDocumentItem.Name = "ImportExcelDataIntoDbDocumentItem";
            this.ImportExcelDataIntoDbDocumentItem.Size = new System.Drawing.Size(130, 22);
            this.ImportExcelDataIntoDbDocumentItem.Text = "Document";
            this.ImportExcelDataIntoDbDocumentItem.Click += new System.EventHandler(this.ImportExcelDataIntoDbDocumentItem_Click);
            // 
            // ImportExcelDataIntoDbRelationItem
            // 
            this.ImportExcelDataIntoDbRelationItem.Name = "ImportExcelDataIntoDbRelationItem";
            this.ImportExcelDataIntoDbRelationItem.Size = new System.Drawing.Size(130, 22);
            this.ImportExcelDataIntoDbRelationItem.Text = "Relation";
            this.ImportExcelDataIntoDbRelationItem.Click += new System.EventHandler(this.ImportExcelDataIntoDbRelationItem_Click);
            // 
            // AnalyseDataMenu
            // 
            this.AnalyseDataMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.listTablesDBItem});
            this.AnalyseDataMenu.Name = "AnalyseDataMenu";
            this.AnalyseDataMenu.Size = new System.Drawing.Size(216, 22);
            this.AnalyseDataMenu.Text = "Анализ данных";
            this.AnalyseDataMenu.Click += new System.EventHandler(this.AnalyseDataMenu_Click);
            // 
            // listTablesDBItem
            // 
            this.listTablesDBItem.Name = "listTablesDBItem";
            this.listTablesDBItem.Size = new System.Drawing.Size(271, 22);
            this.listTablesDBItem.Text = "Список таблиц базы (в Лог Работы)";
            this.listTablesDBItem.Click += new System.EventHandler(this.listTablesDBItem_Click);
            // 
            // ControlHostMenu
            // 
            this.ControlHostMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RDPMenuItem});
            this.ControlHostMenu.Name = "ControlHostMenu";
            this.ControlHostMenu.Size = new System.Drawing.Size(216, 22);
            this.ControlHostMenu.Text = "Управлять Хостом";
            this.ControlHostMenu.ToolTipText = "Control of the Selected target";
            this.ControlHostMenu.Click += new System.EventHandler(this.ControlHostMenu_Click);
            // 
            // RDPMenuItem
            // 
            this.RDPMenuItem.Name = "RDPMenuItem";
            this.RDPMenuItem.Size = new System.Drawing.Size(96, 22);
            this.RDPMenuItem.Text = "RDP";
            this.RDPMenuItem.ToolTipText = "Подключиться/отключиться по RDP";
            this.RDPMenuItem.Click += new System.EventHandler(this.RDPMenuItem_Click);
            // 
            // StopSearchItem
            // 
            this.StopSearchItem.Name = "StopSearchItem";
            this.StopSearchItem.Size = new System.Drawing.Size(216, 22);
            this.StopSearchItem.Text = "Прервать поиск";
            this.StopSearchItem.ToolTipText = "STOP any searching action";
            this.StopSearchItem.Click += new System.EventHandler(this.StopSearchItem_Click);
            // 
            // SignerItem
            // 
            this.SignerItem.Name = "SignerItem";
            this.SignerItem.Size = new System.Drawing.Size(216, 22);
            this.SignerItem.Text = "Signer";
            this.SignerItem.ToolTipText = "Make Private.key and Public.key";
            this.SignerItem.Visible = false;
            this.SignerItem.Click += new System.EventHandler(this.SignerItem_Click);
            // 
            // SignerRecoverItem
            // 
            this.SignerRecoverItem.Name = "SignerRecoverItem";
            this.SignerRecoverItem.Size = new System.Drawing.Size(216, 22);
            this.SignerRecoverItem.Text = "SignerRecover";
            this.SignerRecoverItem.ToolTipText = "Make key";
            this.SignerRecoverItem.Visible = false;
            this.SignerRecoverItem.Click += new System.EventHandler(this.SignerRecoverItem_Click);
            // 
            // SignGnerateLicenseItem
            // 
            this.SignGnerateLicenseItem.Name = "SignGnerateLicenseItem";
            this.SignGnerateLicenseItem.Size = new System.Drawing.Size(216, 22);
            this.SignGnerateLicenseItem.Text = "SignGenerateLicense";
            this.SignGnerateLicenseItem.ToolTipText = "Make myELLic.key";
            this.SignGnerateLicenseItem.Click += new System.EventHandler(this.SignGenerateLicenseItem_Click);
            // 
            // MenuHelp
            // 
            this.MenuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LicenseItem,
            this.HelpItem,
            this.AboutItem,
            this.importExcelItem});
            this.MenuHelp.Name = "MenuHelp";
            this.MenuHelp.Size = new System.Drawing.Size(65, 20);
            this.MenuHelp.Text = "Справка";
            // 
            // LicenseItem
            // 
            this.LicenseItem.Name = "LicenseItem";
            this.LicenseItem.Size = new System.Drawing.Size(187, 22);
            this.LicenseItem.Text = "License";
            this.LicenseItem.Click += new System.EventHandler(this.licenseItem_Click);
            // 
            // HelpItem
            // 
            this.HelpItem.Name = "HelpItem";
            this.HelpItem.Size = new System.Drawing.Size(187, 22);
            this.HelpItem.Text = "Help";
            this.HelpItem.Visible = false;
            // 
            // AboutItem
            // 
            this.AboutItem.Name = "AboutItem";
            this.AboutItem.Size = new System.Drawing.Size(187, 22);
            this.AboutItem.Text = "О программе";
            this.AboutItem.Click += new System.EventHandler(this.AboutSoft);
            // 
            // importExcelItem
            // 
            this.importExcelItem.Name = "importExcelItem";
            this.importExcelItem.Size = new System.Drawing.Size(187, 22);
            this.importExcelItem.Text = "Импорт Excel-файла";
            this.importExcelItem.ToolTipText = "Пункты меню с импортом Excel файлов в базу и на экран";
            this.importExcelItem.Click += new System.EventHandler(this.importExcelItem_Click);
            // 
            // labelNets
            // 
            this.labelNets.BackColor = System.Drawing.Color.Transparent;
            this.labelNets.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.labelNets.Location = new System.Drawing.Point(667, 37);
            this.labelNets.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelNets.Name = "labelNets";
            this.labelNets.Size = new System.Drawing.Size(58, 41);
            this.labelNets.TabIndex = 16;
            this.labelNets.Text = "Активные сети:";
            this.labelNets.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // timer1
            // 
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // labelTables
            // 
            this.labelTables.BackColor = System.Drawing.Color.Transparent;
            this.labelTables.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.labelTables.Location = new System.Drawing.Point(3, 37);
            this.labelTables.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelTables.Name = "labelTables";
            this.labelTables.Size = new System.Drawing.Size(55, 42);
            this.labelTables.TabIndex = 20;
            this.labelTables.Text = "Таблицы с данными";
            this.labelTables.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // comboTables
            // 
            this.comboTables.Location = new System.Drawing.Point(59, 31);
            this.comboTables.Margin = new System.Windows.Forms.Padding(2);
            this.comboTables.Name = "comboTables";
            this.comboTables.Size = new System.Drawing.Size(204, 21);
            this.comboTables.TabIndex = 19;
            this.comboTables.SelectedIndexChanged += new System.EventHandler(this.comboTables_SelectedIndexChanged);
            this.comboTables.Enter += new System.EventHandler(this.comboTables_Enter);
            this.comboTables.Leave += new System.EventHandler(this.comboTables_Leave);
            // 
            // comboAnalyse
            // 
            this.comboAnalyse.Items.AddRange(new object[] {
            "Входы и выходы пользователей",
            "Заблокированные входы пользователей",
            "Включение и выключение ПК",
            "Подключение нового внешнего устройства",
            "Активные процессы",
            "Запущенные службы",
            "Включение и выключение ПК 2",
            "Журналы событий"});
            this.comboAnalyse.Location = new System.Drawing.Point(59, 68);
            this.comboAnalyse.Margin = new System.Windows.Forms.Padding(2);
            this.comboAnalyse.Name = "comboAnalyse";
            this.comboAnalyse.Size = new System.Drawing.Size(204, 21);
            this.comboAnalyse.TabIndex = 21;
            this.comboAnalyse.SelectedIndexChanged += new System.EventHandler(this.comboAnalyse_SelectedIndexChanged);
            this.comboAnalyse.Enter += new System.EventHandler(this.comboAnalyse_Enter);
            this.comboAnalyse.Leave += new System.EventHandler(this.comboAnalyse_Leave);
            // 
            // labelCurrentNet
            // 
            this.labelCurrentNet.Enabled = false;
            this.labelCurrentNet.Location = new System.Drawing.Point(723, 89);
            this.labelCurrentNet.Name = "labelCurrentNet";
            this.labelCurrentNet.Size = new System.Drawing.Size(139, 18);
            this.labelCurrentNet.TabIndex = 23;
            this.labelCurrentNet.Text = "CurrentNET";
            this.labelCurrentNet.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBoxLicense
            // 
            this.textBoxLicense.Location = new System.Drawing.Point(235, 29);
            this.textBoxLicense.Name = "textBoxLicense";
            this.textBoxLicense.ReadOnly = true;
            this.textBoxLicense.Size = new System.Drawing.Size(348, 20);
            this.textBoxLicense.TabIndex = 26;
            this.textBoxLicense.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxLicense.WordWrap = false;
            // 
            // timer2
            // 
            this.timer2.Interval = 200;
            this.timer2.Tick += new System.EventHandler(this._timer2_Tick);
            // 
            // labelResultCheckingLicense
            // 
            this.labelResultCheckingLicense.Enabled = false;
            this.labelResultCheckingLicense.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.labelResultCheckingLicense.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelResultCheckingLicense.Location = new System.Drawing.Point(224, 6);
            this.labelResultCheckingLicense.Name = "labelResultCheckingLicense";
            this.labelResultCheckingLicense.Size = new System.Drawing.Size(368, 84);
            this.labelResultCheckingLicense.TabIndex = 27;
            this.labelResultCheckingLicense.Text = "License";
            this.labelResultCheckingLicense.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // labelDomain
            // 
            this.labelDomain.BackColor = System.Drawing.Color.Transparent;
            this.labelDomain.Location = new System.Drawing.Point(180, 28);
            this.labelDomain.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelDomain.Name = "labelDomain";
            this.labelDomain.Size = new System.Drawing.Size(244, 22);
            this.labelDomain.TabIndex = 28;
            this.labelDomain.Text = "Домен:";
            this.labelDomain.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelTargedPCColor
            // 
            this.labelTargedPCColor.BackColor = System.Drawing.Color.Transparent;
            this.labelTargedPCColor.Location = new System.Drawing.Point(180, 62);
            this.labelTargedPCColor.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelTargedPCColor.Name = "labelTargedPCColor";
            this.labelTargedPCColor.Size = new System.Drawing.Size(244, 23);
            this.labelTargedPCColor.TabIndex = 29;
            this.labelTargedPCColor.Text = "Хосты";
            this.labelTargedPCColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelNetsColor
            // 
            this.labelNetsColor.BackColor = System.Drawing.Color.Transparent;
            this.labelNetsColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.labelNetsColor.Location = new System.Drawing.Point(667, 28);
            this.labelNetsColor.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelNetsColor.Name = "labelNetsColor";
            this.labelNetsColor.Size = new System.Drawing.Size(197, 58);
            this.labelNetsColor.TabIndex = 30;
            this.labelNetsColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelTablesColor
            // 
            this.labelTablesColor.BackColor = System.Drawing.Color.Transparent;
            this.labelTablesColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.labelTablesColor.Location = new System.Drawing.Point(2, 30);
            this.labelTablesColor.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelTablesColor.Name = "labelTablesColor";
            this.labelTablesColor.Size = new System.Drawing.Size(262, 60);
            this.labelTablesColor.TabIndex = 31;
            this.labelTablesColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelRows
            // 
            this.labelRows.BackColor = System.Drawing.Color.Transparent;
            this.labelRows.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.labelRows.Location = new System.Drawing.Point(264, 37);
            this.labelRows.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelRows.Name = "labelRows";
            this.labelRows.Size = new System.Drawing.Size(48, 46);
            this.labelRows.TabIndex = 32;
            this.labelRows.Text = "Фильтр поля";
            this.labelRows.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelRowsColor
            // 
            this.labelRowsColor.BackColor = System.Drawing.Color.Transparent;
            this.labelRowsColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.labelRowsColor.Location = new System.Drawing.Point(264, 29);
            this.labelRowsColor.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelRowsColor.Name = "labelRowsColor";
            this.labelRowsColor.Size = new System.Drawing.Size(234, 60);
            this.labelRowsColor.TabIndex = 33;
            this.labelRowsColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // comboRows
            // 
            this.comboRows.Items.AddRange(new object[] {
            "Входы и выходы пользователей",
            "Заблокированные входы пользователей",
            "Включение и выключение ПК",
            "Подключение нового внешнего устройства",
            "Активные процессы",
            "Запущенные службы",
            "Включение и выключение ПК 2",
            "Журналы событий"});
            this.comboRows.Location = new System.Drawing.Point(311, 30);
            this.comboRows.Margin = new System.Windows.Forms.Padding(2);
            this.comboRows.Name = "comboRows";
            this.comboRows.Size = new System.Drawing.Size(188, 21);
            this.comboRows.TabIndex = 34;
            this.comboRows.SelectedIndexChanged += new System.EventHandler(this.comboRows_SelectedIndexChanged);
            this.comboRows.Enter += new System.EventHandler(this.comboRows_Enter);
            this.comboRows.Leave += new System.EventHandler(this.comboRows_Leave);
            // 
            // textBoxRows
            // 
            this.textBoxRows.Location = new System.Drawing.Point(311, 68);
            this.textBoxRows.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxRows.Name = "textBoxRows";
            this.textBoxRows.Size = new System.Drawing.Size(188, 20);
            this.textBoxRows.TabIndex = 35;
            this.textBoxRows.Text = "Фильтр поля";
            this.textBoxRows.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxRows.Click += new System.EventHandler(this.textBoxRows_Click);
            this.textBoxRows.Enter += new System.EventHandler(this.textBoxRows_Enter);
            this.textBoxRows.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxRows_KeyUp);
            this.textBoxRows.Leave += new System.EventHandler(this.textBoxRows_Leave);
            // 
            // labelNameTable
            // 
            this.labelNameTable.BackColor = System.Drawing.Color.Transparent;
            this.labelNameTable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.labelNameTable.Location = new System.Drawing.Point(320, 50);
            this.labelNameTable.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelNameTable.Name = "labelNameTable";
            this.labelNameTable.Size = new System.Drawing.Size(164, 18);
            this.labelNameTable.TabIndex = 36;
            this.labelNameTable.Text = "Таблица";
            this.labelNameTable.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBoxSearchUser
            // 
            this.textBoxSearchUser.Location = new System.Drawing.Point(502, 63);
            this.textBoxSearchUser.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxSearchUser.Name = "textBoxSearchUser";
            this.textBoxSearchUser.Size = new System.Drawing.Size(129, 20);
            this.textBoxSearchUser.TabIndex = 37;
            this.textBoxSearchUser.Enter += new System.EventHandler(this.comboUsers_Enter);
            this.textBoxSearchUser.Leave += new System.EventHandler(this.comboUsers_Leave);
            // 
            // labelSearchUser
            // 
            this.labelSearchUser.BackColor = System.Drawing.Color.Transparent;
            this.labelSearchUser.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.labelSearchUser.Location = new System.Drawing.Point(428, 62);
            this.labelSearchUser.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelSearchUser.Name = "labelSearchUser";
            this.labelSearchUser.Size = new System.Drawing.Size(222, 23);
            this.labelSearchUser.TabIndex = 38;
            this.labelSearchUser.Text = "Поиск логина";
            this.labelSearchUser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboRows2
            // 
            this.comboRows2.Items.AddRange(new object[] {
            "Входы и выходы пользователей",
            "Заблокированные входы пользователей",
            "Включение и выключение ПК",
            "Подключение нового внешнего устройства",
            "Активные процессы",
            "Запущенные службы",
            "Включение и выключение ПК 2",
            "Журналы событий"});
            this.comboRows2.Location = new System.Drawing.Point(517, 30);
            this.comboRows2.Margin = new System.Windows.Forms.Padding(2);
            this.comboRows2.Name = "comboRows2";
            this.comboRows2.Size = new System.Drawing.Size(187, 21);
            this.comboRows2.TabIndex = 40;
            this.comboRows2.SelectedIndexChanged += new System.EventHandler(this.comboRows2_SelectedIndexChanged);
            this.comboRows2.Enter += new System.EventHandler(this.comboRows2_Enter);
            this.comboRows2.Leave += new System.EventHandler(this.comboRows2_Leave);
            // 
            // textBoxRows2
            // 
            this.textBoxRows2.Location = new System.Drawing.Point(517, 68);
            this.textBoxRows2.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxRows2.Name = "textBoxRows2";
            this.textBoxRows2.Size = new System.Drawing.Size(187, 20);
            this.textBoxRows2.TabIndex = 41;
            this.textBoxRows2.Text = "Фильтр поля";
            this.textBoxRows2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxRows2.Click += new System.EventHandler(this.textBoxRows2_Click);
            this.textBoxRows2.Enter += new System.EventHandler(this.textBoxRows2_Enter);
            this.textBoxRows2.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxRows_KeyUp);
            this.textBoxRows2.Leave += new System.EventHandler(this.textBoxRows2_Leave);
            // 
            // labelNameTable2
            // 
            this.labelNameTable2.BackColor = System.Drawing.Color.Transparent;
            this.labelNameTable2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.labelNameTable2.Location = new System.Drawing.Point(526, 50);
            this.labelNameTable2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelNameTable2.Name = "labelNameTable2";
            this.labelNameTable2.Size = new System.Drawing.Size(164, 18);
            this.labelNameTable2.TabIndex = 42;
            this.labelNameTable2.Text = "Таблица";
            this.labelNameTable2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelRowsColor2
            // 
            this.labelRowsColor2.BackColor = System.Drawing.Color.Transparent;
            this.labelRowsColor2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.labelRowsColor2.Location = new System.Drawing.Point(502, 29);
            this.labelRowsColor2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelRowsColor2.Name = "labelRowsColor2";
            this.labelRowsColor2.Size = new System.Drawing.Size(202, 60);
            this.labelRowsColor2.TabIndex = 43;
            this.labelRowsColor2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelListRow
            // 
            this.labelListRow.BackColor = System.Drawing.Color.Transparent;
            this.labelListRow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.labelListRow.Location = new System.Drawing.Point(721, 27);
            this.labelListRow.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelListRow.Name = "labelListRow";
            this.labelListRow.Size = new System.Drawing.Size(142, 60);
            this.labelListRow.TabIndex = 44;
            this.labelListRow.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // comboBoxMask1
            // 
            this.comboBoxMask1.Items.AddRange(new object[] {
            "Входы и выходы пользователей",
            "Заблокированные входы пользователей",
            "Включение и выключение ПК",
            "Подключение нового внешнего устройства",
            "Активные процессы",
            "Запущенные службы",
            "Включение и выключение ПК 2",
            "Журналы событий"});
            this.comboBoxMask1.Location = new System.Drawing.Point(359, 89);
            this.comboBoxMask1.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxMask1.Name = "comboBoxMask1";
            this.comboBoxMask1.Size = new System.Drawing.Size(140, 21);
            this.comboBoxMask1.TabIndex = 236;
            // 
            // comboBoxMask2
            // 
            this.comboBoxMask2.Items.AddRange(new object[] {
            "Входы и выходы пользователей",
            "Заблокированные входы пользователей",
            "Включение и выключение ПК",
            "Подключение нового внешнего устройства",
            "Активные процессы",
            "Запущенные службы",
            "Включение и выключение ПК 2",
            "Журналы событий"});
            this.comboBoxMask2.Location = new System.Drawing.Point(564, 89);
            this.comboBoxMask2.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxMask2.Name = "comboBoxMask2";
            this.comboBoxMask2.Size = new System.Drawing.Size(140, 21);
            this.comboBoxMask2.TabIndex = 237;
            // 
            // openFileDialogExcel
            // 
            this.openFileDialogExcel.FileName = "openFileDialogExcel";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(871, 550);
            this.Controls.Add(this.comboBoxMask1);
            this.Controls.Add(this.comboBoxMask2);
            this.Controls.Add(this.comboRows2);
            this.Controls.Add(this.textBoxRows2);
            this.Controls.Add(this.labelNameTable2);
            this.Controls.Add(this.comboRows);
            this.Controls.Add(this.textBoxRows);
            this.Controls.Add(this.labelNameTable);
            this.Controls.Add(this.labelRows);
            this.Controls.Add(this.labelRowsColor);
            this.Controls.Add(this.labelRowsColor2);
            this.Controls.Add(this.comboAnalyse);
            this.Controls.Add(this.comboTables);
            this.Controls.Add(this.labelTables);
            this.Controls.Add(this.labelTablesColor);
            this.Controls.Add(this.textBoxLogin);
            this.Controls.Add(this.labelLogin);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.labelPassword);
            this.Controls.Add(this.textBoxDomain);
            this.Controls.Add(this.labelDomain);
            this.Controls.Add(this.comboBoxTargedPC);
            this.Controls.Add(this.labelTargedPCColor);
            this.Controls.Add(this.textBoxInputNetOrInputPC);
            this.Controls.Add(this.labelInputNetOrInputPC);
            this.Controls.Add(this.comboUsers);
            this.Controls.Add(this.textBoxSearchUser);
            this.Controls.Add(this.labelSearchUser);
            this.Controls.Add(this.listBoxNetsRow);
            this.Controls.Add(this.labelNets);
            this.Controls.Add(this.labelNetsColor);
            this.Controls.Add(this.labelListRow);
            this.Controls.Add(this.labelCurrentNet);
            this.Controls.Add(this.textBoxLicense);
            this.Controls.Add(this.labelLicense);
            this.Controls.Add(this.labelResultCheckingLicense);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabPageCtrl.ResumeLayout(false);
            this.groupBoxDevices.ResumeLayout(false);
            this.groupBoxDevices.PerformLayout();
            this.groupBoxPC.ResumeLayout(false);
            this.groupBoxPC.PerformLayout();
            this.groupBoxService.ResumeLayout(false);
            this.groupBoxService.PerformLayout();
            this.groupBoxProcess.ResumeLayout(false);
            this.groupBoxProcess.PerformLayout();
            this.tabPageLog.ResumeLayout(false);
            this.tabPageLog.PerformLayout();
            this.tabDataGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogoDataGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.contextMenuDataGrid.ResumeLayout(false);
            this.tabPageRDP.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.RDP)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel1;
        private System.Windows.Forms.ToolStripProgressBar ProgressBar1;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageLog;
        private System.Windows.Forms.TextBox textBoxLogs;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel2;
        private System.Windows.Forms.ListBox listBoxNetsRow;
        private System.Windows.Forms.TextBox textBoxLogin;
        private System.Windows.Forms.TextBox textBoxDomain;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelLicense;
        private System.Windows.Forms.Label labelLogin;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.Label labelInputNetOrInputPC;
        private System.Windows.Forms.ComboBox comboBoxTargedPC;
        private System.Windows.Forms.TextBox textBoxInputNetOrInputPC;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem MainMenu;
        private System.Windows.Forms.ToolStripMenuItem MenuFunction;
        private System.Windows.Forms.ToolStripMenuItem GetDataMenu;
        private System.Windows.Forms.Label labelNets;
        private System.Windows.Forms.ToolStripMenuItem MenuHelp;
        private System.Windows.Forms.ToolStripMenuItem HelpItem;
        private System.Windows.Forms.ToolStripMenuItem AboutItem;
        private System.Windows.Forms.ToolStripMenuItem ExitItem;
        private System.Windows.Forms.ToolStripMenuItem ResetAuthorizationItem;
        private System.Windows.Forms.ToolStripMenuItem ChangePCItem;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TabPage tabDataGrid;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ToolStripMenuItem SearchMenu;
        private System.Windows.Forms.ToolStripMenuItem ScanEnvironmentItem;
        private System.Windows.Forms.ToolStripMenuItem ScanSelectedNetItem;
        private System.Windows.Forms.ToolStripMenuItem GetLogRemotePCItem;
        private System.Windows.Forms.ToolStripMenuItem DBMenu;
        private System.Windows.Forms.ToolStripMenuItem DBCreateTestItem;
        private System.Windows.Forms.ToolStripMenuItem DBInsertTestItem;
        private System.Windows.Forms.ToolStripMenuItem AnalyseDataMenu;
        private System.Windows.Forms.Label labelTables;
        private System.Windows.Forms.ComboBox comboTables;
        private System.Windows.Forms.ComboBox comboAnalyse;
        private System.Windows.Forms.ToolStripMenuItem CurrentTCPConnectionsItem;
        private System.Windows.Forms.Label labelCurrentNet;
        private System.Windows.Forms.ToolStripMenuItem ControlHostMenu;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabelCurrent;
        private System.Windows.Forms.TabPage tabPageCtrl;
        private System.Windows.Forms.ToolStripMenuItem listTablesDBItem;
        private System.Windows.Forms.TabPage tabPageRDP;
        private AxMSTSCLib.AxMsRdpClient9NotSafeForScripting RDP;
        private System.Windows.Forms.ToolStripMenuItem RDPMenuItem;
        private System.Windows.Forms.ToolStripMenuItem GetHostsFromDomainItem;
        private System.Windows.Forms.ToolStripMenuItem SignerItem;
        private System.Windows.Forms.ToolStripMenuItem SignerRecoverItem;
        private System.Windows.Forms.TextBox textBoxLicense;
        private System.Windows.Forms.ToolStripMenuItem SignGnerateLicenseItem;
        private System.Windows.Forms.ToolStripMenuItem LicenseItem;
        private System.Windows.Forms.ToolStripMenuItem StopSearchItem;
        private System.Windows.Forms.ToolStripMenuItem GetFilesItem;
        private System.Windows.Forms.ToolStripMenuItem GetRegItem;
        public System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Label labelResultCheckingLicense;
        private System.Windows.Forms.Label labelDomain;
        private System.Windows.Forms.Label labelTargedPCColor;
        private System.Windows.Forms.Label labelNetsColor;
        private System.Windows.Forms.Label labelTablesColor;
        private System.Windows.Forms.Label labelRows;
        private System.Windows.Forms.Label labelRowsColor;
        private System.Windows.Forms.ComboBox comboRows;
        private System.Windows.Forms.TextBox textBoxRows;
        private System.Windows.Forms.Label labelNameTable;
        private System.Windows.Forms.ToolStripMenuItem PingHostItem;
        private System.Windows.Forms.TextBox textBoxSearchUser;
        private System.Windows.Forms.Label labelSearchUser;
        private System.Windows.Forms.ComboBox comboRows2;
        private System.Windows.Forms.TextBox textBoxRows2;
        private System.Windows.Forms.Label labelNameTable2;
        private System.Windows.Forms.Label labelRowsColor2;
        private System.Windows.Forms.ContextMenuStrip contextMenuDataGrid;
        private System.Windows.Forms.ToolStripMenuItem GetFileItems;
        private System.Windows.Forms.ToolStripMenuItem GetRegistryItem;
        private System.Windows.Forms.ToolStripMenuItem GetEventsItem;
        private System.Windows.Forms.Label labelListRow;
        private System.Windows.Forms.GroupBox groupBoxProcess;
        private System.Windows.Forms.CheckBox checkBox6;
        private System.Windows.Forms.CheckBox checkBoxRunProcess;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.TextBox textBoxNameProgramm;
        private System.Windows.Forms.Button buttonGetProcess;
        private System.Windows.Forms.ComboBox comboBoxProcess;
        private System.Windows.Forms.CheckBox checkBoxKillProcess;
        private System.Windows.Forms.GroupBox groupBoxService;
        private System.Windows.Forms.CheckBox chkbxService;
        private System.Windows.Forms.Button buttonGetServices;
        private System.Windows.Forms.ComboBox comboBoxService;
        private System.Windows.Forms.GroupBox groupBoxPC;
        private System.Windows.Forms.ComboBox comboUsers;
        private System.Windows.Forms.Button buttonPing;
        private System.Windows.Forms.Label labelControlPing;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkbxUser;
        private System.Windows.Forms.Label labelPowerOff;
        private System.Windows.Forms.CheckBox chkbxPowerOff;
        private System.Windows.Forms.Label labelReboot1;
        private System.Windows.Forms.CheckBox chkbxReboot1;
        private System.Windows.Forms.ToolStripMenuItem SmartSearchMenu;
        private System.Windows.Forms.ToolStripMenuItem ArchievGotFromNetsItem;
        private System.Windows.Forms.ToolStripMenuItem smartLoginToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem smartWordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem smartLoginAndWordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem GetDataHostsArchievItem;
        private System.Windows.Forms.ToolStripMenuItem GetArchieveOfHostItem;
        private System.Windows.Forms.ToolStripMenuItem GetPreviousScanItem;
        private System.Windows.Forms.ToolStripSeparator Separator1;
        private System.Windows.Forms.ToolStripSeparator Separator2;
        private System.Windows.Forms.ToolStripSeparator Separator3;
        private System.Windows.Forms.ToolStripSeparator Separator4;
        private System.Windows.Forms.ToolStripSeparator Separator5;
        private System.Windows.Forms.ToolStripSeparator Separator6;
        private System.Windows.Forms.ToolStripMenuItem GetArchievesHostsItem;
        private System.Windows.Forms.ToolStripSeparator Separator7;
        private System.Windows.Forms.ToolStripSeparator Separator0;
        private System.Windows.Forms.ToolStripMenuItem ScannedNetsTime1;
        private System.Windows.Forms.ToolStripMenuItem ScannedNetsTime2;
        private System.Windows.Forms.ToolStripMenuItem ScannedNetsTime3;
        private System.Windows.Forms.ToolStripSeparator Separator8;
        private System.Windows.Forms.ToolStripMenuItem FullScanItem;
        private System.Windows.Forms.ToolStripMenuItem DBReadNamesOfTablesItem;
        private System.Windows.Forms.ToolStripMenuItem DBReadColumnsOfTheTableItem;
        private System.Windows.Forms.ToolStripMenuItem GetWholeDataItem;
        private System.Windows.Forms.ToolStripMenuItem DeSelectUnknowItem;
        private System.Windows.Forms.ToolStripMenuItem DeSelectUnknowNoPermissionItem;
        private System.Windows.Forms.ToolStripMenuItem PingItem;
        private System.Windows.Forms.PictureBox pictureBoxLogoDataGrid;
        private System.Windows.Forms.ComboBox comboBoxMask1;
        private System.Windows.Forms.ComboBox comboBoxMask2;
        private System.Windows.Forms.ToolTip toolTipText1;
        private System.Windows.Forms.Label labelReboot2;
        private System.Windows.Forms.CheckBox chkbxReboot2;
        private System.Windows.Forms.Label labelLockScreen;
        private System.Windows.Forms.CheckBox chkbxLockScreen;
        private System.Windows.Forms.GroupBox groupBoxDevices;
        private System.Windows.Forms.ComboBox comboBoxSelectDevice;
        private System.Windows.Forms.CheckBox checkBoxDeviceDisable;
        private System.Windows.Forms.Button DisableDevice;
        private System.Windows.Forms.Label labelDisplayNameService;
        private System.Windows.Forms.Label labelSelectedService;
        private System.Windows.Forms.Label labelStatusService;
        private System.Windows.Forms.Label labelProcess;
        private System.Windows.Forms.Label labelParentProcess;
        private System.Windows.Forms.Label labelPathAndTimeProcess;
        private System.Windows.Forms.Button buttonGetInfoDevices;
        private System.Windows.Forms.CheckBox checkBoxReRunProcess;
        private System.Windows.Forms.CheckBox checkBoxChangeStateService;
        private System.Windows.Forms.ToolStripMenuItem CorrectAccessErrorItem;
        private System.Windows.Forms.ToolStripMenuItem DBHostsAccessItem;
        private System.Windows.Forms.ToolStripMenuItem DBUsersPrivateItem;
        private System.Windows.Forms.ToolStripMenuItem DBHostsAccessDeleteRowItem;
        private System.Windows.Forms.ToolStripMenuItem DBUsersPrivateDeleteRowItem;
        private System.Windows.Forms.OpenFileDialog openFileDialogExcel;
        private System.Windows.Forms.ToolStripMenuItem ImportExcelDataItem;
        private System.Windows.Forms.ToolStripMenuItem ViewStoredDataPersonalMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ImportExcelDataIntoDBMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ImportExcelDataIntoDbMainItem;
        private System.Windows.Forms.ToolStripMenuItem ImportExcelDataIntoDbPhoneItem;
        private System.Windows.Forms.ToolStripMenuItem ImportExcelDataIntoDbAddressItem;
        private System.Windows.Forms.ToolStripMenuItem ImportExcelDataIntoDbWorkItem;
        private System.Windows.Forms.ToolStripMenuItem ImportExcelDataIntoDbLearnItem;
        private System.Windows.Forms.ToolStripMenuItem ImportExcelDataIntoDbDocumentItem;
        private System.Windows.Forms.ToolStripMenuItem ImportExcelDataIntoDbRelationItem;
        private System.Windows.Forms.ToolStripMenuItem FindTablesFitIntoPersonalItem;
        private System.Windows.Forms.ToolStripMenuItem importExcelItem;
        private System.Windows.Forms.ToolStripMenuItem ShrinkDBPersonalItem;
        private System.Windows.Forms.ToolStripMenuItem ViewStoredDataPersonalMainItem;
        private System.Windows.Forms.ToolStripMenuItem ViewStoredDataPersonalPhonesItem;
        private System.Windows.Forms.ToolStripMenuItem ViewStoredDataPersonalAddressItem;
        private System.Windows.Forms.ToolStripMenuItem ViewStoredDataPersonalWorkItem;
        private System.Windows.Forms.ToolStripMenuItem ViewStoredDataPersonalLearnItem;
        private System.Windows.Forms.ToolStripMenuItem ViewStoredDataPersonalDocumentItem;
        private System.Windows.Forms.ToolStripMenuItem ViewStoredDataPersonalRelationItem;
    }
}

