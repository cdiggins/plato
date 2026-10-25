"""Small conditional optimizer experiments around a complete recipe."""

FULL_FLAGS = ("--inline", "--fuse", "--optimize", "--loops", "--fuse-consumers", "--exact-output")
ARRAY_REASON = "--optimize-arrays targets helpers absent from the live wrapper runtime; excluded pending repair or retirement"


def validate_flags(flags):
    flags = list(flags)
    if len(flags) != len(set(flags)):
        raise ValueError("Duplicate compiler flag")
    if "--optimize-arrays" in flags:
        raise ValueError(ARRAY_REASON)
    if any(f not in FULL_FLAGS and f != "--static-abstract" and not f.startswith(("--inline-policy=", "--inline-growth=")) for f in flags):
        raise ValueError("Unsupported optimizer flag")
    if any(f in flags for f in ("--fuse-consumers", "--exact-output")) and "--loops" not in flags:
        raise ValueError("Consumer fusion and exact output require --loops")
    policies = [f for f in flags if f.startswith("--inline-policy=")]
    growth = [f for f in flags if f.startswith("--inline-growth=")]
    if (policies or growth) and "--inline" not in flags:
        raise ValueError("Inline parameters require --inline")
    if len(policies) > 1 or len(growth) > 1:
        raise ValueError("Inline parameters must each occur once")
    if policies and policies[0].split("=", 1)[1] not in ("statements", "lambdas", "iterative"):
        raise ValueError("Unknown inline policy")
    if growth and (not policies or not growth[0].split("=", 1)[1].isdigit() or int(growth[0].split("=", 1)[1]) <= 0):
        raise ValueError("Inline growth requires an experimental policy and a positive integer")
    return flags


def without_feature(flags, feature):
    removed = {feature}
    if feature == "--loops":
        removed.update(("--fuse-consumers", "--exact-output"))
    return [f for f in flags if f not in removed and not (
        feature == "--inline" and f.startswith(("--inline-policy=", "--inline-growth=")))]


def make_recipes(shipping_flags, *, experimental=False, inline_policies=(), growth_limits=()):
    """Return baseline, full, conditional ablations and optional one-factor sweeps."""
    inline_policies, growth_limits = list(inline_policies), list(growth_limits)
    def recipe(name, flags, kind, description, **extra):
        return dict(name=name, flags=validate_flags(flags), kind=kind, description=description, **extra)

    result = [recipe("shipping", shipping_flags, "reference", "Recipe read from the committed regeneration script"),
              recipe("full", FULL_FLAGS, "control", "Complete supported optimization recipe"),
              recipe("unoptimized", (), "reference", "All optional optimizer passes disabled")]
    for flag in FULL_FLAGS:
        removed = [f for f in FULL_FLAGS if f not in without_feature(FULL_FLAGS, flag)]
        result.append(recipe("no-" + flag[2:], without_feature(FULL_FLAGS, flag), "ablation",
                             "Remove " + ", ".join(removed) + " from full", removed_flags=removed))
    result.append(dict(name="optimize-arrays", flags=["--optimize-arrays"], kind="unsupported",
                       status="unsupported", description=ARRAY_REASON))
    if (inline_policies or growth_limits) and not experimental:
        raise ValueError("Inline sweeps require experimental=True")
    if experimental:
        if growth_limits and "statements" not in inline_policies:
            inline_policies.append("statements")
        for policy in inline_policies:
            result.append(recipe("inline-" + policy, [*FULL_FLAGS, "--inline-policy=" + policy],
                                 "parameter", "Change inline policy within full", experimental=True, reference="full"))
        # Growth has meaning only within an explicit policy. Hold that policy fixed.
        for growth in growth_limits:
            result.append(recipe("inline-statements-growth-" + str(growth),
                                 [*FULL_FLAGS, "--inline-policy=statements", "--inline-growth=" + str(growth)],
                                 "parameter", "Change growth within full and statement inlining", experimental=True,
                                 reference="inline-statements"))
    names = [r["name"] for r in result]
    if len(names) != len(set(names)):
        raise ValueError("Duplicate recipe name")
    return result
