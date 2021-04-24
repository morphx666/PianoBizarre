Imports SimpleSynth

Public Class Channel
    Public ReadOnly Property Instrument As SignalGenerator
    Public ReadOnly Property Notes As New List(Of TrackerNote)
    Public ReadOnly Property Pattern As Pattern

    Public Property Name As String

    Public Sub New(pattern As Pattern)
        Me.Pattern = pattern

        Instrument = New SignalGenerator()

        Instrument.Envelop.Attack = New Envelope.EnvelopePoint(1, 10)
        Instrument.Envelop.Decay = New Envelope.EnvelopePoint(0.6, 100)
        Instrument.Envelop.Sustain = New Envelope.EnvelopePoint(0.6, Integer.MaxValue)
        Instrument.Envelop.Release = New Envelope.EnvelopePoint(0, 100)

        Instrument.Volume = 0.2
        Instrument.WaveForm = Oscillator.WaveForms.Sinusoidal
    End Sub
End Class
