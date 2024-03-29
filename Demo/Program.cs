﻿using System;

using System.Collections.Generic;

using Engine;
using Nums;


namespace Demo {
    class Program {
        
        public static Gameobject camera;

        static void Main(string[] args) {

            //Console.WriteLine("Working Dir: " + System.IO.Directory.GetCurrentDirectory());
            //Console.Read();

	    
            Application.run(load);            
        }
        

        static void load() {
            camera = new Gameobject(new CameraFlyController());
            camera.enterScene(Scene.active);


            setupTestScene();
            //setupOceanScene();
        }

        static void setupOceanScene() {
            var scene = new Scene();
            initBasicDirlight(scene);

            var waterObj = new Gameobject(new WaterChunk());
            waterObj.enterScene(scene);


            Scene.active = scene;
            camera.enterScene(scene);
        }

        static void initBasicDirlight(Scene scene) {
            scene.dirlights.Add(new Dirlight {
                dir = new Nums.vec3(2,5,3).normalized(),
                color = Nums.vec3.one * 4f
            });
        }

        static void setupTestScene() {

            Assets.getPrefab("Engine.data.models.Ships.Frigate").createInstance().enterScene(Scene.active);


            Scene.active.skybox = CubemapSkybox.generate(Assets.getShader("genCubemap"));


            { // lights

                Scene.active.dirlights.Add(new Dirlight {
                    dir = new Nums.vec3(2,5,3).normalized(),
                    color = Nums.vec3.one * 4f
                });
            }

            { // textured plane
                var m = new Mesh<Vertex>(MeshFactory<Vertex>.genPlane(10, 10f));

                var pixels = new color[100,100];
                for (int i = 0; i < 100; i++) {
                    for (int j = 0; j < 100; j++) {
                        var scale = math.gradnoise(new vec2(i,j) / 10f)*.5f+.5f;
                        pixels[i,j] = scale switch {
                            < 0.4f => color.rgb(0,0,1),
                            < 0.5f => color.rgb(1,1,0),
                            < 0.7f => color.rgb(0,1,0),
                            _ => color.rgb(0,0.6f,0)
                        };
                    }
                }

                var tex = new Texture2D(pixels);

                Renderer.startGameOfLife(tex);

                var g = new Gameobject();
                g.addComponent(new MeshRenderer {
                    mesh = m,
                    materials = new[] { new PBRMaterial {
                        albedoMap = tex,
                        albedo = 1f,
                        roughness = 0.5f,
                        //metallic = 1f
                    } }
                });
                g.addComponent(new AABBCollider() {
                    size = (1,1,1)
                });

                g.transform.position.z += 8;
                g.transform.rotate(vec3.unitx, math.half_pi);
                //g.transform.scale.xz *= 2000;

                g.enterScene(Scene.active);

            }

            { // material spectrum
                var mesh = new Mesh<Vertex>(MeshFactory<Vertex>.genSphere(100, 1f));

                mesh.data.mutate((v, i) => {
                    v.position += v.position * Nums.math.range(0, 0.01f);
                    return v;
                });
                mesh.data.genNormals();
                mesh.updateBuffers();

                for (float r = 0; r <= 1f; r += 0.1f) {
                    for (float m = 0; m <= 1f; m += 0.1f) {
                        var mat = new PBRMaterial {
                            albedo = (vec3.one + new vec3(math.rand(), math.rand(), math.rand())) / 2f,
                            roughness = r,
                            metallic = m
                        };
                        Gameobject g = new();
                        g.addComponent(new MeshRenderer {
                            mesh = mesh,
                            materials = new[] { mat }
                        });
                        
                        // { velocity = (math.rand(), math.rand(), math.rand()) }
                        g.addComponent(new Rigidbody());
                        g.addComponent(new SphereCollider());
                        //g.addComponent(new Pointlight() { color = mat.albedo * 3f });
                        g.addComponent(new ParticleSystem());

                        g.transform.position.xy = new Nums.vec2(r, m + 1) * 25;
                        g.enterScene(Scene.active);
                    }
                }
            }

        
            { // planets
                var mesh = new Mesh<Vertex>(MeshFactory<Vertex>.genSphere(100, 1f));
                float radius = 0;
                mesh.data.mutate((v, i) => {
                    var p = v.position;
                    v.position *= 100f;

                    for (int o = 1; o <= 4; o++) {
                        v.position += p * Nums.math.gradnoise(new Nums.vec3(o,o,o) * 10f + p * 5f * o) * 10f / o;
                        radius = math.max(radius, v.position.sqlength);
                    }

                    return v;
                });
                radius = math.sqrt(radius);
                //mesh.flipIndices();
                mesh.data.genNormals();
                mesh.updateBuffers();


                for (int i = 0; i < 100; i++) {
                    Gameobject planet = new();
                    planet.addComponent(new MeshRenderer {
                        mesh = mesh,
                        materials = new[] { 
                            new PBRMaterial { 
                                roughness = 0.5f,
                                metallic = 1f,
                                albedo = 1f
                            }
                        }
                    });
                    planet.addComponents(new Rigidbody(), new SphereCollider() { radius = radius });
                    planet.transform.position = new vec3(math.rand(), math.rand() *.5f+.5f, math.rand()) * 1000f;

                    planet.enterScene(Scene.active);
                }

            }

            { // quad

                var g = new Gameobject();
                g.addComponent(new MeshRenderer {
                    mesh = new Mesh<Vertex>(MeshFactory<Vertex>.genQuad()),
                    materials = new[] { PBRMaterial.defaultMaterial }
                });

                g.transform.position.yz = (4, 10);
                g.enterScene(Scene.active);
            }
        
            { // prefab test

                var prefab = new Prefab();
                prefab.transform.position = 4;
                prefab.addComponent<MeshRenderer>(new Dictionary<string, object> { 
                    { "mesh", new Mesh<Vertex>(MeshFactory<Vertex>.genCube(1, 1f)) },
                    { "materials", new[] { PBRMaterial.defaultMaterial }}
                });
                prefab.addComponent<Pointlight>(new Dictionary<string, object> {
                    { "color", (object)(vec3.one * 100f) }
                });

                prefab.createInstance().enterScene(Scene.active);

            }

            { // voxelgrid
                var g = new Gameobject();
                //g.transform.position = (-20, 30, 20);
                var vg = new Engine.Voxels.VoxelgridComponent();
                vg.grid = new Engine.Voxels.Voxelgrid();

                for (int x = 0; x < 10; x++) {
                    for (int y = 0; y < 20; y++) {
                        for (int z = 0; z < 40; z++) {
                            var p = new ivec3(x, y, z);
                            if (math.rand() > 0.9f) {
                                vg.grid.voxelAt(p).isSolid = true;
                            }
                        }
                    }
                }

                vg.grid.voxelAt(new ivec3(-10, -20, -25)).isSolid = true;
                vg.grid.voxelAt(new ivec3(-1, -1, -1)).id = 3;

                vg.grid.updateMesh();
                g.addComponent(vg);
                g.enterScene(Scene.active);
            }

            { // spore creature import
                var p = Assets.getPrefab("Engine.data.models.spore.Cel.hack");
                var ins = p.createInstance();
                ins.transform.rotate(vec3.unitx, math.pi / 2f);
                ins.transform.scale *= 10;
                ins.transform.position = (-50, 10, 0);
                ins.enterScene(Scene.active);
            }

        }
    }
}
