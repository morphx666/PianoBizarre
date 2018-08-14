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
        KarplusStrong
    End Enum

    Private mFrequency As Double = 0
    Private mWaveForm As WaveForms = WaveForms.Pulse
    Private mPulseWidth As Double = 0.5
    Private mConstantValue As Double = 0
    Private mCustomFormula As New Evaluator()

    Private Const ToRad As Double = Math.PI / 180.0
    Private Const oscStep As Double = 360.0 / AudioMixer.SampleRate
    Private ksBuffer() As Integer

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
        mCustomFormula.CustomParameters.Add("halfWaveLength", halfWaveLength)
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

    Public Property CustomFunctionHandler As Evaluator.CustomFunctionDel
        Get
            Return mCustomFormula.CustomFunctionHandler
        End Get
        Set(value As Evaluator.CustomFunctionDel)
            mCustomFormula.CustomFunctionHandler = value
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
        mCustomFormula.CustomParameters("halfWaveLength") = halfWaveLength

        ReDim ksBuffer(waveLength - 1)

        For i As Integer = 0 To ksBuffer.Length - 1
            ksBuffer(i) = rnd.Next(Short.MinValue, Short.MaxValue)
        Next
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

                    Case WaveForms.KarplusStrong
                        SyncLock ksBuffer
                            ' FIXME: Figure out why we get an IndexOutOfRangeException, even when SyncLock'ing on ksBuffer
                            Try
                                v = ksBuffer(currentStep) * If(currentStep >= waveLength / 2, 1, -1)
                                ksBuffer(currentStep) = (ksBuffer(If(currentStep = 0, waveLength - 1, currentStep - 1)) + v) / 2
                            Catch ex As Exception
                            End Try
                        End SyncLock
                End Select

                oscillatorOffset += oscStep
            End If

            currentStep += 1
            If currentStep >= waveLength Then currentStep = 0

            Return v
        End Get
    End Property
End Class