using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace tndwolf.ECS {
	[Serializable]
	public class TurnActor: GameComponent {
		public TurnActor(int entity = -1) : base(entity) {
			Health = 1;
			MaxHealth = 1;
			IsManaged = true;
			Skills = new List<Skill>();
			Speed = 10;
			Variables = new Dictionary<string, int>();
		}

		/// <summary>
		/// Gets or sets the Actor's faction.
		/// </summary>
		/// <value>The faction.</value>
		[XmlAttribute]
		public string Faction { get; set; }

		public Skill GetSkill(string skill) {
			return Skills.Find((sk) => sk.Id == skill || sk.Name == skill);
		}

		public Skill GetSkill(List<string> skills) {
			if(skills.Count == 1) return GetSkill(skills[0]);
			skills.Sort();
			var reference = string.Join("", skills.ToArray());
			Services.Logger.Debug("TurnActor.GetSkill", "Searching for " + reference);
			return Skills.Find((sk) => { 
				sk.Combo.Sort();
				Services.Logger.Debug("TurnActor.GetSkill", "vs " + string.Join("", sk.Combo.ToArray()));
				return reference == string.Join("", sk.Combo.ToArray()); 
			});
		}

		/// <summary>
		/// Gets or sets the current health of the Actor
		/// </summary>
		/// <value>The increase.</value>
		[XmlAttribute]
		public int Health { get; set; }

		/// <summary>
		/// Gets or sets the current initiative value.
		/// </summary>
		/// <value>The initiative.</value>
		[XmlAttribute]
		public int Initiative { get; set; }

		/// <summary>
		/// Gets or sets the is player flag.
		/// </summary>
		/// <value>The is player.</value>
		[XmlAttribute]
		public bool IsPlayer { get; set; }

		/// <summary>
		/// Gets or sets the maximum health of the Actor
		/// </summary>
		/// <value>The increase.</value>
		[XmlAttribute]
		public int MaxHealth { get; set; }

		/// <summary>
		/// Gets or sets the script that is run when the actor is created.
		/// Note that created is different from loaded.
		/// </summary>
		/// <value>The script.</value>
		public string OnCreate { get; set; }

		/// <summary>
		/// Gets or sets the script that is run when the actor dies.
		/// Note that deleted is different from dying.
		/// </summary>
		/// <value>The script.</value>
		public string OnDeath { get; set; }

		/// <summary>
		/// Gets or sets the script that is run at every round.
		/// </summary>
		/// <value>The script.</value>
		public string OnRound { get; set; }

		[XmlIgnore]
		public List<Skill> Skills { get; set; }

		/// <summary>
		/// Gets or sets the default increase of initiative after the actor's turn.
		/// Lower values means the actor is faster
		/// </summary>
		/// <value>The increase.</value>
		[XmlAttribute]
		public int Speed { get; set; }

		[XmlIgnore]
		public Dictionary<string, int> Variables { get; set; }
	}
}

