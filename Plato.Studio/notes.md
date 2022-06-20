TODO:
1. Put the PlatoStudio project somewhere
2. Convert the Vim.Math3D to Plato.Math
3. Generate JavaScript from the IR
4. Fix the inlining bug. 
5. Develop an app, that will do the conversion automatically. 
	* I think I need a tree view: otherwise devbugging the output and optimizations will be very hard. 
	* Same with generating JavaScript / C#. 
	* To start I just want two views, side by side.
	* Maybe syncrhonize with a tree view, so as I click on thing in the tree view, it sync the other views. 
	* Once a really basic UI works, the rest will follow.

With Math3D figure out whether I want to follow the idea of generating code from interfaces
https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/operator-overloads
https://gist.github.com/cdiggins/85b819bfaba3527c851901e5b3879419
https://gist.github.com/cdiggins/47b60fa225ce46371138ca2f878ee882

If I do do that, then I need to 

* There is a problem with "this". It shows up in an inlining context. 
	* This wouldn't be a problem if I replaced the methods with a static call. 
* There is a call to uv.Z ... I don't know how that happens.'
* THere is a reference to "Self" where the self got renamed. 