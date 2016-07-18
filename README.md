# PianoBizarre

This is a work in progress and a personal experiment in trying to create a simple synthesizer in VB.NET

So far, the library supports:
- Unlimited polyphony
- Multiple and configurable oscillator modes (Sinusoidal, Pulse, Triangular, Sawtooth, Custom Formulas, etc...)
- Supports Envelopes (Attack, Decay, Sustain and Release)
- Supports triggering multiple tone/signal generators at once (SignalMixer)

Dependencies:
- Audio playback requires [SlimDX](https://slimdx.org/).
- Evaluation of custom formulas requires the [NCalc](https://ncalc.codeplex.com/) library.
