using PoEW.Data;
using PoEW.Data.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for StashTabControl.xaml
    /// </summary>
    public partial class StashTabControl : UserControl {
        StashTab StashTab;

        public StashTabControl() {
            InitializeComponent();
        }

        public void SetStashTab(StashTab stashTab, Dictionary<string, Price> prices) {
            this.Dispatcher.Invoke(() => {
                StashTab = stashTab;

                ClearStashTab();

                PushToGrid(prices);
            });
        }

        public void ClearStashTab() {
            normalStashGrid.Clear();
        }

        private void PushToGrid(Dictionary<string, Price> prices) {
            foreach (var item in StashTab.Items) {
                normalStashGrid.Push(item, prices.ContainsKey(item.Id) ? prices[item.Id] : null);
            }
        }
    }
}
