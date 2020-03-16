Public Enum Orientation
    Horizontal = 0
    Vertical = 1
End Enum

Public Class Edge2F
    Public Property Right As Single
    Public Property Bottom As Single

    Public Sub SetEdge(b As BoxDim)
        Right = b.Right.RenderLength
        Bottom = b.Bottom.RenderLength
    End Sub
    Public Sub SetTransistionEdge(a As Edge2F, b As Edge2F, t As Single)
        Dim nt As Single = 1.0F - t
        Right = a.Right * nt + b.Right * t
        Bottom = a.Bottom * nt + b.Bottom * t
    End Sub
End Class

Public Structure FloatPair
    Public Property Beginning As Single
    Public Property Ending As Single

    Public ReadOnly Property Length As Single
        Get
            Return Ending - Beginning
        End Get
    End Property

    Public Sub New(f1 As Single, f2 As Single)
        Beginning = f1
        Ending = f2
    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("[{0},{1}]", Beginning, Ending)
    End Function
End Structure
Public Class Pair(Of T1, T2)
    Public Property First As T1
    Public Property Second As T2

    Public Sub New(v1 As T1, v2 As T2)
        First = v1
        Second = v2
    End Sub
    Public Overrides Function ToString() As String
        Return String.Format("[{0},{1}]", First, Second)
    End Function
End Class
Public Structure Size2F

    Private lengths() As Single
    Public Property Width As Single
        Get
            Return lengths(0)
        End Get
        Set(value As Single)
            lengths(0) = value
        End Set
    End Property
    Public Property Height As Single
        Get
            Return lengths(1)
        End Get
        Set(value As Single)
            lengths(1) = value
        End Set
    End Property

    Public ReadOnly Property Length(axis As Orientation) As Single
        Get
            Return lengths(axis)
        End Get
    End Property

    Public Sub New(w As Single, h As Single)
        lengths = {w, h}
    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("[{0},{1}]", Width, Height)
    End Function
End Structure

Public Class Rect2F

    Private pairs() As FloatPair = {New FloatPair, New FloatPair}
    Public Shared Infinity As New Rect2F() With {
        .Left = -32768.0F, .Top = -32768.0F, .Right = 32768.0F, .Bottom = 32768.0F
    }

    Public Property X As FloatPair
        Get
            Return pairs(0)
        End Get
        Set(value As FloatPair)
            pairs(0) = value
        End Set
    End Property
    Public Property Y As FloatPair
        Get
            Return pairs(1)
        End Get
        Set(value As FloatPair)
            pairs(1) = value
        End Set
    End Property

    Public Property Left As Single
        Get
            Return pairs(0).Beginning + DeltaX
        End Get
        Set(value As Single)
            pairs(0).Beginning = value
        End Set
    End Property
    Public Property Right As Single
        Get
            Return pairs(0).Ending + DeltaX
        End Get
        Set(value As Single)
            pairs(0).Ending = value
        End Set
    End Property
    Public Property Top As Single
        Get
            Return pairs(1).Beginning + DeltaY
        End Get
        Set(value As Single)
            pairs(1).Beginning = value
        End Set
    End Property
    Public Property Bottom As Single
        Get
            Return pairs(1).Ending + DeltaY
        End Get
        Set(value As Single)
            pairs(1).Ending = value
        End Set
    End Property

    Public ReadOnly Property OriginLeft As Single
        Get
            Return pairs(0).Beginning
        End Get
    End Property
    Public ReadOnly Property OriginRight As Single
        Get
            Return pairs(0).Ending
        End Get
    End Property
    Public ReadOnly Property OriginTop As Single
        Get
            Return pairs(1).Beginning
        End Get
    End Property
    Public ReadOnly Property OriginBottom As Single
        Get
            Return pairs(1).Ending
        End Get
    End Property

    Public Property DeltaX As Single
    Public Property DeltaY As Single

    Public ReadOnly Property Points() As Point2F()
        Get
            Return {New Point2F(Left, Top),
                    New Point2F(Left, Bottom),
                    New Point2F(Right, Top),
                    New Point2F(Right, Bottom)}
        End Get
    End Property

    Public ReadOnly Property Width As Single
        Get
            Return pairs(0).Ending - pairs(0).Beginning
        End Get
    End Property
    Public ReadOnly Property Height As Single
        Get
            Return pairs(1).Ending - pairs(1).Beginning
        End Get
    End Property

    Public ReadOnly Property Pair(axis As Orientation) As FloatPair
        Get
            Return pairs(axis)
        End Get
    End Property
    Public ReadOnly Property Size As Size2F
        Get
            Return New Size2F(pairs(0).Length, pairs(1).Length)
        End Get
    End Property

    Public Shared NullRect2F As Rect2F = DoublePointRect(0F, 0F, 0F, 0F)

    Public Overrides Function ToString() As String
        Return String.Format("[{0},{1},{2},{3}]", Left, Top, Right, Bottom)
    End Function

    Public Sub CopyRect(rect As Rect2F)
        Left = rect.Pair(0).Beginning
        Top = rect.Pair(1).Beginning
        Right = rect.Pair(0).Ending
        Bottom = rect.Pair(1).Ending
        DeltaX = rect.DeltaX
        DeltaY = rect.DeltaY
    End Sub


    Public Sub SetDelta(dX As Single, dY As Single)
        DeltaX = dX
        DeltaY = dY
    End Sub

    '为了防止滚动使用原始大小
    Public Sub SetClipInit(rect As Rect2F)
        SetDoublePointRect(rect.OriginRight, rect.OriginBottom, rect.OriginTop, rect.OriginLeft)
    End Sub
    Public Sub SetClippedClipInit(rect As Rect2F)
        SetDoublePointRect(rect.Right, rect.Bottom, rect.Left, rect.Top)
    End Sub

    Public Function SetRedrawClip(rect1 As Rect2F, rect2 As Rect2F)
        Left = MathF.Max(rect1.Left, rect2.Left)
        Top = MathF.Max(rect1.Top, rect2.Top)
        Right = MathF.Min(rect1.Right, rect2.Right)
        Bottom = MathF.Min(rect1.Bottom, rect2.Bottom)
        If Left > Right OrElse Top > Bottom Then Return False
        Return True
    End Function
    Public Sub RedrawClipForChildren(rX As Single, rY As Single)
        Left -= rX
        Top -= rY
        Right -= rX
        Bottom -= rY
    End Sub

    Public Function CheckIn(rX As Single, rY As Single, rect As Rect2F) As Boolean
        Return rect.Bottom + rY >= 0F AndAlso rect.Top + rY <= Height AndAlso
               rect.Right + rX >= 0F AndAlso rect.Left + rX <= Width
    End Function
    Public Function CheckIn(rX As Single, rY As Single, p() As Point2F) As Boolean
        Dim height As Single = Me.Height
        Dim width As Single = Me.Width
        For Each pt As Point2F In p
            If (pt.Y + rY >= 0F AndAlso pt.Y + rY <= height AndAlso
                pt.X + rX >= 0F AndAlso pt.X + rX <= width) Then
                Return True
            End If
        Next
        Return False
    End Function
    Public Function SetClipMax(rX As Single, rY As Single, rect As Rect2F) As Boolean

        Dim res As Boolean = True
        res = res And MathF.LetMax(Top, rect.Top + rY)
        res = res And MathF.LetMin(Bottom, rect.Bottom + rY)
        res = res And MathF.LetMax(Left, rect.Left + rX)
        res = res And MathF.LetMin(Right, rect.Right + rX)
        If Top > Bottom OrElse Left > Right Then
            Return False
        End If
        Return Not res
    End Function
    Public Function SetClipBound(rX As Single, rY As Single, rect As Rect2F) As Boolean
        Dim res As Boolean = False
        res = res Or MathF.LetMin(Top, Math.Min(rect.Top, rect.Bottom) + rY)
        res = res Or MathF.LetMax(Bottom, Math.Max(rect.Top, rect.Bottom) + rY)
        res = res Or MathF.LetMin(Left, Math.Min(rect.Left, rect.Right) + rX)
        res = res Or MathF.LetMax(Right, Math.Max(rect.Left, rect.Right) + rX)
        Return res
    End Function
    Public Function SetClipBound(rX As Single, rY As Single, p() As Point2F) As Boolean
        Dim res As Boolean = False
        Dim X() As Single = {p(0).X, p(1).X, p(2).X, p(3).X}
        Dim Y() As Single = {p(0).Y, p(1).Y, p(2).Y, p(3).Y}
        res = res Or MathF.LetMin(Left, MathF.Min(X) + rX)
        res = res Or MathF.LetMin(Top, MathF.Min(Y) + rY)
        res = res Or MathF.LetMax(Right, MathF.Max(X) + rX)
        res = res Or MathF.LetMax(Bottom, MathF.Max(Y) + rY)
        Return res
    End Function
    Public Sub SetWiderClip()
        Left += Math.Sign(Left - Right)
        Top += Math.Sign(Top - Bottom)
        Right += Math.Sign(Right - Left)
        Bottom += Math.Sign(Bottom - Top)
    End Sub
    Public Sub SetWiderClip(width As Single)
        Left += Math.Sign(Left - Right) * width
        Top += Math.Sign(Top - Bottom) * width
        Right += Math.Sign(Right - Left) * width
        Bottom += Math.Sign(Bottom - Top) * width
    End Sub
    '使用源大小
    Public Sub SetTransitionRect(fromRect As Rect2F, toRect As Rect2F, t As Single)
        Dim nt As Single = 1 - t
        Left = fromRect.OriginLeft * nt + toRect.OriginLeft * t
        Top = fromRect.OriginTop * nt + toRect.OriginTop * t
        Right = fromRect.OriginRight * nt + toRect.OriginRight * t
        Bottom = fromRect.OriginBottom * nt + toRect.OriginBottom * t
    End Sub
    Public Sub SetPointSizeRect(l As Single, t As Single, w As Single, h As Single)
        Left = l
        Top = t
        Right = l + w
        Bottom = t + h
    End Sub
    Public Sub SetDoublePointRect(l As Single, t As Single, r As Single, b As Single)
        Left = l
        Top = t
        Right = r
        Bottom = b
    End Sub
    Public Sub SetMarginRect(size As Size2F, l As Single, t As Single, r As Single, b As Single)
        Left = l
        Top = t
        Right = size.Width - r
        Bottom = size.Height - b
    End Sub
    Public Sub SetRelativeMarginRect(rect As Rect2F, l As Single, t As Single, r As Single, b As Single)
        Left = rect.Left + l
        Top = rect.Top + t
        Right = rect.Right - r
        Bottom = rect.Bottom - b
    End Sub

    Public Shared Function DoublePointRect(l As Single, t As Single, r As Single, b As Single) As Rect2F
        Return New Rect2F With {
            .Left = l,
            .Top = t,
            .Right = r,
            .Bottom = b
        }
    End Function
End Class
