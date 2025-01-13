using battleships_game_app.CellRelated;
using battleships_game_app.GameManagerRelated;
using battleships_game_app.WarshipRelated;

public class StandardShip : Warship
{
    private readonly int _length;
    private Position _startPosition;
    private bool _isHorizontal;
    public List<Cell> Body { get; private set; } // Lista komórek statku

    public StandardShip(int length, Position startPosition, Board board, bool isHorizontal = true) : base(board)
    {
        if (length < 2 || length > 3)
            throw new ArgumentException("Standard ship length must be 2 or 3.");

        _length = length;
        _startPosition = startPosition;
        _isHorizontal = isHorizontal;
        Body = new List<Cell>();
        UpdateBody();
    }
    public void RemoveOldCells()
    {
        foreach (var cell in Body)
        {
            var boardCell = WarshipBoard.GetCell(cell.Position);
            if (boardCell != null)
            {
                boardCell.SetState(new Neutral());  // Resetujemy stan komórki
            }
        }
    }
    public void Destroy(Board board)
    {
        RemoveOldCells();
        Console.WriteLine("Ship destroyed and cells reset to neutral.");
    }
    private void UpdateBody()
    {
        Body.Clear();

        // Dodaj komórki w zależności od orientacji
        for (int i = 0; i < _length; i++)
        {
            var x = _startPosition.X;
            var y = _startPosition.Y;

            if (_isHorizontal)
            {
                y += i;  // Dodajemy do poziomu
            }
            else
            {
                x += i;  // Dodajemy do pionu
            }

            var position = new Position(x, y);
            Body.Add(new Cell(position, new Icon()));  // Zmieniamy stan komórki po rotacji
        }
    }


    public void Rotate()
    {
        // Usuń stare komórki z planszy
        RemoveOldCells();

        // Rotacja statku
        _isHorizontal = !_isHorizontal;
        UpdateBody();  // Przeliczenie nowych komórek w ciele statku

        // Zaktualizowanie nowych komórek na planszy
        foreach (var cell in Body)
        {
            var boardCell = WarshipBoard.GetCell(cell.Position);
            if (boardCell != null)
            {
                boardCell.SetState(new NotHit());  // Ustawiamy stan komórki na zajętą przez statek
            }
        }
    }


    public void SetStartPosition(Position newStartPosition)
    {
        // Usuwamy stary stan komórek na planszy
        RemoveOldCells();

        // Ustawiamy nową pozycję początkową
        _startPosition = newStartPosition;

        // Aktualizujemy ciało statku na podstawie nowej pozycji
        UpdateBody();

        // Ustawiamy stan nowych komórek na planszy
        foreach (var cell in Body)
        {
            var boardCell = WarshipBoard.GetCell(cell.Position);
            if (boardCell != null)
            {
                boardCell.SetState(new NotHit());  // Ustawiamy stan komórki na zajętą przez statek
            }
        }
    }

}
