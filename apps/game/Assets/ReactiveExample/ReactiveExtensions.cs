using System;
using UniRx;

public static class ReactivePropertyExtensions
{
    public static IObservable<int> Add(this IntReactiveProperty a, IntReactiveProperty b)
    {
        return a.CombineLatest(b, (aVal, bVal) => aVal + bVal);
    }
}
