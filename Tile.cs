using Godot;
using System;

public partial class Tile : TextureRect
{
	public enum State
	{
		None,
		O,
		X,
	}
	private State state = State.None;
	public void SetState(State newState) 
	{
		state = newState;
		switch (newState) 
		{
			case State.None:
				Texture = null;
				break;
			case State.O:
				Texture = GD.Load<Texture2D>("O.png");
				SetButtonDisable(true);
				break;
			case State.X:
				Texture = GD.Load<Texture2D>("X.png");
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
}
