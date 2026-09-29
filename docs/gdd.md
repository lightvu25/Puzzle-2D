Game Design Document: Kinetic Maze (Working Title)
1. Executive Summary & Design Pillars
Genre: 2D Grid-Based Puzzle Arcade / Maze Runner
Target Platforms: Mobile (iOS / Android), PC (Steam / itch.io)
Visual Target: 2D Pixel Art @ 32 PPU ( Grid Units)
Core Loop: Swipe cardinal directions to fling the player across a labyrinth, building momentum over distance to trigger kinetic destruction, deflecting off angled wedges, and navigating reactive terrain.
Core Pillars
Kinetic Momentum Over Static Sliding: Movement is not merely transportation; velocity dictates interaction. Travel distance determines whether you glide harmlessly across brittle surfaces or blast through obstacles as a kinetic ram.
Deterministic Bank-Shot Physics:  deflection wedges transform rigid cardinal corridors into rhythmic, pool/billiards-style chain reactions.
Juicy  Visual Punch: Crisp pixel clarity paired with impactful squash-and-stretch animations, camera trauma shake, and responsive particle feedback.
2. Technical & Display Specifications
2.1. Pixel Metrics & Resolution
Pixel Density: 32 Pixels Per Unit (PPU).
Grid Cell Size:  pixels.
Actor Sprite Dimensions:  pixels centered inside a  bounding box. This leaves a 4-pixel clearance border to prevent visual overlap with solid geometry.
VFX Particle Chunks:  to  sub-pixel debris.
2.2. Viewport & Camera Configuration
Camera Type: Orthographic, fixed single-screen layout per level (no scrolling required for standard puzzle levels).
Viewport Dimensions (Portrait Mobile Target):
Width: 10 to 12 tiles ( internal render resolution).
Height: 16 to 20 tiles ( internal render resolution).
Pixel Grid Snapping: Camera position and rendering coordinates strictly snap to integer values to prevent sub-pixel shimmering or jitter.
3. Core Mechanics & State Machines
3.1. Movement & Input Handling
Input Scheme: 4-directional cardinal swipes (Up, Down, Left, Right) or D-pad / Arrow keys.
Dash State: Once initiated, the player accelerates instantaneously along the designated vector and travels continuously until interrupted by one of the following:
A solid wall (Stops movement; resets velocity).
A reactive braking tile (e.g., Mud/Tar; stops movement immediately).
A  deflection wedge (Redirects velocity  without halting dash state).
Input Buffering: An active input buffer window ( before collision) registers the next intended swipe, allowing high-speed chained turns.
3.2. Velocity Tiers (The Runway System)
The movement controller tracks TilesTraveled from dash initiation until a stop event occurs. Reaching specific distance thresholds alters the character's physical state:
Tier
Distance (Tiles)
Visual & Audio Cue
Gameplay & Interaction Properties
Tier 1 (Drift)
1–2 tiles
Standard sprite, tiny dust puff on stop, soft thud sound.
Cannot break barriers. Safely crosses brittle glass without shattering it.
Tier 2 (Cruise)
3–4 tiles
Motion trail behind sprite, medium impact sound, minor screen shake.
Standard hazard interactions. Activates heavy floor switches.
Tier 3 (Kinetic Ram)
5+ tiles
White afterimages, spark/smoke particles, heavy bass impact, camera punch.
Shatters Cracked Blocks, crushes small patrolling enemies, shatters brittle glass on contact.

[Start Dash] ──> Tile 1-2 (Tier 1) ──> Tile 3-4 (Tier 2) ──> Tile 5+ (Tier 3: Kinetic Ram)
                      │                       │                       │
                Crosses Glass           Standard Push          Smashes Gates / Kills Mobs


3.3.  Prism Deflection Vector Math
Deflector wedges occupy a full  tile space and split the cell diagonally. When entering a wedge tile, the incoming cardinal vector is immediately mapped to an outgoing cardinal vector without resetting TilesTraveled or interrupting the dash state.
Top-Right Wedge (/ slant, solid bottom-right):
Incoming North ()  Redirects East ().
Incoming West ()  Redirects South ().
Top-Left Wedge (\ slant, solid bottom-left):
Incoming North ()  Redirects West ().
Incoming East ()  Redirects South ().
Blunt Surface Collisions: Swiping directly into the flat back or blunt edge of a wedge functions as a standard solid wall impact (halts movement and resets momentum).
4. Tile & Object Taxonomy
  32x32 Grid Cell Setup
┌───────────────────────────────┐
│ [Tile: 32x32 px]              │
│  - Wall / Wedge: Solid 32x32  │
│  - Floor: 32x32 decal base    │
│  - Actor: 24x24 inside 32x32  │
│    (leaves 4px breathing room)│
└───────────────────────────────┘


4.1. Universal Base Blocks
Solid Basalt Wall (): Standard boundary. Halts movement, resets velocity, emits dust puff.
Cracked Masonry ():
Impact Tier 1–2: Functions as a solid wall. Emits pebble particles and cracking sound.
Impact Tier 3: Destroys the block instantly, leaving passable rubble.
 Deflector Wedges (): Diagonal angled blocks with polished metallic or crystal edging for instant readability.
Mud / Tar Brake Tile (): Absorbs all momentum instantly and forces a dead stop at tile center. Allows mid-corridor turns without perimeter walls.
5. World Themes & Signature Mechanics
World 1: Catacombs  ──> World 2: The Foundry  ──> World 3: Crystal Spire ──> World 4: Clockwork Vault ──> World 5: Void Singularity
(Brakes & Wedges)       (Heat & Slag Gates)       (Polarity & Crystals)     (Gears & Conveyors)         (Gravity & Wormholes)


World 1: The Sunken Catacombs (Foundations & Banking)
Theme: Overgrown ancient ruins, mossy basalt stone, polished brass fixtures.
Focus: Mastering cardinal movement, learning  deflections, and using mud brakes to set up precision trajectory lines.
Signature Mechanics:
Brittle Flagstones: Walkable at Tier 1; shatters permanently into an impassable pit if crossed at Tier 2 or 3.
Brass Spring Bumpers: Mounted flat on walls. Inverts the vector by  without losing momentum tier.
World 2: The Molten Foundry (Thermal Velocity & Slag Barriers)
Theme: Industrial smelting forge, iron grates, glowing lava rivers, heat haze.
Focus: High-speed commitment; players cannot afford to stop on volatile surfaces.
Signature Mechanics:
Superheated Floor Grates: Glowing red tiles that overheat after entry.
If crossed at Tier 2 or Tier 3, the player skates across safely without burning.
If the player stops or enters at Tier 1, they overheat and combust (immediate level reset).
Cooled Slag Blocks: Dense, heat-hardened obsidian barriers. Requires Tier 3 Kinetic Ramming to shatter. Smashing one emits a molten spark blast that ignites adjacent fuse tiles.
Coolant Sprayers: Wall-mounted nozzles that spray liquid nitrogen in rhythm ( on,  off). Crossing while active drops velocity to zero and freezes the player for .
World 3: The Crystal Spire (Polarity & Refraction)
Theme: Prismatic caverns, translucent cyan/amber crystals, bottomless starry chasms.
Focus: Shifting colors/states mid-flight and navigating phase-shifting barriers.
Signature Mechanics:
Refraction Prisms (Color Shift Wedges): A  crystal wedge that deflects the player  and simultaneously swaps the player's core polarity between Cyan (Light) and Amber (Heavy):
Cyan State: High agility, safe from laser fences, but cannot push heavy objects.
Amber State: Doubles mass; destroys cracked crystal nodes, but triggers pressure mines.
Polarized Phase Gates: Cyan laser bars let Cyan runners pass uninterrupted; Amber runners crash and take lethal damage. Amber barriers behave inversely.
Resonance Chimes: Hit-switches that toggle barrier frequencies. Must be struck with an intentional bank shot.
World 4: The Clockwork Vault (Inertial Transfer & Gears)
Theme: Victorian automaton core, rotating brass cogs, ticking pendulums, steam pipes.
Focus: Dynamic and moving redirectors that change room topology in rhythm with time or movement.
Signature Mechanics:
Rotary Turnstiles (Pinwheels): A  central pivot with a 4-blade paddle.
Striking any paddle redirects the runner  clockwise.
The impact rotates the turnstile , altering the next path for any subsequent passes.
Inertial Conveyor Rails:
Parallel Traversal: Moving with the conveyor grants  Velocity Tier immediately. Moving against it drops velocity to Tier 1.
Perpendicular Traversal: Flinging across a conveyor pushes the player  in the belt's flow direction mid-dash, creating a 1-tile offset in trajectory.
Piston Pistons: Wall segments that thrust forward 1 tile every 3 beats, closing corridors or swatting the player into alternate paths.
World 5: The Void Singularity (Gravitational Mechanics & Wormholes)
Theme: Cosmic abyss, floating obsidian debris, violet gravitational anomalies, spatial rifts.
Focus: Vector curving, non-Euclidean routing, and spatial displacement.
Signature Mechanics:
Gravity Singularity Nodes ( Cores):
Exerts a continuous orthogonal pull on adjacent lanes.
A Tier 1 or Tier 2 dash traveling parallel to a singularity has its trajectory bent  inward toward the core (gravitational slingshot).
A Tier 3 (Kinetic Ram) dash has enough escape velocity to break free and maintain a straight cardinal line.
Quantum Wormhole Rifts: Paired spatial apertures (Violet A and Violet B).
Entering Portal A instantly teleports the player to Portal B.
Exiting preserves exact velocity magnitude, momentum tier, and heading direction.
Null-Gravity Drift Pads: Replaces the standard stop behavior. Hitting a null-gravity pad prevents the player from locking down; they continue drifting slowly at 1 tile per second until an arrow input is swiped.
6. Climax / World Boss Concept: The Kinetic Sieges
Rather than traditional combat encounters, world climaxes are high-intensity chase or siege arenas where the boss can only be damaged using the world's signature mechanic:
World 1 Boss (The Golem Warden): A colossal stone guardian with cracked armor plates on its sides. The player must bank through outer corridors, hit Tier 3, and slam directly into its exposed weak points.
World 2 Boss (The Molten Behemoth): Moves along a center track; player must skate over superheated grates to trigger coolant releases that freeze the boss, creating a brief window for a kinetic shatter strike.
World 3 Boss (The Prismatic Sentinel): Emits alternating Cyan and Amber shielding waves; player must hit colored prism wedges to align their phase polarity before striking the boss.
World 4 Boss (The Clockwork Titan): Navigates on rotating gear axles. The player must calculate turnstile trajectories and conveyor boosts to intercept the Titan's exposed gear pinions.
World 5 Boss (The Singularity Leviathan): Uses gravitational warping to distort the player's swipe trajectory. The player must chain wormholes and sling around gravity wells to land three full Tier 3 impacts.
7. Progression, Meta-Game & Star Rating System
To drive replayability and mastery without tedious tile-painting quotas, levels feature a performance-based 3-star rating system:
┌────────────────────────────────────────────────────────┐
│ ★☆☆ Complete the Stage (Reach the Exit Vault)          │
│ ★★☆ Par Swipes (Complete within strict move quota)    │
│ ★★★ Kinetic Mastery (Execute ≥ N Tier-3 Rams or Tasks) │
└────────────────────────────────────────────────────────┘


7.1. Star Evaluation Matrix
Star 1 (Survival / Clear): Reaching the exit tile alive.
Star 2 (Move Optimization): Completing the maze within a designated Par Swipe Count (e.g., solved in  swipes).
Star 3 (Kinetic Challenge): World-specific objective:
World 1: Shatter all cracked blocks on the board.
World 2: Never touch a coolant nozzle.
World 3: Zero mismatched phase gate impacts.
World 4: Rotate all turnstiles to an aligned state.
World 5: Achieve escape velocity from 2 gravity wells.
7.2. Economy & Collectibles
Kinetic Shards (In-level Pickups): 3 gold crystals placed along high-risk secondary paths (e.g., requiring dangerous bank shots or brittle floor crossings).
Runner Masks / Chassis (Cosmetics Only): Shards unlock distinct  sprite skins with unique trail particle effects (e.g., neon sparks, molten cinders, clockwork steam plumes), preserving competitive fairness with zero pay-to-win stat changes.
8. Alternate Mode: "Kinetic Overdrive" (Endless Arcade Ascent)
An homage to classic Tomb of the Mask reflex climbing, repurposed around the Runway Momentum engine:
Core Loop: A procedurally generated, vertically scrolling shaft ().
The Threat (Crushing Plasma Surge): An electrified plasma field steadily rises from the bottom of the screen, accelerating over time.
Momentum-Gated Gates: Every 15–20 vertical tiles, the shaft is sealed by a row of reinforced cracked blocks. Players cannot pass through without finding and executing a vertical or banked 5-tile runway to trigger a Tier 3 Kinetic Ram that blasts the barrier open before the plasma consumes them.
Leaderboards: Scored by vertical depth achieved, total barriers smashed, and average velocity tier sustained.
9. Anti-Softlock & Physics Edge-Case Protocols
High-speed grid rebounding introduces rare edge cases that must be programmatically resolved:
9.1. Infinite Loop Detection
Scenario: The player enters a loop of opposing bumpers or four cyclic  wedges with no intervening stop tile.
Resolution: An internal ConsecutiveBounces counter increments on each deflection without user input. If ConsecutiveBounces > 8:
The actor's kinetic friction spikes instantly to maximum.
The actor drops out of the dash state and comes to an emergency rest on the center of the next valid open floor tile.
A brief stun animation plays (), restoring player swipe control.
9.2. Instant Respawn Flow
In arcade maze runners, downtime between deaths must approach zero:
Death collision trigger   hit-stop frame freeze.
Death VFX: Sprite breaks into 8 direction-scattered  pixel chunks.
Total restart cycle: Exactly  from death to player re-spawning on the start tile with active swipe detection ready.
10. HUD & In-Game UI Architecture
10.1. Minimalist In-Game HUD (Portrait View)
Top Bar ( height):
Left: Level Index & World Icon.
Center: Star Progress Indicators (3 hollow icons filling dynamically as conditions are met).
Right: Pause / Settings Quick-Tap Button.
In-Game Move Counter: Muted number displayed below the player spawn point indicating Swipes Remaining for Star 2.
Velocity Gauge: Subtle chevron indicators flanking the player sprite that ignite at Tier 2 and Tier 3.
11. Audio-Visual Juice & Impact Specifications
11.1. Hit-Stop / Frame Freezes
Tier 1 Stop: 0 frames pause.
Tier 2 Stop: 2 frames () pause.
Tier 3 Collision / Block Smash: 4–6 frames () freeze + 2px screen trauma shake.
11.2. Squash and Stretch Ratios
Standard Moving Form: 20px wide  28px tall.
Wall Impact Form: 28px wide  16px tall for 2 frames before snapping back to  rest state.
11.3. Palette & Readability Guidelines
Background / Floor: Muted dark tones (slate grey, dark navy, deep basalt).
Interactive Paths / Wedges: High-readability brass, gold, or cyan for immediate path identification.
Danger / Breakables: High-contrast amber (cracked blocks) and crimson (spikes/hazards).
12. Settings & Player Configuration Architecture
To support both responsive arcade play and casual or accessibility-focused play, the settings menu is segmented into five structured tabs:
┌────────────────────────────────────────────────────────┐
│ [CONTROLS]  [AUDIO/HAPTIC]  [DISPLAY]  [A11Y]  [SYSTEM]│
└────────────────────────────────────────────────────────┘


12.1. Controls & Input Tuning
Primary Input Method:
Free Swipe (Default): Finger gestures anywhere on screen.
Floating Virtual D-Pad: Appears wherever the initial touch contact occurs.
Fixed On-Screen D-Pad / Cross: Anchored to bottom-left or bottom-right with adjustable layout positioning (Left-Handed / Right-Handed toggle).
Swipe Deadzone Threshold: Slider from  to  (Default: ). Determines minimum touch drag before direction registers.
Input Buffer Window: Slider from  to  (Default: ). Configures how early players can queue the next swipe before hitting a wall or wedge.
Swipe Vector Angle Bias: Toggle between Strict Cardinal Snap (snaps to closest  axis immediately) and Free Drag Direction (requires crossing a  threshold to prevent diagonal misinputs).
Touch Trail Visualization: Toggle (On / Off). Shows a brief faint neon line following finger drags for visual input feedback.
12.2. Audio & Haptic Feedback
Volume Sliders (Independent 0%–100%):
Master Volume
Sound Effects (SFX): Wall impacts, glass breaks, block shattering, jet whooshes.
Music (BGM): Driving chiptune / electronic synth soundscape.
Haptics & Vibration:
Haptics Master Toggle: On / Off.
Haptic Profile Selector:
Full Kinetic: Distinct tactile clicks for wedge deflections, soft hum on Tier 2, heavy double-thump on Tier 3 impacts.
Minimal: Only buzzes on lethal hazard impacts and level clears.
Haptic Strength Slider: , , ,  (governed by device vibration actuator limits).
Mute on Background / Focus Loss: Toggle (Automatically silences audio when app is minimized).
12.3. Display, Performance & "Juice" Toggles
Target Frame Rate: Selector (, ,  / Uncapped). High-refresh mode is critical for sub-millisecond input response on modern  mobile displays.
Screen Shake / Camera Trauma: Slider ( to , Default ). Allows motion-sensitive players to eliminate camera jolt completely while keeping sprite-level impacts.
Hit-Stop (Frame Freeze) Toggle:
Full Impact (Default): Full  freeze on Tier 3 impacts.
Speedrun Minimal: Disables frame freezes for uninterrupted timing continuity.
Retro Shaders / CRT Filter:
Scanlines & Vignette toggle (Subtle retro arcade aesthetic overlay; On / Off).
Chromatic Aberration on Kinetic Ram (Toggle On / Off).
Battery Saver Mode: Locks frame rate to , reduces debris particle density by , and disables real-time heat haze / bloom shaders.
12.4. Accessibility & Assistance (a11y)
Game Speed Multiplier (Practice / Assist Mode):
Options: , ,  (Normal Speed).
Integrity Protection: Playing below  speed allows players to clear stages and practice routes, but disables leaderboard submissions and Star 2 / Star 3 unlocks.
Colorblind Visual Profiles:
Presets: Off, Protanopia, Deuteranopia, Tritanopia, High Contrast Monochrome.
Geometric Polarity Glyphs (World 3): Always enabled or linked to colorblind profiles. Guarantees that Cyan vs. Amber gates and wedges carry diamond () vs. square () micro-stamps.
Flashing Light Reducer (Photosensitivity Guard):
Replaces full-screen white flash on Tier 3 Ram collisions with an expanding dark shockwave ring.
Tile Grid Overlay: Toggle (Displays subtle sub-pixel grid guidelines across the floor to aid trajectory planning).
12.5. Save Data & Profile Management
Haptic & Calibration Reset: Restores deadzone and input sliders to factory defaults.
Erase Stage Stars / Full Progress Reset: Dual-confirmation modal requiring typing "RESET" or holding down for 3 seconds to prevent accidental wipe.
