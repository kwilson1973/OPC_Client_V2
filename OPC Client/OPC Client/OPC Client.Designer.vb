<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form2
    Inherits System.Windows.Forms.Form

    'Form 覆寫 Dispose 以清除元件清單。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    '為 Windows Form 設計工具的必要項
    Private components As System.ComponentModel.IContainer

    '注意: 以下為 Windows Form 設計工具所需的程序
    '可以使用 Windows Form 設計工具進行修改。
    '請不要使用程式碼編輯器進行修改。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle
        Me.dataOPC = New System.Windows.Forms.DataGridView
        Me.ColumnItem = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.ColumnValue = New System.Windows.Forms.DataGridViewTextBoxColumn
        Me.btnDisconnect = New System.Windows.Forms.Button
        Me.btnMonitoringOn = New System.Windows.Forms.Button
        Me.Label6 = New System.Windows.Forms.Label
        Me.Label5 = New System.Windows.Forms.Label
        Me.txtServerName = New System.Windows.Forms.TextBox
        Me.Label3 = New System.Windows.Forms.Label
        Me.txtServerIP = New System.Windows.Forms.TextBox
        Me.btnConnect = New System.Windows.Forms.Button
        Me.lstOPCServer = New System.Windows.Forms.TreeView
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip
        Me.ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel
        Me.StatusConnection = New System.Windows.Forms.ToolStripStatusLabel
        Me.ToolStripStatusLabel2 = New System.Windows.Forms.ToolStripStatusLabel
        Me.StatusCommand = New System.Windows.Forms.ToolStripStatusLabel
        Me.ToolStripStatusLabel4 = New System.Windows.Forms.ToolStripStatusLabel
        Me.ToolStripStatusLabel3 = New System.Windows.Forms.ToolStripStatusLabel
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.DataGridView1 = New System.Windows.Forms.DataGridView
        Me.DataGridView2 = New System.Windows.Forms.DataGridView
        Me.ListBox1 = New System.Windows.Forms.ListBox
        Me.DataGridView3 = New System.Windows.Forms.DataGridView
        Me.btnTest = New System.Windows.Forms.Button
        Me.Timer2 = New System.Windows.Forms.Timer(Me.components)
        CType(Me.dataOPC, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.StatusStrip1.SuspendLayout()
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.DataGridView2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.DataGridView3, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'dataOPC
        '
        Me.dataOPC.BackgroundColor = System.Drawing.Color.Beige
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = System.Drawing.Color.Silver
        DataGridViewCellStyle1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.dataOPC.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle1
        Me.dataOPC.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dataOPC.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.ColumnItem, Me.ColumnValue})
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        DataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dataOPC.DefaultCellStyle = DataGridViewCellStyle2
        Me.dataOPC.Enabled = False
        Me.dataOPC.GridColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.dataOPC.Location = New System.Drawing.Point(21, 130)
        Me.dataOPC.Name = "dataOPC"
        DataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle3.BackColor = System.Drawing.Color.Silver
        DataGridViewCellStyle3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        DataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black
        DataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.dataOPC.RowHeadersDefaultCellStyle = DataGridViewCellStyle3
        Me.dataOPC.RowHeadersWidth = 30
        Me.dataOPC.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing
        Me.dataOPC.RowTemplate.Height = 24
        Me.dataOPC.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dataOPC.Size = New System.Drawing.Size(550, 101)
        Me.dataOPC.TabIndex = 28
        '
        'ColumnItem
        '
        Me.ColumnItem.HeaderText = "Item"
        Me.ColumnItem.Name = "ColumnItem"
        Me.ColumnItem.ReadOnly = True
        Me.ColumnItem.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.ColumnItem.Width = 180
        '
        'ColumnValue
        '
        Me.ColumnValue.HeaderText = "Value"
        Me.ColumnValue.Name = "ColumnValue"
        Me.ColumnValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'btnDisconnect
        '
        Me.btnDisconnect.Location = New System.Drawing.Point(303, 96)
        Me.btnDisconnect.Name = "btnDisconnect"
        Me.btnDisconnect.Size = New System.Drawing.Size(75, 28)
        Me.btnDisconnect.TabIndex = 39
        Me.btnDisconnect.Text = "Disconnect"
        Me.btnDisconnect.UseVisualStyleBackColor = True
        '
        'btnMonitoringOn
        '
        Me.btnMonitoringOn.BackColor = System.Drawing.SystemColors.Control
        Me.btnMonitoringOn.Location = New System.Drawing.Point(303, 63)
        Me.btnMonitoringOn.Name = "btnMonitoringOn"
        Me.btnMonitoringOn.Size = New System.Drawing.Size(76, 27)
        Me.btnMonitoringOn.TabIndex = 25
        Me.btnMonitoringOn.Text = "Monitor"
        Me.btnMonitoringOn.UseVisualStyleBackColor = False
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(430, 14)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(80, 12)
        Me.Label6.TabIndex = 34
        Me.Label6.Text = "OPC Server List"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(30, 78)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(90, 12)
        Me.Label5.TabIndex = 33
        Me.Label5.Text = "OPC Server Neme"
        '
        'txtServerName
        '
        Me.txtServerName.Location = New System.Drawing.Point(122, 75)
        Me.txtServerName.Name = "txtServerName"
        Me.txtServerName.Size = New System.Drawing.Size(159, 22)
        Me.txtServerName.TabIndex = 32
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(30, 41)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(48, 12)
        Me.Label3.TabIndex = 31
        Me.Label3.Text = "Server IP"
        '
        'txtServerIP
        '
        Me.txtServerIP.Location = New System.Drawing.Point(122, 37)
        Me.txtServerIP.Name = "txtServerIP"
        Me.txtServerIP.Size = New System.Drawing.Size(159, 22)
        Me.txtServerIP.TabIndex = 30
        '
        'btnConnect
        '
        Me.btnConnect.BackColor = System.Drawing.SystemColors.Control
        Me.btnConnect.Location = New System.Drawing.Point(303, 30)
        Me.btnConnect.Name = "btnConnect"
        Me.btnConnect.Size = New System.Drawing.Size(76, 27)
        Me.btnConnect.TabIndex = 24
        Me.btnConnect.Text = "Connect"
        Me.btnConnect.UseVisualStyleBackColor = False
        '
        'lstOPCServer
        '
        Me.lstOPCServer.Location = New System.Drawing.Point(397, 32)
        Me.lstOPCServer.Name = "lstOPCServer"
        Me.lstOPCServer.Size = New System.Drawing.Size(174, 92)
        Me.lstOPCServer.TabIndex = 27
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripStatusLabel1, Me.StatusConnection, Me.ToolStripStatusLabel2, Me.StatusCommand, Me.ToolStripStatusLabel4, Me.ToolStripStatusLabel3})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 570)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(591, 24)
        Me.StatusStrip1.TabIndex = 43
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'ToolStripStatusLabel1
        '
        Me.ToolStripStatusLabel1.BackColor = System.Drawing.SystemColors.Control
        Me.ToolStripStatusLabel1.ForeColor = System.Drawing.Color.Gray
        Me.ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        Me.ToolStripStatusLabel1.Size = New System.Drawing.Size(81, 19)
        Me.ToolStripStatusLabel1.Text = "Connection : "
        '
        'StatusConnection
        '
        Me.StatusConnection.BackColor = System.Drawing.SystemColors.Control
        Me.StatusConnection.ForeColor = System.Drawing.Color.Blue
        Me.StatusConnection.Name = "StatusConnection"
        Me.StatusConnection.Size = New System.Drawing.Size(89, 19)
        Me.StatusConnection.Text = "not connected"
        '
        'ToolStripStatusLabel2
        '
        Me.ToolStripStatusLabel2.BackColor = System.Drawing.SystemColors.Control
        Me.ToolStripStatusLabel2.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left
        Me.ToolStripStatusLabel2.ForeColor = System.Drawing.Color.Gray
        Me.ToolStripStatusLabel2.Name = "ToolStripStatusLabel2"
        Me.ToolStripStatusLabel2.Size = New System.Drawing.Size(80, 19)
        Me.ToolStripStatusLabel2.Text = "Command : "
        '
        'StatusCommand
        '
        Me.StatusCommand.AutoSize = False
        Me.StatusCommand.BackColor = System.Drawing.SystemColors.Control
        Me.StatusCommand.ForeColor = System.Drawing.Color.Blue
        Me.StatusCommand.Name = "StatusCommand"
        Me.StatusCommand.Size = New System.Drawing.Size(200, 19)
        Me.StatusCommand.Text = "-"
        Me.StatusCommand.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ToolStripStatusLabel4
        '
        Me.ToolStripStatusLabel4.AutoSize = False
        Me.ToolStripStatusLabel4.BackColor = System.Drawing.SystemColors.Control
        Me.ToolStripStatusLabel4.Name = "ToolStripStatusLabel4"
        Me.ToolStripStatusLabel4.Size = New System.Drawing.Size(80, 19)
        Me.ToolStripStatusLabel4.Text = " "
        '
        'ToolStripStatusLabel3
        '
        Me.ToolStripStatusLabel3.Name = "ToolStripStatusLabel3"
        Me.ToolStripStatusLabel3.Size = New System.Drawing.Size(131, 15)
        Me.ToolStripStatusLabel3.Text = "ToolStripStatusLabel3"
        '
        'Timer1
        '
        Me.Timer1.Enabled = True
        Me.Timer1.Interval = 1000
        '
        'DataGridView1
        '
        Me.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView1.Location = New System.Drawing.Point(21, 237)
        Me.DataGridView1.Name = "DataGridView1"
        Me.DataGridView1.RowTemplate.Height = 24
        Me.DataGridView1.Size = New System.Drawing.Size(550, 112)
        Me.DataGridView1.TabIndex = 44
        '
        'DataGridView2
        '
        Me.DataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView2.Location = New System.Drawing.Point(21, 355)
        Me.DataGridView2.Name = "DataGridView2"
        Me.DataGridView2.RowTemplate.Height = 24
        Me.DataGridView2.Size = New System.Drawing.Size(237, 124)
        Me.DataGridView2.TabIndex = 45
        '
        'ListBox1
        '
        Me.ListBox1.FormattingEnabled = True
        Me.ListBox1.ItemHeight = 12
        Me.ListBox1.Location = New System.Drawing.Point(21, 485)
        Me.ListBox1.Name = "ListBox1"
        Me.ListBox1.Size = New System.Drawing.Size(550, 76)
        Me.ListBox1.TabIndex = 46
        '
        'DataGridView3
        '
        Me.DataGridView3.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView3.Location = New System.Drawing.Point(264, 355)
        Me.DataGridView3.Name = "DataGridView3"
        Me.DataGridView3.RowTemplate.Height = 24
        Me.DataGridView3.Size = New System.Drawing.Size(307, 124)
        Me.DataGridView3.TabIndex = 49
        '
        'btnTest
        '
        Me.btnTest.Location = New System.Drawing.Point(223, 103)
        Me.btnTest.Name = "btnTest"
        Me.btnTest.Size = New System.Drawing.Size(75, 23)
        Me.btnTest.TabIndex = 51
        Me.btnTest.Text = "TEST"
        Me.btnTest.UseVisualStyleBackColor = True
        '
        'Timer2
        '
        Me.Timer2.Enabled = True
        '
        'Form2
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(591, 594)
        Me.Controls.Add(Me.btnTest)
        Me.Controls.Add(Me.DataGridView3)
        Me.Controls.Add(Me.ListBox1)
        Me.Controls.Add(Me.DataGridView2)
        Me.Controls.Add(Me.DataGridView1)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.dataOPC)
        Me.Controls.Add(Me.btnDisconnect)
        Me.Controls.Add(Me.btnMonitoringOn)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.txtServerName)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txtServerIP)
        Me.Controls.Add(Me.btnConnect)
        Me.Controls.Add(Me.lstOPCServer)
        Me.Name = "Form2"
        Me.Text = "OPC Client"
        CType(Me.dataOPC, System.ComponentModel.ISupportInitialize).EndInit()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.DataGridView2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.DataGridView3, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents dataOPC As System.Windows.Forms.DataGridView
    Friend WithEvents ColumnItem As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents ColumnValue As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents btnDisconnect As System.Windows.Forms.Button
    Friend WithEvents btnMonitoringOn As System.Windows.Forms.Button
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents txtServerName As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents txtServerIP As System.Windows.Forms.TextBox
    Friend WithEvents btnConnect As System.Windows.Forms.Button
    Friend WithEvents lstOPCServer As System.Windows.Forms.TreeView
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents ToolStripStatusLabel1 As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents StatusConnection As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents ToolStripStatusLabel2 As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents StatusCommand As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents ToolStripStatusLabel4 As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents DataGridView1 As System.Windows.Forms.DataGridView
    Friend WithEvents DataGridView2 As System.Windows.Forms.DataGridView
    Friend WithEvents ListBox1 As System.Windows.Forms.ListBox
    Friend WithEvents ToolStripStatusLabel3 As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents DataGridView3 As System.Windows.Forms.DataGridView
    Friend WithEvents btnTest As System.Windows.Forms.Button
    Friend WithEvents Timer2 As System.Windows.Forms.Timer

End Class
