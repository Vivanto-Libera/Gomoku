using Godot;
using System;

public partial class Tile : TextureRect
{
	[Signal]
	public delegate void PlayerPressedEventHandler(int row, int column);
	public enum State
	{
		None,
		O,
		X,
	}
	[Export]
	public State state = State.None;
	public int Row;
	public int Column;
	public void SetState(State newState) 
	{
		state = newState;
		switch (newState) 
		{
			case State.None:
				Texture = null;
				break;
			case State.O:
				Texture = GD.Load<Texture2D>("res://Image/O.png");
				SetButtonDisable(true);
				break;
			case State.X:
				Texture = GD.Load<Texture2D>("res://Image/X.png");
				SetButtonDisable(true);
				break;
		}
	}
	public void SetButtonDisable(bool isDisable) 
	{
		GetNode<Button>("Button").SetDeferred(Button.PropertyName.Disabled, isDisable);
	}
	public State GetState()
	{
		return state;
	}
	public void OnButtonPressed() 
	{
		SetState(State.X);
		EmitSignal(SignalName.PlayerPressed, Row, Column);
	}
}
