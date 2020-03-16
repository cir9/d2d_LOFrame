
Imports System.Net
Imports System.IO
Imports System.Dynamic
Imports System.Threading

Public Class Downloads

    Public Shared Function Download(Path As String) As String
        Dim d As New Download(Path, Util.TempDir)
        d.Run()
        Return d.FilePath
    End Function

End Class

Public Class Download
    Public Property BufferSize As Integer = 4096
    Public Property FilePath As String
    Public Property FileName As String
    Public Property URL As String
    Public Property Referer As String
    Public Property FileSize As Integer
    Public Property CurSize As Integer
    ReadOnly Property Percent As Double
        Get
            If FileSize = 0 Then Return 0.0
            If CurSize > FileSize Then Return 1.0 Else Return CurSize / FileSize
        End Get
    End Property

    Public Sub Run(Optional allowCache As Boolean = True)
        If allowCache AndAlso File.Exists(FilePath) Then
        Else
            Download()
        End If
    End Sub
    Private Sub Download()

        Dim headers = New HeaderCollection("Referer", Referer)
        Dim resp As New Response("GET", URL, headers, Nothing, Nothing)
        Dim startTime As Date = Now
        Using st As New FileStream(FilePath, FileMode.Create)
            Using stRespones As Stream = resp.GetDownloadStream

                'Deb.Print("{0}", resp.StatusCode)
                Dim intCurSize As Integer
                Dim bytBuffer(BufferSize - 1) As Byte

                FileSize = resp.P.Headers("Content-Length")
                intCurSize = stRespones.Read(bytBuffer, 0, BufferSize)

                Do While (intCurSize > 0)
                    st.Write(bytBuffer, 0, intCurSize)
                    CurSize += intCurSize
                    intCurSize = stRespones.Read(bytBuffer, 0, BufferSize)
                Loop

                stRespones.Close()
            End Using

            st.Close()
        End Using
        resp.Close()
    End Sub

    Public Sub New(tURL As String, tPath As String, Optional tRef As String = "https://app-api.pixiv.net/")
        Dim URI As New Uri(tURL)
        Dim URISeg() As String = URI.Segments
        Dim truePath As String
        If tPath.Last <> "\"c Then truePath = tPath & "\" Else truePath = tPath
        FileName = URISeg(URISeg.GetUpperBound(0))
        FilePath = truePath & FileName
        URL = tURL
        Referer = tRef
        FileSize = 0
        CurSize = 0
    End Sub

End Class