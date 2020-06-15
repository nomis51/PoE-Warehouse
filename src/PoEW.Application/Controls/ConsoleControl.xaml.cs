using PoEW.API.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
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
    /// Interaction logic for ConsoleControl.xaml
    /// </summary>
    public partial class ConsoleControl : UserControl, IConsole {
        ObservableCollection<ListViewItem> Lines = new ObservableCollection<ListViewItem>();

        int nbErrors = 0;

        public delegate void ErrorLogFound(int nbErrors);
        public event ErrorLogFound OnErrorLogFound;

        public ConsoleControl() {
            InitializeComponent();
        }

        private ListViewItem GenerateLine(string content) {
            ListViewItem item = new ListViewItem();
            item.Padding = new Thickness(0);
            item.FontFamily = new FontFamily("Consolas");
            item.Background = Brushes.Black;
            item.HorizontalAlignment = HorizontalAlignment.Left;
            item.VerticalAlignment = VerticalAlignment.Center;
            item.Content = content;

            if (content.IndexOf("[Debug]") != -1) {
                item.Foreground = Brushes.Orange;
            }

            if (content.IndexOf("[Error]") != -1) {
                ++nbErrors;
                item.Foreground = Brushes.Red;
                OnErrorLogFound(nbErrors);
            }

            return item;
        }

        public void PushLine(string text) {
            this.Dispatcher.Invoke(() => {
                var item = GenerateLine(text);
                Lines.Add(item);
                lstv.ItemsSource = Lines;
                ScrollToTheEnd();
            });
        }

        public void Clear() {
            this.Dispatcher.Invoke(() => {
                Lines.Clear();
                lstv.ItemsSource = Lines;
            });
        }

        private void ScrollToTheEnd() {
            lstv.SelectedIndex = lstv.Items.Count > 0 ? lstv.Items.Count - 1 : 0;
            lstv.ScrollIntoView(lstv.SelectedItem);
        }
    }
}
