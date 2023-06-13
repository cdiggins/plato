//!CheckboxWidget

import {WidgetType} from "@codemirror/view"

class CheckboxWidget extends WidgetType {
  constructor(checked) { 
    super() 
    this.checked = checked;
  }

  eq(other) { return other.checked == this.checked }

  toDOM() {
    let wrap = document.createElement("span")
    wrap.setAttribute("aria-hidden", "true")
    wrap.className = "cm-boolean-toggle"
    let box = wrap.appendChild(document.createElement("input"))
    box.type = "checkbox"
    box.checked = this.checked
    return wrap
  }

  ignoreEvent() { return false }
}

//!checkboxes

import {EditorView, Decoration} from "@codemirror/view"
import {syntaxTree} from "@codemirror/language"

function checkboxes(view) {
  let widgets = []
  for (let {from, to} of view.visibleRanges) {
    syntaxTree(view.state).iterate({
      from, to,
      enter: (node) => {
        if (node.name == "BooleanLiteral") {
          let isTrue = view.state.doc.sliceString(node.from, node.to) == "true"
          let deco = Decoration.widget({
            widget: new CheckboxWidget(isTrue),
            side: 1
          })
          widgets.push(deco.range(node.from))
        }
      }
    })
  }
  return Decoration.set(widgets)
}

//!toggleBoolean

function toggleBoolean(view, pos) {
  let before = view.state.doc.sliceString(Math.max(0, pos), pos + 5)
  let change
  if (before == "false")
    change = {from: pos, to: pos + 5, insert: "true"}
  else if (before.startsWith("true"))
    change = {from: pos, to: pos + 4, insert: "false"}
  else
    return false
  view.dispatch({changes: change})
  return true
}

//!checkboxPlugin

import {ViewUpdate, ViewPlugin} from "@codemirror/view"

const checkboxPlugin = ViewPlugin.fromClass(class {
  decorations

  constructor(view) {
    this.decorations = checkboxes(view)
  }

  update(update) {
    if (update.docChanged || update.viewportChanged)
      this.decorations = checkboxes(update.view)
  }
}, {
  decorations: v => v.decorations,

  eventHandlers: {
    mousedown: (e, view) => {
      let target = e.target 
      if (target.nodeName == "INPUT")
        toggleBoolean(view, view.posAtDOM(target))
      return false
    }
  }
})

//!create

import { lineNumbers, highlightActiveLineGutter, highlightSpecialChars, drawSelection, dropCursor, rectangularSelection, crosshairCursor, highlightActiveLine, keymap } from '@codemirror/view';
export { EditorView } from '@codemirror/view';
import { EditorState } from '@codemirror/state';
import { foldGutter, indentOnInput, syntaxHighlighting, defaultHighlightStyle, bracketMatching, foldKeymap } from '@codemirror/language';
import { history, defaultKeymap, historyKeymap } from '@codemirror/commands';
import { highlightSelectionMatches, searchKeymap } from '@codemirror/search';
import { closeBrackets, autocompletion, closeBracketsKeymap, completionKeymap } from '@codemirror/autocomplete';
import { lintKeymap } from '@codemirror/lint';

const basicSetup = [
    checkboxPlugin,
    lineNumbers(),
    highlightActiveLineGutter(),
    highlightSpecialChars(),
    history(),
    foldGutter(),
    drawSelection(),
    dropCursor(),
    EditorState.allowMultipleSelections.of(true),
    indentOnInput(),
    syntaxHighlighting(defaultHighlightStyle, { fallback: true }),
    bracketMatching(),
    closeBrackets(),
    autocompletion(),
    rectangularSelection(),
    crosshairCursor(),
    highlightActiveLine(),
    highlightSelectionMatches(),
    keymap.of([
        ...closeBracketsKeymap,
        ...defaultKeymap,
        ...searchKeymap,
        ...historyKeymap,
        ...foldKeymap,
        ...completionKeymap,
        ...lintKeymap
    ]),
    javascript()
];

import {javascript} from "@codemirror/lang-javascript"
let timer;

let editor = new EditorView({
    parent: document.querySelector("#editor"),
    state: EditorState.create({
        doc: "let value = true\nif (!value == false)\n  console.log(\"good\")\n",
        extensions: [...basicSetup, EditorView.updateListener.of((v) => {
          if (v.docChanged) {
            if (timer) clearTimeout(timer);
            timer = setTimeout(() => {
              console.log(v.state.doc.toString());
            }, 500);
          }
        })]
    }),
})

