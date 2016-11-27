using System;
using SFML.Graphics;
using SFML.Window;

namespace tndwolf.ECS
{
	public class Utilities {
		public float Magnitude (Vector2f vector) {
			return (float)Math.Sqrt (vector.X * vector.X + vector.Y * vector.Y);
		}

		public Vector2f Normalize (Vector2f vector) {
			var mn = Magnitude (vector);
			return new Vector2f (vector.X / mn, vector.Y / mn);
		}
	}

	public static class Services {
		public static void Close() {
			Window.Close ();
		}

		public static void Initialize() {
			Logger = new DefaultLogger ();
			Rng = new Random ();
			Window = new RenderWindow (new VideoMode (800, 480), "Wizards of Unica");
			Graphics = new DefaultGraphics();
			Inputs = new DefaultInput (Window);
			Audio = new DefaultAudio ();
			GameFactory = new GameFactory ();
			GameMechanics = new GameMechanics();
			Utilities = new Utilities ();
		}

		public static Audio Audio { get; set; }

		public static GameFactory GameFactory { get; set; }

		public static GameMechanics GameMechanics { get; set; }

		public static Graphics Graphics { get; set; }

		public static DefaultLogger Logger { get; set; }

		public static UserInput Inputs { get; set; }

		public static Random Rng { get; set; }

		public static RenderWindow Window { get; set; }

		public static Utilities Utilities { get; set; }
	}
}

