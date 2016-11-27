namespace tndwolf.ECS
{
    class MainClass {
		const string DEFAULT_MODULE = "Data/DefaultModule.xml";
		const string START_SCENE = "START_SCENE";//START_ICE

		public static void Main(string[] args) {
			Services.Initialize();
			Services.Logger.BlackList("EncounterManager.GenerateEncounter");
			Services.Logger.BlackList("GameFactory.BuildFromTemplate");
			Services.Logger.BlackList("GameFactory.GetVector2f");
			Services.Logger.BlackList("GameMechanics.Create");
			//Services.Logger.BlackList("GameMechanics.Damage");
			Services.Logger.BlackList("GameMechanics.Execute");
			Services.Logger.BlackList("GameMechanics.Play");
			Services.Logger.BlackList("GameMechanics.SetAnimation");
			Services.Logger.BlackList("GameMechanics.UpdateGui");
			//Services.Logger.BlackList("GridManager.Move");
			Services.Logger.BlackList("LevelActor");
			Services.Logger.BlackList("WorldFactory.BuildBlock");
			Services.Logger.BlackList("bp_fire_garg1.OnRound");
			//Services.Logger.BlackList("e_dynamic_lava.OnRound");
			var world = new World();
			Services.GameMechanics.Initialize(world);
			Services.GameFactory.Load(DEFAULT_MODULE);
			Services.GameFactory.LoadScene(world, START_SCENE);
			while(Services.Inputs.Command != Command.QUIT) {
				world.Update();
			}
			Services.Close();
		}
	}
}
