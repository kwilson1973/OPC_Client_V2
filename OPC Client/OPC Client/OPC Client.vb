Imports OPCAutomation
Imports Microsoft.VisualBasic
Imports SYS
Imports System.IO



Public Class Form2
    Public MyOPCServers As New OPCServer
    Public MyOPCServer(10) As strOPCServer
    Public WithEvents MyOPCGroups As OPCGroup
    Public WithEvents MyOPCGroup_0 As OPCGroup
    Public WithEvents MyOPCGroup_1 As OPCGroup
    Public WithEvents MyOPCGroup_2 As OPCGroup
    Public WithEvents MyOPCGroup_3 As OPCGroup
    Public MyOPCGroup(10) As OPCGroup



    Public iOPC As Integer
    Public MonItem_Count As Integer = 0
    Public MonItem_Name As New List(Of String)
    Public MonItem_Value As New List(Of VariantType)


    'Public objStreamWriter As StreamWriter                      '輸出文字檔
    Public file As System.IO.StreamWriter                      '輸出文字檔
    Public sOutTXTLine As String                                '放寫入文字檔字串
    Public sOutTXTFileName As String                            '輸出文字檔名
    Public sOutTXTLineCount As Integer                          '計算文字行數

    '20201012 輸出LOG
    Public Logfile As System.IO.StreamWriter                     '輸出Log檔
    Public sOutLogLine As String                                '放寫入Log檔字串
    Public sOutLogFileName As String                            '輸出Log檔名
    Public sOutLogDIR As String                                 '輸出Log位置
    Public iRefreshOutLogCountDown As Integer = 0               'Log檔案輸出更新時間
    Public iRefreshOutLogInterval As Integer = 60               '60秒開關log一次
    Public sLogMsg As String

    Structure strOPCServer
        Dim MyServer As OPCServer
        Dim MyGroup
        Dim MyItem As OPCItem
        Dim ServerHandles As Array
        Dim Errors As Array
        Dim Values As Array
        Dim MonCount As Integer
    End Structure

    Structure strcMonItem
        Dim _Server As Integer
        Dim _Name As String
        Dim _Value
        Dim _Index As Integer
        Dim _ServerHandles
    End Structure

    Public MonItem() As strcMonItem
    Public ItemNameArray(10000, 2) As String            '存ini 讀入要監控的ItemName
    Public dt As New System.Data.DataTable              '車站 月台 車號 到離站資訊
    Public dt_Number As New System.Data.DataTable       '車號對應表    
    'Public dt_Segment As New System.Data.DataTable      '
    Public dt_TID As New System.Data.DataTable          'Segment 與車號對應表
    Public dt_TrainInit As New System.Data.DataTable    '列車初始化 20210514


    '讀ini
    Public OPCItemCount, UpdateRate As Integer
    Public LogFilepath, sOutDIR, Debug As String
    Public iRefreshOutCountDown As Integer = 10 '檔案輸出更新時間
    Public iRefreshOutInterval As Integer = 10 '檔案輸出更新時間

    '放原始tag狀態 
    Public sMsg As String
    Public NowTime As String

    'OPCC 輸出時間 每60秒一次
    Public iRefreshOpcCCountDown As Integer = 10
    Public iRefreshOpcCInterval As Integer = 60
    Dim bOpcCOut As Boolean = True





    '測試模式使用陣列
    Public TestArray(800) As String
    Public testCountDown As Integer = 0
    Public testInterval As Integer = 800

    'Public sOPCCmsg As String





    Private Sub Form2_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load


        Timer1.Enabled = True
        txtServerIP.Text = "127.0.0.1"
        txtServerName.Text = "ICONICS.SimulatorOPCDA.2"

        dataOPC.Columns(0).Width = 400
        dataOPC.Enabled = True

        'sOPCCmsg = ""




        'DataGridView 環境設定
        DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
        DataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
        DataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
        DataGridView1.RowHeadersVisible = False                                         '去除左邊ROW
        DataGridView2.RowHeadersVisible = False
        DataGridView3.RowHeadersVisible = False

        btnConnect.Enabled = True
        btnMonitoringOn.Enabled = False  'OPC 為連結前Monitor鍵反白
        btnDisconnect.Enabled = False    'OPC 為連結前Disconnect鍵反白


        sOutLogDIR = Application.StartupPath '20201012 儲存LOG 路徑


        '讀取INI
        InitialParam()
        INI_dt()
        INI_dt_Nunber()
        INI_dt_TID()

        TestMode()


        '20201012 開啟log記錄檔
        If Debug = "1" Then
            OpenOutLogFile()
        End If

        '開啟記錄檔
        OpenOutTextFile()
        sLogMsg = Format(Now, "yyyy/MM/dd HH:mm:ss") & "  " & "Initialization successfully completed"
        ListBox1.Items.Add(sLogMsg)
        If Debug = "1" Then Logfile.WriteLine(sLogMsg)



        For i = 0 To 10 '存不同的Server 目前只用到1個
            MyOPCServer(i).MyServer = New OPCServer
            Dim Dims() As Integer = New Integer() {10000}
            Dim Bounds() As Integer = New Integer() {1}
            MyOPCServer(i).ServerHandles = Array.CreateInstance(GetType(Integer), Dims, Bounds)
            MyOPCServer(i).Errors = Array.CreateInstance(GetType(Integer), Dims, Bounds)
            MyOPCServer(i).Values = Array.CreateInstance(GetType(Object), Dims, Bounds)
        Next
    End Sub
    Private Sub Timer1_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles Timer1.Tick

        NowTime = Format(Now, "yyyy/MM/dd HH:mm:ss") & "  "
        iRefreshOutCountDown = iRefreshOutCountDown - 1         '輸出文字檔間隔
        iRefreshOutLogCountDown = iRefreshOutLogCountDown - 1   'log 輸出間隔
        If Debug = "1" Then
            Me.Text = "OPC Client 20210514_" & iRefreshOutCountDown & "  (除錯模式)"
        Else
            Me.Text = "OPC Client 20210514_" & iRefreshOutCountDown
        End If

        '開關輸出檔案
        If iRefreshOutCountDown <= 0 Then
            SendTrainLocation()
            file.Close()
            OpenOutTextFile()
            dataGridViewUpdate()
            iRefreshOutCountDown = iRefreshOutInterval
        End If

        '開關Log輸出檔案
        If iRefreshOutLogCountDown <= 0 And Debug = "1" Then
            Logfile.Close()
            OpenOutLogFile()
            iRefreshOutLogCountDown = iRefreshOutLogInterval
        End If

        '輸出OpcC 每60秒
        iRefreshOpcCCountDown = iRefreshOpcCCountDown - 1
        If iRefreshOpcCCountDown <= 0 Then
            bOpcCOut = True
            iRefreshOpcCCountDown = iRefreshOpcCInterval
            'Else
            '    bOpcCOut = False
        End If

        


    End Sub

    Private Sub btnConnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnConnect.Click
        btnConnect.Enabled = False
        Try
            MyOPCServer(iOPC).MyServer.Connect(txtServerName.Text, txtServerIP.Text)

            If MyOPCServer(iOPC).MyServer.ServerState = 1 Then
                StatusConnection.Text = "Connected"


                If MyOPCGroup(iOPC) Is Nothing Then
                    MyOPCGroup(iOPC) = MyOPCServer(iOPC).MyServer.OPCGroups.Add("MyOPC_" & iOPC)
                End If
                MyOPCGroup(iOPC).IsActive = False
                MyOPCGroup(iOPC).IsSubscribed = True
                MyOPCGroup(iOPC).UpdateRate = UpdateRate

                AddHandler MyOPCGroup(iOPC).DataChange, AddressOf MyOPCGroup_DataChange

                ListServer()
                Show_Items()       '依照INI 項目顯示

                ListBox1.Items.Add(NowTime & "Connected")

                btnMonitoringOn.Enabled = True  'OPC 為連結前Monitor鍵反白
                btnDisconnect.Enabled = True
                btnConnect.Enabled = False

                StartMonitor()
            Else

                StatusConnection.Text = "ERROR Connect!"

            End If
        Catch ex As Exception
            ListBox1.Items.Add(NowTime & "ERROR Connect!")
            StatusConnection.Text = "ERROR Connect!"
            StatusCommand.Text = ex.Message
        End Try

    End Sub


    '列出所有OPCServer Name
    Private Sub ListServer()
        Dim LocalServer
        Dim tmp
        LocalServer = MyOPCServers.GetOPCServers(txtServerIP.Text)
        lstOPCServer.Nodes.Clear()
        For Each tmp In LocalServer
            lstOPCServer.Nodes.Add(tmp)
        Next
    End Sub


    Sub Show_Items()

        Dim a, iA
        iA = OPCItemCount  '子項目個數


        MonItem_Count = 0

        For a = 1 To iA
            Dim name1, name2
            name1 = ItemNameArray(a, 2)  '_BSTR
            name2 = ItemNameArray(a, 1)  'Numeric_BSTR

            '加入List


            '加入DataGridView
            dataOPC.Rows.Add()
            dataOPC.Item(0, MonItem_Count).Value = name2
            MonItem_Count += 1

            ReDim Preserve MonItem(MonItem_Count)
            MonItem(MonItem_Count)._Server = iOPC
            MonItem(MonItem_Count)._Name = name2
            MonItem(MonItem_Count)._Index = dataOPC.RowCount - 1

            MyOPCServer(iOPC).MyItem = MyOPCGroup(iOPC).OPCItems.AddItem(name2, MonItem_Count)
            MyOPCServer(iOPC).ServerHandles(MonItem_Count) = MyOPCServer(iOPC).MyItem.ServerHandle
            MyOPCGroup(iOPC).AsyncRead(MonItem_Count, MyOPCServer(iOPC).ServerHandles, MyOPCServer(iOPC).Errors, Second(Now), Second(Now))
        Next

    End Sub

    Private Sub MyOPCGroup_DataChange(ByVal TransactionID As Integer, ByVal NumItems As Integer, ByRef ClientHandles As System.Array, ByRef ItemValues As System.Array, ByRef Qualities As System.Array, ByRef TimeStamps As System.Array)

        Try


            For i = 1 To NumItems
                Dim iHandles As Integer = ClientHandles(i)
                dataOPC.Item(1, iHandles - 1).Value = ItemValues(i)

                sMsg = Format(Now(), "yyyy/MM/dd HH:mm:ss") & " " & MonItem(iHandles)._Name & " " & ItemValues(i)


                'If Debug = "1" Then ListBox1.Items.Add(sMsg) '寫入除錯視窗


                SplitLine()

            Next
        Catch ex As Exception
            sLogMsg = "MyOPCGroup_DataChange>> Exception!!"
            ErrReport()
        End Try
    End Sub


    Private Sub btnDisconnect_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDisconnect.Click
        Dim i As Integer
        Try
            For i = 0 To 10
                MyOPCServer(i).MyItem = Nothing
                If MyOPCServer(i).MyServer.ServerState = 1 Then
                    MyOPCServer(i).MyServer.OPCGroups.RemoveAll()
                    MyOPCGroup(i).IsSubscribed = False
                    'MyOPCGroup(i).IsActive = False
                End If
                MyOPCGroup(i) = Nothing
            Next

            Dim okDiskonek As Boolean = True
            For i = 0 To 10
                If MyOPCServer(i).MyServer.ServerState = 1 Then
                    MyOPCServer(i).MyServer.Disconnect()
                    If MyOPCServer(i).MyServer.ServerState = 1 Then
                        okDiskonek = False
                    End If
                End If
            Next
            If okDiskonek = True Then
                StatusConnection.Text = "Disconnected"
                ListBox1.Items.Add(NowTime & "Disconnected")
                If Debug = "1" Then Logfile.WriteLine("Disconnected")

                btnDisconnect.Enabled = False
                btnMonitoringOn.Enabled = False
                btnConnect.Enabled = True
            Else
                StatusConnection.Text = "ERROR Disconnect!"
                If Debug = "1" Then Logfile.WriteLine("ERROR Disconnect!")
            End If


            lstOPCServer.Nodes.Clear()
            dataOPC.Rows.Clear()

        Catch ex As Exception
            ListBox1.Items.Add(NowTime & "ERROR Disconnect!")
            StatusConnection.Text = "ERROR Disconnect!"
            StatusCommand.Text = ex.Message

        End Try

    End Sub

    Private Sub btnMonitoringOn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMonitoringOn.Click
        StartMonitor()
    End Sub
    Private Sub StartMonitor()
        For i = 0 To 10
            If MyOPCServer(i).MyServer.ServerState = 1 Then
                MyOPCGroup(i).IsActive = True
            End If
        Next

        ListBox1.Items.Add(NowTime & "Start monitoring")
        btnConnect.Enabled = False
        btnMonitoringOn.Enabled = False
        btnDisconnect.Enabled = True
    End Sub



    '分割字串 填入datatable
    Private Sub SplitLine()
        'Dim objReader As New StreamReader(sMonTXTFilesName)
        Dim sLine As String = ""
        Dim s As Integer = 1
        Dim j As Integer = 1

        Dim sDoorTemp As String
        Dim dtRowsCount As Integer = dt.Rows.Count - 1
        Dim dtNumberRowsCount As Integer = dt_Number.Rows.Count - 1
        Dim dtNumberRowsCount2 As Integer = dt_Number.Rows.Count - 1
        'Dim dtSegmentRowCount As Integer = dt_Segment.Rows.Count - 1
        Dim dtTIDRowCount As Integer = dt_TID.Rows.Count - 1
        Dim dtTrainInitCount As Integer = dt_TrainInit.Rows.Count   '20210514 ADD COUNT  initialization
        Dim sSegTemp, sSegREG, sSegNum, sSegNumber As String
        Dim iSegLen As Integer
        'Dim sSegOut As String
        Dim sINITemp As String
        Try

            If Debug = "1" Then Logfile.WriteLine(sMsg) '20201012寫入log

            sLine = sMsg
            If Not sLine Is Nothing Then

                Dim sAllArray As String() = sLine.Split(" ")                '暫存讀進來的分割的字串 0:日期、1:時間、2:MSG、3:DATA
                Dim sTrainArray As String() = sAllArray(2).Split(".")       '再次分割字串 0:ATO、1:STATION、2:B2U、3:Status、4:station_dwell_time


                If sTrainArray(1) = "Station" Then

                    For s = 0 To dtRowsCount

                        If dt.Rows(s)(0).ToString() = sTrainArray(2) Then
                            dt.Rows(s)(3) = sAllArray(0) & " " & sAllArray(1)                               '日期
                            dt.Rows(s)(4) = Microsoft.VisualBasic.Right(sAllArray(0), 2)                    '日

                            If sTrainArray(4) = "station_dwell_time" Then dt.Rows(s)(7) = sAllArray(3) 'dwell_time
                            If sTrainArray(4) = "train_id" Then dt.Rows(s)(5) = sAllArray(3) 'train id
                            'If dt.Rows(s)(5).ToString = "0" Then dt.Rows(s)(6) = "0" 'if car a=0 then car b=0      20210413 mark 不填入car b

                            If Microsoft.VisualBasic.Right(sTrainArray(4), 13) = "door_1_status" Then       '到站 離站
                                sDoorTemp = dt.Rows(s)(8).ToString

                                If Microsoft.VisualBasic.Left(sTrainArray(2), 2) = "BR" Then
                                    If sAllArray(3) = "16" Then dt.Rows(s)(8) = "D" '離站
                                    If sAllArray(3) = "0" Then dt.Rows(s)(8) = "A" '到站
                                Else
                                    If sAllArray(3) = "16" Then dt.Rows(s)(8) = "D" '離站
                                    If sAllArray(3) = "32" Then dt.Rows(s)(8) = "A" '到站
                                End If
                            End If

                            'if 沒車號則 與列車離站,到離站狀態標註""
                            If dt.Rows(s)(5) = "0" Then
                                dt.Rows(s)(8) = ""
                            End If


                            '填入car b 車組id        20210413 mark 不填入car b
                            'For j = 0 To dtNumberRowsCount2

                            '    If dt_Number(j)(1) = sAllArray(3) And sTrainArray(4) = "train_id" And dt_Number(j)(1) <> dt_Number(j)(2) Then
                            '        dt.Rows(s)(6) = dt_Number(j)(2)
                            '    End If
                            'Next

                        End If



                    Next
                End If



                '車號 車組 對應
                'dt_Number(1)GID、(2)TrainID 、(3)Distance、(4)TIME

                If sTrainArray(1) = "Vehicle" Then 'CAR 車組有變動再處理
                    For s = 0 To dtNumberRowsCount
                        'If dt_Number(s)(2) <> sAllArray(3) Then        '20210510 mark
                        '填入TRAIN ID
                        If sTrainArray(4) = "train_id" Then
                            If dt_Number(s)(0).ToString = sTrainArray(2) Then
                                dt_Number(s)(1) = sAllArray(3)
                                dt_Number(s)(4) = sAllArray(0) & " " & sAllArray(1)                               '日期

                            End If
                        End If
                        '填入Vehicle ID
                        'If sTrainArray(4) = "ccms_veh_slot" Then    '20210413 mark
                        If sTrainArray(4) = "id" Then
                            If dt_Number(s)(0).ToString = sTrainArray(2) Then
                                dt_Number(s)(2) = sAllArray(3)
                                dt_Number(s)(4) = sAllArray(0) & " " & sAllArray(1)

                            End If
                        End If

                        '填入里程
                        If sTrainArray(4) = "distance_traveled" Then
                            If dt_Number(s)(0).ToString = sTrainArray(2) Then
                                dt_Number(s)(3) = sAllArray(3)
                                dt_Number(s)(4) = sAllArray(0) & " " & sAllArray(1)

                            End If
                        End If






                        'End If

                    Next



                End If




                '填入軌道對應表
                Dim iTemp As Integer = 0
                sSegNum = ""
                sSegREG = ""
                sSegNumber = ""


                If sTrainArray(1) = "Train" And sAllArray(3) <> "0" Then

                    '組合出RXSXXX
                    If sTrainArray(4) = "location_segment" Then  'ATO.Train.55.Status.location_segment=xxxxx
                        sSegTemp = sAllArray(3)
                        sSegTemp = Trim(Convert.ToString(CInt(sSegTemp), 2))            '轉2進制
                        iSegLen = Len(sSegTemp)
                        sSegNum = Microsoft.VisualBasic.Right(sSegTemp, 8)              '取得低位元 8bit
                        sSegREG = Microsoft.VisualBasic.Left(sSegTemp, (iSegLen - 8))   '取得高位元    
                        sSegNum = Convert.ToInt32(sSegNum, 2)                           '低位元轉10進制，取得sqgment
                        sSegREG = Convert.ToInt32(sSegREG, 2)                           '高位元轉10進制，取得region
                        sSegNumber = "R" & sSegREG & "S" & sSegNum                      '組合出RXSXXX

                        For s = 0 To dtTIDRowCount
                            If dt_TID(s)(3) = sTrainArray(2) Then
                                dt_TID(s)(0) = sSegNumber
                                dt_TID(s)(2) = Trim(NowTime)
                            End If
                        Next
                    End If


                    '20210412 add speed
                    If sTrainArray(4) = "id" Or sTrainArray(4) = "grand_route" Or sTrainArray(4) = "location_offset" Or sTrainArray(4) = "speed" Then
                        For s = 0 To dtTIDRowCount
                            If dt_TID(s)(3) = sTrainArray(2) Then
                                dt_TID(s)(2) = Trim(NowTime)
                                If sTrainArray(4) = "id" Then dt_TID(s)(1) = sAllArray(3)
                                If sTrainArray(4) = "grand_route" Then dt_TID(s)(4) = sAllArray(3) '路線
                                If sTrainArray(4) = "location_offset" Then dt_TID(s)(5) = sAllArray(3) 'offset
                                If sTrainArray(4) = "speed" Then dt_TID(s)(6) = sAllArray(3) 'speed
                            End If
                        Next
                    End If



                End If


                '20210514 add initialization
                If sTrainArray(1) = "TrainInit" And sAllArray(3) <> "0" Then
                    sINITemp = sAllArray(3)
                    sINITemp = Trim(Convert.ToString(CInt(sINITemp), 2))            '轉2進制

                    For s = 0 To dtTrainInitCount
                        If dt_TrainInit(s)(0) = sTrainArray(2) Then
                            dt_TrainInit(s)(1) = Trim(NowTime)
                            If sTrainArray(4) = "train_init_status" Then dt_TrainInit(s)(2) = sINITemp '初始化狀態
                        End If
                    Next
                End If

            End If



            Console.ReadLine()
            sMsg = ""

        Catch ex As Exception
            sLogMsg = "SplitLin Error >>"
            ErrReport()
        End Try
    End Sub



    '輸出文字檔
    Private Sub OpenOutTextFile()
        Try
            sOutTXTLine = Format(Now, "yyyyMMdd") & Format(Now, "hhmmss")
            sOutTXTFileName = sOutDIR & "\Train_" & Format(Now, "yyyyMMdd") & "_" & Format(Now, "hhmmss") & ".txt"
            file = My.Computer.FileSystem.OpenTextFileWriter(sOutTXTFileName, False, System.Text.Encoding.GetEncoding(950)) '文字檔以ANS格式

        Catch ex As Exception
            sLogMsg = "OpenOutTextFile>> Exception!!"
            ErrReport()
        End Try
    End Sub

    '輸出Log檔
    Private Sub OpenOutLogFile()

        Try
            sOutLogLine = Format(Now, "yyyyMMdd") & Format(Now, "hhmmss")
            sOutLogFileName = sOutLogDIR & "\LOG\OPCClient_" & Format(Now, "yyyyMMdd") & "_" & Format(Now, "hhmmss") & ".txt"
            Logfile = My.Computer.FileSystem.OpenTextFileWriter(sOutLogFileName, False, System.Text.Encoding.GetEncoding(950)) '文字檔以ANS格式

        Catch ex As Exception
            sLogMsg = "OpenOutLogFile>> Exception!!"
            ErrReport()
        End Try
    End Sub
    '寫入所有列車位置到文字檔
    Private Sub SendTrainLocation()

        Dim sSentOot As String
        Try
            For i = 0 To 159
                If dt_TID(i)(1) <> "" Then
                    sSentOot = "[OpcB];RT=" & dt_TID(i)(2) & ";TK=" & dt_TID(i)(0) & ";TID=" & dt_TID(i)(1) & ";GR=" & dt_TID(i)(4) & ";Offs=" & dt_TID(i)(5) & ";Speed=" & dt_TID(i)(6) & ";WT=" & Trim(NowTime)
                    file.WriteLine(sSentOot) '寫入輸出文字檔
                End If
            Next

            For i = 0 To 47
                sSentOot = "[OpcA];STN=" & dt.Rows(i)(1) & ";PF=" & dt.Rows(i)(2) & ";RT=" & dt.Rows(i)(3) & ";TID=" & dt.Rows(i)(5) & ";DT=" & dt.Rows(i)(7) & ";AD=" & dt.Rows(i)(8) & ";WT=" & Trim(NowTime)
                file.WriteLine(sSentOot) '寫入輸出文字檔
            Next

            If bOpcCOut = True Then
                For i = 0 To 159
                    If dt_Number(i)(1) <> "" Then
                        sSentOot = "[OpcC];GID=" & dt_Number(i)(0) & ";TID=" & dt_Number(i)(1) & ";VID=" & dt_Number(i)(2) & ";Dist=" & dt_Number(i)(3) & ";RT=" & dt_Number(i)(4) & ";WT=" & Trim(NowTime)
                        file.WriteLine(sSentOot) '寫入輸出文字檔
                        'bOpcCOut = False
                    End If
                Next

                '20210514 add 
                For i = 0 To 159
                    If dt_TrainInit(i)(1) <> "" Then
                        sSentOot = "[OpcD];ID=" & dt_TrainInit(i)(0) & ";Stat=" & dt_TrainInit(i)(2) & ";RT=" & dt_TrainInit(i)(1) & ";WT=" & Trim(NowTime)
                        file.WriteLine(sSentOot) '寫入輸出文字檔
                    End If
                Next
                bOpcCOut = False


            End If


            '輸出有變動的OPCC 
            'file.WriteLine(sOPCCmsg)
            'sOPCCmsg = ""

        Catch ex As Exception
            sLogMsg = "SendTrainLocation>> Exception!!"
            ErrReport()
        End Try

    End Sub


    '讀取 ini 並將監控TAG填入陣列 
    Private Sub InitialParam()
        Dim Filename, Itemtmp As String
        Dim j As Integer

        Filename = "OPC Client.ini"

        txtServerName.Text = Profile.GetValue(Filename, "SET", "Server")
        txtServerIP.Text = Profile.GetValue(Filename, "SET", "IP")

        UpdateRate = Val(Profile.GetValue(Filename, "SET", "UpdateRate"))
        OPCItemCount = Val(Profile.GetValue(Filename, "OPCItem", "OPCAmount"))
        LogFilepath = Profile.GetValue(Filename, "SET", "LogFilepath")
        sOutDIR = Profile.GetValue(Filename, "SET", "OutDir")
        Debug = Profile.GetValue(Filename, "SET", "Debug")

        '輸出檔案更新時間
        iRefreshOutInterval = Profile.GetValue(Filename, "SET", "RefreshOutFils")
        iRefreshOutCountDown = iRefreshOutInterval




        For j = 1 To OPCItemCount
            Itemtmp = "Item" & Trim(Str(j))
            ItemNameArray(j, 1) = Profile.GetValue(Filename, "OPCItem", Itemtmp)
            ItemNameArray(j, 2) = Microsoft.VisualBasic.Right(ItemNameArray(j, 1), (Len(ItemNameArray(j, 1))) - (InStr(ItemNameArray(j, 1), ".")))
        Next

    End Sub

    '初始化車號對應表
    Private Sub INI_dt_Nunber()
        dt_Number.Columns.Add("GroupID", GetType(String))      '0 GROUP ID
        dt_Number.Columns.Add("TrainID", GetType(String))  '1 
        dt_Number.Columns.Add("VehicleID", GetType(String))     '2 
        dt_Number.Columns.Add("Distance", GetType(String))     '3
        dt_Number.Columns.Add("TIME", GetType(String))     '4 

        Dim i As Integer = 1
        Dim rowN As DataRow = dt_Number.NewRow
        For i = 1 To 160
            rowN("GroupID") = CStr(i)
            rowN("TrainID") = "0"
            rowN("VehicleID") = "0"
            rowN("Distance") = "0"
            rowN("TIME") = ""
            dt_Number.Rows.Add(rowN)
            rowN = dt_Number.NewRow()
        Next
        DataGridView2.DataSource = dt_Number
    End Sub
    Private Sub dataGridViewUpdate()
        DataGridView3.DataSource = dt_TID
        DataGridView2.DataSource = dt_Number
        DataGridView3.Sort(DataGridView3.Columns(2), System.ComponentModel.ListSortDirection.Descending)
    End Sub
    '初始化列車初始化 20210514
    Private Sub INI_dt_TrainInit()
        dt_TrainInit.Columns.Add("ID", GetType(String))      '0  ID'
        dt_TrainInit.Columns.Add("TIME", GetType(String))    '1 
        dt_TrainInit.Columns.Add("Status", GetType(String))  '2  status'

        Dim i As Integer = 1
        Dim rowT As DataRow = dt_TrainInit.NewRow
        For i = 1 To 160
            rowT("ID") = CStr(i)
            rowT("TIME") = ""
            rowT("Status") = ""
            dt_TrainInit.Rows.Add(rowT)
            rowT = dt_TrainInit.NewRow()
        Next

    End Sub



    '初始化車站 月台 車號 到離站資訊
    Private Sub INI_dt()
        dt.Columns.Add("sST", GetType(String))      '0車站月台
        dt.Columns.Add("sSS", GetType(String))      '1車站(opc格式)
        dt.Columns.Add("sDir", GetType(String))     '2月台
        dt.Columns.Add("sDateTime", GetType(String)) '3日期時間
        dt.Columns.Add("sDay", GetType(String))     '4日
        dt.Columns.Add("sTD", GetType(String))      '5 CAR A
        dt.Columns.Add("sCTD", GetType(String))     '6 CAR B
        dt.Columns.Add("sDT", GetType(String))      '7 dwell時間
        dt.Columns.Add("sAD", GetType(String))      '8 到離站 a到站 b離站
        dt.Columns.Add("sSSS", GetType(String))      '9車站(地理資訊系統格式)

        Dim row As DataRow = dt.NewRow

        row("sST") = "B1U"
        row("sSS") = "B1"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B01"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B1D"
        row("sSS") = "B1"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B01"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B2U"
        row("sSS") = "B2"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B02"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B2D"
        row("sSS") = "B2"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B02"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B3U"
        row("sSS") = "B3"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B03"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B3D"
        row("sSS") = "B3"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B03"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B4U"
        row("sSS") = "B4"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B04"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B4D"
        row("sSS") = "B4"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B04"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B5U"
        row("sSS") = "B5"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B05"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B5D"
        row("sSS") = "B5"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B05"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B6U"
        row("sSS") = "B6"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B06"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B6D"
        row("sSS") = "B6"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B06"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B7U"
        row("sSS") = "B7"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B07"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B7D"
        row("sSS") = "B7"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B07"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B8U"
        row("sSS") = "B8"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B08"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B8D"
        row("sSS") = "B8"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B08"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B9U"
        row("sSS") = "B9"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B09"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B9D"
        row("sSS") = "B9"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B09"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B10U"
        row("sSS") = "B10"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B10"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B10D"
        row("sSS") = "B10"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B10"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B11U"
        row("sSS") = "B11"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B11"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "B11D"
        row("sSS") = "B11"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "B11"
        dt.Rows.Add(row)


        row = dt.NewRow()
        row("sST") = "BR1U"
        row("sSS") = "BR1"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR01"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR1D"
        row("sSS") = "BR1"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR01"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR2U"
        row("sSS") = "BR2"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR02"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR2D"
        row("sSS") = "BR2"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR02"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR3U"
        row("sSS") = "BR3"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR03"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR3D"
        row("sSS") = "BR3"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR03"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR4U"
        row("sSS") = "BR4"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR04"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR4D"
        row("sSS") = "BR4"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR04"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR5U"
        row("sSS") = "BR5"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR05"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR5D"
        row("sSS") = "BR5"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR05"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR6U"
        row("sSS") = "BR6"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR06"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR6D"
        row("sSS") = "BR6"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR06"
        dt.Rows.Add(row)


        row = dt.NewRow()
        row("sST") = "BR7U"
        row("sSS") = "BR7"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR07"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR7D"
        row("sSS") = "BR7"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR07"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR8U"
        row("sSS") = "BR8"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR08"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR8D"
        row("sSS") = "BR8"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR08"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR9U"
        row("sSS") = "BR9"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR09"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR9D"
        row("sSS") = "BR9"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR09"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR10U"
        row("sSS") = "BR10"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR10"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR10D"
        row("sSS") = "BR10"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR10"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR11U"
        row("sSS") = "BR11"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR11"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR11D"
        row("sSS") = "BR11"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR11"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR12U"
        row("sSS") = "BR12"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR12"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR12D"
        row("sSS") = "BR12"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR12"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR13U"
        row("sSS") = "BR13"
        row("sDir") = "D1"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR13"
        dt.Rows.Add(row)

        row = dt.NewRow()
        row("sST") = "BR13D"
        row("sSS") = "BR13"
        row("sDir") = "D2"
        row("sTD") = "0"
        row("sCTD") = "0"
        row("sSSS") = "BR13"
        dt.Rows.Add(row)

        DataGridView1.DataSource = dt
        DataGridView1.Sort(DataGridView1.Columns(3), System.ComponentModel.ListSortDirection.Descending)


    End Sub


    Private Sub INI_dt_TID()
        dt_TID.Columns.Add("Seg_num", GetType(String))          '軌道編號 0
        dt_TID.Columns.Add("Train_ID", GetType(String))         '車號 1
        dt_TID.Columns.Add("Occ_Time", GetType(String))         '時間 2
        dt_TID.Columns.Add("TrainL", GetType(String))           '軌道內碼 3
        dt_TID.Columns.Add("grand_route", GetType(String))      '列車路線 4
        dt_TID.Columns.Add("offset", GetType(String))          'offset 5
        dt_TID.Columns.Add("speed", GetType(String))           'speed 6 



        'dt_TID.Columns.Add("direction", GetType(String))         '方向
        'dt_TID.Columns.Add("starting_station_id", GetType(String))           '前區間車起點車站
        'dt_TID.Columns.Add("ending_station_id", GetType(String))           '區間車終點車站
        'dt_TID.Columns.Add("next_grand_route", GetType(String))             '下一路線
        'dt_TID.Columns.Add("assistance_on_route", GetType(String))          '路線





        Dim row_TID As DataRow = dt_TID.NewRow
        For i = 0 To 159
            row_TID("Seg_num") = ""
            row_TID("Train_ID") = ""
            row_TID("TrainL") = CStr(i + 1)
            row_TID("Occ_Time") = ""
            row_TID("grand_route") = ""
            row_TID("speed") = ""
            'row_TID("direction") = ""
            'row_TID("starting_station_id") = ""
            'row_TID("ending_station_id") = ""
            'row_TID("next_grand_route") = ""
            'row_TID("assistance_on_route") = ""

            dt_TID.Rows.Add(row_TID)
            row_TID = dt_TID.NewRow
        Next
        DataGridView3.DataSource = dt_TID
    End Sub

    Private Sub Timer2_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles Timer2.Tick

        'sMsg = TestArray(testCountDown)
        sMsg = TestArray(0)
        SplitLine()
        testCountDown = testCountDown + 1
        If testCountDown = testInterval Then testCountDown = 0
    End Sub

    '輸出錯誤訊息 
    Private Sub ErrReport()
        sLogMsg = sLogMsg & " Error= Erl(" & Err.Erl & "), ErrCode(" & Err.Number & ") " & Err.Description
        ListBox1.Items.Add(sLogMsg)
        If Debug = "1" Then Logfile.WriteLine(sLogMsg)
    End Sub


    '顯示測試按鈕
    Private Sub TestMode()
        If Debug = "1" Then
            btnTest.Enabled = True
            btnTest.Visible = True
        Else
            btnTest.Enabled = False
            btnTest.Visible = False
        End If
    End Sub
    '模擬測試按鈕
    Private Sub btnTest_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTest.Click
        '建立測試用陣列
        'TestArray(0) = "2020/10/12 09:10:39 ATO.Train.142.Status.id 0"
        'TestArray(0) = "2020/10/12 09:10:43 ATO.Vehicle.160.distance_traveled 2095766659"
        TestArray(0) = "2020/10/12 09:10:43 ATO.Vehicle.16.Status.distance_traveled 20"
        TestArray(1) = "2020/10/12 09:10:39 ATO.Train.141.Status.id 0"
        TestArray(2) = "2020/10/12 09:10:39 ATO.Train.140.Status.id 0"
        TestArray(3) = "2020/10/12 09:10:39 ATO.Train.139.Status.id 0"
        TestArray(4) = "2020/10/12 09:10:39 ATO.Train.138.Status.id 0"
        TestArray(5) = "2020/10/12 09:10:39 ATO.Train.137.Status.id 0"
        TestArray(6) = "2020/10/12 09:10:39 ATO.Train.136.Status.id 0"
        TestArray(7) = "2020/10/12 09:10:39 ATO.Train.9.Status.id 37"
        TestArray(8) = "2020/10/12 09:10:39 ATO.Train.8.Status.id 17"
        TestArray(9) = "2020/10/12 09:10:39 ATO.Train.7.Status.id 31"
        TestArray(10) = "2020/10/12 09:10:39 ATO.Train.6.Status.id 21"
        TestArray(11) = "2020/10/12 09:10:39 ATO.Train.5.Status.id 43"
        TestArray(12) = "2020/10/12 09:10:39 ATO.Train.4.Status.id 199"
        TestArray(13) = "2020/10/12 09:10:39 ATO.Train.157.Status.location_segment 0"
        TestArray(14) = "2020/10/12 09:10:39 ATO.Train.156.Status.location_segment 0"
        TestArray(15) = "2020/10/12 09:10:39 ATO.Train.155.Status.location_segment 0"
        TestArray(16) = "2020/10/12 09:10:39 ATO.Train.154.Status.location_segment 0"
        TestArray(17) = "2020/10/12 09:10:39 ATO.Train.153.Status.location_segment 0"
        TestArray(18) = "2020/10/12 09:10:39 ATO.Train.152.Status.location_segment 0"
        TestArray(19) = "2020/10/12 09:10:39 ATO.Train.151.Status.location_segment 0"
        TestArray(20) = "2020/10/12 09:10:39 ATO.Train.117.Status.location_segment 0"
        TestArray(21) = "2020/10/12 09:10:39 ATO.Train.116.Status.location_segment 0"
        TestArray(22) = "2020/10/12 09:10:39 ATO.Train.115.Status.location_segment 0"
        TestArray(23) = "2020/10/12 09:10:39 ATO.Train.114.Status.location_segment 0"
        TestArray(24) = "2020/10/12 09:10:39 ATO.Train.113.Status.location_segment 0"
        TestArray(25) = "2020/10/12 09:10:39 ATO.Train.112.Status.location_segment 0"
        TestArray(26) = "2020/10/12 09:10:39 ATO.Train.111.Status.location_segment 0"
        TestArray(27) = "2020/10/12 09:10:39 ATO.Train.76.Status.id 0"
        TestArray(28) = "2020/10/12 09:10:39 ATO.Train.75.Status.id 0"
        TestArray(29) = "2020/10/12 09:10:39 ATO.Train.74.Status.id 105"
        TestArray(30) = "2020/10/12 09:10:39 ATO.Train.73.Status.id 45"
        TestArray(31) = "2020/10/12 09:10:39 ATO.Train.72.Status.id 5"
        TestArray(32) = "2020/10/12 09:10:39 ATO.Train.71.Status.id 169"
        TestArray(33) = "2020/10/12 09:10:39 ATO.Train.103.Status.id 0"
        TestArray(34) = "2020/10/12 09:10:39 ATO.Train.102.Status.id 0"
        TestArray(35) = "2020/10/12 09:10:39 ATO.Train.101.Status.id 0"
        TestArray(36) = "2020/10/12 09:10:39 ATO.Train.100.Status.id 0"
        TestArray(37) = "2020/10/12 09:10:39 ATO.Train.99.Status.id 0"
        TestArray(38) = "2020/10/12 09:10:39 ATO.Train.98.Status.id 0"
        TestArray(39) = "2020/10/12 09:10:39 ATO.Train.97.Status.id 0"
        TestArray(40) = "2020/10/12 09:10:39 ATO.Train.96.Status.id 0"
        TestArray(41) = "2020/10/12 09:10:39 ATO.Train.95.Status.id 0"
        TestArray(42) = "2020/10/12 09:10:39 ATO.Station.BR5U.Status.train_id 0"
        TestArray(43) = "2020/10/12 09:10:39 ATO.Station.BR12D.Status.train_id 0"
        TestArray(44) = "2020/10/12 09:10:39 ATO.Vehicle.132.Status.train_id 17"
        TestArray(45) = "2020/10/12 09:10:39 ATO.Station.BR4D.Status.station_dwell_time 48"
        TestArray(46) = "2020/10/12 09:10:39 ATO.Station.BR1D.Status.train_id 23"
        TestArray(47) = "2020/10/12 09:10:39 ATO.Vehicle.140.Status.train_id 45"
        TestArray(48) = "2020/10/12 09:10:39 ATO.Station.BR12D.Status.right_door_1_status 16"
        TestArray(49) = "2020/10/12 09:10:39 ATO.Train.111.Status.id 0"
        TestArray(50) = "2020/10/12 09:10:39 ATO.Train.110.Status.id 0"
        TestArray(51) = "2020/10/12 09:10:39 ATO.Train.109.Status.id 0"
        TestArray(52) = "2020/10/12 09:10:39 ATO.Train.108.Status.id 0"
        TestArray(53) = "2020/10/12 09:10:39 ATO.Train.107.Status.id 0"
        TestArray(54) = "2020/10/12 09:10:39 ATO.Train.106.Status.id 0"
        TestArray(55) = "2020/10/12 09:10:39 ATO.Train.105.Status.id 0"
        TestArray(56) = "2020/10/12 09:10:39 ATO.Train.104.Status.id 0"
        TestArray(57) = "2020/10/12 09:10:39 ATO.Train.160.Status.id 0"
        TestArray(58) = "2020/10/12 09:10:39 ATO.Train.130.Status.location_segment 0"
        TestArray(59) = "2020/10/12 09:10:39 ATO.Train.129.Status.location_segment 0"
        TestArray(60) = "2020/10/12 09:10:39 ATO.Train.128.Status.location_segment 0"
        TestArray(61) = "2020/10/12 09:10:39 ATO.Train.127.Status.location_segment 0"
        TestArray(62) = "2020/10/12 09:10:39 ATO.Train.126.Status.location_segment 0"
        TestArray(63) = "2020/10/12 09:10:39 ATO.Train.125.Status.location_segment 0"
        TestArray(64) = "2020/10/12 09:10:39 ATO.Train.154.Status.id 0"
        TestArray(65) = "2020/10/12 09:10:39 ATO.Station.BR10U.Status.train_id 139"
        TestArray(66) = "2020/10/12 09:10:39 ATO.Station.B2D.Status.train_id 0"
        TestArray(67) = "2020/10/12 09:10:39 ATO.Station.B3D.Status.train_id 109"
        TestArray(68) = "2020/10/12 09:10:39 ATO.Station.B4D.Status.train_id 0"
        TestArray(69) = "2020/10/12 09:10:39 ATO.Station.B5D.Status.train_id 0"
        TestArray(70) = "2020/10/12 09:10:39 ATO.Station.B6D.Status.train_id 0"
        TestArray(71) = "2020/10/12 09:10:39 ATO.Station.B7D.Status.train_id 0"
        TestArray(72) = "2020/10/12 09:10:39 ATO.Station.B8D.Status.train_id 0"
        TestArray(73) = "2020/10/12 09:10:39 ATO.Station.B9D.Status.train_id 0"
        TestArray(74) = "2020/10/12 09:10:39 ATO.Vehicle.7.Status.id 199"
        TestArray(75) = "2020/10/12 09:10:39 ATO.Station.BR6D.Status.train_id 0"
        TestArray(76) = "2020/10/12 09:10:39 ATO.Vehicle.36.Status.id 152"
        TestArray(77) = "2020/10/12 09:10:39 ATO.Station.B10D.Status.train_id 0"
        TestArray(78) = "2020/10/12 09:10:39 ATO.Station.BR2U.Status.train_id 0"
        TestArray(79) = "2020/10/12 09:10:39 ATO.Station.B11D.Status.train_id 0"
        TestArray(80) = "2020/10/12 09:10:39 ATO.Vehicle.82.Status.id 134"
        TestArray(81) = "2020/10/12 09:10:39 ATO.Vehicle.1.Status.id 115"
        TestArray(82) = "2020/10/12 09:10:39 ATO.Station.BR13U.Status.train_id 159"
        TestArray(83) = "2020/10/12 09:10:39 ATO.Vehicle.46.Status.id 196"
        TestArray(84) = "2020/10/12 09:10:39 ATO.Vehicle.35.Status.id 121"
        TestArray(85) = "2020/10/12 09:10:39 ATO.Station.BR4U.Status.train_id 0"
        TestArray(86) = "2020/10/12 09:10:39 ATO.Station.BR7U.Status.train_id 197"
        TestArray(87) = "2020/10/12 09:10:39 ATO.Station.B10U.Status.train_id 51"
        TestArray(88) = "2020/10/12 09:10:39 ATO.Station.BR1U.Status.train_id 129"
        TestArray(89) = "2020/10/12 09:10:39 ATO.Vehicle.32.Status.id 180"
        TestArray(90) = "2020/10/12 09:10:39 ATO.Vehicle.28.Status.id 130"
        TestArray(91) = "2020/10/12 09:10:39 ATO.Station.B4U.Status.train_id 0"
        TestArray(92) = "2020/10/12 09:10:39 ATO.Station.BR2D.Status.train_id 19"
        TestArray(93) = "2020/10/12 09:10:39 ATO.Station.B5U.Status.train_id 0"
        TestArray(94) = "2020/10/12 09:10:39 ATO.Station.B6U.Status.train_id 105"
        TestArray(95) = "2020/10/12 09:10:39 ATO.Station.B7U.Status.train_id 35"
        TestArray(96) = "2020/10/12 09:10:39 ATO.Station.B8U.Status.train_id 3"
        TestArray(97) = "2020/10/12 09:10:39 ATO.Station.B9U.Status.train_id 37"
        TestArray(98) = "2020/10/12 09:10:39 ATO.Station.B11U.Status.train_id 9"
        TestArray(99) = "2020/10/12 09:10:39 ATO.Vehicle.3.Status.id 9"
        TestArray(100) = "2020/10/12 09:10:39 ATO.Station.BR3D.Status.train_id 0"
        TestArray(101) = "2020/10/12 09:10:39 ATO.Station.BR8U.Status.train_id 0"
        TestArray(102) = "2020/10/12 09:10:39 ATO.Vehicle.11.Status.id 21"
        TestArray(103) = "2020/10/12 09:10:39 ATO.Vehicle.30.Status.id 26"
        TestArray(104) = "2020/10/12 09:10:39 ATO.Vehicle.10.Status.id 32"
        TestArray(105) = "2020/10/12 09:10:39 ATO.Vehicle.18.Status.id 36"
        TestArray(106) = "2020/10/12 09:10:39 ATO.Vehicle.80.Status.id 144"
        TestArray(107) = "2020/10/12 09:10:39 ATO.Vehicle.73.Status.id 201"
        TestArray(108) = "2020/10/12 09:10:39 ATO.Vehicle.14.Status.id 6"
        TestArray(109) = "2020/10/12 09:10:39 ATO.Vehicle.60.Status.id 132"
        TestArray(110) = "2020/10/12 09:10:39 ATO.Vehicle.15.Status.id 23"
        TestArray(111) = "2020/10/12 09:10:39 ATO.Vehicle.77.Status.id 5"
        TestArray(112) = "2020/10/12 09:10:39 ATO.Vehicle.23.Status.id 175"
        TestArray(113) = "2020/10/12 09:10:39 ATO.Vehicle.51.Status.id 167"
        TestArray(114) = "2020/10/12 09:10:39 ATO.Vehicle.42.Status.id 164"
        TestArray(115) = "2020/10/12 09:10:39 ATO.Vehicle.21.Status.id 19"
        TestArray(116) = "2020/10/12 09:10:39 ATO.Vehicle.19.Status.id 193"
        TestArray(117) = "2020/10/12 09:10:39 ATO.Station.BR3U.Status.train_id 173"
        TestArray(118) = "2020/10/12 09:10:39 ATO.Station.BR6U.Status.train_id 117"
        TestArray(119) = "2020/10/12 09:10:39 ATO.Vehicle.79.Status.id 195"
        TestArray(120) = "2020/10/12 09:10:39 ATO.Station.BR11D.Status.train_id 0"
        TestArray(121) = "2020/10/12 09:10:39 ATO.Station.BR4D.Status.train_id 177"
        TestArray(122) = "2020/10/12 09:10:39 ATO.Station.BR9U.Status.train_id 0"
        TestArray(123) = "2020/10/12 09:10:39 ATO.Station.BR8D.Status.train_id 137"
        TestArray(124) = "2020/10/12 09:10:39 ATO.Vehicle.4.Status.id 20"
        TestArray(125) = "2020/10/12 09:10:39 ATO.Station.BR1U.Status.right_door_1_status 16"
        TestArray(126) = "2020/10/12 09:10:39 ATO.Station.B4D.Status.right_door_1_status 16"
        TestArray(127) = "2020/10/12 09:10:39 ATO.Station.B1D.Status.right_door_1_status 16"
        TestArray(128) = "2020/10/12 09:10:39 ATO.Station.BR9D.Status.station_dwell_time 25"
        TestArray(129) = "2020/10/12 09:10:39 ATO.Station.BR10D.Status.station_dwell_time 25"
        TestArray(130) = "2020/10/12 09:10:39 ATO.Station.BR11D.Status.station_dwell_time 25"
        TestArray(131) = "2020/10/12 09:10:39 ATO.Station.BR12D.Status.station_dwell_time 25"
        TestArray(132) = "2020/10/12 09:10:39 ATO.Station.BR13D.Status.station_dwell_time 45"
        TestArray(133) = "2020/10/12 09:10:39 ATO.Station.BR1U.Status.station_dwell_time 30"
        TestArray(134) = "2020/10/12 09:10:39 ATO.Station.BR2U.Status.station_dwell_time 35"
        TestArray(135) = "2020/10/12 09:10:39 ATO.Station.BR3U.Status.station_dwell_time 43"
        TestArray(136) = "2020/10/12 09:10:39 ATO.Vehicle.16.Status.id 34"
        TestArray(137) = "2020/10/12 09:10:39 ATO.Vehicle.111.Status.id 149"
        TestArray(138) = "2020/10/12 09:10:39 ATO.Vehicle.26.Status.id 142"
        TestArray(139) = "2020/10/12 09:10:39 ATO.Vehicle.5.Status.id 33"
        TestArray(140) = "2020/10/12 09:10:39 ATO.Vehicle.54.Status.id 176"
        TestArray(141) = "2020/10/12 09:10:39 ATO.Vehicle.13.Status.id 31"
        TestArray(142) = "2020/10/12 09:10:39 ATO.Vehicle.53.Status.id 117"
        TestArray(143) = "2020/10/12 09:10:39 ATO.Vehicle.66.Status.ccms_veh_slot 186"
        TestArray(144) = "2020/10/12 09:10:39 ATO.Vehicle.33.Status.id 35"
        TestArray(145) = "2020/10/12 09:10:39 ATO.Train.22.Status.id 29"
        TestArray(146) = "2020/10/12 09:10:39 ATO.Train.21.Status.id 137"
        TestArray(147) = "2020/10/12 09:10:39 ATO.Train.20.Status.id 109"
        TestArray(148) = "2020/10/12 09:10:39 ATO.Train.19.Status.id 139"
        TestArray(149) = "2020/10/12 09:10:39 ATO.Train.18.Status.id 115"
        TestArray(150) = "2020/10/12 09:10:39 ATO.Train.17.Status.id 35"
        TestArray(151) = "2020/10/12 09:10:39 ATO.Station.B6D.Status.station_dwell_time 25"
        TestArray(152) = "2020/10/12 09:10:39 ATO.Station.BR11U.Status.station_dwell_time 25"
        TestArray(153) = "2020/10/12 09:10:39 ATO.Vehicle.143.Status.train_id 0"
        TestArray(154) = "2020/10/12 09:10:39 ATO.Station.BR9U.Status.station_dwell_time 25"
        TestArray(155) = "2020/10/12 09:10:39 ATO.Vehicle.149.Status.train_id 0"
        TestArray(156) = "2020/10/12 09:10:40 ATO.Vehicle.150.Status.train_id 0"
        TestArray(157) = "2020/10/12 09:10:40 ATO.Vehicle.151.Status.train_id 0"
        TestArray(158) = "2020/10/12 09:10:40 ATO.Vehicle.152.Status.train_id 0"
        TestArray(159) = "2020/10/12 09:10:40 ATO.Vehicle.153.Status.train_id 0"
        TestArray(160) = "2020/10/12 09:10:40 ATO.Vehicle.154.Status.train_id 0"
        TestArray(161) = "2020/10/12 09:10:40 ATO.Vehicle.124.Status.train_id 183"
        TestArray(162) = "2020/10/12 09:10:40 ATO.Vehicle.125.Status.train_id 123"
        TestArray(163) = "2020/10/12 09:10:40 ATO.Vehicle.126.Status.train_id 123"
        TestArray(164) = "2020/10/12 09:10:40 ATO.Vehicle.127.Status.train_id 155"
        TestArray(165) = "2020/10/12 09:10:40 ATO.Vehicle.128.Status.train_id 155"
        TestArray(166) = "2020/10/12 09:10:40 ATO.Vehicle.129.Status.train_id 191"
        TestArray(167) = "2020/10/12 09:10:40 ATO.Vehicle.130.Status.train_id 191"
        TestArray(168) = "2020/10/12 09:10:40 ATO.Vehicle.141.Status.train_id 105"
        TestArray(169) = "2020/10/12 09:10:40 ATO.Station.BR7U.Status.station_dwell_time 25"
        TestArray(170) = "2020/10/12 09:10:40 ATO.Vehicle.113.Status.train_id 181"
        TestArray(171) = "2020/10/12 09:10:40 ATO.Vehicle.114.Status.train_id 181"
        TestArray(172) = "2020/10/12 09:10:40 ATO.Vehicle.115.Status.train_id 139"
        TestArray(173) = "2020/10/12 09:10:40 ATO.Vehicle.116.Status.train_id 139"
        TestArray(174) = "2020/10/12 09:10:40 ATO.Vehicle.117.Status.train_id 171"
        TestArray(175) = "2020/10/12 09:10:40 ATO.Vehicle.118.Status.train_id 171"
        TestArray(176) = "2020/10/12 09:10:40 ATO.Vehicle.119.Status.train_id 147"
        TestArray(177) = "2020/10/12 09:10:40 ATO.Vehicle.120.Status.train_id 147"
        TestArray(178) = "2020/10/12 09:10:40 ATO.Vehicle.121.Status.train_id 0"
        TestArray(179) = "2020/10/12 09:10:40 ATO.Vehicle.122.Status.train_id 0"
        TestArray(180) = "2020/10/12 09:10:40 ATO.Vehicle.123.Status.train_id 183"
        TestArray(181) = "2020/10/12 09:10:40 ATO.Train.155.Status.id 0"
        TestArray(182) = "2020/10/12 09:10:40 ATO.Train.144.Status.id 0"
        TestArray(183) = "2020/10/12 09:10:40 ATO.Train.143.Status.id 0"
        TestArray(184) = "2020/10/12 09:10:40 ATO.Vehicle.83.Status.train_id 111"
        TestArray(185) = "2020/10/12 09:10:40 ATO.Vehicle.15.Status.train_id 23"
        TestArray(186) = "2020/10/12 09:10:40 ATO.Vehicle.16.Status.train_id 23"
        TestArray(187) = "2020/10/12 09:10:40 ATO.Vehicle.13.Status.train_id 31"
        TestArray(188) = "2020/10/12 09:10:40 ATO.Vehicle.10.Status.train_id 43"
        TestArray(189) = "2020/10/12 09:10:40 ATO.Vehicle.21.Status.train_id 19"
        TestArray(190) = "2020/10/12 09:10:40 ATO.Train.146.Status.id 0"
        TestArray(191) = "2020/10/12 09:10:40 ATO.Train.145.Status.id 0"
        TestArray(192) = "2020/10/12 09:10:40 ATO.Train.137.Status.location_segment 0"
        TestArray(193) = "2020/10/12 09:10:40 ATO.Train.136.Status.location_segment 0"
        TestArray(194) = "2020/10/12 09:10:40 ATO.Train.135.Status.location_segment 0"
        TestArray(195) = "2020/10/12 09:10:40 ATO.Train.134.Status.location_segment 0"
        TestArray(196) = "2020/10/12 09:10:40 ATO.Train.133.Status.location_segment 0"
        TestArray(197) = "2020/10/12 09:10:40 ATO.Train.132.Status.location_segment 0"
        TestArray(198) = "2020/10/12 09:10:40 ATO.Train.131.Status.location_segment 0"
        TestArray(199) = "2020/10/12 09:10:40 ATO.Vehicle.8.Status.id 128"
        TestArray(200) = "2020/10/12 09:10:40 ATO.Vehicle.70.Status.id 154"
        TestArray(201) = "2020/10/12 09:10:40 ATO.Vehicle.101.Status.id 18"
        TestArray(202) = "2020/10/12 09:10:40 ATO.Vehicle.25.Status.id 141"
        TestArray(203) = "2020/10/12 09:10:40 ATO.Vehicle.2.Status.id 136"
        TestArray(204) = "2020/10/12 09:10:40 ATO.Vehicle.6.Status.id 30"
        TestArray(205) = "2020/10/12 09:10:40 ATO.Vehicle.38.Status.id 194"
        TestArray(206) = "2020/10/12 09:10:40 ATO.Vehicle.29.Status.id 7"
        TestArray(207) = "2020/10/12 09:10:40 ATO.Vehicle.22.Status.id 2"
        TestArray(208) = "2020/10/12 09:10:40 ATO.Vehicle.153.Status.id 0"
        TestArray(209) = "2020/10/12 09:10:40 ATO.Vehicle.72.Status.id 116"
        TestArray(210) = "2020/10/12 09:10:40 ATO.Vehicle.112.Status.id 200"
        TestArray(211) = "2020/10/12 09:10:40 ATO.Vehicle.40.Status.id 25"
        TestArray(212) = "2020/10/12 09:10:40 ATO.Vehicle.107.Status.id 51"
        TestArray(213) = "2020/10/12 09:10:40 ATO.Vehicle.24.Status.id 114"
        TestArray(214) = "2020/10/12 09:10:40 ATO.Vehicle.59.Status.id 177"
        TestArray(215) = "2020/10/12 09:10:40 ATO.Vehicle.78.Status.id 4"
        TestArray(216) = "2020/10/12 09:10:40 ATO.Vehicle.17.Status.id 37"
        TestArray(217) = "2020/10/12 09:10:40 ATO.Vehicle.20.Status.id 126"
        TestArray(218) = "2020/10/12 09:10:40 ATO.Vehicle.69.Status.id 173"
        TestArray(219) = "2020/10/12 09:10:40 ATO.Vehicle.45.Status.id 143"
        TestArray(220) = "2020/10/12 09:10:40 ATO.Station.BR5D.Status.train_id 0"
        TestArray(221) = "2020/10/12 09:10:40 ATO.Station.B8U.Status.right_door_1_status 32"
        TestArray(222) = "2020/10/12 09:10:40 ATO.Station.B9U.Status.right_door_1_status 16"
        TestArray(223) = "2020/10/12 09:10:40 ATO.Station.B10U.Status.right_door_1_status 16"
        TestArray(224) = "2020/10/12 09:10:40 ATO.Station.B11U.Status.right_door_1_status 16"
        TestArray(225) = "2020/10/12 09:10:40 ATO.Station.BR1D.Status.right_door_1_status 16"
        TestArray(226) = "2020/10/12 09:10:40 ATO.Station.BR2D.Status.right_door_1_status 0"
        TestArray(227) = "2020/10/12 09:10:40 ATO.Station.BR3D.Status.right_door_1_status 16"
        TestArray(228) = "2020/10/12 09:10:40 ATO.Station.BR4D.Status.right_door_1_status 16"
        TestArray(229) = "2020/10/12 09:10:40 ATO.Station.BR5D.Status.right_door_1_status 16"
        TestArray(230) = "2020/10/12 09:10:40 ATO.Station.BR6D.Status.right_door_1_status 16"
        TestArray(231) = "2020/10/12 09:10:40 ATO.Station.BR7D.Status.right_door_1_status 16"
        TestArray(232) = "2020/10/12 09:10:40 ATO.Station.BR8D.Status.right_door_1_status 16"
        TestArray(233) = "2020/10/12 09:10:40 ATO.Station.BR9D.Status.right_door_1_status 0"
        TestArray(234) = "2020/10/12 09:10:40 ATO.Station.BR10D.Status.right_door_1_status 16"
        TestArray(235) = "2020/10/12 09:10:40 ATO.Station.B1U.Status.train_id 0"
        TestArray(236) = "2020/10/12 09:10:40 ATO.Train.119.Status.id 0"
        TestArray(237) = "2020/10/12 09:10:40 ATO.Train.118.Status.id 0"
        TestArray(238) = "2020/10/12 09:10:40 ATO.Train.117.Status.id 0"
        TestArray(239) = "2020/10/12 09:10:40 ATO.Train.116.Status.id 0"
        TestArray(240) = "2020/10/12 09:10:40 ATO.Train.115.Status.id 0"
        TestArray(241) = "2020/10/12 09:10:40 ATO.Train.114.Status.id 0"
        TestArray(242) = "2020/10/12 09:10:40 ATO.Train.113.Status.id 0"
        TestArray(243) = "2020/10/12 09:10:40 ATO.Train.112.Status.id 0"
        TestArray(244) = "2020/10/12 09:10:40 ATO.Train.96.Status.location_segment 0"
        TestArray(245) = "2020/10/12 09:10:40 ATO.Train.95.Status.location_segment 0"
        TestArray(246) = "2020/10/12 09:10:40 ATO.Train.94.Status.location_segment 0"
        TestArray(247) = "2020/10/12 09:10:40 ATO.Train.93.Status.location_segment 0"
        TestArray(248) = "2020/10/12 09:10:40 ATO.Train.92.Status.location_segment 0"
        TestArray(249) = "2020/10/12 09:10:40 ATO.Train.91.Status.location_segment 0"
        TestArray(250) = "2020/10/12 09:10:40 ATO.Train.90.Status.location_segment 0"
        TestArray(251) = "2020/10/12 09:10:40 ATO.Station.B2U.Status.train_id 7"
        TestArray(252) = "2020/10/12 09:10:40 ATO.Station.BR11U.Status.train_id 0"
        TestArray(253) = "2020/10/12 09:10:40 ATO.Station.BR7D.Status.train_id 0"
        TestArray(254) = "2020/10/12 09:10:40 ATO.Station.B3U.Status.train_id 0"
        TestArray(255) = "2020/10/12 09:10:40 ATO.Station.BR13D.Status.right_door_1_status 0"
        TestArray(256) = "2020/10/12 09:10:40 ATO.Station.BR2U.Status.right_door_1_status 16"
        TestArray(257) = "2020/10/12 09:10:40 ATO.Station.BR3U.Status.right_door_1_status 0"
        TestArray(258) = "2020/10/12 09:10:40 ATO.Station.BR4U.Status.right_door_1_status 16"
        TestArray(259) = "2020/10/12 09:10:40 ATO.Station.BR5U.Status.right_door_1_status 16"
        TestArray(260) = "2020/10/12 09:10:40 ATO.Station.BR6U.Status.right_door_1_status 0"
        TestArray(261) = "2020/10/12 09:10:40 ATO.Station.BR7U.Status.right_door_1_status 0"
        TestArray(262) = "2020/10/12 09:10:40 ATO.Station.BR8U.Status.right_door_1_status 16"
        TestArray(263) = "2020/10/12 09:10:40 ATO.Station.BR9U.Status.right_door_1_status 16"
        TestArray(264) = "2020/10/12 09:10:40 ATO.Station.BR10U.Status.right_door_1_status 0"
        TestArray(265) = "2020/10/12 09:10:40 ATO.Station.BR11U.Status.right_door_1_status 16"
        TestArray(266) = "2020/10/12 09:10:40 ATO.Station.BR12U.Status.right_door_1_status 16"
        TestArray(267) = "2020/10/12 09:10:40 ATO.Station.BR13U.Status.right_door_1_status 0"
        TestArray(268) = "2020/10/12 09:10:40 ATO.Station.B1D.Status.train_id 0"
        TestArray(269) = "2020/10/12 09:10:40 ATO.Vehicle.103.Status.id 159"
        TestArray(270) = "2020/10/12 09:10:40 ATO.Vehicle.55.Status.id 125"
        TestArray(271) = "2020/10/12 09:10:40 ATO.Vehicle.27.Status.id 189"
        TestArray(272) = "2020/10/12 09:10:40 ATO.Vehicle.106.Status.id 118"
        TestArray(273) = "2020/10/12 09:10:40 ATO.Vehicle.61.Status.id 49"
        TestArray(274) = "2020/10/12 09:10:40 ATO.Vehicle.87.Status.id 185"
        TestArray(275) = "2020/10/12 09:10:40 ATO.Vehicle.86.Status.id 158"
        TestArray(276) = "2020/10/12 09:10:40 ATO.Vehicle.91.Status.id 174"
        TestArray(277) = "2020/10/12 09:10:40 ATO.Vehicle.41.Status.id 137"
        TestArray(278) = "2020/10/12 09:10:40 ATO.Vehicle.37.Status.id 109"
        TestArray(279) = "2020/10/12 09:10:40 ATO.Vehicle.93.Status.id 133"
        TestArray(280) = "2020/10/12 09:10:40 ATO.Station.BR12U.Status.train_id 0"
        TestArray(281) = "2020/10/12 09:10:40 ATO.Vehicle.92.Status.id 151"
        TestArray(282) = "2020/10/12 09:10:40 ATO.Vehicle.50.Status.id 0"
        TestArray(283) = "2020/10/12 09:10:40 ATO.Vehicle.119.Status.id 147"
        TestArray(284) = "2020/10/12 09:10:40 ATO.Vehicle.44.Status.id 14"
        TestArray(285) = "2020/10/12 09:10:40 ATO.Vehicle.17.Status.ccms_veh_slot 37"
        TestArray(286) = "2020/10/12 09:10:40 ATO.Vehicle.47.Status.id 157"
        TestArray(287) = "2020/10/12 09:10:40 ATO.Vehicle.43.Status.id 29"
        TestArray(288) = "2020/10/12 09:10:40 ATO.Vehicle.123.Status.id 183"
        TestArray(289) = "2020/10/12 09:10:40 ATO.Vehicle.76.Status.id 120"
        TestArray(290) = "2020/10/12 09:10:40 ATO.Vehicle.88.Status.id 172"
        TestArray(291) = "2020/10/12 09:10:40 ATO.Vehicle.113.Status.id 181"
        TestArray(292) = "2020/10/12 09:10:40 ATO.Vehicle.6.Status.ccms_veh_slot 30"
        TestArray(293) = "2020/10/12 09:10:40 ATO.Vehicle.110.Status.id 127"
        TestArray(294) = "2020/10/12 09:10:40 ATO.Vehicle.141.Status.id 105"
        TestArray(295) = "2020/10/12 09:10:40 ATO.Vehicle.39.Status.id 15"
        TestArray(296) = "2020/10/12 09:10:40 ATO.Vehicle.49.Status.id 0"
        TestArray(297) = "2020/10/12 09:10:40 ATO.Vehicle.58.Status.id 46"
        TestArray(298) = "2020/10/12 09:10:40 ATO.Vehicle.100.Status.id 0"
        TestArray(299) = "2020/10/12 09:10:40 ATO.Vehicle.68.Status.id 148"
        TestArray(300) = "2020/10/12 09:10:40 ATO.Vehicle.81.Status.id 153"
        TestArray(301) = "2020/10/12 09:10:40 ATO.Vehicle.89.Status.id 129"
        TestArray(302) = "2020/10/12 09:10:40 ATO.Vehicle.128.Status.id 190"
        TestArray(303) = "2020/10/12 09:10:40 ATO.Vehicle.62.Status.id 8"
        TestArray(304) = "2020/10/12 09:10:40 ATO.Vehicle.83.Status.id 111"
        TestArray(305) = "2020/10/12 09:10:40 ATO.Vehicle.52.Status.id 168"
        TestArray(306) = "2020/10/12 09:10:40 ATO.Vehicle.34.Status.id 48"
        TestArray(307) = "2020/10/12 09:10:40 ATO.Vehicle.71.Status.id 113"
        TestArray(308) = "2020/10/12 09:10:40 ATO.Vehicle.90.Status.id 198"
        TestArray(309) = "2020/10/12 09:10:40 ATO.Vehicle.67.Status.id 197"
        TestArray(310) = "2020/10/12 09:10:40 ATO.Vehicle.41.Status.ccms_veh_slot 137"
        TestArray(311) = "2020/10/12 09:10:40 ATO.Vehicle.48.Status.id 166"
        TestArray(312) = "2020/10/12 09:10:40 ATO.Vehicle.64.Status.id 10"
        TestArray(313) = "2020/10/12 09:10:40 ATO.Vehicle.12.Status.ccms_veh_slot 50"
        TestArray(314) = "2020/10/12 09:10:40 ATO.Vehicle.102.Status.id 22"
        TestArray(315) = "2020/10/12 09:10:40 ATO.Vehicle.136.Status.id 28"
        TestArray(316) = "2020/10/12 09:10:40 ATO.Vehicle.84.Status.id 192"
        TestArray(317) = "2020/10/12 09:10:40 ATO.Vehicle.92.Status.ccms_veh_slot 151"
        TestArray(318) = "2020/10/12 09:10:40 ATO.Vehicle.66.Status.id 186"
        TestArray(319) = "2020/10/12 09:10:40 ATO.Vehicle.85.Status.id 161"
        TestArray(320) = "2020/10/12 09:10:40 ATO.Train.85.Status.id 0"
        TestArray(321) = "2020/10/12 09:10:40 ATO.Train.84.Status.id 0"
        TestArray(322) = "2020/10/12 09:10:40 ATO.Train.83.Status.id 0"
        TestArray(323) = "2020/10/12 09:10:40 ATO.Train.82.Status.id 0"
        TestArray(324) = "2020/10/12 09:10:40 ATO.Train.81.Status.id 0"
        TestArray(325) = "2020/10/12 09:10:40 ATO.Train.80.Status.id 0"
        TestArray(326) = "2020/10/12 09:10:40 ATO.Train.79.Status.id 0"
        TestArray(327) = "2020/10/12 09:10:40 ATO.Train.78.Status.id 0"
        TestArray(328) = "2020/10/12 09:10:40 ATO.Train.77.Status.id 0"
        TestArray(329) = "2020/10/12 09:10:40 ATO.Train.88.Status.location_segment 0"
        TestArray(330) = "2020/10/12 09:10:40 ATO.Train.89.Status.location_segment 0"
        TestArray(331) = "2020/10/12 09:10:40 ATO.Train.87.Status.location_segment 0"
        TestArray(332) = "2020/10/12 09:10:40 ATO.Vehicle.63.Status.ccms_veh_slot 41"
        TestArray(333) = "2020/10/12 09:10:40 ATO.Vehicle.65.Status.id 179"
        TestArray(334) = "2020/10/12 09:10:40 ATO.Vehicle.95.Status.id 187"
        TestArray(335) = "2020/10/12 09:10:40 ATO.Vehicle.94.Status.id 122"
        TestArray(336) = "2020/10/12 09:10:40 ATO.Vehicle.39.Status.ccms_veh_slot 15"
        TestArray(337) = "2020/10/12 09:10:40 ATO.Vehicle.118.Status.id 170"
        TestArray(338) = "2020/10/12 09:10:40 ATO.Vehicle.96.Status.id 108"
        TestArray(339) = "2020/10/12 09:10:40 ATO.Vehicle.56.Status.id 124"
        TestArray(340) = "2020/10/12 09:10:40 ATO.Vehicle.99.Status.id 0"
        TestArray(341) = "2020/10/12 09:10:40 ATO.Vehicle.74.Status.id 188"
        TestArray(342) = "2020/10/12 09:10:40 ATO.Vehicle.98.Status.id 156"
        TestArray(343) = "2020/10/12 09:10:40 ATO.Vehicle.63.Status.id 41"
        TestArray(344) = "2020/10/12 09:10:40 ATO.Vehicle.57.Status.id 27"
        TestArray(345) = "2020/10/12 09:10:40 ATO.Vehicle.75.Status.id 119"
        TestArray(346) = "2020/10/12 09:10:40 ATO.Vehicle.97.Status.ccms_veh_slot 131"
        TestArray(347) = "2020/10/12 09:10:40 ATO.Train.46.Status.id 185"
        TestArray(348) = "2020/10/12 09:10:40 ATO.Train.45.Status.id 161"
        TestArray(349) = "2020/10/12 09:10:40 ATO.Train.44.Status.id 111"
        TestArray(350) = "2020/10/12 09:10:40 ATO.Train.43.Status.id 153"
        TestArray(351) = "2020/10/12 09:10:40 ATO.Train.42.Status.id 195"
        TestArray(352) = "2020/10/12 09:10:40 ATO.Train.41.Status.id 15"
        TestArray(353) = "2020/10/12 09:10:40 ATO.Train.34.Status.id 197"
        TestArray(354) = "2020/10/12 09:10:40 ATO.Train.33.Status.id 179"
        TestArray(355) = "2020/10/12 09:10:40 ATO.Train.32.Status.id 41"
        TestArray(356) = "2020/10/12 09:10:40 ATO.Train.31.Status.id 23"
        TestArray(357) = "2020/10/12 09:10:40 ATO.Train.30.Status.id 177"
        TestArray(358) = "2020/10/12 09:10:40 ATO.Train.29.Status.id 27"
        TestArray(359) = "2020/10/12 09:10:40 ATO.Vehicle.24.Status.train_id 175"
        TestArray(360) = "2020/10/12 09:10:40 ATO.Vehicle.52.Status.train_id 167"
        TestArray(361) = "2020/10/12 09:10:40 ATO.Station.BR11D.Status.right_door_1_status 16"
        TestArray(362) = "2020/10/12 09:10:40 ATO.Vehicle.30.Status.train_id 7"
        TestArray(363) = "2020/10/12 09:10:40 ATO.Vehicle.31.Status.train_id 135"
        TestArray(364) = "2020/10/12 09:10:40 ATO.Vehicle.32.Status.train_id 135"
        TestArray(365) = "2020/10/12 09:10:40 ATO.Vehicle.33.Status.train_id 35"
        TestArray(366) = "2020/10/12 09:10:40 ATO.Vehicle.34.Status.train_id 35"
        TestArray(367) = "2020/10/12 09:10:40 ATO.Vehicle.101.Status.train_id 18"
        TestArray(368) = "2020/10/12 09:10:40 ATO.Vehicle.92.Status.train_id 151"
        TestArray(369) = "2020/10/12 09:10:40 ATO.Vehicle.87.Status.train_id 185"
        TestArray(370) = "2020/10/12 09:10:40 ATO.Vehicle.84.Status.train_id 111"
        TestArray(371) = "2020/10/12 09:10:40 ATO.Vehicle.81.Status.train_id 153"
        TestArray(372) = "2020/10/12 09:10:40 ATO.Vehicle.94.Status.train_id 133"
        TestArray(373) = "2020/10/12 09:10:40 ATO.Vehicle.136.Status.train_id 3"
        TestArray(374) = "2020/10/12 09:10:40 ATO.Train.28.Status.id 125"
        TestArray(375) = "2020/10/12 09:10:40 ATO.Train.27.Status.id 117"
        TestArray(376) = "2020/10/12 09:10:40 ATO.Train.26.Status.id 167"
        TestArray(377) = "2020/10/12 09:10:40 ATO.Train.25.Status.id 801"
        TestArray(378) = "2020/10/12 09:10:40 ATO.Train.24.Status.id 157"
        TestArray(379) = "2020/10/12 09:10:40 ATO.Train.23.Status.id 143"
        TestArray(380) = "2020/10/12 09:10:40 ATO.Vehicle.121.Status.id 0"
        TestArray(381) = "2020/10/12 09:10:40 ATO.Vehicle.139.Status.id 45"
        TestArray(382) = "2020/10/12 09:10:40 ATO.Vehicle.108.Status.ccms_veh_slot 38"
        TestArray(383) = "2020/10/12 09:10:40 ATO.Vehicle.19.Status.ccms_veh_slot 193"
        TestArray(384) = "2020/10/12 09:10:40 ATO.Vehicle.124.Status.id 160"
        TestArray(385) = "2020/10/12 09:10:40 ATO.Vehicle.105.Status.id 163"
        TestArray(386) = "2020/10/12 09:10:40 ATO.Vehicle.97.Status.id 131"
        TestArray(387) = "2020/10/12 09:10:40 ATO.Vehicle.96.Status.train_id 187"
        TestArray(388) = "2020/10/12 09:10:40 ATO.Vehicle.57.Status.train_id 27"
        TestArray(389) = "2020/10/12 09:10:40 ATO.Vehicle.58.Status.train_id 27"
        TestArray(390) = "2020/10/12 09:10:41 ATO.Vehicle.59.Status.train_id 177"
        TestArray(391) = "2020/10/12 09:10:41 ATO.Vehicle.60.Status.train_id 177"
        TestArray(392) = "2020/10/12 09:10:41 ATO.Vehicle.61.Status.train_id 49"
        TestArray(393) = "2020/10/12 09:10:41 ATO.Vehicle.62.Status.train_id 49"
        TestArray(394) = "2020/10/12 09:10:41 ATO.Vehicle.63.Status.train_id 41"
        TestArray(395) = "2020/10/12 09:10:41 ATO.Vehicle.64.Status.train_id 41"
        TestArray(396) = "2020/10/12 09:10:41 ATO.Vehicle.65.Status.train_id 179"
        TestArray(397) = "2020/10/12 09:10:41 ATO.Vehicle.66.Status.train_id 179"
        TestArray(398) = "2020/10/12 09:10:41 ATO.Vehicle.67.Status.train_id 197"
        TestArray(399) = "2020/10/12 09:10:41 ATO.Vehicle.68.Status.train_id 197"
        TestArray(400) = "2020/10/12 09:10:41 ATO.Vehicle.69.Status.train_id 173"
        TestArray(401) = "2020/10/12 09:10:41 ATO.Vehicle.70.Status.train_id 173"
        TestArray(402) = "2020/10/12 09:10:41 ATO.Vehicle.71.Status.train_id 113"
        TestArray(403) = "2020/10/12 09:10:41 ATO.Vehicle.72.Status.train_id 113"
        TestArray(404) = "2020/10/12 09:10:41 ATO.Vehicle.73.Status.train_id 201"
        TestArray(405) = "2020/10/12 09:10:41 ATO.Vehicle.74.Status.train_id 201"
        TestArray(406) = "2020/10/12 09:10:41 ATO.Vehicle.75.Status.train_id 119"
        TestArray(407) = "2020/10/12 09:10:41 ATO.Train.103.Status.location_segment 0"
        TestArray(408) = "2020/10/12 09:10:41 ATO.Train.102.Status.location_segment 0"
        TestArray(409) = "2020/10/12 09:10:41 ATO.Train.101.Status.location_segment 0"
        TestArray(410) = "2020/10/12 09:10:41 ATO.Train.100.Status.location_segment 0"
        TestArray(411) = "2020/10/12 09:10:41 ATO.Train.99.Status.location_segment 0"
        TestArray(412) = "2020/10/12 09:10:41 ATO.Train.98.Status.location_segment 0"
        TestArray(413) = "2020/10/12 09:10:41 ATO.Train.97.Status.location_segment 0"
        TestArray(414) = "2020/10/12 09:10:41 ATO.Vehicle.100.Status.train_id 0"
        TestArray(415) = "2020/10/12 09:10:41 ATO.Station.B10D.Status.station_dwell_time 25"
        TestArray(416) = "2020/10/12 09:10:41 ATO.Vehicle.95.Status.train_id 187"
        TestArray(417) = "2020/10/12 09:10:41 ATO.Vehicle.133.Status.train_id 13"
        TestArray(418) = "2020/10/12 09:10:41 ATO.Vehicle.85.Status.train_id 161"
        TestArray(419) = "2020/10/12 09:10:41 ATO.Vehicle.86.Status.train_id 161"
        TestArray(420) = "2020/10/12 09:10:41 ATO.Vehicle.82.Status.train_id 153"
        TestArray(421) = "2020/10/12 09:10:41 ATO.Vehicle.93.Status.train_id 133"
        TestArray(422) = "2020/10/12 09:10:41 ATO.Train.157.Status.id 0"
        TestArray(423) = "2020/10/12 09:10:41 ATO.Train.64.Status.id 183"
        TestArray(424) = "2020/10/12 09:10:41 ATO.Train.63.Status.id 0"
        TestArray(425) = "2020/10/12 09:10:41 ATO.Train.62.Status.id 0"
        TestArray(426) = "2020/10/12 09:10:41 ATO.Train.61.Status.id 171"
        TestArray(427) = "2020/10/12 09:10:41 ATO.Train.60.Status.id 18"
        TestArray(428) = "2020/10/12 09:10:41 ATO.Train.59.Status.id 181"
        TestArray(429) = "2020/10/12 09:10:41 ATO.Train.110.Status.location_segment 0"
        TestArray(430) = "2020/10/12 09:10:41 ATO.Train.109.Status.location_segment 0"
        TestArray(431) = "2020/10/12 09:10:41 ATO.Train.108.Status.location_segment 0"
        TestArray(432) = "2020/10/12 09:10:41 ATO.Train.107.Status.location_segment 0"
        TestArray(433) = "2020/10/12 09:10:41 ATO.Train.106.Status.location_segment 0"
        TestArray(434) = "2020/10/12 09:10:41 ATO.Train.105.Status.location_segment 0"
        TestArray(435) = "2020/10/12 09:10:41 ATO.Train.104.Status.location_segment 0"
        TestArray(436) = "2020/10/12 09:10:41 ATO.Train.58.Status.id 149"
        TestArray(437) = "2020/10/12 09:10:41 ATO.Train.57.Status.id 127"
        TestArray(438) = "2020/10/12 09:10:41 ATO.Train.56.Status.id 51"
        TestArray(439) = "2020/10/12 09:10:41 ATO.Train.55.Status.id 163"
        TestArray(440) = "2020/10/12 09:10:41 ATO.Train.54.Status.id 159"
        TestArray(441) = "2020/10/12 09:10:41 ATO.Train.53.Status.id 800"
        TestArray(442) = "2020/10/12 09:10:41 ATO.Station.BR10D.Status.train_id 0"
        TestArray(443) = "2020/10/12 09:10:41 ATO.Vehicle.50.Status.train_id 0"
        TestArray(444) = "2020/10/12 09:10:41 ATO.Vehicle.17.Status.train_id 37"
        TestArray(445) = "2020/10/12 09:10:41 ATO.Vehicle.4.Status.train_id 9"
        TestArray(446) = "2020/10/12 09:10:41 ATO.Vehicle.122.Status.id 0"
        TestArray(447) = "2020/10/12 09:10:41 ATO.Vehicle.108.Status.id 38"
        TestArray(448) = "2020/10/12 09:10:41 ATO.Vehicle.115.Status.id 139"
        TestArray(449) = "2020/10/12 09:10:41 ATO.Vehicle.104.Status.ccms_veh_slot 138"
        TestArray(450) = "2020/10/12 09:10:41 ATO.Vehicle.53.Status.ccms_veh_slot 117"
        TestArray(451) = "2020/10/12 09:10:41 ATO.Vehicle.151.Status.ccms_veh_slot 0"
        TestArray(452) = "2020/10/12 09:10:41 ATO.Vehicle.138.Status.id 162"
        TestArray(453) = "2020/10/12 09:10:41 ATO.Vehicle.105.Status.ccms_veh_slot 163"
        TestArray(454) = "2020/10/12 09:10:41 ATO.Vehicle.48.Status.ccms_veh_slot 166"
        TestArray(455) = "2020/10/12 09:10:41 ATO.Vehicle.144.Status.id 0"
        TestArray(456) = "2020/10/12 09:10:41 ATO.Vehicle.131.Status.id 17"
        TestArray(457) = "2020/10/12 09:10:41 ATO.Vehicle.117.Status.id 171"
        TestArray(458) = "2020/10/12 09:10:41 ATO.Vehicle.137.Status.id 169"
        TestArray(459) = "2020/10/12 09:10:41 ATO.Vehicle.145.Status.id 1"
        TestArray(460) = "2020/10/12 09:10:41 ATO.Vehicle.148.Status.id 0"
        TestArray(461) = "2020/10/12 09:10:41 ATO.Vehicle.114.Status.id 140"
        TestArray(462) = "2020/10/12 09:10:41 ATO.Vehicle.109.Status.id 106"
        TestArray(463) = "2020/10/12 09:10:41 ATO.Vehicle.151.Status.id 0"
        TestArray(464) = "2020/10/12 09:10:41 ATO.Vehicle.132.Status.id 24"
        TestArray(465) = "2020/10/12 09:10:41 ATO.Vehicle.125.Status.id 123"
        TestArray(466) = "2020/10/12 09:10:41 ATO.Vehicle.147.Status.id 0"
        TestArray(467) = "2020/10/12 09:10:41 ATO.Vehicle.120.Status.id 146"
        TestArray(468) = "2020/10/12 09:10:41 ATO.Vehicle.116.Status.id 104"
        TestArray(469) = "2020/10/12 09:10:41 ATO.Vehicle.7.Status.ccms_veh_slot 199"
        TestArray(470) = "2020/10/12 09:10:41 ATO.Vehicle.155.Status.id 0"
        TestArray(471) = "2020/10/12 09:10:41 ATO.Vehicle.134.Status.id 40"
        TestArray(472) = "2020/10/12 09:10:41 ATO.Vehicle.9.Status.ccms_veh_slot 43"
        TestArray(473) = "2020/10/12 09:10:41 ATO.Vehicle.96.Status.ccms_veh_slot 108"
        TestArray(474) = "2020/10/12 09:10:41 ATO.Vehicle.13.Status.ccms_veh_slot 31"
        TestArray(475) = "2020/10/12 09:10:41 ATO.Vehicle.140.Status.id 44"
        TestArray(476) = "2020/10/12 09:10:41 ATO.Vehicle.129.Status.id 191"
        TestArray(477) = "2020/10/12 09:10:41 ATO.Vehicle.159.Status.id 0"
        TestArray(478) = "2020/10/12 09:10:41 ATO.Vehicle.59.Status.ccms_veh_slot 177"
        TestArray(479) = "2020/10/12 09:10:41 ATO.Vehicle.126.Status.id 150"
        TestArray(480) = "2020/10/12 09:10:41 ATO.Vehicle.26.Status.ccms_veh_slot 142"
        TestArray(481) = "2020/10/12 09:10:41 ATO.Vehicle.21.Status.ccms_veh_slot 19"
        TestArray(482) = "2020/10/12 09:10:41 ATO.Vehicle.127.Status.id 155"
        TestArray(483) = "2020/10/12 09:10:41 ATO.Vehicle.142.Status.id 110"
        TestArray(484) = "2020/10/12 09:10:41 ATO.Vehicle.133.Status.id 13"
        TestArray(485) = "2020/10/12 09:10:41 ATO.Vehicle.87.Status.ccms_veh_slot 185"
        TestArray(486) = "2020/10/12 09:10:41 ATO.Vehicle.1.Status.ccms_veh_slot 115"
        TestArray(487) = "2020/10/12 09:10:41 ATO.Vehicle.154.Status.id 0"
        TestArray(488) = "2020/10/12 09:10:41 ATO.Vehicle.135.Status.id 3"
        TestArray(489) = "2020/10/12 09:10:41 ATO.Vehicle.38.Status.ccms_veh_slot 194"
        TestArray(490) = "2020/10/12 09:10:41 ATO.Vehicle.130.Status.id 182"
        TestArray(491) = "2020/10/12 09:10:41 ATO.Vehicle.146.Status.id 16"
        TestArray(492) = "2020/10/12 09:10:41 ATO.Train.40.Status.location_segment 316"
        TestArray(493) = "2020/10/12 09:10:41 ATO.Vehicle.15.Status.ccms_veh_slot 23"
        TestArray(494) = "2020/10/12 09:10:41 ATO.Vehicle.149.Status.id 0"
        TestArray(495) = "2020/10/12 09:10:41 ATO.Vehicle.23.Status.ccms_veh_slot 175"
        TestArray(496) = "2020/10/12 09:10:41 ATO.Vehicle.11.Status.ccms_veh_slot 21"
        TestArray(497) = "2020/10/12 09:10:41 ATO.Vehicle.37.Status.train_id 109"
        TestArray(498) = "2020/10/12 09:10:41 ATO.Vehicle.44.Status.train_id 29"
        TestArray(499) = "2020/10/12 09:10:41 ATO.Vehicle.6.Status.train_id 33"
        TestArray(500) = "2020/10/12 09:10:41 ATO.Vehicle.4.Status.ccms_veh_slot 20"
        TestArray(501) = "2020/10/12 09:10:41 ATO.Vehicle.143.Status.id 0"
        TestArray(502) = "2020/10/12 09:10:41 ATO.Vehicle.51.Status.ccms_veh_slot 167"
        TestArray(503) = "2020/10/12 09:10:41 ATO.Vehicle.5.Status.ccms_veh_slot 33"
        TestArray(504) = "2020/10/12 09:10:41 ATO.Vehicle.8.Status.ccms_veh_slot 128"
        TestArray(505) = "2020/10/12 09:10:41 ATO.Station.B9D.Status.left_door_1_status 16"
        TestArray(506) = "2020/10/12 09:10:41 ATO.Vehicle.16.Status.ccms_veh_slot 34"
        TestArray(507) = "2020/10/12 09:10:41 ATO.Vehicle.104.Status.id 138"
        TestArray(508) = "2020/10/12 09:10:41 ATO.Train.156.Status.id 0"
        TestArray(509) = "2020/10/12 09:10:41 ATO.Train.40.Status.id 49"
        TestArray(510) = "2020/10/12 09:10:41 ATO.Train.39.Status.id 119"
        TestArray(511) = "2020/10/12 09:10:41 ATO.Train.38.Status.id 201"
        TestArray(512) = "2020/10/12 09:10:41 ATO.Train.37.Status.id 113"
        TestArray(513) = "2020/10/12 09:10:41 ATO.Train.36.Status.id 121"
        TestArray(514) = "2020/10/12 09:10:41 ATO.Train.35.Status.id 173"
        TestArray(515) = "2020/10/12 09:10:41 ATO.Vehicle.76.Status.train_id 119"
        TestArray(516) = "2020/10/12 09:10:41 ATO.Vehicle.77.Status.train_id 5"
        TestArray(517) = "2020/10/12 09:10:41 ATO.Vehicle.78.Status.train_id 5"
        TestArray(518) = "2020/10/12 09:10:41 ATO.Vehicle.79.Status.train_id 195"
        TestArray(519) = "2020/10/12 09:10:41 ATO.Station.BR2D.Status.station_dwell_time 35"
        TestArray(520) = "2020/10/12 09:10:41 ATO.Vehicle.12.Status.id 50"
        TestArray(521) = "2020/10/12 09:10:41 ATO.Vehicle.9.Status.id 43"
        TestArray(522) = "2020/10/12 09:10:41 ATO.Station.BR3D.Status.station_dwell_time 35"
        TestArray(523) = "2020/10/12 09:10:41 ATO.Vehicle.138.Status.train_id 169"
        TestArray(524) = "2020/10/12 09:10:41 ATO.Vehicle.139.Status.train_id 45"
        TestArray(525) = "2020/10/12 09:10:41 ATO.Vehicle.131.Status.train_id 17"
        TestArray(526) = "2020/10/12 09:10:41 ATO.Train.153.Status.id 0"
        TestArray(527) = "2020/10/12 09:10:41 ATO.Vehicle.160.Status.id 0"
        TestArray(528) = "2020/10/12 09:10:41 ATO.Vehicle.3.Status.ccms_veh_slot 9"
        TestArray(529) = "2020/10/12 09:10:41 ATO.Vehicle.61.Status.ccms_veh_slot 49"
        TestArray(530) = "2020/10/12 09:10:41 ATO.Vehicle.158.Status.id 0"
        TestArray(531) = "2020/10/12 09:10:41 ATO.Vehicle.157.Status.id 0"
        TestArray(532) = "2020/10/12 09:10:41 ATO.Vehicle.148.Status.ccms_veh_slot 0"
        TestArray(533) = "2020/10/12 09:10:41 ATO.Vehicle.49.Status.ccms_veh_slot 0"
        TestArray(534) = "2020/10/12 09:10:41 ATO.Vehicle.2.Status.ccms_veh_slot 136"
        TestArray(535) = "2020/10/12 09:10:41 ATO.Vehicle.71.Status.ccms_veh_slot 113"
        TestArray(536) = "2020/10/12 09:10:41 ATO.Vehicle.156.Status.id 0"
        TestArray(537) = "2020/10/12 09:10:41 ATO.Vehicle.10.Status.ccms_veh_slot 32"
        TestArray(538) = "2020/10/12 09:10:41 ATO.Vehicle.117.Status.ccms_veh_slot 171"
        TestArray(539) = "2020/10/12 09:10:41 ATO.Vehicle.150.Status.id 0"
        TestArray(540) = "2020/10/12 09:10:41 ATO.Vehicle.128.Status.ccms_veh_slot 190"
        TestArray(541) = "2020/10/12 09:10:41 ATO.Vehicle.44.Status.ccms_veh_slot 14"
        TestArray(542) = "2020/10/12 09:10:41 ATO.Vehicle.152.Status.id 0"
        TestArray(543) = "2020/10/12 09:10:41 ATO.Vehicle.46.Status.ccms_veh_slot 196"
        TestArray(544) = "2020/10/12 09:10:41 ATO.Vehicle.14.Status.ccms_veh_slot 6"
        TestArray(545) = "2020/10/12 09:10:41 ATO.Vehicle.72.Status.ccms_veh_slot 116"
        TestArray(546) = "2020/10/12 09:10:41 ATO.Vehicle.20.Status.ccms_veh_slot 126"
        TestArray(547) = "2020/10/12 09:10:41 ATO.Vehicle.31.Status.ccms_veh_slot 135"
        TestArray(548) = "2020/10/12 09:10:41 ATO.Vehicle.153.Status.ccms_veh_slot 0"
        TestArray(549) = "2020/10/12 09:10:41 ATO.Vehicle.30.Status.ccms_veh_slot 26"
        TestArray(550) = "2020/10/12 09:10:41 ATO.Vehicle.122.Status.ccms_veh_slot 0"
        TestArray(551) = "2020/10/12 09:10:41 ATO.Vehicle.25.Status.ccms_veh_slot 141"
        TestArray(552) = "2020/10/12 09:10:41 ATO.Vehicle.125.Status.ccms_veh_slot 123"
        TestArray(553) = "2020/10/12 09:10:41 ATO.Vehicle.40.Status.ccms_veh_slot 25"
        TestArray(554) = "2020/10/12 09:10:41 ATO.Vehicle.18.Status.ccms_veh_slot 36"
        TestArray(555) = "2020/10/12 09:10:41 ATO.Vehicle.68.Status.ccms_veh_slot 148"
        TestArray(556) = "2020/10/12 09:10:41 ATO.Vehicle.57.Status.ccms_veh_slot 27"
        TestArray(557) = "2020/10/12 09:10:41 ATO.Vehicle.45.Status.ccms_veh_slot 143"
        TestArray(558) = "2020/10/12 09:10:41 ATO.Vehicle.33.Status.ccms_veh_slot 35"
        TestArray(559) = "2020/10/12 09:10:41 ATO.Vehicle.67.Status.ccms_veh_slot 197"
        TestArray(560) = "2020/10/12 09:10:41 ATO.Vehicle.32.Status.ccms_veh_slot 180"
        TestArray(561) = "2020/10/12 09:10:41 ATO.Vehicle.126.Status.ccms_veh_slot 150"
        TestArray(562) = "2020/10/12 09:10:41 ATO.Vehicle.119.Status.ccms_veh_slot 147"
        TestArray(563) = "2020/10/12 09:10:41 ATO.Vehicle.27.Status.ccms_veh_slot 189"
        TestArray(564) = "2020/10/12 09:10:41 ATO.Vehicle.35.Status.ccms_veh_slot 121"
        TestArray(565) = "2020/10/12 09:10:41 ATO.Vehicle.103.Status.ccms_veh_slot 159"
        TestArray(566) = "2020/10/12 09:10:41 ATO.Vehicle.99.Status.ccms_veh_slot 0"
        TestArray(567) = "2020/10/12 09:10:41 ATO.Vehicle.37.Status.ccms_veh_slot 109"
        TestArray(568) = "2020/10/12 09:10:41 ATO.Vehicle.138.Status.ccms_veh_slot 162"
        TestArray(569) = "2020/10/12 09:10:41 ATO.Vehicle.127.Status.ccms_veh_slot 155"
        TestArray(570) = "2020/10/12 09:10:41 ATO.Vehicle.29.Status.ccms_veh_slot 7"
        TestArray(571) = "2020/10/12 09:10:41 ATO.Vehicle.80.Status.ccms_veh_slot 144"
        TestArray(572) = "2020/10/12 09:10:41 ATO.Vehicle.18.Status.train_id 37"
        TestArray(573) = "2020/10/12 09:10:41 ATO.Vehicle.135.Status.train_id 3"
        TestArray(574) = "2020/10/12 09:10:41 ATO.Vehicle.46.Status.train_id 143"
        TestArray(575) = "2020/10/12 09:10:41 ATO.Vehicle.91.Status.train_id 151"
        TestArray(576) = "2020/10/12 09:10:41 ATO.Station.BR4U.Status.station_dwell_time 45"
        TestArray(577) = "2020/10/12 09:10:41 ATO.Station.BR5U.Status.station_dwell_time 35"
        TestArray(578) = "2020/10/12 09:10:41 ATO.Station.BR6U.Status.station_dwell_time 25"
        TestArray(579) = "2020/10/12 09:10:41 ATO.Station.B8D.Status.station_dwell_time 25"
        TestArray(580) = "2020/10/12 09:10:41 ATO.Station.B9D.Status.station_dwell_time 25"
        TestArray(581) = "2020/10/12 09:10:41 ATO.Station.B11D.Status.station_dwell_time 158"
        TestArray(582) = "2020/10/12 09:10:42 ATO.Station.B1U.Status.station_dwell_time 25"
        TestArray(583) = "2020/10/12 09:10:42 ATO.Station.B2U.Status.station_dwell_time 25"
        TestArray(584) = "2020/10/12 09:10:42 ATO.Station.B3U.Status.station_dwell_time 35"
        TestArray(585) = "2020/10/12 09:10:42 ATO.Station.B4U.Status.station_dwell_time 30"
        TestArray(586) = "2020/10/12 09:10:42 ATO.Station.B5U.Status.station_dwell_time 30"
        TestArray(587) = "2020/10/12 09:10:42 ATO.Station.B6U.Status.station_dwell_time 25"
        TestArray(588) = "2020/10/12 09:10:42 ATO.Station.B7U.Status.station_dwell_time 25"
        TestArray(589) = "2020/10/12 09:10:42 ATO.Station.B8U.Status.station_dwell_time 25"
        TestArray(590) = "2020/10/12 09:10:42 ATO.Vehicle.106.Status.ccms_veh_slot 118"
        TestArray(591) = "2020/10/12 09:10:42 ATO.Vehicle.36.Status.ccms_veh_slot 152"
        TestArray(592) = "2020/10/12 09:10:42 ATO.Vehicle.24.Status.ccms_veh_slot 114"
        TestArray(593) = "2020/10/12 09:10:42 ATO.Station.B11U.Status.left_door_1_status 16"
        TestArray(594) = "2020/10/12 09:10:42 ATO.Vehicle.22.Status.ccms_veh_slot 2"
        TestArray(595) = "2020/10/12 09:10:42 ATO.Vehicle.42.Status.ccms_veh_slot 164"
        TestArray(596) = "2020/10/12 09:10:42 ATO.Station.BR10D.Status.left_door_1_status 16"
        TestArray(597) = "2020/10/12 09:10:42 ATO.Vehicle.28.Status.ccms_veh_slot 130"
        TestArray(598) = "2020/10/12 09:10:42 ATO.Vehicle.47.Status.ccms_veh_slot 157"
        TestArray(599) = "2020/10/12 09:10:42 ATO.Vehicle.34.Status.ccms_veh_slot 48"
        TestArray(600) = "2020/10/12 09:10:42 ATO.Vehicle.50.Status.ccms_veh_slot 0"
        TestArray(601) = "2020/10/12 09:10:42 ATO.Vehicle.43.Status.ccms_veh_slot 29"
        TestArray(602) = "2020/10/12 09:10:42 ATO.Vehicle.73.Status.ccms_veh_slot 201"
        TestArray(603) = "2020/10/12 09:10:42 ATO.Vehicle.102.Status.ccms_veh_slot 22"
        TestArray(604) = "2020/10/12 09:10:42 ATO.Vehicle.81.Status.ccms_veh_slot 153"
        TestArray(605) = "2020/10/12 09:10:42 ATO.Vehicle.86.Status.ccms_veh_slot 158"
        TestArray(606) = "2020/10/12 09:10:42 ATO.Vehicle.62.Status.ccms_veh_slot 8"
        TestArray(607) = "2020/10/12 09:10:42 ATO.Vehicle.64.Status.ccms_veh_slot 10"
        TestArray(608) = "2020/10/12 09:10:42 ATO.Vehicle.52.Status.ccms_veh_slot 168"
        TestArray(609) = "2020/10/12 09:10:42 ATO.Vehicle.118.Status.ccms_veh_slot 170"
        TestArray(610) = "2020/10/12 09:10:42 ATO.Vehicle.70.Status.ccms_veh_slot 154"
        TestArray(611) = "2020/10/12 09:10:42 ATO.Vehicle.69.Status.ccms_veh_slot 173"
        TestArray(612) = "2020/10/12 09:10:42 ATO.Vehicle.56.Status.ccms_veh_slot 124"
        TestArray(613) = "2020/10/12 09:10:42 ATO.Vehicle.95.Status.ccms_veh_slot 187"
        TestArray(614) = "2020/10/12 09:10:42 ATO.Vehicle.54.Status.ccms_veh_slot 176"
        TestArray(615) = "2020/10/12 09:10:42 ATO.Vehicle.55.Status.ccms_veh_slot 125"
        TestArray(616) = "2020/10/12 09:10:42 ATO.Vehicle.79.Status.ccms_veh_slot 195"
        TestArray(617) = "2020/10/12 09:10:42 ATO.Vehicle.124.Status.ccms_veh_slot 160"
        TestArray(618) = "2020/10/12 09:10:42 ATO.Train.24.Status.location_segment 1281"
        TestArray(619) = "2020/10/12 09:10:42 ATO.Vehicle.132.Status.ccms_veh_slot 24"
        TestArray(620) = "2020/10/12 09:10:42 ATO.Vehicle.75.Status.ccms_veh_slot 119"
        TestArray(621) = "2020/10/12 09:10:42 ATO.Vehicle.60.Status.ccms_veh_slot 132"
        TestArray(622) = "2020/10/12 09:10:42 ATO.Train.84.Status.location_segment 0"
        TestArray(623) = "2020/10/12 09:10:42 ATO.Station.B1U.Status.left_door_1_status 16"
        TestArray(624) = "2020/10/12 09:10:42 ATO.Vehicle.78.Status.ccms_veh_slot 4"
        TestArray(625) = "2020/10/12 09:10:42 ATO.Station.BR4U.Status.left_door_1_status 16"
        TestArray(626) = "2020/10/12 09:10:42 ATO.Vehicle.94.Status.ccms_veh_slot 122"
        TestArray(627) = "2020/10/12 09:10:42 ATO.Vehicle.77.Status.ccms_veh_slot 5"
        TestArray(628) = "2020/10/12 09:10:42 ATO.Vehicle.65.Status.ccms_veh_slot 179"
        TestArray(629) = "2020/10/12 09:10:42 ATO.Vehicle.58.Status.ccms_veh_slot 46"
        TestArray(630) = "2020/10/12 09:10:42 ATO.Vehicle.123.Status.ccms_veh_slot 183"
        TestArray(631) = "2020/10/12 09:10:42 ATO.Vehicle.82.Status.ccms_veh_slot 134"
        TestArray(632) = "2020/10/12 09:10:42 ATO.Train.150.Status.id 0"
        TestArray(633) = "2020/10/12 09:10:42 ATO.Vehicle.91.Status.ccms_veh_slot 174"
        TestArray(634) = "2020/10/12 09:10:42 ATO.Train.45.Status.location_segment 1063"
        TestArray(635) = "2020/10/12 09:10:42 ATO.Vehicle.110.Status.ccms_veh_slot 127"
        TestArray(636) = "2020/10/12 09:10:42 ATO.Vehicle.76.Status.ccms_veh_slot 120"
        TestArray(637) = "2020/10/12 09:10:42 ATO.Train.21.Status.location_segment 1384"
        TestArray(638) = "2020/10/12 09:10:42 ATO.Vehicle.120.Status.ccms_veh_slot 146"
        TestArray(639) = "2020/10/12 09:10:42 ATO.Vehicle.90.Status.ccms_veh_slot 198"
        TestArray(640) = "2020/10/12 09:10:42 ATO.Vehicle.74.Status.ccms_veh_slot 188"
        TestArray(641) = "2020/10/12 09:10:42 ATO.Vehicle.83.Status.ccms_veh_slot 111"
        TestArray(642) = "2020/10/12 09:10:42 ATO.Train.152.Status.id 0"
        TestArray(643) = "2020/10/12 09:10:42 ATO.Train.94.Status.id 0"
        TestArray(644) = "2020/10/12 09:10:42 ATO.Train.93.Status.id 0"
        TestArray(645) = "2020/10/12 09:10:42 ATO.Train.92.Status.id 0"
        TestArray(646) = "2020/10/12 09:10:42 ATO.Train.91.Status.id 0"
        TestArray(647) = "2020/10/12 09:10:42 ATO.Train.90.Status.id 0"
        TestArray(648) = "2020/10/12 09:10:42 ATO.Train.89.Status.id 0"
        TestArray(649) = "2020/10/12 09:10:42 ATO.Train.88.Status.id 0"
        TestArray(650) = "2020/10/12 09:10:42 ATO.Train.87.Status.id 0"
        TestArray(651) = "2020/10/12 09:10:42 ATO.Train.86.Status.id 0"
        TestArray(652) = "2020/10/12 09:10:42 ATO.Vehicle.85.Status.ccms_veh_slot 161"
        TestArray(653) = "2020/10/12 09:10:42 ATO.Station.BR6D.Status.left_door_1_status 16"
        TestArray(654) = "2020/10/12 09:10:42 ATO.Train.7.Status.location_segment 1160"
        TestArray(655) = "2020/10/12 09:10:42 ATO.Vehicle.115.Status.ccms_veh_slot 139"
        TestArray(656) = "2020/10/12 09:10:42 ATO.Vehicle.89.Status.ccms_veh_slot 129"
        TestArray(657) = "2020/10/12 09:10:42 ATO.Vehicle.93.Status.ccms_veh_slot 133"
        TestArray(658) = "2020/10/12 09:10:42 ATO.Vehicle.84.Status.ccms_veh_slot 192"
        TestArray(659) = "2020/10/12 09:10:42 ATO.Vehicle.144.Status.ccms_veh_slot 0"
        TestArray(660) = "2020/10/12 09:10:42 ATO.Vehicle.135.Status.ccms_veh_slot 3"
        TestArray(661) = "2020/10/12 09:10:42 ATO.Vehicle.129.Status.ccms_veh_slot 191"
        TestArray(662) = "2020/10/12 09:10:42 ATO.Vehicle.101.Status.ccms_veh_slot 18"
        TestArray(663) = "2020/10/12 09:10:42 ATO.Vehicle.109.Status.ccms_veh_slot 106"
        TestArray(664) = "2020/10/12 09:10:42 ATO.Vehicle.100.Status.ccms_veh_slot 0"
        TestArray(665) = "2020/10/12 09:10:42 ATO.Vehicle.116.Status.ccms_veh_slot 104"
        TestArray(666) = "2020/10/12 09:10:42 ATO.Vehicle.137.Status.ccms_veh_slot 169"
        TestArray(667) = "2020/10/12 09:10:42 ATO.Vehicle.88.Status.ccms_veh_slot 172"
        TestArray(668) = "2020/10/12 09:10:42 ATO.Vehicle.98.Status.ccms_veh_slot 156"
        TestArray(669) = "2020/10/12 09:10:42 ATO.Station.BR7U.Status.left_door_1_status 16"
        TestArray(670) = "2020/10/12 09:10:42 ATO.Station.B7D.Status.station_dwell_time 25"
        TestArray(671) = "2020/10/12 09:10:42 ATO.Vehicle.98.Status.train_id 131"
        TestArray(672) = "2020/10/12 09:10:42 ATO.Vehicle.42.Status.train_id 137"
        TestArray(673) = "2020/10/12 09:10:42 ATO.Station.B9U.Status.station_dwell_time 35"
        TestArray(674) = "2020/10/12 09:10:42 ATO.Station.B8D.Status.right_door_1_status 16"
        TestArray(675) = "2020/10/12 09:10:42 ATO.Station.B2U.Status.right_door_1_status 16"
        TestArray(676) = "2020/10/12 09:10:42 ATO.Station.B6D.Status.right_door_1_status 16"
        TestArray(677) = "2020/10/12 09:10:42 ATO.Station.B7U.Status.right_door_1_status 32"
        TestArray(678) = "2020/10/12 09:10:42 ATO.Station.B10D.Status.right_door_1_status 16"
        TestArray(679) = "2020/10/12 09:10:42 ATO.Station.B1U.Status.right_door_1_status 16"
        TestArray(680) = "2020/10/12 09:10:42 ATO.Station.B5U.Status.right_door_1_status 16"
        TestArray(681) = "2020/10/12 09:10:42 ATO.Station.B11D.Status.right_door_1_status 16"
        TestArray(682) = "2020/10/12 09:10:42 ATO.Station.B7D.Status.right_door_1_status 16"
        TestArray(683) = "2020/10/12 09:10:42 ATO.Station.B3U.Status.right_door_1_status 16"
        TestArray(684) = "2020/10/12 09:10:42 ATO.Station.B9D.Status.right_door_1_status 16"
        TestArray(685) = "2020/10/12 09:10:42 ATO.Station.B3D.Status.right_door_1_status 16"
        TestArray(686) = "2020/10/12 09:10:42 ATO.Station.B4U.Status.right_door_1_status 16"
        TestArray(687) = "2020/10/12 09:10:42 ATO.Station.B2D.Status.right_door_1_status 16"
        TestArray(688) = "2020/10/12 09:10:42 ATO.Station.B6U.Status.right_door_1_status 16"
        TestArray(689) = "2020/10/12 09:10:42 ATO.Station.B5D.Status.right_door_1_status 16"
        TestArray(690) = "2020/10/12 09:10:42 ATO.Vehicle.3.Status.train_id 9"
        TestArray(691) = "2020/10/12 09:10:42 ATO.Vehicle.1.Status.train_id 115"
        TestArray(692) = "2020/10/12 09:10:42 ATO.Train.83.Status.location_segment 0"
        TestArray(693) = "2020/10/12 09:10:42 ATO.Train.52.Status.id 0"
        TestArray(694) = "2020/10/12 09:10:42 ATO.Train.51.Status.id 131"
        TestArray(695) = "2020/10/12 09:10:42 ATO.Train.50.Status.id 187"
        TestArray(696) = "2020/10/12 09:10:42 ATO.Train.49.Status.id 133"
        TestArray(697) = "2020/10/12 09:10:42 ATO.Train.48.Status.id 151"
        TestArray(698) = "2020/10/12 09:10:42 ATO.Train.47.Status.id 129"
        TestArray(699) = "2020/10/12 09:10:42 ATO.Vehicle.35.Status.train_id 121"
        TestArray(700) = "2020/10/12 09:10:42 ATO.Vehicle.8.Status.train_id 199"
        TestArray(701) = "2020/10/12 09:10:42 ATO.Vehicle.14.Status.train_id 31"
        TestArray(702) = "2020/10/12 09:10:42 ATO.Vehicle.90.Status.train_id 129"
        TestArray(703) = "2020/10/12 09:10:42 ATO.Vehicle.9.Status.train_id 43"
        TestArray(704) = "2020/10/12 09:10:42 ATO.Vehicle.107.Status.ccms_veh_slot 51"
        TestArray(705) = "2020/10/12 09:10:42 ATO.Station.BR6U.Status.left_door_1_status 16"
        TestArray(706) = "2020/10/12 09:10:42 ATO.Vehicle.156.Status.ccms_veh_slot 0"
        TestArray(707) = "2020/10/12 09:10:42 ATO.Vehicle.111.Status.ccms_veh_slot 149"
        TestArray(708) = "2020/10/12 09:10:42 ATO.Vehicle.130.Status.ccms_veh_slot 182"
        TestArray(709) = "2020/10/12 09:10:42 ATO.Vehicle.133.Status.ccms_veh_slot 13"
        TestArray(710) = "2020/10/12 09:10:42 ATO.Vehicle.147.Status.ccms_veh_slot 0"
        TestArray(711) = "2020/10/12 09:10:42 ATO.Station.B8U.Status.left_door_1_status 16"
        TestArray(712) = "2020/10/12 09:10:42 ATO.Vehicle.150.Status.ccms_veh_slot 0"
        TestArray(713) = "2020/10/12 09:10:42 ATO.Vehicle.113.Status.ccms_veh_slot 181"
        TestArray(714) = "2020/10/12 09:10:42 ATO.Station.B3D.Status.left_door_1_status 16"
        TestArray(715) = "2020/10/12 09:10:42 ATO.Vehicle.112.Status.ccms_veh_slot 200"
        TestArray(716) = "2020/10/12 09:10:42 ATO.Station.BR7D.Status.left_door_1_status 16"
        TestArray(717) = "2020/10/12 09:10:42 ATO.Station.BR3U.Status.left_door_1_status 16"
        TestArray(718) = "2020/10/12 09:10:42 ATO.Vehicle.114.Status.ccms_veh_slot 140"
        TestArray(719) = "2020/10/12 09:10:42 ATO.Vehicle.142.Status.ccms_veh_slot 110"
        TestArray(720) = "2020/10/12 09:10:42 ATO.Vehicle.131.Status.ccms_veh_slot 17"
        TestArray(721) = "2020/10/12 09:10:42 ATO.Vehicle.149.Status.ccms_veh_slot 0"
        TestArray(722) = "2020/10/12 09:10:42 ATO.Vehicle.121.Status.ccms_veh_slot 0"
        TestArray(723) = "2020/10/12 09:10:42 ATO.Vehicle.140.Status.ccms_veh_slot 44"
        TestArray(724) = "2020/10/12 09:10:42 ATO.Vehicle.152.Status.ccms_veh_slot 0"
        TestArray(725) = "2020/10/12 09:10:42 ATO.Vehicle.143.Status.ccms_veh_slot 0"
        TestArray(726) = "2020/10/12 09:10:42 ATO.Vehicle.134.Status.ccms_veh_slot 40"
        TestArray(727) = "2020/10/12 09:10:42 ATO.Vehicle.160.Status.ccms_veh_slot 0"
        TestArray(728) = "2020/10/12 09:10:42 ATO.Vehicle.145.Status.ccms_veh_slot 1"
        TestArray(729) = "2020/10/12 09:10:42 ATO.Station.B11D.Status.left_door_1_status 16"
        TestArray(730) = "2020/10/12 09:10:42 ATO.Vehicle.157.Status.ccms_veh_slot 0"
        TestArray(731) = "2020/10/12 09:10:42 ATO.Vehicle.139.Status.ccms_veh_slot 45"
        TestArray(732) = "2020/10/12 09:10:42 ATO.Vehicle.155.Status.ccms_veh_slot 0"
        TestArray(733) = "2020/10/12 09:10:42 ATO.Vehicle.158.Status.ccms_veh_slot 0"
        TestArray(734) = "2020/10/12 09:10:42 ATO.Train.28.Status.location_segment 346"
        TestArray(735) = "2020/10/12 09:10:42 ATO.Station.BR1U.Status.left_door_1_status 16"
        TestArray(736) = "2020/10/12 09:10:43 ATO.Vehicle.141.Status.ccms_veh_slot 105"
        TestArray(737) = "2020/10/12 09:10:43 ATO.Vehicle.136.Status.ccms_veh_slot 28"
        TestArray(738) = "2020/10/12 09:10:43 ATO.Station.B8D.Status.left_door_1_status 16"
        TestArray(739) = "2020/10/12 09:10:43 ATO.Vehicle.146.Status.ccms_veh_slot 16"
        TestArray(740) = "2020/10/12 09:10:43 ATO.Station.B7D.Status.left_door_1_status 16"
        TestArray(741) = "2020/10/12 09:10:43 ATO.Station.BR12U.Status.left_door_1_status 16"
        TestArray(742) = "2020/10/12 09:10:43 ATO.Station.B4D.Status.left_door_1_status 16"
        TestArray(743) = "2020/10/12 09:10:43 ATO.Station.B2D.Status.left_door_1_status 16"
        TestArray(744) = "2020/10/12 09:10:43 ATO.Station.B3U.Status.left_door_1_status 16"
        TestArray(745) = "2020/10/12 09:10:43 ATO.Station.BR11D.Status.left_door_1_status 16"
        TestArray(746) = "2020/10/12 09:10:43 ATO.Station.B5D.Status.left_door_1_status 16"
        TestArray(747) = "2020/10/12 09:10:43 ATO.Train.12.Status.location_segment 381"
        TestArray(748) = "2020/10/12 09:10:43 ATO.Station.BR3D.Status.left_door_1_status 16"
        TestArray(749) = "2020/10/12 09:10:43 ATO.Station.B9U.Status.left_door_1_status 16"
        TestArray(750) = "2020/10/12 09:10:43 ATO.Train.38.Status.location_segment 346"
        TestArray(751) = "2020/10/12 09:10:43 ATO.Station.BR13D.Status.left_door_1_status 16"
        TestArray(752) = "2020/10/12 09:10:43 ATO.Station.B1D.Status.left_door_1_status 16"
        TestArray(753) = "2020/10/12 09:10:43 ATO.Vehicle.159.Status.ccms_veh_slot 0"
        TestArray(754) = "2020/10/12 09:10:43 ATO.Train.1.Status.location_segment 1424"
        TestArray(755) = "2020/10/12 09:10:43 ATO.Train.15.Status.location_segment 800"
        TestArray(756) = "2020/10/12 09:10:43 ATO.Station.BR1D.Status.left_door_1_status 32"
        TestArray(757) = "2020/10/12 09:10:43 ATO.Station.B2U.Status.left_door_1_status 16"
        TestArray(758) = "2020/10/12 09:10:43 ATO.Station.BR9D.Status.left_door_1_status 16"
        TestArray(759) = "2020/10/12 09:10:43 ATO.Vehicle.154.Status.ccms_veh_slot 0"
        TestArray(760) = "2020/10/12 09:10:43 ATO.Train.14.Status.location_segment 318"
        TestArray(761) = "2020/10/12 09:10:43 ATO.Station.B6U.Status.left_door_1_status 16"
        TestArray(762) = "2020/10/12 09:10:43 ATO.Train.62.Status.location_segment 0"
        TestArray(763) = "2020/10/12 09:10:43 ATO.Station.BR4D.Status.left_door_1_status 16"
        TestArray(764) = "2020/10/12 09:10:43 ATO.Station.B6D.Status.left_door_1_status 16"
        TestArray(765) = "2020/10/12 09:10:43 ATO.Train.6.Status.location_segment 331"
        TestArray(766) = "2020/10/12 09:10:43 ATO.Station.B4U.Status.left_door_1_status 16"
        TestArray(767) = "2020/10/12 09:10:43 ATO.Station.B10D.Status.left_door_1_status 16"
        TestArray(768) = "2020/10/12 09:10:43 ATO.Station.BR12D.Status.left_door_1_status 16"
        TestArray(769) = "2020/10/12 09:10:43 ATO.Station.BR8D.Status.left_door_1_status 16"
        TestArray(770) = "2020/10/12 09:10:43 ATO.Station.B5U.Status.left_door_1_status 16"
        TestArray(771) = "2020/10/12 09:10:43 ATO.Station.BR5D.Status.left_door_1_status 16"
        TestArray(772) = "2020/10/12 09:10:43 ATO.Station.B10U.Status.left_door_1_status 16"
        TestArray(773) = "2020/10/12 09:10:43 ATO.Station.B7U.Status.left_door_1_status 16"
        TestArray(774) = "2020/10/12 09:10:43 ATO.Train.2.Status.location_segment 579"
        TestArray(775) = "2020/10/12 09:10:43 ATO.Station.BR9U.Status.left_door_1_status 16"
        TestArray(776) = "2020/10/12 09:10:43 ATO.Train.4.Status.location_segment 421"
        TestArray(777) = "2020/10/12 09:10:43 ATO.Train.16.Status.location_segment 614"
        TestArray(778) = "2020/10/12 09:10:43 ATO.Station.BR10U.Status.left_door_1_status 16"
        TestArray(779) = "2020/10/12 09:10:43 ATO.Station.BR2D.Status.left_door_1_status 16"
        TestArray(780) = "2020/10/12 09:10:43 ATO.Train.3.Status.location_segment 1429"
        TestArray(781) = "2020/10/12 09:10:43 ATO.Train.8.Status.location_segment 336"
        TestArray(782) = "2020/10/12 09:10:43 ATO.Train.44.Status.location_segment 421"
        TestArray(783) = "2020/10/12 09:10:43 ATO.Station.BR2U.Status.left_door_1_status 16"
        TestArray(784) = "2020/10/12 09:10:43 ATO.Station.BR11U.Status.left_door_1_status 16"
        TestArray(785) = "2020/10/12 09:10:43 ATO.Station.BR8U.Status.left_door_1_status 16"
        TestArray(786) = "2020/10/12 09:10:43 ATO.Station.BR5U.Status.left_door_1_status 16"
        TestArray(787) = "2020/10/12 09:10:43 ATO.Train.59.Status.location_segment 381"
        TestArray(788) = "2020/10/12 09:10:43 ATO.Train.67.Status.location_segment 406"
        TestArray(789) = "2020/10/12 09:10:43 ATO.Train.68.Status.location_segment 552"
        TestArray(790) = "2020/10/12 09:10:43 ATO.Train.10.Status.location_segment 341"
        TestArray(791) = "2020/10/12 09:10:43 ATO.Train.31.Status.location_segment 909"
        TestArray(792) = "2020/10/12 09:10:43 ATO.Station.BR13U.Status.left_door_1_status 16"
        TestArray(793) = "2020/10/12 09:10:43 ATO.Train.9.Status.location_segment 561"
        TestArray(794) = "2020/10/12 09:10:43 ATO.Train.66.Status.location_segment 1149"
        TestArray(795) = "2020/10/12 09:10:43 ATO.Train.1.Status.grand_route 0"
        TestArray(796) = "2020/10/12 09:10:43 ATO.Train.1.Status.location_offset 1853"
        TestArray(797) = "2020/10/12 09:10:43 ATO.Train.1.Status.location_offset 1853"
        TestArray(798) = "2020/10/12 09:10:43 ATO.Train.29.Status.location_segment 331"
        TestArray(799) = "2020/10/12 09:10:43 ATO.Train.25.Status.location_segment 296"



        Timer2.Enabled = True


    End Sub
End Class
