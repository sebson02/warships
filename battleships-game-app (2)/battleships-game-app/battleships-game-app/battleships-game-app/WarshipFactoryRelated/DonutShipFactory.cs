using battleships_game_app.GameManagerRelated;
using battleships_game_app.WarshipRelated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battleships_game_app.WarshipFactoryRelated
{
    public class DonutShipFactory : WarshipFactory
    {
        private readonly bool _upgraded;

        public DonutShipFactory(bool upgraded)
        {
            _upgraded = upgraded;
        }

        public override IWarship CreateWarship(Board board)
        {
            var ship = new DonutShip(board);
            if (_upgraded)
            {
                Console.WriteLine("Upgraded donut ship created!");
            }
            return ship;
        }
    }
}
