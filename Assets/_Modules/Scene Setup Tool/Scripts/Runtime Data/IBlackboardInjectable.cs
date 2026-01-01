using System.Collections.Generic;
using UnityEngine;

namespace SceneSetupTool
{
    public interface IBlackboardInjectable
    {
        [HideInInspector]
        IEnumerable<string> LocalKeys { get; set; }
    }
}