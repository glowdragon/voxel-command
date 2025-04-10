using NaughtyAttributes;
using UnityEngine;

public class ImperativeExample2 : MonoBehaviour
{
    public int A;

    public int B;

    [ReadOnly]
    public int C;

    private int a;

    private int b;

    void Start()
    {
        C = A + B;

        a = A;
        b = B;
    }

    void Update()
    {
        if (A != a || B != b)
        {
            a = A;
            b = B;
            C = A + B;
        }
    }
}

/*

Polling in Update()

+ Ensures C updates if A or B change at runtime
+ Simple logic

- Inefficient (runs checks every frame regardless of changes)
- Boilerplate code to store and compare previous values

*/