using Kowalski.Common;
using Kowalski.Models;
using Kowalski.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Kowalski.Views
{
    public sealed partial class MainPage : Page
    {
        public ICommand ConnectCommand { get; }

        public ICommand DisconnectCommand { get; }

        public ICommand ShutdownCommand { get; }

        public ICommand PingCommand { get; }

        public MainPage()
        {
            this.InitializeComponent();
            Unloaded += MainPage_Unloaded;

            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);
            ShutdownCommand = new RelayCommand(Shutdown);
            PingCommand = new RelayCommand(Ping);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SoundPlayer.Instance.Play(Sounds.Startup);

            Assistant.OnStartRecognition += Assistant_OnStartRecognition;
            Assistant.OnCommandReceived += Assistant_OnCommandReceived;
            Assistant.OnResponseReceived += Assistant_OnResponseReceived;
            Assistant.StartService();

            base.OnNavigatedTo(e);
        }

        private void Assistant_OnStartRecognition(object sender, EventArgs e)
        {
            ResponseBubble.Visibility = Visibility.Collapsed;
            BotResponse.Text = string.Empty;
        }

        private void Assistant_OnCommandReceived(object sender, EventArgs e)
        {
            WaitingRing.Visibility = Visibility.Visible;
        }

        private async void Assistant_OnResponseReceived(object sender, BotEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                WaitingRing.Visibility = Visibility.Collapsed;
                BotResponse.Text = e.Text;
                ResponseBubble.Visibility = Visibility.Visible;
            });
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Assistant.StopService();

            Assistant.OnStartRecognition -= Assistant_OnStartRecognition;
            Assistant.OnResponseReceived -= Assistant_OnResponseReceived;
        }

        private void Connect()
        {
            Assistant.StartService();
        }

        private void Disconnect()
        {
            Assistant.StopService();
        }

        private void Shutdown()
        {
            try
            {
                // Shutdowns the device immediately.
                if (ApiInformation.IsTypePresent("Windows.System.ShutdownManager"))
                {
                    SoundPlayer.Instance.Play(Sounds.Shutdown);
                    ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(3));
                }
            }
            catch
            {
            }
        }

        private void Ping()
        {
            SoundPlayer.Instance.Play(Sounds.Startup);
        }
    }
}
