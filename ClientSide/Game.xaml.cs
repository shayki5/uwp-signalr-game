using ClientSide.Models;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ClientSide
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Game : Page
    {
        private string userName;
        private string secondUser;
        private bool _flagDice;
        private bool _gameStarted;
        private Board _board;
        private Ellipse _selectedCircle;
        private Polygon[] _polygons;
        private List<Ellipse> _circels;
        private Player _currnetPlayer;
        private Player _secondPlayer;
        private Player _currentTurn;
        private bool _isDoubleTurns;

        private IDisposable rollDicedHandler { get; set; }
        private IDisposable myFirstDicetdHandler { get; set; }
        private IDisposable secondPlayerDicedHandler { get; set; }
        private IDisposable circleClickHandler { get; set; }
        private IDisposable polygonClickHandler { get; set; }
        private IDisposable switchTurnHandler { get; set; }
        private IDisposable userExitGameHandler { get; set; }

        public Game()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _selectedCircle = new Ellipse() { Name = "noSelceted" };
            _board = new Board();
            _circels = new List<Ellipse>();
            _polygons = new Polygon[] { _0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13, _14, _15, _16, _17, _18, _19, _20, _21, _22, _23 };

            dynamic info = e.Parameter;
            userName = info.myUserName;
            secondUser = info.secondUser;

            _currnetPlayer = new Player(userName);
            _secondPlayer = new Player(secondUser);

            ControlEllipseEvetns(false);
            ControlPolygonsEvents(false);

            App myApp = (Application.Current as App);
            rollDicedHandler = myApp.MyHubProxy.On<int>("GetDiceResult", GetDiceResult);
            myFirstDicetdHandler = myApp.MyHubProxy.On<int>("GetDiceToStart", GetDiceToStart);
            secondPlayerDicedHandler = myApp.MyHubProxy.On<int>("GetSecondPlayerDiceResult", GetSecondPlayerDiceResult);
            circleClickHandler = myApp.MyHubProxy.On<string>("MakeCircleClick", MakeCircleClick);
            polygonClickHandler = myApp.MyHubProxy.On<string>("MakePolygonClick", MakePolygonClick);
            switchTurnHandler = myApp.MyHubProxy.On("SwitchTurn", SwitchTurn);
            userExitGameHandler = myApp.MyHubProxy.On<string>("UserExitGame", UserExitGame);
        }

        private async void rollDiceBtn_Click(object sender, RoutedEventArgs e)
        {
            App myApp = (Application.Current as App);
            if (myApp.MyHubConnection.State != ConnectionState.Connected)
            {
                await new MessageDialog("The server is Disconnected").ShowAsync();
                return;
            }

            if (_gameStarted == true)
            {
                await myApp.MyHubProxy.Invoke("RollDice", userName, secondUser, false);
                await Task.Delay(600);

                await myApp.MyHubProxy.Invoke("RollDice", userName, secondUser, false);
            }
            else
            {
                await myApp.MyHubProxy.Invoke("RollDice", userName, secondUser, true);
            }

        }

        private async void GetDiceToStart(int result)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                _currnetPlayer.FirstDice = result;
                DrawDice(result, dice1);

                if (_currnetPlayer.FirstDice == _secondPlayer.FirstDice)
                {
                    startGameTxtBlock.Text = "Please roll again";
                    _currnetPlayer.FirstDice = 0;
                    _secondPlayer.FirstDice = 0;
                }
                else
                {
                    if (_currnetPlayer.FirstDice > _secondPlayer.FirstDice && (_currnetPlayer.FirstDice != 0 && _secondPlayer.FirstDice != 0))
                    {
                        _currnetPlayer.Color = "Black";
                        _secondPlayer.Color = "White";
                        _currentTurn = _currnetPlayer;
                        startGameTxtBlock.Text = $"{_currentTurn.Username} won, he his the Black and he start!";
                        await Task.Delay(1000);
                        dice1.Fill = null;
                        dice2.Fill = null;
                        _gameStarted = true;
                        _board.FillBoard();
                        labelP1.Text = _currnetPlayer.Username;
                        labelP2.Text = _secondPlayer.Username;
                        DrawCircels();
                        myFirstDicetdHandler.Dispose();

                    }
                    else if (_currnetPlayer.FirstDice != 0 && _secondPlayer.FirstDice != 0)
                    {
                        _secondPlayer.Color = "Black";
                        _currnetPlayer.Color = "White";
                        _currentTurn = _secondPlayer;
                        startGameTxtBlock.Text = $"{_currentTurn.Username} won, he his the Black and he start!";
                        await Task.Delay(1000);
                        dice1.Fill = null;
                        dice2.Fill = null;
                        _gameStarted = true;
                        rollDiceBtn.Visibility = Visibility.Collapsed;
                        switchTurnButton.Visibility = Visibility.Collapsed;
                        _board.FillBoard();
                        labelP1.Text = _secondPlayer.Username;
                        labelP2.Text = _currnetPlayer.Username;
                        DrawCircels();
                        myFirstDicetdHandler.Dispose();
                    }
                }
            });
        }

        private async void GetSecondPlayerDiceResult(int result)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                _secondPlayer.FirstDice = result;
                DrawDice(result, dice2);

                if (_currnetPlayer.FirstDice == _secondPlayer.FirstDice)
                {
                    startGameTxtBlock.Text = "Please roll again";

                    _currnetPlayer.FirstDice = 0;
                    _secondPlayer.FirstDice = 0;
                }
                else
                {
                    if (_currnetPlayer.FirstDice > _secondPlayer.FirstDice && (_currnetPlayer.FirstDice != 0 && _secondPlayer.FirstDice != 0))
                    {
                        _currnetPlayer.Color = "Black";
                        _secondPlayer.Color = "White";
                        _currentTurn = _currnetPlayer;
                        startGameTxtBlock.Text = $"{_currentTurn.Username} won, he his the Black and he start!";
                        await Task.Delay(1000);
                        dice1.Fill = null;
                        dice2.Fill = null;
                        _gameStarted = true;
                        _board.FillBoard();
                        DrawCircels();
                        labelP1.Text = _currnetPlayer.Username;
                        labelP2.Text = _secondPlayer.Username;
                        secondPlayerDicedHandler.Dispose();

                    }
                    else if (_currnetPlayer.FirstDice != 0 && _secondPlayer.FirstDice != 0)
                    {
                        _secondPlayer.Color = "Black";
                        _currnetPlayer.Color = "White";
                        _currentTurn = _secondPlayer;
                        startGameTxtBlock.Text = $"{_currentTurn.Username} won, he his the Black and he start!";
                        await Task.Delay(1000);
                        dice1.Fill = null;
                        dice2.Fill = null;
                        _gameStarted = true;
                        _board.FillBoard();
                        DrawCircels();
                        labelP1.Text = _secondPlayer.Username;
                        labelP2.Text = _currnetPlayer.Username;
                        rollDiceBtn.Visibility = Visibility.Collapsed;
                        secondPlayerDicedHandler.Dispose();

                    }
                }
            });
        }

        private async void GetDiceResult(int result)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (_flagDice == false)
                {
                    _flagDice = !_flagDice;
                    _currentTurn.Turns.Add(result);
                    DrawDice(result, dice1);
                }
                else
                {
                    _flagDice = !_flagDice;
                    if (result != _currentTurn.Turns.FirstOrDefault())
                    {
                        _currentTurn.Turns.Add(result);
                        _isDoubleTurns = false;
                    }
                    else
                    {
                        _currentTurn.Turns.Add(result);
                        _currentTurn.Turns.Add(result);
                        _currentTurn.Turns.Add(result);
                        _isDoubleTurns = true;
                    }
                    DrawDice(result, dice2);
                    rollDiceBtn.Visibility = Visibility.Collapsed;
                }
            });
        }

        private void DrawDice(int number, Rectangle dice)
        {
            switch (number)
            {
                case 1:
                    dice.Fill = new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/dice1.png")) };
                    dice.Tag = "1";
                    break;
                case 2:
                    dice.Fill = new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/dice2.png")) };
                    dice.Tag = "2";
                    break;
                case 3:
                    dice.Fill = new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/dice3.png")) };
                    dice.Tag = "3";
                    break;
                case 4:
                    dice.Fill = new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/dice4.png")) };
                    dice.Tag = "4";
                    break;
                case 5:
                    dice.Fill = new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/dice5.png")) };
                    dice.Tag = "5";
                    break;
                case 6:
                    dice.Fill = new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/dice6.png")) };
                    dice.Tag = "6";
                    break;
                default:
                    break;
            }

        }

        private void ClearDice(string tag)
        {
            if (_isDoubleTurns == true)
            {
                if (_currentTurn.Turns.Count() == 4)
                {
                    dice1.Fill.Opacity = 0.5;
                }
                else if (_currentTurn.Turns.Count() == 3)
                {
                    dice2.Fill.Opacity = 0.5;
                }
                else if (_currentTurn.Turns.Count() == 2)
                {
                    dice1.Fill = null;
                }
                else
                {
                    dice2.Fill = null;
                }
            }
            else if ((string)dice1.Tag == tag)
            {
                dice1.Fill = null;
                dice2.Tag = "9";
            }
            else
            {
                dice2.Fill = null;
                dice2.Tag = "9";
            }
        }


        private void DrawCircels()
        {
            gameGrid1.Children.Clear();
            gameGrid2.Children.Clear();
            blackDeadArea.Children.Clear();
            whiteDeadArea.Children.Clear();
            {
                int k = 0;
                for (int i = 0; i < _board.GameBoard.Count(); i++)
                {
                    if (i > 11)
                    {
                        k++;
                    }
                    if (_board.GameBoard[i].Count > 0)
                    {
                        if (i <= 11)
                        {
                            int n = 0;
                            foreach (var item in _board.GameBoard[i])
                            {
                                Ellipse el = new Ellipse();
                                if (item.Color == "Black")
                                    el.Fill = new SolidColorBrush(Colors.Black);
                                else
                                    el.Fill = new SolidColorBrush(Colors.White);

                                if (item.IsSelected == true)
                                {
                                    el.Stroke = new SolidColorBrush(Colors.Yellow);
                                    el.StrokeThickness = 3;
                                }
                                else
                                    el.Stroke = null;
                                el.Height = 45;
                                el.Width = 45;
                                el.HorizontalAlignment = HorizontalAlignment.Center;
                                el.Name = $"{i}_{n}";
                                el.AddHandler(TappedEvent, new TappedEventHandler(Ellipse_Tapped), true);
                                _circels.Add(el);
                                if (i == 6)
                                {
                                    el.HorizontalAlignment = HorizontalAlignment.Left;
                                }
                                Grid.SetColumn(el, 11 - i);
                                Grid.SetRow(el, n);
                                gameGrid1.Children.Add(el);

                                n++;
                            }
                        }

                        if (i > 11)
                        {
                            if (_board.GameBoard[i].Count > 0)
                            {

                                int n = 6;

                                foreach (var item in _board.GameBoard[i])
                                {
                                    Ellipse el = new Ellipse();
                                    if (item.Color == "Black")
                                        el.Fill = new SolidColorBrush(Colors.Black);
                                    else
                                        el.Fill = new SolidColorBrush(Colors.White);

                                    if (item.IsSelected == true)
                                    {
                                        el.Stroke = new SolidColorBrush(Colors.Yellow);
                                        el.StrokeThickness = 3;
                                    }
                                    else
                                        el.Stroke = null;
                                    el.Height = 45;
                                    el.Width = 45;
                                    el.HorizontalAlignment = HorizontalAlignment.Center;
                                    el.Name = $"{i}_{n}";
                                    el.AddHandler(TappedEvent, new TappedEventHandler(Ellipse_Tapped), true);
                                    _circels.Add(el);
                                    if (i == 17)
                                    {
                                        el.HorizontalAlignment = HorizontalAlignment.Left;
                                    }
                                    Grid.SetColumn(el, k - 1);
                                    Grid.SetRow(el, n);
                                    gameGrid2.Children.Add(el);

                                    n--;
                                }
                            }
                        }
                    }
                }
                int index = 0;
                foreach (var item in _currnetPlayer.NumOfDeadCircels)
                {
                    Ellipse el = new Ellipse();
                    if (item.Color == "Black")
                        el.Fill = new SolidColorBrush(Colors.Black);
                    else
                        el.Fill = new SolidColorBrush(Colors.White);
                    if (item.IsSelected == true)
                    {
                        el.Stroke = new SolidColorBrush(Colors.Yellow);
                        el.StrokeThickness = 3;
                    }
                    else
                        el.Stroke = null;
                    el.Height = 45;
                    el.Width = 45;
                    el.HorizontalAlignment = HorizontalAlignment.Center;
                    el.Name = $"dead{index}";
                    _circels.Add(el);
                    el.AddHandler(TappedEvent, new TappedEventHandler(Ellipse_Tapped), true);
                    Grid.SetColumn(el, index);
                    Grid.SetRow(el, index);
                    if (_currnetPlayer.Color == "Black")
                    {
                        blackDeadArea.Children.Add(el);
                    }
                    else
                    {
                        whiteDeadArea.Children.Add(el);
                    }
                    index++;
                }

                foreach (var item in _secondPlayer.NumOfDeadCircels)
                {

                    Ellipse el = new Ellipse();
                    if (item.Color == "Black")
                        el.Fill = new SolidColorBrush(Colors.Black);
                    else
                        el.Fill = new SolidColorBrush(Colors.White);
                    if (item.IsSelected == true)
                    {
                        el.Stroke = new SolidColorBrush(Colors.Yellow);
                        el.StrokeThickness = 3;
                    }
                    else
                        el.Stroke = null;
                    el.Height = 45;
                    el.Width = 45;
                    el.HorizontalAlignment = HorizontalAlignment.Center;
                    el.Name = $"dead{index}";
                    _circels.Add(el);
                    el.AddHandler(TappedEvent, new TappedEventHandler(Ellipse_Tapped), true);
                    Grid.SetColumn(el, index);
                    Grid.SetRow(el, index);
                    if (_secondPlayer.Color == "Black")
                    {
                        blackDeadArea.Children.Add(el);
                    }
                    else
                    {
                        whiteDeadArea.Children.Add(el);
                    }
                    index++;
                }

            }
        }

        private async void Ellipse_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Ellipse el = sender as Ellipse;
            SolidColorBrush solidColor = (SolidColorBrush)el.Fill;
            var color = solidColor.Color.ToString();
            var name = el.Name;

            if (_currentTurn == _currnetPlayer && (_currentTurn.Color == "Black" && color == "#FF000000") || (_currentTurn.Color == "White" && color != "#FF000000"))
            {
                ControlPolygonsEvents(true);
                App myApp = (Application.Current as App);
                if (myApp.MyHubConnection.State != ConnectionState.Connected)
                {
                    await new MessageDialog("The server is Disconnected").ShowAsync();
                    return;
                }
                else
                {
                    await myApp.MyHubProxy.Invoke("CircleClick", userName, secondUser, name);
                    return;
                }
            }
            return;
        }

        private async void MakeCircleClick(string elName)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                Ellipse el = _circels.FirstOrDefault(a => a.Name == elName);

                if (_selectedCircle.Name == "noSelceted")
                {
                    if (_currentTurn.NumOfDeadCircels.Count > 0)
                    {
                        if (el.Name.Substring(0, el.Name.Length - 1) == "dead")
                        {
                            _selectedCircle = el;
                            var name = el.Name.Substring(el.Name.Count() - 1);
                            var getInBoard = _board.GameBoard[int.Parse(name)];
                            ShowWhereCanGo(name);

                            foreach (var item in _currentTurn.NumOfDeadCircels)
                            {
                                item.IsSelected = true;
                            }
                        }
                        else
                        {
                            if (_currentTurn == _currnetPlayer)
                            {
                                await new MessageDialog("You can choose only dead circels!").ShowAsync();
                                return;
                            }
                        }
                    }

                    else
                    {
                        _selectedCircle = el;
                        var nameSplited = el.Name.Split('_');
                        var name = nameSplited[0];
                        var getInBoard = _board.GameBoard[int.Parse(name)];
                        ShowWhereCanGo(name);
                        foreach (var item in getInBoard)
                        {
                            item.IsSelected = true;
                        }
                    }

                    DrawCircels();
                    return;
                }
                if (_selectedCircle.Name == el.Name)
                {
                    if (el.Name.Substring(0, el.Name.Length - 1) == "dead")
                    {
                        foreach (var item in _currentTurn.NumOfDeadCircels)
                        {
                            item.IsSelected = false;
                        }
                    }
                    else
                    {
                        var nameSplited = el.Name.Split('_');
                        var name = nameSplited[0];
                        var getInBoard = _board.GameBoard[int.Parse(name)];

                        foreach (var item in getInBoard)
                        {
                            item.IsSelected = false;
                        }
                    }
                    _selectedCircle = new Ellipse() { Name = "noSelceted" };
                    ControlPolygonsEvents(false);
                    ClearShowWhereCanGo();
                    DrawCircels();
                    return;
                }
            });
            return;

        }

        private void ShowWhereCanGo(string position)
        {
            Polygon po = null;
            for (int i = 0; i < _currentTurn.Turns.Count; i++)
            {
                int target = _currentTurn.Turns[i];

                if (_currentTurn.NumOfDeadCircels.Count == 0)
                {
                    if (_currentTurn.Color == "Black")
                    {
                        target = int.Parse(position) + target;
                    }
                    else
                    {
                        target = int.Parse(position) - target;
                    }
                    po = _polygons.FirstOrDefault(o => o.Name == "_" + target.ToString());
                }
                else
                {
                    if (_currentTurn.Color == "White")
                    {
                        switch (target)
                        {
                            case 1:
                                target = 23;
                                break;
                            case 2:
                                target = 22;
                                break;
                            case 3:
                                target = 21;
                                break;
                            case 4:
                                target = 20;
                                break;
                            case 5:
                                target = 19;
                                break;
                            case 6:
                                target = 18;
                                break;
                            default:
                                break;
                        }
                        po = _polygons.FirstOrDefault(o => o.Name == "_" + target.ToString());
                    }
                    else
                    {
                        target--;
                        po = _polygons.FirstOrDefault(o => o.Name == "_" + target.ToString());
                    }
                }

                if (po != null)
                {
                    var nameWithoutUnderScore = po.Name.Substring(1, po.Name.Length - 1);
                    var getPolygonInBoard = _polygons[int.Parse(nameWithoutUnderScore)];

                    if (CanMoveToThere(target, _currentTurn.Color, _selectedCircle, true))
                    {
                        getPolygonInBoard.Stroke = new SolidColorBrush(Colors.Yellow);

                        getPolygonInBoard.StrokeThickness = 3;
                    }
                }
            }
            if (_currentTurn.NumOfCircelsOnBasis == 15)
            {
                foreach (var dice in _currentTurn.Turns)
                {
                    if (int.Parse(position) <= (dice - 1) || (int.Parse(position) + dice >= 24))
                    {
                        if (_currentTurn.Color == "Black")
                        {
                            basisP1.BorderBrush = new SolidColorBrush(Colors.Yellow);
                            basisP1.BorderThickness = new Thickness(4);
                        }
                        else
                        {
                            basisP2.BorderBrush = new SolidColorBrush(Colors.Yellow);
                            basisP2.BorderThickness = new Thickness(4);
                        }
                    }
                }
            }
        }

        private void ClearShowWhereCanGo()
        {
            foreach (var item in _polygons)
            {
                item.StrokeThickness = 0;
                basisP1.BorderThickness = new Thickness(0);
                basisP2.BorderThickness = new Thickness(0);

            }
        }

        private async void Polygon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Polygon po = sender as Polygon;
            string name = null;

            if (po == null)
            {
                Grid baseGrid = sender as Grid;
                name = baseGrid.Name;
            }
            else
            {
                name = po.Name;
            }

            App myApp = (Application.Current as App);
            if (myApp.MyHubConnection.State != ConnectionState.Connected)
            {
                await new MessageDialog("The server is Disconnected").ShowAsync();
                return;
            }
            else
            {
                await myApp.MyHubProxy.Invoke("PolygonClick", userName, secondUser, name);
            }
        }

        private async void MakePolygonClick(string polygonName)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (_selectedCircle.Name != "noSelceted")
                {
                    if (polygonName == "basisP1" || polygonName == "basisP2")
                    {
                        basis_Tapped(polygonName);
                        return;
                    }
                    Polygon po = _polygons.FirstOrDefault(o => o.Name == polygonName);
                    var nameWithoutUnderScore = po.Name.Substring(1, po.Name.Length - 1);

                    var getPolygonInBoard = _board.GameBoard[int.Parse(nameWithoutUnderScore)];

                    if (_selectedCircle.Name.Contains("dead"))
                    {
                        if (CanMoveToThere(int.Parse(nameWithoutUnderScore), _currentTurn.Color, _selectedCircle, false))
                        {
                            _currentTurn.NumOfDeadCircels.RemoveAt(0);

                            getPolygonInBoard.Add(new Circle() { Color = _currentTurn.Color });

                            foreach (var item in _currentTurn.NumOfDeadCircels)
                            {
                                item.IsSelected = false;
                            }

                            _selectedCircle = new Ellipse() { Name = "noSelceted" };
                            ControlPolygonsEvents(false);
                            DrawCircels();
                            ClearShowWhereCanGo();
                            if (_currentTurn.Turns.Count == 0)
                            {
                                NewTurn();
                                return;
                            }
                        }
                    }

                    else
                    {
                        var nameSplited = _selectedCircle.Name.Split('_');
                        var name = nameSplited[0];
                        var getInBoard = _board.GameBoard[int.Parse(name)];

                        if (CanMoveToThere(int.Parse(nameWithoutUnderScore), _currentTurn.Color, _selectedCircle, false))
                        {
                            var removed = getInBoard.First();
                            removed.NeedToRemove = true;
                            getInBoard.RemoveAt(getInBoard.Count - 1);

                            getPolygonInBoard.Add(new Circle() { Color = _currentTurn.Color });

                            if (int.Parse(nameWithoutUnderScore) <= 5 && _currentTurn.Color == "White" && int.Parse(name) > 5)
                            {
                                _currentTurn.NumOfCircelsOnBasis++;
                            }

                            if (int.Parse(nameWithoutUnderScore) >= 18 && _currentTurn.Color == "Black" && int.Parse(name) < 18)
                            {
                                _currentTurn.NumOfCircelsOnBasis++;
                            }

                            foreach (var item in getInBoard)
                            {
                                item.IsSelected = false;
                            }
                            _selectedCircle = new Ellipse() { Name = "noSelceted" };
                            ControlPolygonsEvents(false);

                            DrawCircels();
                            ClearShowWhereCanGo();
                            if (_currentTurn.Turns.Count == 0)
                            {
                                NewTurn();
                            }
                            return;
                        }
                    }
                }
            });
        }

        private void ControlPolygonsEvents(bool active)
        {
            foreach (var item in _polygons)
            {
                if (active)
                {
                    item.Tapped += Polygon_Tapped;
                }
                else
                {
                    item.Tapped -= Polygon_Tapped;
                }
            }
            if (active)
            {
                basisP1.Tapped += Polygon_Tapped;
                basisP2.Tapped += Polygon_Tapped;
            }
            else
            {
                basisP1.Tapped -= Polygon_Tapped;
                basisP2.Tapped -= Polygon_Tapped;
            }
        }

        private void ControlEllipseEvetns(bool active)
        {
            foreach (var item in _circels)
            {
                if (active)
                {
                    item.Tapped += Ellipse_Tapped;
                }
                else
                {
                    item.Tapped -= Ellipse_Tapped;
                }
            }
        }

        private bool CanMoveToThere(int target, string color, Ellipse currentCircle, bool onlyToShow)
        {
            var polgyonTarget = _board.GameBoard[target];

            var nameSplited = _selectedCircle.Name.Split('_');
            var position = nameSplited[0];

            if (_selectedCircle.Name.Contains("dead"))
            {
                if (onlyToShow == true || color == "Black" && (target == 0 || target == 1 || target == 2 || target == 3 || target == 4 || target == 5)
                    || color == "White" && (target == 23 || target == 22 || target == 21 || target == 20 || target == 19 || target == 18))
                {
                    foreach (var dice in _currentTurn.Turns)
                    {
                        if (target == (dice - 1) || (target + dice == 24) || onlyToShow == true)
                        {
                            if (polgyonTarget.Count == 0)
                            {
                                if (onlyToShow == false)
                                {
                                    ClearDice(dice.ToString());
                                    _currentTurn.Turns.Remove(dice);
                                }
                                return true;
                            }
                            else
                            {
                                if (polgyonTarget.FirstOrDefault().Color == color)
                                {
                                    if (onlyToShow == false)
                                    {
                                        ClearDice(dice.ToString());
                                        _currentTurn.Turns.Remove(dice);
                                    }
                                    return true;
                                }
                                else
                                {
                                    if (polgyonTarget.Count == 1)
                                    {
                                        if (onlyToShow == false)
                                        {
                                            if (polgyonTarget[0].Color != color)
                                            {
                                                if (_currentTurn == _currnetPlayer)
                                                {
                                                    _secondPlayer.NumOfDeadCircels.Add(new Circle() { Color = _secondPlayer.Color });
                                                    _secondPlayer.NumOfCircelsOnBasis--;

                                                }
                                                else
                                                {
                                                    _currnetPlayer.NumOfDeadCircels.Add(new Circle() { Color = _currnetPlayer.Color });
                                                    _currnetPlayer.NumOfCircelsOnBasis--;
                                                }
                                                polgyonTarget.RemoveAt(0);

                                            }
                                            ClearDice(dice.ToString());
                                            _currentTurn.Turns.Remove(dice);
                                        }
                                        return true;
                                    }

                                }
                            }

                        }
                    }
                    return false;
                }
            }
            else
            {
                foreach (var dice in _currentTurn.Turns)
                {
                    if ((color == "Black" && target > int.Parse(position) && int.Parse(position) + dice == target) || (color == "White" && target < int.Parse(position) && int.Parse(position) - dice == target))
                    {

                        if (polgyonTarget.Count == 0)
                        {
                            if (onlyToShow == false)
                            {
                                ClearDice(dice.ToString());
                                _currentTurn.Turns.Remove(dice);
                            }
                            return true;
                        }
                        else
                        {
                            if (polgyonTarget.FirstOrDefault().Color == color)
                            {
                                if (onlyToShow == false)
                                {
                                    ClearDice(dice.ToString());
                                    _currentTurn.Turns.Remove(dice);
                                }
                                return true;
                            }
                            else
                            {
                                if (polgyonTarget.Count == 1)
                                {
                                    if (polgyonTarget[0].Color != color)
                                    {
                                        if (onlyToShow == false)
                                        {
                                            if (_currentTurn == _currnetPlayer)
                                            {
                                                _secondPlayer.NumOfDeadCircels.Add(new Circle() { Color = _secondPlayer.Color });
                                            }
                                            else
                                            {
                                                _currnetPlayer.NumOfDeadCircels.Add(new Circle() { Color = _currnetPlayer.Color });
                                            }

                                            polgyonTarget.RemoveAt(0);

                                            if (color == "Black" && (target == 0 || target == 1 || target == 2 || target == 3 || target == 4 || target == 5)
                                               || color == "White" && (target == 23 || target == 22 || target == 21 || target == 20 || target == 19 || target == 18))
                                            {
                                                if (_currentTurn == _currnetPlayer)
                                                {
                                                    _secondPlayer.NumOfCircelsOnBasis--;

                                                }
                                                else
                                                {
                                                    _currnetPlayer.NumOfCircelsOnBasis--;
                                                }
                                            }
                                            ClearDice(dice.ToString());
                                            _currentTurn.Turns.Remove(dice);
                                        };

                                    }
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }
            return false;
        }

        private async void basis_Tapped(string gridName)
        {
            var nameSplited = _selectedCircle.Name.Split('_');
            var position = nameSplited[0];
            if ((_currentTurn.Color == "Black" && int.Parse(position) >= 18 && gridName == "basisP1") || (_currentTurn.Color == "White" && int.Parse(position) <= 5 && gridName == "basisP2"))
            {
                if (_currentTurn.NumOfCircelsOnBasis == 15)
                {
                    foreach (var dice in _currentTurn.Turns)
                    {
                        if (int.Parse(position) <= (dice - 1) || (int.Parse(position) + dice >= 24))
                        {
                            if (_currentTurn.Color == "Black")
                            {
                                var text = int.Parse(p1CicelsOut.Text);
                                text++;
                                p1CicelsOut.Text = text.ToString();
                            }
                            else
                            {
                                var text = int.Parse(p2CicelsOut.Text);
                                text++;
                                p2CicelsOut.Text = text.ToString();
                            }

                            _board.GameBoard[int.Parse(position)].RemoveAt(0);
                            foreach (var item in _board.GameBoard[int.Parse(position)])
                            {
                                item.IsSelected = false;
                            }
                            _currentTurn.NumOfCircelsOut++;
                            _selectedCircle.Name = "noSelceted";
                            ClearDice(dice.ToString());
                            DrawCircels();
                            ClearShowWhereCanGo();
                            _currentTurn.Turns.Remove(dice);
                            if (_currentTurn.NumOfCircelsOut == 15)
                            {
                                await new MessageDialog($"Congratulations! {_currentTurn.Username} Won!").ShowAsync();
                                rollDicedHandler.Dispose();
                                myFirstDicetdHandler.Dispose();
                                secondPlayerDicedHandler.Dispose();
                                circleClickHandler.Dispose();
                                polygonClickHandler.Dispose();
                                userExitGameHandler.Dispose();
                                switchTurnHandler.Dispose();
                                Frame.Navigate(typeof(Lobby));
                            }
                            if (_currentTurn.Turns.Count == 0)
                            {
                                NewTurn();
                            }
                            return;
                        }
                    }
                }
            }
            else
            {
                if (_currentTurn == _currnetPlayer)
                {
                    await new MessageDialog("You can't move here!").ShowAsync();
                }
            }
        }

        private void NewTurn()
        {
            if (_currentTurn == _currnetPlayer)
            {
                _currentTurn = _secondPlayer;
                startGameTxtBlock.Text = $"{_currentTurn.Username} turn now :)";
                ControlEllipseEvetns(false);
                ControlPolygonsEvents(false);
                rollDiceBtn.Visibility = Visibility.Collapsed;
                switchTurnButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                _currentTurn = _currnetPlayer;
                startGameTxtBlock.Text = $"{_currentTurn.Username} turn now :)";
                ControlEllipseEvetns(true);
                switchTurnButton.Visibility = Visibility.Visible;
                rollDiceBtn.Visibility = Visibility.Visible;
            }
        }

        private async void switchTurnButton_Click(object sender, RoutedEventArgs e)
        {
            App myApp = (Application.Current as App);
            if (myApp.MyHubConnection.State != ConnectionState.Connected)
            {
                await new MessageDialog("The server is Disconnected").ShowAsync();
                return;
            }
            else
            {
                await myApp.MyHubProxy.Invoke("SwitchTurn", userName, secondUser);
            }
        }

        private async void SwitchTurn()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _currentTurn.Turns.Clear();
                dice1.Fill = null;
                dice2.Fill = null;
                if (_selectedCircle.Name != "noSelceted")
                {
                    MakeCircleClick(_selectedCircle.Name);
                }
                NewTurn();
            });
        }

        private async void UserExitGame(string userName)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await new MessageDialog($"{userName} leaved the game, you are the winner!").ShowAsync();
                rollDicedHandler.Dispose();
                myFirstDicetdHandler.Dispose();
                secondPlayerDicedHandler.Dispose();
                circleClickHandler.Dispose();
                polygonClickHandler.Dispose();
                userExitGameHandler.Dispose();
                switchTurnHandler.Dispose();
                Frame.Navigate(typeof(Lobby));
            });
        }

        private async void resignBtn_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog askForExit = new ContentDialog();
            askForExit.Content = $"Are you sure? You will be a looser...!";
            askForExit.PrimaryButtonText = "Yes";
            askForExit.SecondaryButtonText = "Cancel";

            ContentDialogResult result = await askForExit.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                App myApp = (Application.Current as App);
                if (myApp.MyHubConnection.State != ConnectionState.Connected)
                {
                    await new MessageDialog("The server is Disconnected").ShowAsync();
                    return;
                }
                else
                {
                    await myApp.MyHubProxy.Invoke("ExitGame", userName, secondUser);
                }

                rollDicedHandler.Dispose();
                myFirstDicetdHandler.Dispose();
                secondPlayerDicedHandler.Dispose();
                circleClickHandler.Dispose();
                polygonClickHandler.Dispose();
                userExitGameHandler.Dispose();
                switchTurnHandler.Dispose();
                Frame.Navigate(typeof(Lobby));
            }
            if (result == ContentDialogResult.Secondary)
            {

            }
        }
    }
}
