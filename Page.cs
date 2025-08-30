using Godot;
using System;

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

    public void Reset() 
	{
        for (int i = 0; i < 17; i++)
        {
            char row = (char)((int)'A' + i);
            for (int j = 0; j < 18; j++)
            {
                tiles[i, j].SetState(Tile.State.None);
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
                tiles[i, j].PlayerPressed += CallAIMove;
            }
        }
    }
}
