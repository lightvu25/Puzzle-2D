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

        private WaterSource[]   waterSources;
        private FluidTarget[]   fluidTargets;
        private Valve[]         valves;
        private HeatSource[]    heatSources;
        private AirflowSource[] airflowSources;
        private CoolingSource[] coolingSources;
        private ThermalBody[]   thermalBodies;

        // ─────────────────────────────────────────────────────────────
        //  Public Accessors
        // ─────────────────────────────────────────────────────────────

        public WaterSource[]    WaterSources   => waterSources;
        public FluidTarget[]    FluidTargets   => fluidTargets;
        public Valve[]          Valves         => valves;
        public HeatSource[]     HeatSources    => heatSources;
        public AirflowSource[]  AirflowSources => airflowSources;
        public CoolingSource[]  CoolingSources => coolingSources;
        public ThermalBody[]    ThermalBodies  => thermalBodies;

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            waterSources   = GetComponentsInChildren<WaterSource>(includeInactive: true);
            fluidTargets   = GetComponentsInChildren<FluidTarget>(includeInactive: true);
            valves         = GetComponentsInChildren<Valve>(includeInactive: true);
            heatSources    = GetComponentsInChildren<HeatSource>(includeInactive: true);
            airflowSources = GetComponentsInChildren<AirflowSource>(includeInactive: true);
            coolingSources = GetComponentsInChildren<CoolingSource>(includeInactive: true);
            thermalBodies  = GetComponentsInChildren<ThermalBody>(includeInactive: true);
        }

        // ─────────────────────────────────────────────────────────────
        //  Runtime State
        // ─────────────────────────────────────────────────────────────
        
        private LevelPhysicsConfig currentConfig;

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

            foreach (var source in waterSources)
                source.ResetSource();

            foreach (var valve in valves)
                valve.ResetValve();

            foreach (var target in fluidTargets)
                target.ResetTarget();

            foreach (var heater in heatSources)
                heater.ResetSource();

            foreach (var fan in airflowSources)
                fan.ResetSource();

            foreach (var cooler in coolingSources)
                cooler.ResetSource();

            // Reset static thermal bodies and their attached mechanics
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
            foreach (var target in fluidTargets)
            {
                if (target.TargetId == targetId)
                    return target;
            }
            return null;
        }
    }
}
