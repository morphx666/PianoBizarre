Imports SlimDX.DirectSound
Imports SlimDX.Multimedia
Imports System.Runtime.InteropServices
Imports System.Threading

Public Class AudioMixerSlimDX
    Inherits AudioMixer

    <DllImport("user32.dll", CharSet:=CharSet.Auto, ExactSpelling:=True)>
    Private Shared Function GetDesktopWindow() As IntPtr
    End Function

    Private audioDev As DirectSound
    Private bufPlayDesc As SoundBufferDescription
    Private playBuf As SecondarySoundBuffer
    Private notifySize As Integer
    Private numberPlaybackNotifications As Integer
    Private nextPlaybackOffset As Integer

    Private waiter As AutoResetEvent
    Private audioWriteBufferPosition As Integer

    Private Sub MainLoop()
        Dim i As Integer

        Do
            waiter.WaitOne()

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

            WriteAudioBuffer()
        Loop Until cancelAllThreads
    End Sub

    Public Overrides Sub Close()
        MyBase.Close()

        playBuf.Stop()
        waiter.Set()

        playBuf.Dispose()
        audioDev.Dispose()
    End Sub

    Protected Overrides Sub Initialize()
        ReDim mAudioBuffer(SampleRate * 2 / 88 - 1)
        ReDim mainBuffer(mAudioBuffer.Length * 2 - 1)

        numberPlaybackNotifications = 4
        waiter = New AutoResetEvent(False)

        ' Define the capture format
        Dim format As WaveFormat = New WaveFormat()
        With format
            .BitsPerSample = 16
            .Channels = 2
            .FormatTag = WaveFormatTag.Pcm
            .SamplesPerSecond = SampleRate
            .BlockAlignment = CShort(.Channels * .BitsPerSample / 8)
            .AverageBytesPerSecond = .SamplesPerSecond * .BlockAlignment
        End With

        ' Define the size of the notification chunks
        notifySize = mainBuffer.Length
        notifySize -= notifySize Mod format.BlockAlignment

        ' Create a buffer description object
        bufPlayDesc = New SoundBufferDescription()
        With bufPlayDesc
            .Format = format
            .Flags = BufferFlags.ControlPositionNotify Or
                     BufferFlags.GetCurrentPosition2 Or
                     BufferFlags.GlobalFocus Or
                     BufferFlags.Static
            .SizeInBytes = notifySize * numberPlaybackNotifications
        End With

        audioDev = New DirectSound()
        audioDev.SetCooperativeLevel(GetDesktopWindow(), CooperativeLevel.Normal)
        playBuf = New SecondarySoundBuffer(audioDev, bufPlayDesc)

        ' Define the notification events
        Dim np(numberPlaybackNotifications - 1) As NotificationPosition

        For i As Integer = 0 To numberPlaybackNotifications - 1
            np(i) = New NotificationPosition()
            np(i).Offset = (notifySize * i) + notifySize - 1
            np(i).Event = waiter
        Next
        playBuf.SetNotificationPositions(np)

        nextPlaybackOffset = 0
        playBuf.Play(0, PlayFlags.Looping)

        playbackThread = New Thread(AddressOf MainLoop)
        playbackThread.Start()
    End Sub

    Private Sub WriteAudioBuffer()
        Dim lockSize As Integer

        lockSize = playBuf.CurrentWritePosition - nextPlaybackOffset
        If lockSize < 0 Then lockSize += bufPlayDesc.SizeInBytes

        ' Block align lock size so that we always read on a boundary
        lockSize -= lockSize Mod notifySize
        If lockSize = 0 Then Exit Sub

        playBuf.Write(Of Byte)(mainBuffer, nextPlaybackOffset, LockFlags.None)

        nextPlaybackOffset += mainBuffer.Length
        nextPlaybackOffset = nextPlaybackOffset Mod bufPlayDesc.SizeInBytes ' Circular buffer
    End Sub
End Class