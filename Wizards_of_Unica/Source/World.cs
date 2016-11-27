using System.Collections.Generic;
using System.Diagnostics;

namespace tndwolf.ECS {
	public class World {
		public const int INVALID_ENTITY = -1;

		private bool clearMe = false;
		private Stopwatch clock = new Stopwatch();
		private List<GameComponent> components = new List<GameComponent>();
		private List<GameComponent> componentsToAdd = new List<GameComponent>();
		private int lastEntity = -1;
		private long lastUpdateTime = 0;
		private List<GameSystem> systems = new List<GameSystem>();
		private List<GameComponent> unmanaged = new List<GameComponent>();

		/// <summary>
		/// Initializes a new World. In the constructor the systems should be registered
		/// and the world clock is started
		/// </summary>
		public World() {
			Add(new GridManager());
			Add(new TurnManager());
			Add(new Renderer());
			//Add(new EncounterManager());
			Add(new AdaptiveAudioSystem());
			Add(new UserInterafaceSystem());
			clock.Start();
		}

		/// <summary>
		/// Adds the specified component to the world. Components are automatically
		/// registered to the world GameSystems, where applicable
		/// </summary>
		/// <param name="component">Component.</param>
		public void Add(GameComponent component) {
			components.Add(component);
			//Services.Logger.Debug("World.Add", "Adding: " + component);
			if(component.IsManaged) {
				var isOrphan = true;
				foreach(var system in systems) {
					if(system.Register(component) == true) {
						isOrphan = false;
					}
				}
				if(isOrphan) {
					Services.Logger.Warn("World.Add", "Orphan: " + component);
				}
			}
			else {
				component.Initialize(this);
				unmanaged.Add(component);
			}
			if(component.Entity > lastEntity) {
				lastEntity = component.Entity;
			}
		}

		public void AddInternal(GameComponent component) {
			components.Add(component);
			//Services.Logger.Debug("World.Add", "Adding: " + component);
			if(component.IsManaged) {
				var isOrphan = true;
				foreach(var system in systems) {
					if(system.Register(component) == true) {
						isOrphan = false;
					}
				}
				if(isOrphan) {
					Services.Logger.Warn("World.Add", "Orphan: " + component);
				}
			}
			else {
				unmanaged.Add(component);
			}
			if(component.Entity > lastEntity) {
				lastEntity = component.Entity;
			}
		}

		/// <summary>
		/// Adds and initializes the specified system.
		/// </summary>
		/// <param name="system">System.</param>
		protected void Add(GameSystem system) {
			system.Initialize(this);
			system.Active = true;
			systems.Add(system);
		}

		System.Action Callback = null;
		public void Clear(System.Action Callback) {
			this.Callback = Callback;
			clearMe = true;
		}

		protected void ForceClear() {
			lastEntity = -1;
			foreach(var system in systems) {
				system.Initialize(this);
			}
			components.Clear();
			unmanaged.Clear();
			clearMe = false;
		}

		/// <summary>
		/// Gets the number of millisecnds since last Update.
		/// </summary>
		/// <value>The delta time.</value>
		public int DeltaTime { get; set; }

		/// <summary>
		/// Flags a component for deletion, it will actually be deleted at the beginning of the next
		/// Update.
		/// </summary>
		/// <param name="component">Component.</param>
		public void Delete(GameComponent component) {
			component.DeleteMe = true;
		}

		/// <summary>
		/// Flags all components of the entity for deletion, they will actually be deleted at 
		/// the beginning of the next Update.
		/// </summary>
		/// <param name="entity">Entity.</param>
		public void Delete(int entity) {
			foreach(var component in components) {
				if(component.Entity == entity) Delete(component);
			}
		}

		public int Diagnose() {
			var res = 0;
			Services.Logger.Debug("World.Diagnose", "===== DIAGNOSE BEGIN ====================");
			Services.Logger.Debug("World.Diagnose", "===== Checking components ===============");
			foreach(var component in components) {
				Services.Logger.Debug("World.Diagnose", component.Entity);
			}
			Services.Logger.Debug("World.Diagnose", "----- Total components: " + components.Count);
			Services.Logger.Debug("World.Diagnose", "----- Total unmanaged: " + unmanaged.Count);
			Services.Logger.Debug("World.Diagnose", "===== Checking systems ==================");
			foreach(var system in systems) {
				res += system.Diagnose();
			}
			Services.Logger.Debug("World.Diagnose", "----- Total systems: " + systems.Count);
			Services.Logger.Debug("World.Diagnose", "===== EDIAGNOSE END =====================");
			return res;
		}

		/// <summary>
		/// Returns a entity ID that is guaranteed to be available until the next
		/// Add call
		/// </summary>
		/// <returns>The new entity identifier.</returns>
		public int GenerateNewEntityId() {
			return lastEntity + 1;
		}

		/// <summary>
		/// Gets the first component of a specified type owned by an entity, or null.
		/// </summary>
		/// <returns>The component.</returns>
		/// <param name="entity">Entity.</param>
		/// <typeparam name="T">The GameComponent type.</typeparam>
		public T GetComponent<T>(int entity) where T : class {
			return components.Find((component) => component is T && component.Entity == entity) as T;
		}

		/// <summary>
		/// Gets the first component of a specified type owned by an entity, or null.
		/// </summary>
		/// <returns>The component.</returns>
		/// <param name="entity">Entity.</param>
		/// <typeparam name="T">The GameComponent type.</typeparam>
		public T GetComponent<T>(int entity, string tag) where T : class {
			return components.Find(
				(component) => component is T &&
				component.Entity == entity &&
				component.Tags.Contains(tag)) as T;
		}

		/// <summary>
		/// Gets all the components of a specified type, or null. All element inside the 
		/// returned list are of type T, so it is safe to cast.
		/// </summary>
		/// <returns>The components.</returns>
		/// <typeparam name="T">The GameComponent type.</typeparam>
		public List<GameComponent> GetComponents<T>() where T : class {
			return components.FindAll((component) => component is T);
		}

		/// <summary>
		/// Gets all the components owned by an entity, or null.
		/// </summary>
		/// <returns>The components.</returns>
		/// <param name="entity">Entity.</param>
		public List<GameComponent> GetComponents(int entity) {
			return components.FindAll((component) => component.Entity == entity);
		}

		/// <summary>
		/// Gets the system or null if none are registered.
		/// </summary>
		/// <returns>The system.</returns>
		/// <typeparam name="T">The GameSystem type.</typeparam>
		public T GetSystem<T>() where T : class {
			return systems.Find((system) => system is T) as T;
		}

		/// <summary>
		/// Polls the Input service for events and then runs sequentially all the
		/// registered GameSystems, finally it runs all the ophans GameComponents by
		/// themselves
		/// </summary>
		public void Update() {
			if(clearMe) {
				ForceClear();
				Callback.Invoke();
				return;
			}
			// Clear all the deleted components and unregister them
			foreach(var component in components) {
				if(component.DeleteMe)
					Unregister(component);
			}
			components.RemoveAll((c) => c.DeleteMe);
			unmanaged.RemoveAll((c) => c.DeleteMe);
			// Add new components
			componentsToAdd.ForEach((c) => AddInternal(c));
			// Dispatch events, very important
			Services.Window.DispatchEvents();
			// Perform the updates
			DeltaTime = (int)(Time - lastUpdateTime);
			lastUpdateTime = Time;
			foreach(var component in unmanaged) {
				component.Update(this);
			}
			foreach(var system in systems) {
				if(system.Active == true) {
					system.Update(this);
				}
			}
		}

		/// <summary>
		/// Unregister the specified component from the subsystems. This method
		/// does not remove the component from the world, use Delete() to do that
		/// </summary>
		/// <param name="component">Component.</param>
		public void Unregister(GameComponent component) {
			if(component.IsManaged) {
				foreach(var system in systems) {
					system.UnRegister(component);
				}
			}
		}

		/// <summary>
		/// Gets the absolute time in milliseconds.
		/// </summary>
		/// <value>The time.</value>
		public long Time {
			get { return clock.ElapsedMilliseconds; }
		}
	}
}

