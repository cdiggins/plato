//==begin==//
public interface IArray<T/* T@3 */>/* IArray@0 */
{
  Int32/* unresolved */ Count;
   this[Int32/* unresolved */index/* index@110 */]
  {
    get{
        
      }
      }
  }
//==end==//
//==begin==//
public class Array<T/* T@9 */>/* Array@4 */
{
  public Array(Int32/* unresolved */count/* count@111 */, Func/* unresolved */func/* func@112 */)/* @5 */
  {
      return (Count/* Count@7 */, Func/* Func@6 */) = (count/* count@111 */, func/* func@112 */);
      
    }
    Func/* unresolved */ Func;
  Int32/* unresolved */ Count;
   this[Int32/* unresolved */index/* index@113 */]
  {
    get}
  }
//==end==//
//==begin==//
public class Vector2/* Vector2@10 */
{
  public Vector2(Single/* unresolved */x = 0/* x@114 */, Single/* unresolved */y = 0/* y@115 */)/* @11 */
  {
      return (X/* X@17 */, Y/* Y@18 */) = (x/* x@114 */, y/* y@115 */);
      
    }
    Vector2/* Vector2@10 */ Zero;
  Single/* unresolved */ MagnitudeSquared;
  Single/* unresolved */ Magnitude;
  Vector2/* Vector2@10 */ Normal;
  public Vector2/* Vector2@10 */ WithX(Single/* unresolved */x/* x@123 */)
  {
    return new Vector2/* Vector2@10 */(x/* x@123 */, Y/* Y@18 */);
    
  }
  public Vector2/* Vector2@10 */ WithY(Single/* unresolved */y/* y@124 */)
  {
    return new Vector2/* Vector2@10 */(X/* X@17 */, y/* y@124 */);
    
  }
  public Single/* unresolved */ Dot(Vector2/* Vector2@10 */v/* v@125 */)
  {
    return X/* X@17 */*X/* X@17 */+Y/* Y@18 */*Y/* Y@18 */;
    
  }
  public String/* unresolved */ ToString()
  {
    return "Vector2("+X/* X@17 */+","+Y/* Y@18 */+")";
    
  }
  public static Vector2/* Vector2@10 */ operator implicit(Single/* unresolved */v/* v@116 */)
  {
    return new Vector2/* Vector2@10 */(v/* v@116 */, v/* v@116 */);
    
  }
  public static Vector2/* Vector2@10 */ operator +(Vector2/* Vector2@10 */q/* q@117 */, Vector2/* Vector2@10 */r/* r@118 */)
  {
    return new Vector2/* Vector2@10 */(X/* X@17 */+X/* X@17 */, Y/* Y@18 */+Y/* Y@18 */);
    
  }
  public static Vector2/* Vector2@10 */ operator *(Vector2/* Vector2@10 */q/* q@119 */, Vector2/* Vector2@10 */r/* r@120 */)
  {
    return new Vector2/* Vector2@10 */(X/* X@17 */*X/* X@17 */, Y/* Y@18 */*Y/* Y@18 */);
    
  }
  public static Vector2/* Vector2@10 */ operator /(Vector2/* Vector2@10 */q/* q@121 */, Vector2/* Vector2@10 */r/* r@122 */)
  {
    return new Vector2/* Vector2@10 */(X/* X@17 *//X/* X@17 */, Y/* Y@18 *//Y/* Y@18 */);
    
  }
  }
//==end==//
//==begin==//
public class Vector3/* Vector3@26 */
{
  public Vector3(Single/* unresolved */x = 0/* x@126 */, Single/* unresolved */y = 0/* y@127 */, Single/* unresolved */z = 0/* z@128 */)/* @27 */
  {
      return (X/* X@34 */, Y/* Y@35 */, Z/* Z@36 */) = (x/* x@126 */, y/* y@127 */, z/* z@128 */);
      
    }
    Vector3/* Vector3@26 */ Zero;
  Single/* unresolved */ MagnitudeSquared;
  Single/* unresolved */ Magnitude;
  Vector3/* Vector3@26 */ Normal;
  Int32/* unresolved */ Count;
  public Vector3/* Vector3@26 */ WithX(Single/* unresolved */x/* x@138 */)
  {
    return new Vector3/* Vector3@26 */(x/* x@138 */, Y/* Y@35 */, Z/* Z@36 */);
    
  }
  public Vector3/* Vector3@26 */ WithY(Single/* unresolved */y/* y@139 */)
  {
    return new Vector3/* Vector3@26 */(X/* X@34 */, y/* y@139 */, Z/* Z@36 */);
    
  }
  public Vector3/* Vector3@26 */ WithZ(Single/* unresolved */z/* z@140 */)
  {
    return new Vector3/* Vector3@26 */(X/* X@34 */, Y/* Y@35 */, z/* z@140 */);
    
  }
  public Single/* unresolved */ Dot(Vector3/* Vector3@26 */v/* v@141 */)
  {
    return X/* X@34 */*X/* X@34 */+Y/* Y@35 */*Y/* Y@35 */+Z/* Z@36 */*Z/* Z@36 */;
    
  }
  public String/* unresolved */ ToString()
  {
    return "Vector3("+X/* X@34 */+","+Y/* Y@35 */+","+Z/* Z@36 */+")";
    
  }
  public Vector3/* Vector3@26 */ Cross(Vector3/* Vector3@26 */v/* v@142 */)
  {
    return new Vector3/* Vector3@26 */(Y/* Y@35 */*Z/* Z@36 */-Z/* Z@36 */*Y/* Y@35 */, Z/* Z@36 */*X/* X@34 */-X/* X@34 */*Z/* Z@36 */, X/* X@34 */*Y/* Y@35 */-Y/* Y@35 */*X/* X@34 */);
    
  }
   this[Int32/* unresolved */i/* i@143 */]
  {
    get}
  public static Vector3/* Vector3@26 */ operator implicit(Single/* unresolved */v/* v@129 */)
  {
    return new Vector3/* Vector3@26 */(v/* v@129 */, v/* v@129 */, v/* v@129 */);
    
  }
  public static Vector3/* Vector3@26 */ operator +(Vector3/* Vector3@26 */q/* q@130 */, Vector3/* Vector3@26 */r/* r@131 */)
  {
    return new Vector3/* Vector3@26 */(X/* X@34 */+X/* X@34 */, Y/* Y@35 */+Y/* Y@35 */, Z/* Z@36 */+Z/* Z@36 */);
        
  }
  public static Vector3/* Vector3@26 */ operator -(Vector3/* Vector3@26 */q/* q@132 */, Vector3/* Vector3@26 */r/* r@133 */)
  {
    return new Vector3/* Vector3@26 */(X/* X@34 */-X/* X@34 */, Y/* Y@35 */-Y/* Y@35 */, Z/* Z@36 */-Z/* Z@36 */);
    
  }
  public static Vector3/* Vector3@26 */ operator *(Vector3/* Vector3@26 */q/* q@134 */, Vector3/* Vector3@26 */r/* r@135 */)
  {
    return new Vector3/* Vector3@26 */(X/* X@34 */*X/* X@34 */, Y/* Y@35 */*Y/* Y@35 */, Z/* Z@36 */*Z/* Z@36 */);
    
  }
  public static Vector3/* Vector3@26 */ operator /(Vector3/* Vector3@26 */q/* q@136 */, Vector3/* Vector3@26 */r/* r@137 */)
  {
    return new Vector3/* Vector3@26 */(X/* X@34 *//X/* X@34 */, Y/* Y@35 *//Y/* Y@35 */, Z/* Z@36 *//Z/* Z@36 */);
    
  }
  }
//==end==//
//==begin==//
public class Int3/* Int3@48 */
{
  public Int3(Int32/* unresolved */x = 0/* x@144 */, Int32/* unresolved */y = 0/* y@145 */, Int32/* unresolved */z = 0/* z@146 */)/* @49 */
  {
      return (X/* X@52 */, Y/* Y@53 */, Z/* Z@54 */) = (x/* x@144 */, y/* y@145 */, z/* z@146 */);
      
    }
    Int3/* Int3@48 */ Zero;
  Int32/* unresolved */ Count;
  public Int3/* Int3@48 */ WithX(Int32/* unresolved */x/* x@147 */)
  {
    return new Int3/* Int3@48 */(x/* x@147 */, Y/* Y@53 */, Z/* Z@54 */);
    
  }
  public Int3/* Int3@48 */ WithY(Int32/* unresolved */y/* y@148 */)
  {
    return new Int3/* Int3@48 */(X/* X@52 */, y/* y@148 */, Z/* Z@54 */);
    
  }
  public Int3/* Int3@48 */ WithZ(Int32/* unresolved */z/* z@149 */)
  {
    return new Int3/* Int3@48 */(X/* X@52 */, Y/* Y@53 */, z/* z@149 */);
    
  }
  public String/* unresolved */ ToString()
  {
    return "Int3("+X/* X@52 */+","+Y/* Y@53 */+","+Z/* Z@54 */+")";
    
  }
   this[Int32/* unresolved */i/* i@150 */]
  {
    get}
  }
//==end==//
//==begin==//
public class Int4/* Int4@60 */
{
  public Int4(Int32/* unresolved */x = 0/* x@151 */, Int32/* unresolved */y = 0/* y@152 */, Int32/* unresolved */z = 0/* z@153 */, Int32/* unresolved */w = 0/* w@154 */)/* @61 */
  {
      return (X/* X@64 */, Y/* Y@65 */, Z/* Z@66 */, W/* W@67 */) = (x/* x@151 */, y/* y@152 */, z/* z@153 */, w/* w@154 */);
      
    }
    Int4/* Int4@60 */ Zero;
  Int32/* unresolved */ Count;
  public Int4/* Int4@60 */ WithX(Int32/* unresolved */x/* x@155 */)
  {
    return new Int4/* Int4@60 */(x/* x@155 */, Y/* Y@65 */, Z/* Z@66 */, W/* W@67 */);
    
  }
  public Int4/* Int4@60 */ WithY(Int32/* unresolved */y/* y@156 */)
  {
    return new Int4/* Int4@60 */(X/* X@64 */, y/* y@156 */, Z/* Z@66 */, W/* W@67 */);
    
  }
  public Int4/* Int4@60 */ WithZ(Int32/* unresolved */z/* z@157 */)
  {
    return new Int4/* Int4@60 */(X/* X@64 */, Y/* Y@65 */, z/* z@157 */, W/* W@67 */);
    
  }
  public Int4/* Int4@60 */ WithW(Int32/* unresolved */w/* w@158 */)
  {
    return new Int4/* Int4@60 */(X/* X@64 */, Y/* Y@65 */, Z/* Z@66 */, w/* w@158 */);
    
  }
  public String/* unresolved */ ToString()
  {
    return "Int4("+X/* X@64 */+","+Y/* Y@65 */+","+Z/* Z@66 */+","+W/* W@67 */+")";
    
  }
   this[Int32/* unresolved */i/* i@159 */]
  {
    get}
  }
//==end==//
//==begin==//
public class Points/* Points@74 */
{
  public Points(IArray/* IArray@0 */positions/* positions@160 */, IArray/* IArray@0 */uvs/* uvs@161 */, IArray/* IArray@0 */normals/* normals@162 */)/* @75 */
  {
      return (Positions/* Positions@76 */, UVs/* UVs@77 */, Normals/* Normals@78 */) = (positions/* positions@160 */, uvs/* uvs@161 */, normals/* normals@162 */);
      
    }
    IArray/* IArray@0 */ Positions;
  IArray/* IArray@0 */ UVs;
  IArray/* IArray@0 */ Normals;
  }
//==end==//
//==begin==//
public class TriMesh/* TriMesh@79 */
{
  public TriMesh(Points/* Points@74 */points/* points@163 */, IArray/* IArray@0 */faces/* faces@164 */)/* @80 */
  {
      return (Points/* Points@81 */, Faces/* Faces@82 */) = (points/* points@163 */, faces/* faces@164 */);
      
    }
    Points/* Points@74 */ Points;
  IArray/* IArray@0 */ Faces;
  }
//==end==//
//==begin==//
public class QuadMesh/* QuadMesh@83 */
{
  public QuadMesh(Points/* Points@74 */points/* points@165 */, IArray/* IArray@0 */faces/* faces@166 */)/* @84 */
  {
      return (Points/* Points@85 */, Faces/* Faces@86 */) = (points/* points@165 */, faces/* faces@166 */);
      
    }
    Points/* Points@74 */ Points;
  IArray/* IArray@0 */ Faces;
  }
//==end==//
//==begin==//
public class Extensions/* Extensions@87 */
{
  public static IArray/* IArray@0 */ ToIArray(IReadOnlyList/* unresolved */self/* self@167 */)
  {
    return Select/* Select@91 */(#lambdaIr);
    
  }
  public static /* unresolved */ ToArray(IArray/* IArray@0 */self/* self@169 */)
  {
    /* unresolved */ r  = new []{}/* r@170 *//* r@170 */;
    {
      Int32/* unresolved */ i  = 0/* i@171 *//* i@171 */;
      while(i/* i@171 */<Count/* Count@1 */)
      {
          r/* r@170 */[i/* i@171 */] = self/* self@169 */[i/* i@171 */];
          ++i/* i@171 */;
          
        }
        
    }
    return r/* r@170 */;
    
  }
  public static /* unresolved */ ToFloatArray(IArray/* IArray@0 */self/* self@172 */)
  {
    return ToArray/* ToArray@89 */();
    
  }
  public static IArray/* IArray@0 */ Select(Int32/* unresolved */count/* count@173 */, Func/* unresolved */func/* func@174 */)
  {
    return new Array/* Array@4 */(count/* count@173 */, func/* func@174 */);
    
  }
  public static IArray/* IArray@0 */ SelectMany(IArray/* IArray@0 */self/* self@175 */, Func/* unresolved */func/* func@176 */)
  {
    List/* unresolved */ r  = new List/* unresolved */()/* r@177 *//* r@177 */;
    {
      Int32/* unresolved */ i  = 0/* i@178 *//* i@178 */;
      while(i/* i@178 */<Count/* Count@1 */)
      {
          {
            IArray/* IArray@0 */ tmp  = func/* unresolved */(self/* self@175 */[i/* i@178 */])/* tmp@179 *//* tmp@179 */;
            {
              Int32/* unresolved */ j  = 0/* j@180 *//* j@180 */;
              while(j/* j@180 */<Count/* Count@1 */)
              {
                  Add/* unresolved */(tmp/* tmp@179 */[j/* j@180 */]);
                  ++j/* j@180 */;
                  
                }
                
            }
            
          }
          ++i/* i@178 */;
          
        }
        
    }
    return Select/* Select@91 */(#lambdaIr);
    
  }
  public static IArray/* IArray@0 */ Select(IArray/* IArray@0 */self/* self@182 */, Func/* unresolved */func/* func@183 */)
  {
    return Select/* Select@91 */(#lambdaIr);
    
  }
  public static Single/* unresolved */ Cos(Single/* unresolved */self/* self@185 */)
  {
    return Cos/* unresolved */(self/* self@185 */);
    
  }
  public static Single/* unresolved */ Sin(Single/* unresolved */self/* self@186 */)
  {
    return Sin/* unresolved */(self/* self@186 */);
    
  }
  public static Single/* unresolved */ UnitToRad(Single/* unresolved */self/* self@187 */)
  {
    return self/* self@187 */*PI/* unresolved */;
    
  }
  public static IArray/* IArray@0 */ SampleFloats(Int32/* unresolved */count/* count@188 */, Single/* unresolved */max = 1.0f/* max@189 */)
  {
    return Select/* Select@91 */(#lambdaIr);
    
  }
  public static IArray/* IArray@0 */ ToTriangles(IArray/* IArray@0 */self/* self@191 */)
  {
    return SelectMany/* SelectMany@92 */(#lambdaIr);
    
  }
  public static QuadMesh/* QuadMesh@83 */ ToQuadMesh(Func/* unresolved */func/* func@193 */, Int32/* unresolved */rows/* rows@194 */, Int32/* unresolved */cols/* cols@195 */)
  {
    return new QuadMesh/* QuadMesh@83 */(ComputeQuadStripPoints/* ComputeQuadStripPoints@102 */(func/* func@193 */, rows/* rows@194 */, cols/* cols@195 */), ComputeQuadStripIndices/* ComputeQuadStripIndices@104 */(rows/* rows@194 */, cols/* cols@195 */));
    
  }
  public static Vector3/* Vector3@26 */ UVToNormal(Vector2/* Vector2@10 */uv/* uv@196 */, Func/* unresolved */func/* func@197 */, Single/* unresolved */epsilon = 0.00001f/* epsilon@198 */)
  {
    Vector3/* Vector3@26 */ a  = func/* unresolved */(new Vector2/* Vector2@10 */(X/* X@17 */+epsilon/* epsilon@198 */, Y/* Y@18 */))/* a@199 *//* a@199 */;
    Vector3/* Vector3@26 */ b  = func/* unresolved */(new Vector2/* Vector2@10 */(X/* X@17 */-epsilon/* epsilon@198 */, Y/* Y@18 */))/* b@200 *//* b@200 */;
    Vector3/* Vector3@26 */ c  = func/* unresolved */(new Vector2/* Vector2@10 */(X/* X@17 */, Y/* Y@18 */+epsilon/* epsilon@198 */))/* c@201 *//* c@201 */;
    Vector3/* Vector3@26 */ d  = func/* unresolved */(new Vector2/* Vector2@10 */(X/* X@17 */, Y/* Y@18 */-epsilon/* epsilon@198 */))/* d@202 *//* d@202 */;
    Vector3/* Vector3@26 */ v1  = b/* b@200 */-a/* a@199 *//* v1@203 *//* v1@203 */;
    Vector3/* Vector3@26 */ v2  = d/* d@202 */-c/* c@201 *//* v2@204 *//* v2@204 */;
    Vector3/* Vector3@26 */ r  = Cross/* Cross@46 */(v2/* v2@204 */)/* r@205 *//* r@205 */;
    return Normal/* Normal@31 */;
    
  }
  public static Points/* Points@74 */ UVsToPoints(IArray/* IArray@0 */uvs/* uvs@206 */, Func/* unresolved */func/* func@207 */)
  {
    return new Points/* Points@74 */(Select/* Select@93 */(func/* func@207 */), uvs/* uvs@206 */, Select/* Select@93 */(#lambdaIr));
    
  }
  public static Points/* Points@74 */ ComputeQuadStripPoints(Func/* unresolved */func/* func@209 */, Int32/* unresolved */usegs/* usegs@210 */, Int32/* unresolved */vsegs/* vsegs@211 */)
  {
    return UVsToPoints/* UVsToPoints@101 */(func/* func@209 */);
    
  }
  public static IArray/* IArray@0 */ ComputeQuadStripUVs(Int32/* unresolved */usegs/* usegs@212 */, Int32/* unresolved */vsegs/* vsegs@213 */)
  {
    return new Array/* Array@4 */(usegs/* usegs@212 */*vsegs/* vsegs@213 */, #lambdaIr);
    
  }
  public static IArray/* IArray@0 */ ComputeQuadStripIndices(Int32/* unresolved */usegs/* usegs@217 */, Int32/* unresolved */vsegs/* vsegs@218 */)
  {
    return new Array/* Array@4 */((usegs/* usegs@217 */-1)*(vsegs/* vsegs@218 */-1), #lambdaIr);
    
  }
  public static Vector3/* Vector3@26 */ UvToSphere(Vector2/* Vector2@10 */uv/* uv@228 */, Single/* unresolved */radius/* radius@229 */)
  {
    return new Vector3/* Vector3@26 */(-radius/* radius@229 */*Cos/* Cos@94 */()*Sin/* Sin@95 */(), radius/* radius@229 */*Cos/* Cos@94 */(), radius/* radius@229 */*Cos/* Cos@94 */()*Sin/* Sin@95 */());
    
  }
  public static Vector3/* Vector3@26 */ UvToTorus(Vector2/* Vector2@10 */uv/* uv@230 */, Single/* unresolved */radius/* radius@231 */, Single/* unresolved */tube/* tube@232 */)
  {
    uv/* uv@230 */ = uv/* uv@230 */*PI/* unresolved */*2;
    return new Vector3/* Vector3@26 */((radius/* radius@231 */+tube/* tube@232 */*Cos/* Cos@94 */())*Cos/* Cos@94 */(), (radius/* radius@231 */+tube/* tube@232 */*Cos/* Cos@94 */())*Sin/* Sin@95 */(), tube/* tube@232 */*Sin/* Sin@95 */());
    
  }
  public static Void/* unresolved */ TestOperator()
  {
    Vector3/* Vector3@26 */ x  = new Vector3/* Vector3@26 */(1, 2, 3)/* x@233 *//* x@233 */;
    Vector3/* Vector3@26 */ y  = x/* x@233 */+x/* x@233 *//* y@234 *//* y@234 */;
    
  }
  public static QuadMesh/* QuadMesh@83 */ Torus(Int32/* unresolved */rows/* rows@235 */, Int32/* unresolved */cols/* cols@236 */, Single/* unresolved */radius/* radius@237 */, Single/* unresolved */tube/* tube@238 */)
  {
    return ToQuadMesh/* ToQuadMesh@99 */(#lambdaIr, rows/* rows@235 */, cols/* cols@236 */);
    
  }
  public static /* unresolved */ ToIntArray(IArray/* IArray@0 */faces/* faces@240 */)
  {
    return ToArray/* ToArray@89 */();
    
  }
  }
//==end==//
