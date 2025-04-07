using UniRx;
using UnityEngine;

namespace VoxelCommand.Client
{
    public class UnitController : DisposableComponent
    {
        private Unit _unit;

        public void Initialize(Unit unit)
        {
            Dispose();
            _unit = unit;
        }
    }
}
