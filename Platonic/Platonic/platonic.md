What defines Platonic code:

     * Safe
     * Strictly controlled side-effects 

What are the rules:

Main Rules

Regarding the Language

    * Platonic C# expects: 
        * .NET Standard 2.0
        * C# version 7.3 

Regarding Members

    * Methods either:
        * have side-effects ([Effects])
        * or don't 
    * A method with side-effects
        * Will have at least one parameter that is marked [Mutable]
    * Method either
        * Only read members ([ReadOnly])
        * Write to members ([Mutating])
    * Methods that have side-effect 
        * Can't also be mutating. 
        * In other words [Effects] functions are implicitly [ReadOnly]
        * May or may not be static 
    * No fields, only properties
    * Properties are either:
        * auto-backed with public getter
        * auto-backed with public getter, and private setter
        * computed
    * Static properties never have a setter

Regarding External Libraries

    * All external library calls consist of:
        * non-static methods that are considered [Mutating]
        * invoked on objects that are considered [Mutable]   
    
Regarding Types

    * Types either:
        * Can be modified after construction ([Mutable])
        * Can't be modified after construction ([Immutable])
    * Mutable types will have at least one setter    
    * Static types are 
        * Always [Immutable] because static setters (and fields are illegal)
    * Regardless of whether a type is Mutable or Immutable, 
        * It may have functions with side-effects
    * No structs, only classes
        * NOTE: this rule may be relaxed in the future 
    * Immutable classes 
        * may only implement Immutable interfaces 
        * may only inherit from Immutable classes
        * may not reference a mutable class or interface
    * Immutable interfaces
        * may only inherit from other immutable interfaces 
    * Mutable interfaces
        * may inherit from immutable interfaces
    * Mutable classes 
        * may implement mutable or immutable interfaces
        * may inherit from mutable or immutable classes 

Regarding Object Instances

    * Mutable type references        
        * may not be copied (can only be assigned to one variable)
        * may not be captured in a lambda 
       
Additional Rules

    * No unsafe code
    * No async / await 
    * No generators 
    * No protected members 
    * All methods are public 

Not yet supported:

    * Nested types

Rewriting:

    * Methods with side-effects are rewritten so that any parameters passed
        that are "effects"
    
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

 Todo:

* Output the classes
* Make comments about whether the fields are all readonly. 
* Convert fields to properties. If the field is read-only, there is no "setter".
* Check the list of used types. 
* Create a white-list of types. 
* Look at all of the "field assignment" operations 
* Why is Symbol outputting such strange things. 
* Convert structs to classes 
* Make sure that none of the properties have a public setter. 
* Would be nice to make the constructor explicit, maybe? 
* We need to know whether a type is immutable (the default) or not. 
    * Let's output a [ReadOnly] attribute when we know a type is immutable.
    * If a type can be changed then we should mark it as [Mutable]. 
    * For functions they are either, [Pure] => no side effects, [Mutable]
* For the list of [Mutable] types. 
* To leverage the compiler I should hav an output list of Platonic classes and functions. 
* void functions are impure.

//==

