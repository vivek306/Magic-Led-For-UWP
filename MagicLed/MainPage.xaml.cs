using MagicLed.Helpers;
using MagicLedLibrary;
using MagicLedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MagicLed
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        LedLibrary ledLibrary;
        private ObservableCollection<FoundBulbModels> foundBulbs { get; set; }
 
        Socket socket;
        bool scanClicked = false;

        public MainPage()
        {
            this.InitializeComponent();
            foundBulbs = new ObservableCollection<FoundBulbModels>();
            FoundLedGridView.ItemsSource = foundBulbs;
            ledLibrary = new LedLibrary();

            ApplicationView.PreferredLaunchViewSize = new Size(480, 800);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        private void LoadFoundBulbs(List<string> foundList)
        {
            foreach (var item in foundList)
            {
                var splitItem = item.Trim().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (splitItem.Length == 3)
                {
                    FoundBulbModels model = new FoundBulbModels
                    {
                        IPAddress = splitItem[0],
                        Id = splitItem[1],
                        Model = splitItem[2].Trim('\0')
                    };
                    if (foundBulbs.Where(m => m.IPAddress == model.IPAddress).Count() == 0)
                        foundBulbs.Add(model);
                }
            }
        }

        private async Task Scan()
        {
            if (!scanClicked)
            {
                scanClicked = true;
                StatusText.Text = "Scanning...";
                await Task.Delay(1);
                // Send message
                var sendModels = ledLibrary.Send(CommonHelpers.DISCOVERY_PORT);
                StatusText.Text = sendModels.Status;
                socket = sendModels.Socket;
                // Receive message
                await Task.Delay(1);
                var progress = new Progress<string>(status =>
                {
                    StatusText.Text = status;
                });
                LoadFoundBulbs(await ledLibrary.Receive(progress, socket, CommonHelpers.DISCOVERY_PORT, CommonHelpers.TIMEOUT));
                StatusText.Text = "Ready";
                scanClicked = false;
            }
        }

        private async void ScanLamps_Click(object sender, RoutedEventArgs e)
        {
            await Scan();
        }

        private void FoundLedGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(DetailedView), e.ClickedItem);
        }

        private async void About_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog showDialog = new MessageDialog("Make sure the LEDs are connected to the WiFi using the official Magic Pro App");
            await showDialog.ShowAsync();
        }
    }
}
