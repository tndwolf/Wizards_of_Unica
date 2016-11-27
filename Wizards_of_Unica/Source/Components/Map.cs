using System.Collections.Generic;
using SFML.Graphics;
using SFML.Window;

namespace tndwolf.ECS {
	public class Map: Widget {
		public Color AllyColor = new Color(0, 255, 0, 127);
		public Color EnemyColor = new Color(255, 0, 0, 127);
		public Color FloorColor = new Color(255, 255, 255, 127);
		public Color PitColor = new Color(255, 0, 255, 0);
		public Color WallColor = new Color(0, 0, 255, 0);
		public const byte MASK_BLOCKS_LOS = 1 << 1;
		public const byte MASK_BLOCKS_MOVEMENT = 1 << 2;
		public const byte MASK_CONTAINS_ENEMY = 1 << 3;
		public const byte MASK_CONTAINS_ALLY = 1 << 4;
		public const byte MASK_IN_LOS = 1 << 7;
		public const byte FLOOR = 0;
		public const byte PIT = MASK_BLOCKS_MOVEMENT;
		public const byte WALL = MASK_BLOCKS_LOS | MASK_BLOCKS_MOVEMENT;
		public const char GLYPH_FLOOR = '.';
		public const char GLYPH_PIT = '_';
		public const char GLYPH_WALL = '#';

		List<byte> map = new List<byte>();

		public Map(int entity) : base(entity) {
			IsManaged = true;
			DrawGrid = false;
		}

		public override void Draw(RenderTarget target, RenderStates states) {
			if(DrawGrid) {
				states.Transform.Translate(Position);
				states.Transform.Scale(Scale);
				for(int y = 0; y < Height; y++) {
					for(int x = 0; x < Width; x++) {
						var tile = map[x + y * Width];
						var shape = new RectangleShape(new Vector2f(28f, 28f));
						shape.Position = new Vector2f(x * 32f + 2f, y * 32f + 2f);
						if((tile & MASK_IN_LOS) != 0) {
							if((tile & MASK_CONTAINS_ENEMY) != 0) shape.FillColor = EnemyColor;
							else if((tile & MASK_CONTAINS_ALLY) != 0) shape.FillColor = AllyColor;
							else if((tile & MASK_BLOCKS_MOVEMENT) != 0) shape.FillColor = PitColor;
							else if((tile & MASK_BLOCKS_LOS) != 0) shape.FillColor = WallColor;
							else shape.FillColor = FloorColor;
							target.Draw(shape, states);
						}
						/*
						switch(tile) {
							case FLOOR: shape.FillColor = FloorColor; break;
							case PIT: shape.FillColor = PitColor; break;
							case WALL: shape.FillColor = WallColor; break;
						}
						target.Draw(shape, states);//*/
					}
				}
			}
		}

		public bool DrawGrid { get; set; }

		public void Dump(int cx, int cy, int radius = 5) {
			var sx = cx - radius;
			var ex = cx + radius;
			var sy = cy - radius;
			var ey = cy + radius;
			for(int y = sy; y < ey; y++) {
				for(int x = sx; x < ex; x++) {
					try {
						switch(map[x + y * Width]) {
							case FLOOR: System.Console.Write(GLYPH_FLOOR); break;
							case PIT: System.Console.Write(GLYPH_PIT); break;
							case WALL: System.Console.Write(GLYPH_WALL); break;
							default: System.Console.Write("?"); break;
						}
					}
					catch { }
				}
				System.Console.Write("\n");
			}
		}

		public byte Get(int x, int y) {
			return map[x + y * Width];
		}

		public int Height { get; protected set; }

		public bool IsOpaque(int x, int y) {
			return (Get(x, y) & MASK_BLOCKS_LOS) != 0;
		}

		public bool IsVisible(int x, int y) {
			return (Get(x, y) & MASK_IN_LOS) != 0;
		}

		public bool IsWalkable(int x, int y) {
			//Services.Logger.Debug("Map.IsWalkable", "Checking " + x + "," + y);
			//Dump(x, y);
			return (Get(x, y) & MASK_BLOCKS_MOVEMENT) == 0;
		}

		public void SetMap(int width, int height, char defTile, string mapString) {
			Services.Logger.Debug("Map.SetMap", "Setting '" + mapString);
			Width = width;
			Height = height;
			map = new List<byte>();
			var def = WALL;
			switch(defTile) {
				case GLYPH_FLOOR: def = FLOOR; break;
				case GLYPH_PIT: def = PIT; break;
			}
			var i = 0;
			foreach(var c in mapString) {
				switch(c) {
					case GLYPH_FLOOR: map.Add(FLOOR); break;
					case GLYPH_PIT: map.Add(PIT); break;
					case GLYPH_WALL: map.Add(WALL); break;
					default:
						map.Add(def);
						Services.Logger.Warn("Map.SetMap", "Found unrecognized glyph '" + c + "', filling with default");
						break;
				}
				i++;
			}
			//Services.Logger.Debug("Map.SetMap", "Added " + i + " tiles vs " + width*height + ", completing...");
			for(int n = i; n < width * height; n++) {
				map.Add(def);
			}
			BuildRenderer(mapString);
		}

		public void Set(int x, int y, byte mask, bool value) {
			if(value) map[x + y * Width] |= mask;
			else map[x + y * Width] &= (byte)(~mask);
		}

		public void SetVisible(int x, int y, bool visible) {
			Set(x, y, MASK_IN_LOS, visible);
		}

		public Vector2i StartCell { get; set; }

		public int Width { get; protected set; }

		#region rule management
		// TODO this shouldn-t be here, probably part of WorldFactory
		public class Rule {
			public struct Condition {
				public int dx;
				public int dy;
				public char value;

				public override string ToString() {
					return string.Format("<condition dx=\"{0}\" dy=\"{1}\" value=\"{2}\"/>", dx, dy, value);
				}
			}

			public int[] x = { 0 };
			public int[] y = { 0 };
			public int dx = 0;
			public int dy = 0;
			public int maxX = 0;
			public int maxY = 0;
			public int w = 0;
			public int h = 0;
			public int z = 0;
			public string texture = "";
			public List<Condition> conditions = new List<Condition>();

			public bool Test(int x, int y, string map, int width) {
				// test all the conditions, as soon as something does not match
				// return false
				foreach(var condition in this.conditions) {
					try {
						if(map[(y + condition.dy) * width + x + condition.dx] != condition.value)
							return false;
					}
					catch {
						return false;
					}
				}
				// everything matches, return true
				return true;
			}

			/// <summary>
			/// Gets the quad represented by the rule at x,y coordinate on the map.
			/// Useful because the rule can represent "blocks" of tiles in a
			/// repeating pattern
			/// </summary>
			/// <returns>The quad.</returns>
			/// <param name="x">The x coordinate on the map.</param>
			/// <param name="y">The y coordinate on the map.</param>
			virtual public int[] GetQuad(int x, int y) {
				return new int[] {
					//this.x[x % this.x.Length],
					(this.x[y % this.x.Length] + x * this.w) /*% this.maxX*/,
					(this.y[x % this.y.Length] + y * this.h) /*% this.maxY*/,
					this.w,
					this.h
				};
			}

			/// <summary>
			/// Gets the x coordinate of the tile sprite in pixel.
			/// Useful because the rule can represent "blocks" of tiles in a
			/// repeating pattern
			/// </summary>
			/// <returns>The x.</returns>
			/// <param name="x">The x coordinate on the map.</param>
			virtual public int GetX(int x, int y) {
				return this.x[y % this.x.Length];
			}

			/// <summary>
			/// Gets the y coordinate of the tile sprite in pixel.
			/// Useful because the rule can represent "blocks" of tiles in a
			/// repeating pattern
			/// </summary>
			/// <returns>The y.</returns>
			/// <param name="y">The y coordinate on the map.</param>
			virtual public int GetY(int x, int y) {
				return this.y[y % this.y.Length];
			}

			public override string ToString() {
				return string.Format(
					"<tile x=\"{0}\" y=\"{1}\" z=\"{8}\" maxX=\"{6}\" maxY=\"{7}\" dx=\"{2}\" dy=\"{3}\" w=\"{4}\" h=\"{5}\" texture=\"{9}\"/>",
					string.Join(" ", x),
					string.Join(" ", y),
					dx,
					dy,
					w,
					h,
					maxX,
					maxY,
					z,
					texture);
			}
		}

		public class GridRule: Rule {
			/// <summary>
			/// Gets the quad represented by the rule at x,y coordinate on the map.
			/// Useful because the rule can represent "blocks" of tiles in a
			/// repeating pattern
			/// </summary>
			/// <returns>The quad.</returns>
			/// <param name="x">The x coordinate on the map.</param>
			/// <param name="y">The y coordinate on the map.</param>
			override public int[] GetQuad(int x, int y) {
				return new int[] {
					(x * this.w) % this.maxX,
					(y * this.h) % this.maxY,
					this.w,
					this.h
				};
			}

			/// <summary>
			/// Gets the x coordinate of the tile sprite in pixel.
			/// Useful because the rule can represent "blocks" of tiles in a
			/// repeating pattern
			/// </summary>
			/// <returns>The x.</returns>
			/// <param name="x">The x coordinate on the map.</param>
			override public int GetX(int x, int y) {
				return this.x[0] + x * this.w;
			}

			/// <summary>
			/// Gets the y coordinate of the tile sprite in pixel.
			/// Useful because the rule can represent "blocks" of tiles in a
			/// repeating pattern
			/// </summary>
			/// <returns>The y.</returns>
			/// <param name="y">The y coordinate on the map.</param>
			override public int GetY(int x, int y) {
				return this.y[0] + y * this.h;
			}

			public override string ToString() {
				return string.Format(
					"<tile type=\"GRID\" x=\"{0}\" y=\"{1}\" z=\"{8}\" maxX=\"{6}\" maxY=\"{7}\" dx=\"{2}\" dy=\"{3}\" w=\"{4}\" h=\"{5}\" texture=\"{9}\"/>",
					string.Join(" ", x),
					string.Join(" ", y),
					dx,
					dy,
					w,
					h,
					maxX,
					maxY,
					z,
					texture);
			}
		}

		public class RowRule: Rule {
			/// <summary>
			/// Gets the quad represented by the rule at x,y coordinate on the map.
			/// Useful because the rule can represent "blocks" of tiles in a
			/// repeating pattern
			/// </summary>
			/// <returns>The quad.</returns>
			/// <param name="x">The x coordinate on the map.</param>
			/// <param name="y">The y coordinate on the map.</param>
			override public int[] GetQuad(int x, int y) {
				return new int[] {
					//this.x[x % this.x.Length],
					(this.x[y % this.x.Length] + x * this.w) % this.maxX,
					(this.y[x % this.y.Length] + y * this.h) /*% this.maxY*/,
					this.w,
					this.h
				};
			}

			/// <summary>
			/// Gets the x coordinate of the tile sprite in pixel.
			/// Useful because the rule can represent "blocks" of tiles in a
			/// repeating pattern
			/// </summary>
			/// <returns>The x.</returns>
			/// <param name="x">The x coordinate on the map.</param>
			override public int GetX(int x, int y) {
				return (this.x[y % this.x.Length] + x * this.w) % this.maxX;
			}

			/// <summary>
			/// Gets the y coordinate of the tile sprite in pixel.
			/// Useful because the rule can represent "blocks" of tiles in a
			/// repeating pattern
			/// </summary>
			/// <returns>The y.</returns>
			/// <param name="y">The y coordinate on the map.</param>
			override public int GetY(int x, int y) {
				return this.y[y % this.y.Length];// + y * this.h;
			}

			public override string ToString() {
				return string.Format(
					"<tile type=\"ROW\" x=\"{0}\" y=\"{1}\" z=\"{8}\" maxX=\"{6}\" maxY=\"{7}\" dx=\"{2}\" dy=\"{3}\" w=\"{4}\" h=\"{5}\" texture=\"{9}\"/>",
					string.Join(" ", x),
					string.Join(" ", y),
					dx,
					dy,
					w,
					h,
					maxX,
					maxY,
					z,
					texture);
			}
		}

		public Dictionary<int, List<Rule>> ruleset = new Dictionary<int, List<Rule>>();

		public void AddRule(Rule rule, int parentLayer) {
			if(this.ruleset.ContainsKey(parentLayer) == false) {
				var rules = new List<Rule>();
				this.ruleset.Add(parentLayer, rules);
			}
			this.ruleset[parentLayer].Add(rule);
			//Services.Logger.Debug("WorldView.AddRule", "Adding Rule: " + rule.ToString() + " to layer " + parentLayer);
		}

		public void BuildRenderer(string dungeon) {
			try {
				// TODO review the whole function according to new architecture
				var namedLayers = new Dictionary<string, int>();
				namedLayers["FLOOR"] = 1002;
				namedLayers["WALL"] = 1001;
				//var namedLayers = new int[]{ 1001 };
				// Setup dungeon
				for(int y = 0; y < Height; y++) {
					for(int x = 0; x < Width; x++) {
						try {
							//Services.Logger.Debug("WorldView.SetDungeon2", "Processing " + x + "," + y + ": " + dungeon[x + y*Width]);
							foreach(var pair in namedLayers) {
								//Services.Logger.Debug("WorldView.SetDungeon2", "Evaluatinjg for " + pair.Key + " vs " + this.ruleset.Keys);
								var tmpRuleset = this.ruleset[pair.Value];
								//var layer = (TiledLayer)this.layers[pair.Value];
								for(var r = 1; r < tmpRuleset.Count; r++) {
									//Services.Logger.Debug("WorldView.SetDungeon2", "Running rule  " + r);
									var rule = tmpRuleset[r];
									if(rule.Test(x, y, dungeon, Width) == true) {
										//Services.Logger.Debug("WorldView.SetDungeon2", "Applying rule " + rule + ": " + dungeon[x + y * Width]);
										var res = new Object2D(Entity);
										res.Position = new Vector2f(x * 32f, y * 32f);
										res.Parent = namedLayers[pair.Key];
										res.SpriteSheet = new Sprite(Services.Graphics.LoadTexture(rule.texture));
										res.Z = rule.z;

										var animation = new Animation(Entity);
												animation.AddFrame(
													rule.GetX(x, y),
													rule.GetY(x, y),
													rule.w,
													rule.h,
													1000,
													new Vector2f(rule.dx, rule.dy),
													""
												);
											res.AddAnimation(animation, "IDLE");

										res.IdleAnimation = "IDLE";
										Services.GameMechanics.Add(res);
										//Services.Logger.Debug("WorldView.SetDungeon2", "Added o2d" + res.ToString());

										//layer.SetTile(x, y, rule.GetX(x, y), rule.GetY(x, y), rule.w, rule.h, rule.dx, rule.dy);
										break;
									}
								}
							}
						}
						catch {
							//Services.Logger.Debug("WorldView.SetDungeon", ex.ToString());
						}
					}
				}
			}
			catch {
				//Services.Logger.Debug("WorldView.SetDungeon", ex.ToString());
			}
		}
		#endregion
	}
}

