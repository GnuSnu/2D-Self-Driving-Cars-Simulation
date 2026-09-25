# 2D Self-Driving Cars Simulation

A 2D Unity simulation where autonomous agents learn to navigate tracks using a custom feedforward neural network and a genetic algorithm built from scratch in C#.

<img width="746" height="438" alt="Adobe Express - 2025-08-11 20-03-14-good (1)" src="https://github.com/user-attachments/assets/c4c573e3-8343-4508-840c-ab42b90d3e5d" />


## Overview

The goal was to implement evolutionary learning from first principles without relying on external ML frameworks like Unity ML-Agents, PyTorch, or TensorFlow.

- **Population Size:** 200 agents per generation
- **Inputs:** 5 directional raycasts tracking distance to track limits and 1 vehicle speed input
- **Neural Network:** Custom feedforward architecture mapping uppermentioned inputs directly to steering and motor controls
- **Genetic Algorithm:** Selection, crossover, and mutation applied to network weights based on track distance covered and optimal route choice
- **Model serialization:** Compressing neural network data for trained models saving & loading system

## Codebase

The core simulation logic is split between two C# scripts:

* [`Assets/Scripts/Manager.cs`](Assets/Scripts/Manager.cs) – Handles global simulation state, generation lifecycles and user's inputs.
* [`Assets/Scripts/Car.cs`](Assets/Scripts/Simulation.cs) – Implements raycast sensing, Neural network structure & rendering, fitness evaluation, genetic operators (selection & mutation) and vehicle physics.

## Requirements

* Unity 2021.3.6 (or newer)
* Pure C# / Standard Unity Library (no external packages required)
