Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.ComponentModel.DataAnnotations
Imports NAudio.Wave

Public Class AudioMixerNAudio
    Inherits AudioMixer

    Private wo As WaveOut
    Private bwp As BufferedWaveProvider

    Private Sub MainLoop()
        Dim i As Integer

        Do
            Thread.Sleep(10)

            SyncLock SyncObject
                For i = 0 To mBufferProviders.Count - 1
                    mBufferProviders(i).FillAudioBuffer(mAudioBuffer, i = 0)
                Next
            End SyncLock

            ' Hard clipping
            For i = 0 To mAudioBuffer.Length - 1
                Array.Copy(BitConverter.GetBytes(CShort(
                                                 Math.Min(
                                                    Short.MaxValue,
                                                    Math.Max(
                                                        Short.MinValue,
                                                        mAudioBuffer(i) * mVolume)))),
                           0,
                           mainBuffer,
                           i * 2, 2)
            Next

            bwp.AddSamples(mainBuffer, 0, mainBuffer.Length)
        Loop Until cancelAllThreads
    End Sub

    Protected Overrides Sub Initialize()
        Dim n As Integer = SampleRate / 10
        n = n - (n Mod 2)
        ReDim mAudioBuffer(n - 1)
        ReDim mainBuffer(mAudioBuffer.Length * 2 - 1)

        wo = New WaveOut()
        bwp = New BufferedWaveProvider(New WaveFormat(SampleRate, 2)) With {
            .DiscardOnBufferOverflow = True,
            .BufferLength = mainBuffer.Length * 4
        }

        wo.Init(bwp)
        wo.Play()

        playbackThread = New Thread(AddressOf MainLoop)
        playbackThread.Start()
    End Sub
End Class