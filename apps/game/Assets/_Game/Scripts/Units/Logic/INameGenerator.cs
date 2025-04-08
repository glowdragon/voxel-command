using System.Collections.Generic;

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
        /// <param name="allUnits">List of all existing units to check for name conflicts</param>
        /// <returns>A unique name, possibly with a numeric suffix</returns>
        string GetUniqueName(Team team, IEnumerable<Unit> allUnits);
    }
}
