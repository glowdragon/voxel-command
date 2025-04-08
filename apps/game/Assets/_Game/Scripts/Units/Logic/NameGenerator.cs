using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoxelCommand.Client
{
    /// <summary>
    /// Provides random name generation for units
    /// </summary>
    public class NameGenerator : MonoBehaviour, INameGenerator
    {
        [SerializeField]
        private NameList _playerNameList;

        [SerializeField]
        private NameList _enemyNameList;

        private Dictionary<Team, List<string>> _namesByTeam;

        private void InitializeNameLists()
        {
            _namesByTeam = new Dictionary<Team, List<string>>
            {
                { Team.Player, _playerNameList.Names },
                { Team.Enemy, _enemyNameList.Names },
            };
        }

        /// <summary>
        /// Generates a random name based on the unit's team
        /// </summary>
        /// <param name="team">The team for which to generate a name</param>
        /// <returns>A random name</returns>
        public string GetRandomName(Team team)
        {
            if (_namesByTeam == null)
                InitializeNameLists();

            List<string> teamNames = _namesByTeam[team];
            int randomIndex = Random.Range(0, teamNames.Count);
            return teamNames[randomIndex];
        }

        /// <summary>
        /// Generates a name with a numeric suffix if needed for uniqueness
        /// </summary>
        /// <param name="team">The team for which to generate a name</param>
        /// <param name="allUnits">List of all existing units to check for name conflicts</param>
        /// <returns>A unique name, possibly with a numeric suffix</returns>
        public string GetUniqueName(Team team, IEnumerable<Unit> allUnits)
        {
            if (_namesByTeam == null)
                InitializeNameLists();

            // Get the set of names currently in use by existing units
            HashSet<string> usedNames = allUnits.Select(u => u.Name).ToHashSet();

            // Try to find an unused name from the list
            List<string> teamNames = _namesByTeam[team];
            List<string> availableNames = teamNames.Where(name => !usedNames.Contains(name)).ToList();

            // If we have available names, pick one randomly
            if (availableNames.Count > 0)
            {
                int randomIndex = Random.Range(0, availableNames.Count);
                return availableNames[randomIndex];
            }

            // All names are taken, add a numeric suffix
            string baseName = GetRandomName(team);
            string uniqueName;
            int suffix = 2;

            do
            {
                uniqueName = $"{baseName} {suffix}";
                suffix++;
            } while (usedNames.Contains(uniqueName) && suffix < 1000);

            return uniqueName;
        }
    }
}
