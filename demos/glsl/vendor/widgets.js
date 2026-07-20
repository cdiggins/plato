import {
  addOn,
  calpha,
  Drag1D,
  Label,
  part,
  Press,
  rect,
  Row,
  Stack,
  surface,
  v,
  withExt
} from "gratify";
const Button = part()("button", {
  size: (props, m) => v(m.text(props.label).x + 28, 32),
  style: (t, ch, props) => ({
    ...surface(t, ch, { tint: props.danger ? t.danger : props.accent ? t.accent : void 0 }),
    corner: 8,
    lift: 2 * ch.hover - 2 * ch.press
  }),
  render: (node, p, s) => {
    const r = node.rect.raise(s.lift);
    p.box(r, s.corner, s.fill, s.edge, 1);
    p.label(node.props.label, r.center, s.text, { weight: 500 });
  },
  on: [Press((node) => node.props.press)]
});
const Checkbox = part()("checkbox", {
  size: (props, m) => props.label ? v(28 + m.text(props.label, 12).x, 20) : v(20, 20),
  channels: {
    on: { target: (n) => n.props.on ? 1 : 0, spring: { stiffness: 340, damping: 22 } }
  },
  style: (t, ch) => {
    const on = Math.min(1, Math.max(0, ch.on));
    return {
      fill: t.mix(t.surface, t.accent, on * 0.9),
      edge: t.mix(t.muted, t.accent, Math.max(on, ch.hover * 0.6)),
      mark: calpha(t.textBright, on),
      text: t.mix(t.mix(t.text, t.textBright, 0.3 * ch.hover), t.textDim, on),
      pop: ch.on
    };
  },
  render(node, p, s) {
    const r = node.rect;
    const box = rect(r.x + 1, r.y + 1, 18, 18);
    p.box(box, 6, s.fill, s.edge, 1.5);
    const c = box.center, k = Math.min(1.15, Math.max(0, s.pop));
    if (k > 0.02) {
      p.line(v(c.x - 4 * k, c.y), v(c.x - 1 * k, c.y + 3 * k), s.mark, 2);
      p.line(v(c.x - 1 * k, c.y + 3 * k), v(c.x + 4.5 * k, c.y - 3.5 * k), s.mark, 2);
    }
    if (node.props.label) p.label(node.props.label, v(r.x + 28, r.center.y), s.text, { align: "left", size: 12 });
  },
  on: [Press((node) => node.props.toggle)]
});
const Toggle = part()("toggle", {
  size: () => v(42, 24),
  channels: {
    on: { target: (n) => n.props.on ? 1 : 0, spring: { stiffness: 260, damping: 20 } }
  },
  style: (t, ch) => ({
    track: t.mix(t.muted, t.accent, Math.min(1, Math.max(0, ch.on))),
    knob: t.textBright,
    travel: ch.on,
    // a spring, so the knob *thunks*
    glow: 8 * ch.hover
  }),
  render(node, p, s) {
    const r = node.rect;
    p.box(r, r.h / 2, s.track);
    const knobX = r.x + 12 + s.travel * (r.w - 24);
    p.glow(s.track, s.glow, () => p.dot(v(knobX, r.center.y), 8, s.knob));
  },
  on: [Press((node) => node.props.flip)]
});
const Slider = part()("slider", {
  size: (props) => v(props.width ?? 170, 30),
  channels: {
    shown: { target: (n) => n.props.value, spring: { stiffness: 300, damping: 24 } }
  },
  style: (t, ch) => ({
    track: t.muted,
    fill: t.accent,
    knob: t.mix(t.textBright, t.accent, ch.hover * 0.3),
    knobR: 6.5 + 2 * ch.hover + 1 * ch.press,
    glow: 10 * ch.hover
  }),
  render(node, p, s) {
    const r = node.rect;
    const x = r.x + 8, w = r.w - 16, y = r.center.y;
    const t = Math.min(1, Math.max(0, node.ch.shown));
    p.box(rect(x, y - 2.5, w, 5), 2.5, s.track);
    p.box(rect(x, y - 2.5, w * t, 5), 2.5, s.fill);
    p.glow(s.fill, s.glow, () => p.dot(v(x + w * t, y), s.knobR, s.knob));
  },
  on: [Drag1D({ axis: "x", to: (node, f) => node.props.set(f) })]
});
const CloseButton = part()("close-button", {
  size: () => v(20, 20),
  style: (t, ch) => ({
    fill: calpha(t.danger, 0.12 + 0.5 * ch.hover),
    x: t.mix(t.textDim, t.danger, ch.hover),
    spin: ch.press
  }),
  render(node, p, s) {
    const r = node.rect.inset(1), c = r.center, k = 3.6 * (1 - 0.3 * s.spin);
    p.box(r, 6, s.fill);
    p.line(v(c.x - k, c.y - k), v(c.x + k, c.y + k), s.x, 1.8);
    p.line(v(c.x - k, c.y + k), v(c.x + k, c.y - k), s.x, 1.8);
  },
  on: [Press((node) => node.props.press)]
});
const Card = part()("card", {
  style: (t, ch) => ({
    fill: t.mix(t.surface, t.surfaceHi, 0.35 + 0.4 * ch.hover),
    edge: t.mix(t.muted, t.accent, 0.2 + 0.5 * ch.hover),
    corner: 10
  }),
  render: (node, p, s) => p.box(node.rect, s.corner, s.fill, s.edge, 1),
  body: (props, children) => [
    Stack("layout", { pad: 14, gap: 10, align: "stretch" }, [
      Row("head", { gap: 8, justify: "between" }, [
        Label("title", { text: props.title, weight: 600, size: 12 }),
        ...props.value ? [Label("value", { text: props.value, dim: true, size: 11 })] : []
      ]),
      ...children
    ])
  ]
});
const Labeled = (key, text, el, press) => Row(key, { gap: 8, align: "center" }, [
  press === void 0 ? Label(`${key}/l`, { text, dim: true, size: 11 }) : withExt(Label(`${key}/l`, { text, dim: true, size: 11 }), addOn(Press(() => press))),
  el
]);
export {
  Button,
  Card,
  Checkbox,
  CloseButton,
  Labeled,
  Slider,
  Toggle
};
