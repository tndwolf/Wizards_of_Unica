using System;
namespace tndwolf.ECS {
	public class ShowHideBehaviour: GameComponent {
		const int FADE_OUT_FACTOR = 25;
		Object2D sprite;

		public ShowHideBehaviour(int entity, bool show = true): base(entity) {
			Show = show;
		}

		public bool Hide { get { return !Show; } set { Show = !value; } }

		public override void Initialize(World world) {
			sprite = world.GetComponent<Object2D>(Entity);
			if(sprite == null) DeleteMe = true;
		}

		public bool Show { get; set; }

		public override void Update(World world) {
			if(sprite != null) {
				var buff = sprite.Color;
				if(Show == true) {
					buff.A = (byte)((buff.A + FADE_OUT_FACTOR > 255) ? 255 : buff.A + FADE_OUT_FACTOR);
					if(buff.A == 255) DeleteMe = true;
				}
				else {
					buff.A = (byte)((buff.A < FADE_OUT_FACTOR) ? 0 : buff.A - FADE_OUT_FACTOR);
					if(buff.A == 0) DeleteMe = true;
				}
				sprite.Color = buff;
				sprite.ShadowAlpha = buff.A;
			}
		}
	}
}
