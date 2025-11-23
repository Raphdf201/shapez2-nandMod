namespace SignalApi;

public class NAndGateSimulationRenderer(IMapModel map)
    : StatelessBuildingSimulationRenderer<NAndGateSimulation, INAndGateDrawData>(map);
