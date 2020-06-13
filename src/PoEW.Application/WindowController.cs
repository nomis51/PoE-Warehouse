using PoEW.Application.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PoEW.Application {
    public class WindowController {
        #region Singleton
        private static WindowController _instance;
        public static WindowController Instance() {
            if (_instance == null) {
                _instance = new WindowController();
            }

            return _instance;
        }
        #endregion

        #region Windows

        LoginWindow _loginWin;
        public LoginWindow LoginWin {
            get {
                if (_loginWin == null) {
                    _loginWin = new LoginWindow();
                }

                return _loginWin;
            }
        }
        PricingForm _pricingFormWin;
        public PricingForm PricingFormWin {
            get {
                if (_pricingFormWin == null) {
                    _pricingFormWin = new PricingForm();
                    _pricingFormWin.Closing += _pricingFormWin_Closing;
                }

                return _pricingFormWin;
            }
        }

        private void _pricingFormWin_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            _pricingFormWin.Hide();
        }

        ShopForm _shopFormWin;
        public ShopForm ShopFormWin {
            get {
                if (_shopFormWin == null) {
                    _shopFormWin = new ShopForm();
                    _shopFormWin.Closing += _shopFormWin_Closing;
                }

                return _shopFormWin;
            }
        }

        private void _shopFormWin_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            _shopFormWin.Hide();
        }

        LoginWebBrowser _loginWebBrowser;
        public LoginWebBrowser LoginWebBrowserWin {
            get {
                if (_loginWebBrowser == null) {
                    _loginWebBrowser = new LoginWebBrowser();
                }

                return _loginWebBrowser;
            }
        }
        #endregion

        private WindowController() {
        }
    }
}
