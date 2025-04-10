using System;
using NaughtyAttributes;
using UniRx;
using UnityEngine;

public class ReactiveExample2 : MonoBehaviour
{
    public IntReactiveProperty A = new();

    public IntReactiveProperty B = new();

    [ReadOnly]
    public IntReactiveProperty C = new();

    void Start()
    {
        A.Add(B).Delay(TimeSpan.FromMilliseconds(500)).Subscribe(sum => C.Value = sum).AddTo(this);
    }
}

/*

It's helpful to understand how UniRx fits in with standard Unity approaches.

Update() is typically for continuous changes to internal state
(e.g., object movement frame by frame)

Events are better suited for discrete reactions triggered by specific occurrences or external signals
(e.g., a player input causing a speed change command)

UniRx offers a powerful alternative, especially for managing asynchronous operations and complex event-driven logic:

+ Control Inversion:
  Instead of one object directly calling methods on another, objects can observe public properties or event streams
  of others and react automatically when changes occur (like `A` and `B` being observed here to calculate `C`).
  Advantage: Decoupling, easier to test, easier to change.

+ Rich Operators:
  UniRx provides a standardized, composable way to handle common patterns like filtering, delaying,
  combining streams, managing subscription lifetimes, and much more, reducing boilerplate code.
  Advantage: Standardized (also outside of game dev), composable, reusable.

+ Async Synergy:
  It integrates seamlessly with asynchronous operations.
  Advantage: Easier to work with async operations, which are common in multiplayer games.

While not a silver bullet for every situation, UniRx provides a robust and declarative framework for
handling reactivity and managing complexity in event flows. It's a valuable toolset worth considering,
especially as projects scale. Even Nintendo uses it for their Unity games.

*/