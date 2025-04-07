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

        private void Start()
        {
            // Create 5 units on both sides
            for (int i = 0; i < 5; i++)
            {
                CreateUnit(new Vector3(-5, 0, (i * 2) - 4), Quaternion.identity, Team.Player);
                CreateUnit(new Vector3(5, 0, (i * 2) - 4), Quaternion.identity, Team.Enemy);
            }
        }

        public Unit CreateUnit(Vector3 position, Quaternion rotation, Team team)
        {
            Unit unit = _unitFactory.Create();
            unit.transform.position = position;
            unit.transform.rotation = rotation;
            unit.Initialize(_defaultUnitConfig, team);

            return unit;
        }
    }
}
