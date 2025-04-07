# NavMesh System Setup for Voxel Command

This folder contains all the necessary files for the NavMesh navigation system used by units in the game.

## Installation

The basic AI.Navigation package is already included in the project, but for runtime NavMesh generation, you'll need to install the NavMeshComponents:

1. Open the Package Manager (Window > Package Manager)
2. Click the "+" button and select "Add package from git URL..."
3. Enter the URL: `https://github.com/Unity-Technologies/NavMeshComponents.git#package`
4. Click "Add"

## Components Overview

- **PathfindingService**: Provides path calculation functions using Unity's NavMesh system.
- **NavigationInstaller**: Zenject installer to bind navigation services.
- **UnitController**: Handles unit movement using NavMeshAgent.

## Setting Up a Scene

1. **Add a NavMesh**: 
   - Window > AI > Navigation
   - In the Navigation window, select the "Bake" tab
   - Select your walkable areas and adjust settings as needed
   - Click "Bake" to generate the NavMesh

2. **Setup for Runtime Generation** (optional):
   - Add a NavMeshSurface component to a GameObject in your scene
   - Configure the NavMeshSurface settings (include layers, object types, etc.)
   - Call `navMeshSurface.BuildNavMesh()` at runtime to generate/update the NavMesh

3. **Add the NavigationInstaller**:
   - Create an empty GameObject in your scene
   - Add the NavigationInstaller component
   - Add the PathfindingService component to the same GameObject
   - Add this GameObject to your Zenject Scene Context's Mono Installers list

## Common Issues

- **Units not navigating**: Ensure they have NavMeshAgent components and are placed on a valid NavMesh area.
- **Cannot find path**: Check that start and end points are on the NavMesh (use NavMesh.SamplePosition to find nearest points).
- **Units stuck at obstacles**: Adjust the NavMeshAgent radius, height, and obstacle avoidance settings.

## Advanced Usage

For advanced NavMesh features such as NavMesh Links, NavMesh Obstacles, or NavMesh Modifiers, refer to Unity's documentation:
https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html 