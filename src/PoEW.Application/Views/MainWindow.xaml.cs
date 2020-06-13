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
        private Session _session;
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

            _session = new Session();

            stashTabSelectorControl.OnStashTabSelected += StashTabSelectorControl_OnStashTabSelected;

            PricingForm.OnPriceAdded += PricingForm_OnPriceAdded;
            PricingForm.OnPriceRemoved += PricingForm_OnPriceRemoved;

            Session.OnLocalStashTabsUpdated += Session_OnLocalStashTabsUpdated;

            var lastPlayer = _session.GetLastPlayer().Result;

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

            stashTabControl.SetStashTab(stashTabs.Values.ElementAt(_session.GetSelectedTabIndex()), _session.GetShop().GetPrices());
        }

        private void PricingForm_OnPriceRemoved(string itemId) {
            _session.GetShop().UnsetPrice(itemId);
        }

        private void PricingForm_OnPriceAdded(Price price, string itemId) {
            _session.GetShop().SetPrice(itemId, price);
        }

        private void StashTabSelectorControl_OnStashTabSelected(int index) {
            _session.SetSelectedTab(index);
            stashTabControl.SetStashTab(_session.GetShop().GetStashTab(index), _session.GetShop().GetPrices());
        }

        private async void ShopFormWin_Closed(object sender, EventArgs e) {
            if (ShopFormWin.Success) {
                if (!_session.AnyShops(ShopFormWin.ThreadId)) {
                    await _session.AddShop(ShopFormWin.ThreadId, ShopFormWin.League);
                    _session.SetCurrentThreadId(ShopFormWin.ThreadId);

                    HamburderMenuItems.Add(HamburgerMenuHeader);
                    HamburderMenuItems.Add(HamburgerMenuSeparator);
                    HamburgerMenuGlyphItem item = new HamburgerMenuGlyphItem();
                    item.Label = _session.GetShop().Title;
                    item.Glyph = ShopFormWin.ThreadId.ToString();
                    HamburderMenuItems.Add(item);
                    hamMenShopThreads.ItemsSource = HamburderMenuItems;

                    _ = InitUI(ShopFormWin.ThreadId, ShopFormWin.League);
                }
            } else if (!_session.AnyShops()) {
                ShopFormWin = new ShopForm();
                ShopFormWin.Closed += ShopFormWin_Closed;
                ShopFormWin.SetLeagues(_session.GetLeagues());
                ShopFormWin.ShowDialog();
            }
        }

        private async Task InitUI(int threadId, string league) {
            _session.IsLocalStashUpdaterPaused = true;

            txtbThreadId.Text = $"Thread {threadId}";
            txtbLeague.Text = league;

            await _session.LoadLocalStash();

            stashTabSelectorControl.ClearTabs();

            if (!_session.GetShop().GetStashTabs().Any()) {
                stashTabControl.ClearStashTab();
                await _session.UpdateLocalStash(threadId);
            }

            foreach (var tab in _session.GetShop().GetStashTabsName()) {
                stashTabSelectorControl.AddTab(tab.Value, tab.Key);
            }

            if (_session.GetShop().GetStashTabs().Any()) {
                _session.SetSelectedTab(0);
                stashTabControl.SetStashTab(_session.GetShop().GetStashTab(0), _session.GetShop().GetPrices());
                stashTabSelectorControl.SetActiveTab(0);
            }

            _session.IsLocalStashUpdaterPaused = false;

            loaderStash.IsActive = false;
        }

        private async void BtnChangeShopThread_Click(object sender, RoutedEventArgs e) {
            Button btn = (Button)sender;
            _session.SetCurrentThreadId(Convert.ToInt32(btn.Content.ToString().Substring(7)));
            await InitUI(_session.CurrentThreadId, _session.GetShop().League.Name);
        }

        private async void LoginWin_Closed(object sender, EventArgs e) {
            if (!LoginWin.Success) {
                System.Windows.Application.Current.Shutdown();
                return;
            }

            await _session.SetConnectedPlayer(new Player() {
                SessionId = LoginWin.POESESSID
            });

            btnAccount.Content = _session.Player.AccountName;

            _session.RunAutoOnlineUpdater();

            await _session.LoadShops();

            HamburderMenuItems.Add(HamburgerMenuHeader);
            HamburderMenuItems.Add(HamburgerMenuSeparator);

            foreach (var shop in _session.GetShops()) {
                HamburgerMenuGlyphItem item = new HamburgerMenuGlyphItem();
                item.Label = shop.Title;
                item.Glyph = shop.ThreadId.ToString();
                HamburderMenuItems.Add(item);
            }

            hamMenShopThreads.ItemsSource = HamburderMenuItems;
            hamMenShopThreads.SelectedIndex = 0;

            if (!_session.AnyShops()) {
                ShopFormWin = new ShopForm();
                ShopFormWin.Closed += ShopFormWin_Closed;
                ShopFormWin.SetLeagues(_session.GetLeagues());
                ShopFormWin.ShowDialog();
            } else {
                await InitUI(_session.CurrentThreadId, _session.GetShop().League.Name);
            }
        }

        public BitmapImage ToBitmapImage(System.Drawing.Bitmap bitmap) {
            using (var memory = new MemoryStream()) {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        private void btnAddShop_Click(object sender, RoutedEventArgs e) {
            ShopFormWin = new ShopForm();
            ShopFormWin.Closed += ShopFormWin_Closed;
            ShopFormWin.SetLeagues(_session.GetLeagues());
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

            if (_session.AnyShops()) {
                url = url.Replace("$league$", _session.GetShop().League.Name == "Standard" ? "standard" : "challenge");
            }

            if (webBrowser.Address != url) {
                webBrowser.Address = url;
            }
        }

        private async void hamMenShopThreads_ItemClick(object sender, ItemClickEventArgs args) {
            HamburgerMenuGlyphItem item = (HamburgerMenuGlyphItem)hamMenShopThreads.SelectedItem;
            int threadId = Convert.ToInt32(item.Glyph);
            _session.SetCurrentThreadId(threadId);
            await InitUI(_session.CurrentThreadId, _session.GetShop().League.Name);
        }
    }
}
