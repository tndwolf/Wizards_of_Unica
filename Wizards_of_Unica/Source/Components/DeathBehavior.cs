namespace tndwolf.ECS {
	public class DeathBehavior: GameComponent {
		const int FADE_OUT_FACTOR = 10;
		Object2D sprite;

		public DeathBehavior(int entity): base(entity) { }

		public override void Initialize(World world) {
			world.Unregister(world.GetComponent<TurnActor>(Entity));
			sprite = world.GetComponent<Object2D>(Entity);

			var gridObj = world.GetComponent<GridObject>(Entity);
			if(gridObj != null && sprite != null) {
				world.Delete(gridObj);
				//TODO Custom particle
				Services.GameMechanics.AddParticle("ps_death", gridObj.X, gridObj.Y);
				/*var ps = new ParticleSystem(World.INVALID_ENTITY);
				var em = new CircularEmitter(0f, 0f, 10f);
				em.MaxAngle = 1f;
				em.MinAngle = 0f;
				em.MaxSpeed = 0.2f;
				em.MinSpeed = 0.1f;
				em.ParticleTTL = 200;
				em.SpawnCount = 4;
				em.SpawnDeltaTime = 100;
				em.TTL = 1000;
				var par = new PixelParticle();
				par.LightRadius = 16;
				em.AddParticleTemplate(par);
				em.AddColor(Color.Yellow);
				ps.Add(em);
				var at = new InfiniteAttractor(0f, 0.001f);
				at.TTL = 1000;
				ps.Add(at);
				ps.Parent = 1001;
				var tmpPosition = WorldToPixel(gBuff.X, gBuff.Y);
				ps.Position = new Vector2f(outBuff.X, outBuff.Y);
				ps.TTL = 1000;
				World.Add(ps);*/
			}
		}

		public override void Update(World world) {
			if(sprite != null) {
				var buff = sprite.Color;
				buff.A = (byte)((buff.A < FADE_OUT_FACTOR) ? 0 : buff.A - FADE_OUT_FACTOR);
				sprite.Color = buff;
				sprite.ShadowAlpha = buff.A;
				if(buff.A <= 0) {
					world.Delete(Entity);
				}
			}
		}
	}
}
