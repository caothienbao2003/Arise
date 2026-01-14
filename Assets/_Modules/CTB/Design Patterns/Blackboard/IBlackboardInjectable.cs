using System.Collections.Generic;
using UnityEngine;

namespace CTB.DesignPatterns.Blackboard
{
    public interface IBlackboardInjectable
    {
        [HideInInspector]
        IEnumerable<string> LocalKeys { get; set; }
    }
}