# Voxel Command

A tactical strategy game with voice command integration and character-based gameplay.

## Project Overview

Voxel Command is a tactical strategy game where players control units through voice commands. The game features:

- Voice command recognition to control your units
- NPCs with distinct character traits that affect how they follow orders
- Tactical combat with multiple unit classes
- Progression system with leveling and character upgrades
- Enemy units with exploitable psychological weaknesses

## Repository Structure

This is an Nx workspace containing:

- **apps/game**: Unity game client
- **apps/backend**: ASP.NET backend for LLM integration
- **libs/core**: Shared library with common types and utilities

## Setup Instructions

### Prerequisites

- Unity 6 (6000.0.27f1)
- .NET 9.0
- Node.js and npm
- Nx CLI: `npm install -g nx`

### Installation

1. Clone the repository
   ```
   git clone https://github.com/danielkreitschel/voxel-command.git
   cd voxel-command
   ```

2. Install dependencies
   ```
   npm install
   ```

## Development

### Running the Game Client

Use the Unity Editor to open the project in `apps/game` and press play.

### Running the Backend

```
nx run backend:serve
```

The backend will start on http://localhost:5098 and handle LLM integration for voice commands.

## Building for Production

```
nx run-many --target=build --projects=game,backend --prod
```

Game build output will be in `dist/apps/game`, and backend build in `dist/apps/backend`.

## More Information

For more information, see

- [Character System](docs/Character%20System.md)
- [Voice Commands Processing](docs/Voice%20Commands%20Processing.md)
