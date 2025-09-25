using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerOption
{
    NONE, //0
    X, // 1
    O // 2
}

public class TTT : MonoBehaviour
{
    public int Rows;
    public int Columns;
    [SerializeField] bool isBoardEmpty;
    [SerializeField] BoardView board;

    PlayerOption currentPlayer = PlayerOption.X;
    Cell[,] cells;

    List<(int x, int y)> boardCorners;

    // Start is called before the first frame update
    void Start()
    {
        isBoardEmpty = true;

        cells = new Cell[Columns, Rows];

        board.InitializeBoard(Columns, Rows);

        for(int i = 0; i < Rows; i++)
        {
            for(int j = 0; j < Columns; j++)
            {
                cells[j, i] = new Cell();
                cells[j, i].current = PlayerOption.NONE;
            }
        }

        //get corners of my board in a list
        boardCorners = new List<(int x, int y)>()
        {
            (0,0),
            (2,0),
            (0,2),
            (2,2)
        };
    }

    //hint time travel?
    public void MakeOptimalMove()
    {

        isBoardEmpty = IsBoardEmpty();

        //If the board is empty, it is advantageous to take a corner
        if (isBoardEmpty)
        {
            ChooseSpace(2, 0);
            return;
        }
        //If the opponent controls a corner, but the center is open, take the center
        if (cells[1, 1].current ==  PlayerOption.NONE)
        {
            foreach (var corner in boardCorners) 
            {
                if (cells[corner.x, corner.y].current != PlayerOption.NONE &&
                    cells[corner.x, corner.y].current != currentPlayer)
                {
                    ChooseSpace(1, 1);
                    return;
                }
            
            }

        }
        //If a player controls a corner, but not the center, they should take a cell
        //adjacent to the corner they control
        if (cells[1,1].current != currentPlayer)
        {
            foreach(var corner in boardCorners)
            {
                if (cells[corner.x, corner.y].current == currentPlayer)
                {

                    ChooseSpace(2, 1);
                    return;
                }
            }
        }
        //at this point, the processes of attempting to win/blocking will likely play
        //out and result in a tie.
        var (foundMove, moveX, moveY) = IsAboutToWin();
        if (foundMove)
        {
            ChooseSpace(moveX, moveY);
            return;
        }

        //as a fail-safe, if none of the above happens, take a random cell
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                if(cells[j, i].current == PlayerOption.NONE)
                {
                    //pick first empty cell
                    Debug.Log("Found empty cell");
                    ChooseSpace(j, i);
                    return;
                }
            }
        }


    }

    bool IsBoardEmpty()
    {

        //check the board
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                if (cells[j, i].current != PlayerOption.NONE)
                {
                    return false;
                }

            }
        }
        return true;
    }

    (bool, int, int) IsAboutToWin()
    {
        // sum each row/column based on what's in each cell X = 1, O = -1, blank = 0
        // we have a winner if the sum = 3 (X) or -3 (O)
        int sum = 0;
        int tempEmptyX = -1;
        int tempEmptyY = -1;

        // check rows
        for (int i = 0; i < Rows; i++)
        {
            sum = 0;
            for (int j = 0; j < Columns; j++)
            {
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;
                else if ((cells[j, i].current == PlayerOption.NONE))
                {
                    tempEmptyX = j;
                    tempEmptyY = i;

                }

                sum += value;
            }

            if ((sum == 2 || sum == -2))
            {
                Debug.Log("Place to block [" + tempEmptyX + ", " + tempEmptyY + "]");
                return (true, tempEmptyX, tempEmptyY);
            }
                

        }

        // check columns
        for (int j = 0; j < Columns; j++)
        {
            sum = 0;
            for (int i = 0; i < Rows; i++)
            {
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;
                else if ((cells[j, i].current == PlayerOption.NONE))
                {
                    tempEmptyX = j;
                    tempEmptyY = i;
                }

                sum += value;
            }

            if ((sum == 2 || sum == -2))
            {
                Debug.Log("Place to block [" + tempEmptyX + ", " + tempEmptyY + "]");
                return (true, tempEmptyX, tempEmptyY);
            }

        }

        // check diagonals
        // top left to bottom right
        sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            int value = 0;
            if (cells[i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[i, i].current == PlayerOption.O)
                value = -1;
            else if ((cells[i, i].current == PlayerOption.NONE))
            {
                tempEmptyY = i;
            }

            sum += value;
        }

        if ((sum == 2 || sum == -2))
        {
            Debug.Log("Place to block [" + tempEmptyY + ", " + tempEmptyY + "]");
            return (true, tempEmptyY, tempEmptyY);
        }

        // top right to bottom left
        sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            int value = 0;

            if (cells[Columns - 1 - i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[Columns - 1 - i, i].current == PlayerOption.O)
                value = -1;
            else if ((cells[i, i].current == PlayerOption.NONE))
            {
                tempEmptyY = i;
            }

            sum += value;
        }

        if ((sum == 2 || sum == -2))
        {
            Debug.Log("Place to block [" + tempEmptyY + ", " + tempEmptyY + "]");
            return (true, tempEmptyY, tempEmptyY);
        }

        return (false, -1, -1);
    }

    public PlayerOption Opponent()
    {
        if (currentPlayer == PlayerOption.X)
        {
            return PlayerOption.O;
        }
        else if (currentPlayer == PlayerOption.O)
        {
            return PlayerOption.X;
        }

        return PlayerOption.NONE;
    }

    public void ChooseSpace(int column, int row)
    {
        // can't choose space if game is over
        if (GetWinner() != PlayerOption.NONE)
            return;

        // can't choose a space that's already taken
        if (cells[column, row].current != PlayerOption.NONE)
            return;

        // set the cell to the player's mark
        cells[column, row].current = currentPlayer;

        // update the visual to display X or O
        board.UpdateCellVisual(column, row, currentPlayer);

        // if there's no winner, keep playing, otherwise end the game
        if(GetWinner() == PlayerOption.NONE)
            EndTurn();
        else
        {
            Debug.Log("GAME OVER!");
        }
    }

    public void EndTurn()
    {
        // increment player, if it goes over player 2, loop back to player 1
        currentPlayer += 1;
        if ((int)currentPlayer > 2)
            currentPlayer = PlayerOption.X;
    }

    public PlayerOption GetWinner()
    {
        // sum each row/column based on what's in each cell X = 1, O = -1, blank = 0
        // we have a winner if the sum = 3 (X) or -3 (O)
        int sum = 0;

        // check rows
        for (int i = 0; i < Rows; i++)
        {
            sum = 0;
            for (int j = 0; j < Columns; j++)
            {
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;

                sum += value;
            }

            if (sum == 3)
                return PlayerOption.X;
            else if (sum == -3)
                return PlayerOption.O;

        }

        // check columns
        for (int j = 0; j < Columns; j++)
        {
            sum = 0;
            for (int i = 0; i < Rows; i++)
            {
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;

                sum += value;
            }

            if (sum == 3)
                return PlayerOption.X;
            else if (sum == -3)
                return PlayerOption.O;

        }

        // check diagonals
        // top left to bottom right
        sum = 0;
        for(int i = 0; i < Rows; i++)
        {
            int value = 0;
            if (cells[i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }

        if (sum == 3)
            return PlayerOption.X;
        else if (sum == -3)
            return PlayerOption.O;

        // top right to bottom left
        sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            int value = 0;

            if (cells[Columns - 1 - i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[Columns - 1 - i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }

        if (sum == 3)
            return PlayerOption.X;
        else if (sum == -3)
            return PlayerOption.O;

        return PlayerOption.NONE;
    }
}
