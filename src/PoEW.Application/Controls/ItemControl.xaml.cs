﻿using PoEW.API.Enums;
using PoEW.Application.Views;
using PoEW.Data;
using PoEW.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PoEW.Application.Controls {
    /// <summary>
    /// Interaction logic for ItemControl.xaml
    /// </summary>
    public partial class ItemControl : UserControl {
        Item Item;
        Price Price;

        SolidColorBrush DarkBlue = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0e0f2f"));
        SolidColorBrush DarkRed = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2a0403"));

        List<Image> SocketsImage = new List<Image>();

        public ItemControl(Item item, Price price = null) {
            InitializeComponent();
            Item = item;
            Price = price;
            Init();
        }

        private void Init() {
            InitTooltip();
            SetImage(Item.IconUrl);
            SetStackSize();
            SetSockets();
            SetItemBackground();
            SetupEvents();
        }

        private void SetupEvents() {
            WindowController.Instance().PricingFormWin.IsVisibleChanged += PricingFormWin_IsVisibleChanged;
        }

        private void PricingFormWin_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (WindowController.Instance().PricingFormWin.Visibility == Visibility.Hidden) {
                PricingForm_Closed();
            }
        }

        private void SetItemBackground() {
            rectBackground.Fill = Item.Identified ? DarkBlue : DarkRed;
        }

        private Brush GetFrameColor() {
            switch (Item.FrameType) {
                case 1:
                    return Brushes.Blue;

                case 2:
                    return Brushes.Gold;

                default:
                    return Brushes.LightGray;
            }
        }

        private void SetStackSize() {
            if (Item.Size > 0) {
                txtbStackSize.Visibility = Visibility.Visible;
                txtbStackSize.Text = Item.Size.ToString();
            }
        }

        private void InitTooltip() {
            itemTooltipControl.AddName(Item.Name);
            itemTooltipControl.AddType(Item.Type);

            foreach (var property in Item.Properties) {
                itemTooltipControl.AddProperty(property.ToString());
            }

            foreach (var implicitMod in Item.ImplicitMods) {
                itemTooltipControl.AddImplicitMod(implicitMod);
            }

            foreach (var explicitMod in Item.ExplicitMods) {
                itemTooltipControl.AddExplicitMod(explicitMod);
            }

            itemTooltipControl.SetFrameColor(GetFrameColor());
            itemTooltipControl.SetCorrupted(Item.Corrupted);
            itemTooltipControl.SetUnidentified(!Item.Identified);

            if (Price != null) {
                itemTooltipControl.AddPriceNote(Price.ToString());
                txtbPriceAmount.Text = $"{Price.Value}x";

                if (Price.Value > 999 && Item.Width < 2) {
                    txtbPriceAmount.FontSize = 8;
                }

                imgPriceCurrency.Source = new BitmapImage(new Uri(Shop.CurrencyTypeToImageUrl[Price.Currency]));
            }
        }

        private ItemSocketLayout GetSocketLayout() {
            if (Item.Width == 2 && Item.Height == 3) {
                return ItemSocketLayout.Six;
            } else if (Item.Width == 2 && Item.Height == 2) {
                return ItemSocketLayout.Three_Or_Four_Corner;
            } else if (Item.Width == 1 && Item.Height == 3) {
                return ItemSocketLayout.Three;
            } else if (Item.Width == 1 && Item.Height == 4) {
                return ItemSocketLayout.Four;
            } else {
                return ItemSocketLayout.One;
            }
        }

        private void SetSockets() {
            ItemSocketLayout layout = GetSocketLayout();
            int width = 0;
            int height = 0;

            foreach (var socket in Item.Sockets) {
                Image img = new Image();

                img.HorizontalAlignment = HorizontalAlignment.Center;
                img.VerticalAlignment = VerticalAlignment.Center;
                img.Width = 25;
                img.Height = 25;
                // Avoid tooltip from not displaying, because the actual item image isn't mouse hover
                // same thing for the pricing form
                img.MouseEnter += imgIconUrl_MouseEnter;
                img.MouseLeave += imgIconUrl_MouseLeave;
                img.MouseRightButtonUp += imgIconUrl_MouseRightButtonUp;


                switch (socket.Colour) {
                    case "R":
                        img.Source = Utils.ToBitmapImage(Properties.Resources.red_socket);
                        break;

                    case "G":
                        img.Source = Utils.ToBitmapImage(Properties.Resources.green_socket);
                        break;

                    case "B":
                        img.Source = Utils.ToBitmapImage(Properties.Resources.blue_socket);
                        break;
                }

                switch (layout) {
                    case ItemSocketLayout.One:
                        if (width == 0 && height == 0) {
                            img.SetValue(Grid.RowProperty, height++ * 12);
                            img.SetValue(Grid.RowSpanProperty, 12);
                            img.SetValue(Grid.ColumnProperty, width++ * 12);
                            img.SetValue(Grid.ColumnSpanProperty, 12);
                        }
                        break;

                    case ItemSocketLayout.Three:
                        if (width == 0 && height <= 2) {
                            img.SetValue(Grid.RowProperty, height++ * 4);
                            img.SetValue(Grid.RowSpanProperty, 4);
                            img.SetValue(Grid.ColumnProperty, width);
                            img.SetValue(Grid.ColumnSpanProperty, 12);
                        }
                        break;

                    case ItemSocketLayout.Four:
                        if (width == 0 && height <= 3) {
                            img.SetValue(Grid.RowProperty, height++ * 3);
                            img.SetValue(Grid.RowSpanProperty, 3);
                            img.SetValue(Grid.ColumnProperty, width);
                            img.SetValue(Grid.ColumnSpanProperty, 12);
                        }
                        break;

                    case ItemSocketLayout.Three_Or_Four_Corner:
                        if (width <= 1 && height <= 1) {
                            img.SetValue(Grid.RowProperty, height * 6);
                            img.SetValue(Grid.RowSpanProperty, 6);
                            img.SetValue(Grid.ColumnProperty, width++ * 6);
                            img.SetValue(Grid.ColumnSpanProperty, 6);

                            if (width == 2) {
                                ++height;
                                width = 0;
                            }
                        }
                        break;

                    case ItemSocketLayout.Six:
                        if (width <= 1 && height <= 2) {
                            img.SetValue(Grid.RowProperty, height * 4);
                            img.SetValue(Grid.RowSpanProperty, 4);
                            img.SetValue(Grid.ColumnProperty, width++ * 6);
                            img.SetValue(Grid.ColumnSpanProperty, 6);

                            if (width == 2) {
                                ++height;
                                width = 0;
                            }
                        }
                        break;
                }

                img.Visibility = Visibility.Hidden;

                SocketsImage.Add(img);
                grd.Children.Add(img);
            }
        }

        private void SetImage(string url) {
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(url, UriKind.Absolute);
            bmp.EndInit();

            imgIconUrl.Source = bmp;
        }

        private void ShowSockets(bool visible = true) {
            foreach (var socket in SocketsImage) {
                socket.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
            }
        }


        private void imgIconUrl_MouseEnter(object sender, MouseEventArgs e) {
            ShowSockets();
            popItemTooltip.IsOpen = true;

        }

        private void imgIconUrl_MouseLeave(object sender, MouseEventArgs e) {
            ShowSockets(false);
            popItemTooltip.IsOpen = false;
        }

        private void imgIconUrl_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var price = Session.Instance().GetShop().GetPrice(Item.Id);

            if (price != null) {
                WindowController.Instance().PricingFormWin.SetPrice(price);
            } else {
                WindowController.Instance().PricingFormWin.Reset();
            }

            WindowController.Instance().PricingFormWin.SetItemId(Item.Id);
            WindowController.Instance().PricingFormWin.ShowDialog();
        }

        private void PricingForm_Closed() {
            if (WindowController.Instance().PricingFormWin.ItemId == Item.Id) {
                EditPriceNote();
            }
        }

        private void EditPriceNote() {
            if (WindowController.Instance().PricingFormWin.Price != null) {
                itemTooltipControl.AddPriceNote(WindowController.Instance().PricingFormWin.Price.ToString());
                txtbPriceAmount.Text = $"{ WindowController.Instance().PricingFormWin.Price.Value}x";

                if (WindowController.Instance().PricingFormWin.Price.Value > 999 && Item.Width < 2) {
                    txtbPriceAmount.FontSize = 8;
                }

                imgPriceCurrency.Source = new BitmapImage(new Uri(Shop.CurrencyTypeToImageUrl[WindowController.Instance().PricingFormWin.Price.Currency]));
            } else {
                itemTooltipControl.RemovePriceNote();
            }
        }
    }
}
