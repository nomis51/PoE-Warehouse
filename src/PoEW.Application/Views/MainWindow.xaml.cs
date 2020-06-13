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

namespace PoEW.Application {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow {
        private LoginWindow LoginWin = new LoginWindow();
        private ShopForm ShopFormWin = new ShopForm();

        private Dictionary<int, Button> ShopThreadButtons = new Dictionary<int, Button>();

        private string Url_PoETrade = "https://www.pathofexile.com/trade";
        private string Url_PoENinja_Currency_Template = "https://poe.ninja/$league$/currency";
        private string Url_PoENinja_Builds = "https://poe.ninja/challenge/builds";
        private Regex regPoENinjaUrl = new Regex("https://poe.ninja/[a-z]+/(builds|currency)");
        private string LastUrlLoaded = "";

        ObservableCollection<HamburgerMenuItemBase> HamburderMenuItems = new ObservableCollection<HamburgerMenuItemBase>();
        HamburgerMenuHeaderItem HamburgerMenuHeader;
        HamburgerMenuSeparatorItem HamburgerMenuSeparator;

        public MainWindow() {
            InitializeComponent();

            stashTabSelectorControl.OnStashTabSelected += StashTabSelectorControl_OnStashTabSelected;

            Session.OnLocalStashTabsUpdated += Session_OnLocalStashTabsUpdated;

            var lastPlayer = Session.Instance().GetLastPlayer().Result;

            if (lastPlayer != null) {
                LoginWin.SetPlayer(lastPlayer);
            }

            webBrowser.FrameLoadEnd += WebBrowser_FrameLoadEnd;
            webBrowser.FrameLoadStart += WebBrowser_FrameLoadStart;

            InitHamburgerMenuItems();

            LoginWin.Closed += LoginWin_Closed;
            LoginWin.ShowDialog();
        }

        private void InitHamburgerMenuItems() {
            HamburgerMenuHeader = new HamburgerMenuHeaderItem();
            HamburgerMenuHeader.Label = "Shops";
            HamburgerMenuSeparator = new HamburgerMenuSeparatorItem();
        }

        private void WebBrowser_FrameLoadStart(object sender, CefSharp.FrameLoadStartEventArgs e) {
            this.Dispatcher.Invoke(() => {
                if (LastUrlLoaded != webBrowser.Address) {
                    loaderWebBrowser.IsActive = true;
                    webBrowser.Visibility = Visibility.Hidden;
                }
            });
        }

        private void WebBrowser_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e) {
            this.Dispatcher.Invoke(() => {
                if (LastUrlLoaded != webBrowser.Address) {
                    LastUrlLoaded = webBrowser.Address;

                    if (regPoENinjaUrl.IsMatch(webBrowser.Address)) {
                        webBrowser.ZoomLevel = 5.46149645 * Math.Log(60) - 25.12;
                    } else {
                        webBrowser.ZoomLevel = 0;
                    }

                    loaderWebBrowser.IsActive = false;
                    webBrowser.Visibility = Visibility.Visible;
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

        private async void ShopFormWin_Closed(object sender, EventArgs e) {
            if (ShopFormWin.Success) {
                if (!Session.Instance().AnyShops(ShopFormWin.ThreadId)) {
                    await Session.Instance().AddShop(ShopFormWin.ThreadId, ShopFormWin.League);
                    Session.Instance().SetCurrentThreadId(ShopFormWin.ThreadId);

                    HamburderMenuItems.Add(HamburgerMenuHeader);
                    HamburderMenuItems.Add(HamburgerMenuSeparator);
                    HamburgerMenuGlyphItem item = new HamburgerMenuGlyphItem();
                    item.Label = Session.Instance().GetShop().Title;
                    item.Glyph = ShopFormWin.ThreadId.ToString();
                    HamburderMenuItems.Add(item);
                    hamMenShopThreads.ItemsSource = HamburderMenuItems;

                    _ = InitUI(ShopFormWin.ThreadId, ShopFormWin.League);
                }
            } else if (!Session.Instance().AnyShops()) {
                ShopFormWin = new ShopForm();
                ShopFormWin.Closed += ShopFormWin_Closed;
                ShopFormWin.SetLeagues(Session.Instance().GetLeagues());
                ShopFormWin.ShowDialog();
            }
        }

        private async Task InitUI(int threadId, string league) {
            Session.Instance().IsLocalStashUpdaterPaused = true;

            txtbThreadId.Text = $"Thread {threadId}";
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

        private async void BtnChangeShopThread_Click(object sender, RoutedEventArgs e) {
            Button btn = (Button)sender;
            Session.Instance().SetCurrentThreadId(Convert.ToInt32(btn.Content.ToString().Substring(7)));
            await InitUI(Session.Instance().CurrentThreadId, Session.Instance().GetShop().League.Name);
        }

        private async void LoginWin_Closed(object sender, EventArgs e) {
            if (!LoginWin.Success) {
                System.Windows.Application.Current.Shutdown();
                return;
            }

            await Session.Instance().SetConnectedPlayer(new Player() {
                SessionId = LoginWin.POESESSID
            });

            btnAccount.Content = Session.Instance().Player.AccountName;

            Session.Instance().RunAutoOnlineUpdater();

            await Session.Instance().LoadShops();

            HamburderMenuItems.Add(HamburgerMenuHeader);
            HamburderMenuItems.Add(HamburgerMenuSeparator);

            foreach (var shop in Session.Instance().GetShops()) {
                HamburgerMenuGlyphItem item = new HamburgerMenuGlyphItem();
                item.Label = shop.Title;
                item.Glyph = shop.ThreadId.ToString();
                HamburderMenuItems.Add(item);
            }

            hamMenShopThreads.ItemsSource = HamburderMenuItems;
            hamMenShopThreads.SelectedIndex = 0;

            if (!Session.Instance().AnyShops()) {
                ShopFormWin = new ShopForm();
                ShopFormWin.Closed += ShopFormWin_Closed;
                ShopFormWin.SetLeagues(Session.Instance().GetLeagues());
                ShopFormWin.ShowDialog();
            } else {
                await InitUI(Session.Instance().CurrentThreadId, Session.Instance().GetShop().League.Name);
            }
        }

        private void btnAddShop_Click(object sender, RoutedEventArgs e) {
            ShopFormWin = new ShopForm();
            ShopFormWin.Closed += ShopFormWin_Closed;
            ShopFormWin.SetLeagues(Session.Instance().GetLeagues());
            ShopFormWin.ShowDialog();
        }

        private void toggleResourceMenu_Click(object sender, RoutedEventArgs e) {
            popResourceMenu.IsOpen = !popResourceMenu.IsOpen;
        }

        private void menItQuit_Click(object sender, RoutedEventArgs e) {
            System.Windows.Application.Current.Shutdown();
        }

        private void menItPoETrade_Click(object sender, RoutedEventArgs e) {
            popResourceMenu.IsOpen = false;

            if (webBrowser.Address != Url_PoETrade) {
                webBrowser.Address = Url_PoETrade;
            }
        }

        private void menuItPoENinjaBuilds_Click(object sender, RoutedEventArgs e) {
            popResourceMenu.IsOpen = false;

            if (webBrowser.Address != Url_PoENinja_Builds) {
                webBrowser.Address = Url_PoENinja_Builds;
            }
        }

        private void menuItPoENinjaCurrency_Click(object sender, RoutedEventArgs e) {
            popResourceMenu.IsOpen = false;

            string url = Url_PoENinja_Currency_Template;

            if (Session.Instance().AnyShops()) {
                url = url.Replace("$league$", Session.Instance().GetShop().League.Name == "Standard" ? "standard" : "challenge");
            }

            if (webBrowser.Address != url) {
                webBrowser.Address = url;
            }
        }

        private async void hamMenShopThreads_ItemClick(object sender, ItemClickEventArgs args) {
            HamburgerMenuGlyphItem item = (HamburgerMenuGlyphItem)hamMenShopThreads.SelectedItem;
            int threadId = Convert.ToInt32(item.Glyph);
            Session.Instance().SetCurrentThreadId(threadId);
            await InitUI(Session.Instance().CurrentThreadId, Session.Instance().GetShop().League.Name);
        }
    }
}
