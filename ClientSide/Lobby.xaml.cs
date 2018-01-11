using Microsoft.AspNet.SignalR.Client;
using Windows.UI.Popups;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ClientSide
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Lobby : Page
    {
        private App myApp = (Application.Current as App);
        private IDisposable receiveMessageHandler { get; set; }
        private IDisposable PushConnectMessageHandler { get; set; }
        private IDisposable PushDisconnectMessageHandler { get; set; }
        private IDisposable PushAskForGameHandler { get; set; }
        private IDisposable SaveMessagesHandler { get; set; }
        private IDisposable AnswerToGameHandler { get; set; }
        private IDisposable GameCacnceledHandler { get; set; }
        private DispatcherTimer timerNewMessage = new DispatcherTimer();
        private DispatcherTimer timerNewConnect = new DispatcherTimer();
        private DispatcherTimer timerAskForGame = new DispatcherTimer();
        private ContentDialog _AnswerForGameDialog = new ContentDialog();
        private ContentDialog _AskForGameDialog = new ContentDialog();
        private const long TIME_FOR_TIMER = 300000000;

        public Lobby()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            nameTxt.Text = App.CurrentUser.Username;
            UpdateUsers();
            PushConnectMessageHandler = myApp.MyHubProxy.On<string>("PushConnectMessage", PushConnectMessage);
            PushDisconnectMessageHandler = myApp.MyHubProxy.On<string>("PushDisconnectMessage", PushDisconnectMessage);
            receiveMessageHandler = myApp.MyHubProxy.On<string, string, DateTime>("receivemessage", PushNewMessage);
            PushAskForGameHandler = myApp.MyHubProxy.On<string>("AskForGame", AskForGame);
            SaveMessagesHandler = myApp.MyHubProxy.On<string, string>("SaveMessages", SaveMessages);
            AnswerToGameHandler = myApp.MyHubProxy.On<string, string, bool>("AnswerToGame", AnswerToGame);
            GameCacnceledHandler = myApp.MyHubProxy.On<string>("GameCacnceled", GameCacnceled);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            PushConnectMessageHandler.Dispose();
            PushDisconnectMessageHandler.Dispose();
            receiveMessageHandler.Dispose();
            PushAskForGameHandler.Dispose();
            SaveMessagesHandler.Dispose();
            AnswerToGameHandler.Dispose();
            GameCacnceledHandler.Dispose();
        }

        private async void GameCacnceled(string userName)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                _AnswerForGameDialog.Hide();
                await new MessageDialog($"{userName} canceled the game request").ShowAsync();
            });
        }

        private async void AnswerToGame(string userName, string secondUser, bool isWant)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                if (isWant == false)
                {
                    _AskForGameDialog.Hide();
                    await new MessageDialog($"\"{secondUser}\" not want to play :(").ShowAsync();
                }
                else
                {
                    _AskForGameDialog.Hide();
                    dynamic info = new { myUserName = userName, secondUser = secondUser };
                    Frame.Navigate(typeof(Game), info);
                }
            });
        }

        private async void SaveMessages(string userName, string newMessage)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (App.MessagesReceivedLists.ContainsKey(userName))
                {
                    App.MessagesReceivedLists[userName].Add(newMessage);
                    return;
                }
                else
                {
                    App.MessagesReceivedLists.Add(userName, new List<string> { newMessage });
                }
            });
        }

        private async void AskForGame(string userName)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                if (userName != App.CurrentUser.Username)
                {
                    _AnswerForGameDialog.Content = $"\"{userName}\" want to play with you Backgammon";
                    _AnswerForGameDialog.PrimaryButtonText = "Accept";
                    _AnswerForGameDialog.SecondaryButtonText = "Decline";

                    timerAskForGame.Interval = new TimeSpan(TIME_FOR_TIMER);
                    timerAskForGame.Tick += timerAskForGame_Tick;
                    timerAskForGame.Start();

                    ContentDialogResult result = await _AnswerForGameDialog.ShowAsync();
                    PushAskForGameHandler.Dispose();

                    if (result == ContentDialogResult.Primary)
                    {
                        timerAskForGame.Stop();
                        await myApp.MyHubProxy.Invoke("AnswerToGameRequest", App.CurrentUser.Username, userName, true);
                    }
                    if (result == ContentDialogResult.Secondary)
                    {
                        timerAskForGame.Stop();
                        await myApp.MyHubProxy.Invoke("AnswerToGameRequest", App.CurrentUser.Username, userName, false);
                    }                   
                }
            });
        }

        private async void PushConnectMessage(string userName)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (userName != App.CurrentUser.Username)
                {
                    UpdateUsers();
                    pushConnectGrid.Visibility = Visibility.Visible;
                    userConnectTxtBlock.Text = userName;
                    disconnectTxtBlock.Visibility = Visibility.Collapsed;
                    connectTxtBlock.Visibility = Visibility.Visible;
                    timerNewConnect.Interval = new TimeSpan(TIME_FOR_TIMER);
                    timerNewConnect.Tick += TimerNewConnect_Tick;
                    timerNewConnect.Start();
                }
            });
        }

        private async void PushDisconnectMessage(string userName)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (userName != App.CurrentUser.Username)
                {
                    UpdateUsers(); 
                    pushConnectGrid.Visibility = Visibility.Visible;
                    userConnectTxtBlock.Text = userName;
                    connectTxtBlock.Visibility = Visibility.Collapsed;
                    disconnectTxtBlock.Visibility = Visibility.Visible;
                    timerNewConnect.Interval = new TimeSpan(TIME_FOR_TIMER);
                    timerNewConnect.Tick += TimerNewConnect_Tick;
                    timerNewConnect.Start();
                }
            });
        }

        private void timerAskForGame_Tick(object sender, object e)
        {
            _AnswerForGameDialog.Hide();
            timerAskForGame.Stop();
        }

        private void TimerNewConnect_Tick(object sender, object e)
        {
            pushConnectGrid.Visibility = Visibility.Collapsed;
            timerNewConnect.Stop();
        }

        private async void PushNewMessage(string userName, string message, DateTime sendTime)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                playSound.Play();
                pushMsgGrid.Visibility = Visibility.Visible;
                pushMsg.Text = $"{sendTime.ToString("MM-dd HH:mm:ss")}\n{userName}: {message}";
                timerNewMessage.Interval = new TimeSpan(TIME_FOR_TIMER);
                timerNewMessage.Tick += TimerNewMessage_Tick;
                timerNewMessage.Start();
            });

        }

        private void TimerNewMessage_Tick(object sender, object e)
        {
            pushMsgGrid.Visibility = Visibility.Collapsed;
            pushMsg.Text = "";
            timerNewMessage.Stop();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            List<string> recentMessages = new List<string>();
            var secondUser = (sender as Button).Tag.ToString();
            if (secondUser == App.CurrentUser.Username)
            {
                await new MessageDialog("You can't chat with yourself ;)").ShowAsync();
            }
            else
            {
                SaveMessagesHandler.Dispose();
                var connId = myApp.MyHubConnection.ConnectionId;
                if (App.MessagesReceivedLists.ContainsKey(secondUser))
                {
                    recentMessages = App.MessagesReceivedLists[secondUser];
                }
                dynamic info = new { connId = connId, userName = App.CurrentUser.Username, secondUser = secondUser, recentMessages = recentMessages };
                Frame.Navigate(typeof(Chat), info);
            }
        }

        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        private void logoutBtn_Click(object sender, RoutedEventArgs e)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(App.CurrentUser), Encoding.UTF8, "application/json");
                var result = client.PostAsync($"{App.BaseUri}/offlineUser", content).Result;
            }
            myApp.MyHubConnection.Dispose();
            Frame.Navigate(typeof(MainPage));
        }

        private void UpdateUsers()
        {
            using (var client = new System.Net.Http.HttpClient())
            {

                var getOnlineUsers = client.GetAsync($"{App.BaseUri}/getOnline").Result.Content.ReadAsStringAsync().Result;

                var onlineUsers = JsonConvert.DeserializeObject<List<User>>(getOnlineUsers);

                if (onlineUsers != null)
                {
                    onlineList.ItemsSource = onlineUsers;
                }

                var getOfflineUsers = client.GetAsync($"{App.BaseUri}/getOffline").Result.Content.ReadAsStringAsync().Result;

                var offlineUsers = JsonConvert.DeserializeObject<List<User>>(getOfflineUsers);

                offlineList.ItemsSource = offlineUsers;

            }
        }

        private async void Button_Click_Game(object sender, RoutedEventArgs e)
        {
            var secondUser = (sender as Button).Tag.ToString();

            if (secondUser == App.CurrentUser.Username)
            {
                await new MessageDialog("You can't play with yourself ;)").ShowAsync();
            }

            else
            {
                await myApp.MyHubProxy.Invoke("PushAskForGame", App.CurrentUser.Username, secondUser);
                _AskForGameDialog.Content = $"Waiting for \"{secondUser}\" answer";
                _AskForGameDialog.PrimaryButtonText = "Cancel";

                ContentDialogResult result = await _AskForGameDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await myApp.MyHubProxy.Invoke("CancelGameRequest", App.CurrentUser.Username, secondUser);
                }
            }
        }
    }
}
