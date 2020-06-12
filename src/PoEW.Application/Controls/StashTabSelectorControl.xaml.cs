using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Image = System.Windows.Controls.Image;

namespace PoEW.Application.Controls {
    /// <summary>
    /// Interaction logic for StashTabSelectorControl.xaml
    /// </summary>
    public partial class StashTabSelectorControl : UserControl {
        Dictionary<string, int> Tabs = new Dictionary<string, int>();
        Dictionary<int, Image> Images = new Dictionary<int, Image>();
        Dictionary<int, Label> Labels = new Dictionary<int, Label>();
        BitmapImage bmpTab;
        BitmapImage bmpSelectedTab;

        public delegate void StashTabSelected(int index);
        public event StashTabSelected OnStashTabSelected;

        public StashTabSelectorControl() {
            InitializeComponent();
            LoadAssets();
        }

        private void LoadAssets() {
            bmpTab = ToBitmapImage(Properties.Resources.tab);
            bmpSelectedTab = ToBitmapImage(Properties.Resources.tab_selected);
        }

        public BitmapImage ToBitmapImage(Bitmap bitmap) {
            using (var memory = new MemoryStream()) {
                bitmap.Save(memory, ImageFormat.Png);
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

        public void SetActiveTab(int index) {
            Images[index + 1].Source = bmpSelectedTab;
            UnselectOtherTabs(index + 1);
        }

        public void ClearTabs() {
            this.Dispatcher.Invoke(() => {
                foreach (var img in Images.Values) {
                    grd.Children.Remove(img);
                }

                foreach (var lbl in Labels.Values) {
                    grd.Children.Remove(lbl);
                }

                Tabs.Clear();
                Images.Clear();
                Labels.Clear();
            });
        }

        public void AddTab(int index, string name) {
            this.Dispatcher.Invoke(() => {
                Tabs.Add(name, index);

                Image imgTab = new Image();
                imgTab.SetValue(Grid.ColumnProperty, Tabs.Count);
                imgTab.Source = bmpTab;
                imgTab.HorizontalAlignment = HorizontalAlignment.Stretch;
                imgTab.VerticalAlignment = VerticalAlignment.Stretch;
                imgTab.MouseDown += tab_MouseDown;
                imgTab.Name = $"img{index}{name}";
                Images.Add(Tabs.Count, imgTab);
                grd.Children.Add(imgTab);

                Label lblTab = new Label();
                lblTab.Foreground = System.Windows.Media.Brushes.DarkGoldenrod;
                lblTab.SetValue(Grid.ColumnProperty, Tabs.Count);
                lblTab.HorizontalAlignment = HorizontalAlignment.Center;
                lblTab.VerticalAlignment = VerticalAlignment.Center;
                lblTab.Content = name.Substring(0, name.Length > 6 ? 6 : name.Length) + (name.Length > 6 ? "..." : "");
                lblTab.MouseDown += tab_MouseDown;
                lblTab.Name = $"lbl{index}{name}";
                Labels.Add(Tabs.Count, lblTab);
                grd.Children.Add(lblTab);
            });
        }

        private void tab_MouseDown(object sender, MouseButtonEventArgs e) {
            UIElement element = (UIElement)sender;
            int col = (int)element.GetValue(Grid.ColumnProperty);
            SetActiveTab(col - 1);
            OnStashTabSelected(col - 1);
        }

        private void UnselectOtherTabs(int selectedTab) {
            foreach (var key in Images.Keys) {
                if (key != selectedTab) {
                    Images[key].Source = bmpTab;
                }
            }
        }
    }
}
