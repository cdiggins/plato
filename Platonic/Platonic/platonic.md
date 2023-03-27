What defines Platonic code:

     * Safe
     * Strictly controlled side-effects 

Converting into Platonic code

    * Remove setters and initers 
    * No unsafe code
    * No external libraries 
    * Struct => Class
    * Fields => properties 
    * No Records
    * IReadOnlyList => IArray
    * IEnumerable => ISequence
    * ForEach => ISequenceIterator (a mutable pattern)
    * No async / await
    * No yields

//== 

    * Mutable classes
    * used in one place
    * not captured in a lambda
    
I need to:

    * Copy everything into new projects.
    * I may need to make small changes to make stuff work.
    * This won't work unless I get the source for a project.
 
I will need:

 * Streams

 Casting of arrays.

 * I need a safe API to cast an array from one type into another.
 * for now, it will be a big stupid copy.

 Now:

 * What types are used
 * What functions are used
 * Can I remove things safely? 
 * Extension methods 

 Ideas:

 * Maybe I can just remove and fix the Math libraries issue. 

 The BFAST code turned into a pile of shit. 

 Let's just simplify it. 

 In a way, this all might be easier to do manually. 

