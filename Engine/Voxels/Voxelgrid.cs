
using System.Collections.Generic;
using Nums;

namespace Engine.Voxels {

    public struct voxel {
        public ushort id;
        public bool isSolid {
            get => id != 0;
            set => id = 1;
        }
    }

    public class Voxelgrid {
        const int chunkSize = 16;
        readonly Dictionary<ivec3, Chunk> chunks = new Dictionary<ivec3, Chunk>();

        //return new ivec3(pos.x % chunkSize, pos.y % chunkSize, pos.z % chunkSize);
        private ivec3 getChunkCoord(in ivec3 worldPos) => worldPos / chunkSize;
        private ivec3 worldToLocal(in ivec3 worldPos) => new ivec3(worldPos.x % chunkSize, worldPos.y % chunkSize, worldPos.z % chunkSize);

        private Chunk getChunk(in ivec3 pos) {
            if (!chunks.ContainsKey(pos)) chunks[pos] = new Chunk();
            return chunks[pos];
        }


        public ref voxel voxelAt(in ivec3 worldPos) => ref getChunk(worldPos / chunkSize).voxelAt(worldToLocal(in worldPos));
        

        static Meshdata<Vertex> cube;
        static Voxelgrid() {
            cube = MeshFactory<Vertex>.genCube(1, 1f);
        }

        Mesh<Vertex> mesh = new Mesh<Vertex>();
        public void render() {
            PBRMaterial.defaultMaterial.updateUniforms();
            mesh.render();
        }

        public void updateMesh() {
            foreach(var chunk in chunks) {
                for (int x = 0; x < chunkSize; x++) {
                    for (int y = 0; y < chunkSize; y++) {
                        for (int z = 0; z < chunkSize; z++) {
                            var voxel = chunk.Value.voxels[x,y,z];
                            if (!voxel.isSolid) continue;     
                            
                            var voxelLocalPos = new vec3(x, y, z);
                            var voxelWorldPos = chunk.Key + voxelLocalPos;
                            //cube.render();
                            mesh.data.add(cube, in voxelWorldPos);

                        }
                    }
                }
            }
            mesh.updateBuffers();
        }

        void recalculateChunkMesh(Chunk chunk) {
            for (int x = 0; x < chunkSize; x++) for (int y = 0; y < chunkSize; y++) for (int z = 0; z < chunkSize; z++) {
                //chunk.voxels[x, y, z].isSolid
            }
        }

        private class Chunk {

            public readonly voxel[,,] voxels = new voxel[chunkSize, chunkSize, chunkSize];
            
            Mesh<Vertex> mesh;

            public ref voxel voxelAt(ivec3 localPos) => ref voxels[localPos.x, localPos.y, localPos.z];
            
            public Chunk() {
                for (int x = 0; x < chunkSize; x++) for (int y = 0; y < chunkSize; y++) for (int z = 0; z < chunkSize; z++) {
                    voxels[x,y,z] = new voxel();
                }
            }
            
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

            

            /*public IEnumerator<(ivec3 localPos, voxel voxel)> enumerateVoxels() {
                for (int )
            }*/

            void genMesh() {
                for (int x = 0; x < Voxelgrid.chunkSize; x++) {
                    for (int y = 0; y < Voxelgrid.chunkSize; y++) {
                        for (int z = 0; z < Voxelgrid.chunkSize; z++) {
                            var voxel = voxels[x,y,z];
                            if (!voxel.isSolid) continue;     
                            
                            var pos = new vec3(x, y, z);

                            foreach (var ofs in vertexOffsets) {
                                mesh.data.vertices.Add(new Vertex {
                                    position = pos - ofs
                                });
                            }


                        }
                    }
                }
            }

        }
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