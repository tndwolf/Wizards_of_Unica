using System.Collections.Generic;
using SFML.Graphics;
using SFML.Window;

namespace tndwolf.ECS
{
	/// <summary>
	/// Interface for the particles managed by a ParticleSystem.
	/// </summary>
	public interface IParticle: Drawable {
		IParticle Clone();
		Light2D Light { get; set; }
		Color LightColor { get; set; }
		int LightRadius { get; set; }
		ParticleSystem ParentSystem { get; set; }
		Color ParticleColor { get; set; }
		Vector2f ParticlePosition { get; set; }
		void ParticleUpdate(World world);
		Vector2f Velocity { get; set; }
		int TTL { get; set; }
	}

	/// <summary>
	/// Default particle (a single pixel).
	/// </summary>
	public class PixelParticle: RectangleShape, IParticle {
		public PixelParticle() {
			FillColor = Color.White;
			Size = new Vector2f(1f, 1f);
			Velocity = new Vector2f();
			Light = null;
		}

		public IParticle Clone() {
			var res = new PixelParticle();
			res.FillColor = FillColor;
			res.LightColor = new Color(LightColor);
			res.LightRadius = LightRadius;
			res.Size = new Vector2f(Size.X, Size.Y);
			res.ParticlePosition = new Vector2f(ParticlePosition.X, ParticlePosition.Y); ;
			res.Velocity = new Vector2f(Velocity.X, Velocity.Y);
			return res;
		}

		public Light2D Light { get; set; }

		public Color LightColor { get; set; }

		public int LightRadius { get; set; }

		public ParticleSystem ParentSystem { get; set; }

		public Color ParticleColor { get { return FillColor; } set { FillColor = value; } }

		public Vector2f ParticlePosition { 
			get { return Position; } 
			set { 
				Position = value;
				if(Light != null) Light.Position = value + ParentSystem.Position;
			} 
		}

		public void ParticleUpdate(World world) {
			TTL -= world.DeltaTime;
		}

		public int TTL { get; set; }
		public Vector2f Velocity { get; set; }
	}

	/// <summary>
	/// Sprite based particle.
	/// </summary>
	public class SpriteParticle: Object2D, IParticle {
		private SpriteParticle(int entity): base(entity) { }

		public SpriteParticle(Object2D original): base(-1) {
			original.Clone(this);
		}

		public IParticle Clone() {
			var res = new SpriteParticle(this);
			res.Light = null;
			res.LightColor = new Color(LightColor);
			res.LightRadius = LightRadius;
			return res;
		}

		public Light2D Light { get; set; }

		public Color LightColor { get; set; }

		public int LightRadius { get; set; }

		public ParticleSystem ParentSystem { get; set; }

		public Color ParticleColor { get { return Color; } set { Color = value; } }

		public Vector2f ParticlePosition {
			get { return Position; }
			set {
				Position = value;
				if(Light != null) Light.Position = value + ParentSystem.Position;
			}
		}

		public void ParticleUpdate(World world) {
			Update(world);
			TTL -= world.DeltaTime;
		}

		public int TTL { get; set; }
		public Vector2f Velocity { get; set; }
	}

	public class ParticleSystem: Widget {
		List<Attractor> attractors = new List<Attractor> ();
		List<Emitter> emitters = new List<Emitter> ();
		public int LightsLayer = -1;
		public int SpawnDelay = 100;
		List<IParticle> particles = new List<IParticle>();

		public ParticleSystem (int entity) : base (entity) {}

		public void Add (Attractor attractor) {
			attractors.Add (attractor);
		}

		public void Add (Emitter emitter) {
			emitters.Add (emitter);
		}

		public void Add(IParticle particle) {
			particles.Add (particle);
		}

		public override void Draw (RenderTarget target, RenderStates states) {
			states.Transform.Translate (Position);
			states.Transform.Scale (Scale);
			foreach (var particle in particles) {
				particle.Draw (target, states);
			}
		}

		public override string ToString (){
			var res = string.Format ("<particleSystem ttl=\"{0}\">", TTL);
			foreach (var emitter in emitters) {
				res += emitter.ToString();
			}
			foreach (var attractor in attractors) {
				res += attractor.ToString ();
			}
			res += "</particleSystem>";
			return res;
		}

		public int TTL { get; set; }

		public override void Update (World world) {
			TTL -= world.DeltaTime;
			if(TTL <= 0) {
				attractors.Clear();
				emitters.Clear();
				foreach(var particle in particles) {
					if(particle.Light != null) {
						//particle.ParticlePosition = new Vector2f(-10000, -10000);
						world.Delete(particle.Light);
					}
				}
				particles.Clear();
				world.Delete(this);
			}
			else {
				foreach(var emitter in emitters) {
					emitter.Update(this, world);
				}
				foreach(var attractor in attractors) {
					attractor.Update(particles, world.DeltaTime);
				}
				foreach(var particle in particles) {
					particle.ParticleUpdate(world);
					if(particle.TTL <= 0 && particle.Light != null) {
						//particle.ParticlePosition = new Vector2f(-10000, -10000);
						world.Delete(particle.Light);
					}
				}
				attractors.RemoveAll((a) => a.TTL <= 0);
				emitters.RemoveAll((e) => e.TTL <= 0);
				particles.RemoveAll((p) => p.TTL <= 0);
			}
		}
	}
}

