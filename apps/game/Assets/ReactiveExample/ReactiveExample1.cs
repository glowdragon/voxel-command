using NaughtyAttributes;
using UniRx;
using UnityEngine;

public class ReactiveExample1 : MonoBehaviour
{
    public IntReactiveProperty A = new();

    public IntReactiveProperty B = new();

    [ReadOnly]
    public IntReactiveProperty C = new();

    void Start()
    {
        A.Add(B).Subscribe(sum => C.Value = sum).AddTo(this);
    }
}

/*

Reactive Approach using UniRx

+ Declarative (defines relationships, not step-by-step execution)
+ Automatically handles updates when A or B change
+ Less boilerplate for change detection
+ Integrates well with asynchronous operations

- Introduces a dependency (UniRx)
- Steeper learning curve compared to simple imperative code
- Debugging can sometimes be more complex due to implicit data flow

*/