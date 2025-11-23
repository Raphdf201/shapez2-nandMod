using System;
using ShapezShifter.Kit;
using UnityEngine;

namespace SignalApi;

public class NAndGateDrawData : INAndGateDrawData
{
    public static BuildingDrawData CreateCubeDrawData()
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
    
    public static BuildingDrawData CreateMeshDrawData(ModFolderLocator modResourcesLocator)
    {
        string baseMeshPath = modResourcesLocator.SubPath("nand.fbx");
        Console.WriteLine("MESH PATH: " + baseMeshPath);
        Mesh baseMesh = FileMeshLoader.LoadSingleMeshFromFile(baseMeshPath);

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
