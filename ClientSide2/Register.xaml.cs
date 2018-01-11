using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ClientSide2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Register : Page
    {
        App myApp = (Application.Current as App);
        private const string SERVER_URL = "http://localhost:59920";

        public Register()
        {
            this.InitializeComponent();
        }

        private async void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            if (userBox.Text == "" || passwordBox.Password.ToString() == "")
            {
                await new MessageDialog("Username & Password are required!").ShowAsync();
                return;
            }
            var user = new User
            {
                Username = userBox.Text,
                Password = passwordBox.Password.ToString()
            };

            using (var client = new System.Net.Http.HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                var result = client.PostAsync($"{App.BaseUri}/register", content).Result;
                if (result.IsSuccessStatusCode == true)
                {
                    var a = result.Content.ReadAsStringAsync().Result;
                    if (a == "\"exist\"")
                    {
                        await new MessageDialog("The usename is exist, try another user name").ShowAsync();
                    }
                    else
                    {
                        await new MessageDialog("Success! :)").ShowAsync();
                        var currentUser = JsonConvert.DeserializeObject<User>(a);
                        App.CurrentUser = currentUser;

                        Dictionary<string, string> userDetails = new Dictionary<string, string>();
                        userDetails.Add("userName", App.CurrentUser.Username);
                        myApp.MyHubConnection = new Microsoft.AspNet.SignalR.Client.HubConnection(SERVER_URL, userDetails);

                        myApp.MyHubProxy = myApp.MyHubConnection.CreateHubProxy("ChatHub");
                        if (myApp.MyHubConnection.State != Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)
                        {
                            try
                            {
                                await myApp.MyHubConnection.Start();

                            }
                            catch
                            {
                                await new MessageDialog($"Can't connect to server {myApp.MyHubConnection.Url}").ShowAsync();
                                return;
                            }
                        }

                        Frame.Navigate(typeof(Lobby));
                    }
                }
                else
                {
                    await new MessageDialog("Can't connect to server").ShowAsync();
                    return;
                }
            }
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }

}
