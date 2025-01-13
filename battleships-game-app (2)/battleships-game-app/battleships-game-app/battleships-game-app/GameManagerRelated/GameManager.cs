using battleships_game_app.CellRelated;
using battleships_game_app.CommandRelated;
using battleships_game_app.GameRelated;
using battleships_game_app.WarshipFactoryRelated;
using battleships_game_app.WarshipRelated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battleships_game_app.GameManagerRelated
{
    internal class GameManager
    {
        public Game Game { get; private set; }

        public void SetGame(Game game)
        {
            Game = game;
        }

        /// <summary>
        /// Initializes the game with two players and sets up the board and game state.
        /// </summary>
        /// <param name="p1">First player</param>
        /// <param name="p2">Second player</param>
        public void InitStandardGame(Player p1, Player p2)
        {
            if (p1 == null || p2 == null)
            {
                throw new ArgumentNullException("Both players must be provided.");
            }

            // Initialize a standard game board
            int boardWidth = 10; // Example board width
            int boardHeight = 10; // Example board height
            var board = new Board(boardWidth, boardHeight);
            InitializeBoardCells(board);

            // Create a new game instance
            Game = new Game(p1, p2, board)
            {
                GameHistory = new CommandInvoker(),
                SavedStates = new Stack<BoardMemento>()
            };

            // Notify players that the game is starting
            NotifyPlayers(Game.player, Game.player2);

            Console.WriteLine("Game initialized successfully.");
        }

        /// <summary>
        /// Initializes the cells on the board with default settings.
        /// </summary>
        /// <param name="board">The game board to initialize</param>
        private void InitializeBoardCells(Board board)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            board.Fields.Clear(); // Clear the existing fields

            for (int i = 0; i < board.Width; i++)
            {
                for (int j = 0; j < board.Height; j++)
                {
                    var position = new Position(i, j); // Pozycja komórki
                    var iconManager = new Icon(); // Manager ikon dla wizualizacji
                    var cell = new Cell(position, iconManager); // Tworzenie komórki bez Id
                    cell.SetState(new Neutral());
                    board.Fields.Add(cell); // Dodanie komórki do planszy
                }
            }

            Console.WriteLine($"Board initialized with {board.Fields.Count} cells.");
        }

        /// <summary>
        /// Prints the current state of the game board.
        /// </summary>
        public void PrintBoard()
        {
            if (Game.board == null || Game.board.Fields == null)
            {
                Console.WriteLine("Board is not initialized.");
                return;
            }

            Console.Clear(); // Clear the console before displaying the board
            Console.WriteLine("Current Board:");

            // Column headers (A, B, C, ...)
            Console.Write("   "); // Offset for row numbers
            for (int col = 0; col < Game.board.Width; col++)
            {
                Console.Write($" {Convert.ToChar('A' + col)} ");
            }
            Console.WriteLine();

            // Draw the board
            for (int row = 0; row < Game.board.Height; row++)
            {
                // Row number (1, 2, 3, ...)
                Console.Write($"{row + 1,2} ");

                for (int col = 0; col < Game.board.Width; col++)
                {
                    // Get the cell based on its position
                    var cell = Game.board.Fields.FirstOrDefault(c => c.Position.X == row && c.Position.Y == col);

                    if (cell != null)
                    {
                        // Display the icon of the cell
                        Console.Write($" {cell.IconManager.GetIconForState(cell.State)} ");
                    }
                    else
                    {
                        // If no cell, use default symbol
                        Console.Write(" ~ ");
                    }
                }

                Console.WriteLine(); // New line after each row
            }

            Console.WriteLine(); // Empty line at the end of the board
        }

        /// <summary>
        /// Notifies players that the game has started.
        /// </summary>
        /// <param name="player1">First player</param>
        /// <param name="player2">Second player</param>
        private void NotifyPlayers(Player player1, Player player2)
        {
            Console.WriteLine($"Player 1 ({player1.Name}) and Player 2 ({player2.Name}) are ready.");
            Console.WriteLine("Let the battle begin!");
        }

        public void AddShips(Player player)
        {
            Console.WriteLine($"Adding ships for player: {player.Name}");

            while (true)
            {
                PrintBoard();  // Print the current state of the board after each change
                Console.WriteLine("Enter starting position (e.g., A1) or type 'done' to finish:");
                var positionInput = Console.ReadLine();

                if (positionInput?.ToLower() == "done")
                    break;

                if (!TryParsePosition(positionInput, out Position startPosition))
                {
                    Console.WriteLine("Invalid position. Try again.");
                    continue;
                }

                Console.WriteLine("Enter ship length (2 or 3):");
                if (!int.TryParse(Console.ReadLine(), out int length) || (length != 2 && length != 3))
                {
                    Console.WriteLine("Invalid length. Only 2 or 3 are allowed.");
                    continue;
                }

                var factory = new StandardShipFactory(length, startPosition);
                var ship = (StandardShip)factory.CreateWarship(Game.board);

                bool shipPlaced = false;
                List<Cell> cellsToReset = new List<Cell>(); // Lista komórek do przywrócenia w przypadku anulowania
                ship.Rotate();
                ship.Rotate();
                while (!shipPlaced)
                {
                    Console.WriteLine("Current ship placement:");
                    PrintBoard();  // Show the updated board with current ship position

                    Console.WriteLine("Options: (1) Place ship, (2) Rotate, (3) Change position, (4) Cancel");
                    var option = Console.ReadLine();

                    switch (option)
                    {
                        case "1":  // Place ship
                            if (CanPlaceShip(ship, Game.board))
                            {
                                PlaceShipOnBoard(ship, Game.board);
                                Console.WriteLine("Ship placed.");
                                shipPlaced = true;  // Exit the loop after placing the ship
                            }
                            else
                            {
                                Console.WriteLine("Cannot place the ship here. Try again.");
                            }
                            break;

                        case "2":  // Rotate ship
                                   // Zapisujemy komórki zajmowane przez statek przed rotacją
                            cellsToReset = GetShipCells(ship);

                            ship.Rotate();  // Rotujemy statek
                            Console.WriteLine("Ship rotated.");

                            // Po rotacji, możemy zapisać nową pozycję, ale przy anulowaniu musimy cofnąć zmiany
                            break;

                        case "3":  // Change ship's starting position
                            Console.WriteLine("Enter new starting position (e.g., A1):");
                            var newPositionInput = Console.ReadLine();
                            if (TryParsePosition(newPositionInput, out Position newPosition))
                            {
                                // Przywracamy poprzednie komórki do Neutral przed zmianą pozycji
                                foreach (var cell in cellsToReset)
                                {
                                    var boardCell = Game.board.GetCell(cell.Position);
                                    if (boardCell != null)
                                    {
                                        boardCell.SetState(new Neutral());
                                    }
                                }

                                ship.SetStartPosition(newPosition);  // Update ship position
                                cellsToReset = GetShipCells(ship);   // Zapisujemy nowe komórki do resetu
                                Console.WriteLine("Ship position updated.");
                            }
                            else
                            {
                                Console.WriteLine("Invalid position. Try again.");
                            }
                            break;

                        case "4":  // Cancel ship placement
                            Console.WriteLine("Cancelled ship placement.");
                            ship.Destroy(Game.board);

                            // Po anulowaniu zmiany, kończymy ustawianie statku
                            return; // Exit the method, cancelling the ship placement

                        default:
                            Console.WriteLine("Invalid option. Try again.");
                            break;
                    }
                }
            }
        }


        private List<Cell> GetShipCells(StandardShip ship)
        {
            List<Cell> cells = new List<Cell>();
            foreach (var cell in ship.GetBody())
            {
                var boardCell = Game.board.GetCell(cell.Position);
                if (boardCell != null)
                {
                    cells.Add(boardCell);
                }
            }
            return cells;
        }





        private bool CanPlaceShip(IWarship ship, Board board)
        {
            foreach (var cell in ship.GetBody())
            {
                if (cell.Position.X < 0 || cell.Position.Y < 0 ||
                    cell.Position.X >= board.Height || cell.Position.Y >= board.Width ||
                    board.Fields.Any(existingCell => existingCell.Position.Equals(cell.Position)))
                {
                    return false;
                }
            }
            return true;
        }

        private void PlaceShipOnBoard(IWarship ship, Board board)
        {
            foreach (var cell in ship.GetBody())
            {
                var boardCell = board.GetCell(cell.Position);
                if (boardCell != null)
                {
                    boardCell.SetState(new NotHit()); // Mark the cell as occupied by the ship
                }
            }
        }

        private bool TryParsePosition(string input, out Position position)
        {
            position = null;

            if (string.IsNullOrEmpty(input) || input.Length < 2)
                return false;

            char column = input[0];
            if (!char.IsLetter(column))
                return false;

            string rowString = input.Substring(1);
            if (!int.TryParse(rowString, out int row))
                return false;

            position = new Position(row - 1, char.ToUpper(column) - 'A');
            return true;
        }
    }
}
