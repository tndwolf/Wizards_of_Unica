using System.Xml;
using tndwolf.Utils;
using System.Collections.Generic;
using SFML.Window;
using SFML.Graphics;
using System.Xml.Serialization;

namespace tndwolf.ECS {
	public class GameFactory: XmlParser {
		/// <summary>
		/// Builds and entity from template and inserts it into the world.
		/// </summary>
		/// <returns>The ID assigned to the newly created entity.</returns>
		/// <param name="world">World.</param>
		/// <param name="templateId">Template identifier.</param>
		/// <param name="entityId">Entity identifier. If the value is negative a new 
		/// entity ID will be created automatically</param>
		public int BuildFromTemplate(World world, string templateId, int entityId = -1) {
			if(entityId < 0) {
				entityId = world.GenerateNewEntityId();
			}
			var template = GetById(templateId);
			var entity = LoadEntity(template, entityId);
			Services.Logger.Debug("GameFactory.BuildFromTemplate", "Built entity " + entityId);
			foreach(var e in entity) {
				world.Add(e);
				Services.Logger.Debug("GameFactory.BuildFromTemplate", e.ToString());
			}
			return entityId;
		}

		/// <summary>
		/// Gets a color from a node attribute or return the default value if none is found.
		/// </summary>
		/// <returns>The color.</returns>
		/// <param name="attribute">Attribute.</param>
		/// <param name="node">Node.</param>
		/// <param name="def">Default value.</param>
		public Color GetColor(string attribute, XmlNode node, Color def) {
			Color res = def;
			var i = GetIntArray(attribute, node);
			if(i.Count > 2) {
				res.R = (byte)i[0];
				res.G = (byte)i[1];
				res.B = (byte)i[2];
				if(i.Count > 3) {
					res.A = (byte)i[3];
				}
			}
			return res;
		}

		/// <summary>
		/// Gets a Time-To-Live. TTL is normally a positive number, if it is
		/// negative then MAXINT is returned *meaning infinite TTL for most
		/// purposes)
		/// </summary>
		/// <returns>The ttl.</returns>
		/// <param name="attribute">Attribute.</param>
		/// <param name="node">Node.</param>
		/// <param name="def">Def.</param>
		public int GetTTL(string attribute, XmlNode node, int def = -1) {
			var res = GetInt(attribute, node, def);
			return (res < 0) ? int.MaxValue : res;
		}

		/// <summary>
		/// Gets a vector2f from a node attribute or return the default value if none is found.
		/// </summary>
		/// <returns>The vector2f.</returns>
		/// <param name="attribute">Attribute.</param>
		/// <param name="node">Node.</param>
		/// <param name="def">Default value.</param>
		public Vector2f GetVector2f(string attribute, XmlNode node, Vector2f def) {
			var res = def;
			var i = GetFloatArray(attribute, node);
			if(i.Count > 1) {
				res.X = i[0];
				res.Y = i[1];
			}
			Services.Logger.Debug("GameFactory.GetVector2f", res + " from " + GetString(attribute, node));
			return res;
		}

		public void LoadEmitterColors(XmlNode node, Emitter emitter) {
			var templates = GetNodeArray("color", node);
			foreach(var template in templates) {
				emitter.AddColor(GetColor("value", template, Color.White));
			}
		}

		/// <summary>
		/// Loads an entity from its XML description. The returned entity is a collection
		/// of components that still need to be added to the world
		/// </summary>
		/// <returns>The collection of components making up an entity.</returns>
		/// <param name="xEntity">XML description.</param>
		/// <param name="entityId">Entity identifier.</param>
		public List<GameComponent> LoadEntity(XmlNode xEntity, int entityId = -1) {
			if(entityId < 0) {
				entityId = GetInt("id", xEntity);
			}
			var res = new List<GameComponent>();
			var template = GetString("template", xEntity);
			if(template != "") {
				var xTemplate = GetById(template);
				res.AddRange(LoadEntity(xTemplate, entityId));
			}
			var components = xEntity.ChildNodes;
			for(var i = 0; i < components.Count; i++) {
				var component = components[i];
				var tags = GetStringArray("tags", component);
				switch(component.Name) {
					// First thing to do for a new case it to check if a component
					// was already created from the template persing, if not a new
					// one is created, then it is populated
					case "camera2d":
						var c2dBuff = new Camera2D(entityId);
						c2dBuff.LookAt = GetInt("lookAt", component);
						c2dBuff.Parent = GetInt("parent", component);
						c2dBuff.Tags = tags;
						res.Add(c2dBuff);
						break;
					case "GridObject":
						var ggoBuff = (res.Find((c) => c is GridObject) == null)
							? new GridObject(entityId)
							: res.Find((c) => c is GridObject) as GridObject;
						ggoBuff.IsDressing = GetBool("IsDressing", component);
						ggoBuff.IsTrigger = GetBool("IsTrigger", component);
						if (ggoBuff.IsTrigger) ggoBuff.OnEnter = GetNode("OnEnter", component).InnerText;
						var ggoPosition = GetIntArray("position", component);
						if(ggoPosition.Count > 1)
							ggoBuff.SetPosition(ggoPosition[0], ggoPosition[1]);
						ggoBuff.Tags = tags;
						res.Add(ggoBuff);
						break;
					case "layer":
						var laBuff = new Layer(entityId);
						LoadWidget(component, laBuff as Widget);
						laBuff.Parent = GetInt("parent", component);
						laBuff.ClearColor = GetColor("clearColor", component, laBuff.ClearColor);
						switch(GetString("blendMode", component)) {
							case "ADD": laBuff.BlendMode = BlendMode.Add; break;
							case "MULTIPLY": laBuff.BlendMode = BlendMode.Multiply; break;
							default: laBuff.BlendMode = BlendMode.Alpha; break;
						}
						laBuff.Tags = tags;
						res.Add(laBuff);
						break;
					case "LevelActor":
						var lvlBuff = LoadLevelActor(component, entityId);
						lvlBuff.Tags = tags;
						res.Add(lvlBuff);
						break;
					case "light2d":
						var l2dBuff = new Light2D(entityId);
						l2dBuff.Color = GetColor("color", component, Color.White);
						var l2lookAt = GetInt("lookAt", component, -1);
						l2dBuff.LookAt = (l2lookAt < 0) ? entityId : l2lookAt;
						l2dBuff.Parent = GetInt("parent", component);
						l2dBuff.Radius = GetInt("radius", component);
						l2dBuff.Tags = tags;
						res.Add(l2dBuff);
						break;
					case "map":
						var mapBuff = (res.Find((c) => c is Map) == null)
							? new Map(entityId)
							: res.Find((c) => c is Map) as Map;
						LoadWidget(component, mapBuff);
						mapBuff.SetMap(
							GetInt("width", component),
							GetInt("height", component),
							GetString("default", component)[0],
							GetString("map", component)
						);
						mapBuff.Tags = tags;
						res.Add(mapBuff);
						break;
					case "object2d":
						var o2dBuff = (res.Find((c) => c is Object2D) == null)
							? LoadObject2D(component, entityId)
							: res.Find((c) => c is Object2D) as Object2D;
						LoadWidget(component, o2dBuff);
						o2dBuff.Tags = tags;
						res.Add(o2dBuff);
						break;
					case "particleSystem":
						var psBuff = (res.Find((c) => c is ParticleSystem) == null)
								? LoadParticleSystem(component, entityId)
								: res.Find((c) => c is ParticleSystem) as ParticleSystem;
						LoadWidget(component, psBuff);
						psBuff.Tags = tags;
						res.Add(psBuff);
						Services.Logger.Debug("GameFactory.LoadEntity", "Created ps: " + psBuff.ToString());
						break;
					case "polygon2d":
						var p2dBuff = new Polygon2D(entityId);
						p2dBuff.DefaultColor = GetColor("defaultColor", component, Color.White);
						p2dBuff.LookAt = GetInt("lookAt", component);
						p2dBuff.Parent = GetInt("parent", component);
						var vertices = GetNodeArray("vertex", component);
						foreach(var xVertex in vertices) {
							p2dBuff.AddVertex(
								GetVector2f("position", xVertex, new Vector2f()),
								GetColor("color", xVertex, p2dBuff.DefaultColor)
							);
						}
						p2dBuff.Tags = tags;
						res.Add(p2dBuff);
						break;
					case "TurnActor":
						var taBuff = (res.Find((c) => c is TurnActor) == null)
							? LoadTurnActor(component, entityId)
							: res.Find((c) => c is TurnActor) as TurnActor;
						taBuff.Tags = tags;
						res.Add(taBuff);
						break;
					case "tiles2d":
						var g2dBuff = (res.Find((c) => c is Tiles2D) == null)
							? LoadGrid2D(component, entityId)
							: res.Find((c) => c is Tiles2D) as Tiles2D;
						LoadWidget(component, g2dBuff);
						g2dBuff.GridWidth = GetInt("gridWidth", component, 1);
						g2dBuff.GridHeight = GetInt("gridHeight", component, 1);
						g2dBuff.Tags = tags;
						res.Add(g2dBuff);
						break;
				}
			}
			return res;
		}

		/// <summary>
		/// Loads an Object2D component from its XML description.
		/// The component still needs to be added to the world
		/// </summary>
		/// <returns>The component.</returns>
		/// <param name="node">XML description.</param>
		/// <param name="entityId">Entity identifier.</param>
		public Object2D LoadObject2D(XmlNode node, int entityId) {
			var res = new Object2D(entityId);
			LoadWidget(node, res as Widget);
			res.SpriteSheet = new Sprite(
				Services.Graphics.LoadTexture(GetString("spriteSheet", node))
			);
			res.Origin = GetVector2f("origin", node, new Vector2f());
			var animations = GetNodeArray("animation", node);
			foreach(var xAnimation in animations) {
				var animation = new Animation(entityId);
				animation.Blocking = GetBool("blocking", xAnimation);
				animation.Loop = GetBool("loop", xAnimation);
				var xFrames = GetNodeArray("frame", xAnimation);
				foreach(var xFrame in xFrames) {
					animation.AddFrame(
						GetInt("x", xFrame),
						GetInt("y", xFrame),
						GetInt("width", xFrame),
						GetInt("height", xFrame),
						GetInt("duration", xFrame),
						GetVector2f("origin", xFrame, res.Origin),
						GetString("sfx", xFrame)
					);
				}
				res.AddAnimation(animation, GetString("name", xAnimation));
			}
			res.IdleAnimation = GetString("idleAnimation", node, "IDLE");
			var createAnimation = GetString("onCreate", node, "");
			if(createAnimation != "") res.Animation = createAnimation;
			res.SetShadow(GetFloat("shadow", node));
			return res;
		}

		/// <summary>
		/// Loads an Object2D component from its XML description.
		/// The component still needs to be added to the world
		/// </summary>
		/// <returns>The component.</returns>
		/// <param name="node">XML description.</param>
		/// <param name="entityId">Entity identifier.</param>
		public Tiles2D LoadGrid2D(XmlNode node, int entityId) {
			var res = new Tiles2D(entityId);
			LoadWidget(node, res as Widget);
			res.SpriteSheet = new Sprite(
				Services.Graphics.LoadTexture(GetString("spriteSheet", node))
			);
			var animations = GetNodeArray("animation", node);
			foreach(var xAnimation in animations) {
				var animation = new Animation(entityId);
				animation.Blocking = GetBool("blocking", xAnimation);
				animation.Loop = GetBool("loop", xAnimation);
				var xFrames = GetNodeArray("frame", xAnimation);
				foreach(var xFrame in xFrames) {
					animation.AddFrame(
						GetInt("x", xFrame),
						GetInt("y", xFrame),
						GetInt("width", xFrame),
						GetInt("height", xFrame),
						GetInt("duration", xFrame),
						GetVector2f("origin", xFrame, res.Origin),
						GetString("sfx", xFrame)
					);
				}
				res.AddAnimation(animation, GetString("name", xAnimation));
			}
			res.IdleAnimation = GetString("idleAnimation", node, "IDLE");
			var createAnimation = GetString("onCreate", node, "");
			if(createAnimation != "") res.Animation = createAnimation;
			return res;
		}

		public LevelActor LoadLevelActor(XmlNode node, int entityId) {
			var serializer = new XmlSerializer(typeof(LevelActor));
			var res = (LevelActor)serializer.Deserialize(new XmlNodeReader(node));
			//*
			var builder = new System.Text.StringBuilder();
			var setting = new XmlWriterSettings();
			setting.OmitXmlDeclaration = true;
			using(var writer = XmlWriter.Create(builder, setting)) {
				serializer.Serialize(writer, res);
				Services.Logger.Error("+++++++++++++++++++++++++", builder.ToString());
			}//*/
			return res;
		}

		public ParticleSystem LoadParticleSystem(XmlNode node, int entityId) {
			var res = new ParticleSystem(entityId);
			LoadWidget(node, res as Widget);
			res.LightsLayer = GetInt("lightsLayer", node, -1);
			res.TTL = GetTTL("ttl", node);
			var components = node.ChildNodes;
			for(var i = 0; i < components.Count; i++) {
				var component = components[i];
				switch(component.Name) {
					case "emitter":
						var eBuff = GetFloatArray("position", component);
						var emitter = new Emitter(eBuff[0], eBuff[1]);
						emitter.MaxAngle = GetFloat("maxAngle", component, 6.283f);
						emitter.MinAngle = GetFloat("minAngle", component, 0f);
						emitter.MaxSpeed = GetFloat("maxSpeed", component, 0f);
						emitter.MinSpeed = GetFloat("minSpeed", component, 0f);
						emitter.ParticleTTL = GetTTL("particleTtl", component);
						emitter.SpawnCount = GetInt("spawnCount", component);
						emitter.SpawnDeltaTime = GetInt("spawnDeltaTime", component);
						emitter.StartTime = GetInt("startTime", component);
						emitter.TTL = GetTTL("ttl", component);
						LoadParticleTemplate(component, emitter);
						LoadEmitterColors(component, emitter);
						res.Add(emitter);
						break;
					case "boxEmitter":
						var beBuff = GetFloatArray("position", component);
						var bemitter = new BoxEmitter(
							beBuff[0],
							beBuff[1],
							GetFloat("width", component),
							GetFloat("height", component)
						);
						bemitter.MaxAngle = GetFloat("maxAngle", component, 6.283f);
						bemitter.MinAngle = GetFloat("minAngle", component, 0f);
						bemitter.MaxSpeed = GetFloat("maxSpeed", component, 0f);
						bemitter.MinSpeed = GetFloat("minSpeed", component, 0f);
						bemitter.ParticleTTL = GetTTL("particleTtl", component);
						bemitter.SpawnCount = GetInt("spawnCount", component);
						bemitter.SpawnDeltaTime = GetInt("spawnDeltaTime", component);
						bemitter.StartTime = GetInt("startTime", component);
						bemitter.TTL = GetTTL("ttl", component);
						LoadParticleTemplate(component, bemitter);
						LoadEmitterColors(component, bemitter);
						res.Add(bemitter);
						break;
					case "circularEmitter":
						var ceBuff = GetFloatArray("position", component);
						var cemitter = new CircularEmitter(
							ceBuff[0],
							ceBuff[1],
							GetFloat("maxRadius", component),
							GetFloat("minRadius", component)
						);
						cemitter.MaxAngle = GetFloat("maxAngle", component, 6.283f);
						cemitter.MinAngle = GetFloat("minAngle", component, 0f);
						cemitter.MaxSpeed = GetFloat("maxSpeed", component, 0f);
						cemitter.MinSpeed = GetFloat("minSpeed", component, 0f);
						cemitter.ParticleTTL = GetTTL("particleTtl", component);
						cemitter.SpawnCount = GetInt("spawnCount", component);
						cemitter.SpawnDeltaTime = GetInt("spawnDeltaTime", component);
						cemitter.StartTime = GetInt("startTime", component);
						cemitter.TTL = GetTTL("ttl", component);
						LoadParticleTemplate(component, cemitter);
						LoadEmitterColors(component, cemitter);
						res.Add(cemitter);
						break;
					case "infiniteAttractor":
						var aBuff = GetFloatArray("acceleration", component);
						var iattractor = new InfiniteAttractor(aBuff[0], aBuff[1]);
						iattractor.StartTime = GetInt("startTime", component);
						iattractor.TTL = GetTTL("ttl", component);
						res.Add(iattractor);
						break;
					case "pointAttractor":
						var pBuff = GetFloatArray("position", component);
						var pattractor = new PointAttractor(pBuff[0], pBuff[1], GetFloat("acceleration", component));
						pattractor.StartTime = GetInt("startTime", component);
						pattractor.TTL = GetTTL("ttl", component);
						res.Add(pattractor);
						break;
				}
			}
			return res;
		}

		public void LoadParticleTemplate(XmlNode node, Emitter emitter) {
			var templates = GetNodeArray("particleTemplate", node);
			foreach(var template in templates) {
				switch(GetString("type", template)) {
					case "PIXEL":
						var pt = new PixelParticle();
						pt.LightColor = GetColor("lightColor", template, Color.Transparent);
						pt.LightRadius = GetInt("lightRadius", template);
						emitter.AddParticleTemplate(pt);
						break;
					case "SPRITE":
						var o2d = new SpriteParticle(LoadObject2D(template, -1));
						o2d.LightColor = GetColor("lightColor", template, Color.Transparent);
						o2d.LightRadius = GetInt("lightRadius", template);
						emitter.AddParticleTemplate(o2d);
						break;
				}
			}
		}

		public TurnActor LoadTurnActor(XmlNode node, int entityId) {
			/*
			var serializer = new XmlSerializer(typeof(TurnActor));
			var res = (TurnActor)serializer.Deserialize(new XmlNodeReader(node));
			res.Entity = entityId;

			//*
			var builder = new System.Text.StringBuilder();
			var setting = new XmlWriterSettings();
			setting.OmitXmlDeclaration = true;
			using(var writer = XmlWriter.Create(builder, setting)) {
				serializer.Serialize(writer, res);
				Services.Logger.Error("+++++++++++++++++++++++++", builder.ToString());
			}//*/

			var res = new TurnActor(entityId);
			res.Faction = GetString("Faction", node, "");
			res.Health = GetInt("Health", node, 1);
			res.MaxHealth = GetInt("Health", node, 1);
			res.Speed = GetInt("Speed", node, GameMechanics.ROUND_LENGTH);
			//*
			var xSkills = GetNodeArray("skill", node);
			foreach(var xSkill in xSkills) {
				var xSkillDefinition = GetById(GetString("ref", xSkill));
				var skill = new Skill();
				skill.Combo = GetStringArray("Combo", xSkillDefinition);
				skill.CoolDown = GetInt("cooldown", xSkillDefinition);
				skill.Id = GetString("id", xSkillDefinition);
				skill.Name = GetString("name", xSkillDefinition);
				skill.OnUsed = xSkillDefinition.InnerText;
				skill.Priority = GetInt("priority", xSkillDefinition);
				skill.Range = GetInt("range", xSkillDefinition);
				skill.Show = GetBool("show", xSkillDefinition);
				skill.Template = GetString("template", xSkillDefinition);
				res.Skills.Add(skill);
			}
			var onRoundNode = GetNode("OnRound", node);
			if(onRoundNode != null && onRoundNode.InnerText != "") {
				res.OnRound = onRoundNode.InnerText;
			}//*/
			return res;
		}

		/// <summary>
		/// Loads a scene from its template and adds its components to the world
		/// </summary>
		/// <param name="world">World.</param>
		/// <param name="sceneId">Scene template identifier.</param>
		public void LoadScene(World world, string sceneId) {
			world.Clear(new System.Action(()=>InnerLoadScene(world, sceneId)));
		}
		public void InnerLoadScene(World world, string sceneId) {
			//Services.Logger.Error("GameFactory.InnerLoadScene", "Starting \n" + Root.OuterXml);
			var scene = GetById(sceneId);
			var entities = GetNodeArray("entity", scene);
			foreach(var xEntity in entities) {
				Services.Logger.Error("GameFactory.InnerLoadScene", "Loading entity " + xEntity);
				var entity = LoadEntity(xEntity);
				foreach(var e in entity) {
					world.Add(e);
				}
			}
			var onLoad = GetNode("onLoad", scene);
			if(onLoad != null) {
				world.Diagnose();
				Services.GameMechanics.Execute(onLoad.InnerText);
			}
			Services.Logger.Error("GameFactory.InnerLoadScene", "Finished");
		}

		/// <summary>
		/// Loads the base attributes of a widget from its XML description.
		/// The component still needs to be added to the world
		/// </summary>
		/// <param name="node">XML description.</param>
		/// <param name="res">Res.</param>
		public void LoadWidget(XmlNode node, Widget res) {
			if(res.Parent == 0) res.Parent = GetInt("parent", node);
			if(res.Z == 0) res.Z = GetInt("z", node);
			var position = GetIntArray("position", node);
			if(position.Count > 1)
				res.Position = new Vector2f(position[0], position[1]);
			var scale = GetFloatArray("scale", node);
			if(scale.Count > 1)
				res.Scale = new Vector2f(scale[0], scale[1]);
		}
	}
}

