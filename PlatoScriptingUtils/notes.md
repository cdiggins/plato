# NOTE:

* Compute face normals. 
* Profile the code in C#. 
* Output the results to a file for test purposes
* Profile the code after Plato to C# 
* Compare the results 
* Automate the generation of the code. 
* Automate the compilation. 
* Test generating code from a regular environement
* Write RoslynUtils 
* Consider using the output window which 

Experiment with different outputs.

1. structs versus classes
2. ref readonly struct 
3. fields versus properties
4. lambda => static 
5. inlining

// * Summary *

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1645 (21H2)
Intel Core i5-1035G1 CPU 1.00GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=6.0.202
  [Host]     : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT
  DefaultJob : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT


| Method |     Mean |    Error |   StdDev |
|------- |---------:|---------:|---------:|
|  Torus | 71.45 ms | 1.424 ms | 2.175 ms |
