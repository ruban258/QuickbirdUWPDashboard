﻿namespace Agronomist.ViewModels
{
    using System;
    using System.Diagnostics;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using NetLib;
    using Util;
    using Views;

    public class LandingPageViewModel : ViewModelBase
    {
        private bool _loginEnabled;
        
        public bool LoginEnabled
        {
            get { return _loginEnabled; }
            set
            {
                if (value == _loginEnabled) return;
                _loginEnabled = value;
                OnPropertyChanged();
            }
        }

        private void UpdateCredsAndTokens()
        {
            var settings = new Settings();
            Debug.WriteLine(settings.CredToken ?? "No saved auth.");
        }

        public async void Login()
        {
            LoginEnabled = false;
            const string entryUrl = "https://ghapi46azure.azurewebsites.net/.auth/login/twitter";
            const string resultUrl = "https://ghapi46azure.azurewebsites.net/.auth/login/done";

            Creds creds;
            try
            {
                creds = await Creds.FromBroker(entryUrl, resultUrl);
            }
            catch (Exception)
            {
                Debug.WriteLine("Login failed or cancelled.");
                LoginEnabled = true;
                return;
            }

            var settings = new Settings();
            settings.SetNewCreds(creds.Token, creds.Userid, Guid.Parse(creds.StableSid.Remove(0, 4)));

            UpdateCredsAndTokens();

            ((Frame)Window.Current.Content).Navigate(typeof(Shell));
        }

    }
}