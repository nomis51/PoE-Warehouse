using CefSharp;
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

namespace PoEW.Application.Views {
    /// <summary>
    /// Interaction logic for WebBrowser.xaml
    /// </summary>
    public partial class LoginWebBrowser {
        private string PoEWebsiteLoginPage = "https://www.pathofexile.com/login";
        private string PoEWebsiteAccountPage = "https://www.pathofexile.com/my-account";
        private string PoESessionIdCookieName = "POESESSID";
        public string POESESSID = null;

        private CookieCollector _cookieVisitor;

        public bool Success { get; private set; } = false;

        public LoginWebBrowser() {
            InitializeComponent();

            _cookieVisitor = new CookieCollector();

            SetupEvents();
        }

        private void SetupEvents() {
            webBrowser.FrameLoadEnd += WebBrowser_FrameLoadEnd;
            webBrowser.Address = PoEWebsiteLoginPage;
        }

        private async Task GetSessiondId() {
            var cookieManager = Cef.GetGlobalCookieManager();
            cookieManager.VisitAllCookies(_cookieVisitor);
            var cookies = await _cookieVisitor.Task;
            var poeSessionIdCookie = cookies.Find(c => c.Name == PoESessionIdCookieName);

            if (poeSessionIdCookie != null) {
                POESESSID = poeSessionIdCookie.Value;
            }
        }

        private void VerifyLogin() {
            if (Utils.ValidSessionId(POESESSID)) {
                this.Dispatcher.Invoke(() => {
                    Success = true;
                    Close();
                });
            }
        }

        private async void WebBrowser_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e) {
            if (e.Url == PoEWebsiteAccountPage) {
                await GetSessiondId();
                VerifyLogin();
            }
        }
    }
}
