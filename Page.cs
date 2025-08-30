using Godot;
using System;
using System.ComponentModel;
using System.Data.Common;
using System.Runtime.Intrinsics.Arm;
using static Tile.State;

public partial class Page : TextureRect
{
	Tile[,] tiles = new Tile[17, 18];

    public void CallAIMove()
    {
        CallDeferred(nameof(AIMove));
    }
    public void AIMove()
    {

    }
    public void OnPlayerPressed(int row, int column) 
    {

    }
    public void Reset() 
	{
        for (int i = 0; i < 17; i++)
        {
            char row = (char)((int)'A' + i);
            for (int j = 0; j < 18; j++)
            {
                tiles[i, j].SetState(None);
            }
        }
        setAllDisable(true);
	}
	public void setAllDisable(bool isDisable) 
	{
		for (int i = 0; i < 17; i++)
		{
			char row = (char)((int)'A' + i);
			for (int j = 0; j < 18; j++)
			{
				tiles[i, j].SetButtonDisable(isDisable);
			}
		}
	}
    public override void _Ready()
    {
        for (int i = 0; i < 17; i++)
        {
            char row = (char)((int)'A' + i);
            for (int j = 0; j < 18; j++)
            {
                string TileName = row + j.ToString();
                tiles[i, j] = GetNode<Tile>(TileName);
                tiles[i, j].SetState(Tile.State.None);
                tiles[i, j].Row = i;
                tiles[i, j].Column = j;
                tiles[i, j].PlayerPressed += OnPlayerPressed;
            }
        }
    }
}


public class AlphaBeta
{
    public enum WhoWin
    {
        OWin,
        XWin,
        Draw,
        NotEnd,
    }
    public struct Point 
    {
        public int row;
        public int column;
        public Point(int row1, int column1) 
        {
            row = row1;
            column = column1;
        }
    }

    const int INFINITY = 10000;
    Point point = new Point(-1, -1);
    Point lastMove = new Point();
    int maxValue = -INFINITY;
    public Tile.State[,] board = new Tile.State[17, 18];
    private const int DEPTH = 8;
    public AlphaBeta(Tile[,] tiles)
    {
        for (int i = 0; i < 17; i++)
        {
            for (int j = 0; j < 18; j++)
            {
                board[i, j] = tiles[i, j].GetState();
            }
        }
    }
    public Point GetColumn()
    {
        MaxValue(-INFINITY, INFINITY, true, 0);
        return point;
    }
    private int MaxValue(int alpha, int beta, bool isFirst, int dep)
    {
        WhoWin who = JudgeWhoWin(lastMove.row, lastMove.column);
        if (who == WhoWin.XWin)
        {
            return -5000;
        }
        if (who == WhoWin.Draw)
        {
            return 0;
        }
        if (dep == DEPTH)
        {
            return Evaluate();
        }
        int v = -INFINITY;
        for (int i = 0; i < 17; i++)
        {
            for (int j = 0; j < 18; j++)
            {
                if (board[i, j] != None) 
                {
                    continue;
                }
                board[i, j] = O;
                lastMove = new Point(i, j);
                int newV = MinValue(alpha, beta, dep + 1);
                if (isFirst)
                {
                    if (newV > maxValue)
                    {
                        point = new Point(i, j);
                        maxValue = newV;
                    }
                }
                if (newV >= beta)
                {
                    board[i, j] = None;
                    return newV;
                }
                v = newV > v ? newV : v;
                alpha = v > alpha ? v : alpha;
                board[i, j] = None;
            }
        }
        return v;
    }
    private int MinValue(int alpha, int beta, int dep)
    {
        WhoWin who = JudgeWhoWin(lastMove.row, lastMove.column);
        if (who == WhoWin.OWin)
        {
            return 5000;
        }
        if (who == WhoWin.Draw)
        {
            return 0;
        }
        if (dep == DEPTH)
        {
            return Evaluate();
        }
        int v = INFINITY;
        for (int i = 0; i < 17; i++)
        {
            for (int j = 0; j < 18; j++)
            {
                if (board[i, j] != None)
                {
                    continue;
                }
                board[i, j] = X;
                lastMove = new Point(i, j);
                int newV = MinValue(alpha, beta, dep + 1);
                if (newV <= alpha)
                {
                    board[i, j] = None;
                    return newV;
                }
                v = newV < v ? newV : v;
                beta = v < beta ? v : beta;
                board[i, j] = None;
            }
        }
        return v;
    }
    private int Evaluate()
    {
        int v = 0;
        for(int column = 0; column < 18; column++) 
        {
            bool isUpNone = false;
            bool isDownNone = false;
            for(int row = 0; row < 17; row++) 
            {
                isDownNone = false;
                if (board[row, column] == None) 
                {
                    isUpNone = true;
                    continue;
                }
                int connect = 1;
                for(int k = 1;k < 5; k++) 
                {
                    if(row + k > 16) 
                    {
                        row = 16;
                        v += WhosScore(board[row, column], CountSocore(connect, isUpNone, isDownNone));
                        break;
                    }
                    if (board[row, column] == board[row + k, column]) 
                    {
                        connect++;
                    }
                    else 
                    {
                        if (board[row + k, column] == None) 
                        {
                            isDownNone = true;
                        }
                        row = row + k - 1;
                        isUpNone = false;
                        v += WhosScore(board[row, column], CountSocore(connect, isUpNone, isDownNone));
                        break;
                    }
                }
            }
        }
        for (int row = 0; row < 17; row++)
        {
            bool isLeftNone = false;
            bool isRightNone = false;
            for (int column = 0; column < 18; column++)
            {
                isRightNone = false;
                if (board[row, column] == None)
                {
                    isLeftNone = true;
                    continue;
                }
                int connect = 1;
                for (int k = 1; k < 5; k++)
                {
                    if (column + k > 17)
                    {
                        column = 17;
                        v += WhosScore(board[row, column], CountSocore(connect, isLeftNone, isRightNone));
                        break;
                    }
                    if (board[row, column] == board[row, column + k])
                    {
                        connect++;
                    }
                    else
                    {
                        if (board[row, column + k] == None)
                        {
                            isRightNone = true;
                        }
                        column = column + k - 1;
                        isLeftNone = false;
                        v += WhosScore(board[row, column], CountSocore(connect, isLeftNone, isRightNone));
                        break;
                    }
                }
            }
        }
        for(int column = 0; column < 13; column++) 
        {
            for(int row = 0; row < 12; row++) 
            {
                for (int i = row; i < 17; i++)
                {
                    bool isLeftNone = false;
                    bool isRightNone = false;
                    for (int j = column; j < 18; j++)
                    {
                        isRightNone = false;
                        if (board[i, j] == None)
                        {
                            isLeftNone = true;
                            continue;
                        }
                        int connect = 1;
                        for (int k = 1; k < 5; k++)
                        {
                            if (j + k > 17 || i + k > 16)
                            {
                                j = 17;
                                v += WhosScore(board[row, column], CountSocore(connect, isLeftNone, isRightNone));
                                break;
                            }
                            if (board[i, j] == board[i + k, j + k])
                            {
                                connect++;
                            }
                            else
                            {
                                if (board[i + k, j + k] == None)
                                {
                                    isRightNone = true;
                                }
                                j = j + k - 1;
                                i = i + k - 1;
                                isLeftNone = false;
                                v += WhosScore(board[row, column], CountSocore(connect, isLeftNone, isRightNone));
                                break;
                            }
                        }
                    }
                }
            }
        }
        for (int column = 17; column > 5; column--)
        {
            for (int row = 0; row < 12; row++)
            {
                for (int i = row; i < 17; i++)
                {
                    bool isLeftNone = false;
                    bool isRightNone = false;
                    for (int j = column; j >= 0; j--)
                    {
                        isRightNone = false;
                        if (board[i, j] == None)
                        {
                            isLeftNone = true;
                            continue;
                        }
                        int connect = 1;
                        for (int k = 1; k < 5; k++)
                        {
                            if (j - k < 0 || i + k > 16)
                            {
                                j = 0;
                                v += WhosScore(board[row, column], CountSocore(connect, isLeftNone, isRightNone));
                                break;
                            }
                            if (board[i, j] == board[i + k, j - k])
                            {
                                connect++;
                            }
                            else
                            {
                                if (board[i + k, j - k] == None)
                                {
                                    isRightNone = true;
                                }
                                j = j + k + 1;
                                i = i + k - 1;
                                isLeftNone = false;
                                v += WhosScore(board[row, column], CountSocore(connect, isLeftNone, isRightNone));
                                break;
                            }
                        }
                    }
                }
            }
        }
        return v;
    }
    private int CountSocore(int connect, bool isUpNone, bool isDownNone) 
    {
        if(connect == 2) 
        {
            if (isUpNone && isDownNone) 
            {
                return 3;
            }
            else if(isUpNone || isDownNone) 
            {
                return 1;
            }
            return 0;
        }
        else if(connect == 3) 
        {
            if (isUpNone && isDownNone)
            {
                return 50;
            }
            else if (isUpNone || isDownNone)
            {
                return 5;
            }
            return 0;
        }
        else if (connect == 4)
        {
            if (isUpNone && isDownNone)
            {
                return 300;
            }
            else if (isUpNone || isDownNone)
            {
                return 100;
            }
            return 0;
        }
        return 0;
    }
    private int WhosScore(Tile.State who, int score) 
    {
        if(who == O) 
        {
            return score;
        }
        else 
        {
            return -score;
        }
    }
    public WhoWin JudgeWhoWin(int row, int column)
    {
        for(int i = 0; i < 5; i++) 
        {
            bool isWin = true;
            if(row < i || row > 12 + i) 
            {
                continue;
            }
            for(int j = 4 - i;j >= 0 - i; j--) 
            {
                if(j == 0) 
                {
                    continue;
                }
                if (board[row, column] != board[row + j, column])
                {
                    isWin = false;
                    break;
                }
            }
            if (isWin) 
            {
                return StateToWhoWin(board[row, column]);
            }
        }
        for (int i = 0; i < 5; i++)
        {
            bool isWin = true;
            if (column < i || row > 13 + i)
            {
                continue;
            }
            for (int j = 4 - i; j >= 0 - i; j--)
            {
                if (j == 0)
                {
                    continue;
                }
                if (board[row, column] != board[row, column + j])
                {
                    isWin = false;
                    break;
                }
            }
            if (isWin)
            {
                return StateToWhoWin(board[row, column]);
            }
        }
        for (int i = 0; i < 5; i++)
        {
            bool isWin = true;
            if ((row < i || row > 12 + i) || (column < i || row > 13 + i))
            {
                continue;
            }
            for (int j = 4 - i; j >= 0 - i; j--)
            {
                if (j == 0)
                {
                    continue;
                }
                if (board[row, column] != board[row + j, column + j])
                {
                    isWin = false;
                    break;
                }
            }
            if (isWin)
            {
                return StateToWhoWin(board[row, column]);
            }
        }
        for (int i = 0; i < 5; i++)
        {
            bool isWin = true;
            if ((row < 4 - i || row > 16 - i) || (column < i || row > 13 + i))
            {
                continue;
            }
            for (int j = 4 - i; j >= 0 - i; j--)
            {
                if (j == 0)
                {
                    continue;
                }
                if (board[row, column] != board[row - j, column + j])
                {
                    isWin = false;
                    break;
                }
            }
            if (isWin)
            {
                return StateToWhoWin(board[row, column]);
            }
        }
        for(int i = 0; i < 17; i++) 
        {
            for(int j = 0; j < 18; j++) 
            {
                if (board[i, j] == None) 
                {
                    return WhoWin.NotEnd;
                }
            }
        }
        return WhoWin.Draw;
    }
    public WhoWin StateToWhoWin(Tile.State state)
    {
        if (state == O)
        {
            return WhoWin.OWin;
        }
        else if (state == X)
        {
            return WhoWin.XWin;
        }
        else
        {
            return WhoWin.NotEnd;
        }
    }
}
