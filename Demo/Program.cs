﻿using System;

using System.Collections.Generic;

using Engine;
using Nums;

namespace Demo {
    class Program {
        

        static void Main(string[] args) => app.run(load);
        

        static void load() {


            test();
        }


        static void test() {

            Assets.getPrefab("Frigate").createInstance().enterScene(Scene.active);

            { // lights

                Scene.active.dirlights.Add(new Dirlight {
                    dir = new Nums.vec3(1,1,1).normalized(),
                    color = Nums.vec3.one * 1f
                });

                /*
                dirlights.Add(new Dirlight {
                    color = (0.2f, 0.2f, 1)
                });

                dirlights.Add(new Dirlight {
                    dir = new Nums.vec3(0,0,-1),
                    color = (0,1,0)
                });

                dirlights.Add(new Dirlight {
                    dir = new Nums.vec3(-1,1,-1).normalized(),
                    color = (1,0.2f,0.2f)
                });*/

                Scene.active.pointlights.Add(new Pointlight {
                    position = (3, 3, 3),
                    color = 100 
                });

    /*
                pointlights.Add(new Pointlight {
                    position = (6, 6, 3),
                    color = (3, 30, 3)
                });*/
            }

            { // textured plane
                var m = MeshFactory<Vertex>.genPlane(10, 10f, 10);

                var tex = new Texture2D(WrapMode.MirroredRepeat, Filter.Nearest, 1000, 1000);
                for (int i = 0; i < tex.width; i++) {
                    for (int j = 0; j < tex.height; j++) {
                        var scale = math.gradnoise(new vec2(i,j) / 10f)*.5f+.5f;
                        tex.pixels[i,j] = scale switch {
                            < 0.4f => color.rgb(0,0,1),
                            < 0.5f => color.rgb(1,1,0),
                            < 0.7f => color.rgb(0,1,0),
                            _ => color.rgb(0,0.6f,0)
                        };
                    }
                }
                tex.applyChanges();

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

                g.transform.position.z += 20;
                g.transform.scale.xz *= 2000;

                g.enterScene(Scene.active);

            }

            { // material spectrum
                var mesh = MeshFactory<Vertex>.genSphere(100, 1f);

                mesh.mutate((v, i) => {
                    v.position += v.position * Nums.math.range(0, 0.01f);
                    return v;
                });
                mesh.genNormals();
                //mesh.bufferdata();

                for (float r = 0; r <= 1f; r += 0.1f) {
                    for (float m = 0; m <= 1f; m += 0.1f) {
                        var mat = new PBRMaterial {
                            albedo = Nums.vec3.one,
                            roughness = r,
                            metallic = m
                        };
                        Gameobject g = new();
                        g.addComponent(new MeshRenderer {
                            mesh = mesh,
                            materials = new[] { mat }
                        });

                        g.transform.position.xy = new Nums.vec2(r, m) * 25;
                        g.enterScene(Scene.active);
                    }
                }
            }

        
            { // planets
                var mesh = MeshFactory<Vertex>.genSphere(100, 1f);
                mesh.mutate((v, i) => {
                    var p = v.position;
                    v.position *= 100f;

                    for (int o = 1; o <= 4; o++) {
                        v.position += p * Nums.math.gradnoise(new Nums.vec3(o,o,o) * 10f + p * 5f * o) * 10f / o;
                    }

                    return v;
                });
                //mesh.flipIndices();
                mesh.genNormals();
                mesh.bufferdata();


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
                    planet.transform.position = new vec3(math.rand(), math.rand() *.5f+.5f, math.rand()) * 1000f;

                    planet.enterScene(Scene.active);
                }

            }

            { // quad

                var g = new Gameobject();
                g.addComponent(new MeshRenderer {
                    mesh = MeshFactory<Vertex>.genQuad(),
                    materials = new[] { PBRMaterial.defaultMaterial }
                });

                g.transform.position.yz = (4, 10);
                g.enterScene(Scene.active);
            }
        
            { // prefab test

                var prefab = new Prefab();
                prefab.transform.position = 4;
                prefab.addComponent<MeshRenderer>(new Dictionary<string, object> { 
                    { "mesh", MeshFactory<Vertex>.genCube(1, 1f) },
                    { "materials", new[] { PBRMaterial.defaultMaterial }}
                });

                prefab.createInstance().enterScene(Scene.active);

            }

        }
    }
}