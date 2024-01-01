# ChangeLog
## [30/12/2023]#1.1.1
- ### Fixed
Methods that use the private method `Array CreateExclude(CollisionObject2D[]);` had the null reference problem.
This occurred when the `Array CreateExclude(CollisionObject2D[]);` method received an empty list which caused the method to return a null list of type `Godot.Collections.Array`.
- ### Added
The CHANGELOG.md file has been added.
- ### Changed
The com.cobilas.godot.utility.csproj file has been changed.
## [31/12/2023]#1.1.2
- ### Fixed
O An exception of type `InvalidCastexception` in the explicit operator of the `Hit2D` structure where the `Collision` property could receive an object of another type than `CollisionObject2D` which would inevitably cause `InvalidCastexception`.
The explicit operator of the `RayHit2D` structure was also changed to avoid the same problem as the explicit operator of the `Hit2D` structure.
## [31/12/2023]#1.2.0
- ### Added
The `Gizmos` class has been added.
The `Gizmos` class allows you to use drawing functions statically.
## [01/01/2024]#1.2.1
- ### Fixed
In the static constructor of the `Screen` class it only obtained the resolutions saved in the <kbd>res://AddResolution.json</kbd> file but with the correction the <kbd>projectrootfolder://AddResolution.json</kbd> file in the root folder of the already compiled project is also obtained.