using GridTool;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FN
{
    public class GameLifetimeScope : LifetimeScope
    {
        public GridDataSO GridData;

        public SpawnDataSO SpawnData;
        
        public InputReader InputReader;
        
        protected override void Configure(IContainerBuilder builder)
        {
            //Register ScriptableObjects
            builder.RegisterInstance(GridData);
            builder.RegisterInstance(SpawnData);
            builder.RegisterInstance(InputReader);

            //Register Services
            builder.Register<GridService>(Lifetime.Singleton)
                .AsImplementedInterfaces();
            
            builder.Register<ObjectPoolingService>(Lifetime.Singleton)
                .AsImplementedInterfaces();
            
            builder.Register<SpawnService>(Lifetime.Singleton)
                .AsImplementedInterfaces();
            
            //Register GameObjects
            builder.RegisterComponentInHierarchy<LevelInitializer>();
            
            //Entry point
            builder.RegisterEntryPoint<GameEntryPoint>();
        }
    }
}