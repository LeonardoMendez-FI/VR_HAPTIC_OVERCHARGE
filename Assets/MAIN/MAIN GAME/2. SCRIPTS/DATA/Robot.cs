using UnityEngine;

public class Robot : ElectronicObject
{

    [Header("Robot Managers")]
    public MoveManager moveManager;
    public AttackManager attackManager;

    public bool IsAttacking(){
        return attackManager.is_attacking;
    }

}
