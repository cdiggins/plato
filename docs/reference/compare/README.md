# If you already know another language

Two pages map constructs you already know onto Plato. Each is a set of tables: the
familiar construct, the Plato form, and a short note on what differs. Every Plato cell
links the page that owns the rule, so a table is a map, not a second specification.

- [If you know Python](python.md)
- [If you know C#](csharp.md)

Both pages say plainly where Plato has no equivalent. Plato leaves out much that
both languages take for granted: mutable objects, exceptions, null, inheritance,
imports. It also adds things neither language has: capabilities bound by interface,
and laws written as ordinary functions. The language
[model](../language/model.md) and [SEMANTICS](../../SEMANTICS.md) are the authority when
a table cell and a rule seem to disagree.

A note for both audiences: the parser accepts more C#-shaped syntax than the language
supports. A form that parses can still be rejected later, so a familiar spelling is not
evidence. See [diagnostics and unsupported forms](../language/diagnostics.md).

Inline code in the tables is Plato source unless a column says otherwise. These pages
record no counts or measurements; the [reference home](../README.md) says where those live.
