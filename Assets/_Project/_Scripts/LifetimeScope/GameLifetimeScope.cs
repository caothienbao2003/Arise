using GridTool;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FN
{
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("Grid Data")] [SerializeField] public GridDataSO GridData;

        protected override void Configure(IContainerBuilder builder)
        {
            //Register ScriptableObjects
            builder.RegisterInstance(this.GridData);

            //Register Services
            builder.Register<GridService>(Lifetime.Singleton)
                .AsImplementedInterfaces();
            
            //Register GameObjects
            builder.RegisterComponentInHierarchy<MoveToPositionPathfinding>();
            
            //Entry point
            builder.RegisterEntryPoint<GameEntryPoint>();
        }
    }
}