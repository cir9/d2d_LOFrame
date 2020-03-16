Imports DX = SharpDX
Imports D2D = SharpDX.Direct2D1
Imports WIC = SharpDX.WIC
Imports DDW = SharpDX.DirectWrite
Imports DXGI = SharpDX.DXGI
Imports D3D = SharpDX.Direct3D11
Imports SharpDX.Mathematics.Interop

Imports System.Text.RegularExpressions

Public Class ThreadSafety
    Public Shared RenderLock As New ThreadSafety
End Class

Public Class ResourceLoader
    Public Shared ImagingFactory As New WIC.ImagingFactory
    Public Shared Function LoadBitmap(Render As D2D.RenderTarget, File As String, FrameIndex As Integer) As D2D.Bitmap
        If Util.IsUrl(File) Then File = Downloads.Download(File)

        SyncLock ThreadSafety.RenderLock
            Dim Decoder As New WIC.BitmapDecoder(ImagingFactory, File, DX.IO.NativeFileAccess.Read, WIC.DecodeOptions.CacheOnLoad)

            If FrameIndex > Decoder.FrameCount - 1 OrElse FrameIndex < 0 Then FrameIndex = 0

            Dim Source As WIC.BitmapFrameDecode = Decoder.GetFrame(FrameIndex)

            Dim Converter As New WIC.FormatConverter(ImagingFactory)
            Converter.Initialize(Source, WIC.PixelFormat.Format32bppPBGRA)

            Return D2D.Bitmap.FromWicBitmap(Render, Converter)
        End SyncLock
    End Function
End Class

Public Class FactoryUtil
    Public Shared DDWFactory As DDW.Factory
    Public Shared D2DFactory As D2D.Factory
    Public Shared D2DContext As D2D.DeviceContext
    Shared Sub CreateIndependentResource()
        DDWFactory = New DDW.Factory
        D2DFactory = New D2D.Factory
    End Sub
    Shared Sub CreateDependentResource(r As D2D.RenderTarget)
        D2DContext = r.QueryInterface(Of D2D.DeviceContext)
    End Sub
End Class


Public Class RenderUtil
    Public Shared Now As Single

    Public Shared Todos As New Queue(Of Action)
    Public Shared Sub Init()
        Timing.Init()
    End Sub

    Public Shared Sub AddTodo(s As Action)
        SyncLock Todos
            Todos.Enqueue(s)
        End SyncLock
    End Sub
    Public Shared Sub NewFrame()
        SyncLock Todos
            Do While Todos.Count > 0
                Dim act As Action = Todos.Dequeue
                act()
            Loop
        End SyncLock
        Now = Timing.Timer
        AnimationUtil.Run()
    End Sub
End Class

Public Class Factory
    Inherits Page

    Protected RenderTarget As D2D.WindowRenderTarget

    Protected transWidth As Single, transHeight As Single
    Public Overrides Sub Render()
        SyncLock ThreadSafety.RenderLock
            RenderUtil.NewFrame()
            'Debug.Print("Animation Complete")
            MyBase.Render()
        End SyncLock
    End Sub

    Private _redrawClip As New Rect2F
    Public Sub Redraw(clip As Rectangle)
        SyncLock ThreadSafety.RenderLock
            _redrawClip.SetDoublePointRect(clip.Left * transWidth, clip.Top * transHeight,
                                           clip.Right * transWidth, clip.Bottom * transHeight)
            RecieveBoundClip(_redrawClip)
        End SyncLock
    End Sub

    Public Sub New(Target As Control)

        FactoryUtil.CreateIndependentResource()

        Dim P As New D2D.PixelFormat(DXGI.Format.B8G8R8A8_UNorm, D2D.AlphaMode.Ignore)

        Dim H As New D2D.HwndRenderTargetProperties With {
            .Hwnd = Target.Handle,
            .PixelSize = New DX.Size2(Target.Width, Target.Height),
            .PresentOptions = D2D.PresentOptions.None
        }

        Name = "FACTORY"

        Dim RP As New D2D.RenderTargetProperties(D2D.RenderTargetType.Hardware,
                                                                  P, 0, 0,
                                                                  D2D.RenderTargetUsage.None,
                                                                  D2D.FeatureLevel.Level_DEFAULT)

        RenderTarget = New D2D.WindowRenderTarget(FactoryUtil.D2DFactory, RP, H)

        FactoryUtil.CreateDependentResource(RenderTarget)


        r = New Renderer(RenderTarget)

        boundBox = r.RedrawClip
        Dim vW As Single = RenderTarget.Size.Width
        Dim vH As Single = RenderTarget.Size.Height
        Measures.ViewWidth = vW
        Measures.ViewHeight = vH

        transWidth = vW / RenderTarget.PixelSize.Width
        transHeight = vH / RenderTarget.PixelSize.Height

        Box.Width = New AbsoluteMeasure(vW)
        Box.Height = New AbsoluteMeasure(vH)

        'Overflow = False

        Update(Rect2F.DoublePointRect(0F, 0F, r.R.Size.Width, r.R.Size.Height).Size)

        RenderUtil.Init()
    End Sub

End Class
