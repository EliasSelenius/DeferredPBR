
using System.Collections.Generic;
using Nums;

namespace Engine.Voxels {

    public class Voxelgrid {
        public const int chunkSize = 16;
        public readonly Dictionary<ivec3, Chunk> chunks = new Dictionary<ivec3, Chunk>();

        private ivec3 getChunkCoord(in ivec3 pos) {
            return new ivec3(pos.x % chunkSize,pos.y % chunkSize,pos.z % chunkSize);
        }

        public Chunk getChunk(in ivec3 pos) {
            if (!chunks.ContainsKey(pos)) chunks[pos] = new Chunk();
            return chunks[pos];
        }

        

    }

    public class Chunk {

        public readonly Voxel[,,] voxels = new Voxel[Voxelgrid.chunkSize,Voxelgrid.chunkSize,Voxelgrid.chunkSize];
        
        Mesh<Vertex> mesh;
        
        static vec3[] vertexOffsets = new vec3[] {
            (-.5f, -.5f, -.5f),
            ( .5f, -.5f, -.5f),
            (-.5f,  .5f, -.5f),
            ( .5f,  .5f, -.5f),
            (-.5f, -.5f,  .5f),
            ( .5f, -.5f,  .5f),
            (-.5f,  .5f,  .5f),
            ( .5f,  .5f,  .5f)
        };

        void genMesh() {
            for (int x = 0; x < Voxelgrid.chunkSize; x++) {
                for (int y = 0; y < Voxelgrid.chunkSize; y++) {
                    for (int z = 0; z < Voxelgrid.chunkSize; z++) {
                        var voxel = voxels[x,y,z];
                        if (voxel.isAir) continue;     
                        
                        var pos = new vec3(x, y, z);

                        foreach (var ofs in vertexOffsets) {
                            mesh.vertices.Add(new Vertex {
                                position = pos - ofs
                            });
                        }


                    }
                }
            }
        }


    }

    public struct Voxel {
        public bool isAir;
    }

}