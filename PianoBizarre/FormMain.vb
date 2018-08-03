Imports System.Threading
Imports SimpleSynth

Public Class FormMain
    Private refreshThread As Thread
    Private abortThreads As Boolean
    Private am As AudioMixer

    Private Enum WaveFormRendererModes
        Line
        Blobs
    End Enum

    Private largeFont As New Font("Consolas", 13, FontStyle.Bold)
    Private waveFormRendererMode As WaveFormRendererModes = WaveFormRendererModes.Line
    Private keyboardKeys As New List(Of KeyRenderer)
    Private bufferHistory As New List(Of Integer())

    Private Sub FormMain_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        abortThreads = True
        am.Close()
    End Sub

    Private Sub FormMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Me.SetStyle(ControlStyles.ResizeRedraw, True)
        Me.SetStyle(ControlStyles.UserPaint, True)

        'Dim midi As New MidiParser("La Roux - In For The Kill.mid")

        InitKeyboardUI()
        InitAudioMixer()
        SetupEventHandlers()

        refreshThread = New Thread(Sub()
                                       Do
                                           Thread.Sleep(33)
                                           Me.Invalidate()
                                       Loop Until abortThreads
                                   End Sub)
        refreshThread.Start()

        'For Each bp In am.BufferProviders
        '    bp.Envelop.Attack.Duration = 0
        '    bp.Envelop.Release.Duration = 2
        'Next
        ''"BB DB BB CE GD BB"
        'Dim x As String = "A A CA A ABD F CAA" '.Reverse().ToArray()
        'For i As Integer = 0 To x.Length - 1
        '    For Each bp In am.BufferProviders
        '        If x(i) <> " " Then bp.Note = $"{x(i)} 4"
        '        Debug.Write(x(i))
        '        Thread.Sleep(100)
        '        bp.Frequency = 0
        '        Thread.Sleep(180)
        '        Exit For
        '    Next
        '    Debug.WriteLine("")
        'Next
    End Sub

    Private Sub SetupEventHandlers()
        AddHandler Me.KeyDown, Sub(s1 As Object, e1 As KeyEventArgs)
                                   Dim newNote As String = KeyToNote(e1.KeyCode)

                                   If newNote = "" OrElse am.BufferProviders.Any(Function(bp) bp.Tag = e1.KeyCode) Then Exit Sub

                                   For Each bp In am.BufferProviders
                                       If bp.Frequency = 0 Then
                                           bp.Note = newNote
                                           bp.Tag = e1.KeyCode
                                           Debug.WriteLine($"{bp.Note}: {bp.Frequency:N4} Hz")
                                           Exit For
                                       End If
                                   Next
                               End Sub

        AddHandler Me.KeyUp, Sub(s1 As Object, e1 As KeyEventArgs)
                                 Dim note As String = KeyToNote(e1.KeyCode)
                                 If note = "" Then Exit Sub

                                 For Each bp In am.BufferProviders
                                     If bp.Tag = e1.KeyCode Then ' bp.Note = note Then
                                         bp.Frequency = 0
                                         bp.Tag = Nothing
                                         Exit For
                                     End If
                                 Next
                             End Sub
    End Sub

    Private Sub InitAudioMixer()
        am = New AudioMixerSlimDX()
        'am = New AudioMixerNAudio()
        For i As Integer = 1 To 6 ' note polyphony
            ' Simple sinusoidal oscillator
            'am.BufferProviders.Add(CreateInstrument1())

            ' Multiple oscillators, panning and automation (SignalMixer)
            'am.BufferProviders.Add(CreateInstrument2())

            ' Custom formula
            am.BufferProviders.Add(CreateInstrument3())

            ' Karplus-Strong Algorithm (String instrument simulation)
            'am.BufferProviders.Add(CreateInstrument4())

            ' Noise
            'am.BufferProviders.Add(CreateInstrument5())
        Next
        am.Volume = 0.8
    End Sub

    Private Sub InitKeyboardUI()
        Dim padding As Integer = 1
        Dim whitesSize As New Size(40, 180)
        Dim blacksSize As New Size(whitesSize.Width * 0.8, whitesSize.Height / 2)

        Dim p As New Point(0, 0)

        Dim n As New Note("C 4")
        For i As Integer = 0 To 17 - 1
            If Not n.IsSharp Then
                keyboardKeys.Add(New KeyRenderer(n, New Rectangle(p, whitesSize)))
                p.X += whitesSize.Width
            End If
            p.X += padding
            n += 1
        Next

        p.X = 0
        n = "C 4"
        For i As Integer = 0 To 17 - 1
            If n.IsSharp Then
                keyboardKeys.Add(New KeyRenderer(n, New Rectangle(New Point(p.X - blacksSize.Width / 2, p.Y), blacksSize)))
            Else
                p.X += whitesSize.Width
            End If
            p.X += padding
            n += 1
        Next
    End Sub

    ''' <summary>
    ''' Simple sinusoidal signal generator
    ''' </summary>
    ''' <returns><see cref="SignalGenerator"/></returns>
    Private Function CreateInstrument1() As SignalGenerator
        Dim sg As New SignalGenerator()

        ' http://making-music.com/wp-content/uploads/2012/07/Envelopes-ADSR.jpg
        '  A  D   S   R
        '    /\       
        '   /  ───╫───\
        '  /           \
        sg.Envelop.Attack = New Envelope.EnvelopePoint(1, 10)
        sg.Envelop.Decay = New Envelope.EnvelopePoint(0.6, 100)
        sg.Envelop.Sustain = New Envelope.EnvelopePoint(0.6, Integer.MaxValue)
        sg.Envelop.Release = New Envelope.EnvelopePoint(0, 100)

        sg.Volume = 0.5
        sg.WaveForm = Oscillator.WaveForms.Sinusoidal

        Return sg
    End Function

    ''' <summary>
    ''' Composite instrument using multiple <see cref="SignalGenerator"/>s combined inside a <see cref="SignalMixer"/>  
    ''' </summary>
    ''' <returns><see cref="SignalMixer"/> </returns>
    Private Function CreateInstrument2() As SignalMixer
        Dim sg As SignalGenerator
        Dim m As New SignalMixer()

        sg = New SignalGenerator() With {
            .WaveForm = Oscillator.WaveForms.Pulse,
            .PulseWidth = 0.3,
            .Volume = 0.35,
            .Panning = 0.8
        }
        sg.Envelop.Sustain = New Envelope.EnvelopePoint(1, 500)
        sg.Envelop.Release = New Envelope.EnvelopePoint(0, 300)
        m.SignalGenerators.Add(sg)

        sg = New SignalGenerator() With {
            .Volume = 0.2,
            .WaveForm = Oscillator.WaveForms.Sinusoidal
        }
        sg.Envelop.Attack = New Envelope.EnvelopePoint(1, 300)
        sg.Envelop.Release = New Envelope.EnvelopePoint(0, 400)

        Dim osc As New Oscillator() With {
            .WaveForm = Oscillator.WaveForms.Sinusoidal,
            .Frequency = 4
        }
        sg.Automation.Set("Panning", osc)

        m.SignalGenerators.Add(sg)

        sg = New SignalGenerator() With {
            .Volume = 0.4,
            .Panning = -0.8
        }
        sg.NoteShiftOffset -= 12
        sg.WaveForm = Oscillator.WaveForms.SawTooth
        sg.Envelop.Sustain = New Envelope.EnvelopePoint(1, 800)
        sg.Envelop.Release = New Envelope.EnvelopePoint(0, 600)
        m.SignalGenerators.Add(sg)

        sg = New SignalGenerator() With {
            .WaveForm = Oscillator.WaveForms.Noise,
            .Volume = 0.04
        }
        sg.Envelop.Sustain = New Envelope.EnvelopePoint(1, 500)
        sg.Envelop.Release = New Envelope.EnvelopePoint(0, 400)
        m.SignalGenerators.Add(sg)
        Return m
    End Function

    ''' <summary>
    ''' A more complex type of instrument making use of a <see cref="Oscillator.WaveForms.CustomFormula"/> 
    ''' </summary>
    ''' <returns><see cref="SignalGenerator"/></returns>
    Private Function CreateInstrument3() As SignalGenerator
        Dim sg As New SignalGenerator()

        sg.WaveForm = Oscillator.WaveForms.CustomFormula
        sg.Envelop.Release = New Envelope.EnvelopePoint(0, 300)
        sg.Volume = 0.5

        ' ~~~ Organ ~~~
        'sg.Formula = "0.3 * (Sin(ToRad(oscillatorOffset * frequency))
        '                    - Pow(Cos(ToRad(oscillatorOffset * frequency * 2)), 2)
        '                    + Sin(ToRad(oscillatorOffset / 2 * frequency)))"

        ' ~~~ ??? ~~~
        'sg.Formula = "Sin(frequency * ToRad(oscillatorOffset)) * 
        '                IIf(Sin(oscillatorOffset / 7 + frequency * 4 * ToRad(oscillatorOffset / 2)) > 0, 1, -1)"

        ' ~~~ UFO ~~~
        'sg.Formula = "Osc(10000000 * 1/3) 
        '                * IIf(Osc(10000000 * 1/8) > 0, 1, 0.5) 
        '                * Sin(IIf(Osc(10000000 * 1/8) > 0, 2, 4) 
        '                    * frequency * ToRad(oscillatorOffset))"

        ' ~~~ Electronic Organ ~~~
        sg.Formula = "0.000015 * IIf(currentStep <= halfWaveLength,
                        -32768 + 65535 * Rnd((currentStep / halfWaveLength)),
                         32767 - 65535 * Rnd(((currentStep - halfWaveLength) / halfWaveLength)))"

        Return sg
    End Function

    ''' <summary>
    ''' A Karplus-Strong Algorithm based instrument
    ''' </summary>
    ''' <returns><see cref="SignalGenerator"/></returns>
    Private Function CreateInstrument4() As SignalGenerator
        Dim sg As New SignalGenerator()

        sg.WaveForm = Oscillator.WaveForms.KarplusStrong
        sg.Volume = 1.0

        Return sg
    End Function

    Private Function CreateInstrument5() As SignalGenerator
        Dim sg As New SignalGenerator()

        sg.Envelop.Attack = New Envelope.EnvelopePoint(1, 0)
        sg.Envelop.Decay = New Envelope.EnvelopePoint(1, 10)
        sg.Envelop.Sustain = New Envelope.EnvelopePoint(1, Integer.MaxValue)
        sg.Envelop.Release = New Envelope.EnvelopePoint(0, 0)

        sg.Volume = 0.3
        sg.WaveForm = Oscillator.WaveForms.Noise

        Return sg
    End Function

    Private Function KeyToNote(k As Keys) As String
        Select Case k
            Case Keys.Z : Return "C 4"
            Case Keys.S : Return "C#4"
            Case Keys.X : Return "D 4"
            Case Keys.D : Return "D#4"
            Case Keys.C : Return "E 4"
            Case Keys.V : Return "F 4"
            Case Keys.G : Return "F#4"
            Case Keys.B : Return "G 4"
            Case Keys.H : Return "G#4"
            Case Keys.N : Return "A 4"
            Case Keys.J : Return "A#4"
            Case Keys.M : Return "B 4"
            Case Keys.Oemcomma : Return "C 5"
            Case Keys.L : Return "C#5"
            Case Keys.OemPeriod : Return "D 5"
            Case Keys.OemSemicolon : Return "D#5"
            Case Keys.OemQuestion : Return "E 5"
            Case Else
                Debug.WriteLine($"Invalid Key: {k}")
                Return ""
        End Select
    End Function

    Private Sub FormMain_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        Dim g As Graphics = e.Graphics
        Dim r As Rectangle = Me.DisplayRectangle

        g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality

        g.Clear(Color.FromArgb(&HFF, &H13, &H2F, &H30))

        keyboardKeys.ForEach(Sub(k)
                                 k.State = If(am.BufferProviders.Any(Function(bp) bp.Envelop.EnvelopStep <> Envelope.EnvelopeSteps.Release AndAlso
                                                                                  bp.Frequency = k.Note.Frequency),
                                                        KeyRenderer.KeyStates.Pushed,
                                                        KeyRenderer.KeyStates.Released)
                                 k.Render(g)
                             End Sub)

        Dim h As Integer = r.Height * 0.9
        Dim y As Integer = (r.Height - h) / 2
        Dim x As Integer

        Dim bufLen As Integer = am.AudioBuffer.Length / 2
        Dim bufAvg = Function(index As Integer) bufferHistory.Average(Function(k) k(index))

        SyncLock AudioMixerSlimDX.SyncObject
            If bufferHistory.Count >= 4 Then bufferHistory.RemoveAt(0)
            Dim b(am.AudioBuffer.Length / 2 - 1) As Integer
            For i As Integer = 0 To am.AudioBuffer.Length - 1 Step 2
                b(i / 2) = (am.AudioBuffer(i) + am.AudioBuffer(i + 1)) / 2
            Next
            bufferHistory.Add(b)
        End SyncLock

        'Using p As New Pen(Color.FromArgb(255, 33, 33, 33))
        '    g.DrawLine(p, 0, y, r.Width, y)
        'End Using

        Select Case waveFormRendererMode
            Case WaveFormRendererModes.Line
                Dim p(bufLen - 1) As Point
                For i As Integer = 0 To bufLen - 1
                    x = i / bufLen * r.Width
                    p(i) = New Point(x, (32768 - bufAvg(i) * am.Volume) / 65536 * h + y)
                Next
                Using pc As New Pen(Color.Black, 5)
                    g.DrawLines(pc, p)
                End Using
                Using pc As New Pen(Color.FromArgb(&HFF, &H7, &HD8, &HDB), 3)
                    g.DrawLines(pc, p)
                End Using
                Using pc As New Pen(Color.FromArgb(&HE0, &H7 + 30, &HD8 + 30, &HDB + 30), 1.5)
                    g.DrawLines(pc, p)
                End Using
            Case WaveFormRendererModes.Blobs
                Dim l As New Rectangle()
                Dim m As Integer = h / 2 + y
                l.Width = 4
                For i As Integer = 0 To bufLen - 1 Step 4
                    l.X = i / bufLen * r.Width
                    l.Y = (32768 - bufAvg(i) * am.Volume) / 65536 * h + y
                    If bufAvg(i) = 0 Then
                        l.Height = 1
                    ElseIf bufAvg(i) < 0 Then
                        l.Height = Math.Abs(l.Y - m) * 2
                        l.Y -= l.Height
                    Else
                        l.Height = Math.Abs(l.Y - m) * 2
                    End If
                    g.FillRectangle(Brushes.DodgerBlue, l)
                Next
        End Select

        x = keyboardKeys.Max(Function(k) k.Bounds.Right) + 8
        y = 8
        For Each bp In am.BufferProviders
            If bp.Frequency <> 0 Then
                g.DrawString(bp.Note, largeFont, Brushes.OrangeRed, New Point(x, y))
                y += (Me.Font.Height + 4)
            End If
        Next
    End Sub
End Class