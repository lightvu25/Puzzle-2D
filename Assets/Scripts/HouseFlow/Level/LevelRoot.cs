using UnityEngine;
using HouseFlow.Fluid;
using HouseFlow.Mechanical;
using HouseFlow.Thermal;

namespace HouseFlow.Level
{
    /// <summary>
    /// Marker and coordinator for a HOUSEFLOW Layout Prefab.
    /// This component MUST be on the root GameObject of every Layout Prefab.
    ///
    /// Responsibilities:
    ///   - Cache all puzzle objects in the layout at Awake.
    ///   - Provide controlled access to WaterSources, FluidTargets, and Valves.
    ///   - Coordinate level-wide reset without knowing about LevelLoader or LevelData.
    /// </summary>
    public class LevelRoot : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────
        //  Cached References (populated at Awake)
        // ─────────────────────────────────────────────────────────────

        private WaterSource[]          waterSources;
        private FluidTarget[]          fluidTargets;
        private Valve[]                valves;
        private HeatSource[]           heatSources;
        private AirflowSource[]        airflowSources;
        private CoolingSource[]        coolingSources;
        private ThermalBody[]          thermalBodies;
        private PneumaticGate[]        pneumaticGates;
        private DuctSegment[]          ductSegments;
        private Balloon[]              balloons;
        private CounterweightBladder[] counterweightBladders;
        private HouseFlow.Electricity.PowerSource[]      powerSources;
        private HouseFlow.Electricity.ElectricSwitch[]   electricSwitches;
        private HouseFlow.Electricity.CircuitBreaker[]   circuitBreakers;
        private HouseFlow.Electricity.ElectricTerminal[] electricTerminals;
        private HouseFlow.Electricity.ElectricPump[]       electricPumps;

        // ─────────────────────────────────────────────────────────────
        //  Public Accessors
        // ─────────────────────────────────────────────────────────────

        public WaterSource[]          WaterSources          => waterSources;
        public FluidTarget[]          FluidTargets          => fluidTargets;
        public Valve[]                Valves                => valves;
        public HeatSource[]           HeatSources           => heatSources;
        public AirflowSource[]        AirflowSources        => airflowSources;
        public CoolingSource[]        CoolingSources        => coolingSources;
        public ThermalBody[]          ThermalBodies         => thermalBodies;
        public PneumaticGate[]        PneumaticGates        => pneumaticGates;
        public DuctSegment[]          DuctSegments          => ductSegments;
        public Balloon[]              Balloons              => balloons;
        public CounterweightBladder[] CounterweightBladders => counterweightBladders;
        public HouseFlow.Electricity.PowerSource[]      PowerSources      => powerSources;
        public HouseFlow.Electricity.ElectricSwitch[]   ElectricSwitches  => electricSwitches;
        public HouseFlow.Electricity.CircuitBreaker[]   CircuitBreakers   => circuitBreakers;
        public HouseFlow.Electricity.ElectricTerminal[] ElectricTerminals => electricTerminals;
        public HouseFlow.Electricity.ElectricPump[]       ElectricPumps     => electricPumps;

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (waterSources != null) return;

            waterSources          = GetComponentsInChildren<WaterSource>(includeInactive: true);
            fluidTargets          = GetComponentsInChildren<FluidTarget>(includeInactive: true);
            valves                = GetComponentsInChildren<Valve>(includeInactive: true);
            heatSources           = GetComponentsInChildren<HeatSource>(includeInactive: true);
            airflowSources        = GetComponentsInChildren<AirflowSource>(includeInactive: true);
            coolingSources        = GetComponentsInChildren<CoolingSource>(includeInactive: true);
            thermalBodies         = GetComponentsInChildren<ThermalBody>(includeInactive: true);
            pneumaticGates        = GetComponentsInChildren<PneumaticGate>(includeInactive: true);
            ductSegments          = GetComponentsInChildren<DuctSegment>(includeInactive: true);
            balloons              = GetComponentsInChildren<Balloon>(includeInactive: true);
            counterweightBladders = GetComponentsInChildren<CounterweightBladder>(includeInactive: true);
            powerSources          = GetComponentsInChildren<HouseFlow.Electricity.PowerSource>(includeInactive: true);
            electricSwitches      = GetComponentsInChildren<HouseFlow.Electricity.ElectricSwitch>(includeInactive: true);
            circuitBreakers       = GetComponentsInChildren<HouseFlow.Electricity.CircuitBreaker>(includeInactive: true);
            electricTerminals     = GetComponentsInChildren<HouseFlow.Electricity.ElectricTerminal>(includeInactive: true);
            electricPumps         = GetComponentsInChildren<HouseFlow.Electricity.ElectricPump>(includeInactive: true);
        }

        // ─────────────────────────────────────────────────────────────
        //  Runtime State
        // ─────────────────────────────────────────────────────────────
        
        private LevelPhysicsConfig currentConfig;
        
        public LevelPhysicsConfig PhysicsConfig => currentConfig;

        // ─────────────────────────────────────────────────────────────
        //  Initialisation
        // ─────────────────────────────────────────────────────────────

        public void InitializePhysics(LevelPhysicsConfig config)
        {
            currentConfig = config;
            foreach (var source in waterSources)
            {
                source.InitializeSource(config);
            }
            
            // Initialize static thermal bodies (like Boilers and Bimetallic Strips)
            foreach (var tb in thermalBodies)
            {
                // Note: FluidParticles re-initialize their own ThermalBody on Spawn()
                if (tb.GetComponent<FluidParticle>() == null)
                {
                    tb.Initialize(config.ambientTemperature, config.defaultFluidCoolingRate);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Level-Wide Reset
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Resets every puzzle object in this layout to its initial state.
        /// Called by LevelFlowController.ResetLevel().
        /// </summary>
        public void ResetAll()
        {
            EnsureInitialized();

            // Note: Currently active particles need to be returned to pool.
            // Since we use the generic ObjectPoolManager, we can find all active FluidParticles in the scene.
            var activeParticles = FindObjectsByType<FluidParticle>(FindObjectsSortMode.None);
            foreach(var particle in activeParticles)
            {
                if (particle.gameObject.activeInHierarchy)
                {
                    particle.ReturnToPool();
                }
            }

            // 2. Core Mechanics Reset
            foreach (var source in waterSources)
                source.ResetSource();

            foreach (var valve in valves)
                valve.ResetValve();

            foreach (var target in fluidTargets)
                target.ResetTarget();

            foreach (var heater in heatSources)
                heater.ResetSource();

            foreach (var cooler in coolingSources)
                cooler.ResetSource();

            // 3. Pneumatic Mechanics Reset
            // Reset gates first so blockers are in correct state
            foreach (var gate in pneumaticGates)
                gate.ResetGate();

            // Reset fans next. This will trigger OnFlowChanged cascades down the DuctSegments.
            foreach (var fan in airflowSources)
                fan.ResetSource();

            // As a fallback guarantee, forcibly recalculate all ducts after gates and fans are reset
            foreach (var duct in ductSegments)
                duct.ForceRecalculate();

            // Reset free-floating inflatables
            foreach (var balloon in balloons)
                balloon.ResetBalloon();

            // Reset bladders
            foreach (var bladder in counterweightBladders)
                bladder.ResetBladder();

            // 4. Electricity Mechanics Reset
            foreach (var breaker in circuitBreakers)
                breaker.ResetBreaker();

            foreach (var sw in electricSwitches)
                sw.ResetSwitch();

            foreach (var src in powerSources)
                src.ResetSource();

            foreach (var term in electricTerminals)
                term.ResetTerminal();

            foreach (var pump in electricPumps)
                pump.ResetPump();

            // 5. Reset static thermal bodies and their attached mechanics
            foreach (var tb in thermalBodies)
            {
                if (tb.GetComponent<FluidParticle>() == null)
                {
                    tb.Initialize(currentConfig.ambientTemperature, currentConfig.defaultFluidCoolingRate);
                    
                    var boiler = tb.GetComponent<Boiler>();
                    if (boiler != null) boiler.ResetBoiler();
                    
                    // Note: BimetallicStrip handles its own animation based on temperature,
                    // so simply resetting its ThermalBody (above) will naturally return it to ambient position.
                }
            }
        }

        /// <summary>
        /// Finds the FluidTarget with the given targetId. Returns null if not found.
        /// Used by the objective system to resolve target references.
        /// </summary>
        public FluidTarget FindTargetById(string targetId)
        {
            EnsureInitialized();

            foreach (var target in fluidTargets)
            {
                if (target.TargetId == targetId)
                    return target;
            }
            return null;
        }
    }
}
