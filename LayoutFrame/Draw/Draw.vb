Imports DX = SharpDX
Imports D2D = SharpDX.Direct2D1
Imports WIC = SharpDX.WIC
Imports DDW = SharpDX.DirectWrite
Imports DXGI = SharpDX.DXGI
Imports SharpDX.Mathematics.Interop
Imports SharpDX.Direct2D1.Effects
Imports D2W = D2D1Wrapper.D2D1Wrapper
Imports LayoutFrame
Imports SharpDX
Imports SharpDX.Direct2D1
Imports SharpDX.DirectWrite
Public Class TextLayout
    Inherits TextFormat


    Protected Friend Layout As DDW.TextLayout


    Public Property MaxWidth As Single
        Get
            Return Layout.MaxWidth
        End Get
        Set(value As Single)
            Layout.MaxWidth = value
        End Set
    End Property
    Public Property MaxHeight As Single
        Get
            Return Layout.MaxHeight
        End Get
        Set(value As Single)
            Layout.MaxHeight = value
        End Set
    End Property
    Public Property Text As String
    Public Overrides Sub Update()
        MyBase.Update()
        Layout = New DDW.TextLayout(FactoryUtil.DDWFactory, Text, Format, 100.0F, 100.0F)
    End Sub

End Class

Public Class TextFormat
    Protected Friend Format As DDW.TextFormat
    Public Property FontFamily As String = "exo 2.0"
    Public Property FontSize As Single = 15.0F
    Public Overridable Sub Update()
        Format = New DDW.TextFormat(FactoryUtil.DDWFactory, FontFamily, FontSize) With {
            .ParagraphAlignment = DDW.ParagraphAlignment.Center
        }
    End Sub
End Class

Public Class OutlineTextRenderer
    Implements DDW.TextRenderer

    Public r As Renderer
    Public OutlineBrush As Brush
    Public FillBrush As Brush
    Public Property StrokeWidth As Single

    Public Property Shadow As IDisposable Implements ICallbackable.Shadow


    Public Function DrawGlyphRun(clientDrawingContext As Object,
                                 baselineOriginX As Single, baselineOriginY As Single,
                                 measuringMode As MeasuringMode,
                                 glyphRun As GlyphRun, glyphRunDescription As GlyphRunDescription,
                                 clientDrawingEffect As ComObject) As Result Implements TextRenderer.DrawGlyphRun
        Using pathGeometry As New D2D.PathGeometry(FactoryUtil.D2DFactory)
            Using sink As D2D.GeometrySink = pathGeometry.Open()
                glyphRun.FontFace.GetGlyphRunOutline(
                glyphRun.FontSize,
                glyphRun.Indices,
                glyphRun.Advances,
                glyphRun.Offsets,
                glyphRun.IsSideways,
                False,
                sink)
                sink.Close()
                Dim m As New RawMatrix3x2(1.0F, 0F, 0F, 1.0F, baselineOriginX, baselineOriginY)
                Using transformedGeometry As New D2D.TransformedGeometry(FactoryUtil.D2DFactory,
                                                               pathGeometry,
                                                               m)

                    r.R.DrawGeometry(transformedGeometry, OutlineBrush.brush, StrokeWidth)
                    r.R.FillGeometry(transformedGeometry, FillBrush.brush)
                End Using
            End Using
        End Using
    End Function

    Public Function DrawUnderline(clientDrawingContext As Object, baselineOriginX As Single, baselineOriginY As Single, ByRef underline As Underline, clientDrawingEffect As ComObject) As Result Implements TextRenderer.DrawUnderline
        Throw New NotImplementedException()
    End Function

    Public Function DrawStrikethrough(clientDrawingContext As Object, baselineOriginX As Single, baselineOriginY As Single, ByRef strikethrough As Strikethrough, clientDrawingEffect As ComObject) As Result Implements TextRenderer.DrawStrikethrough
        Throw New NotImplementedException()
    End Function

    Public Function DrawInlineObject(clientDrawingContext As Object, originX As Single, originY As Single, inlineObject As InlineObject, isSideways As Boolean, isRightToLeft As Boolean, clientDrawingEffect As ComObject) As Result Implements TextRenderer.DrawInlineObject
        Throw New NotImplementedException()
    End Function

    Public Function IsPixelSnappingDisabled(clientDrawingContext As Object) As Boolean Implements PixelSnapping.IsPixelSnappingDisabled
        Return False
    End Function

    Public Function GetCurrentTransform(clientDrawingContext As Object) As RawMatrix3x2 Implements PixelSnapping.GetCurrentTransform
        Return r.R.Transform
    End Function

    Public Function GetPixelsPerDip(clientDrawingContext As Object) As Single Implements PixelSnapping.GetPixelsPerDip
        Return r.R.DotsPerInch.Width / 96
        'Throw New NotImplementedException()
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 要检测冗余调用

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: 释放托管状态(托管对象)。
            End If
            Shadow.Dispose()
            ' TODO: 释放未托管资源(未托管对象)并在以下内容中替代 Finalize()。
            ' TODO: 将大型字段设置为 null。
        End If
        disposedValue = True
    End Sub

    ' TODO: 仅当以上 Dispose(disposing As Boolean)拥有用于释放未托管资源的代码时才替代 Finalize()。
    'Protected Overrides Sub Finalize()
    '    ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' Visual Basic 添加此代码以正确实现可释放模式。
    Public Sub Dispose() Implements IDisposable.Dispose
        ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
        Dispose(True)
        ' TODO: 如果在以上内容中替代了 Finalize()，则取消注释以下行。
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
Public Class Brush
    Friend r As Renderer
    Friend brush As D2D.Brush
    Public Color As New Color4F

    Public ReadOnly Property WillDraw As Boolean
        Get
            Return Color.A > 0F
        End Get
    End Property
    Public Sub SetColor(r As Single, g As Single, b As Single, a As Single)
        Color.SetColor(r, g, b, a)
        Update()
    End Sub
    Public Sub SetColor(tColor As Color4F)
        Color.SetColor(tColor)
        Update()
    End Sub
    Public Sub SetColor(fromColor As Color4F, toColor As Color4F, t As Single)
        Dim nt As Single = 1.0F - t
        Color.SetColor(fromColor * nt + toColor * t)
        Update()
    End Sub
    Public Sub Init(rb As Renderer)
        r = rb
        Update()
    End Sub
    Public Sub Update()
        If WillDraw Then
            If brush IsNot Nothing Then brush.Dispose()
            brush = New D2D.SolidColorBrush(r.R, Conv.RawColor4(Color))
        End If
    End Sub


    Public Sub DrawTextLayout(textLayout As DDW.TextLayout)
        r.R.DrawTextLayout(Conv.nullVector2, textLayout, brush)
    End Sub
    Public Sub DrawText(text As String, format As DDW.TextFormat, rel As Rect2F)
        r.R.DrawText(text, format, Conv.RawRect(rel), brush)
    End Sub
    Public Sub FillRectangle(rect As Rect2F)
        If WillDraw Then
            r.R.FillRectangle(Conv.RawRect(rect), brush)
        End If
    End Sub
    Public Sub DrawRectangle(rect As Rect2F, width As Single)
        If WillDraw Then
            r.R.DrawRectangle(Conv.RawRect(rect, width), brush, width)
        End If
    End Sub
    Public Sub FillRoundedRectangle(rect As Rect2F, radius As Single)
        If WillDraw Then
            Dim rr As New D2D.RoundedRectangle() With {
                         .Rect = Conv.RawRect(rect),
                         .RadiusX = radius,
                         .RadiusY = radius}
            D2W.FillRoundedRectangle(r.R, rr, brush)
        End If
    End Sub
    Public Sub DrawRoundedRectangle(rect As Rect2F, radius As Single, width As Single)
        If WillDraw Then
            radius -= width * 0.5F
            Dim rr As New D2D.RoundedRectangle() With {
                         .Rect = Conv.RawRect(rect, width * 0.5F),
                         .RadiusX = radius,
                         .RadiusY = radius}
            r.R.DrawRoundedRectangle(rr, brush, width)
        End If
    End Sub
End Class
Public Class Renderer
    Public Shared Transparent As New RawColor4(0F, 0F, 0F, 0F)

    Public R As D2D.RenderTarget

    Public RedrawClip As New Rect2F
    Public RenderArea As New Rect2F

    Public Property Latest As Boolean = False
    Public Sub New(rt As D2D.RenderTarget)
        R = rt
        RenderArea.SetPointSizeRect(0, 0, R.Size.Width, R.Size.Height)
        debugBrush = New D2D.SolidColorBrush(R, New RawColor4(0F, 1.0F, 0F, 0.2F))
        debugBrush2 = New D2D.SolidColorBrush(R, New RawColor4(0F, 0F, 0.8F, 1.0F))
        debugBrush3 = New D2D.SolidColorBrush(R, New RawColor4(0F, 0F, 0.8F, 0.2F))
        debugBrush4 = New D2D.SolidColorBrush(R, New RawColor4(0F, 0.8F, 0F, 0.4F))
    End Sub
#Region "<Matrix>"

    Public Property Matrix As Matrix3x2
        Get
            Return R.Transform
        End Get
        Set(value As Matrix3x2)
            R.Transform = value
        End Set
    End Property

    Public Sub AddVector(X As Single, Y As Single)
        Dim m As RawMatrix3x2 = R.Transform
        R.Transform = New Matrix3x2(m.M11, m.M12,
                                    m.M21, m.M22,
                                    m.M31 + m.M11 * X + m.M21 * Y, m.M32 + m.M12 * X + m.M22 * Y)
    End Sub
    Public Sub SubVector(X As Single, Y As Single)
        Dim m As RawMatrix3x2 = R.Transform
        R.Transform = New Matrix3x2(m.M11, m.M12,
                                    m.M21, m.M22,
                                    m.M31 - m.M11 * X - m.M21 * Y, m.M32 - m.M12 * X - m.M22 * Y)
    End Sub

#End Region
#Region "<Layer>"

    Private layers As New List(Of D2D.Layer)
    Private layerCount As Single
    Public Sub PushLayer(opa As Single)
        layerCount += 1
        If layerCount > layers.Count Then layers.Add(New D2D.Layer(R))
        R.PushLayer(New D2D.LayerParameters() With {
                        .ContentBounds = Conv.InfiniteRect,
                        .Opacity = opa
                    }, layers(layerCount - 1))
    End Sub
    Public Sub PushLayer(rel As Rect2F, opa As Single)
        layerCount += 1
        If layerCount > layers.Count Then layers.Add(New D2D.Layer(R))
        R.PushLayer(New D2D.LayerParameters() With {
                        .ContentBounds = Conv.RawRect(rel),
                        .Opacity = opa
                    }, layers(layerCount - 1))
    End Sub
    Public Sub PopLayer()
        layerCount -= 1
        R.PopLayer()
    End Sub
#End Region
#Region "<BoundClip>"
    Public Sub PushClip()
        R.PushAxisAlignedClip(Conv.RawRect(RedrawClip), D2D.AntialiasMode.Aliased)
    End Sub
    Public Sub PushClip(rect As Rect2F)
        R.PushAxisAlignedClip(Conv.RawRect(rect), D2D.AntialiasMode.Aliased)
    End Sub
    Public Sub PopClip()
        R.PopAxisAlignedClip()
    End Sub

#End Region
#Region "<Draw>"
    Dim useClip As RawRectangleF
    Public Sub BeginDraw()
        RedrawClip.SetWiderClip()
        useClip = Conv.RawRect(RedrawClip)
        'Debug.Print("----" & RedrawClip.ToString)
        R.Transform = Conv.IdentityMatrix3x2
        R.BeginDraw()
        PushClip()
        R.Clear(Transparent)
    End Sub
    Public Sub EndDraw(rel As Rect2F)
        DrawDebug(useClip)

        PopClip()
        R.EndDraw()
        'Debug.Print("-------Frame-------")
        Latest = True
    End Sub
#End Region
#Region "<Debug>"

    Private debugBrush As D2D.Brush
    Private debugBrush2 As D2D.Brush
    Private debugBrush3 As D2D.Brush
    Private debugBrush4 As D2D.Brush
    Private debugFormat As New DDW.TextFormat(FactoryUtil.DDWFactory, "exo 2.0", 10.0F)

    Public Sub DrawDebugText3(text As String, rect As Rect2F)
        R.DrawText(text, debugFormat, Conv.RawRect(rect), debugBrush3)
    End Sub
    Public Sub DrawDebugText4(text As String, rect As Rect2F)
        R.DrawText(text, debugFormat, Conv.RawRect(rect), debugBrush4)
    End Sub
    Public Sub DrawDebug(rect As Rect2F)
        R.DrawRectangle(Conv.RawRect(rect), debugBrush, 4.0F)
    End Sub
    Public Sub DrawDebug(rect As RawRectangleF)
        Static count As Integer
        count = 1 - count
        If count = 1 Then
            R.DrawRectangle(rect, debugBrush, 4.0F)
        Else
            R.DrawRectangle(rect, debugBrush3, 4.0F)
        End If
    End Sub
    Public Sub DrawDebug2(rect As Rect2F)
        R.DrawRectangle(Conv.RawRect(rect), debugBrush2, 2.0F)
    End Sub
    Public Sub DrawDebug2(rect As RawRectangleF)
        R.DrawRectangle(rect, debugBrush2, 2.0F)
    End Sub
    Public Sub DrawDebug3(rect As Rect2F)
        R.DrawRectangle(Conv.RawRect(rect), debugBrush3, 2.0F)
    End Sub
    Public Sub DrawDebug4(rect As Rect2F)
        R.DrawRectangle(Conv.RawRect(rect), debugBrush3, 2.0F)
    End Sub
#End Region

End Class

