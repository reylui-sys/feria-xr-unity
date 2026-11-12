# Feria XR — VR fairground & AR alien shooter (Unity 6 · OpenXR)

[![Demo video](https://img.youtube.com/vi/IszvCUDUt_Q/hqdefault.jpg)](https://www.youtube.com/watch?v=IszvCUDUt_Q)
<sub>▶ Demo video</sub>

**English** · [Español](#español)

A two-part XR project for *Multimodal Interaction* (BSc Multimedia Engineering, Universitat de València, 2025-26), built in a team of two with **Unity 6 (6000.3)**, **XR Interaction Toolkit 3**, **AR Foundation 6** and **OpenXR** for **Meta Quest**.

| Scene | Experience |
|---|---|
| `Assets/Scenes/VR.unity` | **Feria VR**: a fairground with three booths, **Shooting gallery**, **Darts** and **Air hockey**, controlled with the Quest controllers and **voice commands**. |
| `Assets/Scenes/AR.unity` | **Alien invasion (mixed reality)**: the headset scans the room, finds the real walls and spawns aliens on them, and you shoot them with a tracked blaster. |

## Part 1 — Feria VR

Each booth has its own panel with a timer (it turns red in the last 10 s), the current score, a per-booth **high score**, the voice commands, difficulty buttons and, in the shooting booth, the ammo left.

| Booth | Mechanics |
|---|---|
| **Shooting** | Grab the gun and hold the trigger to **charge the shot** (power bar and haptics), then release to fire. Shots charged to 80 % or more earn bonus points. Knock down **can towers** that the booth rebuilds. Say *"RECARGAR"* to reload. |
| **Darts** | Grab a dart, aim with a guide line and throw it with a natural release. The board scores 50 / 25 / 10 by ring, and darts return to their holder automatically. |
| **Hockey** | Grab the stick and hit the puck: the force depends on the controller's velocity. Score past a goalkeeper that oscillates in front of the goal. The puck resets when it leaves the rink. |

**Voice commands** (`VoiceManager.cs`): *EMPEZAR DISPAROS / DARDOS / HOCKEY*, *RECARGAR*, *DIFICULTAD FÁCIL / NORMAL / DIFÍCIL*, *TERMINAR PARTIDA*. Each command plays an audio cue.

**Global difficulty** (`DifficultyManager.cs`), applied to every booth:

| | Round time | Goalkeeper speed | Dartboard | Points |
|---|---|---|---|---|
| Easy | 90 s | ×0.5 | bigger | ×1 |
| Normal | 60 s | ×1 | normal | ×1 |
| Hard | 45 s | ×2 | smaller | ×2 |

Main scripts: `GameManager`, `MinigamePanel`, `DifficultyManager`/`DifficultyUI`, `VoiceManager`, `GunController`, `Bullet`, `CanTowerSpawner`, `CanTarget`/`BottleTarget`, `DartStick`, `DartBoard`, `HockeyDisc`, `GoalDetector`, `ObstacleOscillator`, `BrightnessController`.

## Part 2 — AR alien invasion

- **Wall detection from the environment mesh** (`MeshScript.cs`): attached to the `ARMeshManager` mesh prefab. It waits until the room mesh is ready, keeps only the triangles with a horizontal normal (the walls), builds an invisible `MeshCollider` from them and sends evenly spaced spawn points (position and normal) to the game manager.
- **Game loop** (`GameManagerAR.cs`): a singleton that runs the timer, the score and the start/restart UI. It spawns aliens at random intervals on the detected walls, oriented by the wall normal and capped at a maximum number alive.
- **Charged blaster** (`GunControllerAR.cs`): the VR gun mechanic adapted to AR, with an aim line, power UI, haptics and raycast hits.
- **Aliens** (`Alien.cs`): a pop-in spawn animation, a squash effect when hit, health and points, a lifetime, death VFX and sounds.
- **Sockets** (`SocketReturn.cs`, `SocketHighlightAR.cs`): the gun returns to its socket when released, and the socket glows to show the state (idle pulse, or green on hover).
- **Occlusion helpers** (`Shaders/`): a depth-only shader and a stencil hole mask, adapted from the course's AR template.

## Open the project
1. Unity **6000.3.5f2** or newer with Android build support.
2. Open the folder; Unity restores the packages listed in `Packages/manifest.json`.
3. Open `Assets/Scenes/VR.unity` or `Assets/Scenes/AR.unity`.

> **Notes**
> - Voice commands use Unity's `KeywordRecognizer` (Windows speech), so they work in the editor and over Quest Link on Windows.
> - The VR-part scripts were removed from the course repository when the project moved on to AR. They've been restored here from the version control history with their original `.meta` GUIDs, so the VR scene references them again.
> - **Third-party assets not included:** Asset Store content (*Grass And Flowers Pack 1*, the *Epic_BlueSunset* skybox) and the high-resolution terrain textures are left out because their licences don't allow redistribution. Some materials will show as missing, but the gameplay doesn't depend on them. 3D models and sound effects come from free online sources.

---

## Español

Proyecto XR en dos partes para *Interacción Multimodal* (Ingeniería Multimedia, UV, 2025-26), hecho en pareja con **Unity 6**, **XR Interaction Toolkit 3**, **AR Foundation 6** y **OpenXR** para **Meta Quest**. [Vídeo demo](https://www.youtube.com/watch?v=IszvCUDUt_Q).

**Feria VR**: tres casetas de feria (**Disparos**, **Dardos** y **Hockey**), cada una con su panel (temporizador, puntuación, récord, comandos de voz y munición).
- **Disparos**: disparo cargado con barra de potencia y vibración háptica, puntos extra si cargas al 80 % o más, y torres de latas que se vuelven a montar.
- **Dardos**: línea guía para apuntar y diana con zonas de 50, 25 y 10 puntos.
- **Hockey**: la fuerza del golpe depende de la velocidad del mando, y hay un portero móvil.
- **Comandos de voz**: *EMPEZAR …*, *RECARGAR*, *DIFICULTAD …* y *TERMINAR PARTIDA*.
- **Dificultad global**: Fácil (90 s), Normal (60 s) y Difícil (45 s, portero ×2, diana pequeña y puntos ×2).

**Invasión AR**: el casco escanea la habitación, detecta las paredes reales a partir de la malla del entorno (filtrando los triángulos con normal horizontal) y hace aparecer aliens sobre ellas. Se disparan con la pistola de disparo cargado adaptada a AR, y el socket se ilumina según su estado.

**Notas:** los comandos de voz usan `KeywordRecognizer` de Windows, así que funcionan en el editor o con Quest Link. Los scripts de la parte VR se recuperaron del historial de git, porque se borraron al pasar a AR. Los paquetes del Asset Store y las texturas de terreno grandes no se incluyen porque su licencia no permite redistribuirlos.
