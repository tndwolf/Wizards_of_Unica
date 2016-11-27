using System;
using SFML.Graphics;
using SFML.Window;

namespace tndwolf.ECS {
	public class DialogueBehavior: GameComponent {
		const int FADE_OUT_FACTOR = 10;
		const int TEXT_SPEED_FACTOR = 50;
		UserInterafaceSystem ui;

		public DialogueBehavior(int entity): base(entity) { }

		public Object2D Icon { get; set; }

		public override void Initialize(World world) {
			TextBox = new TextBox(Entity);
			TextBox.Font = Services.Graphics.DefaultFont;
			TextBox.Size = 16;
			TextBox.Position = new Vector2f(96f, 16f);
			TextBox.BackgroundColor = new Color(33, 60, 59);
			TextBox.DefaultColor = Color.White;
			TextBox.MaxWidth = 316f;
			TextBox.Width = 316f;
			TextBox.Height = 64f;
			world.Add(TextBox);

			ui = world.GetSystem<UserInterafaceSystem>();
			ui.InFocus = TextBox;
			ui.HoldFor = 1000;
			Icon.Animation = "TALK";
			TimePerChar = TEXT_SPEED_FACTOR;
		}

		public override void Update(World world) {
			TimePerChar -= world.DeltaTime;
			if(TimePerChar < 0 && TextIndex < Text.Length) {
				TimePerChar = TEXT_SPEED_FACTOR;
				ui.HoldFor = 1000;
				TextBox.Append(Text.Substring(TextIndex, 1));
				TextIndex++;
			}
			else if(TimePerChar < 0) {
				ui.HoldFor = 0;
				Icon.Animation = Icon.IdleAnimation;
				world.Delete(this);
			}
		}

		public string Text { get; set; }

		public TextBox TextBox { get; set; }

		public int TextIndex { get; set; }

		public int TimePerChar { get; set; }
	}
}
