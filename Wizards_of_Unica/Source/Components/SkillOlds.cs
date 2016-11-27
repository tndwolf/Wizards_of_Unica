using System;
using SFML.Graphics;
using System.Collections.Generic;

namespace tndwolf.ECS {
	public class Skill: IComparable {
		public Skill() { 
			Range = 1;
			Combo = new List<string>();
		}

		public List<string> Combo { get; set; }

		public int CompareTo(object obj) {
			try {
				return (obj as Skill).Priority.CompareTo(Priority);
			}
			catch(Exception ex) {
				Services.Logger.Debug("Skill.CompareTo", "Trying to compare a wrong object" + ex);
				return 0;
			}
		}

		public int CoolDown { get; set; }

		public string Id { get; set; }

		public string Name { get; set; }

		public string OnEmptyScript { get; set; }

		public string OnSelfScript { get; set; }

		public string OnTargetScript { get; set; }

		public string OnUsed { get; set; }

		/*
		public bool OnEmpty(int actor, int gx, int gy) {
			if(OnEmptyScript != null && OnEmptyScript.Count > 0) {
				Services.Logger.Debug("Skill.OnEmpty", actor + " " + OnEmptyScript);
				if(OnEmptyScript[0].Run(actor, -1, gx, gy)) {
					RoundsToGo = CoolDown;
					CurrentCoolDown = CoolDown;
					return true;
				}
				else {
					return false;
				}
			}
			else {
				return false;
			}
		}

		public bool OnSelf(int actor) {
			if(OnSelfScript != null && OnSelfScript.Count > 0) {
				Services.Logger.Debug("Skill.OnSelf", actor + " " + OnSelfScript);
				if(OnSelfScript[0].Run(actor, actor, -1, -1)) {
					RoundsToGo = CoolDown;
					CurrentCoolDown = CoolDown;
					return true;
				}
				else {
					return false;
				}
			}
			else {
				return false;
			}
		}

		public bool OnTarget(int actor, int target) {
			if(OnTargetScript != null && OnTargetScript.Count > 0) {
				Services.Logger.Debug("Skill.OnTarget", actor + " vs " + target);
				var done = false;
				foreach(var behaviour in OnTargetScript) {
					Services.Logger.Debug("Skill.OnTarget", "Running " + behaviour);
					if(behaviour.Run(actor, target, 0, 0) == true) {
						RoundsToGo = CoolDown;
						CurrentCoolDown = CoolDown;
						done = true;
					}
				}
				return done;
			}
			else {
				return false;
			}
		}//*/

		public int Priority { get; set; }

		public int Range { get; set; }

		int roundsToGo = 0;
		public int RoundsToGo {
			get { return roundsToGo; }
			set { roundsToGo = (value < 0) ? 0 : value; }
		}

		public bool Show { get; set; }

		public string Template { get; set; }
	}

	public class SkillOld: IComparable {
		public SkillOld () {
			Range = 1;
			Combo = new List<string>();
		}

		public List<string> Combo { get; set; }

		public int CoolDown { get; set; }

		public int CurrentCoolDown {
			// XXX This way combo spells have no effect on cooldowns
			// XXX remove this to have the cooldown derived from the combo skill
			get { return CoolDown; } 
			set {}
		}

		public string ID { get; set; }

		public string MouseIconTexture { get; set; }

		public IntRect MouseIconRect { get; set; }

		public string Name { get; set; }

		int roundsToGo = 0;

		public int RoundsToGo { 
			get { return roundsToGo; }
			set { roundsToGo = (value < 0) ? 0 : value; }
		}

		public int Priority { get; set; }

		public int Range { get; set; }

		public List<SkillBehaviour> OnEmptyScript { get; set; }

		public List<SkillBehaviour> OnSelfScript { get; set; }

		public List<SkillBehaviour> OnTargetScript { get; set; }

		public bool OnEmpty (int actor, int gx, int gy) {
			if (OnEmptyScript != null && OnEmptyScript.Count > 0) {
				Services.Logger.Debug ("Skill.OnEmpty", actor + " " + OnEmptyScript);
				if (OnEmptyScript[0].Run (actor, -1, gx, gy)) {
					RoundsToGo = CoolDown;
					CurrentCoolDown = CoolDown;
					return true;
				}
				else {
					return false;
				}
			}
			else {
				return false;
			}
		}

		public bool OnSelf (int actor) {
			if (OnSelfScript != null && OnSelfScript.Count > 0) {
				Services.Logger.Debug ("Skill.OnSelf", actor + " " + OnSelfScript);
				if (OnSelfScript[0].Run (actor, actor, -1, -1)) {
					RoundsToGo = CoolDown;
					CurrentCoolDown = CoolDown;
					return true;
				}
				else {
					return false;
				}
			}
			else {
				return false;
			}
		}

		public bool OnTarget (int actor, int target) {
			if (OnTargetScript != null && OnTargetScript.Count > 0) {
				Services.Logger.Debug ("Skill.OnTarget", actor + " vs " + target);
				var done = false;
				foreach (var behaviour in OnTargetScript) {
					Services.Logger.Debug ("Skill.OnTarget", "Running " + behaviour);
					if (behaviour.Run (actor, target, 0, 0) == true) {
						RoundsToGo = CoolDown;
						CurrentCoolDown = CoolDown;
						done = true;
					}
				}
				return done;
				/*if (OnTargetScript[0].Run (actor, target, 0, 0)) {
					RoundsToGo = CoolDown;
					CurrentCoolDown = CoolDown;
					return true;
				}
				else {
					return false;
				}*/
			}
			else {
				return false;
			}
		}

		#region OutputUserInterface
		//internal ButtonIcon OutIcon { get; set; }

		//internal SolidBorder Border { get; set; }

		//internal DamageBarDecorator DamageBar { get; set; }

		public bool Show { get; set; }
		#endregion

		#region IComparable implementation
		public int CompareTo (object obj) {
			try {
				//var comp = obj as Skill;
				return (obj as SkillOld).Priority.CompareTo (Priority);
			}
			catch (Exception ex) {
				Services.Logger.Debug ("OutObject.CompareTo", "Trying to compare a wrong object" + ex.ToString ());
				return 0;
			}
		}
		#endregion
	}

	public class SkillBehaviour {
		public string SelfAnimation { get; set; }

		public string SelfParticle { get; set; }

		public string TargetAnimation { get; set; }

		public string TargetParticle { get; set; }

		virtual public bool Run (int actor, int target, int gx, int gy) {
			Services.Logger.Debug ("SkillScript.Run", "Not doing anything");
			return true;
		}
	}

	public class DamageBehaviour: SkillBehaviour {
		public DamageBehaviour (int damage = 1, string damageType = GameMechanics.DAMAGE_TYPE_UNTYPED) {
			Damage = damage;
			DamageType = damageType;
		}

		public int Damage { get; set; }

		public string DamageType { get; set; }

		override public bool Run (int actor, int target, int gx, int gy) {
			return false;
			/*
			if (actor != null && target != null) {
				//Services.Logger.Debug ("DamageSkillScript", "Run", actor.ID + " vs " + target.ID);
				Simulator.Instance.CreateParticleOn (SelfParticle, actor);
				//Services.Logger.Debug ("DamageSkillScript", "Run", "Actor animation: " + SelfAnimation);
				actor.OutObject.SetAnimation (SelfAnimation);
				Simulator.Instance.CreateParticleOn (TargetParticle, target);
				//Services.Logger.Debug ("DamageSkillScript", "Run", "Target animation: " + TargetAnimation);
				target.OutObject.SetAnimation (TargetAnimation);
				var bonusDamage = 0;
				if (DamageType == GameMechanics.DAMAGE_TYPE_PHYSICAL) {
					bonusDamage += actor.GetVar ("STRENGTH");
				}
				Simulator.Instance.Attack (actor.ID, target.ID, Damage + bonusDamage, DamageType);
				return true;
			}
			else {
				return false;
			}//*/
		}
	}

	public class EffectSkillScript: SkillBehaviour {
		public Effect Effect { get; set; }

		override public bool Run (int actor, int target, int gx, int gy) {
			if (target >= 0) {
				/*Simulator.Instance.CreateParticleOn (SelfParticle, actor);
				actor.OutObject.SetAnimation (SelfAnimation);
				Simulator.Instance.CreateParticleOn (TargetParticle, target);
				target.OutObject.SetAnimation (TargetAnimation);
				Simulator.Instance.AddEffect (target.ID, Effect.Clone);*/
				return true;
			}
			else {
				return false;
			}
		}
	}

	public class SpawnBehaviour: SkillBehaviour {
		internal List<string> templateIds = new List<string> ();

		/// <summary>
		/// Gets or sets a value indicating whether the spawned entity is independent from its parent
		/// (i.e. it does not count against the spawn limit)
		/// </summary>
		/// <value><c>true</c> if independent; otherwise, <c>false</c>.</value>
		public bool Independent { get; set; }

		public bool Loop { get; set; }

		public string SpawnTemplateId { 
			get { return templateIds [Services.Rng.Next (templateIds.Count)]; }
			set { templateIds.Add (value); }
		}

		override public bool Run (int actor, int target, int gx, int gy) {
			return false;
			/*
			if (actor != Simulator.PLAYER_ID && Simulator.Instance.InitiativeCount < GameMechanics.ROUND_LENGTH) {
				return false;
			}
			if (gx < 0 || gy < 0) {
				gx = actor.X;
				gy = actor.Y;
			}
			if (target != null) {
				gx = target.X;
				gy = target.Y;
			}
			if (Loop) {
				Services.Logger.Debug ("SpawnSkillScript.Run", "Searching for old siblings...");
				var siblings = Simulator.Instance.GetByParent (actor.ID);
				if (siblings.Count > 0) {
					Services.Logger.Debug ("SpawnSkillScript.Run", "Killing old sibling " + siblings [0].ID);
					Simulator.Instance.CreateParticleAt ("P_SPAWN", siblings [0].X, siblings [0].Y);
					Simulator.Instance.DestroyObject (siblings [0].ID);
				}
			}
			if (Independent || actor.GetVar("SPAWNS") < actor.GetVar("MAX_SPAWNS")) {
				Services.Logger.Debug ("SpawnSkillScript.Run", "Spawning " + SpawnTemplateId);
				Simulator.Instance.CreateParticleOn (SelfParticle, actor);
				Services.Logger.Debug ("SpawnSkillScript.Run", "Actor animation: " + SelfAnimation);
				actor.OutObject.SetAnimation (SelfAnimation);
				var createdId = Simulator.Instance.CreateObject (SpawnTemplateId, gx, gy);

				var created = Simulator.Instance.GetObject (createdId);
				Simulator.Instance.CreateParticleOn (TargetParticle, created);
				if (created != null && !Independent) {
					created.SpawnedBy = actor.ID;
					actor.ChangeVar ("SPAWNS", 1);
				}
				return true;
			}
			else {
				Services.Logger.Debug ("SpawnSkillScript.Run", "Not spawning " + SpawnTemplateId);
				return false;
			}//*/
		}
	}
}

