using Godot;
using System;

public partial class Main : Node
{
	public Page page = new Page();
	public void GameStart() 
	{
		page = GetNode<Page>("Page");
		GetNode<Button>("First").Show();
		GetNode<Button>("Second").Show();
		page.Reset();
	}
	public override void _Ready()
	{
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
		AIMove();
	}
}
