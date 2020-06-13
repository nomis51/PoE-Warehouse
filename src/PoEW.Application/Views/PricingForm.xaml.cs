﻿using PoEW.Data;
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

        public delegate void PriceAdded(Price price, string itemId);
        public static event PriceAdded OnPriceAdded;

        public delegate void PriceRemoved(string itemId);
        public static event PriceRemoved OnPriceRemoved;

        private string ItemId;
        public Price Price;

        public bool Success { get; private set; } = false;

        public PricingForm(string itemId) {
            InitializeComponent();

            ItemId = itemId;

            Init();
        }

        private void Init() {
            cboCurrencies.ItemsSource = Shop.CurrencyTypeToString.Values.ToList().FindAll(c => !string.IsNullOrEmpty(c));
            cboTypes.ItemsSource = Shop.PriceTypeToPrefix.Values.ToList().FindAll(c => !string.IsNullOrEmpty(c));

            InitCurrencyImages();
        }

        private void InitCurrencyImages() {
            imgAlc.Source = new BitmapImage(new Uri(Url_Alchemy, UriKind.Absolute));
            imgChaos.Source = new BitmapImage(new Uri(Url_Chaos, UriKind.Absolute));
            imgExalt.Source = new BitmapImage(new Uri(Url_Exalt, UriKind.Absolute));
        }

        private void btnClear_Click(object sender, RoutedEventArgs e) {
            Price = null;
            OnPriceRemoved(ItemId);
            Success = true;
            Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            int price = -1;
            if (cboTypes.SelectedIndex != -1 && cboCurrencies.SelectedIndex != -1 && !string.IsNullOrEmpty(txtPrice.Text) && int.TryParse(txtPrice.Text, out price)) {
                Price = new Price(Shop.StringToCurrencyType[cboCurrencies.SelectedValue.ToString()], Shop.PrefixToPriceType[cboTypes.SelectedValue.ToString()], price);
                OnPriceAdded(Price, ItemId);

                Success = true;
                Close();
            }
        }
    }
}