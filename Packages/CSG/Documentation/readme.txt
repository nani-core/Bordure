CSG - Perform Boolean Operations on Meshes
v1.2 - Released 21/06/2017 by Alan Baylis
----------------------------------------


Foreword
----------------------------------------
Thank you for downloading CSG. This program allows you to create new meshes by performing Subtraction, Union and Intersection between objects. The creation is done in the editor and support for run time operations is in the works. The resulting geometry preserves the texture UV coordinates of the original meshes. The program also handles rotated and scaled objects as expected and is suitable for objects with multiple materials and submeshes. 


Notes
----------------------------------------
The CSG operations cannot be performed on skinned meshes.

The newly created object may contain a lot of triangles and these are not optimized to reduce the triangle count.

It is not recommended to do more that a few operations on the same target object or the geometry may become corrupted.

While the program works well on simple objects it may fail on very complicated objects or objects with underlying problems in their geometry.

It is highly recommended that you save your work and scene before using this software


To-do List.
----------------------------------------
Done: Add an easy to use run time API for scripting.
Done: Option to group triangles that share the same materials.
Mesh optimization to remove excess triangles after each operation.


Common Issues / FAQ
----------------------------------------
Please visit the home page at http://www.meshmaker.com for the latest news and help forum.


Contact
----------------------------------------
Alan Baylis
www.meshmaker.com
support@meshmaker.com


Update Log
-----------------------------------------
v1.0 released 05/02/17
First release of CSG.

v1.1 released 10/06/17
General fixes for latest versions of Unity

v1.2 released 21/06/17
Small change to improve the code
