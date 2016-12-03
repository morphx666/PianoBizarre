Imports System.ComponentModel.DataAnnotations

Public Class SignalGenerator
    Inherits BufferProvider

    Public Sub New()
    End Sub

    Public Property WaveForm As Oscillator.WaveForms
        Get
            Return MyBase.Oscillator.WaveForm
        End Get
        Set(value As Oscillator.WaveForms)
            MyBase.Oscillator.WaveForm = value
        End Set
    End Property

    <RangeAttribute(0.0, 1.0)>
    Public Property PulseWidth As Double
        Get
            Return MyBase.Oscillator.PulseWidth
        End Get
        Set(value As Double)
            MyBase.Oscillator.PulseWidth = value
        End Set
    End Property

    <RangeAttribute(0.0, 1.0)>
    Public Property ConstantValue As Double
        Get
            Return MyBase.Oscillator.ConstantValue
        End Get
        Set(value As Double)
            MyBase.Oscillator.ConstantValue = value
        End Set
    End Property

    Public Property Formula As String
        Get
            Return MyBase.Oscillator.Formula
        End Get
        Set(value As String)
            MyBase.Oscillator.Formula = value
        End Set
    End Property

    Protected Friend Overrides Sub FillAudioBuffer(audioBuffer() As Integer, isFirst As Boolean)
        Dim v As Integer
        Dim bufferWritePosition As Integer

        Dim leftPannning As Double = If(MyBase.Panning <= 0.0, 1.0, 1.0 - MyBase.Panning)
        Dim rightPannning As Double = If(MyBase.Panning >= 0.0, 1.0, 1.0 - -MyBase.Panning)

        Do
            v = MyBase.Oscillator.Value
            v *= MyBase.Volume
            v *= MyBase.Envelop.Volume

            If isFirst Then
                audioBuffer(bufferWritePosition) = v * leftPannning
                audioBuffer(bufferWritePosition + 1) = v * rightPannning
            Else
                audioBuffer(bufferWritePosition) += v * leftPannning
                audioBuffer(bufferWritePosition + 1) += v * rightPannning
            End If

            If bufferWritePosition = audioBuffer.Length - 2 Then Exit Do

            bufferWritePosition += 2
        Loop
    End Sub
End Class
