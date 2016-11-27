using System;
using SFML.Graphics;
using SFML.Window;

namespace tndwolf.ECS {
	public enum Command {
		RETURN,
		CANCEL,
		TAB,
		QUIT,
		MOVE,
		LEFT_MOUSE,
		RIGHT_MOUSE,
		WHEEL,
		SKILL,
		UNDEFINED
	}

	public interface UserInput {
		Command Command { get; }
		void Clear();
		Vector2i MousePosition { get; }
		int HorizontalMovement { get; }
		string Unicode { get; }
		int VerticalMovement { get; }
		int WheelMovement { get; }
	}

	public class DefaultInput: UserInput {
		public DefaultInput(RenderWindow window) {
			window.Closed += (object sender, EventArgs e) => { 
				window.Close(); Command = Command.QUIT; 
			}; 
			window.KeyPressed += KeyPressed;
			window.KeyReleased += KeyReleased;
			window.MouseButtonReleased += MouseReleased;
			window.MouseWheelMoved += WheelMoved;
			Command = Command.UNDEFINED;
		}

		public void Clear() {
			Command = Command.UNDEFINED;
			WheelMovement = 0;
		}

		public Command Command { get; protected set; }

		public Vector2i MousePosition { get; protected set; }

		public int HorizontalMovement { get; protected set; }

		protected void KeyPressed(object sender, KeyEventArgs e) {
			Unicode = e.Code.ToString();
			switch(e.Code) {
				case Keyboard.Key.A:
					HorizontalMovement = -1;
					Command = Command.MOVE;
					break;
				case Keyboard.Key.D:
					HorizontalMovement = 1;
					Command = Command.MOVE;
					break;
				case Keyboard.Key.W:
					VerticalMovement = -1;
					Command = Command.MOVE;
					break;
				case Keyboard.Key.S:
					VerticalMovement = 1;
					Command = Command.MOVE;
					break;
				case Keyboard.Key.Space:
				case Keyboard.Key.Return:
					Command = Command.RETURN;
					break;
				case Keyboard.Key.Tab:
					Command = Command.TAB;
					break;
				case Keyboard.Key.Escape:
					Command = Command.QUIT;
					break;
				case Keyboard.Key.Num1:
				case Keyboard.Key.Numpad1:
					Command = Command.SKILL;
					Unicode = "1";
					break;
				case Keyboard.Key.Num2:
				case Keyboard.Key.Numpad2:
					Command = Command.SKILL;
					Unicode = "2";
					break;
				case Keyboard.Key.Num3:
				case Keyboard.Key.Numpad3:
					Command = Command.SKILL;
					Unicode = "3";
					break;
				case Keyboard.Key.Num4:
				case Keyboard.Key.Numpad4:
					Command = Command.SKILL;
					Unicode = "4";
					break;
			}
		}

		protected void KeyReleased(object sender, KeyEventArgs e) {
			Command = Command.UNDEFINED;
			switch(e.Code) {
				case Keyboard.Key.A:
				case Keyboard.Key.D:
					HorizontalMovement = 0;
					break;
				case Keyboard.Key.W:
				case Keyboard.Key.S:
					VerticalMovement = 0;
					break;
			}
		}

		protected void MouseReleased(object sender, MouseButtonEventArgs e) {
			switch(e.Button) {
				case Mouse.Button.Left: Command = Command.LEFT_MOUSE; break;
				case Mouse.Button.Right: Command = Command.RIGHT_MOUSE; break;
			}
			MousePosition = new Vector2i(e.X, e.Y);
		}

		public string Unicode { get; protected set; }

		public int VerticalMovement { get; protected set; }

		protected void WheelMoved(object sender, MouseWheelEventArgs e) {
			Command = Command.WHEEL;
			WheelMovement = e.Delta;
			MousePosition = new Vector2i(e.X, e.Y);
		}

		public int WheelMovement { get; protected set; }
	}
}

