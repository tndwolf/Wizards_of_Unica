using System.Collections.Generic;
using SFML.Window;

namespace tndwolf.ECS {
	public abstract class Attractor {
		public int StartTime { get; set; }

		public int TTL { get; set; }

		public virtual void Update(List<IParticle> particles, int deltaTimeMillis) {
			return;
		}
	}

	/// <summary>
	/// This attractor simulates a pulling force at an infinite distance (like gravity).
	/// </summary>
	public class InfiniteAttractor: Attractor {
		public InfiniteAttractor(float accelerationX, float accelerationY) {
			Acceleration = new Vector2f(accelerationX, accelerationY);
		}

		public Vector2f Acceleration { get; set; }

		public override string ToString() {
			var res = string.Format(
				"<infiniteAttractor acceleration=\"{0} {1}\" startTime=\"{2}\" ttl=\"{3}\"/>",
				Acceleration.X,
				Acceleration.Y,
				StartTime,
				TTL
			);
			return res;
		}

		public override void Update(List<IParticle> particles, int deltaTimeMillis) {
			if((StartTime -= deltaTimeMillis) < 0) {
				TTL -= deltaTimeMillis;
				var gravity = new Vector2f(0f, 0.0098f / 1000f);
				foreach(var particle in particles) {
					particle.Velocity += Acceleration * deltaTimeMillis;
					particle.ParticlePosition += particle.Velocity;
				}
			}
		}
	}

	/// <summary>
	/// This attractor simulates a pulling force from a point (like a magnet).
	/// </summary>
	public class PointAttractor: Attractor {
		public PointAttractor(float x, float y, float acceleration) {
			Position = new Vector2f(x, y);
			Acceleration = acceleration;
		}

		public float Acceleration { get; set; }

		public Vector2f Position { get; set; }

		public override string ToString() {
			var res = string.Format(
				"<pointAttractor acceleration=\"{0}\" position=\"{1} {2}\" startTime=\"{3}\" ttl=\"{4}\"/>",
				Acceleration,
				Position.X,
				Position.Y,
				StartTime,
				TTL
			);
			return res;
		}

		public override void Update(List<IParticle> particles, int deltaTimeMillis) {
			if((StartTime -= deltaTimeMillis) < 0) {
				TTL -= deltaTimeMillis;
				foreach(var particle in particles) {
					particle.Velocity += Services.Utilities.Normalize(Position - particle.ParticlePosition) * Acceleration * deltaTimeMillis;
					particle.ParticlePosition += particle.Velocity;
				}
			}
		}
	}
}

