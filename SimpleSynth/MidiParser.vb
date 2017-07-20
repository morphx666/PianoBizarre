Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

' https://www.csie.ntu.edu.tw/~r92092/ref/midi/#mff0
' http://www.music.mcgill.ca/~ich/classes/mumt306/StandardMIDIfileformat.html

Public Class MidiParser
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)>
    Public Structure HeaderData
        Public Format As UInt16
        Public Tracks As UInt16
        Public Division As UInt16

        Public Sub FixEndianness()
            Format = Format.ToBigEndian()
            Tracks = Tracks.ToBigEndian()
            Division = Division.ToBigEndian()
        End Sub
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)>
    Public Structure HeaderChunk
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=4)> Public ChunkBytes() As Byte
        Public Length As UInt32
        Public Data As HeaderData

        Public Sub FixEndianness()
            Length = Length.ToBigEndian()
            Data.FixEndianness()
        End Sub

        Public ReadOnly Property Chunk As String
            Get
                Return Text.Encoding.ASCII.GetString(ChunkBytes).TrimEnd()
            End Get
        End Property
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)>
    Public Structure TrackChunk
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=4)> Public ChunkBytes() As Byte
        Public Length As UInt32

        Public Sub FixEndianness()
            Length = Length.ToBigEndian()
        End Sub

        Public ReadOnly Property Chunk As String
            Get
                Return Text.Encoding.ASCII.GetString(ChunkBytes).TrimEnd()
            End Get
        End Property
    End Structure

    Private mFileName As String

    Private deltaTime As UInt32

    Public Sub New(fileName As String)
        mFileName = fileName

        Parse()
    End Sub

    Public ReadOnly Property FileName As String
        Get
            Return mFileName
        End Get
    End Property

    Private Sub Parse()
        Dim fs As IO.FileStream = New IO.FileStream(mFileName, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite)

        Dim b(56 - 1) As Byte
        fs.Read(b, 0, b.Length)
        Dim pb As GCHandle = GCHandle.Alloc(b, GCHandleType.Pinned)
        Dim hc As HeaderChunk = Marshal.PtrToStructure(pb.AddrOfPinnedObject(), GetType(HeaderChunk))
        hc.FixEndianness()

        Select Case hc.Data.Format
            Case 0 ' Single Track
                Dim tc As TrackChunk = Marshal.PtrToStructure(pb.AddrOfPinnedObject(), GetType(TrackChunk))
                ParseTrackData(fs, tc.Length)
        End Select

        Stop
    End Sub

    Private Sub ParseTrackData(fs As IO.FileStream, len As UInt32)
        Dim b As Byte

        Do
            deltaTime = ParseVlv(fs)
            Threading.Thread.Sleep(deltaTime)

            b = fs.ReadByte()
            Select Case b
                Case &HF0, &HF7 : ParseSysExEvent(fs)
                Case &HFF : ParseMetaEvent(fs)
                Case Else : ParseMidiEvent(fs, b)
            End Select
        Loop

        Stop
    End Sub

    Private Sub ParseSysExEvent(fs As IO.FileStream)
        LogMsg("SysEx", 0)

        Dim len As UInt32 = ParseVlv(fs)

        Dim b(len - 1) As Byte
        fs.Read(b, 0, b.Length)
    End Sub

    Private Sub ParseMidiEvent(fs As IO.FileStream, b As Byte)
        Dim status As Byte = (b And &HF0) >> 4
        Dim channel As Byte = b And &HF

        Dim v1 As Byte = fs.ReadByte()
        Dim v2 As Byte = fs.ReadByte()

        LogMsg($"MidiEvent: {channel}", 0)

        Select Case status
            Case &H8 ' Note Off
                LogMsg($"Note Off: {v1}:{v2}", 1)
                ' v1 = Key
                ' v2 = Velocity
            Case &H9 ' Note On
                LogMsg($"Note On: {v1}:{v2}", 1)
                ' v1 = Key
                ' v2 = Velocity
            Case &HA ' Polyphonic Key Pressure
                LogMsg($"Polyphonic Key Pressure: {v1}:{v2}", 1)
                ' v1 = Key
                ' v2 = Pressure
            Case &HB ' Controller Change
                LogMsg($"Controller Change: {v1}:{v2}", 1)
                ' v1 = Controller Number
                ' v2 = Controller Value

                Select Case v1
                    Case &H0 ' Bank Select 0-127 MSB
                        LogMsg("Bank Select 0-127 MSB", 2)
                    Case &H1 ' Modulation wheel 0-127 MSB
                        LogMsg("Modulation wheel 0-127 MSB", 2)
                    Case &H2 ' Breath control 0-127 MSB
                        LogMsg("Breath control 0-127 MSB", 2)
                    Case &H3 ' Undefined 0-127 MSB
                        LogMsg("Undefined 0-127 MSB", 2)
                    Case &H4 ' Foot controller 0-127 MSB
                        LogMsg("Foot controller 0-127 MSB", 2)
                    Case &H5 ' Portamento time 0-127 MSB
                        LogMsg("Portamento time 0-127 MSB", 2)
                    Case &H6 ' Data Entry 0-127 MSB
                        LogMsg("Data Entry 0-127 MSB", 2)
                    Case &H7 ' Channel Volume (formerly Main Volume) 0-127 MSB
                        LogMsg("Channel Volume (formerly Main Volume) 0-127 MSB", 2)
                    Case &H8 ' Balance 0-127 MSB
                        LogMsg("Balance 0-127 MSB", 2)
                    Case &H9 ' Undefined 0-127 MSB
                        LogMsg("Undefined 0-127 MSB", 2)
                    Case &HA ' Pan 0-127 MSB
                        LogMsg("Pan 0-127 MSB", 2)
                    Case &HB ' Expression Controller 0-127 MSB
                        LogMsg("Expression Controller 0-127 MSB", 2)
                    Case &HC ' Effect control 1 0-127 MSB
                        LogMsg("Effect control 1 0-127 MSB", 2)
                    Case &HD ' Effect control 2 0-127 MSB
                        LogMsg("Effect control 2 0-127 MSB", 2)
                    Case &HE ' Undefined 0-127 MSB
                        LogMsg("Undefined 0-127 MSB", 2)
                    Case &HF ' Undefined 0-127 MSB
                        LogMsg("Undefined 0-127 MSB", 2)
                    Case &H10 ' General Purpose Controller #1 0-127 MSB
                        LogMsg("General Purpose Controller #1 0-127 MSB", 2)
                    Case &H11 ' General Purpose Controller #2 0-127 MSB
                        LogMsg("General Purpose Controller #2 0-127 MSB", 2)
                    Case &H12 ' General Purpose Controller #3 0-127 MSB
                        LogMsg("General Purpose Controller #3 0-127 MSB", 2)
                    Case &H13 ' General Purpose Controller #4 0-127 MSB
                        LogMsg("General Purpose Controller #4 0-127 MSB", 2)
                    Case &H14 ' Undefined 0-127 MSB
                        LogMsg("Undefined 0-127 MSB", 2)
                    Case &H15 ' Undefined 0-127 MSB
                        LogMsg("Undefined 0-127 MSB", 2)
                    Case &H16 ' Undefined 0-127 MSB
                        LogMsg("Undefined 0-127 MSB", 2)
                    Case &H17 ' Undefined 0-127 MSB
                        LogMsg("Undefined 0-127 MSB", 2)
                    Case &H18 ' Undefined 0-127 MSB
                        LogMsg("Undefined 0-127 MSB", 2)
                    Case &H19 ' Undefined 0-127 MSB
                        LogMsg("Undefined 0-127 MSB", 2)
                    Case &H1A ' Undefined 0-127 MSB
                        LogMsg("Undefined 0-127 MSB", 2)
                    Case &H1B ' Undefined 0-127 MSB
                        LogMsg("Undefined 0-127 MSB", 2)
                    Case &H1C ' Undefined 0-127 MSB
                        LogMsg("Undefined 0-127 MSB", 2)
                    Case &H1D ' Undefined 0-127 MSB
                        LogMsg("Undefined 0-127 MSB", 2)
                    Case &H1E ' Undefined 0-127 MSB
                        LogMsg("Undefined 0-127 MSB", 2)
                    Case &H1F ' Undefined 0-127 MSB
                        LogMsg("Undefined 0-127 MSB", 2)
                    Case &H20 ' Bank Select 0-127 LSB
                        LogMsg("Bank Select 0-127 LSB", 2)
                    Case &H21 ' Modulation wheel 0-127 LSB
                        LogMsg("Modulation wheel 0-127 LSB", 2)
                    Case &H22 ' Breath control 0-127 LSB
                        LogMsg("Breath control 0-127 LSB", 2)
                    Case &H23 ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H24 ' Foot controller 0-127 LSB
                        LogMsg("Foot controller 0-127 LSB", 2)
                    Case &H25 ' Portamento time 0-127 LSB
                        LogMsg("Portamento time 0-127 LSB", 2)
                    Case &H26 ' Data entry 0-127 LSB
                        LogMsg("Data entry 0-127 LSB", 2)
                    Case &H27 ' Channel Volume (formerly Main Volume) 0-127 LSB
                        LogMsg("Channel Volume (formerly Main Volume) 0-127 LSB", 2)
                    Case &H28 ' Balance 0-127 LSB
                        LogMsg("Balance 0-127 LSB", 2)
                    Case &H29 ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H2A ' Pan 0-127 LSB
                        LogMsg("Pan 0-127 LSB", 2)
                    Case &H2B ' Expression Controller 0-127 LSB
                        LogMsg("Expression Controller 0-127 LSB", 2)
                    Case &H2C ' Effect control 1 0-127 LSB
                        LogMsg("Effect control 1 0-127 LSB", 2)
                    Case &H2D ' Effect control 2 0-127 LSB
                        LogMsg("Effect control 2 0-127 LSB", 2)
                    Case &H2E ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H2F ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H30 ' General Purpose Controller #1 0-127 LSB
                        LogMsg("General Purpose Controller #1 0-127 LSB", 2)
                    Case &H31 ' General Purpose Controller #2 0-127 LSB
                        LogMsg("General Purpose Controller #2 0-127 LSB", 2)
                    Case &H32 ' General Purpose Controller #3 0-127 LSB
                        LogMsg("General Purpose Controller #3 0-127 LSB", 2)
                    Case &H33 ' General Purpose Controller #4 0-127 LSB
                        LogMsg("General Purpose Controller #4 0-127 LSB", 2)
                    Case &H34 ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H35 ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H36 ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H37 ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H38 ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H39 ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H3A ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H3B ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H3C ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H3D ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H3E ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H3F ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H40 ' Damper pedal on/off (Sustain) <63=off >64=on
                        LogMsg("Damper pedal on/off (Sustain) <63=off >64=on", 2)
                    Case &H41 ' Portamento on/off <63=off >64=on
                        LogMsg("Portamento on/off <63=off >64=on", 2)
                    Case &H42 ' Sustenuto on/off <63=off >64=on
                        LogMsg("Sustenuto on/off <63=off >64=on", 2)
                    Case &H43 ' Soft pedal on/off <63=off >64=on
                        LogMsg("Soft pedal on/off <63=off >64=on", 2)
                    Case &H44 ' Legato Footswitch <63=off >64=on
                        LogMsg("Legato Footswitch <63=off >64=on", 2)
                    Case &H45 ' Hold 2 <63=off >64=on
                        LogMsg("Hold 2 <63=off >64=on", 2)
                    Case &H46 ' Sound Controller 1 (Sound Variation) 0-127 LSB
                        LogMsg("Sound Controller 1 (Sound Variation) 0-127 LSB", 2)
                    Case &H47 ' Sound Controller 2 (Timbre) 0-127 LSB
                        LogMsg("Sound Controller 2 (Timbre) 0-127 LSB", 2)
                    Case &H48 ' Sound Controller 3 (Release Time) 0-127 LSB
                        LogMsg("Sound Controller 3 (Release Time) 0-127 LSB", 2)
                    Case &H49 ' Sound Controller 4 (Attack Time) 0-127 LSB
                        LogMsg("Sound Controller 4 (Attack Time) 0-127 LSB", 2)
                    Case &H4A ' Sound Controller 5 (Brightness) 0-127 LSB
                        LogMsg("Sound Controller 5 (Brightness) 0-127 LSB", 2)
                    Case &H4B ' Sound Controller 6 0-127 LSB
                        LogMsg("Sound Controller 6 0-127 LSB", 2)
                    Case &H4C ' Sound Controller 7 0-127 LSB
                        LogMsg("Sound Controller 7 0-127 LSB", 2)
                    Case &H4D ' Sound Controller 8 0-127 LSB
                        LogMsg("Sound Controller 8 0-127 LSB", 2)
                    Case &H4E ' Sound Controller 9 0-127 LSB
                        LogMsg("Sound Controller 9 0-127 LSB", 2)
                    Case &H4F ' Sound Controller 10 0-127 LSB
                        LogMsg("Sound Controller 10 0-127 LSB", 2)
                    Case &H50 ' General Purpose Controller #5 0-127 LSB
                        LogMsg("General Purpose Controller #5 0-127 LSB", 2)
                    Case &H51 ' General Purpose Controller #6 0-127 LSB
                        LogMsg("General Purpose Controller #6 0-127 LSB", 2)
                    Case &H52 ' General Purpose Controller #7 0-127 LSB
                        LogMsg("General Purpose Controller #7 0-127 LSB", 2)
                    Case &H53 ' General Purpose Controller #8 0-127 LSB
                        LogMsg("General Purpose Controller #8 0-127 LSB", 2)
                    Case &H54 ' Portamento Control 0-127 Source Note
                        LogMsg("Portamento Control 0-127 Source Note", 2)
                    Case &H55 ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H56 ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H57 ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H58 ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H59 ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H5A ' Undefined 0-127 LSB
                        LogMsg("Undefined 0-127 LSB", 2)
                    Case &H5B ' Effects 1 Depth 0-127 LSB
                        LogMsg("Effects 1 Depth 0-127 LSB", 2)
                    Case &H5C ' Effects 2 Depth 0-127 LSB
                        LogMsg("Effects 2 Depth 0-127 LSB", 2)
                    Case &H5D ' Effects 3 Depth 0-127 LSB
                        LogMsg("Effects 3 Depth 0-127 LSB", 2)
                    Case &H5E ' Effects 4 Depth 0-127 LSB
                        LogMsg("Effects 4 Depth 0-127 LSB", 2)
                    Case &H5F ' Effects 5 Depth 0-127 LSB
                        LogMsg("Effects 5 Depth 0-127 LSB", 2)

                    Case &H60 ' Data entry +1 N/A
                        LogMsg("Data entry +1 N/A", 2)
                        fs.Seek(-1, IO.SeekOrigin.Current)
                    Case &H61 ' Data entry -1 N/A
                        LogMsg("Data entry -1 N/A", 2)
                        fs.Seek(-1, IO.SeekOrigin.Current)

                    Case &H62 ' Non-Registered Parameter Number LSB 0-127 LSB
                        LogMsg("Non-Registered Parameter Number LSB 0-127 LSB", 2)
                    Case &H63 ' Non-Registered Parameter Number MSB 0-127 MSB
                        LogMsg("Non-Registered Parameter Number MSB 0-127 MSB", 2)
                    Case &H64 ' Registered Parameter Number LSB 0-127 LSB
                        LogMsg("Registered Parameter Number LSB 0-127 LSB", 2)
                    Case &H65 ' Registered Parameter Number MSB 0-127 MSB
                        LogMsg("Registered Parameter Number MSB 0-127 MSB", 2)

                    Case &H66 ' Undefined ?
                        LogMsg("Undefined ?", 2)
                    Case &H67 ' Undefined ?
                        LogMsg("Undefined ?", 2)
                    Case &H68 ' Undefined ?
                        LogMsg("Undefined ?", 2)
                    Case &H69 ' Undefined ?
                        LogMsg("Undefined ?", 2)
                    Case &H6A ' Undefined ?
                        LogMsg("Undefined ?", 2)
                    Case &H6B ' Undefined ?
                        LogMsg("Undefined ?", 2)
                    Case &H6C ' Undefined ?
                        LogMsg("Undefined ?", 2)
                    Case &H6D ' Undefined ?
                        LogMsg("Undefined ?", 2)
                    Case &H6E ' Undefined ?
                        LogMsg("Undefined ?", 2)
                    Case &H6F ' Undefined ?
                        LogMsg("Undefined ?", 2)
                    Case &H70 ' Undefined ?
                        LogMsg("Undefined ?", 2)
                    Case &H71 ' Undefined ?
                        LogMsg("Undefined ?", 2)
                    Case &H72 ' Undefined ?
                        LogMsg("Undefined ?", 2)
                    Case &H73 ' Undefined ?
                        LogMsg("Undefined ?", 2)
                    Case &H74 ' Undefined ?
                        LogMsg("Undefined ?", 2)
                    Case &H75 ' Undefined ?
                        LogMsg("Undefined ?", 2)
                    Case &H76 ' Undefined ?
                        LogMsg("Undefined ?", 2)
                    Case &H77 ' Undefined ?
                        LogMsg("Undefined ?", 2)

                    Case &H78 ' All Sound Off
                        LogMsg("All Sound Off", 2)
                    Case &H79 ' Reset All Controllers
                        LogMsg("Reset All Controllers", 2)
                    Case &H7A ' Local Control
                        LogMsg("Local Control", 2)
                        ' If v2 = 0 Then, disconnect the local keyboard from the sound generating functions Of a synthesizer.
                        ' If v2 = &h7F Then, reconnect the local keyboard To the sound-generator. 
                    Case &H7B ' All Notes Off
                        LogMsg("All Notes Off", 2)
                    Case &H7C ' Omni Mode Off
                        LogMsg("Omni Mode Off", 2)
                    Case &H7D ' Omni Mode On
                        LogMsg("Omni Mode On", 2)
                    Case &H7E ' Monophonic Mode On (Polyphonic Off)
                        LogMsg("Monophonic Mode On (Polyphonic Off)", 2)
                        ' v2 = Number of MIDI Channels to use when in Mode 4
                        ' This parameter has no effect in Mode 2
                    Case &H7F ' Monophonic Mode Off (Polyphonic On)
                        LogMsg("Monophonic Mode Off (Polyphonic On)", 2)

                    Case Else
                        Stop
                End Select
            Case &HC ' Program Change
                LogMsg($"Program Change: {v1}", 1)
                ' v1 = Program Number
                fs.Seek(-1, IO.SeekOrigin.Current)
            Case &HD ' Channel Key Pressure
                LogMsg($"Channel Key Pressure: {v1}", 1)
                ' v1 = Channel Pressure Value
                fs.Seek(-1, IO.SeekOrigin.Current)
            Case &HE ' Pitch Bend
                LogMsg($"Pitch Bend: {v1}", 1)
                ' v1 = LSB
                ' v2 = MSB

            Case Else
                Stop
        End Select
    End Sub

    Private Sub ParseMetaEvent(fs As IO.FileStream)
        LogMsg("MetaEvent", 0)

        Dim b As Byte = fs.ReadByte()

        Dim buf() As Byte
        Dim txt As String = ""
        Dim len As Short = ParseVlv(fs)

        Dim ReadText = Function() As String
                           ReDim buf(len - 1)
                           fs.Read(buf, 0, buf.Length)
                           Return Text.Encoding.ASCII.GetString(buf)
                       End Function

        Select Case b
            Case &H0 ' Sequence Number
                LogMsg("Sequence Number", 1)
                fs.ReadByte()
                fs.ReadByte()
            Case &H1 ' Text Event
                txt = ReadText()
                LogMsg($"Text Event: {txt}", 1)
            Case &H2 ' Copyright Notice
                txt = ReadText()
                LogMsg($"Text Event: {txt}", 1)
            Case &H3 ' Sequence/Track Name
                txt = ReadText()
                LogMsg($"Sequence/Track Name: {txt}", 1)
            Case &H4 ' Instrument Name
                txt = ReadText()
                LogMsg($"Instrument Name: {txt}", 1)
            Case &H5 ' Lyric
                txt = ReadText()
                LogMsg($"Lyric: {txt}", 1)
            Case &H6 ' Marker
                txt = ReadText()
                LogMsg($"Marker: {txt}", 1)
            Case &H7 '  Cue Point
                txt = ReadText()
                LogMsg($"Cue Point: {txt}", 1)
            Case &H20 ' MIDI Channel Prefix
                LogMsg("MIDI Channel Prefix", 1)
                fs.ReadByte()
            Case &H2F ' End of Track
                LogMsg("End of Track", 1)
            Case &H51 ' Set Tempo
                LogMsg("Set Tempo", 1)
                fs.ReadByte()
                fs.ReadByte()
                fs.ReadByte()
            Case &H54 ' SMTPE Offset
                LogMsg("SMTPE Offset", 1)
                fs.ReadByte()
                fs.ReadByte()
                fs.ReadByte()
                fs.ReadByte()
                fs.ReadByte()
            Case &H58 ' Time Signature
                LogMsg("Time Signature", 1)
                fs.ReadByte()
                fs.ReadByte()
                fs.ReadByte()
                fs.ReadByte()
            Case &H59 ' Key Signature
                LogMsg("Key Signature", 1)
                fs.ReadByte()
                fs.ReadByte()
            Case &H7F ' Sequencer-Specific Meta-event
                LogMsg("Sequencer-Specific Meta-event", 1)
                fs.ReadByte()
                fs.ReadByte()
        End Select
    End Sub

    Private Function ParseVlv(fs As IO.FileStream) As UInt32
        Dim b As Byte
        Dim bytes As New List(Of Byte)

        Do
            b = fs.ReadByte()
            bytes.Add(b)
        Loop Until (b And &B10000000) = 0

        Select Case bytes.Count
            Case 1 : Return bytes(0)
            Case 2 : Return BitConverter.ToUInt16(bytes.ToArray(), 0)
            Case 4 : Return BitConverter.ToUInt32(bytes.ToArray(), 0)
        End Select

        Return BitConverter.ToUInt32(bytes.ToArray(), 0)
    End Function

    Private Sub LogMsg(msg As String, indentLevel As Integer)
        Debug.WriteLine($"{deltaTime}".PadLeft(4) + ": " + "".PadLeft(indentLevel * 2) + msg)
    End Sub
End Class

Module Extensions
    <Extension()> Public Function ToBigEndian(value As UInt16) As UInt16
        Return BitConverter.ToInt16(BitConverter.GetBytes(value).Reverse().ToArray(), 0)
    End Function

    <Extension()> Public Function ToBigEndian(value As UInt32) As UInt32
        Dim b() As Byte = BitConverter.GetBytes(value)
        Dim low As UInt16 = BitConverter.ToInt16({b(2), b(3)}, 0)
        Dim high As UInt16 = BitConverter.ToInt16({b(0), b(1)}, 0)
        Return high * 65535 + low
    End Function
End Module