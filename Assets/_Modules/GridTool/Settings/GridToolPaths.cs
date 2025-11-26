namespace GridTool
{
    public static class GridToolPaths
    {
        public const string MODULE_FOLDER = "Assets/_Modules/GridTool";
        public const string RESOURCES_FOLDER = MODULE_FOLDER + "/Resources";

        public class Settings
        {
            private const string GRIDTOOL_SETTINGS_ASSET_NAME = "GridToolSettings";
            private const string SETTINGS_FOLDER = MODULE_FOLDER + "/Settings";
            public const string SETTINGS_ASSET_PATH = SETTINGS_FOLDER + "/" + GRIDTOOL_SETTINGS_ASSET_NAME + ".asset";
        }
        
        public class TerrainTypes
        {
            public const string TERRAIN_TYPE_FOLDER = RESOURCES_FOLDER + "/TerrainTypes";
        }

        public class Levels
        {
            public const string LEVELS_SCENES_FOLDER = RESOURCES_FOLDER + "/Levels";
            public const string LEVELS_DATA_FOLDER = RESOURCES_FOLDER + "/Levels";
        }

        public class GridData
        {
            public const string GRID_DATA_FOLDER = RESOURCES_FOLDER + "/Levels";
        }
    }
}