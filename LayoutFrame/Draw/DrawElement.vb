Imports DX = SharpDX
Imports D2D = SharpDX.Direct2D1
Imports WIC = SharpDX.WIC
Imports DDW = SharpDX.DirectWrite
Imports DXGI = SharpDX.DXGI
Imports SharpDX.Mathematics.Interop
Imports LayoutFrame
Public Class OutlineCaption
    Inherits Caption
    Dim textRenderer As New OutlineTextRenderer
    Public OutlineBrush As New Brush
    Public Property StrokeWidth As Single
        Get
            Return textRenderer.StrokeWidth
        End Get
        Set(value As Single)
            textRenderer.StrokeWidth = value
        End Set
    End Property
    Public Property OutlineColor As Color4F
        Get
            Return OutlineBrush.Color
        End Get
        Set(value As Color4F)
            OutlineBrush.Color = value
            If Inited Then OutlineBrush.Update()
        End Set
    End Property
    Public Overrides Sub Init(father As Div)
        MyBase.Init(father)
        OutlineBrush.Init(r)
        textRenderer.r = r
    End Sub
    Public Overrides Sub Update(rel As Size2F)
        MyBase.Update(rel)
        textRenderer.FillBrush = ForeColorBrush
        textRenderer.OutlineBrush = OutlineBrush
    End Sub
    Public Overrides Sub Paint()
        PaintBackground()

        r.AddVector(Box.ReltX, Box.ReltY)

        TextLayout.Layout.Draw(textRenderer, 0F, 0F)

        r.SubVector(Box.ReltX, Box.ReltY)
    End Sub
End Class
Public Class Caption
    Inherits FastCaption
    Public Property TextLayout As New TextLayout
    Public Sub New()
        TextFormat = TextLayout
    End Sub
    Public Overrides Property Text As String
        Get
            Return TextLayout.Text
        End Get
        Set(value As String)
            TextLayout.Text = value
            TextLayout.Update()
        End Set
    End Property
    Public Overrides Sub Update(rel As Size2F)
        MyBase.Update(rel)
        textLayout.MaxWidth = Box.ContentArea.Width
        textLayout.MaxHeight = Box.ContentArea.Height
    End Sub
    Public Overrides Sub Paint()
        PaintBackground()
        r.AddVector(Box.ReltX, Box.ReltY)
        ForeColorBrush.DrawTextLayout(TextLayout.Layout)
        r.SubVector(Box.ReltX, Box.ReltY)
    End Sub
End Class
Public Class FastCaption
    Inherits Element

    Public ForeColorBrush As New Brush

    Public Property TextFormat As TextFormat
    Public Overridable Property Text As String
    Public Property ForeColor As Color4F
        Get
            Return ForeColorBrush.Color
        End Get
        Set(value As Color4F)
            ForeColorBrush.Color = value
            If Inited Then ForeColorBrush.Update()
        End Set
    End Property

    Public Sub New()
        TextFormat = New TextFormat
    End Sub
    Public Overrides Sub UpdateBoundClip()
        If BackgroundBrush.WillDraw OrElse BorderBrush.WillDraw Then
            UpdateBoundClipOfRect(Box.RenderArea)
        Else
            RecieveBoundClip(Box.RenderArea.Left, Box.RenderArea.Top, Box.ContentArea)
        End If
    End Sub

    Public Overrides Sub Init(father As Div)
        MyBase.Init(father)
        ForeColorBrush.Init(r)
        TextFormat.Update()
    End Sub
    Public Overrides Sub Paint()
        MyBase.Paint()
        r.AddVector(Box.ReltX, Box.ReltY)
        ForeColorBrush.DrawText(Text, TextFormat.Format, Box.ContentArea)
        r.SubVector(Box.ReltX, Box.ReltY)
    End Sub

End Class
Public Class Picture
    Inherits Element

    Private image As D2D.Bitmap

    Private RenderArea As New Rect2F

    Private _imagePath As String
    Public Property ImagePath As String
        Get
            Return _imagePath
        End Get
        Set(value As String)
            _imagePath = value
            LoadImage(value)
        End Set
    End Property
    Private Async Sub LoadImage(path As String)
        Await Task.Run(Sub() image = ResourceLoader.LoadBitmap(FactoryUtil.D2DContext, path, 0))
        Debug.Print("image loaded!")
        SyncLock ThreadSafety.RenderLock
            UpdateBoundClip()
        End SyncLock
    End Sub

    Public Overrides Sub UpdateBoundClip()
        'Debug.Print(Box.RenderArea.ToString)
        MyBase.UpdateBoundClip()
        Resize(Box.ContentArea.Size)
        'RecieveBoundClip(RenderArea)
    End Sub
    Public Overrides Sub Paint()
        MyBase.Paint()
        If image IsNot Nothing Then
            r.AddVector(Box.ReltX, Box.ReltY)
            r.R.DrawBitmap(image, Conv.RawRect(RenderArea), 1.0F, 1)
            r.SubVector(Box.ReltX, Box.ReltY)
        End If
    End Sub

    Public Sub Resize(rel As Size2F)
        If image IsNot Nothing Then
            Dim rWidth As Single, rHeight As Single
            Dim dVal As Single

            rWidth = rel.Width
            rHeight = rel.Height
            If rWidth * image.Size.Height < rHeight * image.Size.Width Then
                dVal = (rHeight - rWidth / image.Size.Width * image.Size.Height) / 2
                RenderArea.SetMarginRect(rel, 0F, dVal, 0F, dVal)
            Else
                dVal = (rWidth - rHeight / image.Size.Height * image.Size.Width) / 2
                RenderArea.SetMarginRect(rel, dVal, 0F, dVal, 0F)
            End If
        End If
    End Sub
End Class

Public Class Page
    Inherits Div
    Public Overrides Sub UpdateChildBoundClip()
        MyBase.UpdateChildBoundClip()
        r.Latest = False
    End Sub
    Public Overrides Sub Paint()
        MyBase.Paint()

    End Sub


    Public Sub UpdateAll()
        Update(r.RenderArea.Size)
        UpdateBoundClip()
    End Sub
    Public Overridable Sub Render()
        If Not r.Latest Then
            RedrawClip.CopyRect(r.RedrawClip)
            r.BeginDraw()
            BeginPaint()
            r.EndDraw(Box.RenderArea)
        End If
    End Sub
End Class
