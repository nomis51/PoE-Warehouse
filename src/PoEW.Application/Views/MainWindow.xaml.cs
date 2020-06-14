using MahApps.Metro.Controls;
using PoEW.API;
using PoEW.API.Models;
using PoEW.Application.Views;
using PoEW.Data;
using PoEW.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Button = System.Windows.Controls.Button;
using Price = PoEW.Data.Price;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using ListViewItem = System.Windows.Controls.ListViewItem;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using CefSharp.Wpf;
using CefSharp;

namespace PoEW.Application {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow {
        private string Url_PoETrade = "https://www.pathofexile.com/trade";
        private string Url_PoENinja_Currency_Template = "https://poe.ninja/$league$/currency";
        private string Url_PoENinja_Builds = "https://poe.ninja/challenge/builds";
        private Regex regPoENinjaUrl = new Regex("https://poe.ninja/[a-z]+/(builds|currency)");
        private string LastUrlLoaded = "";
        private string LoadingUrl;

        ObservableCollection<HamburgerMenuItemBase> HamburderMenuItems = new ObservableCollection<HamburgerMenuItemBase>();
        HamburgerMenuHeaderItem HamburgerMenuHeader;
        HamburgerMenuSeparatorItem HamburgerMenuSeparator;

        public MainWindow() {
            InitializeComponent();

            Init();

            WindowController.Instance().LoginWin.ShowDialog();
        }

        private void Init() {
            LoadingUrl = Url_PoETrade;

            var lastPlayer = Session.Instance().GetLastPlayer().Result;

            if (lastPlayer != null) {
                WindowController.Instance().LoginWin.SetPlayer(lastPlayer);
            }

            InitHamburgerMenuItems();

            SetupEvents();
        }

        private async Task SetPoETradeBrowserCookie() {
            var cookieManager = Cef.GetGlobalCookieManager();
            Cookie cookie = new Cookie();
            cookie.Name = "POESESSID";
            cookie.Value = Session.Instance().Player.SessionId;
            if (await cookieManager.SetCookieAsync("https://www.pathofexile.com/trade", cookie)) {
                var g = 0;
            }
        }

        private void SetupEvents() {
            WindowController.Instance().LoginWin.Closed += LoginWin_Closed;
            WindowController.Instance().ShopFormWin.IsVisibleChanged += ShopFormWin_IsVisibleChanged; ;
            stashTabSelectorControl.OnStashTabSelected += StashTabSelectorControl_OnStashTabSelected;
            Session.OnLocalStashTabsUpdated += Session_OnLocalStashTabsUpdated;
            webBrowser_PoENinja_Builds.FrameLoadEnd += WebBrowser_FrameLoadEnd;
            webBrowser_PoENinja_Builds.FrameLoadStart += WebBrowser_FrameLoadStart;
            webBrowser_PoENinja_ChallengeCurrency.FrameLoadEnd += WebBrowser_FrameLoadEnd;
            webBrowser_PoENinja_ChallengeCurrency.FrameLoadStart += WebBrowser_FrameLoadStart;
            webBrowser_PoENinja_StandardCurrency.FrameLoadEnd += WebBrowser_FrameLoadEnd;
            webBrowser_PoENinja_StandardCurrency.FrameLoadStart += WebBrowser_FrameLoadStart;
            webBrowser_PoETrade.FrameLoadEnd += WebBrowser_FrameLoadEnd;
            webBrowser_PoETrade.FrameLoadStart += WebBrowser_FrameLoadStart;
            webBrowser_PoETrade.IsBrowserInitializedChanged += WebBrowser_PoETrade_IsBrowserInitializedChanged; ;
        }

        private void ShopFormWin_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
          if(WindowController.Instance().ShopFormWin.Visibility == Visibility.Hidden) {
                ShopFormWin_Closed();
            }
        }

        private void WebBrowser_PoETrade_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e) {
            _ = SetPoETradeBrowserCookie();
        }

        private void InitHamburgerMenuItems() {
            HamburgerMenuHeader = new HamburgerMenuHeaderItem();
            HamburgerMenuHeader.Label = "Shops";
            HamburgerMenuSeparator = new HamburgerMenuSeparatorItem();
        }

        private ChromiumWebBrowser GetVisibleBrowser() {
            if (webBrowser_PoETrade.Visibility == Visibility.Visible) {
                return webBrowser_PoETrade;
            } else if (webBrowser_PoENinja_StandardCurrency.Visibility == Visibility.Visible) {
                return webBrowser_PoENinja_StandardCurrency;
            } else if (webBrowser_PoENinja_ChallengeCurrency.Visibility == Visibility.Visible) {
                return webBrowser_PoENinja_ChallengeCurrency;
            } else if (webBrowser_PoENinja_Builds.Visibility == Visibility.Visible) {
                return webBrowser_PoENinja_Builds;
            }

            return null;
        }

        private void WebBrowser_FrameLoadStart(object sender, CefSharp.FrameLoadStartEventArgs e) {
            ChromiumWebBrowser webBrowser = (ChromiumWebBrowser)sender;
            this.Dispatcher.Invoke(() => {
                var visibleBrowser = GetVisibleBrowser();

                if (visibleBrowser != null && visibleBrowser.Name == webBrowser.Name) {
                    if (LastUrlLoaded != webBrowser.Address) {
                        loaderWebBrowser.IsActive = true;
                    }
                }
            });
        }

        private void WebBrowser_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e) {
            ChromiumWebBrowser webBrowser = (ChromiumWebBrowser)sender;
            this.Dispatcher.Invoke(() => {
                var visibleBrowser = GetVisibleBrowser();

                if (visibleBrowser != null && visibleBrowser.Name == webBrowser.Name) {
                    if (LastUrlLoaded != webBrowser.Address) {
                        LastUrlLoaded = webBrowser.Address;

                        if (regPoENinjaUrl.IsMatch(webBrowser.Address)) {
                            webBrowser.ZoomLevel = 5.46149645 * Math.Log(60) - 25.12;
                        } else {
                            webBrowser.ZoomLevel = 0;
                        }

                        loaderWebBrowser.IsActive = false;
                    }
                }
            });
        }

        private void Session_OnLocalStashTabsUpdated(Dictionary<int, StashTab> stashTabs) {
            stashTabSelectorControl.ClearTabs();

            foreach (var tab in stashTabs) {
                stashTabSelectorControl.AddTab(tab.Key, tab.Value.Name);
            }

            stashTabControl.SetStashTab(stashTabs.Values.ElementAt(Session.Instance().GetSelectedTabIndex()), Session.Instance().GetShop().GetPrices());
        }

        private void StashTabSelectorControl_OnStashTabSelected(int index) {
            Session.Instance().SetSelectedTab(index);
            stashTabControl.SetStashTab(Session.Instance().GetShop().GetStashTab(index), Session.Instance().GetShop().GetPrices());
        }

        private async void HandleShopWinClosed() {
            await Session.Instance().AddShop(WindowController.Instance().ShopFormWin.ThreadId, WindowController.Instance().ShopFormWin.League, WindowController.Instance().ShopFormWin.GenerateNewThread);
            Session.Instance().SetCurrentThreadId(WindowController.Instance().ShopFormWin.ThreadId);

            HamburgerMenuGlyphItem item = new HamburgerMenuGlyphItem();
            item.Label = Session.Instance().GetShop().Title;
            item.Glyph = WindowController.Instance().ShopFormWin.League;
            AddHamMenuItem(item);

            _ = InitUI(WindowController.Instance().ShopFormWin.ThreadId, WindowController.Instance().ShopFormWin.League);
        }

        private void SetHamMenuItems(List<HamburgerMenuItemBase> items) {
            HamburderMenuItems.Clear();
            HamburderMenuItems.Add(HamburgerMenuHeader);
            HamburderMenuItems.Add(HamburgerMenuSeparator);

            foreach (var item in items) {
                HamburderMenuItems.Add(item);
            }

            hamMenShopThreads.ItemsSource = HamburderMenuItems;
        }

        private void AddHamMenuItem(HamburgerMenuItemBase item) {
            HamburderMenuItems.Add(item);
            hamMenShopThreads.ItemsSource = HamburderMenuItems;
        }

        private void ShopFormWin_Closed() {
            if (WindowController.Instance().ShopFormWin.Success) {
                if (!Session.Instance().AnyShops(WindowController.Instance().ShopFormWin.ThreadId)) {
                    HandleShopWinClosed();
                }
            } else if (!Session.Instance().AnyShops()) {
                WindowController.Instance().ShopFormWin.SetLeagues(Session.Instance().GetLeagues());
                WindowController.Instance().ShopFormWin.ShowDialog();
            }
        }

        private async Task InitUI(int threadId, string league) {
            Session.Instance().IsLocalStashUpdaterPaused = true;

            txtbThreadId.Text = $"Shop {threadId}";
            txtbLeague.Text = league;

            await Session.Instance().LoadLocalStash();

            stashTabSelectorControl.ClearTabs();

            if (!Session.Instance().GetShop().GetStashTabs().Any()) {
                stashTabControl.ClearStashTab();
                await Session.Instance().UpdateLocalStash(threadId);
            }

            foreach (var tab in Session.Instance().GetShop().GetStashTabsName()) {
                stashTabSelectorControl.AddTab(tab.Value, tab.Key);
            }

            if (Session.Instance().GetShop().GetStashTabs().Any()) {
                Session.Instance().SetSelectedTab(0);
                stashTabControl.SetStashTab(Session.Instance().GetShop().GetStashTab(0), Session.Instance().GetShop().GetPrices());
                stashTabSelectorControl.SetActiveTab(0);
            }

            Session.Instance().IsLocalStashUpdaterPaused = false;

            loaderStash.IsActive = false;
        }

        private async Task HandleLogin() {
            await Session.Instance().SetConnectedPlayer(new Player() {
                SessionId = WindowController.Instance().LoginWin.POESESSID
            });

            btnAccount.Content = Session.Instance().Player.AccountName;
        }

        private void SetHamMenuItemsFromShops() {
            HamburderMenuItems.Add(HamburgerMenuHeader);
            HamburderMenuItems.Add(HamburgerMenuSeparator);

            foreach (var shop in Session.Instance().GetShops()) {
                HamburgerMenuGlyphItem item = new HamburgerMenuGlyphItem();
                item.Label = shop.Title;
                item.Glyph = shop.League.Name.ToString();
                HamburderMenuItems.Add(item);
            }

            hamMenShopThreads.ItemsSource = HamburderMenuItems;
        }

        private async void LoginWin_Closed(object sender, EventArgs e) {
            if (!WindowController.Instance().LoginWin.Success) {
                System.Windows.Application.Current.Shutdown();
                return;
            }

            await HandleLogin();

            Session.Instance().RunAutoOnlineUpdater();

            await Session.Instance().LoadShops();

            SetHamMenuItemsFromShops();

            hamMenShopThreads.SelectedIndex = 0;

            if (!Session.Instance().AnyShops()) {
                WindowController.Instance().ShopFormWin.SetLeagues(Session.Instance().GetLeagues());
                WindowController.Instance().ShopFormWin.ShowDialog();
            } else {
                await InitUI(Session.Instance().CurrentThreadId, Session.Instance().GetShop().League.Name);
            }
        }

        private void btnAddShop_Click(object sender, RoutedEventArgs e) {
            WindowController.Instance().ShopFormWin.SetLeagues(Session.Instance().GetLeagues());
            WindowController.Instance().ShopFormWin.ShowDialog();
        }

        private void toggleResourceMenu_Click(object sender, RoutedEventArgs e) {
            popResourceMenu.IsOpen = !popResourceMenu.IsOpen;
        }

        private void menItQuit_Click(object sender, RoutedEventArgs e) {
            System.Windows.Application.Current.Shutdown();
        }

        private void menItPoETrade_Click(object sender, RoutedEventArgs e) {
            popResourceMenu.IsOpen = false;

            HideBrowsers();

            if (webBrowser_PoETrade.Visibility != Visibility.Visible) {
                webBrowser_PoETrade.Visibility = Visibility.Visible;
            }
        }

        private void menuItPoENinjaBuilds_Click(object sender, RoutedEventArgs e) {
            popResourceMenu.IsOpen = false;

            HideBrowsers();

            if (webBrowser_PoENinja_Builds.Visibility != Visibility.Visible) {
                webBrowser_PoENinja_Builds.Visibility = Visibility.Visible;
            }
        }

        private void menuItPoENinjaCurrency_Click(object sender, RoutedEventArgs e) {
            popResourceMenu.IsOpen = false;

            HideBrowsers();

            if (Session.Instance().AnyShops()) {
                if (Session.Instance().GetShop().League.Name == "Standard") {
                    if (webBrowser_PoENinja_StandardCurrency.Visibility != Visibility.Visible) {
                        webBrowser_PoENinja_StandardCurrency.Visibility = Visibility.Visible;
                    }
                } else {
                    if (webBrowser_PoENinja_ChallengeCurrency.Visibility != Visibility.Visible) {
                        webBrowser_PoENinja_ChallengeCurrency.Visibility = Visibility.Visible;
                    }
                }

            } else {
                if (webBrowser_PoENinja_ChallengeCurrency.Visibility != Visibility.Visible) {
                    webBrowser_PoENinja_ChallengeCurrency.Visibility = Visibility.Visible;
                }
            }
        }

        private void HideBrowsers() {
            webBrowser_PoETrade.Visibility = Visibility.Hidden;
            webBrowser_PoENinja_StandardCurrency.Visibility = Visibility.Hidden;
            webBrowser_PoENinja_ChallengeCurrency.Visibility = Visibility.Hidden;
            webBrowser_PoENinja_Builds.Visibility = Visibility.Hidden;
        }

        private async void hamMenShopThreads_ItemClick(object sender, ItemClickEventArgs args) {
            HamburgerMenuGlyphItem item = (HamburgerMenuGlyphItem)hamMenShopThreads.SelectedItem;
            int threadId = Convert.ToInt32(item.Glyph);
            Session.Instance().SetCurrentThreadId(threadId);
            await InitUI(Session.Instance().CurrentThreadId, Session.Instance().GetShop().League.Name);
        }
    }
}
