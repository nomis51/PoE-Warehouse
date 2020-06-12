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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PoEW.Application.Controls {
    /// <summary>
    /// Interaction logic for NomalStashTabGridControl.xaml
    /// </summary>
    public partial class NomalStashTabGridControl : UserControl {
        private SolidColorBrush TileBorderColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(79, 67, 49));

        public NomalStashTabGridControl() {
            InitializeComponent();
            SetupTiles();
        }

        public void Push(Item item, Price price = null) {
            ItemControl itemControl = new ItemControl(item, price);

            itemControl.SetValue(Grid.RowProperty, item.Position.Y);
            itemControl.SetValue(Grid.ColumnProperty, item.Position.X);

            itemControl.SetValue(Grid.RowSpanProperty, item.Height);
            itemControl.SetValue(Grid.ColumnSpanProperty, item.Width);

            itemControl.HorizontalAlignment = HorizontalAlignment.Stretch;
            itemControl.VerticalAlignment = VerticalAlignment.Stretch;

            grd.Children.Add(itemControl);
        }

        public void Clear() {
            for (int i = 0; i < grd.Children.Count; ++i) {
                if (grd.Children[i].GetType() == typeof(ItemControl)) {
                    grd.Children.Remove(grd.Children[i]);
                    --i;
                }
            }
        }

        private void SetupTiles() {
            for (int i = 0; i < 12; ++i) {
                for (int k = 0; k < 12; ++k) {
                    Border border = new Border();
                    border.BorderBrush = TileBorderColor;
                    border.BorderThickness = new Thickness(0.5d, 0.5d, 0.5d, 0.5d);
                    border.SetValue(Grid.RowProperty, i);
                    border.SetValue(Grid.ColumnProperty, k);
                    grd.Children.Add(border);
                }
            }
        }
    }
}
