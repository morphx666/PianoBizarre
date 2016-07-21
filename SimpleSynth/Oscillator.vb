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

    Private Const ToRad As Double = Math.PI / 180.0
    Private Const oscStep As Double = 360.0 / AudioMixer.SampleRate

    Private waveLength As Integer
    Private halfWaveLength As Integer
    Private currentStep As Integer
    Private oscillatorOffset As Double
    Private rnd As New Random()

    Public Sub New()
        mCustomFormula.CustomParameters.Add("oscillatorOffset", oscillatorOffset)
        mCustomFormula.CustomParameters.Add("frequency", mFrequency)
        mCustomFormula.CustomParameters.Add("currentStep", currentStep)
        mCustomFormula.CustomParameters.Add("waveLength", waveLength)
    End Sub

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

        mCustomFormula.CustomParameters("frequency") = mFrequency
        mCustomFormula.CustomParameters("waveLength") = waveLength
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
                v = mConstantValue * Short.MaxValue
            ElseIf waveLength = 0 Then
                v = 0
            Else
                Select Case mWaveForm
                    Case WaveForms.Pulse
                        If currentStep <= waveLength * mPulseWidth Then
                            v = Short.MaxValue
                        Else
                            v = Short.MinValue
                        End If

                    Case WaveForms.Sinusoidal
                        v = Math.Sin(oscillatorOffset * mFrequency * ToRad) * Short.MaxValue

                    Case WaveForms.Triangular
                        If currentStep <= halfWaveLength Then
                            v = Short.MinValue + UShort.MaxValue * (currentStep / halfWaveLength)
                        Else
                            v = Short.MaxValue - UShort.MaxValue * ((currentStep - halfWaveLength) / halfWaveLength)
                        End If

                    Case WaveForms.SawTooth
                        v = Short.MinValue + UShort.MaxValue * (currentStep / waveLength)

                    Case WaveForms.Noise
                        v = rnd.Next(Short.MinValue, Short.MaxValue)

                    Case WaveForms.CustomFormula
                        mCustomFormula.CustomParameters("oscillatorOffset") = oscillatorOffset
                        mCustomFormula.CustomParameters("currentStep") = currentStep
                        v = mCustomFormula.Evaluate() * Short.MaxValue

                End Select

                oscillatorOffset += oscStep
            End If

            currentStep += 1
            If currentStep >= waveLength Then currentStep = 0

            Return v
        End Get
    End Property
End Class
