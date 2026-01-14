using System;
using Sirenix.OdinInspector;

namespace CTB.DesignPatterns.Blackboard
{
    [Serializable]
    [InlineProperty]
    [HideReferenceObjectPicker]
    public class BlackboardOutputBinding
    {
        [HideLabel]
        public BlackboardOutput Output;

        [HideLabel]
        public TypedValue TypedValue;

        public void TrySave(Blackboard blackboard)
        {
            if (Output == null || TypedValue == null)
                return;

            Output.TrySave(blackboard, TypedValue.Value);
        }
    }
}