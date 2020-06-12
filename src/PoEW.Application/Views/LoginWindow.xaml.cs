using PoEW.Application.Controls;
using PoEW.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
        private WebBrowser _webBrowser = new WebBrowser();

        public bool Success { get; private set; } = false;

        public LoginWindow() {
            InitializeComponent();

            _webBrowser.Closed += WebBrowser_Closed;
        }

        public void SetPlayer(Player player) {
            loginDialogPOESESSID.txtPOESSESID.Text = player.SessionId;
        }

        private void tabPoEWebsiteLogin_MouseUp(object sender, MouseButtonEventArgs e) {
            Visibility = Visibility.Hidden;
            _webBrowser.ShowDialog();
        }

        private void WebBrowser_Closed(object sender, EventArgs e) {
            if (_webBrowser.POESESSID != null) {
                POESESSID = _webBrowser.POESESSID;
                Success = true;
                Close();
            }
        }

        private void tabcLoginOptions_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            SetLoginType(((TabItem)e.AddedItems[0]).Header.ToString());
        }

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

        private void btnLogin_Click(object sender, RoutedEventArgs e) {
            switch (LoginType) {
                case LoginType.POESSESID:
                    POESESSID = loginDialogPOESESSID.txtPOESSESID.Text;
                    Success = true;
                    Close();
                    break;

                case LoginType.Steam:
                    break;
            }
        }
    }
}
