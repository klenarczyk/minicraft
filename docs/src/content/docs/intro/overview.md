---
title: Project Overview
description: High-level overview of the Minicraft structure.
---

High-level overview of the Minicraft structure as well as **Getting Started** instructions.

## Directory Structure

The project is strictly separated into the following domains to enforce clean architecture:

### 1. `GameWindow.cs`

The entry point and heart of the application. It inherits from OpenTK's `GameWindow` and manages the application lifecycle:

-   **Initialization**: Sets up the Graphics Context and Resource Managers.
-   **Game Loop**: Orchestrates `OnUpdateFrame` (logic/physics) and `OnRenderFrame` (draw calls).
-   **Input**: Polling keyboard and mouse state.

### 2. Engine (`/Engine`)

The "Engine" contains reusable, low-level code that is agnostic of the specific game being played. It handles:

-   **Graphics**: OpenTK (OpenGL) wrappers, Vertex Array Objects (VAOs), and Batch Rendering.
-   **Diagnostics**: Debugging tools, performance timers, and the static `Logger.cs`.
-   **IO**: File handling for loading textures and text files.

### 3. Game (`/Game`)

The "Game" contains the specific gameplay implementation of Minicraft:

-   **World**: Voxel data storage, Chunk generation algorithms, and mesh building.
-   **ECS**: The Entity Component System managing the Player and Physics.
-   **Items**: Registry definitions for Blocks (`Block.cs`) and Items.

### 4. Assets (`/Assets`)

Contains all external game resources loaded at runtime:

-   **Data**: JSON files defining Block properties (e.g., hardness, texture mappings).
-   **Textures**: `.png` files and texture atlases for block faces, items and UI elements.
-   **Shaders**: GLSL source code (`.vert` and `.frag`) for the rendering pipeline.
-   **UI**: Fonts and layout definitions.

## Getting Started

To run the project locally:

1. Open `Minicraft.sln`.
2. Ensure `.NET 10.0` (or later) is installed.
3. Set `Minicraft` as the StartUp project.
4. Build and run the project (you can use `dotnet run` in the terminal).
