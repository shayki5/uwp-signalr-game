using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using ServerSide.Models;
using Windows.UI.Xaml.Shapes;

namespace ServerSide.Hubs
{
    public class ChatHub : Hub
    {
        private static Dictionary<string, string> _usersList = new Dictionary<string, string>();
        private Random rnd = new Random();
        private List<string> connIdList;

        public void SendToClient(string secondUser, string userName, string message, DateTime sendTime)
        {
            var connIdFirstUser = _usersList[userName];
            var connIdSecondUser = _usersList[secondUser];

            connIdList = new List<string>() { connIdFirstUser, connIdSecondUser };
            
            Clients.Clients(connIdList).ReceiveMessage(userName, message, sendTime);
            Clients.Clients(connIdList).PushNewMessage(userName, message, sendTime);
            Clients.Client(connIdSecondUser).SaveMessages(userName, $"{sendTime.ToString("MM-dd HH:mm:ss")}\n{userName}: {message}");

        }

        public void CircleClick(string userName, string secondUser, string name)
        {
            var connIdFirstUser = _usersList[userName];
            var connIdSecondUser = _usersList[secondUser];
            string elName = name;
            
            connIdList = new List<string>() { connIdFirstUser, connIdSecondUser };

            Clients.Clients(connIdList).MakeCircleClick(elName);
        }

        public void PolygonClick(string userName, string secondUser, string name)
        {
            var connIdFirstUser = _usersList[userName];
            var connIdSecondUser = _usersList[secondUser];
            string polygonName = name;
            connIdList = new List<string>() { connIdFirstUser, connIdSecondUser };

            Clients.Clients(connIdList).MakePolygonClick(polygonName);
        }

        public void RollDice(string userName, string secondUser, bool IsFirstDice)
        {
            int result = rnd.Next(1, 7);

            var connIdFirstUser = _usersList[userName];
            var connIdSecondUser = _usersList[secondUser];

            connIdList = new List<string>() { connIdFirstUser, connIdSecondUser };

            if (IsFirstDice == true)
            {
                Clients.Client(connIdFirstUser).GetDiceToStart(result);
                Clients.Client(connIdSecondUser).GetSecondPlayerDiceResult(result);
            }
            else
            {
                Clients.Clients(connIdList).GetDiceResult(result);
            }

        }

        public void SwitchTurn(string userName, string secondUser)
        {
            var connIdFirstUser = _usersList[userName];
            var connIdSecondUser = _usersList[secondUser];

            connIdList = new List<string>() { connIdFirstUser, connIdSecondUser };

            Clients.Clients(connIdList).SwitchTurn();
        }

        public void ExitGame(string userName, string secondUser)
        {
            var connIdFirstUser = _usersList[userName];
            var connIdSecondUser = _usersList[secondUser];

            Clients.Client(connIdSecondUser).UserExitGame(userName);
        }

        public void PushAskForGame(string userName, string secondUser)
        {
            var connIdSecondUser = _usersList[secondUser];
            Clients.Client(connIdSecondUser).AskForGame(userName);
        }

        public void CancelGameRequest(string userName, string secondUser)
        {
            var connIdSecondUser = _usersList[secondUser];
            Clients.Client(connIdSecondUser).GameCacnceled(secondUser);
        }

        public void AnswerToGameRequest(string userName, string secondUser, bool isWant)
        {
            var connIdFirstUser = _usersList[userName];
            var connIdSecondUser = _usersList[secondUser];

            connIdList = new List<string>() { connIdFirstUser, connIdSecondUser };

            if (isWant == false)
            {
                Clients.Client(connIdSecondUser).AnswerToGame(secondUser, userName, isWant);
            }
            else
            {
                Clients.Client(connIdFirstUser).AnswerToGame(userName, secondUser, isWant);
                Clients.Client(connIdSecondUser).AnswerToGame(secondUser, userName, isWant);
            }
        }

        public void PushWhenConnect()
        {
            var userName = Context.QueryString["userName"].ToString();
            Clients.All.PushConnectMessage(userName);
        }

        public void PushWhenDisconnect()
        {
            var userName = Context.QueryString["userName"].ToString();
            Clients.All.PushDisconnectMessage(userName);
        }

        public void GetMessages(string secondUser, string userName, List<string> messages)
        {
            var connIdFirstUser = _usersList[userName];
            var connIdSecondUser = _usersList[secondUser];

            connIdList = new List<string>() { connIdFirstUser, connIdSecondUser };

            if (messages ==  null)
            {
                return;
            }
            else
            {
                Clients.Clients(connIdList).GetMessages(messages);
            }
        }

        public override Task OnConnected()
        {
            var userName = Context.QueryString["userName"].ToString();
            _usersList.Add(userName, Context.ConnectionId);
            PushWhenConnect();
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var userName = Context.QueryString["userName"].ToString();
            _usersList.Remove(userName);
            PushWhenDisconnect();
            return base.OnDisconnected(stopCalled);
        }

    }
}
