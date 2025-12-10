using System.Collections.Generic;
using System.Linq;
using Core.Localization;
using Game.Orchestration;
using MonoMod.RuntimeDetour;
using ShapezShifter.Flow;
using ShapezShifter.Flow.Atomic;
using ShapezShifter.Flow.Research;
using ShapezShifter.Flow.Toolbar;
using ShapezShifter.Kit;
using ShapezShifter.SharpDetour;
using ShapezShifter.Textures;
using UnityEngine;
using ILogger = Core.Logging.ILogger;

namespace NandMod;

public class Main : IMod
{
    private readonly BuildingDefinitionId _defId = new("nand");
    private readonly BuildingDefinitionGroupId _groupId = new("nandgroup");
    private readonly Hook _keyboardHook;
    private readonly Hook _keyboardHook2;
    private readonly IToolbarEntryInsertLocation _location =
        ToolbarElementLocator.Root().ChildAt(2).ChildAt(6).ChildAt(^1).InsertAfter();
    protected readonly ILogger _logger;
    private readonly Hook _modSystemHook;
    private readonly Hook _updateHook;
    private readonly string _bind = "nandmod.my-custom-action"; 

    public Main(ILogger logger)
    {
        _logger = logger;
        var res = ModDirectoryLocator.CreateLocator<Main>().SubLocator("Resources");
        var bldingGroup = BuildingGroup.Create(_groupId)
            .WithTitle("building-variant.nand-gate.title".T())
            .WithDescription("building-variant.nand-gate.description".T())
            .WithIcon(FileTextureLoader.LoadTextureAsSprite(res.SubPath("icon.png"), out _))
            .AsNonTransportableBuilding()
            .WithPreferredPlacement(DefaultPreferredPlacementMode.Single)
            .AutoConnected();

        var connectorData = BuildingConnectors.SingleTile()
            .AddWireInput(WireConnectorConfig.CustomInput(TileDirection.South))
            .AddWireInput(WireConnectorConfig.CustomInput(TileDirection.North))
            .AddWireOutput(WireConnectorConfig.CustomOutput(TileDirection.East))
            .Build();

        var blding = Building.Create(_defId)
            .WithConnectorData(connectorData)
            .DynamicallyRendering<NAndGateSimulationRenderer, NAndGateSimulation, INAndGateDrawData>(
                new NAndGateDrawData())
            .WithStaticDrawData(NAndGateDrawData.CreateMeshDrawData(res))
            .WithoutPrediction()
            .WithoutSound()
            .WithoutSimulationConfiguration()
            .WithEfficiencyData(new BuildingEfficiencyData(2, 1));

        AtomicBuildings.Extend()
            .AllScenarios()
            .WithBuilding(blding, bldingGroup)
            .UnlockedAtMilestone(new ByIndexMilestoneSelector(2))
            .WithDefaultPlacement()
            .InToolbar(_location)
            .WithSimulation(new NAndGateFactoryBuilder())
            .WithCustomModules(new NAndGateModuleDataProvider())
            .Build();

        _modSystemHook = DetourHelper.CreatePostfixHook<BuiltinSimulationSystems, IEnumerable<ISimulationSystem>>(
            simulationSystems => simulationSystems.CreateSimulationSystems(),
            CreateModSystems);

        _updateHook = DetourHelper.CreatePostfixHook<GameSessionOrchestrator, float>(
            (orchestrator, time) => orchestrator.Tick(time),
            Update);
    }

    public void Dispose()
    {
        _modSystemHook.Dispose();
        _updateHook.Dispose();
        _keyboardHook.Dispose();
    }
    static bool logged = false;

    private void Update(GameSessionOrchestrator orchestrator, float time)
    {
        if (!logged)
        {
            logged = true;
            foreach (var key in orchestrator.InputManager.Keybindings.KeybindingsById.Keys)
            {
                _logger.Info?.Log($"Keybinding: {key}");
            }
        }
        if (orchestrator.InputManager.DownstreamContext.IsActive(_bind))
            _logger.Info?.Log("Pressed key yay isActive RAPD");
        if (orchestrator.InputManager.DownstreamContext.IsActivated(_bind))
            _logger.Info?.Log("Pressed key yay isActivated RAPD");
        if (orchestrator.InputManager.DownstreamContext.ConsumeIsActive(_bind))
            _logger.Info?.Log("Pressed key yay consumeIsActive RAPD");
        if (orchestrator.InputManager.DownstreamContext.ConsumeWasActivated(_bind))
            _logger.Info?.Log("Pressed key yay consumeWasActivated RAPD");
    }

    private IEnumerable<ISimulationSystem> CreateModSystems(
        BuiltinSimulationSystems builtinSimulationSystems,
        IEnumerable<ISimulationSystem> systems)
    {
        return systems.Append(
            new AtomicStatefulBuildingSimulationSystem<NAndGateSimulation, LogicGate2In1OutSimulationState>(
                new NAndGateSimulationFactory(), _defId));
    }

    private KeybindingsLayer[] SetupKeybindings(KeybindingsLayer[] layers)
    {
        return layers.Append(new KeybindingsLayer(_bind.Split(".")[0], [
            new Keybinding(
                _bind.Split(".")[1],
                new KeySet(KeyCode.F8),
                new KeySet(),
                false,
                false,
                0.1f
            )
        ])).ToArray();
    }
    private void PostKeybindingsConstruction(Keybindings keybindings)
    {
        // Manually add your keybinding to the dictionary after construction
        var binding = new Keybinding(
            "my-custom-action",
            new KeySet(KeyCode.F8),
            blockableByUI: false
        );
        
        // Manually assign the full ID and load it
        binding.AssignFullIdAndLoad(_bind);
        
        // Add to the dictionary
        if (!keybindings.KeybindingsById.ContainsKey(_bind))
        {
            // You'll need to access the private _KeybindingsById field via reflection
            var field = typeof(Keybindings).GetField("_KeybindingsById", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var dict = (Dictionary<string, Keybinding>)field.GetValue(keybindings);
            dict.Add(_bind, binding);
            
            _logger.Info?.Log($"Added keybinding: {_bind}");
        }
    }
}