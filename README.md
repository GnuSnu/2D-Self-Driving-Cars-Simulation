# 2D Self-Driving Cars Simulation

A 2D Unity simulation where autonomous agents learn to navigate tracks using a custom feedforward neural network and a genetic algorithm built from scratch in C#.

![Simulation Demo](demo.gif)

## Overview

The goal was to implement evolutionary learning from first principles without relying on external ML frameworks like Unity ML-Agents, PyTorch, or TensorFlow.

- **Population Size:** 200 agents per generation
- **Inputs:** 5 directional raycasts tracking distance to track limits and 1 vehicle speed input
- **Neural Network:** Custom feedforward architecture mapping uppermentioned inputs directly to steering and motor controls
- **Genetic Algorithm:** Selection, crossover, and mutation applied to network weights based on track distance covered and optimal route choice
- **Model serialization:** Compressing neural network data for trained models saving & loading system

## Codebase

The core simulation logic is split between two C# scripts:

* [`Assets/Scripts/Manager.cs`](Assets/Scripts/Manager.cs) – Handles global simulation state, generation lifecycles, fitness evaluation, and genetic operators (selection & mutation).
* [`Assets/Scripts/Car.cs`](Assets/Scripts/Car.cs) – Implements raycast sensing, matrix forward pass, vehicle physics, and collision handling.

## Requirements

* Unity 2021.3.6 (or newer)
* Pure C# / Standard Unity Library (no external packages required)
