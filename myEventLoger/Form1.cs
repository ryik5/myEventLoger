using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net;
//using System.Net.NetworkInformation;

using OfficeOpenXml;
using System.Runtime.InteropServices;

using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Diagnostics;

using Microsoft.Win32;  // для работы с реестром
using MSTSCLib;           //RDP
// using System.DirectoryServices;   //Domain

using SysSec=System.Security.Cryptography;
using crc32Sec=CRC32.Security.Cryptography;  //скачать и установить через NuGet пакет -  Crc32.NET


namespace myEventLoger
{
    // ---------------------------- Main Part -------------------------------
    public partial class Form1 :Form
    {
        // ------ Search Date modifying of the hive of registry. Start of Block
        // Must be able at the current Project -  "Сборка\Разрешить небезопасный код"
        [DllImport("kernel32.dll", SetLastError = true)]
        unsafe static extern int RegQueryInfoKey(IntPtr hKey,
        void* lpClass,
        int* lpcClass,
        int* lpReserved,
        int* lpcSubKeys,
        int* lpcMaxSubKeyLen,
        int* lpcMaxClassLen,
        int* lpcValues,
        int* lpcMaxValueNameLen,
        int* lpcMaxValueLen,
        int* lpcbSecurityDescriptor,
        long* lpftLastWriteTime);        // ------ Search Date modifying of the hive of registry. End of Block

        [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool GetProfilesDirectory(StringBuilder path, ref int size);



        //Triggers of emerging of Stop searching
        static AutoResetEvent waitFile = new AutoResetEvent(false);
        static AutoResetEvent waitStop = new AutoResetEvent(false);
        static AutoResetEvent waitNetStop = new AutoResetEvent(false);
        static AutoResetEvent waitFilePing = new AutoResetEvent(false);
        static AutoResetEvent waitNetPing = new AutoResetEvent(false);
        AutoResetEvent waiter = new AutoResetEvent(false);
        //        private bool _stopScan = false;
        private bool _StopSearchNets = false;   //1
        private bool _GlobalStopSearch = false; //Stop search Everywhere      - add into config
        private bool _StopScanLogEvents = false;
        private bool _StopSearchAliveHosts = false;
        private bool _StopSearchFiles = false;
        private bool _StopScannig = false; //The marker of Stop Scanning
        private bool _RunTimer = false;


        // the list of the found hosts
        private List<string> _addressesPing = new List<string>();
        private List<string> _addressNet = new List<string>();
        private HashSet<string> _addrPing = new HashSet<string>(); //Temporary list - for remove dublicate used HashSet
        private HashSet<string> _listBoxNetsRowSelected = new HashSet<string>();
        private int listBoxNetsRowSelectedIndex = -1;


        //additional Helping variables and lists
        private List<string> _StatusLabel2ListChangingText = new List<string>();   //List of changing texts in the "StatusLabel2"


        //State of ping
        private bool _IPexist = false;
        private bool _CycleWait = true;
        private double _PingStartTime = 0;
        private double _PingEndTime = 1;
        private bool _InputedPCNameCorrect = false;
        private bool _InputedPCIPCorrect = false;


        //my local host features
        private string _myIP = "";
        private string _myHostName = "";
        private string _myNET = "";
        private string _myUserName = "";
        private bool _Authorized = true;                              //Runner of this tool have highest privelegies!
        private string _myELKey = "";
        private int maxLicense = 0;

        //Alive Hosts and Nets
        private string[] _NetAlive = new string[50];                  //the Neariest Alive networks
        private string[] _IPAlive = new string[4096];                 //Neariest Alive hosts
        private string[] _NetDateScan = new string[4];                // ContextMenu of listboxNets
        private string[] _DateScan = new string[4];                   // ContextMenu DataGridView


        //Current Date's and Time's Stamp of running
        private string _Date = "2001-01-01"; //yyyy-mm-dd
        private string _Time = "01:01:01"; //hh:mm:ss
        static string formatsrc = "dd.mm.yyyy";
        static string formatdst = "yyyy-mm-dd";
        // static string formatsrcDateTime = "dd.mm.yyyy 01:01:01";
        //static string formatdstDateTime = "yyyy-mm-dd 01:01:01";


        //TextBoxes' Text to temporary keis
        private string textBoxDomainText = "";
        private int textBoxDomainLength = 0;
        private string textBoxPasswordText = "";
        private int textBoxPasswordLength = 0;
        private string textBoxLoginText = "";
        private int textBoxLoginLength = 0;
        private string textBoxInputNetOrInputPCText = "";
        private int textBoxInputNetOrInputPCLength = 0;


        //Current Authority on Every Remote hosts
        private string _User = ""; //my login at Remote Hosts
        private string _Password = ""; //my Password at Remote Hosts


        //Current temporary host
        private string _currentHostName = "";
        private string _currentHostIP = "";
        private string _currentHostNet = "";
        private string _currentUser = ""; //Found user
        private string _currentUserSID = ""; //Found SID of user
        private string _currentUserProfilePath = ""; //Found user Profile Path
        private bool _currentRemoteRegistryRunning = false; //Service RemoteRegistry is running now
        private string _SystemPath = "";
        private string _ProductName = "";

        //Temporary  - Change and Replace into _currentHost....
        private string _TempLogonUser = "";
        private string _TempSN = "";
        private string _TempModel = "";
        //Name Logs
        private string[] _nameLogs = new string[100];


        //found used USB
        private string[] _UsbKey = new string[100];
        private string[] _UsbName = new string[100];
        private string[] _UsbSN = new string[100];
        private string[] _UsbLastTime = new string[100];
        private string[] _UsbInstallDate = new string[100];
        private string[] _UsbInstallTime = new string[100];


        //MSOffice's  and Adobe's features within files in Windows Registry
        private string[] MSOffProgramms = { "Excel", "Word", "Access" };
        private string[] MSOffPlace = { "File MRU", "Place MRU" };
        private string[] MSOffNumber = { "14", "12", "11", "8" };
        private string[] AdobeNumber = { "8", "9", "10", "11", "12" };


        //Паттерны для поиска файлов на диске
        private string[] mySearchPattern =
        {  ".xls", ".doc", ".txt", ".ppt", ".pdf", ".lnk", ".rar", ".zip", ".iso", ".jpg", ".png", ".bmp" };


        //DB Application.StartupPath + @"\
        private const string databaseAllTables = @"myEventLoger\AllTables.db";
        private const string databaseAliveHosts = @"myEventLoger\AliveHosts.db";
        private const string databaseAllUsers = @"myEventLoger\AllUsers.db";
        private string databaseHost = "";                                //Application.StartupPath + "\\myEventLoger\\"+ HostName + ".db"
        private string currentDbName = Application.StartupPath + @"\" + databaseAllTables;
        private string DataGridViewSelectedTable = "";
        private bool selectedPC = false;
        private int allRecords = 0;
        private string SelectedPeriod = ""; //"" -No Select

        private string[] _selectSQLNames =
        {
            "AllTables",
            "UsbUsed",
            "UsbMounted",
            "WindowsFeature",
            "USBHub",
            "PnPEntity",
            "DiskDrive",
            "Volume",
            "PhysicalMemory",
            "Product",
            "Process",
            "Services",
            "EventsLogs",
            "UsersDocuments",
            "FoundDocuments",
            "AliveHosts",
            "AliveNets",
            "AllUsers"
        };

        private string[] _selectSQL =
            {
            "SELECT ComputerName, NameTable, Date, Time FROM 'AllTables' LIMIT 1000  ",
            "SELECT ComputerName, UsbName, UsbSN, InstallDate, InstallTime, LastDate, LastTime FROM 'UsbUsed' LIMIT 1000 ",
            "SELECT ComputerName, ValueName, Value FROM 'UsbMounted' LIMIT 1000 ",
            "SELECT ComputerName, Name, Value, Value1 FROM 'WindowsFeature' LIMIT 1000 ",
            "SELECT ComputerName, Caption, DeviceID, Status FROM 'USBHub' LIMIT 1000 ",
            "SELECT ComputerName, Caption, DeviceID, Manufacturer, Status, SystemName FROM 'PnPEntity' LIMIT 1000 ",
            "SELECT ComputerName, Model, SerialNumber, MediaType, Manufacturer, Status, Size FROM 'DiskDrive' LIMIT 1000 ",
            "SELECT ComputerName, Caption, DriveLetter, FileSystem, DriveType, Size, Capacity, FreeSpace, SizeRemaining FROM 'Volume' LIMIT 1000 ",
            "SELECT ComputerName, Caption, banklabel, DeviceLocator, SerialNumber, MemoryType, Model, Speed, Manufacturer, FormFactor, TotalWidth, InterleavePosition, Capacity FROM 'PhysicalMemory' LIMIT 1000 ",
            "SELECT ComputerName, Caption, InstallDate FROM 'Product' LIMIT 1000 ",
            "SELECT ComputerName, Name, iDProcess, Date, Time FROM 'Process' LIMIT 1000 ",
            "SELECT ComputerName, ServiceName, DisplayName, Status, Date, Time FROM 'Services' ",
            "SELECT ComputerName, Logfile, RecordNumber, EventCode, SourceName, EventType, Type, EventIdentifier, Category, CategoryString, Date, Time, Message, User FROM 'EventsLogs' LIMIT 1000  ",
            "SELECT ComputerName, UserSID, UserName, DocumentPath, Software FROM 'UsersDocuments'  WHERE Date not like '0' LIMIT 2000  ",
            "SELECT ComputerName, DocumentPath, Date, Time FROM 'FoundDocuments' WHERE Date not like '0' LIMIT 2000  ",
            "SELECT ComputerName, ComputerDomainName, ComputerIP, ComputerModel, ComputerSN, LogOnUser, Date FROM 'AliveHosts' LIMIT 2000  ",
            "SELECT Net, ScannerIP,  Date, Time FROM 'AliveNets' LIMIT 100 ",
            "SELECT UserLogin, UserFIO, UserSID, UserNAV, UserMail, UserDomain, LastLogonDate, LastLogonTime, CreateLoginDate  FROM 'AllUsers' LIMIT 2000 "
             };

        private string[] _selectRaw =
            {
            " Id, ComputerName, NameTable, Date, Time ",
            " Id, ComputerName, UsbName, UsbSN, InstallDate, InstallTime, LastDate, LastTime ",
            " Id, ComputerName, ValueName, Value ",
            " Id, ComputerName, Name, Value, Value1 ",
            " Id, ComputerName, Caption, DeviceID, Status ",
            " Id, ComputerName, Caption, DeviceID, Manufacturer, Status, SystemName ",
            " Id, ComputerName, Model, SerialNumber, MediaType, Manufacturer, Status, Size ",
            " Id, ComputerName, Caption, DriveLetter, FileSystem, DriveType, Size, Capacity, FreeSpace, SizeRemaining ",
            " Id, ComputerName, Caption, banklabel, DeviceLocator, SerialNumber, MemoryType, Model, Speed, Manufacturer, FormFactor, TotalWidth, InterleavePosition, Capacity ",
            " Id, ComputerName, Caption, InstallDate ",
            " Id, ComputerName, Name, iDProcess, Date, Time ",
            " Id, ComputerName, ServiceName, DisplayName, Status, Date, Time ",
            " Id, ComputerName, Logfile, RecordNumber, EventCode, SourceName, EventType, Type, EventIdentifier, Category, CategoryString, Date, Time, Message, User ",
            " Id, ComputerName, UserSID, UserName, DocumentPath, Software ",
            " Id, ComputerName, DocumentPath, Date, Time ",
            " Id, ComputerName, ComputerNameShort, ComputerDomainName, ComputerIP, ComputerModel, ComputerSN, LogOnUser, Date, Time ",
            " Id, Net, ScannerIP,  Date, Time ",
            " Id, UserLogin, UserFIO, UserSID, UserNAV, UserMail, UserDomain, LastLogonDate, LastLogonTime, CreateLoginDate "
            };

        private string[] _selectTable =
            {
            " AllTables ",        //1
            " UsbUsed ",          //2
            " UsbMounted ",       //3
            " WindowsFeature ",   //4
            " USBHub ",           //5
            " PnPEntity ",        //6
            " DiskDrive ",        //7
            " Volume ",           //8
            " PhysicalMemory ",   //9
            " Product ",          //10
            " Process ",          //11
            " Services ",         //12
            " EventsLogs ",       //16
            " UsersDocuments ",   //17
            " FoundDocuments ",   //18
            " AliveHosts ",       //19
            " AliveNets ",         //17
            " AllUsers "         //18
            };

        private string[] _createTablesName =
            {
            " 'AllTables' ",        //1
            " 'UsbUsed' ",          //2
            " 'UsbMounted' ",       //3
            " 'WindowsFeature' ",   //4
            " 'USBHub' ",           //5
            " 'PnPEntity' ",        //6
            " 'DiskDrive' ",        //7
            " 'Volume' ",           //8
            " 'PhysicalMemory' ",   //9
            " 'Product' ",          //10
            " 'Process' ",          //11
            " 'Services' ",         //12
            " 'EventsLogs' ",       //13
            " 'UsersDocuments' ",   //14
            " 'FoundDocuments' ",   //15
            " 'AliveHosts' ",       //16
            " 'AliveNets' ",         //17
            " 'AllUsers' "         //18
            };

        private string[] _createTables =
        {
/*15*/                " 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'TableId' INTEGER, 'NameTable' TEXT, 'ComputerIP' TEXT, 'ComputerName' TEXT, 'ScannerName' TEXT, 'ScannerIP' TEXT, 'Date' TEXT, 'Time' TEXT ",
/*1*/                " 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'USBId' INTEGER, 'ComputerName' TEXT, 'UsbKey' TEXT, 'UsbName' TEXT, 'UsbSN' TEXT, 'InstallDate' TEXT, 'InstallTime' TEXT, 'LastDate' TEXT, 'LastTime' TEXT, 'LastUser' TEXT ",
/*2*/                " 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'USBMId' INTEGER, 'ComputerName' TEXT, 'ValueName' TEXT, 'Value' TEXT ",
/*3*/                " 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'Name' TEXT, 'Value' TEXT, 'Value1' TEXT, 'ComputerName' TEXT, 'Date' TEXT, 'Time' TEXT ",
/*4*/                " 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'USBHubId' TEXT, 'ComputerName' TEXT, 'Caption' TEXT, 'DeviceID' TEXT, 'NumberOfPorts' TEXT, 'Status' TEXT, 'SystemName' TEXT, 'Date' TEXT, 'Time' TEXT ",
/*5*/                " 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'PnPId' TEXT, 'ComputerName' TEXT, 'Caption' TEXT, 'installDate' TEXT, 'DeviceID' TEXT, 'HardwareID' TEXT, 'Manufacturer' TEXT, 'Status' TEXT, 'SystemName' TEXT ",
/*6*/                " 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'DDiD' TEXT, 'ComputerName' TEXT, 'Model' TEXT, 'SerialNumber' TEXT, 'MediaType' TEXT, 'Manufacturer' TEXT, 'Status' TEXT, 'Size' TEXT, 'Date' TEXT, 'Time' TEXT ",
/*7*/                " 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'VolumeiD' TEXT, 'ComputerName' TEXT, 'Caption' TEXT, 'DriveLetter' TEXT, 'FileSystem' TEXT, 'DriveType' TEXT, 'Size' TEXT, 'Capacity' TEXT, 'FreeSpace' TEXT, 'SizeRemaining' TEXT, 'Date' TEXT, 'Time' TEXT ",
/*8*/                " 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'PhysicalMemoryId' INTEGER, 'ComputerName' TEXT, 'Caption' TEXT, 'banklabel' TEXT, 'DeviceLocator' TEXT, 'SerialNumber' TEXT, 'MemoryType' TEXT, 'Model' TEXT, 'Speed' TEXT, 'Manufacturer' TEXT, 'FormFactor' TEXT, 'TotalWidth' TEXT, 'InterleavePosition' TEXT, 'Capacity' TEXT, 'Date' TEXT, 'Time' TEXT ",
/*9*/                " 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'ProductId' INTEGER, 'ComputerName' TEXT, 'Caption' TEXT, 'InstallDate' TEXT ",
/*10*/                " 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'ProcessId' INTEGER, 'ComputerName' TEXT, 'Name' TEXT, 'iDProcess' TEXT, 'Date' TEXT, 'Time' TEXT ",
/*11*/                " 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'ServicesId' INTEGER, 'ComputerName' TEXT, 'ServiceName' TEXT, 'DisplayName' TEXT, 'Status' TEXT, 'Date' TEXT, 'Time' TEXT ",
/*12*/                " 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'LogId' INTEGER, 'ComputerName' TEXT, 'RecordNumber' INTEGER, 'EventCode' TEXT, 'EventType' TEXT, 'SourceName' TEXT, 'EventIdentifier' TEXT, 'Type' TEXT, 'Category' TEXT, 'CategoryString' TEXT, 'Date' TEXT, 'Time' TEXT, 'Message' TEXT, 'User' TEXT , 'Logfile' TEXT ",
/*13*/                " 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'UserDocId' INTEGER, 'ComputerName' TEXT, 'UserSID' TEXT, 'UserName' TEXT, 'DocumentPath' TEXT, 'Software' TEXT, 'Length' TEXT, 'Date' TEXT, 'Time' TEXT ",
/*14*/                " 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'UserDocId' INTEGER, 'ComputerName' TEXT, 'DocumentPath' TEXT, 'Length' TEXT, 'Date' TEXT, 'Time' TEXT ",
/*16*/                " 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'ComputerId' INTEGER, 'ComputerName' TEXT, 'ComputerNameShort' TEXT, 'ComputerDomainName' TEXT, 'ComputerIP' TEXT, 'ComputerModel' TEXT, 'ComputerSN' TEXT, 'LogOnUser' TEXT, 'ScannerName' TEXT, 'ScannerIP' TEXT, 'Date' TEXT, 'Time' TEXT, UNIQUE ('ComputerName', 'ComputerDomainName', 'ComputerSN') ON CONFLICT REPLACE ",
/*17*/                " 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'NetId' INTEGER, 'ScannerName' TEXT, 'ScannerIP' TEXT, 'Date' TEXT, 'Time' TEXT, 'Net' TEXT NOT NULL, UNIQUE ('Net') ON CONFLICT REPLACE  ",
/*18*/                " 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'UserId' INTEGER, 'UserFIO' TEXT, 'UserSID' TEXT, 'UserNAV' TEXT, 'UserMail' TEXT, 'DomainOrHost' TEXT, 'LastLogonDate' TEXT, 'LastLogonTime' TEXT, 'PreviousLogonHosts' TEXT, 'CreateLoginDate' TEXT,'CreateLoginTime' TEXT, 'Date' TEXT, 'Time' TEXT, 'UserLogin' TEXT NOT NULL, 'UserDomain' TEXT NOT NULL, UNIQUE ('UserLogin', 'UserDomain') ON CONFLICT REPLACE "
    };

        private string[] _eventAnalyse = {
            "SELECT ComputerName, RecordNumber, EventCode, SourceName, Date, Time, Message, User FROM 'EventsLogs' WHERE Logfile = 'Security' AND (EventCode=4624 or EventCode=4634 or EventCode=540 or EventCode=528 or EventCode=4800 or EventCode=4801) LIMIT 1000  ",
            "SELECT ComputerName, RecordNumber, EventCode, SourceName, Date, Time, Message, User FROM 'EventsLogs' WHERE Logfile = 'Security' AND ( EventCode=4625 or EventCode=5461 or EventCode=529 or EventCode=530 or EventCode=531 or EventCode=532 or EventCode=533 or EventCode=534 or EventCode=535 or EventCode=539) LIMIT 1000  ",
            "SELECT ComputerName, RecordNumber, EventCode, SourceName, Date, Time, Message, User FROM 'EventsLogs' WHERE Logfile = 'Security' AND (EventCode=4608 OR EventCode=4609 OR EventCode=42 OR EventCode=1074) LIMIT 1000  " ,
            "SELECT ComputerName, RecordNumber, EventCode, SourceName, Date, Time, Message, User FROM 'EventsLogs' WHERE Logfile = 'Security' AND EventCode=6416 LIMIT 1000  ",
            "SELECT ComputerName, Name, iDProcess, Date, Time FROM 'Process' LIMIT 1000 ",
            "SELECT ComputerName, ServiceName, DisplayName, Status, Date, Time FROM 'Services' WHERE Status='Running' LIMIT 1000 ",
            "SELECT ComputerName, RecordNumber, EventCode, SourceName, Date, Time, Message, User FROM 'EventsLogs' WHERE Logfile = 'System' AND EventCode=1 LIMIT 1000  ",
            "SELECT Logfile AS 'Имя Журнала', COUNT (Logfile) AS 'Количество записей' FROM 'EventsLogs' GROUP BY Logfile LIMIT 1000  " //HAVING COUNT(Logfile) > 2; - для отбора логов содержащих более 2 записей
        };

        private int combotables = -1;
        private string comborows = "";
        private string comborows2 = "";

        //Заполнить массив
        private string[,] _mask = new string[3, 9]
         {
            {
            "вывести все",
            "содержит ",
            "не содержит ",
            "равно ",
            "не равно ",
            "больше чем ",
            "меньше чем ",
            "начинается с ",
            "заканчивается на "
             },
            {
                 " LIKE '*",
                 " LIKE '%",
                 " NOT LIKE '%",
                 " LIKE '",
                 " NOT LIKE '",
                 " > '",
                 " < '",
                 " LIKE '",
                 " LIKE '%"
             },
            {
                 "' ",
                 "%' ",
                 "%' ",
                 "' ",
                 "' ",
                 "' ",
                 "' ",
                 "%' ",
                 "' "
             },
         };

        private string[,] selectedDevice = new string[2, 5]    // Name and class disable/enable devices
{
   {
    "Display",
    "Mouse",
    "Keyboard",
    "Net",
    ""
   },
   {
    "display",
    "mouse",
    "keyboard",
    "net",
    ""
   }
};

        private List<string> processeshost = new List<string>();

        private System.Diagnostics.FileVersionInfo myFileVersionInfo;
        private string about = "";
        private ContextMenu contextMenu1;





        //---------------//--------------//     Main Form. Start of the block.      //---------------//--------------//

        public Form1()
        {
            InitializeComponent();
        }


        private async void Form1_Load(object sender, EventArgs e)
        {
            OnStartup(); //Проверка приложения, что оно запущено только ОДИН раз

            FormLoading f2 = new FormLoading();
            f2.Show();

            /*
            Task.Run(() => MessageBox.Show("Приложение для \nАнализа и управление безопасностью\nлокального или удаленного ПК" +
                                 "\n\nЖдите, приложение загружается!",
                                 "Информация о загрузке программы", MessageBoxButtons.OK), cts.Token);
            */
            myFileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
            about = myFileVersionInfo.Comments + " ver." + myFileVersionInfo.FileVersion + " " + myFileVersionInfo.LegalCopyright;
            StatusLabel1.Alignment = ToolStripItemAlignment.Right;
            StatusLabel1.Text = about;
            this.Text = about;
            /*
            StatusLabel1.BorderSides = ToolStripStatusLabelBorderSides.All;
            StatusLabel1.BorderStyle = Border3DStyle.SunkenOuter;
            StatusLabelCurrent.BorderSides = ToolStripStatusLabelBorderSides.All;
            StatusLabelCurrent.BorderStyle = Border3DStyle.SunkenOuter; //выпуклой стиль
            */
            //my LOGO
            Icon = Properties.Resources.iconRYIK;

            contextMenu1 = new ContextMenu();  //Context Menu on notify Icon            
            contextMenu1.MenuItems.Add("About", AboutSoft);
            contextMenu1.MenuItems.Add("Exit", ApplicationExit);

            notifyIcon.Icon = Properties.Resources.iconRYIK;
            notifyIcon.Text = myFileVersionInfo.Comments + " " + myFileVersionInfo.LegalCopyright;
            notifyIcon.BalloonTipText = about;
            notifyIcon.ContextMenu = contextMenu1;
            //contextmenu notifyIcon. works
            // notifyIcon.ContextMenuStrip = contextMenuDataGrid;
            //http://www.cyberforum.ru/windows-forms/thread1626319.html
            //http://www.cyberforum.ru/windows-forms/thread317661.html

            //My Logo set UP on the DataGridView1
            // System.Windows.Forms.PictureBox picturebox = new PictureBox();
            // picturebox.Location=new Point(100, 20);
            Bitmap bmplogo = new Bitmap(Properties.Resources.logoRYIK, 250, 250);
            pictureBoxLogoDataGrid.Image = bmplogo;
            pictureBoxLogoDataGrid.BackColor = Color.Transparent;
            System.Drawing.Drawing2D.GraphicsPath gp = BuildTransparencyPath(pictureBoxLogoDataGrid.Image);
            pictureBoxLogoDataGrid.Region = new Region(gp);


            _ControlVisibleAnalyse(false);
            _ControlVisibleLicense(false);
            _ControlVisibleStartUP(false);

            _CheckLicenseKeyOnStart();   //Проверка наличия лицензии
            _CheckMyNet();
            _CheckWorkDir();
            _GetLastDBInfo();
            _startUpSettings();

            _loadDateAllPreviosScanNet();
            _ContexListBoxAdd(); //Prepare Contex Menu of ListBoxNets

            toolTipText1.SetToolTip(tabControl, "Security and Information Events and Management by hosts");

            toolTipText1.SetToolTip(labelReboot1, "Аккуратная перезагрузка хоста c сохранением данных!");
            toolTipText1.SetToolTip(labelReboot2, "Вызов сбоев в работе хоста\nс вызовом грубая перезагрузки,\nбез сохранения данных!");
            ControlHostMenu.Enabled = true;
            await Task.Run(() => _myTCPConnectionsStatus());
            _ControlVisibleStartUP(true);

            tabControl.SelectedTab = tabPageCtrl;

            f2.Close();
            f2.Dispose();

            //SendKeys.Send("{ENTER}");
            ShowInTaskbar = true;
            textBoxLogin.Focus();
        }

        // private System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();

        protected void OnStartup() //Проверка приложения, что оно запущено только ОДИН раз
        {
            if (!InstanceCheck())
            {
                MessageBox.Show("Приложение для \nАнализа и управление безопасностью\nлокального или удаленного ПК" +
                      "\n\n\nОдин экземпляр приложения уже был запущен. \nЭтот экземпляр программы сейчас будет закрыт!",
                      "Информация о программе",
                  MessageBoxButtons.OK,
                  MessageBoxIcon.Error);
                ApplicationExit();
            }
        }

        static Mutex InstanceCheckMutex;
        static bool InstanceCheck()
        {
            bool existed;
            string guid = Marshal.GetTypeLibGuidForAssembly(System.Reflection.Assembly.GetExecutingAssembly()).ToString(); // получаем GIUD приложения
            InstanceCheckMutex = new Mutex(true, guid, out existed);   //Проверка приложения на запуск 1 раз за раз
            return existed;
        }

        /* 
          Анимация в строке статуса.
          int i = 0;
          timerStatuslLabel2.Start();
          string str = "Hello, windows forms application!";
          private void timerStatuslLabel2_Tick(object sender, EventArgs e)
          {
              if (i == str.Length + 1) i = 0;
              statusStrip1.Items[0].Text =
                  str.Substring(i);
              i++;
          }*/

        private void AboutSoft(object sender, EventArgs e) //for the Context Menu on notify Icon
        { AboutSoft(); }

        private async void ApplicationExit(object sender, EventArgs e) //for the Context Menu on notify Icon
        {
            FormClosing f2 = new FormClosing();
            f2.Show();

            // Task.Run(() => MessageBox.Show("Приложение для \nАнализа и управление безопасностью\nлокального или удаленного ПК",
            //          "Ждите, приложение закрывается!"));
            await Task.Run(() => ApplicationExit());
            Task.Delay(4000).Wait();
            f2.Close();
            f2.Dispose();
            Application.Exit();
        }

        private void AboutSoft()
        {
            String strVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            DialogResult result = MessageBox.Show(
                "Программа предназначена\nдля сбора информации с журналов регистрации событий ПК\nи анализа полученной информации\n" +
                "\nOriginal name: " + myFileVersionInfo.OriginalFilename + "\n" + myFileVersionInfo.LegalCopyright +
                "\n" + "\nFull path: " + Application.ExecutablePath + "\nВерсия: " + myFileVersionInfo.FileVersion + "\nBuild: " +
                strVersion,
                "Информация о программе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void ApplicationExit()
        {
            string myEvLogKey = @"SOFTWARE\RYIK\SIEM";
            try
            {
                using (var EvUserKey = Registry.CurrentUser.CreateSubKey(myEvLogKey))
                {
                    EvUserKey.SetValue("UserLogin", textBoxLogin.Text, RegistryValueKind.String);
                    EvUserKey.SetValue("UserPassword", textBoxPassword.Text, RegistryValueKind.String);
                    EvUserKey.SetValue("UserDomain", textBoxDomain.Text, RegistryValueKind.String);
                    EvUserKey.SetValue("InputedPC", textBoxInputNetOrInputPC.Text, RegistryValueKind.String);
                }
            } catch { }

            try
            {
                if (RDP.Connected.ToString() == "1")
                { RDP.Disconnect(); }
            } catch { }
            _stopSearch();
        }


        //---------------//--------------//     Main Form. End of the block.      //---------------//--------------//







        //-------------/\/\/\/\------------//  Start UP Functions. Start of the Block //-------------/\/\/\/\------------// 

        public static System.Drawing.Drawing2D.GraphicsPath BuildTransparencyPath(Image im)     // Прорисовка рисунка с прозрачностью (исп. дл я прорисовки Логотипа  в PictureBox поверх DataGridView1)
        {
            int x, y;
            Bitmap bmp = new Bitmap(im);
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            Color mask = bmp.GetPixel(0, 0);

            for (x = 0; x <= bmp.Width - 1; x++)
            {
                for (y = 0; y <= bmp.Height - 1; y++)
                {
                    if (!bmp.GetPixel(x, y).Equals(mask))
                    { gp.AddRectangle(new Rectangle(x, y, 1, 1)); }
                }
            }
            bmp.Dispose();
            return gp;
        }

        private void _CheckWorkDir() //Check existing working Folders
        {
            DirectoryInfo info1 = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\tmp");
            DirectoryInfo info2 = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\ready");
            DirectoryInfo info3 = new DirectoryInfo(Application.StartupPath + "\\myEventLoger");
            FileInfo fi = new FileInfo(Application.StartupPath + @"\" + databaseAllTables);

            if (!File.Exists(fi.FullName))
            {
                try { info3.Create(); } catch { }
                try { info3.CreateSubdirectory("tmp"); } catch { }
                try { info3.CreateSubdirectory("ready"); } catch { }
            }
            else
            {
                if (info1.Exists)
                {
                    try
                    {
                        info1.Delete(true);
                        Task.Delay(500).Wait();
                        info3.CreateSubdirectory("tmp");
                    } catch { }
                }
                else
                { info3.CreateSubdirectory("tmp"); }

                if (info2.Exists)
                {
                    try
                    {
                        info2.Delete(true);
                        Task.Delay(500).Wait();
                        info3.CreateSubdirectory("ready");
                    } catch { }
                }
                else
                { info3.CreateSubdirectory("tmp"); }
            }
        }

        private void _CheckMyNet() //Check my Network Info and Set StartUP settings
        {
            for (int i = 0; i < 50; i++) //Инициализация массива
            { _NetAlive[i] = "0"; }

            for (int i = 0; i < 4096; i++) //Инициализация массива
            { _IPAlive[i] = "0"; }

            _myHostName = Environment.MachineName;
            _myUserName = Environment.UserName;

            IPAddress ip = Dns.GetHostByName(_myHostName).AddressList[0];

            /*  host = Dns.GetHostName("IP_address");
                IPAddress addr = IPAddress.Parse(ip);
                IPHostEntry entry = Dns.GetHostEntry(addr);
                Name = entry.HostName;
            */

            _myIP = ip.ToString();

            string[] ipn2 = Regex.Split(_myIP, "[.]");
            _myNET = ipn2[0] + "." + ipn2[1] + "." + ipn2[2] + ".";
            _listBoxNetsRow(_myNET + "xxx");

            _textBoxInputNetOrInputPC(_myIP);
            _comboBoxTargedPCAdd(_myHostName + " | " + _myIP);
            _StatusLabel2Text("Данный ПК: " + _myHostName + " | " + _myIP);
            buttonPing.Text = "Выполнить";
            buttonPing.Enabled = false;
            buttonPing.Visible = true;
            labelCurrentNet.BackColor = System.Drawing.Color.LightSkyBlue;
            StatusLabelCurrent.Text = "";
            labelControlPing.Visible = false;
            labelControlPing.Text = "It is checking:\n" + _currentHostIP;
            toolTipText1.SetToolTip(listBoxNetsRow, "Перечень ближайших сетей с активными сетевыми устройствами");
            _labelCurrentNet(_myNET + "xxx");
            toolTipText1.SetToolTip(statusStrip1, "Мой IP - " + _myIP + " | Моя сеть - " + _myNET + "xxx");
            CurrentTCPConnectionsItem.Text = "Открытые сетевые соединения " + _myIP;
        }

        int accessDBError = 0;
        private void _GetLastDBInfo() //Get Last gathered Info from the local DB
        {
            accessDBError++;
            HashSet<string> _listNet = new HashSet<string>();
            DirectoryInfo info = new DirectoryInfo(Application.StartupPath + @"\" + databaseAllTables);

            try
            {
                if (File.Exists(info.FullName))
                {
                    //                SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite"); //инициализация
                    try
                    {
                        using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", Application.StartupPath + @"\" + databaseAllTables)))
                        {
                            connection.Open();

                            /*
                            DataTable dt = new DataTable();
                            SQLiteDataAdapter dAdapter = new SQLiteDataAdapter("SELECT * FROM WindowsFeature", connection);
                            dAdapter.Fill(dt);
                            */

                            SQLiteCommand command = new SQLiteCommand("SELECT * FROM 'WindowsFeature';", connection);
                            SQLiteDataReader reader = command.ExecuteReader();
                            try
                            {
                                foreach (DbDataRecord record in reader)
                                {
                                    if (record != null)
                                    {
                                        string id = record["ComputerName"].ToString();
                                        string a = "";
                                        if (record["Name"].ToString().Contains("Date gathering"))
                                        { a = record["Value"].ToString(); }
                                        if (a.Length > 0 && id.Length > 0)
                                        { MessageBox.Show("В базе собраны данные по \n" + id + "\nДанные были собраны: " + a); }
                                    }
                                }
                            } catch { }

                            command = new SQLiteCommand("SELECT Net FROM 'AliveNets' ;", connection);
                            reader = command.ExecuteReader();

                            try
                            {
                                foreach (DbDataRecord record in reader)
                                {
                                    if (record != null)
                                    {
                                        string id = record["Net"].ToString();
                                        if (record["Net"].ToString().ToLower().Contains(".xxx"))
                                        { _listNet.Add(record["Net"].ToString()); }
                                    }
                                }
                            } catch { }
                            reader.Close();
                            command.Dispose();

                        }
                    } catch { _DBCheckFull("AllTables"); }
                    _DBCheckFull("AllTables");
                    _DBCheckFull("AliveHosts");
                    _DBCheckFull("AllUsers");

                }
                else
                {
                    _DBCheckFull("AllTables");
                    _DBCheckFull("AliveHosts");
                    _DBCheckFull("AllUsers");
                }

                string st = "";

                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", Application.StartupPath + @"\" + databaseAliveHosts)))
                {
                    connection.Open();
                    try
                    {
                        SQLiteCommand command = new SQLiteCommand("SELECT max(Date) FROM AliveHosts;", connection);
                        var reader = command.ExecuteScalar();
                        st = reader.ToString();
                    } catch { }
                    connection.Close();
                }

                selectedPC = false;
                currentDbName = Application.StartupPath + @"\" + databaseAliveHosts;
                DataGridViewSelectedTable = "AliveHosts";
                _ContextData(databaseAliveHosts, "AliveHosts", st);
                _ContextDataGridViewData(databaseAliveHosts, "AliveHosts");

                //List of preivious found Nets
                listBoxNetsRow.Items.Clear();
                string[] listNet = _listNet.ToArray();
                foreach (string s in listNet)
                { listBoxNetsRow.Items.Add(s); }
            } catch (Exception expt)
            {
                File.Delete(info.FullName);
                if (accessDBError == 1)
                { _GetLastDBInfo(); }
                else if (accessDBError == 2)
                {
                    DirectoryInfo directoryProgramm = new DirectoryInfo(Application.StartupPath + "\\myEventLoger");
                    try { directoryProgramm.Create(); } catch { }
                }
                else
                {
                    MessageBox.Show("Удалите вручную папку\n" + Application.StartupPath + "\\myEventLoger\n\n\n" + expt.ToString(),
                        myFileVersionInfo.ToString(),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void _myTCPConnectionsStatus()  //Check living TCP connections
        {
            _textBoxLogs("TCP connection status:\n\n");
            string[] cons = Disconnector.Connections(Disconnector.State.Established);
            foreach (string s in cons)
            { _textBoxLogs(s + "\n"); }
            _textBoxLogs("\n");
        }

        private void _startUpSettings() //Установка первоначальных настроек
        {
            try
            {
                comboBoxMask1.Items.Clear();
                comboBoxMask2.Items.Clear();
                comboBoxSelectDevice.Items.Clear();

                for (int i = 0; i < 9; i++)
                {
                    comboBoxMask1.Items.Add(_mask[0, i]);
                    comboBoxMask2.Items.Add(_mask[0, i]);
                }
                comboBoxMask1.SelectedIndex = 0;
                comboBoxMask2.SelectedIndex = 0;

                for (int i = 0; i < 5; i++)
                {
                    comboBoxSelectDevice.Items.Add(selectedDevice[0, i]);
                }

                comboBoxSelectDevice.SelectedIndex = 0;

                //new Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold)
                //new Font(labelSelectedService.Font, labelSelectedService.Font.Style | FontStyle.Bold)
                labelSelectedService.Font = new Font(FontFamily.GenericSansSerif, labelSelectedService.Font.Size, FontStyle.Bold);
                labelSelectedService.Text = "";
                labelStatusService.Text = "";
                labelDisplayNameService.Text = "";

                labelProcess.Font = new Font(FontFamily.GenericSansSerif, labelProcess.Font.Size, FontStyle.Bold);
                labelProcess.Text = "";
                labelPathAndTimeProcess.Text = "";
                labelParentProcess.Text = "";

                labelNameTable.Text = "";
                labelNameTable2.Text = "";
                textBoxRows.Text = "";
                textBoxRows2.Text = "";

                comboTables.Items.Clear();
                textBoxRows.ForeColor = System.Drawing.Color.Gray;
                textBoxRows2.ForeColor = System.Drawing.Color.Gray;
                foreach (string s in _selectSQLNames)
                { comboTables.Items.Add(s); }

                textBoxPassword.Enabled = false;
                StopSearchItem.Enabled = false;

                Separator0.Visible = false;
                Separator1.Visible = true; //Context Menu DataGrid
                GetArchieveOfHostItem.Visible = false;
                Separator5.Visible = false;
                GetFileItems.Visible = false;
                GetRegistryItem.Visible = false;
                PingItem.Visible = false;
                GetEventsItem.Visible = false;
                GetWholeDataItem.Visible = false;
                DeSelectUnknowItem.Visible = false;
                DeSelectUnknowNoPermissionItem.Visible = false;
                ScannedNetsTime1.Visible = false;
                ScannedNetsTime2.Visible = false;
                ScannedNetsTime3.Visible = false;

                //MenuItems of Names imported Excel file
                ImportExcelDataIntoDbMainItem.Visible = false;
                ImportExcelDataIntoDbWorkItem.Visible = false;
                ImportExcelDataIntoDbLearnItem.Visible = false;
                ImportExcelDataIntoDbAddressItem.Visible = false;
                ImportExcelDataIntoDbPhoneItem.Visible = false;
                ImportExcelDataIntoDbRelationItem.Visible = false;
                ImportExcelDataIntoDbDocumentItem.Visible = false;

            } catch (Exception expt)
            { textBoxLogs.AppendText(expt.Message); }

            try
            {
                using (RegistryKey EvUserKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\RYIK\SIEM", RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey))
                {
                    textBoxLogin.Text = EvUserKey.GetValue("UserLogin").ToString();
                    textBoxPassword.Text = EvUserKey.GetValue("UserPassword").ToString();
                    textBoxDomain.Text = EvUserKey.GetValue("UserDomain").ToString();
                    textBoxInputNetOrInputPC.Text = EvUserKey.GetValue("InputedPC").ToString();
                    if (textBoxInputNetOrInputPC.TextLength > 0)
                        buttonPing.Enabled = true;
                }
            } catch { }

            textBoxLogs.AppendText("\n");
            //Check at the local Registry of fixing of the access to shares of any WindowsXP's systems
            string checkRegCorrectErrorValue = "0";
            try
            {
                using (RegistryKey regCorrectError = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Lsa", RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey))
                { checkRegCorrectErrorValue = regCorrectError.GetValue("LmCompatibilityLevel").ToString(); }
            } catch (Exception expt)
            {
                checkRegCorrectErrorValue = "0";
                textBoxLogs.AppendText(expt.Message);
            }

            if (checkRegCorrectErrorValue != "1")
            { CorrectAccessErrorItem.Enabled = false; }

            //Блокирование доступа к пунктам меню при отсутствии баз на диске
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\HostsUsersAccess.db");
            if (!fi.Exists)
            {
                DBHostsAccessItem.Visible = false;
            }

            fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");
            if (!fi.Exists)
            {
                //       DBUsersPrivateItem.Visible = false;
            }
        }

        private void _loadDateAllPreviosScanNet()
        {
            for (int i = 0; i < 3; i++)
            { _NetDateScan[i] = ""; }

            HashSet<string> _listDateScanNet = new HashSet<string>();
            DirectoryInfo info = new DirectoryInfo(Application.StartupPath + @"\" + databaseAllTables);
            if (File.Exists(info.FullName))
            {
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", Application.StartupPath + @"\" + databaseAllTables)))
                {
                    connection.Open();
                    try
                    {
                        using (SQLiteCommand command = new SQLiteCommand(" SELECT Date FROM AliveNets GROUP BY Date ORDER BY Date DESC; ", connection))
                        {
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                foreach (DbDataRecord record in reader)
                                {
                                    if (record != null)
                                    { _listDateScanNet.Add(record["Date"].ToString()); }
                                }
                            }
                        }
                    } catch (Exception expt) { _textBoxLogs(expt.Message); }
                    connection.Close();
                }
            }
            string[] listDateScanNet = _listDateScanNet.ToArray();
            for (int i = 0; i < listDateScanNet.Length; i++)
            {
                if (listDateScanNet[i].ToString().Trim().Length > 3)
                { _NetDateScan[i] = listDateScanNet[i]; }
            }
        }

        private void _CheckLicenseKeyOnStart() // Проверка наличия лицензии и запись ее в переменную
        {
            _labelResultCheckingLicenseVisible(false);
            List<bool> isValidArray = new List<bool>();
            bool isValid; string plec = ""; bool KeyInRegistry = false; string tempkey = "";

            string myEvLogKey = @"SOFTWARE\RYIK\SIEM";

            FileInfo plck = new FileInfo("myELLic.key");
            try
            {
                if (File.Exists(plck.FullName))
                {
                    plec = File.ReadAllText(plck.FullName);
                    string plic = "", plac = "";
                    string[] keylic = new string[5];

                    if (plec.Contains(@"<myEventLogerKeyValue><LicKey>"))
                    {
                        plic = plec.Remove(0, 30);
                        keylic[0] = plic.Remove(0, 0).Remove(8); //Доделать формирование ключей
                        keylic[1] = plic.Remove(0, 8).Remove(8);
                        keylic[2] = plic.Remove(0, 16).Remove(8);
                        keylic[3] = plic.Remove(0, 24).Remove(8);
                        keylic[4] = plic.Remove(0, 32).Remove(8);

                        for (int i = 0; i < 5; i++)
                        {
                            if (i < 4)
                            { plac += keylic[i] + "-"; }
                            else { plac += keylic[i]; }
                        }
                        try { _textBoxLicense(plac); } catch { }
                    }

                    string LicPublic = myEventLoger.Properties.Resources.LicKeypublic;
                    try
                    {
                        KuSigner PublicSigner = new KuSigner(LicPublic);
                        string[] UnoKeys = Regex.Split(_GetUniqKey("|"), "[|]");
                        for (int ik = 0; ik < 5; ik++)
                        {
                            isValid = PublicSigner.VerifySignature(Encoding.UTF8.GetBytes(UnoKeys[ik]), StringToByteArray(keylic[ik]));// Проверка подписи строки 
                            isValidArray.Add(isValid);
                        }
                    } catch { }

                    /*FileInfo pbk = new FileInfo("public.key");
                    if (File.Exists(pbk.FullName))
                    {
                        try
                        {
                            KuSigner PublicSigner = new KuSigner(File.ReadAllText(pbk.FullName));
                            string[] UnoKeys = Regex.Split(_GetUniqKey("|"), "[|]");
                            for (int ik = 0; ik < 5; ik++)
                            {
                                isValid = PublicSigner.VerifySignature(Encoding.UTF8.GetBytes(UnoKeys[ik]), StringToByteArray(keylic[ik]));// Проверка подписи строки 
                                isValidArray.Add(isValid);
                            }
                        }
                        catch { }
                    }
*/
                    isValidArray.ToArray(); int iAll = 0;
                    for (int i = 0; i < isValidArray.ToArray().Length; i++)
                    {
                        if (isValidArray.ToArray()[i])
                        { iAll++; }
                    }

                    if (iAll > (isValidArray.ToArray().Length - 3) && (isValidArray.ToArray()[isValidArray.ToArray().Length - 1]))
                    {
                        _textBoxLicenseForeColor(System.Drawing.Color.Black);
                        _textBoxLicenseBackColor(System.Drawing.Color.YellowGreen);
                        _labelResultCheckingLicenseBackColor(System.Drawing.Color.DarkSeaGreen);
                        _labelResultCheckingLicenseText("Используется легальная лицензия ПО для данного ПК");
                        _myELKey = plec;
                        maxLicense = 1;
                        pictureBoxLogoDataGrid.Visible = false;
                    }
                    if (iAll > (isValidArray.ToArray().Length - 2) && (isValidArray.ToArray()[isValidArray.ToArray().Length - 1]))
                    {
                        _textBoxLicenseForeColor(System.Drawing.Color.Black);
                        _textBoxLicenseBackColor(System.Drawing.Color.DarkSeaGreen);
                        _labelResultCheckingLicenseBackColor(System.Drawing.Color.DarkSeaGreen);
                        _labelResultCheckingLicenseText("Используется легальная лицензия ПО для данного ПК");

                        FileInfo tmpk = new FileInfo("temp.key");
                        if (File.Exists(tmpk.FullName))
                        { File.Delete(tmpk.FullName); }
                        else { }

                        try
                        {
                            using (var LicKey = Registry.CurrentUser.CreateSubKey(myEvLogKey)) //Get last Time using of USB
                            { LicKey.SetValue("License", plec); }
                        } catch { }
                        _myELKey = plec;
                        try { _textBoxLicense(plac); } catch { }
                        maxLicense = 2;
                        pictureBoxLogoDataGrid.Visible = false;
                    }
                    else
                    {
                        _GenerateTempKey();
                        _textBoxLicenseForeColor(System.Drawing.Color.Black);
                        _textBoxLicenseBackColor(System.Drawing.Color.SandyBrown);
                        _labelResultCheckingLicenseBackColor(System.Drawing.Color.SandyBrown);
                        _labelResultCheckingLicenseText("Некорректная лицензия!");

                        maxLicense = 0;
                        pictureBoxLogoDataGrid.Visible = true;
                    }
                }
                else { }

                isValidArray = new List<bool>();
                try
                {
                    using (var LicKey = Registry.CurrentUser.OpenSubKey(myEvLogKey)) //Get last Time using of USB
                    { tempkey = LicKey.GetValue("License").ToString(); KeyInRegistry = true; }
                } catch { }

                if (KeyInRegistry && maxLicense < 2)
                {
                    string plic = "", plac = "";
                    string[] keylic = new string[5];

                    if (tempkey.Contains(@"<myEventLogerKeyValue><LicKey>"))
                    {
                        plic = tempkey.Remove(0, 30);
                        keylic[0] = plic.Remove(0, 0).Remove(8); //Доделать формирование ключей
                        keylic[1] = plic.Remove(0, 8).Remove(8);
                        keylic[2] = plic.Remove(0, 16).Remove(8);
                        keylic[3] = plic.Remove(0, 24).Remove(8);
                        keylic[4] = plic.Remove(0, 32).Remove(8);

                        for (int i = 0; i < 5; i++)
                        {
                            if (i < 4)
                            { plac += keylic[i] + "-"; }
                            else { plac += keylic[i]; }
                        }
                    }

                    string LicPublic = myEventLoger.Properties.Resources.LicKeypublic;
                    try
                    {
                        KuSigner PublicSigner = new KuSigner(LicPublic);
                        string[] UnoKeys = Regex.Split(_GetUniqKey("|"), "[|]");
                        for (int ik = 0; ik < 5; ik++)
                        {
                            isValid = PublicSigner.VerifySignature(Encoding.UTF8.GetBytes(UnoKeys[ik]), StringToByteArray(keylic[ik]));// Проверка подписи строки 
                            isValidArray.Add(isValid);
                        }
                    } catch { }

                    /*FileInfo pbk = new FileInfo("public.key");
                    if (File.Exists(pbk.FullName))
                    {
                        try
                        {
                            KuSigner PublicSigner = new KuSigner(File.ReadAllText(pbk.FullName));
                            string[] UnoKeys = Regex.Split(_GetUniqKey("|"), "[|]");
                            for (int ik = 0; ik < 5; ik++)
                            {
                                isValid = PublicSigner.VerifySignature(Encoding.UTF8.GetBytes(UnoKeys[ik]), StringToByteArray(keylic[ik]));// Проверка подписи строки 
                                isValidArray.Add(isValid);
                            }
                        }
                        catch { }
                    }
*/
                    isValidArray.ToArray(); int iAll = 0;
                    for (int i = 0; i < isValidArray.ToArray().Length; i++)
                    {
                        if (isValidArray.ToArray()[i])
                        { iAll++; }
                    }

                    if (iAll > (isValidArray.ToArray().Length - 3) && (isValidArray.ToArray()[isValidArray.ToArray().Length - 1]))
                    {
                        _textBoxLicenseForeColor(System.Drawing.Color.Black);
                        _textBoxLicenseBackColor(System.Drawing.Color.YellowGreen);
                        _labelResultCheckingLicenseBackColor(System.Drawing.Color.DarkSeaGreen);
                        _labelResultCheckingLicenseText("Используется легальная лицензия ПО для данного ПК");
                        if (maxLicense < 2)
                        {
                            try { _textBoxLicense(plac); } catch { }
                            _myELKey = plec;
                            maxLicense = 1;
                        }
                    }
                    if (iAll > (isValidArray.ToArray().Length - 2) && (isValidArray.ToArray()[isValidArray.ToArray().Length - 1]))
                    {
                        _textBoxLicenseForeColor(System.Drawing.Color.Black);
                        _textBoxLicenseBackColor(System.Drawing.Color.DarkSeaGreen);
                        _labelResultCheckingLicenseBackColor(System.Drawing.Color.DarkSeaGreen);
                        _labelResultCheckingLicenseText("Используется легальная лицензия ПО для данного ПК");


                        FileInfo tmpk = new FileInfo("temp.key");
                        if (File.Exists(tmpk.FullName))
                        { File.Delete(tmpk.FullName); }
                        else { }

                        try
                        {
                            using (var LicKey = Registry.CurrentUser.CreateSubKey(myEvLogKey)) //Get last Time using of USB
                            { LicKey.SetValue("License", plec); }
                        } catch { }
                        _myELKey = plec;
                        try { _textBoxLicense(plac); } catch { }
                        maxLicense = 2;
                    }
                    else
                    {
                        if (maxLicense < 1)
                        {
                            _GenerateTempKey();
                            _textBoxLicenseForeColor(System.Drawing.Color.Black);
                            _textBoxLicenseBackColor(System.Drawing.Color.SandyBrown);
                            _labelResultCheckingLicenseBackColor(System.Drawing.Color.SandyBrown);
                            _labelResultCheckingLicenseText("Некорректная лицензия!");

                            maxLicense = 0;
                        }
                    }
                }
                else { }
            } catch (Exception exp) { MessageBox.Show(exp.ToString()); }
            try { _textBoxLogs(_myELKey); } catch { }
        }

        //Test. Does mot Use
        private async void _searchNearistNets() // Автопоиск ближайших сетей на старте системы. запись данных только в листбокснет
        {
            _StopSearchNets = false;
            waitNetPing = new AutoResetEvent(false);
            waitFile = new AutoResetEvent(false);
            waitStop = new AutoResetEvent(false);
            waitNetStop = new AutoResetEvent(false);
            waitFilePing = new AutoResetEvent(false);
            await Task.Run(() => _searchAliveNet());
        }




        private void dBHostsAccessItem_Click(object sender, EventArgs e) //call DBHostsAccessItem()
        {
            DBHostsAccess();
        }

        private void DBHostsAccess() //add new info into DB "HostsUsersAccess"
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\HostsUsersAccess.db");
            if (!fi.Exists)
            {
                SQLiteConnection.CreateFile(fi.FullName);

                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", fi.FullName)))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(
                        "CREATE TABLE IF NOT EXISTS 'UsersByHostsControl' ( 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'ComputerName' TEXT, 'ComputerNameShort' TEXT, 'ComputerDomainName' TEXT, 'ComputerIP' TEXT, " +
                        "'User' TEXT, 'Password' TEXT, 'GroupHosts' TEXT, 'Date' TEXT, 'Time' TEXT, UNIQUE('ComputerName', 'GroupHosts', 'User') ON CONFLICT REPLACE );",
                        connection))
                    { try { command.ExecuteNonQuery(); } catch { } }
                }
            }
            else
            {
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", fi.FullName)))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(
                        "CREATE TABLE IF NOT EXISTS 'UsersByHostsControl' ( 'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'ComputerName' TEXT, 'ComputerNameShort' TEXT, 'ComputerDomainName' TEXT, 'ComputerIP' TEXT, " +
                        "'User' TEXT, 'Password' TEXT, 'GroupHosts' TEXT, 'Date' TEXT, 'Time' TEXT, UNIQUE('ComputerName', 'GroupHosts', 'User') ON CONFLICT REPLACE );",
                        connection))
                    { try { command.ExecuteNonQuery(); } catch { } }
                }
            }

            if (textBoxPassword.TextLength > 0 && textBoxLogin.TextLength > 0 && textBoxInputNetOrInputPC.TextLength > 0 && textBoxNameProgramm.TextLength > 0)
            {
                textBoxToTemporary();
                ParseTextboxInputNetOrPC();
                _GetTimeRunScan();

                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", fi.FullName)))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'UsersByHostsControl' ('ComputerName', 'ComputerIP', 'GroupHosts', 'User', 'Password', 'Date', 'Time') VALUES (@ComputerName, @ComputerIP, @GroupHosts, @User, @Password, @Date, @Time)", connection))
                    {
                        command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                        command.Parameters.AddWithValue("@ComputerIP", _currentHostIP);
                        command.Parameters.AddWithValue("@GroupHosts", textBoxNameProgramm.Text.Trim());
                        command.Parameters.AddWithValue("@User", textBoxLogin.Text.Trim());
                        command.Parameters.AddWithValue("@Password", _Password);
                        command.Parameters.AddWithValue("@Date", _Date);
                        command.Parameters.AddWithValue("@Time", _Time);
                        try { command.ExecuteNonQuery(); } catch { }
                    }
                }
            }

            _UpdateDataGrid("SELECT * FROM 'UsersByHostsControl' ORDER BY 'GroupHosts' DESC;", fi.FullName);

            DataGridViewSelectedTable = "usersbyhostscontrol";
            tabControl.SelectedTab = tabDataGrid;
            dataGridView1.Focus();
        }

        private void DBHostsAccessDeleteRowItem_Click(object sender, EventArgs e)  //call DeleteSelectedRowDatagridView()
        {
            DeleteSelectedRowDatagridView
                (
                "ComputerName", "ComputerIP", "User", "GroupHosts", "UsersByHostsControl", "HostsUsersAccess"
                );
        }

        private void DeleteSelectedRowDatagridView(string header1, string header2, string header3, string headerSorted, string tableName, string dbName)
        {
            if (DataGridViewSelectedTable.Contains(tableName.ToLower()))
            {
                int IndexCurrentRow = dataGridView1.CurrentRow.Index;
                int IndexColumn1 = 0;           // индекс 1-й колонки в датагрид
                int IndexColumn2 = 0;           // индекс 2-й колонки колонки в датагрид
                int IndexColumn3 = 0;           // индекс 3-й колонки колонки в датагрид
                string TextColumn1 = "";  // содержимое "ComputerName" выбранной строки в датагрид
                string TextColumn2 = "";  // содержимое "ComputerIP" выбранной строки в датагрид
                string TextColumn3 = "";  // содержимое "User" выбранной строки в датагрид

                for (int i = 0; i < dataGridView1.ColumnCount; i++)
                {
                    if (dataGridView1.Columns[i].HeaderText.ToString() == header1)
                        IndexColumn1 = i;
                    if (dataGridView1.Columns[i].HeaderText.ToString() == header2)
                        IndexColumn2 = i;
                    if (dataGridView1.Columns[i].HeaderText.ToString() == header3)
                        IndexColumn3 = i;
                }
                TextColumn1 = dataGridView1.Rows[IndexCurrentRow].Cells[IndexColumn1].Value.ToString();
                TextColumn2 = dataGridView1.Rows[IndexCurrentRow].Cells[IndexColumn2].Value.ToString();
                TextColumn3 = dataGridView1.Rows[IndexCurrentRow].Cells[IndexColumn3].Value.ToString();

                FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + dbName + @".db");

                if (fi.Exists)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", fi.FullName)))
                    {
                        connection.Open();
                        using (SQLiteCommand command = new SQLiteCommand("DELETE FROM " + tableName + " Where " + header1 + "= @" + header1 + " And " + header2 + "= @" + header2 + " And " + header3 + "= @" + header3, connection))
                        {
                            command.Parameters.AddWithValue("@" + header1, TextColumn1);
                            command.Parameters.AddWithValue("@" + header2, TextColumn2);
                            command.Parameters.AddWithValue("@" + header3, TextColumn3);
                            try { command.ExecuteNonQuery(); } catch { }
                        }

                        //Clean DB after Executed Command's "Delete Row"
                        using (SQLiteCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "vacuum;";
                            try { command.ExecuteNonQuery(); } catch { }
                        }

                        _UpdateDataGrid("SELECT * FROM '" + tableName + "' ORDER BY '" + headerSorted + "' DESC;", fi.FullName);
                    }

                    DataGridViewSelectedTable = tableName.ToLower();
                    tabControl.SelectedTab = tabDataGrid;
                    dataGridView1.Focus();
                }
            }
        }

        private void DBUsersPrivateItem_Click(object sender, EventArgs e)        // Change and Check up Headers of these tables!!!!!
        {
            _GetTimeRunScan();
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");
            if (!fi.Exists)
            {
                SQLiteConnection.CreateFile(fi.FullName);
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", fi.FullName)))
                {
                    connection.Open();
                    //1
                    using (SQLiteCommand command = new SQLiteCommand(
                        "CREATE TABLE IF NOT EXISTS 'UsersMain' ('Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'NAVOrId' TEXT, 'UserFIOUkr' TEXT, 'UserLogin' TEXT, 'UserMail' TEXT, " +
                        "'UserFolder' TEXT, 'UserAddInfo' TEXT, 'CityTSP' TEXT, 'BirthDay' TEXT, 'BirthDayPlace' TEXT, 'EmploymentDate' TEXT, 'DismissalDate' TEXT, 'WorkInCompany' TEXT, 'Subdivision' TEXT, " +
                        "'UserPosition' TEXT, 'Experience' TEXT, 'Sex' TEXT, UNIQUE('NAVOrId') ON CONFLICT REPLACE );",
                        connection))
                    { command.ExecuteNonQuery(); }

                    //2
                    using (SQLiteCommand command = new SQLiteCommand(
                        "CREATE TABLE IF NOT EXISTS 'UserWork' ('Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'NAVOrId' TEXT, 'UserFIOUkr' TEXT, " +
                        "'CityTSP' TEXT, 'EmploymentDate' TEXT, 'DismissalDate' TEXT, 'WorkInCompany' TEXT, 'Subdivision' TEXT, " +
                        "'UserPosition' TEXT, UNIQUE('NAVOrId', 'EmploymentDate', 'WorkInCompany', 'Subdivision', 'UserPosition') ON CONFLICT REPLACE );",
                        connection))
                    { command.ExecuteNonQuery(); }

                    //3
                    using (SQLiteCommand command = new SQLiteCommand(
                        "CREATE TABLE IF NOT EXISTS 'UserLearn' ('Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'NAVOrId' TEXT, 'UserFIOUkr' TEXT, " +
                        "'CityLearn' TEXT, 'LearnDateBegin' TEXT, 'LearnDateEnd' TEXT, 'School' TEXT, 'Qualification' TEXT, " +
                        "'TypeOfEducation' TEXT, UNIQUE('NAVOrId', 'School', 'Qualification', 'LearnDateBegin') ON CONFLICT REPLACE );",
                        connection))
                    { command.ExecuteNonQuery(); }

                    //4
                    using (SQLiteCommand command = new SQLiteCommand(
                          "CREATE TABLE IF NOT EXISTS 'UsersAddress' ('Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'NAVOrId' TEXT, 'UserFIOUkr' TEXT, 'UserAddress' TEXT, " +
                           "'Date' TEXT, 'Time' TEXT, UNIQUE('NAVOrId', 'UserAddress', 'Date')  ON CONFLICT REPLACE );",
                          connection))
                    { command.ExecuteNonQuery(); }

                    //5
                    using (SQLiteCommand command = new SQLiteCommand(
                          "CREATE TABLE IF NOT EXISTS 'UsersPhone' ('Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'NAVOrId' TEXT, " +
                           "'UserPhone' TEXT, 'PhoneType' TEXT, 'Date' TEXT, 'Time' TEXT, UNIQUE('NAVOrId', 'UserPhone') ON CONFLICT REPLACE );",
                          connection))
                    { command.ExecuteNonQuery(); }

                    //6
                    using (SQLiteCommand command = new SQLiteCommand(
                          "CREATE TABLE IF NOT EXISTS 'UsersRelation' ('Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'NAVOrId' TEXT, 'NAVOrIdSecond' TEXT, 'NAVOrIdSecontTypeConnected' TEXT, " +
                          "'Date' TEXT, 'Time' TEXT, UNIQUE('NAVOrId', 'NAVOrIdSecond' , 'NAVOrIdSecontTypeConnected') ON CONFLICT REPLACE );",
                          connection))
                    { command.ExecuteNonQuery(); }

                    //7
                    using (SQLiteCommand command = new SQLiteCommand(
                          "CREATE TABLE IF NOT EXISTS 'UsersDocuments' ('Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'NAVOrId' TEXT, 'UserFIOUkr' TEXT, 'UserFIO' TEXT, " +
                          "'PasportId' TEXT, 'PasportDate' TEXT, 'PasportStaff' TEXT, 'PasportType' TEXT, 'Date' TEXT, 'Time' TEXT, UNIQUE('NAVOrId', 'PasportId') ON CONFLICT REPLACE );",
                          connection))
                    { command.ExecuteNonQuery(); }

                    //8
                    using (SQLiteCommand command = new SQLiteCommand(
                          "CREATE TABLE IF NOT EXISTS 'TypePeopleConnected' ('Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'ManMain' TEXT, 'ManSlave' TEXT,  UNIQUE('ManMain', 'ManSlave') ON CONFLICT REPLACE );",
                          connection))
                    { command.ExecuteNonQuery(); }
                }
            }
            else
            {
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", fi.FullName)))
                {
                    connection.Open();
                    //1
                    using (SQLiteCommand command = new SQLiteCommand(
                        "CREATE TABLE IF NOT EXISTS 'UsersMain' ('Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'NAVOrId' TEXT, 'UserFIOUkr' TEXT, 'UserLogin' TEXT, 'UserMail' TEXT, " +
                        "'UserFolder' TEXT, 'UserAddInfo' TEXT, 'CityTSP' TEXT, 'BirthDay' TEXT, 'BirthDayPlace' TEXT, 'EmploymentDate' TEXT, 'DismissalDate' TEXT, 'WorkInCompany' TEXT, 'Subdivision' TEXT, " +
                        "'UserPosition' TEXT, 'Experience' TEXT, 'Sex' TEXT, UNIQUE('NAVOrId') ON CONFLICT REPLACE );",
                        connection))
                    { command.ExecuteNonQuery(); }

                    //2
                    using (SQLiteCommand command = new SQLiteCommand(
                        "CREATE TABLE IF NOT EXISTS 'UserWork' ('Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'NAVOrId' TEXT, 'UserFIOUkr' TEXT, " +
                        "'CityTSP' TEXT, 'EmploymentDate' TEXT, 'DismissalDate' TEXT, 'WorkInCompany' TEXT, 'Subdivision' TEXT, " +
                        "'UserPosition' TEXT, UNIQUE('NAVOrId', 'EmploymentDate', 'WorkInCompany', 'Subdivision', 'UserPosition') ON CONFLICT REPLACE );",
                        connection))
                    { command.ExecuteNonQuery(); }

                    //3
                    using (SQLiteCommand command = new SQLiteCommand(
                        "CREATE TABLE IF NOT EXISTS 'UserLearn' ('Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'NAVOrId' TEXT, 'UserFIOUkr' TEXT, " +
                        "'CityLearn' TEXT, 'LearnDateBegin' TEXT, 'LearnDateEnd' TEXT, 'School' TEXT, 'Qualification' TEXT, " +
                        "'TypeOfEducation' TEXT, UNIQUE('NAVOrId', 'School', 'Qualification', 'LearnDateBegin') ON CONFLICT REPLACE );",
                        connection))
                    { command.ExecuteNonQuery(); }

                    //4
                    using (SQLiteCommand command = new SQLiteCommand(
                          "CREATE TABLE IF NOT EXISTS 'UsersAddress' ('Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'NAVOrId' TEXT, 'UserFIOUkr' TEXT, 'UserAddress' TEXT, " +
                           "'Date' TEXT, 'Time' TEXT, UNIQUE('NAVOrId', 'UserAddress', 'Date')  ON CONFLICT REPLACE );",
                          connection))
                    { command.ExecuteNonQuery(); }

                    //5
                    using (SQLiteCommand command = new SQLiteCommand(
                          "CREATE TABLE IF NOT EXISTS 'UsersPhone' ('Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'NAVOrId' TEXT, " +
                           "'UserPhone' TEXT, 'PhoneType' TEXT, 'Date' TEXT, 'Time' TEXT, UNIQUE('NAVOrId', 'UserPhone') ON CONFLICT REPLACE );",
                          connection))
                    { command.ExecuteNonQuery(); }

                    //6
                    using (SQLiteCommand command = new SQLiteCommand(
                          "CREATE TABLE IF NOT EXISTS 'UsersRelation' ('Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'NAVOrId' TEXT, 'NAVOrIdSecond' TEXT, 'NAVOrIdSecontTypeConnected' TEXT, " +
                          "'Date' TEXT, 'Time' TEXT, UNIQUE('NAVOrId', 'NAVOrIdSecond' , 'NAVOrIdSecontTypeConnected') ON CONFLICT REPLACE );",
                          connection))
                    { command.ExecuteNonQuery(); }

                    //7
                    using (SQLiteCommand command = new SQLiteCommand(
                          "CREATE TABLE IF NOT EXISTS 'UsersDocuments' ('Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'NAVOrId' TEXT, 'UserFIOUkr' TEXT, 'UserFIO' TEXT, " +
                          "'PasportId' TEXT, 'PasportDate' TEXT, 'PasportStaff' TEXT, 'PasportType' TEXT, 'Date' TEXT, 'Time' TEXT, UNIQUE('NAVOrId', 'PasportId') ON CONFLICT REPLACE );",
                          connection))
                    { command.ExecuteNonQuery(); }

                    //8
                    using (SQLiteCommand command = new SQLiteCommand(
                          "CREATE TABLE IF NOT EXISTS 'TypePeopleConnected' ('Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'ManMain' TEXT, 'ManSlave' TEXT,  UNIQUE('ManMain', 'ManSlave') ON CONFLICT REPLACE );",
                          connection))
                    { command.ExecuteNonQuery(); }
                }
            }
        }

        private void ImportExcelDataIntoDBItem_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");

            if (fileExcelWillImport == null)
            {
                MessageBox.Show("Вначале необходимо выбрать Excel-файл\nи проверить,\nподходит ли его таблицы для импорта");
                return;
            }
            else
            {
                ImportDataIntoTempList(fileExcelWillImport, "UsersMain",
                   "INSERT INTO 'UsersMain' ('NAVOrId', 'UserFIOUkr', 'UserLogin', 'UserMail', 'UserFoder', 'UserAddInfo', 'CityTSP', 'BirthDay', 'BirthDayPlace', 'EmploymentDate', 'DismissalDate', " +
                   "'WorkInCompany', 'Subdivision', 'UserPosition', 'Experience', 'Sex') VALUES(@NAVOrId, @UserFIOUkr, @UserLogin, @UserMail, @UserFoder, @UserAddInfo, @CityTSP, @BirthDay, " +
                   "@BirthDayPlace, @EmploymentDate, @DismissalDate, @WorkInCompany, @Subdivision, @UserPosition, @Experience, @Sex)",
                   fi.FullName);

                ImportDataIntoTempList(fileExcelWillImport, "UserWork",
                "INSERT INTO 'UserWork' ('NAVOrId', 'UserFIOUkr', 'CityTSP', 'EmploymentDate', 'DismissalDate', 'WorkInCompany', 'Subdivision', 'UserPosition') VALUES (@NAVOrId, @UserFIOUkr," +
                " @CityTSP, @EmploymentDate, @DismissalDate, @WorkInCompany, @Subdivision, @UserPosition)",
                fi.FullName);

                ImportDataIntoTempList(fileExcelWillImport, "UserLearn",
                "INSERT INTO 'UserLearn' ('NAVOrId', 'UserFIOUkr', 'CityLearn', 'LearnDateBegin', 'LearnDateEnd', 'School', 'Qualification', 'TypeOfEducation') VALUES (@NAVOrId, @UserFIOUkr, " +
                "@CityLearn, @LearnDateBegin, @LearnDateEnd, @School, @Qualification, @TypeOfEducation)",
                fi.FullName);

                ImportDataIntoTempList(fileExcelWillImport, "UsersAddress",
                "INSERT INTO 'UsersAddress' ('NAVOrId', 'UserFIOUkr', 'UserAddress', 'Date', 'Time') VALUES (@NAVOrId, @UserFIOUkr, @UserAddress, @Date, @Time)",
                fi.FullName);

                ImportDataIntoTempList(fileExcelWillImport, "UsersPhone",
                "INSERT INTO 'UsersPhone' ('NAVOrId', 'UserPhone', 'PhoneType', 'Date', 'Time') VALUES (@NAVOrId, @UserPhone, @PhoneType, @Date, @Time)",
                fi.FullName);

                ImportDataIntoTempList(fileExcelWillImport, "UsersRelation",
                "INSERT INTO 'UsersRelation' ('NAVOrId', 'NAVOrIdSecond', 'NAVOrIdSecontTypeConnected', 'Date', 'Time') VALUES (@NAVOrId, @NAVOrIdSecond, @NAVOrIdSecontTypeConnected, @Date, @Time)",
                fi.FullName);

                ImportDataIntoTempList(fileExcelWillImport, "UsersDocuments",
                "INSERT INTO 'UsersDocuments' ('NAVOrId', 'UserFIOUkr', 'UserFIO', 'PasportId', 'PasportDate', 'PasportStaff', 'PasportType', 'Date', 'Time') VALUES (@NAVOrId, @UserFIOUkr, @UserFIO, " +
                "@PasportId, @PasportDate, @PasportStaff, @PasportType, @Date, @Time)",
                fi.FullName);
            }
            _UpdateDataGrid("SELECT * FROM 'UsersMain' ORDER BY 'NAVOrId' ASC;", fi.FullName);
            DataGridViewSelectedTable = "usersprivate";
            tabControl.SelectedTab = tabDataGrid;
            dataGridView1.Focus();
        }

        private void ImportExcelDataIntoDbMainItem_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");

            /*
-- Try to update any existing row
UPDATE players SET user_name='steven', age=32 WHERE user_name='steven';

-- Make sure it exists
INSERT OR IGNORE INTO players (user_name, age) VALUES ('steven', 32); 
*/
            ImportDataIntoTempList(fileExcelWillImport, "UsersMain",
               "INSERT OR REPLACE INTO 'UsersMain' ('NAVOrId', 'UserFIOUkr', 'UserLogin', 'UserMail', 'UserFolder', 'UserAddInfo', 'CityTSP', 'BirthDay', 'BirthDayPlace', 'EmploymentDate', 'DismissalDate', " +
               "'WorkInCompany', 'Subdivision', 'UserPosition', 'Experience', 'Sex') VALUES(@NAVOrId, @UserFIOUkr, @UserLogin, @UserMail, @UserFolder, @UserAddInfo, @CityTSP, @BirthDay, " +
               "@BirthDayPlace, @EmploymentDate, @DismissalDate, @WorkInCompany, @Subdivision, @UserPosition, @Experience, @Sex)",
               fi.FullName);

            _UpdateDataGrid("SELECT * FROM 'UsersMain' ORDER BY 'NAVOrId' ASC;", fi.FullName);
            DataGridViewSelectedTable = "usersprivate";
        }

        private void ImportExcelDataIntoDbPhoneItem_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");

            ImportDataIntoTempList(fileExcelWillImport, "UsersPhone",
            "INSERT OR REPLACE INTO 'UsersPhone' ('NAVOrId', 'UserPhone', 'PhoneType', 'Date', 'Time') VALUES (@NAVOrId, @UserPhone, @PhoneType, @Date, @Time)",
            fi.FullName);

            _UpdateDataGrid("SELECT * FROM 'UsersPhone' ORDER BY 'NAVOrId' ASC;", fi.FullName);
            DataGridViewSelectedTable = "usersprivate";
        }

        private void ImportExcelDataIntoDbAddressItem_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");

            ImportDataIntoTempList(fileExcelWillImport, "UsersAddress",
            "INSERT OR REPLACE INTO 'UsersAddress' ('NAVOrId', 'UserFIOUkr', 'UserAddress', 'Date', 'Time') VALUES (@NAVOrId, @UserFIOUkr, @UserAddress, @Date, @Time)",
            fi.FullName);

            _UpdateDataGrid("SELECT * FROM 'UsersAddress' ORDER BY 'NAVOrId' ASC;", fi.FullName);
            DataGridViewSelectedTable = "usersprivate";
        }

        private void ImportExcelDataIntoDbWorkItem_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");

            ImportDataIntoTempList(fileExcelWillImport, "UserWork",
    "INSERT OR REPLACE INTO 'UserWork' ('NAVOrId', 'UserFIOUkr', 'CityTSP', 'EmploymentDate', 'DismissalDate', 'WorkInCompany', 'Subdivision', 'UserPosition') VALUES (@NAVOrId, @UserFIOUkr," +
    " @CityTSP, @EmploymentDate, @DismissalDate, @WorkInCompany, @Subdivision, @UserPosition)",
    fi.FullName);

            _UpdateDataGrid("SELECT * FROM 'UserWork' ORDER BY 'NAVOrId' ASC;", fi.FullName);
            DataGridViewSelectedTable = "usersprivate";
        }

        private void ImportExcelDataIntoDbLearnItem_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");

            ImportDataIntoTempList(fileExcelWillImport, "UserLearn",
            "INSERT OR REPLACE INTO 'UserLearn' ('NAVOrId', 'UserFIOUkr', 'CityLearn', 'LearnDateBegin', 'LearnDateEnd', 'School', 'Qualification', 'TypeOfEducation') VALUES (@NAVOrId, @UserFIOUkr, " +
            "@CityLearn, @LearnDateBegin, @LearnDateEnd, @School, @Qualification, @TypeOfEducation)",
            fi.FullName);

            _UpdateDataGrid("SELECT * FROM 'UserLearn' ORDER BY 'NAVOrId' ASC;", fi.FullName);
            DataGridViewSelectedTable = "usersprivate";
        }

        private void ImportExcelDataIntoDbDocumentItem_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");

            ImportDataIntoTempList(fileExcelWillImport, "UsersDocuments",
            "INSERT OR REPLACE INTO 'UsersDocuments' ('NAVOrId', 'UserFIOUkr', 'UserFIO', 'PasportId', 'PasportDate', 'PasportStaff', 'PasportType', 'Date', 'Time') VALUES (@NAVOrId, @UserFIOUkr, @UserFIO, " +
            "@PasportId, @PasportDate, @PasportStaff, @PasportType, @Date, @Time)",
            fi.FullName);
            _UpdateDataGrid("SELECT * FROM 'UsersDocuments' ORDER BY 'NAVOrId' ASC;", fi.FullName);
            DataGridViewSelectedTable = "usersprivate";
        }

        private void ImportExcelDataIntoDbRelationItem_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");

            ImportDataIntoTempList(fileExcelWillImport, "UsersRelation",
            "INSERT OR REPLACE INTO 'UsersRelation' ('NAVOrId', 'NAVOrIdSecond', 'NAVOrIdSecontTypeConnected', 'Date', 'Time') VALUES (@NAVOrId, @NAVOrIdSecond, @NAVOrIdSecontTypeConnected, @Date, @Time)",
            fi.FullName);

            _UpdateDataGrid("SELECT * FROM 'UsersRelation' ORDER BY 'NAVOrId' ASC;", fi.FullName);
            DataGridViewSelectedTable = "usersprivate";
            tabControl.SelectedTab = tabDataGrid;
            dataGridView1.Focus();
        }

        private void ImportExcelToDB()
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");

            ImportDataIntoTempList(fileExcelWillImport, "UsersMain",
               "INSERT INTO 'UsersMain' ('NAVOrId', 'UserFIOUkr', 'UserLogin', 'UserMail', 'UserFoder', 'UserAddInfo', 'CityTSP', 'BirthDay', 'BirthDayPlace', 'EmploymentDate', 'DismissalDate', " +
               "'WorkInCompany', 'Subdivision', 'UserPosition', 'Experience', 'Sex') VALUES(@NAVOrId, @UserFIOUkr, @UserLogin, @UserMail, @UserFoder, @UserAddInfo, @CityTSP, @BirthDay, " +
               "@BirthDayPlace, @EmploymentDate, @DismissalDate, @WorkInCompany, @Subdivision, @UserPosition, @Experience, @Sex)",
               fi.FullName);

            ImportDataIntoTempList(fileExcelWillImport, "UserWork",
            "INSERT INTO 'UserWork' ('NAVOrId', 'UserFIOUkr', 'CityTSP', 'EmploymentDate', 'DismissalDate', 'WorkInCompany', 'Subdivision', 'UserPosition') VALUES (@NAVOrId, @UserFIOUkr," +
            " @CityTSP, @EmploymentDate, @DismissalDate, @WorkInCompany, @Subdivision, @UserPosition)",
            fi.FullName);

            ImportDataIntoTempList(fileExcelWillImport, "UserLearn",
            "INSERT INTO 'UserLearn' ('NAVOrId', 'UserFIOUkr', 'CityLearn', 'LearnDateBegin', 'LearnDateEnd', 'School', 'Qualification', 'TypeOfEducation') VALUES (@NAVOrId, @UserFIOUkr, " +
            "@CityLearn, @LearnDateBegin, @LearnDateEnd, @School, @Qualification, @TypeOfEducation)",
            fi.FullName);

            ImportDataIntoTempList(fileExcelWillImport, "UsersAddress",
            "INSERT INTO 'UsersAddress' ('NAVOrId', 'UserFIOUkr', 'UserAddress', 'Date', 'Time') VALUES (@NAVOrId, @UserFIOUkr, @UserAddress, @Date, @Time)",
            fi.FullName);

            ImportDataIntoTempList(fileExcelWillImport, "UsersPhone",
            "INSERT INTO 'UsersPhone' ('NAVOrId', 'UserPhone', 'PhoneType', 'Date', 'Time') VALUES (@NAVOrId, @UserPhone, @PhoneType, @Date, @Time)",
            fi.FullName);

            ImportDataIntoTempList(fileExcelWillImport, "UsersRelation",
            "INSERT INTO 'UsersRelation' ('NAVOrId', 'NAVOrIdSecond', 'NAVOrIdSecontTypeConnected', 'Date', 'Time') VALUES (@NAVOrId, @NAVOrIdSecond, @NAVOrIdSecontTypeConnected, @Date, @Time)",
            fi.FullName);

            ImportDataIntoTempList(fileExcelWillImport, "UsersDocuments",
            "INSERT INTO 'UsersDocuments' ('NAVOrId', 'UserFIOUkr', 'UserFIO', 'PasportId', 'PasportDate', 'PasportStaff', 'PasportType', 'Date', 'Time') VALUES (@NAVOrId, @UserFIOUkr, @UserFIO, " +
            "@PasportId, @PasportDate, @PasportStaff, @PasportType, @Date, @Time)",
            fi.FullName);

            _UpdateDataGrid("SELECT * FROM 'UsersMain' ORDER BY 'NAVOrId' ASC;", fi.FullName);
            DataGridViewSelectedTable = "usersprivate";
            tabControl.SelectedTab = tabDataGrid;
            dataGridView1.Focus();
        }

        private List<string> tempListHeadersCollumns = new List<string>();
        private List<string> tempListDataCollumns = new List<string>();
        private List<string> personData = new List<string>();

        private void ImportToPersonData(FileInfo fileExcel)      // import data from Excel's file into the List "PersonData"
        {
            string temp = "";
            using (ExcelPackage package = new ExcelPackage(fileExcel, true))
            {
                personData = new List<string>();
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1]; //get ONly first WorkSheet from the Excel's file
                int totalRows = worksheet.Dimension.End.Row, totalCols = 0;

                // Get names of collumns
                for (int j = 1; j <= worksheet.Dimension.End.Column; j++)  //get the first row with names of collumns and get totals collumns with data
                { try { temp += worksheet.Cells[1, j].Value.ToString().Trim() + "|"; totalCols += 1; } catch { } }

                if (totalCols == 0) //try to get names of collumns and get totals collumns with data from the second row if first row is empty
                {
                    for (int j = 1; j <= worksheet.Dimension.End.Column; j++)
                    { try { temp += worksheet.Cells[2, j].Value.ToString().Trim() + "|"; totalCols += 1; } catch { } }
                }

                if (temp.Length > 0 && totalCols > 1)
                    personData.Add(temp);

                int checkFirstRawDataLength = 0;
                for (int i = 1; i <= totalRows; i++) //writes only data with value
                {
                    temp = "";
                    checkFirstRawDataLength = 0;
                    try { checkFirstRawDataLength = worksheet.Cells[i, 1].Value.ToString().Trim().Length; } catch { }
                    try { checkFirstRawDataLength += worksheet.Cells[i, 2].Value.ToString().Trim().Length; } catch { }

                    for (int j = 1; j <= totalCols; j++)
                    {
                        if (i > 1 && checkFirstRawDataLength > 0)
                            try { temp += worksheet.Cells[i, j].Value.ToString().Trim() + "|"; } catch { temp += "|"; }
                    }
                    if (temp.Length > 0 && !temp.ToLower().Contains("код карточки"))
                    {
                        personData.Add(temp);
                        _textBoxLogs(temp + "\n");
                    }
                }
                _textBoxLogs("\n");
                _textBoxLogs("End. Import - personData\n");
                _textBoxLogs("\n");
            }
        }

        private string[] personHeadersAll = {
"NAVOrId",
"UserFIOUkr",
"UserLogin",
"UserMail",
"UserFolder",
"UserAddInfo",
"BirthDay",
"BirthDayPlace",
"EmploymentDate",
"DismissalDate",
"WorkInCompany",
"Subdivision",
"UserPosition",
"Experience",
"Sex",
"CityTSP",
"CityLearn",
"LearnDateBegin",
"LearnDateEnd",
"School",
"Qualification",
"TypeOfEducation",
"UserAddress",
"UserPhone",
"PhoneType",
"NAVOrIdSecond",
"NAVOrIdSecontTypeConnected",
"UserFIO",
"PasportId",
"PasportDate",
"PasportStaff",
"PasportType"
        };

        private string[] personHeadersUsersMainMatch = {
"NAVOrId",
"UserFIOUkr"
        };

        private string[] personHeadersUsersMain = {
"NAVOrId",
"UserFIOUkr",
"UserLogin",
"UserMail",
"UserFolder",
"UserAddInfo",
"CityTSP",
"BirthDay",
"BirthDayPlace",
"EmploymentDate",
"DismissalDate",
"WorkInCompany",
"Subdivision",
"UserPosition",
"Experience",
"Sex"
        };

        private string[] personHeadersUserWorkMatch = {
"NAVOrId",
"UserFIOUkr",
"CityTSP",
"EmploymentDate",
"WorkInCompany"
        };

        private string[] personHeadersUserWork = {
"NAVOrId",
"UserFIOUkr",
"CityTSP",
"EmploymentDate",
"DismissalDate",
"WorkInCompany",
"Subdivision",
"UserPosition"
        };

        private string[] personHeadersUserLearnMatch = {
"NAVOrId",
"UserFIOUkr",
"CityLearn",
"LearnDateBegin",
"School"
        };

        private string[] personHeadersUserLearn = {
"NAVOrId",
"UserFIOUkr",
"CityLearn",
"LearnDateBegin",
"LearnDateEnd",
"School",
"Qualification",
"TypeOfEducation"
        };

        private string[] personHeadersUsersAddressMatch = {
"NAVOrId",
"UserAddress",
        };

        private string[] personHeadersUsersAddress = {
"NAVOrId",
"UserAddress",
"Date",
"Time"
        };

        private string[] personHeadersUsersPhoneMatch = {
"NAVOrId",
"UserPhone"
        };

        private string[] personHeadersUsersPhone = {
"NAVOrId",
"UserPhone",
"PhoneType",
"Date",
"Time"
        };

        private string[] personHeadersUsersRelationMatch = {
"NAVOrId",
"NAVOrIdSecond",
"NAVOrIdSecontTypeConnected"
        };

        private string[] personHeadersUsersRelation = {
"NAVOrId",
"NAVOrIdSecond",
"NAVOrIdSecontTypeConnected",
"Date",
"Time"
        };

        private string[] personHeadersUsersDocumentsMatch = {
"NAVOrId",
"UserFIO",
"PasportId",
"PasportDate",
"PasportStaff"
        };

        private string[] personHeadersUsersDocuments = {
"NAVOrId",
"UserFIOUkr",
"UserFIO",
"PasportId",
"PasportDate",
"PasportStaff",
"PasportType",
"Date",
"Time"
        };

        private void ImportDataIntoTempList(string importedFile, string tableName, string stringSQL, string dbFullPath)
        {
            _GetTimeRunScan();

            int[] indxHeaders = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
            int importedRows = 0;

            string[] personHeaders = personHeadersUsersMain;
            string[] personHeadersMatch = personHeadersUsersMainMatch;

            if (tableName.ToLower().Contains("usersphone"))
            {
                personHeaders = personHeadersUsersPhone;
                personHeadersMatch = personHeadersUsersPhoneMatch;
            }
            else if (tableName.ToLower().Contains("userwork"))
            {
                personHeaders = personHeadersUserWork;
                personHeadersMatch = personHeadersUserWorkMatch;
            }
            else if (tableName.ToLower().Contains("userlearn"))
            {
                personHeaders = personHeadersUserLearn;
                personHeadersMatch = personHeadersUserLearnMatch;
            }
            else if (tableName.ToLower().Contains("usersaddress"))
            {
                personHeaders = personHeadersUsersAddress;
                personHeadersMatch = personHeadersUsersAddressMatch;
            }
            else if (tableName.ToLower().Contains("usersrelation"))
            {
                personHeaders = personHeadersUsersRelation;
                personHeadersMatch = personHeadersUsersRelationMatch;
            }
            else if (tableName.ToLower().Contains("usersdocuments"))
            {
                personHeaders = personHeadersUsersDocuments;
                personHeadersMatch = personHeadersUsersDocumentsMatch;
            }
            string[] importedRow = new string[personHeaders.Length];
            string[] importedRowMatch = new string[personHeadersMatch.Length];
            string[] storedRowInTable = new string[personHeaders.Length];

            try
            {
                FileInfo fiExcel = new FileInfo(importedFile);
                ImportToPersonData(fiExcel);
                string[] headersArray = personData.ToArray();

                // Get Data and set it into the table
                for (int indxRow = 0; indxRow < headersArray.Length; indxRow++)
                {
                    if (headersArray[indxRow] != null && headersArray[indxRow].Length > 0)
                    {
                        string[] tmpRowArray = Regex.Split(headersArray[indxRow], "[|]");
                        if (indxRow == 0)
                        {
                            for (int idxT = 0; idxT < tmpRowArray.Length - 1; idxT++)
                            {
                                for (int idxPH = 0; idxPH < personHeaders.Length; idxPH++)
                                {
                                    if (tmpRowArray[idxT] != null && personHeaders[idxPH] != null && tmpRowArray[idxT] == personHeaders[idxPH])
                                    {
                                        indxHeaders[idxPH] = idxT;
                                    }
                                }
                            }
                        }
                        else
                        {
                            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", dbFullPath)))
                            {
                                connection.Open();

                                //start variation
                                using (SQLiteCommand command = new SQLiteCommand(stringSQL + ";", connection))
                                {
                                    importedRow = new string[personHeaders.Length];
                                    importedRowMatch = new string[personHeadersMatch.Length];
                                    storedRowInTable = new string[personHeaders.Length];

                                    for (int idxT = 0; idxT < tmpRowArray.Length - 1; idxT++)
                                    {
                                        for (int idxPH = 0; idxPH < personHeaders.Length; idxPH++)
                                        {
                                            if (idxT == indxHeaders[idxPH])
                                            {
                                                _textBoxLogs("\n0." + personHeaders[idxPH] + " - " + tmpRowArray[idxT] + "| ");
                                                if (personHeaders[idxPH] != null && tmpRowArray[idxT] != null && personHeaders[idxPH].Length > 0 && tmpRowArray[idxT].Trim().Length > 0)
                                                {
                                                    //add the data from the cell situated at personHeaders' collumn and the selected row
                                                    //command.Parameters.Add("@" + personHeaders[idxPH], DbType.String).Value = tmpRowArray[idxT].Trim();
                                                    importedRow[idxPH] = tmpRowArray[idxT].Trim();
                                                    for (int idxHeader = 0; idxHeader < personHeadersMatch.Length; idxHeader++)    //заполняем матрицу значений импортируемой строки для проверки, есть ли эти данные в таблице
                                                    {
                                                        if (personHeadersMatch[idxHeader] == personHeaders[idxPH])
                                                        {
                                                            importedRowMatch[idxHeader] = tmpRowArray[idxT].Trim();
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    _textBoxLogs("\n");

                                    //Формируем SQL-запрос к отобраннойтаблице БД на основе ключевых полей 
                                    //для проверки совпадений и только для исключения удаления уже внесенных данных
                                    string sqlCheckRow = null;
                                    for (int idxHed = 0; idxHed < personHeadersMatch.Length; idxHed++)
                                    {
                                        if (importedRowMatch[idxHed] != null && importedRowMatch[idxHed].Length > 0)
                                        {
                                            if (sqlCheckRow != null)
                                            { sqlCheckRow += " AND " + personHeadersMatch[idxHed] + " LIKE '" + importedRowMatch[idxHed] + "'"; }
                                            else
                                            { sqlCheckRow += personHeadersMatch[idxHed] + " LIKE '" + importedRowMatch[idxHed] + "'"; }
                                        }
                                    }
                                    _textBoxLogs("\nsqlCheckRow: " + sqlCheckRow + "\n");

                                    //Ищем данные в отобраннойтаблице БД по ключевым полям и заполняем ими структуру storedRowInTable
                                    for (int idxHed = 0; idxHed < personHeaders.Length; idxHed++)
                                    {
                                        if (importedRow[idxHed] != null && importedRow[idxHed].Length > 0)
                                        {
                                            using (SQLiteCommand command1 = new SQLiteCommand("SELECT * FROM " + tableName + " WHERE " + sqlCheckRow + ";", connection))
                                            {
                                                SQLiteDataReader reader = command1.ExecuteReader();
                                                try
                                                {
                                                    string dataStored = "";
                                                    foreach (DbDataRecord record in reader)
                                                    {
                                                        dataStored = "";
                                                        if (record != null)
                                                        {
                                                            for (int indexStoredRaw = 0; indexStoredRaw < personHeaders.Length; indexStoredRaw++)
                                                            {
                                                                dataStored += record[personHeaders[indexStoredRaw]].ToString() + " ";     // only for test!!!
                                                                storedRowInTable[indexStoredRaw] = record[personHeaders[indexStoredRaw]].ToString();
                                                            }
                                                            textBoxLogs.AppendText(dataStored + " \n");
                                                        }
                                                    }
                                                } catch { }
                                            }
                                        }
                                    }

                                    //добавить сверку данных и запись в базу только отсутствующих данных
                                    string tmp = "";
                                    for (int idxHed = 0; idxHed < personHeaders.Length; idxHed++)
                                    {
                                        if (storedRowInTable[0] != null || importedRow[0] != null)
                                        {
                                            if (storedRowInTable[idxHed] != null && storedRowInTable[idxHed].Length > 0)
                                            {
                                                tmp = storedRowInTable[idxHed];
                                            }
                                            else if (importedRow[idxHed] != null && importedRow[idxHed].Length > 0)
                                            {
                                                tmp = importedRow[idxHed];
                                            }
                                            _textBoxLogs("\npersonHeaders:" + personHeaders[idxHed] + " | storedRowInTable:" + storedRowInTable[idxHed] + " | importedRow:" + importedRow[idxHed] + " | tmp:" + tmp + "\n");
                                            command.Parameters.Add("@" + personHeaders[idxHed], DbType.String).Value = tmp;
                                        }
                                        tmp = "";
                                    }

                                    // данные затираются - проверить какие данные попадают

                                    try { command.ExecuteNonQuery(); importedRows += 1; } catch (Exception expt) { _textBoxLogs("\n1." + tableName + ": " + expt.Message); }
                                }      //End

                            }
                        }
                    }
                }
                //                   MessageBox.Show(fiExcel+" импортирован в базу успешно!");
                _textBoxLogs("\n");
                _textBoxLogs("из " + fiExcel + " импортировано " + importedRows + " строк \nв таблицу " + tableName + " БД: " + dbFullPath);
                _textBoxLogs("\n");
                _StatusLabelCurrentText("из " + fiExcel.Name + " импортировано " + importedRows + " строк");
            } catch (Exception expt) { MessageBox.Show(expt.Message); }
            //           dataInDb.Clear();
        }

        private void importDataIntoDataGridItem_Click(object sender, EventArgs e) //call ImportDataIntoDatagrid();
        {
            ImportDataIntoDatagrid();
        }

        private void ImportDataIntoDatagrid() //iMPORT Data from Excel into Datagrid
        {
            //https://www.codeproject.com/Articles/680421/Create-Read-Edit-Advance-Excel-Report-in
            //http://epplus.codeplex.com/documentation

            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");

            if (fileExcelWillImport == null)
            {
                MessageBox.Show("Предварительно необходимо выбрать Excel-файл\nи проверить,\nподходит ли его таблицы для импорта");
                return;
            }
            else
            {
                FileInfo fiExcel = new FileInfo(fileExcelWillImport);
                using (ExcelPackage package = new ExcelPackage(fiExcel, true))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1]; //get ONly first WorkSheet from the Excel's file
                    System.Collections.ArrayList Empty = new System.Collections.ArrayList();
                    dataGridView1.DataSource = Empty;
                    int totalRows = worksheet.Dimension.End.Row;
                    int totalCols = 0;
                    DataTable dt = new DataTable(worksheet.Name);
                    DataRow dr = null;

                    for (int j = 1; j <= worksheet.Dimension.End.Column; j++)
                    { try { dt.Columns.Add(worksheet.Cells[1, j].Value.ToString()); totalCols += 1; } catch { } } //get the first row with names of collumns and get totals collumns with data

                    if (totalCols == 0)
                    {
                        for (int j = 1; j <= worksheet.Dimension.End.Column; j++)
                        { try { dt.Columns.Add(worksheet.Cells[2, j].Value.ToString()); totalCols += 1; } catch { } } //try to get names of collumns and get totals collumns with data from the second row if first row is empty
                    }

                    for (int i = 1; i <= totalRows; i++)
                    {
                        if (i > 1) dr = dt.Rows.Add();
                        for (int j = 1; j <= totalCols; j++)
                        {
                            if (i > 1)
                                try { dr[j - 1] = worksheet.Cells[i, j].Value.ToString(); } catch { } //writes only data with value
                        }
                    }

                    dataGridView1.DataSource = dt;
                    Empty = null;
                    tabControl.SelectedTab = tabDataGrid;
                    dataGridView1.Focus();
                }
            }
        }

        private void findTablesFitItem_Click(object sender, EventArgs e)        //Определить какие в какую базу импортировать файл
        {
            MatchSelectedExcelFileAndTableDb();
        }

        private string fileExcelWillImport = null; //для DefiniteDbForImportSelectedExcelFile()
        private List<string> personDataExcelSelected = new List<string>();
        private int offsetRowfileExcelWillImport = 0;

        private void MatchSelectedExcelFileAndTableDb()
        {
            fileExcelWillImport = null;
            ImportExcelDataIntoDbMainItem.Visible = false;
            ImportExcelDataIntoDbWorkItem.Visible = false;
            ImportExcelDataIntoDbLearnItem.Visible = false;
            ImportExcelDataIntoDbAddressItem.Visible = false;
            ImportExcelDataIntoDbPhoneItem.Visible = false;
            ImportExcelDataIntoDbRelationItem.Visible = false;
            ImportExcelDataIntoDbDocumentItem.Visible = false;

            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");

            openFileDialogExcel.FileName = @"*.xlsx";
            openFileDialogExcel.Filter = "Excel файлы (*.xlsx)|*.xlsx";

            openFileDialogExcel.ShowDialog();
            if (openFileDialogExcel.FileName == null)
                return;
            else
            {
                int[] indxHeaders = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
                List<string> headersData = new List<string>();
                string tempData = "", checkData = ""; offsetRowfileExcelWillImport = 0;

                try
                {
                    FileInfo fiExcel = new FileInfo(openFileDialogExcel.FileName);
                    using (ExcelPackage package = new ExcelPackage(fiExcel, true))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[1]; //get ONly first WorkSheet from the Excel's file
                        int totalRows = worksheet.Dimension.End.Row, totalCols = 0;

                        // Get names of collumns
                        for (int findoffset = 0; findoffset < 10; findoffset++)
                        {
                            for (int j = 1; j <= worksheet.Dimension.End.Column; j++)  //get the first row with names of collumns and get totals collumns with data
                            {
                                try
                                {
                                    checkData = worksheet.Cells[1 + findoffset, j].Value.ToString().Trim();
                                    if (checkData.Length > 1)
                                    {
                                        headersData.Add(checkData);
                                        tempData += checkData + "|";
                                        totalCols += 1;
                                        offsetRowfileExcelWillImport = findoffset;
                                    }
                                    checkData = "";
                                } catch (Exception expt) { _textBoxLogs("\nExcel-file: " + openFileDialogExcel.FileName + "\nHas error: " + expt.Message + "\nValue in the cell: " + worksheet.Cells[1 + findoffset, j] + "\n"); }
                            }
                            if (totalCols > 0)
                                break;
                        }

                        if (tempData.Length > 0 && totalCols > 1)
                        {
                            string[] headersArray = headersData.ToArray();
                            string tmpTables = null; bool enableImport = false;
                            //Match Names of Excel's collumns and names collumns' of tables of DB  
                            if (MatchCollumnsExcelAndDB(headersArray, personHeadersUsersMainMatch, "UsersMain"))
                            {
                                ImportExcelDataIntoDbMainItem.Visible = true;
                                tmpTables += "- Main\n";
                                enableImport = true;
                            }

                            if (MatchCollumnsExcelAndDB(headersArray, personHeadersUserWorkMatch, "UserWork"))
                            {
                                ImportExcelDataIntoDbWorkItem.Visible = true;
                                tmpTables += "- Work\n";
                                enableImport = true;
                            }

                            if (MatchCollumnsExcelAndDB(headersArray, personHeadersUserLearnMatch, "UserLearn"))
                            {
                                ImportExcelDataIntoDbLearnItem.Visible = true;
                                tmpTables += "- Learn\n";
                                enableImport = true;
                            }

                            if (MatchCollumnsExcelAndDB(headersArray, personHeadersUsersAddressMatch, "UsersAddress"))
                            {
                                ImportExcelDataIntoDbAddressItem.Visible = true;
                                tmpTables += "- Address\n";
                                enableImport = true;
                            }

                            if (MatchCollumnsExcelAndDB(headersArray, personHeadersUsersPhoneMatch, "UsersPhone"))
                            {
                                ImportExcelDataIntoDbPhoneItem.Visible = true;
                                tmpTables += "- Phone\n";
                                enableImport = true;
                            }

                            if (MatchCollumnsExcelAndDB(headersArray, personHeadersUsersRelationMatch, "UsersRelation"))
                            {
                                ImportExcelDataIntoDbRelationItem.Visible = true;
                                tmpTables += "- Relation\n";
                                enableImport = true;
                            }

                            if (MatchCollumnsExcelAndDB(headersArray, personHeadersUsersDocumentsMatch, "UsersDocuments"))
                            {
                                ImportExcelDataIntoDbDocumentItem.Visible = true;
                                tmpTables += "- Documents\n";
                                enableImport = true;
                            }

                            _StatusLabelCurrentText("Will Import: " + fileExcelWillImport);
                            if (enableImport)
                            {
                                fileExcelWillImport = openFileDialogExcel.FileName;
                                _textBoxLogs(fileExcelWillImport + " можно импортировать в следующие таблицы БД:\n" + tmpTables);
                                MessageBox.Show("Выбранный Excel-файл можно импортировать в следующие таблицы БД:\n" + tmpTables);
                            }
                        }
                        else
                        { MessageBox.Show("Выбранный Excel-файл не подходит!\nМного пустых строк вначале первого листа."); _textBoxLogs(fileExcelWillImport + " не подходит!\nМного пустых строк вначале первого листа.\n"); }
                    }
                } catch (Exception expt) { _textBoxLogs("\nОшибка, при попытке доступа к " + fileExcelWillImport + "\n" + expt.Message + "\n"); MessageBox.Show(expt.Message); }
            }
        }

        private bool MatchCollumnsExcelAndDB(Array headersExcelArray, Array headersTableDB, string headersTableDBName)    //Match Names of collumns of table of DB and  Names collumns of Excel
        {
            int personHeaders = 0, personHeadersExcel = 0;
            try
            {
                foreach (string header in headersTableDB)
                {
                    if (header.Length > 0)
                    {
                        personHeaders += 1;
                        foreach (string importedHeader in headersExcelArray)
                        {
                            if (header.ToLower() == importedHeader.ToLower() && importedHeader.Length > 0)
                            {
                                personHeadersExcel += 1;
                            }
                        }
                    }
                }
                //                if (headersTableDBName.Contains("UsersAddress") || headersTableDBName.Contains("UsersRelation") || headersTableDBName.Contains("UsersPhone") || headersTableDBName.Contains("UsersDocuments"))   
                //                    personHeadersExcel += 2;
            } catch { }

            if (personHeaders == personHeadersExcel && personHeaders != 0 && personHeadersExcel != 0)
            {
                _textBoxLogs("\n" + headersTableDBName + " true: " + personHeadersExcel + "-" + -personHeaders + "\n");
                return true;
            }
            else
            {
                _textBoxLogs("\n" + headersTableDBName + " false: " + personHeadersExcel + "-" + -personHeaders + "\n");
                return false;
            }
        }

        private void ImportDataItem_Click(object sender, EventArgs e)     //test
        {
            openFileDialogExcel.FileName = @"*.xlsx";
            openFileDialogExcel.Filter = "Excel файлы (*.xlsx)|*.xlsx";

            openFileDialogExcel.ShowDialog();
            if (openFileDialogExcel.FileName == null)
                return;
            else
            {
                FileInfo fiExcel = new FileInfo(openFileDialogExcel.FileName);
                using (ExcelPackage package = new ExcelPackage(fiExcel, true))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1]; //get ONly first WorkSheet from the Excel's file
                    ArrayList Empty = new ArrayList();
                    dataGridView1.DataSource = Empty;
                    DataTable dt = WorksheetToDataTable(worksheet);
                    //       dataGridView1.DataSource = WorksheetToDataTable(worksheet);
                    dataGridView1.DataSource = dt;
                    Empty = null;
                }
            }
        }

        private DataTable WorksheetToDataTable(ExcelWorksheet oSheet)    //test
        {

            //https://www.codeproject.com/Articles/680421/Create-Read-Edit-Advance-Excel-Report-in
            //http://epplus.codeplex.com/documentation
            int totalRows = oSheet.Dimension.End.Row;
            int totalCols = 0;
            DataTable dt = new DataTable(oSheet.Name);
            DataRow dr = null;

            for (int j = 1; j <= oSheet.Dimension.End.Column; j++)
            { try { dt.Columns.Add(oSheet.Cells[1, j].Value.ToString()); totalCols += 1; } catch { } } //get the first row with names of collumns and get totals collumns with data

            if (totalCols == 0)
            {
                for (int j = 1; j <= oSheet.Dimension.End.Column; j++)
                { try { dt.Columns.Add(oSheet.Cells[2, j].Value.ToString()); totalCols += 1; } catch { } } //try to get names of collumns and get totals collumns with data from the second row if first row is empty
            }

            for (int i = 1; i <= totalRows; i++)
            {
                if (i > 1) dr = dt.Rows.Add();
                for (int j = 1; j <= totalCols; j++)
                {
                    if (i > 1)
                        try { dr[j - 1] = oSheet.Cells[i, j].Value.ToString(); } catch { } //writes only data with value
                }
            }
            return dt;

        }

        private void ExportToExcel()     //test
        {
            Microsoft.Office.Interop.Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook ExcelWorkBook;
            Microsoft.Office.Interop.Excel.Worksheet ExcelWorkSheet;
            //Книга.
            ExcelWorkBook = ExcelApp.Workbooks.Add(System.Reflection.Missing.Value);
            //Таблица.
            ExcelWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)ExcelWorkBook.Worksheets.get_Item(1);
            ExcelApp.Columns.ColumnWidth = 15;

            ExcelApp.Cells[1, 1] = "Departament";
            ExcelApp.Cells[1, 2] = "Type";
            ExcelApp.Cells[1, 3] = "Name";
            ExcelApp.Cells[1, 4] = "Number";
            ExcelApp.Cells[1, 5] = "Number of items";
            ExcelApp.Cells[1, 6] = "Delivery";
            ExcelApp.Cells[1, 7] = "Responsible";
            ExcelApp.Cells[1, 8] = "Note";
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                for (int j = 0; j < dataGridView1.ColumnCount; j++)
                {
                    ExcelApp.Cells[i + 2, j + 1] = dataGridView1.Rows[i].Cells[j].Value;
                }
            }
            //Вызываем нашу созданную эксельку.
            ExcelApp.Visible = true;
            ExcelApp.UserControl = true;
        }

        private void mainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");
            _UpdateDataGrid("SELECT * FROM 'UsersMain' ORDER BY 'NAVOrId' ASC;", fi.FullName);
            DataGridViewSelectedTable = "usersmain";
            tabControl.SelectedTab = tabDataGrid;
            dataGridView1.Focus();
        }

        private void phonesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");
            _UpdateDataGrid("SELECT * FROM 'usersphone' ORDER BY 'NAVOrId' ASC;", fi.FullName);
            DataGridViewSelectedTable = "usersphone";
            tabControl.SelectedTab = tabDataGrid;
            dataGridView1.Focus();
        }

        private void addressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");
            _UpdateDataGrid("SELECT * FROM 'usersaddress' ORDER BY 'NAVOrId' ASC;", fi.FullName);
            DataGridViewSelectedTable = "usersaddress";
            tabControl.SelectedTab = tabDataGrid;
            dataGridView1.Focus();
        }

        private void ViewStoredDataPersonalWorkItem_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");
            _UpdateDataGrid("SELECT * FROM 'userwork' ORDER BY 'NAVOrId' ASC;", fi.FullName);
            DataGridViewSelectedTable = "userwork";
            tabControl.SelectedTab = tabDataGrid;
            dataGridView1.Focus();
        }

        private void ViewStoredDataPersonalLearnItem_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");
            _UpdateDataGrid("SELECT * FROM 'userlearn' ORDER BY 'NAVOrId' ASC;", fi.FullName);
            DataGridViewSelectedTable = "userlearn";
            tabControl.SelectedTab = tabDataGrid;
            dataGridView1.Focus();
        }

        private void ViewStoredDataPersonalDocumentItem_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");
            _UpdateDataGrid("SELECT * FROM 'usersdocuments' ORDER BY 'NAVOrId' ASC;", fi.FullName);
            DataGridViewSelectedTable = "usersdocuments";
            tabControl.SelectedTab = tabDataGrid;
            dataGridView1.Focus();
        }

        private void ViewStoredDataPersonalRelationItem_Click(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");
            _UpdateDataGrid("SELECT * FROM 'usersrelation' ORDER BY 'NAVOrId' ASC;", fi.FullName);
            DataGridViewSelectedTable = "usersrelation";
            tabControl.SelectedTab = tabDataGrid;
            dataGridView1.Focus();
        }

        private void ShrinkDBItem_Click(object sender, EventArgs e)             // call of function - ShrinkDBUsersPrivate()
        {
            ShrinkDBUsersPrivate();
        }

        private void ShrinkDBUsersPrivate()
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");

            if (fi.Exists)
            {
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", fi.FullName)))
                {
                    connection.Open();

                    HelperSqlClean("usersmain", personHeadersUsersMainMatch, connection);
                    HelperSqlClean("usersphone", personHeadersUsersPhoneMatch, connection);
                    HelperSqlClean("userwork", personHeadersUserWorkMatch, connection);
                    HelperSqlClean("userlearn", personHeadersUserLearnMatch, connection);
                    HelperSqlClean("usersaddress", personHeadersUsersAddressMatch, connection);
                    HelperSqlClean("usersrelation", personHeadersUsersRelationMatch, connection);
                    HelperSqlClean("usersdocuments", personHeadersUsersDocumentsMatch, connection);

                    //Clean DB after Executed Command's of "Delete"
                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "vacuum;";
                        try { command.ExecuteNonQuery(); } catch { }
                    }
                }

                _textBoxLogs("\n");
                _StatusLabelCurrentText("База контактов сжата");

                _UpdateDataGrid("SELECT * FROM 'UsersMain' ORDER BY 'NAVOrId' ASC;", fi.FullName);
                DataGridViewSelectedTable = "usersmain";
                tabControl.SelectedTab = tabDataGrid;
                dataGridView1.Focus();
            }
        }

        private void HelperSqlClean(string nameTable, string[] personHeadersMatch, SQLiteConnection connection)      //subfunction for "ShrinkDBItem_Click()"
        {
            string sqlString = nameTable + " Where ";
            for (int idxStrName = 0; idxStrName < personHeadersMatch.Length - 1; idxStrName++)
            {
                if (idxStrName == 0)
                { sqlString += personHeadersMatch[idxStrName] + "= @" + personHeadersMatch[idxStrName] + " "; }
                else
                { sqlString += " AND " + personHeadersMatch[idxStrName] + "= @" + personHeadersMatch[idxStrName] + " "; }
            }

            _textBoxLogs("\n");
            _textBoxLogs("\nnameTable: " + nameTable);
            _textBoxLogs("\n");
            _textBoxLogs("sqlString -  " + sqlString);
            _textBoxLogs("\n");
            _StatusLabelCurrentText("База контактов сжата");


            using (SQLiteCommand command = new SQLiteCommand("DELETE FROM " + sqlString + ";", connection))
            {
                foreach (string strName in personHeadersMatch)
                {
                    command.Parameters.Add("@" + strName, DbType.String).Value = "";
                }
                try { command.ExecuteNonQuery(); } catch { }
            }
        }

        private void DeleteSelectedInDB()           // Test
        {
            if (DataGridViewSelectedTable.Contains("usersprivate"))
            {
                int IndexCurrentRow = dataGridView1.CurrentRow.Index;
                int IndexColumn1 = 0;           // индекс колонки "ComputerName" в датагрид
                int IndexColumn2 = 0;           // индекс колонки "ComputerIP" в датагрид
                int IndexColumn3 = 0;           // индекс колонки "User" в датагрид
                string TextColumn1 = "";  // содержимое "ComputerName" выбранной строки в датагрид
                string TextColumn2 = "";  // содержимое "ComputerIP" выбранной строки в датагрид
                string TextColumn3 = "";  // содержимое "User" выбранной строки в датагрид

                for (int i = 0; i < dataGridView1.ColumnCount; i++)
                {
                    if (dataGridView1.Columns[i].HeaderText.ToString() == "NAV")
                        IndexColumn1 = i;
                    if (dataGridView1.Columns[i].HeaderText.ToString() == "ComputerIP")
                        IndexColumn2 = i;
                    if (dataGridView1.Columns[i].HeaderText.ToString() == "User")
                        IndexColumn3 = i;
                }
                TextColumn1 = dataGridView1.Rows[IndexCurrentRow].Cells[IndexColumn1].Value.ToString();
                TextColumn2 = dataGridView1.Rows[IndexCurrentRow].Cells[IndexColumn2].Value.ToString();
                TextColumn3 = dataGridView1.Rows[IndexCurrentRow].Cells[IndexColumn3].Value.ToString();

                FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\UsersPrivate.db");

                if (fi.Exists)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", fi.FullName)))
                    {
                        connection.Open();
                        using (SQLiteCommand command = new SQLiteCommand("DELETE FROM UsersNav Where ComputerName=@ComputerName And ComputerIP=@ComputerIP And User=@User", connection))
                        {
                            command.Parameters.AddWithValue("@ComputerName", TextColumn1);
                            command.Parameters.AddWithValue("@ComputerIP", TextColumn2);
                            command.Parameters.AddWithValue("@User", TextColumn3);
                            try { command.ExecuteNonQuery(); } catch { }
                        }

                        //Clean DB after Executed Command's of "Delete"
                        using (SQLiteCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "vacuum;";
                            try { command.ExecuteNonQuery(); } catch { }
                        }

                        _UpdateDataGrid("SELECT * FROM 'UsersNav' ORDER BY 'GroupHosts' DESC;", fi.FullName);
                    }

                    DataGridViewSelectedTable = "usersprivate";
                    tabControl.SelectedTab = tabDataGrid;
                    dataGridView1.Focus();
                }
            }
        }

        private void DBUsersPrivateDeleteRowItem_Click(object sender, EventArgs e) //delete data from UsersPrivate. Use DeleteSelectedRowDatagridViewAnyTable()
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + "UsersPrivate" + @".db");
            if (fi.Exists)
            {
                if (DataGridViewSelectedTable == "userlearn" ||
                    DataGridViewSelectedTable == "userwork" ||
                    DataGridViewSelectedTable == "usersdocuments" ||
                    DataGridViewSelectedTable == "usersrelation" ||
                    DataGridViewSelectedTable == "usersaddress" ||
                    DataGridViewSelectedTable == "usersphone" ||
                    DataGridViewSelectedTable == "usersmain")
                {
                    DeleteSelectedRowDatagridViewAnyTable("NAVOrId", DataGridViewSelectedTable, fi.FullName);
                    _UpdateDataGrid("SELECT * FROM '" + DataGridViewSelectedTable + "' ORDER BY '" + "NAVOrId" + "' ASC;", fi.FullName);
                }
                else
                { _StatusLabelCurrentText("Have to select any Table from UsersPrivate"); _StatusLabelCurrentColor(Color.DarkRed); }
                tabControl.SelectedTab = tabDataGrid;
                dataGridView1.Focus();
            }
        }

        private void DeleteSelectedRowDatagridViewAnyTable(string headerbySorted, string tableName, string dbFullName)
        {
            int IndexCurrentRow = dataGridView1.CurrentRow.Index;
            int IndexColumn = 0;           // индекс колонки в датагрид
            string TextColumn = "";  // содержимое выбранной строки в датагрид
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                if (dataGridView1.Columns[i].HeaderText.ToString().ToLower() == "id")
                    IndexColumn = i;
            }
            TextColumn = dataGridView1.Rows[IndexCurrentRow].Cells[IndexColumn].Value.ToString();

            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", dbFullName)))
            {
                connection.Open();
                _textBoxLogs("Deleted FROM: " + tableName + "\n");
                using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM " + tableName + " Where id='" + TextColumn + "'; ", connection))
                {
                    SQLiteDataReader reader = command.ExecuteReader();
                    try
                    {
                        foreach (DbDataRecord record in reader)
                        {
                            if (record != null)
                            {
                                _textBoxLogs("id: " + record["id"].ToString() + "  |   NavOrId: " + record["NavOrId"].ToString() + " \n");
                            }
                        }
                    } catch { }
                }

                using (SQLiteCommand command = new SQLiteCommand("DELETE FROM " + tableName + " Where id=@id", connection))
                {
                    command.Parameters.AddWithValue("@id", TextColumn);
                    try { command.ExecuteNonQuery(); } catch { }
                }

                //Compact DB after it has executed Command's "Delete Row"
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "vacuum;";
                    try { command.ExecuteNonQuery(); } catch { }
                }

                DataGridViewSelectedTable = tableName.ToLower();
            }
        }

        private void importExcelItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Меню: База данных\\База людей\\Импорт Excel-файла в БД\\Определить тип таблицы\n" +
                "------------------------------------\n" +
                "Если в Excel-таблице есть все поля для импорта, то данный пункт меню становится доступным. " +
                "Возможен импорт только 1 файла в формате Excel 2007-2010 за один раз " +
                "со следующим минимальным набором имен колонок на первом листе:\n" +
                "---\n" +
                "Main: NAVOrId, UserFIOUkr, UserLogin, UserMail, UserFoder, UserAddInfo, CityTSPCurrent, BirthDay, BirthDayPlace, EmploymentDate, DismissalDate, WorkInCompany, Subdivision, UserPosition, Experience, Sex\n\n" +
                "Work: NAVOrId, UserFIOUkr, CityTSP, EmploymentDate, DismissalDate, WorkInCompany, Subdivision, UserPosition\n\n" +
                "Learn: NAVOrId, UserFIOUkr, CityLearn, LearnDateBegin, LearnDateEnd, School, Qualification, TypeOfEducation\n\n" +
                "Address: NAVOrId, UserAddress\n\n" +
                "Phone: NAVOrId, UserPhone, PhoneType\n\n" +
                "Relation: NAVOrId, NAVOrIdSecond, NAVOrIdSecontTypeConnected\n\n" +
                "Documents: NAVOrId, UserFIOUkr, UserFIO, PasportId, PasportDate, PasportStaff, PasportType\n",
                "Описание пункта Меню",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);
        }

        /*
       // GroupJoin 
            // Fill the DataSet.
DataSet ds = new DataSet();
ds.Locale = CultureInfo.InvariantCulture;
FillDataSet(ds);

DataTable contacts = ds.Tables["Contact"];
DataTable orders = ds.Tables["SalesOrderHeader"];

var query =
    from contact in contacts.AsEnumerable()
    join order in orders.AsEnumerable()
    on contact.Field<Int32>("ContactID") equals
    order.Field<Int32>("ContactID")
    select new
    {
        ContactID = contact.Field<Int32>("ContactID"),
        SalesOrderID = order.Field<Int32>("SalesOrderID"),
        FirstName = contact.Field<string>("FirstName"),
        Lastname = contact.Field<string>("Lastname"),
        TotalDue = order.Field<decimal>("TotalDue")
    };


foreach (var contact_order in query)
{
    Console.WriteLine("ContactID: {0} "
                    + "SalesOrderID: {1} "
                    + "FirstName: {2} "
                    + "Lastname: {3} "
                    + "TotalDue: {4}",
        contact_order.ContactID,
        contact_order.SalesOrderID,
        contact_order.FirstName,
        contact_order.Lastname,
        contact_order.TotalDue);
}

            //join
            // Fill the DataSet.
DataSet ds = new DataSet();
ds.Locale = CultureInfo.InvariantCulture;
FillDataSet(ds);

DataTable orders = ds.Tables["SalesOrderHeader"];
DataTable details = ds.Tables["SalesOrderDetail"];

var query =
    from order in orders.AsEnumerable()
    join detail in details.AsEnumerable()
    on order.Field<int>("SalesOrderID") equals
        detail.Field<int>("SalesOrderID")
    where order.Field<bool>("OnlineOrderFlag") == true
    && order.Field<DateTime>("OrderDate").Month == 8
    select new
    {
        SalesOrderID =
            order.Field<int>("SalesOrderID"),
        SalesOrderDetailID =
            detail.Field<int>("SalesOrderDetailID"),
        OrderDate =
            order.Field<DateTime>("OrderDate"),
        ProductID =
            detail.Field<int>("ProductID")
    };


foreach (var order in query)
{
    Console.WriteLine("{0}\t{1}\t{2:d}\t{3}",
        order.SalesOrderID,
        order.SalesOrderDetailID,
        order.OrderDate,
        order.ProductID);
}

 */

        //-------------/\/\/\/\------------//  Start UP Functions. End of the Block  //-------------/\/\/\/\------------//





        //-------------/\/\/\/\------------//  Generate Certifications and KEYS for This Programm. Start of Block  //-------------/\/\/\/\------------//

        //Signer
        private void SignerItem_Click(object sender, EventArgs e) //Generate Keys - Only for me!!!!!
        { _GenerateKeysCerificates(); }

        private void SignerRecoverItem_Click(object sender, EventArgs e) //Sign and Check signer
        { SignAndCheckKeys(); }

        private void licenseItem_Click(object sender, EventArgs e)
        { _CheckLicense(); }

        private void SignGenerateLicenseItem_Click(object sender, EventArgs e) //Генерирование лицензии на основе данных временноого ключа
        { generateLicense(); }

        private void _GenerateKeysCerificates() //Для администратора -  Generate Keys!!!!!
        {
            KuSigner kuSigner = new KuSigner(1000, 4); // стойкость ключа , длина подписи
            kuSigner.GenerateKeys(); // генерация ключей: открытый и закрытый.

            string privateKey = kuSigner.ToXmlString(true); // оба ключа, и публичный и приватный 
            FileInfo prk = new FileInfo("private.key"); //Generate privateKey and publicKey
            if (File.Exists(prk.FullName))
            {
                File.Delete(prk.FullName);
                File.WriteAllText(prk.FullName, privateKey);
            }
            else
            { File.WriteAllText(prk.FullName, privateKey); }

            //Generate PublicKey
            //Сгенерированный ключ внутри программы - LicKeypublic
            string publicKey = kuSigner.ToXmlString(false); // только публичный ключ
            FileInfo pbk = new FileInfo("public.key");  //Generate publicKey
            if (File.Exists(pbk.FullName))
            {
                File.Delete(pbk.FullName);
                File.WriteAllText(pbk.FullName, publicKey);
            }
            else
            { File.WriteAllText(pbk.FullName, publicKey); }

            textBoxLogs.AppendText("\n-----------------------------------------------------\nСертификаты сгенерированы\n-----------------------------------------------------\n");
        }

        private void SignAndCheckKeys() //Для администратора - генерация лицензии. Sign and Check signer
        {
            List<string> signatureString = new List<string>();
            List<byte[]> signatureBytes = new List<byte[]>();
            string labelTemp = labelLicense.Text;
            _ControlVisibleStartUP(false); //visible only Licence input
            _ControlVisibleAnalyse(false);
            _ControlVisibleLicense(true);
            labelLicense.Text = "Лицензия:";
            //--------------------

            //Generate Private and Public keys. Part only for me
            string privateKey = "";
            FileInfo prk = new FileInfo("private.key");
            if (File.Exists(prk.FullName))
            { privateKey = File.ReadAllText(prk.FullName); }
            KuSigner privateSigner = new KuSigner(privateKey);

            string[] UnoKeysSign = Regex.Split(_GetUniqKey("|"), "[|]");
            for (int i = 0; i < UnoKeysSign.Length; i++)
            {
                if (UnoKeysSign[i] != null && UnoKeysSign[i].Length > 0)
                {
                    signatureBytes.Add(privateSigner.Sign(Encoding.UTF8.GetBytes(UnoKeysSign[i]))); // для красоты, байты можно представить в виде строки (серийный номер)
                }
            }
            signatureBytes.ToArray();


            //Public Part
            string LicPublic = myEventLoger.Properties.Resources.LicKeypublic;
            /*
                                    FileInfo pbk = new FileInfo("public.key");
                                    if (File.Exists(pbk.FullName))
                                    { publicKey = File.ReadAllText(pbk.FullName); }
                        */

            KuSigner PublicSigner = new KuSigner(LicPublic);

            string[] UnoKeys = Regex.Split(_GetUniqKey("|"), "[|]");
            textBoxLogs.AppendText("\n--------------------\n");
            textBoxLogs.AppendText("UnoKeysSign[j]  isValid.ToString()  \n");
            bool isValid = true; int j = 0;
            foreach (byte[] SignatureBytes in signatureBytes)
            {
                if (SignatureBytes != null && SignatureBytes.Length > 0)
                {
                    isValid = PublicSigner.VerifySignature(Encoding.UTF8.GetBytes(UnoKeysSign[j]), SignatureBytes);// Проверка подписи строки 
                    signatureString.Add(BitConverter.ToString(SignatureBytes).Replace("-", string.Empty));
                    textBoxLogs.AppendText("UnoKeysSign:  " + UnoKeysSign[j] + "  isValid: " + isValid.ToString() + "   \n");
                    j++;
                }
            }

            signatureString.ToArray();
            textBoxLogs.AppendText("signatureString:\n");
            j = 0;
            foreach (string SignatureString in signatureString)
            {
                if (SignatureString != null && SignatureString.Length > 0)
                { textBoxLogs.AppendText("UnoKeysSign:  " + UnoKeysSign[j] + "   SignatureString:" + SignatureString + "\n"); j++; }
            }

            textBoxLogs.AppendText("\n");
            labelLicense.Text = labelTemp;
        }

        private void _CheckLicense() //Проверка наличия лицензии, запись ее в реестр
        {
            try
            {
                string labelTemp = "";
                if (labelLicense.Text != null)
                { labelTemp = labelLicense.Text; }
                _ControlVisibleStartUP(false); //visible only Licence input
                _ControlVisibleAnalyse(false);
                _ControlVisibleLicense(true);
                labelLicense.Visible = true;
                _labelLicense("Лицензия:");
                _labelResultCheckingLicenseVisible(true);
                //--------------------

                List<bool> isValidArray = new List<bool>();
                bool isValid;
                string plec = "";
                bool KeyInRegistry = false;
                string myEvLogKeyRead = @"SOFTWARE\RYIK\SIEM";
                try
                {
                    using (var LicKey = Registry.CurrentUser.OpenSubKey(myEvLogKeyRead)) //Get last Time using of USB
                    { plec = LicKey.GetValue("License").ToString(); KeyInRegistry = true; }
                } catch { }

                FileInfo plck = new FileInfo("myELLic.key");
                if (File.Exists(plck.FullName) || KeyInRegistry)
                {
                    if (File.Exists(plck.FullName))
                    { plec = File.ReadAllText(plck.FullName); }

                    string plic = "", plac = "";
                    string[] keylic = new string[5];

                    if (plec.Contains(@"<myEventLogerKeyValue><LicKey>"))
                    {
                        plic = plec.Remove(0, 30);
                        keylic[0] = plic.Remove(0, 0).Remove(8); //Доделать формирование ключей
                        keylic[1] = plic.Remove(0, 8).Remove(8);
                        keylic[2] = plic.Remove(0, 16).Remove(8);
                        keylic[3] = plic.Remove(0, 24).Remove(8);
                        keylic[4] = plic.Remove(0, 32).Remove(8);

                        for (int i = 0; i < 5; i++)
                        {
                            if (i < 4)
                            { plac += keylic[i] + "-"; }
                            else { plac += keylic[i]; }
                        }
                        _textBoxLicense(plac);
                    }

                    string LicPublic = myEventLoger.Properties.Resources.LicKeypublic;
                    try
                    {
                        KuSigner PublicSigner = new KuSigner(LicPublic);

                        string[] UnoKeys = Regex.Split(_GetUniqKey("|"), "[|]");
                        for (int ik = 0; ik < 5; ik++)
                        {
                            isValid = PublicSigner.VerifySignature(Encoding.UTF8.GetBytes(UnoKeys[ik]), StringToByteArray(keylic[ik]));// Проверка подписи строки 
                            isValidArray.Add(isValid);
                            textBoxLogs.AppendText("UnoKeys:  " + UnoKeys[ik] + "  isValid: " + isValid.ToString() + "  keylic: " + keylic[ik] + "   \n");
                        }
                    } catch { MessageBox.Show("Поврежден файл \"public.key\" в папке с программой!"); }

                    /*
                    FileInfo pbk = new FileInfo("public.key");
                    if (File.Exists(pbk.FullName))
                    {
                        try
                        {
                            KuSigner PublicSigner = new KuSigner(File.ReadAllText(pbk.FullName));

                            string[] UnoKeys = Regex.Split(_GetUniqKey("|"), "[|]");
                            for (int ik = 0; ik < 5; ik++)
                            {
                                isValid = PublicSigner.VerifySignature(Encoding.UTF8.GetBytes(UnoKeys[ik]), StringToByteArray(keylic[ik]));// Проверка подписи строки 
                                isValidArray.Add(isValid);
                                textBoxLogs.AppendText("UnoKeys:  " + UnoKeys[ik] + "  isValid: " + isValid.ToString() + "  keylic: " + keylic[ik] + "   \n");
                            }
                        }
                        catch { MessageBox.Show("Поврежден файл \"public.key\" в папке с программой!"); }
                    }
                    else MessageBox.Show("Отсутствует файл \"public.key\" в папке с программой!");
*/
                }
                else { MessageBox.Show("Отсутствует лицензионный ключ к программе!"); }
                isValidArray.ToArray(); int iAll = 0;
                for (int i = 0; i < isValidArray.ToArray().Length; i++)
                {
                    if (isValidArray.ToArray()[i])
                    { iAll++; }
                }

                if (iAll > (isValidArray.ToArray().Length - 3) && (isValidArray.ToArray()[isValidArray.ToArray().Length - 1]))
                {
                    _textBoxLicenseBackColor(System.Drawing.Color.YellowGreen);
                    _textBoxLicenseForeColor(System.Drawing.Color.Black);
                    _labelResultCheckingLicenseBackColor(System.Drawing.Color.DarkSeaGreen);
                    _labelResultCheckingLicenseText("Используется легальная лицензия ПО для данного ПК");

                    //                _myELKey = plec;
                }
                if (iAll > (isValidArray.ToArray().Length - 2) && (isValidArray.ToArray()[isValidArray.ToArray().Length - 1]))
                {
                    _textBoxLicenseBackColor(System.Drawing.Color.DarkSeaGreen);
                    _textBoxLicenseForeColor(System.Drawing.Color.Black);
                    _labelResultCheckingLicenseBackColor(System.Drawing.Color.DarkSeaGreen);
                    _labelResultCheckingLicenseText("Используется легальная лицензия ПО для данного ПК");

                    FileInfo tmpk = new FileInfo("temp.key");
                    if (File.Exists(tmpk.FullName))
                    { File.Delete(tmpk.FullName); }
                    else { }
                    _myELKey = plec;
                    string myEvLogKey = @"SOFTWARE\RYIK\SIEM";
                    try
                    {
                        using (var LicKey = Registry.CurrentUser.CreateSubKey(myEvLogKey)) //Get last Time using of USB
                        { LicKey.SetValue("License", plec); }
                    } catch { MessageBox.Show("Ошибка доступа к реестру!/nНе могу сохранить ключ в реестре!"); }
                    MessageBox.Show("Разблокированы все возможности программы!");
                }
                else
                {
                    _GenerateTempKey();
                    _textBoxLicenseBackColor(System.Drawing.Color.SandyBrown);
                    _textBoxLicenseForeColor(System.Drawing.Color.Black);
                    _labelResultCheckingLicenseBackColor(System.Drawing.Color.SandyBrown);
                    _labelResultCheckingLicenseText("Некорректная лицензия!");

                    string s = Environment.CurrentDirectory;
                    System.Diagnostics.Process.Start("explorer", s);
                    MessageBox.Show("Сгенерирован временный ключ для получения лицензии\n\"temp.key\"");
                }
                Task.Delay(5000).Wait();
                _labelLicense(labelTemp);
            } catch (Exception e) { MessageBox.Show(e.ToString()); }
            _ControlVisibleStartUP(true);// Only License input is invisible
            _ControlVisibleAnalyse(false);
            _ControlVisibleLicense(false);
            _labelResultCheckingLicenseVisible(false);
        }

        private void generateLicense()  //Для администратора -  Генерирование лицензии на основе временного ключа (на основе публичного ключа в прг) и приватного из _GenerateKeysCerificates()
        {
            /*       KuSigner kuSigner = new KuSigner(1000, 4); // стойкость ключа , длина подписи
                   kuSigner.GenerateKeys(); // генерация ключей: открытый и закрытый.
            string privateKey = kuSigner.ToXmlString(true); // оба ключа, и публичный и приватный 
            FileInfo prk1 = new FileInfo("private.key"); //Generate privateKey and publicKey
            if (File.Exists(prk1.FullName))
            {
                File.Delete(prk1.FullName);
                File.WriteAllText(prk1.FullName, privateKey1);
            }
            else
            { File.WriteAllText(prk1.FullName, privateKey1); }
            */

            string privateKey = "";
            FileInfo prk = new FileInfo("private.key");
            if (File.Exists(prk.FullName))
            {
                privateKey = File.ReadAllText(prk.FullName);
            }


            FileInfo plck = new FileInfo("temp.key");
            if (File.Exists(plck.FullName))
            {
                string tmlic = File.ReadAllText(plck.FullName);
                string plic = "", plac = "";
                string[] keylic = new string[5];

                if (tmlic.Contains(@"<myEventLogerKeyValue><TempKey>"))
                {
                    plic = tmlic.Remove(0, 31);
                    keylic[0] = plic.Remove(0, 0).Remove(6); //Доделать формирование ключей
                    keylic[1] = plic.Remove(0, 6).Remove(6);
                    keylic[2] = plic.Remove(0, 12).Remove(6);
                    keylic[3] = plic.Remove(0, 18).Remove(8);
                    keylic[4] = plic.Remove(0, 26).Remove(8);

                    /*        string privateKey = "";
                          FileInfo prk = new FileInfo("private.key");
                           if (File.Exists(prk.FullName))
                           { privateKey = File.ReadAllText(prk.FullName); }*/

                    KuSigner privateSigner = new KuSigner(privateKey);
                    plac = @"<myEventLogerKeyValue><LicKey>";
                    for (int i = 0; i < keylic.Length; i++)
                    {
                        if (keylic[i] != null && keylic[i].Length > 0)
                        {
                            plac += BitConverter.ToString(privateSigner.Sign(Encoding.UTF8.GetBytes(keylic[i]))).Replace("-", string.Empty); // для красоты, байты можно представить в виде строки (серийный номер)
                        }
                    }
                    plac += @"</LicKey></myEventLogerKeyValue>";
                    FileInfo plick = new FileInfo("myELLic.key");
                    if (File.Exists(plick.FullName))
                    {
                        File.Delete(plick.FullName);
                        File.WriteAllText(plick.FullName, plac);
                    }
                    else
                    { File.WriteAllText(plick.FullName, plac); }

                    string s = Environment.CurrentDirectory;
                    System.Diagnostics.Process.Start("explorer", s);
                    MessageBox.Show("Сгенерирован лицензионный ключ\n\"myELLic.key\"\nна основании временного ключа:\n\"temp.key\"");
                }
            }
        }

        private static byte[] StringToByteArray(string value) //Hex to Byte Array
        {
            value = "0x" + value;
            byte[] bytes = null;
            if (string.IsNullOrEmpty(value))
                bytes = null;
            else
            {
                int string_length = value.Length;
                int character_index = (value.StartsWith("0x", StringComparison.Ordinal)) ? 2 : 0; // Does the string define leading HEX indicator '0x'. Adjust starting index accordingly.               
                int number_of_characters = string_length - character_index;

                bool add_leading_zero = false;
                if (0 != (number_of_characters % 2))
                {
                    add_leading_zero = true;

                    number_of_characters += 1;  // Leading '0' has been striped from the string presentation.
                }

                bytes = new byte[number_of_characters / 2]; // Initialize our byte array to hold the converted string.

                int write_index = 0;
                if (add_leading_zero)
                {
                    bytes[write_index++] = FromCharacterToByte(value[character_index], character_index);
                    character_index += 1;
                }

                for (int read_index = character_index; read_index < value.Length; read_index += 2)
                {
                    byte upper = FromCharacterToByte(value[read_index], read_index, 4);
                    byte lower = FromCharacterToByte(value[read_index + 1], read_index + 1);

                    bytes[write_index++] = (byte)(upper | lower);
                }
            }

            return bytes;
        }

        private static byte FromCharacterToByte(char character, int index, int shift = 0) //Helper for StringToByteArray()
        {
            byte value = (byte)character;
            if (((0x40 < value) && (0x47 > value)) || ((0x60 < value) && (0x67 > value)))
            {
                if (0x40 == (0x40 & value))
                {
                    if (0x20 == (0x20 & value))
                        value = (byte)(((value + 0xA) - 0x61) << shift);
                    else
                        value = (byte)(((value + 0xA) - 0x41) << shift);
                }
            }
            else if ((0x29 < value) && (0x40 > value))
                value = (byte)((value - 0x30) << shift);
            else
                throw new InvalidOperationException(string.Format("Character '{0}' at index '{1}' is not valid alphanumeric character.", character, index));

            return value;
        }

        private string _GetUniqKey(string dev = null) //Generate Uniq Key for this Programm
        {
            string LocalAllUniqKeys = "";
            Dictionary<string, string> ids = new Dictionary<string, string>();
            ManagementObjectSearcher searcher;

            //процессор
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            foreach (ManagementObject queryObj in searcher.Get())
                ids.Add("ProcessorId", queryObj["ProcessorId"].ToString());

            //ОС
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM CIM_OperatingSystem");
            foreach (ManagementObject queryObj in searcher.Get())
                ids.Add("OSSerialNumber", queryObj["SerialNumber"].ToString());

            //UUID
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT UUID FROM Win32_ComputerSystemProduct");
            foreach (ManagementObject queryObj in searcher.Get())
                ids.Add("UUID", queryObj["UUID"].ToString());

            //Volume C
            ManagementObject disk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + "C" + @":""");
            disk.Get();
            string volumeSerial = disk["VolumeSerialNumber"].ToString();
            disk.Dispose();

            textBoxLogs.AppendText("\n");
            using (SysSec.MD5 md5Hash = SysSec.MD5.Create())
            {
                foreach (var x in ids)
                {
                    string hash = GetMd5Hash(md5Hash, x.Value);
                    LocalAllUniqKeys += hash.ToUpper().Remove(0, hash.Length - 6) + dev;
                }
            }
            LocalAllUniqKeys += volumeSerial.ToUpper() + dev;

            //Programm
            crc32Sec.Crc32 crc32 = new crc32Sec.Crc32(); //Check CRC32 this Programm
            string PrgHash = string.Empty;
            using (FileStream fs = File.OpenRead(Application.ExecutablePath))
            { foreach (byte b in crc32.ComputeHash(fs)) PrgHash += b.ToString("x2").ToUpper(); }
            LocalAllUniqKeys += PrgHash;

            return LocalAllUniqKeys;
        }

        private void _GenerateTempKey()   //Генерирование временного ключа для запроса лицензии
        {
            string key = @"<myEventLogerKeyValue><TempKey>";
            key += _GetUniqKey() + @"</TempKey></myEventLogerKeyValue>";
            FileInfo ptk = new FileInfo("temp.key");
            if (File.Exists(ptk.FullName))
            {
                File.Delete(ptk.FullName);
                File.WriteAllText(ptk.FullName, key);
            }
            else
            { File.WriteAllText(ptk.FullName, key); }
        }

        static string GetMd5Hash(SysSec.MD5 md5Hash, string input) //Генерация хэшей для ключей
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            { sBuilder.Append(data[i].ToString("x2")); }
            return sBuilder.ToString();
        }

        static bool VerifyMd5Hash(SysSec.MD5 md5Hash, string input, string hash)// Проверка хэшей
        {
            string hashOfInput = GetMd5Hash(md5Hash, input);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            if (0 == comparer.Compare(hashOfInput, hash))
            { return true; }
            else
            { return false; }
        }

        //-------------/\/\/\/\------------//  Generate Certifications and KEYS for This Programm. End of Block  //-------------/\/\/\/\------------//




        //-------------/\/\/\/\------------//  Common Functions. Start of the Block  //-------------/\/\/\/\------------// 

        private void _GetTimeRunScan() //Get Date and Time now
        {
            DateTime d = DateTime.Now;
            string _month = d.Month.ToString();
            if (_month.Length == 1) { _month = "0" + d.Month.ToString(); }
            string _day = d.Day.ToString();
            if (_day.Length == 1) { _day = "0" + d.Day.ToString(); }

            _Date = d.Year + "-" + _month + "-" + _day; ;
            _Time = d.ToLongTimeString();
        }

        unsafe DateTime GetRegKeyModifiedTime(RegistryKey key)      // Search the modifying Date of the hive of registry.
        {
            if (key == null)
                return DateTime.MinValue;

            long time = 0L;

            if (RegQueryInfoKey(
                    key.Handle.DangerousGetHandle(),
                    null, null, null, null,
                    null, null, null, null,
                    null, null, &time
                    ) != 0)
            { return DateTime.MinValue; }
            return DateTime.FromFileTime(time);
        }

        private void checkRunningRemoteRegistry(string HostName, string HostIP, string userLogin, string userPassword, string nameService = "RemoteRegistry", string stateService = "Run") //Check running of service "RemoteRegistry"
        {
            string errorState = "False";
            string correctState = "True";
            string actionCorrectState = " start ";
            string actionErrorState = "Stopped";

            if (stateService == "Run")
            {
                errorState = "False";
                correctState = "True";
                actionCorrectState = " start ";
                actionErrorState = "Stopped";
            }
            else
            {
                errorState = "True";
                correctState = "False";
                actionCorrectState = " stop ";
                actionErrorState = "Started";
            }

            _StatusLabel2Text("Проверяю настройки хоста... ");
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + HostName + @".db");
            databaseHost = fi.FullName;

            _currentRemoteRegistryRunning = false;

            ConnectionOptions co = new ConnectionOptions();   //Connect to WMI of the Remote Host 
            co.Impersonation = ImpersonationLevel.Impersonate;
            co.Authentication = AuthenticationLevel.Packet;
            co.EnablePrivileges = true;
            co.Timeout = new TimeSpan(0, 0, 30);

            ManagementPath mp = new ManagementPath();
            mp.NamespacePath = @"\root\cimv2";
            try
            {
                if (_currentHostName != _myHostName && _User.Length > 0 && _Password.Length > 0)
                {
                    co.Username = _User;
                    co.Password = _Password;
                    mp.Server = HostName;
                }
                else if (_currentHostName != _myHostName && _User.Length == 0 && _Password.Length == 0)
                { mp.Server = HostName; }

                //Используемые протоколы и порты, на которых служба ожидает входящий трафик: TCP: 135, TCP: 593
                ManagementScope ms = new ManagementScope(mp, co);
                ms.Connect();
                bool isConnected = ms.IsConnected;
                if (isConnected)
                {
                    // Проверить скорость работы и память!!!
                    /*System.ServiceProcess.ServiceController service = new System.ServiceProcess.ServiceController( "IISADMIN", "srv2" );
    service.Start();
    TimeSpan t = TimeSpan.FromSeconds( "10" );
    service.WaitForStatus( System.ServiceProcess.ServiceControllerStatus.Running, t );
    */
                    //TermService - RDP
                    ManagementObjectSearcher srcd;
                    ManagementObjectCollection queryCollection;
                    for (int k = 1; k < 5; k++)   //try to start up to 5 times of the service "RemoteRegistry" 
                    {
                        srcd = new ManagementObjectSearcher(ms, new ObjectQuery("SELECT Name,Started FROM Win32_Service")); //try Caption?
                        queryCollection = srcd.Get();
                        foreach (ManagementObject m in queryCollection)            //check running of the service "RemoteRegistry"
                        {
                            if (m["Name"].ToString().Contains("MpsSvc") && m["Started"].ToString().Contains("True"))
                            {
                                System.Diagnostics.Process process1 = new System.Diagnostics.Process();
                                string strCmdLine = "/c sc \\\\" + HostName + " stop MpsSvc";
                                process1.StartInfo.UseShellExecute = false;
                                process1.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                System.Diagnostics.Process.Start("CMD.exe", strCmdLine);
                                Task.Delay(200).Wait(); //ждать 0,2 с
                                process1.Close();
                            }

                            if (m["Name"].ToString().Contains(nameService) && m["Started"].ToString().Contains(errorState))
                            {

                                System.Diagnostics.Process process1 = new System.Diagnostics.Process();
                                string strCmdLine = "/c sc \\\\" + HostName + " config \"" + nameService + "\" start= demand";
                                process1.StartInfo.UseShellExecute = false;
                                process1.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                System.Diagnostics.Process.Start("CMD.exe", strCmdLine);
                                Task.Delay(200).Wait(); //ждать 0,2 с
                                process1.Close();


                                process1 = new System.Diagnostics.Process();
                                strCmdLine = "/c sc \\\\" + HostName + actionCorrectState + nameService;
                                process1.StartInfo.UseShellExecute = false;
                                process1.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                System.Diagnostics.Process.Start("CMD.exe", strCmdLine);
                                Task.Delay(200).Wait(); //ждать 0,2 с
                                process1.Close();
                            }

                            else if (m["Name"].ToString().Contains(nameService) && m["Started"].ToString().Contains(correctState) || m["Name"].ToString().Contains("MpsSvc") && m["Started"].ToString().Contains("False"))
                            {
                                _currentRemoteRegistryRunning = true;
                                k = 5;
                            }
                            //                           Task.Delay(200).Wait(); //ждать 0,2 с
                        }
                    }

                    srcd = new ManagementObjectSearcher(ms, new ObjectQuery("SELECT Name,Started FROM Win32_Service")); //try Caption?
                    queryCollection = srcd.Get();
                    foreach (ManagementObject m in queryCollection)            //check running of the service "RemoteRegistry"
                    {
                        if (m["Name"].ToString().Contains(nameService) && m["Started"].ToString().Contains(errorState))
                        {
                            _currentRemoteRegistryRunning = false;
                            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;journal_mode =MEMORY;", databaseHost)))
                            {
                                connection.Open();
                                using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection))
                                {
                                    command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                                    command.Parameters.AddWithValue("@Name", "Error");
                                    command.Parameters.AddWithValue("@Value", nameService);
                                    command.Parameters.AddWithValue("@Value1", actionErrorState);
                                    command.Parameters.AddWithValue("@Date", _Date);
                                    command.Parameters.AddWithValue("@Time", _Time);
                                    try { command.ExecuteNonQuery(); } catch { }
                                }
                                connection.Close();
                            }

                            _StatusLabel2ForeColor(System.Drawing.Color.SandyBrown);
                            _StatusLabel2Text("Can't start the service of " + nameService + "!");
                            Task.Delay(500).Wait();
                            //MessageBox.Show("Can't start the service of \"RemoteRegistry\"");
                            break;
                        }

                        if (m["Name"].ToString().Contains("MpsSvc") && m["Started"].ToString().Contains("True"))
                        {
                            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;journal_mode =MEMORY;", databaseHost)))
                            {
                                connection.Open();
                                using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection))
                                {
                                    command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                                    command.Parameters.AddWithValue("@Name", "Warning");
                                    command.Parameters.AddWithValue("@Value", "MpsSvc");
                                    command.Parameters.AddWithValue("@Value1", "Running");
                                    command.Parameters.AddWithValue("@Date", _Date);
                                    command.Parameters.AddWithValue("@Time", _Time);
                                    try { command.ExecuteNonQuery(); } catch { }
                                }
                                connection.Close();
                            }
                        }
                    }
                }
                else
                {
                    using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;journal_mode =MEMORY;", databaseHost)))
                    {
                        connection.Open();
                        using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection))
                        {
                            command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                            command.Parameters.AddWithValue("@Name", "Error");
                            command.Parameters.AddWithValue("@Value", "Access at " + _currentHostName);
                            command.Parameters.AddWithValue("@Value1", "No Access!");
                            command.Parameters.AddWithValue("@Date", _Date);
                            command.Parameters.AddWithValue("@Time", _Time);
                            try { command.ExecuteNonQuery(); } catch { }
                        }
                        connection.Close();
                    }
                    _StatusLabel2Text("You should use other Username or Passsword!");
                }
            } catch { }
            if (HostIP == _myIP)
            { _currentRemoteRegistryRunning = true; }
            _StatusLabel2Text("");
        }

        private void textBoxToTemporary() //Copy TextBoxes' Data into variables
        {
            textBoxDomainText = textBoxDomain.Text.Trim().ToUpper();
            textBoxDomainLength = textBoxDomain.Text.Trim().Length;
            textBoxPasswordText = textBoxPassword.Text.Trim();
            textBoxPasswordLength = textBoxPassword.Text.Trim().Length;
            textBoxLoginText = textBoxLogin.Text.Trim();
            textBoxLoginLength = textBoxLogin.Text.Trim().Length;
            textBoxInputNetOrInputPCText = textBoxInputNetOrInputPC.Text.Trim();
            textBoxInputNetOrInputPCLength = textBoxInputNetOrInputPC.Text.Trim().Length;
            if (textBoxDomainLength == 0) //the domain has inputed. User isn't the domain one 
            {
                //                textBoxDomain.Text = textBoxInputNetOrInputPC.Text;
                //                textBoxDomainText = textBoxInputNetOrInputPC.Text;
            }

            if (textBoxDomainLength > 2 && textBoxPasswordLength > 0 && textBoxLoginLength > 0) //the domain has inputed. User isn't the domain one 
            {
                _User = textBoxDomainText + "\\" + textBoxLoginText;
                _Password = textBoxPasswordText;
            }
            else if (textBoxDomainLength < 3 && textBoxPasswordLength > 0 && textBoxLoginLength > 0) //the domain hasn't inputed. Will use No DOMAIN's USER
            {
                //                _User = _currentHostIP + "\\" + textBoxLoginText;
                _User = textBoxLoginText;
                _Password = textBoxPasswordText;
            }
            else
            {
                _User = "";
                _Password = "";
            }
        }

        private void ParseTextboxInputNetOrPC()
        {
            _InputedPCNameCorrect = false;
            _InputedPCIPCorrect = false;

            textBoxInputNetOrInputPCText = textBoxInputNetOrInputPC.Text.Trim().ToUpper();
            textBoxInputNetOrInputPCLength = textBoxInputNetOrInputPC.Text.Trim().Length;
            textBoxDomainText = textBoxDomain.Text.Trim().ToUpper();

            //            MessageBox.Show("1 _currentHostIP " + _currentHostIP + "\n_currentHostName " + _currentHostName);
            if (textBoxInputNetOrInputPCText != _myIP && textBoxInputNetOrInputPCText != _myHostName.ToUpper() && textBoxInputNetOrInputPCLength > 0)
            {
                try
                {
                    IPAddress addr = IPAddress.Parse(textBoxInputNetOrInputPCText); //if textBoxInputNetOrInputPCText like "192.168.0.1"
                    IPHostEntry entry = Dns.GetHostEntry(addr);

                    _currentHostIP = textBoxInputNetOrInputPCText;
                    _currentHostName = entry.HostName.ToUpper();
                    string[] tmpPC = Regex.Split(_currentHostIP, "[.]");
                    _currentHostNet = tmpPC[0].Trim() + "." + tmpPC[1].Trim() + "." + tmpPC[2].Trim();
                    _InputedPCNameCorrect = true;
                    _InputedPCIPCorrect = true;
                    textBoxInputNetOrInputPC.Text = _currentHostIP;
                } catch { _InputedPCIPCorrect = false; }

                if (!_InputedPCIPCorrect)
                {
                    try
                    {
                        foreach (IPAddress IPA in Dns.GetHostAddresses(textBoxInputNetOrInputPCText))
                        {
                            if (IPA.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                _currentHostIP = IPA.ToString();
                                _currentHostName = textBoxInputNetOrInputPCText;
                                string[] tmpPC = Regex.Split(_currentHostIP, "[.]");
                                _currentHostNet = tmpPC[0].Trim() + "." + tmpPC[1].Trim() + "." + tmpPC[2].Trim();
                                _InputedPCNameCorrect = true;
                                _InputedPCIPCorrect = true;
                                textBoxInputNetOrInputPC.Text = _currentHostIP;
                                break;
                            }
                        }
                    } catch { _InputedPCNameCorrect = false; }
                }

                if (!_InputedPCIPCorrect || !_InputedPCNameCorrect)
                {
                    try
                    {
                        foreach (IPAddress IPA in Dns.GetHostAddresses(textBoxInputNetOrInputPCText + @"." + textBoxDomainText))
                        {
                            if (IPA.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                IPHostEntry entry1 = Dns.GetHostEntry(IPA);
                                _currentHostIP = IPA.ToString();
                                _currentHostName = textBoxInputNetOrInputPCText + @"." + textBoxDomainText;
                                //                                textBoxDomain.Text = textBoxDomainText;

                                string[] tmpPC = Regex.Split(_currentHostIP, "[.]");
                                _currentHostNet = tmpPC[0].Trim() + "." + tmpPC[1].Trim() + "." + tmpPC[2].Trim();
                                _InputedPCNameCorrect = true;
                                _InputedPCIPCorrect = true;
                                textBoxInputNetOrInputPC.Text = _currentHostIP;
                                break;
                            }
                        }
                    } catch { _InputedPCNameCorrect = false; }
                }

                if (!_InputedPCIPCorrect)
                {
                    try
                    {
                        foreach (IPAddress IPA in Dns.GetHostAddresses(textBoxInputNetOrInputPCText + @"." + textBoxDomainText + @".ais"))
                        {
                            if (IPA.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                IPHostEntry entry1 = Dns.GetHostEntry(IPA);
                                _currentHostIP = IPA.ToString();
                                _currentHostName = textBoxInputNetOrInputPCText + @"." + textBoxDomainText + @".AIS";
                                //                                textBoxDomain.Text = textBoxDomainText + @".AIS";

                                string[] tmpPC = Regex.Split(_currentHostIP, "[.]");
                                _currentHostNet = tmpPC[0].Trim() + "." + tmpPC[1].Trim() + "." + tmpPC[2].Trim();
                                _InputedPCNameCorrect = true;
                                _InputedPCIPCorrect = true;
                                textBoxInputNetOrInputPC.Text = _currentHostIP;
                                break;
                            }
                        }
                    } catch
                    {
                        _currentHostIP = "127.0.0.1";
                        _currentHostName = textBoxInputNetOrInputPCText;
                        _currentHostNet = "127.0.0";
                        _InputedPCNameCorrect = false;
                    }
                }

                if (_InputedPCNameCorrect && _InputedPCIPCorrect)
                {
                    textBoxDomain.Enabled = false;
                    textBoxPassword.Enabled = false;
                    textBoxLogin.Enabled = false;
                    /*            IPAddress addr = IPAddress.Parse(textBoxInputNetOrInputPCText); 
                                IPHostEntry entry = Dns.GetHostEntry(addr);
                                _currentHostIP = addr.ToString();
                                string st = textBoxInputNetOrInputPCText;
                                try { st = Regex.Split(textBoxInputNetOrInputPCText, "[.]")[0]; } catch { }
                                if (Regex.Split(entry.HostName, "[.]")[0].ToUpper() == st)
                                {
                                    _currentHostName = entry.HostName;
                                    _textBoxInputNetOrInputPC(_currentHostIP);
                                    string[] tmpPC = Regex.Split(_currentHostIP, "[.]");
                                    _currentHostNet = tmpPC[0].Trim() + "." + tmpPC[1].Trim() + "." + tmpPC[2].Trim();
                                    InputedPC = true;
                                }   */
                }
            }
            else
            {
                _currentHostIP = _myIP;
                _currentHostName = _myHostName.ToUpper();
                _currentHostNet = _myNET;
            }

            if (_InputedPCIPCorrect)//MessageBox.Show("Dns Name is the known!");
            {
                _labelCurrentNet(_currentHostNet + ".xxx");

                if (textBoxDomainLength > 2 && textBoxPasswordLength > 0 && textBoxLoginLength > 0) //the domain has inputed. User isn't the domain one 
                {
                    _User = textBoxDomainText + "\\" + textBoxLoginText;
                    _Password = textBoxPasswordText;
                }
                else if (textBoxDomainLength < 3 && textBoxPasswordLength > 0 && textBoxLoginLength > 0) //the domain hasn't inputed. Will use No DOMAIN's USER
                {
                    //                    textBoxDomain.Text = _currentHostIP;
                    //                    _User =_currentHostName+"\\"+ textBoxLoginText;
                    _User = _currentHostIP + "\\" + textBoxLoginText;
                    _Password = textBoxPasswordText;
                }
                else
                {
                    _User = "";
                    _Password = "";
                }
            }

            _PingHostItem("Пинг " + _currentHostName);
            _RDPMenuItem("RDP коннект к " + _currentHostName);
            _GetLogRemotePCItem("Получить список событий и конфигурацию " + _currentHostName);
            _GetFilesItem("Получить список файлов " + _currentHostName);
            _GetRegItem("Сканировать реестр " + _currentHostName);
            GetArchieveOfHostItem.Text = "Архив данных собранных с " + _currentHostName;
            GetRegistryItem.Text = "Сканировать реестр " + _currentHostName;
            PingItem.Text = "Пинг хоста " + _currentHostName;
            GetEventsItem.Text = "Загрузить все события и конфигурацию " + _currentHostName;
            GetFileItems.Text = "Получить список файлов на " + _currentHostName;
            GetArchieveOfHostItem.Text = "Архив данных собранных с " + _currentHostName;
            GetWholeDataItem.Text = "Сбор всей информации c " + _currentHostName;
            _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP);
        }

        private static double DateTimeToUnixTimestamp(DateTime dateTime) //DateTime to UNIX timestamp         
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)   // Unix timestamp is seconds past epoch
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            DateTime res = DateTime.ParseExact(dtDateTime.ToString(), "dd.mm.yyyy hh:mm:ss", CultureInfo.InvariantCulture);
            return res;
        }

        private string TransformObjectToDateTime(string datetime)     // Transform  object DateTime format like yyyymmddhhmmss  to separate Date and Time like yyy-mm-dd hh:mm:ss
        {
            string ax = datetime;
            string datestr = Regex.Split(getDateTimeFromDmtfDate(ax), " ")[0];
            DateTime result = DateTime.ParseExact(datestr, formatsrc, CultureInfo.InvariantCulture);
            string dateCr = result.ToString(formatdst);
            string timeCr = Regex.Split(getDateTimeFromDmtfDate(ax), " ")[1];

            string datetimereturn = dateCr + " " + timeCr;
            return datetimereturn;
        }

        private static string getDmtfFromDateTime(DateTime dateTime)
        { return ManagementDateTimeConverter.ToDmtfDateTime(dateTime); }

        private static string getDmtfFromDateTime(string dateTime)
        {
            DateTime dateTimeValue = Convert.ToDateTime(dateTime);
            return getDmtfFromDateTime(dateTimeValue);
        }

        private static string getDateTimeFromDmtfDate(string dateTime)
        { return ManagementDateTimeConverter.ToDateTime(dateTime).ToString(); }


        //-------------/\/\/\/\------------//  Common Functions. End of the Block //-------------/\/\/\/\------------// 



        //get local %windir%   - Environment.GetEnvironmentVariable("windir")
        //-------------/\/\/\/\------------//  Search Hosts. Start of the Block //-------------/\/\/\/\------------// 

        private void _ReadRemoteRegistry(string HostName, string HostIP, string userLogin, string userPassword) //Read Remote Registryby Using of Class "NetworkShare"
        {
            _StatusLabel2Text("Собираю данные по конфигурации хоста... ");
            if (_StopScanLogEvents == false)
            {
                try
                {
                    string a1 = "", a2 = "", a3 = "", a4 = "", a5 = "", a6 = "", a7 = "", a8 = "", a9 = "", a10 = "", a11 = "", a12 = "", query = "";
                    FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + HostName + @".db");
                    databaseHost = fi.FullName;

                    //SQLite connection
                    using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};page_size = 65536; cache_size =100000;journal_mode =MEMORY;", databaseHost)))
                    {
                        connection.Open();
                        SQLiteCommand command = new SQLiteCommand(query, connection);
                        _StatusLabel2Text("Собираю данные по ОС... ");

                        if (_currentRemoteRegistryRunning == true)
                        {
                            for (int i = 0; i < _UsbKey.Length; i++)
                            { _UsbKey[i] = "0"; }

                            int NumberConfiguration = 1;

                            RegistryKey key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, HostName);
                            if (key != null)
                            {
                                key = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                                if (key != null)
                                {
                                    _ProductName = key.GetValue("ProductName").ToString();
                                    _SystemPath = key.GetValue("SystemRoot").ToString();
                                }
                            }

                            query = "INSERT INTO 'WindowsFeature' ('Name', 'Value', 'Value1', 'ComputerName', 'Date', 'Time')"
                                   + " VALUES ('OS: ','" + _ProductName + "','Set into: " + _SystemPath + "','" + _currentHostName + "','" + _Date + "','" + _Time + "'); ";

                            command = new SQLiteCommand(query, connection);
                            try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }


                            //Get number of configuration of System
                            key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, HostName);
                            if (key != null)
                            {
                                key = key.OpenSubKey(@"SYSTEM\Select");
                                if (key != null)
                                { NumberConfiguration = Convert.ToInt16(key.GetValue("LastKnownGood").ToString()); }
                            }

                            _StatusLabel2Text("Собираю данные по USB... ");

                            try
                            { //find out for used USB at CurrentControlSet 
                                key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, HostName);
                                string _USBKeyString = @"SYSTEM\CurrentControlSet\Enum\USBSTOR";
                                if (key != null)
                                {
                                    using (key = key.OpenSubKey(_USBKeyString))
                                    {
                                        string[] subNames = key.GetSubKeyNames();
                                        foreach (string s in subNames)                                                         //Get key of USB
                                        {
                                            if (s != null)
                                            {
                                                key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, HostName);
                                                string _USBKeyString1 = _USBKeyString + "\\" + s;
                                                using (key = key.OpenSubKey(_USBKeyString1))
                                                {
                                                    foreach (string s1 in key.GetSubKeyNames())                         //Get s/n of USB
                                                    {
                                                        if (s1 != null)
                                                        {
                                                            key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, HostName);
                                                            string _USBKeyString2 = _USBKeyString1 + "\\" + s1;
                                                            using (key = key.OpenSubKey(_USBKeyString2))
                                                            {
                                                                foreach (string valueName in key.GetValueNames())     //Get Name of USB
                                                                {
                                                                    string s2 = key.GetValue(valueName).ToString();
                                                                    if (valueName.Contains("FriendlyName") || valueName.Contains("ParentIdPrefix"))
                                                                    {
                                                                        for (int i = 0; i < _UsbKey.Length; i++)
                                                                        {
                                                                            try
                                                                            {
                                                                                if (_UsbKey[i].Length < 2)
                                                                                {
                                                                                    string usbBaseKey3 = _USBKeyString + s;
                                                                                    key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, HostName);
                                                                                    using (key = key.OpenSubKey(usbBaseKey3)) //Get last Time using of USB
                                                                                    { _UsbLastTime[i] = GetRegKeyModifiedTime(key).ToString(); }
                                                                                    _UsbKey[i] = s.ToString();
                                                                                    _UsbName[i] = s2.ToString();
                                                                                    _UsbSN[i] = s1.ToString();
                                                                                    i = _UsbKey.Length;
                                                                                }
                                                                            } catch { }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                for (int knownConfiguration = 1; knownConfiguration == NumberConfiguration; knownConfiguration++)    //find out for used USB at ControlSets
                                {
                                    key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, HostName);
                                    _USBKeyString = "SYSTEM\\ControlSet00" + knownConfiguration + "\\Enum\\USBSTOR";
                                    if (key != null)
                                    {
                                        using (key = key.OpenSubKey(_USBKeyString))
                                        {
                                            string[] subNames = key.GetSubKeyNames();
                                            foreach (string s in subNames)                                                         //Get key of USB
                                            {
                                                if (s != null)
                                                {
                                                    key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, HostName);
                                                    string _USBKeyString1 = _USBKeyString + "\\" + s;
                                                    using (key = key.OpenSubKey(_USBKeyString1))
                                                    {
                                                        if (key != null)
                                                        {
                                                            foreach (string s1 in key.GetSubKeyNames())                         //Get s/n of USB
                                                            {
                                                                if (s1 != null)
                                                                {
                                                                    key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, HostName);
                                                                    string _USBKeyString2 = _USBKeyString1 + "\\" + s1;
                                                                    using (key = key.OpenSubKey(_USBKeyString2))
                                                                    {
                                                                        foreach (string valueName in key.GetValueNames())     //Get Name of USB
                                                                        {
                                                                            if (valueName != null)
                                                                            {
                                                                                string s2 = key.GetValue(valueName).ToString();
                                                                                if (valueName.Contains("FriendlyName") || valueName.Contains("ParentIdPrefix"))
                                                                                {
                                                                                    for (int i = 0; i < _UsbKey.Length; i++)
                                                                                    {
                                                                                        try
                                                                                        {
                                                                                            if (_UsbKey[i].Length < 2)
                                                                                            {
                                                                                                string usbBaseKey3 = _USBKeyString + s;
                                                                                                key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, HostName);
                                                                                                using (key = key.OpenSubKey(usbBaseKey3)) //Get last Time using of USB
                                                                                                { _UsbLastTime[i] = GetRegKeyModifiedTime(key).ToString(); }
                                                                                                _UsbKey[i] = s.ToString();
                                                                                                _UsbName[i] = s2.ToString();
                                                                                                _UsbSN[i] = s1.ToString();
                                                                                                i = _UsbKey.Length;
                                                                                            }
                                                                                        } catch { }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            } catch
                            {
                                command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                                            " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                                command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                                command.Parameters.Add(new SQLiteParameter("@Name", "Warning"));
                                command.Parameters.AddWithValue("@Value", "USBCONNECTED");
                                command.Parameters.AddWithValue("@Value1", "Nothing");
                                command.Parameters.AddWithValue("@Date", _Date);
                                command.Parameters.AddWithValue("@Time", _Time);
                                try { command.ExecuteNonQuery(); } catch { }
                            }

                            try
                            {
                                for (int i = 0; i < _UsbKey.Length; i++)
                                {
                                    if (HostIP != _myIP)
                                    { getInstallTimeR(HostName, i); }
                                    else
                                    { getInstallTime(i); }
                                }
                            } catch (Exception e) { MessageBox.Show(e.ToString()); }

                            for (int i = 0; i < _UsbKey.Length; i++)
                            {
                                a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = ""; a7 = ""; query = "";
                                if (_UsbKey[i].Length > 1)
                                {
                                    _ProgressWork1();
                                    a1 = _UsbName[i];
                                    a2 = _UsbKey[i];
                                    a3 = _UsbSN[i].Substring(0, _UsbSN[i].Length - 2); //remove last 2 char

                                    try { a4 = _UsbInstallDate[i].Replace("[/]", "-"); } catch { }
                                    try { a5 = _UsbInstallTime[i].Remove(_UsbInstallTime[i].Length - 4); } catch { } //remove last 4 char
                                    try
                                    {
                                        string[] ax = Regex.Split(_UsbLastTime[i], " ");
                                        string at = ax[0].Trim();
                                        a6 = at.Substring(6) + "-" + at.Substring(3, 2) + "-" + at.Substring(0, 2);
                                        a7 = ax[1].Trim();
                                    } catch { }

                                    command = new SQLiteCommand("INSERT INTO 'UsbUsed' ('ComputerName', 'UsbKey', 'UsbSN', 'UsbName', 'InstallDate', 'InstallTime', 'LastDate', 'LastTime') " +
                                                " VALUES (@ComputerName, @UsbKey, @UsbSN, @UsbName, @InstallDate, @InstallTime, @LastDate, @LastTime)", connection);
                                    command.Parameters.AddWithValue("@ComputerName", HostName);
                                    command.Parameters.AddWithValue("@UsbKey", a1);
                                    command.Parameters.AddWithValue("@UsbSN", a3);
                                    command.Parameters.AddWithValue("@UsbName", a2);
                                    command.Parameters.AddWithValue("@InstallDate", a4);
                                    command.Parameters.AddWithValue("@InstallTime", a5);
                                    command.Parameters.AddWithValue("@LastDate", a6);
                                    command.Parameters.AddWithValue("@LastTime", a7);
                                    try { command.ExecuteNonQuery(); } catch { };
                                }
                            }

                            //Get mounted USB to Letters of local disk 
                            key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, HostName);
                            string _USBKey2 = "SYSTEM\\MountedDevices";
                            using (key = key.OpenSubKey(_USBKey2))
                            {
                                if (key != null)
                                {
                                    foreach (string valueName in key.GetValueNames())     //Get Name of USB
                                    {
                                        if (valueName != null)
                                        {
                                            a1 = ""; a2 = ""; query = "";
                                            byte[] s2 = (byte[])key.GetValue(valueName);
                                            var value = Encoding.Unicode.GetString(s2);
                                            a1 = valueName.ToString();
                                            if (a1.Contains("Volume"))
                                            {
                                                string ax = a1.Remove(0, 3); //remove first 3 char
                                                a1 = ax;
                                            }
                                            a2 = value.ToString();
                                            if (a2.Contains("USBSTOR"))
                                            {
                                                string ax = a2.Remove(0, 12); //remove first 12 char
                                                a2 = ax;
                                            }
                                            if (a2.Contains("IDE"))
                                            {
                                                string ax = a2.Remove(0, 8); //remove first 8 char
                                                a2 = ax;
                                            }

                                            command = new SQLiteCommand("INSERT INTO 'UsbMounted' ('ComputerName', 'ValueName', 'Value', 'Date', 'Time') VALUES (@ComputerName, @ValueName, @Value, @Date, @Time)", connection);
                                            command.Parameters.AddWithValue("@ComputerName", HostName);
                                            command.Parameters.AddWithValue("@ValueName", a1);
                                            command.Parameters.AddWithValue("@Value", a2);
                                            command.Parameters.AddWithValue("@Date", _Date);
                                            command.Parameters.AddWithValue("@Time", _Time);
                                            try { command.ExecuteNonQuery(); } catch { };
                                        }
                                    }
                                }
                            }
                            key.Close();
                        }

                        //----- WMI ----
                        ConnectionOptions co = new ConnectionOptions();   //Connect to WMI of the Remote Host 
                        co.Impersonation = ImpersonationLevel.Impersonate;
                        co.Authentication = AuthenticationLevel.Packet;
                        co.EnablePrivileges = true;
                        co.Timeout = new TimeSpan(0, 0, 1200);

                        ManagementPath mp = new ManagementPath();
                        mp.NamespacePath = @"\root\cimv2";
                        //                    if (_currentHostIP != _myIP && _User.Length > 0 && _Password.Length > 0)
                        //                    {
                        co.Username = _User;
                        co.Password = _Password;
                        mp.Server = HostName;
                        /*                   }
                                           else if (_currentHostIP != _myIP && _User.Length == 0 && _Password.Length == 0)
                                           { mp.Server = HostName; }
                                           else
                                           { mp.Server = _myHostName; }
                          */
                        try
                        {
                            command = new SQLiteCommand("begin", connection);
                            command.ExecuteNonQuery();

                            ManagementScope ms = new ManagementScope(mp, co);
                            ms.Connect();
                            bool isConnected = ms.IsConnected;
                            if (isConnected)
                            {

                                _StatusLabel2Text("Собираю данные из WMI по ОС... ");

                                //OS
                                ManagementObjectSearcher srcd = new ManagementObjectSearcher(ms, new ObjectQuery("Select * from Win32_OperatingSystem"));
                                ManagementObjectCollection queryCollection = srcd.Get();

                                foreach (ManagementObject m in queryCollection)
                                {
                                    if (m["Name"] != null)
                                    {
                                        string b1 = "", b2 = "", b3 = "", b4 = "", b5 = "", c1 = "", c2 = "", c3 = "";
                                        try { a1 = m["Caption"].ToString(); } catch { }
                                        try { b1 = m["CSDVersion"].ToString(); } catch { }
                                        try { a2 = m["SerialNumber"].ToString(); } catch { }
                                        try
                                        {
                                            b2 = TransformObjectToDateTime(m["InstallDate"].ToString());

                                            /*                                            string b21 = m["InstallDate"].ToString();
                                                                                        string datestr = Regex.Split(getDateTimeFromDmtfDate(b21), " ")[0];
                                                                                        DateTime result = DateTime.ParseExact(datestr, formatsrc, CultureInfo.InvariantCulture);
                                                                                        b2 = result.ToString(formatdst) + " " + Regex.Split(getDateTimeFromDmtfDate(b21), " ")[1];
                                              */
                                        } catch { }
                                        try
                                        {
                                            b3 = TransformObjectToDateTime(m["LastBootUpTime"].ToString());

                                            /*                                           string b21 = m["LastBootUpTime"].ToString();
                                                                                       string datestr = Regex.Split(getDateTimeFromDmtfDate(b21), " ")[0];
                                                                                       DateTime result = DateTime.ParseExact(datestr, formatsrc, CultureInfo.InvariantCulture);
                                                                                       b3 = result.ToString(formatdst) + " " + Regex.Split(getDateTimeFromDmtfDate(b21), " ")[1];
                                              */
                                        } catch { }
                                        try { b4 = m["OSArchitecture"].ToString(); } catch { }
                                        try { b5 = m["ProductType"].ToString(); } catch { }
                                        try { c1 = m["SystemDirectory"].ToString(); } catch { }
                                        try { c2 = m["WindowsDirectory"].ToString(); } catch { }
                                        try { c3 = m["OperatingSystemSKU"].ToString(); } catch { }

                                        query = "INSERT INTO 'WindowsFeature' (Name, Value, Value1, ComputerName, Date, Time)"
                                            + " VALUES ('OS: " + a1 + " " + b1 + "','OSArchitecture: " + b4 + "','SN: " + a2 + "','" + _currentHostName + "','" + _Date + "','" + _Time + "'); ";
                                        query += "INSERT INTO 'WindowsFeature' (Name, Value, Value1, ComputerName, Date, Time)"
                                          + " VALUES ('InstallDate: " + b2 + "','LastBootUpTime: " + b3 + "','ProductType: " + b5 + "','" + _currentHostName + "','" + _Date + "','" + _Time + "'); ";
                                        query += "INSERT INTO 'WindowsFeature' (Name, Value, Value1, ComputerName, Date, Time)"
                                          + " VALUES ('SystemDirectory: " + c1 + "','WindowsDirectory: " + c2 + "','OperatingSystemSKU: " + c3 + "','" + _currentHostName + "','" + _Date + "','" + _Time + "'); ";
                                        try { command.ExecuteNonQuery(); } catch { }
                                    }
                                }

                                _StatusLabel2Text("Собираю данные из WMI по USB... ");

                                //USBHub
                                srcd = new ManagementObjectSearcher(ms, new ObjectQuery("SELECT * FROM Win32_USBHub"));
                                queryCollection = srcd.Get();
                                foreach (ManagementObject m in queryCollection)
                                {
                                    if (m["Caption"] != null)
                                    {
                                        a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = ""; a7 = ""; query = "";
                                        try { a1 = m["Caption"].ToString(); } catch { }
                                        try { a3 = m["DeviceID"].ToString(); } catch { }
                                        try { a4 = m["NumberOfPorts"].ToString(); } catch { }
                                        try { a5 = m["Status"].ToString(); } catch { }
                                        try { a6 = m["SystemName"].ToString(); } catch { }

                                        command = new SQLiteCommand("INSERT INTO 'USBHub' ('ComputerName', 'Caption', 'DeviceID', 'NumberOfPorts', 'Status', 'SystemName') " +
                                                    " VALUES (@ComputerName, @Caption, @DeviceID, @NumberOfPorts, @Status, @SystemName)", connection);
                                        command.Parameters.AddWithValue("@ComputerName", HostName);
                                        command.Parameters.AddWithValue("@Caption", a1);
                                        command.Parameters.AddWithValue("@DeviceID", a3);
                                        command.Parameters.AddWithValue("@NumberOfPorts", a4);
                                        command.Parameters.AddWithValue("@Status", a5);
                                        command.Parameters.AddWithValue("@SystemName", a6);
                                        try { command.ExecuteNonQuery(); } catch
                                        {
                                            command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                                                         " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                                            command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                                            command.Parameters.AddWithValue("@Name", "Warning");
                                            command.Parameters.AddWithValue("@Value", "Win32USBHub");
                                            command.Parameters.AddWithValue("@Value1", "Exeption");
                                            command.Parameters.AddWithValue("@Date", _Date);
                                            command.Parameters.AddWithValue("@Time", _Time);
                                            try { command.ExecuteNonQuery(); } catch { }
                                        };
                                    }
                                }

                                //PnPEntity
                                srcd = new ManagementObjectSearcher(ms, new ObjectQuery("SELECT * FROM Win32_PnPEntity"));
                                queryCollection = srcd.Get();
                                foreach (ManagementObject m in queryCollection)
                                {
                                    if (m["Caption"] != null)
                                    {
                                        a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = ""; a7 = ""; query = "";
                                        try { a1 = m["Caption"].ToString(); } catch { }
                                        try { a2 = m["InstallDate"].ToString(); } catch { }
                                        try { a3 = m["DeviceID"].ToString(); } catch { }
                                        try { a4 = m["HardwareID"].ToString(); } catch { }
                                        try { a5 = m["Manufacturer"].ToString(); } catch { }
                                        try { a6 = m["Status"].ToString(); } catch { }
                                        try { a7 = m["SystemName"].ToString(); } catch { }
                                        query = "INSERT INTO 'PnPEntity' ('ComputerName', 'Caption', 'installDate', 'DeviceID', 'HardwareID', 'Manufacturer', 'Status', 'SystemName') " +
                                            " VALUES ('" + HostName + "','" + a1 + "','" + a2 + "','" + a3 + "','" + a4 + "','" + a5 + "','" + a6 + "','" + a7 + "'); ";
                                        command = new SQLiteCommand(query, connection);
                                        try { command.ExecuteNonQuery(); } catch { }
                                    }
                                }

                                _StatusLabel2Text("Собираю данные из WMI по накопителям... ");

                                //DiskDrive
                                srcd = new ManagementObjectSearcher(ms, new ObjectQuery("SELECT * FROM Win32_DiskDrive"));
                                queryCollection = srcd.Get();
                                foreach (ManagementObject m in queryCollection)
                                {
                                    if (m["Caption"] != null)
                                    {
                                        a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = ""; a7 = ""; query = "";
                                        try { a1 = m["Model"].ToString().Trim(); } catch { }
                                        try { a2 = m["SerialNumber"].ToString().Trim(); } catch { }
                                        try { a4 = m["MediaType"].ToString(); } catch { }
                                        try { a5 = m["Status"].ToString(); } catch { }
                                        try { a6 = m["Manufacturer"].ToString(); } catch { }
                                        try { a7 = (Convert.ToDouble(m["Size"]) / 1024 / 1024 / 1024).ToString("#.##") + " GB"; } catch { }
                                        query = "INSERT INTO 'DiskDrive' ('ComputerName', 'Model', 'SerialNumber', 'MediaType', 'Status', 'Manufacturer', 'Size', 'Date', 'Time') " +
                                            " VALUES ('" + HostName + "','" + a1 + "','" + a2 + "','" + a4 + "','" + a5 + "','" + a6 + "','" + a7 + "','" + _Date + "','" + _Time + "'); ";
                                        command = new SQLiteCommand(query, connection);
                                        try { command.ExecuteNonQuery(); } catch
                                        {
                                            command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                                     " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                                            command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                                            command.Parameters.AddWithValue("@Name", "Warning");
                                            command.Parameters.AddWithValue("@Value", "Win32_DiskDrive");
                                            command.Parameters.AddWithValue("@Value1", "Exeption");
                                            command.Parameters.AddWithValue("@Date", _Date);
                                            command.Parameters.AddWithValue("@Time", _Time);
                                            try { command.ExecuteNonQuery(); } catch { }
                                        }
                                    }
                                }

                                //Volume
                                srcd = new ManagementObjectSearcher(ms, new ObjectQuery("SELECT * FROM Win32_Volume"));
                                queryCollection = srcd.Get();
                                foreach (ManagementObject m in queryCollection)
                                {
                                    if (m["Caption"] != null)
                                    {
                                        a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = ""; a7 = ""; a8 = ""; query = "";
                                        try { a1 = m["Caption"].ToString().Trim(); } catch { }
                                        try { a2 = m["DriveLetter"].ToString(); } catch { }
                                        try { a3 = m["FileSystem"].ToString(); } catch { }
                                        try { a4 = m["DriveType"].ToString(); } catch { }
                                        try { a5 = (Convert.ToDouble(m["Size"]) / 1024 / 1024 / 1024).ToString("#.##") + " GB"; } catch { }
                                        try { a6 = (Convert.ToDouble(m["Capacity"]) / 1024 / 1024 / 1024).ToString("#.##") + " GB"; } catch { }
                                        try { a7 = (Convert.ToDouble(m["FreeSpace"]) / 1024 / 1024 / 1024).ToString("#.##") + " GB"; } catch { }
                                        try { a8 = (Convert.ToDouble(m["SizeRemaining"]) / 1024 / 1024 / 1024).ToString("#.##") + " GB"; } catch { }
                                        query = "INSERT INTO 'Volume' ('ComputerName', 'Caption', 'DriveLetter', 'FileSystem', 'DriveType', 'Size', 'Capacity', 'FreeSpace', 'SizeRemaining', 'Date', 'Time') " +
                                            " VALUES ('" + HostName + "','" + a1 + "','" + a2 + "','" + a3 + "','" + a4 + "','" + a5 + "','" + a6 + "','" + a7 + "','" + a8 + "','" + _Date + "','" + _Time + "'); ";
                                        command = new SQLiteCommand(query, connection);
                                        try { command.ExecuteNonQuery(); } catch
                                        {
                                            command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                                     " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                                            command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                                            command.Parameters.AddWithValue("@Name", "Warning");
                                            command.Parameters.AddWithValue("@Value", "Win32_Volume");
                                            command.Parameters.AddWithValue("@Value1", "Exeption");
                                            command.Parameters.AddWithValue("@Date", _Date);
                                            command.Parameters.AddWithValue("@Time", _Time);
                                            try { command.ExecuteNonQuery(); } catch { }
                                        }
                                    }
                                }

                                _StatusLabel2Text("Собираю данные из WMI по другим устройствам... ");

                                //PhysicalMemory
                                srcd = new ManagementObjectSearcher(ms, new ObjectQuery("SELECT * FROM Win32_PhysicalMemory"));
                                queryCollection = srcd.Get();
                                foreach (ManagementObject m in queryCollection)
                                {
                                    if (m["Caption"] != null)
                                    {
                                        a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = ""; a7 = ""; a8 = ""; a9 = ""; a10 = ""; a11 = ""; a12 = ""; query = "";
                                        try { a1 = m["Caption"].ToString(); } catch { }
                                        try { a2 = m["banklabel"].ToString(); } catch { }
                                        try { a3 = m["DeviceLocator"].ToString(); } catch { }
                                        try { a4 = m["SerialNumber"].ToString(); } catch { }
                                        try { a5 = m["MemoryType"].ToString(); } catch { }
                                        try { a6 = m["Model"].ToString(); } catch { }
                                        try { a7 = (Convert.ToDouble(m["Speed"])).ToString("#.#") + " MHz"; } catch { }
                                        try { a8 = m["Manufacturer"].ToString(); } catch { }
                                        try { a9 = m["FormFactor"].ToString(); } catch { }
                                        try { a10 = m["TotalWidth"].ToString(); } catch { }
                                        try { a11 = m["InterleavePosition"].ToString(); } catch { }
                                        try { a12 = (Convert.ToDouble(m["Capacity"]) / 1024 / 1024 / 1024).ToString("#.#") + " GB"; } catch { }
                                        query = "INSERT INTO 'PhysicalMemory' ('ComputerName', 'Caption', 'banklabel', 'DeviceLocator', 'SerialNumber', 'MemoryType', 'Model', 'Speed', 'Manufacturer', 'FormFactor', 'TotalWidth', 'InterleavePosition', 'Capacity') " +
                                            " VALUES ('" + HostName + "','" + a1 + "','" + a2 + "','" + a3 + "','" + a4 + "','" + a5 + "','" + a6 + "','" + a7 + "','" + a8 + "','" + a9 + "','" + a10 + "','" + a11 + "','" + a12 + "'); ";
                                        command = new SQLiteCommand(query, connection);
                                        try { command.ExecuteNonQuery(); } catch
                                        {
                                            command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                                     " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                                            command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                                            command.Parameters.AddWithValue("@Name", "Warning");
                                            command.Parameters.AddWithValue("@Value", "Win32_PhysicalMemory");
                                            command.Parameters.AddWithValue("@Value1", "Exeption");
                                            command.Parameters.AddWithValue("@Date", _Date);
                                            command.Parameters.AddWithValue("@Time", _Time);
                                            try { command.ExecuteNonQuery(); } catch { }
                                        }
                                    }
                                }

                                //Video
                                srcd = new ManagementObjectSearcher(ms, new ObjectQuery("SELECT * FROM Win32_VideoController"));
                                queryCollection = srcd.Get();
                                foreach (ManagementObject m in queryCollection)
                                {
                                    if (m["Caption"] != null)
                                    {
                                        a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = ""; a7 = ""; a8 = ""; a9 = ""; a10 = ""; a11 = ""; a12 = ""; query = "";
                                        try { a1 = m["Caption"].ToString(); } catch { }
                                        try { a2 = m["Description"].ToString(); } catch { }
                                        try { a3 = m["VideoProcessor"].ToString(); } catch { }
                                        try { a4 = (Convert.ToDouble(m["AdapterRAM"]) / 1024 / 1024 / 1024).ToString("0.#") + " GB"; } catch { }
                                        query = "INSERT INTO 'WindowsFeature' ('Name', 'Value', 'Value1' ,'ComputerName', 'Date', 'Time') " +
                                            " VALUES ('Video: " + a1 + "','" + a2 + "/" + a3 + "','RAM: " + a4 + "','" + _currentHostName + "','" + _Date + "','" + _Time + "'); ";
                                        command = new SQLiteCommand(query, connection);
                                        try { command.ExecuteNonQuery(); } catch { }
                                    }
                                }

                                //CPU
                                srcd = new ManagementObjectSearcher(ms, new ObjectQuery("SELECT * FROM win32_processor"));
                                queryCollection = srcd.Get();
                                foreach (ManagementObject m in queryCollection)
                                {
                                    if (m["Name"] != null)
                                    {
                                        a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = ""; a7 = ""; a8 = ""; a9 = ""; a10 = ""; a11 = ""; a12 = ""; query = "";
                                        try { a1 = m["Name"].ToString(); } catch { }
                                        try { a2 = m["processorID"].ToString(); } catch { }
                                        try { a3 = m["DeviceID"].ToString(); } catch { }
                                        try { a4 = m["CurrentClockSpeed"].ToString(); } catch { }
                                        query = "INSERT INTO 'WindowsFeature' ('Name', 'Value', 'Value1' ,'ComputerName', 'Date', 'Time') " +
                                            " VALUES ('" + a3 + ": " + a1 + "','Arch: " + GetProcessorArchitecture() + "/" + a4 + "MHz','CPUiD: " + a2 + "','" + _currentHostName + "','" + _Date + "','" + _Time + "'); ";
                                        command = new SQLiteCommand(query, connection);
                                        try { command.ExecuteNonQuery(); } catch { }
                                    }
                                }
                            }

                            command = new SQLiteCommand("end", connection);
                            command.ExecuteNonQuery();
                        } catch { }

                        command.Dispose();
                        connection.Close();
                    }
                } catch (Exception e) { MessageBox.Show(e.ToString()); }
            }
        }

        private void _ReadUsbfromLocalRegistry()         // Using of the Function - Search Date modifying of the hive of registry.
        {
            _StatusLabel2Text("Определяю конфигурацию USB хоста... ");

            if (_StopScanLogEvents == false)
            {
                FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
                databaseHost = fi.FullName;

                //SQLite connection
                SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};page_size = 52428; cache_size =100000;journal_mode =MEMORY;", databaseHost));
                connection.Open();
                SQLiteCommand command = new SQLiteCommand(); ;
                string a1 = "", a2 = "", a3 = "", a4 = "", a5 = "", a6 = "", a7 = "", query = ""; int aa1 = 0;

                Task t;
                for (int i = 0; i < _UsbKey.Length; i++)
                { _UsbKey[i] = "0"; }

                string _USBKeyString = "SYSTEM\\CurrentControlSet\\Enum\\USBSTOR";
                using (var usbBaseKey = Registry.LocalMachine.OpenSubKey(_USBKeyString, false))
                {
                    string[] subNames = usbBaseKey.GetSubKeyNames();
                    foreach (string s in subNames)                                                         //Get key of USB
                    {
                        if (s != null)
                        {
                            string _USBKeyString1 = _USBKeyString + "\\" + s;
                            using (var usbBaseKey1 = Registry.LocalMachine.OpenSubKey(_USBKeyString1, false))
                            {
                                foreach (string s1 in usbBaseKey1.GetSubKeyNames())                         //Get s/n of USB
                                {
                                    if (s1 != null)
                                    {
                                        string _USBKeyString2 = _USBKeyString1 + "\\" + s1;
                                        using (var usbBaseKey2 = Registry.LocalMachine.OpenSubKey(_USBKeyString2, false))
                                        {
                                            foreach (string valueName in usbBaseKey2.GetValueNames())     //Get Name of USB
                                            {
                                                string s2 = usbBaseKey2.GetValue(valueName).ToString();
                                                if (valueName.Contains("FriendlyName") || valueName.Contains("ParentIdPrefix"))
                                                {
                                                    for (int i = 0; i < _UsbKey.Length; i++)
                                                    {
                                                        try
                                                        {
                                                            if (_UsbKey[i].Length < 2)
                                                            {
                                                                string usbBaseKey3 = "SYSTEM\\CurrentControlSet\\Enum\\USBSTOR\\" + s;
                                                                using (var _USBKeyString4 = Registry.LocalMachine.OpenSubKey(usbBaseKey3, false)) //Get last Time using of USB
                                                                { _UsbLastTime[i] = GetRegKeyModifiedTime(_USBKeyString4).ToString(); }
                                                                _UsbKey[i] = s.ToString();
                                                                _UsbName[i] = s2.ToString();
                                                                _UsbSN[i] = s1.ToString();
                                                                i = _UsbKey.Length;
                                                            }
                                                        } catch { }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }

                _USBKeyString = "SYSTEM\\ControlSet001\\Enum\\USBSTOR";
                using (var usbBaseKey = Registry.LocalMachine.OpenSubKey(_USBKeyString, false))
                {
                    string[] subNames = usbBaseKey.GetSubKeyNames();
                    foreach (string s in subNames)                                                         //Get key of USB
                    {
                        if (s != null)
                        {
                            string _USBKeyString1 = _USBKeyString + "\\" + s;
                            using (var usbBaseKey1 = Registry.LocalMachine.OpenSubKey(_USBKeyString1, false))
                            {
                                foreach (string s1 in usbBaseKey1.GetSubKeyNames())                         //Get s/n of USB
                                {
                                    if (s1 != null)
                                    {
                                        string _USBKeyString2 = _USBKeyString1 + "\\" + s1;
                                        using (var usbBaseKey2 = Registry.LocalMachine.OpenSubKey(_USBKeyString2, false))
                                        {
                                            foreach (string valueName in usbBaseKey2.GetValueNames())     //Get Name of USB
                                            {
                                                string s2 = usbBaseKey2.GetValue(valueName).ToString();
                                                if (valueName.Contains("FriendlyName") || valueName.Contains("ParentIdPrefix"))
                                                {
                                                    for (int i = 0; i < _UsbKey.Length; i++)
                                                    {
                                                        try
                                                        {
                                                            if (_UsbKey[i] == s.ToString() && _UsbKey[i].Length > 2)
                                                            {
                                                                i = _UsbKey.Length;
                                                            }

                                                            if (_UsbKey[i].Length < 2)
                                                            {
                                                                string usbBaseKey3 = "SYSTEM\\ControlSet001\\Enum\\USBSTOR\\" + s;
                                                                using (var _USBKeyString4 = Registry.LocalMachine.OpenSubKey(usbBaseKey3, false)) //Get last Time using of USB
                                                                { _UsbLastTime[i] = GetRegKeyModifiedTime(_USBKeyString4).ToString(); }
                                                                _UsbKey[i] = s.ToString();
                                                                _UsbName[i] = s2.ToString();
                                                                _UsbSN[i] = s1.ToString();
                                                                i = _UsbKey.Length;
                                                            }
                                                        } catch { }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < _UsbKey.Length; i++)
                {
                    t = Task.Run(() =>
                    getInstallTime(i)
                    );
                    t.Wait();
                }
                query = "";
                for (int i = 0; i < _UsbKey.Length; i++)
                {
                    a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = ""; a7 = ""; query = "";
                    //                try
                    //                {
                    if (_UsbKey[i].Length > 1)
                    {
                        _ProgressWork1();
                        a1 = _UsbName[i];
                        a2 = _UsbKey[i];
                        a3 = _UsbSN[i].Remove(_UsbSN[i].Length - 2); //remove last 2 char

                        a4 = _UsbInstallDate[i];
                        try
                        {
                            if (a4.Length > 1 && a4.Contains('/'))
                            { a4 = _UsbInstallDate[i].Replace('/', '-'); }
                        } catch { }
                        a5 = _UsbInstallTime[i]; //remove last 4 char
                        try
                        {
                            if (a5.Length > 4)
                            { a5 = _UsbInstallTime[i].Remove(_UsbInstallTime[i].Length - 4); }; //remove last 4 char
                        } catch { }
                        string[] ax = Regex.Split(_UsbLastTime[i], " ");
                        string at = ax[0].Trim();
                        a6 = at.Substring(6) + "-" + at.Substring(3, 2) + "-" + at.Substring(0, 2);
                        a7 = ax[1].Trim();

                        aa1++;
                        query += "INSERT INTO 'UsbUsed' ('USBId', 'ComputerName', 'UsbKey', 'UsbSN', 'UsbName', 'InstallDate', 'InstallTime', 'LastDate', 'LastTime') " +
                        " VALUES ('" + aa1 + "','" + _myHostName + "','" + a1 + "','" + a3 + "','" + a2 + "','" + a4 + "','" + a5 + "','" + a6 + "','" + a7 + "'); ";
                    }
                    command = new SQLiteCommand(query, connection);
                    command.ExecuteNonQuery();
                }

                //Get mounted USB to Letters of local disk 
                string _USBKeyString3 = "SYSTEM\\MountedDevices";
                aa1 = 0; query = "";
                using (var usbBaseKey3 = Registry.LocalMachine.OpenSubKey(_USBKeyString3, false))
                {
                    foreach (string valueName in usbBaseKey3.GetValueNames())     //Get mounted USB to Letters of local disk  
                    {
                        if (valueName != null)
                        {
                            a1 = ""; a2 = ""; query = "";
                            byte[] s2 = (byte[])usbBaseKey3.GetValue(valueName);
                            var value = Encoding.Unicode.GetString(s2).ToString();
                            a1 = valueName.ToString();
                            if (a1.Contains("Volume"))
                            {
                                string ax = a1.Remove(0, 3); //remove first 3 char
                                a1 = ax;
                            }
                            a2 = value.ToString();
                            if (a2.Contains("USBSTOR"))
                            {
                                string ax = a2.Remove(0, 12); //remove first 12 char
                                a2 = ax;
                            }
                            aa1++;
                            command = new SQLiteCommand("INSERT INTO 'UsbMounted' ('USBMId', 'ComputerName', 'ValueName', 'Value', 'Date', 'Time') " +
                                        " VALUES (@USBMId, @ComputerName, @ValueName, @Value, @Date, @Time)", connection);
                            command.Parameters.AddWithValue("@USBMId", aa1);
                            command.Parameters.AddWithValue("@ComputerName", _myHostName);
                            command.Parameters.AddWithValue("@ValueName", a1);
                            command.Parameters.AddWithValue("@Value", a2);
                            command.Parameters.AddWithValue("@Date", _Date);
                            command.Parameters.AddWithValue("@Time", _Time);
                            try { command.ExecuteNonQuery(); } catch { };
                        }
                    }
                }
            }
        }

        private void _LocalConfigFromWMI() //Read WMI the local host
        {
            _StatusLabel2Text("Считываю конфигурацию железа ... ");

            if (_StopScanLogEvents == false)
            {
                FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
                databaseHost = fi.FullName;

                //Task ts;
                // Проверить герерация данных
                // var s = new ManagementObjectSearcher(@"SELECT * FROM Win32_processor");
                // var o = s.Get().OfType<ManagementObject>().First();
                // o["CurrentClockSpeed"]

                SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}; page_size = 65536; cache_size =100000; journal_mode =MEMORY;", databaseHost));    //SQLite connection
                connection.Open(); SQLiteCommand command = new SQLiteCommand();
                string a1 = "", a2 = "", a3 = "", a4 = "", a5 = "", a6 = "", a7 = "", a8 = "", a9 = "", a10 = "", a11 = "", a12 = "", query = ""; int aa1 = 0;

                command = new SQLiteCommand("begin", connection);
                command.ExecuteNonQuery();

                query = " INSERT INTO 'WindowsFeature' ('Name', 'Value' ,'ComputerName', 'Date', 'Time') VALUES ('" + "Date gathering" + "','" + _Date + "','" + _myHostName + "','" + _Date + "','" + _Time + "'); ";
                command = new SQLiteCommand(query, connection);
                command.ExecuteNonQuery();

                aa1 = 0;
                _StatusLabel2Text("Считываю конфигурацию USB... ");

                //USB
                ManagementObjectSearcher mbs = new ManagementObjectSearcher("Select * From Win32_USBHub"); //https://msdn.microsoft.com/en-us/library/aa394506(v=vs.85).aspx
                ManagementObjectCollection mbsList = mbs.Get();
                foreach (ManagementObject m in mbsList)
                {
                    if (m["Caption"] != null)
                    {
                        a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = ""; query = "";
                        try { a1 = m["Caption"].ToString(); } catch { }
                        try { a3 = m["DeviceID"].ToString(); } catch { }
                        try { a4 = m["NumberOfPorts"].ToString(); } catch { }
                        try { a5 = m["Status"].ToString(); } catch { }
                        try { a6 = m["SystemName"].ToString(); } catch { }
                        aa1++;
                        query += "INSERT INTO 'USBHub' ('USBHubId','ComputerName', 'Caption', 'DeviceID', 'NumberOfPorts', 'Status', 'SystemName') " +
                            " VALUES ('" + aa1 + "','" + _myHostName + "','" + a1 + "','" + a3 + "','" + a4 + "','" + a5 + "','" + a6 + "'); ";
                        command = new SQLiteCommand(query, connection);
                        command.ExecuteNonQuery();
                    }
                }

                _StatusLabel2Text("Считываю конфигурацию PnP... ");

                //USB
                mbs = new ManagementObjectSearcher("Select * From Win32_PnPEntity");// @"SELECT * FROM Win32_PnPEntity where DeviceID Like ""USB%""")
                mbsList = mbs.Get();
                aa1 = 0; query = "";
                foreach (ManagementObject m in mbsList)
                {
                    if (m["Caption"] != null)
                    {
                        a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = ""; a7 = ""; query = "";
                        try { a1 = m["Caption"].ToString(); } catch { }
                        //                    try { a2 = m["InstallDate"].ToString(); } catch { }
                        try { a3 = m["DeviceID"].ToString(); } catch { }
                        try { a4 = m["HardwareID"].ToString(); } catch { }
                        try { a5 = m["Manufacturer"].ToString(); } catch { }
                        try { a6 = m["Status"].ToString(); } catch { }
                        try { a7 = m["SystemName"].ToString(); } catch { }
                        aa1++;
                        query += "INSERT INTO 'PnPEntity' ('PnPId','ComputerName', 'Caption', 'DeviceID', 'HardwareID', 'Manufacturer', 'Status', 'SystemName') " +
                            " VALUES ('" + aa1 + "','" + _myHostName + "','" + a1 + "','" + a3 + "','" + a4 + "','" + a5 + "','" + a6 + "','" + a7 + "'); ";
                        command = new SQLiteCommand(query, connection);
                        command.ExecuteNonQuery();
                    }
                }

                _StatusLabel2Text("Считываю конфигурацию дисков... ");

                //Disk Drives
                mbs = new ManagementObjectSearcher("Select * From Win32_DiskDrive");
                mbsList = mbs.Get();
                aa1 = 0; query = "";
                foreach (ManagementObject m in mbsList)
                {
                    if (m["Caption"] != null)
                    {
                        a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = ""; a7 = ""; query = "";
                        try { a1 = m["Model"].ToString().Trim(); } catch { }
                        try { a2 = m["SerialNumber"].ToString().Trim(); } catch { }
                        try { a4 = m["MediaType"].ToString(); } catch { }
                        try { a5 = m["Status"].ToString(); } catch { }
                        try { a6 = m["Manufacturer"].ToString(); } catch { }
                        try { a7 = (Convert.ToDouble(m["Size"]) / 1024 / 1024 / 1024).ToString("0.##") + " GB"; } catch { }
                        aa1++;
                        query += "INSERT INTO 'DiskDrive' ('DDiD','ComputerName', 'Model', 'SerialNumber', 'MediaType', 'Status', 'Manufacturer', 'Size', 'Date', 'Time') " +
                            " VALUES ('" + aa1 + "','" + _myHostName + "','" + a1 + "','" + a2 + "','" + a4 + "','" + a5 + "','" + a6 + "','" + a7 + "','" + _Date + "','" + _Time + "'); ";
                        command = new SQLiteCommand(query, connection);
                        command.ExecuteNonQuery();
                    }
                }

                //Volumes
                mbs = new ManagementObjectSearcher("SELECT * FROM Win32_Volume");
                mbsList = mbs.Get();
                aa1 = 0; query = "";
                foreach (ManagementObject m in mbsList)
                {
                    if (m["Caption"] != null)
                    {
                        a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = ""; a7 = ""; a8 = ""; query = "";
                        try { a1 = m["Caption"].ToString().Trim(); } catch { }
                        try { a2 = m["DriveLetter"].ToString(); } catch { }
                        try { a3 = m["FileSystem"].ToString(); } catch { }
                        try { a4 = m["DriveType"].ToString(); } catch { }
                        try { a5 = (Convert.ToDouble(m["Size"]) / 1024 / 1024 / 1024).ToString("0.#") + " GB"; } catch { }
                        try { a6 = (Convert.ToDouble(m["Capacity"]) / 1024 / 1024 / 1024).ToString("0.#") + " GB"; } catch { }
                        try { a7 = (Convert.ToDouble(m["FreeSpace"]) / 1024 / 1024 / 1024).ToString("0.#") + " GB"; } catch { }
                        try { a8 = (Convert.ToDouble(m["SizeRemaining"]) / 1024 / 1024 / 1024).ToString("0.#") + " GB"; } catch { }
                        aa1++;
                        query += "INSERT INTO 'Volume' ('VolumeiD','ComputerName', 'Caption', 'DriveLetter', 'FileSystem', 'DriveType', 'Size', 'Capacity', 'FreeSpace', 'SizeRemaining') " +
                            " VALUES ('" + aa1 + "','" + _myHostName + "','" + a1 + "','" + a2 + "','" + a3 + "','" + a4 + "','" + a5 + "','" + a6 + "','" + a7 + "','" + a8 + "'); ";
                        command = new SQLiteCommand(query, connection);
                        command.ExecuteNonQuery();
                    }
                }

                _StatusLabel2Text("Считываю конфигурацию остального железа... ");

                //RAM
                mbs = new ManagementObjectSearcher("Select * From Win32_PhysicalMemory");
                mbsList = mbs.Get();
                aa1 = 0; query = "";
                foreach (ManagementObject m in mbsList)
                {
                    if (m["Caption"] != null)
                    {
                        a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = ""; a7 = ""; a8 = ""; a9 = ""; a10 = ""; a11 = ""; a12 = ""; query = "";
                        try { a1 = m["Caption"].ToString(); } catch { }
                        try { a2 = m["banklabel"].ToString(); } catch { }
                        try { a3 = m["DeviceLocator"].ToString(); } catch { }
                        try { a4 = m["SerialNumber"].ToString(); } catch { }
                        try { a5 = m["MemoryType"].ToString(); } catch { }
                        try { a6 = m["Model"].ToString(); } catch { }
                        try { a7 = (Convert.ToDouble(m["Speed"])).ToString("#.#") + " MHz"; } catch { }
                        try { a8 = m["Manufacturer"].ToString(); } catch { }
                        try { a9 = m["FormFactor"].ToString(); } catch { }
                        try { a10 = m["TotalWidth"].ToString(); } catch { }
                        try { a11 = m["InterleavePosition"].ToString(); } catch { }
                        try { a12 = (Convert.ToDouble(m["Capacity"]) / 1024 / 1024 / 1024).ToString("0.#") + " GB"; } catch { }
                        aa1++;
                        query += "INSERT INTO 'PhysicalMemory' ('PhysicalMemoryId','ComputerName', 'Caption', 'banklabel', 'DeviceLocator', 'SerialNumber', 'MemoryType', 'Model', 'Speed', 'Manufacturer', 'FormFactor', 'TotalWidth', 'InterleavePosition', 'Capacity') " +
                            " VALUES ('" + aa1 + "','" + _myHostName + "','" + a1 + "','" + a2 + "','" + a3 + "','" + a4 + "','" + a5 + "','" + a6 + "','" + a7 + "','" + a8 + "','" + a9 + "','" + a10 + "','" + a11 + "','" + a12 + "'); ";
                        command = new SQLiteCommand(query, connection);
                        command.ExecuteNonQuery();
                    }
                }

                //Video
                mbs = new ManagementObjectSearcher("Select * From Win32_VideoController");
                mbsList = mbs.Get();
                foreach (ManagementObject m in mbsList)
                {
                    if (m["Caption"] != null)
                    {
                        a1 = ""; a2 = ""; a3 = ""; a4 = ""; query = "";
                        try { a1 = m["Caption"].ToString(); } catch { }
                        try { a2 = m["Description"].ToString(); } catch { }
                        try { a3 = m["VideoProcessor"].ToString(); } catch { }
                        try { a4 = (Convert.ToDouble(m["AdapterRAM"]) / 1024 / 1024 / 1024).ToString("0.#") + " GB"; } catch { }
                        query = "INSERT INTO 'WindowsFeature' ('Name', 'Value', 'Value1' ,'ComputerName', 'Date', 'Time') " +
                            " VALUES ('Video: " + a1 + "','" + a2 + "/" + a3 + "','RAM: " + a4 + "','" + _myHostName + "','" + _Date + "','" + _Time + "'); ";
                        command = new SQLiteCommand(query, connection);
                        command.ExecuteNonQuery();
                    }
                }

                //CPU
                mbs = new ManagementObjectSearcher("Select * From win32_processor");
                mbsList = mbs.Get();
                foreach (ManagementObject m in mbsList)
                {
                    if (m["Name"] != null)
                    {
                        a1 = ""; a2 = ""; a3 = ""; a4 = ""; query = "";
                        try { a1 = m["Name"].ToString(); } catch { }            // CPU iD
                        try { a2 = m["processorID"].ToString(); } catch { }
                        try { a3 = m["DeviceID"].ToString(); } catch { }
                        try { a4 = m["CurrentClockSpeed"].ToString(); } catch { }
                        query = "INSERT INTO 'WindowsFeature' ('Name', 'Value', 'Value1' ,'ComputerName', 'Date', 'Time') " +
                            " VALUES ('" + a3 + ": " + a1 + "','Arch: " + GetProcessorArchitecture() + "/" + a4 + "MHz','CPUiD: " + a2 + "','" + _myHostName + "','" + _Date + "','" + _Time + "'); ";
                        command = new SQLiteCommand(query, connection);
                        command.ExecuteNonQuery();
                    }
                }

                //MotherBoard
                mbs = new ManagementObjectSearcher("SELECT Manufacturer,Product,SerialNumber FROM Win32_BaseBoard");
                mbsList = mbs.Get();
                foreach (ManagementObject m in mbsList)
                {
                    a1 = ""; a2 = ""; a3 = "";
                    try { a1 = (string)m["Manufacturer"]; } catch { }
                    try { a2 = (string)m["Product"]; } catch { }
                    try { a3 = (string)m["SerialNumber"]; } catch { }
                    query = " INSERT INTO 'WindowsFeature' ('Name', 'Value', 'Value1','ComputerName', 'Date', 'Time') "
                        + " VALUES ('MB: " + a1 + "','Model: " + a2 + "','SN: " + a3 + "','" + _myHostName + "','" + _Date + "','" + _Time + "');";
                }

                //BIOS
                mbs = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS"); mbsList = mbs.Get();
                foreach (ManagementObject mo in mbsList)
                { try { _TempSN = mo["SerialNumber"].ToString(); } catch { } }

                mbs = new ManagementObjectSearcher("SELECT Name, UserName,Model,Manufacturer,TotalPhysicalMemory,Domain  FROM Win32_ComputerSystem"); mbsList = mbs.Get();
                foreach (ManagementObject mo in mbsList)
                {
                    if (mo != null)
                    {
                        try { _TempLogonUser = mo["UserName"].ToString(); } catch { } //Name of the logon User
                        try { _TempModel = mo["Model"].ToString(); } catch { } //model PC
                        try { a4 = mo["Domain"].ToString(); } catch { } //PC Domain's
                        try { a3 = mo["PartOfDomain"].ToString(); } catch { } //PC in Domain
                        try { a5 = mo["Manufacturer"].ToString(); } catch { } //Manufacturer PC
                        try { a6 = (Convert.ToDouble(mo["TotalPhysicalMemory"]) / 1024 / 1024 / 1024).ToString("0.#") + " GB"; } catch { } //RAM

                        query = " INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time')" +
                                 " VALUES ('" + _myHostName + "','UserNameOnline: " + "','" + _TempLogonUser + "','" + "Current logon User" + "','" + _Date + "','" + _Time + "'); ";

                        query += " INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time')" +
                                 " VALUES ('" + _myHostName + "','Domain: " + "','" + a4 + "','" + a3 + "','" + _Date + "','" + _Time + "'); ";

                        query += " INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time')" +
                                 " VALUES ('" + _myHostName + "','PC: " + _TempModel + "','" + a5 + "','SN: " + _TempSN + "" + "','" + _Date + "','" + _Time + "'); ";

                        query += " INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time')" +
                                 " VALUES ('" + _myHostName + "','RAM: " + "','" + a6 + "','" + "" + "','" + _Date + "','" + _Time + "'); ";
                        command = new SQLiteCommand(query, connection);
                        try { command.ExecuteNonQuery(); } catch { }
                    }
                }

                //OS
                mbs = new ManagementObjectSearcher("SELECT *  FROM Win32_OperatingSystem"); mbsList = mbs.Get();
                foreach (ManagementObject m in mbsList)
                {
                    if (m["Name"] != null)
                    {
                        string b1 = "", b2 = "", b3 = "", b4 = "", b5 = "", c1 = "", c2 = "", c3 = "";
                        try { a1 = m["Caption"].ToString(); } catch { }
                        try { b1 = m["CSDVersion"].ToString(); } catch { }
                        try { a2 = m["SerialNumber"].ToString(); } catch { }
                        try
                        {
                            b2 = TransformObjectToDateTime(m["InstallDate"].ToString());

                            /* string b21 = m["InstallDate"].ToString();
                                                        string datestr = Regex.Split(getDateTimeFromDmtfDate(b21), " ")[0];
                                                        DateTime result = DateTime.ParseExact(datestr, formatsrc, CultureInfo.InvariantCulture);
                                                        b2 = result.ToString(formatdst) + " " + Regex.Split(getDateTimeFromDmtfDate(b21), " ")[1];
                              */
                        } catch { }
                        try
                        {
                            b3 = TransformObjectToDateTime(m["LastBootUpTime"].ToString());

                            /*string b21 = m["LastBootUpTime"].ToString();
                                                        string datestr = Regex.Split(getDateTimeFromDmtfDate(b21), " ")[0];
                                                        DateTime result = DateTime.ParseExact(datestr, formatsrc, CultureInfo.InvariantCulture);
                                                        b3 = result.ToString(formatdst) + " " + Regex.Split(getDateTimeFromDmtfDate(b21), " ")[1];
                              */
                        } catch { }
                        try { b4 = m["OSArchitecture"].ToString(); } catch { }
                        try { b5 = m["ProductType"].ToString(); } catch { }
                        try { c1 = m["SystemDirectory"].ToString(); } catch { }
                        try { c2 = m["WindowsDirectory"].ToString(); } catch { }
                        try { c3 = m["OperatingSystemSKU"].ToString(); } catch { }

                        query = "INSERT INTO 'WindowsFeature' (Name, Value, Value1, ComputerName, Date, Time)"
                            + " VALUES ('OS: " + a1 + " " + b1 + "','OSArchitecture: " + b4 + "','SN: " + a2 + "','" + _currentHostName + "','" + _Date + "','" + _Time + "'); ";
                        query += "INSERT INTO 'WindowsFeature' (Name, Value, Value1, ComputerName, Date, Time)"
                          + " VALUES ('OS: ', 'Install Time: " + b2 + "','LastBootUpTime: " + b3 + "','" + _currentHostName + "','" + _Date + "','" + _Time + "'); ";
                        query += "INSERT INTO 'WindowsFeature' (Name, Value, Value1, ComputerName, Date, Time)"
                          + " VALUES ('OS: ', 'SystemDirectory: " + c1 + "','WindowsDirectory: " + c2 + "','" + _currentHostName + "','" + _Date + "','" + _Time + "'); ";
                        command = new SQLiteCommand(query, connection);
                        try { command.ExecuteNonQuery(); } catch { }
                    }
                }

                //NET
                mbs = new ManagementObjectSearcher("Select * from Win32_NetworkAdapterConfiguration"); mbsList = mbs.Get();
                aa1 = 0;
                foreach (ManagementObject mo in mbsList)
                {
                    if (mo["Caption"] != null)
                    {
                        a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = "";
                        try
                        {
                            query = "";
                            try
                            {
                                string[] addres1 = Regex.Split(mo["Caption"].ToString(), "[]]");
                                a1 = addres1[1];
                            } catch { a1 = "no"; }
                            try { a2 = mo["MACAddress"].ToString(); } catch { a2 = "no"; }
                            try
                            {
                                string[] addresses1 = (string[])mo["IPSubnet"];
                                a3 = addresses1[0];
                            } catch { a3 = "no"; }
                            try { a4 = mo["DHCPServer"].ToString(); } catch { a4 = "no"; }
                            try
                            {
                                string[] addresses1 = (string[])mo["DefaultIPGateway"];
                                a5 = addresses1[0];
                            } catch { a5 = "no"; }
                            try { a6 = mo["IPEnabled"].ToString(); } catch { a6 = "no"; }
                            query =
                                "INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time')" +
                                " VALUES ('" + _myHostName + "','Net - " + a6 + " :" + a1 + "','MAC: " + a2 + "','Net/DHCP/GW: " + a3 + "/" + a4 + "/" + a5 + "','" + _Date + "','" + _Time + "'); ";
                            command = new SQLiteCommand(query, connection);
                            command.ExecuteNonQuery();
                        } catch
                        {
                            command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                            command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                            command.Parameters.AddWithValue("@Name", "Warning");
                            command.Parameters.AddWithValue("@Value", "Win32_NetworkAdapterConfiguration");
                            command.Parameters.AddWithValue("@Value1", "No Access! " + mo["Caption"].ToString());
                            command.Parameters.AddWithValue("@Date", _Date);
                            command.Parameters.AddWithValue("@Time", _Time);
                            try { command.ExecuteNonQuery(); } catch { }
                        }
                    }
                }
                command = new SQLiteCommand("end", connection);
                command.ExecuteNonQuery();

                a1 = ""; a2 = ""; query = ""; aa1 = 0;

                command = new SQLiteCommand("begin", connection);
                command.ExecuteNonQuery();
                try
                {
                    mbs = new ManagementObjectSearcher("Select * from Win32_Product");
                    mbsList = mbs.Get();
                    foreach (ManagementObject mo in mbsList)
                    {
                        if (mo["Caption"].ToString() != null && mo["InstallDate"].ToString().Length > 7 && _StopScanLogEvents == false)
                        {
                            query = "";
                            a1 = mo["Caption"].ToString();

                            string datestr = mo["InstallDate"].ToString();
                            DateTime result = DateTime.ParseExact(datestr, formatsrc, CultureInfo.InvariantCulture);
                            a2 = result.ToString(formatdst);

                            aa1++;
                            query = "INSERT INTO 'Product' ('ProductId', 'ComputerName', 'Caption', 'InstallDate') " +
                                "VALUES ('" + aa1 + "','" + _myHostName + "','" + a1 + "','" + a2 + "'); ";
                            command = new SQLiteCommand(query, connection);
                            try { command.ExecuteNonQuery(); } catch
                            {
                                command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                                command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                                command.Parameters.AddWithValue("@Name", "Warning");
                                command.Parameters.AddWithValue("@Value", "Win32_Product");
                                command.Parameters.AddWithValue("@Value1", "No Access! " + mo["Caption"].ToString());
                                command.Parameters.AddWithValue("@Date", _Date);
                                command.Parameters.AddWithValue("@Time", _Time);
                                try { command.ExecuteNonQuery(); } catch { }
                            }
                        }
                    }
                } catch
                {
                    command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                    command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                    command.Parameters.AddWithValue("@Name", "Error");
                    command.Parameters.AddWithValue("@Value", "Win32_Product");
                    command.Parameters.AddWithValue("@Value1", "No Access!");
                    command.Parameters.AddWithValue("@Date", _Date);
                    command.Parameters.AddWithValue("@Time", _Time);
                    try { command.ExecuteNonQuery(); } catch { }
                }

                command = new SQLiteCommand("end", connection);
                command.ExecuteNonQuery();
                command.Dispose();
                connection.Close();
            }
        }

        private string GetProcessorArchitecture()         // Using of the Function - Search Date modifying of the hive of registry.
        {
            using (RegistryKey environmentKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", RegistryKeyPermissionCheck.ReadSubTree))
            {
                string strenvironment = environmentKey.GetValue("PROCESSOR_ARCHITECTURE").ToString();
                return strenvironment;
            }
        }

        /*
 Win32_Processor

Architecture
 Caption
 CurrentClockSpeed
 DeviceID
 ExtClock
 Family
 MaxClockSpeed
 NumberOfCores
 NumberOfLogicalProcessors
 SocketDesignation
 UpgradeMethod

         - CPU-
      Family
Other (1)
Unknown (2)
8086 (3)
80286 (4)
80386 (5)
80486 (6)
8087 (7)
80287 (8)
80387 (9)
80487 (10)
Pentium(R) brand (11)    Pentium Brand
Pentium(R) Pro (12)    Pentium Pro
Pentium(R) II (13)
Pentium(R) processor with MMX(TM) technology (14)
Celeron(TM) (15)
Pentium(R) II Xeon(TM) (16)
Pentium(R) III (17)
M1 Family (18)
M2 Family (19)
K5 Family (24)    AMD Duron™ Processor Family
K6 Family (25)
K6-2 (26)
K6-3 (27)
AMD Athlon(TM) Processor Family (28)
AMD(R) Duron(TM) Processor (29)
AMD29000 Family (30)
K6-2+ (31)
Power PC Family (32)
Power PC 601 (33)
Power PC 603 (34)
Power PC 603+ (35)
Power PC 604 (36)
Power PC 620 (37)
Power PC X704 (38)
Power PC 750 (39)
Alpha Family (48)
Alpha 21064 (49)
Alpha 21066 (50)
Alpha 21164 (51)
Alpha 21164PC (52)
Alpha 21164a (53)
Alpha 21264 (54)
Alpha 21364 (55)
MIPS Family (64)
MIPS R4000 (65)
MIPS R4200 (66)
MIPS R4400 (67)
MIPS R4600 (68)
MIPS R10000 (69)
SPARC Family (80)
SuperSPARC (81)
microSPARC II (82)
microSPARC IIep (83)
UltraSPARC (84)
UltraSPARC II (85)
UltraSPARC IIi (86)
UltraSPARC III (87)
UltraSPARC IIIi (88)
68040 (96)
68xxx Family (97)
68000 (98)
68010 (99)
68020 (100)
68030 (101)
Hobbit Family (112)
Crusoe(TM) TM5000 Family (120)
Crusoe(TM) TM3000 Family (121)
Efficeon(TM) TM8000 Family (122)
Weitek (128)
Itanium(TM) Processor (130)
AMD Athlon(TM) 64 Processor Family (131)
AMD Opteron(TM) Family (132)
PA-RISC Family (144)
PA-RISC 8500 (145)
PA-RISC 8000 (146)
PA-RISC 7300LC (147)
PA-RISC 7200 (148)
PA-RISC 7100LC (149)
PA-RISC 7100 (150)
V30 Family (160)
Pentium(R) III Xeon(TM) (176)
Pentium(R) III Processor with Intel(R) SpeedStep(TM) Technology (177)
Pentium(R) 4 (178)
Intel(R) Xeon(TM) (179)
AS400 Family (180)
Intel(R) Xeon(TM) processor MP (181)
AMD AthlonXP(TM) Family (182)
AMD AthlonMP(TM) Family (183)
Intel(R) Itanium(R) 2 (184)
Intel Pentium M Processor (185)
K7 (190)
Intel Core™ i7-2760QM (198-199)
IBM390 Family (200)
G4 (201)
G5 (202)
G6 (203)
z/Architecture base (204)
i860 (250)
i960 (251)
SH-3 (260)
SH-4 (261)
ARM (280)
StrongARM (281)
6x86 (300)
MediaGX (301)
MII (302)
WinChip (320)
DSP (350)
Video Processor (500)

            UpgradeMethod
Other (1)
Unknown (2)
Daughter Board (3)
ZIF Socket (4)
Replacement/Piggy Back (5)
None (6)
LIF Socket (7)
Slot 1 (8)
Slot 2 (9)
370 Pin Socket (10)
Slot A (11)
Slot M (12)
Socket 423 (13)
Socket A (Socket 462) (14)
Socket 478 (15)
Socket 754 (16)
Socket 940 (17)
Socket 939 (18)
  */


        /*MemoryType
    0	Неизвестный
    1	Другой
    2	DRAM
    3	Synchronous DRAM
    4	Cache DRAM
    5	EDO
    6	EDRAM
    7	VRAM
    8	SRAM
    9	RAM
    10	ROM
    11	Flash
    12	EEPROM
    13	FEPROM
    14	EPROM
    15	CDRAM
    16	3DRAM
    17	SDRAM
    18	SGRAM
    19	RDRAM
    20	DDR
    21	DDR1
    22  DDR2 FB-DIMM
    23  DDR3
    24  FBD2

            FormFactor
(0)    Unknown
(1)    Other
(2)    SIP
(3)    DIP
(4)    ZIP
(5)    SOJ
(6)    Proprietary
(7)    SIMM
(8)    DIMM
(9)    TSOP
(10)    PGA
(11)    RIMM
(12)    SODIMM
(13)    SRIMM
(14)    SMD
(15)    SSMP
(16)    QFP
(17)    TQFP
(18)    SOIC
(19)    LCC
(20)    PLCC
(21)    BGA
(22)    FPBGA
(23)    LGA

             */


        private void getInstallTimeR(string HostName, int i) //Parse setupapi.dev.log and search time of installing of USB
        {
            try
            {
                string[] substrPath = Regex.Split(_SystemPath, ":");
                DirectoryInfo info = new DirectoryInfo("\\\\" + HostName + "\\" + substrPath[0] + "$" + substrPath[1]);
                string mystatus1 = "0", s = null;
                if (File.Exists(info.FullName + @"\INF\setupapi.dev.log"))
                {
                    using (StreamReader Reader = new StreamReader(info.FullName + @"\INF\setupapi.dev.log"))
                    {
                        while ((s = Reader.ReadLine()) != null)
                        {
                            if (s.Contains(_UsbKey[i]) && s.Contains("Install"))
                            { mystatus1 = "1"; }

                            if (s.Contains("Section start") && mystatus1 == "1")
                            {
                                string[] substring = Regex.Split(s.Trim(), "Section start");
                                string[] substring1 = Regex.Split(substring[1].Trim(), " ");
                                _UsbInstallDate[i] = substring1[0].Trim();
                                _UsbInstallTime[i] = substring1[1].Trim();
                                mystatus1 = "0";
                            }
                        }
                    }
                }
            } catch { }
        }

        private void getInstallTime(int i) //Parse local setupapi.dev.log and search time of installing of USB
        {
            try
            {
                DirectoryInfo info = new DirectoryInfo(Environment.GetEnvironmentVariable("windir"));
                string mystatus1 = "0", s = null;
                if (File.Exists(info.FullName + @"\INF\setupapi.dev.log"))
                {
                    using (StreamReader Reader = new StreamReader(info.FullName + @"\INF\setupapi.dev.log"))
                    {
                        while ((s = Reader.ReadLine()) != null)
                        {
                            if (s.Contains(_UsbKey[i]) && s.Contains("Install"))
                            { mystatus1 = "1"; }

                            if (s.Contains("Section start") && mystatus1 == "1")
                            {
                                string[] substring = Regex.Split(s.Trim(), "Section start");
                                string[] substring1 = Regex.Split(substring[1].Trim(), " ");
                                _UsbInstallDate[i] = substring1[0].Trim();
                                _UsbInstallTime[i] = substring1[1].Trim();
                                mystatus1 = "0";
                            }
                        }
                    }
                }
            } catch { }
        }

        private async void scanEnvironmentItem_Click(object sender, EventArgs e) //Searching of the neariest subnetworks
        {
            _StopSearchNets = false;
            StopSearchItem.Enabled = true;
            waitNetPing = new AutoResetEvent(false);
            waitFile = new AutoResetEvent(false);
            waitStop = new AutoResetEvent(false);
            waitNetStop = new AutoResetEvent(false);
            waitFilePing = new AutoResetEvent(false);

            _labelCurrentNet(_myNET + "xxx");
            listBoxNetsRow.Items.Clear();
            _StatusLabel2ForeColor(System.Drawing.Color.Black);
            _StatusLabel2Text("Определяю ближайшие сети с активными хостами...");
            Thread t = new Thread(new ThreadStart(_StatusLabel2ChangeColor));
            t.Start();
            _ProgressBar1Value0();
            timer1Start();
            _GetTimeRunScan();    //Write the timetag of the action
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}; page_size = 65536; cache_size =100000; journal_mode =MEMORY;", Application.StartupPath + @"\" + databaseAllTables)))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand("INSERT INTO 'AllTables' ('NameTable', 'ScannerName', 'ScannerIP', 'Date', 'Time') " +
                            " VALUES (@NameTable, @ScannerName, @ScannerIP, @Date, @Time)", connection);
                command.Parameters.AddWithValue("@NameTable", "AliveNets");
                command.Parameters.AddWithValue("@ScannerName", _myHostName);
                command.Parameters.AddWithValue("@ScannerIP", _myIP);
                command.Parameters.AddWithValue("@Date", _Date);
                command.Parameters.AddWithValue("@Time", _Time);
                try { command.ExecuteNonQuery(); } catch (Exception expt) { MessageBox.Show(expt.ToString()); };
                connection.Close();
                _ProgressWork1();
            }

            await Task.Run(() => _searchAliveNet());
            await Task.Run(() => _NetIntoDB());
        }

        private async void _searchAliveNet() //Search able network
        {
            string MainNet = Regex.Split(_myNET, "[.]")[0] + "." + Regex.Split(_myNET, "[.]")[1] + ".";
            Task[] t = new Task[1];
            for (int j = 0; j < 255; j++)
            {
                if (_StopSearchNets == false)
                {
                    StatusLabel2.Text = "Выясняю наличие хостов в сети: " + MainNet + j + ".xxx";
                    for (int i = 0; i < 255; i += 3)
                    {
                        if (_StopSearchNets == false)
                        {
                            string ItNetAlive = "0";
                            if (CheckExistIP(MainNet + j + "." + 254, 3) && ItNetAlive == "0")
                            {
                                for (int k = 0; k < 50; k++)
                                {
                                    if (_NetAlive[k].Length < 7)
                                    {
                                        _ProgressWork1();
                                        string tmp = MainNet + j + ".xxx";
                                        t[0] = Task.Run((() => _listBoxNetsRow(tmp)));
                                        _NetAlive[k] = MainNet + j + ".";
                                        k = 50;
                                    }
                                }
                                ItNetAlive = "1";
                                i = 255;
                            }

                            if (CheckExistIP(MainNet + j + "." + 250, 3) && ItNetAlive == "0")
                            {
                                for (int k = 0; k < 50; k++)
                                {
                                    if (_NetAlive[k].Length < 7)
                                    {
                                        _ProgressWork1();
                                        string tmp = MainNet + j + ".xxx";
                                        t[0] = Task.Run((() => _listBoxNetsRow(tmp)));
                                        _NetAlive[k] = MainNet + j + ".";
                                        k = 50;
                                    }
                                }
                                ItNetAlive = "1";
                                i = 255;
                            }

                            if (CheckExistIP(MainNet + j + "." + 100, 3) && ItNetAlive == "0")
                            {
                                for (int k = 0; k < 50; k++)
                                {
                                    if (_NetAlive[k].Length < 7)
                                    {
                                        _ProgressWork1();
                                        string tmp = MainNet + j + ".xxx";
                                        t[0] = Task.Run((() => _listBoxNetsRow(tmp)));
                                        _NetAlive[k] = MainNet + j + ".";
                                        k = 50;
                                    }
                                }
                                ItNetAlive = "1";
                                i = 255;
                            }


                            if (CheckExistIP(MainNet + j + "." + 200, 3) && ItNetAlive == "0")
                            {
                                for (int k = 0; k < 50; k++)
                                {
                                    if (_NetAlive[k].Length < 7)
                                    {
                                        _ProgressWork1();
                                        string tmp = MainNet + j + ".xxx";
                                        t[0] = Task.Run((() => _listBoxNetsRow(tmp)));
                                        _NetAlive[k] = MainNet + j + ".";
                                        k = 50;
                                    }
                                }
                                ItNetAlive = "1";
                                i = 255;
                            }

                            if (CheckExistIP(MainNet + j + "." + 1, 3) && ItNetAlive == "0")
                            {
                                for (int k = 0; k < 50; k++)
                                {
                                    if (_NetAlive[k].Length < 7)
                                    {
                                        _ProgressWork1();
                                        string tmp = MainNet + j + ".xxx";
                                        t[0] = Task.Run((() => _listBoxNetsRow(tmp)));
                                        _NetAlive[k] = MainNet + j + ".";
                                        k = 50;
                                    }
                                }
                                ItNetAlive = "1";
                                i = 255;
                            }

                            if (CheckExistIP(MainNet + j + "." + i, 3) && ItNetAlive == "0")
                            {
                                for (int k = 0; k < 50; k++)
                                {
                                    if (_NetAlive[k].Length < 7)
                                    {
                                        _ProgressWork1();
                                        string tmp = MainNet + j + ".xxx";
                                        t[0] = Task.Run((() => _listBoxNetsRow(tmp)));
                                        _NetAlive[k] = MainNet + j + ".";
                                        k = 50;
                                    }
                                }
                                ItNetAlive = "1";
                                i = 255;
                            }
                        }
                    }
                }
            }
            await Task.WhenAll(t);

            if (_StopSearchNets == false)
            {
                _StatusLabel2ForeColor(System.Drawing.Color.Black);
                _StatusLabel2Text("Поиск сетей завершен!");
            }

            else if (_StopSearchNets == true) //Break if press STOP
            {
                _StatusLabel2ForeColor(System.Drawing.Color.Crimson);
                _StatusLabel2Text("Поиск сетей прерван!");
            }
            waitNetPing.Set();
        }

        static bool CheckExistIP(string ip, int WaitingPing) //checking of the host alive
        {
            bool i = false;
            try
            {
                System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions();
                options.DontFragment = true;
                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"; //Buffer - 32 Bytes
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                System.Net.NetworkInformation.PingReply pingReply = ping.Send(ip, WaitingPing, buffer, options);
                IPAddress addres = pingReply.Address;
                if (pingReply.Status == System.Net.NetworkInformation.IPStatus.Success)
                { i = true; }
            } catch { }
            return i;
        }

        private void _NetIntoDB() //Add found able networks into DB
        {
            waitNetPing.WaitOne(); //wait for the End of ping networks
            _StatusLabel2Text("Записываю собранные данные в базу...");

            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}; journal_mode =MEMORY;", Application.StartupPath + @"\" + databaseAllTables)))
            {
                connection.Open();
                foreach (string n in listBoxNetsRow.Items)
                {
                    if (n.Length > 6 && n != null)
                    {
                        using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'AliveNets' ('Net', 'ScannerName', 'ScannerIP', 'Date', 'Time') VALUES (@Net, @ScannerName, @ScannerIP, @Date, @Time)", connection))
                        {
                            command.Parameters.AddWithValue("@Net", n);
                            command.Parameters.AddWithValue("@ScannerName", _myHostName);
                            command.Parameters.AddWithValue("@ScannerIP", _myIP);
                            command.Parameters.AddWithValue("@Date", _Date);
                            command.Parameters.AddWithValue("@Time", _Time);
                            try { command.ExecuteNonQuery(); } catch { };
                        }
                    }
                }
            }

            _RunTimer = false;
            if (_StopSearchNets == true) //Break if press STOP
            {
                _timer2State(false);
                _StatusLabel2ForeColor(System.Drawing.Color.Crimson);
                _StatusLabel2Text("Поиск сетей прерван!");
                _StopScannig = false;
            }
            else
            { _StatusLabel2Text("Поиск сетей завершен..."); }
            timer1Stop();
            _ProgressBar1Value100();
        }

        private string TempConfigSingleHost = "";

        private void _getUserAndSN(string HostName, IPAddress IPHostName) //Get Info from the scanning host now
        {
            if (!HostName.ToLower().Contains("unknow") && !IPHostName.ToString().Contains("127.0.0.1"))
            {
                string _TempSN1 = "";
                string _TempNamePC1 = "";
                string _TempModel1 = "";
                string _TempDomainPC1 = "";
                string _TempLogonUser1 = "";
                var Coder = Encoding.GetEncoding(65001);

                /*          if (textBoxDomainLength > 2 && textBoxPasswordLength > 0 && textBoxLoginLength > 0) //the domain has inputed. User isn't the domain one 
                          {
                              _User = textBoxDomainText + "\\" + textBoxLoginText;
                              _Password = textBoxPasswordText;
                          }
                          else if (textBoxDomainLength < 3 && textBoxPasswordLength > 0 && textBoxLoginLength > 0) //the domain hasn't inputed. Will use No DOMAIN's USER
                          {
                              _User = _currentHostIP + "\\" + textBoxLoginText;
                              _Password = textBoxPasswordText;
                          }
                          else
                          {
                              _User = "";
                              _Password = "";
                          }*/

                NetworkShare share = new NetworkShare(_currentHostName, "ipc$", _User, _Password);
                share.Connect();  // Connect to the remote drive
                try
                {
                    ConnectionOptions co = new ConnectionOptions();   //Connect to WMI of the Remote Host 
                    co.Impersonation = ImpersonationLevel.Impersonate;
                    co.Authentication = AuthenticationLevel.Packet;
                    co.EnablePrivileges = true;
                    co.Timeout = new TimeSpan(0, 0, 10);

                    co.Username = textBoxLoginText;
                    co.Password = _Password;
                    //                        co.Locale = "MS_409";
                    co.Authority = "ntlmdomain:" + textBoxDomainText;
                    ManagementScope ms = new ManagementScope("\\\\" + HostName + "\\root\\cimv2", co);
                    ms.Connect();
                    bool isConnected = ms.IsConnected;
                    if (isConnected)
                    {
                        ManagementObjectSearcher srcd = new ManagementObjectSearcher(ms, new ObjectQuery("SELECT SerialNumber FROM Win32_BIOS"));
                        ManagementObjectCollection queryCollection = srcd.Get();
                        foreach (ManagementObject mo in queryCollection)
                        { try { _TempSN1 = mo["SerialNumber"].ToString(); } catch { } }

                        srcd = new ManagementObjectSearcher(ms, new ObjectQuery("SELECT Name,UserName,Model,Domain  FROM Win32_ComputerSystem"));
                        queryCollection = srcd.Get();
                        foreach (ManagementObject mo in queryCollection)
                        {
                            if (mo["Name"] != null)
                            {
                                _TempNamePC1 = mo["Name"].ToString();       //Name of the PC
                                try { _TempDomainPC1 = mo["Domain"].ToString(); } catch { };   //Domain's Name of the PC
                                try { _TempLogonUser1 = mo["UserName"].ToString(); } catch { };  //Name of the logon User
                                try { _TempModel1 = mo["Model"].ToString(); } catch { };          //model PC
                            }
                        }
                    }
                    _addressesPing.Add(HostName.ToUpper() + " | " + _TempNamePC1.ToUpper() + " | " + IPHostName + " | " + _TempDomainPC1.ToUpper() + " | " + _TempModel1 + " | " + _TempSN1 + " | " + _TempLogonUser1 + " | ");
                    TempConfigSingleHost = HostName.ToUpper() + " | " + _TempNamePC1.ToUpper() + " | " + IPHostName + " | " + _TempDomainPC1.ToUpper() + " | " + _TempModel1 + " | " + _TempSN1 + " | " + _TempLogonUser1 + " | ";
                    share.Disconnect();
                } catch
                {
                    _addressesPing.Add(HostName.ToUpper() + " | " + _TempNamePC1.ToUpper() + " | " + IPHostName + " | No Permission | No Permission | No Permission | No Permission | ");
                    TempConfigSingleHost = HostName.ToUpper() + " | " + _TempNamePC1.ToUpper() + " | " + IPHostName + " | No Permission | No Permission | No Permission | No Permission | ";
                    share.Disconnect();
                }
            }
            else
            {
                _addressesPing.Add(HostName.ToUpper() + " | Unknow in DNS | " + IPHostName + " | No Permission | No Permission | No Permission | No Permission | ");
                TempConfigSingleHost = HostName.ToUpper() + " | Unknow in DNS | " + IPHostName + " | No Permission | No Permission | No Permission | No Permission | ";
            }
        }

        private void StopScan()      //System.Drawing.Color.DarkRed
        {
            _StatusLabel2ForeColor(System.Drawing.Color.Crimson);
            _StatusLabel2Text("Сканирование сетей прервано!");
            _ProgressBar1Value100();
            timer1Stop();
        }

        private void currentTCPConnectionsItem_Click(object sender, EventArgs e) //Current network connections
        {
            textBoxLogs.Clear();
            _myTCPConnectionsStatus();
        }

        public static bool IsServerUp(string server, int port, int timeout) //check alive port. Doesn't work!
        {
            bool isUp;
            try
            {
                using (System.Net.Sockets.TcpClient tcp = new System.Net.Sockets.TcpClient())
                {
                    IAsyncResult ar = tcp.BeginConnect(server, port, null, null);
                    WaitHandle wh = ar.AsyncWaitHandle;

                    try
                    {
                        if (!wh.WaitOne(TimeSpan.FromMilliseconds(timeout), false))
                        {
                            tcp.EndConnect(ar);
                            tcp.Close();
                            throw new System.Net.Sockets.SocketException();
                        }

                        isUp = true;
                        tcp.EndConnect(ar);
                    } finally
                    {
                        wh.Close();
                    }
                }
            } catch (System.Net.Sockets.SocketException e)
            {
                //                LOGGER.Warn(string.Format("TCP connection to server {0} failed.", server), e);
                isUp = false;
            }

            return isUp;
        }

        private bool RunScanNetForce = false;

        private async void scanSelectedNetworkItem_Click(object sender, EventArgs e)
        {
            StopSearchItem.Enabled = true;

            waitNetPing = new AutoResetEvent(false);
            waitFile = new AutoResetEvent(false);
            waitStop = new AutoResetEvent(false);
            waitNetStop = new AutoResetEvent(false);
            waitFilePing = new AutoResetEvent(false);

            _StopSearchAliveHosts = false;
            timer1Start();
            _StatusLabel2Text("Сканирую выбранные сети");
            _ProgressBar1Value0();
            Task.Run(() => _StatusLabel2ChangeColor());

            _StatusLabel2ListChangingText.Clear();
            _GetTimeRunScan();  //Write the timetag of the action
            textBoxToTemporary();
            Task.Delay(200).Wait();
            _DBCheckFull("AliveHosts");
            Task.Delay(200).Wait();
            listBoxNetsRowSelectedIndex = -1;
            foreach (object item in listBoxNetsRow.SelectedItems)
            {
                _listBoxNetsRowSelected.Add(item.ToString());
                listBoxNetsRowSelectedIndex++;
            }

            Task.Delay(200).Wait();
            Task.Run(() => _StatusLabel2ChangeTextbyCircle());
            await Task.Run(() => _scanSelectedNetwork());
        }

        private void _scanSelectedNetwork()
        {
            try
            {
                string _tempNET = "";
                _comboBoxTargedPcClr();
                _addressesPing.Clear();
                _addrPing.Clear();
                _addrPing.Add("0.0.0.0");

                string[] lns = _listBoxNetsRowSelected.ToArray();
                if (listBoxNetsRowSelectedIndex > -1)
                {
                    foreach (string scannedNet in lns)  //Copy hosts from listbox to ComboBoxTargedPC
                    {
                        if (_StopSearchAliveHosts)
                        { break; }
                        else
                        {
                            string[] ipn2 = Regex.Split(scannedNet, "[.]");
                            _tempNET = ipn2[0] + "." + ipn2[1] + "." + ipn2[2] + ".";
                            scanRangeIP(_tempNET);
                            _StatusLabel2ListChangingText.Add("Ищу хосты в сети: " + _tempNET + "xxx");   //Формирование списка для смены сообщений в StatusLabel2
                        }
                    }
                    _CheckFilesPing();
                }
                else if (listBoxNetsRowSelectedIndex == -1 && textBoxInputNetOrInputPCLength > 6)
                {
                    string[] ipn2 = Regex.Split(textBoxInputNetOrInputPCText, "[.]");
                    _tempNET = ipn2[0] + "." + ipn2[1] + "." + ipn2[2] + ".";
                    scanRangeIP(_tempNET);
                    _CheckFilesPing();
                    //                    Task.WaitAll();
                }
                else
                { MessageBox.Show("Нужно выбрать сеть для сканирования из списка\nили\nуказать вручную"); }

                _RunTimer = false;
                timer1Stop();
                _timer2State(false);
                //Task.Delay(1000).Wait();
                _ProgressBar1Value100();

                if (_StopSearchAliveHosts)
                {
                    _StatusLabel2ForeColor(System.Drawing.Color.DarkRed);
                    _StatusLabel2Text("Поиск хостов прерван!");
                }
                else
                {
                    _StatusLabel2ForeColor(System.Drawing.Color.Black);
                    _StatusLabel2Text("Поиск хостов завершен!");
                }
            } catch (Exception expt) { MessageBox.Show(expt.ToString()); }
        }

        //https://msdn.microsoft.com/ru-ru/library/ms144961(v=vs.110).aspx
        //http://stackoverflow.com/questions/24158814/ping-sendasync-always-successful-even-if-client-is-not-pingable-in-cmd
        //http://stackoverflow.com/questions/13405848/how-to-perform-multiple-pings-in-parallel-using-c-sharp
        //http://stackoverflow.com/questions/22078510/how-can-i-make-many-pings-asynchronously-at-the-same-time

        private Task<System.Net.NetworkInformation.PingReply> PingAsync(string address, int WaitedTime)   //Asynchro ping
        {
            var tcs = new TaskCompletionSource<System.Net.NetworkInformation.PingReply>();
            System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
            ping.PingCompleted += (obj, sender) =>
            { tcs.SetResult(sender.Reply); };
            ping.SendAsync(address, WaitedTime);
            return tcs.Task;
        }

        private void scanRangeIP(string net)
        {
            List<string> addresses = new List<string>();
            for (int i = 0; i < 254; ++i)
                addresses.Add(net + i);

            List<Task<System.Net.NetworkInformation.PingReply>> pingTasks = new List<Task<System.Net.NetworkInformation.PingReply>>();
            foreach (var address in addresses)
            {
                pingTasks.Add(PingAsync(address, 30));
                if (_StopSearchAliveHosts)
                {
                    waitFilePing.Set();            //Stop Ping wating and Run _CheckFilesPing()
                    break;
                }
            }
            Task.WhenAll(pingTasks.ToArray());    //Wait for all the tasks to complete

            foreach (var pingTask in pingTasks)  //Now you can iterate over your list of pingTasks
            {
                _addrPing.Add(Convert.ToString(pingTask.Result.Address));
                if (_StopSearchAliveHosts)
                {
                    waitFilePing.Set();            //Stop Ping wating and Run _CheckFilesPing()
                    break;
                }
            }
            waitFilePing.Set();   //Run _CheckFilesPing()
        }

        private void _CheckFilesPing()  // Check Access into every hosts of list ones. Add they into "comboBoxTargedPC" and DB "AliveHosts"
        {
            int i = 0;
            waitFilePing.WaitOne();   // wait the moment waitFilePing.Set()
            try
            {
                foreach (string _apa in _addrPing.ToArray())  //Copy hosts from listbox to ComboBoxTargedPC
                {
                    if (_apa != "0.0.0.0" && _apa != null && !_StopSearchAliveHosts)
                    {
                        IPAddress addr = IPAddress.Parse(_apa);
                        string hostname = "";
                        try
                        {
                            IPHostEntry entry = Dns.GetHostEntry(addr);
                            hostname = entry.HostName.ToString();
                        } catch { hostname = "unknow" + i; }
                        _getUserAndSN(hostname, addr);
                        _comboBoxTargedPCAdd(hostname + " | " + _apa);
                        _textBoxLogs(hostname + " | " + addr + "\n");
                        i++;
                    }
                }

                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}; journal_mode =MEMORY;", databaseAliveHosts)))
                {
                    connection.Open();
                    string[] _addressesing = _addressesPing.ToArray();
                    foreach (string adr in _addressesing)
                    {
                        if (adr != null)
                        {
                            string[] s = Regex.Split(adr, @"[|]");
                            using (SQLiteCommand command = new SQLiteCommand("REPLACE INTO 'AliveHosts' ('ComputerName', 'ComputerNameShort', 'ComputerDomainName', 'ComputerIP', 'ComputerModel', 'ComputerSN', 'LogOnUser', 'ScannerName', 'ScannerIP', 'Date', 'Time') VALUES (@ComputerName, @ComputerNameShort, @ComputerDomainName, @ComputerIP, @ComputerModel, @ComputerSN, @LogOnUser, @ScannerName, @ScannerIP, @Date, @Time)", connection))
                            {
                                command.Parameters.Add(new SQLiteParameter("@ComputerName", s[0].Trim()));
                                command.Parameters.Add(new SQLiteParameter("@ComputerNameShort", s[1].Trim()));
                                command.Parameters.Add(new SQLiteParameter("@ComputerIP", s[2].Trim()));
                                command.Parameters.Add(new SQLiteParameter("@ComputerDomainName", s[3].Trim()));
                                command.Parameters.Add(new SQLiteParameter("@ComputerModel", s[4].Trim()));
                                command.Parameters.Add(new SQLiteParameter("@ComputerSN", s[5].Trim()));
                                command.Parameters.Add(new SQLiteParameter("@LogOnUser", s[6].Trim()));
                                command.Parameters.Add(new SQLiteParameter("@ScannerName", _myHostName));
                                command.Parameters.Add(new SQLiteParameter("@ScannerIP", _myIP));
                                command.Parameters.Add(new SQLiteParameter("@Date", _Date));
                                command.Parameters.Add(new SQLiteParameter("@Time", _Time));
                                try { command.ExecuteNonQuery(); } catch { };
                            }
                        }
                    }
                }
            } catch (Exception expt) { MessageBox.Show(expt.ToString()); }
        }

        private async void getHostsFromDomainMenu_Click(object sender, EventArgs e)
        {
            textBoxToTemporary();

            StopSearchItem.Enabled = true;
            _StopSearchAliveHosts = false;
            waitNetPing = new AutoResetEvent(false);
            waitFile = new AutoResetEvent(false);
            waitStop = new AutoResetEvent(false);
            waitNetStop = new AutoResetEvent(false);
            waitFilePing = new AutoResetEvent(false);


            if (textBoxLoginLength > 0 && textBoxPasswordLength > 0)
            {
                Thread t = new Thread(new ThreadStart(_StatusLabel2ChangeColor));
                t.Start();
                await Task.Run(() => _getDomainData());
            }

            else if (RunScanNetForce == true)
            {
                Thread t = new Thread(new ThreadStart(_StatusLabel2ChangeColor));
                t.Start();
                await Task.Run(() => _getDomainData());
            }

            else
            {
                MessageBox.Show("Не введены данные для получения информации!\n\n" +
                    "Выберите пункт с получением данных с домена повторно...\n" +
                    "Список ПК будет получен, если программа запущена под доменными учетными данными...");
                _ControlVisibleAnalyse(false);
                _ControlVisibleLicense(false);
                _ControlVisibleStartUP(true);
                RunScanNetForce = true;
                textBoxLogin.Focus();
            }
            comboUsers.Enabled = true;
        }


        /*
        private void coverttimeToolStripMenuItem_Click(object sender, EventArgs e)  //test
        {
            UsersLastLogOnDate();
        }

        public static Dictionary<string, DateTime> UsersLastLogOnDate() //Test //Searching for lastLogon attribute of user in multiple domain servers
        {
                var lastLogins = new Dictionary<string, DateTime>();
                System.DirectoryServices.ActiveDirectory.DomainControllerCollection domains = System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain().DomainControllers;
                foreach (System.DirectoryServices.ActiveDirectory.DomainController controller in domains)
                {
                    try
                    {
                        using (var directoryEntry = new System.DirectoryServices.DirectoryEntry(string.Format("LDAP://{0}", controller.Name)))
                        {
                            using (var searcher = new System.DirectoryServices.DirectorySearcher(directoryEntry))
                            {
                                searcher.PageSize = 1000;
                                searcher.Filter = "(&(objectClass=user)(!objectClass=computer))";
                                searcher.PropertiesToLoad.AddRange(new[] { "distinguishedName", "lastLogon" });
                                foreach (System.DirectoryServices.SearchResult searchResult in searcher.FindAll())
                                {
                                    if (searchResult.Properties.Contains("lastLogon"))
                                    {
                                        var lastLogOn = DateTime.FromFileTime((long)searchResult.Properties["lastLogon"][0]);
                                        var username = searchResult.Properties["distinguishedName"][0].ToString();
                                        if (lastLogins.ContainsKey(username))
                                        {
                                            if (DateTime.Compare(lastLogOn, lastLogins[username]) > 0)
                                            { lastLogins[username] = lastLogOn; }
                                        }
                                        else
                                        { lastLogins.Add(username, lastLogOn); }
                                    }
                                }
                            }

                        }


                    }
                    catch (System.Runtime.InteropServices.COMException comException)
                    {
                        // Domain controller is down or not responding
            //            Log.DebugFormat("Domain controller {0} is not responding.", controller.Name);
             //           Log.Error("Error in one of the domain controllers.", comException);
                        continue;
                    }
                }
                return lastLogins;
        }

        private static string ConvertSidToString(byte[] objectSid) //Test //Turn of the Bytes of the objectSID into string
        {
            System.Security.Principal.SecurityIdentifier si = new System.Security.Principal.SecurityIdentifier(objectSid, 0);
            return si.ToString();
        }
           */


        private void _getDomainData() //Get list of PC's and users's from the inputed DOMAIN
        {
            _StatusLabelCurrentText("Выбран домен: " + textBoxDomainText);
            _ProgressBar1Value0();
            _StopScannig = false;
            timer1Start();
            _StopSearchAliveHosts = false;
            _comboBoxTargedPcClr();
            _comboUsersClr();

            List<string> ListDomainElementsUsers = new List<string>();
            List<string> ListDomainElements = new List<string>();

            //        Hashtable hs = new Hashtable();
            //            HashSet<String> hsName = new HashSet<string>();

            string deConnectionString = "LDAP://" + textBoxDomainText;

            try
            {
                _textBoxLogs("\n");
                //Users
                ListDomainElementsUsers.Clear();
                _StatusLabel2Text("Получаю перечень логинов из домена " + textBoxDomainText);
                using (var entry = new System.DirectoryServices.DirectoryEntry(deConnectionString, textBoxLoginText, textBoxPasswordText, System.DirectoryServices.AuthenticationTypes.Secure))
                using (var search = new System.DirectoryServices.DirectorySearcher(entry))
                {
                    const string query = "(&(objectClass=user)(objectCategory=person)(sAMAccountName=*))"; //  "(&(objectCategory=person)(objectClass=user)(memberOf=*))";
                    search.Filter = query;
                    search.PageSize = 1000;
                    search.PropertiesToLoad.Add("displayName");         //FIO
                    search.PropertiesToLoad.Add("extensionAttribute1"); //NAV-code
                    search.PropertiesToLoad.Add("sAMAccountName");      //login
                    search.PropertiesToLoad.Add("lastLogon");           //LastLogon Date
                    search.PropertiesToLoad.Add("lastLogonTimestamp");  //LastLogon Date
                    search.PropertiesToLoad.Add("objectSid");           //SID
                    search.PropertiesToLoad.Add("whenCreated");         //Date and Time Created login
                    search.PropertiesToLoad.Add("userPrincipalName");    //e-mail

                    using (var mySearchResultColl = search.FindAll())
                    {

                        search.SizeLimit = mySearchResultColl.Count;
                        foreach (System.DirectoryServices.SearchResult result in mySearchResultColl)
                        {
                            string sid = "";
                            string displayName = result.Properties["displayName"].Count > 0
                                ? result.Properties["displayName"][0].ToString() : string.Empty;

                            string sAMAccountName = result.Properties["sAMAccountName"].Count > 0
                                ? result.Properties["sAMAccountName"][0].ToString() : string.Empty;

                            string NAVAccountName = result.Properties["extensionAttribute1"].Count > 0
                                ? result.Properties["extensionAttribute1"][0].ToString() : string.Empty;

                            // convert Binary SID to string
                            try
                            {
                                var sd = new System.Security.Principal.SecurityIdentifier((byte[])result.Properties["objectSid"][0], 0).ToString();
                                sid = sd.ToString();
                            } catch { }

                            string SIDAccount = result.Properties["objectSid"].Count > 0
                            ? sid : string.Empty;


                            string LastTimeLogonAccount = result.Properties["lastLogon"].Count > 0
                                ? DateTime.FromFileTime((long)result.Properties["lastLogon"][0]).ToString("u", CultureInfo.InvariantCulture) : string.Empty;

                            string LastTimeLogonTimeStampAccount = result.Properties["lastLogonTimestamp"].Count > 0
                            ? DateTime.FromFileTime((long)result.Properties["lastLogonTimestamp"][0]).ToString("u", CultureInfo.InvariantCulture) : string.Empty;

                            string TimeCreatedAccount = result.Properties["whenCreated"].Count > 0
                                ? result.Properties["whenCreated"][0].ToString() : string.Empty;

                            string MailAccount = result.Properties["userPrincipalName"].Count > 0
                                ? result.Properties["userPrincipalName"][0].ToString() : string.Empty;

                            if (!string.IsNullOrWhiteSpace(displayName) && !string.IsNullOrWhiteSpace(sAMAccountName)) // Пустые имена не нужны
                            {
                                ListDomainElementsUsers.Add(sAMAccountName + " | " + NAVAccountName + " | " + displayName + " | " + SIDAccount + " | " + LastTimeLogonAccount + " | " + LastTimeLogonTimeStampAccount + " | " + TimeCreatedAccount + " | " + MailAccount);
                                //                               _textBoxLogs(sAMAccountName + " | " + NAVAccountName + " | " + displayName + " | " + SIDAccount + " | " + LastTimeLogonAccount + " | " + LastTimeLogonTimeStampAccount + " | " + TimeCreatedAccount + " | " + MailAccount + "\n");
                                _textBoxLogs(sAMAccountName + "\n");
                                _comboUsersAdd(sAMAccountName);
                            }
                        }
                    }
                }

                // 'UserLogin' TEXT, 'UserFIO' TEXT, 'UserSID' TEXT, 'UserNAV' TEXT, 'UserMail' TEXT, 'UserDomain' TEXT, 'DomainOrHost' TEXT, 'LastLogonDate' TEXT, 'LastLogonTime' TEXT, 'PreviousLogonHosts' TEXT, 'CreateLoginDate' TEXT,'CreateLoginTime' TEXT, 'Date' TEXT, 'Time' TEXT "
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}; journal_mode =MEMORY;", databaseAllUsers)))
                {
                    string d1 = "", d2 = "", d3 = "", d4 = "";
                    int d1r = 0, d3r = 0;

                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand("begin", connection);
                    command.ExecuteNonQuery();
                    string[] str = ListDomainElementsUsers.ToArray();
                    foreach (string lde in str)
                    {
                        string[] l = Regex.Split(lde, "[|]");
                        command = new SQLiteCommand("REPLACE INTO AllUsers (UserLogin, UserSID, UserFIO, UserNAV, UserMail, UserDomain, LastLogonDate, LastLogonTime, CreateLoginDate) " +
                            " VALUES (@UserLogin, @UserSID, @UserFIO, @UserNAV, @UserMail, @UserDomain, @LastLogonDate, @LastLogonTime, @CreateLoginDate)", connection);
                        command.Parameters.AddWithValue("@UserLogin", l[0].Trim());
                        command.Parameters.AddWithValue("@UserNAV", l[1].Trim());
                        command.Parameters.AddWithValue("@UserMail", l[7].Trim());

                        command.Parameters.AddWithValue("@UserFIO", l[2].Trim());
                        command.Parameters.AddWithValue("@UserSID", l[3].Trim());

                        // Set into DB only last day login. Start block
                        try
                        {
                            string[] crdt = Regex.Split(l[4].Trim(), "[ ]");
                            d1 = crdt[0].Trim();
                            d2 = crdt[1].Trim();

                            if (!int.TryParse(d1.Replace("-", ""), out d1r))
                                d1r = 0;
                            else
                                d1r = int.Parse(d1.Replace("-", ""));
                        } catch
                        {
                            d1 = "";
                            d2 = "";
                            d1r = 0;
                        }
                        try
                        {
                            string[] crdt = Regex.Split(l[5].Trim(), "[ ]");
                            d3 = crdt[0].Trim();
                            d4 = crdt[1].Trim();
                            if (!int.TryParse(d3.Replace("-", ""), out d3r))
                                d3r = 0;
                            else
                                d3r = int.Parse(d3.Replace("-", ""));
                        } catch
                        {
                            d3 = "";
                            d4 = "";
                            d3r = 0;
                        }

                        if (d1r > d3r)
                        {
                            command.Parameters.AddWithValue("@LastLogonDate", d1);
                            command.Parameters.AddWithValue("@LastLogonTime", d2.Remove(d2.Length - 1));
                        }
                        else if (d1r < d3r)
                        {
                            command.Parameters.AddWithValue("@LastLogonDate", d3);
                            command.Parameters.AddWithValue("@LastLogonTime", d4.Remove(d4.Length - 1));
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@LastLogonDate", "");
                            command.Parameters.AddWithValue("@LastLogonTime", "");
                        }
                        // Set into DB only a day of the last login. End block

                        try
                        {
                            string[] stcr = Regex.Split(Regex.Split(l[6].Trim(), "[ ]")[0], "[.]"); //convert DateTime into date with format yyyy-mm-dd
                            command.Parameters.AddWithValue("@CreateLoginDate", stcr[2] + "-" + stcr[1] + "-" + stcr[0]);
                        } catch
                        {
                            command.Parameters.AddWithValue("@CreateLoginDate", "");
                        }

                        command.Parameters.AddWithValue("@UserDomain", textBoxDomainText);
                        try { command.ExecuteNonQuery(); } catch { };
                    }
                    command = new SQLiteCommand("end", connection);
                    command.ExecuteNonQuery();
                }

                //Hosts
                _textBoxLogs("\n");
                ListDomainElements.Clear();
                _StatusLabel2Text("Получаю список хостов из домена " + textBoxDomainText);
                using (var entry = new System.DirectoryServices.DirectoryEntry(deConnectionString, textBoxLoginText, textBoxPasswordText, System.DirectoryServices.AuthenticationTypes.Secure))
                using (var search = new System.DirectoryServices.DirectorySearcher(entry))
                {
                    const string query = "(&(objectClass=user)(objectCategory=computer)(sAMAccountName=*))"; //  "(&(objectCategory=person)(objectClass=user)(memberOf=*))";
                    search.Filter = query;
                    search.PageSize = 1000;
                    search.PropertiesToLoad.Add("cn");
                    search.PropertiesToLoad.Add("dNSHostName");
                    search.PropertiesToLoad.Add("lastLogonTimestamp");
                    search.PropertiesToLoad.Add("lastLogon");

                    using (var mySearchResultColl = search.FindAll())
                    {
                        search.SizeLimit = mySearchResultColl.Count;
                        // progressBar1.Maximum = mySearchResultColl.Count;
                        foreach (System.DirectoryServices.SearchResult result in mySearchResultColl)
                        {
                            string displayNameHost = result.Properties["cn"].Count > 0
                                ? result.Properties["cn"][0].ToString().ToUpper() : string.Empty;
                            string displayDNSNameHost = result.Properties["dNSHostName"].Count > 0
                                ? result.Properties["dNSHostName"][0].ToString().ToUpper() : string.Empty;

                            string LastLogonDateHost = result.Properties["lastLogon"].Count > 0
                               ? DateTime.FromFileTime((long)result.Properties["lastLogon"][0]).ToString("u", CultureInfo.InvariantCulture) : string.Empty;

                            string LastLogonDateStampHost = result.Properties["lastLogonTimestamp"].Count > 0
                               ? DateTime.FromFileTime((long)result.Properties["lastLogonTimestamp"][0]).ToString("u", CultureInfo.InvariantCulture) : string.Empty;

                            if (!string.IsNullOrWhiteSpace(displayNameHost)) // Пустые имена не нужны
                            {
                                _comboBoxTargedPCAdd(displayNameHost);
                                _textBoxLogs(displayNameHost + "\n");
                                ListDomainElements.Add(displayNameHost + " | " + displayDNSNameHost + " | " + LastLogonDateHost + " | " + LastLogonDateStampHost);
                            }
                        }
                    }
                }

                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}; journal_mode =MEMORY;", databaseAliveHosts)))
                {
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand("begin", connection);
                    command.ExecuteNonQuery();
                    string[] str = ListDomainElements.ToArray();
                    foreach (string lde in str)
                    {
                        string[] l = Regex.Split(lde, "[|]");
                        command = new SQLiteCommand("REPLACE INTO AliveHosts (ComputerName, ComputerNameShort, ComputerDomainName, Date, Time) VALUES (@ComputerName, @ComputerNameShort, @ComputerDomainName, @Date, @Time)", connection);
                        command.Parameters.Add(new SQLiteParameter("@ComputerName", l[1].Trim()));
                        command.Parameters.Add(new SQLiteParameter("@ComputerNameShort", l[0].Trim()));
                        command.Parameters.Add(new SQLiteParameter("@ComputerDomainName", textBoxDomainText));
                        string[] ld = Regex.Split(l[2].Trim(), "[ ]");
                        if (ld.Length > 1)
                        {
                            command.Parameters.Add(new SQLiteParameter("@Date", ld[0].Trim()));
                            command.Parameters.Add(new SQLiteParameter("@Time", ld[1].Trim()));
                        }
                        else
                        {
                            command.Parameters.Add(new SQLiteParameter("@Date", DateTime.Now.ToString("yyyy-MM-dd")));
                            command.Parameters.Add(new SQLiteParameter("@Time", DateTime.Now.ToString("Thh:mm:ss")));
                        }
                        try { command.ExecuteNonQuery(); } catch { };
                    }
                    command = new SQLiteCommand("end", connection);
                    command.ExecuteNonQuery();
                    //                    connection.Close();
                }
            } catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Error");
            }

            _RunTimer = false;
            timer1Stop();
            _ProgressBar1Value100();
            _StatusLabel2Text("Вся информация из домена " + textBoxDomainText + " получена!");

            if (_StopSearchAliveHosts == true)
            { _StatusLabel2ForeColor(System.Drawing.Color.Crimson); _StatusLabel2Text("Сбор информации из домена " + textBoxDomainText + " прерван!"); }

        }

        //-------------/\/\/\/\------------// Search Hosts. End of the Block //-------------/\/\/\/\------------// 






        //-------------/\/\/\/\------------// Page Control of host. Start of the block//-------------/\/\/\/\------------// 


        private void ControlHostMenu_Click(object sender, EventArgs e) //
        {
            textBoxToTemporary();
            ParseTextboxInputNetOrPC();
            _CycleWait = false;

            if (_currentHostIP != _myIP) //Remote PC
            {
                NetworkShare share = new NetworkShare(_currentHostName, "ipc$", _User, _Password);
                share.Connect();  // Connect to the remote drive

                // Task.Delay(500).Wait();
                //    Task.Run(() => checkRunningRemoteRegistry(_currentHostIP, _currentHostIP, _User, _Password)); //Лучше использовать IP


                if (chkbxUser.Checked)
                {
                    Reboot2(_currentHostName);
                    StatusLabel2.Text = "Завершен сеанс активного пользователя на " + _currentHostName;
                    chkbxUser.Checked = false;
                }

                else if (chkbxPowerOff.Checked)
                {
                    SwitchOff(_currentHostName, 6);
                    StatusLabel2.Text = "Выполнено выключение " + _currentHostName;
                    chkbxPowerOff.Checked = false;
                }

                else if (chkbxReboot1.Checked)
                {
                    SwitchOff(_currentHostName, 4);
                    StatusLabel2.Text = "Выполнена перезагрузка " + _currentHostName;
                    chkbxReboot1.Checked = false;
                }

                else if (chkbxReboot2.Checked)
                {
                    PutHostDownRude(_currentHostName);
                    StatusLabel2.Text = "Выполнена принудительная перезагрузка " + _currentHostName;
                    chkbxReboot2.Checked = false;
                }

                else if (chkbxLockScreen.Checked)
                {
                    LockScreenHost(_currentHostName);
                    StatusLabel2.Text = "Заблокирован экран у пользователя на " + _currentHostName;
                    chkbxLockScreen.Checked = false;
                }

                else if (chkbxService.Checked)
                {
                    ControlServices(_currentHostName, comboBoxService.SelectedItem.ToString(), 1);
                    StatusLabel2.Text = "Выполнено выключение " + comboBoxService.SelectedItem.ToString() + " на " + _currentHostName;
                    chkbxService.Checked = false;
                }

                else if (checkBoxKillProcess.Checked)
                {
                    if (comboBoxProcess.SelectedIndex > -1)
                    {
                        string nameprocess = Regex.Split(comboBoxProcess.SelectedItem.ToString(), "[|]")[0].Trim();
                        ControlProcess(_currentHostName, nameprocess);
                        StatusLabel2.Text = "Остановлена программа: " + nameprocess + " на " + _currentHostName;
                    }
                    else
                    {
                        StatusLabel2.Text = "Не выбрана конечная цель на  " + _currentHostName;
                    }
                    checkBoxKillProcess.Checked = false;
                }

                else if (checkBoxRunProcess.Checked)
                {
                    if (textBoxNameProgramm.TextLength > 4)
                    {
                        ControlProcessRunTry(_currentHostName, textBoxNameProgramm.Text.ToLower().Trim());
                        StatusLabel2.Text = "Попытка запустить программу: " + textBoxNameProgramm.Text.Trim() + " на " + _currentHostName;
                    }
                    else
                    {
                        StatusLabel2.Text = "Неправильный формат программы";
                    }
                    checkBoxRunProcess.Checked = false;
                }

                else if (checkBoxReRunProcess.Checked)
                {
                    string nameprocess = "";
                    try { nameprocess = Regex.Split(comboBoxProcess.SelectedItem.ToString(), "[|]")[0].Trim(); } catch { }

                    if (textBoxNameProgramm.TextLength > 4)
                    {
                        ControlProcessReRun(_currentHostName, textBoxNameProgramm.Text.Trim());
                        StatusLabel2.Text = "Попытка перезапустить программу: " + textBoxNameProgramm.Text.Trim() + "\nна " + _currentHostName;
                    }
                    else if (nameprocess.Length > 4)
                    {
                        ControlProcessReRun(_currentHostName, nameprocess);
                        StatusLabel2.Text = "Попытка перезапустить программу: " + nameprocess + "\nна " + _currentHostName;
                    }
                    else
                    {
                        StatusLabel2.Text = "Неправильный формат программы";
                    }
                    checkBoxRunProcess.Checked = false;
                }


                //Проверить!!!
                else if (checkBox3.Checked)
                {
                    if (textBoxNameProgramm.TextLength > 4)
                    {
                        ControlProcess3(_currentHostName, textBoxNameProgramm.Text.Trim());
                        StatusLabel2.Text = "Попытка запустить программу: " + textBoxNameProgramm.Text.Trim() + " на " + _currentHostName;
                    }
                    else
                    {
                        StatusLabel2.Text = "Неправильный формат программы";
                    }
                    checkBox3.Checked = false;
                }

                else if (checkBox4.Checked)
                {
                    if (textBoxNameProgramm.TextLength > 4)
                    {
                        ControlProcess4(_currentHostName, textBoxNameProgramm.Text.Trim());
                        StatusLabel2.Text = "Попытка запустить программу: " + textBoxNameProgramm.Text.Trim() + " на " + _currentHostName;
                    }
                    else
                    {
                        StatusLabel2.Text = "Неправильный формат программы";
                    }
                    checkBox4.Checked = false;
                }

                else if (checkBox6.Checked)
                {
                    if (textBoxNameProgramm.TextLength > 4)
                    {
                        ControlProcess6(_currentHostName, textBoxNameProgramm.Text.Trim());
                        StatusLabel2.Text = "Попытка запустить программу: " + textBoxNameProgramm.Text.Trim() + " на " + _currentHostName;
                    }
                    else
                    {
                        StatusLabel2.Text = "Неправильный формат программы";
                    }
                    checkBox6.Checked = false;
                }

                else
                {
                    StatusLabel2.Text = "Пинг хоста " + _currentHostName + " | " + _currentHostIP;
                }
                _pingEnable();
                comboBoxProcess.SelectedIndex = -1;
                comboBoxService.SelectedIndex = -1;
                //                Task.Delay(500).Wait();
                share.Disconnect();
            }
        }

        private void ButtonPing_Click(object sender, EventArgs e) // button "Stop Ping"
        {
            _CycleWait = false;
            //waiter - НЕ ТАМ инициализация!!!!! Перенести в секцию инициализации     waiter = new AutoResetEvent(false); 
            if (buttonPing.Text == "Stop Ping")
            {
                _StatusLabel2Text(" ");
                //      waiter.Set();                         //Check this waiter (in pinghost)
                buttonPing.Text = "Выполнить";
            }

            else if (buttonPing.Text == "Выполнить")
            {
                textBoxToTemporary();
                ParseTextboxInputNetOrPC();
                //                _CycleWait = true;
                Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));
                if (_currentHostIP != _myIP) //Remote PC
                {
                    UpdateSelectedHost();

                    foreach (CheckBox checkBox in groupBoxPC.Controls.OfType<CheckBox>())      //Перебираем все чекбоксы на форме
                    {
                        if (checkBox.Checked)
                        {
                            int tagcheckbox = -1;
                            try { tagcheckbox = Convert.ToInt32(checkBox.Tag); } catch { }
                            switch (tagcheckbox)
                            {
                                case (0):
                                    _pingEnable();
                                    Reboot2(_currentHostName);
                                    _StatusLabel2Text("Выполнена перезагрузка " + _currentHostName + " | " + _currentHostIP);
                                    checkBox.Checked = false;
                                    break;
                                case (1):
                                    _pingEnable();
                                    SwitchOff(_currentHostName, 6);
                                    _StatusLabel2Text("Выполнена выключение " + _currentHostName + " | " + _currentHostIP);
                                    checkBox.Checked = false;
                                    break;
                                case (2):
                                    SwitchOff(_currentHostName, 4);
                                    _StatusLabel2Text("Завершение сеанса пользователя на " + _currentHostName + " | " + _currentHostIP);
                                    checkBox.Checked = false;
                                    break;
                                case (3):
                                    _pingEnable();
                                    PutHostDownRude(_currentHostName);
                                    StatusLabel2.Text = "Выполнена перезагрузка " + _currentHostName;
                                    checkBox.Checked = false;
                                    break;
                                case (4):
                                    _pingEnable();
                                    LockScreenHost(_currentHostName);
                                    StatusLabel2.Text = "Заблокирован экран у пользователя на " + _currentHostName;
                                    chkbxLockScreen.Checked = false;
                                    break;

                                default:
                                    _pingEnable();
                                    _StatusLabel2Text("Пинг хоста " + _currentHostName + " | " + _currentHostIP);
                                    break;
                            }
                        }
                        else
                        { _pingEnable(); _StatusLabel2Text("Пинг хоста " + _currentHostName + " | " + _currentHostIP); }
                    }
                    StatusLabel2.Text = "Готово!";
                }
            }
        }

        private void UpdateSelectedHost()
        {
            textBoxToTemporary();
            ParseTextboxInputNetOrPC();
            IPAddress addr = IPAddress.Parse(_currentHostIP);
            _getUserAndSN(_currentHostName, addr);

            _GetTimeRunScan();
            string[] stringHost = Regex.Split(TempConfigSingleHost, @"[|]");
            Task.Delay(200).Wait();
            StatusLabel2.Text = "Обновляю данные в базе... ";
            string TempComputerName = "", TempComputerIP = "", TempComputerModel = "", TempComputerSN = "", TempLogOnUser = "", TempComputerNameShort = "", TempComputerDomainName = "";

            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}; journal_mode =MEMORY;", databaseAliveHosts)))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("SELECT ComputerName, ComputerNameShort, ComputerDomainName, ComputerIP, ComputerModel, ComputerSN, LogOnUser, Date FROM AliveHosts Where ComputerName like '%" + _currentHostName + "%' ;", connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        foreach (DbDataRecord record in reader)
                        {
                            TempComputerName = record["ComputerName"].ToString();
                            TempComputerIP = record["ComputerIP"].ToString();
                            TempComputerModel = record["ComputerModel"].ToString();
                            TempComputerSN = record["ComputerSN"].ToString();
                            TempLogOnUser = record["LogOnUser"].ToString();
                            TempComputerNameShort = record["ComputerNameShort"].ToString();
                            TempComputerDomainName = record["ComputerDomainName"].ToString();
                        }
                    }
                }

                if (!stringHost[1].Trim().ToLower().Contains("unknow") && TempComputerName.Trim().Length > 0)
                {
                    TempComputerIP = stringHost[2].Trim();
                    if (!stringHost[4].ToLower().Contains("permission") || !stringHost[5].ToLower().Contains("permission"))
                    {
                        TempComputerModel = stringHost[4].Trim();
                        TempComputerSN = stringHost[5].Trim();
                        TempLogOnUser = stringHost[6].Trim();
                    }

                    using (SQLiteCommand command = new SQLiteCommand("UPDATE AliveHosts SET ComputerNameShort=@ComputerNameShort, ComputerDomainName=@ComputerDomainName, ComputerIP=@ComputerIP, ComputerModel=@ComputerModel," +
                            " ComputerSN=@ComputerSN, LogOnUser=@LogOnUser, ScannerName=@ScannerName, ScannerIP=@ScannerIP, Date=@Date, Time=@Time WHERE ComputerName=@ComputerName LIMIT 1", connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@ComputerNameShort", stringHost[1].Trim()));
                        command.Parameters.Add(new SQLiteParameter("@ComputerIP", TempComputerIP));
                        command.Parameters.Add(new SQLiteParameter("@ComputerDomainName", TempComputerDomainName));
                        command.Parameters.Add(new SQLiteParameter("@ComputerModel", TempComputerModel));
                        command.Parameters.Add(new SQLiteParameter("@ComputerSN", TempComputerSN));
                        command.Parameters.Add(new SQLiteParameter("@LogOnUser", TempLogOnUser));
                        command.Parameters.Add(new SQLiteParameter("@ScannerName", _myHostName));
                        command.Parameters.Add(new SQLiteParameter("@ScannerIP", _myIP));
                        command.Parameters.Add(new SQLiteParameter("@Date", _Date));
                        command.Parameters.Add(new SQLiteParameter("@Time", _Time));
                        try { command.ExecuteNonQuery(); } catch { };
                    }
                }
                else if (!stringHost[1].Trim().ToLower().Contains("unknow") && TempComputerName.Trim().Length == 0)
                {
                    TempComputerIP = stringHost[2].Trim();
                    TempComputerModel = stringHost[4].Trim();
                    TempComputerSN = stringHost[5].Trim();
                    TempLogOnUser = stringHost[6].Trim();

                    using (SQLiteCommand command = new SQLiteCommand("INSERT INTO AliveHosts ('ComputerName', 'ComputerNameShort', 'ComputerDomainName', 'ComputerIP', 'ComputerModel', 'ComputerSN', 'LogOnUser', 'ScannerName', 'ScannerIP', 'Date', 'Time') VALUES (@ComputerName, @ComputerNameShort, @ComputerDomainName, @ComputerIP, @ComputerModel, @ComputerSN, @LogOnUser, @ScannerName, @ScannerIP, @Date, @Time)", connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@ComputerName", stringHost[0].Trim()));
                        command.Parameters.Add(new SQLiteParameter("@ComputerNameShort", stringHost[1].Trim()));
                        command.Parameters.Add(new SQLiteParameter("@ComputerIP", TempComputerIP));
                        command.Parameters.Add(new SQLiteParameter("@ComputerDomainName", TempComputerDomainName));
                        command.Parameters.Add(new SQLiteParameter("@ComputerModel", TempComputerModel));
                        command.Parameters.Add(new SQLiteParameter("@ComputerSN", TempComputerSN));
                        command.Parameters.Add(new SQLiteParameter("@LogOnUser", TempLogOnUser));
                        command.Parameters.Add(new SQLiteParameter("@ScannerName", _myHostName));
                        command.Parameters.Add(new SQLiteParameter("@ScannerIP", _myIP));
                        command.Parameters.Add(new SQLiteParameter("@Date", _Date));
                        command.Parameters.Add(new SQLiteParameter("@Time", _Time));
                        try { command.ExecuteNonQuery(); } catch { };
                    }
                }
            }
        }

        // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection) 
        /* NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces(); 

         foreach (NetworkInterface network in networkInterfaces) 
         { 
             // Read the IP configuration for each network 
             IPInterfaceProperties properties = network.GetIPProperties(); 

             // Each network interface may have multiple IP addresses 
             foreach (IPAddressInformation address in properties.UnicastAddresses) 
             { 
                 // We're only interested in IPv4 addresses for now 
                 if (address.Address.AddressFamily != AddressFamily.InterNetwork) 
                     continue; 

                 // Ignore loopback addresses (e.g., 127.0.0.1) 
                 if (IPAddress.IsLoopback(address.Address)) 
                     continue; 

                 sb.AppendLine(address.Address.ToString() + " (" + network.Name + ")"); 
             } 
         } 
         */


        private void _pingEnable()
        {
            waiter = new AutoResetEvent(false);
            _PingStartTime = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            buttonPing.Text = "Stop Ping";
            labelControlPing.Text = "It is checking:\n" + _currentHostName;
            buttonPing.Enabled = true;
            labelControlPing.Visible = true;
            _CycleWait = true;
            Task.Run(() => _pingHost(_currentHostIP));
        }

        private void _pingHost(string ipHost) //ping host for alive
        {
            _StopSearchAliveHosts = false;

            if (ipHost.Contains("127.0.0.1"))
            {
                _StatusLabelCurrentText("");
                _labelControlPing(textBoxInputNetOrInputPCText + "\nЭтот хост не имеет DNS записей");
                _StatusLabel2Text("IPv4 адрес для хоста " + textBoxInputNetOrInputPCText + " не одпределен...");
                _labelControlPingBackColor(System.Drawing.Color.SandyBrown);
                _StopSearchAliveHosts = true;
            }
            else if (textBoxInputNetOrInputPCText.ToUpper() == _myHostName.ToUpper() || ipHost == _myIP || textBoxInputNetOrInputPCText == _myIP)
            {
                _StatusLabelCurrentText("Выбран " + _myHostName + " | " + _myIP);
                _labelControlPing(_myHostName + " - это имя и\n" + _myIP + " - адрес локального хоста");
                _StatusLabel2Text(_myIP + " - это IPv4 адрес локального хоста...");
                _labelControlPingBackColor(System.Drawing.Color.SandyBrown);
                _StopSearchAliveHosts = true;
            }
            else
            {
                try
                {
                    int PingTimeOut = 1;
                    _PingStartTime = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    _IPexist = false;

                    for (int i = 0; i < 1000000; i++)
                    {
                        PingTimeOut = 2 * PingTimeOut;
                        pingAsync(ipHost, PingTimeOut);

                        _PingEndTime = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                        int timepingout = (int)(_PingEndTime - _PingStartTime);

                        if (_IPexist == false && _CycleWait == true)
                        {
                            _labelControlPing(ipHost + "\nне доступен " + timepingout + " c");
                            _labelControlPingBackColor(System.Drawing.Color.SandyBrown);
                            _StatusLabel2Text(ipHost + " не доступен " + timepingout + " c");
                        }
                        else if (_IPexist == false && _CycleWait == false)
                        {
                            _labelControlPing(ipHost + "\nне доступен " + timepingout + " c");
                            _labelControlPingBackColor(System.Drawing.Color.SandyBrown);
                            _StatusLabel2Text(ipHost + " не доступен " + timepingout + " c");
                            i = 1000000;
                        }
                        else if (_IPexist == true && _CycleWait == true)
                        {
                            PingTimeOut = PingTimeOut / 2;
                            if (PingTimeOut < 2)
                            { PingTimeOut = 2; }
                            _labelControlPingBackColor(System.Drawing.Color.LightSkyBlue);
                            Task.Delay(1000).Wait();
                        }

                        else if (_IPexist == true && _CycleWait == false)
                        { i = 1000000; }

                        if (PingTimeOut > 5000)
                            PingTimeOut = 5000;
                        else if (PingTimeOut < 2)
                            PingTimeOut = 2;
                        else
                            continue;
                        if (_StopSearchAliveHosts)
                        {
                            break;
                        }
                        Task.Delay(500).Wait();
                        Task.WaitAll();
                    }
                } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); MessageBox.Show(expt.Message); }
            }
            _buttonPingText("Выполнить");
        }

        private void pingAsync(string ipHost, int PingTimOut) //ping host for "_pingHost()"
        {
            try
            {
                string data = "aaaaaaaaaaaaaaaa";
                byte[] buffer = Encoding.ASCII.GetBytes(data);

                System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping();
                IPAddress address = IPAddress.Parse(ipHost);
                System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions(64, true);

                pingSender.PingCompleted += new System.Net.NetworkInformation.PingCompletedEventHandler(pingCompletedCallback);
                pingSender.SendAsync(address, PingTimOut, buffer, options, waiter);
                waiter.WaitOne();
            } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); MessageBox.Show(expt.Message); }
        }
        //Replace at _labelping ("DNS Name is error" )

        private async void pingCompletedCallback(object sender, System.Net.NetworkInformation.PingCompletedEventArgs e) //event for "_pingAsync()"
        {
            string s = "";
            if (e.Cancelled)
            {
                s = "Ping canceled.";
                _labelControlPing("Ping canceled:\n" + e.Error.ToString());

                ((AutoResetEvent)e.UserState).Set();
            }

            if (e.Error != null)
            {
                s = "Ping failed:";
                s += e.Error.ToString();
                _labelControlPing("Ping failed:\n" + e.Error.ToString());
                ((AutoResetEvent)e.UserState).Set();
            }

            System.Net.NetworkInformation.PingReply reply = e.Reply;
            await Task.Run(() => pingDisplayReply(reply, s));
            ((AutoResetEvent)e.UserState).Set();
        }

        private void pingDisplayReply(System.Net.NetworkInformation.PingReply reply, string s) //display results of "_pingHost()"
        {
            if (reply == null)
            {
                _IPexist = false;
                _labelControlPing(_currentHostName + " \n " + reply.Address.ToString() + "\nPing status: " + reply.Status);
                _StatusLabel2Text(_currentHostName + " | ping - " + reply.Status);
                _StatusLabel2ForeColor(System.Drawing.Color.DarkOrange);
                return;
            }
            else if (reply.Status.ToString().ToLower().Contains("time out") || reply.Status.ToString().ToLower().Contains("превышен интервал"))
            {
                _IPexist = false;
                _labelControlPing(_currentHostName + " \n " + reply.Address.ToString() + "\nPing status: " + reply.Status);
                _StatusLabel2Text(_currentHostName + " | ping - " + reply.Status);
                _StatusLabel2ForeColor(System.Drawing.Color.DarkOrange);
            }
            else if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                _IPexist = true;
                _labelControlPingBackColor(System.Drawing.Color.LightSkyBlue);
                _labelControlPing(_currentHostName + " \n " + reply.Address.ToString() + " \n Ping - " + reply.Status + " \n RoundTrip time: " + reply.RoundtripTime);
                _StatusLabel2Text("ping " + _currentHostName + " - " + reply.RoundtripTime + " c");
            }
        }

        // Не использовал                                                           
        private async void _AsyncPing()          // Check exists of hosts
        {
            List<string> allIps = new List<string> { _myIP };
            var goodIps = await _Ping(allIps);
            if (goodIps.Count == 0)
                _labelControlPing("Все плохие");
            else
                _labelControlPing("\n" + goodIps);
        }

        // Не использовал                                                           
        public async static Task<List<string>> _Ping(List<string> ips)
        {
            int timeout = 2;
            List<string> goodIps = new List<string>();
            List<string> badIps = new List<string>();
            System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
            foreach (var ipAndPort in ips)
            {
                var answer = await ping.SendPingAsync(ipAndPort, timeout);
                if (answer.Status == System.Net.NetworkInformation.IPStatus.Success)
                    goodIps.Add(ipAndPort);
                else
                    badIps.Add(ipAndPort);
            }
            return goodIps;
        }

        // Проверить скорость работы и память!!!
        /*System.ServiceProcess.ServiceController service = new System.ServiceProcess.ServiceController( "IISADMIN", "srv2" );
service.Start();
TimeSpan t = TimeSpan.FromSeconds( "10" );
service.WaitForStatus( System.ServiceProcess.ServiceControllerStatus.Running, t );
*/

        private void ControlServices(string HostName, string NameService, int Combo = 0, string actionOverDone = "Stop") //Управление службами на удаленном пк
        {
            if (Combo == 1)
            { string NameProces = Regex.Split(comboBoxService.SelectedItem.ToString(), "[|]")[0].Trim(); }

            try
            {
                ConnectionOptions co = new ConnectionOptions();   //Connect to WMI of the Remote Host 
                co.Impersonation = ImpersonationLevel.Impersonate;
                co.Authentication = AuthenticationLevel.Packet;
                co.EnablePrivileges = true;
                co.Timeout = new TimeSpan(0, 0, 30);

                ManagementPath mp = new ManagementPath();
                mp.NamespacePath = @"\root\cimv2";
                if (_currentHostName != _myHostName && _User.Length > 0 && _Password.Length > 0)
                {
                    co.Username = _User;
                    co.Password = _Password;
                    mp.Server = HostName;
                }
                else if (_currentHostName != _myHostName && _User.Length == 0 && _Password.Length == 0)
                { mp.Server = HostName; }

                ManagementScope scope = new ManagementScope(mp, co);
                scope.Connect();
                bool isConnected = scope.IsConnected;
                if (isConnected)
                {
                    try
                    {
                        WqlObjectQuery query = new WqlObjectQuery("SELECT * FROM Win32_Service  WHERE Name=\"" + NameService + "\"");

                        if (actionOverDone != "Stop")
                        {
                            for (int k = 1; k < 5; k++)   //try to start up to 5 times of the service "RemoteRegistry" 
                            {
                                using (ManagementObjectSearcher srcd = new ManagementObjectSearcher(scope, query))
                                {
                                    ManagementObjectCollection queryCollection = srcd.Get();
                                    foreach (ManagementObject m in queryCollection)            //check running of the service  "RemoteRegistry"
                                    {
                                        if (m["Name"].ToString().Contains(NameService) && m["Started"].ToString().Contains("False"))
                                        {
                                            System.Diagnostics.Process process = new System.Diagnostics.Process();
                                            string strCmdLine = "/c sc \\\\" + HostName + " config \"" + NameService + "\" start= demand";
                                            process.StartInfo.UseShellExecute = false;
                                            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                            System.Diagnostics.Process.Start("CMD.exe", strCmdLine);

                                            process.StartInfo.RedirectStandardOutput = true;
                                            textBoxLogs.AppendText(process.StartInfo.FileName.ToString() + " ");
                                            textBoxLogs.AppendText(process.StartInfo.Arguments.ToString());
                                            textBoxLogs.AppendText(process.StandardOutput.ReadToEnd());

                                            Task.Delay(200).Wait(); //ждать 0,2 с
                                            // process.Close();
                                        }
                                    }
                                }
                            }
                        }
                    } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); }
                    // обработка ошибок
                    Task.Delay(200).Wait(); //ждать 0,2 с

                    try
                    {
                        WqlObjectQuery query = new WqlObjectQuery("SELECT * FROM Win32_Service  WHERE Name=\"" + NameService + "\"");
                        ManagementObjectSearcher fo = new ManagementObjectSearcher(scope, query);

                        foreach (ManagementObject mo in fo.Get())
                        {
                            if (actionOverDone != "Stop")
                            {
                                mo.InvokeMethod("StartService", new object[] { });
                            }
                            else
                            {
                                mo.InvokeMethod("StopService", new object[] { });
                            }
                            mo.Dispose();
                        }
                    } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); }
                    // обработка ошибок
                }
            } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); }
            // обработка ошибок
        }

        private static void Stop_CallBack(object sender, CompletedEventArgs e)
        {   /* что-то происходит*/         }

        //processInParams["CommandLine"] = string.Format("cmd /c \"taskkill /f /pid {0}\"", serviceObj["ProcessId"].ToString());
        //https://www.codeproject.com/articles/18146/how-to-almost-everything-in-wmi-via-c-part-proce
        /*        https://www.codeproject.com/articles/18146/how-to-almost-everything-in-wmi-via-c-part-proce
SelectQuery query = new SelectQuery("select * from Win32_process where name = '" + processName + "'");
object[] methodArgs = { "notepad.exe", null, null, 0 };
using (ManagementObjectSearcher searcher = new
        ManagementObjectSearcher(scope, query))
{
foreach (ManagementObject process in searcher.Get())
{
    //process.InvokeMethod("Terminate", null);
    process.InvokeMethod("Create", methodArgs);
}
}

*/

        private void ControlProcess(string HostName, string NameProcess) //Прерывание процесса на удаленном пк
        {
            textBoxLogs.AppendText("\n");
            textBoxLogs.AppendText("on \"" + HostName + "\" killed process: " + NameProcess);
            textBoxLogs.AppendText("\n");

            ConnectionOptions co = new ConnectionOptions();   //Connect to WMI of the Remote Host 
            co.Impersonation = ImpersonationLevel.Impersonate;
            co.Authentication = AuthenticationLevel.Packet;
            co.EnablePrivileges = true;
            co.Timeout = new TimeSpan(0, 0, 30);

            ManagementPath mp = new ManagementPath();
            mp.NamespacePath = @"\root\cimv2";
            if (_currentHostName != _myHostName && _User.Length > 0 && _Password.Length > 0)
            {
                co.Username = _User;
                co.Password = _Password;
                mp.Server = HostName;
            }
            else if (_currentHostName != _myHostName && _User.Length == 0 && _Password.Length == 0)
            { mp.Server = HostName; }

            try
            {
                ManagementScope scope = new ManagementScope(mp, co);
                scope.Connect();
                bool isConnected = scope.IsConnected;
                if (isConnected)
                {
                    comboBoxProcess.Items.Clear();
                    WqlObjectQuery q = new WqlObjectQuery("SELECT * FROM Win32_Process WHERE Name='" + NameProcess + "'");
                    ManagementObjectSearcher mbs = new ManagementObjectSearcher(scope, q);
                    ManagementObjectCollection mbsList = mbs.Get();
                    try
                    {
                        foreach (ManagementObject mo in mbsList)
                        {
                            mo.InvokeMethod("Terminate", null);
                        }
                    } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); }
                }
            } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); }
        }

        private void ControlProcessRunTry(string HostIP, string NameProcess)
        {
            ControlServices(HostIP, "RemoteRegistry", 0, "Start");

            string pathTolIntellect = "";
            string[] filesSlaveIntellect = { "Slave.exe", "intellect.exe" };
            string[] filesSlaveIntellectPath = {
                "C:\\Program Files (x86)\\Интеллект",
                "C:\\Program Files\\Интеллект",
                "D:\\Program Files (x86)\\Интеллект",
                "D:\\Program Files\\Интеллект"
                };

            if (NameProcess.Contains("slave") || NameProcess.Contains("intellect") || NameProcess.Contains("vitl"))
            {
                try
                {
                    using (RegistryKey key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, _currentHostName))
                    {
                        if (key != null)
                        {
                            using (RegistryKey regIntellect = key.OpenSubKey("SOFTWARE\\Wow6432Node\\ITV\\INTELLECT\\", RegistryKeyPermissionCheck.ReadSubTree))
                            {
                                if (regIntellect != null)
                                {
                                    pathTolIntellect = (string)regIntellect.GetValue("InstallPath");
                                    textBoxLogs.AppendText("\n" + _currentHostIP + " InstallPathIntellect: " + pathTolIntellect + "\n");
                                }
                            }
                        }
                    }
                } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); }

                try
                {
                    using (RegistryKey key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, _currentHostName))
                    {
                        if (key != null)
                        {
                            using (RegistryKey regIntellect = key.OpenSubKey("SOFTWARE\\ITV\\INTELLECT\\", RegistryKeyPermissionCheck.ReadSubTree))
                            {
                                if (regIntellect != null)
                                {
                                    pathTolIntellect = (string)regIntellect.GetValue("InstallPath");
                                    textBoxLogs.AppendText("\n" + _currentHostIP + " InstallPathIntellect: " + pathTolIntellect + "\n");
                                }
                            }
                        }
                    }
                } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); MessageBox.Show(expt.Message); }

                foreach (string fileSlaveIntellect in filesSlaveIntellect)
                {
                    string path = "";
                    try
                    {
                        path = pathTolIntellect;
                        textBoxLogs.AppendText("\n" + _currentHostIP + " InstallPathIntellect: \n" + pathTolIntellect + "\n");
                        ControlProcessRun(HostIP, @pathTolIntellect + fileSlaveIntellect);
                    } catch (Exception expt)
                    {
                        textBoxLogs.AppendText("\n" + _currentHostIP + " has Error: " + expt.Message + "\n");
                        foreach (string fileAndPath in filesSlaveIntellectPath)
                        {
                            textBoxLogs.AppendText("\n" + _currentHostIP + " fileAndPath\n" + fileAndPath + "\n");
                            ControlProcessRun(HostIP, @fileAndPath + "\\" + fileSlaveIntellect);
                        }
                    }
                }
            }
            else
                ControlProcessRun(HostIP, NameProcess);
        }

        private void ControlProcessRun(string HostIP, string NameProcess) //Запуск процесса на удаленном пк
        {
            ConnectionOptions co = new ConnectionOptions();   //Connect to WMI of the Remote Host 
            co.Impersonation = ImpersonationLevel.Impersonate;
            co.Authentication = AuthenticationLevel.Packet;
            co.EnablePrivileges = true;
            co.Timeout = new TimeSpan(0, 0, 60);

            ManagementPath mp = new ManagementPath();
            mp.NamespacePath = @"\root\cimv2";
            try
            {
                if (_currentHostName != _myHostName && _User.Length > 0 && _Password.Length > 0)
                {
                    co.Username = _User;
                    co.Password = _Password;
                    mp.Server = HostIP;
                }
                else if (_currentHostName != _myHostName && _User.Length == 0 && _Password.Length == 0)
                { mp.Server = HostIP; }


                System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(Environment.CurrentDirectory);
                info.FileName = Environment.CurrentDirectory + "\\psexec.exe";
                //                MessageBox.Show(info.FileName.ToString());
                info.Arguments = @"\\" + HostIP + " -u " + _User + " -p " + _Password + @" -d -i " + "\"" + NameProcess + "\"";

                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;
                info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                System.Diagnostics.Process p = System.Diagnostics.Process.Start(info);

                textBoxLogs.AppendText("\nRun Process: " + info.FileName.ToString() + " ");
                textBoxLogs.AppendText("\n" + info.Arguments.ToString());
                textBoxLogs.AppendText("\n" + p.StandardOutput.ReadToEnd());
                textBoxLogs.AppendText("\n");

                Task.Delay(200).Wait(); //ждать 1 с
                p.Close();
            } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); MessageBox.Show(expt.Message); }
        }

        private void ControlProcessReRun(string HostIP, string NameProcess) //Перезапуск процесса на удаленном пк
        {
            //processeshost.Add(name + " | " + nameProcessId + " | " + nameParentProcessId + " | " + nameExecutablePath + " | " + nameCreationDate + " | " + nameDescription + " | " + nameUser + " | " + HostName);
            string processname = "";
            string procesexe = "";

            try
            {
                foreach (string fullprocess in processeshost.ToArray())
                {
                    if (NameProcess.ToLower().Contains("intellect") || NameProcess.ToLower().Contains("vitl") || NameProcess.ToLower().Contains("axxon"))
                    {
                        if (fullprocess.ToLower().Contains("intellect"))
                        {
                            procesexe = Regex.Split(fullprocess, "[|]")[3].Trim();
                        }
                        ControlProcess(HostIP, "intellect.exe");
                        ControlProcess(HostIP, "Slave.exe");
                        ControlProcess(HostIP, "itvscript.exe");
                        ControlProcess(HostIP, "Video.run");
                        ControlProcess(HostIP, "player.run");
                        ControlProcess(HostIP, "vitlprview.run");
                        ControlProcess(HostIP, "vitlprvmon.run");
                        ControlProcess(HostIP, "abc.run");
                        ControlProcess(HostIP, "agency_person.run");
                    }
                    else if (NameProcess.ToLower().Contains("slave"))
                    {
                        if (fullprocess.ToLower().Contains("slave"))
                        {
                            procesexe = Regex.Split(fullprocess, "[|]")[3].Trim();
                        }
                        ControlProcess(HostIP, "Slave.exe");
                        ControlProcess(HostIP, "Video.run");
                        ControlProcess(HostIP, "vitlprview.run");
                        ControlProcess(HostIP, "vitlprvmon.run");
                    }

                    else if (fullprocess.ToLower().Contains(NameProcess.ToLower()))
                    {
                        processname = Regex.Split(fullprocess, "[|]")[0].Trim();
                        procesexe = Regex.Split(fullprocess, "[|]")[3].Trim();
                        ControlProcess(HostIP, processname);
                    }
                }
            } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); MessageBox.Show(expt.Message); }

            ControlProcessRun(HostIP, procesexe);
        }


        //Will Check this function!!!!
        private void ControlProcess3(string HostName, string NameProcess) //Управление службами на удаленном пк
        {
            ConnectionOptions co = new ConnectionOptions();   //Connect to WMI of the Remote Host 
            co.Impersonation = ImpersonationLevel.Impersonate;
            co.Authentication = AuthenticationLevel.Packet;
            co.EnablePrivileges = true;
            co.Timeout = new TimeSpan(0, 0, 30);

            ManagementPath mp = new ManagementPath();
            mp.NamespacePath = @"\root\cimv2";
            if (_currentHostName != _myHostName && _User.Length > 0 && _Password.Length > 0)
            {
                co.Username = _User;
                co.Password = _Password;
                mp.Server = HostName;
            }
            else if (_currentHostName != _myHostName && _User.Length == 0 && _Password.Length == 0)
            { mp.Server = HostName; }

            try
            {
                ManagementScope scope = new ManagementScope(mp, co);
                scope.Connect();
                bool isConnected = scope.IsConnected;
                if (isConnected)
                {
                    ObjectGetOptions options = new ObjectGetOptions();
                    ManagementPath path = new ManagementPath("Win32_Process");
                    ManagementPath pathw = new ManagementPath("Win32_ProcessStartup");
                    ManagementClass classObj = new ManagementClass(scope, path, options);
                    ManagementClass classObjW = new ManagementClass(scope, pathw, options);

                    ManagementObject processStartupInstance = classObjW.CreateInstance();
                    processStartupInstance["ShowWindow"] = 1; // A const value for showing the window normally

                    ManagementBaseObject inParams = null;
                    inParams = classObj.GetMethodParameters("Create");
                    inParams["CommandLine"] = NameProcess;
                    inParams["ProcessStartupInformation"] = processStartupInstance;
                    inParams["CurrentDirectory"] = "C:\\WINDOWS\\system32\\";
                    ManagementBaseObject outParams = classObj.InvokeMethod("Create", inParams, null);
                }
            } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); MessageBox.Show(expt.Message); }
        }

        //Work only background!
        private void ControlProcess4(string HostName, string NameProcess) //Управление службами на удаленном пк
        {
            ConnectionOptions co = new ConnectionOptions();   //Connect to WMI of the Remote Host 
            co.Impersonation = ImpersonationLevel.Impersonate;
            co.Authentication = AuthenticationLevel.Packet;
            co.EnablePrivileges = true;
            co.Timeout = new TimeSpan(0, 0, 30);

            ManagementPath mp = new ManagementPath();
            mp.NamespacePath = @"\root\cimv2";
            if (_currentHostName != _myHostName && _User.Length > 0 && _Password.Length > 0)
            {
                co.Username = _User;
                co.Password = _Password;
                mp.Server = HostName;
            }
            else if (_currentHostName != _myHostName && _User.Length == 0 && _Password.Length == 0)
            { mp.Server = HostName; }

            try
            {
                ManagementScope scope = new ManagementScope(mp, co);
                scope.Connect();
                bool isConnected = scope.IsConnected;
                if (isConnected)
                {
                    System.Diagnostics.Process process1 = new System.Diagnostics.Process();
                    string strCmdLine;///IMPLEVEL:Impersonate /PRIVILEGES:ENABLE /AUTHLEVEL:Pktintegrity /INTERACTIVE:ON /AUTHORITY:ALIAS
                    strCmdLine = "  /node: " + HostName + " /user: " + _User + " /password: " + _Password + " /privileges:enable process call create " + NameProcess;
                    process1.StartInfo.UseShellExecute = false;
                    process1.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    System.Diagnostics.Process.Start("wmic.exe", strCmdLine);
                    Task.Delay(1000).Wait(); //ждать 1 с
                    process1.Close();
                    // strCmdLine = " /user:" + userLogin+" /password:" + userPassword + " /PRIVILEGES:ENABLE /AUTHLEVEL:Pktintegrity /node:10.0.12.100 process call create " + NameProcess;
                    //schtasks /create /s server6.td.local /tn install /tr \\main\data\install.cmd /sc once /st 13:00 /ru system

                    //Удаленно включаем службу удаленный рабочий стол (Remote Desktop)
                    //Wmic / node:»servername» / user:»user @domain» / password:»password» RDToggle where ServerName =»server name» call SetAllowTSConnections 1
                }
            } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); MessageBox.Show(expt.Message); }
        }

        //Will Check this function!!!!
        private void ControlProcess6(string HosIP, string NameProcess) //Запуск процесса на удаленном пк c SchTasks.exe
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(Environment.SystemDirectory);
            try
            {
                //http://ab57.ru/cmdlist/schtasks.html
                info.FileName = Environment.CurrentDirectory + "\\SchTasks.exe";
                //                MessageBox.Show(info.FileName.ToString());
                info.Arguments = " /create /S " + HosIP + " /U " + _User + " /P " + _Password + " /RU " + _User + " /RP " + _Password + " /RL HIGHEST /SC ONSTART /TN \"RemoteProcess\" /TR '" + NameProcess + "'";
                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;
                info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                System.Diagnostics.Process p = System.Diagnostics.Process.Start(info);
                Task.Delay(1000).Wait(); //ждать 1 с
            } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); MessageBox.Show(expt.Message); }
            try
            {
                info.FileName = Environment.CurrentDirectory + "\\SchTasks.exe";
                //                MessageBox.Show(info.FileName.ToString());
                info.Arguments = " /Run /S " + HosIP + " /I /TN \"RemoteProcess\" ";
                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;
                info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                System.Diagnostics.Process p = System.Diagnostics.Process.Start(info);
                Task.Delay(1000).Wait(); //ждать 1 с
            } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); MessageBox.Show(expt.Message); }
            try
            {
                info.FileName = Environment.CurrentDirectory + "\\SchTasks.exe";
                //                MessageBox.Show(info.FileName.ToString());

                info.Arguments = " /Delete /S " + HosIP + " /TN \"RemoteProcess\" ";
                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;
                info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                System.Diagnostics.Process p = System.Diagnostics.Process.Start(info);
                Task.Delay(1000).Wait(); //ждать 1 с

                //You can use -  /Query /S COMPUTERNAME /TN "RemoteProcess" /V to find the current status
            } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); MessageBox.Show(expt.Message); }
        }

        private void checkBoxKillProcess_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxKillProcess.Checked)
            {
                if (textBoxNameProgramm.TextLength > 3)
                {
                    labelProcess.BackColor = System.Drawing.Color.Transparent;
                    textBoxNameProgramm.BackColor = System.Drawing.Color.Crimson;
                }
                else
                {
                    labelProcess.BackColor = System.Drawing.Color.Crimson;
                    textBoxNameProgramm.BackColor = System.Drawing.Color.White;
                }
                buttonGetProcess.Text = "Управлять процессами";
                checkBoxRunProcess.Checked = false;
                checkBoxReRunProcess.Checked = false;
            }
            else
            {
                textBoxNameProgramm.BackColor = System.Drawing.Color.White;
                buttonGetProcess.Text = "Получить список запущенных программ";
                labelProcess.BackColor = System.Drawing.Color.Transparent;
            }
        }

        private void checkBoxRunProcess_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxRunProcess.Checked)
            {
                buttonGetProcess.Text = "Управлять процессами";
                labelProcess.BackColor = System.Drawing.Color.Transparent;
                textBoxNameProgramm.BackColor = System.Drawing.Color.DarkCyan;
                checkBoxKillProcess.Checked = false;
                checkBoxReRunProcess.Checked = false;
                toolTipText1.SetToolTip(this.checkBoxRunProcess, "Запустить на " + textBoxInputNetOrInputPC.Text + "\n" + textBoxNameProgramm.Text);
            }
            else
            {
                buttonGetProcess.Text = "Получить список запущенных программ";
                textBoxNameProgramm.BackColor = System.Drawing.Color.White;
            }
        }

        private void checkBoxReRunProcess_CheckStateChanged(object sender, EventArgs e)
        {
            if (checkBoxReRunProcess.Checked)
            {
                if (textBoxNameProgramm.TextLength > 3)
                {
                    labelProcess.BackColor = System.Drawing.Color.Transparent;
                    textBoxNameProgramm.BackColor = System.Drawing.Color.Crimson;
                    toolTipText1.SetToolTip(this.checkBoxReRunProcess, "Перезапустить программу на " + textBoxInputNetOrInputPC.Text + "\n" + textBoxNameProgramm.Text);
                }
                else
                {
                    textBoxNameProgramm.BackColor = System.Drawing.Color.White;
                    labelProcess.BackColor = System.Drawing.Color.Crimson;
                    toolTipText1.SetToolTip(this.checkBoxReRunProcess, "Перезапустить программу на " + textBoxInputNetOrInputPC.Text + "\n" + labelProcess.Text);
                }

                buttonGetProcess.Text = "Управлять процессами";
                checkBoxRunProcess.Checked = false;
                checkBoxKillProcess.Checked = false;
            }
            else
            {
                labelProcess.BackColor = System.Drawing.Color.Transparent;
                textBoxNameProgramm.BackColor = System.Drawing.Color.White;
                buttonGetProcess.Text = "Получить список запущенных программ";
            }
        }

        private void textBoxNameProgramm_TextChanged(object sender, EventArgs e)
        {
            if (textBoxNameProgramm.TextLength > 3)
            {
                if (checkBoxKillProcess.Checked || checkBoxReRunProcess.Checked)
                {
                    labelProcess.BackColor = System.Drawing.Color.Transparent;
                    textBoxNameProgramm.BackColor = System.Drawing.Color.Crimson;
                }
                else
                {
                    textBoxNameProgramm.BackColor = System.Drawing.Color.White;
                }

                toolTipText1.SetToolTip(this.checkBoxRunProcess, "Запустить на " + textBoxInputNetOrInputPC.Text + "\n" + textBoxNameProgramm.Text);
                toolTipText1.SetToolTip(this.checkBoxReRunProcess, "Перезапустить программу на " + textBoxInputNetOrInputPC.Text + "\n" + textBoxNameProgramm.Text);
            }
            else
            {
                textBoxNameProgramm.BackColor = System.Drawing.Color.White;
                buttonGetProcess.Text = "Получить список запущенных программ";

                if (checkBoxKillProcess.Checked || checkBoxReRunProcess.Checked)
                {
                    labelProcess.BackColor = System.Drawing.Color.Crimson;
                }
            }
        }

        private void buttonGetProcess_Click(object sender, EventArgs e)
        {
            textBoxToTemporary();
            ParseTextboxInputNetOrPC();
            ManageProcess(_currentHostName);
        }

        private void ManageProcess(string HostName)
        {
            NetworkShare share = new NetworkShare(_currentHostName, "ipc$", _User, _Password);
            share.Connect();  // Connect to the remote drive

            _CycleWait = false;

            if (buttonGetProcess.Text.ToLower().Contains("управлять"))
            {
                if (checkBoxKillProcess.Checked)
                {
                    if (textBoxNameProgramm.Text.Trim().Length > 3)
                    {
                        GetProcess(HostName);
                        string textboxprg = textBoxNameProgramm.Text.Trim().ToLower();
                        if (textboxprg.Contains("intellect") || textboxprg.Contains("vitl") || textboxprg.Contains("axxon") || textboxprg.Contains(@"Video.run") || textboxprg.Contains("slave"))
                        {
                            foreach (string fullprocess in processeshost.ToArray())
                            {
                                try
                                {
                                    string processname = Regex.Split(fullprocess, "[|]")[0].Trim().ToLower();

                                    if (processname.Contains("intellect") || processname.Contains("vitlpr") || processname.Contains(@"itvscript.exe") || processname.Contains("axxon") || processname.Contains(@"video.run") || processname.Contains("slave") || processname.Contains(@"play.run"))
                                    {
                                        ControlProcess(_currentHostName, processname);
                                    }
                                } catch { }
                            }
                        }
                    }

                    else if (comboBoxProcess.SelectedIndex > -1)
                    {
                        string nameprocess = "";
                        try { nameprocess = Regex.Split(comboBoxProcess.SelectedItem.ToString(), "[|]")[0].Trim().ToLower(); } catch { }

                        string processname = "";
                        string procesexe = "";

                        try
                        {
                            foreach (string fullprocess in processeshost.ToArray())
                            {
                                string fullprocessl = fullprocess.ToLower();
                                if (nameprocess.Contains("intellect") || nameprocess.Contains("itvscript") || nameprocess.Contains("vitlpr") || nameprocess.Contains("axxon") || nameprocess.Contains(@"video.run") || nameprocess.Contains(@"player.run"))
                                {
                                    if (fullprocessl.Contains("intellect"))
                                    {
                                        procesexe = Regex.Split(fullprocessl, "[|]")[3].Trim();
                                    }
                                    ControlProcess(_currentHostName, "intellect.exe");
                                    ControlProcess(_currentHostName, "Slave.exe");
                                    ControlProcess(_currentHostName, "itvscript.exe");
                                    ControlProcess(_currentHostName, "Video.run");
                                    ControlProcess(_currentHostName, "player.run");
                                    ControlProcess(_currentHostName, "vitlprview.run");
                                    ControlProcess(_currentHostName, "vitlprvmon.run");
                                    ControlProcess(_currentHostName, "abc.run");
                                    ControlProcess(_currentHostName, "agency_person.run");
                                }
                                else if (nameprocess.Contains("slave") || nameprocess.Contains(@"Video.run") || nameprocess.Contains("vitlpr"))
                                {
                                    if (fullprocessl.Contains("slave"))
                                    {
                                        procesexe = Regex.Split(fullprocessl, "[|]")[3].Trim();
                                    }
                                    ControlProcess(_currentHostName, "Slave.exe");
                                    ControlProcess(_currentHostName, "Video.run");
                                    ControlProcess(_currentHostName, "vitlprview.run");
                                    ControlProcess(_currentHostName, "vitlprvmon.run");
                                }

                                else if (fullprocessl.Contains(nameprocess))
                                {
                                    processname = Regex.Split(fullprocessl, "[|]")[0].Trim();
                                    procesexe = Regex.Split(fullprocessl, "[|]")[3].Trim();
                                    ControlProcess(_currentHostName, processname);
                                }
                            }
                        } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); MessageBox.Show(expt.Message); }

                        // ControlProcess(_currentHostName, nameprocess);


                        StatusLabel2.Text = "Остановлена программа: " + nameprocess + " на " + _currentHostName;
                    }
                    else
                    {
                        StatusLabel2.Text = "Не выбрана конечная цель на  " + _currentHostName;
                    }
                    checkBoxKillProcess.Checked = false;
                }

                else if (checkBoxRunProcess.Checked)
                {
                    if (textBoxNameProgramm.TextLength > 3)
                    {
                        ControlProcessRunTry(_currentHostIP, textBoxNameProgramm.Text.ToLower().Trim());
                        StatusLabel2.Text = "Попытка запустить программу: " + textBoxNameProgramm.Text.Trim() + "\nна " + _currentHostName;
                    }
                    else
                    {
                        StatusLabel2.Text = "Неправильный формат программы";
                    }
                    checkBoxRunProcess.Checked = false;
                }

                else if (checkBoxReRunProcess.Checked)
                {
                    string nameprocess = "";
                    try { nameprocess = Regex.Split(comboBoxProcess.SelectedItem.ToString(), "[|]")[0].Trim(); } catch { }

                    if (textBoxNameProgramm.TextLength > 4)
                    {
                        ControlProcessReRun(_currentHostName, textBoxNameProgramm.Text.Trim());
                        StatusLabel2.Text = "Попытка перезапустить программу: " + textBoxNameProgramm.Text.Trim() + "\nна " + _currentHostName;
                    }
                    else if (nameprocess.Length > 4)
                    {
                        ControlProcessReRun(_currentHostName, nameprocess);
                        StatusLabel2.Text = "Попытка перезапустить программу: " + nameprocess + "\nна " + _currentHostName;
                    }
                    else
                    {
                        StatusLabel2.Text = "Неправильный формат программы";
                    }
                    checkBoxRunProcess.Checked = false;
                }


                //Проверить!!!
                else if (checkBox3.Checked)
                {
                    if (textBoxNameProgramm.TextLength > 4)
                    {
                        ControlProcess3(_currentHostName, textBoxNameProgramm.Text.Trim());
                        StatusLabel2.Text = "Попытка запустить программу: " + textBoxNameProgramm.Text.Trim() + "\nна " + _currentHostName;
                    }
                    else
                    {
                        StatusLabel2.Text = "Неправильный формат программы";
                    }
                    checkBox3.Checked = false;
                }

                else if (checkBox4.Checked)
                {
                    if (textBoxNameProgramm.TextLength > 4)
                    {
                        ControlProcess4(_currentHostIP, textBoxNameProgramm.Text.Trim());
                        StatusLabel2.Text = "Попытка запустить программу: " + textBoxNameProgramm.Text.Trim() + "\nна " + _currentHostName;
                    }
                    else
                    {
                        StatusLabel2.Text = "Неправильный формат программы";
                    }
                    checkBox4.Checked = false;
                }

                else if (checkBox6.Checked)
                {
                    if (textBoxNameProgramm.TextLength > 4)
                    {
                        ControlProcess6(_currentHostIP, textBoxNameProgramm.Text.Trim());
                        StatusLabel2.Text = "Попытка запустить программу: " + textBoxNameProgramm.Text.Trim() + "\nна " + _currentHostName;
                    }
                    else
                    {
                        StatusLabel2.Text = "Неправильный формат программы";
                    }
                    checkBox6.Checked = false;
                }
            }

            GetProcess(HostName);
            _StatusLabel2Text(" ");

            _pingEnable();

            labelProcess.BackColor = System.Drawing.Color.Transparent;
            textBoxNameProgramm.Clear();
            textBoxNameProgramm.BackColor = System.Drawing.Color.White;
            buttonGetProcess.Text = "Получить список запущенных программ";

            foreach (CheckBox checkBox in groupBoxProcess.Controls.OfType<CheckBox>())
            { checkBox.Checked = false; }
            //            share.Disconnect();
        }

        private void GetProcess(string HostName) //Управление службами на удаленном пк
        {
            processeshost.Clear();
            try { comboBoxProcess.Items.Clear(); } catch { }
            comboBoxProcess.Sorted = true;
            NetworkShare share = new NetworkShare(_currentHostName, "ipc$", _User, _Password);
            share.Connect();  // Connect to the remote drive

            try
            {
                ConnectionOptions co = new ConnectionOptions();   //Connect to WMI of the Remote Host 
                co.Impersonation = ImpersonationLevel.Impersonate;
                co.Authentication = AuthenticationLevel.Packet;
                co.EnablePrivileges = true;
                co.Timeout = new TimeSpan(0, 0, 30);

                ManagementPath mp = new ManagementPath();
                mp.NamespacePath = @"\root\cimv2";
                if (_currentHostName != _myHostName && _User.Length > 0 && _Password.Length > 0)
                {
                    co.Username = _User;
                    co.Password = _Password;
                    mp.Server = HostName;
                }
                else if (_currentHostName != _myHostName && _User.Length == 0 && _Password.Length == 0)
                { mp.Server = HostName; }

                ManagementScope scope = new ManagementScope(mp, co);
                scope.Connect();
                bool isConnected = scope.IsConnected;
                string name = "";
                string nameProcessId = "";
                string nameParentProcessId = "";
                string nameExecutablePath = "";
                string nameCreationDate = "";
                string nameDescription = "";
                string nameUser = "";

                if (isConnected)
                {
                    WqlObjectQuery q = new WqlObjectQuery("SELECT * FROM Win32_Process");
                    ManagementObjectSearcher mbs = new ManagementObjectSearcher(scope, q);
                    ManagementObjectCollection mbsList = mbs.Get();
                    foreach (ManagementObject mo in mbsList)
                    {
                        try
                        {
                            if (mo["Name"].ToString() != null)
                            {
                                name = "";
                                nameProcessId = "";
                                nameParentProcessId = "";
                                nameExecutablePath = "";
                                nameDescription = "";
                                nameUser = "";
                                nameCreationDate = "";

                                try
                                {
                                    name = mo["Name"].ToString();
                                } catch { }
                                try
                                {
                                    nameProcessId = mo["ProcessId"].ToString();
                                } catch { }
                                try
                                {
                                    nameParentProcessId = mo["ParentProcessId"].ToString();
                                } catch { }
                                try
                                {
                                    nameExecutablePath = mo["ExecutablePath"].ToString();
                                } catch { }
                                try
                                {
                                    nameDescription = mo["Description"].ToString();
                                } catch { }

                                try
                                {
                                    //                                    string ax = mo["CreationDate"].ToString();
                                    //                                    string datestr = Regex.Split(getDateTimeFromDmtfDate(ax), " ")[0];
                                    //                                    DateTime result = DateTime.ParseExact(datestr, formatsrc, CultureInfo.InvariantCulture);
                                    //                                    string dateCr = result.ToString(formatdst);
                                    //                                    string timeCr = Regex.Split(getDateTimeFromDmtfDate(ax), " ")[1];

                                    //                                    nameCreationDate = dateCr + " " + timeCr;
                                    nameCreationDate = TransformObjectToDateTime(mo["CreationDate"].ToString());
                                } catch { }
                                try
                                {
                                    ManagementBaseObject outParams = mo.InvokeMethod("GetOwner", null, null);
                                    nameUser = outParams["Domain"].ToString() + "\\" + outParams["User"].ToString();
                                } catch { }

                                //description - отсутствует
                                comboBoxProcess.Items.Add(name + " | " + nameProcessId + " | " + nameParentProcessId + " | " + nameExecutablePath + " | " + nameCreationDate);
                                processeshost.Add(name + " | " + nameProcessId + " | " + nameParentProcessId + " | " + nameExecutablePath + " | " + nameCreationDate + " | " + nameDescription + " | " + nameUser + " | " + HostName);

                                //         CreationDate 'дата и время начала выполнения процесса
                                //         ExecutablePath 'полный путь к исполняемому файлу процесса
                                //         ParentProcessId 'PID родительского процесса
                            }
                        } catch { }
                    }
                }
            } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); MessageBox.Show(expt.Message); }
            share.Disconnect();
            try { comboBoxProcess.SelectedIndex = 0; } catch { }
        }

        private void buttonGetServices_Click(object sender, EventArgs e)
        {
            textBoxToTemporary();
            ParseTextboxInputNetOrPC();
            if (!buttonGetServices.Text.ToLower().Contains("получить") && checkBoxChangeStateService.Checked)
            {
                NetworkShare share = new NetworkShare(_currentHostName, "ipc$", _User, _Password);
                share.Connect();  // Connect to the remote drive

                if (chkbxService.Checked)
                    ControlServices(_currentHostName, labelSelectedService.Text, 0, "Start");
                else
                    ControlServices(_currentHostName, labelSelectedService.Text);
                share.Disconnect();
                checkBoxChangeStateService.Checked = false;
                chkbxService.Checked = false;
            }
            Thread.Sleep(200);
            buttonGetServices.Text = "Получить список служб";

            GetServices(_currentHostName);
            _pingEnable();
        }

        private void GetServices(string HostName) //Управление службами на удаленном пк
        {
            labelSelectedService.Text = "";
            labelStatusService.Text = "";
            labelDisplayNameService.Text = "";

            try { comboBoxService.Items.Clear(); } catch { }
            comboBoxService.Sorted = true;

            NetworkShare share = new NetworkShare(_currentHostName, "ipc$", _User, _Password);
            share.Connect();  // Connect to the remote drive

            try
            {
                ConnectionOptions co = new ConnectionOptions();   //Connect to WMI of the Remote Host 
                co.Impersonation = ImpersonationLevel.Impersonate;
                co.Authentication = AuthenticationLevel.Packet;
                co.EnablePrivileges = true;
                co.Timeout = new TimeSpan(0, 0, 30);

                ManagementPath mp = new ManagementPath();
                mp.NamespacePath = @"\root\cimv2";
                if (_currentHostName != _myHostName && _User.Length > 0 && _Password.Length > 0)
                {
                    co.Username = _User;
                    co.Password = _Password;
                    mp.Server = HostName;
                }
                else if (_currentHostName != _myHostName && _User.Length == 0 && _Password.Length == 0)
                { mp.Server = HostName; }

                ManagementScope scope = new ManagementScope(mp, co);
                scope.Connect();
                bool isConnected = scope.IsConnected;
                if (isConnected)
                {
                    comboBoxService.Items.Clear();
                    WqlObjectQuery q = new WqlObjectQuery("SELECT * FROM Win32_Service");
                    ManagementObjectSearcher mbs = new ManagementObjectSearcher(scope, q);
                    ManagementObjectCollection mbsList = mbs.Get();
                    foreach (ManagementObject mo in mbsList)
                    {
                        try
                        {
                            if (mo["Name"].ToString() != null)
                            {
                                comboBoxService.Items.Add(mo["Name"].ToString() + " | " + mo["State"].ToString() + " | " + mo["DisplayName"].ToString() + " | " + mo["Description"].ToString() + " | " + HostName);
                            }
                        } catch { }
                    }
                }
            } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); MessageBox.Show(expt.Message); }
            try { comboBoxService.SelectedIndex = 0; } catch { }
            share.Disconnect();
        }

        private void SwitchOff(string HostName, int LevelControl) //Test Reboot PC 1
        {
            textBoxToTemporary();
            ParseTextboxInputNetOrPC();

            if (_currentHostIP != _myIP)
            {
                //                NetworkShare share = new NetworkShare(_currentHostName, "ipc$", _User, _Password);
                //                share.Connect();  // Connect to the remote drive

                try
                {
                    ConnectionOptions co = new ConnectionOptions();   //Connect to WMI of the Remote Host 
                    co.Impersonation = ImpersonationLevel.Impersonate;
                    co.Authentication = AuthenticationLevel.Packet;
                    co.EnablePrivileges = true;
                    co.Timeout = new TimeSpan(0, 0, 30);

                    ManagementPath mp = new ManagementPath();
                    mp.NamespacePath = @"\root\cimv2";
                    if (_User.Length > 0 && _Password.Length > 0)
                    {
                        co.Username = _User;
                        co.Password = _Password;
                        mp.Server = HostName;
                    }
                    else if (_User.Length == 0 && _Password.Length == 0)
                    { mp.Server = HostName; }

                    ManagementScope ms = new ManagementScope(mp, co);
                    ms.Connect();
                    bool isConnected = ms.IsConnected;
                    if (isConnected)
                    {
                        SelectQuery query = new SelectQuery("Win32_OperatingSystem");
                        ManagementObjectSearcher searcher = new ManagementObjectSearcher(ms, query);

                        foreach (ManagementObject os in searcher.Get())
                        {
                            ManagementBaseObject inParams = os.GetMethodParameters("Win32Shutdown");  // Obtain in-parameters for the method
                            inParams["Flags"] = LevelControl;        // Add the input parameters.
                            ManagementBaseObject outParams = os.InvokeMethod("Win32Shutdown", inParams, null);   // Execute the method and obtain the return values.
                                                                                                                 /*
                                                                                             LogOff = 0,
                                                                                             Shutdown = 1,
                                                                                             Reboot = 2,
                                                                                             ForcedLogOff = 4
                                                                                             ForcedShutdown = 5,
                                                                                             ForcedReboot = 6,
                                                                                             PowerOff = 8,
                                                                                             ForcedPowerOff = 12;*/
                        }
                    }
                } catch (ManagementException err)
                { MessageBox.Show(HostName + " has An error occurred while trying to execute the WMI method: " + err.Message); } catch (System.UnauthorizedAccessException unauthorizedErr)
                { MessageBox.Show(HostName + " has Connection error (user name or password might be incorrect): " + unauthorizedErr.Message); }
                //                share.Disconnect();
            }
            else { MessageBox.Show("Выполнена попытка перезагрузить локальный ПК"); }
        }

        private void Reboot2(string HostName)  //Test Reboot PC 2
        {
            textBoxToTemporary();
            ParseTextboxInputNetOrPC();

            if (_currentHostIP != _myIP)
            {
                //                NetworkShare share = new NetworkShare(_currentHostName, "ipc$", _User, _Password);
                //                share.Connect();  // Connect to the remote drive

                try
                {
                    ConnectionOptions co = new ConnectionOptions();   //Connect to WMI of the Remote Host 
                    co.Impersonation = ImpersonationLevel.Impersonate;
                    co.Authentication = AuthenticationLevel.Packet;
                    co.EnablePrivileges = true;
                    co.Timeout = new TimeSpan(0, 0, 30);

                    ManagementPath mp = new ManagementPath();
                    mp.NamespacePath = @"\root\cimv2";
                    if (_User.Length > 0 && _Password.Length > 0)
                    {
                        co.Username = _User;
                        co.Password = _Password;
                        mp.Server = HostName;
                    }
                    else if (_User.Length == 0 && _Password.Length == 0)
                    { mp.Server = HostName; }

                    ManagementScope ms = new ManagementScope(mp, co);
                    ms.Connect();
                    bool isConnected = ms.IsConnected;
                    if (isConnected)
                    {
                        //Query remote computer across the connection
                        ObjectQuery oq = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                        ManagementObjectSearcher query1 = new ManagementObjectSearcher(ms, oq);
                        ManagementObjectCollection queryCollection1 = query1.Get();
                        foreach (ManagementObject mo in queryCollection1)
                        {
                            string[] ss = { "" };
                            mo.InvokeMethod("Reboot", ss);
                            _textBoxLogs(mo.ToString());
                        }
                    }
                } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); MessageBox.Show(expt.Message); }
                //                share.Disconnect();
            }
            else { MessageBox.Show("Выполнена попытка перезагрузить локальный ПК"); }
        }

        private void PutHostDownRude(string HostName)  //Test Forced Reboot PC
        {
            textBoxToTemporary();
            ParseTextboxInputNetOrPC();

            if (_currentHostIP != _myIP)
            {
                //                NetworkShare share = new NetworkShare(_currentHostName, "ipc$", _User, _Password);
                //                share.Connect();  // Connect to the remote drive

                try
                {
                    System.Diagnostics.Process process1 = new System.Diagnostics.Process();
                    string strCmdLine = "/c taskkill /S " + HostName + " /U " + _User + " /P " + _Password + " /F " + " /IM \"svchost.exe\" /T";
                    process1.StartInfo.UseShellExecute = false;

                    process1.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    System.Diagnostics.Process.Start("CMD.exe", strCmdLine);
                    Task.Delay(200).Wait(); //ждать 0,5 с
                    process1.Close();
                } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); MessageBox.Show(expt.Message); }
                //                share.Disconnect();
            }
            else { MessageBox.Show("Выполнена попытка принудительно перезагрузить локальный ПК"); }
        }

        private void PingItem_Click(object sender, EventArgs e)
        {
            _CycleWait = false;
            StatusLabel2.Text = "Пинг хоста " + _currentHostName;
            _textBoxInputNetOrInputPC(_currentHostName);
            ParseTextboxInputNetOrPC();
            _pingEnable();
            tabControl.SelectedTab = tabPageCtrl;
            groupBoxPC.Focus();
        }

        private void buttonGetInfoDevices_Click(object sender, EventArgs e)
        {
            textBoxToTemporary();
            ParseTextboxInputNetOrPC();

            if (_myIP != _currentHostIP)
                ControlRemoteDevices("findAll");
            else
                ControlLocalDevices("findAll");

            tabControl.SelectedTab = tabPageLog;
            textBoxLogs.Focus();
        }

        private void buttonControlDevices_Click(object sender, EventArgs e)
        {
            textBoxToTemporary();
            ParseTextboxInputNetOrPC();

            if (_myIP != _currentHostIP)
                ControlRemoteDevices();
            else
                ControlLocalDevices();

            tabControl.SelectedTab = tabPageLog;
            textBoxLogs.Focus();
        }

        private void ControlRemoteDevices(string getaction = "disableEnable")
        {
            textBoxToTemporary();
            ParseTextboxInputNetOrPC();

            NetworkShare share = new NetworkShare(_currentHostName, "IPC$", _User, _Password);
            share.Connect();  // Connect to the remote drive

            string[] filesdevcon = { "devcon.exe", "devcon32.exe", "devcon64.exe" };
            string[] filespsexec = { "psexec.exe", "psexec64.exe" };

            string actionOverDevice = checkStatuscheckBoxDeviceDisable();
            if (getaction != "disableEnable")
            { actionOverDevice = getaction; }

            foreach (string filedevcon in filesdevcon)
            {
                try
                {
                    FileInfo fi = new FileInfo(Application.StartupPath + @"\" + filedevcon);
                    if (!System.IO.Directory.Exists(_currentHostName + @"\c$\" + filedevcon))
                    { System.IO.File.Copy(fi.FullName, @"\\" + _currentHostName + @"\c$\" + filedevcon, true); }
                } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); }
            }

            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(Environment.CurrentDirectory);

            textBoxLogs.AppendText("\n");
            textBoxLogs.AppendText(_currentHostName + " (" + _currentHostIP + ")" + ":");
            textBoxLogs.AppendText("\n");
            foreach (string filedevcon in filesdevcon)
            {
                foreach (string filepsexec in filespsexec)
                {
                    try
                    {
                        info.FileName = Environment.CurrentDirectory + "\\" + filepsexec;
                        info.Arguments = " -accepteula \\\\" + _currentHostIP + " -u " + _User + " -p \"" + _Password + "\" -s -d \\\\" + _currentHostIP + "\\c$\\" + filedevcon + " " + actionOverDevice + " =" + selectedDevice[1, comboBoxSelectDevice.SelectedIndex].Trim();

                        info.RedirectStandardOutput = true;
                        info.UseShellExecute = false;
                        info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        System.Diagnostics.Process p = System.Diagnostics.Process.Start(info);
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardOutput = true;

                        textBoxLogs.AppendText(info.FileName.ToString() + " ");
                        textBoxLogs.AppendText(info.Arguments.ToString());
                        textBoxLogs.AppendText(p.StandardOutput.ReadToEnd());
                        p.Close();
                    } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); }

                    try
                    {
                        info.FileName = Environment.CurrentDirectory + @"\" + filepsexec;
                        info.Arguments = " -accepteula \\\\" + _currentHostIP + " -u " + _User + " -p \"" + _Password + "\" -s -d \\\\" + _currentHostIP + "\\c$\\" + filedevcon + " " + actionOverDevice + " *" + selectedDevice[1, comboBoxSelectDevice.SelectedIndex].Trim() + "*";

                        info.RedirectStandardOutput = true;
                        info.UseShellExecute = false;
                        info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        System.Diagnostics.Process p = System.Diagnostics.Process.Start(info);
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardOutput = true;

                        textBoxLogs.AppendText(info.FileName.ToString() + " ");
                        textBoxLogs.AppendText(info.Arguments.ToString());
                        textBoxLogs.AppendText(p.StandardOutput.ReadToEnd());
                        p.Close();
                        textBoxLogs.AppendText("\n");
                    } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); }
                }
            }
            Task.Delay(1000).Wait(); //ждать 1 с
            foreach (string filedevcon in filesdevcon)
            {
                System.IO.File.Delete(@"\\" + _currentHostName + @"\c$\" + filedevcon);
            }
            share.Disconnect();
        }

        private void ControlLocalDevices(string getaction = "disableEnable")
        {
            string[] filesdevcon = { "devcon.exe", "devcon32.exe", "devcon64.exe" };

            string actionOverDevice = checkStatuscheckBoxDeviceDisable();
            if (getaction != "disableEnable")
            { actionOverDevice = getaction; }

            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(Environment.CurrentDirectory);

            textBoxLogs.AppendText("\n");
            textBoxLogs.AppendText(_currentHostName + ":");
            textBoxLogs.AppendText("\n");
            foreach (string filedevcon in filesdevcon)
            {
                try
                {
                    info.FileName = Environment.CurrentDirectory + @"\" + filedevcon;
                    info.Arguments = " " + actionOverDevice + " =" + selectedDevice[1, comboBoxSelectDevice.SelectedIndex].Trim();
                    info.UseShellExecute = false;
                    info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    System.Diagnostics.Process p = System.Diagnostics.Process.Start(info);
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    textBoxLogs.AppendText(p.StandardOutput.ReadToEnd());
                    p.Close();
                } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); }

                try
                {
                    info.FileName = Environment.CurrentDirectory + @"\" + filedevcon;
                    info.Arguments = " " + actionOverDevice + " *" + selectedDevice[1, comboBoxSelectDevice.SelectedIndex].Trim() + "*";
                    info.UseShellExecute = false;
                    info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    System.Diagnostics.Process p = System.Diagnostics.Process.Start(info);
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    textBoxLogs.AppendText(p.StandardOutput.ReadToEnd());
                    p.Close();
                } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); }
                textBoxLogs.AppendText("\n");
            }
            Task.Delay(1000).Wait(); //ждать 1 с
            foreach (string filedevcon in filesdevcon)
            {
                System.IO.File.Delete(@"\\" + _currentHostName + @"\c$\" + filedevcon);
            }
        }

        private string checkStatuscheckBoxDeviceDisable() //return disable if cecked
        {
            if (!checkBoxDeviceDisable.Checked)
                return "enable";
            else
                return "disable";
        }

        private void LockScreenHost(string HostName)
        {
            ConnectionOptions co = new ConnectionOptions();   //Connect to WMI of the Remote Host 
            co.Impersonation = ImpersonationLevel.Impersonate;
            co.Authentication = AuthenticationLevel.Packet;
            co.EnablePrivileges = true;
            co.Timeout = new TimeSpan(0, 0, 30);

            ManagementPath mp = new ManagementPath();
            mp.NamespacePath = @"\root\cimv2";
            if (_currentHostIP != _myIP && _User.Length > 0 && _Password.Length > 0)
            {
                co.Username = _User;
                co.Password = _Password;
                mp.Server = _currentHostName;
            }
            else if (_currentHostIP != _myIP && _User.Length == 0 && _Password.Length == 0)
            { mp.Server = _currentHostName; }

            try
            {
                System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(Environment.CurrentDirectory);
                info.FileName = Environment.CurrentDirectory + "\\psexec.exe";
                //                MessageBox.Show(info.FileName.ToString());
                info.Arguments = @"\\" + _myIP + " -u " + _User + " -p " + _Password + @" -i RunDll32.exe user32.dll,LockWorkStation ";

                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;
                info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                System.Diagnostics.Process p = System.Diagnostics.Process.Start(info);
                Task.Delay(1000).Wait(); //ждать 1 с
            } catch (Exception expt) { _textBoxLogs("\n" + _currentHostIP + ": " + expt.Message + "\n"); MessageBox.Show(expt.Message); }

            //Mouse Button – Swap left button to function as right     Rundll32 User32.dll,SwapMouseButton
            //Stored passwords  RunDll32.exe keymgr.dll,KRShowKeyMgr
            //User Accounts            RunDll32.exe shell32.dll,Control_RunDLL nusrmgr.cpl
            //

        }

        private void checkBoxDeviceDisable_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDeviceDisable.Checked)
            {
                DisableDevice.Text = "Disable " + selectedDevice[0, comboBoxSelectDevice.SelectedIndex];
            }
            else
            {
                DisableDevice.Text = "Enable " + selectedDevice[0, comboBoxSelectDevice.SelectedIndex];
            }
        }


        //-------------/\/\/\/\------------// Page Control of host. End of the block//-------------/\/\/\/\------------// 






        private void listTablesDBItem_Click(object sender, EventArgs e)
        { DBReadNameTables(); }

        private void RDPMenuItem_Click(object sender, EventArgs e) //button "RDP"
        {
            textBoxToTemporary();
            ParseTextboxInputNetOrPC();

            if (_currentHostIP != _myIP)
            {
                if (RDP.Connected.ToString() == "1")
                { try { RDP.Disconnect(); } catch { } }

                else
                {
                    try
                    {
                        //https://www.codeproject.com/Articles/33979/Multi-RDP-Client-NET
                        RDP.Server = _currentHostIP;
                        RDP.UserName = _User;
                        IMsTscNonScriptable secured = (IMsTscNonScriptable)RDP.GetOcx();
                        secured.ClearTextPassword = _Password;

                        RDP.Connect();
                        RDP.AdvancedSettings5.DisplayConnectionBar = true;
                        RDP.AdvancedSettings5.ConnectionBarShowRestoreButton = true;
                        RDP.AdvancedSettings5.ConnectionBarShowMinimizeButton = true;
                        RDP.AdvancedSettings5.ConnectionBarShowRestoreButton = true;
                        RDP.AdvancedSettings5.EnableWindowsKey = 1;
                        RDP.AdvancedSettings5.GrabFocusOnConnect = true;
                        RDP.AdvancedSettings5.RedirectDrives = true;
                        RDP.AdvancedSettings5.RedirectPrinters = false;
                        RDP.AdvancedSettings5.PinConnectionBar = true;
                        RDP.AdvancedSettings5.ConnectionBarShowRestoreButton = true;
                        RDP.AdvancedSettings5.SmartSizing = true;
                        RDP.AdvancedSettings5.MinutesToIdleTimeout = 10;
                        RDP.AdvancedSettings4.ScaleBitmapCachesByBPP = 1; // should we disable wallpaper in the remote pc?
                        RDP.SyncSessionDisplaySettings(); // should we disable the full window drag in the remote pc and just show the box while dragging?
                                                          //                RDP.ColorDepth = 16; // int value can be 8, 15, 16, or 24
                                                          //                RDP.Fullscreen = true;
                        RDP.Height = 768;
                        RDP.Width = 1024;
                        RDP.DesktopWidth = 1024; // int value 
                        RDP.DesktopHeight = 768; // int value 
                    } catch { MessageBox.Show(_currentHostIP + "\nвыключен,\nили, возможно,\nне запущена или не настроена служба RDP,\nили\nзаблокированы порты Firewall'ом"); }
                }
            }
            else { MessageBox.Show("Для исключения зацикливания, не следует использовать RDP для доступа к локальному ПК"); }
        }


        private void _TagStopWork(string HostName)
        {
            timer1Stop();
            _RunTimer = false;
            _ProgressBar1Value0();
            _StopScannig = true;
            _ProgressBar1Value100();
            _StatusLabel2Text("Загрузка логов с  " + HostName + "  завершена.");
        }

        private void _ReadRemotePC(string HostName, string Namelog, string userLogin, string userPassword)
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
            databaseHost = fi.FullName;


            SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}; page_size = 65536; cache_size =100000; journal_mode =MEMORY;", databaseHost));
            connection.Open(); string query = "";
            SQLiteCommand command = new SQLiteCommand(query, connection);

            ConnectionOptions co = new ConnectionOptions();   //Connect to WMI of the Remote Host 
            co.Impersonation = ImpersonationLevel.Impersonate;
            co.Authentication = AuthenticationLevel.Packet;
            co.EnablePrivileges = true;
            co.Timeout = new TimeSpan(0, 0, 30);

            ManagementPath mp = new ManagementPath();
            mp.NamespacePath = @"\root\cimv2";
            if (_currentHostName != _myHostName && _User.Length > 0 && _Password.Length > 0)
            {
                co.Username = _User;
                co.Password = _Password;
                mp.Server = HostName;
            }
            else if (_currentHostName != _myHostName && _User.Length == 0 && _Password.Length == 0)
            { mp.Server = HostName; }

            ManagementScope ms = new ManagementScope(mp, co);
            ms.Connect();
            ManagementObjectSearcher srcd;
            bool isConnected = ms.IsConnected;
            if (isConnected)
            {
                string a2 = "", a3 = "", a4 = "", a5 = "", a6 = "", a61 = "", a7 = "", a8 = "";

                srcd = new ManagementObjectSearcher(ms, new ObjectQuery("SELECT  * FROM Win32_NTLogEvent WHERE Logfile =" + "'" + Namelog + "'"));
                ManagementObjectCollection queryCollection = srcd.Get();
                try
                {
                    foreach (ManagementObject mo in queryCollection)
                    {
                        if (mo["RecordNumber"] != null)
                        {
                            a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = ""; a7 = ""; a8 = ""; query = "";
                            try { a2 = mo["RecordNumber"].ToString(); } catch { }
                            try { a3 = mo["EventCode"].ToString(); } catch { }
                            try { a4 = mo["SourceName"].ToString(); } catch { }
                            try { a5 = mo["Category"].ToString(); } catch { }
                            try { a6 = Regex.Split(mo["TimeWritten"].ToString(), " ")[0].Trim(); } catch { }
                            try { a61 = Regex.Split(mo["TimeWritten"].ToString(), " ")[1].Trim(); } catch { }
                            try { a7 = mo["Message"].ToString(); } catch { }
                            try { a8 = mo["User"].ToString(); } catch { }

                            command = new SQLiteCommand("INSERT INTO 'EventsLogs' ('Logfile','ComputerName', 'RecordNumber', 'EventCode', 'SourceName', 'Category', 'Date', 'Time','Message', 'User') " +
                                                                         " VALUES (@Logfile, @ComputerName, @RecordNumber, @EventCode, @SourceName, @Category, @Date, @Time, @Message, @User)", connection);
                            command.Parameters.AddWithValue("@Logfile", Namelog);
                            command.Parameters.AddWithValue("@ComputerName", HostName);
                            command.Parameters.AddWithValue("@RecordNumber", a2);
                            command.Parameters.AddWithValue("@EventCode", a3);
                            command.Parameters.AddWithValue("@SourceName", a4);
                            command.Parameters.AddWithValue("@Category", a5);
                            command.Parameters.AddWithValue("@Date", a6);
                            command.Parameters.AddWithValue("@Time", a61);
                            command.Parameters.AddWithValue("@Message", a7);
                            command.Parameters.AddWithValue("@User", a8);
                            try { command.ExecuteNonQuery(); } catch (Exception expt) { MessageBox.Show(expt.ToString()); };
                        }
                    }
                } catch (Exception exp) { MessageBox.Show(exp.ToString()); }
                command.Dispose();
            }
            connection.Close();
        }

        private void _ReadRemotePCNet(string HostName, string HostIP, string userLogin, string userPassword)
        {
            _StatusLabel2Text("Определяю конфигурацию сети хоста... ");

            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
            databaseHost = fi.FullName;

            string a1 = "", a2 = "", a3 = "", a4 = "", a5 = "", a6 = "", query = "";

            ConnectionOptions co = new ConnectionOptions();   //Connect to WMI of the Remote Host 
            co.Impersonation = ImpersonationLevel.Impersonate;
            co.Authentication = AuthenticationLevel.Packet;
            co.EnablePrivileges = true;
            co.Timeout = new TimeSpan(0, 0, 600);

            ManagementPath mp = new ManagementPath();
            mp.NamespacePath = @"\root\cimv2";
            if (_currentHostName != _myHostName && _User.Length > 0 && _Password.Length > 0)
            {
                co.Username = _User;
                co.Password = _Password;
                mp.Server = HostName;
            }
            else if (_currentHostName != _myHostName && _User.Length == 0 && _Password.Length == 0)
            { mp.Server = HostName; }

            ManagementScope ms = new ManagementScope(mp, co);
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}; page_size = 65536; cache_size =100000; journal_mode =MEMORY;", databaseHost)))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand();
                try
                {
                    ms.Connect();
                    ManagementObjectSearcher srcd;

                    bool isConnected = ms.IsConnected;
                    if (isConnected)
                    {
                        query = ""; a4 = ""; a5 = ""; a6 = ""; _TempLogonUser = ""; _TempModel = "";

                        srcd = new ManagementObjectSearcher(ms, new ObjectQuery("SELECT SerialNumber FROM Win32_BIOS"));
                        ManagementObjectCollection queryCollection = srcd.Get();
                        foreach (ManagementObject mo in queryCollection)
                        { try { _TempSN = mo["SerialNumber"].ToString(); } catch { } }

                        srcd = new ManagementObjectSearcher(ms, new ObjectQuery("SELECT Name, UserName,Model,Manufacturer,TotalPhysicalMemory,Domain  FROM Win32_ComputerSystem"));
                        queryCollection = srcd.Get();
                        foreach (ManagementObject mo in queryCollection)
                        {
                            if (mo["Name"] != null)
                            {
                                try { _TempLogonUser = mo["UserName"].ToString(); } catch { } //Name of the logon User
                                try { _TempModel = mo["Model"].ToString(); } catch { } //model PC
                                try { a4 = mo["Domain"].ToString(); } catch { } //PC Domain's
                                try { a3 = mo["PartOfDomain"].ToString(); } catch { } //PC in Domain
                                try { a5 = mo["Manufacturer"].ToString(); } catch { } //Manufacturer PC
                                try { a6 = (Convert.ToDouble(mo["TotalPhysicalMemory"]) / 1024 / 1024 / 1024).ToString("0.#") + " GB"; } catch { } //RAM

                                query = " INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time')" +
                                         " VALUES ('" + HostName + "','UserNameOnline: " + "','" + _TempLogonUser + "','" + "Current logon User" + "','" + _Date + "','" + _Time + "'); ";

                                query += " INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time')" +
                                         " VALUES ('" + HostName + "','Domain: " + "','" + a4 + "','" + a3 + "','" + _Date + "','" + _Time + "'); ";

                                query += " INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time')" +
                                         " VALUES ('" + HostName + "','PC: " + _TempModel + "','" + a5 + "','SN: " + _TempSN + "" + "','" + _Date + "','" + _Time + "'); ";

                                query += " INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time')" +
                                         " VALUES ('" + HostName + "','RAM: " + "','" + a6 + "','" + "" + "','" + _Date + "','" + _Time + "'); ";
                                command = new SQLiteCommand(query, connection);
                                try { command.ExecuteNonQuery(); } catch { }


                                /*                        command.CommandText = "INSERT INTO 'AliveHosts' ('ComputerName', 'ComputerNameShort', 'ComputerDomainName', 'ComputerIP', 'ComputerModel', 'ComputerSN', 'LogOnUser', 'ScannerName', 'ScannerIP', 'Date', 'Time') " +
                                        " VALUES (@ComputerName, @ComputerNameShort, @ComputerDomainName, @ComputerIP, @ComputerModel, @ComputerSN, @LogOnUser, @ScannerName, @ScannerIP, @Date, @Time)";
                                    command.CommandType = CommandType.Text;
                                    command.Parameters.Add(new SQLiteParameter("@ComputerName", s[0].Trim()));
                                    command.Parameters.Add(new SQLiteParameter("@ComputerNameShort", s[1].Trim()));
                                    command.Parameters.Add(new SQLiteParameter("@ComputerIP", s[2].Trim()));
                                    command.Parameters.Add(new SQLiteParameter("@ComputerDomainName", s[3].Trim()));
                                    command.Parameters.Add(new SQLiteParameter("@ComputerModel", s[4].Trim()));
                                    command.Parameters.Add(new SQLiteParameter("@ComputerSN", s[5].Trim()));
                                    command.Parameters.Add(new SQLiteParameter("@LogOnUser", s[6].Trim()));
                                    command.Parameters.Add(new SQLiteParameter("@ScannerName", _myHostName));
                                    command.Parameters.Add(new SQLiteParameter("@ScannerIP", _myIP));
                                    command.Parameters.Add(new SQLiteParameter("@Date", _Date));
                                    command.Parameters.Add(new SQLiteParameter("@Time", _Time));
                                    command.ExecuteNonQuery();
            */

                            }
                        }

                        query = "";
                        //            srcd = new ManagementObjectSearcher(ms, new ObjectQuery("Select * from Win32_NetworkAdapterConfiguration WHERE IPEnabled=True"));
                        srcd = new ManagementObjectSearcher(ms, new ObjectQuery("Select * from Win32_NetworkAdapterConfiguration"));
                        queryCollection = srcd.Get();
                        foreach (ManagementObject mo in queryCollection)
                        {
                            if (mo["Caption"] != null)
                            {
                                a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = "";
                                try
                                {
                                    try
                                    {
                                        string[] addres1 = Regex.Split(mo["Caption"].ToString(), "[]]");
                                        a1 = addres1[1];
                                    } catch { a1 = "no"; }
                                    try { a2 = mo["MACAddress"].ToString(); } catch { a2 = "no"; }
                                    try
                                    {
                                        string[] addresses1 = (string[])mo["IPSubnet"];
                                        a3 = addresses1[0];
                                    } catch { a3 = "no"; }
                                    try { a4 = mo["DHCPServer"].ToString(); } catch { a4 = "no"; }
                                    try
                                    {
                                        string[] addresses1 = (string[])mo["DefaultIPGateway"];
                                        a5 = addresses1[0];
                                    } catch { a5 = "no"; }
                                    try { a6 = mo["IPEnabled"].ToString(); } catch { a6 = "no"; }
                                    query =
                                        "INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time')" +
                                        " VALUES ('" + HostName + "','Net - " + a6 + " :" + a1 + "','MAC: " + a2 + "','Net/DHCP/GW: " + a3 + "/" + a4 + "/" + a5 + "','" + _Date + "','" + _Time + "'); ";
                                } catch
                                {
                                    command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                             " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                                    command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                                    command.Parameters.AddWithValue("@Name", "Warning");
                                    command.Parameters.AddWithValue("@Value", "Win32_NetworkAdapterConfiguration");
                                    command.Parameters.AddWithValue("@Value1", "Exeption");
                                    command.Parameters.AddWithValue("@Date", _Date);
                                    command.Parameters.AddWithValue("@Time", _Time);
                                    try { command.ExecuteNonQuery(); } catch { }
                                }
                            }
                        }
                        command = new SQLiteCommand(query, connection);
                        command.ExecuteNonQuery();

                        a1 = ""; a2 = ""; a3 = ""; query = "";
                        srcd = new ManagementObjectSearcher(ms, new ObjectQuery("Select Caption,InstallDate from Win32_Product"));
                        queryCollection = srcd.Get();
                        foreach (ManagementObject mo in queryCollection)
                        {
                            if (mo["Caption"] != null)
                            {
                                try
                                {
                                    a1 = mo["Caption"].ToString();
                                    string datestr = mo["InstallDate"].ToString();
                                    DateTime result = DateTime.ParseExact(datestr, formatsrc, CultureInfo.InvariantCulture);
                                    a2 = result.ToString(formatdst);
                                    query += "INSERT INTO 'Product' ('ComputerName', 'Caption', 'InstallDate') " +
                                        "VALUES ('" + HostName + "','" + a1 + "','" + a2 + "'); ";
                                } catch { }
                            }
                        }
                        command = new SQLiteCommand(query, connection);
                        try { command.ExecuteNonQuery(); } catch { }
                    }
                } catch { }
                connection.Close();
            }
        }

        private void _GetRemoteNameLogs(string HostName)
        {
            _StatusLabel2Text("Получаю имена логов хоста... ");

            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + HostName + @".db");
            databaseHost = fi.FullName;

            for (int i = 0; i < 99; i++) { _nameLogs[i] = ""; }
            string query = "";

            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3; page_size = 65536; cache_size =100000; journal_mode =MEMORY;", databaseHost)))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand("", connection);
                command.ExecuteNonQuery();

                try
                {
                    System.Diagnostics.EventLog[] eventLogs = System.Diagnostics.EventLog.GetEventLogs(HostName);
                    foreach (System.Diagnostics.EventLog e in eventLogs)
                    {
                        Int64 sizeKB = 0;
                        // Determine if there is an event log file for this event log.
                        try
                        {
                            using (RegistryKey key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, HostName))
                            {
                                if (key != null)
                                {
                                    using (RegistryKey regEventLog = key.OpenSubKey("System\\CurrentControlSet\\Services\\EventLog\\" + e.Log, RegistryKeyPermissionCheck.ReadSubTree))
                                    {
                                        if (regEventLog != null)
                                        {
                                            Object temp = regEventLog.GetValue("File");
                                            if (temp != null)
                                            {
                                                FileInfo file = new FileInfo(temp.ToString());
                                                if (Convert.ToInt32(e.Entries.Count.ToString()) > 0)
                                                {
                                                    query = " INSERT INTO 'WindowsFeature' ('Name', 'Value', 'Value1' ,'ComputerName', 'Date', 'Time') VALUES ('Log: " + e.Log + "','Events: " + e.Entries.Count.ToString() + "/Size, kB: " + sizeKB.ToString() + "','Path: " + temp.ToString() + "','" + HostName + "','" + _Date + "','" + _Time + "'); ";
                                                    command = new SQLiteCommand(query, connection);
                                                    command.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        } catch { MessageBox.Show("Программу нужно запускать пользователем\nс правами администратора!"); }
                    }

                    HashSet<String> NamesOfLogs = new HashSet<string>(); //Temporary list - for remove dublicate from list

                    command = new SQLiteCommand("SELECT Name FROM 'WindowsFeature' Where ComputerName like '%" + HostName + "%' AND Value1 like 'LogName';", connection);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        try
                        {
                            foreach (DbDataRecord record in reader)
                            {
                                if (record != null)
                                { NamesOfLogs.Add(record["Name"].ToString()); }
                            }
                        } catch { }
                    }

                    command.Dispose();
                    //Transfer names of logs from HashSet into the global array "_nameLogs";
                    int k = 0;
                    string[] OnlyOneNameOfLog = NamesOfLogs.ToArray();
                    foreach (string namesLog in OnlyOneNameOfLog)
                    {
                        _nameLogs[k] = namesLog;
                        k++;
                    }
                } catch
                {
                    _currentRemoteRegistryRunning = false; //Не доступа до реестра
                    command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                     " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                    command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                    command.Parameters.AddWithValue("@Name", "Error");
                    command.Parameters.AddWithValue("@Value", "_GetRemoteNameLogs");
                    command.Parameters.AddWithValue("@Value1", "No Access!");
                    command.Parameters.AddWithValue("@Date", _Date);
                    command.Parameters.AddWithValue("@Time", _Time);
                    try { command.ExecuteNonQuery(); } catch { }
                }
            }
        }

        private void _loadLog(string Namelog, int stop = 0) //log to the TextBox
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
            databaseHost = fi.FullName;

            System.Diagnostics.EventLog myLog = new System.Diagnostics.EventLog();
            myLog.Log = Namelog;

            if (textBoxInputNetOrInputPCText != _myIP)
            { myLog.MachineName = _currentHostIP; }

            SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3; page_size = 65536; cache_size =100000; journal_mode =MEMORY;", databaseHost));
            connection.Open();
            SQLiteCommand command;
            string query =
                " INSERT INTO 'WindowsFeature' ('Name', 'Value' ,'ComputerName','Date') " +
                 "VALUES ('Log " + Namelog + " has events:','" + myLog.Entries.Count + "','" + _myHostName + "','" + _Date + "'); ";
            command = new SQLiteCommand(query, connection);
            command.ExecuteNonQuery();

            string a2 = "", a3 = "", a4 = "", a5 = "", a6 = "", a61 = "", a7 = "", a8 = ""; query = ""; int aa1 = 0;
            foreach (System.Diagnostics.EventLogEntry entry in myLog.Entries)
            {
                a2 = ""; a3 = ""; a4 = ""; a6 = ""; a7 = ""; a8 = "";
                try { a2 = entry.Index.ToString(); } catch { }
                try { a3 = entry.InstanceId.ToString(); } catch { }
                try { a4 = entry.Source.ToString(); } catch { }
                try
                {
                    string at = Regex.Split(entry.TimeGenerated.ToString(), " ")[0].Trim();
                    a6 = at.Substring(6) + "-" + at.Substring(3, 2) + "-" + at.Substring(0, 2);
                } catch { }
                try { a61 = Regex.Split(entry.TimeGenerated.ToString(), " ")[1].Trim(); } catch { }
                try { a7 = entry.Message.ToString().Trim(); } catch { }
                try { a8 = entry.UserName.ToString().Trim(); } catch { }
                aa1++;
                query = "INSERT INTO 'LOG" + Namelog.ToUpper() + "' ('log" + Namelog.ToLower() + "id', 'computername', 'recordnumber', 'eventcode', 'sourcename', 'category', 'date', 'time', 'message', 'user') " +
                " VALUES ('" + aa1 + "','" + _currentHostIP + "','" + a2 + "','" + a3 + "','" + a4 + "','" + a5 + "','" + a6 + "','" + a61 + "','" + a7 + "','" + a8 + "'); ";
                command = new SQLiteCommand(query, connection);
                try { command.ExecuteNonQuery(); } catch (Exception e) { _textBoxLogs(e.ToString()); }
            }
            command.Dispose();
            connection.Close();

            if (stop == 1)
            {
                timer1Stop();
                ProgressBar1.Value = 100;
                textBoxPassword.Enabled = true;
                textBoxLogin.Enabled = true;
                textBoxDomain.Enabled = true;
                tabControl.Enabled = true;
                tabControl.Visible = true;
                StatusLabel2.Text = "Загрузка логов с  " + _currentHostIP + "  завершена.";
            }
        }



        //-------------/\/\/\/\------------// FullScan. Start of the block //-------------/\/\/\/\------------// 
        private async void GetFullDataMenu_Click(object sender, EventArgs e) //
        {
            ProgressBar1.Value = 0;
            textBoxToTemporary();

            _currentRemoteRegistryRunning = true;
            if (tabControl.SelectedTab == tabDataGrid)
            { bool str = ParseSelectedCell(); }
            else
            { ParseTextboxInputNetOrPC(); }

            Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));
            Thread t = new Thread(new ThreadStart(_StatusLabel2ChangeColor));
            StatusLabel2.Text = "Готовлю переменные... ";
            t.Start();
            timer1Start();
            Task.Delay(200).Wait();

            StopSearchItem.Enabled = true;

            waitFile = new AutoResetEvent(false);
            waitStop = new AutoResetEvent(false);
            waitNetStop = new AutoResetEvent(false);
            waitFilePing = new AutoResetEvent(false);
            waitNetPing = new AutoResetEvent(false);

            _GlobalStopSearch = false;
            _StopSearchFiles = false;
            _StopSearchAliveHosts = false;
            _StopScanLogEvents = false;
            _StopScannig = false;

            StatusLabel2.Text = "Готовлю базу... ";
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
            databaseHost = fi.FullName;
            _DBCheckFull(_currentHostName);
            Task.Delay(200).Wait();

            _GetTimeRunScan();
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection))
                {
                    command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                    command.Parameters.AddWithValue("@Name", "ComputerIP");
                    command.Parameters.AddWithValue("@Value", _currentHostIP);
                    command.Parameters.AddWithValue("@Value1", _currentHostName);
                    command.Parameters.AddWithValue("@Date", _Date);
                    command.Parameters.AddWithValue("@Time", _Time);
                    try { command.ExecuteNonQuery(); } catch { }
                }
            }

            NetworkShare share = new NetworkShare(_currentHostName, "ipc$");//no use authintification 
            if (_currentHostIP != _myIP) //Remote PC
            {
                share = new NetworkShare(_currentHostName, "ipc$", _User, _Password);
                share.Connect();
                Task.Delay(500).Wait();
                StatusLabel2.Text = "Устанавливаю соединение... ";

                checkRunningRemoteRegistry(_currentHostName, _currentHostIP, _User, _Password); //Лучше использовать IP
                _ProgressWork5();

                IPAddress addr = IPAddress.Parse(_currentHostIP);
                _getUserAndSN(_currentHostName, addr);
                _GetTimeRunScan();
                string[] s = Regex.Split(TempConfigSingleHost, @"[|]");

                Task.Delay(200).Wait();
                StatusLabel2.Text = "Обновляю данные в базе... ";
                string TempComputerName = "", TempComputerIP = "", TempComputerModel = "", TempComputerSN = "", TempLogOnUser = "", TempComputerNameShort = "", TempComputerDomainName = "";
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}; journal_mode =MEMORY;", databaseAliveHosts)))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand("SELECT ComputerName, ComputerNameShort, ComputerDomainName, ComputerIP, ComputerModel, ComputerSN, LogOnUser, Date FROM AliveHosts Where ComputerName like '%" + _currentHostName + "%' ;", connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            foreach (DbDataRecord record in reader)
                            {
                                TempComputerName = record["ComputerName"].ToString();
                                TempComputerIP = record["ComputerIP"].ToString();
                                TempComputerModel = record["ComputerModel"].ToString();
                                TempComputerSN = record["ComputerSN"].ToString();
                                TempLogOnUser = record["LogOnUser"].ToString();
                                TempComputerNameShort = record["ComputerNameShort"].ToString();
                                TempComputerDomainName = record["ComputerDomainName"].ToString();
                            }
                        }
                    }

                    if (!s[1].Trim().ToLower().Contains("unknow") && TempComputerName.Trim().Length > 0)
                    {
                        TempComputerIP = s[2].Trim();
                        if (!s[4].ToLower().Contains("permission") || !s[5].ToLower().Contains("permission"))
                        {
                            TempComputerModel = s[4].Trim();
                            TempComputerSN = s[5].Trim();
                            TempLogOnUser = s[6].Trim();
                        }

                        using (SQLiteCommand command = new SQLiteCommand("UPDATE AliveHosts SET ComputerNameShort=@ComputerNameShort, ComputerDomainName=@ComputerDomainName, ComputerIP=@ComputerIP, ComputerModel=@ComputerModel," +
                                " ComputerSN=@ComputerSN, LogOnUser=@LogOnUser, ScannerName=@ScannerName, ScannerIP=@ScannerIP, Date=@Date, Time=@Time WHERE ComputerName=@ComputerName LIMIT 1", connection))
                        {
                            command.Parameters.Add(new SQLiteParameter("@ComputerNameShort", s[1].Trim()));
                            command.Parameters.Add(new SQLiteParameter("@ComputerIP", TempComputerIP));
                            command.Parameters.Add(new SQLiteParameter("@ComputerDomainName", TempComputerDomainName));
                            command.Parameters.Add(new SQLiteParameter("@ComputerModel", TempComputerModel));
                            command.Parameters.Add(new SQLiteParameter("@ComputerSN", TempComputerSN));
                            command.Parameters.Add(new SQLiteParameter("@LogOnUser", TempLogOnUser));
                            command.Parameters.Add(new SQLiteParameter("@ScannerName", _myHostName));
                            command.Parameters.Add(new SQLiteParameter("@ScannerIP", _myIP));
                            command.Parameters.Add(new SQLiteParameter("@Date", _Date));
                            command.Parameters.Add(new SQLiteParameter("@Time", _Time));
                            try { command.ExecuteNonQuery(); } catch { };
                        }
                    }
                    else if (!s[1].Trim().ToLower().Contains("unknow") && TempComputerName.Trim().Length == 0)
                    {
                        TempComputerIP = s[2].Trim();
                        TempComputerModel = s[4].Trim();
                        TempComputerSN = s[5].Trim();
                        TempLogOnUser = s[6].Trim();

                        using (SQLiteCommand command = new SQLiteCommand("INSERT INTO AliveHosts ('ComputerName', 'ComputerNameShort', 'ComputerDomainName', 'ComputerIP', 'ComputerModel', 'ComputerSN', 'LogOnUser', 'ScannerName', 'ScannerIP', 'Date', 'Time') VALUES (@ComputerName, @ComputerNameShort, @ComputerDomainName, @ComputerIP, @ComputerModel, @ComputerSN, @LogOnUser, @ScannerName, @ScannerIP, @Date, @Time)", connection))
                        {
                            command.Parameters.Add(new SQLiteParameter("@ComputerName", s[0].Trim()));
                            command.Parameters.Add(new SQLiteParameter("@ComputerNameShort", s[1].Trim()));
                            command.Parameters.Add(new SQLiteParameter("@ComputerIP", TempComputerIP));
                            command.Parameters.Add(new SQLiteParameter("@ComputerDomainName", TempComputerDomainName));
                            command.Parameters.Add(new SQLiteParameter("@ComputerModel", TempComputerModel));
                            command.Parameters.Add(new SQLiteParameter("@ComputerSN", TempComputerSN));
                            command.Parameters.Add(new SQLiteParameter("@LogOnUser", TempLogOnUser));
                            command.Parameters.Add(new SQLiteParameter("@ScannerName", _myHostName));
                            command.Parameters.Add(new SQLiteParameter("@ScannerIP", _myIP));
                            command.Parameters.Add(new SQLiteParameter("@Date", _Date));
                            command.Parameters.Add(new SQLiteParameter("@Time", _Time));
                            try { command.ExecuteNonQuery(); } catch { };
                        }
                    }
                }

                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection))
                    {
                        command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                        command.Parameters.AddWithValue("@Name", "ComputerIP");
                        command.Parameters.AddWithValue("@Value", _currentHostIP);
                        command.Parameters.AddWithValue("@Value1", _currentHostName);
                        command.Parameters.AddWithValue("@Date", _Date);
                        command.Parameters.AddWithValue("@Time", _Time);
                        try { command.ExecuteNonQuery(); } catch { }
                    }
                }

                share.Disconnect();
                Task.Delay(1000).Wait();


                if (!_GlobalStopSearch)
                    await Task.Run(() => _ReadRemoteRegistry(_currentHostName, _currentHostIP, _User, _Password));
                _ProgressWork5();

                if (!_GlobalStopSearch)
                    await Task.Run(() => _ReadRemotePCNet(_currentHostName, _currentHostIP, _User, _Password));
                _ProgressWork5();

                if (!_GlobalStopSearch)
                    await Task.Run(() => DisplayEventLogPropertiesRemotePC(_currentHostName, _currentHostIP, _User, _Password));
                _ProgressWork5();

                if (_currentRemoteRegistryRunning & !_GlobalStopSearch)
                {
                    await Task.Run(() => _GetRemoteNameLogs(_currentHostName));
                    foreach (string NameLog in _nameLogs)
                    {
                        if (NameLog != null && NameLog.Length > 1 && !_GlobalStopSearch)
                            await Task.Run(() => _ReadRemotePCEventLog(_currentHostName, _currentHostIP, NameLog, _User, _Password));
                    }
                }
                else if (!_currentRemoteRegistryRunning & !_GlobalStopSearch)
                {
                    await Task.Run(() => _ReadRemotePCFullEventLogs(_currentHostName, _currentHostIP, _User, _Password));
                }
                _ProgressWork5();

                timer1Stop();
                _RunTimer = false;
                Task.WaitAll();
                Task.Delay(500).Wait();
                share.Disconnect();  // DisConnect to the remote drive
            }

            else //local PC
            {
                if (!_GlobalStopSearch)
                    await Task.Run(() => _LocalConfigFromWMI());
                _GetNameLogs();
                if (!_GlobalStopSearch)
                    await Task.Run(() => GetLogPropertiesServicesProcess());
                if (!_GlobalStopSearch)
                    await Task.Run(() => _ReadUsbfromLocalRegistry());
                await Task.Run(() => _GetUsersName());
                foreach (string NameLog in _nameLogs)
                {
                    if (NameLog != null && NameLog.Length > 1 && !_GlobalStopSearch)
                        await Task.Run(() => _loadlogEventLog(NameLog));
                }
                if (!_GlobalStopSearch)
                    await Task.Run(() => _loadlogEventLog("", "1"));
                Task.Delay(500).Wait();
            }
            await Task.WhenAll();
            Task.Delay(2000).Wait();

            // Scan Registry
            ProgressBar1.Value = 0;
            StatusLabel2.Text = "Настраиваю соединение... ";

            Task.Delay(500).Wait();
            if (!_GlobalStopSearch)
                await Task.Run(() => _loadRegDocuments());
            Task.WaitAll();

            // Поиск файлов на разделах хоста
            DirectoryInfo di = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\ready\\tmpfile.stop");
            if (File.Exists(di.FullName)) { try { File.Delete(di.FullName); } catch { } }
            DirectoryInfo di1 = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\tmp\\tmpfile.stop");
            if (File.Exists(di1.FullName)) { try { File.Delete(di1.FullName); } catch { } }

            Task.Delay(500).Wait();
            ProgressBar1.Value = 0;
            timer1Start();
            _GetTimeRunScan();
            t = new Thread(new ThreadStart(_StatusLabel2ChangeColor));
            t.Start();
            StatusLabel2.Text = "Настраиваю соединение... ";

            Thread t1 = new Thread(new ParameterizedThreadStart((obj) => SearchFilesOnLogicalDisks(_currentHostName)));
            t1.Start();

            Thread t2 = new Thread(new ThreadStart(_CheckFoundFiles));
            t2.Start();
            Task.Delay(5000).Wait();

            while (t1.IsAlive)  //Проверить корректность условия
            { Task.Delay(1000).Wait(); }
            share.Disconnect();
        }
        //-------------/\/\/\/\------------// FullScan. End of the block //-------------/\/\/\/\------------// 



        private async void loadRemoteLogsItem_Click(object sender, EventArgs e) //"Загрузить информацию с удаленного ПК"
        {
            ProgressBar1.Value = 0;
            waitFile = new AutoResetEvent(false);
            waitStop = new AutoResetEvent(false);
            waitFilePing = new AutoResetEvent(false);
            _StopScanLogEvents = false;
            _currentRemoteRegistryRunning = true;
            _StopScannig = false;
            StopSearchItem.Enabled = true;
            timer1Start();
            Thread t = new Thread(new ThreadStart(_StatusLabel2ChangeColor));
            t.Start();

            textBoxToTemporary();
            if (tabControl.SelectedTab == tabDataGrid)
            { bool str = ParseSelectedCell(); }
            else
            { ParseTextboxInputNetOrPC(); }

            Task.Delay(200).Wait();

            Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));

            if (_currentHostIP != _myIP) //Remote PC
            {
                StatusLabel2.Text = "Проверяю доступность хоста... ";


                //----- Can Use for PING and other command by the selected host -----//

                NetworkShare share = new NetworkShare(_currentHostName, "ipc$", _User, _Password);
                share.Connect();
                Task.Delay(200).Wait();
                checkRunningRemoteRegistry(_currentHostName, _currentHostIP, _User, _Password); //Лучше использовать IP
                _ProgressWork5();

                StatusLabel2.Text = "Готовлю базу... ";
                FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
                databaseHost = fi.FullName;
                _DBCheckFull(_currentHostName);

                IPAddress addr = IPAddress.Parse(_currentHostIP);
                _getUserAndSN(_currentHostName, addr);
                _GetTimeRunScan();
                string[] s = Regex.Split(TempConfigSingleHost, @"[|]");

                Task.Delay(200).Wait();
                StatusLabel2.Text = "Обновляю данные в базе... ";
                string TempComputerName = "", TempComputerIP = "", TempComputerModel = "", TempComputerSN = "", TempLogOnUser = "", TempComputerNameShort = "", TempComputerDomainName = "";
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}; journal_mode =MEMORY;", databaseAliveHosts)))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand("SELECT ComputerName, ComputerNameShort, ComputerDomainName, ComputerIP, ComputerModel, ComputerSN, LogOnUser, Date FROM AliveHosts Where ComputerName like '%" + _currentHostName + "%' ;", connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            foreach (DbDataRecord record in reader)
                            {
                                TempComputerName = record["ComputerName"].ToString();
                                TempComputerIP = record["ComputerIP"].ToString();
                                TempComputerModel = record["ComputerModel"].ToString();
                                TempComputerSN = record["ComputerSN"].ToString();
                                TempLogOnUser = record["LogOnUser"].ToString();
                                TempComputerNameShort = record["ComputerNameShort"].ToString();
                                TempComputerDomainName = record["ComputerDomainName"].ToString();
                            }
                        }
                    }

                    if (!s[1].Trim().ToLower().Contains("unknow") && TempComputerName.Trim().Length > 0)
                    {
                        TempComputerIP = s[2].Trim();
                        if (!s[4].ToLower().Contains("permission") || !s[5].ToLower().Contains("permission"))
                        {
                            TempComputerModel = s[4].Trim();
                            TempComputerSN = s[5].Trim();
                            TempLogOnUser = s[6].Trim();
                        }

                        using (SQLiteCommand command = new SQLiteCommand("UPDATE AliveHosts SET ComputerNameShort=@ComputerNameShort, ComputerDomainName=@ComputerDomainName, ComputerIP=@ComputerIP, ComputerModel=@ComputerModel," +
                                " ComputerSN=@ComputerSN, LogOnUser=@LogOnUser, ScannerName=@ScannerName, ScannerIP=@ScannerIP, Date=@Date, Time=@Time WHERE ComputerName=@ComputerName LIMIT 1", connection))
                        {
                            command.Parameters.Add(new SQLiteParameter("@ComputerNameShort", s[1].Trim()));
                            command.Parameters.Add(new SQLiteParameter("@ComputerIP", TempComputerIP));
                            command.Parameters.Add(new SQLiteParameter("@ComputerDomainName", TempComputerDomainName));
                            command.Parameters.Add(new SQLiteParameter("@ComputerModel", TempComputerModel));
                            command.Parameters.Add(new SQLiteParameter("@ComputerSN", TempComputerSN));
                            command.Parameters.Add(new SQLiteParameter("@LogOnUser", TempLogOnUser));
                            command.Parameters.Add(new SQLiteParameter("@ScannerName", _myHostName));
                            command.Parameters.Add(new SQLiteParameter("@ScannerIP", _myIP));
                            command.Parameters.Add(new SQLiteParameter("@Date", _Date));
                            command.Parameters.Add(new SQLiteParameter("@Time", _Time));
                            try { command.ExecuteNonQuery(); } catch { };
                        }
                    }
                    else if (!s[1].Trim().ToLower().Contains("unknow") && TempComputerName.Trim().Length == 0)
                    {
                        TempComputerIP = s[2].Trim();
                        TempComputerModel = s[4].Trim();
                        TempComputerSN = s[5].Trim();
                        TempLogOnUser = s[6].Trim();

                        using (SQLiteCommand command = new SQLiteCommand("INSERT INTO AliveHosts ('ComputerName', 'ComputerNameShort', 'ComputerDomainName', 'ComputerIP', 'ComputerModel', 'ComputerSN', 'LogOnUser', 'ScannerName', 'ScannerIP', 'Date', 'Time') VALUES (@ComputerName, @ComputerNameShort, @ComputerDomainName, @ComputerIP, @ComputerModel, @ComputerSN, @LogOnUser, @ScannerName, @ScannerIP, @Date, @Time)", connection))
                        {
                            command.Parameters.Add(new SQLiteParameter("@ComputerName", s[0].Trim()));
                            command.Parameters.Add(new SQLiteParameter("@ComputerNameShort", s[1].Trim()));
                            command.Parameters.Add(new SQLiteParameter("@ComputerIP", TempComputerIP));
                            command.Parameters.Add(new SQLiteParameter("@ComputerDomainName", TempComputerDomainName));
                            command.Parameters.Add(new SQLiteParameter("@ComputerModel", TempComputerModel));
                            command.Parameters.Add(new SQLiteParameter("@ComputerSN", TempComputerSN));
                            command.Parameters.Add(new SQLiteParameter("@LogOnUser", TempLogOnUser));
                            command.Parameters.Add(new SQLiteParameter("@ScannerName", _myHostName));
                            command.Parameters.Add(new SQLiteParameter("@ScannerIP", _myIP));
                            command.Parameters.Add(new SQLiteParameter("@Date", _Date));
                            command.Parameters.Add(new SQLiteParameter("@Time", _Time));
                            try { command.ExecuteNonQuery(); } catch { };
                        }
                    }
                }

                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection))
                    {
                        command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                        command.Parameters.AddWithValue("@Name", "ComputerIP");
                        command.Parameters.AddWithValue("@Value", _currentHostIP);
                        command.Parameters.AddWithValue("@Value1", _currentHostName);
                        command.Parameters.AddWithValue("@Date", _Date);
                        command.Parameters.AddWithValue("@Time", _Time);
                        try { command.ExecuteNonQuery(); } catch { }
                    }
                }

                if (!_StopScanLogEvents)
                    await Task.Run(() => _ReadRemoteRegistry(_currentHostName, _currentHostIP, _User, _Password));
                _ProgressWork5();

                if (!_StopScanLogEvents)
                    await Task.Run(() => _ReadRemotePCNet(_currentHostName, _currentHostIP, _User, _Password));
                _ProgressWork5();

                if (!_StopScanLogEvents)
                    await Task.Run(() => DisplayEventLogPropertiesRemotePC(_currentHostName, _currentHostIP, _User, _Password));
                _ProgressWork5();

                if (_currentRemoteRegistryRunning && !_StopScanLogEvents)
                {
                    await Task.Run(() => _GetRemoteNameLogs(_currentHostName));
                    foreach (string NameLog in _nameLogs)
                    {
                        if (NameLog != null && NameLog.Length > 1 && !_StopScanLogEvents)
                            await Task.Run(() => _ReadRemotePCEventLog(_currentHostName, _currentHostIP, NameLog, _User, _Password));
                    }
                }
                else
                {
                    if (!_StopScanLogEvents)
                        await Task.Run(() => _ReadRemotePCFullEventLogs(_currentHostName, _currentHostIP, _User, _Password));
                }
                _ProgressWork5();

                timer1Stop();
                _RunTimer = false;
                Task.WaitAll();
                Task.Delay(500).Wait();
                share.Disconnect();  // DisConnect to the remote drive
            }
            else //local PC
            {
                if (!_StopScanLogEvents)
                    await Task.Run(() => _GetUsersName());
                if (!_StopScanLogEvents)
                    await Task.Run(() => _LocalConfigFromWMI());
                _GetNameLogs();
                if (!_StopScanLogEvents)
                    await Task.Run(() => GetLogPropertiesServicesProcess());
                if (!_StopScanLogEvents)
                    await Task.Run(() => _ReadUsbfromLocalRegistry());
                foreach (string NameLog in _nameLogs)
                {
                    if (NameLog != null && NameLog.Length > 1 && !_StopScanLogEvents)
                    { await Task.Run(() => _loadlogEventLog(NameLog)); }
                }
                if (!_StopScanLogEvents)
                    await Task.Run(() => _loadlogEventLog("", "1"));
                timer1Stop();
                _RunTimer = false;
                Task.Delay(500).Wait();
            }
            await Task.WhenAll();
            ProgressBar1.Value = 100;

            if (!_StopScanLogEvents && _Authorized)
            {
                StatusLabel2.ForeColor = System.Drawing.Color.Black;
                StatusLabel2.Text = "Завершен сбор событий и параметров конфигурации ";
            }

            else if (!_StopScanLogEvents && !_Authorized)
            {
                StatusLabel2.ForeColor = System.Drawing.Color.Crimson;
                StatusLabel2.Text = "Cбор событий и параметров конфигурации завершен с ошибками";
            }
        }

        private void _ReadRemotePCEventLog(string HostName, string HostIP, string NameEventLog, string userLogin, string userPassword) // звменить loadlog!!! не все события  из SYSTEMlog
        {
            _StatusLabel2Text("Настраиваю соединение с хостом... ");

            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + HostName + @".db");
            databaseHost = fi.FullName;

            string a2 = "", a3 = "", a4 = "", a5 = "", a6 = "", a61 = "", a7 = "", a8 = "", a9 = "", a10 = "", a11 = "", a12 = "", query = "";
            string dateTime = getDmtfFromDateTime(DateTime.Today.Subtract(new TimeSpan(1, 0, 0, 0)));
            if (File.Exists(fi.FullName))
            {
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
                {
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand();


                    /*      Второй подход. 
                    //https://support.microsoft.com/EN-US/help/815314 
                    Проверить!!!!

                    string logType = "Application";
                    EventLog ev = new EventLog(logType, HostName);
                    int CountLogEntries = ev.Entries.Count;
                    if ( CountLogEntries <= 0 )
                    Console.WriteLine("No Event Logs in the Log :" + logType);

                    // Read the last 2 records in the specified log. 
                    int i;
                    for ( i = ev.Entries.Count - 1; i>= LastLogToShow - 2; i--)
                    {
                    EventLogEntry CurrentEntry = ev.Entries[i];
                    Console.WriteLine("Event ID : " + CurrentEntry.EventID+" Entry Type : " + CurrentEntry.EntryType.ToString() + " Message :  " + CurrentEntry.Message + "\n");
                    }
                    ev.Close();
                    */

                    ConnectionOptions co = new ConnectionOptions();   //Connect to WMI of the Remote Host 
                    co.Impersonation = ImpersonationLevel.Impersonate;
                    co.Authentication = AuthenticationLevel.Packet;
                    co.EnablePrivileges = true;
                    co.Timeout = new TimeSpan(0, 0, 1200);

                    co.Username = textBoxLoginText;
                    co.Password = _Password;
                    //                        co.Locale = "MS_409";
                    co.Authority = "ntlmdomain:" + textBoxDomainText;
                    ManagementScope scope = new ManagementScope("\\\\" + HostName + "\\root\\cimv2", co);
                    scope.Connect();

                    ObjectQuery q = new ObjectQuery("Select * FROM Win32_NTLogEvent WHERE Logfile='" + NameEventLog + "'");
                    ManagementObjectSearcher mbs = new ManagementObjectSearcher(scope, q);
                    ManagementObjectCollection mbsList = mbs.Get();

                    bool isConnected = scope.IsConnected;

                    if (isConnected)
                    {
                        _StatusLabel2Text("Загружаю журнал " + NameEventLog + "... ");

                        command = new SQLiteCommand("begin", connection);
                        command.ExecuteNonQuery();

                        try
                        {
                            foreach (ManagementObject mo in mbsList)
                            {
                                if (mo["Logfile"] != null && !_StopScanLogEvents && !_GlobalStopSearch)
                                {
                                    a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = ""; a61 = ""; a7 = ""; a8 = ""; a9 = ""; a10 = ""; a11 = ""; a12 = ""; query = "";
                                    try { a9 = mo["EventType"].ToString(); } catch { }
                                    try { a10 = mo["EventIdentifier"].ToString(); } catch { }
                                    try { a11 = mo["Type"].ToString(); } catch { }
                                    try { a12 = mo["CategoryString"].ToString(); } catch { }
                                    try { a2 = mo["RecordNumber"].ToString(); } catch { }
                                    try { a3 = mo["EventCode"].ToString(); } catch { }
                                    try { a4 = mo["SourceName"].ToString(); } catch { }
                                    try { a5 = mo["Category"].ToString(); } catch { }


                                    string[] ax = Regex.Split(TransformObjectToDateTime(mo["TimeWritten"].ToString()), " ");
                                    a6 = ax[0];
                                    a61 = ax[1];

                                    try { a7 = mo["Message"].ToString(); } catch { }
                                    try { a8 = mo["User"].ToString(); } catch { }

                                    query = "INSERT INTO 'EventsLogs' ('ComputerName', 'Logfile', 'EventType', 'EventIdentifier', 'Type', 'CategoryString', 'RecordNumber', 'EventCode', 'SourceName', 'Category', 'Date', 'Time','Message', 'User') " +
                                    " VALUES ('" + HostName + "','" + NameEventLog + "','" + a9 + "','" + a10 + "','" + a11 + "','" + a12 + "','" + a2 + "','" + a3 + "','" + a4 + "','" + a5 + "','" + a6 + "','" + a61 + "','" + a7 + "','" + a8 + "'); ";
                                    command = new SQLiteCommand(query, connection);
                                    try { command.ExecuteNonQuery(); } catch { }
                                }
                            }
                        } catch
                        {
                            command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                     " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                            command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                            command.Parameters.AddWithValue("@Name", "Error");
                            command.Parameters.AddWithValue("@Value", "Log: " + NameEventLog);
                            command.Parameters.AddWithValue("@Value1", "No Access!");
                            command.Parameters.AddWithValue("@Date", _Date);
                            command.Parameters.AddWithValue("@Time", _Time);
                            try { command.ExecuteNonQuery(); } catch { }
                        }
                        command = new SQLiteCommand("end", connection);
                        command.ExecuteNonQuery();
                    }
                    command.Dispose();
                    connection.Close();
                }
            }
            else { MessageBox.Show("Нет файла!\n" + fi.FullName); }
        }

        private void _ReadRemotePCFullEventLogs(string HostName, string HostIP, string userLogin, string userPassword) // звменить loadlog!!! не все события  из SYSTEMlog
        {
            _StatusLabel2Text("Загружаю логи хоста... ");

            string a1 = "", a2 = "", a3 = "", a4 = "", a5 = "", a6 = "", a61 = "", a7 = "", a8 = "", a9 = "", a10 = "", a11 = "", a12 = "", query = "";
            string dateTime = getDmtfFromDateTime(DateTime.Today.Subtract(new TimeSpan(1, 0, 0, 0)));

            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
            databaseHost = fi.FullName;

            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}; page_size = 65536; cache_size =100000; journal_mode =MEMORY;", databaseHost)))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand();
                command = new SQLiteCommand("begin", connection);
                command.ExecuteNonQuery();

                ConnectionOptions co = new ConnectionOptions();   //Connect to WMI of the Remote Host 
                co.Impersonation = ImpersonationLevel.Impersonate;
                co.Authentication = AuthenticationLevel.Packet;
                co.EnablePrivileges = true;
                co.Timeout = new TimeSpan(0, 0, 1200);

                co.Username = textBoxLoginText;
                co.Password = _Password;
                //                        co.Locale = "MS_409";
                co.Authority = "ntlmdomain:" + textBoxDomainText;
                try
                {
                    ManagementScope scope = new ManagementScope("\\\\" + HostName + "\\root\\cimv2", co);
                    scope.Connect();

                    ObjectQuery q = new ObjectQuery("Select * FROM Win32_NTLogEvent ");
                    ManagementObjectSearcher mbs = new ManagementObjectSearcher(scope, q);
                    ManagementObjectCollection mbsList = mbs.Get();

                    bool isConnected = scope.IsConnected;
                    if (isConnected)
                    {
                        try
                        {
                            foreach (ManagementObject mo in mbsList)
                            {
                                if (mo["Logfile"] != null && _StopScanLogEvents == false)
                                {
                                    a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = ""; a61 = ""; a7 = ""; a8 = ""; a9 = ""; a10 = ""; a11 = ""; a12 = ""; query = "";
                                    try { a1 = mo["Logfile"].ToString(); } catch { }
                                    try { a9 = mo["EventType"].ToString(); } catch { }
                                    try { a10 = mo["EventIdentifier"].ToString(); } catch { }
                                    try { a11 = mo["Type"].ToString(); } catch { }
                                    try { a12 = mo["CategoryString"].ToString(); } catch { }
                                    try { a2 = mo["RecordNumber"].ToString(); } catch { }
                                    try { a3 = mo["EventCode"].ToString(); } catch { }
                                    try { a4 = mo["SourceName"].ToString(); } catch { }
                                    try { a5 = mo["Category"].ToString(); } catch { }

                                    string[] ax = Regex.Split(TransformObjectToDateTime(mo["TimeWritten"].ToString()), " ");
                                    a6 = ax[0];
                                    a61 = ax[1];

                                    try { a7 = mo["Message"].ToString(); } catch { }
                                    try { a8 = mo["User"].ToString(); } catch { }

                                    query = "INSERT INTO 'EventsLogs' ('ComputerName', 'Logfile', 'EventType', 'EventIdentifier', 'Type', 'CategoryString', 'RecordNumber', 'EventCode', 'SourceName', 'Category', 'Date', 'Time','Message', 'User') " +
                                    " VALUES ('" + HostName + "','" + a1 + "','" + a9 + "','" + a10 + "','" + a11 + "','" + a12 + "','" + a2 + "','" + a3 + "','" + a4 + "','" + a5 + "','" + a6 + "','" + a61 + "','" + a7 + "','" + a8 + "'); ";
                                    command = new SQLiteCommand(query, connection);
                                    try { command.ExecuteNonQuery(); } catch { }
                                }
                            }
                        } catch
                        {
                            command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                     " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                            command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                            command.Parameters.AddWithValue("@Name", "Error");
                            command.Parameters.AddWithValue("@Value", "_ReadRemotePCFullEventLogs");
                            command.Parameters.AddWithValue("@Value1", "Win32_NTLogEvent");
                            command.Parameters.AddWithValue("@Date", _Date);
                            command.Parameters.AddWithValue("@Time", _Time);
                            try { command.ExecuteNonQuery(); } catch { }
                        }
                    }
                } catch (Exception exp)
                {
                    command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
     " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                    command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                    command.Parameters.AddWithValue("@Name", "Error");
                    command.Parameters.AddWithValue("@Value", "_ReadRemotePCFullEventLogs");
                    command.Parameters.AddWithValue("@Value1", exp.ToString());
                    command.Parameters.AddWithValue("@Date", _Date);
                    command.Parameters.AddWithValue("@Time", _Time);
                    try { command.ExecuteNonQuery(); } catch { }
                }
                command = new SQLiteCommand("end", connection);
                command.ExecuteNonQuery();

                command.Dispose();
                connection.Close();
            }
        }

        private void GetLogPropertiesServicesProcess()
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
            databaseHost = fi.FullName;

            string a1 = "", a2 = "", a3 = "", query = ""; int aa1 = 0;
            _TempLogonUser = ""; _TempModel = "";
            SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand("begin", connection);
            command.ExecuteNonQuery();

            if (_StopScanLogEvents == false)
            {
                System.Diagnostics.EventLog[] eventLogs = System.Diagnostics.EventLog.GetEventLogs();
                if (_Authorized)
                {
                    foreach (System.Diagnostics.EventLog e in eventLogs)
                    {
                        if (e != null)
                        {
                            try
                            {
                                long sizeKB = 0;   // Determine if there is an event log file for this event log.
                                using (RegistryKey regEventLog = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Services\EventLog\" + e.Log, RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey))
                                {
                                    if (regEventLog != null)
                                    {
                                        object temp = regEventLog.GetValue("File");
                                        if (temp != null)
                                        {
                                            FileInfo file = new FileInfo(temp.ToString());
                                            if (file.Exists)  // Get the current size of the event log file.
                                            {
                                                sizeKB = file.Length / 1024;
                                                if ((file.Length % 1024) != 0)
                                                { sizeKB++; }
                                                query = " INSERT INTO 'WindowsFeature' ('Name', 'Value', 'Value1' ,'ComputerName', 'Date', 'Time') VALUES ('Log: " + e.Log + "','Events: " + e.Entries.Count.ToString() + "/Size, kB: " + sizeKB.ToString() + "','Path: " + temp.ToString() + "','" + _currentHostName + "','" + _Date + "','" + _Time + "'); ";
                                                command = new SQLiteCommand(query, connection);
                                                try { command.ExecuteNonQuery(); } catch { }
                                                _ProgressWork1();
                                            }
                                        }
                                    }
                                    //                                            regEventLog.Close();
                                }
                            } catch
                            {
                                command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                                command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                                command.Parameters.AddWithValue("@Name", "Error");
                                command.Parameters.AddWithValue("@Value", "Get NameOfLogs");
                                command.Parameters.AddWithValue("@Value1", "No Access Registry!");
                                command.Parameters.AddWithValue("@Date", _Date);
                                command.Parameters.AddWithValue("@Time", _Time);
                                try { command.ExecuteNonQuery(); } catch { }
                            }

                        }
                    }
                }
            }

            System.ServiceProcess.ServiceController[] services;
            if (_StopScanLogEvents == false)
            {
                services = System.ServiceProcess.ServiceController.GetServices();
                aa1 = 0;
                for (int i = 0; i < services.Length; i++)
                {
                    if (services[i] != null)
                    {
                        try
                        {
                            a1 = ""; a2 = ""; a3 = ""; query = "";
                            a1 = services[i].ServiceName;
                            a2 = services[i].DisplayName;
                            a3 = services[i].Status.ToString();
                            aa1++;
                            query = "INSERT INTO 'Services' ('ServicesId', 'ComputerName', 'ServiceName', 'DisplayName' ,'Status', 'Date', 'Time')" +
                                " VALUES ('" + aa1 + "','" + _myHostName + "','" + a1 + "','" + a2 + "','" + a3 + "','" + _Date + "','" + _Time + "'); ";
                            command = new SQLiteCommand(query, connection);
                            try { command.ExecuteNonQuery(); } catch { }
                        } catch
                        {
                            command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                            command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                            command.Parameters.AddWithValue("@Name", "Error");
                            command.Parameters.AddWithValue("@Value", "GetService " + services[i]);
                            command.Parameters.AddWithValue("@Value1", "No Access!");
                            command.Parameters.AddWithValue("@Date", _Date);
                            command.Parameters.AddWithValue("@Time", _Time);
                            try { command.ExecuteNonQuery(); } catch { }
                        }
                    }
                    _ProgressWork1();
                }
            }

            if (_StopScanLogEvents == false)
            {
                services = System.ServiceProcess.ServiceController.GetDevices();
                aa1 = 0; query = "";
                for (int i = 0; i < services.Length; i++)
                {
                    if (services[i] != null)
                    {
                        try
                        {
                            a1 = ""; a2 = ""; a3 = "";
                            a1 = services[i].ServiceName;
                            a2 = services[i].DisplayName;
                            a3 = services[i].Status.ToString();
                            aa1++;
                            query = " INSERT INTO 'Services' ('ServicesId', 'ComputerName', 'ServiceName', 'DisplayName' ,'Status', 'Date', 'Time')" +
                                " VALUES ('" + aa1 + "','" + _myHostName + "','" + a1 + "','" + a2 + "','" + a3 + "','" + _Date + "','" + _Time + "'); ";
                            command = new SQLiteCommand(query, connection);
                            try { command.ExecuteNonQuery(); } catch { }
                        } catch
                        {
                            command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                            command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                            command.Parameters.AddWithValue("@Name", "Error");
                            command.Parameters.AddWithValue("@Value", "GetDevice " + services[i]);
                            command.Parameters.AddWithValue("@Value1", "No Access!");
                            command.Parameters.AddWithValue("@Date", _Date);
                            command.Parameters.AddWithValue("@Time", _Time);
                            try { command.ExecuteNonQuery(); } catch { }
                        }
                    }
                    _ProgressWork1();
                }
            }

            if (_StopScanLogEvents == false)
            {
                System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcesses();
                aa1 = 0;
                for (int i = 0; i < procs.Length; i++)
                {
                    if (procs[i] != null)
                    {
                        try
                        {
                            a1 = ""; a2 = "";
                            a1 = procs[i].ProcessName;
                            a2 = procs[i].Id.ToString();
                            aa1++;
                            query = "INSERT INTO 'Process' ('ProcessId', 'ComputerName', 'Name', 'iDProcess', 'Date', 'Time')" +
                                " VALUES ('" + aa1 + "','" + _myHostName + "','" + a1 + "','" + a2 + "','" + _Date + "','" + _Time + "'); ";
                            command = new SQLiteCommand(query, connection);
                            try { command.ExecuteNonQuery(); } catch { }
                        } catch
                        {
                            command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                            command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                            command.Parameters.AddWithValue("@Name", "Warning");
                            command.Parameters.AddWithValue("@Value", "GetProcess " + procs[i]);
                            command.Parameters.AddWithValue("@Value1", "No Access!");
                            command.Parameters.AddWithValue("@Date", _Date);
                            command.Parameters.AddWithValue("@Time", _Time);
                            try { command.ExecuteNonQuery(); } catch { }
                        }
                    }
                    _ProgressWork1();
                }
            }

            command = new SQLiteCommand("end", connection);
            command.ExecuteNonQuery();

            command.Dispose();
            connection.Close();
        }

        private void _GetNameLogs()
        {
            HashSet<String> NamesOfLogs = new HashSet<string>(); //Temporary list - for remove dublicate from list
            _nameLogs.Initialize();

            string query = "";

            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
            databaseHost = fi.FullName;


            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand("begin", connection);
                command.ExecuteNonQuery();

                System.Diagnostics.EventLog[] eventLogs = System.Diagnostics.EventLog.GetEventLogs();
                foreach (System.Diagnostics.EventLog e in eventLogs)
                {
                    if (e != null)
                    {
                        Int64 sizeKB = 0; // Determine if there is an event log file for this event log.
                        try
                        {
                            using (RegistryKey regEventLog = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Services\\EventLog\\" + e.Log))
                            {
                                if (regEventLog != null)
                                {
                                    Object temp = regEventLog.GetValue("File");
                                    if (temp != null)
                                    {
                                        FileInfo file = new FileInfo(temp.ToString());
                                        if (Convert.ToInt32(e.Entries.Count.ToString()) > 0)
                                        {
                                            query = " INSERT INTO 'WindowsFeature' ('Name', 'Value', 'Value1' ,'ComputerName', 'Date', 'Time') VALUES ('Log: " + e.Log + "','Events: " + e.Entries.Count.ToString() + "/Size, kB: " + sizeKB.ToString() + "','Path: " + temp.ToString() + "','" + _currentHostName + "','" + _Date + "','" + _Time + "'); ";
                                            command = new SQLiteCommand(query, connection);
                                            try { command.ExecuteNonQuery(); } catch { }
                                        }
                                    }
                                }
                            }
                        } catch
                        {
                            command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                            command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                            command.Parameters.AddWithValue("@Name", "Error");
                            command.Parameters.AddWithValue("@Value", "GetEventNameLogs");
                            command.Parameters.AddWithValue("@Value1", "No Access!");
                            command.Parameters.AddWithValue("@Date", _Date);
                            command.Parameters.AddWithValue("@Time", _Time);
                            try { command.ExecuteNonQuery(); } catch { }
                            _Authorized = false;
                        }
                    }
                }

                command = new SQLiteCommand("end", connection);
                command.ExecuteNonQuery();

                command = new SQLiteCommand("SELECT Name FROM 'WindowsFeature' Where ComputerName like '%" + _currentHostName + "%' AND Value1 like 'LogName';", connection);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    try
                    {
                        foreach (DbDataRecord record in reader)
                        {
                            if (record != null)
                            { NamesOfLogs.Add(record["Name"].ToString()); }
                        }
                    } catch { }
                }
                command.Dispose();
            }

            //Transfer names of logs from HashSet into the global array "_nameLogs";
            int k = 0;
            string[] OnlyOneNameOfLog = NamesOfLogs.ToArray();
            foreach (string namesLog in OnlyOneNameOfLog)
            {
                _nameLogs[k] = namesLog;
                k++;
            }
        }

        private void _GetUsersName()
        {
            _StatusLabel2Text("Определяю перечень пользователей... ");

            string a1 = "", a2 = "", a3 = "", query = "";

            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
            databaseHost = fi.FullName;

            SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand();

            string UserProfiles = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\";
            using (RegistryKey regUseProfilesrKey = Registry.LocalMachine.OpenSubKey(UserProfiles, RegistryKeyPermissionCheck.ReadSubTree))
            {
                string[] subNames = regUseProfilesrKey.GetSubKeyNames();

                command = new SQLiteCommand("begin", connection);
                command.ExecuteNonQuery();

                foreach (string s in subNames)                                                         //Get key of USB
                {
                    if (s != null)
                        using (RegistryKey profilekey = regUseProfilesrKey.OpenSubKey(s, RegistryKeyPermissionCheck.ReadSubTree))
                        {
                            foreach (string valueName in profilekey.GetValueNames())     //Get Name of USB
                            {
                                a1 = ""; a2 = ""; query = "";
                                a1 = valueName.ToString();
                                if (a1.Contains("ProfileImagePath") && s.ToString().Length > 10)
                                {
                                    a2 = s.ToString();
                                    a3 = profilekey.GetValue(valueName).ToString();
                                    query =
                                   "INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                   " VALUES ('" + _currentHostName + "','UserNameOnline: ','" + a3 + "','" + a2 + "','" + _Date + "','" + _Time + "'); ";
                                    command = new SQLiteCommand(query, connection);
                                    try { command.ExecuteNonQuery(); } catch { }
                                }
                            }
                        }
                }

                command = new SQLiteCommand("end", connection);
                command.ExecuteNonQuery();
            }
            command.Dispose();
            connection.Close();
        }

        private void _loadlogEventLog(string NameEventLog, string _all = "0") // зaменить loadlog!!! не все события  из SYSTEMlog
        {
            _StatusLabel2Text("Подготавливаю соединение с хостом... ");

            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
            databaseHost = fi.FullName;

            string a1 = "", a2 = "", a3 = "", a4 = "", a5 = "", a6 = "", a61 = "", a7 = "", a8 = "", a9 = "", a10 = "", a11 = "", a12 = "", query = "";

            /* entire day */
            string dateTime = getDmtfFromDateTime(DateTime.Today.Subtract(new TimeSpan(1, 0, 0, 0)));
            //            string dateTime = getDmtfFromDateTime("09/06/2014 17:00:08"); // DateTime specific

            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
            {//PRAGMA journal_mode = DELETE | TRUNCATE | PERSIST | MEMORY | WAL | OFF   // TRUNCATE - перезапись файла //WAL - одновременная запись и чтение в базу
                //PRAGMA synchronous = 0 | OFF | 1 | NORMAL | 2 | FULL
                //http://zametkinapolyah.ru/zametki-o-mysql/sozdanie-tablic-v-bazax-dannyx-sqlite.html
                //http://www.sqlite.org/pragma.html#syntax

                connection.Open();
                SQLiteCommand command = new SQLiteCommand();

                ConnectionOptions co = new ConnectionOptions();
                co.Impersonation = ImpersonationLevel.Impersonate;
                co.Authentication = AuthenticationLevel.Packet;
                co.Timeout = new TimeSpan(0, 0, 600);
                co.EnablePrivileges = true;
                ManagementPath mp = new ManagementPath();
                mp.NamespacePath = @"\root\cimv2";
                ManagementScope ms = new ManagementScope(mp, co);
                ms.Connect();
                ManagementObjectSearcher mbs;
                string q = "Select * FROM Win32_NTLogEvent WHERE Logfile='" + NameEventLog + "'";
                if (_all == "1")
                {
                    int aa = 1;
                    query = "";
                    foreach (string t in _nameLogs)
                    {
                        if (t != null && t.Length > 1)
                        {
                            if (aa == 0)
                            {
                                query += " OR ";
                                aa = 1;
                            }
                            query += " '" + t + "' ";
                            aa = 0;
                        }
                    }
                    q = "Select * FROM Win32_NTLogEvent WHERE Logfile!=" + query;
                }

                mbs = new ManagementObjectSearcher(q);
                ManagementObjectCollection mbsList = mbs.Get();

                _StatusLabel2Text("Загружаю журналы хоста... ");
                command = new SQLiteCommand("begin", connection);
                command.ExecuteNonQuery();

                try
                {
                    foreach (ManagementObject mo in mbsList)
                    {
                        if (mo["Logfile"] != null && _StopScanLogEvents == false)
                        {
                            a1 = ""; a2 = ""; a3 = ""; a4 = ""; a5 = ""; a6 = ""; a61 = ""; a7 = ""; a8 = ""; a9 = ""; a10 = ""; a11 = ""; a12 = ""; query = "";
                            try { a1 = mo["Logfile"].ToString(); } catch { }
                            try { a9 = mo["EventType"].ToString(); } catch { }
                            try { a10 = mo["EventIdentifier"].ToString(); } catch { }
                            try { a11 = mo["Type"].ToString(); } catch { }
                            try { a12 = mo["CategoryString"].ToString(); } catch { }
                            try { a2 = mo["RecordNumber"].ToString(); } catch { }
                            try { a3 = mo["EventCode"].ToString(); } catch { }
                            try { a4 = mo["SourceName"].ToString(); } catch { }
                            try { a5 = mo["Category"].ToString(); } catch { }

                            string[] ax = Regex.Split(TransformObjectToDateTime(mo["TimeWritten"].ToString()), " ");
                            a6 = ax[0];
                            a61 = ax[1];

                            try { a7 = mo["Message"].ToString(); } catch { }
                            try { a8 = mo["User"].ToString(); } catch { }

                            query = "INSERT INTO 'EventsLogs' ('ComputerName', 'Logfile', 'EventType', 'EventIdentifier', 'Type', 'CategoryString', 'RecordNumber', 'EventCode', 'SourceName', 'Category', 'Date', 'Time','Message', 'User') " +
                            " VALUES ('" + _myHostName + "','" + a1 + "','" + a9 + "','" + a10 + "','" + a11 + "','" + a12 + "','" + a2 + "','" + a3 + "','" + a4 + "','" + a5 + "','" + a6 + "','" + a61 + "','" + a7 + "','" + a8 + "'); ";
                            command = new SQLiteCommand(query, connection);
                            try { command.ExecuteNonQuery(); } catch { }
                        }
                    }
                } catch { }

                command = new SQLiteCommand("end", connection);
                command.ExecuteNonQuery();

                command.Dispose();
                connection.Close();
            }
        }

        private void DisplayEventLogPropertiesRemotePC(string HostName, string HostIP, string userLogin, string userPassword)
        {
            _StatusLabel2Text("Проверяю активные сервисы и процессы хоста... ");

            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
            databaseHost = fi.FullName;

            string a1 = "", a2 = "", a3 = "", query = "";
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand("", connection);
                if (_currentRemoteRegistryRunning == true)
                {
                    try
                    {
                        System.Diagnostics.EventLog[] eventLogs = System.Diagnostics.EventLog.GetEventLogs(HostName);
                        foreach (System.Diagnostics.EventLog e in eventLogs)
                        {
                            Int64 sizeKB = 0;
                            using (RegistryKey key1 = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, HostName))
                            {
                                try
                                {
                                    using (RegistryKey key = key1.OpenSubKey("System\\CurrentControlSet\\Services\\EventLog\\" + e.Log))
                                    {
                                        try
                                        {
                                            Object temp = key.GetValue("File");
                                            if (temp != null)
                                            {
                                                FileInfo file = new FileInfo(temp.ToString());
                                                if (file.Exists)  // Get the current size of the event log file.
                                                {
                                                    sizeKB = file.Length / 1024;
                                                    if ((file.Length % 1024) != 0)
                                                    { sizeKB++; }
                                                    query = " INSERT INTO 'WindowsFeature' ('Name', 'Value' ,'ComputerName', 'Date', 'Time') " +
                                                            "VALUES ('Log " + e.LogDisplayName + " has events:','" + e.Entries.Count.ToString() + "','" + _myHostName + "','" + _Date + "','" + _Time + "'); " +
                                                            " INSERT INTO 'WindowsFeature' ('Name', 'Value' ,'ComputerName', 'Date', 'Time') " +
                                                            "VALUES ('Log " + e.Log + " has size, kB:','" + sizeKB.ToString() + "','" + _myHostName + "','" + _Date + "','" + _Time + "'); " +
                                                            " INSERT INTO 'WindowsFeature' ('Name', 'Value' ,'ComputerName', 'Date', 'Time') " +
                                                            "VALUES ('Log " + e.Log + " has Maximum size, kB:','" + e.MaximumKilobytes + "','" + _myHostName + "','" + _Date + "','" + _Time + "'); " +
                                                            " INSERT INTO 'WindowsFeature' ('Name', 'Value' ,'ComputerName', 'Date', 'Time') " +
                                                            "VALUES ('Log " + e.Log + " at:','" + temp.ToString() + " kB','" + _myHostName + "','" + _Date + "','" + _Time + "'); ";
                                                    command = new SQLiteCommand(query, connection);
                                                    try { command.ExecuteNonQuery(); } catch { }
                                                }
                                            }
                                        } catch { }
                                    }
                                } catch { }
                            }
                        }
                    } catch
                    {
                        try
                        {
                            System.Diagnostics.EventLog[] eventLogs = System.Diagnostics.EventLog.GetEventLogs(HostName);
                            foreach (System.Diagnostics.EventLog e in eventLogs)
                            {
                                Int64 sizeKB = 0;
                                using (RegistryKey key1 = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, HostName))
                                {
                                    try
                                    {
                                        using (RegistryKey key = key1.OpenSubKey("System\\CurrentControlSet\\Services\\EventLog\\" + e.Log, RegistryKeyPermissionCheck.ReadSubTree))
                                        {
                                            try
                                            {
                                                Object temp = key.GetValue("File");
                                                if (temp != null)
                                                {
                                                    FileInfo file = new FileInfo(temp.ToString());
                                                    if (file.Exists == true)  // Get the current size of the event log file.
                                                    {
                                                        sizeKB = file.Length / 1024;
                                                        if ((file.Length % 1024) != 0)
                                                        { sizeKB++; }
                                                        query = " INSERT INTO 'WindowsFeature' ('Name', 'Value' ,'ComputerName', 'Date', 'Time') " +
                                                                "VALUES ('Log " + e.LogDisplayName + " has events:','" + e.Entries.Count.ToString() + "','" + _myHostName + "','" + _Date + "','" + _Time + "'); " +
                                                                " INSERT INTO 'WindowsFeature' ('Name', 'Value' ,'ComputerName', 'Date', 'Time') " +
                                                                "VALUES ('Log " + e.Log + " has size, kB:','" + sizeKB.ToString() + "','" + _myHostName + "','" + _Date + "','" + _Time + "'); " +
                                                                " INSERT INTO 'WindowsFeature' ('Name', 'Value' ,'ComputerName', 'Date', 'Time') " +
                                                                "VALUES ('Log " + e.Log + " has Maximum size, kB:','" + e.MaximumKilobytes + "','" + _myHostName + "','" + _Date + "','" + _Time + "'); " +
                                                                " INSERT INTO 'WindowsFeature' ('Name', 'Value' ,'ComputerName', 'Date', 'Time') " +
                                                                "VALUES ('Log " + e.Log + " at:','" + temp.ToString() + " kB','" + _myHostName + "','" + _Date + "','" + _Time + "'); ";
                                                        command = new SQLiteCommand(query, connection);
                                                        try { command.ExecuteNonQuery(); } catch { }
                                                    }
                                                }
                                            } catch { }
                                        }
                                    } catch { }
                                }
                            }
                        } catch
                        {
                            command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                                      " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                            command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                            command.Parameters.AddWithValue("@Name", "Error");
                            command.Parameters.AddWithValue("@Value", "EventLog");
                            command.Parameters.AddWithValue("@Value1", "Registry Access");
                            command.Parameters.AddWithValue("@Date", _Date);
                            command.Parameters.AddWithValue("@Time", _Time);
                            try { command.ExecuteNonQuery(); } catch { }
                        }
                    }
                }

                try
                {
                    System.ServiceProcess.ServiceController[] services = System.ServiceProcess.ServiceController.GetServices(HostName);

                    a1 = ""; a2 = ""; a3 = ""; query = "";
                    for (int i = 0; i < services.Length; i++)
                    {
                        if (services[i] != null)
                        {
                            try
                            {
                                a1 = ""; a2 = ""; a3 = ""; query = "";
                                a1 = services[i].ServiceName;
                                a2 = services[i].DisplayName;
                                a3 = services[i].Status.ToString();
                                query = "INSERT INTO 'Services' ('ComputerName', 'ServiceName', 'DisplayName', 'Status', 'Date', 'Time')" +
                                    " VALUES ('" + HostName + "','" + a1 + "','" + a2 + "','" + a3 + "','" + _Date + "','" + _Time + "'); ";
                                command = new SQLiteCommand(query, connection);
                                try { command.ExecuteNonQuery(); } catch { }
                            } catch
                            {
                                command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                                command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                                command.Parameters.AddWithValue("@Name", "Error");
                                command.Parameters.AddWithValue("@Value", "GetServices");
                                command.Parameters.AddWithValue("@Value1", services[i].ToString());
                                command.Parameters.AddWithValue("@Date", _Date);
                                command.Parameters.AddWithValue("@Time", _Time);
                                try { command.ExecuteNonQuery(); } catch { }
                            }
                        }
                    }

                    services = System.ServiceProcess.ServiceController.GetDevices(HostName);
                    for (int i = 0; i < services.Length; i++)
                    {
                        if (services[i] != null)
                        {
                            try
                            {
                                a1 = ""; a2 = ""; a3 = ""; query = "";
                                a1 = services[i].ServiceName;
                                a2 = services[i].DisplayName;
                                a3 = services[i].Status.ToString();
                                query = "INSERT INTO 'Services' ('ComputerName', 'ServiceName', 'DisplayName' ,'Status', 'Date', 'Time')" +
                                    " VALUES ('" + HostName + "','" + a1 + "','" + a2 + "','" + a3 + "','" + _Date + "','" + _Time + "'); ";
                                command = new SQLiteCommand(query, connection);
                                command.ExecuteNonQuery();
                            } catch
                            {
                                command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                         " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                                command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                                command.Parameters.AddWithValue("@Name", "Error");
                                command.Parameters.AddWithValue("@Value", "GetDevices");
                                command.Parameters.AddWithValue("@Value1", services[i].ToString());
                                command.Parameters.AddWithValue("@Date", _Date);
                                command.Parameters.AddWithValue("@Time", _Time);
                                try { command.ExecuteNonQuery(); } catch { }
                            }
                        }
                    }

                    System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcesses(HostName);
                    for (int i = 0; i < procs.Length; i++)
                    {
                        if (procs[i] != null)
                        {
                            try
                            {
                                a1 = ""; a2 = ""; query = "";
                                a1 = procs[i].ProcessName;
                                a2 = procs[i].Id.ToString();
                                query = "INSERT INTO 'Process' ('ComputerName', 'Name', 'iDProcess', 'Date', 'Time')" +
                                    " VALUES ('" + HostName + "','" + a1 + "','" + a2 + "','" + _Date + "','" + _Time + "'); ";
                                command = new SQLiteCommand(query, connection);
                                command.ExecuteNonQuery();
                            } catch
                            {
                                command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                         " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                                command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                                command.Parameters.AddWithValue("@Name", "Error");
                                command.Parameters.AddWithValue("@Value", "GetProcesses");
                                command.Parameters.AddWithValue("@Value1", procs[i].ToString());
                                command.Parameters.AddWithValue("@Date", _Date);
                                command.Parameters.AddWithValue("@Time", _Time);
                                try { command.ExecuteNonQuery(); } catch { }
                            }
                        }
                    }
                } catch { }

                command.Dispose();
            }
        }
        //-------------/\/\/\/\------------// Functions. End of the block //-------------/\/\/\/\------------// 





        //-------------/\/\/\/\------------// Allow Access to the Controls from other threads. Start of the block //-------------/\/\/\/\------------// 
        private void _labelLicense(string s) //add string into textBoxLicense from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { labelLicense.Text = s; }));
                else
                    labelLicense.Text = s;
            } catch { }
        }

        private void _labelResultCheckingLicenseBackColor(System.Drawing.Color color) //access to textBoxLicense from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { labelResultCheckingLicense.BackColor = color; }));
                else
                    labelResultCheckingLicense.BackColor = color;
            } catch { }
        }

        private void _labelResultCheckingLicenseText(string s) //add string into textBoxLogs from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { labelResultCheckingLicense.Text = s; }));
                else
                    labelResultCheckingLicense.Text = s;
            } catch { }
        }

        private void _labelResultCheckingLicenseVisible(bool Visible) //add string into textBoxLogs from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { labelResultCheckingLicense.Visible = Visible; }));
                else
                    labelResultCheckingLicense.Visible = Visible;
            } catch { }
        }

        private void _textBoxLicenseForeColor(System.Drawing.Color color) //access to textBoxLicense from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { textBoxLicense.ForeColor = color; }));
                else
                    textBoxLicense.ForeColor = color;
            } catch { }
        }

        private void _textBoxLicenseBackColor(System.Drawing.Color color) //access to textBoxLicense from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { textBoxLicense.BackColor = color; }));
                else
                    textBoxLicense.BackColor = color;
            } catch { }
        }

        private void _textBoxLicense(string s) //add string into textBoxLicense from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { textBoxLicense.Text = s; }));
                else
                    textBoxLicense.Text = s;
            } catch { }
        }

        private void _textBoxLicenseVisible(bool visible) //add string into textBoxLicense from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { textBoxLicense.Visible = visible; }));
                else
                    textBoxLicense.Visible = visible;
            } catch { }
        }

        private void _labelCurrentNet(string s) //add string into labelCurrentNet from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate
                    {
                        labelCurrentNet.Text = s;
                        if (s != _myNET)
                        { labelCurrentNet.ForeColor = System.Drawing.Color.Crimson; }
                        else
                        { labelCurrentNet.ForeColor = System.Drawing.Color.Gray; }
                    }));
                else
                {
                    labelCurrentNet.Text = s;
                    if (s != _myNET)
                    { labelCurrentNet.ForeColor = System.Drawing.Color.Crimson; }
                    else
                    { labelCurrentNet.ForeColor = System.Drawing.Color.Gray; }
                }
            } catch { }
        }

        private void _buttonGetServicesVisible(bool visible) //add string into textBoxLicense from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { buttonGetServices.Visible = visible; }));
                else
                    buttonGetServices.Visible = visible;
            } catch { }
        }

        private void _buttonGetProcessVisible(bool visible) //add string into textBoxLicense from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { buttonGetProcess.Visible = visible; }));
                else
                    buttonGetProcess.Visible = visible;
            } catch { }
        }

        private int _listBoxNetsRowSelectedIndex()
        {
            int Index = -1;
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { Index = listBoxNetsRow.SelectedIndex; }));
                else
                    Index = listBoxNetsRow.SelectedIndex;
            } catch { }
            return Index;
        }

        private void _textBoxInputNetOrInputPC(string s) //add string into textBoxLicense from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { textBoxInputNetOrInputPC.Text = s; }));
                else
                    textBoxInputNetOrInputPC.Text = s;
            } catch { }
        }

        private void _textBoxLogs(string s) //add string into textBoxLogs from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { textBoxLogs.AppendText(s); }));
                else
                    textBoxLogs.AppendText(s);
            } catch { }
        }

        private void _listBoxNetsRow(string s) //add string into listBoxListOfNet from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { listBoxNetsRow.Items.Add(s); }));
                else
                    listBoxNetsRow.Items.Add(s);
            } catch { }
        }

        private void _comboBoxTargedPCAdd(string s) //add string into comboBoxTargedPC from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { comboBoxTargedPC.Items.Add(s); }));
                else
                    comboBoxTargedPC.Items.Add(s);
            } catch { }
        }

        private void _comboBoxTargedPCIndex(int i) //add string into comboBoxTargedPC from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { comboBoxTargedPC.SelectedIndex = i; }));
                else
                    comboBoxTargedPC.SelectedIndex = i;
            } catch { }
        }

        private void _PingItem(string s) //add string into comboBoxTargedPC from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { PingItem.Text = s; }));
                else
                    PingItem.Text = s;
            } catch { }
        }

        private void _PingHostItem(string s) //add string into comboBoxTargedPC from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { PingHostItem.Text = s; }));
                else
                    PingHostItem.Text = s;
            } catch { }
        }

        private void _RDPMenuItem(string s) //add string into comboBoxTargedPC from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { RDPMenuItem.Text = s; }));
                else
                    RDPMenuItem.Text = s;
            } catch { }
        }

        private void _GetLogRemotePCItem(string s) //add string into comboBoxTargedPC from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { GetLogRemotePCItem.Text = s; }));
                else
                    GetLogRemotePCItem.Text = s;
            } catch { }
        }

        private void _GetEventsItem(string s) //add string into comboBoxTargedPC from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { GetEventsItem.Text = s; }));
                else
                    GetEventsItem.Text = s;
            } catch { }
        }

        private void _FullScanItem(string s) //add string into comboBoxTargedPC from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { FullScanItem.Text = s; }));
                else
                    FullScanItem.Text = s;
            } catch { }
        }

        private void _GetWholeDataItem(string s) //add string into comboBoxTargedPC from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { GetWholeDataItem.Text = s; }));
                else
                    GetWholeDataItem.Text = s;
            } catch { }
        }

        private void _GetFilesItem(string s) //add string into comboBoxTargedPC from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { GetFilesItem.Text = s; }));
                else
                    GetFilesItem.Text = s;
            } catch { }
        }

        private void _GetRegItem(string s) //add string into comboBoxTargedPC from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { GetRegItem.Text = s; }));
                else
                    GetRegItem.Text = s;
            } catch { }
        }

        private void _labelControlPing(string s) //add string into labelControlPing from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { labelControlPing.Text = s; }));
                else
                    labelControlPing.Text = s;
            } catch { }
        }

        private void _labelControlPingBackColor(System.Drawing.Color color) //add string into labelControlPing from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { labelControlPing.BackColor = color; }));
                else
                    labelControlPing.BackColor = color;
            } catch { }
        }

        private void _comboUsersAdd(string s) //add string into  from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { comboUsers.Items.Add(s); }));
                else
                    comboUsers.Items.Add(s);
            } catch { }
        }

        private void _comboBoxTargedPcClr() //add string into  from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { comboBoxTargedPC.Items.Clear(); }));
                else
                    comboBoxTargedPC.Items.Clear();
            } catch { }
        }

        private void _comboUsersClr() //add string into  from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { comboUsers.Items.Clear(); }));
                else
                    comboUsers.Items.Clear();
            } catch { }
        }

        private void _StatusLabelCurrentColor(System.Drawing.Color Color) //add string into  from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { StatusLabelCurrent.ForeColor = Color; }));
                else
                    StatusLabelCurrent.ForeColor = Color;
            } catch { }
        }

        private void _StatusLabelCurrentText(string s) //add string into  from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { StatusLabelCurrent.Text = s; }));
                else
                    StatusLabelCurrent.Text = s;
            } catch { }
        }

        private void _StatusLabelCurrentChangeColor()  //Change StatusLabel2 Color
        {
            _StatusLabelCurrentColor(System.Drawing.Color.Black);
            _RunTimer = true;
            while (_RunTimer)
            {
                try
                {
                    if (this.InvokeRequired)
                        BeginInvoke(new MethodInvoker(delegate
                        {
                            if (StatusLabelCurrent.ForeColor == System.Drawing.Color.DarkCyan)
                            { StatusLabelCurrent.ForeColor = System.Drawing.Color.DarkRed; }
                            else { StatusLabelCurrent.ForeColor = System.Drawing.Color.DarkCyan; }
                        }));
                    else
                    {
                        if (StatusLabelCurrent.ForeColor == System.Drawing.Color.DarkCyan)
                        { StatusLabelCurrent.ForeColor = System.Drawing.Color.DarkRed; }
                        else { StatusLabelCurrent.ForeColor = System.Drawing.Color.DarkCyan; }
                    }
                } catch { }
                Task.Delay(300).Wait();
            }
            StatusLabelCurrent.ForeColor = System.Drawing.Color.Black;
        }

        private void _StatusLabelCurrentChangeText(string[] sText, string s1 = "")  //Change StatusLabel2 Color
        {
            _RunTimer = true;
            while (_RunTimer)
            {
                foreach (string s in sText)
                {
                    try
                    {
                        if (this.InvokeRequired)
                            BeginInvoke(new MethodInvoker(delegate
                            {
                                if (StatusLabelCurrent.ForeColor == System.Drawing.Color.DarkCyan)
                                { StatusLabelCurrent.ForeColor = System.Drawing.Color.DarkRed; }
                                else { StatusLabelCurrent.ForeColor = System.Drawing.Color.DarkCyan; }
                                if ((object)s != null)
                                    StatusLabelCurrent.Text = s1 + s;
                            }));
                        else
                        {
                            if (StatusLabelCurrent.ForeColor == System.Drawing.Color.DarkCyan)
                            { StatusLabelCurrent.ForeColor = System.Drawing.Color.DarkRed; }
                            else { StatusLabelCurrent.ForeColor = System.Drawing.Color.DarkCyan; }

                            if ((object)s != null)
                                StatusLabelCurrent.Text = s1 + s;
                        }
                    } catch { }
                    Task.Delay(700).Wait();
                }
                Task.Delay(900).Wait();
            }
        }

        private void _StatusLabel1ForeColor(System.Drawing.Color Color) //add string into  from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { StatusLabel1.ForeColor = Color; }));
                else
                    StatusLabel1.ForeColor = Color;
            } catch { }
        }

        private void _StatusLabel1Text(string s) //add string into  from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { StatusLabel1.Text = s; }));
                else
                    StatusLabel1.Text = s;
            } catch { }
        }

        private void _StatusLabel2ForeColor(System.Drawing.Color Color) //add string into  from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { StatusLabel2.ForeColor = Color; }));
                else
                    StatusLabel2.ForeColor = Color;
            } catch { }
        }

        private void _StatusLabel2Text(string s) //add string into  from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { StatusLabel2.Text = s; }));
                else
                    StatusLabel2.Text = s;
            } catch { }
        }

        private void _StatusLabel2ChangeColor()  //Change StatusLabel2 Color
        {
            _StatusLabel2ForeColor(System.Drawing.Color.Black);
            _RunTimer = true;
            while (_RunTimer)
            {
                try
                {
                    if (this.InvokeRequired)
                        BeginInvoke(new MethodInvoker(delegate
                        {
                            if (StatusLabel2.ForeColor == System.Drawing.Color.DarkCyan)
                            { StatusLabel2.ForeColor = System.Drawing.Color.DarkRed; }
                            else { StatusLabel2.ForeColor = System.Drawing.Color.DarkCyan; }
                        }));
                    else
                    {
                        if (StatusLabel2.ForeColor == System.Drawing.Color.DarkCyan)
                        { StatusLabel2.ForeColor = System.Drawing.Color.DarkRed; }
                        else { StatusLabel2.ForeColor = System.Drawing.Color.DarkCyan; }
                    }
                } catch { }
                Task.Delay(300).Wait();
            }
        }

        private void _StatusLabel2ChangeTextbyCircle()  //Change StatusLabel2 Text by Round from List "_StatusLabel2ListChangingText" 
        {
            _RunTimer = true;
            while (_RunTimer)
            {
                string[] ArrayTexts = _StatusLabel2ListChangingText.ToArray();
                foreach (string s in ArrayTexts)
                {
                    if (_RunTimer)
                    {
                        try
                        {
                            if (this.InvokeRequired)
                                BeginInvoke(new MethodInvoker(delegate
                                { StatusLabel2.Text = s; }));
                            else
                            { StatusLabel2.Text = s; }
                        } catch { }
                    }
                    else break;

                    Task.Delay(750).Wait();
                }
            }
        }

        private void _buttonPingText(string s) //add string into  from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate { buttonPing.Text = s; }));
                else
                    buttonPing.Text = s;
            } catch { }
        }

        private void timer1_Tick(object sender, EventArgs e)  //add into progressBar Value 1 from other threads by timer1
        { _ProgressWork1(); }

        private void timer1Stop() //Set progressBar Value into 0 from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate
                    { timer1.Stop(); _StatusLabel2ForeColor(System.Drawing.Color.Black); }));
                else
                {
                    timer1.Stop(); _StatusLabel2ForeColor(System.Drawing.Color.Black);
                }
            } catch { }
        }

        private void timer1Start() //Set progressBar Value into 0 from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate
                    { timer1.Start(); }));
                else
                { timer1.Start(); ; }
            } catch { }
        }

        private void _timer2State(bool State)
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate
                    {
                        if (State == true)
                            timer2.Enabled = true;
                        else timer2.Enabled = false;
                    }));
                else
                {
                    if (State == true)
                        timer2.Enabled = true;
                    else timer2.Enabled = false;
                }
            } catch { }
        }

        private void _timer2_Tick(object sender, EventArgs e) //changing of the color of the StatusLabel2 
        { _StatusLabel2ChangeColor(); }

        private void _ProgressBar1Value0() //Set progressBar Value into 0 from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate
                    { ProgressBar1.Value = 0; }));
                else
                { ProgressBar1.Value = 0; }
            } catch { }
        }

        private void _ProgressBar1Value100() //Set progressBar Value into 100 from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate
                    { ProgressBar1.Value = 100; }));
                else
                { ProgressBar1.Value = 100; }
            } catch { }
        }

        private void _ProgressWork1() //add into progressBar Value 1 from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate
                    {
                        if (ProgressBar1.Value > 99)
                        { ProgressBar1.Value = 0; }
                        ProgressBar1.Maximum = 100;
                        ProgressBar1.Value += 1;
                    }));
                else
                {
                    if (ProgressBar1.Value > 99)
                    { ProgressBar1.Value = 0; }
                    ProgressBar1.Maximum = 100;
                    ProgressBar1.Value += 1;
                }
            } catch { }
        }

        private void _ProgressWork2()  //add into progressBar Value 2 from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate
                    {
                        if (ProgressBar1.Value > 99)
                        { ProgressBar1.Value = 0; }
                        ProgressBar1.Maximum = 100;
                        ProgressBar1.Value += 2;
                    }));
                else
                {
                    if (ProgressBar1.Value > 99)
                    { ProgressBar1.Value = 0; }
                    ProgressBar1.Maximum = 100;
                    ProgressBar1.Value += 2;
                }
            } catch { }
        }

        private void _ProgressWork5()  //add into progressBar Value 5 from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate
                    {
                        if (ProgressBar1.Value > 99)
                        { ProgressBar1.Value = 0; }
                        ProgressBar1.Maximum = 100;
                        ProgressBar1.Value += 5;
                    }));
                else
                {
                    if (ProgressBar1.Value > 99)
                    { ProgressBar1.Value = 0; }
                    ProgressBar1.Maximum = 100;
                    ProgressBar1.Value += 5;
                }
            } catch { }
        }

        private void _ProgressWork10()  //add into progressBar Value 10 from other threads
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate
                    {
                        if (ProgressBar1.Value > 99)
                        { ProgressBar1.Value = 0; }
                        ProgressBar1.Maximum = 100;
                        ProgressBar1.Value += 10;
                    }));
                else
                {
                    if (ProgressBar1.Value > 99)
                    { ProgressBar1.Value = 0; }
                    ProgressBar1.Maximum = 100;
                    ProgressBar1.Value += 10;
                }
            } catch { }
        }

        private void _LoadDataKeysEnabled(bool Enabled)  //Keys "Load varios data" Enable is  true or false
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate
                    {
                        if (textBoxDomainLength > 0 && textBoxLoginLength > 0)
                        {
                            ControlHostMenu.Enabled = Enabled;
                            comboUsers.Enabled = Enabled;
                            buttonGetServices.Enabled = Enabled;
                            comboBoxService.Enabled = Enabled;
                            buttonGetProcess.Enabled = Enabled;
                            comboBoxProcess.Enabled = Enabled;
                            textBoxNameProgramm.Enabled = Enabled;
                            foreach (CheckBox checkBox in tabPageCtrl.Controls.OfType<CheckBox>())      //Перебираем все чекбоксы на форме
                            { checkBox.Enabled = true; }
                        }
                        else
                        {
                            ControlHostMenu.Enabled = Enabled;
                            comboUsers.Enabled = Enabled;
                            buttonGetServices.Enabled = Enabled;
                            comboBoxService.Enabled = Enabled;
                            buttonGetProcess.Enabled = Enabled;
                            comboBoxProcess.Enabled = Enabled;
                            textBoxNameProgramm.Enabled = Enabled;

                            foreach (CheckBox checkBox in tabPageCtrl.Controls.OfType<CheckBox>())      //Перебираем все чекбоксы на форме
                            { checkBox.Enabled = false; }
                        }
                        buttonGetServices.Enabled = Enabled;
                        buttonGetProcess.Enabled = Enabled;
                    }));
                else
                {
                    if (textBoxDomainLength > 0 && textBoxLoginLength > 0)
                    {
                        ControlHostMenu.Enabled = Enabled;
                        comboUsers.Enabled = Enabled;
                        buttonGetServices.Enabled = Enabled;
                        comboBoxService.Enabled = Enabled;
                        buttonGetProcess.Enabled = Enabled;
                        comboBoxProcess.Enabled = Enabled;
                        textBoxNameProgramm.Enabled = Enabled;
                        foreach (CheckBox checkBox in tabPageCtrl.Controls.OfType<CheckBox>())      //Перебираем все чекбоксы на форме
                        { checkBox.Enabled = true; }
                    }
                    else
                    {
                        ControlHostMenu.Enabled = Enabled;
                        comboUsers.Enabled = Enabled;
                        buttonGetServices.Enabled = Enabled;
                        comboBoxService.Enabled = Enabled;
                        buttonGetProcess.Enabled = Enabled;
                        comboBoxProcess.Enabled = Enabled;
                        textBoxNameProgramm.Enabled = Enabled;

                        foreach (CheckBox checkBox in tabPageCtrl.Controls.OfType<CheckBox>())      //Перебираем все чекбоксы на форме
                        { checkBox.Enabled = false; }
                    }
                    buttonGetServices.Enabled = true;
                    buttonGetProcess.Enabled = true;
                }
            } catch { }
        }

        private void _dataGridView1CurrentCellStyle(System.Drawing.Color color)
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate
                    {
                        dataGridView1.CurrentCell.Style.BackColor = color;
                    }));
                else
                {
                    dataGridView1.CurrentCell.Style.BackColor = color;

                }
            } catch { }
        }

        private void _dataGridView1CurrentRowDefaultCellStyleBackColor(System.Drawing.Color color)
        {
            try
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate
                    {
                        dataGridView1.CurrentRow.DefaultCellStyle.BackColor = color;
                    }));
                else
                {
                    dataGridView1.CurrentRow.DefaultCellStyle.BackColor = color;
                }
            } catch { }
        }

        //-------------/\/\/\/\------------// Allow Access to Controls from other threads. End of the block//-------------/\/\/\/\------------// 





        //-------------/\/\/\/\------------// Searching of files. Start of The Block //-------------/\/\/\/\------------// 

        private void searchFilesMenu_Click(object sender, EventArgs e)
        {
            _currentRemoteRegistryRunning = true;
            textBoxToTemporary();
            if (tabControl.SelectedTab == tabDataGrid)
            {
                bool str = ParseSelectedCell();
                MessageBox.Show("ParseSelectedCell+\n_currentHostName " + _currentHostName + "\n_currentHostIP " + _currentHostIP);
            }
            else
            {
                ParseTextboxInputNetOrPC();
                MessageBox.Show("ParseTextboxInputNetOrPC\n_currentHostName " + _currentHostName + "\n_currentHostIP " + _currentHostIP);
            }

            StatusLabel2.Text = "Готовлю базу... ";
            StopSearchItem.Enabled = true;

            waitNetPing = new AutoResetEvent(false);
            waitFile = new AutoResetEvent(false);
            waitStop = new AutoResetEvent(false);
            waitNetStop = new AutoResetEvent(false);
            waitFilePing = new AutoResetEvent(false);

            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
            databaseHost = fi.FullName;
            _DBCheckFull(_currentHostName);

            DirectoryInfo di = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\ready\\tmpfile.stop");
            if (File.Exists(di.FullName)) { try { File.Delete(di.FullName); } catch { } }
            DirectoryInfo di1 = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\tmp\\tmpfile.stop");
            if (File.Exists(di1.FullName)) { try { File.Delete(di1.FullName); } catch { } }
            for (int i = 0; i < 100; i++)
            {
                DirectoryInfo fi2 = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\tmp\\tmpfile." + i + ".txt");
                if (File.Exists(fi2.FullName))
                { File.Delete(fi2.FullName); }
                DirectoryInfo fi1 = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\ready\\tmpfile." + i + ".txt");
                if (File.Exists(fi1.FullName))
                { File.Delete(fi1.FullName); }
            }


            StatusLabel2.ForeColor = System.Drawing.Color.DarkCyan;
            Task.Delay(500).Wait();
            ProgressBar1.Value = 0;
            _StopSearchFiles = false;
            _StopScannig = false;
            timer1Start();
            _GetTimeRunScan();
            Thread t = new Thread(new ThreadStart(_StatusLabel2ChangeColor));
            t.Start();

            Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));
            StatusLabel2.Text = "Проверяю настройки хоста... ";
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection))
                {
                    command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                    command.Parameters.AddWithValue("@Name", "ComputerIP");
                    command.Parameters.AddWithValue("@Value", _currentHostIP);
                    command.Parameters.AddWithValue("@Value1", "searchFiles");
                    command.Parameters.AddWithValue("@Date", _Date);
                    command.Parameters.AddWithValue("@Time", _Time);
                    try { command.ExecuteNonQuery(); } catch { }
                }
                connection.Close();
            }

            NetworkShare share = new NetworkShare(_currentHostName, "ipc$", _User, _Password);
            share.Connect();
            Task.Delay(200).Wait();
            checkRunningRemoteRegistry(_currentHostName, _currentHostIP, _User, _Password); //Лучше использовать IP
            _ProgressWork5();

            IPAddress addr = IPAddress.Parse(_currentHostIP);
            _getUserAndSN(_currentHostName, addr);
            _GetTimeRunScan();
            string[] s = Regex.Split(TempConfigSingleHost, @"[|]");

            Task.Delay(200).Wait();
            StatusLabel2.Text = "Обновляю данные в базе... ";
            string TempComputerName = "", TempComputerIP = "", TempComputerModel = "", TempComputerSN = "", TempLogOnUser = "", TempComputerNameShort = "", TempComputerDomainName = "";
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}; journal_mode =MEMORY;", databaseAliveHosts)))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("SELECT ComputerName, ComputerNameShort, ComputerDomainName, ComputerIP, ComputerModel, ComputerSN, LogOnUser, Date FROM AliveHosts Where ComputerName like '%" + _currentHostName + "%' ;", connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        foreach (DbDataRecord record in reader)
                        {
                            TempComputerName = record["ComputerName"].ToString();
                            TempComputerIP = record["ComputerIP"].ToString();
                            TempComputerModel = record["ComputerModel"].ToString();
                            TempComputerSN = record["ComputerSN"].ToString();
                            TempLogOnUser = record["LogOnUser"].ToString();
                            TempComputerNameShort = record["ComputerNameShort"].ToString();
                            TempComputerDomainName = record["ComputerDomainName"].ToString();
                        }
                    }
                }

                if (!s[1].Trim().ToLower().Contains("unknow") && TempComputerName.Trim().Length > 0)
                {
                    TempComputerIP = s[2].Trim();
                    if (!s[4].ToLower().Contains("permission") || !s[5].ToLower().Contains("permission"))
                    {
                        TempComputerModel = s[4].Trim();
                        TempComputerSN = s[5].Trim();
                        TempLogOnUser = s[6].Trim();
                    }

                    using (SQLiteCommand command = new SQLiteCommand("UPDATE AliveHosts SET ComputerNameShort=@ComputerNameShort, ComputerDomainName=@ComputerDomainName, ComputerIP=@ComputerIP, ComputerModel=@ComputerModel," +
                            " ComputerSN=@ComputerSN, LogOnUser=@LogOnUser, ScannerName=@ScannerName, ScannerIP=@ScannerIP, Date=@Date, Time=@Time WHERE ComputerName=@ComputerName LIMIT 1", connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@ComputerNameShort", s[1].Trim()));
                        command.Parameters.Add(new SQLiteParameter("@ComputerIP", TempComputerIP));
                        command.Parameters.Add(new SQLiteParameter("@ComputerDomainName", TempComputerDomainName));
                        command.Parameters.Add(new SQLiteParameter("@ComputerModel", TempComputerModel));
                        command.Parameters.Add(new SQLiteParameter("@ComputerSN", TempComputerSN));
                        command.Parameters.Add(new SQLiteParameter("@LogOnUser", TempLogOnUser));
                        command.Parameters.Add(new SQLiteParameter("@ScannerName", _myHostName));
                        command.Parameters.Add(new SQLiteParameter("@ScannerIP", _myIP));
                        command.Parameters.Add(new SQLiteParameter("@Date", _Date));
                        command.Parameters.Add(new SQLiteParameter("@Time", _Time));
                        try { command.ExecuteNonQuery(); } catch { };
                    }
                }
                else if (!s[1].Trim().ToLower().Contains("unknow") && TempComputerName.Trim().Length == 0)
                {
                    TempComputerIP = s[2].Trim();
                    TempComputerModel = s[4].Trim();
                    TempComputerSN = s[5].Trim();
                    TempLogOnUser = s[6].Trim();

                    using (SQLiteCommand command = new SQLiteCommand("INSERT INTO AliveHosts ('ComputerName', 'ComputerNameShort', 'ComputerDomainName', 'ComputerIP', 'ComputerModel', 'ComputerSN', 'LogOnUser', 'ScannerName', 'ScannerIP', 'Date', 'Time') VALUES (@ComputerName, @ComputerNameShort, @ComputerDomainName, @ComputerIP, @ComputerModel, @ComputerSN, @LogOnUser, @ScannerName, @ScannerIP, @Date, @Time)", connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@ComputerName", s[0].Trim()));
                        command.Parameters.Add(new SQLiteParameter("@ComputerNameShort", s[1].Trim()));
                        command.Parameters.Add(new SQLiteParameter("@ComputerIP", TempComputerIP));
                        command.Parameters.Add(new SQLiteParameter("@ComputerDomainName", TempComputerDomainName));
                        command.Parameters.Add(new SQLiteParameter("@ComputerModel", TempComputerModel));
                        command.Parameters.Add(new SQLiteParameter("@ComputerSN", TempComputerSN));
                        command.Parameters.Add(new SQLiteParameter("@LogOnUser", TempLogOnUser));
                        command.Parameters.Add(new SQLiteParameter("@ScannerName", _myHostName));
                        command.Parameters.Add(new SQLiteParameter("@ScannerIP", _myIP));
                        command.Parameters.Add(new SQLiteParameter("@Date", _Date));
                        command.Parameters.Add(new SQLiteParameter("@Time", _Time));
                        try { command.ExecuteNonQuery(); } catch { };
                    }
                }
            }

            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection))
                {
                    command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                    command.Parameters.AddWithValue("@Name", "ComputerIP");
                    command.Parameters.AddWithValue("@Value", _currentHostIP);
                    command.Parameters.AddWithValue("@Value1", _currentHostName);
                    command.Parameters.AddWithValue("@Date", _Date);
                    command.Parameters.AddWithValue("@Time", _Time);
                    try { command.ExecuteNonQuery(); } catch { }
                }
            }



            Thread t1 = new Thread(new ParameterizedThreadStart((obj) => SearchFilesOnLogicalDisks(_currentHostName)));
            t1.Start();

            Thread t2 = new Thread(new ThreadStart(_CheckFoundFiles));
            t2.Start();

            Task.Delay(5000).Wait();

            while (!t1.IsAlive)
            { Task.Delay(2000).Wait(); }
            share.Disconnect();
        }

        private void _CheckFoundFiles()
        {
            bool RunWrite = true;
            DirectoryInfo fi = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\ready\\tmpfile.stop");
            while (RunWrite)
            {
                Task.Delay(500).Wait();
                for (int i = 0; i < 100; i++)
                {
                    DirectoryInfo fi1 = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\ready\\tmpfile." + i + ".txt");
                    if (File.Exists(fi1.FullName))
                    {
                        _StatusLabel2Text("Ищу заданные файлы на хосте и записываю в базу");
                        _TempFoundDocumentsToDB(Application.StartupPath + "\\myEventLoger\\tmp\\tmpfile." + i + ".txt", Application.StartupPath + "\\myEventLoger\\ready\\tmpfile." + i + ".txt");
                    }
                }

                int FileTemp = 0;
                for (int i = 0; i < 100; i++)
                {
                    DirectoryInfo fi2 = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\tmp\\tmpfile." + i + ".txt");
                    if (File.Exists(fi2.FullName))
                    { FileTemp++; }
                    DirectoryInfo fi1 = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\ready\\tmpfile." + i + ".txt");
                    if (File.Exists(fi1.FullName))
                    { FileTemp++; }
                }

                if (File.Exists(fi.FullName) && FileTemp == 0)
                { RunWrite = false; }
            }

            Task.Delay(500).Wait();
            _StatusLabel2Text("Чищу временные папки...");

            for (int i = 0; i < 100; i++)
            {
                DirectoryInfo fi2 = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\tmp\\tmpfile." + i + ".txt");
                if (File.Exists(fi2.FullName))
                { File.Delete(fi2.FullName); }
                DirectoryInfo fi1 = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\ready\\tmpfile." + i + ".txt");
                if (File.Exists(fi1.FullName))
                { File.Delete(fi1.FullName); }
            }

            DirectoryInfo di = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\ready\\tmpfile.stop");
            if (File.Exists(di.FullName)) { try { File.Delete(di.FullName); } catch { } }
            DirectoryInfo di1 = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\tmp\\tmpfile.stop");
            if (File.Exists(di1.FullName)) { try { File.Delete(di1.FullName); } catch { } }

            //            _stopSearch();
            timer1Stop();
            timer2.Stop();
            _RunTimer = false;
            Task.Delay(500).Wait();
            _ProgressBar1Value100();

            if (!_StopSearchFiles && !_GlobalStopSearch)
            {
                StatusLabel2.Text = "Поиск файлов завершен!";
                StatusLabel2.ForeColor = System.Drawing.Color.Black;
            }
            else
            {
                StatusLabel2.Text = "Поиск файлов прерван пользователем!";
                StatusLabel2.ForeColor = System.Drawing.Color.Crimson;

                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection))
                    {
                        command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                        command.Parameters.AddWithValue("@Name", "Warning");
                        command.Parameters.AddWithValue("@Value", "searchFiles");
                        command.Parameters.AddWithValue("@Value1", "Поиск файлов прерван пользователем!");
                        command.Parameters.AddWithValue("@Date", _Date);
                        command.Parameters.AddWithValue("@Time", _Time);
                        try { command.ExecuteNonQuery(); } catch { }
                    }
                    connection.Close();
                }
            }
            _StopSearchFiles = false;
            _GlobalStopSearch = false;
        }

        private async Task SearchFilesOnLogicalDisks(string HostName) //Search logical disk and documents on the disks
        {
            _StopSearchFiles = false; //Flag of force Stop searching
            string logicaldisks = "";

            //Task t;
            _StatusLabel2Text("Составляю перечень дисков...");

            ConnectionOptions connOpt = new ConnectionOptions();   //Connect to WMI of the Remote Host 
            connOpt.Impersonation = ImpersonationLevel.Impersonate;
            connOpt.Timeout = new TimeSpan(0, 0, 300);
            connOpt.EnablePrivileges = true;

            ManagementPath mp = new ManagementPath();
            mp.NamespacePath = @"\root\cimv2";
            if (_currentHostName != _myHostName)
            {
                connOpt.Username = _User;
                connOpt.Password = _Password;
                mp.Server = _currentHostName;
                connOpt.Authentication = AuthenticationLevel.Packet;
            }

            ManagementScope ms = new ManagementScope(mp, connOpt);
            ManagementObjectSearcher srcd = new ManagementObjectSearcher(new ObjectQuery("SELECT Caption FROM Win32_LogicalDisk WHERE DriveType='3' OR DriveType='2' OR DriveType='0' OR DriveType='1' OR DriveType='6'"));
            if (_currentHostName != _myHostName)
            {
                ms.Connect();
                bool isConnected = ms.IsConnected;
                if (isConnected)
                { srcd = new ManagementObjectSearcher(ms, new ObjectQuery("SELECT Caption FROM Win32_LogicalDisk WHERE DriveType='3' OR DriveType='2' OR DriveType='0' OR DriveType='1' OR DriveType='6'")); }
            }

            ManagementObjectCollection queryCollection = srcd.Get();
            foreach (ManagementObject mo in queryCollection)
            {
                if (mo["Caption"].ToString().Trim().Length > 0)
                { logicaldisks += mo["Caption"].ToString().Trim(':') + " "; }
            }
            string tlogicaldisks = logicaldisks.Trim();
            logicaldisks = tlogicaldisks;
            //            StatusLabel2.Text = "Ищу файлы по заданной маске...";

            using (SQLiteConnection sqlconnection = new SQLiteConnection(string.Format("Data Source={0};page_size = 65536; cache_size =100000;journal_mode =MEMORY;", databaseHost)))
            {
                sqlconnection.Open();
                using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                 " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", sqlconnection))
                {
                    command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                    command.Parameters.Add(new SQLiteParameter("@Name", "Logical Disks"));
                    command.Parameters.AddWithValue("@Value", logicaldisks);
                    command.Parameters.AddWithValue("@Value1", "LogicalDisks");
                    command.Parameters.AddWithValue("@Date", _Date);
                    command.Parameters.AddWithValue("@Time", _Time);
                    try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                }
                sqlconnection.Close();
            }
            string[] subs = Regex.Split(logicaldisks, " ");
            string[] substring = new string[36];
            int suNewIndx = 0; //Last Index of the matrix of "substring"

            //Формирование корневой папки для поиска
            if (_currentHostName == _myHostName) //ПК для поиска не выбран. Ищется на локальном ПК
            {
                for (int i = 0; i < subs.Length; i++)
                {
                    if (subs[i].Trim().Length > 0 && subs[i].Trim() != null && subs[i].ToUpper().Contains("A") == false && subs[i].ToUpper().Contains("B") == false)
                    {
                        substring[suNewIndx] = subs[i].Trim() + ":\\";
                        suNewIndx++;
                    }
                }
            }
            else //Указан для поиска не локальный ПК. Поиск ведется на удаленном ПК
            {
                for (int i = 0; i < subs.Length; i++)
                {
                    if (subs[i].Trim().Length > 0 && subs[i].Trim() != null)
                    {
                        substring[suNewIndx] = "\\\\" + _currentHostName + "\\" + subs[i].Trim() + "$\\";
                        suNewIndx++;
                    }
                }
            }
            string test = "";
            for (int i = 0; i < suNewIndx; i++)
            {
                test += substring[i] + "\n";
            }

            int aa1 = 0;
            for (int sd = 0; sd < suNewIndx; sd++)
            {
                for (int s = 0; s < mySearchPattern.Length; s++)
                {
                    if (substring[sd].Length > 0 && mySearchPattern[s].Length > 0)
                    { aa1++; }
                }
            }
            Task[] tasks = new Task[aa1];
            int aa2 = 0;

            _StatusLabel2Text("Создаю задачи для поиска файлов на дисках " + logicaldisks + " хоста");
            for (int s = 0; s < mySearchPattern.Length; s++)
            {
                for (int sd = 0; sd < suNewIndx; sd++)
                {
                    if (substring[sd].Trim().Length > 0 && mySearchPattern[s].Trim().Length > 0 && _StopSearchFiles == false)   // _StopSearchFiles==false - force stop searching files
                    {
                        tasks[aa2] = _SearchFileDocuments(Application.StartupPath + "\\myEventLoger\\tmp\\tmpfile." + aa2 + ".txt", substring[sd], mySearchPattern[s], Application.StartupPath + "\\myEventLoger\\ready\\tmpfile." + aa2 + ".txt", 0);
                        _textBoxLogs(aa2 + " " + substring[sd] + " " + mySearchPattern[s] + "\n");

                        aa2++;

                        if (_StopScannig == true)     // will Check this function. Perhaps this block would need to delete!!!!!!!!!!!!!!!!!
                        {
                            s = mySearchPattern.Length;
                            sd = suNewIndx;
                            await Task.Run(() => File.AppendAllText(Application.StartupPath + "\\myEventLoger\\tmp\\tmpfile.stop", ""));
                            break;
                        }
                    }
                }
            }
            _StatusLabel2Text("Ищу файлы по заданной маске на дисках " + logicaldisks + " хоста. Ждите окончания процесса ");

            await Task.WhenAll(tasks);
            await Task.Run(() => File.AppendAllText(Application.StartupPath + "\\myEventLoger\\tmp\\tmpfile.stop", ""));
            await Task.Run(() => _StopScannig = true);
        }

        public static IEnumerable<string> EnumerateAllFiles(string path, string pattern) //Search Files
        {
            IEnumerable<string> files = null;
            try { files = Directory.EnumerateFiles(path, pattern); } catch { }

            if (files != null)
            { foreach (var file in files) yield return file; }

            IEnumerable<string> directories = null;
            try { directories = Directory.EnumerateDirectories(path); } catch { }

            if (directories != null)
            {
                foreach (var file in directories.SelectMany(d => EnumerateAllFiles(d, pattern)))
                { yield return file; }
            }
        }

        private void StopSearchItem_Click(object sender, EventArgs e) //Menu - "Stop Search!"
        {
            _stopSearch();
            StatusLabel2.Text = "Прерываю поиск... ";
            Task.Delay(4000).Wait();
            ProgressBar1.Value = 100;
            StatusLabel2.ForeColor = System.Drawing.Color.Crimson;
            StatusLabel2.Text = "Поиск прерван!";
        }

        private void _stopSearch() //Set variable to Stop of search
        {
            waitFile.Set();
            _GlobalStopSearch = true;
            _StopScannig = true; //Stop search documents
            _StopSearchNets = true;
            _StopSearchAliveHosts = true;
            _StopScanLogEvents = true;
            _StopSearchFiles = true; //force stop searching files. Works!!!
            _RunTimer = false;
            waitStop.Set();
            waiter.Set();
            waitFilePing.Set();
            waitNetStop.Set();
            waitNetPing.Set();
            waiter.Set();
            timer1Stop();
            _timer2State(false);
        }

        private void _SearchUserDocuments(string rootDirectory, string SearchPattern, string Software = "Explorer") // Search Document at disks by its pattern
        {
            Stack<string> dirs = new Stack<string>(1000);
            FileInfo fi1 = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
            databaseHost = fi1.FullName;

            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
            {
                connection.Open();

                if (!Directory.Exists(rootDirectory))
                { throw new ArgumentException(); }
                dirs.Push(rootDirectory);

                SQLiteCommand command = new SQLiteCommand("INSERT INTO 'AllTables' ('NameTable', 'ScannerName', 'ScannerIP', 'Date', 'Time') " +
                " VALUES (@NameTable, @ScannerName, @ScannerIP, @Date, @Time)", connection);
                command.Parameters.AddWithValue("@NameTable", "UsersDocuments");
                command.Parameters.AddWithValue("@ScannerName", _myHostName);
                command.Parameters.AddWithValue("@ScannerIP", _myIP);
                command.Parameters.AddWithValue("@Date", _Date);
                command.Parameters.AddWithValue("@Time", _Time);
                try { command.ExecuteNonQuery(); } catch (Exception expt) { MessageBox.Show(expt.ToString()); };

                while (dirs.Count > 0)
                {
                    string currentDir = dirs.Pop();
                    string[] subDirs;
                    try { subDirs = Directory.GetDirectories(currentDir); } catch (UnauthorizedAccessException e)
                    {
                        textBoxLogs.AppendText("1" + e.Message + "\n");
                        command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'DocumentPath', 'Software') " +
                        " VALUES (@ComputerName, @DocumentPath, @Software, @Length, @Date, @Time)", connection);
                        command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                        command.Parameters.AddWithValue("@Software", Software);
                        command.Parameters.AddWithValue("@DocumentPath", e.Message);
                        try { command.ExecuteNonQuery(); } catch { };
                        command.Dispose();
                        continue;
                    }

                    string[] files = null;
                    try
                    { files = Directory.GetFiles(currentDir); } catch (UnauthorizedAccessException e)
                    {
                        textBoxLogs.AppendText("2" + e.Message + "\n");
                        command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'DocumentPath', 'Software') " +
                        " VALUES (@ComputerName, @DocumentPath, @Software, @Length, @Date, @Time)", connection);
                        command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                        command.Parameters.AddWithValue("@Software", Software);
                        command.Parameters.AddWithValue("@DocumentPath", e.Message);
                        try { command.ExecuteNonQuery(); } catch { };
                        continue;
                    }

                    foreach (string file in files)
                    {
                        try
                        {
                            FileInfo fi = new FileInfo(file); // Perform whatever action is required in your scenario.
                            if (fi.FullName.ToString().Contains(SearchPattern)) //маска для поиска
                            {
                                string datestr = Regex.Split(fi.LastAccessTime.ToString(), " ")[0];
                                string timestr = Regex.Split(fi.LastAccessTime.ToString(), " ")[1];
                                DateTime result = DateTime.ParseExact(datestr, formatsrc, CultureInfo.InvariantCulture);

                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Length', 'Date', 'Time') " +
                                " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Length, @Date, @Time)", connection);
                                command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                                command.Parameters.Add(new SQLiteParameter("@UserSID", _currentUserSID));
                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                command.Parameters.AddWithValue("@DocumentPath", fi.FullName);
                                command.Parameters.AddWithValue("@Software", Software);
                                command.Parameters.AddWithValue("@Length", fi.Length);
                                command.Parameters.AddWithValue("@Date", result.ToString(formatdst));
                                command.Parameters.AddWithValue("@Time", timestr);
                                try { command.ExecuteNonQuery(); } catch { };
                            }
                        } catch { continue; } // If file was deleted by a separate application
                    }

                    foreach (string str in subDirs)
                    { dirs.Push(str); }
                }
                command.Dispose();
                connection.Close();
            }
        }

        async Task _SearchFileDocuments(string myFile, string rootDirectory, string SearchPattern, string myFileReady, int stop) // Search Document at disks by its pattern
        {
            var Coder = Encoding.GetEncoding(65001);
            File.AppendAllText(myFile, ";Table:FoundDocuments\n", Coder);

            // Task t;
            Stack<string> dirs = new Stack<string>(100);

            if (!Directory.Exists(rootDirectory))
            { throw new ArgumentException(); }
            dirs.Push(rootDirectory);

            while (dirs.Count > 0 && _StopSearchFiles == false)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;
                try { subDirs = Directory.GetDirectories(currentDir); } catch (UnauthorizedAccessException e)
                {
                    await Task.Run(() => File.AppendAllText(myFile, ";| " + _currentHostName + " | " + e.Message + " | " + "0" + " | " + "0" + " | " + "0" + " |\n", Coder));
                    continue;
                } catch { continue; }

                string[] files = null;
                try
                { files = Directory.GetFiles(currentDir); } catch (UnauthorizedAccessException e)
                {
                    await Task.Run(() => File.AppendAllText(myFile, ";| " + _currentHostName + " | " + e.Message + " | " + "0" + " | " + "0" + " | " + "0" + " |\n", Coder));
                    continue;
                } catch { continue; }
                foreach (string file in files)
                {
                    try
                    {
                        FileInfo fi = new FileInfo(file); // Perform whatever action is required in your scenario.
                        if (fi.FullName.ToString().ToLower().Contains(SearchPattern) == true && _StopSearchFiles == false) //маска для поиска
                        {
                            string datestr = Regex.Split(fi.LastAccessTime.ToString(), " ")[0];
                            string timestr = Regex.Split(fi.LastAccessTime.ToString(), " ")[1];
                            DateTime result = DateTime.ParseExact(datestr, formatsrc, CultureInfo.InvariantCulture);
                            await Task.Run(() => File.AppendAllText(myFile, ";| " + _currentHostName + " | " + fi.FullName + " | " + fi.Length + " | " + result.ToString(formatdst) + " | " + timestr + " |\n", Coder));
                            _ProgressWork1();
                        }
                    } catch { continue; }
                }
                foreach (string str in subDirs)
                { dirs.Push(str); }
            }

            FileInfo fi2 = new FileInfo(myFile);
            if (fi2.Length > 25)
            {
                File.AppendAllText(myFileReady, "");
                waitFile.Set();
            }
            else
            { fi2.Delete(); }

            if (stop == 1)         //Perhaps this block doesn't need!!!!!!!!!!!!!
            {
                await Task.Run(() => File.AppendAllText(Application.StartupPath + "\\myEventLoger\\tmp\\tmpfile.stop", ""));
                //  waitStop.Set(); 
            }
        }

        private async Task _TempFoundDocumentsToDB(string myFile, string myFileReady) // Search Document at disks by its pattern
        {
            DirectoryInfo fi = new DirectoryInfo(myFile);
            // Task t;
            FileInfo fi1 = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
            databaseHost = fi1.FullName;

            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand("begin", connection);
                try { command.ExecuteNonQuery(); } catch { };

                var Coder = Encoding.GetEncoding(65001);
                var Reader = new StreamReader(myFile, Coder);
                string sr = null, mystatus = "0";
                try
                {
                    while ((sr = Reader.ReadLine()) != null)
                    {
                        if (mystatus == "1" && sr.ToLower().Contains("\\windows\\") == false && sr.ToLower().Contains("\\programdata\\") == false && sr.ToLower().Contains("program files") == false)
                        {
                            string[] s = Regex.Split(sr, "[|]");
                            command = new SQLiteCommand("INSERT INTO 'FoundDocuments' ('ComputerName', 'DocumentPath', 'Length', 'Date', 'Time') VALUES (@ComputerName, @DocumentPath, @Length, @Date, @Time)", connection);

                            command.Parameters.AddWithValue("@ComputerName", s[1].Trim());
                            command.Parameters.AddWithValue("@DocumentPath", s[2].Trim());
                            command.Parameters.AddWithValue("@Length", (Convert.ToInt64(s[3].Trim()) / 1024).ToString());
                            command.Parameters.AddWithValue("@Date", s[4].Trim());
                            command.Parameters.AddWithValue("@Time", s[5].Trim());
                            try { command.ExecuteNonQuery(); } catch { };

                            _ProgressWork1();
                        }
                        if (sr.Contains(";Table:FoundDocuments"))
                        { mystatus = "1"; }
                    }
                } catch { }
                Reader.Close();
                command = new SQLiteCommand("end", connection);
                try { command.ExecuteNonQuery(); } catch { };

                connection.Close();
            }

            //clear temporary files
            if (File.Exists(myFileReady))
            { await Task.Run(() => File.AppendAllText(Application.StartupPath + "\\myEventLoger\\ready\\tmpfile.stop", "")); }

            FileInfo fi2 = new FileInfo(myFileReady);
            try { await Task.Run(() => fi2.Delete()); } catch { }
            fi2 = new FileInfo(myFile);
            try { await Task.Run(() => fi2.Delete()); } catch { }

            try { await Task.Run(() => File.Delete(fi.FullName)); } catch { }
        }

        //-------------/\/\/\/\------------// Searching of files. End of The Block //-------------/\/\/\/\------------// 





        //-------------/\/\/\/\------------// Scanning of Registry. Start of The Block //-------------/\/\/\/\------------// 

        private async void regScanItem_Click(object sender, EventArgs e)
        {
            textBoxToTemporary();
            _currentRemoteRegistryRunning = true;

            if (tabControl.SelectedTab == tabDataGrid)
            { bool str = ParseSelectedCell(); }
            else
            { ParseTextboxInputNetOrPC(); }

            ProgressBar1.Value = 0;

            Thread t = new Thread(new ThreadStart(_StatusLabel2ChangeColor));
            t.Start();
            Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));
            StatusLabel2.Text = "Готовлю базу... ";

            StopSearchItem.Enabled = true;
            timer1Start();
            _StopScannig = false;
            waitNetPing = new AutoResetEvent(false);
            waitFile = new AutoResetEvent(false);
            waitStop = new AutoResetEvent(false);
            waitNetStop = new AutoResetEvent(false);
            waitFilePing = new AutoResetEvent(false);

            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
            databaseHost = fi.FullName;
            _DBCheckFull(_currentHostName);
            Task.Delay(200).Wait();
            _GetTimeRunScan();

            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection))
                {
                    command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                    command.Parameters.Add(new SQLiteParameter("@Name", "ComputerIP"));
                    command.Parameters.AddWithValue("@Value", _currentHostIP);
                    command.Parameters.AddWithValue("@Value1", _currentHostName);
                    command.Parameters.AddWithValue("@Date", _Date);
                    command.Parameters.AddWithValue("@Time", _Time);
                    try { command.ExecuteNonQuery(); } catch { }
                }
                connection.Close();
            }

            NetworkShare share = new NetworkShare(_currentHostName, "ipc$", _User, _Password);
            share.Connect();
            Task.Delay(200).Wait();
            checkRunningRemoteRegistry(_currentHostName, _currentHostIP, _User, _Password); //Лучше использовать IP
            _ProgressWork5();

            IPAddress addr = IPAddress.Parse(_currentHostIP);
            _getUserAndSN(_currentHostName, addr);
            _GetTimeRunScan();
            string[] s = Regex.Split(TempConfigSingleHost, @"[|]");

            Task.Delay(200).Wait();
            StatusLabel2.Text = "Обновляю данные в базе... ";
            string TempComputerName = "", TempComputerIP = "", TempComputerModel = "", TempComputerSN = "", TempLogOnUser = "", TempComputerNameShort = "", TempComputerDomainName = "";
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}; journal_mode =MEMORY;", databaseAliveHosts)))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("SELECT ComputerName, ComputerNameShort, ComputerDomainName, ComputerIP, ComputerModel, ComputerSN, LogOnUser, Date FROM AliveHosts Where ComputerName like '%" + _currentHostName + "%' ;", connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        foreach (DbDataRecord record in reader)
                        {
                            TempComputerName = record["ComputerName"].ToString();
                            TempComputerIP = record["ComputerIP"].ToString();
                            TempComputerModel = record["ComputerModel"].ToString();
                            TempComputerSN = record["ComputerSN"].ToString();
                            TempLogOnUser = record["LogOnUser"].ToString();
                            TempComputerNameShort = record["ComputerNameShort"].ToString();
                            TempComputerDomainName = record["ComputerDomainName"].ToString();
                        }
                    }
                }

                if (!s[1].Trim().ToLower().Contains("unknow") && TempComputerName.Trim().Length > 0)
                {
                    TempComputerIP = s[2].Trim();
                    if (!s[4].ToLower().Contains("permission") || !s[5].ToLower().Contains("permission"))
                    {
                        TempComputerModel = s[4].Trim();
                        TempComputerSN = s[5].Trim();
                        TempLogOnUser = s[6].Trim();
                    }

                    using (SQLiteCommand command = new SQLiteCommand("UPDATE AliveHosts SET ComputerNameShort=@ComputerNameShort, ComputerDomainName=@ComputerDomainName, ComputerIP=@ComputerIP, ComputerModel=@ComputerModel," +
                            " ComputerSN=@ComputerSN, LogOnUser=@LogOnUser, ScannerName=@ScannerName, ScannerIP=@ScannerIP, Date=@Date, Time=@Time WHERE ComputerName=@ComputerName LIMIT 1", connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@ComputerNameShort", s[1].Trim()));
                        command.Parameters.Add(new SQLiteParameter("@ComputerIP", TempComputerIP));
                        command.Parameters.Add(new SQLiteParameter("@ComputerDomainName", TempComputerDomainName));
                        command.Parameters.Add(new SQLiteParameter("@ComputerModel", TempComputerModel));
                        command.Parameters.Add(new SQLiteParameter("@ComputerSN", TempComputerSN));
                        command.Parameters.Add(new SQLiteParameter("@LogOnUser", TempLogOnUser));
                        command.Parameters.Add(new SQLiteParameter("@ScannerName", _myHostName));
                        command.Parameters.Add(new SQLiteParameter("@ScannerIP", _myIP));
                        command.Parameters.Add(new SQLiteParameter("@Date", _Date));
                        command.Parameters.Add(new SQLiteParameter("@Time", _Time));
                        try { command.ExecuteNonQuery(); } catch { };
                    }
                }
                else if (!s[1].Trim().ToLower().Contains("unknow") && TempComputerName.Trim().Length == 0)
                {
                    TempComputerIP = s[2].Trim();
                    TempComputerModel = s[4].Trim();
                    TempComputerSN = s[5].Trim();
                    TempLogOnUser = s[6].Trim();

                    using (SQLiteCommand command = new SQLiteCommand("INSERT INTO AliveHosts ('ComputerName', 'ComputerNameShort', 'ComputerDomainName', 'ComputerIP', 'ComputerModel', 'ComputerSN', 'LogOnUser', 'ScannerName', 'ScannerIP', 'Date', 'Time') VALUES (@ComputerName, @ComputerNameShort, @ComputerDomainName, @ComputerIP, @ComputerModel, @ComputerSN, @LogOnUser, @ScannerName, @ScannerIP, @Date, @Time)", connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@ComputerName", s[0].Trim()));
                        command.Parameters.Add(new SQLiteParameter("@ComputerNameShort", s[1].Trim()));
                        command.Parameters.Add(new SQLiteParameter("@ComputerIP", TempComputerIP));
                        command.Parameters.Add(new SQLiteParameter("@ComputerDomainName", TempComputerDomainName));
                        command.Parameters.Add(new SQLiteParameter("@ComputerModel", TempComputerModel));
                        command.Parameters.Add(new SQLiteParameter("@ComputerSN", TempComputerSN));
                        command.Parameters.Add(new SQLiteParameter("@LogOnUser", TempLogOnUser));
                        command.Parameters.Add(new SQLiteParameter("@ScannerName", _myHostName));
                        command.Parameters.Add(new SQLiteParameter("@ScannerIP", _myIP));
                        command.Parameters.Add(new SQLiteParameter("@Date", _Date));
                        command.Parameters.Add(new SQLiteParameter("@Time", _Time));
                        try { command.ExecuteNonQuery(); } catch { };
                    }
                }
            }

            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection))
                {
                    command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                    command.Parameters.AddWithValue("@Name", "ComputerIP");
                    command.Parameters.AddWithValue("@Value", _currentHostIP);
                    command.Parameters.AddWithValue("@Value1", _currentHostName);
                    command.Parameters.AddWithValue("@Date", _Date);
                    command.Parameters.AddWithValue("@Time", _Time);
                    try { command.ExecuteNonQuery(); } catch { }
                }
            }

            share.Disconnect();
            Task.Delay(1000).Wait();

            await Task.Run(() => _loadRegDocuments());

            Task.WaitAll();
            _ControlVisibleLicense(false);
            _ControlVisibleStartUP(false);
            _ControlVisibleAnalyse(true);
            StatusLabel2.ForeColor = System.Drawing.Color.Black;
        }

        //---------------- load Remote HIVE ------------
        //https://social.msdn.microsoft.com/Forums/vstudio/en-US/d0d485b8-c3d1-49d0-8180-0515d9cfb04e/read-and-modify-ntuserdat-file?forum=csharpgeneral

        private void _loadRegDocuments() //Take of features of the online user form The Registry. Running Scanning Registry offline and online Users
        {
            _StatusLabel2Text("Устанавливаю соединение... ");
            try
            {
                NetworkShare shareconnection = new NetworkShare(_currentHostName, "ipc$");
                if (_currentHostIP != _myIP)
                {
                    shareconnection = new NetworkShare(_currentHostName, "ipc$", _User, _Password);
                    shareconnection.Connect();  // Connect to the remote drive
                    Task.Delay(200).Wait();
                    Task.WaitAll();
                    checkRunningRemoteRegistry(_currentHostName, _currentHostIP, _User, _Password);
                }
                Task.Delay(200).Wait();

                string UserProfiles = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\";
                string a1 = "", a2 = "", a3 = "", ProfileKey = "";
                _StatusLabel2Text("Проверяю реестр... ");
                _textBoxLogs("Проверяю реестр... \n");
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
                {
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand("");

                    if (_currentHostName != _myHostName && _currentRemoteRegistryRunning)
                    {
                        using (RegistryKey Rkey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, _currentHostName))
                        {
                            try
                            {
                                using (RegistryKey key = Rkey.OpenSubKey(UserProfiles, RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey))
                                {
                                    string[] subNames = key.GetSubKeyNames();
                                    foreach (string s in subNames)
                                    {
                                        if (s != null)
                                        {
                                            using (RegistryKey profilekey = key.OpenSubKey(s, RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey))
                                            {
                                                foreach (string valueName in profilekey.GetValueNames())
                                                {
                                                    a1 = valueName.ToString();
                                                    if (a1.Contains("ProfileImagePath") && s.ToString().Length > 10)
                                                    {
                                                        a2 = s.ToString();
                                                        a3 = profilekey.GetValue(valueName).ToString().Replace("\\", "|").Replace(":", "");
                                                        _textBoxLogs("ProfileImagePath    a2 - " + a2 + " | " + a3 + " - a3 \n");
                                                        //ProfileImagePath    a2 - S-1-5-21-872215606-2233830010-1533580533-1000 | C|Users|admin - a3 
                                                        // записывать в WindowsFeauture, UserNameOnline , UserNameOfline

                                                        command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                                            " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                                                        command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                                                        command.Parameters.AddWithValue("@Name", "UserName: " + Regex.Split(a3, "[|]")[2]);
                                                        command.Parameters.AddWithValue("@Value", "\\\\" + _currentHostName + "\\" + Regex.Split(a3, "[|]")[0] + "$\\" + Regex.Split(a3, "[|]")[1] + "\\" + Regex.Split(a3, "[|]")[2]);
                                                        command.Parameters.AddWithValue("@Value1", s.ToString());
                                                        command.Parameters.AddWithValue("@Date", _Date);
                                                        command.Parameters.AddWithValue("@Time", _Time);
                                                        try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            } catch { }
                        }

                        string[] tmpkeys = Regex.Split(a3, "[|]");
                        ProfileKey = "\\\\" + _currentHostName + "\\" + tmpkeys[0] + "$\\";

                        for (int i = 1; i < tmpkeys.Length - 1; i++)
                        {
                            ProfileKey += tmpkeys[i];
                            if (i < tmpkeys.Length - 2)
                            { ProfileKey += "\\"; }
                        }
                    }
                    else
                    {
                        using (RegistryKey regUseProfilesrKey = Registry.LocalMachine.OpenSubKey(UserProfiles, RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey))
                        {
                            try
                            {
                                string[] subNames = regUseProfilesrKey.GetSubKeyNames();
                                foreach (string s in subNames)
                                {
                                    if (s != null)
                                    {
                                        using (RegistryKey profilekey = regUseProfilesrKey.OpenSubKey(s, RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey))
                                        {
                                            try
                                            {
                                                foreach (string valueName in profilekey.GetValueNames())
                                                {
                                                    a1 = valueName.ToString();
                                                    if (a1.Contains("ProfileImagePath") && s.ToString().Length > 10)
                                                    {
                                                        a2 = s.ToString();
                                                        a3 = profilekey.GetValue(valueName).ToString();
                                                        _textBoxLogs("ProfileImagePath    a2 - " + a2 + " | " + a3 + " - a3 \n");

                                                        command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                                                " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                                                        command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                                                        command.Parameters.AddWithValue("@Name", "UserName: " + Regex.Split(a3, "[|]")[2]);
                                                        command.Parameters.AddWithValue("@Value", a3.ToString());
                                                        command.Parameters.AddWithValue("@Value1", s.ToString());
                                                        command.Parameters.AddWithValue("@Date", _Date);
                                                        command.Parameters.AddWithValue("@Time", _Time);
                                                        try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }

                                                    }
                                                }
                                            } catch { }
                                        }
                                    }
                                }
                            } catch { }
                        }
                        ProfileKey = a3.ToString();
                    }
                    command.Dispose();
                    connection.Close();
                }
                _loadRegRemoteHiveOffline(_currentHostName, ProfileKey);
                _loadRegRemoteHiveOnlineUser(_currentHostName, @"Volatile Environment", @"USERNAME"); //Заремлен на время теста

                Task.WaitAll();
                Task.Delay(200).Wait();
                shareconnection.Disconnect();
            } catch (Exception exp) { MessageBox.Show(exp.ToString()); }

            _RunTimer = false;
            _StopScannig = true;
            timer1Stop();
            _ProgressBar1Value100();
            _StatusLabel2ForeColor(System.Drawing.Color.Black);
            _StatusLabel2Text("Поиск в реестрах пользователей завершен!");
        }

        private void _loadRegRemoteHiveOffline(string HostName, string UserPath)       //offline Registry users
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + HostName + @".db");
            databaseHost = fi.FullName;
            _StatusLabel2Text("Ищу следы файлов в реестрах оффлайн пользователей... ");

            NetworkShare shareconnection = new NetworkShare(_currentHostName, "ipc$", _User, _Password);
            shareconnection.Connect();  // Connect to the remote drive

            try
            {
                _GetTimeRunScan();
                string alluserspath = "";

                StringBuilder path = new StringBuilder(4 * 1024);
                int size = path.Capacity;
                if (GetProfilesDirectory(path, ref size))
                { alluserspath = path.ToString(); }

                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
                {
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                                             " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                    command.Parameters.AddWithValue("@ComputerName", HostName);
                    command.Parameters.Add(new SQLiteParameter("@Name", "UserNameOfline: AllUsers"));
                    command.Parameters.AddWithValue("@Value", alluserspath);
                    command.Parameters.AddWithValue("@Value1", "ProfilesPath");
                    command.Parameters.AddWithValue("@Date", _Date);
                    command.Parameters.AddWithValue("@Time", _Time);
                    try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }

                    if (Directory.Exists(UserPath))
                    {
                        int i = 0;
                        try
                        {
                            string[] sub_dirs = Directory.GetDirectories(UserPath);
                            foreach (string dir in sub_dirs)
                            {
                                i++;
                                string wimHivePath = Path.Combine(dir, "ntuser.dat");        // full path to "ntuser.dat"
                                FileInfo fin = new FileInfo(wimHivePath);  //проверять размер файла
                                if (fin.Length > 50000)  //use only file with length more then 50k 
                                {
                                    string loadedHiveKey = RegistryInterop.Load(wimHivePath, "RemoteRegLoad" + i);    // registryregload
                                    try
                                    {
                                        using (RegistryKey rk = Registry.Users.OpenSubKey(loadedHiveKey))   // hkey_users\registryregload
                                        {
                                            _currentUser = GetParentName(wimHivePath);
                                            _currentUserSID = "Offline User";
                                            _currentUserProfilePath = GetParentFullName(wimHivePath);

                                            command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                                " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                                            command.Parameters.AddWithValue("@ComputerName", HostName);
                                            command.Parameters.AddWithValue("@Name", "UserNameOfline: " + _currentUser);
                                            command.Parameters.AddWithValue("@Value", _currentUserProfilePath);
                                            command.Parameters.AddWithValue("@Value1", "Offline User");
                                            command.Parameters.AddWithValue("@Date", _Date);
                                            command.Parameters.AddWithValue("@Time", _Time);
                                            try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }

                                            command = new SQLiteCommand("begin", connection);
                                            command.ExecuteNonQuery();

                                            //MSOffice
                                            bool SoftExist = false;
                                            _StatusLabel2Text("Ищу следы MS Office в реестре... ");
                                            string Software = "MicrosoftOffice";
                                            foreach (string pr in MSOffProgramms)
                                            {
                                                foreach (string pl in MSOffPlace)
                                                {
                                                    foreach (string n in MSOffNumber)
                                                    {
                                                        using (RegistryKey rk2 = Registry.Users.OpenSubKey(loadedHiveKey + @"\SOFTWARE\Microsoft\Office\" + n + @".0\" + pr + @"\" + pl))
                                                        {
                                                            try
                                                            {
                                                                foreach (string ValueName in rk2.GetValueNames())
                                                                {
                                                                    if (rk2.GetValue(ValueName) != null && ValueName.ToLower().Contains("display") != true)
                                                                    {
                                                                        _textBoxLogs("rk2" + rk2.ToString() + "subkeyName3" + ValueName + "\n");

                                                                        string myPattern = "]."; //MS Office
                                                                        string a2 = ValueName.ToString();
                                                                        string[] a4 = Regex.Split(rk2.GetValue(ValueName).ToString(), myPattern);
                                                                        string a3 = a4[a4.Length - 1];
                                                                        _textBoxLogs("a3  " + a3 + "   a2  " + a2 + "\n");

                                                                        command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                            " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                        command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                        command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                        command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                        command.Parameters.AddWithValue("@DocumentPath", a3);
                                                                        command.Parameters.AddWithValue("@Software", Software);
                                                                        command.Parameters.AddWithValue("@Date", _Date);
                                                                        command.Parameters.AddWithValue("@Time", _Time);
                                                                        try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                                                        SoftExist = true;
                                                                    }
                                                                }
                                                            } catch { }
                                                        }
                                                    }
                                                }
                                            }

                                            if (!SoftExist && _currentUserSID.Trim().Length > 0 && _currentUser.Trim().Length > 0)
                                            {
                                                /*                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                                                  " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                                                command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                                */
                                            }
                                            command = new SQLiteCommand("end", connection);
                                            command.ExecuteNonQuery();
                                            SoftExist = false;

                                            command = new SQLiteCommand("begin", connection);
                                            command.ExecuteNonQuery();

                                            //Adobe
                                            _StatusLabel2Text("Ищу следы Adobe Reader в реестре... ");
                                            Software = "Adobe";
                                            foreach (string an in AdobeNumber)
                                            {
                                                for (int ia = 1; ia < 20; ia++)
                                                {
                                                    using (RegistryKey rk2 = Registry.Users.OpenSubKey(loadedHiveKey + @"\SOFTWARE\Adobe\Acrobat Reader\" + an + @".0\AVGeneral\cRecentFiles\c" + ia))
                                                    {
                                                        try
                                                        {
                                                            foreach (string valueName in rk2.GetValueNames())
                                                            {
                                                                if (rk2.GetValue(valueName).ToString() != null)
                                                                {
                                                                    string myPattern = "].";
                                                                    string a1 = "";
                                                                    if (valueName.ToLower().Contains("item"))
                                                                    {
                                                                        string[] substring = Regex.Split(rk2.GetValue(valueName).ToString(), myPattern);
                                                                        a1 = substring[substring.Length - 1];
                                                                    }
                                                                    if (valueName.ToLower().Contains("file") || valueName.ToLower().Contains("text") || rk2.ToString().ToLower().Contains("rar"))
                                                                    { a1 = rk2.GetValue(valueName).ToString(); }

                                                                    if (a1.Trim().Length > 0)
                                                                    {
                                                                        command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                            " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                        command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                        command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                        command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                        command.Parameters.AddWithValue("@DocumentPath", a1.Trim().ToString());
                                                                        command.Parameters.AddWithValue("@Software", Software);
                                                                        command.Parameters.AddWithValue("@Date", _Date);
                                                                        command.Parameters.AddWithValue("@Time", _Time);
                                                                        try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                        SoftExist = true;
                                                                    }
                                                                }
                                                            }
                                                        } catch { }
                                                    }
                                                }
                                            }
                                            if (!SoftExist && _currentUserSID.Trim().Length > 0 && _currentUser.Trim().Length > 0)
                                            {
                                                /*                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                                                  " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                                                command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                                */
                                            }
                                            SoftExist = false;

                                            command = new SQLiteCommand("end", connection);
                                            command.ExecuteNonQuery();

                                            command = new SQLiteCommand("begin", connection);
                                            command.ExecuteNonQuery();

                                            //Explorer
                                            _StatusLabel2Text("Ищу следы файлов открывавшихся Проводником в реестре... ");
                                            //Поиск в ключах подпапок и ключах подпапок
                                            //HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders  Desktop -  C:\Users\user\Desktop   Personal - C:\Users\user\Documents
                                            Software = "Explorer.RecentDocs";
                                            using (RegistryKey rk2 = Registry.Users.OpenSubKey(loadedHiveKey + @"\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\RecentDocs"))
                                            {
                                                try
                                                {
                                                    foreach (string valueName in rk2.GetValueNames())
                                                    {
                                                        if (rk2.GetValue(valueName).ToString() != null)
                                                        {
                                                            string a1 = "";
                                                            if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString && !valueName.ToLower().Contains("mrulistex"))
                                                            {
                                                                object o = rk2.GetValue(valueName);
                                                                foreach (string st in (string[])o)
                                                                { a1 += st + " "; }
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary && !valueName.ToLower().Contains("mrulistex"))
                                                            {
                                                                byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                                a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.String && !valueName.ToLower().Contains("mrulistex"))
                                                            { a1 = rk2.GetValue(valueName).ToString(); }

                                                            if (a1.Length > 0)
                                                            {
                                                                string pattern = @"([^А-Яа-яіІїЇєЄA-Za-z0-9\!\|\\\#\@\~\$\%\(\)\ \.\]\[\:\{\}\?])";
                                                                string c = Regex.Replace(a1, pattern, " ");
                                                                string b = Regex.Replace(c, "  ", " ");
                                                                while (b.Contains("  "))
                                                                { b = Regex.Replace(b, "  ", " "); }
                                                                a1 = b.Trim();

                                                                if (a1.Length > 4)
                                                                {
                                                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                        " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                    command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                    command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                    command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                    command.Parameters.AddWithValue("@DocumentPath", Convert.ToString(a1));
                                                                    command.Parameters.AddWithValue("@Software", Software);
                                                                    command.Parameters.AddWithValue("@Date", _Date);
                                                                    command.Parameters.AddWithValue("@Time", _Time);
                                                                    try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                    SoftExist = true;
                                                                }
                                                            }
                                                        }
                                                    }
                                                } catch { }
                                            }

                                            Software = "Explorer.RunMRU";
                                            using (RegistryKey rk2 = Registry.Users.OpenSubKey(loadedHiveKey + @"\Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU"))
                                            {
                                                try
                                                {
                                                    foreach (string valueName in rk2.GetValueNames())
                                                    {
                                                        if (rk2.GetValue(valueName).ToString() != null && !valueName.ToLower().Contains("mrulistex"))
                                                        {
                                                            string a1 = "";
                                                            if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                            {
                                                                object o = rk2.GetValue(valueName);
                                                                foreach (string st in (string[])o)
                                                                { a1 += st + " "; }
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                            {
                                                                byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                                a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                            { a1 = rk2.GetValue(valueName).ToString(); }

                                                            if (a1.Length > 0)
                                                            {
                                                                string pattern = @"([^А-Яа-яіІїЇєЄA-Za-z0-9\!\|\\\#\@\~\$\%\(\)\ \.\]\[\:\{\}\?])";
                                                                string c = Regex.Replace(a1, pattern, " ");
                                                                string b = Regex.Replace(c, "  ", " ");
                                                                while (b.Contains("  "))
                                                                { b = Regex.Replace(b, "  ", " "); }
                                                                a1 = b.Trim();

                                                                if (a1.Length > 4)
                                                                {
                                                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                        " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                    command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                    command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                    command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                    command.Parameters.AddWithValue("@DocumentPath", Convert.ToString(a1));
                                                                    command.Parameters.AddWithValue("@Software", Software);
                                                                    command.Parameters.AddWithValue("@Date", _Date);
                                                                    command.Parameters.AddWithValue("@Time", _Time);
                                                                    try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                    SoftExist = true;
                                                                }
                                                            }
                                                        }
                                                    }
                                                } catch { }
                                            }

                                            Software = "Explorer.ComDlg32";
                                            using (RegistryKey RTemp = Registry.Users.OpenSubKey(loadedHiveKey + @"\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32"))
                                            {
                                                try
                                                {
                                                    foreach (string valueNames in RTemp.GetSubKeyNames())
                                                    {
                                                        using (RegistryKey RTemp2 = Registry.Users.OpenSubKey(loadedHiveKey + @"\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\" + valueNames))
                                                        {
                                                            try
                                                            {
                                                                foreach (string valueName in RTemp2.GetValueNames())
                                                                {
                                                                    if (RTemp2.GetValue(valueName).ToString() != null && !RTemp2.GetValue(valueName).ToString().ToLower().Contains("mrulistex"))
                                                                    {
                                                                        string a1 = "";
                                                                        if (RTemp2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                                        {
                                                                            object o = RTemp2.GetValue(valueName);
                                                                            foreach (string st in (string[])o)
                                                                            { a1 += st + " "; }
                                                                        }
                                                                        else if (RTemp2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                                        {
                                                                            byte[] s2 = (byte[])RTemp2.GetValue(valueName);
                                                                            a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                                        }
                                                                        else if (RTemp2.GetValueKind(valueName) == RegistryValueKind.String)
                                                                        { a1 = RTemp2.GetValue(valueName).ToString(); }

                                                                        if (a1.Length > 4)
                                                                        {
                                                                            string pattern = @"([^А-Яа-яіІїЇєЄA-Za-z0-9\!\|\\\#\@\~\$\%\(\)\ \.\]\[\:\{\}\?])";
                                                                            string c = Regex.Replace(a1, pattern, " ");
                                                                            string b = Regex.Replace(c, "  ", " ");
                                                                            while (b.Contains("  "))
                                                                            { b = Regex.Replace(b, "  ", " "); }
                                                                            a1 = b.Trim();

                                                                            if (a1.Length > 4)
                                                                            {
                                                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                                    " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                                command.Parameters.AddWithValue("@DocumentPath", Convert.ToString(a1));
                                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                                SoftExist = true;
                                                                            }
                                                                        }
                                                                    }
                                                                }

                                                                foreach (string RvalueNames in RTemp2.GetSubKeyNames())
                                                                {
                                                                    using (RegistryKey RTemp4 = Registry.Users.OpenSubKey(loadedHiveKey + @"\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\" + valueNames + @"\" + RvalueNames))
                                                                    {
                                                                        try
                                                                        {
                                                                            foreach (string valueName in RTemp4.GetValueNames())
                                                                            {
                                                                                if (RTemp4.GetValue(valueName).ToString() != null && RTemp4.GetValue(valueName).ToString().ToLower().Contains("mrulistex") != true)
                                                                                {
                                                                                    string a1 = "";
                                                                                    if (RTemp4.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                                                    {
                                                                                        object o = RTemp4.GetValue(valueName);
                                                                                        foreach (string st in (string[])o)
                                                                                        { a1 += st + " "; }
                                                                                    }
                                                                                    else if (RTemp4.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                                                    {
                                                                                        byte[] s2 = (byte[])RTemp4.GetValue(valueName);
                                                                                        a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                                                    }
                                                                                    else if (RTemp4.GetValueKind(valueName) == RegistryValueKind.String)
                                                                                    { a1 = RTemp4.GetValue(valueName).ToString(); }

                                                                                    if (a1.Trim().Length > 4)
                                                                                    {
                                                                                        string pattern = @"([^А-Яа-яіІїЇєЄA-Za-z0-9\!\|\\\#\@\~\$\%\(\)\ \.\]\[\:\{\}\?])";
                                                                                        string c = Regex.Replace(a1, pattern, " ");
                                                                                        string b = Regex.Replace(c, "  ", " ");
                                                                                        while (b.Contains("  "))
                                                                                        { b = Regex.Replace(b, "  ", " "); }
                                                                                        a1 = b.Trim();

                                                                                        if (a1.Length > 4)
                                                                                        {
                                                                                            command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                                                " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                                            command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                                            command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                                            command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                                            command.Parameters.AddWithValue("@DocumentPath", RvalueNames + " | " + Convert.ToString(a1));
                                                                                            command.Parameters.AddWithValue("@Software", Software);
                                                                                            command.Parameters.AddWithValue("@Date", _Date);
                                                                                            command.Parameters.AddWithValue("@Time", _Time);
                                                                                            try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                                            SoftExist = true;
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        } catch { }
                                                                    }
                                                                }
                                                            } catch { }
                                                        }
                                                    }
                                                } catch { }
                                            }
                                            if (!SoftExist && _currentUserSID.Trim().Length > 0 && _currentUser.Trim().Length > 0)
                                            {
                                                /*                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                                                  " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                                                command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                                */
                                            }
                                            SoftExist = false;
                                            command = new SQLiteCommand("end", connection);
                                            command.ExecuteNonQuery();

                                            _StatusLabel2Text("Ищу следы другого ПО в реестре... ");
                                            command = new SQLiteCommand("begin", connection);
                                            command.ExecuteNonQuery();

                                            //Paint
                                            Software = "Paint";
                                            using (RegistryKey rk2 = Registry.Users.OpenSubKey(loadedHiveKey + @"\SOFTWARE\Microsoft\Windows\CurrentVersion\Applets\Paint\Recent File List"))
                                            {
                                                try
                                                {
                                                    foreach (string valueName in rk2.GetValueNames())
                                                    {
                                                        if (rk2.GetValue(valueName).ToString() != null)
                                                        {
                                                            string a1 = "";
                                                            if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                            {
                                                                object o = rk2.GetValue(valueName);
                                                                foreach (string st in (string[])o)
                                                                { a1 += st + " "; }
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                            {
                                                                byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                                a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                            { a1 = rk2.GetValue(valueName).ToString(); }

                                                            if (a1.Length > 0)
                                                            {
                                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                    " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                command.Parameters.AddWithValue("@DocumentPath", a1.Trim().ToString());
                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                SoftExist = true;
                                                            }
                                                        }
                                                    }
                                                } catch { }
                                            }
                                            if (!SoftExist && _currentUserSID.Trim().Length > 0 && _currentUser.Trim().Length > 0)
                                            {
                                                /*                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                                                  " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                                                command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                                */
                                            }
                                            SoftExist = false;

                                            //WinRAR
                                            Software = "WinRAR";
                                            using (RegistryKey rk2 = Registry.Users.OpenSubKey(loadedHiveKey + @"\SOFTWARE\WinRAR\ArcHistory"))
                                            {
                                                try
                                                {
                                                    foreach (string valueName in rk2.GetValueNames())
                                                    {
                                                        if (rk2.GetValue(valueName).ToString() != null)
                                                        {
                                                            string a1 = "";
                                                            if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                            {
                                                                object o = rk2.GetValue(valueName);
                                                                foreach (string st in (string[])o)
                                                                { a1 += st + " "; }
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                            {
                                                                byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                                a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                            { a1 = rk2.GetValue(valueName).ToString(); }

                                                            if (a1.Length > 0)
                                                            {
                                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                    " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                command.Parameters.AddWithValue("@DocumentPath", a1.Trim().ToString());
                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                SoftExist = true;
                                                            }
                                                        }
                                                    }
                                                } catch { }
                                            }
                                            using (RegistryKey rk2 = Registry.Users.OpenSubKey(loadedHiveKey + @"\SOFTWARE\WinRAR\DialogEditHistory\ArcName"))
                                            {
                                                try
                                                {
                                                    foreach (string valueName in rk2.GetValueNames())
                                                    {
                                                        if (rk2.GetValue(valueName).ToString() != null)
                                                        {
                                                            string a1 = "";
                                                            if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                            {
                                                                object o = rk2.GetValue(valueName);
                                                                foreach (string st in (string[])o)
                                                                { a1 += st + " "; }
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                            {
                                                                byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                                a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                            { a1 = rk2.GetValue(valueName).ToString(); }

                                                            if (a1.Length > 0)
                                                            {
                                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                    " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                command.Parameters.AddWithValue("@DocumentPath", a1.Trim().ToString());
                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                SoftExist = true;
                                                            }
                                                        }
                                                    }
                                                } catch { }
                                            }
                                            if (!SoftExist && _currentUserSID.Trim().Length > 0 && _currentUser.Trim().Length > 0)
                                            {
                                                /*                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                                                  " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                                                command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                                */
                                            }
                                            SoftExist = false;

                                            //WordPAD
                                            Software = "WordPAD";
                                            using (RegistryKey rk2 = Registry.Users.OpenSubKey(loadedHiveKey + @"\SOFTWARE\Microsoft\Windows\CurrentVersion\Applets\Wordpad\Recent File List"))
                                            {
                                                try
                                                {
                                                    foreach (string valueName in rk2.GetValueNames())
                                                    {
                                                        if (rk2.GetValue(valueName).ToString() != null)
                                                        {
                                                            string a1 = "";
                                                            if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                            {
                                                                object o = rk2.GetValue(valueName);
                                                                foreach (string st in (string[])o)
                                                                { a1 += st + " "; }
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                            {
                                                                byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                                a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                            { a1 = rk2.GetValue(valueName).ToString(); }

                                                            if (a1.Length > 0)
                                                            {
                                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                    " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                command.Parameters.AddWithValue("@DocumentPath", a1.Trim().ToString());
                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                SoftExist = true;
                                                            }
                                                        }
                                                    }
                                                } catch { }
                                            }
                                            if (!SoftExist && _currentUserSID.Trim().Length > 0 && _currentUser.Trim().Length > 0)
                                            {
                                                /*                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                                                  " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                                                command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                                */
                                            }
                                            SoftExist = false;

                                            //WinDjView
                                            Software = "WinDjView";
                                            using (RegistryKey rk2 = Registry.Users.OpenSubKey(loadedHiveKey + @"\SOFTWARE\Andrew Zhezherun\WinDjView\Tabs"))
                                            {
                                                try
                                                {
                                                    foreach (string valueName in rk2.GetValueNames())
                                                    {
                                                        if (rk2.GetValue(valueName).ToString() != null)
                                                        {
                                                            string a1 = "";
                                                            if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                            {
                                                                object o = rk2.GetValue(valueName);
                                                                foreach (string st in (string[])o)
                                                                { a1 += st + " "; }
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                            {
                                                                byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                                a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                            { a1 = rk2.GetValue(valueName).ToString(); }

                                                            if (a1.Length > 0)
                                                            {
                                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                    " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                command.Parameters.AddWithValue("@DocumentPath", a1.Trim().ToString());
                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                SoftExist = true;
                                                            }
                                                        }
                                                    }
                                                } catch { }
                                            }
                                            using (RegistryKey rk2 = Registry.Users.OpenSubKey(loadedHiveKey + @"\SOFTWARE\Andrew Zhezherun\WinDjView\Recent File List"))
                                            {
                                                try
                                                {
                                                    foreach (string valueName in rk2.GetValueNames())
                                                    {
                                                        if (rk2.GetValue(valueName).ToString() != null)
                                                        {
                                                            string a1 = "";
                                                            if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                            {
                                                                object o = rk2.GetValue(valueName);
                                                                foreach (string st in (string[])o)
                                                                { a1 += st + " "; }
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                            {
                                                                byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                                a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                            { a1 = rk2.GetValue(valueName).ToString(); }

                                                            if (a1.Length > 0)
                                                            {
                                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                    " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                command.Parameters.AddWithValue("@DocumentPath", a1.Trim().ToString());
                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                SoftExist = true;
                                                            }
                                                        }
                                                    }
                                                } catch { }
                                            }
                                            if (!SoftExist && _currentUserSID.Trim().Length > 0 && _currentUser.Trim().Length > 0)
                                            {
                                                /*                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                                                  " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                                                command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                                */
                                            }
                                            SoftExist = false;

                                            //AkelPad
                                            Software = "AkelPad";
                                            using (RegistryKey rk2 = Registry.Users.OpenSubKey(loadedHiveKey + @"\SOFTWARE\Akelsoft\AkelPad\Recent"))
                                            {
                                                try
                                                {
                                                    foreach (string valueName in rk2.GetValueNames())
                                                    {
                                                        if (rk2.GetValue(valueName).ToString() != null)
                                                        {
                                                            string a1 = "";
                                                            if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                            {
                                                                object o = rk2.GetValue(valueName);
                                                                foreach (string st in (string[])o)
                                                                { a1 += st + " "; }
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                            {
                                                                byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                                a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                            { a1 = rk2.GetValue(valueName).ToString(); }

                                                            if (a1.Length > 0)
                                                            {
                                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                    " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                command.Parameters.AddWithValue("@DocumentPath", a1.Trim().ToString());
                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                SoftExist = true;
                                                            }
                                                        }
                                                    }
                                                } catch { }
                                            }
                                            if (!SoftExist && _currentUserSID.Trim().Length > 0 && _currentUser.Trim().Length > 0)
                                            {
                                                /*                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                                                  " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                                                command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                                */
                                            }
                                            SoftExist = false;

                                            //VMWare
                                            Software = "VMWare";
                                            using (RegistryKey rk2 = Registry.Users.OpenSubKey(loadedHiveKey + @"\SOFTWARE\VMware\VMware Infrastructure Client\Preferences"))
                                            {
                                                try
                                                {
                                                    foreach (string valueName in rk2.GetValueNames())
                                                    {
                                                        if (rk2.GetValue(valueName).ToString() != null && valueName.ToLower().Contains("recentconnections"))
                                                        {
                                                            string a1 = "";
                                                            if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                            {
                                                                object o = rk2.GetValue(valueName);
                                                                foreach (string st in (string[])o)
                                                                { a1 += st + " "; }
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                            {
                                                                byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                                a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                            { a1 = rk2.GetValue(valueName).ToString(); }

                                                            if (a1.Length > 1)
                                                            {
                                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                    " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                command.Parameters.AddWithValue("@DocumentPath", a1.Trim());
                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                SoftExist = true;
                                                            }
                                                        }
                                                    }
                                                } catch { }
                                            }
                                            using (RegistryKey rk2 = Registry.Users.OpenSubKey(loadedHiveKey + @"\SOFTWARE\VMware\Virtual Infrastructure Client\Preferences\UI\ClientsXml"))
                                            {
                                                try
                                                {
                                                    foreach (string valueName in rk2.GetValueNames())
                                                    {
                                                        if (rk2.GetValue(valueName).ToString() != null)
                                                        {
                                                            string a1 = "";
                                                            if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                            {
                                                                object o = rk2.GetValue(valueName);
                                                                foreach (string st in (string[])o)
                                                                { a1 += st + " "; }
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                            {
                                                                byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                                a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                            }
                                                            else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                            { a1 = rk2.GetValue(valueName).ToString(); }

                                                            if (a1.Length > 0)
                                                            {
                                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                    " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                command.Parameters.AddWithValue("@DocumentPath", valueName.Trim());
                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                SoftExist = true;
                                                            }
                                                        }
                                                    }
                                                } catch { }
                                            }
                                            if (!SoftExist && _currentUserSID.Trim().Length > 0 && _currentUser.Trim().Length > 0)
                                            {
                                                /*                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                                                  " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                                                command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                                */
                                            }
                                            SoftExist = false;

                                            //PuTTY
                                            Software = "PuTTY";
                                            using (RegistryKey RTemp = Registry.Users.OpenSubKey(loadedHiveKey + @"\SOFTWARE\SimonTatham\PuTTY\Sessions"))
                                            {
                                                try
                                                {
                                                    foreach (string valueNames in RTemp.GetSubKeyNames())
                                                    {
                                                        string RTemp1 = Path.Combine(@"SOFTWARE\SimonTatham\PuTTY\Sessions", valueNames);

                                                        using (RegistryKey RTemp2 = rk.OpenSubKey(RTemp1, RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey))
                                                        {
                                                            if (RTemp2 != null)
                                                            {
                                                                foreach (string valueName in RTemp2.GetValueNames())
                                                                {
                                                                    if (RTemp2.GetValue(valueName).ToString() != null && valueName.ToLower().Contains("hostname"))
                                                                    {
                                                                        string a1 = "";
                                                                        if (RTemp2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                                        {
                                                                            object o = RTemp2.GetValue(valueName);
                                                                            foreach (string st in (string[])o)
                                                                            { a1 += st + " "; }
                                                                        }
                                                                        else if (RTemp2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                                        {
                                                                            byte[] s2 = (byte[])RTemp2.GetValue(valueName);
                                                                            a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                                        }
                                                                        else if (RTemp2.GetValueKind(valueName) == RegistryValueKind.String)
                                                                        { a1 = RTemp2.GetValue(valueName).ToString(); }

                                                                        if (a1.Length > 0)
                                                                        {
                                                                            command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                                " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                            command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                            command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                            command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                            command.Parameters.AddWithValue("@DocumentPath", a1.Trim().ToString());
                                                                            command.Parameters.AddWithValue("@Software", Software);
                                                                            command.Parameters.AddWithValue("@Date", _Date);
                                                                            command.Parameters.AddWithValue("@Time", _Time);
                                                                            try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                            SoftExist = true;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                } catch { }
                                            }
                                            if (!SoftExist && _currentUserSID.Trim().Length > 0 && _currentUser.Trim().Length > 0)
                                            {
                                                /*                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                                                  " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                                                command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                                */
                                            }
                                            SoftExist = false;

                                            command = new SQLiteCommand("end", connection);
                                            command.ExecuteNonQuery();
                                            Task.Delay(200).Wait();
                                        }
                                    } catch //(Exception exp)
                                    {
                                        RegistryInterop.Unload("RemoteRegLoad" + i);
                                        //                MessageBox.Show(exp.ToString());
                                        if (_currentUserSID.Trim().Length > 0 && _currentUser.Trim().Length > 0)
                                        {
                                            command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                                                        " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                                            command.Parameters.AddWithValue("@ComputerName", HostName);
                                            command.Parameters.AddWithValue("@Name", "Error");
                                            command.Parameters.AddWithValue("@Value", "UserName: " + _currentUser);
                                            command.Parameters.AddWithValue("@Value1", "No Access to the offline Registry");
                                            command.Parameters.AddWithValue("@Date", _Date);
                                            command.Parameters.AddWithValue("@Time", _Time);
                                            try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                        }
                                    }
                                    Task.Delay(200).Wait();
                                    RegistryInterop.Unload("RemoteRegLoad" + i);
                                    Task.Delay(200).Wait();
                                }
                            }
                        } catch
                        {
                            //Записывать ошибки доступа к реестру
                        }
                    }
                    command.Dispose();
                    connection.Close();
                }
            } catch (Exception e) { MessageBox.Show(e.ToString()); }
            Task.Delay(200).Wait();
            shareconnection.Disconnect();
        }

        private string GetParentName(string path) //Take short Name of the Parent Path. Used in _loadRemoteHive()
        {
            try
            {
                System.IO.DirectoryInfo directoryInfo = System.IO.Directory.GetParent(path);
                return directoryInfo.Name.ToString();
            } catch { return path; }
        }

        private string GetParentFullName(string path) //Take Full Name of the Parent Path. Used in _loadRemoteHive()
        {
            try
            {
                System.IO.DirectoryInfo directoryInfo = System.IO.Directory.GetParent(path);
                return directoryInfo.FullName.ToString();
            } catch { return path; }
        }

        private void _loadRegRemoteHiveOnlineUser(string HostName, string KeyHive, string KeyValue, string Software = "none") //registry keys for online hive i.e. currently logged-in user
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + HostName + @".db");
            databaseHost = fi.FullName;
            _StatusLabel2Text("Ищу следы файлов в реестре онлайн пользователя... ");
            bool SoftExist = false;

            _currentUserSID = ""; _currentUser = "";
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
            {
                connection.Open();

                SQLiteCommand command = new SQLiteCommand("INSERT INTO 'AllTables' ('ComputerName', 'ComputerIP', 'NameTable', 'ScannerName', 'ScannerIP', 'Date', 'Time') " +
                    " VALUES (@ComputerName, @ComputerIP, @NameTable, @ScannerName, @ScannerIP, @Date, @Time)", connection);
                command.Parameters.AddWithValue("@ComputerName", HostName);
                command.Parameters.AddWithValue("@ComputerIP", _currentHostIP);
                command.Parameters.AddWithValue("@NameTable", "UsersDocuments");
                command.Parameters.AddWithValue("@ScannerName", _myHostName);
                command.Parameters.AddWithValue("@ScannerIP", _myIP);
                command.Parameters.AddWithValue("@Date", _Date);
                command.Parameters.AddWithValue("@Time", _Time);
                try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }

                try
                {
                    using (RegistryKey rk = RegistryKey.OpenRemoteBaseKey(RegistryHive.Users, HostName))
                    {
                        //(KeyHive - @"Volatile Environment", , KeyValue - @"USERNAME")
                        foreach (string s in rk.GetSubKeyNames())
                        {
                            string Rkey = Path.Combine(s, KeyHive);
                            if (Rkey != null)
                            {
                                using (RegistryKey rk2 = rk.OpenSubKey(Rkey, false))
                                {
                                    if (rk2 != null)
                                    {
                                        _currentUser = "";
                                        _currentUserSID = "";
                                        _currentUserProfilePath = "";
                                        foreach (string valueName in rk2.GetValueNames())
                                        {
                                            if (valueName.ToString().Contains(KeyValue))
                                            {
                                                _currentUser = rk2.GetValue(valueName).ToString();
                                                _currentUserSID = s.ToString();
                                            }

                                            if (valueName.ToString().Contains(@"USERNAME"))
                                            { _currentUser = rk2.GetValue(valueName).ToString(); }

                                            if (valueName.ToString().Contains(@"USERPROFILE"))
                                            { _currentUserProfilePath = rk2.GetValue(valueName).ToString(); }

                                            if (_currentUser.Length > 0 && _currentUserSID.Length > 0 && _currentUserProfilePath.Length > 0)
                                            {
                                                command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                                                    " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                command.Parameters.AddWithValue("@Name", "UserNameOnline: " + _currentUser);
                                                command.Parameters.AddWithValue("@Value", _currentUserProfilePath);
                                                command.Parameters.AddWithValue("@Value1", _currentUserSID);
                                                command.Parameters.AddWithValue("@Date", _Date);
                                                command.Parameters.AddWithValue("@Time", _Time);
                                                try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                            }
                                        }
                                    }
                                }
                                command = new SQLiteCommand("begin", connection);
                                command.ExecuteNonQuery();

                                //MSOffice
                                _StatusLabel2Text("Ищу в реестре следы файлов MSOffice... ");
                                Software = "MicrosoftOffice";
                                foreach (string pr in MSOffProgramms)
                                {
                                    foreach (string pl in MSOffPlace)
                                    {
                                        foreach (string n in MSOffNumber)
                                        {
                                            string Rkey2 = Path.Combine(s, @"SOFTWARE\Microsoft\Office\" + n + @".0\" + pr + @"\" + pl);
                                            using (RegistryKey rk2 = rk.OpenSubKey(Rkey2, false))
                                            {
                                                if (rk2 != null)
                                                {
                                                    foreach (string valueName in rk2.GetValueNames())
                                                    {
                                                        if (rk2.GetValue(valueName).ToString() != null && valueName.ToLower().Contains("display") != true)
                                                        {
                                                            string myPattern = "]."; //MS Office
                                                            string a1 = s.ToString();
                                                            string a2 = valueName.ToString();
                                                            string[] a4 = Regex.Split(rk2.GetValue(valueName).ToString(), myPattern);
                                                            string a3 = a4[a4.Length - 1];

                                                            command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                            command.Parameters.AddWithValue("@ComputerName", HostName);
                                                            command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                            command.Parameters.AddWithValue("@UserName", _currentUser);
                                                            command.Parameters.AddWithValue("@DocumentPath", a3);
                                                            command.Parameters.AddWithValue("@Software", Software);
                                                            command.Parameters.AddWithValue("@Date", _Date);
                                                            command.Parameters.AddWithValue("@Time", _Time);
                                                            try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                                            command.Dispose();
                                                            SoftExist = true;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (!SoftExist && _currentUser.Length > 0 && _currentUserSID.Length > 0)
                                {
                                    /*                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                          " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                        command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                        command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                        command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                        command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                        command.Parameters.AddWithValue("@Software", Software);
                                                                        command.Parameters.AddWithValue("@Date", _Date);
                                                                        command.Parameters.AddWithValue("@Time", _Time);
                                                                        try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                    */
                                }
                                SoftExist = false;
                                command = new SQLiteCommand("end", connection);
                                command.ExecuteNonQuery();

                                command = new SQLiteCommand("begin", connection);
                                command.ExecuteNonQuery();

                                //Adobe
                                _StatusLabel2Text("Ищу в реестре следы файлов Adobe PDF... ");
                                Software = "Adobe";
                                foreach (string an in AdobeNumber)
                                {
                                    for (int i = 1; i < 10; i++)
                                    {
                                        string Rkey2 = Path.Combine(s, @"SOFTWARE\Adobe\Acrobat Reader\" + an + @".0\AVGeneral\cRecentFiles\c" + i);
                                        using (RegistryKey rk2 = rk.OpenSubKey(Rkey2, false))
                                        {
                                            if (rk2 != null)
                                            {
                                                foreach (string valueName in rk2.GetValueNames())
                                                {
                                                    if (rk2.GetValue(valueName).ToString() != null)
                                                    {
                                                        string myPattern = "].";
                                                        string a1 = "";
                                                        if (valueName.ToLower().Contains("item"))
                                                        {
                                                            string[] substring = Regex.Split(rk2.GetValue(valueName).ToString(), myPattern);
                                                            a1 = substring[substring.Length - 1];
                                                        }

                                                        if (valueName.ToLower().Contains("file") || valueName.ToLower().Contains("text") || rk2.ToString().ToLower().Contains("rar"))
                                                        { a1 = rk2.GetValue(valueName).ToString(); }

                                                        if (a1.Trim().Length > 0)
                                                        {
                                                            command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                            command.Parameters.AddWithValue("@ComputerName", HostName);
                                                            command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                            command.Parameters.AddWithValue("@UserName", _currentUser);
                                                            command.Parameters.AddWithValue("@DocumentPath", a1.Trim().ToString());
                                                            command.Parameters.AddWithValue("@Software", Software);
                                                            command.Parameters.AddWithValue("@Date", _Date);
                                                            command.Parameters.AddWithValue("@Time", _Time);
                                                            try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                            SoftExist = true;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (!SoftExist && _currentUser.Length > 0 && _currentUserSID.Length > 0)
                                {
                                    /*                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                          " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                        command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                        command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                        command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                        command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                        command.Parameters.AddWithValue("@Software", Software);
                                                                        command.Parameters.AddWithValue("@Date", _Date);
                                                                        command.Parameters.AddWithValue("@Time", _Time);
                                                                        try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                    */
                                }
                                SoftExist = false;
                                command = new SQLiteCommand("end", connection);
                                command.ExecuteNonQuery();

                                command = new SQLiteCommand("begin", connection);
                                command.ExecuteNonQuery();

                                _StatusLabel2Text("Ищу следы другого ПО в реестре... ");
                                //Paint
                                string Rkey3 = Path.Combine(s, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Applets\Paint\Recent File List");
                                using (RegistryKey rk2 = rk.OpenSubKey(Rkey3, false))
                                {
                                    Software = "Paint";
                                    if (rk2 != null)
                                    {
                                        foreach (string valueName in rk2.GetValueNames())
                                        {
                                            if (rk2.GetValue(valueName).ToString() != null)
                                            {
                                                string a1 = "";
                                                if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                {
                                                    object o = rk2.GetValue(valueName);
                                                    foreach (string st in (string[])o)
                                                    { a1 += st + " "; }
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                {
                                                    byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                    a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                { a1 = rk2.GetValue(valueName).ToString(); }

                                                if (a1.Length > 0)
                                                {
                                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                        " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                    command.Parameters.AddWithValue("@ComputerName", HostName);
                                                    command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                    command.Parameters.AddWithValue("@UserName", _currentUser);
                                                    command.Parameters.AddWithValue("@DocumentPath", a1.Trim().ToString());
                                                    command.Parameters.AddWithValue("@Software", Software);
                                                    command.Parameters.AddWithValue("@Date", _Date);
                                                    command.Parameters.AddWithValue("@Time", _Time);
                                                    try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                    SoftExist = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (!SoftExist && _currentUser.Length > 0 && _currentUserSID.Length > 0)
                                {
                                    /*                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                          " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                        command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                        command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                        command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                        command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                        command.Parameters.AddWithValue("@Software", Software);
                                                                        command.Parameters.AddWithValue("@Date", _Date);
                                                                        command.Parameters.AddWithValue("@Time", _Time);
                                                                        try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                    */
                                }
                                SoftExist = false;
                                command = new SQLiteCommand("end", connection);
                                command.ExecuteNonQuery();

                                command = new SQLiteCommand("begin", connection);
                                command.ExecuteNonQuery();

                                //WinRAR
                                Rkey3 = Path.Combine(s, @"SOFTWARE\WinRAR\ArcHistory");
                                using (RegistryKey rk2 = rk.OpenSubKey(Rkey3, false))
                                {
                                    Software = "WinRAR";
                                    if (rk2 != null)
                                    {
                                        foreach (string valueName in rk2.GetValueNames())
                                        {
                                            if (rk2.GetValue(valueName).ToString() != null)
                                            {
                                                string a1 = "";
                                                if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                {
                                                    object o = rk2.GetValue(valueName);
                                                    foreach (string st in (string[])o)
                                                    { a1 += st + " "; }
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                {
                                                    byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                    a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                { a1 = rk2.GetValue(valueName).ToString(); }

                                                if (a1.Length > 0)
                                                {
                                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                        " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                    command.Parameters.AddWithValue("@ComputerName", HostName);
                                                    command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                    command.Parameters.AddWithValue("@UserName", _currentUser);
                                                    command.Parameters.AddWithValue("@DocumentPath", a1.Trim().ToString());
                                                    command.Parameters.AddWithValue("@Software", Software);
                                                    command.Parameters.AddWithValue("@Date", _Date);
                                                    command.Parameters.AddWithValue("@Time", _Time);
                                                    try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                    SoftExist = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                Rkey3 = Path.Combine(s, @"SOFTWARE\WinRAR\DialogEditHistory\ArcName");
                                using (RegistryKey rk2 = rk.OpenSubKey(Rkey3, false))
                                {
                                    Software = "WinRAR";
                                    if (rk2 != null)
                                    {
                                        foreach (string valueName in rk2.GetValueNames())
                                        {
                                            if (rk2.GetValue(valueName).ToString() != null)
                                            {
                                                string a1 = "";
                                                if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                {
                                                    object o = rk2.GetValue(valueName);
                                                    foreach (string st in (string[])o)
                                                    { a1 += st + " "; }
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                {
                                                    byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                    a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                { a1 = rk2.GetValue(valueName).ToString(); }

                                                if (a1.Length > 0)
                                                {
                                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                        " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                    command.Parameters.AddWithValue("@ComputerName", HostName);
                                                    command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                    command.Parameters.AddWithValue("@UserName", _currentUser);
                                                    command.Parameters.AddWithValue("@DocumentPath", a1.Trim().ToString());
                                                    command.Parameters.AddWithValue("@Software", Software);
                                                    command.Parameters.AddWithValue("@Date", _Date);
                                                    command.Parameters.AddWithValue("@Time", _Time);
                                                    try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                    SoftExist = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (!SoftExist && _currentUser.Length > 0 && _currentUserSID.Length > 0)
                                {
                                    /*                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                          " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                        command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                        command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                        command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                        command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                        command.Parameters.AddWithValue("@Software", Software);
                                                                        command.Parameters.AddWithValue("@Date", _Date);
                                                                        command.Parameters.AddWithValue("@Time", _Time);
                                                                        try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                    */
                                }
                                SoftExist = false;

                                //WordPAD
                                Rkey3 = Path.Combine(s, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Applets\Wordpad\Recent File List");
                                using (RegistryKey rk2 = rk.OpenSubKey(Rkey3, false))
                                {
                                    Software = "WordPAD";
                                    if (rk2 != null)
                                    {
                                        foreach (string valueName in rk2.GetValueNames())
                                        {
                                            if (rk2.GetValue(valueName).ToString() != null)
                                            {
                                                string a1 = "";
                                                if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                {
                                                    object o = rk2.GetValue(valueName);
                                                    foreach (string st in (string[])o)
                                                    { a1 += st + " "; }
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                {
                                                    byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                    a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                { a1 = rk2.GetValue(valueName).ToString(); }

                                                if (a1.Length > 0)
                                                {
                                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                        " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                    command.Parameters.AddWithValue("@ComputerName", HostName);
                                                    command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                    command.Parameters.AddWithValue("@UserName", _currentUser);
                                                    command.Parameters.AddWithValue("@DocumentPath", a1.Trim().ToString());
                                                    command.Parameters.AddWithValue("@Software", Software);
                                                    command.Parameters.AddWithValue("@Date", _Date);
                                                    command.Parameters.AddWithValue("@Time", _Time);
                                                    try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                    SoftExist = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (!SoftExist && _currentUser.Length > 0 && _currentUserSID.Length > 0)
                                {
                                    /*                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                          " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                        command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                        command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                        command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                        command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                        command.Parameters.AddWithValue("@Software", Software);
                                                                        command.Parameters.AddWithValue("@Date", _Date);
                                                                        command.Parameters.AddWithValue("@Time", _Time);
                                                                        try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                    */
                                }
                                SoftExist = false;

                                //WinDjView
                                Rkey3 = Path.Combine(s, @"SOFTWARE\Andrew Zhezherun\WinDjView\Tabs");
                                using (RegistryKey rk2 = rk.OpenSubKey(Rkey3, false))
                                {
                                    Software = "WinDjView";
                                    if (rk2 != null)
                                    {
                                        foreach (string valueName in rk2.GetValueNames())
                                        {
                                            if (rk2.GetValue(valueName).ToString() != null)
                                            {
                                                string a1 = "";
                                                if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                {
                                                    object o = rk2.GetValue(valueName);
                                                    foreach (string st in (string[])o)
                                                    { a1 += st + " "; }
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                {
                                                    byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                    a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                { a1 = rk2.GetValue(valueName).ToString(); }

                                                if (a1.Length > 0)
                                                {
                                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                        " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                    command.Parameters.AddWithValue("@ComputerName", HostName);
                                                    command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                    command.Parameters.AddWithValue("@UserName", _currentUser);
                                                    command.Parameters.AddWithValue("@DocumentPath", a1.Trim().ToString());
                                                    command.Parameters.AddWithValue("@Software", Software);
                                                    command.Parameters.AddWithValue("@Date", _Date);
                                                    command.Parameters.AddWithValue("@Time", _Time);
                                                    try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                    SoftExist = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                Rkey3 = Path.Combine(s, @"SOFTWARE\Andrew Zhezherun\WinDjView\Recent File List");
                                using (RegistryKey rk2 = rk.OpenSubKey(Rkey3, false))
                                {
                                    Software = "WinDjView";
                                    if (rk2 != null)
                                    {
                                        foreach (string valueName in rk2.GetValueNames())
                                        {
                                            if (rk2.GetValue(valueName).ToString() != null)
                                            {
                                                string a1 = "";
                                                if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                {
                                                    object o = rk2.GetValue(valueName);
                                                    foreach (string st in (string[])o)
                                                    { a1 += st + " "; }
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                {
                                                    byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                    a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                { a1 = rk2.GetValue(valueName).ToString(); }

                                                if (a1.Length > 0)
                                                {
                                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                        " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                    command.Parameters.AddWithValue("@ComputerName", HostName);
                                                    command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                    command.Parameters.AddWithValue("@UserName", _currentUser);
                                                    command.Parameters.AddWithValue("@DocumentPath", a1.Trim().ToString());
                                                    command.Parameters.AddWithValue("@Software", Software);
                                                    command.Parameters.AddWithValue("@Date", _Date);
                                                    command.Parameters.AddWithValue("@Time", _Time);
                                                    try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                    SoftExist = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (!SoftExist && _currentUser.Length > 0 && _currentUserSID.Length > 0)
                                {
                                    /*                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                          " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                        command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                        command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                        command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                        command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                        command.Parameters.AddWithValue("@Software", Software);
                                                                        command.Parameters.AddWithValue("@Date", _Date);
                                                                        command.Parameters.AddWithValue("@Time", _Time);
                                                                        try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                    */
                                }
                                SoftExist = false;

                                //AkelPad
                                Rkey3 = Path.Combine(s, @"SOFTWARE\Akelsoft\AkelPad\Recent");
                                using (RegistryKey rk2 = rk.OpenSubKey(Rkey3, false))
                                {
                                    Software = "AkelPad";
                                    if (rk2 != null)
                                    {
                                        foreach (string valueName in rk2.GetValueNames())
                                        {
                                            if (rk2.GetValue(valueName).ToString() != null)
                                            {
                                                string a1 = "";
                                                if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                {
                                                    object o = rk2.GetValue(valueName);
                                                    foreach (string st in (string[])o)
                                                    { a1 += st + " "; }
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                {
                                                    byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                    a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                { a1 = rk2.GetValue(valueName).ToString(); }

                                                if (a1.Length > 0)
                                                {
                                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                        " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                    command.Parameters.AddWithValue("@ComputerName", HostName);
                                                    command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                    command.Parameters.AddWithValue("@UserName", _currentUser);
                                                    command.Parameters.AddWithValue("@DocumentPath", a1.Trim().ToString());
                                                    command.Parameters.AddWithValue("@Software", Software);
                                                    command.Parameters.AddWithValue("@Date", _Date);
                                                    command.Parameters.AddWithValue("@Time", _Time);
                                                    try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                    SoftExist = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (!SoftExist && _currentUser.Length > 0 && _currentUserSID.Length > 0)
                                {
                                    /*                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                          " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                        command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                        command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                        command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                        command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                        command.Parameters.AddWithValue("@Software", Software);
                                                                        command.Parameters.AddWithValue("@Date", _Date);
                                                                        command.Parameters.AddWithValue("@Time", _Time);
                                                                        try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                    */
                                }
                                SoftExist = false;
                                command = new SQLiteCommand("end", connection);
                                command.ExecuteNonQuery();

                                command = new SQLiteCommand("begin", connection);
                                command.ExecuteNonQuery();

                                //VMWare
                                Software = "VMWare";
                                Rkey3 = Path.Combine(s, @"SOFTWARE\VMware\VMware Infrastructure Client\Preferences");
                                using (RegistryKey rk2 = rk.OpenSubKey(Rkey3, false))
                                {
                                    if (rk2 != null)
                                    {
                                        foreach (string valueName in rk2.GetValueNames())
                                        {
                                            if (rk2.GetValue(valueName).ToString() != null && valueName.ToLower().Contains("recentconnections"))
                                            {
                                                string a1 = "";
                                                if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                {
                                                    object o = rk2.GetValue(valueName);
                                                    foreach (string st in (string[])o)
                                                    { a1 += st + " "; }
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                {
                                                    byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                    a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                { a1 = rk2.GetValue(valueName).ToString(); }

                                                if (a1.Length > 1)
                                                {
                                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                        " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                    command.Parameters.AddWithValue("@ComputerName", HostName);
                                                    command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                    command.Parameters.AddWithValue("@UserName", _currentUser);
                                                    command.Parameters.AddWithValue("@DocumentPath", a1.Trim());
                                                    command.Parameters.AddWithValue("@Software", Software);
                                                    command.Parameters.AddWithValue("@Date", _Date);
                                                    command.Parameters.AddWithValue("@Time", _Time);
                                                    try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                    SoftExist = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                Rkey3 = Path.Combine(s, @"SOFTWARE\VMware\Virtual Infrastructure Client\Preferences\UI\ClientsXml");
                                using (RegistryKey rk2 = rk.OpenSubKey(Rkey3, false))
                                {
                                    if (rk2 != null)
                                    {
                                        foreach (string valueName in rk2.GetValueNames())
                                        {
                                            if (rk2.GetValue(valueName).ToString() != null)
                                            {
                                                string a1 = "";
                                                if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                {
                                                    object o = rk2.GetValue(valueName);
                                                    foreach (string st in (string[])o)
                                                    { a1 += st + " "; }
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                {
                                                    byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                    a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                { a1 = rk2.GetValue(valueName).ToString(); }

                                                if (a1.Length > 0)
                                                {
                                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                        " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                    command.Parameters.AddWithValue("@ComputerName", HostName);
                                                    command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                    command.Parameters.AddWithValue("@UserName", _currentUser);
                                                    command.Parameters.AddWithValue("@DocumentPath", valueName.Trim());
                                                    command.Parameters.AddWithValue("@Software", Software);
                                                    command.Parameters.AddWithValue("@Date", _Date);
                                                    command.Parameters.AddWithValue("@Time", _Time);
                                                    try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                    SoftExist = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (!SoftExist && _currentUser.Length > 0 && _currentUserSID.Length > 0)
                                {
                                    /*                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                          " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                        command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                        command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                        command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                        command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                        command.Parameters.AddWithValue("@Software", Software);
                                                                        command.Parameters.AddWithValue("@Date", _Date);
                                                                        command.Parameters.AddWithValue("@Time", _Time);
                                                                        try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                    */
                                }
                                SoftExist = false;

                                //PuTTY
                                Rkey3 = Path.Combine(s, @"SOFTWARE\SimonTatham\PuTTY\Sessions");
                                using (RegistryKey RTemp = rk.OpenSubKey(Rkey3, false))
                                {
                                    Software = "PuTTY";
                                    if (RTemp != null)
                                    {
                                        foreach (string valueNames in RTemp.GetSubKeyNames())
                                        {
                                            string RTemp1 = Path.Combine(Rkey3, valueNames);

                                            using (RegistryKey RTemp2 = rk.OpenSubKey(RTemp1, false))
                                            {
                                                if (RTemp2 != null)
                                                {
                                                    foreach (string valueName in RTemp2.GetValueNames())
                                                    {
                                                        if (RTemp2.GetValue(valueName).ToString() != null && valueName.ToLower().Contains("hostname"))
                                                        {
                                                            string a1 = "";
                                                            if (RTemp2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                            {
                                                                object o = RTemp2.GetValue(valueName);
                                                                foreach (string st in (string[])o)
                                                                { a1 += st + " "; }
                                                            }
                                                            else if (RTemp2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                            {
                                                                byte[] s2 = (byte[])RTemp2.GetValue(valueName);
                                                                a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                            }
                                                            else if (RTemp2.GetValueKind(valueName) == RegistryValueKind.String)
                                                            { a1 = RTemp2.GetValue(valueName).ToString(); }

                                                            if (a1.Length > 0)
                                                            {
                                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                    " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                command.Parameters.AddWithValue("@DocumentPath", a1.Trim().ToString());
                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                SoftExist = true;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (!SoftExist && _currentUser.Length > 0 && _currentUserSID.Length > 0)
                                {
                                    /*                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                          " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                        command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                        command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                        command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                        command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                        command.Parameters.AddWithValue("@Software", Software);
                                                                        command.Parameters.AddWithValue("@Date", _Date);
                                                                        command.Parameters.AddWithValue("@Time", _Time);
                                                                        try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                    */
                                }
                                SoftExist = false;
                                command = new SQLiteCommand("end", connection);
                                command.ExecuteNonQuery();

                                command = new SQLiteCommand("begin", connection);
                                command.ExecuteNonQuery();

                                //Explorer
                                _StatusLabel2Text("Ищу в реестре следы файлов открывавшихся Проводником... ");
                                Rkey3 = Path.Combine(s, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\RecentDocs"); //Поиск в ключах подпапок и ключах подпапок
                                using (RegistryKey rk2 = rk.OpenSubKey(Rkey3, false))
                                {
                                    Software = "Explorer.RecentDocs";
                                    if (rk2 != null)
                                    {
                                        foreach (string valueName in rk2.GetValueNames())
                                        {
                                            if (rk2.GetValue(valueName).ToString() != null)
                                            {
                                                string a1 = "";
                                                if (rk2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                {
                                                    object o = rk2.GetValue(valueName);
                                                    foreach (string st in (string[])o)
                                                    { a1 += st + " "; }
                                                }

                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                {
                                                    byte[] s2 = (byte[])rk2.GetValue(valueName);
                                                    a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                }
                                                else if (rk2.GetValueKind(valueName) == RegistryValueKind.String)
                                                { a1 = rk2.GetValue(valueName).ToString(); }

                                                if (a1.Length > 0)
                                                {
                                                    string pattern = @"([^А-Яа-яіІїЇєЄA-Za-z0-9\!\|\\\#\@\~\$\%\(\)\ \.\]\[\:\{\}\?])";
                                                    string c = Regex.Replace(a1, pattern, " ");
                                                    string b = Regex.Replace(c, "  ", " ");
                                                    while (b.Contains("  "))
                                                    { b = Regex.Replace(b, "  ", " "); }
                                                    a1 = b.Trim();

                                                    if (a1.Length > 4)
                                                    {
                                                        command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                            " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                        command.Parameters.AddWithValue("@ComputerName", HostName);
                                                        command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                        command.Parameters.AddWithValue("@UserName", _currentUser);
                                                        command.Parameters.AddWithValue("@DocumentPath", Convert.ToString(a1));
                                                        command.Parameters.AddWithValue("@Software", Software);
                                                        command.Parameters.AddWithValue("@Date", _Date);
                                                        command.Parameters.AddWithValue("@Time", _Time);
                                                        try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                        SoftExist = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (!SoftExist && _currentUser.Length > 0 && _currentUserSID.Length > 0)
                                {
                                    /*                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                          " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                        command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                        command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                        command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                        command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                        command.Parameters.AddWithValue("@Software", Software);
                                                                        command.Parameters.AddWithValue("@Date", _Date);
                                                                        command.Parameters.AddWithValue("@Time", _Time);
                                                                        try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                    */
                                }
                                SoftExist = false;
                                Rkey3 = Path.Combine(s, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32"); //Поиск в ключах подподпапок и ключах подпапок
                                using (RegistryKey RTemp = rk.OpenSubKey(Rkey3, false))
                                {
                                    Software = "Explorer.ComDlg32";
                                    if (RTemp != null)
                                    {
                                        foreach (string valueNames in RTemp.GetSubKeyNames())
                                        {
                                            string RTemp1 = Path.Combine(Rkey3, valueNames);
                                            using (RegistryKey RTemp2 = rk.OpenSubKey(RTemp1, false))
                                            {
                                                if (RTemp2 != null)
                                                {
                                                    foreach (string valueName in RTemp2.GetValueNames())
                                                    {
                                                        if (RTemp2.GetValue(valueName).ToString() != null && RTemp2.GetValue(valueName).ToString().ToLower().Contains("mrulistex") != true)
                                                        {
                                                            string a1 = "";
                                                            if (RTemp2.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                            {
                                                                object o = RTemp2.GetValue(valueName);
                                                                foreach (string st in (string[])o)
                                                                { a1 += st + " "; }
                                                            }
                                                            else if (RTemp2.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                            {
                                                                byte[] s2 = (byte[])RTemp2.GetValue(valueName);
                                                                a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                            }
                                                            else if (RTemp2.GetValueKind(valueName) == RegistryValueKind.String)
                                                            { a1 = RTemp2.GetValue(valueName).ToString(); }

                                                            if (a1.Length > 4)
                                                            {
                                                                string pattern = @"([^А-Яа-яіІїЇєЄA-Za-z0-9\!\|\\\#\@\~\$\%\(\)\ \.\]\[\:\{\}\?])";
                                                                string c = Regex.Replace(a1, pattern, " ");
                                                                string b = Regex.Replace(c, "  ", " ");
                                                                while (b.Contains("  "))
                                                                { b = Regex.Replace(b, "  ", " "); }
                                                                a1 = b.Trim();

                                                                if (a1.Length > 4)
                                                                {
                                                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                        " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                    command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                    command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                    command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                    command.Parameters.AddWithValue("@DocumentPath", Convert.ToString(a1));
                                                                    command.Parameters.AddWithValue("@Software", Software);
                                                                    command.Parameters.AddWithValue("@Date", _Date);
                                                                    command.Parameters.AddWithValue("@Time", _Time);
                                                                    try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                    SoftExist = true;
                                                                }
                                                            }
                                                        }
                                                    }

                                                    foreach (string RvalueNames in RTemp2.GetSubKeyNames())
                                                    {
                                                        string RTemp3 = Path.Combine(RTemp1, RvalueNames);
                                                        using (RegistryKey RTemp4 = rk.OpenSubKey(RTemp3, false))
                                                        {
                                                            if (RTemp4 != null)
                                                            {
                                                                foreach (string valueName in RTemp4.GetValueNames())
                                                                {
                                                                    if (RTemp4.GetValue(valueName).ToString() != null && RTemp4.GetValue(valueName).ToString().ToLower().Contains("mrulistex") != true)
                                                                    {
                                                                        string a1 = "";
                                                                        if (RTemp4.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                                        {
                                                                            object o = RTemp4.GetValue(valueName);
                                                                            foreach (string st in (string[])o)
                                                                            { a1 += st + " "; }
                                                                        }
                                                                        else if (RTemp4.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                                        {
                                                                            byte[] s2 = (byte[])RTemp4.GetValue(valueName);
                                                                            a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                                        }
                                                                        else if (RTemp4.GetValueKind(valueName) == RegistryValueKind.String)
                                                                        { a1 = RTemp4.GetValue(valueName).ToString(); }

                                                                        if (a1.Trim().Length > 4)
                                                                        {
                                                                            string pattern = @"([^А-Яа-яіІїЇєЄA-Za-z0-9\!\|\\\#\@\~\$\%\(\)\ \.\]\[\:\{\}\?])";
                                                                            string c = Regex.Replace(a1, pattern, " ");
                                                                            string b = Regex.Replace(c, "  ", " ");
                                                                            while (b.Contains("  "))
                                                                            { b = Regex.Replace(b, "  ", " "); }
                                                                            a1 = b.Trim();

                                                                            if (a1.Length > 4)
                                                                            {
                                                                                command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                                    " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                                command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                                command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                                command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                                command.Parameters.AddWithValue("@DocumentPath", RvalueNames + " | " + Convert.ToString(a1));
                                                                                command.Parameters.AddWithValue("@Software", Software);
                                                                                command.Parameters.AddWithValue("@Date", _Date);
                                                                                command.Parameters.AddWithValue("@Time", _Time);
                                                                                try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                                SoftExist = true;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (!SoftExist && _currentUser.Length > 0 && _currentUserSID.Length > 0)
                                {
                                    /*                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                          " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                        command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                        command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                        command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                        command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                        command.Parameters.AddWithValue("@Software", Software);
                                                                        command.Parameters.AddWithValue("@Date", _Date);
                                                                        command.Parameters.AddWithValue("@Time", _Time);
                                                                        try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                    */
                                }
                                SoftExist = false;
                                command = new SQLiteCommand("end", connection);
                                command.ExecuteNonQuery();

                                command = new SQLiteCommand("begin", connection);
                                command.ExecuteNonQuery();

                                Rkey3 = Path.Combine(s, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\RecentDocs");  //Поиск в ключах подподпапок
                                using (RegistryKey RTemp2 = rk.OpenSubKey(Rkey3, false))
                                {
                                    Software = "Explorer.RecentDocs";
                                    if (RTemp2 != null)
                                    {
                                        foreach (string RvalueNames in RTemp2.GetSubKeyNames())
                                        {
                                            string RTemp3 = Path.Combine(Rkey3, RvalueNames);
                                            using (RegistryKey RTemp4 = rk.OpenSubKey(RTemp3, false))
                                            {
                                                if (RTemp4 != null)
                                                {
                                                    foreach (string valueName in RTemp4.GetValueNames())
                                                    {
                                                        if (RTemp4.GetValue(valueName).ToString() != null && RTemp4.GetValue(valueName).ToString().ToLower().Contains("mrulistex") != true)
                                                        {
                                                            string a1 = "";
                                                            if (RTemp4.GetValueKind(valueName) == RegistryValueKind.MultiString)
                                                            {
                                                                object o = RTemp4.GetValue(valueName);
                                                                foreach (string st in (string[])o)
                                                                { a1 += st + " "; }
                                                            }
                                                            else if (RTemp4.GetValueKind(valueName) == RegistryValueKind.Binary)
                                                            {
                                                                byte[] s2 = (byte[])RTemp4.GetValue(valueName);
                                                                a1 = Convert.ToString(Encoding.Unicode.GetString(s2));
                                                            }
                                                            else if (RTemp4.GetValueKind(valueName) == RegistryValueKind.String)
                                                            { a1 = RTemp4.GetValue(valueName).ToString(); }

                                                            if (a1.Trim().Length > 4)
                                                            {
                                                                string pattern = @"([^А-Яа-яіІїЇєЄA-Za-z0-9\!\|\\\#\@\~\$\%\(\)\ \.\]\[\:\{\}\?])";
                                                                string c = Regex.Replace(a1, pattern, " ");
                                                                string b = Regex.Replace(c, "  ", " ");
                                                                while (b.Contains("  "))
                                                                { b = Regex.Replace(b, "  ", " "); }
                                                                a1 = b.Trim();

                                                                if (a1.Length > 4)
                                                                {
                                                                    command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                        " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                    command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                    command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                    command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                    command.Parameters.AddWithValue("@DocumentPath", RvalueNames + " | " + Convert.ToString(a1));
                                                                    command.Parameters.AddWithValue("@Software", Software);
                                                                    command.Parameters.AddWithValue("@Date", _Date);
                                                                    command.Parameters.AddWithValue("@Time", _Time);
                                                                    try { command.ExecuteNonQuery(); } catch (Exception e) { throw new Exception(e.Message); }
                                                                    SoftExist = true;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (!SoftExist && _currentUser.Length > 0 && _currentUserSID.Length > 0)
                                {
                                    /*                                  command = new SQLiteCommand("INSERT INTO 'UsersDocuments' ('ComputerName', 'UserSID', 'UserName', 'DocumentPath', 'Software', 'Date', 'Time') " +
                                                                        " VALUES (@ComputerName, @UserSID, @UserName, @DocumentPath, @Software, @Date, @Time)", connection);
                                                                      command.Parameters.AddWithValue("@ComputerName", HostName);
                                                                      command.Parameters.AddWithValue("@UserSID", _currentUserSID);
                                                                      command.Parameters.AddWithValue("@UserName", _currentUser);
                                                                      command.Parameters.AddWithValue("@DocumentPath", "Nothing");
                                                                      command.Parameters.AddWithValue("@Software", Software);
                                                                      command.Parameters.AddWithValue("@Date", _Date);
                                                                      command.Parameters.AddWithValue("@Time", _Time);
                                                                      try { command.ExecuteNonQuery(); } catch (Exception e) { MessageBox.Show(e.ToString()); }
                                  */
                                }
                                SoftExist = false;
                                command = new SQLiteCommand("end", connection);
                                command.ExecuteNonQuery();

                            }
                        }
                    }
                } catch
                {
                    command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') " +
                           " VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection);
                    command.Parameters.AddWithValue("@ComputerName", HostName);
                    command.Parameters.AddWithValue("@Name", "Error");
                    command.Parameters.AddWithValue("@Value", "UsersDocuments");
                    command.Parameters.AddWithValue("@Value1", "No Acceess!");
                    command.Parameters.AddWithValue("@Date", _Date);
                    command.Parameters.AddWithValue("@Time", _Time);
                }
                connection.Close();
            }
        }
        //-------------/\/\/\/\------------// Scanning of Registry. End of The Block //-------------/\/\/\/\------------// 





        //-------------/\/\/\/\------------// Работа с базами данных. Start of The Block //-------------/\/\/\/\------------//        
        private void _UpdateDataGrid(string sql, string DBNameFull = databaseAllTables) //Insert new Data at DataGrid
        {
            ArrayList Empty = new ArrayList();
            dataGridView1.DataSource = Empty;
            DataSet ds = new DataSet();
            try
            {//PRAGMA case_sensitive_like = true; - for Case sensetive "LIKE"
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", DBNameFull)))
                {
                    connection.Open();
                    using (SQLiteDataAdapter da = new SQLiteDataAdapter(sql, connection))
                    {
                        da.Fill(ds);
                        dataGridView1.DataSource = ds.Tables[0].DefaultView;
                        toolTipText1.SetToolTip(tabControl, "В данной таблице - " + dataGridView1.RowCount.ToString() + " строк");
                    }
                }
            } catch { }
        }

        private void _DBCheckFull(string DbShortName) //Make file with DB from the DBShortName
        {
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + DbShortName + @".db");
            if (!fi.Exists)
            { SQLiteConnection.CreateFile(fi.FullName); }
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", fi.FullName)))
            {
                connection.Open();
                for (int i = 0; i < _createTablesName.Length; i++)
                {
                    if (_createTablesName[i].ToString().ToLower().Contains("allusers") && DbShortName.ToLower().Contains("allusers"))
                    {
                        using (SQLiteCommand command = new SQLiteCommand("CREATE TABLE IF NOT EXISTS " + _createTablesName[i].ToString().Trim() + " ( " + _createTables[i].ToString().Trim() + " );", connection))
                        { command.ExecuteNonQuery(); }
                        break;
                    }
                    else if (!DbShortName.ToLower().Contains("allusers"))
                    {
                        using (SQLiteCommand command = new SQLiteCommand("CREATE TABLE IF NOT EXISTS " + _createTablesName[i].ToString().Trim() + " ( " + _createTables[i].ToString().Trim() + " );", connection))
                        { command.ExecuteNonQuery(); }
                    }
                }
            }
        }

        private void DBCreateTables(string sqlCreateTable, string database) //Test //Create the DataBase and the whole needed tables
        {
            SQLiteConnection.CreateFile(databaseAllTables);
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", database)))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sqlCreateTable, connection))
                { command.ExecuteNonQuery(); }
            }
        }

        private void DBCreate(string sql, string database) //Create the DataBase and the whole needed tables
        {
            FileInfo fi = new FileInfo(database);
            if (fi.Exists == false)
            { SQLiteConnection.CreateFile(database); }
            SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", database)); connection.Open();

            //1-я таблица
            SQLiteCommand command =
                new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'UsbUsed' (" +
                "'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'USBId' INTEGER, 'ComputerName' TEXT, 'UsbKey' TEXT, 'UsbName' TEXT, " +
                "'UsbSN' TEXT, 'InstallDate' TEXT, 'InstallTime' TEXT, 'LastDate' TEXT, 'LastTime' TEXT, 'LastUser' TEXT " +
                ");", connection);
            command.ExecuteNonQuery();

            //2-я таблица
            command =
               new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'UsbMounted' (" +
                "'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'USBMId' INTEGER, 'ComputerName' TEXT, 'ValueName' TEXT, 'Value' TEXT" +
               ");", connection);
            command.ExecuteNonQuery();

            //3-я таблица //Name=UserName:user //Value=SID or FIO //Value1=ProfilePath //Date,Time = LastLogOn
            command =
                new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'WindowsFeature' (" +
                "'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'Name' TEXT, 'Value' TEXT, 'Value1' TEXT, 'ComputerName' TEXT, 'Date' TEXT, 'Time' TEXT " +
                ");", connection);
            command.ExecuteNonQuery();

            //4-я таблица
            command =
                new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'USBHub' (" +
                "'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'USBHubId' TEXT, 'ComputerName' TEXT, 'Caption' TEXT, 'DeviceID' TEXT, " +
                "'NumberOfPorts' TEXT, 'Status' TEXT, 'SystemName' TEXT, 'Date' TEXT, 'Time' TEXT " +
                ");", connection);
            command.ExecuteNonQuery();

            //5-я таблица
            command =
                new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'PnPEntity' (" +
                "'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'PnPId' TEXT, 'ComputerName' TEXT, 'Caption' TEXT, 'installDate' TEXT, 'DeviceID' TEXT, " +
                "'HardwareID' TEXT, 'Manufacturer' TEXT, 'Status' TEXT, 'SystemName' TEXT " +
                ");", connection);
            command.ExecuteNonQuery();

            //6-я таблица
            command =
                new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'DiskDrive' (" +
                "'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'DDiD' TEXT, 'ComputerName' TEXT, 'Model' TEXT, 'SerialNumber' TEXT, 'MediaType' TEXT, 'Manufacturer' TEXT, 'Status' TEXT, " +
                "'Size' TEXT, 'Date' TEXT, 'Time' TEXT " +
                ");", connection);
            command.ExecuteNonQuery();

            //7-я таблица
            command =
                new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'Volume' (" +
                "'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'VolumeiD' TEXT, 'ComputerName' TEXT, 'Caption' TEXT, 'DriveLetter' TEXT, 'FileSystem' TEXT, 'DriveType' TEXT, 'Size' TEXT, 'Capacity' TEXT, " +
                "'FreeSpace' TEXT, 'SizeRemaining' TEXT, 'Date' TEXT, 'Time' TEXT " +
                ");", connection);
            command.ExecuteNonQuery();

            //8-я таблица
            command =
               new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'PhysicalMemory' (" +
                "'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'PhysicalMemoryId' INTEGER, 'ComputerName' TEXT, 'Caption' TEXT, 'banklabel' TEXT, 'DeviceLocator' TEXT, 'SerialNumber' TEXT, " +
                "'MemoryType' TEXT, 'Model' TEXT, 'Speed' TEXT, 'Manufacturer' TEXT, 'FormFactor' TEXT, 'TotalWidth' TEXT, 'InterleavePosition' TEXT, 'Capacity' TEXT, 'Date' TEXT, 'Time' TEXT " +
                ");", connection);
            command.ExecuteNonQuery();

            //9-я таблица
            command =
                new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'Product' (" +
                "'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'ProductId' INTEGER, 'ComputerName' TEXT, 'Caption' TEXT, 'InstallDate' TEXT " +
                ");", connection);
            command.ExecuteNonQuery();

            //10-я таблица
            command =
                new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'Process' (" +
                "'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'ProcessId' INTEGER, 'ComputerName' TEXT, 'Name' TEXT, 'iDProcess' TEXT, 'Date' TEXT, 'Time' TEXT " +
                ");", connection);
            command.ExecuteNonQuery();

            //11-я таблица
            command =
                new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'Services' (" +
                "'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'ServicesId' INTEGER, 'ComputerName' TEXT, 'ServiceName' TEXT, 'DisplayName' TEXT, 'Status' TEXT, 'Date' TEXT, 'Time' TEXT " +
                ");", connection);
            command.ExecuteNonQuery();

            //Table with events
            //12-я таблица
            command =
                new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'EventsLogs' (" +
                "'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'LogId' INTEGER, 'ComputerName' TEXT, 'RecordNumber' INTEGER, 'EventCode' TEXT, 'EventType' TEXT, " +
                "'SourceName' TEXT, 'EventIdentifier' TEXT, 'Type' TEXT, 'Category' TEXT, 'CategoryString' TEXT, 'Date' TEXT, 'Time' TEXT, 'Message' TEXT, 'User' TEXT , 'Logfile' TEXT  " +
                ");", connection);
            command.ExecuteNonQuery();

            //Tables of other
            //13-я таблица
            command =
                new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'UsersDocuments' (" +
                "'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'UserDocId' INTEGER, 'ComputerName' TEXT, 'UserSID' TEXT, 'UserName' TEXT, 'DocumentPath' TEXT, 'Software' TEXT, 'Length' TEXT, " + //length - ", kB"
                "'Date' TEXT, 'Time' TEXT " +
                ");", connection);
            command.ExecuteNonQuery();

            //14-я таблица
            command =
                new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'FoundDocuments' (" +
                "'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'UserDocId' INTEGER, 'ComputerName' TEXT, 'DocumentPath' TEXT, 'Length' TEXT, 'Date' TEXT, 'Time' TEXT " +
                ");", connection);
            command.ExecuteNonQuery();

            //15-я таблица
            command =
                new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'AllTables' (" +
                "'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'TableId' INTEGER, 'NameTable' TEXT, 'ComputerIP' TEXT, 'ComputerName' TEXT, " +
                "'ScannerName' TEXT, 'ScannerIP' TEXT, 'Date' TEXT, 'Time' TEXT " +
                ");", connection);
            command.ExecuteNonQuery();

            //16-я таблица
            command =
                new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'AliveHosts' (" +
                "'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'ComputerId' INTEGER, 'ComputerName' TEXT, 'ComputerNameShort' TEXT, 'ComputerDomainName' TEXT, 'ComputerIP' TEXT, " +
                " 'ComputerModel' TEXT, 'ComputerSN' TEXT, 'LogOnUser' TEXT, 'ScannerName' TEXT, 'ScannerIP' TEXT, " +
                "'Date' TEXT, 'Time' TEXT " +
                ");", connection);
            command.ExecuteNonQuery();

            //17-я таблица
            command =
                new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'AliveNets' (" +
                "'Id' INTEGER PRIMARY KEY AUTOINCREMENT, 'NetId' INTEGER, 'Net' TEXT, 'ScannerName' TEXT, 'ScannerIP' TEXT, " +
                " 'Date' TEXT, 'Time' TEXT " +
                ");", connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        private void dBReadNamesOfTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DBReadNameTables();
            tabControl.SelectedTab = tabPageLog;
            textBoxLogs.Focus();
        }

        private void DBReadNameTables() //Test Read names of tables of the DB SQLite
        {
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};", currentDbName)))
            {
                connection.Open();
                try
                {
                    textBoxLogs.AppendText("\nDB: " + currentDbName + ":\n");
                    SQLiteCommand command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name; ", connection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    foreach (DbDataRecord record in reader)
                    {
                        SQLiteCommand command1 = new SQLiteCommand("SELECT count (*) FROM '" + record["name"] + "'; ", connection);
                        var reader1 = command1.ExecuteScalar();

                        textBoxLogs.AppendText("Таблица: " + record["name"] + "".PadLeft(40 - record["name"].ToString().Trim().Length, '.') + " Записей - \t" + reader1.ToString() + "\n");
                    }
                    textBoxLogs.AppendText("\n");
                } catch (Exception expt) { MessageBox.Show(expt.ToString()); }
            }
        }

        private void dBReadColumnsOfTheTableToolStripMenuItem_Click(object sender, EventArgs e)          //Get Names of Collums of the sellected table
        {
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};", currentDbName)))
            {
                connection.Open();
                try
                {
                    if (comboTables.SelectedIndex > -1)
                    {
                        textBoxLogs.AppendText("\nDB Name: " + currentDbName);
                        using (SQLiteCommand command = new SQLiteCommand("PRAGMA table_info('" + comboTables.SelectedItem.ToString() + "');", connection))
                        {
                            textBoxLogs.AppendText("\nTable is " + comboTables.SelectedItem.ToString() + "\n");
                            textBoxLogs.AppendText("\nNames of the Columns:\n");
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                foreach (DbDataRecord record in reader)
                                { textBoxLogs.AppendText(record["name"].ToString() + " " + record["CID"].ToString() + " " + record["TYPE"].ToString() + " " + record["PK"].ToString() + "\n"); } //Get Name of collumns
                                textBoxLogs.AppendText("\n");
                                //CID, NAME, TYPE, NOTNULL, DFLT_VALUE, PK - other names of records
                            }
                        }
                    }
                    else
                    {
                        comboTables.SelectedIndex = 0;
                        _ControlVisibleStartUP(false);
                        _ControlVisibleLicense(false);
                        _ControlVisibleAnalyse(true);
                        tabControl.SelectedTab = tabDataGrid;
                        comboTables.Focus();
                    }
                } catch (Exception expt) { MessageBox.Show(expt.Message); }
            }
            tabControl.SelectedTab = tabPageLog;
            textBoxLogs.Focus();
        }

        private void dBInsertToolStripMenuItem_Click(object sender, EventArgs e)    //Test
        {
            DBInsertData();
            DBReadData();
            tabControl.Enabled = true;
            tabControl.Visible = true;
            textBoxLogs.Enabled = true;
            textBoxLogs.Visible = true;
        }

        private void DBInsertData()  //Test Insert Data into Table of myDB
        {
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};", databaseAllTables)))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'UsbUsed' ('USBId', 'FriendlyName', 'LastUser') VALUES (1, 'Kingston','User'); ", connection))
                { command.ExecuteNonQuery(); }
            }
        }

        private void DBReadData() //Test Reading Data
        {
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};", databaseAllTables)))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM 'UsbUsed';", connection))
                {
                    SQLiteDataReader reader = command.ExecuteReader();
                    foreach (DbDataRecord record in reader)
                    { textBoxLogs.AppendText(record["Id"].ToString() + "  " + record["USBId"].ToString() + "  " + record["FriendlyName"].ToString() + "\n"); }
                }
            }
        }

        private void ReadData() //Test Read Data
        {
            DirectoryInfo info = new DirectoryInfo(databaseAllTables);
            if (File.Exists(info.FullName))
            {
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", databaseAllTables)))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM 'UsbMounted';", connection))
                    {
                        SQLiteDataReader reader = command.ExecuteReader();
                        try
                        {
                            foreach (DbDataRecord record in reader)
                            {
                                if (record != null)
                                {
                                    textBoxLogs.AppendText(record["ComputerName"].ToString() + " ");
                                    textBoxLogs.AppendText(record["ValueName"].ToString() + " ");
                                    textBoxLogs.AppendText(record["Value"].ToString() + "\n");
                                }
                            }
                        } catch { }
                    }
                    GC.Collect();  //раcкомментировать для удаления файла с БД
                    connection.Close();
                    connection.Dispose();
                }
            }
        }
        //---/////////////////////////////////////////////////////
        //-------------/\/\/\/\------------// Работа с базами данных. End of The Block //-------------/\/\/\/\------------//         



        // -------- /\/\/\/\ -------- // Reaction on Keys and Mouse. Changing Schems of the color and Visibility of the panels. Start of the Block   // -------- /\/\/\/\ -------- //

        private void _ControlVisibleStartUP(bool iVisible = true)
        {
            textBoxDomain.Visible = iVisible;
            textBoxInputNetOrInputPC.Visible = iVisible;
            textBoxPassword.Visible = iVisible;
            textBoxLogin.Visible = iVisible;
            comboBoxTargedPC.Visible = iVisible;
            listBoxNetsRow.Visible = iVisible;
            labelCurrentNet.Visible = iVisible;
            labelLicense.Visible = iVisible;
            labelInputNetOrInputPC.Visible = iVisible;
            labelLogin.Visible = iVisible;
            labelNets.Visible = iVisible;
            labelNetsColor.Visible = iVisible;
            labelPassword.Visible = iVisible;
            labelTargedPCColor.Visible = iVisible;
            labelDomain.Visible = iVisible;
            labelSearchUser.Visible = iVisible;

            comboUsers.Visible = iVisible;
        }

        private void _ControlVisibleAnalyse(bool iVisible = true)
        {
            comboAnalyse.Visible = iVisible;
            comboTables.Visible = iVisible;
            labelTablesColor.Visible = iVisible;
            labelTables.Visible = iVisible;

            comboRows.Visible = iVisible;
            textBoxRows.Visible = iVisible;
            comboBoxMask1.Visible = iVisible;
            labelNameTable.Visible = iVisible;
            labelRows.Visible = iVisible;
            labelRowsColor.Visible = iVisible;

            comboRows2.Visible = iVisible;
            textBoxRows2.Visible = iVisible;
            labelNameTable2.Visible = iVisible;
            labelRowsColor2.Visible = iVisible;
            comboBoxMask2.Visible = iVisible;

            listBoxNetsRow.Visible = iVisible;
            labelListRow.Visible = iVisible;
        }

        private void _ControlVisibleLicense(bool iVisible = true)
        {
            _textBoxLicenseVisible(iVisible);
        }

        private void AnalyseDataMenu_Click(object sender, EventArgs e)
        {
            _ControlVisibleStartUP(false);
            _ControlVisibleLicense(false);
            _ControlVisibleAnalyse(true);
            tabControl.SelectedTab = tabDataGrid;
            comboTables.Focus();
        }

        private void GetDataMenu_Click_bak(object sender, EventArgs e)
        {
            _ControlVisibleAnalyse(false);
            _ControlVisibleLicense(false);
            _ControlVisibleStartUP(true);
            tabControl.SelectedTab = tabDataGrid;
            tabDataGrid.Focus();
        }

        private void tabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (tabControl.SelectedTab == tabDataGrid)
            {
                _ControlVisibleLicense(false);
                _ControlVisibleStartUP(false);
                _ControlVisibleAnalyse(true);
            }
            else
            {
                _ControlVisibleLicense(false);
                _ControlVisibleAnalyse(false);
                _ControlVisibleStartUP(true);
            }
        }

        private void textBoxUser_Enter(object sender, EventArgs e)
        { labelLogin.BackColor = System.Drawing.Color.LightSkyBlue; }  //LightSteelBlue

        private void textBoxPassword_Enter(object sender, EventArgs e)
        { labelPassword.BackColor = System.Drawing.Color.LightSkyBlue; }  //LightSteelBlue

        private void textBoxUser_Leave(object sender, EventArgs e)
        { labelLogin.BackColor = System.Drawing.Color.Transparent; }

        private void textBoxPassword_Leave(object sender, EventArgs e)
        { labelPassword.BackColor = System.Drawing.Color.Transparent; }

        private void textBoxDomain_Enter(object sender, EventArgs e)
        { labelDomain.BackColor = System.Drawing.Color.LightSkyBlue; }  //LightSteelBlue

        private void textBoxDomain_Leave(object sender, EventArgs e)
        { labelDomain.BackColor = System.Drawing.Color.Transparent; }

        private void textBoxInputNetOrInputPC_Enter_1(object sender, EventArgs e)
        { labelInputNetOrInputPC.BackColor = System.Drawing.Color.LightSkyBlue; }   //LightSteelBlue}

        private void comboBoxTargedPC_Enter(object sender, EventArgs e)
        { labelTargedPCColor.BackColor = System.Drawing.Color.LightSkyBlue; }  //LightSteelBlue        

        private void comboBoxTargedPC_Leave(object sender, EventArgs e)
        { labelTargedPCColor.BackColor = System.Drawing.Color.Transparent; }

        private void comboTables_Enter(object sender, EventArgs e)
        {
            labelTables.BackColor = System.Drawing.Color.LightSkyBlue;  //LightSteelBlue
            labelTablesColor.BackColor = System.Drawing.Color.LightSkyBlue;
        }

        private void comboTables_Leave(object sender, EventArgs e)
        {
            labelTables.BackColor = System.Drawing.Color.Transparent;
            labelTablesColor.BackColor = System.Drawing.Color.Transparent;
        }

        private void comboAnalyse_Enter(object sender, EventArgs e)
        {
            labelTables.BackColor = System.Drawing.Color.LightSkyBlue;  //LightSteelBlue
            labelTablesColor.BackColor = System.Drawing.Color.LightSkyBlue;
        }

        private void comboAnalyse_Leave(object sender, EventArgs e)
        {
            labelTables.BackColor = System.Drawing.Color.Transparent;
            labelTablesColor.BackColor = System.Drawing.Color.Transparent;
        }

        private void listBoxListOfNet_Enter(object sender, EventArgs e)
        {
            labelNetsColor.BackColor = System.Drawing.Color.LightSkyBlue;  //LightSteelBlue
            labelNets.BackColor = System.Drawing.Color.LightSkyBlue;
            labelListRow.BackColor = System.Drawing.Color.LightSkyBlue;
        }

        private void listBoxListOfNet_Leave(object sender, EventArgs e)
        {
            labelNetsColor.BackColor = System.Drawing.Color.Transparent;
            labelNets.BackColor = System.Drawing.Color.Transparent;
            labelListRow.BackColor = System.Drawing.Color.Transparent;
        }

        private void comboRows_Enter(object sender, EventArgs e)
        {
            labelNameTable.BackColor = System.Drawing.Color.LightSkyBlue;  //LightSteelBlue
            labelRows.BackColor = System.Drawing.Color.LightSkyBlue;
            labelRowsColor.BackColor = System.Drawing.Color.LightSkyBlue;
        }

        private void comboRows_Leave(object sender, EventArgs e)
        {
            labelNameTable.BackColor = System.Drawing.Color.Transparent;
            labelRows.BackColor = System.Drawing.Color.Transparent;
            labelRowsColor.BackColor = System.Drawing.Color.Transparent;
        }

        private void textBoxRows_Leave(object sender, EventArgs e)
        {
            labelNameTable.BackColor = System.Drawing.Color.Transparent;
            labelRows.BackColor = System.Drawing.Color.Transparent;
            labelRowsColor.BackColor = System.Drawing.Color.Transparent;
        }

        private void textBoxRows_Enter(object sender, EventArgs e)
        {
            labelNameTable.BackColor = System.Drawing.Color.LightSkyBlue;  //LightSteelBlue
            labelRows.BackColor = System.Drawing.Color.LightSkyBlue;        //DarkSeaGreen
            labelRowsColor.BackColor = System.Drawing.Color.LightSkyBlue;
        }

        private void comboRows2_Enter(object sender, EventArgs e)
        {
            labelNameTable2.BackColor = System.Drawing.Color.LightSkyBlue;  //LightSteelBlue
            labelRowsColor2.BackColor = System.Drawing.Color.LightSkyBlue;   //DarkSeaGreen
        }

        private void comboRows2_Leave(object sender, EventArgs e)
        {
            labelNameTable2.BackColor = System.Drawing.Color.Transparent;
            labelRowsColor2.BackColor = System.Drawing.Color.Transparent;
        }

        private void textBoxRows2_Enter(object sender, EventArgs e)
        {
            labelNameTable2.BackColor = System.Drawing.Color.LightSkyBlue;  //LightSteelBlue
            labelRowsColor2.BackColor = System.Drawing.Color.LightSkyBlue;    //DarkSeaGreen
        }

        private void textBoxRows2_Leave(object sender, EventArgs e)
        {
            labelNameTable2.BackColor = System.Drawing.Color.Transparent;
            labelRowsColor2.BackColor = System.Drawing.Color.Transparent;
        }

        private void comboUsers_Enter(object sender, EventArgs e)
        { labelSearchUser.BackColor = System.Drawing.Color.LightSkyBlue; }  //LightSteelBlue

        private void comboUsers_Leave(object sender, EventArgs e)
        { labelSearchUser.BackColor = System.Drawing.Color.Transparent; }

        private void textBoxUser_TextChanged(object sender, EventArgs e)
        {
            if (textBoxLogin.TextLength > 0)
            { textBoxPassword.Enabled = true; }
            if (textBoxLogin.TextLength == 0)
            { textBoxPassword.Enabled = false; }
        }

        private void textBoxInputNetOrInputPC_KeyUp(object sender, EventArgs e)
        { labelInputNetOrInputPC.BackColor = System.Drawing.Color.Transparent; }

        private void checkBoxReboot_CheckStateChanged(object sender, EventArgs e) //a CheckBox has Changed its state
        { ControlHostMenu.Enabled = true; }

        private void chkbxService_CheckedChanged(object sender, EventArgs e)
        {
            if (chkbxService.Checked && checkBoxChangeStateService.Checked)
            {
                buttonGetServices.Text = "Запустить службу";
            }
            else if (!chkbxService.Checked && checkBoxChangeStateService.Checked)
            {
                buttonGetServices.Text = "Остановить службу";
            }
            else
            {
                buttonGetServices.Text = "Получить список служб";
            }
        }

        private void comboBoxService_SelectedIndexChanged(object sender, EventArgs e)
        {
            string serviceDescription = "";
            string host = "";
            string state = "запущен ";
            try
            {
                string[] selectedservice = Regex.Split(comboBoxService.SelectedItem.ToString(), "[|]");
                labelSelectedService.Text = selectedservice[0].Trim();
                labelStatusService.Text = "Состояние службы = " + selectedservice[1].Trim();
                labelDisplayNameService.Text = selectedservice[2].Trim();
                serviceDescription = selectedservice[3].Trim();
                host = selectedservice[4].Trim();

                if (selectedservice[1].Contains("Stopped"))
                {
                    state = "остановлен ";
                    chkbxService.Checked = false;
                }
                else if (selectedservice[1].Contains("Running"))
                {
                    chkbxService.Checked = true;
                }
            } catch
            {
                labelSelectedService.Text = "";
                labelStatusService.Text = "";
                labelDisplayNameService.Text = "";
            }
            toolTipText1.SetToolTip(this.labelSelectedService, "На " + host + "\n" + state + "сервис:\n" + serviceDescription);

        }

        private void comboBoxProcess_SelectedIndexChanged(object sender, EventArgs e)
        {   //BTHSAmpPalService.exe | 5576 | 640 | C:\Program Files\Intel\BluetoothHS\BTHSAmpPalService.exe | 2017 - 04 - 29 0:54:05
            string parent = "";
            string processdescription = "";
            string processid = "";
            string user = "";
            string host = "";

            try
            {
                string[] selectedprocess = Regex.Split(comboBoxProcess.SelectedItem.ToString(), "[|]");
                processid = selectedprocess[1].Trim();
                labelProcess.Text = selectedprocess[0].Trim() + "   id: " + processid;
                parent = selectedprocess[2].Trim();
            } catch
            {
                labelProcess.Text = "";
                labelPathAndTimeProcess.Text = "";
                labelParentProcess.Text = "";
            }

            try
            {
                foreach (string fullprocess in processeshost.ToArray())
                {
                    string parentprocessname = Regex.Split(fullprocess, "[|]")[0].Trim();
                    string child = Regex.Split(fullprocess, "[|]")[1].Trim();

                    if (child == parent)
                    {
                        labelParentProcess.Text = "Родительский процесс: " + parentprocessname;
                    }
                    if (processid == Regex.Split(fullprocess, "[|]")[1].Trim())
                    {
                        labelPathAndTimeProcess.Text = "Путь: " + Regex.Split(fullprocess, "[|]")[3].Trim() + "\nЗапущен: " + Regex.Split(fullprocess, "[|]")[4].Trim();
                        processdescription = Regex.Split(fullprocess, "[|]")[5].Trim();
                        user = Regex.Split(fullprocess, "[|]")[6].Trim();
                        host = Regex.Split(fullprocess, "[|]")[7].Trim();
                    }
                }
            } catch
            {
                labelParentProcess.Text = "";
            }
            toolTipText1.SetToolTip(this.labelProcess, "На " + host + "\n" + processdescription + "\nзапущен от имени " + user);
            toolTipText1.SetToolTip(this.checkBoxKillProcess, "Прервать на " + host + "\n" + labelProcess.Text);
        }

        private void textBoxInputNetOrInputPC_Enter(object sender, EventArgs e)
        { textBoxLogin.Focus(); }

        private void comboTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataGridViewSelectedTable = _selectTable[comboTables.SelectedIndex].ToLower();

            try
            {
                if (DataGridViewSelectedTable.Contains("alivehosts") || DataGridViewSelectedTable.Contains("archivesofhost"))
                {
                    SelectedPeriod = "";
                    _UpdateDataGrid(_selectSQL[comboTables.SelectedIndex], Application.StartupPath + @"\" + databaseAliveHosts);
                    labelNameTable.Text = DataGridViewSelectedTable;
                    labelNameTable2.Text = DataGridViewSelectedTable; //Сделать логику взаимоисключения полей

                    comboRows.Items.Clear();
                    comboRows2.Items.Clear();
                    using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};", Application.StartupPath + @"\" + databaseAliveHosts)))
                    {
                        connection.Open();              //comboTables.SelectedItem.ToString()
                        try
                        {
                            using (SQLiteCommand command = new SQLiteCommand("PRAGMA table_info('AliveHosts');", connection))  //Get Name of collumns of the sellected tables
                            {
                                SQLiteDataReader reader = command.ExecuteReader();
                                foreach (DbDataRecord record in reader)
                                {
                                    comboRows.Items.Add(record["name"].ToString());
                                    comboRows2.Items.Add(record["name"].ToString());
                                }
                                connection.Close();
                            }
                        } catch (Exception expt) { MessageBox.Show(expt.ToString()); }
                    }

                    currentDbName = databaseAliveHosts;
                    combotables = comboTables.SelectedIndex;
                    try { comboRows.SelectedIndex = 0; } catch { }
                    try { comboRows2.SelectedIndex = 0; } catch { }
                    DeSelectUnknowItem.Visible = true;
                    DeSelectUnknowNoPermissionItem.Visible = true;
                }

                else if (DataGridViewSelectedTable.Contains("allusers"))
                {
                    _UpdateDataGrid(_selectSQL[comboTables.SelectedIndex], Application.StartupPath + @"\" + databaseAllUsers);
                    labelNameTable.Text = DataGridViewSelectedTable;
                    labelNameTable2.Text = DataGridViewSelectedTable; //Сделать логику взаимоисключения полей

                    comboRows.Items.Clear();
                    comboRows2.Items.Clear();
                    using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};", Application.StartupPath + @"\" + databaseAllUsers)))
                    {
                        connection.Open();              //comboTables.SelectedItem.ToString()
                        try
                        {
                            using (SQLiteCommand command = new SQLiteCommand("PRAGMA table_info('AllUsers');", connection))  //Get Name of collumns of the sellected tables
                            {
                                SQLiteDataReader reader = command.ExecuteReader();
                                foreach (DbDataRecord record in reader)
                                {
                                    comboRows.Items.Add(record["name"].ToString());
                                    comboRows2.Items.Add(record["name"].ToString());
                                }
                                connection.Close();
                            }
                        } catch (Exception expt) { MessageBox.Show(expt.ToString()); }
                    }

                    currentDbName = databaseAllUsers;
                    combotables = comboTables.SelectedIndex;
                    try { comboRows.SelectedIndex = 0; } catch { }
                    try { comboRows2.SelectedIndex = 0; } catch { }
                }
                else if (_currentHostName.Length > 0)
                {
                    FileInfo fi1 = new FileInfo(Application.StartupPath + "\\myEventLoger\\" + _currentHostName + ".db");
                    databaseHost = fi1.FullName;

                    if (fi1.Exists)
                    {
                        if (databaseHost.ToString().Length > 3)
                        {
                            currentDbName = databaseHost;
                            _UpdateDataGrid(_selectSQL[comboTables.SelectedIndex], databaseHost);
                        }
                        else
                        {
                            currentDbName = databaseAllTables;
                            _UpdateDataGrid(_selectSQL[comboTables.SelectedIndex]);
                        }

                        labelNameTable.Text = comboTables.SelectedItem.ToString();
                        labelNameTable2.Text = comboTables.SelectedItem.ToString(); //Сделать логику взаимоисключения полей

                        comboRows.Items.Clear();
                        comboRows2.Items.Clear();
                        using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};", currentDbName)))
                        {
                            connection.Open();              //comboTables.SelectedItem.ToString()
                            try
                            {
                                using (SQLiteCommand command = new SQLiteCommand("PRAGMA table_info('" + comboTables.SelectedItem.ToString() + "');", connection))  //Get Name of collumns of the sellected tables
                                {
                                    SQLiteDataReader reader = command.ExecuteReader();
                                    foreach (DbDataRecord record in reader)
                                    {
                                        comboRows.Items.Add(record["name"].ToString());
                                        comboRows2.Items.Add(record["name"].ToString());
                                    }
                                    reader.Close();
                                    connection.Close();
                                }
                            } catch (Exception expt) { connection.Close(); MessageBox.Show(expt.ToString()); }
                        }
                    }
                    combotables = comboTables.SelectedIndex;
                    DeSelectUnknowItem.Visible = false;
                    DeSelectUnknowNoPermissionItem.Visible = false;
                }
            } catch (Exception exp) { MessageBox.Show(exp.ToString()); }

            try { comboRows.SelectedIndex = 0; } catch { }
            try { comboRows2.SelectedIndex = 0; } catch { }
            textBoxRows.Text = "";
            textBoxRows2.Text = "";
        }

        private void comboAnalyse_SelectedIndexChanged(object sender, EventArgs e)
        {
            _UpdateDataGrid(_eventAnalyse[comboAnalyse.SelectedIndex]);
        }

        private void PingHostItem_Click(object sender, EventArgs e)
        {
            _CycleWait = false;
            if (tabControl.SelectedTab == tabDataGrid)
            { bool str = ParseSelectedCell(); }
            else
            { ParseTextboxInputNetOrPC(); }
            StatusLabel2.Text = "Пинг хоста " + _currentHostName;
            _pingEnable();
            tabControl.SelectedTab = tabPageCtrl;
            groupBoxPC.Focus();
        }

        private void textBoxInputNetOrInputPC_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string myEvLogKey = @"SOFTWARE\RYIK\SIEM";
                try
                {
                    using (var EvUserKey = Registry.CurrentUser.CreateSubKey(myEvLogKey))
                    {
                        EvUserKey.SetValue("UserLogin", textBoxLogin.Text, RegistryValueKind.String);
                        EvUserKey.SetValue("UserPassword", textBoxPassword.Text, RegistryValueKind.String);
                        EvUserKey.SetValue("UserDomain", textBoxDomain.Text, RegistryValueKind.String);
                        EvUserKey.SetValue("InputedPC", textBoxInputNetOrInputPC.Text, RegistryValueKind.String);
                    }
                } catch { }

                if (selectedPC == false)
                {
                    textBoxToTemporary();
                    ParseTextboxInputNetOrPC();
                    _CycleWait = false;

                    if (buttonPing.Text == "Stop Ping")
                    {
                        Task.Delay(500).Wait();
                        buttonPing.Text = "Выполнить";
                    }

                    else if (buttonPing.Text == "Выполнить")
                    {
                        _pingEnable();
                        Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));
                    }

                    tabControl.SelectedTab = tabPageCtrl;
                    groupBoxPC.Focus();
                }

                else if (selectedPC == true)
                {
                    selectedPC = false;
                    comboTables.Items.Clear();

                    foreach (string ct in _selectSQLNames)
                    { comboTables.Items.Add(ct); }

                    _ControlVisibleLicense(false);
                    _ControlVisibleStartUP(false);
                    _ControlVisibleAnalyse(true);

                    comboTables.Focus();
                }
            }
        }

        private void textBoxUser_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            { textBoxDomain.Focus(); }
        }

        private void textBoxDomain_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            { textBoxPassword.Focus(); }
        }

        private void textBoxInputNetOrInputPC_TextChanged(object sender, EventArgs e)
        {
            textBoxToTemporary();
            if (buttonPing.Text == "Stop Ping")
            {
                _CycleWait = false;
                buttonPing.Text = "Выполнить";
                buttonPing.Enabled = true;
            }
        }

        private void textBoxPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textBoxDomain.Enabled = false;
                textBoxPassword.Enabled = false;
                textBoxLogin.Enabled = false;

                textBoxToTemporary();
                ParseTextboxInputNetOrPC();
                if (textBoxInputNetOrInputPCText != _myIP)
                {
                    ControlHostMenu.Enabled = true;
                    if (buttonPing.Text == "Stop Ping")
                    {
                        _CycleWait = false;
                        buttonPing.Text = "Выполнить";
                        buttonPing.Enabled = true;
                    }

                    if (textBoxLoginLength > 0)
                    {
                        ControlHostMenu.Enabled = true;
                        comboUsers.Enabled = true;
                        buttonGetServices.Enabled = true;
                        comboBoxService.Enabled = true;
                        buttonGetProcess.Enabled = true;
                        comboBoxProcess.Enabled = true;
                        textBoxNameProgramm.Enabled = true;
                        foreach (CheckBox checkBox in tabPageCtrl.Controls.OfType<CheckBox>())      //Перебираем все чекбоксы на форме
                        { checkBox.Enabled = true; }
                    }
                    else
                    {
                        comboUsers.Enabled = false;
                        buttonGetServices.Enabled = false;
                        comboBoxService.Enabled = false;
                        buttonGetProcess.Enabled = false;
                        comboBoxProcess.Enabled = false;
                        textBoxNameProgramm.Enabled = false;

                        foreach (CheckBox checkBox in tabPageCtrl.Controls.OfType<CheckBox>())      //Перебираем все чекбоксы на форме
                        { checkBox.Enabled = false; }
                    }
                    textBoxInputNetOrInputPC.Focus();
                }
            }
        }

        private void GetDataHostsArchievItem_Click(object sender, EventArgs e)
        {
            DirectoryInfo rootDirectory = new DirectoryInfo(Application.StartupPath + @"\myEventLoger\");
            var Coder = Encoding.GetEncoding(65001);
            // Task t;
            Stack<string> dirs = new Stack<string>(100);

            if (!Directory.Exists(rootDirectory.FullName))
            { throw new ArgumentException(); }
            dirs.Push(rootDirectory.FullName);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;
                try { subDirs = Directory.GetDirectories(currentDir); } catch { continue; }

                string[] files = null;
                try { files = Directory.GetFiles(currentDir); } catch { continue; }
                foreach (string file in files)
                {
                    try
                    {
                        FileInfo fi = new FileInfo(file); // Perform whatever action is required in your scenario.
                        if (fi.FullName.ToString().ToLower().Contains(@".db") == true && (fi.FullName.ToString().ToLower().Contains(@"alltables.db") == false)) //маска для поиска
                        {

                            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}; page_size = 65536; cache_size =100000; journal_mode =MEMORY;", fi.FullName)))
                            {
                                connection.Open();
                                string TName = fi.Name.ToString().Remove(fi.Name.ToString().Length - 3);

                                SQLiteCommand command = new SQLiteCommand("SELECT Value FROM WindowsFeature Where Name like '%ComputerIP%' AND ComputerName like '%" + TName + "%' ;", connection);
                                var reader = command.ExecuteScalar();

                                _comboBoxTargedPCAdd(fi.Name.ToString().Remove(fi.Name.ToString().Length - 3) + " | " + reader.ToString());
                                connection.Close();
                            }
                        }
                    } catch { continue; } // If file was deleted by a separate application
                }
                foreach (string str in subDirs)
                { dirs.Push(str); }
            }

            selectedPC = true;
            _ControlVisibleLicense(false);
            _ControlVisibleStartUP(false);
            _ControlVisibleAnalyse(true);
            comboTables.Focus();
            comboBoxTargedPC.Focus();
        }

        private void resetI_Click(object sender, EventArgs e)
        {
            textBoxLogin.Clear();
            textBoxDomain.Clear();
            textBoxPassword.Clear();
            textBoxDomain.Enabled = true;
            textBoxPassword.Enabled = true;
            textBoxLogin.Enabled = true;

            textBoxInputNetOrInputPC.Text = _myIP;
            labelCurrentNet.Text = _myNET + "xxx";
            textBoxPassword.Enabled = false;
        }

        private void changePC_Click(object sender, EventArgs e)
        {
            _DBCheckFull("AllTables");
            comboBoxTargedPC.SelectedIndex = -1;
            listBoxNetsRow.SelectedIndex = -1;
            textBoxLogs.Clear();
        }

        private void textBoxRows_Click(object sender, EventArgs e)
        {
            textBoxRows.Clear();
            textBoxRows.ForeColor = System.Drawing.Color.Black;
        }

        private void textBoxRows2_Click(object sender, EventArgs e)
        {
            textBoxRows2.Clear();
            textBoxRows2.ForeColor = System.Drawing.Color.Black;
        }


        private void comboBoxTargedPC_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                textBoxInputNetOrInputPC.Text = Regex.Split(comboBoxTargedPC.SelectedItem.ToString(), "[|]")[1].Trim();
                if (Regex.Split(comboBoxTargedPC.SelectedItem.ToString(), "[|]")[0].ToLower().Contains("unknow") == false)
                { ParseTextboxInputNetOrPC(); }
            } catch { }
        }

        private void textBoxRows_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    string txt1 = textBoxRows.Text.Trim();
                    int txtlength1 = textBoxRows.Text.Trim().Length;
                    if (comboBoxMask1.SelectedIndex == 0)
                    {
                        txt1 = "";
                        txtlength1 = 0;
                        textBoxRows.Clear();
                    }
                    int idxcomboBoxMask1 = comboBoxMask1.SelectedIndex;
                    int idxcomboBoxMask2 = comboBoxMask2.SelectedIndex;

                    string txt2 = textBoxRows2.Text.Trim();
                    int txtlength2 = textBoxRows2.Text.Trim().Length;
                    if (comboBoxMask2.SelectedIndex == 0)
                    {
                        txt2 = "";
                        txtlength2 = 0;
                        textBoxRows2.Clear();
                    }

                    string sql = "SELECT ";
                    string sqlcount = "SELECT COUNT(*) ";

                    if (txtlength1 > 0 && txtlength2 == 0)
                    {
                        sql += _selectRaw[combotables] +
                                " FROM " + _selectTable[combotables] +
                                " WHERE " + comborows + " " +
                                _mask[1, idxcomboBoxMask1] +
                                txt1 +
                                _mask[2, idxcomboBoxMask1] +
                                " LIMIT 1000";

                        sqlcount += " FROM " + _selectTable[combotables] +
                                " WHERE " + comborows + " " +
                                _mask[1, idxcomboBoxMask1] +
                                txt1 +
                                _mask[2, idxcomboBoxMask1];
                    }

                    else if (txtlength1 == 0 && txtlength2 > 0)
                    {
                        sql += _selectRaw[combotables] +
                                " FROM " + _selectTable[combotables] +
                                " WHERE " + comborows2 + " " +
                                _mask[1, idxcomboBoxMask2] +
                                txt2 +
                                _mask[2, idxcomboBoxMask2] +
                                " LIMIT 1000";

                        sqlcount += " FROM " + _selectTable[combotables] +
                                " WHERE " + comborows2 + " " +
                                _mask[1, idxcomboBoxMask2] +
                                txt2 +
                                _mask[2, idxcomboBoxMask2];
                    }

                    else if (txtlength1 > 0 && txtlength2 > 0)
                    {
                        sql += _selectRaw[combotables] +
                              " FROM " + _selectTable[combotables] +
                              " WHERE " + comborows + " " +
                              _mask[1, idxcomboBoxMask1] +
                              txt1 +
                              _mask[2, idxcomboBoxMask1] + " AND " +
                              comborows2 + " " +
                              _mask[1, idxcomboBoxMask2] +
                               txt2 +
                               _mask[2, idxcomboBoxMask2] +
                              " LIMIT 1000";

                        sqlcount += " FROM " + _selectTable[combotables] +
                                " WHERE " + comborows + " " +
                                _mask[1, idxcomboBoxMask1] +
                                txt1 +
                                _mask[2, idxcomboBoxMask1] + " AND " +
                                comborows2 + " " +
                                _mask[1, idxcomboBoxMask2] +
                                txt2 +
                                _mask[2, idxcomboBoxMask2];
                    }
                    else
                    {
                        sql += _selectRaw[comboTables.SelectedIndex] +
                                " FROM " + _selectTable[comboTables.SelectedIndex] +
                                " LIMIT 1000";
                        sqlcount += " FROM " + _selectTable[comboTables.SelectedIndex];
                    }

                    //Получаем количество строк в выборке
                    using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", currentDbName)))
                    {
                        connection.Open();
                        using (SQLiteCommand command = new SQLiteCommand(sqlcount, connection))
                        { StatusLabelCurrent.Text = " Строк в выборке: " + command.ExecuteScalar().ToString(); }
                    }

                    //                    MessageBox.Show(tooltip1 + "\n" + tooltip2);
                    if (idxcomboBoxMask1 > -1)
                        toolTipText1.SetToolTip(this.textBoxRows, _mask[0, idxcomboBoxMask1] + txt1);
                    if (idxcomboBoxMask2 > -1)
                        toolTipText1.SetToolTip(this.textBoxRows2, _mask[0, idxcomboBoxMask2] + txt2);

                    _UpdateDataGrid(sql, currentDbName);

                    tabControl.SelectedTab = tabDataGrid;
                    tabDataGrid.Focus();
                } catch (Exception exp) { MessageBox.Show(exp.ToString()); }
            }
        }
         
        private void comboRows_SelectedIndexChanged(object sender, EventArgs e)   //Select list of Collumns of the Selected Table from database
        {
            comboRows2.Items.Clear();

            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};", databaseAllTables)))
            {
                connection.Open();              //comboTables.SelectedItem.ToString()
                try
                {
                    using (SQLiteCommand command = new SQLiteCommand("PRAGMA table_info('" + comboTables.SelectedItem.ToString() + "');", connection))  //Get Name of collumns of the sellected tables
                    {
                        SQLiteDataReader reader = command.ExecuteReader();
                        foreach (DbDataRecord record in reader)
                        {
                            if (comboRows.SelectedItem.ToString() != record["name"].ToString())
                                comboRows2.Items.Add(record["name"].ToString());
                        }
                        reader.Close();
                    }
                } catch (Exception expt) { MessageBox.Show(expt.ToString()); }
                connection.Close();
            }

            if (comboRows.SelectedIndex > -1)
            { comborows = comboRows.SelectedItem.ToString(); }
        }

        private void comboRows2_SelectedIndexChanged(object sender, EventArgs e)      //Select list of Collumns of the Selected Table from database
        {
            comboRows.Items.Clear();

            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};", databaseAllTables)))
            {
                connection.Open();              //comboTables.SelectedItem.ToString()
                try
                {
                    using (SQLiteCommand command = new SQLiteCommand("PRAGMA table_info('" + comboTables.SelectedItem.ToString() + "');", connection))  //Get Name of collumns of the sellected tables
                    {
                        SQLiteDataReader reader = command.ExecuteReader();
                        foreach (DbDataRecord record in reader)
                        {
                            if (comboRows2.SelectedItem.ToString() != record["name"].ToString())
                                comboRows.Items.Add(record["name"].ToString());
                        }
                        reader.Close();
                    }
                } catch (Exception expt) { MessageBox.Show(expt.ToString()); }
                connection.Close();
            }

            if (comboRows2.SelectedIndex > -1)
            { comborows2 = comboRows2.SelectedItem.ToString(); }
        }

        private void archievGotFromNetsItem_Click(object sender, EventArgs e)
        {
            string sql = " Select ComputerName, ComputerDomainName, ComputerIP, ComputerModel, ComputerSN, LogOnUser, Date FROM AliveHosts GROUP BY ComputerIP ORDER BY ComputerIP LIMIT 1000 ";
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\AliveHosts.db");
            _UpdateDataGrid(sql, fi.FullName);
            DataGridViewSelectedTable = "AliveHosts";
            currentDbName = fi.FullName;
            selectedPC = false;
            tabControl.SelectedTab = tabDataGrid;
        }

        // -------- /\/\/\/\ -------- // Reaction on Keys and Mouse. End of the Block   // -------- /\/\/\/\ -------- //
        // -------- /\/\/\/\ -------- // Changing Schems of the color and Visibility of the panels. End of the Block   // -------- /\/\/\/\ -------- //




        // -------- /\/\/\/\ -------- // Context Menu for DataGridView. Start of the Block   // -------- /\/\/\/\ -------- //
        public void InitializeMyToolBar() //Test. Проверить работу!!!!
        {
            // Create the ToolBar, ToolBarButton controls, and menus.
            ToolBarButton toolBarButton1 = new ToolBarButton("Open");
            ToolBarButton toolBarButton2 = new ToolBarButton();
            ToolBarButton toolBarButton3 = new ToolBarButton();
            ToolBar toolBar1 = new ToolBar();
            MenuItem menuItem1 = new MenuItem("Print");
            menuItem1.Index = 0;
            MenuItem menuItem2 = new MenuItem("-");
            menuItem2.Index = 1;
            System.Windows.Forms.MenuItem menuItem3 = new System.Windows.Forms.MenuItem();
            //            menuItem3.Click += new EventHandler(MenuItem2_Click);
            menuItem3.Index = 2;
            menuItem3.Text = "Menu Item 2";
            ContextMenu contextMenu2 = new ContextMenu(new MenuItem[] { menuItem1, menuItem2, menuItem3 });

            // Add the ToolBarButton controls to the ToolBar.
            toolBar1.Buttons.Add(toolBarButton1);
            toolBar1.Buttons.Add(toolBarButton2);
            toolBar1.Buttons.Add(toolBarButton3);

            // Assign an ImageList to the ToolBar and show ToolTips.
            //            toolBar1.ImageList = imageList1;
            toolBar1.ShowToolTips = true;

            /* Assign ImageIndex, ContextMenu, Text, ToolTip, and 
               Style properties of the ToolBarButton controls. */
            toolBarButton2.Style = ToolBarButtonStyle.Separator;
            toolBarButton3.Text = "Print";
            toolBarButton3.Style = ToolBarButtonStyle.DropDownButton;
            toolBarButton3.ToolTipText = "Print";
            toolBarButton3.ImageIndex = 0;
            toolBarButton3.DropDownMenu = contextMenu2;

            // Add the ToolBar to a form.
            Controls.Add(toolBar1);
        }

        //Проверить работу!!!!
        private void cancelChangesToolStripMenuItem_Click(object sender, EventArgs e) //узнать источник запуска контекстного меню.
        {
            var sourceControl = ((ContextMenuStrip)((ToolStripMenuItem)sender).GetCurrentParent()).SourceControl;
            //...
        }

        private bool dgvcorrect = true;
        private string dgvCell = "";

        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)    //isn't Used  // Select Row in the position of cursor
        {
            if (e.Button == MouseButtons.Right)
            {
                Task.Run(() => _checkAddress());
                int rowSelected = e.RowIndex;
                if (e.RowIndex != -1)
                {
                    dataGridView1.Rows[rowSelected].Selected = true;
                }
            }
        }

        private void _checkAddress()
        {
            dgvcorrect = true;
            dgvCell = dataGridView1.CurrentCell.Value.ToString().ToLower().Trim();
            if (dgvCell.Contains("unknow"))
            { dgvcorrect = false; MessageBox.Show("Имя выбранного хоста не резолвится!"); }

            else
            {
                try
                {
                    IPAddress addr = IPAddress.Parse(dgvCell);
                    IPHostEntry entry = Dns.GetHostEntry(addr);
                    _currentHostName = entry.HostName;
                    _currentHostIP = addr.ToString();
                    string[] tmpPC = Regex.Split(_currentHostIP.ToString(), "[.]");
                    _currentHostNet = tmpPC[0].Trim() + "." + tmpPC[1].Trim() + "." + tmpPC[2].Trim();
                    Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));
                    _labelCurrentNet(_currentHostNet + ".xxx");
                    dgvcorrect = true;
                } catch { dgvcorrect = false; }

                if (!dgvcorrect)
                {
                    try
                    {
                        IPAddress addr = Dns.GetHostAddresses(dgvCell)[0];
                        IPHostEntry entry = Dns.GetHostEntry(addr);
                        _currentHostName = entry.HostName;
                        _currentHostIP = addr.ToString();
                        string[] tmpPC = Regex.Split(_currentHostIP.ToString(), "[.]");
                        _currentHostNet = tmpPC[0].Trim() + "." + tmpPC[1].Trim() + "." + tmpPC[2].Trim();
                        Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));
                        _labelCurrentNet(_currentHostNet + ".xxx");
                        dgvcorrect = true;
                    } catch { dgvcorrect = false; }
                }
            }
        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            int SelectComputerNameCellIndexColumn = 0;     // индекс колонки ComputerName в датагрид
            int SelectComputerIPCellIndexColumn = 2;       // индекс колонки ComputerIP в датагрид
            string SelectComputerNameCell = "";
            string SelectComputerIPCell = "";
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                if (dataGridView1.Columns[i].HeaderText.ToString() == "ComputerIP")
                    SelectComputerIPCellIndexColumn = i;

                if (dataGridView1.Columns[i].HeaderText.ToString() == "ComputerName")
                    SelectComputerNameCellIndexColumn = i;
            }

            if (DataGridViewSelectedTable.ToLower().Contains("alivehosts") || DataGridViewSelectedTable.ToLower().Contains("archivesofhost"))
            {
                int IndexCurrentRow = dataGridView1.CurrentRow.Index;
                SelectComputerNameCell = dataGridView1.Rows[IndexCurrentRow].Cells[SelectComputerNameCellIndexColumn].Value.ToString();
                SelectComputerIPCell = dataGridView1.Rows[IndexCurrentRow].Cells[SelectComputerIPCellIndexColumn].Value.ToString();

                if (!SelectComputerNameCell.ToLower().Contains("unknow") && SelectComputerNameCell.Trim().Length > 0)
                {
                    _textBoxInputNetOrInputPC(SelectComputerNameCell);
                    _currentHostName = SelectComputerNameCell;
                    GetArchieveOfHostItem.Text = "Архив данных собранных с " + SelectComputerNameCell;
                    GetRegistryItem.Text = "Сканировать реестр " + SelectComputerNameCell;
                    PingItem.Text = "Пинг хоста " + SelectComputerNameCell;
                    GetEventsItem.Text = "Загрузить все события и конфигурацию " + SelectComputerNameCell;
                    GetFileItems.Text = "Получить список файлов на " + SelectComputerNameCell;
                    GetArchieveOfHostItem.Text = "Архив данных собранных с " + SelectComputerNameCell;
                    GetWholeDataItem.Text = "Сбор всей информации c " + SelectComputerNameCell;
                    Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));

                    Separator1.Visible = true; //Context Menu DataGrid
                    Separator0.Visible = true; //Context Menu DataGrid
                    GetArchieveOfHostItem.Visible = true;
                    Separator5.Visible = true;
                    GetFileItems.Visible = true;
                    GetRegistryItem.Visible = true;
                    PingItem.Visible = true;
                    GetEventsItem.Visible = true;
                    GetWholeDataItem.Visible = true;
                }
            }

            if (SelectComputerNameCell.ToLower().Contains("unknow"))
            {
                Separator1.Visible = false; //Context Menu DataGrid
                Separator0.Visible = false; //Context Menu DataGrid
                GetArchieveOfHostItem.Visible = false;
                GetWholeDataItem.BackColor = System.Drawing.Color.SandyBrown;
                Separator5.Visible = false;
                GetFileItems.Visible = false;
                GetRegistryItem.Visible = false;
                PingItem.Visible = false;
                GetEventsItem.Visible = false;
                GetWholeDataItem.Visible = false;
            }
            else
                //  selectedPC = true;     //?????????????????????????
                e.Control.ContextMenuStrip = contextMenuDataGrid;
        }

        private bool ParseSelectedCell()
        {
            textBoxToTemporary();
            int SelectComputerNameCellIndexColumn = 0;     // индекс колонки ComputerName в датагрид
            int SelectComputerIPCellIndexColumn = 2;       // индекс колонки ComputerIP в датагрид
            string SelectComputerNameCell = "";
            string SelectComputerIPCell = "";
            bool dgvcorrect = true;
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                if (dataGridView1.Columns[i].HeaderText.ToString() == "ComputerIP")
                    SelectComputerIPCellIndexColumn = i;
                if (dataGridView1.Columns[i].HeaderText.ToString() == "ComputerName")
                    SelectComputerNameCellIndexColumn = i;
            }

            int selectedRowCount = dataGridView1.Rows.GetRowCount(DataGridViewElementStates.Selected);

            if (DataGridViewSelectedTable.ToLower().Contains("alivehosts") || DataGridViewSelectedTable.ToLower().Contains("archivesofhost"))
            {
                int IndexCurrentRow = dataGridView1.CurrentRow.Index;
                SelectComputerNameCell = dataGridView1.Rows[IndexCurrentRow].Cells[SelectComputerNameCellIndexColumn].Value.ToString();
                SelectComputerIPCell = dataGridView1.Rows[IndexCurrentRow].Cells[SelectComputerIPCellIndexColumn].Value.ToString();

                if (!SelectComputerNameCell.ToLower().Contains("unknow") && SelectComputerNameCell.Trim().Length > 0)
                {
                    _textBoxInputNetOrInputPC(SelectComputerNameCell);
                    GetArchieveOfHostItem.Text = "Архив данных собранных с " + SelectComputerNameCell;
                    GetRegistryItem.Text = "Сканировать реестр " + SelectComputerNameCell;
                    PingItem.Text = "Пинг хоста " + SelectComputerNameCell;
                    GetEventsItem.Text = "Загрузить все события и конфигурацию " + SelectComputerNameCell;
                    GetFileItems.Text = "Получить список файлов на " + SelectComputerNameCell;
                    GetArchieveOfHostItem.Text = "Архив данных собранных с " + SelectComputerNameCell;
                    GetWholeDataItem.Text = "Сбор всей информации c " + SelectComputerNameCell;
                    _PingHostItem("Пинг " + SelectComputerNameCell);
                    _RDPMenuItem("RDP коннект к " + SelectComputerNameCell);
                    _GetLogRemotePCItem("Получить список событий и конфигурацию " + SelectComputerNameCell);
                    _GetFilesItem("Получить список файлов " + SelectComputerNameCell);
                    _GetRegItem("Сканировать реестр " + SelectComputerNameCell);

                    _comboBoxTargedPCAdd(SelectComputerNameCell + " | " + SelectComputerIPCell);
                    Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));
                    _labelCurrentNet(_currentHostNet + ".xxx");

                    try
                    {
                        IPAddress addr = Dns.GetHostAddresses(SelectComputerNameCell)[0];
                        _currentHostName = SelectComputerNameCell;
                        _currentHostIP = addr.ToString();
                        _textBoxInputNetOrInputPC(_currentHostIP);
                        _comboBoxTargedPCAdd(_currentHostName + " | " + _currentHostIP);
                        string[] tmpPC = Regex.Split(_currentHostIP.ToString(), "[.]");
                        _currentHostNet = tmpPC[0].Trim() + "." + tmpPC[1].Trim() + "." + tmpPC[2].Trim();
                        Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));
                        _labelCurrentNet(_currentHostNet + ".xxx");
                        dgvcorrect = true;
                    } catch { dgvcorrect = false; MessageBox.Show("Для получения данных следует выбрать ячейку с именем хоста или его IP\nили же имя выбранного хоста не резолвится"); }

                    if (dgvcorrect)
                    {
                        if (textBoxDomainLength > 2 && textBoxPasswordLength > 0 && textBoxLoginLength > 0) //the domain has inputed. User isn't the domain one 
                        {
                            _User = textBoxDomainText + "\\" + textBoxLoginText;
                            _Password = textBoxPasswordText;
                        }
                        else if (textBoxDomainLength < 3 && textBoxPasswordLength > 0 && textBoxLoginLength > 0) //the domain hasn't inputed. Will use No DOMAIN's USER
                        {
                            _User = _currentHostIP + "\\" + textBoxLoginText;
                            _Password = textBoxPasswordText;
                        }
                        else
                        {
                            _User = "";
                            _Password = "";
                        }

                        NetworkShare share = new NetworkShare(_currentHostName, "ipc$", _User, _Password);
                        share.Connect();

                        Task.WaitAll(Task.Run(() => checkRunningRemoteRegistry(_currentHostName, _currentHostIP, _User, _Password)));

                        ConnectionOptions co = new ConnectionOptions();   //Connect to WMI of the Remote Host 
                        co.Impersonation = ImpersonationLevel.Impersonate;
                        co.Authentication = AuthenticationLevel.Packet;
                        co.EnablePrivileges = true;
                        co.Timeout = new TimeSpan(0, 0, 600);

                        ManagementPath mp = new ManagementPath();
                        mp.NamespacePath = @"\root\cimv2";
                        if (_currentHostName != _myHostName && _User.Length > 0 && _Password.Length > 0)
                        {
                            co.Username = _User;
                            co.Password = _Password;
                            mp.Server = _currentHostName;
                        }
                        else if (_currentHostName != _myHostName && _User.Length == 0 && _Password.Length == 0)
                        { mp.Server = _currentHostName; }

                        ManagementScope ms = new ManagementScope(mp, co);
                        try
                        {
                            ms.Connect();
                            bool isConnected = ms.IsConnected;
                            if (isConnected)
                            { dgvcorrect = true; }
                            else { MessageBox.Show("Для получения данных:\n1. следует указать корректные login\\password\n2.Хост в данный момент должен быть доступен\n3. Должны корректно работать WMI и RemoteRegistry"); }
                        } catch { dgvcorrect = false; }
                    }
                    else { dgvcorrect = false; }

                }
                else if (SelectComputerNameCell.ToLower().Contains("unknow"))
                { dgvcorrect = false; MessageBox.Show("Данный хост не имеет записей в DNS!"); }
                else { dgvcorrect = false; }
            }

            if (dgvcorrect == true) { return true; }
            else { return false; }
        }

        private async void GetRegistry_Click(object sender, EventArgs e)
        {
            comboBoxTargedPC.SelectedIndex = -1;
            textBoxToTemporary();

            _currentRemoteRegistryRunning = true;
            StopSearchItem.Enabled = true;

            waitNetPing = new AutoResetEvent(false);
            waitFile = new AutoResetEvent(false);
            waitStop = new AutoResetEvent(false);
            waitNetStop = new AutoResetEvent(false);
            waitFilePing = new AutoResetEvent(false);

            if (textBoxLoginLength > 0 && textBoxPasswordLength > 0)
            {
                await Task.Run(() => _GetRegistry());
            }
            else
            {
                MessageBox.Show("Не введены данные для подключения!");
                _ControlVisibleAnalyse(false);
                _ControlVisibleLicense(false);
                _ControlVisibleStartUP(true);
                textBoxLogin.Focus();
            }
        }

        private async void _GetRegistry()
        {
            _StatusLabel2ForeColor(System.Drawing.Color.DarkCyan);
            _StatusLabel2Text("Ищу улики в реестре... ");

            if (ParseSelectedCell() == true)
            {
                FileInfo fi = new FileInfo(Application.StartupPath + "\\myEventLoger\\" + _currentHostName + ".db");
                databaseHost = fi.FullName;
                _DBCheckFull(_currentHostName);

                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection))
                    {
                        command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                        command.Parameters.Add(new SQLiteParameter("@Name", "ComputerIP"));
                        command.Parameters.AddWithValue("@Value", _currentHostIP);
                        command.Parameters.AddWithValue("@Value1", _currentHostName);
                        command.Parameters.AddWithValue("@Date", _Date);
                        command.Parameters.AddWithValue("@Time", _Time);
                        try { command.ExecuteNonQuery(); } catch { }
                    }
                    connection.Close();
                }

                Task.Delay(500).Wait();
                _ProgressBar1Value0();
                _StopScannig = false;
                timer1Start();
                _GetTimeRunScan();
                Thread t = new Thread(new ThreadStart(_StatusLabel2ChangeColor));
                t.Start();
                Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));
                await Task.Run(() => _loadRegDocuments());
            }
            else { MessageBox.Show("Для получения данных следует:\n1. выбрать ячейку с именем хоста или IP\n2. Указать корректные учетные данныею\n3. Возможно хост в данный момент не доступен\n3. Возможно выключен WMI или RemoteRegistry"); }
            timer1Stop();
            _StopScannig = true;
            _ProgressBar1Value100();
            _StatusLabel2ForeColor(System.Drawing.Color.Black);
        }

        private void GetArchievesHostsItem_Click(object sender, EventArgs e)
        {
            SelectedPeriod = "";       //Period has not selected
            allRecords = 0;
            ArrayList Empty = new ArrayList();
            dataGridView1.DataSource = Empty;
            DataTable dt = new DataTable("ArchivesOfHost");
            DataColumn[] dc ={
                                  new DataColumn("Name",typeof(string)),
                                  new DataColumn("FullName",typeof(string)),
                                  new DataColumn("IP",typeof(string)),
                                  new DataColumn("Date",typeof(string)),
                                  new DataColumn("Error",typeof(string)),
                                  new DataColumn("Warning",typeof(string)),
                              };
            dt.Columns.AddRange(dc);

            DirectoryInfo rootDirectory = new DirectoryInfo(Application.StartupPath + @"\myEventLoger\");
            var Coder = Encoding.GetEncoding(65001);
            // Task t;
            Stack<string> dirs = new Stack<string>(100);


            if (!Directory.Exists(rootDirectory.FullName))
            { throw new ArgumentException(); }
            dirs.Push(rootDirectory.FullName);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs; string[] files = null;
                try { subDirs = Directory.GetDirectories(currentDir); } catch { continue; }
                try { files = Directory.GetFiles(currentDir); } catch { continue; }
                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file); // Perform whatever action is required in your scenario.
                    string TName = fi.Name.ToString().Remove(fi.Name.ToString().Length - 3);

                    if (fi.FullName.ToString().ToLower().Contains(@".db") == true) //маска для поиска
                    {
                        if (!fi.FullName.ToString().ToLower().Contains("alltables") && !fi.FullName.ToString().ToLower().Contains("alivehosts")) //маска для поиска
                        {
                            try
                            {
                                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}; page_size = 65536; cache_size =100000; journal_mode =MEMORY;", fi.FullName)))
                                {
                                    connection.Open();

                                    SQLiteCommand command = new SQLiteCommand("SELECT Value1 FROM WindowsFeature Where Name like '%ComputerIP%' AND ComputerName like '%" + TName + "%' ;", connection);
                                    var reader = command.ExecuteScalar();
                                    string s = reader.ToString();
                                    if (s != null && s.Length > 1)
                                    {
                                        DataRow dr = dt.NewRow();
                                        dr["Name"] = reader.ToString();
                                        dr["FullName"] = fi.Name.ToString().Remove(fi.Name.ToString().Length - 3) + " | " + reader.ToString();

                                        command = new SQLiteCommand("SELECT Value FROM WindowsFeature Where Name like '%ComputerIP%' AND ComputerName like '%" + TName + "%' ;", connection);
                                        reader = command.ExecuteScalar();
                                        dr["IP"] = reader.ToString();

                                        command = new SQLiteCommand("SELECT Date FROM WindowsFeature Where Name like '%ComputerIP%' AND ComputerName like '%" + TName + "%' ;", connection);
                                        reader = command.ExecuteScalar();
                                        dr["Date"] = reader.ToString();


                                        command = new SQLiteCommand("SELECT Count(Name) FROM WindowsFeature Where Name like '%Warning%' AND ComputerName like '%" + TName + "%' ;", connection);
                                        reader = command.ExecuteScalar();
                                        dr["Warning"] = reader.ToString();

                                        command = new SQLiteCommand("SELECT Count(Name) FROM WindowsFeature Where Name like '%Error%' AND ComputerName like '%" + TName + "%' ;", connection);
                                        reader = command.ExecuteScalar();
                                        dr["Error"] = reader.ToString();
                                        dt.Rows.Add(dr);

                                        connection.Close();
                                        allRecords++;
                                    }
                                }
                            } catch { } // If file was deleted by a separate application
                        }
                    }
                }
                foreach (string str in subDirs)
                { dirs.Push(str); }
            }
            selectedPC = true;
            dataGridView1.DataSource = dt;

            FileInfo fi1 = new FileInfo(Application.StartupPath + @"\myEventLoger\AliveHosts.db");
            currentDbName = fi1.FullName;
            tabControl.SelectedTab = tabDataGrid;
            selectedPC = false;

            DataGridViewSelectedTable = "ArchivesOfHost";
            StatusLabelCurrent.Text = " Строк в выборке: " + allRecords.ToString();
        }

        private async void GetEvents_Click(object sender, EventArgs e)
        {
            _comboBoxTargedPCIndex(-1);
            textBoxToTemporary();
            _currentRemoteRegistryRunning = true;
            ProgressBar1.Value = 0;
            timer1Start();

            for (int i = 0; i < 99; i++)
            { _nameLogs[i] = ""; }

            _StopScanLogEvents = false;
            _StopScannig = false;

            StopSearchItem.Enabled = true;

            waitNetPing = new AutoResetEvent(false);
            waitFile = new AutoResetEvent(false);
            waitStop = new AutoResetEvent(false);
            waitNetStop = new AutoResetEvent(false);
            waitFilePing = new AutoResetEvent(false);


            if (ParseSelectedCell() == true)
            {
                FileInfo fi = new FileInfo(Application.StartupPath + "\\myEventLoger\\" + _currentHostName + ".db");
                databaseHost = fi.FullName;
                _DBCheckFull(_currentHostName);

                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection))
                    {
                        command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                        command.Parameters.Add(new SQLiteParameter("@Name", "ComputerIP"));
                        command.Parameters.AddWithValue("@Value", _currentHostIP);
                        command.Parameters.AddWithValue("@Value1", _currentHostName);
                        command.Parameters.AddWithValue("@Date", _Date);
                        command.Parameters.AddWithValue("@Time", _Time);
                        try { command.ExecuteNonQuery(); } catch { }
                    }
                    connection.Close();
                }

                Task.Delay(200).Wait();
                _GetTimeRunScan();
                Thread t = new Thread(new ThreadStart(_StatusLabel2ChangeColor));
                t.Start();
                Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));
                _StatusLabel2Text("Получаю конфигурацию хоста и список событий... ");

                NetworkShare share = new NetworkShare(_currentHostName, "ipc$", _User, _Password);
                share.Connect();

                Task.Delay(500).Wait();
                Task.WaitAll(Task.Run(() => Task.Run(() => checkRunningRemoteRegistry(_currentHostName, _currentHostIP, _User, _Password)))); //Лучше использовать IP
                _ProgressWork5();

                await Task.Run(() => _ReadRemoteRegistry(_currentHostName, _currentHostIP, _User, _Password));
                _ProgressWork5();

                await Task.Run(() => _ReadRemotePCNet(_currentHostName, _currentHostIP, _User, _Password));
                _ProgressWork5();

                await Task.Run(() => DisplayEventLogPropertiesRemotePC(_currentHostName, _currentHostIP, _User, _Password));
                _ProgressWork5();

                if (_currentRemoteRegistryRunning == true)
                {
                    await Task.Run(() => _GetRemoteNameLogs(_currentHostName));
                    for (int i = 0; i < 99; i++)
                    {
                        if (_nameLogs[i] != null && _nameLogs[i].Length > 1)
                        {
                            await Task.Run(() => _ReadRemotePCEventLog(_currentHostName, _currentHostIP, _nameLogs[i], _User, _Password));
                        }
                    }
                }
                else
                { await Task.Run(() => _ReadRemotePCFullEventLogs(_currentHostName, _currentHostIP, _User, _Password)); }

                _ProgressWork5();

                await Task.WhenAll();
                await Task.Run(() => _TagStopWork(_currentHostName));
                Task.Delay(500).Wait();
                share.Disconnect();  // DisConnect to the remote drive

                if (_StopScanLogEvents == false && _Authorized == true)
                {
                    StatusLabel2.ForeColor = System.Drawing.Color.Black;
                    StatusLabel2.Text = "Завершен сбор событий и параметров конфигурации ";
                }

                else if (_StopScanLogEvents == false && _Authorized == false)
                {
                    StatusLabel2.ForeColor = System.Drawing.Color.Crimson;
                    StatusLabel2.Text = "Cбор событий и параметров конфигурации завершен с ошибками";
                }
                else
                { }
            }
            else { }
        }

        private void GetFiles_Click(object sender, EventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\ready\\tmpfile.stop");
            if (File.Exists(di.FullName)) { try { File.Delete(di.FullName); } catch { } }
            comboBoxTargedPC.SelectedIndex = -1;
            textBoxToTemporary();

            _currentRemoteRegistryRunning = true;
            StopSearchItem.Enabled = true;

            _StopSearchFiles = false;
            waitNetPing = new AutoResetEvent(false);
            waitFile = new AutoResetEvent(false);
            waitStop = new AutoResetEvent(false);
            waitNetStop = new AutoResetEvent(false);
            waitFilePing = new AutoResetEvent(false);

            if (ParseSelectedCell() == true)
            {
                FileInfo fi = new FileInfo(Application.StartupPath + "\\myEventLoger\\" + _currentHostName + ".db");
                databaseHost = fi.FullName;
                _DBCheckFull(_currentHostName);

                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection))
                    {
                        command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                        command.Parameters.Add(new SQLiteParameter("@Name", "ComputerIP"));
                        command.Parameters.AddWithValue("@Value", _currentHostIP);
                        command.Parameters.AddWithValue("@Value1", _currentHostName);
                        command.Parameters.AddWithValue("@Date", _Date);
                        command.Parameters.AddWithValue("@Time", _Time);
                        try { command.ExecuteNonQuery(); } catch { }
                    }
                    connection.Close();
                }
                _StatusLabel2ForeColor(System.Drawing.Color.DarkCyan);
                Task.Delay(500).Wait();
                ProgressBar1.Value = 0;
                _StopScannig = false;
                timer1Start();
                _GetTimeRunScan();
                Thread t = new Thread(new ThreadStart(_StatusLabel2ChangeColor));
                t.Start();
                Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));
                _StatusLabel2Text("Получаю список файлов... ");

                Thread t1 = new Thread(new ParameterizedThreadStart((obj) => SearchFilesOnLogicalDisks(_currentHostName)));
                t1.Start();

                Thread t2 = new Thread(new ThreadStart(_CheckFoundFiles));
                t2.Start();
            }
            else { MessageBox.Show("Для получения данных следует:\n1. Выбрать ячейку с именем хоста или IP\n2. Указать корректные учетные данные\n3. Возможно хост в данный момент не доступен\n4. Возможно выключены WMI или RemoteRegistry"); }
        }

        private void GetArchiveOfHost_Click(object sender, EventArgs e) //Чтение базы собранных данных
        {
            selectedPC = false;
            if (DataGridViewSelectedTable == "AliveHosts")
            {
                try
                {  //                string dgvCell = dataGridView1.CurrentCell.Value.ToString().Trim();
                    string NameCollum = dataGridView1.Columns[dataGridView1.CurrentCell.ColumnIndex].HeaderText.ToString();
                    int selectedRowCount = dataGridView1.Rows.GetRowCount(DataGridViewElementStates.Selected);
                    int IndexCurrentRow = dataGridView1.CurrentRow.Index;
                    string SelectComputerNameCell = dataGridView1.Rows[IndexCurrentRow].Cells[0].Value.ToString();
                    string SelectComputerIPCell = dataGridView1.Rows[IndexCurrentRow].Cells[2].Value.ToString();
                    _currentHostName = SelectComputerNameCell;
                    _currentHostIP = SelectComputerIPCell;
                    string[] tmpPC = Regex.Split(_currentHostIP.ToString(), "[.]");
                    _currentHostNet = tmpPC[0].Trim() + "." + tmpPC[1].Trim() + "." + tmpPC[2].Trim();
                    _textBoxInputNetOrInputPC(SelectComputerIPCell);
                    _labelCurrentNet(_currentHostNet + ".xxx");

                    FileInfo fi = new FileInfo(Application.StartupPath + "\\myEventLoger\\" + SelectComputerNameCell + ".db");
                    if (File.Exists(fi.FullName) == true)
                    {
                        _dataGridView1CurrentRowDefaultCellStyleBackColor(System.Drawing.Color.DarkSeaGreen); //Selected row
                        databaseHost = fi.FullName.ToString();
                        _DBCheckFull(SelectComputerNameCell);
                        _StatusLabel2Text("Данные сканирования " + SelectComputerNameCell + " загружены!");
                        Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));
                        DataGridViewSelectedTable = "ArchiveOfHost";
                    }
                    else
                    {
                        _dataGridView1CurrentRowDefaultCellStyleBackColor(System.Drawing.Color.SandyBrown); //Selected row
                        MessageBox.Show(SelectComputerNameCell + " ранее не сканировался!");
                        tabControl.SelectedTab = tabDataGrid;
                    }
                    selectedPC = true;
                } catch { }
            }
            else if (DataGridViewSelectedTable == "ArchivesOfHost")
            {
                try
                {
                    string NameCollum = dataGridView1.Columns[dataGridView1.CurrentCell.ColumnIndex].HeaderText.ToString();
                    int selectedRowCount = dataGridView1.Rows.GetRowCount(DataGridViewElementStates.Selected);
                    int IndexCurrentRow = dataGridView1.CurrentRow.Index;
                    string SelectComputerNameCell = dataGridView1.Rows[IndexCurrentRow].Cells[0].Value.ToString();
                    string SelectComputerIPCell = dataGridView1.Rows[IndexCurrentRow].Cells[1].Value.ToString();
                    _currentHostName = SelectComputerNameCell;
                    _currentHostIP = SelectComputerIPCell;
                    string[] tmpPC = Regex.Split(_currentHostIP.ToString(), "[.]");
                    _currentHostNet = tmpPC[0].Trim() + "." + tmpPC[1].Trim() + "." + tmpPC[2].Trim();
                    _textBoxInputNetOrInputPC(SelectComputerIPCell);
                    _labelCurrentNet(_currentHostNet + ".xxx");

                    FileInfo fi = new FileInfo(Application.StartupPath + "\\myEventLoger\\" + SelectComputerNameCell + ".db");
                    if (File.Exists(fi.FullName) == true)
                    {
                        _dataGridView1CurrentRowDefaultCellStyleBackColor(System.Drawing.Color.DarkSeaGreen); //Selected row
                        databaseHost = fi.FullName.ToString();
                        _DBCheckFull(SelectComputerNameCell);
                        _StatusLabel2Text("Данные сканирования " + SelectComputerNameCell + " загружены!");
                        Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));
                        DataGridViewSelectedTable = "ArchiveOfHost";
                    }
                    else
                    {
                        _dataGridView1CurrentRowDefaultCellStyleBackColor(System.Drawing.Color.SandyBrown); //Selected row
                        MessageBox.Show(SelectComputerNameCell + " ранее не сканировался!");
                        tabControl.SelectedTab = tabDataGrid;
                    }
                    selectedPC = true;
                } catch { }
            }
        }

        private void GetPreviousScanItem_Click(object sender, EventArgs e)
        {
            SelectedPeriod = "";       //Period has not selected

            string sql = " Select ComputerName, ComputerDomainName, ComputerIP, ComputerModel, ComputerSN, LogOnUser, Date FROM AliveHosts GROUP BY ComputerName ORDER BY ComputerName LIMIT 1000 ";

            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\AliveHosts.db");
            DataGridViewSelectedTable = "AliveHosts";
            currentDbName = fi.FullName;
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", currentDbName)))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand(" SELECT COUNT(Id) AS AllRecords FROM AliveHosts ", connection);
                allRecords = Convert.ToInt32(command.ExecuteScalar());
                StatusLabelCurrent.Text = " Строк в выборке: " + allRecords.ToString();
                connection.Close();
            }

            _UpdateDataGrid(sql, currentDbName);
            tabControl.SelectedTab = tabDataGrid;
            _ContextDataGridViewData(@"myEventLoger\AliveHosts.db", "AliveHosts");
            selectedPC = false;
            DeSelectUnknowItem.Visible = true;
            DeSelectUnknowNoPermissionItem.Visible = true;
        }

        private void DeSelectUnknowItem_Click(object sender, EventArgs e)
        {
            string sql = "";
            if (SelectedPeriod.Length < 1)
            { sql = " Select ComputerName, ComputerDomainName, ComputerIP, ComputerModel, ComputerSN, LogOnUser, Date FROM AliveHosts Where ComputerName NOT LIKE '%unknow%' GROUP BY ComputerName ORDER BY ComputerIP LIMIT 1000 "; }
            else { sql = " Select ComputerName, ComputerDomainName, ComputerIP, ComputerModel, ComputerSN, LogOnUser, Date FROM AliveHosts Where ComputerName NOT LIKE '%unknow%' AND Date like '%" + SelectedPeriod + "%' GROUP BY ComputerName ORDER BY ComputerIP LIMIT 1000 "; }
            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\AliveHosts.db");
            DataGridViewSelectedTable = "AliveHosts";
            currentDbName = fi.FullName;
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", currentDbName)))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand(" SELECT COUNT(Id) AS AllRecords FROM AliveHosts ", connection);
                allRecords = Convert.ToInt32(command.ExecuteScalar());
                StatusLabelCurrent.Text = " Строк в выборке: " + allRecords.ToString();
                connection.Close();
            }

            _UpdateDataGrid(sql, currentDbName);
            tabControl.SelectedTab = tabDataGrid;
            _ContextDataGridViewData(@"myEventLoger\AliveHosts.db", "AliveHosts");
            selectedPC = false;
        }

        private void DeSelectUnknowNoPermissionItem_Click(object sender, EventArgs e)
        {
            string sql = "";
            if (SelectedPeriod.Length < 1)
            { sql = " Select ComputerName, ComputerDomainName, ComputerIP, ComputerModel, ComputerSN, LogOnUser, Date FROM AliveHosts Where ComputerName NOT LIKE '%unknow%' AND ComputerModel NOT like '%Permission%' GROUP BY ComputerName ORDER BY ComputerIP LIMIT 1000 "; }
            else { sql = sql = " Select ComputerName, ComputerDomainName, ComputerIP, ComputerModel, ComputerSN, LogOnUser, Date FROM AliveHosts Where ComputerName NOT LIKE '%unknow%' AND ComputerModel NOT like '%Permission%' AND Date like '%" + SelectedPeriod + "%' GROUP BY ComputerName ORDER BY ComputerIP LIMIT 1000 "; }

            FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\AliveHosts.db");
            DataGridViewSelectedTable = "AliveHosts";
            currentDbName = fi.FullName;
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", currentDbName)))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand(" SELECT COUNT(Id) AS AllRecords FROM AliveHosts ", connection);
                allRecords = Convert.ToInt32(command.ExecuteScalar());
                StatusLabelCurrent.Text = " Строк в выборке: " + allRecords.ToString();
                connection.Close();
            }

            _UpdateDataGrid(sql, currentDbName);
            tabControl.SelectedTab = tabDataGrid;
            _ContextDataGridViewData(@"myEventLoger\AliveHosts.db", "AliveHosts");
            selectedPC = false;
        }

        private void _ContextDataGridViewData(string DBPath, string Table)
        {
            ScannedNetsTime1.Visible = false;
            ScannedNetsTime2.Visible = false;
            ScannedNetsTime3.Visible = false;

            for (int i = 0; i < 3; i++)
            { _DateScan[i] = ""; }

            HashSet<String> _listDateScan = new HashSet<string>();
            DirectoryInfo info = new DirectoryInfo(DBPath);
            if (File.Exists(info.FullName))
            {
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", DBPath)))
                {
                    connection.Open();
                    try
                    {
                        SQLiteCommand command = new SQLiteCommand(" SELECT Date FROM " + Table + " GROUP BY Date ORDER BY Date DESC ; ", connection);
                        SQLiteDataReader reader = command.ExecuteReader();
                        foreach (DbDataRecord record in reader)
                        {
                            if (record != null)
                            { _listDateScan.Add(record["Date"].ToString()); }
                        }
                        reader.Close();
                    } catch (Exception e) { MessageBox.Show(e.ToString()); }
                    connection.Close();
                }
            }
            string[] listDateScan = _listDateScan.ToArray();
            if (listDateScan.Length > 0)
            {
                for (int i = 0; i < listDateScan.Length; i++)
                {
                    if (listDateScan[i].ToString().Trim().Length > 3)
                    {
                        if (i < 3)
                        {
                            _DateScan[i] = listDateScan[i];
                        }
                        else
                        { continue; }
                    }
                }
                for (int i = 0; i < 3; i++)
                {
                    if (_DateScan[i].ToString().Length > 3)
                    {
                        if (i == 0)
                        {
                            ScannedNetsTime1.Visible = true;
                            ScannedNetsTime1.Text = "Поиск хостов от: " + _DateScan[0];
                            ScannedNetsTime1.Tag = _DateScan[0];
                        }
                        if (i == 1)
                        {
                            ScannedNetsTime2.Visible = true;
                            ScannedNetsTime2.Text = "Поиск хостов от: " + _DateScan[1];
                            ScannedNetsTime2.Tag = _DateScan[1];
                        }
                        if (i == 2)
                        {
                            ScannedNetsTime3.Visible = true;
                            ScannedNetsTime3.Text = "Поиск хостов от: " + _DateScan[2];
                            ScannedNetsTime3.Tag = _DateScan[2];
                        }
                    }
                }
            }
        }

        private void _ContexListBoxAdd() //ContextMenu for ListBoxNets
        {
            System.Windows.Forms.ContextMenu contextMenuNetsMenu1;
            contextMenuNetsMenu1 = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem loadPreviosNetItem;
            loadPreviosNetItem = new System.Windows.Forms.MenuItem();

            System.Windows.Forms.MenuItem SimpleScan;
            SimpleScan = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem LongScan;
            LongScan = new System.Windows.Forms.MenuItem();

            System.Windows.Forms.MenuItem menuItem1;
            menuItem1 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem menuItem2;
            menuItem2 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem menuItem3;
            menuItem3 = new System.Windows.Forms.MenuItem();

            System.Windows.Forms.MenuItem menuItemDate1;
            menuItemDate1 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem menuItemDate2;
            menuItemDate2 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem menuItemDate3;
            menuItemDate3 = new System.Windows.Forms.MenuItem();

            contextMenuNetsMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { loadPreviosNetItem, SimpleScan, LongScan, menuItem1, menuItemDate1, menuItemDate2, menuItemDate3 });

            loadPreviosNetItem.Index = 0;
            loadPreviosNetItem.Text = "Предыдущие результаты поиска сетей";
            loadPreviosNetItem.Click += new System.EventHandler(this._loadPreviosNet_Click);

            SimpleScan.Index = 1;
            SimpleScan.Text = "Сканировать выбранные сети...";
            SimpleScan.Click += new System.EventHandler(this.scanSelectedNetworkItem_Click);

            LongScan.Index = 2;
            LongScan.Text = "Поиск ближайших сетей";//
            LongScan.Click += new System.EventHandler(this.scanEnvironmentItem_Click);

            menuItem1.Index = 3;
            menuItem1.Text = "-";

            menuItem2.Index = 3;
            menuItem2.Text = "-";

            menuItem3.Index = 3;
            menuItem3.Text = "-";

            menuItemDate1.Index = 4;
            if (_NetDateScan[0].Length > 3)
            {
                menuItemDate1.Text = "Хосты, обнаруженные " + _NetDateScan[0];
                menuItemDate1.Click += new System.EventHandler(this._loadDatePreviosNet1_Click);
            }
            else { menuItemDate1.Visible = false; }

            menuItemDate2.Index = 5;
            if (_NetDateScan[1].Length > 3)
            {
                menuItemDate2.Text = "Хосты, обнаруженные " + _NetDateScan[1];
                menuItemDate2.Click += new System.EventHandler(this._loadDatePreviosNet2_Click);
            }
            else { menuItemDate2.Visible = false; }

            menuItemDate3.Index = 6;
            if (_NetDateScan[2].ToString().Trim().Length > 3)
            {
                menuItemDate3.Text = "Хосты, обнаруженные " + _NetDateScan[2];
                menuItemDate3.Click += new System.EventHandler(this._loadDatePreviosNet3_Click);
            }
            else { menuItemDate3.Visible = false; }

            listBoxNetsRow.ContextMenu = contextMenuNetsMenu1;
        }

        private void _loadPreviosNet_Click(object Sender, EventArgs e)
        {
            listBoxNetsRow.Items.Clear();
            _loadPreviousNet();
        }

        private void _loadPreviousNet()
        {
            HashSet<String> _listNet = new HashSet<string>();
            DirectoryInfo info = new DirectoryInfo(databaseAllTables);
            if (File.Exists(info.FullName))
            {
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", databaseAllTables)))
                {
                    connection.Open();
                    try
                    {
                        SQLiteCommand command = new SQLiteCommand(" SELECT Net FROM AliveNets; ", connection);
                        SQLiteDataReader reader = command.ExecuteReader();

                        foreach (DbDataRecord record in reader)
                        {
                            if (record != null)
                            {
                                string id = record["Net"].ToString();
                                if (record["Net"].ToString().ToLower().Contains(".xxx"))
                                { _listNet.Add(record["Net"].ToString()); }
                            }
                        }
                        reader.Close();
                    } catch (Exception e) { MessageBox.Show(e.ToString()); }
                    connection.Close();
                    connection.Dispose();
                }
            }
            string[] listNet = _listNet.ToArray();
            foreach (string s in listNet)
            { listBoxNetsRow.Items.Add(s); }
        }

        private string _SelectedDateScan = "";
        private void _loadDatePreviosNet1_Click(object Sender, EventArgs e)
        {
            listBoxNetsRow.Items.Clear();
            _SelectedDateScan = _NetDateScan[0];
            _loadSelectedPreviosNet(_SelectedDateScan);
        }

        private void _loadDatePreviosNet2_Click(object Sender, EventArgs e)
        {
            listBoxNetsRow.Items.Clear();
            _SelectedDateScan = _NetDateScan[1];
            _loadSelectedPreviosNet(_SelectedDateScan);
        }

        private void _loadDatePreviosNet3_Click(object Sender, EventArgs e)
        {
            listBoxNetsRow.Items.Clear();
            _SelectedDateScan = _NetDateScan[2];
            _loadSelectedPreviosNet(_SelectedDateScan);
        }

        private void _loadSelectedPreviosNet(string s)
        {
            HashSet<String> _listNet = new HashSet<string>();
            DirectoryInfo info = new DirectoryInfo(databaseAllTables);
            if (File.Exists(info.FullName))
            {
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", databaseAllTables)))
                {
                    connection.Open();
                    try
                    {
                        SQLiteCommand command = new SQLiteCommand(" SELECT Net FROM AliveNets where Date Like '%" + s + "%'; ", connection);
                        SQLiteDataReader reader = command.ExecuteReader();

                        foreach (DbDataRecord record in reader)
                        {
                            if (record != null)
                            {
                                string id = record["Net"].ToString();
                                if (record["Net"].ToString().ToLower().Contains(".xxx"))
                                { _listNet.Add(record["Net"].ToString()); }
                            }
                        }
                        reader.Close();
                    } catch (Exception e) { MessageBox.Show(e.ToString()); }
                    connection.Close();
                    connection.Dispose();
                }
            }
            string[] listNet = _listNet.ToArray();
            foreach (string st in listNet)
            { listBoxNetsRow.Items.Add(st); }
        }

        private void _ContextData(string DBPath, string Table, string s)
        {
            string sql = "SELECT ComputerName, ComputerDomainName, ComputerIP, ComputerModel, ComputerSN, LogOnUser, Date FROM " + Table + " Where  ComputerName NOT LIKE '%unknow%' AND ComputerModel NOT like '%Permission%' AND Date like '%" + s + "%' GROUP BY ComputerIP ORDER BY ComputerIP ;";
            _UpdateDataGrid(sql, DBPath);
        }

        private void ScannedNetsTime1_Click(object sender, EventArgs e)
        {
            SelectedPeriod = ScannedNetsTime1.Tag.ToString();
            currentDbName = @"myEventLoger\AliveHosts.db";
            _ContextData(@"myEventLoger\AliveHosts.db", "AliveHosts", ScannedNetsTime1.Tag.ToString());
            selectedPC = false;
        }

        private void ScannedNetsTime2_Click(object sender, EventArgs e)
        {
            SelectedPeriod = ScannedNetsTime2.Tag.ToString();
            currentDbName = @"myEventLoger\AliveHosts.db";
            _ContextData(@"myEventLoger\AliveHosts.db", "AliveHosts", ScannedNetsTime2.Tag.ToString());
        }

        private void ScannedNetsTime3_Click(object sender, EventArgs e)
        {
            SelectedPeriod = ScannedNetsTime3.Tag.ToString();
            currentDbName = @"myEventLoger\AliveHosts.db";
            _ContextData(@"myEventLoger\AliveHosts.db", "AliveHosts", ScannedNetsTime3.Tag.ToString());
            selectedPC = false;
        }

        private void GetDataMenu_DropDownOpened(object sender, EventArgs e)
        {
            string SelectComputerNameCell = "";
            string SelectComputerIPCell = "";
            if (textBoxDomain.TextLength > 1)
                GetHostsFromDomainItem.Text = "Получить перечень хостов и логинов из домена " + textBoxDomain.Text;
            if (tabControl.SelectedTab == tabDataGrid)
            {
                if (DataGridViewSelectedTable == "AliveHosts" || DataGridViewSelectedTable == "ArchivesOfHost")
                {
                    try
                    {
                        string NameCollum = dataGridView1.Columns[dataGridView1.CurrentCell.ColumnIndex].HeaderText.ToString();
                        int selectedRowCount = dataGridView1.Rows.GetRowCount(DataGridViewElementStates.Selected);
                        int IndexCurrentRow = dataGridView1.CurrentRow.Index;
                        SelectComputerNameCell = dataGridView1.Rows[IndexCurrentRow].Cells[0].Value.ToString();
                        SelectComputerIPCell = dataGridView1.Rows[IndexCurrentRow].Cells[2].Value.ToString();
                        _currentHostName = SelectComputerNameCell;

                        _textBoxInputNetOrInputPC(SelectComputerNameCell);
                        _PingHostItem("Пинг " + SelectComputerNameCell);
                        _RDPMenuItem("RDP коннект с " + SelectComputerNameCell);
                        _GetLogRemotePCItem("Получить список событий и конфигурацию " + SelectComputerNameCell);
                        _PingItem("Пинг хоста " + SelectComputerNameCell);
                        _GetEventsItem("Загрузить список событий и конфигурацию " + SelectComputerNameCell);
                        _GetFilesItem("Получить список файлов " + SelectComputerNameCell);
                        _GetRegItem("Сканировать реестр " + SelectComputerNameCell);
                        _FullScanItem("Сбор всей информации c " + SelectComputerNameCell);
                        _GetWholeDataItem("Сбор всей информации c " + SelectComputerNameCell);
                    } catch { DataGridViewSelectedTable = ""; }
                }
            }
            else
            {
                SelectComputerNameCell = textBoxInputNetOrInputPC.Text.Trim().ToUpper();
                _PingHostItem("Пинг " + SelectComputerNameCell);
                _RDPMenuItem("RDP коннект с " + SelectComputerNameCell);
                _GetLogRemotePCItem("Получить список событий и конфигурацию " + SelectComputerNameCell);
                _PingItem("Пинг хоста " + SelectComputerNameCell);
                _GetEventsItem("Загрузить список событий и конфигурацию " + SelectComputerNameCell);
                _GetFilesItem("Получить список файлов " + SelectComputerNameCell);
                _GetRegItem("Сканировать реестр " + SelectComputerNameCell);
                _FullScanItem("Сбор всей информации c " + SelectComputerNameCell);
                _GetWholeDataItem("Сбор всей информации c " + SelectComputerNameCell);
            }
            ControlHostMenu.Enabled = true;
        }

        private async void GetWholeDataItem_Click(object sender, EventArgs e)
        {
            comboBoxTargedPC.SelectedIndex = -1;
            textBoxToTemporary();
            _currentRemoteRegistryRunning = true;

            if (ParseSelectedCell() == true)
            {
                Task.Delay(500).Wait();

                Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));
                StatusLabel2.Text = "Проверяю доступность хоста... ";
                FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
                databaseHost = fi.FullName;
                _DBCheckFull(_currentHostName);
                StatusLabel2.ForeColor = System.Drawing.Color.DarkCyan;

                Thread t = new Thread(new ThreadStart(_StatusLabel2ChangeColor));
                t.Start();

                StopSearchItem.Enabled = true;

                waitFile = new AutoResetEvent(false);
                waitStop = new AutoResetEvent(false);
                waitNetStop = new AutoResetEvent(false);
                waitFilePing = new AutoResetEvent(false);
                waitNetPing = new AutoResetEvent(false);
                _StopSearchFiles = false;
                _StopSearchAliveHosts = false;
                _StopScanLogEvents = false;

                _StopScannig = false;
                timer1Start();
                ProgressBar1.Value = 0;

                _GetTimeRunScan();
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;cache_size =100000;journal_mode =MEMORY;", databaseHost)))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand("INSERT INTO 'WindowsFeature' ('ComputerName', 'Name', 'Value', 'Value1', 'Date', 'Time') VALUES (@ComputerName, @Name, @Value, @Value1, @Date, @Time)", connection))
                    {
                        command.Parameters.AddWithValue("@ComputerName", _currentHostName);
                        command.Parameters.AddWithValue("@Name", "ComputerIP");
                        command.Parameters.AddWithValue("@Value", _currentHostIP);
                        command.Parameters.AddWithValue("@Value1", _currentHostName);
                        command.Parameters.AddWithValue("@Date", _Date);
                        command.Parameters.AddWithValue("@Time", _Time);
                        try { command.ExecuteNonQuery(); } catch { }
                    }
                    connection.Close();
                }

                NetworkShare share = new NetworkShare(_currentHostName, "ipc$");//no use authintification 
                if (_currentHostIP != _myIP) //Remote PC
                {
                    share = new NetworkShare(_currentHostName, "ipc$", _User, _Password);
                    share.Connect();
                    Task.Delay(500).Wait();

                    await Task.Run(() => checkRunningRemoteRegistry(_currentHostName, _currentHostIP, _User, _Password)); //Лучше использовать IP
                    Task.WaitAll();
                    _ProgressWork5();

                    await Task.Run(() => _ReadRemoteRegistry(_currentHostName, _currentHostIP, _User, _Password));
                    _ProgressWork5();

                    await Task.Run(() => _ReadRemotePCNet(_currentHostName, _currentHostIP, _User, _Password));
                    _ProgressWork5();

                    await Task.Run(() => DisplayEventLogPropertiesRemotePC(_currentHostName, _currentHostIP, _User, _Password));
                    _ProgressWork5();

                    if (_currentRemoteRegistryRunning == true)
                    {
                        await Task.Run(() => _GetRemoteNameLogs(_currentHostName));
                        foreach (string NameLog in _nameLogs)
                        {
                            if (NameLog != null && NameLog.Length > 1)
                            {
                                await Task.Run(() => _ReadRemotePCEventLog(_currentHostName, _currentHostIP, NameLog, _User, _Password));
                            }
                        }
                    }
                    else
                    { await Task.Run(() => _ReadRemotePCFullEventLogs(_currentHostName, _currentHostIP, _User, _Password)); }
                    _ProgressWork5();

                    timer1Stop();
                    _RunTimer = false;
                    Task.WaitAll();
                    Task.Delay(500).Wait();
                    share.Disconnect();  // DisConnect to the remote drive
                }
                else //local PC
                {
                    await Task.Run(() => _LocalConfigFromWMI());
                    _GetNameLogs();
                    await Task.Run(() => GetLogPropertiesServicesProcess());
                    await Task.Run(() => _ReadUsbfromLocalRegistry());

                    await Task.Run(() => _GetUsersName());
                    foreach (string NameLog in _nameLogs)
                    {
                        if (NameLog != null && NameLog.Length > 1)
                        { await Task.Run(() => _loadlogEventLog(NameLog)); }
                    }

                    await Task.Run(() => _loadlogEventLog("", "1"));
                    Task.Delay(500).Wait();
                }
                await Task.WhenAll();
                Task.Delay(2000).Wait();

                // Scan Реестра
                ProgressBar1.Value = 0;
                timer1Start();
                await Task.Run(() => _loadRegDocuments());
                Task.WaitAll();

                // Поиск файлов на разделах хоста
                DirectoryInfo di = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\ready\\tmpfile.stop");
                if (File.Exists(di.FullName)) { try { File.Delete(di.FullName); } catch { } }
                DirectoryInfo di1 = new DirectoryInfo(Application.StartupPath + "\\myEventLoger\\tmp\\tmpfile.stop");
                if (File.Exists(di1.FullName)) { try { File.Delete(di1.FullName); } catch { } }

                StatusLabel2.ForeColor = System.Drawing.Color.DarkCyan;
                Task.Delay(500).Wait();
                ProgressBar1.Value = 0;
                timer1Start();

                _GetTimeRunScan();
                t = new Thread(new ThreadStart(_StatusLabel2ChangeColor));
                t.Start();
                StatusLabel2.Text = "Получаю список файлов... ";

                Thread t1 = new Thread(new ParameterizedThreadStart((obj) => SearchFilesOnLogicalDisks(_currentHostName)));
                t1.Start();

                Thread t2 = new Thread(new ThreadStart(_CheckFoundFiles));
                t2.Start();
                Task.Delay(5000).Wait();

                while (t1.IsAlive)  //Проверить корректность условия
                { Task.Delay(2000).Wait(); }
                share.Disconnect();
            }
            else { MessageBox.Show("Для получения данных следует:\n1. Выбрать ячейку с именем хоста или IP\n2. Указать корректные учетные данные\n3. Возможно хост в данный момент не доступен\n4. Возможно выключены WMI или RemoteRegistry"); }
        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            string datagridTable = DataGridViewSelectedTable.ToLower();
            if (datagridTable.Contains("alivehosts") || datagridTable.Contains("archivesofhost"))
            {
                try
                {
                    string NameCollum = dataGridView1.Columns[dataGridView1.CurrentCell.ColumnIndex].HeaderText.ToString(); //имя колонки выбранной ячейки

                    int SelectComputerNameCellIndexColumn = 0;     // индекс колонки ComputerName в датагрид
                    int SelectComputerIPCellIndexColumn = 2;       // индекс колонки ComputerIP в датагрид
                    for (int i = 0; i < dataGridView1.ColumnCount; i++)
                    {
                        if (dataGridView1.Columns[i].HeaderText.ToString() == "ComputerIP")
                        { SelectComputerIPCellIndexColumn = i; }
                        if (dataGridView1.Columns[i].HeaderText.ToString() == "ComputerName")
                        { SelectComputerNameCellIndexColumn = i; }
                    }

                    int selectedRowCount = dataGridView1.Rows.GetRowCount(DataGridViewElementStates.Selected);
                    int IndexCurrentRow = dataGridView1.CurrentRow.Index;
                    string SelectComputerNameCell = dataGridView1.Rows[IndexCurrentRow].Cells[SelectComputerNameCellIndexColumn].Value.ToString();
                    string SelectComputerIPCell = dataGridView1.Rows[IndexCurrentRow].Cells[SelectComputerIPCellIndexColumn].Value.ToString();

                    if (SelectComputerNameCell.ToLower().Contains("unknow"))
                    { MessageBox.Show("Этот хост не имеет записи в DNS"); }
                    else if (!SelectComputerNameCell.ToLower().Contains("unknow") && SelectComputerNameCell.Trim().Length > 0)
                    {
                        FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
                        databaseHost = fi.FullName;
                        _DBCheckFull(_currentHostName);

                        GetArchieveOfHostItem.Text = "Архив данных собранных с " + SelectComputerNameCell;
                        GetRegistryItem.Text = "Сканировать реестр " + SelectComputerNameCell;
                        PingItem.Text = "Пинг хоста " + SelectComputerNameCell;
                        GetEventsItem.Text = "Загрузить все события и конфигурацию " + SelectComputerNameCell;
                        GetFileItems.Text = "Получить список файлов на " + SelectComputerNameCell;
                        GetWholeDataItem.Text = "Сбор всей информации c " + SelectComputerNameCell;
                        GetArchieveOfHostItem.Text = "Архив данных собранных с " + SelectComputerNameCell;

                        _currentHostName = SelectComputerNameCell;
                        _currentHostIP = SelectComputerIPCell;
                        string[] tmpPC = Regex.Split(_currentHostIP.ToString(), "[.]");
                        _currentHostNet = tmpPC[0].Trim() + "." + tmpPC[1].Trim() + "." + tmpPC[2].Trim();
                        textBoxInputNetOrInputPC.Text = SelectComputerNameCell;
                        _labelCurrentNet(_currentHostNet + ".xxx");
                        Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));
                        selectedPC = true;     //?????????????????????????

                        _UpdateDataGrid("select * from WindowsFeature", databaseHost); //Select of the last data of the Host
                        //                       tabControl.SelectedTab = tabPageCtrl;
                        comboTables.SelectedIndex = 3;
                    }
                    else
                    { }
                } catch { }
            }
            else if (datagridTable.Contains("usersbyhostscontrol"))
            {
                try
                {
                    string NameCollum = dataGridView1.Columns[dataGridView1.CurrentCell.ColumnIndex].HeaderText.ToString(); //имя колонки выбранной ячейки

                    int SelectComputerNameCellIndexColumn = 0;     // индекс колонки ComputerName в датагрид
                    int SelectComputerIPCellIndexColumn = 4;       // индекс колонки ComputerIP в датагрид
                    int SelectUserCellIndexColumn = 5;             // индекс колонки UserAdmin в датагрид
                    int SelectPasswordCellIndexColumn = 6;             // индекс колонки PasswordUserAdmin в датагрид
                    int SelectComputerDomainCellIndexColumn = 3;         // индекс колонки Domain в датагрид
                    int SelectGroupCellIndexColumn = 2;         // индекс колонки GroupHosts в датагрид

                    for (int i = 0; i < dataGridView1.ColumnCount; i++)
                    {
                        if (dataGridView1.Columns[i].HeaderText.ToString() == "ComputerIP")
                        { SelectComputerIPCellIndexColumn = i; }
                        else if (dataGridView1.Columns[i].HeaderText.ToString() == "ComputerName")
                        { SelectComputerNameCellIndexColumn = i; }
                        else if (dataGridView1.Columns[i].HeaderText.ToString() == "ComputerDomainName")
                        { SelectComputerDomainCellIndexColumn = i; }
                        else if (dataGridView1.Columns[i].HeaderText.ToString() == "User")
                        { SelectUserCellIndexColumn = i; }
                        else if (dataGridView1.Columns[i].HeaderText.ToString() == "Password")
                        { SelectPasswordCellIndexColumn = i; }
                        else if (dataGridView1.Columns[i].HeaderText.ToString() == "GroupHosts")
                        { SelectGroupCellIndexColumn = i; }
                    }

                    int selectedRowCount = dataGridView1.Rows.GetRowCount(DataGridViewElementStates.Selected);
                    int IndexCurrentRow = dataGridView1.CurrentRow.Index;
                    _currentHostName = dataGridView1.Rows[IndexCurrentRow].Cells[SelectComputerNameCellIndexColumn].Value.ToString();
                    _currentHostIP = dataGridView1.Rows[IndexCurrentRow].Cells[SelectComputerIPCellIndexColumn].Value.ToString();
                    textBoxDomain.Text = dataGridView1.Rows[IndexCurrentRow].Cells[SelectComputerDomainCellIndexColumn].Value.ToString();
                    textBoxInputNetOrInputPC.Text = _currentHostIP;
                    textBoxLogin.Text = dataGridView1.Rows[IndexCurrentRow].Cells[SelectUserCellIndexColumn].Value.ToString();
                    textBoxPassword.Text = dataGridView1.Rows[IndexCurrentRow].Cells[SelectPasswordCellIndexColumn].Value.ToString();
                    textBoxNameProgramm.Text = dataGridView1.Rows[IndexCurrentRow].Cells[SelectGroupCellIndexColumn].Value.ToString();

                    FileInfo fi = new FileInfo(Application.StartupPath + @"\myEventLoger\" + _currentHostName + @".db");
                    databaseHost = fi.FullName;
                    _DBCheckFull(_currentHostName);

                    GetArchieveOfHostItem.Text = "Архив данных собранных с " + _currentHostName;
                    GetRegistryItem.Text = "Сканировать реестр " + _currentHostName;
                    PingItem.Text = "Пинг хоста " + _currentHostName;
                    GetEventsItem.Text = "Загрузить все события и конфигурацию " + _currentHostName;
                    GetFileItems.Text = "Получить список файлов на " + _currentHostName;
                    GetWholeDataItem.Text = "Сбор всей информации c " + _currentHostName;
                    GetArchieveOfHostItem.Text = "Архив данных собранных с " + _currentHostName;

                    string[] tmpPC = Regex.Split(_currentHostIP.ToString(), "[.]");
                    _currentHostNet = tmpPC[0].Trim() + "." + tmpPC[1].Trim() + "." + tmpPC[2].Trim();
                    _labelCurrentNet(_currentHostNet + ".xxx");
                    Task.Run(() => _StatusLabelCurrentText("Выбран " + _currentHostName + " | " + _currentHostIP));
                    selectedPC = true;     //?????????????????????????

                    tabControl.SelectedTab = tabPageCtrl;
                } catch { }
            }
        }

        // -------- /\/\/\/\ -------- // Context Menu for DataGridView. End of the Block   // -------- /\/\/\/\ -------- //

            

        private void checkBoxChangeStateService_CheckStateChanged(object sender, EventArgs e)
        {
            if (checkBoxChangeStateService.Checked)
                buttonGetServices.Text = "Остановить/запустить службу";
            else
                buttonGetServices.Text = "Получить список служб";
        }

        private void CorrectAccessErrorItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (RegistryKey regCorrectError = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Lsa", true))
                { regCorrectError.SetValue("LmCompatibilityLevel", "1", RegistryValueKind.DWord); }
            } catch (Exception expt) { MessageBox.Show(expt.Message); }
        }

    }//The End of Form1

    //-----------------/\/\/\/\-----------------// Классы и структуры. Начало блока //-----------------/\/\/\/\-----------------//

    public class PersonDB
    {
        public string NAVOrId;
        public string UserFIOUkr;
        public string UserFIO;
        public string UserLogin;
        public string UserMail;
        public string UserFoder;
        public string UserAddInfo;
        public string CityTSPCurrent;
        public string BirthDay;
        public string BirthDayPlace;

        public string EmploymentDate;
        public string DismissalDate;
        public string WorkInCompany;
        public string Subdivision;
        public string CityTSP;
        public string UserPosition;

        public string UserAddress;
        public string UserPhone;
        public string PhoneType;

        public string PasportId;
        public string PasportDate;
        public string PasportStaff;
        public string PasportType;
        public int idxNAVOrId;
        public int idxUserFIOUkr;
        public int idxUserFIO;
        public int idxUserLogin;
        public int idxUserMail;
        public int idxUserFoder;
        public int idxUserAddInfo;
        public int idxCityTSPCurrent;
        public int idxBirthDay;
        public int idxBirthDayPlace;

        public int idxEmploymentDate;
        public int idxDismissalDate;
        public int idxWorkInCompany;
        public int idxSubdivision;
        public int idxCityTSP;
        public int idxUserPosition;

        public int idxUserAddress;
        public int idxUserPhone;
        public int idxPhoneType;

        public int idxPasportId;
        public int idxPasportDate;
        public int idxPasportStaff;
        public int idxPasportType;
    }

    //---------------/\/\/\/\---------------// работа с реестром, офлайн юзерами (function _loadRemoteHive()). Start of the Block //---------------/\/\/\/\---------------//
    //https://social.msdn.microsoft.com/Forums/vstudio/en-US/d0d485b8-c3d1-49d0-8180-0515d9cfb04e/read-and-modify-ntuserdat-file?forum=csharpgeneral
    //http://stackoverflow.com/questions/7894909/load-registry-hive-from-c-sharp-fails
    public class RegistryInterop
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct LUID
        {
            public int LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TOKEN_PRIVILEGES
        {
            public LUID Luid;
            public int Attributes;
            public int PrivilegeCount;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        public static extern int OpenProcessToken(int ProcessHandle, int DesiredAccess, ref int tokenhandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetCurrentProcess();

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        public static extern int LookupPrivilegeValue(string lpsystemname, string lpname, [MarshalAs(UnmanagedType.Struct)] ref LUID lpLuid);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        public static extern int AdjustTokenPrivileges(int tokenhandle, int disableprivs, [MarshalAs(UnmanagedType.Struct)]ref TOKEN_PRIVILEGES Newstate, int bufferlength, int PreivousState, int Returnlength);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int RegLoadKey(uint hKey, string lpSubKey, string lpFile);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int RegUnLoadKey(uint hKey, string lpSubKey);

        public const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        public const int TOKEN_QUERY = 0x00000008;
        public const int SE_PRIVILEGE_ENABLED = 0x00000002;
        public const string SE_RESTORE_NAME = "SeRestorePrivilege";
        public const string SE_BACKUP_NAME = "SeBackupPrivilege";
        public const uint HKEY_USERS = 0x80000003;
        public const uint HKEY_CURRENT_USER = 0x80000001;
        public const uint HKEY_LOCAL_MACHINE = 0x80000002;

        //temporary hive key
        //        public const string HIVE_SUBKEY = "RemoteRegLoad";
        static private Boolean gotPrivileges = false;
        static private void GetPrivileges()
        {
            int token = 0;
            int retval = 0;
            TOKEN_PRIVILEGES tpRestore = new TOKEN_PRIVILEGES();
            TOKEN_PRIVILEGES tpBackup = new TOKEN_PRIVILEGES();
            LUID RestoreLuid = new LUID();
            LUID BackupLuid = new LUID();

            retval = OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref token);
            retval = LookupPrivilegeValue(null, SE_RESTORE_NAME, ref RestoreLuid);
            retval = LookupPrivilegeValue(null, SE_BACKUP_NAME, ref BackupLuid);


            tpRestore.PrivilegeCount = 1;
            tpRestore.Attributes = SE_PRIVILEGE_ENABLED;
            tpRestore.Luid = RestoreLuid;

            tpBackup.PrivilegeCount = 1;
            tpBackup.Attributes = SE_PRIVILEGE_ENABLED;
            tpBackup.Luid = BackupLuid;

            retval = AdjustTokenPrivileges(token, 0, ref tpRestore, 1024, 0, 0);
            retval = AdjustTokenPrivileges(token, 0, ref tpBackup, 1024, 0, 0);

            gotPrivileges = true;
        }

        static public string Load(string file, string HIVE_SUBKEY)
        {
            if (!gotPrivileges)
                GetPrivileges();
            RegLoadKey(HKEY_USERS, HIVE_SUBKEY, file);
            return HIVE_SUBKEY;
        }

        static public void Unload(string HIVE_SUBKEY)
        {
            if (!gotPrivileges)
                GetPrivileges();
            int output = RegUnLoadKey(HKEY_USERS, HIVE_SUBKEY);
        }
    }

    //---------------/\/\/\/\---------------// работа с реестром, офлайн юзерами (function _loadRemoteHive()). End of the Block //---------------/\/\/\/\---------------//


    //---------------/\/\/\/\---------------// TCPstat. (function _ListNetworkProfiles()). Start of the Block //---------------/\/\/\/\---------------// 

    public class Disconnector
    {
        // Enumeration of the states 
        public enum State
        {
            /// <summary> All </summary> 
            All = 0,
            /// <summary> Closed </summary> 
            Closed = 1,
            /// <summary> Listen </summary> 
            Listen = 2,
            /// <summary> Syn_Sent </summary> 
            Syn_Sent = 3,
            /// <summary> Syn_Rcvd </summary> 
            Syn_Rcvd = 4,
            /// <summary> Established </summary> 
            Established = 5,
            /// <summary> Fin_Wait1 </summary> 
            Fin_Wait1 = 6,
            /// <summary> Fin_Wait2 </summary> 
            Fin_Wait2 = 7,
            /// <summary> Close_Wait </summary> 
            Close_Wait = 8,
            /// <summary> Closing </summary> 
            Closing = 9,
            /// <summary> Last_Ack </summary> 
            Last_Ack = 10,
            /// <summary> Time_Wait </summary> 
            Time_Wait = 11,
            /// <summary> Delete_TCB </summary> 
            Delete_TCB = 12
        }

        private struct MIB_TCPROW  // Connection info 
        {
            public int dwState;
            public int dwLocalAddr;
            public int dwLocalPort;
            public int dwRemoteAddr;
            public int dwRemotePort;
        }

        //API to get list of connections 
        [DllImport("iphlpapi.dll")]
        private static extern int GetTcpTable(IntPtr pTcpTable, ref int pdwSize, bool bOrder);

        //API to change status of connection 
        [DllImport("iphlpapi.dll")]
        //private static extern int SetTcpEntry(MIB_TCPROW tcprow); 
        private static extern int SetTcpEntry(IntPtr pTcprow);

        //Convert 16-bit value from network to host byte order 
        [DllImport("wsock32.dll")]
        private static extern int ntohs(int netshort);

        //Convert 16-bit value back again 
        [DllImport("wsock32.dll")]
        private static extern int htons(int netshort);

        /// <summary> 
        /// Close all connection to the remote IP 
        /// </summary> 
        /// <param name="IP">IP remote PC</param> 
        public static void CloseRemoteIP(string IP)
        {
            MIB_TCPROW[] rows = getTcpTable();
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i].dwRemoteAddr == IPStringToInt(IP))
                {
                    rows[i].dwState = (int)State.Delete_TCB;
                    IntPtr ptr = GetPtrToNewObject(rows[i]);
                    int ret = SetTcpEntry(ptr);
                }
            }
        }

        /// <summary> 
        /// Close all connections at current local IP 
        /// </summary> 
        /// <param name="IP"></param> 
        public static void CloseLocalIP(string IP)
        {
            MIB_TCPROW[] rows = getTcpTable();
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i].dwLocalAddr == IPStringToInt(IP))
                {
                    rows[i].dwState = (int)State.Delete_TCB;
                    IntPtr ptr = GetPtrToNewObject(rows[i]);
                    int ret = SetTcpEntry(ptr);
                }
            }
        }

        /// <summary> 
        /// Closes all connections to the remote port 
        /// </summary> 
        /// <param name="port"></param> 
        public static void CloseRemotePort(int port)
        {
            MIB_TCPROW[] rows = getTcpTable();
            for (int i = 0; i < rows.Length; i++)
            {
                if (port == ntohs(rows[i].dwRemotePort))
                {
                    rows[i].dwState = (int)State.Delete_TCB;
                    IntPtr ptr = GetPtrToNewObject(rows[i]);
                    int ret = SetTcpEntry(ptr);
                }
            }
        }

        /// <summary> 
        /// Closes all connections to the local port 
        /// </summary> 
        /// <param name="port"></param> 
        public static void CloseLocalPort(int port)
        {
            MIB_TCPROW[] rows = getTcpTable();
            for (int i = 0; i < rows.Length; i++)
            {
                if (port == ntohs(rows[i].dwLocalPort))
                {
                    rows[i].dwState = (int)State.Delete_TCB;
                    IntPtr ptr = GetPtrToNewObject(rows[i]);
                    int ret = SetTcpEntry(ptr);
                }
            }
        }

        /// <summary> 
        /// Close a connection by returning the connectionstring 
        /// </summary> 
        /// <param name="connectionstring"></param> 
        public static void CloseConnection(string connectionstring)
        {
            try
            {
                //Split the string to its subparts 
                string[] parts = connectionstring.Split('-');
                if (parts.Length != 4) throw new Exception("Invalid connectionstring - use the one provided by Connections.");
                string[] loc = parts[0].Split(':');
                string[] rem = parts[1].Split(':');
                string[] locaddr = loc[0].Split('.');
                string[] remaddr = rem[0].Split('.');

                //Fill structure with data 
                MIB_TCPROW row = new MIB_TCPROW();
                row.dwState = 12;
                byte[] bLocAddr = new byte[] { byte.Parse(locaddr[0]), byte.Parse(locaddr[1]), byte.Parse(locaddr[2]), byte.Parse(locaddr[3]) };
                byte[] bRemAddr = new byte[] { byte.Parse(remaddr[0]), byte.Parse(remaddr[1]), byte.Parse(remaddr[2]), byte.Parse(remaddr[3]) };
                row.dwLocalAddr = BitConverter.ToInt32(bLocAddr, 0);
                row.dwRemoteAddr = BitConverter.ToInt32(bRemAddr, 0);
                row.dwLocalPort = htons(int.Parse(loc[1]));
                row.dwRemotePort = htons(int.Parse(rem[1]));

                //Make copy of the structure into memory and use the pointer to call SetTcpEntry 
                IntPtr ptr = GetPtrToNewObject(row);
                int ret = SetTcpEntry(ptr);

                if (ret == -1) throw new Exception("Unsuccessful");
                if (ret == 65) throw new Exception("User has no sufficient privilege to execute this API successfully");
                if (ret == 87) throw new Exception("Specified port is not in state to be closed down");
                if (ret != 0) throw new Exception("Unknown error (" + ret + ")");
            } catch (Exception ex)
            { throw new Exception("CloseConnection failed (" + connectionstring + ")! [" + ex.GetType().ToString() + "," + ex.Message + "]"); }
        }

        public static string[] Connections()  // Gets all connections 
        { return Connections(State.All); }

        public static string[] Connections(State state)  // Gets a connection list of connections with a defined state 
        {
            MIB_TCPROW[] rows = getTcpTable();
            ArrayList arr = new ArrayList();

            foreach (MIB_TCPROW row in rows)
            {
                if (state == State.All || state == (State)row.dwState)
                {
                    string localaddress = IPIntToString(row.dwLocalAddr) + ":" + ntohs(row.dwLocalPort);
                    string remoteaddress = IPIntToString(row.dwRemoteAddr) + ":" + ntohs(row.dwRemotePort);
                    arr.Add("Local: " + localaddress + "\t\tRemote: " + remoteaddress + "\t\tState: " + ((State)row.dwState).ToString() + "\t" + row.dwState);
                }
            }
            return (string[])arr.ToArray(typeof(System.String));
        }

        private static MIB_TCPROW[] getTcpTable()      //The function that fills the MIB_TCPROW array with connection info 
        {
            IntPtr buffer = IntPtr.Zero; bool allocated = false;
            try
            {
                int iBytes = 0;
                GetTcpTable(IntPtr.Zero, ref iBytes, false); //Getting size of return data 
                buffer = Marshal.AllocCoTaskMem(iBytes); //allocating the datasize 
                allocated = true;
                GetTcpTable(buffer, ref iBytes, false); //Run it again to fill the memory with the data 
                int structCount = Marshal.ReadInt32(buffer); // Get the number of structures 

                IntPtr buffSubPointer = buffer; //Making a pointer that will point into the buffer 
                buffSubPointer = (IntPtr)((int)buffer + 4); //Move to the first data (ignoring dwNumEntries from the original MIB_TCPTABLE struct) 

                MIB_TCPROW[] tcpRows = new MIB_TCPROW[structCount]; //Declaring the array 

                //Get the struct size 
                MIB_TCPROW tmp = new MIB_TCPROW();
                int sizeOfTCPROW = Marshal.SizeOf(tmp);

                //Fill the array 1 by 1 
                for (int i = 0; i < structCount; i++)
                {
                    tcpRows[i] = (MIB_TCPROW)Marshal.PtrToStructure(buffSubPointer, typeof(MIB_TCPROW)); //copy struct data 
                    buffSubPointer = (IntPtr)((int)buffSubPointer + sizeOfTCPROW); //move to next structdata 
                }
                return tcpRows;
            } catch (Exception ex)
            { throw new Exception("getTcpTable failed! [" + ex.GetType().ToString() + "," + ex.Message + "]"); } finally
            { if (allocated) Marshal.FreeCoTaskMem(buffer); } //Free the allocated memory 
        }

        private static IntPtr GetPtrToNewObject(object obj)
        {
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(obj));
            Marshal.StructureToPtr(obj, ptr, false);
            return ptr;
        }

        private static int IPStringToInt(string IP)        //Convert an IP string to the INT value 
        {
            if (IP.IndexOf(".") < 0) throw new Exception("Invalid IP address");
            string[] addr = IP.Split('.');
            if (addr.Length != 4) throw new Exception("Invalid IP address");
            byte[] bytes = new byte[] { byte.Parse(addr[0]), byte.Parse(addr[1]), byte.Parse(addr[2]), byte.Parse(addr[3]) };
            return BitConverter.ToInt32(bytes, 0);
        }

        private static string IPIntToString(int IP)   //Convert an IP integer to IP string
        {
            byte[] addr = System.BitConverter.GetBytes(IP);
            return addr[0] + "." + addr[1] + "." + addr[2] + "." + addr[3];
        }
    }

    //---------------/\/\/\/\---------------// TCPstat. (function _ListNetworkProfiles()). End of the Block //---------------/\/\/\/\---------------// 

    //-----------------\/\/\/\/-----------------// Классы и структуры. Конец блока //-----------------/\/\/\/-----------------//

}
//The End of Programm