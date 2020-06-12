using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace PoEW.Application.Controls {
    /// <summary>
    /// Interaction logic for ItemTooltipControl.xaml
    /// </summary>
    public partial class ItemTooltipControl : UserControl {
        int propertiesStartRow = 2;
        int maxPropertiesRows = 6;
        int nbProperties = 0;

        int implicitModsStartRow = 8;
        int maxImplicitModsRows = 5;
        int nbImplicitMods = 0;

        int explicitModsStartRow = 13;
        int maxExplicitModsRows = 6;
        int nbExplicitMods = 0;

        Label lblPrice;

        public ItemTooltipControl() {
            InitializeComponent();
        }

        public void AddName(string name) {
            if (!string.IsNullOrEmpty(name)) {
                Label lbl = GenerateLine(name, 12.0d, 0, null);
                grd.Children.Add(lbl);
            }
        }

        public void AddType(string type) {
            if (!string.IsNullOrEmpty(type)) {
                Label lbl = GenerateLine(type, 12.0d, 1, null);
                grd.Children.Add(lbl);
            }
        }

        public void AddProperty(string property) {
            if (maxPropertiesRows > nbProperties + 1) {
                Label lbl = GenerateLine(property, 9.0d, (nbProperties++) + propertiesStartRow, Brushes.LightGray);
                grd.Children.Add(lbl);
            }
        }

        public void AddImplicitMod(string mod) {
            if (maxImplicitModsRows > nbImplicitMods + 1) {
                Label lbl = GenerateLine(mod, 9.0d, (nbImplicitMods++) + implicitModsStartRow, null);
                grd.Children.Add(lbl);
            }
        }

        public void AddExplicitMod(string mod) {
            if (maxExplicitModsRows > nbExplicitMods + 1) {
                Label lbl = GenerateLine(mod, 9.0d, (nbExplicitMods++) + explicitModsStartRow, null);
                grd.Children.Add(lbl);
            }
        }

        public void SetCorrupted(bool isCorrupted) {
            if (isCorrupted) {
                Label lbl = GenerateLine("Corrupted", 9.0d, 19, Brushes.Red);
                grd.Children.Add(lbl);
            }
        }

        public void SetUnidentified(bool isUnidentified) {
            if (isUnidentified) {
                Label lbl = GenerateLine("Unidentified", 9.0d, 19, Brushes.Red);
                grd.Children.Add(lbl);
            }
        }

        public void RemovePriceNote() {
            if (lblPrice != null) {
                grd.Children.Remove(lblPrice);
            }
        }

        public void AddPriceNote(string priceNote) {
            if (lblPrice != null) {
                grd.Children.Remove(lblPrice);
            }

            lblPrice = GenerateLine($"Note: {priceNote}", 9.0d, 21, Brushes.Orange);
            grd.Children.Add(lblPrice);

        }

        public void SetFrameColor(Brush color) {
            brdFrame.BorderBrush = color;
            brdLine1.BorderBrush = color;
            brdLine2.BorderBrush = color;
            brdLine3.BorderBrush = color;
            brdLine4.BorderBrush = color;
        }

        private Label GenerateLine(string value, double fontSize, int row, Brush color) {
            Label lbl = new Label();
            lbl.SetValue(Grid.RowProperty, row);
            lbl.Content = value;
            lbl.FontSize = fontSize;
            lbl.Foreground = color == null ? Brushes.White : color;
            lbl.HorizontalAlignment = HorizontalAlignment.Center;
            lbl.VerticalAlignment = VerticalAlignment.Center;

            return lbl;
        }
    }
}
