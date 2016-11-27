using SFML.Graphics;
using System.Collections.Generic;
using SFML.Window;

namespace tndwolf.ECS {
	public class Object2D: Widget {
		const byte SHADOW_ALPHA = 127;

		Dictionary<string, Animation> animations = new Dictionary<string, Animation>();
		Animation currentAnimation;
		string currentAnimationId;
		CircleShape shadow = new CircleShape();

		public Object2D(int entity) : base(entity) {
			IsManaged = true;
		}

		/// <summary>
		/// Adds an animation definition to the list of possible animations.
		/// </summary>
		/// <param name="animation">Animation.</param>
		/// <param name="id">The animation ID to be used to switch between them.</param>
		public void AddAnimation(Animation animation, string id) {
			animations[id] = animation;
			if(currentAnimation == null) {
				currentAnimation = animation;
				IdleAnimation = id;
				Animation = id;
			}
		}

		/// <summary>
		/// Sets the current animation. When the animation ends (and is not configured
		/// to loop) the animation will be set to the idle one automatically
		/// </summary>
		/// <value>The animation.</value>
		public string Animation {
			set {
				if(animations.ContainsKey(value)) {
					currentAnimation = animations[value];
					currentAnimation.Play();
					currentAnimationId = value;
				}
			}
		}

		/// <summary>
		/// Gets the lowest coordinate of the object relative to the current animation.
		/// </summary>
		/// <value>The bottom.</value>
		public float Bottom {
			get { return Y - Origin.Y + SpriteSheet.TextureRect.Height; }
		}

		public void Clone(Object2D target) {
			target.SpriteSheet = SpriteSheet;
			target.Origin = new Vector2f(Origin.X, Origin.Y);
			target.Position = new Vector2f(Position.X, Position.Y);
			foreach(var pair in animations) {
				target.AddAnimation(pair.Value.Clone(), pair.Key);
			}
			target.IdleAnimation = IdleAnimation;
			target.Animation = IdleAnimation;
			target.SetShadow(shadow.Radius);
		}

		public Color Color {
			get { return SpriteSheet.Color; }
			set { SpriteSheet.Color = value; }
		}

		/// <summary>
		/// Draw the sprite.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="states">States.</param>
		public override void Draw(RenderTarget target, RenderStates states) {
			states.Transform.Translate(Position);
			states.Transform.Scale(Scale);
			shadow.Draw(target, states);
			SpriteSheet.Draw(target, states);
		}

		/// <summary>
		/// Gets or sets the idle animation.
		/// </summary>
		/// <value>The idle animation.</value>
		public string IdleAnimation { get; set; }

		/// <summary>
		/// Returns true if the current animation is blocking.
		/// </summary>
		/// <value><c>true</c> if current animation is blocking; otherwise, <c>false</c>.</value>
		public bool IsAnimationBlocking { 
			get {
				return currentAnimationId != IdleAnimation && currentAnimation.Blocking;
			}
		}

		/// <summary>
		/// Returns true if the current animation is the idle one.
		/// </summary>
		/// <value><c>true</c> if this animation is the idle one; otherwise, 
		/// <c>false</c>.</value>
		public bool IsIdle { get { return currentAnimationId == IdleAnimation; } }

		/// <summary>
		/// By default 2D sprites face right (this is a convention, it depends on the
		/// actual sprite, of course), when mirrored the sprite faces left.
		/// </summary>
		/// <value><c>true</c> if mirrored; otherwise, <c>false</c>.</value>
		public bool Mirror { get; set; }

		/// <summary>
		/// Gets or sets the drawing center of the object.
		/// </summary>
		/// <value>The origin.</value>
		public Vector2f Origin {
			get { return SpriteSheet.Origin; }
			set { SpriteSheet.Origin = value; }
		}

		/// <summary>
		/// Updates the sprite and its animation. When an animation ends 
		/// (and is not configured to loop) the animation will be set to the idle
		/// one automatically.
		/// Called automatically on each World.Update
		/// </summary>
		/// <param name="world">World.</param>
		public override void Update(World world) {
			IntRect rect;
			Vector2f origin;
			currentAnimation.Update(world.DeltaTime, out rect, out origin);
			Origin = origin;
			if(Mirror) {
				SpriteSheet.TextureRect = new IntRect(
					rect.Left + rect.Width,
					rect.Top,
					-rect.Width,
					rect.Height
				);
			}
			else {
				SpriteSheet.TextureRect = rect;
			}
			if(currentAnimation.HasEnded) {
				Animation = IdleAnimation;
			}
			shadow.Position = new Vector2f(0f, SpriteSheet.TextureRect.Height - Origin.Y);
		}

		/// <summary>
		/// Sets the shadow (a semi transparent oval) under the object.
		/// If the radius is less than 0.1 no shadow will be drawn. 
		/// </summary>
		/// <returns>The shadow.</returns>
		/// <param name="radius">Radius.</param>
		public void SetShadow(float radius) {
			if(radius <= 0.1f) {
				shadow.FillColor = Color.Transparent;
			}
			else {
				shadow.FillColor = new Color(0, 0, 0, SHADOW_ALPHA);
				shadow.Radius = radius;
				shadow.Origin = new Vector2f(radius, radius);
				shadow.Scale = new Vector2f(1f, 0.5f);
			}
		}

		/// <summary>
		/// Gets or sets the shadow alpha value (between 0 and SHADOW_ALPHA).
		/// </summary>
		/// <value>The shadow alpha.</value>
		public int ShadowAlpha {
			get { return shadow.FillColor.A; }
			set {
				var a = (value > SHADOW_ALPHA) ? SHADOW_ALPHA : (value < 0) ? 0 : value;
				shadow.FillColor = new Color(0, 0, 0, (byte)a);
			}
		}

		/// <summary>
		/// Gets or sets the sprite sheet.
		/// </summary>
		/// <value>The sprite sheet.</value>
		public Sprite SpriteSheet { get; set; }

		override public string ToString() {
			return string.Format(
				@"<object2d parent=""{1}"" spriteSheet=""{0}"" position=""{2}"">",
				Parent,
				SpriteSheet,
				Position
			);
		}

		#region IComparable implementation
		public override int CompareTo(object obj) {
			try {
				var rhs = obj as Object2D;
				if(rhs == null) {
					return Z.CompareTo((obj as Widget).Z);
				}
				else {
					var comp = Z.CompareTo(rhs.Z);
					if(comp == 0) {
						return Bottom.CompareTo(rhs.Bottom);
					}
					else {
						return comp;
					}
				}
			}
			catch {
				Services.Logger.Warn("Object2D", "Trying to compare to non widget/object2D object");
				return 0;
			}
		}
		#endregion
	}
}

