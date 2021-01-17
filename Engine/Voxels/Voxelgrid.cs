
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

        private ivec3 worldToLocal(in ivec3 world) {
            return world - getChunkCoord(in world) * chunkSize;
        }

        /*
            voxel grid pos  (world)
            voxel chunk pos (local)
            chunk coords

            gridpos => chunkCoord
            worldpos => localpos

        */

        public ref voxel voxelAt(in ivec3 pos) {
            var cc = getChunkCoord(in pos);
            var chunk = getChunk(in cc);
            return ref chunk.voxelAt(worldToLocal(in pos));
        }
        

        static Mesh<Vertex> cube;
        static Voxelgrid() {
            cube = MeshFactory<Vertex>.genCube(1, 1f);
        }

        public void render() {
            foreach(var chunk in chunks) {
                for (int x = 0; x < Voxelgrid.chunkSize; x++) {
                    for (int y = 0; y < Voxelgrid.chunkSize; y++) {
                        for (int z = 0; z < Voxelgrid.chunkSize; z++) {
                            var voxel = chunk.Value.voxels[x,y,z];
                            if (!voxel.isSolid) continue;     
                            
                            var pos = new vec3(x, y, z);
                            cube.render();
                        }
                    }
                }
            }
        }


    }

    public class Chunk {

        public readonly voxel[,,] voxels = new voxel[Voxelgrid.chunkSize,Voxelgrid.chunkSize,Voxelgrid.chunkSize];
        
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

        public ref voxel voxelAt(ivec3 pos) => ref voxels[pos.x, pos.y, pos.z];

        void genMesh() {
            for (int x = 0; x < Voxelgrid.chunkSize; x++) {
                for (int y = 0; y < Voxelgrid.chunkSize; y++) {
                    for (int z = 0; z < Voxelgrid.chunkSize; z++) {
                        var voxel = voxels[x,y,z];
                        if (!voxel.isSolid) continue;     
                        
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

    public struct voxel {
        public bool isSolid;
    }

    public class VoxelgridComponent : Component, IRenderer {

        public Voxelgrid grid;

        public void render() {
            gameobject.calcModelMatrix(out mat4 mat);
            GLUtils.setUniformMatrix4(Renderer.geomPass.id, "model", ref mat);
            grid.render();
        }

        protected override void onEnter() {
            scene.renderers.Add(this);
        }

        protected override void onLeave() {
            scene.renderers.Remove(this);
        }


    } 

}