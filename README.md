# Procedural Vegetation

The goal of this project is to create a procedural vegetation manager for large scenarios.
> This project is not intended to deal with realistic lighting.

Currently, the system distributes plants procedurally using the concept of presets (or also called Layers in coding), shown in the image below. Each preset is a set of plants that have the same characteristics to exist in nature.

![preset](https://github.com/ffranzin/ProceduralVegetation/blob/master/Assets/SampleImages/PresetSettings.png)

For example, plants that exist in forest or grassland regions (look the bushes in a forest zone). 
![adaptability_example_1](https://github.com/ffranzin/ProceduralVegetation/blob/master/Assets/SampleImages/adaptability_example_1.png)
![adaptability_example_2](https://github.com/ffranzin/ProceduralVegetation/blob/master/Assets/SampleImages/adaptability_example_2.png)

It is also possible to configure the distribution of plants using a noise, allowing to position similar plants grouped or sparsely . 
![noise_example_1](https://github.com/ffranzin/ProceduralVegetation/blob/master/Assets/SampleImages/noise_example_1.png)
![noise_example_1](https://github.com/ffranzin/ProceduralVegetation/blob/master/Assets/SampleImages/noise_example_2.png)



During a distribution, it is necessary to inform the system one bounds. The distribution of plants is made according to the current distance from the bounds to a camera.
If the bounds is at a large distance, only trees will be distributed. When the distance is, other plants of smaller size will be distributed as well. A solution uses only one structure for each boundary. When the system identifies the need to render smaller plans, all data is destroyed and initialized in a new configuration. The inverse process occurs for nodes that are distanced from the observer.

> To test a solution, a hash was created where each cell requests the vegetation. However, you can also adopt a quadtree, even being able to distribute the vegetation along with several levels.

The LOD is done by the GPU using compute shaders, which are dispatched before rendering to compute the LOD for each plant and also to organize the data to use GPU-instancing. Each plant has its LOD configuration and may even adopt different configurations for the LOD-geometry and LOD-shadow (image below). 

![lod](https://github.com/ffranzin/ProceduralVegetation/blob/master/Assets/SampleImages/LODSettings.PNG)


Here you can see some examples of how it works.
![LOD-Geometry-1](https://github.com/ffranzin/ProceduralVegetation/blob/master/Assets/SampleImages/lod_example_1.png)
![LOD-Geometry-1](https://github.com/ffranzin/ProceduralVegetation/blob/master/Assets/SampleImages/lod_example_2.png)
![LOD-Shadow](https://github.com/ffranzin/ProceduralVegetation/blob/master/Assets/SampleImages/lod_example_3.png)
![LOD-CullDistance](https://github.com/ffranzin/ProceduralVegetation/blob/master/Assets/SampleImages/lod_example_4.png)



Testing
---------------------

To test it is necessary to initialize the Libraries (in a real game, this object must be serialized) by locating the _VegetationScene asset and clicking on InitializeLibraries.

To configure new vegetation, you can access the menu Assets>Create>Vegetation and click on VegetationScene.

> Currently, it is not possible to choose which VegetationScene will be loaded, being located hard-coded by the component.

To configure a new preset, you can access the menu Assets>Create>Vegetation and choose between SpeedTree, CommonVegetation (used for 3D models) and GroundVegetation (used for grass). After that, it is possible to configure it to insert it in some VegetationScene (don't forget to initialize the VegetationScene).

A plant to be inserted into a preset must contain the PlantDescriptor component.

In playmode, you can edit some configuration in the presets and/or in the PlantDescriptor of some plants. Press space to apply updates.
