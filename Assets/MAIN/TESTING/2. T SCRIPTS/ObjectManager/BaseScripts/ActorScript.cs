using UnityEngine;

[System.Serializable]
public abstract class ActorScript<TManager> : IActorScript
    where TManager : ManagerScript
{

    public TManager managerScript;

    public override bool MeetsRequirements(){
        return managerScript != null || managerScript.electronicObject != null;
    }
    public override bool Solve(){
        
        if (!MeetsRequirements()){
            return false;
        }

        if (!is_executing) StartExecution();
        else UpdateExecution();
        return true;

    }

    public override void StartExecution(){
        is_executing = true;
    }

    public override void UpdateExecution(){
        Debug.Log("Actualizando");
    }

    public override void StopExecution(){
        is_executing = false;
    }

}
