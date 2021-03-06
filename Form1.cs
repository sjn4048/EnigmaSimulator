﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace EnigmaStimulator
{
    public partial class Form1 : Form
    {
        bool Is_Decrypting;
        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

        public Form1()
        {
            InitializeComponent();
        }

        private char[] Alter(char[] plugboard, TextBox textBox)
        {
            if (textBox.Text.Length == 0)
                return plugboard;
            if (textBox.Text.Length != 2)
                MessageBox.Show(text: "Illegal Plugboard", icon: MessageBoxIcon.Warning, caption: "Error", buttons: MessageBoxButtons.OK);

            char ch1 = textBox.Text[0];
            char ch2 = textBox.Text[1];

            if (plugboard[ch1 - 'A'] != ch1 || plugboard[ch2 - 'A'] != ch2) //已经被换过了
                MessageBox.Show(text: "Illegal Plugboard", icon: MessageBoxIcon.Warning, caption: "Error", buttons: MessageBoxButtons.OK);
            else
            {
                plugboard[ch1 - 'A'] = ch2;
                plugboard[ch2 - 'A'] = ch1;
            }
            return plugboard;
        }

        private char[] Get_Plugboard()
        {
            char[] plugboard = new char[26];
            for (int i = 0; i < 26; i++)
                plugboard[i] = (char)('A' + i);

            //交换
            foreach (TextBox textBox in Plgbd.Controls)
                Alter(plugboard, textBox);

            return plugboard;
        } //根据winform输入，获取plugboard

        private int?[] Get_Rotor()
        {
            int?[] rotor = new int?[3];
            if (Rotor1.Text == string.Empty)
                rotor[0] = null;
            else
                rotor[0] = int.Parse(Rotor1.Text);

            if (Rotor2.Text == string.Empty)
                rotor[1] = null;
            else
                rotor[1] = int.Parse(Rotor2.Text);

            if (Rotor3.Text == string.Empty)
                rotor[2] = null;
            else
                rotor[2] = int.Parse(Rotor3.Text);

            return rotor;
        }

        private char?[] Get_Ring_Setting()
        {
            char?[] ring_setting = new char?[3];
            if (RS1.Text == string.Empty)
                ring_setting[0] = null;
            else
                ring_setting[0] = RS1.Text[0];

            if (RS2.Text == string.Empty)
                ring_setting[1] = null;
            else
                ring_setting[1] = RS2.Text[0];

            if (RS3.Text == string.Empty)
                ring_setting[2] = null;
            else
                ring_setting[2] = RS3.Text[0];

            return ring_setting;
        }

        private char?[] Get_Message_Key()
        {
            char?[] message_key = new char?[3];
            if (MK1.Text == string.Empty)
                message_key[0] = null;
            else
                message_key[0] = MK1.Text[0];

            if (MK2.Text == string.Empty)
                message_key[1] = null;
            else
                message_key[1] = MK2.Text[0];

            if (MK3.Text == string.Empty)
                message_key[2] = null;
            else
                message_key[2] = MK3.Text[0];

            return message_key;
        }

        private void encrypt_btn_Click(object sender, EventArgs e)
        {
            var plugboard = Get_Plugboard();
            var rotor_num = Get_Rotor();
            var ring_setting = Get_Ring_Setting();
            var message_key = Get_Message_Key();
            string plain_text = PlainText.Text;
            string cypher_text = CypherText.Text;
            string md5 = MD5.Text;
            string cyphered;//加密后的


            Enigma enigma = new Enigma(plugboard, rotor_num, ring_setting, message_key, plain_text, cypher_text, md5);
            try
            {
                cyphered = enigma.Encrypt();
            }
            catch (Exception ex)
            {
                MessageBox.Show(text: ex.Message, icon: MessageBoxIcon.Error, caption: "Error", buttons: MessageBoxButtons.OK);
                return;
            }
            CypherText.ForeColor = Color.DarkBlue;
            CypherText.Text = cyphered;
            MD5.ForeColor = Color.DarkBlue;
            MD5.Text = enigma.md5;
        }

        private async void decrypt_btn_Click(object sender, EventArgs e)
        {
            if (Is_Decrypting)
            {
                if (MessageBox.Show(icon: MessageBoxIcon.Warning, buttons: MessageBoxButtons.YesNo, caption: "Abort", text: "The program is decrypting now, are you sure you want to abort?") == DialogResult.No)
                    return;
                cancelTokenSource.Cancel();
                Is_Decrypting = false;
                //复苏被冻结的Textbox
                decrypt_btn.Text = "Decrypt";
                foreach (TextBox textBox in Plgbd.Controls)
                    textBox.ReadOnly = false;
                foreach (TextBox textBox in Rotor.Controls)
                    textBox.ReadOnly = false;
                foreach (TextBox textBox in RingSetting.Controls)
                    textBox.ReadOnly = false;
                foreach (TextBox textBox in MessageKey.Controls)
                    textBox.ReadOnly = false;
                CypherText.ReadOnly = MD5.ReadOnly = false;
                PlainText.ForeColor = Color.DarkBlue;
                PlainText.Text = "**DECRYPT_ABORTED**";
                return;
            }

            Is_Decrypting = true;

            string decrypted = string.Empty;//解密后的明文
            int count = 0; //缺失的信息的个数
            foreach (TextBox textBox in Rotor.Controls)
                if (textBox.Text == string.Empty)
                    count++;
            foreach (TextBox textBox in RingSetting.Controls)
                if (textBox.Text == string.Empty)
                    count++;
            foreach (TextBox textBox in MessageKey.Controls)
                if (textBox.Text == string.Empty)
                    count++;
            if (count == 6 && MessageBox.Show(icon: MessageBoxIcon.Warning, buttons: MessageBoxButtons.OKCancel, caption: "Warning", text: "This might take a long time, continue?") == DialogResult.Cancel)
                return;
            if (count >= 7 && MessageBox.Show(icon: MessageBoxIcon.Warning, buttons: MessageBoxButtons.OKCancel, caption: "Warning", text: "This might take VERY LONG time, continue?") == DialogResult.Cancel)
                return;

            //生成cancellationtoken与stopwatch
            CancellationToken cancellation = cancelTokenSource.Token;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //开始在UI线程上做相关的准备，比如更改button名字、Unable各种东西
            foreach (TextBox textBox in Plgbd.Controls)
                textBox.ReadOnly = true;
            foreach (TextBox textBox in Rotor.Controls)
                textBox.ReadOnly = true;
            foreach (TextBox textBox in RingSetting.Controls)
                textBox.ReadOnly = true;
            foreach (TextBox textBox in MessageKey.Controls)
                textBox.ReadOnly = true;
            CypherText.ReadOnly = MD5.ReadOnly = true;
            decrypt_btn.Text = "Abort";
            PlainText.Text = "Decrypting, Please wait";

            //开始运行破解任务
            await Task.Run(() =>
            {
                try
                {
                    var plugboard = Get_Plugboard();
                    var rotor_num = Get_Rotor();
                    var ring_setting = Get_Ring_Setting();
                    var message_key = Get_Message_Key();
                    // plain_text = PlainText.Text;
                    string cypher_text = CypherText.Text;
                    string md5 = MD5.Text;
                    Enigma enigma = new Enigma(plugboard, rotor_num, ring_setting, message_key, "", cypher_text, md5);
                    decrypted = enigma.Decrpyt();
                    //显示破译数据
                    this.Invoke(new Action(() =>
                    {
                        if (Rotor1.Text == string.Empty || Rotor1.Text != enigma.rotor_num[0].ToString()) { Rotor1.ForeColor = Color.DarkBlue; Rotor1.Text = enigma.rotor_num[0].ToString(); }
                        if (Rotor2.Text == string.Empty || Rotor2.Text != enigma.rotor_num[1].ToString()) { Rotor2.ForeColor = Color.DarkBlue; Rotor2.Text = enigma.rotor_num[1].ToString(); }
                        if (Rotor3.Text == string.Empty || Rotor3.Text != enigma.rotor_num[2].ToString()) { Rotor3.ForeColor = Color.DarkBlue; Rotor3.Text = enigma.rotor_num[2].ToString(); }
                        if (RS1.Text == string.Empty || RS1.Text != enigma.ring_setting[0].ToString()) { RS1.ForeColor = Color.DarkBlue; RS1.Text = enigma.ring_setting[0].ToString(); }
                        if (RS2.Text == string.Empty || RS2.Text != enigma.ring_setting[1].ToString()) { RS2.ForeColor = Color.DarkBlue; RS2.Text = enigma.ring_setting[1].ToString(); }
                        if (RS3.Text == string.Empty || RS3.Text != enigma.ring_setting[2].ToString()) { RS3.ForeColor = Color.DarkBlue; RS3.Text = enigma.ring_setting[2].ToString(); }
                        if (MK1.Text == string.Empty || MK1.Text != enigma.message_key[0].ToString()) { MK1.ForeColor = Color.DarkBlue; MK1.Text = enigma.message_key[0].ToString(); }
                        if (MK2.Text == string.Empty || MK2.Text != enigma.message_key[1].ToString()) { MK2.ForeColor = Color.DarkBlue; MK2.Text = enigma.message_key[1].ToString(); }
                        if (MK3.Text == string.Empty || MK3.Text != enigma.message_key[2].ToString()) { MK3.ForeColor = Color.DarkBlue; MK3.Text = enigma.message_key[2].ToString(); }
                    }));
                    stopwatch.Stop();
                    MessageBox.Show(text: "Success. Time:" + stopwatch.ElapsedMilliseconds / 1000.0 + "second", icon: MessageBoxIcon.Information, caption: "Success", buttons: MessageBoxButtons.OK);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(text: "Failed. Error:" + ex.Message + ", Time: " + stopwatch.ElapsedMilliseconds / 1000.0 + "second", icon: MessageBoxIcon.Error, caption: "Error", buttons: MessageBoxButtons.OK);
                }
            }, cancellation);
            Is_Decrypting = false;
            //复苏被冻结的Textbox
            decrypt_btn.Text = "Decrypt";
            foreach (TextBox textBox in Plgbd.Controls)
                textBox.ReadOnly = false;
            foreach (TextBox textBox in Rotor.Controls)
                textBox.ReadOnly = false;
            foreach (TextBox textBox in RingSetting.Controls)
                textBox.ReadOnly = false;
            foreach (TextBox textBox in MessageKey.Controls)
                textBox.ReadOnly = false;
            CypherText.ReadOnly = MD5.ReadOnly = false;
            PlainText.ForeColor = Color.DarkBlue;
            PlainText.Text = decrypted;
        }

        /// <summary>
        /// 以下为UI函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 

        private void Plgbd_Leave(object sender, EventArgs e)
        {
            TextBox this_textBox = (TextBox)sender;
            if (this_textBox.Text.Length == 1 || this_textBox.Text.Length > 2) //如果字符数不规范
                this_textBox.ForeColor = Color.Red;
            else this_textBox.ForeColor = Color.DarkGreen;

            //检测是否出现重复交换
            if (this_textBox.Text.Length == 2)
            {
                foreach (TextBox textBox in Plgbd.Controls)
                {
                    if (textBox == this_textBox)
                        continue;
                    if (textBox.Text.Contains(this_textBox.Text[0]) || textBox.Text.Contains(this_textBox.Text[1]))
                        this_textBox.ForeColor = Color.Red;
                }
            }
        }

        private void Plgbd_KeyPress(object sender, KeyPressEventArgs e) //处理Plugboard的键盘输入，自动更正非法项
        {
            //TODO：按下Tab之后自动切换到下一行

            TextBox textBox = (TextBox)sender;
            if (e.KeyChar == '\b') //如果输入的是退格键，则放行
                return;

            if (textBox.Text.Length >= 2 && textBox.SelectedText == "") //如果已经满了，则不计入这次输入
                e.Handled = true;

            if (e.KeyChar >= 'A' && e.KeyChar <= 'Z') //自动将小写转换为大写、并舍弃非字母
                ; //不执行任何东西
            else if (e.KeyChar >= 'a' && e.KeyChar <= 'z')
            {
                e.KeyChar = (char)(e.KeyChar - 32);
            }
            else//中止这次输入
            {
                e.Handled = true;
                return;
            }

            if (textBox.Text.Length - textBox.SelectedText.Length >= 1)
                SendKeys.Send("{tab}");
        }

        private void Rotor_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (e.KeyChar == '\b') //如果输入的是退格键，则放行
                return;

            if (textBox.Text.Length >= 1 && textBox.SelectedText == "") //如果已经满了，则不计入这次输入
                e.Handled = true;

            if (e.KeyChar < '0' || e.KeyChar > '5') //舍弃不合法输入
            {
                e.Handled = true;
                return;
            }
            if (textBox.Text.Length >= 0) //自动跳进
                SendKeys.Send("{tab}");
        }

        private void Rotor_Leave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            //textBox.BackColor = Color.White; 如果未输入则自动变色，暂时还没想到特别好的实践
            if (textBox.Text.Length > 1) //如果字符数不规范
                textBox.ForeColor = Color.Red;
            else if (textBox.Text.Length == 1)
                textBox.ForeColor = Color.DarkGreen;
            //else
                //textBox.BackColor = Color.Pink;
        }

        private void MKRS_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (e.KeyChar == '\b') //如果输入的是退格键，则放行
                return;

            if (textBox.Text.Length >= 1 && textBox.SelectedText == "") //如果已经满了，则不计入这次输入
                e.Handled = true;

            if (e.KeyChar >= 'A' && e.KeyChar <= 'Z') //自动将小写转换为大写、并舍弃非字母
                ;//不执行任何语句
            else if (e.KeyChar >= 'a' && e.KeyChar <= 'z')
            {
                e.KeyChar = (char)(e.KeyChar - 32);
            }
            else
            {
                e.Handled = true;
                return;
            }
            if (textBox.Text.Length >= 0) //自动跳进
                SendKeys.Send("{tab}");
        }

        private void MKRS_Leave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text.Length > 1) //如果字符数不规范
                textBox.ForeColor = Color.Red;
            else textBox.ForeColor = Color.DarkGreen;
        }

        private void Text_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            ///TODO:判断Ctrl引入的内容是否合法，防止bug
            if (e.KeyChar == '\b' || (Control.ModifierKeys & Keys.Control) == Keys.Control) //如果输入的是退格键或按下了Ctrl，则放行
                return;

            if (e.KeyChar >= 'A' && e.KeyChar <= 'Z') //自动将小写转换为大写、并舍弃非字母
                ;//不执行任何语句
            else if (e.KeyChar >= 'a' && e.KeyChar <= 'z')
            {
                e.KeyChar = (char)(e.KeyChar - 32);
            }
            else
            {
                e.Handled = true;
                return;
            }
        }

        private void MD5_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (e.KeyChar == '\b' || (Control.ModifierKeys & Keys.Control) == Keys.Control) //如果输入的是退格或Ctrl键，则放行
                return;

            if (e.KeyChar >= 'A' && e.KeyChar <= 'Z' || char.IsNumber(e.KeyChar)) //自动将小写转换为大写、并舍弃非字母、非数字
                ;//不执行任何语句
            else if (e.KeyChar >= 'a' && e.KeyChar <= 'z')
            {
                e.KeyChar = (char)(e.KeyChar - 32);
            }
            else
            {
                e.Handled = true;
                return;
            }
        }

        private void Text_Leave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.ForeColor = Color.DarkGreen;
        }

        private void MD5_Leave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text.Length == 16 || textBox.Text.Length == 32)
                textBox.ForeColor = Color.DarkGreen;
            else
                textBox.ForeColor = Color.Red;
        }

        private void Textbox_Enter(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.SelectAll();
        }

        private void Textbox_MouseClick(object sender, MouseEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.SelectAll();
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void clearCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (TextBox textBox in Plgbd.Controls)
                textBox.Text = string.Empty;
            foreach (TextBox textBox in Rotor.Controls)
                textBox.Text = string.Empty;
            foreach (TextBox textBox in RingSetting.Controls)
                textBox.Text = string.Empty;
            foreach (TextBox textBox in MessageKey.Controls)
                textBox.Text = string.Empty;
            PlainText.Text = CypherText.Text = MD5.Text = string.Empty;
        }

        private void encryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            encrypt_btn.PerformClick();
        }

        private void decryptDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            decrypt_btn.PerformClick();
        }
    }
}
