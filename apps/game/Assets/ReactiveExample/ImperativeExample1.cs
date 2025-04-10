using NaughtyAttributes;
using UnityEngine;

public class ImperativeExample1 : MonoBehaviour
{
    public int A;

    public int B;

    [ReadOnly]
    public int C;

    void Start()
    {
        C = A + B;
    }
}

/*

One-time Calculation

+ Very simple

- C does not update if A or B change after Start

*/