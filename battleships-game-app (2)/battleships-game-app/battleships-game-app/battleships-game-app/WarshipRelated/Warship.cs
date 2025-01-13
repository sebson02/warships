using battleships_game_app.CellRelated;
using battleships_game_app.GameManagerRelated;
using System;
using System.Collections.Generic;

namespace battleships_game_app.WarshipRelated
{
    public abstract class Warship : IWarship
    {
        protected List<Cell> Body = new();
        public Board WarshipBoard { get; private set; }
        public bool IsSunk => Body.TrueForAll(cell => cell.State is Sunk);

        // Konstruktor, który przypisuje planszę
        public Warship(Board board)
        {
            WarshipBoard = board ?? throw new ArgumentNullException(nameof(board), "Board cannot be null.");
        }

        // Metoda do przypisywania statku do planszy
        public void AddCell(Cell cell)
        {
            if (WarshipBoard.GetCell(cell.Position) != null)  // Sprawdzamy, czy komórka należy do planszy
            {
                Body.Add(cell);
                cell.SetState(new NotHit());  // Zmieniamy stan komórki na "Ship"
            }
            else
            {
                throw new InvalidOperationException("Cell does not belong to the board.");
            }
        }

        // Metoda do pobrania ciała statku
        public IEnumerable<Cell> GetBody()
        {
            return Body;
        }

        // Metoda do umieszczania statku na planszy
        public void PlaceOnBoard(Position startPosition, int length, bool isHorizontal)
        {
            for (int i = 0; i < length; i++)
            {
                var offsetX = isHorizontal ? i : 0;
                var offsetY = isHorizontal ? 0 : i;

                var cellPosition = new Position(startPosition.X + offsetX, startPosition.Y + offsetY);
                var boardCell = WarshipBoard.GetCell(cellPosition);

                if (boardCell != null && boardCell.State is Neutral)  // Sprawdzamy, czy komórka jest pusta
                {
                    AddCell(boardCell);  // Dodajemy komórkę do ciała statku
                }
                else
                {
                    throw new InvalidOperationException("Cannot place ship at the specified location.");
                }
            }
        }

        // Jeśli musisz zmieniać planszę w trakcie działania, możesz dodać metodę ustawiającą planszę
        public void SetBoard(Board board)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board), "Board cannot be null.");
            }
            WarshipBoard = board;
        }
    }
}
