using PoEW.Application.Controls;
using PoEW.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PoEW.Application.Views {
    public enum LoginType {
        POESSESID,
        PoEWebsite,
        Steam
    };

    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow {
        public LoginType LoginType { get; set; }
        public string POESESSID = null;
        private LoginWebBrowser _webBrowser = new LoginWebBrowser();

        public bool Success { get; private set; } = false;

        public LoginWindow() {
            InitializeComponent();

            SetupEvents();
        }

        /// <summary>
        /// Initialiaze Events of the Window
        /// </summary>
        private void SetupEvents() {
            _webBrowser.Closed += WebBrowser_Closed;
        }

        /// <summary>
        /// Set SessionId in the XAML
        /// </summary>
        /// <param name="player">Existing player informations</param>
        public void SetPlayer(Player player) {
            loginDialogPOESESSID.txtPOESSESID.Text = player.SessionId;
        }

        /// <summary>
        /// Open the login web browser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabPoEWebsiteLogin_MouseUp(object sender, MouseButtonEventArgs e) {
            ShowLoginWebBrowser();
        }

        /// <summary>
        /// Show the login web browser
        /// </summary>
        private void ShowLoginWebBrowser() {
            Visibility = Visibility.Hidden;
            _webBrowser.ShowDialog();
        }


        /// <summary>
        /// Handle login process when the login browser is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebBrowser_Closed(object sender, EventArgs e) {
            HandleWebBrowserLogin();
        }

        /// <summary>
        /// Set the POESESSID for the authentication process
        /// </summary>
        private void HandleWebBrowserLogin() {
            if (_webBrowser.POESESSID != null) {
                POESESSID = _webBrowser.POESESSID;
                Success = true;
                Close();
            }
        }

        /// <summary>
        /// Set the login type according to the selected tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabcLoginOptions_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            SetLoginType(((TabItem)e.AddedItems[0]).Header.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tabHeader">Value of the selected item in the Tab Control</param>
        private void SetLoginType(string tabHeader) {
            switch (tabHeader) {
                case "POESESSID Login":
                    LoginType = LoginType.POESSESID;
                    btnLogin.IsEnabled = true;
                    break;

                case "Steam Login":
                    LoginType = LoginType.Steam;
                    btnLogin.IsEnabled = false;
                    break;

                case "PoE Website Login":
                    LoginType = LoginType.PoEWebsite;
                    btnLogin.IsEnabled = true;
                    break;
            }
        }

        private void VerifyLogin() {
            switch (LoginType) {
                case LoginType.POESSESID:
                    POESESSID = loginDialogPOESESSID.txtPOESSESID.Text;
                    if (Utils.ValidSessionId(POESESSID)) {
                        Success = true;
                        Close();
                    }
                    break;

                case LoginType.Steam:
                    // TODO: implement
                    break;
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e) {
            VerifyLogin();
        }
    }
}
