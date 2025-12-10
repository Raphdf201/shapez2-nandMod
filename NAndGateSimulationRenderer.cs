namespace NandMod;

public abstract class NAndGateSimulationRenderer(IMapModel map)
    : StatelessBuildingSimulationRenderer<NAndGateSimulation, INAndGateDrawData>(map);
