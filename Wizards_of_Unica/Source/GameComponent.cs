using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace tndwolf.ECS {
	/// <summary>
	/// Game component base class from which all others have to derive
	/// </summary>
	[Serializable]
	public class GameComponent {
		/// <summary>
		/// Gets or sets the entity this component refers to.
		/// </summary>
		/// <value>The entity.</value>
		[XmlAttribute]
		public int Entity;

		public GameComponent(int entity = -1) {
			Entity = entity;
			Tags = new List<string>();
		}

		/// <summary>
		/// Called automatically when the component is removed from a World
		/// </summary>
		public virtual void Delete(World world) { }

		/// <summary>
		/// Gets or sets the delete me flag, after each World.Update components
		/// flagged are safely removed.
		/// </summary>
		/// <value>The delete me.</value>
		[XmlAttribute]
		public bool DeleteMe { get; set; }

		/// <summary>
		/// Called automatically when the component is added to a World
		/// </summary>
		public virtual void Initialize(World world) { }

		/// <summary>
		/// Sets the component to be "managed" (run by a System) or
		/// "unmanaged" (run by the World).
		/// </summary>
		/// <value>True if managed, false otherwise.</value>
		[XmlIgnore]
		public bool IsManaged { get; protected set; }

		/// <summary>
		/// Called automatically on each World.Update
		/// </summary>
		public virtual void Update(World world) { }

		/// <summary>
		/// Gets or sets the tags of the component.
		/// </summary>
		/// <value>The tags.</value>
		public List<string> Tags { get; set; }

		override public string ToString() { return "<GameComponent/>"; }
	}
}

