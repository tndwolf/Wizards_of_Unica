using System.Collections.Generic;
using SFML.Graphics;
using SFML.Window;

namespace tndwolf.ECS {
	public class Animation: GameComponent {
		protected struct Frame {
			public string SoundEffect;
			public int DurationMillis;
			public IntRect Sprite;
			public Vector2f Origin;
		}

		List<Frame> frames = new List<Frame>();
		int currentFrame = 0;
		protected int currentTimeMillis = 0;
		protected int nextTimeMillis = 0;

		public Animation(int entity) : base(entity) { }

		public void AddFrame(int u, int v, int width, int height, int durationMillis, Vector2f origin, string sfx = "") {
			var frame = new Frame() {
				Sprite = new IntRect(u, v, width, height),
				DurationMillis = durationMillis,
				Origin = origin,
				SoundEffect = sfx
			};
			frames.Add(frame);
			Duration += durationMillis;
		}

		public bool Blocking { get; set; }

		public Animation Clone() {
			var res = new Animation(Entity);
			res.currentFrame = currentFrame;
			res.currentTimeMillis = currentTimeMillis;
			res.nextTimeMillis = nextTimeMillis;
			foreach(var frame in frames) {
				res.frames.Add(frame);
			}
			return res;
		}

		public int Duration { get; protected set; }

		public bool Loop { get; set; }

		public bool HasEnded { get; protected set; }

		public void Play() {
			currentFrame = 0;
			currentTimeMillis = 0;
			nextTimeMillis = 0;
			HasEnded = false;
			if(frames[currentFrame].SoundEffect != "") {
				Services.GameMechanics.Play(frames[currentFrame].SoundEffect, Entity);
			}
		}

		/// <summary>
		/// Update the animation.
		/// </summary>
		/// <param name="deltaTimeMillis">The milliseconds since the last update.</param>
		/// <param name="sprite">The sprite definition (x, y, wight, height).</param>
		/// <param name="origin">The sprite origin, used as potential offset.</param>
		public void Update(int deltaTimeMillis, out IntRect sprite, out Vector2f origin) {
			currentTimeMillis += deltaTimeMillis;
			if(currentTimeMillis > nextTimeMillis) {
				currentFrame++;
				if(currentFrame >= frames.Count) {
					if(Loop) {
						currentFrame = 0;
						currentTimeMillis -= Duration;
						nextTimeMillis = 0;
					}
					else {
						currentFrame--;
						HasEnded = true;
					}
				}
				if(frames[currentFrame].SoundEffect != "") {
					Services.GameMechanics.Play(frames[currentFrame].SoundEffect, Entity);
				}
				nextTimeMillis += frames[currentFrame].DurationMillis;
			}
			sprite = frames[currentFrame].Sprite;
			origin = frames[currentFrame].Origin;
		}
	}
}

