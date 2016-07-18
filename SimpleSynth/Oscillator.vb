Imports System.ComponentModel.DataAnnotations

''' <summary>
''' 
''' </summary>
Public Class Oscillator
    Public Enum WaveForms
        Pulse
        Sinusoidal
        Triangular
        SawTooth
        Constant
        Noise
        CustomFormula
    End Enum

    Private mFrequency As Double = 0
    Private mWaveForm As WaveForms = WaveForms.Pulse
    Private mPulseWidth As Double = 0.5
    Private mConstantValue As Double = 0
    Private mCustomFormula As New Evaluator()

    Private Const ToRad As Double = Math.PI / 180

    Private waveLength As Integer
    Private halfWaveLength As Integer
    Private currentStep As Integer
    Private oscillatorOffset As Double
    Private rnd As New Random()

    Public Sub New()
    End Sub

    'Public Sub New(waveForm As WaveForms, Optional frequency As Double = 0, Optional pulseWidth As Double = 0.5, Optional constantValue As Integer = 0)
    '    Me.WaveForm = waveForm
    '    Me.Frequency = frequency
    '    Me.PulseWidth = pulseWidth
    '    Me.ConstantValue = constantValue
    'End Sub

    <RangeAttribute(0.0, Double.MaxValue)>
    Public Property Frequency As Double
        Get
            Return mFrequency
        End Get
        Set(value As Double)
            mFrequency = value
            ParametersChanged()
        End Set
    End Property

    Public Property WaveForm As WaveForms
        Get
            Return mWaveForm
        End Get
        Set(value As WaveForms)
            mWaveForm = value
            ParametersChanged()
        End Set
    End Property

    <RangeAttribute(0.0, 1.0)>
    Public Property PulseWidth As Double
        Get
            Return mPulseWidth
        End Get
        Set(value As Double)
            mPulseWidth = value
        End Set
    End Property

    Public Property Formula As String
        Get
            Return mCustomFormula.Formula
        End Get
        Set(value As String)
            mCustomFormula.Formula = value
        End Set
    End Property

    Protected Overridable Sub ParametersChanged()
        If mFrequency > 0 Then
            waveLength = AudioMixer.SampleRate / mFrequency
        Else
            waveLength = 0
        End If

        halfWaveLength = waveLength / 2
        oscillatorOffset = 0
    End Sub

    <RangeAttribute(-1.0, 1.0)>
    Public Property ConstantValue As Double
        Get
            Return mConstantValue
        End Get
        Set(value As Double)
            mConstantValue = value
        End Set
    End Property

    Public ReadOnly Property Value() As Integer
        Get
            Dim v As Integer

            If mWaveForm = WaveForms.Constant Then
                v = mConstantValue * 32768
            ElseIf mFrequency = 0 OrElse waveLength = 0 Then
                v = 0
            Else
                Select Case mWaveForm
                    Case WaveForms.Pulse
                        If currentStep <= waveLength * mPulseWidth Then
                            v = 32767
                        Else
                            v = -32768
                        End If

                    Case WaveForms.Sinusoidal
                        v = Math.Sin(oscillatorOffset * mFrequency * ToRad) * 32768

                    Case WaveForms.Triangular
                        If currentStep <= halfWaveLength Then
                            v = -32768 + 65535 * (currentStep / halfWaveLength)
                        Else
                            v = 32767 - 65535 * ((currentStep - halfWaveLength) / halfWaveLength)
                        End If

                    Case WaveForms.SawTooth
                        v = -32768 + 65535 * (currentStep / waveLength)

                    Case WaveForms.Noise
                        v = rnd.Next(-32768, 32767)

                    Case WaveForms.CustomFormula
                        'v = (Math.Sin(oscillatorOffset * mFrequency * ToRad) -
                        '     Math.Cos(oscillatorOffset * mFrequency * 2 * ToRad) ^ 2 +
                        '     Math.Sin(oscillatorOffset / 2 * mFrequency * ToRad)) * 32768

                        Dim cp As New Dictionary(Of String, Double)
                        cp.Add("oscillatorOffset", oscillatorOffset)
                        cp.Add("frequency", mFrequency)
                        cp.Add("currentStep", currentStep)
                        cp.Add("waveLength", waveLength)
                        v = mCustomFormula.Evaluate(cp) * 32768
                End Select

                oscillatorOffset += 360 / AudioMixer.SampleRate
            End If

            currentStep += 1
            If currentStep >= waveLength Then currentStep = 0

            Return v
        End Get
    End Property
End Class
