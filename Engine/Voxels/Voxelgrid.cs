
using System.Collections.Generic;
using Nums;

namespace Engine.Voxels {

    public class Voxelgrid {
        public readonly Dictionary<ivec3, Chunk> chunks = new Dictionary<ivec3, Chunk>();



    }

    public class Chunk {

        const int chunkSize = 16;

        public readonly Voxel[,,] voxels = new Voxel[chunkSize,chunkSize,chunkSize];

        private Mesh<Vertex> mesh;


        void genMesh() {
            for (int x = 0; x < chunkSize; x++) {
                for (int y = 0; y < chunkSize; y++) {
                    for (int z = 0; z < chunkSize; z++) {
                        var voxel = voxels[x,y,z];
                        if (voxel == null) continue;

                        

                    }
                }
            }
        }


    }

    public class Voxel {

    }

}