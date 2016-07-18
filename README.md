# PianoBizarre

This is a work in progress and a personal experiment in trying to create a simple synthesizer in VB.NET

So far, the library supports:
- Unlimited polyphony
- Multiple and configurable oscillator modes (Sinusoidal, Pulse, Triangular, Sawtooth, Custom Formulas, etc...)
- Supports Envelopes (Attack, Decay, Sustain and Release)
- Supports triggering multiple tone/signal generators at once (SignalMixer)

Missing features and functionality that I hope to implement:
- Support for envelope curves (exponetial, logarithmic, s-curve, etc...)
- Better audio mixing algorithm (instead of simply clipping it)
- Stereo support (with panning control)
- Automation (ability to control parameters, such as the Volume of a signal generator, through an oscillator)
- Implement a signal generator that can playback audio files (sampler)
- MIDI support
- A *real* UI to showcase the library

Dependencies:
- Audio playback requires [SlimDX](https://slimdx.org/).
- Evaluation of custom formulas requires the [NCalc](https://ncalc.codeplex.com/) library.
