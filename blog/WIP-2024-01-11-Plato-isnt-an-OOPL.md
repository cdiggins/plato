# Why Plato isn't an Object Oriented Language 

Musings on the Plato language design. 

## 2024-01-11

There are several properties that we associate with programming language 

* Abstraction
* Polymorphism
* Encapsulation
* Information Hiding
* Composition
* Inheritance

The object oriented paradigm is useful in some contexts. 

Putting state in an instance of a class.

For example if I want to design a simple simulation, it can be much easier to reason about 
if the state transition rules are kept internal to objects. 

However, it turns out to be a rather poor method of modeling everything. 

# Information Hiding 

Information hiding refers to visibility specification feature (e.g., `public` and `private` keywords) 
of many object-oriented programming languages.

Many of us were taught that the hiding fields is a good thing. 

This is true when the inner workings of a class are complex, and the object can undergo state transformations.   

However, it can get quite silly, when a class has public getters and setters for every field. 
You haven't done anything except make your code longer. 

It becomes more useful, if you omit setters. However, if you don't have mutable fields, why not let people see it? 

If it is useful, expose it. 

It turns out that when we program in an immutable style, all of the interesting work occurs within the constructor anyway. 

Everything else is just in support of providing useful views of the constructed data. 

So in Plato we don't have `public`, `private`, `protected`, `internal` or anything else like that. 
And we save ourselves a lecture in the computer science text book. 

# Polymorphism

In Plato, data types can be easily to/from each other especially when they have a similar shape. 

