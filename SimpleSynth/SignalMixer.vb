Imports System.ComponentModel.DataAnnotations

Public Class SignalMixer
    Inherits BufferProvider

    Private mFrequency As Double = 0
    Private mSignalGenerators As New List(Of SignalGenerator)

    Public Sub New()
    End Sub

    Public Overrides Sub Close()
        MyBase.Close()

        mSignalGenerators.ForEach(Sub(sg) sg.Close())
    End Sub

    Public ReadOnly Property SignalGenerators As List(Of SignalGenerator)
        Get
            Return mSignalGenerators
        End Get
    End Property

    <RangeAttribute(0.0, Double.MaxValue)>
    Public Overrides Property Frequency As Double
        Get
            Return mFrequency
        End Get
        Set(value As Double)
            mFrequency = value

            For Each sg In mSignalGenerators
                sg.Frequency = mFrequency
            Next
        End Set
    End Property

    Public Overrides Property Note As Note
        Get
            Return MyBase.Note
        End Get
        Set(value As Note)
            MyBase.Note = value
        End Set
    End Property

    Protected Friend Overrides Sub FillAudioBuffer(audioBuffer() As Integer, isFirst As Boolean)
        For Each sg In mSignalGenerators
            sg.FillAudioBuffer(audioBuffer, isFirst)
            If isFirst Then isFirst = False
        Next
    End Sub
End Class
