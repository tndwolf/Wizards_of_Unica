using SFML.Graphics;
using SFML.Window;
using tndwolf.Utils;

namespace tndwolf.ECS
{
	/// <summary>
	/// Emits particles inside a box, (x,y) are the top-left coordinates of the box
	/// </summary>
	public class BoxEmitter : Emitter {
		protected float width;
		protected float height;

		public BoxEmitter (float x, float y, float width, float height):
		base(x, y) {
			this.width = width;
			this.height = height;
		}

		public override void Update (ParticleSystem parent, World world)
		{
			if ((StartTime -= world.DeltaTime) < 0) {
				currentTimeMillis += world.DeltaTime;
				TTL -= world.DeltaTime;
				if (currentTimeMillis > SpawnDeltaTime) {
					currentTimeMillis -= SpawnDeltaTime;
					for (int i = 0; i < SpawnCount; i++) {
						var particle = BuildParticle (world, parent);
						particle.ParticlePosition += new Vector2f(
							(float)Services.Rng.NextDouble () * width,
							(float)Services.Rng.NextDouble () * height
						);
						parent.Add (particle);
					}
				}
			}
		}
	}

	/// <summary>
	/// Emits particles in a circular shape around the origin position
	/// </summary>
	public class CircularEmitter : Emitter {
		public CircularEmitter (float x, float y, float maxRadius, float minRadius = 0f) : base (x, y) {
			MaxRadius = maxRadius;
			MinRadius = minRadius;
		}

		public float MaxRadius { get; set; }

		public float MinRadius { get; set; }

		override public void Update (ParticleSystem parent, World world) {
			if ((StartTime -= world.DeltaTime) < 0) {
				currentTimeMillis += world.DeltaTime;
				TTL -= world.DeltaTime;
				if (currentTimeMillis > SpawnDeltaTime) {
					currentTimeMillis -= SpawnDeltaTime;
					for (int i = 0; i < SpawnCount; i++) {
						var particle = BuildParticle (world, parent);
						var rndRadius = (float)Services.Rng.NextDouble ();
						rndRadius = MinRadius + rndRadius * (MaxRadius - MinRadius);
						Services.Logger.Debug("CircularEmitter.Update", "Spawning min radius " + MinRadius);
						Services.Logger.Debug ("CircularEmitter.Update", "Spawning at radius " + rndRadius);
						var rndAngle = Services.Rng.NextDouble () * 6.283;
						particle.ParticlePosition += new Vector2f(
							rndRadius * (float)System.Math.Cos(rndAngle), 
							rndRadius * (float)System.Math.Sin(rndAngle)
						);
						parent.Add (particle);
					}
				}
			}
		}
	}

	/// <summary>
	/// Emits particles from a point
	/// </summary>
	public class Emitter {
		protected RandomList<Color> colors;
		protected int currentTimeMillis = 0;
		protected RandomList<IParticle> templates;
		protected float x;
		protected float y;

		public Emitter (float x, float y) {
			this.x = x;
			this.y = y;
			colors = new RandomList<Color>();
			templates = new RandomList<IParticle>();
		}

		public void AddColor(Color color) {
			colors.Add(color);
		}

		public void AddParticleTemplate(IParticle template) {
			templates.Add(template);
		}

		protected virtual IParticle BuildParticle (World world, ParticleSystem parent)	{
			IParticle res;
			if(templates.Count > 0) {
				res = templates.Random().Clone();
			}
			else {
				res = new PixelParticle();
			}
			if(colors.Count > 0) {
				res.ParticleColor = colors.Random();
			}
			var rndAngle = Services.Rng.NextDouble ();
			rndAngle = rndAngle * MaxAngle + (1f - rndAngle) * MinAngle;
			var rndSpeed = (float)Services.Rng.NextDouble ();
			rndSpeed = rndSpeed * MaxSpeed + (1f - rndSpeed) * MinSpeed;
			res.Velocity = new Vector2f (
				(float)System.Math.Cos (rndAngle) * rndSpeed,
				(float)System.Math.Sin (rndAngle) * rndSpeed
			);
			res.ParticlePosition = new Vector2f(x, y);
			res.TTL = ParticleTTL;
			res.ParentSystem = parent;
			if(res.LightRadius > 1) {
				res.Light = new Light2D(-1);
				res.Light.Parent = parent.LightsLayer;
				res.Light.Color = res.LightColor;
				res.Light.Radius = res.LightRadius;
				world.Add(res.Light);
			}
			return res;
		}

		public float MaxAngle { get; set; }

		public float MaxSpeed { get; set; }

		public float MinAngle { get; set; }

		public float MinSpeed { get; set; }

		public int ParticleTTL { get; set; }

		public int SpawnCount { get; set; }

		public int SpawnDeltaTime { get; set; }

		public int StartTime { get; set; }

		public int TTL { get; set; }

		virtual public void Update (ParticleSystem parent, World world) {
			if ((StartTime -= world.DeltaTime) < 0) {
				currentTimeMillis += world.DeltaTime;
				TTL -= world.DeltaTime;
				if (currentTimeMillis > SpawnDeltaTime) {
					currentTimeMillis -= SpawnDeltaTime;
					for (int i = 0; i < SpawnCount; i++) {
						parent.Add (BuildParticle(world, parent));
					}
				}
			}
		}
	}
}

