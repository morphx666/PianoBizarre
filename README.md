# PianoBizarre

This is a work in progress and a personal experiment in trying to create a simple synthesizer in VB.NET

So far, the library supports:
- Unlimited polyphony
- Multiple and configurable [oscillator](https://github.com/morphx666/PianoBizarre/blob/master/SimpleSynth/Oscillator.vb) modes (Sinusoidal, Pulse, Triangular, Sawtooth, Custom Formulas, etc...)
- Supports [Envelopes](https://github.com/morphx666/PianoBizarre/blob/master/SimpleSynth/Envelope.vb) (Attack, Decay, Sustain and Release)
- Supports triggering multiple tone/signal generators at once ([SignalMixer](https://github.com/morphx666/PianoBizarre/blob/master/SimpleSynth/SignalMixer.vb))
- Stereo support (with panning control)
- [Automation](https://github.com/morphx666/PianoBizarre/blob/master/SimpleSynth/Automation.vb) (ability to control parameters, such as the Volume of a signal generator, through an oscillator)

![PianoBizarre](https://whenimbored.xfx.net/wp-content/uploads/2016/07/pianobizarre01.png)

Missing features and functionality that I hope to implement:
- Support for envelope curves (exponetial, logarithmic, s-curve, etc...)
- Better audio mixing algorithm (instead of simply clipping it)
- Implement a signal generator that can playback audio files (sampler)
- MIDI support ([Work in progress](https://github.com/morphx666/PianoBizarre/blob/master/SimpleSynth/MidiParser.vb))
- A *real* UI to showcase the library
- Use NAudio instead of SlimDX ([Work in progress](https://github.com/morphx666/PianoBizarre/blob/master/SimpleSynth/AudioMixerNAudio.vb))

Dependencies:
- Audio playback requires [SlimDX](https://slimdx.org/).
- Evaluation of custom formulas requires the [NCalc](https://github.com/sheetsync/NCalc) library.
