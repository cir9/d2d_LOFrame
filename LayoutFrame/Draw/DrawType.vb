Imports DX = SharpDX
Imports D2D = SharpDX.Direct2D1
Imports WIC = SharpDX.WIC
Imports DDW = SharpDX.DirectWrite
Imports DXGI = SharpDX.DXGI
Imports SharpDX.Mathematics.Interop
Imports SharpDX.Direct2D1.Effects
Imports D2W = D2D1Wrapper.D2D1Wrapper
Imports LayoutFrame


Public Structure Color4F
    Public R As Single
    Public G As Single
    Public B As Single
    Public A As Single
    Public Sub SetColor(r As Single, g As Single, b As Single, a As Single)
        Me.R = r
        Me.G = g
        Me.B = b
        Me.A = a
    End Sub
    Public Sub SetColor(color As Color4F)
        R = color.R
        G = color.G
        B = color.B
        A = color.A
    End Sub
    Public Sub SetColor(fromColor As Color4F, toColor As Color4F, t As Single)
        Dim nt As Single = 1.0F - t
        SetColor(fromColor * nt + toColor * t)
    End Sub
    Public Sub New(r As Single, g As Single, b As Single, a As Single)
        SetColor(r, g, b, a)
    End Sub

    Public Shared Operator *(c As Color4F, t As Single) As Color4F
        Return New Color4F(c.R * t, c.G * t, c.B * t, c.A * t)
    End Operator
    Public Shared Operator +(c As Color4F, d As Color4F) As Color4F
        Return New Color4F(c.R + d.R, c.G + d.G, c.B + d.B, c.A + d.A)
    End Operator
    Public Overrides Function ToString() As String
        Return String.Format("rgba({0:0},{1:0},{2:0},{3})", R * 255, G * 255, B * 255, A)
    End Function
End Structure
Public Class Conv
    Public Shared nullVector2 As New RawVector2(0F, 0F)
    Public Shared InfiniteRect As New RawRectangleF(-32768.0F, -32768.0F, 32768.0F, 32768.0F)
    Public Shared IdentityMatrix3x2 As New RawMatrix3x2(1.0F, 0F, 0F, 1.0F, 0F, 0F)
    Public Shared Function RawRect(r As Rect2F) As RawRectangleF
        Return New RawRectangleF(r.Left, r.Top, r.Right, r.Bottom)
    End Function
    Public Shared Function RawRect(r As Size2F) As RawRectangleF
        Return New RawRectangleF(0F, 0F, r.Width, r.Height)
    End Function
    Public Shared Function RawRect(r As Rect2F, border As Single) As RawRectangleF
        border *= 0.5F
        Return New RawRectangleF(r.Left + border, r.Top + border, r.Right - border, r.Bottom - border)
    End Function
    Public Shared Function RawColor4(c As Color4F) As RawColor4
        Return New RawColor4(c.R, c.G, c.B, c.A)
    End Function
    Public Shared Function RawColor4(c As Color4F, opacity As Single) As RawColor4
        Return New RawColor4(c.R, c.G, c.B, c.A * opacity)
    End Function

End Class

