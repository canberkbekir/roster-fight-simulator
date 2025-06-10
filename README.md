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

## Gene System Overview

`GeneData` assets (`Assets/Scripts/Creatures/Genes/Base/ScriptableObjects/GeneData.cs`) represent individual genes. Each gene contains a set of `GeneFeatureData` objects (`Assets/Scripts/Creatures/Genes/Base/ScriptableObjects/GeneFeatureData.cs`) which describe how that gene affects a chicken. Concrete feature types such as `StatGeneFeatureData`, `AppearanceGeneFeatureData` and `SkillGeneFeatureData` live in `Assets/Scripts/Creatures/Genes/Features/`.

During breeding, `BreedingService.CrossGenes` (`Assets/Scripts/Services/BreedingService.cs`) mixes the gene arrays from a rooster and a hen. Genes with the same ID are randomly chosen from either parent before being passed to the new egg.

When a chicken's genome changes, components react to the features inside each gene. For appearance genes, `ChickenAppearance` dispatches features to `ChickenAppearanceHandler` and `BodyPart` objects (see `Assets/Scripts/Creatures/Chickens/Base/Components/ChickenAppearance.cs` and `Assets/Scripts/Creatures/Chickens/Base/Utils/BodyPart.cs`) to update colors and other visuals. Stat and skill features follow a similar pattern and can be extended to modify values in `ChickenStats` or trigger abilities.

## License
This project is licensed under the MIT License. See [LICENSE.md](LICENSE.md) for details.
