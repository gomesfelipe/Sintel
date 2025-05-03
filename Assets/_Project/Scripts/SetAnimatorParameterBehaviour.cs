using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAnimatorParameterBehaviour : StateMachineBehaviour
{
    public enum ParameterType { Bool, Float, Trigger }

    public ParameterType parameterType = ParameterType.Bool;
    public string parameterName;

    // Flags de ativação
    public bool updateOnStateEnter, updateOnStateExit;
    public bool updateOnStateMachineEnter, updateOnStateMachineExit;

    // Valores usados para bool e float
    public bool boolValueOnEnter, boolValueOnExit;
    public float floatValueOnEnter, floatValueOnExit;

    // Triggers normalmente só são setados (não "resetados" com frequência)
    public bool setTriggerOnEnter = true;
    public bool resetTriggerOnExit = false;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (updateOnStateEnter)
            ApplyValue(animator, true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (updateOnStateExit)
            ApplyValue(animator, false);
    }

    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        if (updateOnStateMachineEnter)
            ApplyValue(animator, true);
    }

    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        if (updateOnStateMachineExit)
            ApplyValue(animator, false);
    }

    private void ApplyValue(Animator animator, bool isEntering)
    {
        switch (parameterType)
        {
            case ParameterType.Bool:
                animator.SetBool(parameterName, isEntering ? boolValueOnEnter : boolValueOnExit);
                break;

            case ParameterType.Float:
                animator.SetFloat(parameterName, isEntering ? floatValueOnEnter : floatValueOnExit);
                break;

            case ParameterType.Trigger:
                if (isEntering && setTriggerOnEnter)
                    animator.SetTrigger(parameterName);
                else if (!isEntering && resetTriggerOnExit)
                    animator.ResetTrigger(parameterName);
                break;
        }
    }
}
