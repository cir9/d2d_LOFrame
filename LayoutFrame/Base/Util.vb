Imports System.Text.RegularExpressions

Public Class Timing
    Private Shared startTime As Date
    Public Shared Sub Init()
        startTime = Now
    End Sub
    Public Shared ReadOnly Property Timer As Double
        Get
            Return (Now - startTime).TotalMilliseconds
        End Get
    End Property
End Class
Public Class Util
    Private Shared regex_isUrl As New Regex("^https?://.+")

    Public Shared TempDir As String = CheckDir(Application.StartupPath & "\Temp\")

    Public Shared Function IsUrl(path As String) As Boolean
        Return regex_isUrl.IsMatch(path)
    End Function

    Public Shared Function CheckDir(path As String) As String
        If Not My.Computer.FileSystem.DirectoryExists(path) Then
            My.Computer.FileSystem.CreateDirectory(path)
        End If
        Return path
    End Function
End Class
