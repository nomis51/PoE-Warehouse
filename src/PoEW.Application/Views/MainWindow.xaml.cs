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

namespace PoEW.Application {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow {
        private Session _session;
        private LoginWindow LoginWin = new LoginWindow();
        private ShopForm ShopFormWin = new ShopForm();

        private Dictionary<int, Button> ShopThreadButtons = new Dictionary<int, Button>();

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

            LoginWin.Closed += LoginWin_Closed;
            LoginWin.ShowDialog();
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

                    //Button btn = GenerateShopThreadButton($"Thread {ShopFormWin.ThreadId}", true);
                    //ShopThreadButtons.Add(_session.CurrentThreadId, btn);
                    //dockShopThreadTabs.Children.Add(btn);
                    ListViewItem item = new ListViewItem();
                    item.Content = $"{ShopFormWin.ThreadId} { _session.GetShop().Title}";
                    lstvShopThreads.Items.Add(item);

                    // SetShopThreadButtonActive(_session.CurrentThreadId);

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
        }

        private Button GenerateShopThreadButton(string content, bool active = false) {
            Button btn = new Button();
            btn.HorizontalAlignment = HorizontalAlignment.Left;
            btn.VerticalAlignment = VerticalAlignment.Stretch;
            btn.Click += BtnChangeShopThread_Click;
            btn.Content = content;
            return btn;
        }

        private async void BtnChangeShopThread_Click(object sender, RoutedEventArgs e) {
            Button btn = (Button)sender;
            _session.SetCurrentThreadId(Convert.ToInt32(btn.Content.ToString().Substring(7)));
            await InitUI(_session.CurrentThreadId, _session.GetShop().League.Name);

            // SetShopThreadButtonActive(_session.CurrentThreadId);
        }

        //private void SetShopThreadButtonActive(int threadId) {
        //    foreach (var b in ShopThreadButtons) {
        //        int index = dockShopThreadTabs.Children.IndexOf(b.Value);

        //        if (index != -1) {
        //            ((Button)dockShopThreadTabs.Children[index]).Background = b.Key == threadId ? Brushes.Gray : Brushes.DarkGray;
        //        }
        //    }
        //}

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

            foreach (var shop in _session.GetShops()) {
                //Button btn = GenerateShopThreadButton($"Thread {shop.ThreadId}");
                //ShopThreadButtons.Add(shop.ThreadId, btn);
                //dockShopThreadTabs.Children.Add(btn);

                ListViewItem item = new ListViewItem();
                item.Content = $"{shop.ThreadId} { shop.Title}";
                lstvShopThreads.Items.Add(item);
            }

            if (!_session.AnyShops()) {
                ShopFormWin = new ShopForm();
                ShopFormWin.Closed += ShopFormWin_Closed;
                ShopFormWin.SetLeagues(_session.GetLeagues());
                ShopFormWin.ShowDialog();
            } else {
                // SetShopThreadButtonActive(_session.CurrentThreadId);

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

        private async void lstvShopThreads_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            string strThreadId = ((ListViewItem)lstvShopThreads.SelectedItem).Content.ToString();
            strThreadId = strThreadId.Substring(0, strThreadId.IndexOf(" "));
            _session.SetCurrentThreadId(Convert.ToInt32(strThreadId));
            await InitUI(_session.CurrentThreadId, _session.GetShop().League.Name);
        }
    }
}
