using UniRx;
using UnityEngine;

public class UnitState : MonoBehaviour
{
    public IntReactiveProperty Experience = new(0);
    public IntReactiveProperty Level = new(1);
    public IntReactiveProperty AvailableStatPoints = new(0);

    public IntReactiveProperty HealthRank = new(0);
    public IntReactiveProperty DamageRank = new(0);
    public IntReactiveProperty DefenseRank = new(0);
    public IntReactiveProperty SpeedRank = new(0);

    public FloatReactiveProperty MaxHealth = new(1);
    public FloatReactiveProperty DamageOutput = new(1);
    public FloatReactiveProperty IncomingDamageReduction = new(0);
    public FloatReactiveProperty MovementSpeed = new(1);

    public FloatReactiveProperty Health = new(1);
}
