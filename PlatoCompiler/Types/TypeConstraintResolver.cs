namespace Plato.Compiler.Types
{
    // Resolves constraints. 
    // Eliminates constraints and possibilities from the graph.
    // Most possibilities are tied to the fact that the precise function is not known. 
    // When a constraint conflict is detected the type constraint resolver has to eliminate
    // associated constraints from the graph, and the function from consideration.
    // So we need to know what constraints are associated with which function. .
    // We may need a way of describing a conflict. A graph leads to a set of constraints
    // and a set, a conflict means we have to back up through the graph and choose a different path. 
    // Ideally ... we arrive at only one possibility.
    // So once we have a "good" set (by walking through the graph), we have to then still visit other 
    // possible paths and eliminate them. 
    // There should be one clear path through the graph. 
    // Note that it could be possible that there are multiple functions that fit in the end still ... so 
    // we have to pick the best one. 
    // Once we have a good possibility, we can then eliminate bad possibilities (e.g., those that absolutely don't fit) 
    // The thing thatI really don't know how to do is to easily associate a graph position with a function choice.
    // Perhaps I don't need to worry about it so much ... but I assume that it will be done in the TypeConstraintCollector. 
    // The most important thing for now is gather constraints. 
    public class TypeConstraintResolver
    {

    }
}