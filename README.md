# Particle gravity simulations
GPU-accelerated (with compute shader) .NET GUI application (WPF) for visalisation of particles in gravitation field of multiple planets.

## Features
 * 1-15 planets
 * Millions of particles
 * Draggable and zoomable
 * Draggable planets and particle starting region
 * Attractor mode (shows state after fixed number of integration steps depending on the planets positions)
 * simple integrator implemented in compute shader
 * OpenGL rendering

## Example captures

![capture](https://github.com/panjanek/particle-gravity/blob/8a1d6d823b8265c4ee8e2c69a94a5dd437ce8804/captures/gravity2.png "capure")
![capture](https://github.com/panjanek/particle-gravity/blob/8a1d6d823b8265c4ee8e2c69a94a5dd437ce8804/captures/gravity3.png "capure")
![6 planets](https://github.com/panjanek/particle-gravity/blob/af1d2e8b19dcb4256076e0b23946b1b945b8abf2/captures/gravity-6planets.png "6 planets")

## Keys
 * <kbd>M</kbd> switch between simulation mode and attractor mode
 * <kbd>1</kbd>-<kbd>9</kbd> change number of plantes
 * <kbd>Q</kbd>,<kbd>W</kbd> change particles count
 * <kbd>C</kbd> toggle colors
 * <kbd>F</kbd> full screen
 * <kbd>H</kbd> toggle markers visibility
 * <kbd>+</kbd>,<kbd>-</kbd> change integration step DT
 * <kbd>A</kbd>,<kbd>S</kbd> change integration steps count count for attractor mode
 * <kbd>P</kbd> save capture PNG\n"+
 * <kbd>Space</kbd> pause
