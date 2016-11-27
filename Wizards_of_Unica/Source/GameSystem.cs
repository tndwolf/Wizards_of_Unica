namespace tndwolf.ECS {
	/// <summary>
	/// Game system base class.
	/// </summary>
	public class GameSystem {
		public bool Active { get; set; }

		public bool ClearMe { get; set; }

		/// <summary>
		/// Diagnose this system, usually by printing out the internal status.
		/// </summary>
		virtual public int Diagnose() { return 0; }

		/// <summary>
		/// Initialize the game system, it may or may not use the world parameter
		/// </summary>
		/// <param name="world">World.</param>
		virtual public void Initialize(World world) { }

		/// <summary>
		/// This function is called automatically whenever a new component is added
		/// to the world. This function decides if the component will be used or not
		/// and ideally adds it to the system execution queue
		/// </summary>
		/// <param name="component">Component.</param>
		virtual public bool Register(GameComponent component) { return false; }

		/// <summary>
		/// This function is called automatically whenever a new component is removed
		/// from the world, and should thus be removed by all systems using it
		/// </summary>
		/// <param name="component">Component.</param>
		virtual public void UnRegister(GameComponent component) { }

		/// <summary>
		/// Called automatically every time the world updates.
		/// </summary>
		/// <param name="world">World.</param>
		virtual public void Update(World world) { }
	}
}

