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
    /// Interaction logic for ItemTooltip.xaml
    /// </summary>
    public partial class ItemTooltip : Window {
        int propertiesStartRow = 2;
        int maxPropertiesRows = 6;
        int nbProperties = 0;

        public ItemTooltip() {
            InitializeComponent();
        }

        public void SetName(string name) {
            lblName.Content = name;
        }

        public void SetType(string type) {
            lblType.Content = type;
        }

        public void AddProperty(string property) {
            if (maxPropertiesRows > nbProperties + 1) {
                Label lbl = new Label();
                lbl.SetValue(Grid.RowProperty, (nbProperties++) + propertiesStartRow);
                lbl.Content = property;
                lbl.HorizontalAlignment = HorizontalAlignment.Center;
                lbl.VerticalAlignment = VerticalAlignment.Center;
                grd.Children.Add(lbl);
            }
        }

        public void AddImplicitMod(string mod) {

        }

        public void AddExplicitMod(string mod) {

        }

        public void SetCorrupted(bool isCorrupted) {

        }

        public void SetUnidentified(bool isUnidentified) {

        }

        
    }
}
