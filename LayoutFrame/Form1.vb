
Imports System.Text.RegularExpressions

Public Class Form1

    Dim reg As New Regex("^https?://.+")
    Dim f As Factory
    Dim s As Stack
    Dim p As Element, b As Div
    Dim sc As Div
    Dim title As Caption
    Dim div As Div
    Dim runThread As Threading.Thread

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        f = New Factory(Panel1)

        s = New Stack(New StackBoxes(Orientation.Vertical, Measures.ParseMany(
                                     "10% *"))) With {
            .Box = New Box("0", "0", "* *"),
            .Name = "HeadBodyDivider",
            .Father = f
        }

        Dim head As Div
        head = New Div With {
            .Box = New Box("0", "5px 20px 5px 5px", "* *"),
            .BackgroundColor = New Color4F(1.0F, 1.0F, 1.0F, 1.0F),
            .Name = "Head",
            .Father = s(0)
        }

        title = New Caption() With {
            .Box = New Box("0 *", "0", "115%vh *"),
            .Text = "FixedTextCaption",
            .Name = "Title",
            .ForeColor = New Color4F(0.1F, 0.1F, 0.7F, 1.0F),
            .Father = head
        }


        div = New Div() With {
            .Box = New Box("0", "0.05h 0", "* *"),
            .Name = "BodyDiv",
            .BackgroundColor = New Color4F(0.9F, 0.9F, 0.9F, 1.0F),
            .Father = s(1)
        }

        p = New Element() With {
            .Box = New Box("0 20px", "0", "50px 50px"),
            .Name = "Picture",
            .BorderWidth = 1.5F,
            .BorderColor = New Color4F(0F, 0.8F, 1.0F, 0F),
            .BackgroundColor = New Color4F(0.95F, 0.95F, 0.95F, 1.0F)
        }

        Dim rs(4) As Element
        For i As Integer = 0 To 4
            rs(i) = New Element() With {
                    .Box = New Box("0", "0", "150px 50px"),
                    .BackgroundColor = New Color4F(Rnd, Rnd, Rnd, 1.0F),
                    .Name = "TestRect"
                }
        Next


        b = New Flow() With {
            .Box = New Box("0 *", "20px", "115%vh *"),
            .Name = "Bar",
            .BackgroundColor = New Color4F(1.0F, 1.0F, 1.0F, 1.0F),
            .Overflow = False,
            .Father = div,
            .Contains = {
                rs(0), rs(1),' rs(2),
                p, 'rs(3), rs(4),
                New Element() With {
                    .Box = New Box("0", "0", "50px 50px"),
                    .BackgroundColor = New Color4F(0F, 0F, 0.5F, 1.0F),
                    .Name = "TestRect"
                }
            }
        }



        f.UpdateAll()

        runThread = New Threading.Thread(AddressOf Render) With {.IsBackground = True}
        runThread.Start()
    End Sub


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Static times As Integer
        SyncLock ThreadSafety.RenderLock
            times = 1 - times
            If times = 1 Then
                RenderUtil.AddTodo(
                    Sub()
                        AnimationUtil.Start(p, {
                            New BoxAnimation(2000.0F, Ease.Quart.EaseOut,
                                             New Box("0 *", "0", "300px 400px")),
                            New ColorAnimation(600.0F, Ease.Quart.EaseOut,
                                               New Color4F(0F, 0.8F, 1.0F, 1.0F)) With
                                               {.Brush = p.BorderBrush},
                            New ColorAnimation(600.0F, Ease.Quart.EaseOut,
                                               New Color4F(1.0F, 1.0F, 1.0F, 0F)) With
                                               {.Brush = p.BackgroundBrush}
                        })
                    End Sub)
            Else
                RenderUtil.AddTodo(
                    Sub()
                        AnimationUtil.Start(p, {
                            New BoxAnimation(2000.0F, Ease.Quart.EaseOut,
                                             New Box("0 20px", "0", "50px 50px")),
                            New ColorAnimation(600.0F, Ease.Quart.EaseIn,
                                               New Color4F(0F, 0.8F, 1.0F, 0F)) With
                                               {.Brush = p.BorderBrush},
                            New ColorAnimation(600.0F, Ease.Quart.EaseIn,
                                               New Color4F(0.95F, 0.95F, 0.95F, 1.0F)) With
                                               {.Brush = p.BackgroundBrush}
                        })
                    End Sub)
            End If
        End SyncLock

        'TextBox1.Focus()
        'Label1.Text = reg.IsMatch(TextBox1.Text)

    End Sub

    Sub Render()
        Dim sig As Single = 1000.0F / 240.0F
        Do
            f.Render()
            'Debug.Print(p.Box.RenderArea.DeltaX & ", " & p.Box.RenderArea.DeltaY)
            'Debug.Print(b.Box.RenderArea.Height & " / " & div.ScrollHeight)
            Threading.Thread.Sleep(sig)
        Loop
    End Sub

    Private Sub Panel1_Paint(sender As Object, e As PaintEventArgs) Handles Panel1.Paint
        f.Redraw(e.ClipRectangle)
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged

    End Sub

    Private Sub TrackBar1_Scroll(sender As Object, e As EventArgs) Handles TrackBar1.Scroll
        Dim u As Single = TrackBar1.Value / TrackBar1.Maximum * b.ScrollAreaHeight + b.ScrollTop
        RenderUtil.AddTodo(
            Sub() b.ScrollY = u)
    End Sub
End Class
