//Created by vivanto(E-Mail:1425078256@qq.com)(GitHub:Vivanto-Libera)
using Godot;
using System;

public partial class Main : Node
{
	public Page page = new Page();

	public override void _Ready()
	{
		page = GetNode<Page>("Page");
		GameStart();
	}

	public void GameStart() 
	{
		GetNode<Button>("First").Show();
		GetNode<Button>("Second").Show();
		page.Reset();
	}
	public async void OnGameOver(int whoWin)
	{
		page.setAllDisable(true);
		Label message = GetNode<Label>("Message");
		if (whoWin == (int)AlphaBeta.WhoWin.OWin)
		{
			message.Text = "你输了";
		}
		else if (whoWin == (int)AlphaBeta.WhoWin.XWin)
		{
			message.Text = "你赢了";
		}
		else
		{
			message.Text = "平局";
		}
		message.Show();
		await ToSignal(GetTree().CreateTimer(3), Timer.SignalName.Timeout);
		message.Hide();
		GameStart();
	}

	public void OnFirstPressed()
	{
		GetNode<Button>("First").Hide();
		GetNode<Button>("Second").Hide();
		page.setAllDisable(false);
	}
	public void OnSecondPressed()
	{
		GetNode<Button>("First").Hide();
		GetNode<Button>("Second").Hide();
		page.CallAIMove();
	}
}
