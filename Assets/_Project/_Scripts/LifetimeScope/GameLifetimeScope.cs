using GridTool;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FN
{
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("Grid Data")] [SerializeField] private GridDataSO gridData;

        protected override void Configure(IContainerBuilder builder)
        {
            //Register ScriptableObjects
            builder.RegisterInstance(this.gridData);

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