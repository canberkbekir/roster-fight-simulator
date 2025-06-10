# Rooster Fight Simulator

Rooster Fight Simulator is a Unity project that simulates chickens living, breeding and fighting in a small environment. The project uses the Mirror networking library so it can be played in multiplayer mode. Various managers and services handle day/night cycles, spawning, breeding and inventory management.

## Features
- AI controllers for roosters, hens and chicks
- Breeding system with gene features controlling appearance and stats
- Inventory and container system for items and genes
- Day and night cycle with adjustable time scale
- Input actions using Unity's Input System
- Networked multiplayer powered by Mirror

## Requirements
- **Unity 2022.3.61f1** (or a compatible 2022 LTS version)

All dependencies are managed through the Unity Package Manager. Important packages include Cinemachine, TextMeshPro and Mirror.

## Getting Started
1. Clone this repository
2. Open the project in Unity Hub using the required editor version
3. Open `Assets/Scenes/SampleScene.unity`
4. Enter Play mode to try the simulation

## License
This project is licensed under the MIT License. See [LICENSE.md](LICENSE.md) for details.
