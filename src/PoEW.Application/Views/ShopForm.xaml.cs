using PoEW.API.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for ShopForm.xaml
    /// </summary>
    public partial class ShopForm {
        private ObservableCollection<string> leagues = new ObservableCollection<string>();
        public int ThreadId;
        public string League;
        public bool GenerateNewThread = true;
        private Regex regThreadUrl = new Regex("https://www.pathofexile.com/forum/view-thread/[0-9]+");

        public bool Success { get; private set; } = false;

        public ShopForm() {
            InitializeComponent();

            cboLeagues.ItemsSource = leagues;
        }

        public void SetLeagues(List<League> leagues) {
            foreach (var league in leagues) {
                this.leagues.Add(league.Name);
            }
        }

        private bool ParseThreadId() {
            if (GenerateNewThread) {
                return true;
            }

            if (!string.IsNullOrEmpty(txtThreadId.Text)) {
                int threadId = -1;

                if (regThreadUrl.IsMatch(txtThreadId.Text)) {
                    string strThreadId = txtThreadId.Text.Substring(txtThreadId.Text.IndexOf("view-thread/") + 12);

                    if (!int.TryParse(strThreadId, out threadId)) {
                        return false;
                    }

                    ThreadId = threadId;

                    return true;
                } else if (int.TryParse(txtThreadId.Text, out threadId)) {
                    ThreadId = threadId;
                    return true;
                }
            }

            return false;
        }

        private bool ParseLeague() {
            if (cboLeagues.SelectedValue != null) {
                League = cboLeagues.SelectedValue.ToString();
                return true;
            }

            return false;
        }

        private void btnSetShop_Click(object sender, RoutedEventArgs e) {
            if (ParseThreadId() && ParseLeague()) {
                Success = true;
                Close();
            }
        }

        private void chkAutoGenThread_Checked(object sender, RoutedEventArgs e) {
            bool state = chkAutoGenThread.IsChecked == true;
            DisableThreadIdField(state);
            GenerateNewThread = state;
        }

        private void DisableThreadIdField(bool disable = true) {
            if (txtThreadId != null) {
                txtThreadId.IsReadOnly = disable;
            }
        }

        private void chkAutoGenThread_Unchecked(object sender, RoutedEventArgs e) {

        }
    }
}
