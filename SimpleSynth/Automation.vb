Imports System.Reflection

Public Class Automation
    Private mSignalGenerator As SignalGenerator
    Private mOscillator As Oscillator
    Private mTargetProperty As PropertyInfo
    Private mFactor As Integer

    Private isSet As Boolean

    Public Sub New(signalGenerator As SignalGenerator)
        mSignalGenerator = signalGenerator
    End Sub

    Public Sub [Set](propertyname As String, oscillator As Oscillator, Optional factor As Integer = Short.MaxValue)
        mOscillator = oscillator
        mTargetProperty = mSignalGenerator.GetType().GetProperty(propertyname)
        mFactor = Short.MaxValue
        isSet = True
    End Sub

    Public Sub Apply()
        If isSet Then
            mTargetProperty.SetValue(mSignalGenerator, mOscillator.Value / Short.MaxValue)
        End If
    End Sub
End Class
