// Contract stub (reference contract, "Reader data v2" and "Playground"). Chunk K replaces the
// body; the export name and props stay as written here.
//
// Props:
//   examples  — the `groups` object of examples.json, or null while it loads
//   group     — the selected group name from the route, or null for the group list
//   payload   — the opaque text after `~` in the route, or null
//   navigate  — function(routeString) that moves to a reader hash route
export default function Playground({ examples, group, payload, navigate }) {
  void payload; void navigate;
  if (!examples) return <p>Loading examples…</p>;
  return (
    <section className="playground">
      <h1>Playground</h1>
      <p>Examples: {Object.keys(examples).length}. Selected: {group ?? 'none'}.</p>
    </section>
  );
}
