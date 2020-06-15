using PoEW.Data;
using PoEW.Data.Models;
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
    /// Interaction logic for PricingForm.xaml
    /// </summary>
    public partial class PricingForm {
        private string Url_Alchemy = "https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyUpgradeToRare.png?w=1&h=1&scale=1&v=89c110be97333995522c7b2c29cae728";
        private string Url_Chaos = "https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyRerollRare.png?w=1&h=1&scale=1&v=c60aa876dd6bab31174df91b1da1b4f9";
        private string Url_Exalt = "https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyAddModToRare.png?w=1&h=1&scale=1&v=1745ebafbd533b6f91bccf588ab5efc5";

        public Price Price = null;
        public string ItemId { get; private set; }
        private bool IsPriceActive = true;

        public bool Success { get; private set; } = false;

        public PricingForm() {
            InitializeComponent();

            this.Loaded += PricingForm_Loaded;
        }

        public void Reset() {
            txtPrice.Text = "";
            cboCurrencies.SelectedValue = null;
            cboTypes.SelectedValue = Shop.PriceTypeToPrefix[Data.Enums.PriceType.PRICE_TYPE_BUYOUT];
        }

        private void PricingForm_Loaded(object sender, RoutedEventArgs e) {
            Init();
        }

        public void SetPrice(Price price) {
            Price = price;

            txtPrice.Text = Price.Value.ToString();
            cboCurrencies.SelectedValue = Shop.CurrencyTypeToString[Price.Currency];
            cboTypes.SelectedValue = Shop.PriceTypeToPrefix[Price.Type];
        }

        public void SetItemId(string itemId) {
            ItemId = itemId;

            Init();
        }

        private void Init() {
            cboCurrencies.ItemsSource = Shop.CurrencyTypeToString.Values.ToList().FindAll(c => !string.IsNullOrEmpty(c));
            cboTypes.ItemsSource = Shop.PriceTypeToPrefix.Values.ToList().FindAll(c => !string.IsNullOrEmpty(c));
            cboTypes.SelectedValue = Shop.PriceTypeToPrefix[Data.Enums.PriceType.PRICE_TYPE_BUYOUT];

            InitCurrencyImages();
        }

        private void InitCurrencyImages() {
            imgAlc.Source = new BitmapImage(new Uri(Url_Alchemy, UriKind.Absolute));
            imgChaos.Source = new BitmapImage(new Uri(Url_Chaos, UriKind.Absolute));
            imgExalt.Source = new BitmapImage(new Uri(Url_Exalt, UriKind.Absolute));
        }

        private void btnClear_Click(object sender, RoutedEventArgs e) {
            UnsetPrice();
        }

        private void UnsetPrice() {
            Session.Instance().GetShop().UnsetPrice(ItemId);
            Success = true;
            Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            SetPrice();
        }

        private void SetPrice() {
            int price = -1;

            if ((price = ValidatePrice()) != -1) {
                Price = new Price(Shop.StringToCurrencyType[cboCurrencies.SelectedValue.ToString()], Shop.PrefixToPriceType[cboTypes.SelectedValue.ToString()], price);
                Session.Instance().GetShop().SetPrice(ItemId, Price, IsPriceActive);

                Success = true;
                Close();
            }
        }

        private int ValidatePrice() {
            int price = -1;
            if (cboTypes.SelectedIndex != -1 && cboCurrencies.SelectedIndex != -1 && !string.IsNullOrEmpty(txtPrice.Text) && int.TryParse(txtPrice.Text, out price)) {
                return price;
            }

            return -1;
        }

        private void btnShowHidePrice_Click(object sender, RoutedEventArgs e) {
            IsPriceActive = !IsPriceActive;
            btnShowHidePrice.Content = IsPriceActive ? "Hide" : "Show";
        }

        private void imgAlc_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            cboCurrencies.SelectedIndex = cboCurrencies.Items.IndexOf(Shop.CurrencyTypeToString[Data.Enums.CurrencyType.CURRENCY_ORB_OF_ALCHEMY]);
        }

        private void imgChaos_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            cboCurrencies.SelectedIndex = cboCurrencies.Items.IndexOf(Shop.CurrencyTypeToString[Data.Enums.CurrencyType.CURRENCY_CHAOS_ORB]);
        }

        private void imgExalt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            cboCurrencies.SelectedIndex = cboCurrencies.Items.IndexOf(Shop.CurrencyTypeToString[Data.Enums.CurrencyType.CURRENCY_EXALTED_ORB]);
        }
    }
}
