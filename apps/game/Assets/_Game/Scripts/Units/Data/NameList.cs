using System.Collections.Generic;
using UnityEngine;

namespace VoxelCommand.Client
{
    [CreateAssetMenu(fileName = "Name List", menuName = "Game/Name List")]
    public class NameList : ScriptableObject
    {
        [SerializeField]
        private List<string> _names = new();

        public List<string> Names => _names;
    }
}
