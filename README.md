# MeshInspector
3DS Max .Net Plugin to check meshes for numerous errors

1. General
--------------------------------------
Inside modern 3D-Engines you can present a mesh with normal maps. 
To get valid normal maps, the mesh needs correct vertex normals, tangents and binormals.

Those vertex normals can get invalid and NaN quite easily.
This tool will check the max selection for those possible problems inside a mesh.

I also documented the sources so other can see how the mnaged max sdk api is working (or not working *g*)
On my work with the managed sdk i noticed there are no samples or good examples available.
So here is one :)

2. Install Instruction for Source
--------------------------------------
Download from GitHub and set the following referenced dll to the correct autodesk max sdk path 
AssemblyLoader.dll
Autodesk.Max
ManagedServices
MaxCustomControls

change post build event copy target pathes to your autodesk max path\bin\assemblies

3. Install instruction for binaries
--------------------------------------
Download from github and unzip the dll files to your autodesk max path\bin\assemblies



