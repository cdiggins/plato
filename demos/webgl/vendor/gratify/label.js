// ============================================================================
// Label — the built-in text part. Its color blending lives in a style facet
// (not render), so a theme extension can restyle text and the framework obeys
// its own rule: render reads only rect + the resolved style.
// ============================================================================
import { calpha, v } from "./core.js";
import { part } from "./part.js";
import { textTone } from "./style.js";
export const Label = part()("label", {
    size: (props, m) => {
        const s = m.text(props.text, props.size ?? 13);
        return v(s.x + 2, Math.max(s.y, 18));
    },
    // a `done` state dims and fades — the common list-item idiom
    style: (t, ch, props) => {
        const { text, fade } = textTone(t, { dim: props.dim, bright: props.bright, done: ch.done || 0 });
        return { color: calpha(text, fade) };
    },
    render: (node, p, s) => {
        const props = node.props;
        p.label(props.text, { x: node.rect.x + 1, y: node.rect.center.y }, s.color, { size: props.size ?? 13, weight: props.weight, align: "left" });
    },
});
//# sourceMappingURL=label.js.map