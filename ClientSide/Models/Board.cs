using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSide.Models
{
    public class Board
    {
        public List<Circle>[] GameBoard { get; set; }

        public Board()
        {
            GameBoard = new List<Circle>[24];

            for (int i = 0; i < GameBoard.Count(); i++)
            {
                GameBoard[i] = new List<Circle>();
            }
        }

        public void FillBoard()
        {
            GameBoard[0].Add(new Circle() { Color = "Black" });
            GameBoard[0].Add(new Circle() { Color = "Black" });
            GameBoard[11].Add(new Circle() { Color = "Black" });
            GameBoard[11].Add(new Circle() { Color = "Black" });
            GameBoard[11].Add(new Circle() { Color = "Black" });
            GameBoard[11].Add(new Circle() { Color = "Black" });
            GameBoard[11].Add(new Circle() { Color = "Black" });
            GameBoard[16].Add(new Circle() { Color = "Black" });
            GameBoard[16].Add(new Circle() { Color = "Black" });
            GameBoard[16].Add(new Circle() { Color = "Black" });
            GameBoard[18].Add(new Circle() { Color = "Black" });
            GameBoard[18].Add(new Circle() { Color = "Black" });
            GameBoard[18].Add(new Circle() { Color = "Black" });
            GameBoard[18].Add(new Circle() { Color = "Black" });
            GameBoard[18].Add(new Circle() { Color = "Black" });
            GameBoard[5].Add(new Circle() { Color = "White" });
            GameBoard[5].Add(new Circle() { Color = "White" });
            GameBoard[5].Add(new Circle() { Color = "White" });
            GameBoard[5].Add(new Circle() { Color = "White" });
            GameBoard[5].Add(new Circle() { Color = "White" });
            GameBoard[7].Add(new Circle() { Color = "White" });
            GameBoard[7].Add(new Circle() { Color = "White" });
            GameBoard[7].Add(new Circle() { Color = "White" });
            GameBoard[12].Add(new Circle() { Color = "White" });
            GameBoard[12].Add(new Circle() { Color = "White" });
            GameBoard[12].Add(new Circle() { Color = "White" });
            GameBoard[12].Add(new Circle() { Color = "White" });
            GameBoard[12].Add(new Circle() { Color = "White" });
            GameBoard[23].Add(new Circle() { Color = "White" });
            GameBoard[23].Add(new Circle() { Color = "White" });
        }
    }
}
