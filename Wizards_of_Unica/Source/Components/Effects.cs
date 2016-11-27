// Wizard's Duel, a procedural tactical RPG
// Copyright (C) 2015  Luca Carbone
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace tndwolf.ECS {
	public class Effect {
		public readonly string ID = "NULL_EFFECT";
		public const int INFINITE_DURATION = 999999999;
		protected int lastInitiative = 0;

		virtual public Effect Clone {
			get {
				var res = new Effect();
				res.lastInitiative = lastInitiative;
				res.Duration = Duration;
				return res;
			}
		}

		public int Duration { get; set; }

		/// <summary>
		/// Initialize the effect, setting the initiative counter
		/// It should be called by all its derivates
		/// </summary>
		virtual public void OnAdded() {
			//TODO lastInitiative = Simulator.Instance.InitiativeCount;
		}

		virtual public void OnRemoved() {
			return;
		}

		/// <summary>
		/// It should be called by all its derivates to update the duration and clear the
		/// effect if the duration is expired
		/// </summary>
		virtual public void OnRound() {
			Services.Logger.Debug("Effect.OnRound", "Old duration " + Duration);
			//Duration -= Simulator.Instance.InitiativeCount - lastInitiative;
			//lastInitiative = Simulator.Instance.InitiativeCount;
			Services.Logger.Debug("Effect.OnRound", "Updated duration " + Duration);
			if(Duration < 1) {
				Services.Logger.Debug("Effect.OnRound", "Set to be removed from " + Parent);
				RemoveMe = true;
			}
		}

		public int Parent { get; internal set; }

		virtual public int ProcessDamage(int howMuch, string type) {
			return howMuch;
		}

		public bool RemoveMe { get; protected set; }

		public int Target { get; set; }
	}

	public class BurningEffect: Effect {
		new public readonly string ID = "BURN_EFFECT";

		public BurningEffect(int duration = GameMechanics.ROUND_LENGTH * 4, int strength = 1) {
			Duration = duration + 1;
			Strength = strength;
		}

		override public Effect Clone {
			get {
				var res = new BurningEffect();
				res.lastInitiative = lastInitiative;
				res.Duration = Duration;
				res.Strength = Strength;
				return res;
			}
		}

		override public void OnAdded() {
			base.OnAdded();
			//Parent.OutObject.Color = new SFML.Graphics.Color(255, 127, 127);
			//Simulator.Instance.CreateParticleOn("p_burning", Parent);
		}

		override public void OnRemoved() {
			//Parent.OutObject.Color = SFML.Graphics.Color.White;
			//Simulator.Instance.RemoveParticle(Parent, "p_burning");
		}

		override public void OnRound() {
			base.OnRound();
			//Parent.Damage(Strength, GameMechanics.DAMAGE_TYPE_FIRE);
		}

		public int Strength { get; set; }
	}

	public class FreezeEffect: Effect {
		new public readonly string ID = "FREEZE_EFFECT";

		public FreezeEffect(int duration = GameMechanics.ROUND_LENGTH * 4) {
			Duration = duration + 1;
		}

		override public Effect Clone {
			get {
				var res = new FreezeEffect();
				res.lastInitiative = lastInitiative;
				res.Duration = Duration;
				return res;
			}
		}

		override public void OnAdded() {
			base.OnAdded();
			//Parent.Frozen = true;
			//Parent.OutObject.StopAnimation = true;
			//Parent.OutObject.Color = new SFML.Graphics.Color(127, 255, 255);
			//Simulator.Instance.CreateParticleOn("p_freeze", Parent);
		}

		override public void OnRemoved() {
			//Parent.Frozen = false;
			//Parent.OutObject.StopAnimation = false;
			//Parent.OutObject.Color = SFML.Graphics.Color.White;
			//Simulator.Instance.RemoveParticle(Parent, "p_freeze");
		}
	}

	public class GuardEffect: Effect {
		new public string ID = "GUARD_EFFECT";

		public GuardEffect() {
			Duration = GameMechanics.ROUND_LENGTH * 3 + 1;
			Strength = 1;
			Type = GameMechanics.DAMAGE_TYPE_UNTYPED;
		}

		override public Effect Clone {
			get {
				var res = new GuardEffect();
				res.lastInitiative = lastInitiative;
				res.Duration = Duration;
				res.Strength = Strength;
				res.Type = Type;
				return res;
			}
		}

		override public void OnAdded() {
			base.OnAdded();
			Services.Logger.Debug("GuardEffect.OnAdded", "Adding effect to " + Parent);
			//Parent.OutObject.SetAnimation("CAST1");
			//Simulator.Instance.CreateParticleOn("p_truce", Parent);
		}

		override public void OnRemoved() {
			Services.Logger.Debug("GuardEffect.OnRemoved", "Removing effect to " + Parent);
			//Simulator.Instance.RemoveParticle(Parent, "p_truce");
		}

		override public int ProcessDamage(int howMuch, string type) {
			if(Type == GameMechanics.DAMAGE_TYPE_UNTYPED || type == Type) {
				howMuch -= Strength;
			}
			Services.Logger.Debug("GuardEffect.ProcessDamage", "Reducing damage to " + Parent + " to " + howMuch.ToString());
			return (howMuch < 0) ? 0 : howMuch;
		}

		public int Strength { get; set; }

		private string type;
		public string Type {
			get { return type; }
			set {
				type = value;
				ID = "GUARD_EFFECT_" + type;
			}
		}
	}

	public class VulnerableEffect: Effect {
		new public string ID = "VULNERABLE_EFFECT";

		public VulnerableEffect(float multiplier, string toDamageType) {
			Type = toDamageType;
			Multiplier = multiplier;
			Duration = Effect.INFINITE_DURATION;
		}

		override public Effect Clone {
			get {
				var res = new VulnerableEffect(Multiplier, Type);
				res.lastInitiative = lastInitiative;
				res.Duration = Duration;
				return res;
			}
		}

		public float Multiplier { get; set; }

		override public int ProcessDamage(int howMuch, string type) {
			if(type == Type || Type == GameMechanics.DAMAGE_TYPE_UNTYPED) {
				return (int)(howMuch * Multiplier);
			}
			else {
				return howMuch;
			}
		}

		public string Type { get; set; }
	}
}

