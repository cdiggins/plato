 // class   Any 
 // class   Func0 
 Any     Invoke () { 
 } 
 // class   Func1 
 Any     Invoke ( Any     x ) { 
 } 
 // class   Func2 
 Any     Invoke ( Any     x0 , Any     x1 ) { 
 } 
 // class   Func3 
 Any     Invoke ( Any     x0 , Any     x1 , Any     x2 ) { 
 } 
 // class   Func4 
 Any     Invoke ( Any     x0 , Any     x1 , Any     x2 , Any     x3 ) { 
 } 
 // class   Array 
 // class   Sequence 
 // class   SequenceIterator 
 // class   WhereIterator 
 // class   SelectIterator 
 // class   Counted 
 // class   CountedSequence 
 // class   Map 
 // class   Remap 
 // class   OtherExtensions 
 Sequence     Where ( Sequence     self , Func1     predicate ) { 
 return  self  .  WhereWithIndex ((    x ,    i ) =>  { 
 return  predicate ( x ) ; 
 } 
) ; 
 } 
 Sequence     WhereIndex ( Sequence     self , Func1     predicate ) { 
 return  self  .  WhereWithIndex ((    x ,    i ) =>  { 
 return  predicate ( i ) ; 
 } 
) ; 
 } 
 Sequence     Where ( Array     self , Array     mask ) { 
 return  self  .  WhereIndex ( mask  .  ToFunction ()) ; 
 } 
 Sequence     WhereWithIndex ( Sequence     self , Func2     predicate ) { 
 return  new  WhereIterator < T >( self  .  Iterator , predicate ) ; 
 } 
 Sequence     Select ( Sequence     self , Func1     f ) { 
 return  self  .  SelectWithIndex ((    x ,    i ) =>  { 
 return  f ( x ) ; 
 } 
) ; 
 } 
 Sequence     SelectIndex ( Sequence     self , Func1     f ) { 
 return  self  .  SelectWithIndex ((    x ,    i ) =>  { 
 return  f ( i ) ; 
 } 
) ; 
 } 
 Sequence     SelectWithIndex ( Sequence     self , Func2     f ) { 
 return  new  SelectIterator < T , U >( self  .  Iterator , f ) ; 
 } 
 Any     Aggregate ( Sequence     self , Any     init , Func3     func ) { 
 { 
 var     j  =  0  ; 
 { 
 var     i  =  self  .  Iterator  ; 
 while  (  i  .  HasValue  )  { 
 init  =  func ( init , i  .  Value , j ) ; 
 { 
 i  =  i  .  Next  ; 
 ++  j  ; 
 } 
 } 
 } 
 return  init  ; 
 } 
 } 
 Sequence     IndicesWhere ( CountedSequence     self , Func1     f ) { 
 return  self  .  Indices () .  Where ((    i ) =>  { 
 return  f ( self  [  i  ] ) ; 
 } 
) ; 
 } 
 Sequence     IndicesWhere ( CountedSequence     self , Func2     f ) { 
 return  self  .  IndicesWhere ((    i ) =>  { 
 return  f ( self  [  i  ] , i ) ; 
 } 
) ; 
 } 
 Sequence     IndicesWhere ( CountedSequence     self , Func1     f ) { 
 return  self  .  Indices () .  Where ((    i ) =>  { 
 return  f ( i ) ; 
 } 
) ; 
 } 
 Sequence     IndicesWhere ( CountedSequence     self , Array     mask ) { 
 return  self  .  IndicesWhere ( mask  .  ToFunction ()) ; 
 } 
 bool     All ( Sequence     self , Func1     predicate ) { 
 { 
 { 
 var     i  =  self  .  Iterator  ; 
 while  (  i  .  HasValue  )  { 
 if  (  !  predicate ( i  .  Value ) ) 
 return  false  ; 
 else  ; 
 i  =  i  .  Next  ; 
 } 
 } 
 return  true  ; 
 } 
 } 
 bool     All ( Sequence     self ) { 
 return  self  .  All ((    x ) =>  { 
 return  x  ; 
 } 
) ; 
 } 
 bool     Any ( Sequence     self ) { 
 return  self  .  Any ((    x ) =>  { 
 return  (  bool  )  x  ; 
 } 
) ; 
 } 
 bool     Any ( Sequence     self , Func1     predicate ) { 
 { 
 { 
 var     i  =  self  .  Iterator  ; 
 while  (  i  .  HasValue  )  { 
 if  (  predicate ( i  .  Value ) ) 
 return  false  ; 
 else  ; 
 i  =  i  .  Next  ; 
 } 
 } 
 return  true  ; 
 } 
 } 
 // class   MapExtensions 
 Func1     ToFunction ( Map     self ) { 
 return (    x ) =>  { 
 return  self  [  x  ]  ; 
 } 
 ; 
 } 
 Map     Select ( Map     self , Func1     f ) { 
 return  new  Remap ( self ,(    x ) =>  { 
 return  f ( self  [  x  ] ) ; 
 } 
) ; 
 } 
 // class   ArrayExtensions 
 Array < Any >    CopyTo ( Array     self ) { 
 return  self  .  CopyTo ( new  Any  [  self  .  Count  ] ) ; 
 } 
 Array < Any >    CopyTo ( Array     self , Array < Any >    xs ) { 
 return  self  .  CopyTo ( xs , 0 , self  .  Count ) ; 
 } 
 Array < Any >    CopyTo ( Array     self , Array < Any >    xs , int     indexFrom ) { 
 return  self  .  CopyTo ( xs , indexFrom , self  .  Count ) ; 
 } 
 Array < Any >    CopyTo ( Array     self , Array < Any >    xs , int     indexFrom , int     count ) { 
 { 
 { 
 var     i  =  indexFrom  ; 
 while  (  i  <  indexFrom  +  count  )  { 
 xs  [  i  ]  =  self  [  i  ]  ; 
 ++  i  ; 
 } 
 } 
 return  xs  ; 
 } 
 } 
 Array     Indices ( Counted     self ) { 
 return  self  .  Count  .  Range () ; 
 } 
 Array     Create ( Array < Any >    self ) { 
 return  ToArray ( self ) ; 
 } 
 Array     Select ( int     count , Func1     f ) { 
 return  FunctionalArray  .  Create ( count , f ) ; 
 } 
 Array     ToArray ( Array < Any >    self ) { 
 return  Select ( self  .  Length ,(    i ) =>  { 
 return  self  [  i  ]  ; 
 } 
) ; 
 } 
 Array     Concatenate ( Array     self , int     count ) { 
 return  Select ( count  *  self  .  Count ,(    i ) =>  { 
 return  self  [  i  %  self  .  Count  ]  ; 
 } 
) ; 
 } 
 Array     Generate ( Any     init , int     count , Func1     f ) { 
 { 
 var     r  =  new  Any  [  count  ]  ; 
 { 
 var     i  =  0  ; 
 while  (  i  <  count  )  { 
 { 
 r  [  i  ]  =  init  ; 
 init  =  f ( init ) ; 
 } 
 ++  i  ; 
 } 
 } 
 return  r  .  ToIArray () ; 
 } 
 } 
 Any     First ( Array     self ) { 
 return  self  .  IsEmpty () ?  @default  :  self  [  0  ]  ; 
 } 
 Any     Last ( Array     self , Any     @default  =  default ) { 
 return  self  .  IsEmpty () ?  @default  :  self  [  self  .  Count  -  1  ]  ; 
 } 
 bool     InRange ( Array     self , int     n ) { 
 return  n  >=  0  &&  n  <  self  .  Count  ; 
 } 
 bool     IsEmpty ( Array     self ) { 
 return  self  .  Count  >  0  ; 
 } 
 bool     Any ( Array     self ) { 
 return  self  .  Count  !=  0  ; 
 } 
 Array     Select ( Array     self , Func1     f ) { 
 return  Select ( self  .  Count ,(    i ) =>  { 
 return  f ( self  [  i  ] ) ; 
 } 
) ; 
 } 
 Array     SelectWithIndex ( Array     self , Func2     f ) { 
 return  Select ( self  .  Count ,(    i ) =>  { 
 return  f ( self  [  i  ] , i ) ; 
 } 
) ; 
 } 
 Array     SelectIndices ( Array     self , Func1     f ) { 
 return  self  .  Count  .  Select ( f ) ; 
 } 
 Array     Flatten ( Array     self , int     n ) { 
 return  Select ( self  .  Count  *  n ,(    i ) =>  { 
 return  self  [  i  /  n  ]  [  i  %  n  ]  ; 
 } 
) ; 
 } 
 Array     Flatten ( Array     self ) { 
 { 
 var     counts  =  self  .  Select ((    x ) =>  { 
 return  x  .  Count  ; 
 } 
) .  PostAccumulate ((    x ,    y ) =>  { 
 return  x  +  y  ; 
 } 
) ; 
 var     r  =  new  Any  [  counts  .  Last () ]  ; 
 { 
 var     i  =  0  ; 
 while  (  i  <  xs  .  Count  )  { 
 xs  .  CopyTo ( r , counts  [  i  i  ] ) ; 
 ++  i  ; 
 } 
 } 
 return  r  .  ToIArray () ; 
 } 
 } 
 Array     ZipWithIndex ( Array     self ) { 
 return  self  .  Select ((    v ,    i ) =>  { 
 return ( v , i ) ; 
 } 
) ; 
 } 
 Array     SelectMany ( Array     self , int     count ) { 
 return  Select ( self  .  Count ,(    i ) =>  { 
 return  self  [  i  /  count  ]  [  i  %  count  ]  ; 
 } 
) ; 
 } 
 Array     SelectMany ( Array     self , Func1     func ) { 
 { 
 var     count  =  self  .  Sum ((    x ) =>  { 
 return  func ( x ) .  Count  ; 
 } 
) ; 
 var     xs  =  new  Any  [  count  ]  ; 
 var     offset  =  0  ; 
 { 
 var     i  =  0  ; 
 while  (  i  <  self  .  Count  )  { 
 { 
 var     sub  =  func ( self  [  i  ] ) ; 
 sub  .  CopyTo ( xs , offset ) ; 
 offset  +=  sub  .  Count  ; 
 } 
 ++  i  ; 
 } 
 } 
 return  xs  .  ToIArray () ; 
 } 
 } 
 Array     SelectMany ( Array     self , Func2     func ) { 
 { 
 var     count  =  self  .  Aggregate ( 0 ,(    acc ,    x ,    index ) =>  { 
 return  acc  +  func ( x , index ) .  Count  ; 
 } 
) ; 
 var     xs  =  new  Any  [  count  ]  ; 
 var     offset  =  0  ; 
 { 
 var     i  =  0  ; 
 while  (  i  <  self  .  Count  )  { 
 { 
 var     sub  =  func ( self  [  i  ] , i ) ; 
 sub  .  CopyTo ( xs , offset ) ; 
 offset  +=  sub  .  Count  ; 
 } 
 ++  i  ; 
 } 
 } 
 return  xs  .  ToIArray () ; 
 } 
 } 
 Array     SelectMany ( Array     self , Func2     func ) { 
 { 
 var     r  =  new  Any  [  self  .  Count  *  2  ]  ; 
 { 
 var     i  =  0  ; 
 while  (  i  <  self  .  Count  )  { 
 { 
 var     tmp  =  func ( self  [  i  ] ) ; 
 r  [  i  *  2  ]  =  tmp  .  Item1  ; 
 r  [  i  *  2  +  1  ]  =  tmp  .  Item2  ; 
 } 
 ++  i  ; 
 } 
 } 
 return  r  .  ToIArray () ; 
 } 
 } 
 Array     Zip ( Array     self , Array     other , Func2     f ) { 
 return  Select ( Math  .  Min ( self  .  Count , other  .  Count ),(    i ) =>  { 
 return  f ( self  [  i  ] , other  [  i  ] ) ; 
 } 
) ; 
 } 
 Array     ZipEachWithNext ( Array     self , Func2     f ) { 
 return  self  .  Zip ( self  .  Skip (), f ) ; 
 } 
 Array     Slice ( Array     self , int     from , int     to ) { 
 return  Select ( to  -  from ,(    i ) =>  { 
 return  self  [  i  +  from  ]  ; 
 } 
) ; 
 } 
 Array     SubArraysFixed ( Array     self , int     size ) { 
 return  (  self  .  Count  /  size  )  .  Select ((    i ) =>  { 
 return  self  .  SubArray ( i , size ) ; 
 } 
) ; 
 } 
 Array     SubArrays ( Array     self , int     size ) { 
 return  self  .  Count  %  size  ==  0  ?  self  .  SubArraysFixed ( size ) :  self  .  SubArraysFixed ( size ) .  Append ( self  .  TakeLast ( self  .  Count  %  size )) ; 
 } 
 Array     SubArray ( Array     self , int     from , int     count ) { 
 return  self  .  Slice ( from , count  +  from ) ; 
 } 
 Array     Slice ( Array     self , int     from , int     to , int     stride ) { 
 return  Select ( to  -  from  /  stride ,(    i ) =>  { 
 return  self  [  i  *  stride  +  from  ]  ; 
 } 
) ; 
 } 
 Array     Stride ( Array     self , int     n ) { 
 return  Select ( self  .  Count  /  n ,(    i ) =>  { 
 return  self  [  i  *  n  %  self  .  Count  ]  ; 
 } 
) ; 
 } 
 Array     Take ( Array     self , int     n ) { 
 return  self  .  Slice ( 0 , n ) ; 
 } 
 Array     TakeAtMost ( Array     self , int     n ) { 
 return  self  .  Count  >  n  ?  self  .  Slice ( 0 , n ) :  self  ; 
 } 
 Array     Skip ( Array     self , int     n  =  1 ) { 
 return  self  .  Slice ( n , self  .  Count ) ; 
 } 
 Array     TakeLast ( Array     self , int     n  =  1 ) { 
 return  self  .  Skip ( self  .  Count  -  n ) ; 
 } 
 Array     DropLast ( Array     self , int     n  =  1 ) { 
 return  self  .  Count  >  n  ?  self  .  Take ( self  .  Count  -  n ) :  self  .  Empty () ; 
 } 
 Array     MapIndices ( Array     self , Func1     f ) { 
 return  self  .  Count  .  Select ((    i ) =>  { 
 return  self  [  f ( i ) ]  ; 
 } 
) ; 
 } 
 Array     Reverse ( Array     self ) { 
 return  self  .  MapIndices ((    i ) =>  { 
 return  self  .  Count  -  1  -  i  ; 
 } 
) ; 
 } 
 Array     SelectByIndex ( Array     self , Array     indices ) { 
 return  indices  .  Select ((    i ) =>  { 
 return  self  [  i  ]  ; 
 } 
) ; 
 } 
 Array     Choose ( Array     indices , Array     values ) { 
 return  values  .  SelectByIndex ( indices ) ; 
 } 
 Array     Resize ( Array     self , int     count ) { 
 return  Select ( count ,(    i ) =>  { 
 return  self  [  i  %  self  .  Count  ]  ; 
 } 
) ; 
 } 
 Array     Empty ( Array     self ) { 
 return  self  .  Take ( 0 ) ; 
 } 
 string     Join ( Array     self , string     sep  =  " " ) { 
 return  self  .  Aggregate ( new  StringBuilder (),(    sb ,    x ) =>  { 
 return  sb  .  Append ( x ) .  Append ( sep ) ; 
 } 
) .  ToString () ; 
 } 
 Array     Concatenate ( Array     self , Array     other ) { 
 return  Select ( self  .  Count  +  other  .  Count ,(    i ) =>  { 
 return  i  <  self  .  Count  ?  self  [  i  ]  :  other  [  i  -  self  .  Count  ]  ; 
 } 
) ; 
 } 
 Array     AdjacentDifferences ( Array     self ) { 
 return  self  .  ZipEachWithNext ((    a ,    b ) =>  { 
 return  b  -  a  ; 
 } 
) ; 
 } 
 Array     Append ( Array     self , Any     x ) { 
 return  (  self  .  Count  +  1  )  .  Select ((    i ) =>  { 
 return  i  <  self  .  Count  ?  self  [  i  ]  :  x  ; 
 } 
) ; 
 } 
 Array     Append ( Array     self , Array < Any >    x ) { 
 return  self  .  Concatenate ( x  .  ToIArray ()) ; 
 } 
 Array     Prepend ( Array     self , Any     x ) { 
 return  (  self  .  Count  +  1  )  .  Select ((    i ) =>  { 
 return  i  ==  0  ?  x  :  self  [  i  -  1  ]  ; 
 } 
) ; 
 } 
 Any     ElementAt ( Array     self , int     n ) { 
 return  self  [  n  ]  ; 
 } 
 Any     ElementAtModulo ( Array     self , int     n ) { 
 return  self  .  ElementAt ( n  %  self  .  Count ) ; 
 } 
 Any     ElementAtOrDefault ( Array     xs , int     n , Any     defaultValue  =  default ) { 
 return  xs  !=  null  &&  n  >=  0  &&  n  <  xs  .  Count  ?  xs  [  n  ]  :  defaultValue  ; 
 } 
 int     CountWhere ( Array     self , Func1     p ) { 
 return  self  .  Aggregate ( 0 ,(    n ,    x ) =>  { 
 return  n  +  (  p ( x ) ?  1  :  0  )  ; 
 } 
) ; 
 } 
 int     CountWhere ( Array     self ) { 
 return  self  .  CountWhere ((    x ) =>  { 
 return  x  ; 
 } 
) ; 
 } 
 Array     Accumulate ( Array     self , Func2     f ) { 
 { 
 var     n  =  self  .  Count  ; 
 if  (  n  ==  0  ) 
 return  self  ; 
 else  ; 
 var     r  =  Any  [  n  ]  ; 
 var     prev  =  r  [  0  ]  =  self  [  0  ]  ; 
 { 
 var     i  =  1  ; 
 while  (  i  <  n  )  { 
 { 
 prev  =  r  [  i  ]  =  f ( prev , self  [  i  ] ) ; 
 } 
 ++  i  ; 
 } 
 } 
 return  r  .  ToIArray () ; 
 } 
 } 
 Array     Split ( Array     self , Array     indices ) { 
 return  indices  .  Prepend ( 0 ) .  Zip ( indices  .  Append ( self  .  Count ),( int     x , int     y ) =>  { 
 return  self  .  Slice ( x , y ) ; 
 } 
) ; 
 } 
 Array     Split ( Array     self , int     index ) { 
 return  Create ( self  .  Take ( index ), self  .  Skip ( index )) ; 
 } 
 ValueTuple < Array , Array >    Unzip ( Array     self ) { 
 return ( self  .  Select ((    pair ) =>  { 
 return  pair  .  Item1  ; 
 } 
), self  .  Select ((    pair ) =>  { 
 return  pair  .  Item2  ; 
 } 
)) ; 
 } 
 long     Sum ( Array     self , Func1     func ) { 
 return  self  .  Aggregate ( 0L ,(    init ,    x ) =>  { 
 return  init  +  func ( x ) ; 
 } 
) ; 
 } 
 double     Sum ( Array     self , Func1     func ) { 
 return  self  .  Aggregate ( 0.0 ,(    init ,    x ) =>  { 
 return  init  +  func ( x ) ; 
 } 
) ; 
 } 
 Array     SelectPairs ( Array     xs , Func2     f ) { 
 return  (  xs  .  Count  /  2  )  .  Select ((    i ) =>  { 
 return  f ( xs  [  i  *  2  ] , xs  [  i  *  2  +  1  ] ) ; 
 } 
) ; 
 } 
 Array     SelectTriplets ( Array     xs , Func3     f ) { 
 return  (  xs  .  Count  /  3  )  .  Select ((    i ) =>  { 
 return  f ( xs  [  i  *  3  ] , xs  [  i  *  3  +  1  ] , xs  [  i  *  3  +  2  ] ) ; 
 } 
) ; 
 } 
 Array     SelectQuartets ( Array     xs , Func4     f ) { 
 return  (  xs  .  Count  /  4  )  .  Select ((    i ) =>  { 
 return  f ( xs  [  i  *  4  ] , xs  [  i  *  4  +  1  ] , xs  [  i  *  4  +  2  ] , xs  [  i  *  4  +  3  ] ) ; 
 } 
) ; 
 } 
 Array     Cast ( Array     xs ) { 
 return  xs  .  Select ((    x ) =>  { 
 return  (  Any  )  x  ; 
 } 
) ; 
 } 
 bool     Contains ( Array     xs , Any     value ) { 
 return  xs  .  Any ((    x ) =>  { 
 return  value  .  Equals ( x ) ; 
 } 
) ; 
 } 
 Any     FirstOrDefault ( Array     xs ) { 
 return  xs  .  FirstOrDefault ( xs  .  DefaultElement ) ; 
 } 
 Any     FirstOrDefault ( Array     xs , Any     @default ) { 
 return  xs  .  Count  >  0  ?  xs  [  0  ]  :  @default  ; 
 } 
 Any     FirstOrDefault ( Array     xs , Func1     predicate ) { 
 return  xs  .  Where ( predicate ) .  FirstOrDefault () ; 
 } 
 Array     ToLongs ( Array     xs ) { 
 return  xs  .  Select ((    x ) =>  { 
 return  (  long  )  x  ; 
 } 
) ; 
 } 
 Array     PrefixSums ( Array     self ) { 
 return  self  .  ToLongs () .  PrefixSums () ; 
 } 
 Array     PrefixSums ( Array     self ) { 
 return  self  .  Scan ( 0f ,(    a ,    b ) =>  { 
 return  a  +  b  ; 
 } 
) ; 
 } 
 Array     PrefixSums ( Array     self ) { 
 return  self  .  Scan ( 0.0 ,(    a ,    b ) =>  { 
 return  a  +  b  ; 
 } 
) ; 
 } 
 Array     Scan ( Array     self , Any     init , Func2     scanFunc ) { 
 { 
 if  (  self  .  Count  ==  0  ) 
 return  Empty<Any> () ; 
 else  ; 
 var     r  =  new  Any  [  self  .  Count  ]  ; 
 { 
 var     i  =  0  ; 
 while  (  i  <  self  .  Count  )  { 
 init  =  r  [  i  ]  =  scanFunc ( init , self  [  i  ] ) ; 
 ++  i  ; 
 } 
 } 
 return  r  .  ToIArray () ; 
 } 
 } 
 Array     PrefixSums ( Array     counts ) { 
 return  counts  .  Scan ( 0L ,(    a ,    b ) =>  { 
 return  a  +  b  ; 
 } 
) ; 
 } 
 Array     CountsToOffsets ( Array     counts ) { 
 { 
 var     r  =  new  int  [  counts  .  Count  ]  ; 
 { 
 var     i  =  1  ; 
 while  (  i  <  counts  .  Count  )  { 
 r  [  i  ]  =  r  [  i  -  1  ]  +  counts  [  i  -  1  ]  ; 
 ++  i  ; 
 } 
 } 
 return  r  .  ToIArray () ; 
 } 
 } 
 Array     OffsetsToCounts ( Array     offsets , int     last ) { 
 return  offsets  .  Indices () .  Select ((    i ) =>  { 
 return  i  <  offsets  .  Count  -  1  ?  offsets  [  i  +  1  ]  -  offsets  [  i  ]  :  last  -  offsets  [  i  ]  ; 
 } 
) ; 
 } 
 Array     SetElementAt ( Array     self , int     index , Any     value ) { 
 return  self  .  SelectIndices ((    i ) =>  { 
 return  i  ==  index  ?  value  :  self  [  i  ]  ; 
 } 
) ; 
 } 
 Array     SetFirstElementWhere ( Array     self , Func1     predicate , Any     value ) { 
 { 
 var     index  =  self  .  IndexOf ( predicate ) ; 
 if  (  index  <  0  ) 
 return  self  ; 
 else  ; 
 return  self  .  SetElementAt ( index , value ) ; 
 } 
 } 
