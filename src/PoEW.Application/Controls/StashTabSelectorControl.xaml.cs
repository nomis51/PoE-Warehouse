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
        int ActiveTabIndex = 0;

        public delegate void StashTabSelected(int index);
        public event StashTabSelected OnStashTabSelected;

        public StashTabSelectorControl() {
            InitializeComponent();
            LoadAssets();
        }

        private void LoadAssets() {
            bmpTab = Utils.ToBitmapImage(Properties.Resources.tab);
            bmpSelectedTab = Utils.ToBitmapImage(Properties.Resources.tab_selected);
        }

        public void SetActiveTab(int index) {
            ActiveTabIndex = index;
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

        private Image GenerateTabImage(int index, string name) {
            Image imgTab = new Image();
            imgTab.SetValue(Grid.ColumnProperty, Tabs.Count);
            imgTab.Source = bmpTab;
            imgTab.HorizontalAlignment = HorizontalAlignment.Stretch;
            imgTab.VerticalAlignment = VerticalAlignment.Stretch;
            imgTab.MouseDown += tab_MouseDown;
            imgTab.Name = $"img{index}{name}";

            return imgTab;
        }

        private Label GenerateTabLabel(int index, string name) {
            Label lblTab = new Label();
            lblTab.Foreground = System.Windows.Media.Brushes.DarkGoldenrod;
            lblTab.SetValue(Grid.ColumnProperty, Tabs.Count);
            lblTab.HorizontalAlignment = HorizontalAlignment.Center;
            lblTab.VerticalAlignment = VerticalAlignment.Center;
            lblTab.Content = name.Substring(0, name.Length > 6 ? 6 : name.Length) + (name.Length > 6 ? "..." : "");
            lblTab.MouseDown += tab_MouseDown;
            lblTab.Name = $"lbl{index}{name}";

            return lblTab;
        }

        public void AddTab(int index, string name) {
            this.Dispatcher.Invoke(() => {
                Tabs.Add(name, index);

                Image imgTab = GenerateTabImage(index, name);
                Images.Add(Tabs.Count, imgTab);
                grd.Children.Add(imgTab);

                Label lblTab = GenerateTabLabel(index, name);
                Labels.Add(Tabs.Count, lblTab);
                grd.Children.Add(lblTab);

                if (Tabs.Count > ActiveTabIndex) {
                    SetActiveTab(ActiveTabIndex);
                }
            });
        }

        private void HandleTabSelection(int gridColumn) {
            SetActiveTab(gridColumn - 1);
            OnStashTabSelected(gridColumn - 1);
        }

        private void tab_MouseDown(object sender, MouseButtonEventArgs e) {
            UIElement element = (UIElement)sender;
            int col = (int)element.GetValue(Grid.ColumnProperty);
            HandleTabSelection(col);
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
