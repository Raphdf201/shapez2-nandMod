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

namespace SignalApi;

public class Main : IMod
{
    private readonly Hook _modSystemHook;
    private readonly BuildingDefinitionId _defId = new("nand");
    private readonly BuildingDefinitionGroupId _groupId = new("nandgroup");

    private readonly IToolbarEntryInsertLocation _location =
        ToolbarElementLocator.Root().ChildAt(2).ChildAt(6).ChildAt(^1).InsertAfter();
    //                               Wires tab      Logic gates

    public Main()
    {
        ModFolderLocator res = ModDirectoryLocator.CreateLocator<Main>().SubLocator("Resources");
        IBuildingGroupBuilder bldingGroup = BuildingGroup.Create(_groupId)
            .WithTitle("nand".T())
            .WithDescription("this is a nand gate".T())
            .WithIcon(FileTextureLoader.LoadTextureAsSprite(res.SubPath("icon.png"), out _))
            .AsNonTransportableBuilding()
            .WithPreferredPlacement(DefaultPreferredPlacementMode.Single)
            .AutoConnected();

        IBuildingConnectorData connectorData = BuildingConnectors.SingleTile()
            .AddWireInput(WireConnectorConfig.CustomInput(TileDirection.South))
            .AddWireInput(WireConnectorConfig.CustomInput(TileDirection.North))
            .AddWireOutput(WireConnectorConfig.CustomOutput(TileDirection.East))
            .Build();

        IBuildingBuilder blding = Building.Create(_defId)
            .WithConnectorData(connectorData)
            .DynamicallyRendering<NAndGateSimulationRenderer, NAndGateSimulation, INAndGateDrawData>(new NAndGateDrawData())
            .WithStaticDrawData(CreateDrawData())
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

        _modSystemHook = DetourHelper
            .CreatePostfixHook<BuiltinSimulationSystems, IEnumerable<ISimulationSystem>>(
                simulationSystems => simulationSystems.CreateSimulationSystems(),
                CreateModSystems);
    }

    public void Dispose()
    {
        _modSystemHook.Dispose();
    }

    private IEnumerable<ISimulationSystem> CreateModSystems(
        BuiltinSimulationSystems builtinSimulationSystems,
        IEnumerable<ISimulationSystem> systems)
    {
        return systems.Append(new AtomicStatefulBuildingSimulationSystem<NAndGateSimulation, LogicGate2In1OutSimulationState>(
            new NAndGateSimulationFactory(), _defId));
    }
    
    private static BuildingDrawData CreateDrawData()
    {
        Mesh baseMesh = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>().sharedMesh;;

        LOD6Mesh baseModLod = MeshLod.Create().AddLod0Mesh(baseMesh).BuildLod6Mesh();

        return new BuildingDrawData(
            renderVoidBelow: false,
            [baseModLod, baseModLod, baseModLod],
            baseModLod,
            baseModLod,
            baseModLod.LODClose,
            new LODEmptyMesh(),
            BoundingBoxHelper.CreateBasicCollider(baseMesh),
            new NAndGateDrawData(),
            false,
            null,
            false);
    }
}
