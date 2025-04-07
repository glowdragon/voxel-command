using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class UnitManager : MonoBehaviour
    {
        [Inject]
        private UnitFactory _unitFactory;

        [Inject]
        private UnitConfig _defaultUnitConfig;

        public Unit CreateUnit(Vector3 position, Quaternion rotation, Team team, UnitConfig config = null)
        {
            Unit unit = _unitFactory.Create();
            unit.transform.position = position;
            unit.transform.rotation = rotation;
            unit.Initialize(config ?? _defaultUnitConfig, team);

            return unit;
        }
    }
}
