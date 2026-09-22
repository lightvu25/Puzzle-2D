Thẻ 1 
HOUSEFLOW — Comprehensive Master Game Design Document
Document Version: 1.5 (Production Harmonization & Onboarding Alignment)
Genre: 2D Systemic Physics Puzzle / Hybrid-Casual / Social Showcase
Target Platform: Mobile (Android First, iOS Target)
Business Model: Free-to-Play (Player-Centric: Aesthetics, Convenience, Season Pass)
Engine Baseline: 2D Physics Engine (Box2D / Unity 2D Physics with Screen-Space Metaballs)
Primary Design Goal: Deliver tactile physical puzzle simulation with emergent interactions, scalable data-driven architecture, a grounded meta-collection loop, and ethical, high-conversion monetization.
PART I: VISION, FANTASY & CORE LOOP
1. High Concept & Core Identity
1.1 One-Sentence Pitch
HOUSEFLOW is a systemic 2D physics puzzle game where players manipulate water, electricity, air, and steam to make eccentric houses work, unearth hidden curios, and furnish a personal home showcase.
1.2 Portfolio & Technical Positioning
HOUSEFLOW is a data-driven 2D physics puzzle platform built around scalable systemic simulation and hybrid-casual meta-retention. Utilizing a low-overhead Box2D particle pool coupled with screen-space metaball shaders, it achieves tactile fluid dynamics on budget mobile devices. The architecture features JSON-driven level pipelines, multi-tier touch disambiguation, an asynchronous NPC-to-peer social progression loop, and a player-centric cosmetic monetization framework.
1.3 The Core Identity
HOUSEFLOW is not a generic house-renovation game with detached puzzles stapled on.
It is a tactile physics puzzle game whose world naturally produces tools, discoveries, architectural artifacts, and aesthetic pride. The house exists because the player has unearthed treasures worth keeping. The treasures exist because the player made dysfunctional, eccentric machines work. That physical, contextual connection is the foundational anchor of the game.
1.4 Core Experiential Flow
Players interact with and control five core elemental utilities:
●	💧 Water (Continuous fluid dynamics, pressure, pooling)
●	⚡ Electricity (Circuits, conductivity, power actuation)
●	💨 Air (Directional drafts, pneumatic pressure, displacement)
●	🔥 Heat (Thermal conduction, heating, expansion)
●	♨️ Steam (Buoyancy, phase transition, pressure mechanics)
●	🧲 Mechanical Objects (Valves, switches, pumps, gears, counterweights)
The player routes, redirects, combines, activates, or contains these systems to fulfill straightforward physical objectives while discovering hidden curiosities.
 Completing puzzles awards soft currency (Coins), rare Discoveries, decorative furnishings, and emergency tools that feed directly into a personal house showcase.
2. Design Philosophy & Product Principles
HOUSEFLOW builds upon the proven design heritage of classic mobile physics puzzlers (such as Where's My Water?):
 The design adheres to six immutable product principles:
1.	The Puzzle is the Hero: Every meta-system and economic feature exists to celebrate the physical puzzle. The puzzle is the primary product; the house is the persistent showcase.
2.	Depth Through Interaction, Not Rule Sprawl: Rather than inventing bespoke gimmicks for every stage, mechanical depth emerges naturally from bounded, systemic interactions among physical utilities.
3.	Curiosity is Rewarded: Optional exploration and secret routing detours always yield tangible narrative or decorative prizes.
4.	Instant Clarity of Purpose: The player must understand what needs to be accomplished within the first 3 seconds of entering a stage; the challenge lies strictly in how to orchestrate the systems.
5.	Tactile Grounding: Puzzles are physical architectural installations (pipes, boilers, switches, loose insulation, drains), not abstract mathematical grids.
6.	Honest Monetization: Monetize identity, craftsmanship pride, and convenience—never puzzle fairness. Players are never blocked from solving a puzzle to coerce payment.
3. Core Player Fantasy
The player is the ingenious on-site troubleshooter restoring life and functionality to quirky, dysfunctional architectural oddities.
●	The Prompt: "Fill the vintage clawfoot bathtub on the 2nd floor."
●	The Reality On-Site: The main water valve is rusted shut, requiring steam pressure from a basement boiler; the boiler requires electricity routed across an ungrounded junction; an open drip threatens to short-circuit the master fuse box; and a rare brass antique fixture is buried behind loose insulation.
●	The Emotional Payoff: The player is not just solving an abstract puzzle; they are making a living, complex physical ecosystem work smoothly.
4. Master Game Loop Architecture
                 ┌────────────────────────────────┐
                 │          PUZZLE STAGE          │
                 │   Water • Electricity • Heat   │
                 │        Airflow • Steam         │
                 └───────────────┬────────────────┘
                                 │
                         Solve / Discover
                                 │
              ┌──────────────────┼──────────────────┐
              ▼                  ▼                  ▼
          🪙 Coins          🔎 Discoveries     🛠 Power-ups
              │                  │                  │
              └──────────────────┼──────────────────┘
                                 │
                                 ▼
                     ┌────────────────────────┐
                     │     YOUR HOME META     │
                     │  • Customize Slots     │
                     │  • Curio Display Shelf │
                     │  • NPC & Peer Tours    │
                     └───────────┬────────────┘
                                 │
                                 ▼
                     ┌────────────────────────┐
                     │   UNLOCKED PROGRESS    │
                     │  • New Neighborhoods   │
                     │  • Seasonal Tours      │
                     │  • Daily Challenges    │
                     └───────────┬────────────┘
                                 │
                                 └────────► (Re-enter Puzzle)


The puzzle remains the primary hero experience. The house serves as the player's persistent identity and trophy room. Social showcases and tours supply mid-to-long term retention.
PART II: PUZZLE SYSTEMS, SIMULATION & LEVEL DESIGN
5. Core Puzzle Interaction & Touch Disambiguation
Interaction is strictly touch-first, immediate, and ergonomic. Primary gestures are simple and universal: Tap, Drag, Swipe, Draw, and Hold.
5.1 Input Priority Protocol
To eliminate touch ambiguity on mobile screens (e.g., accidentally carving soil when attempting to turn a valve):
1.	Priority 1 — Interactive Mechanical Widgets (Tap / Drag):
○	Tapping or dragging valves, switches, breakers, and plugs consumes touch input immediately.
○	Standardized touch hitboxes are padded to a minimum of  .
2.	Priority 2 — Carvable Substrates (Swipe / Carve):
○	Swiping across loose soil, packed gravel, or insulation triggers continuous contour carving.
3.	Priority 3 — Background Canvas Touch (Drag / Pan):
○	Panning across multi-screen stages (locked on single-screen rooms).
5.2 Level Design Buffer Rule
Level designers must maintain physical separation between interactive mechanical fixtures and carvable substrates (minimum   clearance buffer). Mechanical widgets are placed in dedicated structural alcoves so swipe paths never collide with tap targets.
6. Puzzle Systems
HOUSEFLOW is powered by 5 elemental utility systems plus mechanical fixtures:
6.1 Water
●	Physical Properties: Gravity-driven, continuous flow, static pressure, pooling, absorption into porous media, splitting.
●	Mechanical Objects: Rigid pipes, rotary valves, one-way check valves, suction pumps, holding tanks, bathtubs, drains, impulse water-wheels.
●	Benchmark Goal: Route   of available fluid particles ( ) into the designated target basin.
6.2 Electricity
●	Physical Properties: Instantaneous circuit propagation, conductivity thresholds, switch states, circuit breaker overload failure.
●	Mechanical Objects: Dry-cell batteries, generators, manual knife switches, insulated wiring, contact terminals, electric motors, indicator lights.
●	Behavior: Energizing an uninsulated wire touching water electrifies the connected fluid body instantly.
6.3 Air
●	Physical Properties: Directional force vectors, convective draft, pneumatic pressure, displacement. Inside closed ducting, airflow behaves as a 1D vector line segment; when vented into open rooms, it dissipates as a conical draft attenuating within   ( ).
●	Mechanical Objects: Rotary intake/exhaust fans, ductwork, one-way pneumatic flap gates, balloons, inflatable counterweight bladders.
6.4 Heat
●	Physical Properties: Thermal conduction through solid metals, radiant ambient zones, convective upward dissipation, cooling thresholds.
●	Mechanical Objects: Gas burners, coal fireboxes, bimetallic expansion strips, thermal switches, heat sinks, insulation barriers.
6.5 Steam
●	Physical Properties: Emerges naturally from Water + Heat ( ). Negative gravity (buoyant upward acceleration), high expansion pressure, condensation into water droplets upon contact with cold surfaces.
●	Interactions: Drives steam pistons, trips pressure-relief whistles, spins turbine fans.
7. System Interaction Matrix
To ensure emergent gameplay while preventing combinatorial explosion, cross-system reactions are strictly bounded:

System A	System B	Emergent Reaction	Practical Gameplay Application
Water	Electricity	Fluid Electrification	Powers remote circuits via fluid paths; creates hazardous zones
Water	Heat	Phase Shift   Steam	Pressurizes closed pipes; actuates steam switches; spins turbines
Steam	Cold Surface	Condensation   Water	Collects water into isolated chambers across sealed barriers
Electricity	Mechanical	Actuator Activation	Opens sliding blast doors; starts motorized fluid pumps
Airflow	Steam / Gas	Directional Vector Deflection	Reroutes rising vapor away from hazards or into intake vents
Heat	Bimetallic Strip	Thermal Deflection	Closes or breaks electrical circuits based on local temperature
Airflow	Fire / Heat	Convective Thermal Spread	Extends thermal reach across open vertical shafts
8. Level Design Philosophy & Architectural Contract
Rather than designing individual levels as disconnected puzzle boxes, all stages are governed by a three-tier structural contract:
1.	Primary Objective (Non-Negotiable): A single clear physical mandate (e.g., "Fill the clawfoot tub to  ", "Power the elevator winch", "Cool the boiler chamber").
2.	Optional Performance Metric: A mastery challenge targeting precision and efficiency (e.g., "Retain   of fluid particles", "Finish within time limit", "Use zero power-ups").
3.	The Hidden Discovery: At least one buried narrative collectible or decorative antique hidden behind destructible substrate or inside an optional routing detour.
9. Progression Architecture: Macro Level Ranges & World Blueprints
To maintain clear pedagogical pacing and production discipline, stages are structured into four distinct macro level ranges across thematic worlds, rather than arbitrary ad-hoc numbers:
[ RANGE 1: TUTORIAL ] ────► [ RANGE 2: EARLY WORLD ] ──► [ RANGE 3: MID WORLD ] ──► [ RANGE 4: END WORLD ]
Levels 1–5                   Levels 6–15                 Levels 16–30               Levels 31–50+
Onboarding & Single Verbs    Mechanical Foundations      Dual-System Hazards        Thermodynamic Synthesis
Unlock: Room 1 (Workshop)    Unlock: Workshop Expansion  Unlock: Room 2 (Bathhouse) Unlock: Room 3 (Phase 2)

9.1 Range 1: Tutorial & First Contact (Levels 1–5 — "The Basics")
●	Design Purpose: Zero cognitive friction. Introduce one physical verb at a time without hazard penalties.
●	Input Focus: Tap to activate valve   Single swipe to carve soft clay   Observe fluid gravity.
●	Hazard Density: Zero ( ). No short circuits, no spill failure states.
●	Campaign Milestone: Clearing Level 5 officially unlocks Room 1 (The Workshop) and the first Curio Trophy Shelf, closing the core D1 loop within the player's first 5-minute session.
●	Blueprint Archetype (Level Range 1–5 Representative):
○	Active Systems: Pure Water + Single Mechanical Valve (  rotation).
○	Layout: Linear vertical corridor with pre-laid brass pipes terminating above a wash basin.
○	Player Action: Tap the rotary valve to release trapped water; carve a single   notch through clay insulation to guide flow into the basin.
○	Primary Metric: Deliver   fluid volume ( ).
9.2 Range 2: Early Worlds — Mechanical Foundations (Levels 6–15 — World 1)
●	Design Purpose: Introduce spatial choice, momentum, fluid splitting, and buried collectibles.
●	Environment Theme: The Master Plumber's Pantry & Laundry Cellar.
●	Mechanics Introduced: Fluid splitters, check valves, siphons, water-wheels, counterweights.
●	Hazard Density: Low. Water can be lost down floor drains, requiring conscious routing.
●	Buried Discoveries: Optional detours hide antique brass keys, copper coins, and vintage porcelain shards.
●	Campaign Milestone: Completing Level 15 unlocks the Workshop Expansion and triggers the formal appraisal review by Master Plumber Higgins.
●	Blueprint Archetype (Level Range 6–15 Representative):
○	Active Systems: Water + Multi-Valve Routing + Density Siphon.
○	Layout: Split-level basement with two destination basins and an optional side pocket.
○	Player Action: Balance fluid volume between two vats using a triangular knife divider; carve a narrow bypass channel to unearth a hidden Antique Valve Handle.
○	Primary Metric: Deliver   fluid volume ( ) across both targets.
9.3 Range 3: Mid Worlds — Dual-System Coupling & Hazards (Levels 16–30 — World 2)
●	Design Purpose: Introduce secondary physical forces (Electricity), systemic hazards, and timing synchronization.
●	Environment Theme: The Industrial Conduit & Electric Utility Basement.
●	Mechanics Introduced: Battery circuits, knife switches, circuit breakers, conductive fluid bridging, electric pumps.
●	Hazard Density: High. Uninsulated wires electrify water; circuit overloads trip master breakers; flooded switchboards cause failures.
●	Campaign Milestone: Completing Level 30 unlocks Room 2 (The Bathhouse) and introduces The Clockwork Countess.
●	Blueprint Archetype (Level Range 16–30 Representative):
○	Active Systems: Water + Electricity (Conductivity Bridging).
○	Layout: Dual-chamber circuit room with an open electrical gap between battery and centrifugal pump.
○	Player Action: Route a water stream through a ceramic conduit to pool between two open electrode terminals. The conductive fluid completes the circuit, powering the motorized pump to drain an isolated reservoir.
○	Primary Metric: Electrify pump without letting fluid spill onto the master fuse box.
9.4 Range 4: End Worlds — Full Systemic Synthesis (Levels 31–50+ — Worlds 3–5)
●	Design Purpose: Test complete mastery through multi-system thermodynamic loops, phase shifts, and state transformations.
●	Environment Theme: Victorian Sub-Basement Boiler, The Solarium, The Clockwork Estate.
●	Mechanics Introduced: Coal burners, steam phase transitions ( ), bimetallic expansion switches, directional air drafts, condensation plates.
●	Hazard Density: Maximum. Thermal explosions, steam dissipation, conflicting environmental circuits.
●	Blueprint Archetype (Level Range 31–50+ Representative):
○	Active Systems: Water + Heat + Steam + Electricity (Full Synthesis Loop).
○	Layout: Multi-chamber vertical boiler room.
○	Player Action: Route cold water into a sealed boiler chamber; close an electric arc knife switch to activate the heating burner; boil water into rising steam; use directional airflow to deflect steam onto a copper chill plate; collect condensed pure water into an elevated clawfoot tub.
○	Primary Metric: Deliver   condensed water volume ( ) while retaining   total system volume ( ).
(For the complete stage-by-stage progression breakdown, refer to MVP 30-Level Campaign & Level Design Specification:houseflow_mvp_campaign_spec.md).
10. Difficulty & World Progression Matrix
Difficulty scales through system combinations rather than control dexterity, mapped across thematic narrative worlds:
●	World 1 — The Plumbing Apprentice (Levels 1–15):
○	Focus: Water + Mechanical Fixtures (Gravity, terrain carving, rotary valves, check valves, siphons).
○	Milestone: Unlocks Room 1 (The Workshop) at Level 5; unlocks Workshop Expansion and first NPC visitor (Master Plumber Higgins) at Level 15. (MVP)
●	World 2 — Live Wire & The Conduit (Levels 16–30):
○	Focus: Electricity + Conductivity (Circuits, knife switches, conductive fluid routing, breaker overloads, motorized pumps).
○	Milestone: Unlocks Room 2 (The Bathhouse) and Curio Trophy Shelf 2 at Level 30. (MVP)
●	World 3 — The Thermodynamic Engine (Levels 31–50):
○	Focus: Heat & Steam (Boilers, phase shifts, thermal conduction, bimetallic strips, condensation cycles).
○	Milestone: Unlocks Room 3 (The Conservatory) and exterior garden facade. (Post-MVP Phase 2)
●	World 4 — Draft & Pressure (Levels 51–70):
○	Focus: Pneumatics & Airflow (Intake/exhaust fans, directional drafts, closed ducting, pneumatic flap gates, displacement). (Post-MVP Phase 3)
●	World 5+ — The Eccentric Estate (Levels 71+):
○	Focus: Full Multi-System Synthesis (Coupled 5-system physical loops with complex multi-screen machinery). (Post-MVP Phase 4)
PART III: META-GAME, CUSTOMIZATION & SOCIAL SHOWCASE
11. Collectibles & Discoveries
Discoveries are tangible objects unearthed during puzzle solutions:
●	Keep & Display: Retained permanently in the player's personal inventory. Discoveries can be mounted in the room's dedicated Curio Trophy Shelf (which holds up to 10 curios) or placed in accent furniture slots.
●	Unlock: Finding an object unlocks its related decorative collection in the catalog.
●	Collection: Completing collections grants substantial soft-currency bounties, gems, and exclusive architectural finishes (e.g., completing the Old Plumbing Collection unlocks the Vintage Victorian Tile pattern).
12. House Meta
The player's home is an interactive sandbox showroom reflecting their mechanical mastery and aesthetic journey. It serves as:
●	The Identity Hub: A personal visual expression of the player's taste.
●	The Trophy Room: Physical proof of discovered secrets and completed chapters.
●	The Social Stage: The destination for visiting NPC inspectors and asynchronous player tours.
13. House Structure & Slot Architecture
To prevent cognitive overload and avoid inventory saturation, house customization is slot-based and lightweight:
●	Starting Architecture (MVP): 1 House, 2 Distinct Rooms (Room 1: The Workshop and Room 2: The Bathhouse). Expands to Room 3: The Conservatory in Post-MVP Phase 2.
●	Anchor Slots per Room (  curated nodes):
○	Structural Nodes (3): Floor Material, Wall Finish, Window Architecture.
○	Centerpiece Node (1): Hero Appliance / Main Furniture (e.g., Antique Work-Bench, Clawfoot Tub).
○	Accent Nodes (2–3): Lighting fixture, wall art, tabletop accent, live plant.
○	The Curio Trophy Shelf (1 Dedicated Anchor): A multi-tier architectural showcase fixture in each room that holds up to 10 discovered curios simultaneously, ensuring the player can permanently display all 15 MVP curios without discarding functional furniture.
●	Catalog Scale:   high-quality, hand-crafted decorative assets at MVP launch (expanding to 60+ in Phase 2).
14. Why Decorate?
Decorating directly drives the long-term retention loop:
●	Tangible Progression: Converting puzzle rewards into visible, persistent architectural value.
●	Narrative Unfolding: Placing specific artifacts triggers lore snippets and architectural history.
●	Social Currency: Decorated rooms generate "House Acclaim" from visitors, unlocking exclusive decorative tiers.
15. Social Features
All social functionality is strictly asynchronous to minimize server infrastructure and eliminate matchmaking friction:
●	Asynchronous house visits via unique Player Codes or friend links.
●	One-tap non-toxic social endorsements ("Inspired", "Cozy", "Ingenious").
●	Daily featured community houses curated by popularity and aesthetic balance.
16. House Tours & The Cold-Start Social Solution
To prevent the "ghost-town" failure of launch-day asynchronous social systems, HOUSEFLOW introduces a staged transition:
Phase 1 (Day 1 – Launch): Quirky NPC Inhabitants
●	Eccentric virtual tenants and inspectors (e.g., Master Plumber Higgins, The Clockwork Countess) visit the player's house daily.
●	NPCs react dynamically to room themes and placed discoveries, leaving written review cards, tips, and exclusive decorative gifts.
●	Provides immediate emotional payoff and validation without relying on real player density.
Phase 2 (Post-Critical Mass): Themed Community Showcases
●	Activates automatically once active community size reaches scale (e.g., global DAU   or Day 30 post-launch).
●	Weekly community tour challenges with rotating aesthetic briefs (e.g., "Overgrown Botanical Solarium", "Cozy Rainy Night").
●	Pairwise Voting: Players review two anonymous houses and vote on their favorite, earning soft currency (Coins) and Showcase Acclaim XP to unlock prestige badges and exclusive cosmetic catalog tiers. No extraneous token currencies are introduced.
17. House Progression
House expansion is directly tethered to campaign completion and discovery milestones across world tiers:
[ Complete Tutorial Range (Level 5) ] ──► Unlock Room 1 (The Workshop) + Curio Trophy Shelf
                                                │
[ Reach Level 10 (Mid Early World) ] ───► Trigger Starter Bundle ("Welcome Home Kit")
                                                │
[ Complete World 1 (Level 15) ] ────────► Unlock Workshop Expansion + Higgins Appraisal Visit
                                                │
[ Complete World 2 (Level 30) ] ────────► Unlock Room 2 (The Bathhouse) + Countess Visit
                                                │
[ Complete World 3 (Level 50) ] ────────► Unlock Room 3 (The Conservatory) (Phase 2)


Progression unlocks expressive variety and visual space, never artificial gameplay power.
PART IV: ECONOMY, POWER-UPS & MONETIZATION
18. Economy Architecture
The economy is strictly dual-currency, transparent, and free of artificial friction:
●	Coins (Soft Currency): Earned through solving puzzles, beating optional challenges, and receiving visitor tips. Used for basic decorations, unlocking decorative slots, and standard tools.
●	Gems (Hard / Premium Currency): Purchased via IAP or awarded via major milestones/events. Used for premium thematic collections, exclusive shaders, and seasonal showcase passes.
●	Showcase Acclaim (Prestige Level): A non-spendable visual experience metric accumulated through room decoration and showcase votes. Unlocks high-tier cosmetic catalog availability without functioning as a depletable wallet currency.
●	Banned Friction Mechanics: No energy meters, no timer gates on room building, and no pay-to-win locks.
19. Power-ups & The Troubleshooter Toolkit
Power-ups are diagetic physical tools, not magical auto-solves:
1.	The Rewind Valve: Smoothly rewinds the physics simulation clock by 5 seconds to undo a spill or catastrophic circuit trip.
2.	Fix-It Tool (Patch Tape): Instantly seals one fractured pipe joint or puncture point.
3.	Vacuum Pump: Pulls water or air toward a selected nozzle location to correct misdirected fluid streams.
4.	Magnet: Relocates loose metallic objects, pipes, or valves across barriers.
5.	Architectural Blueprint (Hint): Ghost-renders an important conduit path or key interaction without auto-solving the puzzle.
20. Power-up Acquisition
●	Gameplay Earned: Distributed via first-time level clears, collection completion, and daily challenge chests.
●	Fairness Principle: Every level is mathematically verified to be solvable with 100% efficiency without using a single power-up. Power-ups represent convenience, experimentation, and recovery—never mandatory tollbooths.
21. Monetization Philosophy
"Monetize identity, pride of craftsmanship, and convenience—never puzzle fairness."
Players are never punished for failing a puzzle; monetization is entirely centered on self-expression, delightful cosmetic transformations, and high-value time-saving options.
22. Cosmetic Shop
The primary revenue engine of HOUSEFLOW:
●	Thematic Furniture Suites: Mid-Century Modern, Steampunk Workshop, Cyberpunk Micro-Loft, Overgrown Victorian Greenhouse, Japanese Onsen.
●	Architectural Finishes: Parquet hardwoods, polished concrete, exposed brick, brass fixtures.
●	Dynamic Lighting Accents: Neon wall strips, flickering gas lanterns, Edison bulbs.
23. Cosmetic Puzzle Effects
Cosmetic customization extends directly into the active puzzle simulation itself:
●	Fluid Shaders: Bioluminescent Blue, Molten Gold Water, Sparking Stardust Fluid, Dark Mineral Sludge.
●	Hardware Finishes: Weathered Patina Copper, Brushed Gunmetal, Frosted Acrylic Neon Tubing.
●	Steam Shaders: Chromatic Aberration Vapor, Rose Cloud Mist.
●	Integrity Rule: Fluid and conduit cosmetics modify only visual shaders; particle density, collision radius, and mass remain 100% identical.
24. Premium Currency Packages
Gems are distributed across clear, industry-standard tiers:
Package Tier	Gem Count	Bonus Gems	USD Reference Price
Starter Box	100	—	$0.99
Small Utility Crate	320	20	$2.99
Medium Toolbox	550	50	$4.99
Architect Trunk	1,200	200	$9.99
Master Builder Vault	2,600	600	$19.99
25. Starter Bundle: "The Welcome Home Kit"
Triggered dynamically upon completing Level 10 (midway through Early World 1, after the player has customized Room 1 since Level 5):
●	$1.99 one-time promotional conversion package.
●	Includes: 250 Gems, exclusive Gilded Vintage Clawfoot Tub, bespoke Stardust Water Particle Skin, and 3 of each Troubleshooter Tool.
26. Rewarded Video Ads
Ads are strictly opt-in and respect player agency:
●	Targeted Hint: View an ad to highlight an overlooked valve connection.
●	Emergency Rewind: View an ad to rewind state following an accidental blowout.
●	Coin Multiplier: Optional post-level 1.5x multiplier on soft currency earned.
●	Daily Visitor Gift: View an ad to double an NPC inspector's daily tip.
27. Failure Offers & Graceful Recovery
When a puzzle objective fails (e.g., reservoir runs dry):
●	Immediate, friction-free "Restart Level" button (zero ad penalty).
●	Alternative: "Use Rewind Tool" (from inventory or via rewarded ad) to step back 5 seconds prior to the failure point.
28. Tool Kits & Bundles
Convenience items are packaged into thematic bundles rather than sold as individual items:
●	Plumber's Rapid Repair Kit: 5x Rewind, 5x Fix-It ($1.99).
●	Master Troubleshooter Crate: 10x Rewind, 10x Fix-It, 5x Vacuum, 5x Magnet ($4.99).
29. Live Events
Time-limited seasonal celebrations introducing temporary puzzle packs and exclusive aesthetics:
●	The Haunted Manor (October): Ghostly green ectoplasm fluid, gothic pipe skins, haunted antique furniture.
●	The Winter Solstice (December): Freezing pipes, thermal thawing mechanics, frosted glass finishes.
30. Seasonal "House Tour Pass"
A 30-day tiered battle pass driven by daily puzzle activity and showcase acclaim:
●	Free Track: Soft currency, standard tools, common decorative fixtures.
●	Premium Track ($4.99): Exclusive room architectural skin (e.g., The Glass Solarium), animated hero furniture piece, unique liquid cosmetic shader.
31. Mystery / Reward Crates (The Curio Box)
●	Transparent, non-predatory curio chests.
●	Duplicate protection: Crates prioritize unowned cosmetic items within the selected theme.
●	Drop tables and probability weights are clearly visible in the UI.
32. "Remove Ads" Utility Purchase
●	$3.99 one-time direct purchase.
●	Completely disables any incidental ads while leaving Rewarded Ads available at the player's sole discretion for extra perks.
33. Monetization Priority Hierarchy
1.	Aesthetic Customization (Room Themes & In-Puzzle Fluid Skins) — Core Revenue Anchor.
2.	Seasonal Tour Passes (Battle Pass) — Mid-Core Retention Driver.
3.	Curated Convenience Tool Bundles — Low-Friction Spender Conversion.
4.	Opt-in Rewarded Ads — Free-Player Monetization.
PART V: TECHNICAL ARCHITECTURE, ENGINE & SETTINGS
34. Scalability Strategy & Data-Driven Architecture
To allow non-technical level designers to produce 100+ levels without engine re-compilation, game logic is completely decoupled from content definitions via structured JSON data files. Both introductory tutorial levels and complex end-world synthesis benchmarks parse through the same deterministic schema:
{
  "levelId": "lvl_tier4_boiler_synthesis",
  "worldTier": "EndWorld_Victorian_Basement",
  "physicsLimits": {
    "maxActiveParticles": 200,
    "simulationHz": 60
  },
  "primaryObjective": {
    "type": "DeliverFluidToContainer",
    "targetTag": "Tub_01",
    "fluidType": "Water_Condensed",
    "requiredParticleCount": 140
  },
  "optionalObjectives": [
    { "type": "MaxSpillParticleCount", "threshold": 10 },
    { "type": "CollectArtifact", "artifactId": "art_antique_brass_gauge" }
  ],
  "allowedTools": ["Valve_Turn", "Wire_Connect", "Dirt_Carve"]
}

(For complete C++ particle memory layout, Clipper terrain subtraction calls, and telemetry schemas, refer to Appendix A: Engineering Implementation Notes:houseflow_appendix_a_engineering.md).
35. Puzzle Engine Architecture & Physics Model
To deliver smooth physical simulation on low-end mobile devices without thermal throttling:
35.1 World-to-Physics Metric Standards
To avoid ambiguity between volumetric quantities, screen density, and Box2D physics meters:
  All level design benchmarks are expressed either as percentage volume ( ) or discrete particle thresholds ( ).
35.2 The Box2D + Screen-Space Metaball Model
●	Rigid-Body Particle Pool: Capped pool of   Box2D circular rigid bodies per level. These interact natively with levers, hinged valves, and floating switches without custom fluid-coupling code.
●	Screen-Space Metaball Rendering:
 Drops visually merge when adjacent, creating smooth surface tension and organic fluid flow at near-zero GPU cost.
●	Destructible Terrain: 2D polygon clipping (Clipper library) generating static 2D edge colliders dynamically along carved contours.
35.3 State Transitions
●	Thermal Phase Transition: When a water particle reaches  , its gravityScale transitions smoothly from   to  , and its visual tag swaps to the translucent steam shader.
●	Conductivity Propagation: When an uninsulated wire contact touches water, an electrification bitmask propagates across adjacent particles ( ) via a bounded breadth-first search queue (max 200 checks/frame).
36. Content Scalability Vectors
New content can be produced along 4 independent axes:
1.	New Physical Combinations: Pairing existing systems in novel configurations (e.g., Steam + Airflow).
2.	New Mechanical Fixtures: Bimetallic valves, siphon tubes, pneumatic lifters.
3.	New Architectural Environments: Victorian Manor, Industrial Brewery, Hydroelectric Dam, Retro-Futurist Submarine.
4.	New Objective Modes: Filtration, cooling boilers, maintaining pressure equilibrium.
37. Daily Puzzles: "The Morning Routine"
●	A standalone daily puzzle refreshed every 24 hours.
●	Employs unique modular modifier twists (e.g., "Inverted Gravity Day", "Super-Conductive Pipelines").
●	Rewards daily login streaks with exclusive blueprint fragments and curio crate keys.
38. Achievements & Mastery Badges
Tied to gameplay ingenuity rather than mundane grinding:
●	Clean Plumber: Solve 15 levels without losing a single drop of fluid.
●	Superconductor: Electrify 4 separate mechanisms simultaneously using a single fluid stream.
●	Archival Curator: Unearth 100% of hidden discoveries across Early World 1 and Mid World 2.
●	Master Restorer: Fully furnish 2 complete rooms to maximum acclaim.
39. Collections System
Collections bridge the campaign puzzles and the home meta:
●	The Vintage Plumber Set: Brass Faucet, Cast-Iron Valve, Pressure Gauge, Solder Torch.
●	The Hydroponic Gardener Set: Copper Mist Nozzle, Glass Terrarium, Pruning Shears, Rare Spore.
●	Reward: Assembling a full collection grants an exclusive room wallpaper and a permanent boost to visitor tips.
40. Player Retention Loop
           ┌────────────────────────────────────────┐
           │        DAILY ENGAGEMENT (5 min)        │
           │  • Complete Daily Puzzle               │
           │  • Collect NPC Visitor Tips & Reviews  │
           └───────────────────┬────────────────────┘
                               │
                               ▼
           ┌────────────────────────────────────────┐
           │        CAMPAIGN PROGRESSION (15 min)   │
           │  • Advance 3–5 Stages in Current World │
           │  • Discover Buried Artifacts           │
           │  • Earn Soft Currency & Blueprints     │
           └───────────────────┬────────────────────┘
                               │
                               ▼
           ┌────────────────────────────────────────┐
           │       META-CUSTOMIZATION (5 min)       │
           │  • Display Curios on Trophy Shelf      │
           │  • Furnish Rooms with New Finds        │
           │  • Submit House to Weekly Showcase     │
           └────────────────────────────────────────┘


41. Player Personas & Cohort Support
●	The Logical Solver: Prioritizes 3-star optimization, zero-spill solutions, and minimal tool usage.
●	The Archival Collector: Driven by unearthing every buried secret and filling their Curio Trophy Shelves.
●	The Interior Stylist: Motivated by house customization, color palettes, and social showcase ratings.
●	The Snack Casual: Plays 1–2 levels during short commutes; benefits from instant loading and clear objectives.
42. Session Design & Flow
●	Target Session Length:  .
●	Loading Speed Target:   cold-start to playable room.
●	Micro-Pacing: Each individual puzzle is solvable within   once the solution is identified.
43. Mobile UX & Ergonomic Design
●	Thumb-Zone Optimization: Critical HUD controls (Reset, Rewind, Toolkit) are pinned to the lower ergonomic thumb arcs.
●	Left/Right Hand Mirroring: Toggleable HUD orientation to prevent hand occlusion during carving.
●	Haptic Confirmation: Crisp, subtle haptic pulses when valves snap into detents or when electrical circuits close.
44. Accessibility & Inclusive Design (A11y)
●	Color Independence Rule: Gameplay information is never conveyed exclusively through color:
○	Electricity: Marked with animated directional electrical arcs and chevrons.
○	Hot vs. Cold: Marked with animated heat distortion shimmers vs. crystalline frost decals.
●	Colorblind Profiles: Specialized palette presets for Deuteranopia, Protanopia, and Tritanopia.
●	Reduced Motion: Toggles off camera screen-shake and dampens high-voltage electrical flashes.
45. Technical Portfolio Value
●	Physics & Mathematics: Bounded Box2D rigid-body particle simulation, custom Verlet fluid shaders, 2D polygon clipping geometry.
●	Mobile Optimization: Dynamic draw-call batching, render-target downsampling, memory pooling for zero-garbage collection during gameplay.
●	Software Architecture: Pure Data-Driven Level Pipeline (JSON), decoupled Event Bus, asynchronous Cloud Save architecture.
46. Telemetry & Analytics Schema
Granular tracking to identify drop-off and level difficulty spikes:
●	puzzle_start / puzzle_complete / puzzle_retry: Tracks duration, attempt counts, and fluid spill percentages across World Tiers.
●	tool_used: Logs specific power-up triggers to diagnose puzzle difficulty imbalances.
●	discovery_found: Measures exploration conversion per stage.
●	room_customized: Tracks cosmetic engagement and slot popularity.
47. Hardware Performance & Settings Architecture
47.1 Thermal & Frame Rate Profiles
●	60 FPS (Default High Fidelity): Full   physics sub-stepping with screen-space Gaussian metaball fluid rendering.
●	30 FPS (Thermal / Battery Saver): Drops physics and render clocks to  , downscaling off-screen metaball render targets to   resolution.
●	Low-End Fallback Shader: Automatically detects low-tier GPUs (e.g., Mali-G52 or older) and replaces multi-pass metaball blur with unblurred specular disc particles.
●	Dynamic Particle Clamp: Automatically adjusts particle pools by 25% on detected budget chipsets (  particles), dynamically scaling target volume objectives ( ).
47.2 Audio & Haptic Controls
●	Five-channel independent audio mixer: Master, Music, SFX, Atmospheric Ambience, and Tactile Material Audio (valve clicks, dirt carving rasps, fluid slosh).
●	Three-tier haptic intensity toggle: Off / Subtle / Crisp.
PART VI: PRODUCTION, ROADMAP & INTEGRITY
48. Example Player Journey
●	Day 1 (Tutorial Range 1 & Early World 1): Installs game   Masters fluid carving (Levels 1–5)   Completes Level 5 and unlocks Room 1 (The Workshop) + first Curio Trophy Shelf   Advances into Early World 1 (Levels 6–10)   Discovers Old Copper Coin on Level 8   Mounts coin on Trophy Shelf   Reaches Level 10 and encounters the $1.99 Welcome Home Kit offer.
●	Day 2 (World 1 Mastery): Advances through Levels 11–15   Solves Level 15 (Boss: The Manor Boiler)   Unlocks Workshop Expansion   Receives first formal appraisal visit and tip from Master Plumber Higgins.
●	Day 3 (Mid World 2): Enters Mid World 2 (Levels 16+)   Learns water conductivity   Rewinds first accidental short-circuit   Solves stage.
●	Day 7 (World 2 Completion): Solves Level 30 (Finale: The Countess's Clock)   Unlocks Room 2 (The Bathhouse) and Curio Trophy Shelf 2   Receives showcase visit from The Clockwork Countess   Earns Acclaim Badge.
49. MVP Scope (Minimum Viable Product)
The initial vertical slice is strictly scoped:
●	Core Physics: Water + Mechanical (Pipes, Valves, Switches) + Carvable Soil + Electricity.
●	Content: 30 handcrafted levels across Tutorial Range (Levels 1–5), Early World 1 (Levels 6–15), and Mid World 2 (Levels 16–30).
●	Meta: 1 House, 2 customizable rooms (The Workshop and The Bathhouse), 2 Curio Trophy Shelves, 25 decorative assets.
●	Monetization Prototype: Gem shop, 1 Starter Bundle, Rewarded Video Ads for Hints/Rewind.
●	Social Prototype: NPC Visitor system (simulated social validation via Higgins & Countess).
50. Post-MVP Roadmap
●	Phase 2 (Post-Launch Month 1): Heat & Steam mechanics, World 3: The Thermodynamic Engine (Levels 31–50), Room 3 unlock (The Conservatory), True asynchronous community voting.
●	Phase 3 (Month 3): Pneumatics & Airflow systems, World 4: Draft & Pressure (Levels 51–70), Seasonal House Tour Pass rollout, Event system (The Haunted Manor).
●	Phase 4 (Month 6): Advanced cosmetics (animated wallpapers, soundscape packs), expanded neighborhood architectural styles (Industrial Brewery, Retro Submarine, Levels 71+).
51. UGC / Level Editor Pipeline (Long-Term Scalability)
An internal web-based or in-app node editor enabling players to build and publish levels:
1.	Drag-and-drop structural placement (Pipes, Valves, Basins).
2.	Carvable substrate boundary drawing.
3.	Automated validation test: Headless simulation runs to ensure level is physically solvable before publication.
4.	Curated community spotlight playlists.
52. Explicit Design Constraints & Anti-Patterns
●	NO Combinatorial Explosion: Systems interact only through the established matrix.
●	NO Pay-to-Win Gates: Levels are never designed around mandatory power-up consumption.
●	NO Energy Timers: Players can attempt puzzles indefinitely without artificial lockouts.
●	NO Intrusive Forced Interstitials: Zero unprompted popup ads during active problem solving.
●	NO Content-Tied Code Bloat: Adding new puzzles requires only JSON configuration, never engine re-compilation. 
Thẻ 2 
Appendix A: Engineering Implementation Notes
Document Version: 1.1
Target Disciplines: Systems Engineering, Physics Programming, Graphics/Tech Art, Tools & Infrastructure
Companion Spec: HOUSEFLOW — Comprehensive Master Game Design Document (v1.5)
1. Physical Units & Volumetric Constants
To guarantee deterministic simulation and clear engineering contracts:
Standard Unit	Engine Equivalent	Definition / Conversion
World Distance	$1.0\text{ Box2D Meter}$	$100\text{ Screen DP}$ (Standard resolution baseline)
Discrete Particle Pool	$N = 200\text{ Bodies}$	Hard clamp per level scene ($150$ on budget chipsets)
Particle Volume	$1\text{ Fluid Particle}$	$0.005\text{ Liters}$ ($200\text{ particles} = 1.0\text{ Liter Total Pool}$)
Airflow Propagation	$3.5\text{ Meters}$	$350\text{ DP}$ maximum draft attenuation distance
Touch Raycast Target	$48 \times 48\text{ DP}$	Interactive widget minimum collider boundary
2. Simulation & Particle State Engine
2.1 Box2D Rigid Particle Structure
To balance memory cache locality, SIMD batching, and lightweight memory overhead across heterogeneous mobile hardware (ARMv7/ARM64), particle states are stored in contiguous memory arrays rather than individual heap objects.

#pragma pack(push, 1)
struct FluidParticleData {
    uint32_t particleId;       // Unique ID mapping to Box2D body pointer
    uint8_t  substanceType;    // 0: Water, 1: Steam, 2: Sludge, 3: Oil
    int16_t  temperatureC;     // Signed Celcius (-50 to +500)
    uint8_t  flags;            // Bitmask: [0: isElectrified, 1: isConductive, 2: isFrozen, 3: isBoiling]
    float    pressureNorm;     // Normalized pressure [0.0 - 1.0] for dispersion
    float    lifetimeSec;      // Evaporation / dissipation timer (Steam/Gas)
};
#pragma pack(pop)

2.2 State Transition State Machine
The core engine executes state evaluations at a fixed $60\text{ Hz}$ tick (FixedUpdate in Unity / custom sub-step loop in Box2D):

                      [ Water Particle ]
                      (density: 1000 kg/m³)
                         (gravity: +1.0)
                                │
               + Heat Zone      │      + Conductive Wire Contact
              (T >= 100°C)      │      (V >= 12V)
                                │
                                ▼
                       [ State Evaluation ]
                                │
            ┌───────────────────┴───────────────────┐
            ▼                                       ▼
    [ Steam Transition ]                    [ Electrification ]
  • gravityScale: -0.6                    • flags |= FLAG_ELECTRIFIED
  • linearDamping: 1.8                    • visualShaderTag = "plasma_spark"
  • collisionMask: Layer_Vents            • Propagate to adjacent water (r <= 1.2R)
  • lifetime: 6.0s (decay to 0)           • Duration: Active while source closed

2.3 Fluid Electrification Propagation Algorithm
To prevent recursive stack overflows when electrifying fluid clusters:
1.	When an electrified contact triggers an OnCollisionEnter2D on any fluid particle, that particle is enqueued into a breadth-first search (BFS) queue.
2.	The engine traverses nearby neighbors within a radius $R_{\text{electrify}} = 1.2 \times \text{particleRadius}$.
3.	Neighbor particles update flags |= FLAG_ELECTRIFIED up to a hard cap of 200 checks per physics frame, dampening CPU spikes.
3. Rendering Pipeline: Screen-Space Metaball Shader
Step 1: Render Discs
●	Draw Box2D particles as unlit white discs with soft falloff to an off-screen render texture (Target RT).
●	Downsample: 0.5x resolution (for Thermal Mode) or 1.0x (High Fidelity).
Step 2: Dual Gaussian Blur Pass
●	Horizontal & Vertical separable 9-tap Gaussian blur on Target RT.
●	Smooths out individual particle outlines into a cohesive density field.
Step 3: Color Ramp & Threshold Pass (Fragment Shader)
Sample density alpha A(u, v):
●	If A(u, v) < Threshold_Min (0.60): Discard (Empty space).
●	If Threshold_Min <= A(u, v) < Threshold_Border (0.72): Render Outer Meniscus / Specular Edge Highlight.
If A(u, v) >= Threshold_Border: Sample fluid color/gradient ramp + dynamic refraction normal.

3.1 Hardware Fallback Protocol
On legacy mobile GPUs (e.g., ARM Mali-400, Mali-T830, or Adreno 308/505):
●	Pass 2 & Pass 3 are bypassed completely.
●	Particles fall back to instanced alpha-blended specular sprites with slight overlapping depth offsets to guarantee a stable 60 FPS.
4. Destructible Terrain Subtraction Pipeline (Clipper Integration)
1.	User Input: Swipe gesture generates a line strip with brush width $W_{\text{carve}}$ (default $48\text{ dp}$).
2.	Polygon Construction: Inflate the line strip into a series of convex 2D bounding quads.
3.	Boolean Subtraction:
4.	
ClipperLib.Clipper clipper = new ClipperLib.Clipper();
clipper.AddPaths(currentTerrainPolygons, PolyType.ptSubject, true);
clipper.AddPaths(brushCarvePolygons, PolyType.ptClip, true);
clipper.Execute(ClipType.ctDifference, out solutionPolygons);
5.	Collider Generation: Convert resulting polygon boundaries to 2D Edge Colliders. Recalculate edge normals strictly within the modified bounding box to prevent full-level geometry rebuilds.
5. Airflow & Containment Physics Rules
●	Closed Ducts: Flow velocity $\vec{v}_{\text{air}}$ inside rigid ducting is modeled as a 1D vector line segment with linear velocity dampening:

 $$\vec{v}_{\text{out}} = \vec{v}_{\text{in}} \cdot \left(1 - \mu \frac{L}{D}\right)$$
 (where $L$ is duct length, $D$ is hydraulic diameter, and $\mu$ is friction factor).
●	Ambient Dissipation: When airflow leaves a pipe or open vent into unconstrained rooms, it transitions into a conical vector field with a $35^\circ$ spread angle, attenuating to $0\text{ m/s}$ within $3.5\text{ meters}$ ($350\text{ dp}$) of world space.
6. Telemetry & Analytics Event Payloads

{
  "event_name": "puzzle_completed",
  "payload": {
    "level_id": "lvl_018_steam_valve",
    "session_duration_sec": 74.2,
    "attempt_count": 2,
    "tools_used": {
      "rewind": 1,
      "fix_it": 0,
      "vacuum": 0
    },
    "water_particles_delivered": 176,
    "water_delivered_ratio": 0.88,
    "discovery_unlocked": "brass_clockwork_cog",
    "frame_rate_avg": 59.4,
    "thermal_profile_active": "60fps_high"
  }
}


