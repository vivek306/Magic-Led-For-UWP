using MagicLed.Helpers;
using MagicLedLibrary;
using MagicLedLibrary.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MagicLed
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DetailedView : Page
    {
        FoundBulbModels foundBulb;
        Socket socket;
        LedLibrary ledLibrary;

        public DetailedView()
        {
            this.InitializeComponent();
            ledLibrary = new LedLibrary();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            foundBulb = e.Parameter as FoundBulbModels;
            IPAddressText.Text = Dns.GetHostEntry(foundBulb.IPAddress).HostName;
            var connectModels = ledLibrary.Connect(foundBulb.IPAddress, CommonHelpers.CONNECT_PORT);

            socket = connectModels.Socket;
            StatusText.Text = connectModels.Status;

            RefreshState();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            socket.Dispose();
        }

        private void RefreshState()
        {
            if (socket != null)
            {
                myColorPicker.Visibility = Visibility.Visible;
                var refereshModels = ledLibrary.Refresh(socket, CommonHelpers.MAX_BUFFER_SIZE);
                isBulbOn.IsOn = refereshModels.PowerState;
                myColorPicker.Color = refereshModels.CurrentColor;
            }
            else myColorPicker.Visibility = Visibility.Collapsed;
        }


        private void isBulbOn_Toggled(object sender, RoutedEventArgs e)
        {
            List<byte> msg = new List<byte>();

            if (isBulbOn.IsOn)
                msg = new List<byte> { 0x71, 0x23, 0x0f };
            else msg = new List<byte> { 0x71, 0x24, 0x0f };

            ledLibrary.Write(socket, msg);
        }

        private void myColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            ledLibrary.SetRGB(args.NewColor.R, args.NewColor.G, args.NewColor.B, socket);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshState();
        }
    }

}
