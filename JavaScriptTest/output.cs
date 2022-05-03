//==begin==//
public interface IArray<T/* T@3 */> : /* IArray@0 */
{
  public Int32/* unresolved */ Count{
  get
    {
      
    }
    }
  ;
  public T/* T@3 */ this[Int32/* unresolved */index/* index@117 */]
  {
  get
  {
      
    }
    }
  }
//==end==//
//==begin==//
public class Array<T/* T@9 */> : /* Array@4 */
{
  public Array(Int32/* unresolved */count/* count@118 */, Func<Int32/* unresolved */, T/* T@9 */>/* unresolved */func/* func@119 */)/* @5 */
  {
      return (Count/* Count@7 */, Func/* Func@6 */) = (count/* count@118 */, func/* func@119 */);
      
    }
    public Func<Int32/* unresolved */, T/* T@9 */>/* unresolved */ Func{
  get
    {
      
    }
    }
  ;
  public Int32/* unresolved */ Count{
  get
    {
      
    }
    }
  ;
  public T/* T@9 */ this[Int32/* unresolved */index/* index@120 */]
  {
  get
  }
  }
//==end==//
//==begin==//
public class Vector2 : /* Vector2@10 */
{
  public Vector2(Single/* unresolved */x = 0/* x@121 */, Single/* unresolved */y = 0/* y@122 */)/* @11 */
  {
      return (X/* X@17 */, Y/* Y@18 */) = (x/* x@121 */, y/* y@122 */);
      
    }
    public static Vector2/* Vector2@10 */ Zero{
  get
    {
      return new Vector2/* Vector2@10 */();
      
    }
    }
  ;
  public Single/* unresolved */ MagnitudeSquared{
  get
    {
      return Dot/* Dot@24 */(this);
      
    }
    }
  ;
  public Single/* unresolved */ Magnitude{
  get
    {
      return Sqrt/* unresolved */(MagnitudeSquared/* MagnitudeSquared@13 */);
      
    }
    }
  ;
  public Vector2/* Vector2@10 */ Normal{
  get
    {
      return this/Magnitude/* Magnitude@14 */;
      
    }
    }
  ;
  public Vector2/* Vector2@10 */ WithX(Single/* unresolved */x/* x@130 */)
  {
    return new Vector2/* Vector2@10 */(x/* x@130 */, Y/* Y@18 */);
    
  }
  public Vector2/* Vector2@10 */ WithY(Single/* unresolved */y/* y@131 */)
  {
    return new Vector2/* Vector2@10 */(X/* X@17 */, y/* y@131 */);
    
  }
  public Single/* unresolved */ Dot(Vector2/* Vector2@10 */v/* v@132 */)
  {
    return X/* X@17 */*v/* v@132 */.X/* X@17 */+v/* v@132 */.Y/* Y@18 */*Y/* Y@18 */;
    
  }
  public String/* unresolved */ ToString()
  {
    return "Vector2("+X/* X@17 */+","+Y/* Y@18 */+")";
    
  }
  public static Vector2/* Vector2@10 */ operator implicit(Single/* unresolved */v/* v@123 */)
  {
    return new Vector2/* Vector2@10 */(v/* v@123 */, v/* v@123 */);
    
  }
  public static Vector2/* Vector2@10 */ operator +(Vector2/* Vector2@10 */q/* q@124 */, Vector2/* Vector2@10 */r/* r@125 */)
  {
    return new Vector2/* Vector2@10 */(q/* q@124 */.X/* X@17 */+r/* r@125 */.X/* X@17 */, q/* q@124 */.Y/* Y@18 */+r/* r@125 */.Y/* Y@18 */);
    
  }
  public static Vector2/* Vector2@10 */ operator *(Vector2/* Vector2@10 */q/* q@126 */, Vector2/* Vector2@10 */r/* r@127 */)
  {
    return new Vector2/* Vector2@10 */(q/* q@126 */.X/* X@17 */*r/* r@127 */.X/* X@17 */, q/* q@126 */.Y/* Y@18 */*r/* r@127 */.Y/* Y@18 */);
    
  }
  public static Vector2/* Vector2@10 */ operator /(Vector2/* Vector2@10 */q/* q@128 */, Vector2/* Vector2@10 */r/* r@129 */)
  {
    return new Vector2/* Vector2@10 */(q/* q@128 */.X/* X@17 *//r/* r@129 */.X/* X@17 */, q/* q@128 */.Y/* Y@18 *//r/* r@129 */.Y/* Y@18 */);
    
  }
  }
//==end==//
//==begin==//
public class Vector3 : /* Vector3@26 */
{
  public Vector3(Single/* unresolved */x = 0/* x@133 */, Single/* unresolved */y = 0/* y@134 */, Single/* unresolved */z = 0/* z@135 */)/* @27 */
  {
      return (X/* X@34 */, Y/* Y@35 */, Z/* Z@36 */) = (x/* x@133 */, y/* y@134 */, z/* z@135 */);
      
    }
    public static Vector3/* Vector3@26 */ Zero{
  get
    {
      return new Vector3/* Vector3@26 */();
      
    }
    }
  ;
  public Single/* unresolved */ MagnitudeSquared{
  get
    {
      return Dot/* Dot@44 */(this);
      
    }
    }
  ;
  public Single/* unresolved */ Magnitude{
  get
    {
      return Sqrt/* unresolved */(MagnitudeSquared/* MagnitudeSquared@29 */);
      
    }
    }
  ;
  public Vector3/* Vector3@26 */ Normal{
  get
    {
      return this/Magnitude/* Magnitude@30 */;
      
    }
    }
  ;
  public Int32/* unresolved */ Count{
  get
    {
      return 3;
      
    }
    }
  ;
  public Vector3/* Vector3@26 */ WithX(Single/* unresolved */x/* x@145 */)
  {
    return new Vector3/* Vector3@26 */(x/* x@145 */, Y/* Y@35 */, Z/* Z@36 */);
    
  }
  public Vector3/* Vector3@26 */ WithY(Single/* unresolved */y/* y@146 */)
  {
    return new Vector3/* Vector3@26 */(X/* X@34 */, y/* y@146 */, Z/* Z@36 */);
    
  }
  public Vector3/* Vector3@26 */ WithZ(Single/* unresolved */z/* z@147 */)
  {
    return new Vector3/* Vector3@26 */(X/* X@34 */, Y/* Y@35 */, z/* z@147 */);
    
  }
  public Single/* unresolved */ Dot(Vector3/* Vector3@26 */v/* v@148 */)
  {
    return X/* X@34 */*v/* v@148 */.X/* X@34 */+v/* v@148 */.Y/* Y@35 */*Y/* Y@35 */+Z/* Z@36 */*v/* v@148 */.Z/* Z@36 */;
    
  }
  public String/* unresolved */ ToString()
  {
    return "Vector3("+X/* X@34 */+","+Y/* Y@35 */+","+Z/* Z@36 */+")";
    
  }
  public Vector3/* Vector3@26 */ Cross(Vector3/* Vector3@26 */v/* v@149 */)
  {
    return new Vector3/* Vector3@26 */(Y/* Y@35 */*v/* v@149 */.Z/* Z@36 */-Z/* Z@36 */*v/* v@149 */.Y/* Y@35 */, Z/* Z@36 */*v/* v@149 */.X/* X@34 */-X/* X@34 */*v/* v@149 */.Z/* Z@36 */, X/* X@34 */*v/* v@149 */.Y/* Y@35 */-Y/* Y@35 */*v/* v@149 */.X/* X@34 */);
    
  }
  public Single/* unresolved */ this[Int32/* unresolved */i/* i@150 */]
  {
  get
  }
  public static Vector3/* Vector3@26 */ operator implicit(Single/* unresolved */v/* v@136 */)
  {
    return new Vector3/* Vector3@26 */(v/* v@136 */, v/* v@136 */, v/* v@136 */);
    
  }
  public static Vector3/* Vector3@26 */ operator +(Vector3/* Vector3@26 */q/* q@137 */, Vector3/* Vector3@26 */r/* r@138 */)
  {
    return new Vector3/* Vector3@26 */(q/* q@137 */.X/* X@34 */+r/* r@138 */.X/* X@34 */, q/* q@137 */.Y/* Y@35 */+r/* r@138 */.Y/* Y@35 */, q/* q@137 */.Z/* Z@36 */+r/* r@138 */.Z/* Z@36 */);
    
  }
  public static Vector3/* Vector3@26 */ operator -(Vector3/* Vector3@26 */q/* q@139 */, Vector3/* Vector3@26 */r/* r@140 */)
  {
    return new Vector3/* Vector3@26 */(q/* q@139 */.X/* X@34 */-r/* r@140 */.X/* X@34 */, q/* q@139 */.Y/* Y@35 */-r/* r@140 */.Y/* Y@35 */, q/* q@139 */.Z/* Z@36 */-r/* r@140 */.Z/* Z@36 */);
    
  }
  public static Vector3/* Vector3@26 */ operator *(Vector3/* Vector3@26 */q/* q@141 */, Vector3/* Vector3@26 */r/* r@142 */)
  {
    return new Vector3/* Vector3@26 */(q/* q@141 */.X/* X@34 */*r/* r@142 */.X/* X@34 */, q/* q@141 */.Y/* Y@35 */*r/* r@142 */.Y/* Y@35 */, q/* q@141 */.Z/* Z@36 */*r/* r@142 */.Z/* Z@36 */);
    
  }
  public static Vector3/* Vector3@26 */ operator /(Vector3/* Vector3@26 */q/* q@143 */, Vector3/* Vector3@26 */r/* r@144 */)
  {
    return new Vector3/* Vector3@26 */(q/* q@143 */.X/* X@34 *//r/* r@144 */.X/* X@34 */, q/* q@143 */.Y/* Y@35 *//r/* r@144 */.Y/* Y@35 */, q/* q@143 */.Z/* Z@36 *//r/* r@144 */.Z/* Z@36 */);
    
  }
  }
//==end==//
//==begin==//
public class Int3 : /* Int3@48 */
{
  public Int3(Int32/* unresolved */x = 0/* x@151 */, Int32/* unresolved */y = 0/* y@152 */, Int32/* unresolved */z = 0/* z@153 */)/* @49 */
  {
      return (X/* X@52 */, Y/* Y@53 */, Z/* Z@54 */) = (x/* x@151 */, y/* y@152 */, z/* z@153 */);
      
    }
    public static Int3/* Int3@48 */ Zero{
  get
    {
      return new Int3/* Int3@48 */();
      
    }
    }
  ;
  public Int32/* unresolved */ Count{
  get
    {
      return 3;
      
    }
    }
  ;
  public Int3/* Int3@48 */ WithX(Int32/* unresolved */x/* x@154 */)
  {
    return new Int3/* Int3@48 */(x/* x@154 */, Y/* Y@53 */, Z/* Z@54 */);
    
  }
  public Int3/* Int3@48 */ WithY(Int32/* unresolved */y/* y@155 */)
  {
    return new Int3/* Int3@48 */(X/* X@52 */, y/* y@155 */, Z/* Z@54 */);
    
  }
  public Int3/* Int3@48 */ WithZ(Int32/* unresolved */z/* z@156 */)
  {
    return new Int3/* Int3@48 */(X/* X@52 */, Y/* Y@53 */, z/* z@156 */);
    
  }
  public String/* unresolved */ ToString()
  {
    return "Int3("+X/* X@52 */+","+Y/* Y@53 */+","+Z/* Z@54 */+")";
    
  }
  public Int32/* unresolved */ this[Int32/* unresolved */i/* i@157 */]
  {
  get
  }
  }
//==end==//
//==begin==//
public class Int4 : /* Int4@60 */
{
  public Int4(Int32/* unresolved */x = 0/* x@158 */, Int32/* unresolved */y = 0/* y@159 */, Int32/* unresolved */z = 0/* z@160 */, Int32/* unresolved */w = 0/* w@161 */)/* @61 */
  {
      return (X/* X@64 */, Y/* Y@65 */, Z/* Z@66 */, W/* W@67 */) = (x/* x@158 */, y/* y@159 */, z/* z@160 */, w/* w@161 */);
      
    }
    public static Int4/* Int4@60 */ Zero{
  get
    {
      return new Int4/* Int4@60 */();
      
    }
    }
  ;
  public Int32/* unresolved */ Count{
  get
    {
      return 4;
      
    }
    }
  ;
  public Int4/* Int4@60 */ WithX(Int32/* unresolved */x/* x@162 */)
  {
    return new Int4/* Int4@60 */(x/* x@162 */, Y/* Y@65 */, Z/* Z@66 */, W/* W@67 */);
    
  }
  public Int4/* Int4@60 */ WithY(Int32/* unresolved */y/* y@163 */)
  {
    return new Int4/* Int4@60 */(X/* X@64 */, y/* y@163 */, Z/* Z@66 */, W/* W@67 */);
    
  }
  public Int4/* Int4@60 */ WithZ(Int32/* unresolved */z/* z@164 */)
  {
    return new Int4/* Int4@60 */(X/* X@64 */, Y/* Y@65 */, z/* z@164 */, W/* W@67 */);
    
  }
  public Int4/* Int4@60 */ WithW(Int32/* unresolved */w/* w@165 */)
  {
    return new Int4/* Int4@60 */(X/* X@64 */, Y/* Y@65 */, Z/* Z@66 */, w/* w@165 */);
    
  }
  public String/* unresolved */ ToString()
  {
    return "Int4("+X/* X@64 */+","+Y/* Y@65 */+","+Z/* Z@66 */+","+W/* W@67 */+")";
    
  }
  public Int32/* unresolved */ this[Int32/* unresolved */i/* i@166 */]
  {
  get
  }
  }
//==end==//
//==begin==//
public class Points : /* Points@74 */
{
  public Points(IArray<Vector3/* Vector3@26 */>/* IArray@0 */positions/* positions@167 */, IArray<Vector2/* Vector2@10 */>/* IArray@0 */uvs/* uvs@168 */, IArray<Vector3/* Vector3@26 */>/* IArray@0 */normals/* normals@169 */)/* @75 */
  {
      return (Positions/* Positions@76 */, UVs/* UVs@77 */, Normals/* Normals@78 */) = (positions/* positions@167 */, uvs/* uvs@168 */, normals/* normals@169 */);
      
    }
    public IArray<Vector3/* Vector3@26 */>/* IArray@0 */ Positions{
  get
    {
      
    }
    }
  ;
  public IArray<Vector2/* Vector2@10 */>/* IArray@0 */ UVs{
  get
    {
      
    }
    }
  ;
  public IArray<Vector3/* Vector3@26 */>/* IArray@0 */ Normals{
  get
    {
      
    }
    }
  ;
  }
//==end==//
//==begin==//
public class TriMesh : /* TriMesh@79 */
{
  public TriMesh(Points/* Points@74 */points/* points@170 */, IArray<Int3/* Int3@48 */>/* IArray@0 */faces/* faces@171 */)/* @80 */
  {
      return (Points/* Points@81 */, Faces/* Faces@82 */) = (points/* points@170 */, faces/* faces@171 */);
      
    }
    public Points/* Points@74 */ Points{
  get
    {
      
    }
    }
  ;
  public IArray<Int3/* Int3@48 */>/* IArray@0 */ Faces{
  get
    {
      
    }
    }
  ;
  }
//==end==//
//==begin==//
public class QuadMesh : /* QuadMesh@83 */
{
  public QuadMesh(Points/* Points@74 */points/* points@172 */, IArray<Int4/* Int4@60 */>/* IArray@0 */faces/* faces@173 */)/* @84 */
  {
      return (Points/* Points@85 */, Faces/* Faces@86 */) = (points/* points@172 */, faces/* faces@173 */);
      
    }
    public Points/* Points@74 */ Points{
  get
    {
      
    }
    }
  ;
  public IArray<Int4/* Int4@60 */>/* IArray@0 */ Faces{
  get
    {
      
    }
    }
  ;
  }
//==end==//
//==begin==//
public class Extensions : /* Extensions@87 */
{
  public static IArray<T/* T@89 */>/* IArray@0 */ ToIArray<T/* T@89 */>(IReadOnlyList<T/* T@89 */>/* unresolved */self/* self@174 */)
  {
    return Select/* Select@93 */(/* Captured: IReadOnlyList<T/* T@89 */>/* unresolved */self/* self@174 */*/(i/* i@175 */)
     => {
        return self/* self@174 */[i/* i@175 */];
        
      }
      );
    
  }
  public static /* unresolved */ ToArray<T/* T@91 */>(IArray<T/* T@91 */>/* IArray@0 */self/* self@176 */)
  {
    /* unresolved */ r  = new []{}/* r@177 *//* r@177 */;
    {
      Int32/* unresolved */ i  = 0/* i@178 *//* i@178 */;
      while(i/* i@178 */<self/* self@176 */.Count/* Count@1 */)
      {
          r/* r@177 */[i/* i@178 */] = self/* self@176 */[i/* i@178 */];
          ++i/* i@178 */;
          
        }
        
    }
    return r/* r@177 */;
    
  }
  public static /* unresolved */ ToFloatArray(IArray<Vector3/* Vector3@26 */>/* IArray@0 */self/* self@179 */)
  {
    return ToArray/* ToArray@90 */();
    
  }
  public static IArray<T/* T@94 */>/* IArray@0 */ Select<T/* T@94 */>(Int32/* unresolved */count/* count@180 */, Func<Int32/* unresolved */, T/* T@94 */>/* unresolved */func/* func@181 */)
  {
    return new Array<T/* T@94 */>/* Array@4 */(count/* count@180 */, func/* func@181 */);
    
  }
  public static IArray<U/* U@97 */>/* IArray@0 */ SelectMany<T/* T@96 */, U/* U@97 */>(IArray<T/* T@96 */>/* IArray@0 */self/* self@182 */, Func<T/* T@96 */, IArray<U/* U@97 */>/* IArray@0 */>/* unresolved */func/* func@183 */)
  {
    List<U/* U@97 */>/* unresolved */ r  = new List<U/* U@97 */>/* unresolved */()/* r@184 *//* r@184 */;
    {
      Int32/* unresolved */ i  = 0/* i@185 *//* i@185 */;
      while(i/* i@185 */<self/* self@182 */.Count/* Count@1 */)
      {
          {
            IArray<U/* U@97 */>/* IArray@0 */ tmp  = func/* unresolved */(self/* self@182 */[i/* i@185 */])/* tmp@186 *//* tmp@186 */;
            {
              Int32/* unresolved */ j  = 0/* j@187 *//* j@187 */;
              while(j/* j@187 */<tmp/* tmp@186 */.Count/* Count@1 */)
              {
                  Add/* unresolved */(tmp/* tmp@186 */[j/* j@187 */]);
                  ++j/* j@187 */;
                  
                }
                
            }
            
          }
          ++i/* i@185 */;
          
        }
        
    }
    return Select/* Select@93 */(/* Captured: List<U/* U@97 */>/* unresolved */ r  = new List<U/* U@97 */>/* unresolved */()/* r@184 */i/* i@188 */*/(i/* i@188 */)
     => {
        return r/* r@184 */[i/* i@188 */];
        
      }
      );
    
  }
  public static IArray<U/* U@100 */>/* IArray@0 */ Select<T/* T@99 */, U/* U@100 */>(IArray<T/* T@99 */>/* IArray@0 */self/* self@189 */, Func<T/* T@99 */, U/* U@100 */>/* unresolved */func/* func@190 */)
  {
    return Select/* Select@93 */(/* Captured: IArray<T/* T@99 */>/* IArray@0 */self/* self@189 */Func<T/* T@99 */, U/* U@100 */>/* unresolved */func/* func@190 */*/(i/* i@191 */)
     => {
        return func/* unresolved */(self/* self@189 */[i/* i@191 */]);
        
      }
      );
    
  }
  public static Single/* unresolved */ Cos(Single/* unresolved */self/* self@192 */)
  {
    return Cos/* unresolved */(self/* self@192 */);
    
  }
  public static Single/* unresolved */ Sin(Single/* unresolved */self/* self@193 */)
  {
    return Sin/* unresolved */(self/* self@193 */);
    
  }
  public static Single/* unresolved */ UnitToRad(Single/* unresolved */self/* self@194 */)
  {
    return self/* self@194 */*MathF/* unresolved */.PI/* unresolved */;
    
  }
  public static IArray<Single/* unresolved */>/* IArray@0 */ SampleFloats(Int32/* unresolved */count/* count@195 */, Single/* unresolved */max = 1.0f/* max@196 */)
  {
    return Select/* Select@93 */(/* Captured: Int32/* unresolved */count/* count@195 */Single/* unresolved */max = 1.0f/* max@196 */*/(i/* i@197 */)
     => {
        return max/* max@196 */*count/* count@195 */;
        
      }
      );
    
  }
  public static IArray<Int3/* Int3@48 */>/* IArray@0 */ ToTriangles(IArray<Int4/* Int4@60 */>/* IArray@0 */self/* self@198 */)
  {
    return SelectMany/* SelectMany@95 */(/* Captured: */(f/* f@199 */)
     => {
        return ToIArray/* ToIArray@88 */();
        
      }
      );
    
  }
  public static QuadMesh/* QuadMesh@83 */ ToQuadMesh(Func<Vector2/* Vector2@10 */, Vector3/* Vector3@26 */>/* unresolved */func/* func@200 */, Int32/* unresolved */rows/* rows@201 */, Int32/* unresolved */cols/* cols@202 */)
  {
    return new QuadMesh/* QuadMesh@83 */(ComputeQuadStripPoints/* ComputeQuadStripPoints@109 */(func/* func@200 */, rows/* rows@201 */, cols/* cols@202 */), ComputeQuadStripIndices/* ComputeQuadStripIndices@111 */(rows/* rows@201 */, cols/* cols@202 */));
    
  }
  public static Vector3/* Vector3@26 */ UVToNormal(Vector2/* Vector2@10 */uv/* uv@203 */, Func<Vector2/* Vector2@10 */, Vector3/* Vector3@26 */>/* unresolved */func/* func@204 */, Single/* unresolved */epsilon = 0.00001f/* epsilon@205 */)
  {
    Vector3/* Vector3@26 */ a  = func/* unresolved */(new Vector2/* Vector2@10 */(uv/* uv@203 */.X/* X@17 */+epsilon/* epsilon@205 */, uv/* uv@203 */.Y/* Y@18 */))/* a@206 *//* a@206 */;
    Vector3/* Vector3@26 */ b  = func/* unresolved */(new Vector2/* Vector2@10 */(uv/* uv@203 */.X/* X@17 */-epsilon/* epsilon@205 */, uv/* uv@203 */.Y/* Y@18 */))/* b@207 *//* b@207 */;
    Vector3/* Vector3@26 */ c  = func/* unresolved */(new Vector2/* Vector2@10 */(uv/* uv@203 */.X/* X@17 */, uv/* uv@203 */.Y/* Y@18 */+epsilon/* epsilon@205 */))/* c@208 *//* c@208 */;
    Vector3/* Vector3@26 */ d  = func/* unresolved */(new Vector2/* Vector2@10 */(uv/* uv@203 */.X/* X@17 */, uv/* uv@203 */.Y/* Y@18 */-epsilon/* epsilon@205 */))/* d@209 *//* d@209 */;
    Vector3/* Vector3@26 */ v1  = b/* b@207 */-a/* a@206 *//* v1@210 *//* v1@210 */;
    Vector3/* Vector3@26 */ v2  = d/* d@209 */-c/* c@208 *//* v2@211 *//* v2@211 */;
    Vector3/* Vector3@26 */ r  = Cross/* Cross@46 */(v2/* v2@211 */)/* r@212 *//* r@212 */;
    return r/* r@212 */.Normal/* Normal@31 */;
    
  }
  public static Points/* Points@74 */ UVsToPoints(IArray<Vector2/* Vector2@10 */>/* IArray@0 */uvs/* uvs@213 */, Func<Vector2/* Vector2@10 */, Vector3/* Vector3@26 */>/* unresolved */func/* func@214 */)
  {
    return new Points/* Points@74 */(Select/* Select@98 */(func/* func@214 */), uvs/* uvs@213 */, Select/* Select@98 */(/* Captured: Func<Vector2/* Vector2@10 */, Vector3/* Vector3@26 */>/* unresolved */func/* func@214 */*/(uv/* uv@215 */)
     => {
        return UVToNormal/* UVToNormal@107 */(func/* func@214 */);
        
      }
      ));
    
  }
  public static Points/* Points@74 */ ComputeQuadStripPoints(Func<Vector2/* Vector2@10 */, Vector3/* Vector3@26 */>/* unresolved */func/* func@216 */, Int32/* unresolved */usegs/* usegs@217 */, Int32/* unresolved */vsegs/* vsegs@218 */)
  {
    return UVsToPoints/* UVsToPoints@108 */(func/* func@216 */);
    
  }
  public static IArray<Vector2/* Vector2@10 */>/* IArray@0 */ ComputeQuadStripUVs(Int32/* unresolved */usegs/* usegs@219 */, Int32/* unresolved */vsegs/* vsegs@220 */)
  {
    return new Array<Vector2/* Vector2@10 */>/* Array@4 */(usegs/* usegs@219 */*vsegs/* vsegs@220 */, /* Captured: Int32/* unresolved */usegs/* usegs@219 */Int32/* unresolved */vsegs/* vsegs@220 */*/(i/* i@221 */)
     => {
        Int32/* unresolved */ row  = i/* i@221 *//vsegs/* vsegs@220 *//* row@222 *//* row@222 */;
        Int32/* unresolved */ col  = i/* i@221 */%usegs/* usegs@219 *//* col@223 *//* col@223 */;
        return new Vector2/* Vector2@10 */((Single/* unresolved */)col/* col@223 *//(usegs/* usegs@219 */-1), (Single/* unresolved */)row/* row@222 *//(vsegs/* vsegs@220 */-1));
        
      }
      );
    
  }
  public static IArray<Int4/* Int4@60 */>/* IArray@0 */ ComputeQuadStripIndices(Int32/* unresolved */usegs/* usegs@224 */, Int32/* unresolved */vsegs/* vsegs@225 */)
  {
    return new Array<Int4/* Int4@60 */>/* Array@4 */((usegs/* usegs@224 */-1)*(vsegs/* vsegs@225 */-1), /* Captured: Int32/* unresolved */usegs/* usegs@224 */Int32/* unresolved */vsegs/* vsegs@225 */*/(i/* i@226 */)
     => {
        Int32/* unresolved */ row  = i/* i@226 *//(vsegs/* vsegs@225 */-1)/* row@227 *//* row@227 */;
        Int32/* unresolved */ col  = i/* i@226 */%(usegs/* usegs@224 */-1)/* col@228 *//* col@228 */;
        Int32/* unresolved */ nextCol  = (col/* col@228 */+1)/* nextCol@229 *//* nextCol@229 */;
        Int32/* unresolved */ nextRow  = (row/* row@227 */+1)/* nextRow@230 *//* nextRow@230 */;
        Int32/* unresolved */ a  = (row/* row@227 */*usegs/* usegs@224 */)+col/* col@228 *//* a@231 *//* a@231 */;
        Int32/* unresolved */ b  = (row/* row@227 */*usegs/* usegs@224 */)+nextCol/* nextCol@229 *//* b@232 *//* b@232 */;
        Int32/* unresolved */ c  = (nextRow/* nextRow@230 */*usegs/* usegs@224 */)+nextCol/* nextCol@229 *//* c@233 *//* c@233 */;
        Int32/* unresolved */ d  = (nextRow/* nextRow@230 */*usegs/* usegs@224 */)+col/* col@228 *//* d@234 *//* d@234 */;
        return new Int4/* Int4@60 */(a/* a@231 */, b/* b@232 */, c/* c@233 */, d/* d@234 */);
        
      }
      );
    
  }
  public static Vector3/* Vector3@26 */ UvToSphere(Vector2/* Vector2@10 */uv/* uv@235 */, Single/* unresolved */radius/* radius@236 */)
  {
    return new Vector3/* Vector3@26 */(-radius/* radius@236 */*Cos/* Cos@101 */()*Sin/* Sin@102 */(), radius/* radius@236 */*Cos/* Cos@101 */(), radius/* radius@236 */*Cos/* Cos@101 */()*Sin/* Sin@102 */());
    
  }
  public static Vector3/* Vector3@26 */ UvToTorus(Vector2/* Vector2@10 */uv/* uv@237 */, Single/* unresolved */radius/* radius@238 */, Single/* unresolved */tube/* tube@239 */)
  {
    uv/* uv@237 */ = uv/* uv@237 */*MathF/* unresolved */.PI/* unresolved */*2;
    return new Vector3/* Vector3@26 */((radius/* radius@238 */+tube/* tube@239 */*Cos/* Cos@101 */())*Cos/* Cos@101 */(), (radius/* radius@238 */+tube/* tube@239 */*Cos/* Cos@101 */())*Sin/* Sin@102 */(), tube/* tube@239 */*Sin/* Sin@102 */());
    
  }
  public static Void/* unresolved */ TestOperator()
  {
    Vector3/* Vector3@26 */ x  = new Vector3/* Vector3@26 */(1, 2, 3)/* x@240 *//* x@240 */;
    Vector3/* Vector3@26 */ y  = x/* x@240 */+x/* x@240 *//* y@241 *//* y@241 */;
    
  }
  public static QuadMesh/* QuadMesh@83 */ Torus(Int32/* unresolved */rows/* rows@242 */, Int32/* unresolved */cols/* cols@243 */, Single/* unresolved */radius/* radius@244 */, Single/* unresolved */tube/* tube@245 */)
  {
    return ToQuadMesh/* ToQuadMesh@106 */(/* Captured: Single/* unresolved */radius/* radius@244 */Single/* unresolved */tube/* tube@245 */*/(uv/* uv@246 */)
     => {
        return UvToTorus/* UvToTorus@113 */(uv/* uv@246 */, radius/* radius@244 */, tube/* tube@245 */);
        
      }
      , rows/* rows@242 */, cols/* cols@243 */);
    
  }
  public static /* unresolved */ ToIntArray(IArray<Int3/* Int3@48 */>/* IArray@0 */faces/* faces@247 */)
  {
    return ToArray/* ToArray@90 */();
    
  }
  }
//==end==//
