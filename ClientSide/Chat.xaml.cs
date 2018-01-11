using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ClientSide
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Chat : Page
    {
        private string _userName;
        private string _secondUser;
        private IDisposable receiveMessageHandler { get; set; }
        private IDisposable receiveRecentMessageHandler { get; set; }
        private List<string> _messages = new List<string>();

        public Chat()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            dynamic info = e.Parameter;
            _userName = info.userName;
            _secondUser = info.secondUser;
            _messages = info.recentMessages;

            for (int i = 0; i < _messages.Count; i++)
            {
                chatHistory.Items.Add(_messages[i]);
            }

            App myApp = (Application.Current as App);
            receiveMessageHandler = myApp.MyHubProxy.On<string, string, DateTime>("receivemessage", ReceiveMessage);

            userBlock.Text = _userName;
            secondUserBlock.Text = _secondUser;

            if (myApp.MyHubConnection.State != ConnectionState.Connected)
            {
                return;
            }
            myApp.MyHubProxy.Invoke("GetRecentMessages", _secondUser, _userName);

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            receiveMessageHandler.Dispose();
            base.OnNavigatedFrom(e);
        }

        private async void RecentMessage(List<string> messages)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                chatHistory.Items.Add(messages);
            });
        }

        private async void ReceiveMessage(string userName, string message, DateTime sendTime)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                chatHistory.Items.Add($"{sendTime.ToString("MM-dd HH:mm:ss")}\n{userName}: {message}");
                if (App.MessagesReceivedLists.ContainsKey(_secondUser))
                {
                    App.MessagesReceivedLists[_secondUser].Add($"{sendTime.ToString("MM-dd HH:mm:ss")}\n{userName}: {message}");
                }
                else
                {
                    App.MessagesReceivedLists.Add(_secondUser, new List<string>() { $"{sendTime.ToString("MM-dd HH:mm:ss")}\n{userName}: {message}" });
                }
            });
        }

        private async void sendButton_Click(object sender, RoutedEventArgs e)
        {
            App myApp = (Application.Current as App);
            if (myApp.MyHubConnection.State != ConnectionState.Connected)
            {
                await new MessageDialog("The server is Disconnected").ShowAsync();
                return;
            }

            string message = this.messageBox.Text.Trim();
            if (message.Length > 0)
            {
                myApp.MyHubProxy.Invoke("SendToClient", _secondUser, _userName, message, DateTime.Now);
                myApp.MyHubProxy.Invoke("Push");
            }
            this.messageBox.Text = string.Empty;
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void messageBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key.ToString() == "Enter")
            {
                sendButton_Click(sender, e);
            }
        }
    }
}
