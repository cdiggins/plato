# Animation, signals and optimization vocabulary

Read [keyframes and tracks](../../api/future/keyframes.types.md), [skeletal animation](../../api/future/skeletal-animation.types.md), [signal records](../../api/future/signals.types.md), and [optimization records](../../api/future/optimization.types.md). These families are grouped here because they primarily describe data/control choices; declaring them does not establish a complete evaluator or solver catalog.

## Tracks and skeletal placement

A keyframe couples a time with a value. Tangent-bearing keys additionally describe interpolation controls. Interpolation between keys, extrapolation outside the track, playback direction and blending need explicit choices. An easing of a single tween is not a complete track evaluator.

A skeleton is a forest of bones addressed by typed indices. A negative parent denotes no parent. Poses are local to the parent; root poses are relative to the model. Computing global bone transforms must respect [transform composition](../foundation/matrices-transforms.md) and acyclic parent references. Skin weights, bind poses, sockets, morph targets, IK chains and retarget maps retain different information; an IK target record is not a solved joint configuration.

## Sampled signals

Uniform scalar sample `i` occurs at `i/SampleRate`; spectrum bin `k` is at `k*BinWidth`, with bin zero at DC. A spectrum, complex spectrum and spectrogram have different stored information. Dropping complex phase can prevent reconstruction even if magnitudes are unchanged.

Window functions, FIR/IIR/biquad selectors, rate-conversion controls and envelopes describe intended processing. An anti-aliasing filter is needed before downsampling when the signal exceeds the target bandwidth; storing a new sample rate does not perform that filtering. Noise color and waveform names need actual generators before their spectral properties are established.

## Optimization descriptions

An optimization problem separates objective, constraints, algorithm controls, stopping criteria and result. Iteration limits and convergence reasons must be interpreted exactly, including any “no limit” sentinel. A returned result is not necessarily a successful minimum; a stopping tolerance is not proof of global optimality. Root brackets need a meaningful domain and sign/continuity assumptions. A declared L-BFGS, simplex, evolutionary or swarm parameter record is not evidence of an implemented solver.

## Related work

[glTF 2.0 animation and skinning](https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html) are related representations for sampled channels, skeletons and weights. Plato's exploratory track/IK/blending vocabulary is broader in some directions and must be mapped explicitly; its declarations do not claim a glTF animation runtime.
