using battleships_game_app.GameManagerRelated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battleships_game_app.WarshipRelated
{
    public class DonutShip : Warship
    {
        public DonutShip(Board board) : base(board)
        {
            Console.WriteLine("Donut-shaped ship created.");
        }
    }
}
