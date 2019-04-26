Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.ComponentModel.DataAnnotations
Imports NAudio.Wave

Public Class AudioMixerNAudio
    Inherits AudioMixer

    Private wo As WaveOut
    Private bwp As CustomBufferProvider

    Protected Overrides Sub Initialize()
        wo = New WaveOut() With {
            .NumberOfBuffers = 8,
            .DesiredLatency = 200
        }

        bwp = New CustomBufferProvider(AddressOf FillAudioBuffer, SampleRate, 16, 2)
        ReDim mAudioBuffer(0)

        wo.Init(bwp)
        wo.Play()
    End Sub

    Private Sub FillAudioBuffer(buffer() As Byte)
        If buffer.Length <> mAudioBuffer.Length Then ReDim mAudioBuffer(buffer.Length \ 2 - 1)
        For i = 0 To mBufferProviders.Count - 1
            mBufferProviders(i).FillAudioBuffer(mAudioBuffer, i = 0)
        Next

        For i = 0 To mAudioBuffer.Length - 2
            Array.Copy(BitConverter.GetBytes(CShort(
                                             Math.Min(
                                                Short.MaxValue,
                                                Math.Max(
                                                    Short.MinValue,
                                                    mAudioBuffer(i) * mVolume)))),
                       0,
                       buffer,
                       i * 2, 2)
        Next
    End Sub
End Class

Public Class CustomBufferProvider
    Implements IWaveProvider

    Public Delegate Sub FillBuffer(buffer() As Byte)

    Public ReadOnly Property WaveFormat As WaveFormat Implements IWaveProvider.WaveFormat
    Private fb As FillBuffer

    Public Sub New(bufferFiller As FillBuffer, sampleRate As Integer, bitDepth As Integer, channels As Integer)
        WaveFormat = New WaveFormat(sampleRate, bitDepth, channels)
        fb = bufferFiller
    End Sub

    Public Function Read(buffer() As Byte, offset As Integer, count As Integer) As Integer Implements IWaveProvider.Read
        fb.Invoke(buffer)
        Return count
    End Function
End Class