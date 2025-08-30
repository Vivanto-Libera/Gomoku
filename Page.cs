using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Threading.Tasks;
using static Tile.State;

public partial class Page : TextureRect
{
	[Signal]
	public delegate void GameOverEventHandler(int whoWin);

	public Tile[,] tiles = new Tile[17, 18];
	public AlphaBeta.Point lastMove = new AlphaBeta.Point(-1, -1);
	public async void CallAIMove()
	{
		await Task.Run(() => AIMove());
	}
	public void AIMove()
	{
		AlphaBeta.Point point = new AlphaBeta(tiles).GetPoint();
		tiles[point.row, point.column].SetState(O);
		lastMove = new AlphaBeta.Point(point.row, point.column);
		Moved(false);
	}
	public void OnPlayerPressed(int row, int column) 
	{
		lastMove = new AlphaBeta.Point(row, column);
		Moved(true);
	}
	public void Moved(bool isPlayer) 
	{
		AlphaBeta.WhoWin who = new AlphaBeta(tiles).JudgeWhoWin(lastMove.row, lastMove.column);
		if (who != AlphaBeta.WhoWin.NotEnd)
		{
			EmitSignal(SignalName.GameOver, (int)who);
			return;
		}
		if (isPlayer)
		{
			setAllDisable(true);
			CallAIMove();
		}
		else
		{
			setAllDisable(false);
		}
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
		public Point(Point point) 
		{
			row = point.row;
			column = point.column;
		}
	}

	int[] BFSX = { 1, 0, -1, 0, 1, 1, -1, -1};
	int[] BFSY = { 0, 1, 0, -1, 1, -1, 1, -1 };
	const int INFINITY = 10000;
	Point point = new Point(-1, -1);
	Point lastMove = new Point(8, 8);
	int maxValue = -INFINITY;
	public Tile.State[,] board = new Tile.State[17, 18];
	private const int DEPTH = 2;

	private int[] RowStone = new int[17];
	private int[] ColumnStone = new int[18];
	private int[] DialogStone1 = new int[24];
	private int[] DialogStone2 = new int[24];

	public AlphaBeta(Tile[,] tiles)
	{
		for (int i = 0; i < 17; i++)
		{
			for (int j = 0; j < 18; j++)
			{
				board[i, j] = tiles[i, j].GetState();
				if (board[i, j] != None) 
				{
					RowStone[i]++;
					ColumnStone[j]++;
					if(i - j <= 11 && j - i <= 12) 
					{
						if(i >= j) 
						{
							DialogStone1[i - j]++;
						}
						else 
						{
							DialogStone1[j - i + 11]++;
						}
					}
					if (i + j >= 5 && i + j <= 28)
					{
						if (i + j >= 17)
						{
							DialogStone2[i + j - 17]++;
						}
						else
						{
							DialogStone2[28 - i - j]++;
						}
					}
				}
			}
		}
	}
	public Point GetPoint()
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
		Queue<Point> points = new Queue<Point>();
		points.Enqueue(new Point(lastMove));
		bool[,] searched = new bool[17, 18];
		while(points.Count != 0) 
		{
			Point aPoint = points.Dequeue();
			if (searched[aPoint.row, aPoint.column]) 
			{
				continue;
			}
			searched[aPoint.row, aPoint.column] = true;
			for(int i = 0; i < 8; i++) 
			{
				Point newPoint = new Point(aPoint.row + BFSX[i], aPoint.column + BFSY[i]);
				if (newPoint.row < 0 || newPoint.row >= 17 || newPoint.column < 0 || newPoint.column >= 18) 
				{
					continue;
				}
				points.Enqueue(newPoint);
			}
			if (board[aPoint.row, aPoint.column] != None)
			{
				continue;
			}
			board[aPoint.row, aPoint.column] = O;
			RowStone[aPoint.row]++;
			ColumnStone[aPoint.column]++;
			int dialogStone1 = 0;
			int dialogStone2 = 0;
			int stone1Num = 0;
			int stone2Num = 0;
			if (aPoint.row - aPoint.column <= 11 && aPoint.column - aPoint.row <= 12)
			{
				if (aPoint.row >= aPoint.column)
				{
					stone1Num = aPoint.row - aPoint.column;
				}
				else
				{
					stone1Num = aPoint.column - aPoint.row + 11;
				}
				DialogStone1[stone1Num]++;
				dialogStone1 = -1;
			}
			if (aPoint.row + aPoint.column >= 5 && aPoint.row + aPoint.column <= 28)
			{
				if (aPoint.row + aPoint.column >= 17)
				{
					stone2Num = aPoint.row + aPoint.column - 17;
				}
				else
				{
					stone2Num = 28 - aPoint.column - aPoint.row;
				}
				DialogStone2[stone2Num]++;
				dialogStone2 = -1;
			}
			lastMove = new Point(aPoint);
			int newV = MinValue(alpha, beta, dep + 1);
			if (isFirst)
			{
				if (newV > maxValue)
				{
					point = new Point(aPoint);
					maxValue = newV;
				}
			}
			if (newV >= beta)
			{
				board[aPoint.row, aPoint.column] = None;
				RowStone[aPoint.row]--;
				ColumnStone[aPoint.column]--;
				DialogStone1[stone1Num] += dialogStone1;
				DialogStone2[stone2Num] += dialogStone2;
			}
			v = newV > v ? newV : v;
			alpha = v > alpha ? v : alpha;
			board[aPoint.row, aPoint.column] = None;
			RowStone[aPoint.row]--;
			ColumnStone[aPoint.column]--;
			DialogStone1[stone1Num] += dialogStone1;
			DialogStone2[stone2Num] += dialogStone2;
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
		Queue<Point> points = new Queue<Point>();
		points.Enqueue(new Point(lastMove));
		bool[,] searched = new bool[17, 18];
		while (points.Count != 0)
		{
			Point aPoint = points.Dequeue();
			if (searched[aPoint.row, aPoint.column])
			{
				continue;
			}
			searched[aPoint.row, aPoint.column] = true;
			for (int i = 0; i < 8; i++)
			{
				Point newPoint = new Point(aPoint.row + BFSX[i], aPoint.column + BFSY[i]);
				if (newPoint.row < 0 || newPoint.row >= 17 || newPoint.column < 0 || newPoint.column >= 18)
				{
					continue;
				}
				points.Enqueue(newPoint);
			}
			if (board[aPoint.row, aPoint.column] != None)
			{
				continue;
			}
			board[aPoint.row, aPoint.column] = X;
			RowStone[aPoint.row]++;
			ColumnStone[aPoint.column]++;
			int dialogStone1 = 0;
			int dialogStone2 = 0;
			int stone1Num = 0;
			int stone2Num = 0;
			if (aPoint.row - aPoint.column <= 11 && aPoint.column - aPoint.row <= 12)
			{
				if (aPoint.row >= aPoint.column)
				{
					stone1Num = aPoint.row - aPoint.column;
				}
				else
				{
					stone1Num = aPoint.column - aPoint.row + 11;
				}
				DialogStone1[stone1Num]++;
				dialogStone1 = -1;
			}
			if (aPoint.row + aPoint.column >= 5 && aPoint.row + aPoint.column <= 28)
			{
				if (aPoint.row + aPoint.column >= 17)
				{
					stone2Num = aPoint.row + aPoint.column - 17;
				}
				else
				{
					stone2Num = 28 - aPoint.column - aPoint.row;
				}
				DialogStone2[stone2Num]++;
				dialogStone2 = -1;
			}
			lastMove = new Point(aPoint);
			int newV = MaxValue(alpha, beta, false, dep + 1);
			if (newV <= alpha)
			{
				board[aPoint.row, aPoint.column] = None;
				RowStone[aPoint.row]--;
				ColumnStone[aPoint.column]--;
				DialogStone1[stone1Num] += dialogStone1;
				DialogStone2[stone2Num] += dialogStone2;
				return newV;
			}
			v = newV < v ? newV : v;
			beta = v < beta ? v : beta;
			board[aPoint.row, aPoint.column] = None;
			RowStone[aPoint.row]--;
			ColumnStone[aPoint.column]--;
			DialogStone1[stone1Num] += dialogStone1;
			DialogStone2[stone2Num] += dialogStone2;
		}
		return v;
	}
	public int Evaluate()
	{
		int v = 0;
		for(int column = 0; column < 18; column++) 
		{
			bool isUpNone = false;
			bool isDownNone = false;
			int columnStone = ColumnStone[column];
			for (int row = 0; row < 17; row++) 
			{
				if(column < 2) 
				{
					break;
				}
				isDownNone = false;
				if (board[row, column] == None) 
				{
					isUpNone = true;
					continue;
				}
				int connect = 1;
				columnStone--;
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
						columnStone--;
					}
					else 
					{
						if (board[row + k, column] == None) 
						{
							isDownNone = true;
						}
						row = row + k - 1;
						v += WhosScore(board[row, column], CountSocore(connect, isUpNone, isDownNone));
						isUpNone = false;
						break;
					}
				}
			}
		}
		for (int row = 0; row < 17; row++)
		{
			bool isLeftNone = false;
			bool isRightNone = false;
			int rowStone = RowStone[row];
			for (int column = 0; column < 18; column++)
			{
				if(rowStone < 2)
				{
					break;
				}
				isRightNone = false;
				if (board[row, column] == None)
				{
					isLeftNone = true;
					continue;
				}
				rowStone--;
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
						rowStone--;
					}
					else
					{
						if (board[row, column + k] == None)
						{
							isRightNone = true;
						}
						column = column + k - 1;
						v += WhosScore(board[row, column], CountSocore(connect, isLeftNone, isRightNone));
						isLeftNone = false;
						break;
					}
				}
			}
		}

		for(int row = 0; row < 12; row++) 
		{
			int column = 0;
			bool isLeftNone = false;
			bool isRightNone = false;
			int dialogStone = DialogStone1[row];
			for (int j = column, i = row; j < 18 && i < 17; j++, i++)
			{
				if (dialogStone < 2)
				{
					break;
				}
				isRightNone = false;
				if (board[i, j] == None)
				{
					isLeftNone = true;
					continue;
				}
				int connect = 1;
				dialogStone--;
				for (int k = 1; k < 5; k++)
				{
					if (j + k > 17 || i + k > 16)
					{
						j = 17;
						i = 16;
						v += WhosScore(board[i, j], CountSocore(connect, isLeftNone, isRightNone));
						break;
					}
					if (board[i, j] == board[i + k, j + k])
					{
						connect++;
						dialogStone--;
					}
					else
					{
						if (board[i + k, j + k] == None)
						{
							isRightNone = true;
						}
						j = j + k - 1;
						i = i + k - 1;
						v += WhosScore(board[i, j], CountSocore(connect, isLeftNone, isRightNone));
						isLeftNone = false;
						break;
					}
				}
			}
		}
		for(int column = 1; column < 13; column++) 
		{
			int row = 0;
			bool isLeftNone = false;
			bool isRightNone = false;
			int dialogStone = DialogStone1[11 + column];
			for (int j = column, i = row; j < 18 && i < 17; j++, i++)
			{
				if(dialogStone < 2) 
				{
					break;
				}
				isRightNone = false;
				if (board[i, j] == None)
				{
					isLeftNone = true;
					continue;
				}
				int connect = 1;
				dialogStone--;
				for (int k = 1; k < 5; k++)
				{
					if (j + k > 17 || i + k > 16)
					{
						j = 17;
						i = 16;
						v += WhosScore(board[i, j], CountSocore(connect, isLeftNone, isRightNone));
						break;
					}
					if (board[i, j] == board[i + k, j + k])
					{
						connect++;
						dialogStone--;
					}
					else
					{
						if (board[i + k, j + k] == None)
						{
							isRightNone = true;
						}
						j = j + k - 1;
						i = i + k - 1;
						v += WhosScore(board[i, j], CountSocore(connect, isLeftNone, isRightNone));
						isLeftNone = false;
						break;
					}
				}
			}
		}

		for (int row = 0; row < 12; row++)
		{
			int column = 17;
			bool isLeftNone = false;
			bool isRightNone = false;
			int dialogStone = DialogStone2[row];
			for (int j = column, i = row; j >= 0 && i < 17; j--, i++)
			{
				if (dialogStone < 2)
				{
					break;
				}
				isRightNone = false;
				if (board[i, j] == None)
				{
					isLeftNone = true;
					continue;
				}
				int connect = 1;
				dialogStone--;
				for (int k = 1; k < 5; k++)
				{
					if (j - k < 0 || i + k > 16)
					{
						j = 0;
						i = 16;
						v += WhosScore(board[i, j], CountSocore(connect, isLeftNone, isRightNone));
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
						j = j - k + 1;
						i = i + k - 1;
						v += WhosScore(board[i, j], CountSocore(connect, isLeftNone, isRightNone));
						isLeftNone = false;
						break;
					}
				}
			}
		}
		for (int column = 17; column >= 6; column--)
		{
			int row = 0;
			bool isLeftNone = false;
			bool isRightNone = false;
			int dialogStone = DialogStone2[28 - column];
			for (int j = column, i = row; j >= 0 && i < 17; j--, i++)
			{
				if (dialogStone < 2)
				{
					break;
				}
				isRightNone = false;
				if (board[i, j] == None)
				{
					isLeftNone = true;
					continue;
				}
				int connect = 1;
				dialogStone--;
				for (int k = 1; k < 5; k++)
				{
					if (j - k < 0 || i + k > 16)
					{
						j = 0;
						i = 16;
						v += WhosScore(board[i, j], CountSocore(connect, isLeftNone, isRightNone));
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
						j = j - k + 1;
						i = i + k - 1;
						v += WhosScore(board[i, j], CountSocore(connect, isLeftNone, isRightNone));
						isLeftNone = false;
						break;
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
			if (column < i || column > 13 + i)
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
			if ((row < i || row > 12 + i) || (column < i || column > 13 + i))
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
			if ((row < 4 - i || row > 16 - i) || (column < i || column > 13 + i))
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
