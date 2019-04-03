using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Security.Cryptography;
using CRYPTEXTLib;

namespace WindowsFormsApplication13 {
    public partial class Form1 : Form {
        const string stringKey = "%[|/&8";
        string currentFileName = "";
        string currentFileData = "";
        Thread currentThread = null;
        public const string password = "123";
        public Form1() {
            InitializeComponent();            
            checkBox1.Checked = true;
            UpdateUI = new Action<int>(delegate (int value) {
                progressBar1.Value = value;
            });            
        }

        private delegate void Action<T>(T value);
        private Action<int> UpdateUI;

        private void EncryptDecrypt() {                                     
            EncryptDecryptFile(textBox1.Text, checkBox1.Checked);            
        }
     
         private void Button2_Click(object sender, EventArgs e) {
            if (textBox2.Text != password)
            {
                MessageBox.Show("Вы ввели неверный пароль!\n(Пароль : 123)");
                return;
            }
            if (!File.Exists(textBox1.Text))
            {
                MessageBox.Show("Такого файла не сущесвует!");
                return;
            }
            button2.Enabled = false;
            button1.Enabled = true;

            var thread = new Thread(new ThreadStart(EncryptDecrypt))
            {
                IsBackground = true
            };
            thread.Start();
            currentThread = thread;
        }         
            
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = dialog.FileName;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox2.Checked = false;
            }
            else checkBox2.Checked = true;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                checkBox1.Checked = false;
            }
            else checkBox1.Checked = true;
        }

        public void EncryptDecryptFile(String FileName, bool isEncrypt)
        {            
            string data = "";
            using (StreamReader reader = new StreamReader(textBox1.Text))
            {
                data = reader.ReadToEnd();
            }
            currentFileName = FileName;
            currentFileData = data;

            using (FileStream fStream = File.Open(FileName, FileMode.OpenOrCreate))
            {
                if (fStream.Length <= 0)
                {
                    MessageBox.Show("Файл пустой! Там нечего шифровать!");
                    return;
                }
                byte[] b1 = Encoding.ASCII.GetBytes(data);
                byte[] key = Encoding.ASCII.GetBytes(stringKey);
                byte[] result = new byte[b1.Length];
                int countBytes = 0;

                DateTime afterSecond = DateTime.Now;
                afterSecond.AddSeconds(1);

                int j = 0;
                for (int i = 0; i < b1.Length; i++)
                {
                    if(DateTime.Now >= afterSecond)
                    {
                        afterSecond.AddSeconds(1);
                        labelSpeed.Text = countBytes + " Кб/Сек";
                        countBytes = 0;
                    }
                    if(isEncrypt)result[i] = (byte)((int)(b1[i]) ^ (int)(key[j++]));                
                    else result[i] = (byte)((int)(key[j++]) ^ (int)(b1[i]));

                    countBytes++;

                    if (j >= key.Length)
                    {
                        j = 0;
                    }
                    var progressBarResult = Convert.ToInt32((i / (Convert.ToDouble(b1.Length) / 100.0)) / 2);
                    if (this.InvokeRequired)
                        this.Invoke(UpdateUI, progressBarResult);
                    else
                        UpdateUI(progressBarResult);

                }
                fStream.SetLength(0);
                int index = 0;
                foreach (var Byte in result)
                {
                    fStream.WriteByte(Byte);
                    var progressBarResult = Convert.ToInt32((index / (Convert.ToDouble(result.Length) / 100.0)) / 2 + 50);
                    if (this.InvokeRequired)
                        this.Invoke(UpdateUI, progressBarResult);
                    else
                        UpdateUI(progressBarResult);
                    index++;
                }
                if (this.InvokeRequired)
                    this.Invoke(UpdateUI, 100);
                else
                    UpdateUI(100);
                button1.Enabled = false;
                button2.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            currentThread.Abort();           
            button1.Enabled = false;
            button2.Enabled = true;
            //Если пользователь нажал отмена, то нужно вернуть все на свои места
            using(FileStream stream = new FileStream(currentFileName, FileMode.OpenOrCreate))
            {
                stream.SetLength(0);
                stream.Write(Encoding.ASCII.GetBytes(currentFileData), 0, currentFileData.Length);
            }            
        }

        private void ShowHint(object sender, EventArgs e)
        {
            labelSpeed.Visible = true;                                    
        }

        private void HideHint(object sender, EventArgs e)
        {
            labelSpeed.Visible = false;
        }
    }
}
