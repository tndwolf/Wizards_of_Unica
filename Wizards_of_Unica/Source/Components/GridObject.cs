using System;
using System.Xml.Serialization;

namespace tndwolf.ECS {
	[Serializable]
	public class GridObject: GameComponent {
		public GridObject(int entity) : base(entity) {
			IsManaged = true;
			MoveSpeed = 64f / 1000f;
			Width = 1;
			Height = 1;
		}

		/// <summary>
		/// Gets or sets the movement speed in pixels/millisec.
		/// </summary>
		/// <value>The move speed.</value>
		[XmlAttribute]
		public float MoveSpeed { get; set; }

		[XmlIgnore]
		public bool HasToMove {
			get { return OldX != X || OldY != Y; }
		}

		[XmlAttribute]
		public int Height { get; set; }

		[XmlAttribute]
		public bool IsDressing { get; set; }

		[XmlAttribute]
		public bool IsTrigger { get; set; }

		[XmlIgnore]
		public int MaxX { get { return X + Width - 1; } }

		[XmlIgnore]
		public int MaxY { get { return Y + Height - 1; } }

		public string OnEnter { get; set; }

		public void SetPosition(int x, int y) {
			OldX = X = x;
			OldY = Y = y;
		}

		[XmlAttribute]
		public int X { get; set; }

		[XmlAttribute]
		public int Y { get; set; }

		[XmlAttribute]
		public int Width { get; set; }

		[XmlIgnore]
		public int OldX { get; set; }

		[XmlIgnore]
		public int OldY { get; set; }
	}
}

