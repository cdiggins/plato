# Animation, signals and optimization vocabulary

This vocabulary is in the `future` tier, which is not part of the default library. The families below mostly describe data and control choices. Declaring them does not supply a complete evaluator or solver catalog.

## Tracks and skeletal placement

A keyframe pairs a time with a value. A key that carries tangents also describes interpolation controls. Interpolation between keys, extrapolation outside the track, playback direction, and blending are separate choices. An easing on a single tween is not a track evaluator.

A skeleton is a forest of bones addressed by typed indices. A negative parent means no parent. A pose is local to the parent. A root pose is relative to the model. A global bone transform has to follow transform composition and has to stay on acyclic parent references. Skin weights, bind poses, sockets, morph targets, IK chains, and retarget maps keep different information. An IK target record is not a solved joint configuration.

## Sampled signals

Uniform scalar sample `i` is at time `i/SampleRate`. Spectrum bin `k` is at frequency `k*BinWidth`, and bin zero is DC. A spectrum, a complex spectrum, and a spectrogram store different information. Dropping complex phase can make reconstruction impossible even when the magnitudes are unchanged.

Window functions, FIR, IIR, and biquad selectors, rate-conversion controls, and envelopes describe intended processing. Downsampling a signal that exceeds the target bandwidth needs an anti-aliasing filter first. Storing a new sample rate does not apply that filter. A noise-color name or a waveform name needs a generator before its spectral properties are real.

## An optimization record is not a solved minimum

An optimization problem separates the objective, the constraints, the algorithm controls, the stopping criteria, and the result. Read iteration limits and convergence reasons exactly, including any "no limit" sentinel. A returned result is not necessarily a successful minimum. A stopping tolerance is not global optimality. A root bracket needs a meaningful domain and the sign or continuity assumptions the method uses. A declared L-BFGS, simplex, evolutionary, or swarm parameter record is not an implemented solver.

## Where to look

- [Keyframes and tracks](../../api/future/keyframes.types.md)
- [Skeletal animation](../../api/future/skeletal-animation.types.md)
- [Signal records](../../api/future/signals.types.md)
- [Optimization records](../../api/future/optimization.types.md)
- [Transform composition](../foundation/matrices-transforms.md)
- [Aspirational library domains](domains.md)

[glTF 2.0 animation and skinning](https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html) describe sampled channels, skeletons, and weights. The track, IK, and blending vocabulary here has to be mapped explicitly, and the declarations are not a glTF animation runtime.
