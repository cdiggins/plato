public interface IArray/* IArray@0 */
{
  }
 this[Int32/* unresolved */index/* index@85 */]
{
  get{
      
    }
    }
public class Array/* Array@2 */
{
  public .ctor(Int32/* unresolved */count/* count@86 */, Func/* unresolved */func/* func@87 */)/* @3 */
  {
      return (Count/* unresolved */, Func/* unresolved */) = (count/* count@86 */, func/* func@87 */);
      
    }
    }
public .ctor(Int32/* unresolved */count/* count@86 */, Func/* unresolved */func/* func@87 */)/* @3 */
{
    return (Count/* unresolved */, Func/* unresolved */) = (count/* count@86 */, func/* func@87 */);
    
  }
   this[Int32/* unresolved */index/* index@88 */]
{
  get}
public class Vector2/* Vector2@5 */
{
  public .ctor(Single/* unresolved */x = 0/* x@89 */, Single/* unresolved */y = 0/* y@90 */)/* @6 */
  {
      return (X/* X@8 */, Y/* Y@9 */) = (x/* x@89 */, y/* y@90 */);
      
    }
    public Vector2/* Vector2@5 */ WithX(Single/* unresolved */x/* x@98 */)
  {
    return new Vector2/* Vector2@5 */(x/* x@98 */, Y/* Y@9 */);
    
  }
  public Vector2/* Vector2@5 */ WithY(Single/* unresolved */y/* y@99 */)
  {
    return new Vector2/* Vector2@5 */(X/* X@8 */, y/* y@99 */);
    
  }
  public Single/* unresolved */ Dot(Vector2/* Vector2@5 */v/* v@100 */)
  {
    return X/* X@8 */*X/* X@8 */+Y/* Y@9 */*Y/* Y@9 */;
    
  }
  public String/* unresolved */ ToString()
  {
    return "Vector2("+X/* X@8 */+","+Y/* Y@9 */+")";
    
  }
  public static  operator implicit(Single/* unresolved */v/* v@91 */)
  {
    return new Vector2/* Vector2@5 */(v/* v@91 */, v/* v@91 */);
    
  }
  public static  operator +(Vector2/* Vector2@5 */q/* q@92 */, Vector2/* Vector2@5 */r/* r@93 */)
  {
    return new Vector2/* Vector2@5 */(X/* X@8 */+X/* X@8 */, Y/* Y@9 */+Y/* Y@9 */);
    
  }
  public static  operator *(Vector2/* Vector2@5 */q/* q@94 */, Vector2/* Vector2@5 */r/* r@95 */)
  {
    return new Vector2/* Vector2@5 */(X/* X@8 */*X/* X@8 */, Y/* Y@9 */*Y/* Y@9 */);
    
  }
  public static  operator /(Vector2/* Vector2@5 */q/* q@96 */, Vector2/* Vector2@5 */r/* r@97 */)
  {
    return new Vector2/* Vector2@5 */(X/* X@8 *//X/* X@8 */, Y/* Y@9 *//Y/* Y@9 */);
    
  }
  }
public .ctor(Single/* unresolved */x = 0/* x@89 */, Single/* unresolved */y = 0/* y@90 */)/* @6 */
{
    return (X/* X@8 */, Y/* Y@9 */) = (x/* x@89 */, y/* y@90 */);
    
  }
  public static  operator implicit(Single/* unresolved */v/* v@91 */)
{
  return new Vector2/* Vector2@5 */(v/* v@91 */, v/* v@91 */);
  
}
Single/* unresolved */ X /* X@8 */;
Single/* unresolved */ Y /* Y@9 */;
public static  operator +(Vector2/* Vector2@5 */q/* q@92 */, Vector2/* Vector2@5 */r/* r@93 */)
{
  return new Vector2/* Vector2@5 */(X/* X@8 */+X/* X@8 */, Y/* Y@9 */+Y/* Y@9 */);
  
}
public static  operator *(Vector2/* Vector2@5 */q/* q@94 */, Vector2/* Vector2@5 */r/* r@95 */)
{
  return new Vector2/* Vector2@5 */(X/* X@8 */*X/* X@8 */, Y/* Y@9 */*Y/* Y@9 */);
  
}
public static  operator /(Vector2/* Vector2@5 */q/* q@96 */, Vector2/* Vector2@5 */r/* r@97 */)
{
  return new Vector2/* Vector2@5 */(X/* X@8 *//X/* X@8 */, Y/* Y@9 *//Y/* Y@9 */);
  
}
public Vector2/* Vector2@5 */ WithX(Single/* unresolved */x/* x@98 */)
{
  return new Vector2/* Vector2@5 */(x/* x@98 */, Y/* Y@9 */);
  
}
public Vector2/* Vector2@5 */ WithY(Single/* unresolved */y/* y@99 */)
{
  return new Vector2/* Vector2@5 */(X/* X@8 */, y/* y@99 */);
  
}
public Single/* unresolved */ Dot(Vector2/* Vector2@5 */v/* v@100 */)
{
  return X/* X@8 */*X/* X@8 */+Y/* Y@9 */*Y/* Y@9 */;
  
}
public String/* unresolved */ ToString()
{
  return "Vector2("+X/* X@8 */+","+Y/* Y@9 */+")";
  
}
public class Vector3/* Vector3@17 */
{
  public .ctor(Single/* unresolved */x = 0/* x@101 */, Single/* unresolved */y = 0/* y@102 */, Single/* unresolved */z = 0/* z@103 */)/* @18 */
  {
      return (X/* X@20 */, Y/* Y@21 */, Z/* Z@22 */) = (x/* x@101 */, y/* y@102 */, z/* z@103 */);
      
    }
    public Vector3/* Vector3@17 */ WithX(Single/* unresolved */x/* x@113 */)
  {
    return new Vector3/* Vector3@17 */(x/* x@113 */, Y/* Y@21 */, Z/* Z@22 */);
    
  }
  public Vector3/* Vector3@17 */ WithY(Single/* unresolved */y/* y@114 */)
  {
    return new Vector3/* Vector3@17 */(X/* X@20 */, y/* y@114 */, Z/* Z@22 */);
    
  }
  public Vector3/* Vector3@17 */ WithZ(Single/* unresolved */z/* z@115 */)
  {
    return new Vector3/* Vector3@17 */(X/* X@20 */, Y/* Y@21 */, z/* z@115 */);
    
  }
  public Single/* unresolved */ Dot(Vector3/* Vector3@17 */v/* v@116 */)
  {
    return X/* X@20 */*X/* X@20 */+Y/* Y@21 */*Y/* Y@21 */+Z/* Z@22 */*Z/* Z@22 */;
    
  }
  public String/* unresolved */ ToString()
  {
    return "Vector3("+X/* X@20 */+","+Y/* Y@21 */+","+Z/* Z@22 */+")";
    
  }
  public Vector3/* Vector3@17 */ Cross(Vector3/* Vector3@17 */v/* v@117 */)
  {
    return new Vector3/* Vector3@17 */(Y/* Y@21 */*Z/* Z@22 */-Z/* Z@22 */*Y/* Y@21 */, Z/* Z@22 */*X/* X@20 */-X/* X@20 */*Z/* Z@22 */, X/* X@20 */*Y/* Y@21 */-Y/* Y@21 */*X/* X@20 */);
    
  }
  public static  operator implicit(Single/* unresolved */v/* v@104 */)
  {
    return new Vector3/* Vector3@17 */(v/* v@104 */, v/* v@104 */, v/* v@104 */);
    
  }
  public static  operator +(Vector3/* Vector3@17 */q/* q@105 */, Vector3/* Vector3@17 */r/* r@106 */)
  {
    return new Vector3/* Vector3@17 */(X/* X@20 */+X/* X@20 */, Y/* Y@21 */+Y/* Y@21 */, Z/* Z@22 */+Z/* Z@22 */);
    
  }
  public static  operator -(Vector3/* Vector3@17 */q/* q@107 */, Vector3/* Vector3@17 */r/* r@108 */)
  {
    return new Vector3/* Vector3@17 */(X/* X@20 */-X/* X@20 */, Y/* Y@21 */-Y/* Y@21 */, Z/* Z@22 */-Z/* Z@22 */);
    
  }
  public static  operator *(Vector3/* Vector3@17 */q/* q@109 */, Vector3/* Vector3@17 */r/* r@110 */)
  {
    return new Vector3/* Vector3@17 */(X/* X@20 */*X/* X@20 */, Y/* Y@21 */*Y/* Y@21 */, Z/* Z@22 */*Z/* Z@22 */);
    
  }
  public static  operator /(Vector3/* Vector3@17 */q/* q@111 */, Vector3/* Vector3@17 */r/* r@112 */)
  {
    return new Vector3/* Vector3@17 */(X/* X@20 *//X/* X@20 */, Y/* Y@21 *//Y/* Y@21 */, Z/* Z@22 *//Z/* Z@22 */);
    
  }
  }
public .ctor(Single/* unresolved */x = 0/* x@101 */, Single/* unresolved */y = 0/* y@102 */, Single/* unresolved */z = 0/* z@103 */)/* @18 */
{
    return (X/* X@20 */, Y/* Y@21 */, Z/* Z@22 */) = (x/* x@101 */, y/* y@102 */, z/* z@103 */);
    
  }
  public static  operator implicit(Single/* unresolved */v/* v@104 */)
{
  return new Vector3/* Vector3@17 */(v/* v@104 */, v/* v@104 */, v/* v@104 */);
  
}
Single/* unresolved */ X /* X@20 */;
Single/* unresolved */ Y /* Y@21 */;
Single/* unresolved */ Z /* Z@22 */;
public static  operator +(Vector3/* Vector3@17 */q/* q@105 */, Vector3/* Vector3@17 */r/* r@106 */)
{
  return new Vector3/* Vector3@17 */(X/* X@20 */+X/* X@20 */, Y/* Y@21 */+Y/* Y@21 */, Z/* Z@22 */+Z/* Z@22 */);
  
}
public static  operator -(Vector3/* Vector3@17 */q/* q@107 */, Vector3/* Vector3@17 */r/* r@108 */)
{
  return new Vector3/* Vector3@17 */(X/* X@20 */-X/* X@20 */, Y/* Y@21 */-Y/* Y@21 */, Z/* Z@22 */-Z/* Z@22 */);
  
}
public static  operator *(Vector3/* Vector3@17 */q/* q@109 */, Vector3/* Vector3@17 */r/* r@110 */)
{
  return new Vector3/* Vector3@17 */(X/* X@20 */*X/* X@20 */, Y/* Y@21 */*Y/* Y@21 */, Z/* Z@22 */*Z/* Z@22 */);
  
}
public static  operator /(Vector3/* Vector3@17 */q/* q@111 */, Vector3/* Vector3@17 */r/* r@112 */)
{
  return new Vector3/* Vector3@17 */(X/* X@20 *//X/* X@20 */, Y/* Y@21 *//Y/* Y@21 */, Z/* Z@22 *//Z/* Z@22 */);
  
}
public Vector3/* Vector3@17 */ WithX(Single/* unresolved */x/* x@113 */)
{
  return new Vector3/* Vector3@17 */(x/* x@113 */, Y/* Y@21 */, Z/* Z@22 */);
  
}
public Vector3/* Vector3@17 */ WithY(Single/* unresolved */y/* y@114 */)
{
  return new Vector3/* Vector3@17 */(X/* X@20 */, y/* y@114 */, Z/* Z@22 */);
  
}
public Vector3/* Vector3@17 */ WithZ(Single/* unresolved */z/* z@115 */)
{
  return new Vector3/* Vector3@17 */(X/* X@20 */, Y/* Y@21 */, z/* z@115 */);
  
}
public Single/* unresolved */ Dot(Vector3/* Vector3@17 */v/* v@116 */)
{
  return X/* X@20 */*X/* X@20 */+Y/* Y@21 */*Y/* Y@21 */+Z/* Z@22 */*Z/* Z@22 */;
  
}
public String/* unresolved */ ToString()
{
  return "Vector3("+X/* X@20 */+","+Y/* Y@21 */+","+Z/* Z@22 */+")";
  
}
public Vector3/* Vector3@17 */ Cross(Vector3/* Vector3@17 */v/* v@117 */)
{
  return new Vector3/* Vector3@17 */(Y/* Y@21 */*Z/* Z@22 */-Z/* Z@22 */*Y/* Y@21 */, Z/* Z@22 */*X/* X@20 */-X/* X@20 */*Z/* Z@22 */, X/* X@20 */*Y/* Y@21 */-Y/* Y@21 */*X/* X@20 */);
  
}
 this[Int32/* unresolved */i/* i@118 */]
{
  get}
public class Int3/* Int3@34 */
{
  public .ctor(Int32/* unresolved */x = 0/* x@119 */, Int32/* unresolved */y = 0/* y@120 */, Int32/* unresolved */z = 0/* z@121 */)/* @35 */
  {
      return (X/* X@36 */, Y/* Y@37 */, Z/* Z@38 */) = (x/* x@119 */, y/* y@120 */, z/* z@121 */);
      
    }
    public Int3/* Int3@34 */ WithX(Int32/* unresolved */x/* x@122 */)
  {
    return new Int3/* Int3@34 */(x/* x@122 */, Y/* Y@37 */, Z/* Z@38 */);
    
  }
  public Int3/* Int3@34 */ WithY(Int32/* unresolved */y/* y@123 */)
  {
    return new Int3/* Int3@34 */(X/* X@36 */, y/* y@123 */, Z/* Z@38 */);
    
  }
  public Int3/* Int3@34 */ WithZ(Int32/* unresolved */z/* z@124 */)
  {
    return new Int3/* Int3@34 */(X/* X@36 */, Y/* Y@37 */, z/* z@124 */);
    
  }
  public String/* unresolved */ ToString()
  {
    return "Int3("+X/* X@36 */+","+Y/* Y@37 */+","+Z/* Z@38 */+")";
    
  }
  }
public .ctor(Int32/* unresolved */x = 0/* x@119 */, Int32/* unresolved */y = 0/* y@120 */, Int32/* unresolved */z = 0/* z@121 */)/* @35 */
{
    return (X/* X@36 */, Y/* Y@37 */, Z/* Z@38 */) = (x/* x@119 */, y/* y@120 */, z/* z@121 */);
    
  }
  Int32/* unresolved */ X /* X@36 */;
Int32/* unresolved */ Y /* Y@37 */;
Int32/* unresolved */ Z /* Z@38 */;
public Int3/* Int3@34 */ WithX(Int32/* unresolved */x/* x@122 */)
{
  return new Int3/* Int3@34 */(x/* x@122 */, Y/* Y@37 */, Z/* Z@38 */);
  
}
public Int3/* Int3@34 */ WithY(Int32/* unresolved */y/* y@123 */)
{
  return new Int3/* Int3@34 */(X/* X@36 */, y/* y@123 */, Z/* Z@38 */);
  
}
public Int3/* Int3@34 */ WithZ(Int32/* unresolved */z/* z@124 */)
{
  return new Int3/* Int3@34 */(X/* X@36 */, Y/* Y@37 */, z/* z@124 */);
  
}
public String/* unresolved */ ToString()
{
  return "Int3("+X/* X@36 */+","+Y/* Y@37 */+","+Z/* Z@38 */+")";
  
}
 this[Int32/* unresolved */i/* i@125 */]
{
  get}
public class Int4/* Int4@44 */
{
  public .ctor(Int32/* unresolved */x = 0/* x@126 */, Int32/* unresolved */y = 0/* y@127 */, Int32/* unresolved */z = 0/* z@128 */, Int32/* unresolved */w = 0/* w@129 */)/* @45 */
  {
      return (X/* X@46 */, Y/* Y@47 */, Z/* Z@48 */, W/* W@49 */) = (x/* x@126 */, y/* y@127 */, z/* z@128 */, w/* w@129 */);
      
    }
    public Int4/* Int4@44 */ WithX(Int32/* unresolved */x/* x@130 */)
  {
    return new Int4/* Int4@44 */(x/* x@130 */, Y/* Y@47 */, Z/* Z@48 */, W/* W@49 */);
    
  }
  public Int4/* Int4@44 */ WithY(Int32/* unresolved */y/* y@131 */)
  {
    return new Int4/* Int4@44 */(X/* X@46 */, y/* y@131 */, Z/* Z@48 */, W/* W@49 */);
    
  }
  public Int4/* Int4@44 */ WithZ(Int32/* unresolved */z/* z@132 */)
  {
    return new Int4/* Int4@44 */(X/* X@46 */, Y/* Y@47 */, z/* z@132 */, W/* W@49 */);
    
  }
  public Int4/* Int4@44 */ WithW(Int32/* unresolved */w/* w@133 */)
  {
    return new Int4/* Int4@44 */(X/* X@46 */, Y/* Y@47 */, Z/* Z@48 */, w/* w@133 */);
    
  }
  public String/* unresolved */ ToString()
  {
    return "Int4("+X/* X@46 */+","+Y/* Y@47 */+","+Z/* Z@48 */+","+W/* W@49 */+")";
    
  }
  }
public .ctor(Int32/* unresolved */x = 0/* x@126 */, Int32/* unresolved */y = 0/* y@127 */, Int32/* unresolved */z = 0/* z@128 */, Int32/* unresolved */w = 0/* w@129 */)/* @45 */
{
    return (X/* X@46 */, Y/* Y@47 */, Z/* Z@48 */, W/* W@49 */) = (x/* x@126 */, y/* y@127 */, z/* z@128 */, w/* w@129 */);
    
  }
  Int32/* unresolved */ X /* X@46 */;
Int32/* unresolved */ Y /* Y@47 */;
Int32/* unresolved */ Z /* Z@48 */;
Int32/* unresolved */ W /* W@49 */;
public Int4/* Int4@44 */ WithX(Int32/* unresolved */x/* x@130 */)
{
  return new Int4/* Int4@44 */(x/* x@130 */, Y/* Y@47 */, Z/* Z@48 */, W/* W@49 */);
  
}
public Int4/* Int4@44 */ WithY(Int32/* unresolved */y/* y@131 */)
{
  return new Int4/* Int4@44 */(X/* X@46 */, y/* y@131 */, Z/* Z@48 */, W/* W@49 */);
  
}
public Int4/* Int4@44 */ WithZ(Int32/* unresolved */z/* z@132 */)
{
  return new Int4/* Int4@44 */(X/* X@46 */, Y/* Y@47 */, z/* z@132 */, W/* W@49 */);
  
}
public Int4/* Int4@44 */ WithW(Int32/* unresolved */w/* w@133 */)
{
  return new Int4/* Int4@44 */(X/* X@46 */, Y/* Y@47 */, Z/* Z@48 */, w/* w@133 */);
  
}
public String/* unresolved */ ToString()
{
  return "Int4("+X/* X@46 */+","+Y/* Y@47 */+","+Z/* Z@48 */+","+W/* W@49 */+")";
  
}
 this[Int32/* unresolved */i/* i@134 */]
{
  get}
public class Points/* Points@56 */
{
  public .ctor(IArray/* IArray@0 */positions/* positions@135 */, IArray/* IArray@0 */uvs/* uvs@136 */, IArray/* IArray@0 */normals/* normals@137 */)/* @57 */
  {
      return (Positions/* unresolved */, UVs/* unresolved */, Normals/* unresolved */) = (positions/* positions@135 */, uvs/* uvs@136 */, normals/* normals@137 */);
      
    }
    }
public .ctor(IArray/* IArray@0 */positions/* positions@135 */, IArray/* IArray@0 */uvs/* uvs@136 */, IArray/* IArray@0 */normals/* normals@137 */)/* @57 */
{
    return (Positions/* unresolved */, UVs/* unresolved */, Normals/* unresolved */) = (positions/* positions@135 */, uvs/* uvs@136 */, normals/* normals@137 */);
    
  }
  public class TriMesh/* TriMesh@58 */
{
  public .ctor(Points/* Points@56 */points/* points@138 */, IArray/* IArray@0 */faces/* faces@139 */)/* @59 */
  {
      return (Points/* unresolved */, Faces/* unresolved */) = (points/* points@138 */, faces/* faces@139 */);
      
    }
    }
public .ctor(Points/* Points@56 */points/* points@138 */, IArray/* IArray@0 */faces/* faces@139 */)/* @59 */
{
    return (Points/* unresolved */, Faces/* unresolved */) = (points/* points@138 */, faces/* faces@139 */);
    
  }
  public class QuadMesh/* QuadMesh@60 */
{
  public .ctor(Points/* Points@56 */points/* points@140 */, IArray/* IArray@0 */faces/* faces@141 */)/* @61 */
  {
      return (Points/* unresolved */, Faces/* unresolved */) = (points/* points@140 */, faces/* faces@141 */);
      
    }
    }
public .ctor(Points/* Points@56 */points/* points@140 */, IArray/* IArray@0 */faces/* faces@141 */)/* @61 */
{
    return (Points/* unresolved */, Faces/* unresolved */) = (points/* points@140 */, faces/* faces@141 */);
    
  }
  public class Extensions/* Extensions@62 */
{
  public static IArray/* IArray@0 */ ToIArray(IReadOnlyList/* unresolved */self/* self@142 */)
  {
    return Select/* Select@66 */(#lambdaIr);
    
  }
  public static /* unresolved */ ToArray(IArray/* IArray@0 */self/* self@144 */)
  {
    /* unresolved */ r  = new []{}/* r@145 *//* r@145 */;
    {
      Int32/* unresolved */ i  = 0/* i@146 *//* i@146 */;
      while(i/* i@146 */<Count/* unresolved */)
      {
          r/* r@145 */[i/* i@146 */] = self/* self@144 */[i/* i@146 */];
          ++i/* i@146 */;
          
        }
        
    }
    return r/* r@145 */;
    
  }
  public static /* unresolved */ ToFloatArray(IArray/* IArray@0 */self/* self@147 */)
  {
    return ToArray/* ToArray@64 */();
    
  }
  public static IArray/* IArray@0 */ Select(Int32/* unresolved */count/* count@148 */, Func/* unresolved */func/* func@149 */)
  {
    return new Array/* Array@2 */(count/* count@148 */, func/* func@149 */);
    
  }
  public static IArray/* IArray@0 */ SelectMany(IArray/* IArray@0 */self/* self@150 */, Func/* unresolved */func/* func@151 */)
  {
    List/* unresolved */ r  = new List/* unresolved */()/* r@152 *//* r@152 */;
    {
      Int32/* unresolved */ i  = 0/* i@153 *//* i@153 */;
      while(i/* i@153 */<Count/* unresolved */)
      {
          {
            IArray/* IArray@0 */ tmp  = func/* unresolved */(self/* self@150 */[i/* i@153 */])/* tmp@154 *//* tmp@154 */;
            {
              Int32/* unresolved */ j  = 0/* j@155 *//* j@155 */;
              while(j/* j@155 */<Count/* unresolved */)
              {
                  Add/* unresolved */(tmp/* tmp@154 */[j/* j@155 */]);
                  ++j/* j@155 */;
                  
                }
                
            }
            
          }
          ++i/* i@153 */;
          
        }
        
    }
    return Select/* Select@66 */(#lambdaIr);
    
  }
  public static IArray/* IArray@0 */ Select(IArray/* IArray@0 */self/* self@157 */, Func/* unresolved */func/* func@158 */)
  {
    return Select/* Select@66 */(#lambdaIr);
    
  }
  public static Single/* unresolved */ Cos(Single/* unresolved */self/* self@160 */)
  {
    return Cos/* unresolved */(self/* self@160 */);
    
  }
  public static Single/* unresolved */ Sin(Single/* unresolved */self/* self@161 */)
  {
    return Sin/* unresolved */(self/* self@161 */);
    
  }
  public static Single/* unresolved */ UnitToRad(Single/* unresolved */self/* self@162 */)
  {
    return self/* self@162 */*PI/* unresolved */;
    
  }
  public static IArray/* IArray@0 */ SampleFloats(Int32/* unresolved */count/* count@163 */, Single/* unresolved */max = 1.0f/* max@164 */)
  {
    return Select/* Select@66 */(#lambdaIr);
    
  }
  public static IArray/* IArray@0 */ ToTriangles(IArray/* IArray@0 */self/* self@166 */)
  {
    return SelectMany/* SelectMany@67 */(#lambdaIr);
    
  }
  public static QuadMesh/* QuadMesh@60 */ ToQuadMesh(Func/* unresolved */func/* func@168 */, Int32/* unresolved */rows/* rows@169 */, Int32/* unresolved */cols/* cols@170 */)
  {
    return new QuadMesh/* QuadMesh@60 */(ComputeQuadStripPoints/* ComputeQuadStripPoints@77 */(func/* func@168 */, rows/* rows@169 */, cols/* cols@170 */), ComputeQuadStripIndices/* ComputeQuadStripIndices@79 */(rows/* rows@169 */, cols/* cols@170 */));
    
  }
  public static Vector3/* Vector3@17 */ UVToNormal(Vector2/* Vector2@5 */uv/* uv@171 */, Func/* unresolved */func/* func@172 */, Single/* unresolved */epsilon = 0.00001f/* epsilon@173 */)
  {
    Vector3/* Vector3@17 */ a  = func/* unresolved */(new Vector2/* Vector2@5 */(X/* X@8 */+epsilon/* epsilon@173 */, Y/* Y@9 */))/* a@174 *//* a@174 */;
    Vector3/* Vector3@17 */ b  = func/* unresolved */(new Vector2/* Vector2@5 */(X/* X@8 */-epsilon/* epsilon@173 */, Y/* Y@9 */))/* b@175 *//* b@175 */;
    Vector3/* Vector3@17 */ c  = func/* unresolved */(new Vector2/* Vector2@5 */(X/* X@8 */, Y/* Y@9 */+epsilon/* epsilon@173 */))/* c@176 *//* c@176 */;
    Vector3/* Vector3@17 */ d  = func/* unresolved */(new Vector2/* Vector2@5 */(X/* X@8 */, Y/* Y@9 */-epsilon/* epsilon@173 */))/* d@177 *//* d@177 */;
    Vector3/* Vector3@17 */ v1  = b/* b@175 */-a/* a@174 *//* v1@178 *//* v1@178 */;
    Vector3/* Vector3@17 */ v2  = d/* d@177 */-c/* c@176 *//* v2@179 *//* v2@179 */;
    Vector3/* Vector3@17 */ r  = Cross/* Cross@32 */(v2/* v2@179 */)/* r@180 *//* r@180 */;
    return Normal/* unresolved */;
    
  }
  public static Points/* Points@56 */ UVsToPoints(IArray/* IArray@0 */uvs/* uvs@181 */, Func/* unresolved */func/* func@182 */)
  {
    return new Points/* Points@56 */(Select/* Select@68 */(func/* func@182 */), uvs/* uvs@181 */, Select/* Select@68 */(#lambdaIr));
    
  }
  public static Points/* Points@56 */ ComputeQuadStripPoints(Func/* unresolved */func/* func@184 */, Int32/* unresolved */usegs/* usegs@185 */, Int32/* unresolved */vsegs/* vsegs@186 */)
  {
    return UVsToPoints/* UVsToPoints@76 */(func/* func@184 */);
    
  }
  public static IArray/* IArray@0 */ ComputeQuadStripUVs(Int32/* unresolved */usegs/* usegs@187 */, Int32/* unresolved */vsegs/* vsegs@188 */)
  {
    return new Array/* Array@2 */(usegs/* usegs@187 */*vsegs/* vsegs@188 */, #lambdaIr);
    
  }
  public static IArray/* IArray@0 */ ComputeQuadStripIndices(Int32/* unresolved */usegs/* usegs@192 */, Int32/* unresolved */vsegs/* vsegs@193 */)
  {
    return new Array/* Array@2 */((usegs/* usegs@192 */-1)*(vsegs/* vsegs@193 */-1), #lambdaIr);
    
  }
  public static Vector3/* Vector3@17 */ UvToSphere(Vector2/* Vector2@5 */uv/* uv@203 */, Single/* unresolved */radius/* radius@204 */)
  {
    return new Vector3/* Vector3@17 */(-radius/* radius@204 */*Cos/* Cos@69 */()*Sin/* Sin@70 */(), radius/* radius@204 */*Cos/* Cos@69 */(), radius/* radius@204 */*Cos/* Cos@69 */()*Sin/* Sin@70 */());
    
  }
  public static Vector3/* Vector3@17 */ UvToTorus(Vector2/* Vector2@5 */uv/* uv@205 */, Single/* unresolved */radius/* radius@206 */, Single/* unresolved */tube/* tube@207 */)
  {
    uv/* uv@205 */ = uv/* uv@205 */*PI/* unresolved */*2;
    return new Vector3/* Vector3@17 */((radius/* radius@206 */+tube/* tube@207 */*Cos/* Cos@69 */())*Cos/* Cos@69 */(), (radius/* radius@206 */+tube/* tube@207 */*Cos/* Cos@69 */())*Sin/* Sin@70 */(), tube/* tube@207 */*Sin/* Sin@70 */());
    
  }
  public static Void/* unresolved */ TestOperator()
  {
    Vector3/* Vector3@17 */ x  = new Vector3/* Vector3@17 */(1, 2, 3)/* x@208 *//* x@208 */;
    Vector3/* Vector3@17 */ y  = x/* x@208 */+x/* x@208 *//* y@209 *//* y@209 */;
    
  }
  public static QuadMesh/* QuadMesh@60 */ Torus(Int32/* unresolved */rows/* rows@210 */, Int32/* unresolved */cols/* cols@211 */, Single/* unresolved */radius/* radius@212 */, Single/* unresolved */tube/* tube@213 */)
  {
    return ToQuadMesh/* ToQuadMesh@74 */(#lambdaIr, rows/* rows@210 */, cols/* cols@211 */);
    
  }
  public static /* unresolved */ ToIntArray(IArray/* IArray@0 */faces/* faces@215 */)
  {
    return ToArray/* ToArray@64 */();
    
  }
  }
public static IArray/* IArray@0 */ ToIArray(IReadOnlyList/* unresolved */self/* self@142 */)
{
  return Select/* Select@66 */(#lambdaIr);
  
}
public static /* unresolved */ ToArray(IArray/* IArray@0 */self/* self@144 */)
{
  /* unresolved */ r  = new []{}/* r@145 *//* r@145 */;
  {
    Int32/* unresolved */ i  = 0/* i@146 *//* i@146 */;
    while(i/* i@146 */<Count/* unresolved */)
    {
        r/* r@145 */[i/* i@146 */] = self/* self@144 */[i/* i@146 */];
        ++i/* i@146 */;
        
      }
      
  }
  return r/* r@145 */;
  
}
public static /* unresolved */ ToFloatArray(IArray/* IArray@0 */self/* self@147 */)
{
  return ToArray/* ToArray@64 */();
  
}
public static IArray/* IArray@0 */ Select(Int32/* unresolved */count/* count@148 */, Func/* unresolved */func/* func@149 */)
{
  return new Array/* Array@2 */(count/* count@148 */, func/* func@149 */);
  
}
public static IArray/* IArray@0 */ SelectMany(IArray/* IArray@0 */self/* self@150 */, Func/* unresolved */func/* func@151 */)
{
  List/* unresolved */ r  = new List/* unresolved */()/* r@152 *//* r@152 */;
  {
    Int32/* unresolved */ i  = 0/* i@153 *//* i@153 */;
    while(i/* i@153 */<Count/* unresolved */)
    {
        {
          IArray/* IArray@0 */ tmp  = func/* unresolved */(self/* self@150 */[i/* i@153 */])/* tmp@154 *//* tmp@154 */;
          {
            Int32/* unresolved */ j  = 0/* j@155 *//* j@155 */;
            while(j/* j@155 */<Count/* unresolved */)
            {
                Add/* unresolved */(tmp/* tmp@154 */[j/* j@155 */]);
                ++j/* j@155 */;
                
              }
              
          }
          
        }
        ++i/* i@153 */;
        
      }
      
  }
  return Select/* Select@66 */(#lambdaIr);
  
}
public static IArray/* IArray@0 */ Select(IArray/* IArray@0 */self/* self@157 */, Func/* unresolved */func/* func@158 */)
{
  return Select/* Select@66 */(#lambdaIr);
  
}
public static Single/* unresolved */ Cos(Single/* unresolved */self/* self@160 */)
{
  return Cos/* unresolved */(self/* self@160 */);
  
}
public static Single/* unresolved */ Sin(Single/* unresolved */self/* self@161 */)
{
  return Sin/* unresolved */(self/* self@161 */);
  
}
public static Single/* unresolved */ UnitToRad(Single/* unresolved */self/* self@162 */)
{
  return self/* self@162 */*PI/* unresolved */;
  
}
public static IArray/* IArray@0 */ SampleFloats(Int32/* unresolved */count/* count@163 */, Single/* unresolved */max = 1.0f/* max@164 */)
{
  return Select/* Select@66 */(#lambdaIr);
  
}
public static IArray/* IArray@0 */ ToTriangles(IArray/* IArray@0 */self/* self@166 */)
{
  return SelectMany/* SelectMany@67 */(#lambdaIr);
  
}
public static QuadMesh/* QuadMesh@60 */ ToQuadMesh(Func/* unresolved */func/* func@168 */, Int32/* unresolved */rows/* rows@169 */, Int32/* unresolved */cols/* cols@170 */)
{
  return new QuadMesh/* QuadMesh@60 */(ComputeQuadStripPoints/* ComputeQuadStripPoints@77 */(func/* func@168 */, rows/* rows@169 */, cols/* cols@170 */), ComputeQuadStripIndices/* ComputeQuadStripIndices@79 */(rows/* rows@169 */, cols/* cols@170 */));
  
}
public static Vector3/* Vector3@17 */ UVToNormal(Vector2/* Vector2@5 */uv/* uv@171 */, Func/* unresolved */func/* func@172 */, Single/* unresolved */epsilon = 0.00001f/* epsilon@173 */)
{
  Vector3/* Vector3@17 */ a  = func/* unresolved */(new Vector2/* Vector2@5 */(X/* X@8 */+epsilon/* epsilon@173 */, Y/* Y@9 */))/* a@174 *//* a@174 */;
  Vector3/* Vector3@17 */ b  = func/* unresolved */(new Vector2/* Vector2@5 */(X/* X@8 */-epsilon/* epsilon@173 */, Y/* Y@9 */))/* b@175 *//* b@175 */;
  Vector3/* Vector3@17 */ c  = func/* unresolved */(new Vector2/* Vector2@5 */(X/* X@8 */, Y/* Y@9 */+epsilon/* epsilon@173 */))/* c@176 *//* c@176 */;
  Vector3/* Vector3@17 */ d  = func/* unresolved */(new Vector2/* Vector2@5 */(X/* X@8 */, Y/* Y@9 */-epsilon/* epsilon@173 */))/* d@177 *//* d@177 */;
  Vector3/* Vector3@17 */ v1  = b/* b@175 */-a/* a@174 *//* v1@178 *//* v1@178 */;
  Vector3/* Vector3@17 */ v2  = d/* d@177 */-c/* c@176 *//* v2@179 *//* v2@179 */;
  Vector3/* Vector3@17 */ r  = Cross/* Cross@32 */(v2/* v2@179 */)/* r@180 *//* r@180 */;
  return Normal/* unresolved */;
  
}
public static Points/* Points@56 */ UVsToPoints(IArray/* IArray@0 */uvs/* uvs@181 */, Func/* unresolved */func/* func@182 */)
{
  return new Points/* Points@56 */(Select/* Select@68 */(func/* func@182 */), uvs/* uvs@181 */, Select/* Select@68 */(#lambdaIr));
  
}
public static Points/* Points@56 */ ComputeQuadStripPoints(Func/* unresolved */func/* func@184 */, Int32/* unresolved */usegs/* usegs@185 */, Int32/* unresolved */vsegs/* vsegs@186 */)
{
  return UVsToPoints/* UVsToPoints@76 */(func/* func@184 */);
  
}
public static IArray/* IArray@0 */ ComputeQuadStripUVs(Int32/* unresolved */usegs/* usegs@187 */, Int32/* unresolved */vsegs/* vsegs@188 */)
{
  return new Array/* Array@2 */(usegs/* usegs@187 */*vsegs/* vsegs@188 */, #lambdaIr);
  
}
public static IArray/* IArray@0 */ ComputeQuadStripIndices(Int32/* unresolved */usegs/* usegs@192 */, Int32/* unresolved */vsegs/* vsegs@193 */)
{
  return new Array/* Array@2 */((usegs/* usegs@192 */-1)*(vsegs/* vsegs@193 */-1), #lambdaIr);
  
}
public static Vector3/* Vector3@17 */ UvToSphere(Vector2/* Vector2@5 */uv/* uv@203 */, Single/* unresolved */radius/* radius@204 */)
{
  return new Vector3/* Vector3@17 */(-radius/* radius@204 */*Cos/* Cos@69 */()*Sin/* Sin@70 */(), radius/* radius@204 */*Cos/* Cos@69 */(), radius/* radius@204 */*Cos/* Cos@69 */()*Sin/* Sin@70 */());
  
}
public static Vector3/* Vector3@17 */ UvToTorus(Vector2/* Vector2@5 */uv/* uv@205 */, Single/* unresolved */radius/* radius@206 */, Single/* unresolved */tube/* tube@207 */)
{
  uv/* uv@205 */ = uv/* uv@205 */*PI/* unresolved */*2;
  return new Vector3/* Vector3@17 */((radius/* radius@206 */+tube/* tube@207 */*Cos/* Cos@69 */())*Cos/* Cos@69 */(), (radius/* radius@206 */+tube/* tube@207 */*Cos/* Cos@69 */())*Sin/* Sin@70 */(), tube/* tube@207 */*Sin/* Sin@70 */());
  
}
public static Void/* unresolved */ TestOperator()
{
  Vector3/* Vector3@17 */ x  = new Vector3/* Vector3@17 */(1, 2, 3)/* x@208 *//* x@208 */;
  Vector3/* Vector3@17 */ y  = x/* x@208 */+x/* x@208 *//* y@209 *//* y@209 */;
  
}
public static QuadMesh/* QuadMesh@60 */ Torus(Int32/* unresolved */rows/* rows@210 */, Int32/* unresolved */cols/* cols@211 */, Single/* unresolved */radius/* radius@212 */, Single/* unresolved */tube/* tube@213 */)
{
  return ToQuadMesh/* ToQuadMesh@74 */(#lambdaIr, rows/* rows@210 */, cols/* cols@211 */);
  
}
public static /* unresolved */ ToIntArray(IArray/* IArray@0 */faces/* faces@215 */)
{
  return ToArray/* ToArray@64 */();
  
}
Int32/* unresolved */index/* index@85 */Int32/* unresolved */count/* count@86 */Func/* unresolved */func/* func@87 */Int32/* unresolved */index/* index@88 */Single/* unresolved */x = 0/* x@89 */Single/* unresolved */y = 0/* y@90 */Single/* unresolved */v/* v@91 */Vector2/* Vector2@5 */q/* q@92 */Vector2/* Vector2@5 */r/* r@93 */Vector2/* Vector2@5 */q/* q@94 */Vector2/* Vector2@5 */r/* r@95 */Vector2/* Vector2@5 */q/* q@96 */Vector2/* Vector2@5 */r/* r@97 */Single/* unresolved */x/* x@98 */Single/* unresolved */y/* y@99 */Vector2/* Vector2@5 */v/* v@100 */Single/* unresolved */x = 0/* x@101 */Single/* unresolved */y = 0/* y@102 */Single/* unresolved */z = 0/* z@103 */Single/* unresolved */v/* v@104 */Vector3/* Vector3@17 */q/* q@105 */Vector3/* Vector3@17 */r/* r@106 */Vector3/* Vector3@17 */q/* q@107 */Vector3/* Vector3@17 */r/* r@108 */Vector3/* Vector3@17 */q/* q@109 */Vector3/* Vector3@17 */r/* r@110 */Vector3/* Vector3@17 */q/* q@111 */Vector3/* Vector3@17 */r/* r@112 */Single/* unresolved */x/* x@113 */Single/* unresolved */y/* y@114 */Single/* unresolved */z/* z@115 */Vector3/* Vector3@17 */v/* v@116 */Vector3/* Vector3@17 */v/* v@117 */Int32/* unresolved */i/* i@118 */Int32/* unresolved */x = 0/* x@119 */Int32/* unresolved */y = 0/* y@120 */Int32/* unresolved */z = 0/* z@121 */Int32/* unresolved */x/* x@122 */Int32/* unresolved */y/* y@123 */Int32/* unresolved */z/* z@124 */Int32/* unresolved */i/* i@125 */Int32/* unresolved */x = 0/* x@126 */Int32/* unresolved */y = 0/* y@127 */Int32/* unresolved */z = 0/* z@128 */Int32/* unresolved */w = 0/* w@129 */Int32/* unresolved */x/* x@130 */Int32/* unresolved */y/* y@131 */Int32/* unresolved */z/* z@132 */Int32/* unresolved */w/* w@133 */Int32/* unresolved */i/* i@134 */IArray/* IArray@0 */positions/* positions@135 */IArray/* IArray@0 */uvs/* uvs@136 */IArray/* IArray@0 */normals/* normals@137 */Points/* Points@56 */points/* points@138 */IArray/* IArray@0 */faces/* faces@139 */Points/* Points@56 */points/* points@140 */IArray/* IArray@0 */faces/* faces@141 */IReadOnlyList/* unresolved */self/* self@142 */i/* i@143 */IArray/* IArray@0 */self/* self@144 *//* unresolved */ r  = new []{}/* r@145 */Int32/* unresolved */ i  = 0/* i@146 */IArray/* IArray@0 */self/* self@147 */Int32/* unresolved */count/* count@148 */Func/* unresolved */func/* func@149 */IArray/* IArray@0 */self/* self@150 */Func/* unresolved */func/* func@151 */List/* unresolved */ r  = new List/* unresolved */()/* r@152 */Int32/* unresolved */ i  = 0/* i@153 */IArray/* IArray@0 */ tmp  = func/* unresolved */(self/* self@150 */[i/* i@153 */])/* tmp@154 */Int32/* unresolved */ j  = 0/* j@155 */i/* i@156 */IArray/* IArray@0 */self/* self@157 */Func/* unresolved */func/* func@158 */i/* i@159 */Single/* unresolved */self/* self@160 */Single/* unresolved */self/* self@161 */Single/* unresolved */self/* self@162 */Int32/* unresolved */count/* count@163 */Single/* unresolved */max = 1.0f/* max@164 */i/* i@165 */IArray/* IArray@0 */self/* self@166 */f/* f@167 */Func/* unresolved */func/* func@168 */Int32/* unresolved */rows/* rows@169 */Int32/* unresolved */cols/* cols@170 */Vector2/* Vector2@5 */uv/* uv@171 */Func/* unresolved */func/* func@172 */Single/* unresolved */epsilon = 0.00001f/* epsilon@173 */Vector3/* Vector3@17 */ a  = func/* unresolved */(new Vector2/* Vector2@5 */(X/* X@8 */+epsilon/* epsilon@173 */, Y/* Y@9 */))/* a@174 */Vector3/* Vector3@17 */ b  = func/* unresolved */(new Vector2/* Vector2@5 */(X/* X@8 */-epsilon/* epsilon@173 */, Y/* Y@9 */))/* b@175 */Vector3/* Vector3@17 */ c  = func/* unresolved */(new Vector2/* Vector2@5 */(X/* X@8 */, Y/* Y@9 */+epsilon/* epsilon@173 */))/* c@176 */Vector3/* Vector3@17 */ d  = func/* unresolved */(new Vector2/* Vector2@5 */(X/* X@8 */, Y/* Y@9 */-epsilon/* epsilon@173 */))/* d@177 */Vector3/* Vector3@17 */ v1  = b/* b@175 */-a/* a@174 *//* v1@178 */Vector3/* Vector3@17 */ v2  = d/* d@177 */-c/* c@176 *//* v2@179 */Vector3/* Vector3@17 */ r  = Cross/* Cross@32 */(v2/* v2@179 */)/* r@180 */IArray/* IArray@0 */uvs/* uvs@181 */Func/* unresolved */func/* func@182 */uv/* uv@183 */Func/* unresolved */func/* func@184 */Int32/* unresolved */usegs/* usegs@185 */Int32/* unresolved */vsegs/* vsegs@186 */Int32/* unresolved */usegs/* usegs@187 */Int32/* unresolved */vsegs/* vsegs@188 */i/* i@189 */Int32/* unresolved */ row  = i/* i@189 *//vsegs/* vsegs@188 *//* row@190 */Int32/* unresolved */ col  = i/* i@189 */%usegs/* usegs@187 *//* col@191 */Int32/* unresolved */usegs/* usegs@192 */Int32/* unresolved */vsegs/* vsegs@193 */i/* i@194 */Int32/* unresolved */ row  = i/* i@194 *//(vsegs/* vsegs@193 */-1)/* row@195 */Int32/* unresolved */ col  = i/* i@194 */%(usegs/* usegs@192 */-1)/* col@196 */Int32/* unresolved */ nextCol  = (col/* col@196 */+1)/* nextCol@197 */Int32/* unresolved */ nextRow  = (row/* row@195 */+1)/* nextRow@198 */Int32/* unresolved */ a  = (row/* row@195 */*usegs/* usegs@192 */)+col/* col@196 *//* a@199 */Int32/* unresolved */ b  = (row/* row@195 */*usegs/* usegs@192 */)+nextCol/* nextCol@197 *//* b@200 */Int32/* unresolved */ c  = (nextRow/* nextRow@198 */*usegs/* usegs@192 */)+nextCol/* nextCol@197 *//* c@201 */Int32/* unresolved */ d  = (nextRow/* nextRow@198 */*usegs/* usegs@192 */)+col/* col@196 *//* d@202 */Vector2/* Vector2@5 */uv/* uv@203 */Single/* unresolved */radius/* radius@204 */Vector2/* Vector2@5 */uv/* uv@205 */Single/* unresolved */radius/* radius@206 */Single/* unresolved */tube/* tube@207 */Vector3/* Vector3@17 */ x  = new Vector3/* Vector3@17 */(1, 2, 3)/* x@208 */Vector3/* Vector3@17 */ y  = x/* x@208 */+x/* x@208 *//* y@209 */Int32/* unresolved */rows/* rows@210 */Int32/* unresolved */cols/* cols@211 */Single/* unresolved */radius/* radius@212 */Single/* unresolved */tube/* tube@213 */uv/* uv@214 */IArray/* IArray@0 */faces/* faces@215 */