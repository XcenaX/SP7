using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace WindowsFormsApplication13 {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();

            // Mikant
            UpdateUI = new Action<int>(delegate(int value) {
                progressBar1.Value = value;
            });
            // !Mikant
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CopyFileEx(string lpExistingFileName, string lpNewFileName,
           CopyProgressRoutine lpProgressRoutine, IntPtr lpData, ref Int32 pbCancel,
           CopyFileFlags dwCopyFlags);

        delegate CopyProgressResult CopyProgressRoutine(
        long TotalFileSize,
        long TotalBytesTransferred,
        long StreamSize,
        long StreamBytesTransferred,
        uint dwStreamNumber,
        CopyProgressCallbackReason dwCallbackReason,
        IntPtr hSourceFile,
        IntPtr hDestinationFile,
        IntPtr lpData);

        int pbCancel;

        enum CopyProgressResult : uint {
            PROGRESS_CONTINUE = 0,
            PROGRESS_CANCEL = 1,
            PROGRESS_STOP = 2,
            PROGRESS_QUIET = 3
        }

        enum CopyProgressCallbackReason : uint {
            CALLBACK_CHUNK_FINISHED = 0x00000000,
            CALLBACK_STREAM_SWITCH = 0x00000001
        }

        [Flags]
        enum CopyFileFlags : uint {
            COPY_FILE_FAIL_IF_EXISTS = 0x00000001,
            COPY_FILE_RESTARTABLE = 0x00000002,
            COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x00000004,
            COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x00000008
        }

        private void XCopy(string oldFile, string newFile) {
            CopyFileEx(oldFile, newFile, new CopyProgressRoutine(this.CopyProgressHandler), IntPtr.Zero, ref pbCancel, CopyFileFlags.COPY_FILE_RESTARTABLE);
        }

        private CopyProgressResult CopyProgressHandler(long total, long transferred, long streamSize, long StreamByteTrans, uint dwStreamNumber, CopyProgressCallbackReason reason, IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData) {
            
            // Mikant
            int currentProgress = (int)((transferred/(float)total)*100);

            if (this.InvokeRequired)
                this.Invoke(UpdateUI, currentProgress);
            else
                UpdateUI(currentProgress);
            // !Mikant

            return CopyProgressResult.PROGRESS_CONTINUE;
        }

        // Mikant
        private void button1_Click(object sender, EventArgs e) {
            using (FileStream fs = File.Open("файло", FileMode.Create)) {
                fs.Seek(104857599, SeekOrigin.Begin);
                fs.WriteByte(0);
            }
        }

        // Mikant
        private void button2_Click(object sender, EventArgs e) {
            string oldFileName = Path.Combine(Application.StartupPath, "файло");
            string newFileName = Path.Combine(Application.StartupPath, "файло2");
            XCopy(oldFileName, newFileName);
        }

        // Mikant
        private delegate void Action<T>(T value);

        // Mikant
        private Action<int> UpdateUI;
    }
}
