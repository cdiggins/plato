import Init

/- A bounded teaching model; no claim of compiler refinement. -/
namespace PlatoCore

inductive Ty where
  | integer | boolean
  deriving DecidableEq, Repr

inductive Expr where
  | integer (n : Int)
  | boolean (b : Bool)
  | add (a b : Expr)
  | cond (c yes no : Expr)
  | fault (t : Ty)
  deriving DecidableEq, Repr

inductive HasType : Expr → Ty → Prop where
  | integer : HasType (.integer n) .integer
  | boolean : HasType (.boolean b) .boolean
  | add : HasType a .integer → HasType b .integer → HasType (.add a b) .integer
  | cond : HasType c .boolean → HasType a t → HasType b t → HasType (.cond c a b) t
  | fault : HasType (.fault t) t

def infer : Expr → Option Ty
  | .integer _ => some .integer
  | .boolean _ => some .boolean
  | .fault t => some t
  | .add a b => if infer a == some .integer && infer b == some .integer then some .integer else none
  | .cond c a b => if infer c == some .boolean && infer a == infer b then infer a else none

theorem infer_complete (h : HasType e t) : infer e = some t := by
  induction h with
  | integer => rfl
  | boolean => rfl
  | fault => rfl
  | add _ _ ha hb => simp [infer, ha, hb]
  | cond _ _ _ hc ha hb => simp [infer, hc, ha, hb]

theorem infer_sound : ∀ e t, infer e = some t → HasType e t := by
  intro e
  induction e with
  | integer n => intro t h; cases h; exact .integer
  | boolean b => intro t h; cases h; exact .boolean
  | fault s => intro t h; cases h; exact .fault
  | add a b ha hb =>
    intro t h
    simp only [infer] at h
    split at h
    next ok =>
      have hs : infer a = some Ty.integer ∧ infer b = some Ty.integer := by simpa using ok
      cases h
      exact .add (ha _ hs.1) (hb _ hs.2)
    next => contradiction
  | cond c a b hc ha hb =>
    intro t h
    simp only [infer] at h
    split at h
    next ok =>
      have hs : infer c = some Ty.boolean ∧ infer a = infer b := by simpa using ok
      exact .cond (hc _ hs.1) (ha _ h) (hb _ (hs.2.symm.trans h))
    next => contradiction

theorem infer_iff : infer e = some t ↔ HasType e t :=
  ⟨infer_sound e t, infer_complete⟩

theorem type_unique (a : HasType e t) (b : HasType e u) : t = u := by
  have h := (infer_complete a).symm.trans (infer_complete b)
  exact Option.some.inj h

inductive Value where
  | integer (n : Int)
  | boolean (b : Bool)
  deriving DecidableEq, Repr

def valueType : Value → Ty
  | .integer _ => .integer
  | .boolean _ => .boolean

inductive Outcome where
  | value (v : Value)
  | fault
  | typeError
  deriving DecidableEq, Repr

/- Left-to-right and short-circuiting: unselected branches are not evaluated. -/
def eval : Expr → Outcome
  | .integer n => .value (.integer n)
  | .boolean b => .value (.boolean b)
  | .fault _ => .fault
  | .add a b => match eval a with
    | .fault => .fault
    | .value (.integer x) => match eval b with
      | .fault => .fault
      | .value (.integer y) => .value (.integer (x + y))
      | _ => .typeError
    | _ => .typeError
  | .cond c a b => match eval c with
    | .fault => .fault
    | .value (.boolean true) => eval a
    | .value (.boolean false) => eval b
    | _ => .typeError

/- Fault is a modeled runtime failure, not a type error. -/
def Safe (t : Ty) : Outcome → Prop
  | .value v => valueType v = t
  | .fault => True
  | .typeError => False

theorem type_safety (h : HasType e t) : Safe t (eval e) := by
  induction h with
  | integer => rfl
  | boolean => rfl
  | fault => trivial
  | @add a b _ _ ha hb =>
    cases ea : eval a <;> simp_all [eval, Safe, valueType]
    case value v =>
      cases v <;> simp_all [valueType]
      cases eb : eval b <;> simp_all [Safe, valueType]
      case value v => cases v <;> simp_all [valueType]
  | @cond c a t b _ _ _ hc ha hb =>
    cases ec : eval c <;> simp_all [eval, Safe, valueType]
    case value v =>
      cases v <;> simp_all [valueType]
      rename_i b
      cases b <;> simp_all

/- Big-step evaluation is the graph of the terminating evaluator. -/
def Evaluates (e : Expr) (r : Outcome) : Prop := eval e = r

theorem deterministic (h : Evaluates e a) (k : Evaluates e b) : a = b :=
  h.symm.trans k

theorem typed_result (h : HasType e t) :
    eval e = .fault ∨ ∃ v, eval e = .value v ∧ valueType v = t := by
  have hs := type_safety h
  cases he : eval e with
  | fault => exact Or.inl rfl
  | typeError => simp [he, Safe] at hs
  | value v => exact Or.inr ⟨v, rfl, by simpa [he, Safe] using hs⟩

/- Source conjunction desugars to a conditional. -/
def andThen (a b : Expr) : Expr := .cond a b (.boolean false)

theorem and_typed (a : HasType e .boolean) (b : HasType f .boolean) :
    HasType (andThen e f) .boolean := .cond a b .boolean

theorem false_and (e : Expr) : eval (andThen (.boolean false) e) = .value (.boolean false) := rfl

theorem true_and (e : Expr) : eval (andThen (.boolean true) e) = eval e := rfl

theorem constant_cond_true (a b : Expr) : eval (.cond (.boolean true) a b) = eval a := rfl

theorem constant_cond_false (a b : Expr) : eval (.cond (.boolean false) a b) = eval b := rfl

/- Recursive optimizer folds constant guards but preserves every other evaluation site. -/
def fold : Expr → Expr
  | .add a b => .add (fold a) (fold b)
  | .cond c a b => match fold c with
    | .boolean true => fold a
    | .boolean false => fold b
    | c' => .cond c' (fold a) (fold b)
  | e => e

theorem fold_correct (e : Expr) : eval (fold e) = eval e := by
  induction e with
  | integer => rfl
  | boolean => rfl
  | fault => rfl
  | add a b ha hb => simp [fold, eval, ha, hb]
  | cond c a b hc ha hb =>
    simp only [fold]
    cases he : fold c <;> simp_all [eval]
    case boolean v => cases v <;> simp_all [eval] <;> rw [← hc]

/- An eager Boolean conjunction differs in the presence of faults. -/
def strictAnd (a b : Expr) : Outcome :=
  match eval a with
  | .fault => .fault
  | .value (.boolean x) => match eval b with
    | .fault => .fault
    | .value (.boolean y) => .value (.boolean (x && y))
    | _ => .typeError
  | _ => .typeError

theorem eager_and_counterexample :
    strictAnd (.boolean false) (.fault .boolean) ≠
      eval (andThen (.boolean false) (.fault .boolean)) := by decide

end PlatoCore
