using System.Collections.Generic;
using UnityEngine;

namespace VoxelCommand.Client
{
    /// <summary>
    /// Interface for name generation services
    /// </summary>
    public interface INameGenerator
    {
        /// <summary>
        /// Generates a random name based on the unit's team
        /// </summary>
        /// <param name="team">The team for which to generate a name</param>
        /// <returns>A random name</returns>
        string GetRandomName(Team team);

        /// <summary>
        /// Generates a name with a numeric suffix if needed for uniqueness
        /// </summary>
        /// <param name="team">The team for which to generate a name</param>
        /// <returns>A unique name, possibly with a numeric suffix</returns>
        string GetUniqueName(Team team);

        /// <summary>
        /// Releases a name back to the pool so it can be reused
        /// </summary>
        /// <param name="team">The team the name belongs to</param>
        /// <param name="name">The name to release</param>
        void ReleaseName(Team team, string name);
    }

    /// <summary>
    /// Provides random name generation for units
    /// </summary>
    public class NameGenerator : INameGenerator
    {
        private readonly List<string> _playerNames = new List<string>
        {
            "Alex",
            "Amadeus",
            "Andrea",
            "Bene",
            "Benjamin",
            "Berk",
            "Chris",
            "Daniel",
            "David",
            "Davide",
            "Giovanni",
            "Hilde",
            "Jan",
            "Jasmin",
            "Javid",
            "Julia",
            "Justin",
            "Michael",
            "Nadine",
            "Seb",
            "Steve",
            "Thomas",
            "Vera",
            "William",
            "Xavier",
            "Yacine",
        };

        private readonly List<string> _enemyNames = new List<string>
        {
            "Alice",
            "Donald",
            "Eike",
            "Elon",
            "Friedrich",
            "Josh",
            "Mia",
            "Philipp",
            "Tim",
            "Vladimir",
        };

        private Dictionary<Team, List<string>> _namesByTeam;
        private Dictionary<Team, HashSet<string>> _usedNames;

        public NameGenerator()
        {
            _namesByTeam = new Dictionary<Team, List<string>> { { Team.Player, _playerNames }, { Team.Enemy, _enemyNames } };

            _usedNames = new Dictionary<Team, HashSet<string>>
            {
                { Team.Player, new HashSet<string>() },
                { Team.Enemy, new HashSet<string>() },
            };
        }

        /// <summary>
        /// Generates a random name based on the unit's team
        /// </summary>
        /// <param name="team">The team for which to generate a name</param>
        /// <returns>A random name</returns>
        public string GetRandomName(Team team)
        {
            List<string> teamNames = _namesByTeam[team];
            HashSet<string> usedTeamNames = _usedNames[team];

            // If all names are used, reset the used names collection
            if (usedTeamNames.Count >= teamNames.Count)
            {
                usedTeamNames.Clear();
            }

            // Find an unused name
            string name;
            do
            {
                int randomIndex = Random.Range(0, teamNames.Count);
                name = teamNames[randomIndex];
            } while (usedTeamNames.Contains(name));

            // Mark as used
            usedTeamNames.Add(name);
            return name;
        }

        /// <summary>
        /// Generates a name with a numeric suffix if needed for uniqueness
        /// </summary>
        /// <param name="team">The team for which to generate a name</param>
        /// <returns>A unique name, possibly with a numeric suffix</returns>
        public string GetUniqueName(Team team)
        {
            string baseName = GetRandomName(team);

            // Check if we need to add a number suffix for further uniqueness
            if (IsTeamNameFull(team))
            {
                int suffix = Random.Range(1, 100);
                return $"{baseName}-{suffix}";
            }

            return baseName;
        }

        private bool IsTeamNameFull(Team team)
        {
            return _usedNames[team].Count >= _namesByTeam[team].Count * 0.75f;
        }

        /// <summary>
        /// Releases a name back to the pool so it can be reused
        /// </summary>
        /// <param name="team">The team the name belongs to</param>
        /// <param name="name">The name to release</param>
        public void ReleaseName(Team team, string name)
        {
            // Extract base name if it has a numeric suffix
            string baseName = name;
            int dashIndex = name.LastIndexOf('-');
            if (dashIndex >= 0)
            {
                baseName = name.Substring(0, dashIndex);
            }

            _usedNames[team].Remove(baseName);
        }
    }
}
